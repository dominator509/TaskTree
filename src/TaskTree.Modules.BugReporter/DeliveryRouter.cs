// SPEC-DERIVED-PHASE3E  HALT #15/#16/#17/#18
// Architecture.md Section 9.2.4 routing matrix and Section 9.2.6 rate limit.
// Gap #272/#273/#274: BugSeverity enum reconciliation, partial retry semantics, and live adapter replacement deferred.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Routes redacted bug reports to configured delivery channels by severity.</summary>
    public sealed class DeliveryRouter
    {
        private readonly EmailDeliveryAdapter _email;
        private readonly GitHubIssueAdapter _github;
        private readonly FileDropAdapter _fileDrop;
        private readonly BugReportRateLimiter _rateLimiter;
        private readonly IClock _clock;
        public DeliveryRouter(EmailDeliveryAdapter email, GitHubIssueAdapter github, FileDropAdapter fileDrop, BugReportRateLimiter rateLimiter, IClock clock)
        { _email=email??throw new ArgumentNullException(nameof(email)); _github=github??throw new ArgumentNullException(nameof(github)); _fileDrop=fileDrop??throw new ArgumentNullException(nameof(fileDrop)); _rateLimiter=rateLimiter??throw new ArgumentNullException(nameof(rateLimiter)); _clock=clock??throw new ArgumentNullException(nameof(clock)); }
        public async Task<BugReportDeliveryResult> DeliverAsync(BugReport report)
        {
            if (report is null) throw new ArgumentNullException(nameof(report));
            if (!report.Redacted) return new BugReportDeliveryResult(false,"Router","Report is not marked redacted.");
            var adapters = Routes(report.Severity).ToList();
            var failures = new List<string>();
            foreach(var adapter in adapters)
            {
                var outbound = adapter.Channel != "FileDrop";
                if(outbound && !_rateLimiter.CanSend(_clock.UtcNow)){failures.Add($"{adapter.Channel}: rate limited"); continue;}
                try
                {
                    var result = await adapter.DeliverAsync(report).ConfigureAwait(false);
                    if(result.Success){if(outbound)_rateLimiter.RecordSend(_clock.UtcNow);} else failures.Add($"{adapter.Channel}: {result.Message}");
                }
                catch(NotImplementedException ex){failures.Add($"{adapter.Channel}: {ex.Message}");}
            }
            return failures.Count==0 ? new BugReportDeliveryResult(true,"Router","Delivered") : new BugReportDeliveryResult(false,"Router",string.Join("; ",failures));
        }
        private IEnumerable<IBugReportDeliveryAdapter> Routes(BugSeverity severity)
        {
            if(severity==BugSeverity.Critical || severity==BugSeverity.High){yield return _email; yield return _github; yield break;}
            if(severity==BugSeverity.Normal || severity==BugSeverity.Low){yield return _github; yield break;}
            yield return _fileDrop;
        }
    }
}
