// SPEC-DERIVED-PHASE3D  capture/queue/redaction foundation
// SPEC-DERIVED-PHASE3E  HALT #19/#20 FlushQueueAsync DeliveryRouter integration
// Architecture.md Sections 4.8 and 9.2.1-9.2.6.
// Gap #275/#276: constructor changes; queue flush removes only successful routes until live adapters exist.

using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    public sealed class BugReporter : IBugReporter
    {
        private readonly BugReportQueue _queue; private readonly RedactionPipeline _redaction; private readonly CrashCaptureHook _crashHook; private readonly IClock _clock; private readonly IAppLogger _logger; private readonly DeliveryRouter? _deliveryRouter;
        public BugReporter(BugReportQueue queue, RedactionPipeline redaction, CrashCaptureHook crashHook, IClock clock, IAppLogger logger, DeliveryRouter? deliveryRouter=null)
        { _queue=queue??throw new ArgumentNullException(nameof(queue)); _redaction=redaction??throw new ArgumentNullException(nameof(redaction)); _crashHook=crashHook??throw new ArgumentNullException(nameof(crashHook)); _clock=clock??throw new ArgumentNullException(nameof(clock)); _logger=logger??throw new ArgumentNullException(nameof(logger)); _deliveryRouter=deliveryRouter; }
        public bool RedactionEnabled { get; set; } = true;
        public async Task<Guid> SubmitAsync(BugReport report){if(report is null)throw new ArgumentNullException(nameof(report));if(!RedactionEnabled)_logger.LogWarning("RedactionEnabled=false requested; still redacting.");var redacted=_redaction.Redact(Normalize(report));await _queue.EnqueueAsync(redacted).ConfigureAwait(false);return redacted.Id;}
        public async Task<int> FlushQueueAsync()
        {
            if(_deliveryRouter is null){_logger.LogWarning("BugReporter delivery router not configured.");return 0;}
            var delivered=0;
            foreach(var report in await _queue.GetAllAsync().ConfigureAwait(false))
            {
                var result=await _deliveryRouter.DeliverAsync(report).ConfigureAwait(false);
                if(result.Success){await _queue.RemoveAsync(report.Id).ConfigureAwait(false);delivered++;}
            }
            return delivered;
        }
        public void HookGlobalCrashHandler(){_crashHook.CrashCaptured+=OnCrashCaptured;_crashHook.HookGlobalCrashHandler();}
        private async void OnCrashCaptured(object? sender, Exception ex){try{await SubmitAsync(new BugReport(Guid.NewGuid(),_clock.UtcNow,BugReportType.Crash,BugSeverity.High,ex.GetType().Name,new BugReportDescription("Application should not crash.",ex.ToString()),new BugReportEnvironment(Environment.OSVersion.VersionString,"unknown","unknown",UpdateChannel.Stable),Guid.NewGuid(),string.Empty,Array.Empty<BugReportAttachment>(),false)).ConfigureAwait(false);}catch(Exception captureEx){_logger.LogError(captureEx,"Crash capture queue failed: {0}: {1}",captureEx.GetType().Name,captureEx.Message);}}
        private BugReport Normalize(BugReport report){var desc=report.Description??new BugReportDescription(string.Empty,string.Empty);var env=report.Environment??new BugReportEnvironment(string.Empty,string.Empty,string.Empty,UpdateChannel.Stable);var fp=IsHex64(report.Fingerprint)?report.Fingerprint.ToUpperInvariant():ComputeFingerprint(report.Type,report.Severity,report.Title??string.Empty,desc.Actual??string.Empty);return report with{Id=report.Id==Guid.Empty?Guid.NewGuid():report.Id,Timestamp=report.Timestamp==default?_clock.UtcNow:report.Timestamp,CorrelationId=report.CorrelationId==Guid.Empty?Guid.NewGuid():report.CorrelationId,Description=desc,Environment=env,Attachments=report.Attachments??Array.Empty<BugReportAttachment>(),Fingerprint=fp};}
        internal static string ComputeFingerprint(BugReportType type,BugSeverity severity,string title,string actual)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{type}|{severity}|{title}|{actual}")));
        private static bool IsHex64(string? value){if(string.IsNullOrWhiteSpace(value)||value.Length!=64)return false;foreach(var c in value){var ok=(c>='0'&&c<='9')||(c>='a'&&c<='f')||(c>='A'&&c<='F');if(!ok)return false;}return true;}
    }
}
