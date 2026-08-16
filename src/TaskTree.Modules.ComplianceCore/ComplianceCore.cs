// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.ComplianceCore/ComplianceCore.cs
//  Purpose: HIPAA technical-safeguards orchestrator implementing IComplianceCore per §4.6.
//  Architecture.md References: §4.6, §10.5, §10.7, §10.4, §9.2.3, §3.3
//  Roadmap.md References: Phase 1C — ComplianceCore baseline (Msg 1 of 5)
//  D1 anti-drift: header cites Architecture.md sections.
//  D5 anti-drift: StartIdleMonitor is backed by the Win32 idle/input APIs.
//  D10 anti-drift: XML doc on every public member.
//  Forward-references: PhiRedactor (Msg 2) and AuditChainWriter (Msg 3) ship later this phase.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.ComplianceCore;

/// <summary>
/// HIPAA technical-safeguards orchestrator implementing
/// <see cref="IComplianceCore"/> per Architecture §4.6. Delegates audit chain
/// operations to <c>AuditChainWriter</c> (Msg 3) and PHI redaction to
/// <c>PhiRedactor</c> (Msg 2). The idle monitor uses Win32 input telemetry and
/// requests a workstation lock when the configured timeout expires.
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
/// </remarks>
public sealed class ComplianceCore : IComplianceCore, IDisposable
{
    private readonly PhiRedactor _redactor;
    private readonly AuditChainWriter _auditWriter;
    private readonly IAppLogger _logger;
    private readonly object _lifecycleGate = new();
    private Timer? _idleTimer;
    private TimeSpan _idleTimeout;
    private int _idleTriggered;
    private bool _disposed;

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint TickCount;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLastInputInfo(ref LastInputInfo info);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();

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
        _logger = logger;
        _redactor = redactor ?? throw new ArgumentNullException(nameof(redactor));
        _auditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
    }

    /// <summary>
    /// Raised when the idle monitor exceeds the configured timeout.
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
    /// The monitor polls <c>GetLastInputInfo</c> once per second and requests a
    /// Windows workstation lock once per idle interval.
    /// </remarks>
    /// <param name="timeout">Inactivity threshold (default 15 minutes per §10.4).</param>
    public void StartIdleMonitor(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        Timer? previousTimer;
        lock (_lifecycleGate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ComplianceCore));
            _idleTimeout = timeout;
            Interlocked.Exchange(ref _idleTriggered, 0);
            previousTimer = _idleTimer;
            _idleTimer = new Timer(CheckIdleState, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        previousTimer?.Dispose();
    }

    /// <inheritdoc />
    public string RedactPhi(string text) => _redactor.Redact(text);

    private void CheckIdleState(object? state)
    {
        TimeSpan idleTimeout;
        lock (_lifecycleGate)
        {
            if (_disposed || _idleTimer is null) return;
            idleTimeout = _idleTimeout;
        }

        var info = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        if (!GetLastInputInfo(ref info)) return;

        var idleMilliseconds = unchecked((uint)Environment.TickCount - info.TickCount);
        var idle = TimeSpan.FromMilliseconds(idleMilliseconds);
        if (idle < idleTimeout)
        {
            Interlocked.Exchange(ref _idleTriggered, 0);
            return;
        }
        if (Interlocked.Exchange(ref _idleTriggered, 1) != 0) return;

        try
        {
            lock (_lifecycleGate)
            {
                if (_disposed || _idleTimer is null) return;
                AutoLogoffTriggered?.Invoke(this, EventArgs.Empty);
                if (_disposed || _idleTimer is null) return;
                if (!LockWorkStation())
                    _logger.LogWarning("LockWorkStation failed with Win32 error {0}.", Marshal.GetLastWin32Error());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Idle lock handling failed: {0}: {1}", ex.GetType().Name, ex.Message);
        }
    }

    /// <summary>Stops the idle monitor and releases its timer.</summary>
    public void Dispose()
    {
        Timer? timer;
        lock (_lifecycleGate)
        {
            if (_disposed) return;
            _disposed = true;
            timer = _idleTimer;
            _idleTimer = null;
        }

        timer?.Dispose();
    }
}
