// SPEC-DERIVED-PHASE3D  HALT #4/#5/#6/#7/#8
// Architecture.md Section 9.2.3 requires all free-text fields to pass through IComplianceCore.RedactPhi().
// Gap #247/#248: validate no unredacted free text is persisted; null normalization is derived.

using System;
using System.Linq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Redacts all BugReport free-text fields before persistence or delivery.</summary>
    public sealed class RedactionPipeline
    {
        private readonly IComplianceCore _compliance;
        public RedactionPipeline(IComplianceCore compliance) => _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
        public BugReport Redact(BugReport report)
        {
            if (report is null) throw new ArgumentNullException(nameof(report));
            var description = report.Description ?? new BugReportDescription(string.Empty, string.Empty);
            var environment = report.Environment ?? new BugReportEnvironment(string.Empty, string.Empty, string.Empty, report.Environment?.Channel ?? default);
            var attachments = (report.Attachments ?? Array.Empty<BugReportAttachment>())
                .Select(a => new BugReportAttachment(R(a.Name), true, a.SizeBytes))
                .ToArray();
            return report with
            {
                Title = R(report.Title),
                Description = new BugReportDescription(R(description.Expected), R(description.Actual)),
                Environment = new BugReportEnvironment(R(environment.Os), R(environment.AppVersion), R(environment.Build), environment.Channel),
                Attachments = attachments,
                Redacted = true,
            };
        }
        private string R(string? value) => _compliance.RedactPhi(value ?? string.Empty) ?? string.Empty;
    }
}
