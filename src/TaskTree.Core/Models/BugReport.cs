// SPEC-DERIVED-MSG3
// SPEC-DERIVED-PHASE3D  HALT #1/#3
// Architecture.md Section 9.2.1 bug report payload schema.
// Gap #244/#246: verify schema compatibility and add exact JSON schema tests in Phase 5C.

using System;
using System.Collections.Generic;
using TaskTree.Core.Enums;

namespace TaskTree.Core.Models
{
    /// <summary>PHI-safe bug report payload matching Architecture.md Section 9.2.1.</summary>
    public sealed record BugReport(
        Guid Id,
        DateTimeOffset Timestamp,
        BugReportType Type,
        BugSeverity Severity,
        string Title,
        BugReportDescription Description,
        BugReportEnvironment Environment,
        Guid CorrelationId,
        string Fingerprint,
        IReadOnlyList<BugReportAttachment> Attachments,
        bool Redacted);

    /// <summary>Expected/actual description block.</summary>
    public sealed record BugReportDescription(string Expected, string Actual);

    /// <summary>Environment block for bug reports.</summary>
    public sealed record BugReportEnvironment(string Os, string AppVersion, string Build, UpdateChannel Channel);

    /// <summary>Attachment metadata for redacted bug report attachments.</summary>
    public sealed record BugReportAttachment(string Name, bool Redacted, long SizeBytes);
}
