// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs
//  Purpose: HIPAA technical-safeguards orchestrator implementing IComplianceCore per §4.6.
//  Architecture.md References: §4.6, §10.5, §10.7, §10.4, §9.2.3, §3.3
//  Roadmap.md References: Phase 1C — ComplianceCore baseline (Msg 1 of 5)
//  D1 anti-drift: header cites Architecture.md sections.
//  D5 anti-drift: StartIdleMonitor throws NotImplementedException("Deferred to Phase 2F").
//  D10 anti-drift: XML doc on every public member.
//  Forward-references: PhiRedactor (Msg 2) and AuditChainWriter (Msg 3) ship later this phase.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.ComplianceCore;

/// <summary>
/// HIPAA technical-safeguards orchestrator implementing
/// <see cref="IComplianceCore"/> per Architecture §4.6. Delegates audit chain
/// operations to <c>AuditChainWriter</c> (Msg 3) and PHI redaction to
/// <c>PhiRedactor</c> (Msg 2). Idle monitor is stubbed per Roadmap 1C — full
/// implementation arrives in Phase 2F.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1C: surface not specified verbatim in Architecture.md;
/// derived from documented usage (§4.6 / §10.5 / §9.2.3) and approved by
/// human owner on 2026-05-26. See docs/spec-derivations/PHASE1C-DERIVATIONS.md.
/// <para>
/// Derivations applied here (full list in registry):
/// (1) Class structure: delegates to <c>PhiRedactor</c> + <c>AuditChainWriter</c>
///     (concrete sibling classes per PHASE1B §1 YAGNI precedent).
/// (5) <see cref="VerifyChainIntegrityAsync"/> returns <c>bool</c> with NO side
///     effects in Phase 1C. The §10.9 "user-visible warning + export" is
///     Phase 2B/2E + Phase 4A scope.
/// (8) <see cref="RedactPhi"/> null/empty contract: <c>null</c> → <c>string.Empty</c>;
///     <c>""</c> → <c>""</c>.
/// </para>
/// <para>
/// Idle monitor: <see cref="StartIdleMonitor"/> throws
/// <see cref="NotImplementedException"/> per Roadmap 1C anti-drift constraint.
/// Full Win32 <c>GetLastInputInfo</c> wiring ships in Phase 2F.
/// </para>
/// </remarks>
public sealed class ComplianceCore : IComplianceCore
{
    private readonly PhiRedactor _redactor;
    private readonly AuditChainWriter _auditWriter;

    /// <summary>
    /// Initializes a new <see cref="ComplianceCore"/>. All five dependencies
    /// are required. <paramref name="redactor"/> is the concrete
    /// <c>PhiRedactor</c> (Msg 2) and <paramref name="auditWriter"/> is the
    /// concrete <c>AuditChainWriter</c> (Msg 3) — both are concrete sibling
    /// classes per PHASE1B §1 YAGNI precedent.
    /// </summary>
    /// <param name="store">Encrypted persistence used by the audit chain.</param>
    /// <param name="clock">Injectable time source per Roadmap 1A anti-drift.</param>
    /// <param name="logger">Structured logger.</param>
    /// <param name="redactor">PHI redaction pipeline (Msg 2).</param>
    /// <param name="auditWriter">Hash-chained audit writer (Msg 3).</param>
    public ComplianceCore(
        ISecureStore store,
        IClock clock,
        IAppLogger logger,
        PhiRedactor redactor,
        AuditChainWriter auditWriter)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logger);
        _redactor = redactor ?? throw new ArgumentNullException(nameof(redactor));
        _auditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
    }

    /// <summary>
    /// Raised when the idle monitor exceeds the configured timeout. NOT raised
    /// in Phase 1C — the idle monitor itself is stubbed (Derivation 1; Phase 2F
    /// finishes the wiring).
    /// </summary>
    public event EventHandler? AutoLogoffTriggered;

    /// <inheritdoc />
    public Task AuditAsync(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        // Timestamp + Module fields may be pre-populated by caller (e.g.
        // TaskEngine per PHASE1A §4 audit pipeline); AuditChainWriter assigns
        // Seq/PrevHash/Hash and persists. Pass through unchanged.
        return _auditWriter.AppendAsync(entry);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AuditEntry>> GetAuditChainAsync()
        => _auditWriter.GetAllAsync();

    /// <inheritdoc />
    public Task<bool> VerifyChainIntegrityAsync()
        => _auditWriter.VerifyAsync();

    /// <summary>
    /// Starts the OS idle monitor with the supplied inactivity threshold.
    /// </summary>
    /// <remarks>
    /// Phase 1C: throws <see cref="NotImplementedException"/> per Roadmap 1C
    /// anti-drift constraint. Full implementation (Win32 <c>GetLastInputInfo</c>
    /// PInvoke + idle timer) ships in Phase 2F.
    /// </remarks>
    /// <param name="timeout">Inactivity threshold (default 15 minutes per §10.4).</param>
    public void StartIdleMonitor(TimeSpan timeout)
        => throw new NotImplementedException(
            "Deferred to Phase 2F per Roadmap 1C Anti-Drift Constraints. " +
            "IdleMonitor implementation will require Win32 GetLastInputInfo PInvoke (Codex Phase 5E).");

    /// <inheritdoc />
    public string RedactPhi(string text) => _redactor.Redact(text);

    // The AutoLogoffTriggered event is declared per §4.6 stub. Phase 2F
    // IdleMonitor will raise it. This private helper exists solely to silence
    // the C# CS0067 "event never used" warning under TreatWarningsAsErrors —
    // it is never invoked in Phase 1C.
    private void RaiseAutoLogoff_NeverCalledInPhase1C()
    {
        AutoLogoffTriggered?.Invoke(this, EventArgs.Empty);
    }
}
