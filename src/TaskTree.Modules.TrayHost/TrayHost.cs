// ============================================================================
// File: src/TaskTree.Modules.TrayHost/TrayHost.cs
// Module: TaskTree.Modules.TrayHost
// Implements: TaskTree.Core.Abstractions.ITrayHost (Architecture §4.1)
// Default hotkey: Architecture §13 Ctrl+Alt+T (deferred to Phase 2A HotkeyConfig)
// Audit schema: Architecture §10.5 (live wiring deferred to Codex Phase 5E)
// Gap classification: Architecture §21 Environment Gap (HIGH complexity)
// Roadmap: Sub-Phase 1E (P1E-AC1 compiles; P1E-AC2 events raise; P1E-AC3 stubbed)
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1E
//   HALT #2  ctor (IAppLogger, IComplianceCore) — 3rd LOAD-BEARING audit injection (Gap #56)
//   HALT #3  _compliance stored for Phase 5E live wiring; no AuditAsync in stub (Gap #57)
//   HALT #4  Canonical HIGH-stub text format for Phase 5E grep-based gap closure
//   HALT #5  Internal Raise*() methods for P1E-AC2 satisfaction (InternalsVisibleTo) — Gap #58
//   HALT #7  public sealed class TrayHost : ITrayHost, IDisposable
//   HALT #8  Real idempotent Dispose (no Win32 resources owned in stub state)
//   HALT #9  ShowBalloon validates params first, then throws NotImplementedException
//   HALT #10 No Initialize idempotency check in stub — Codex Phase 5E adds it (Gap #59)
//   HALT #12 _initialized field declared; Codex Phase 5E sets to true on success
// Public API surface UNCHANGED when Codex implements live functionality.
// See: docs/spec-derivations/PHASE1E-DERIVATIONS.md
// ============================================================================

using System;
using TaskTree.Core.Abstractions;

namespace TaskTree.Modules.TrayHost
{
    /// <summary>
    /// Tray icon + global hotkey host. Phase 1E HIGH-stub —
    /// <see cref="Initialize"/> and <see cref="ShowBalloon"/> throw
    /// <see cref="NotImplementedException"/> per D5; events are declared
    /// and raisable via internal <c>Raise*()</c> methods for test
    /// verification (P1E-AC2). Real NotifyIcon + RegisterHotKey wiring
    /// deferred to Codex Phase 5E.
    /// </summary>
    /// <remarks>
    /// Phase 1E scope per Roadmap Sub-Phase 1E. Public API surface unchanged
    /// when Codex implements live functionality at Phase 5E.
    /// </remarks>
    public sealed class TrayHost : ITrayHost, IDisposable
    {
        private readonly IAppLogger _logger;

        // HALT #3 — stored for Phase 5E live wiring; not invoked in stub (Gap #57).
        private readonly IComplianceCore _compliance;

        // HALT #12 — declared; Codex Phase 5E sets to true at end of Initialize().
        private bool _initialized;

        private bool _disposed;

        /// <summary>
        /// Creates a new TrayHost. All dependencies required; nulls throw
        /// <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <remarks>
        /// LOAD-BEARING for Phase 1F: <c>IComplianceCore</c> must be registered
        /// and injected. Cross-Phase Gap #56 (third audit-injection flag after
        /// TaskEngine R6 and ReminderScheduler HALT-Msg2 #2). Phase 5E will use
        /// <c>_compliance.AuditAsync</c> on every Show/Add/Exit event (Gap #57).
        /// </remarks>
        public TrayHost(IAppLogger logger, IComplianceCore compliance)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
        }

        /// <inheritdoc />
        public event EventHandler? ShowTreeRequested;

        /// <inheritdoc />
        public event EventHandler? AddTaskRequested;

        /// <inheritdoc />
        public event EventHandler? ExitRequested;

        /// <inheritdoc />
        /// <remarks>
        /// HIGH-stub per Roadmap §1E. Codex Phase 5E implements NotifyIcon
        /// creation + global hotkey registration via
        /// <see cref="HotkeyInterop.Register"/>; on success sets
        /// <c>_initialized = true</c>. Codex must also add idempotency guard
        /// (Gap #59) and AuditAsync calls (Gap #57).
        /// </remarks>
        public void Initialize()
        {
            ThrowIfDisposed();
            throw new NotImplementedException(
                "HIGH: NotifyIcon + RegisterHotKey require live env — Codex Phase 5E");
        }

        /// <inheritdoc />
        /// <remarks>
        /// HIGH-stub per Roadmap §1E. Param validation runs now (HALT #9) —
        /// null/empty/whitespace throws <see cref="ArgumentException"/>; the
        /// live balloon call is deferred to Codex Phase 5E.
        /// </remarks>
        public void ShowBalloon(string title, string message)
        {
            ThrowIfDisposed();
            if (title is null) throw new ArgumentNullException(nameof(title));
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(title));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty or whitespace.", nameof(message));
            throw new NotImplementedException(
                "HIGH: NotifyIcon balloon requires live env — Codex Phase 5E");
        }

        // --------------------------------------------------------------------
        // Test-only manual event raise (HALT #5 / Gap #58)
        // Accessible only to TaskTree.Modules.TrayHost.Tests via InternalsVisibleTo
        // (see Properties/AssemblyInfo.cs). Production callers MUST NOT use these.
        // --------------------------------------------------------------------

        /// <summary>Test-only raise of <see cref="ShowTreeRequested"/>.</summary>
        internal void RaiseShowTreeRequested() => ShowTreeRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>Test-only raise of <see cref="AddTaskRequested"/>.</summary>
        internal void RaiseAddTaskRequested() => AddTaskRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>Test-only raise of <see cref="ExitRequested"/>.</summary>
        internal void RaiseExitRequested() => ExitRequested?.Invoke(this, EventArgs.Empty);

        // --------------------------------------------------------------------
        // IDisposable (HALT #8 — real, idempotent)
        // --------------------------------------------------------------------

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TrayHost));
        }

        /// <summary>
        /// Idempotent disposal. In Phase 1E stub state, no Win32 resources are
        /// owned (NotifyIcon not created until <see cref="Initialize"/>, which
        /// throws). Codex Phase 5E will extend to dispose NotifyIcon and call
        /// <see cref="HotkeyInterop.Unregister"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            try { _logger.LogInformation("TrayHost disposed."); }
            catch { /* swallow logger failure during disposal */ }
            _disposed = true;
        }
    }
}
