// SPEC-DERIVED-PHASE2F  HALT #5/#6/#7/#8/#9/#10
// Gap #169: Real Windows session event hook deferred to Codex Phase 5E.
// Gap #171/#172: Compliance policy must document SessionLock audit vocabulary.

using System;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.SessionLock
{
    public sealed class SessionLockService : ISessionLockService, IDisposable
    {
        private readonly IClock _clock;
        private readonly IComplianceCore _compliance;
        private readonly IAppLogger _logger;
        private bool _running;
        private bool _disposed;

        public event EventHandler<SessionLockChangedEventArgs>? SessionLockChanged;
        public bool IsLocked { get; private set; }

        public SessionLockService(IClock clock, IComplianceCore compliance, IAppLogger logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken ct)
        {
            ThrowIfDisposed();
            if (_running) throw new InvalidOperationException("SessionLockService already running.");
            _logger.LogWarning("HIGH: Windows session lock hook deferred - Codex Phase 5E");
            _running = true;
            await AuditAsync("SessionLockStarted").ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            ThrowIfDisposed();
            if (!_running) return;
            _running = false;
            await AuditAsync("SessionLockStopped").ConfigureAwait(false);
        }

        internal async Task RaiseLockedForTestsAsync() => await SetLockedAsync(true).ConfigureAwait(false);
        internal async Task RaiseUnlockedForTestsAsync() => await SetLockedAsync(false).ConfigureAwait(false);

        private async Task SetLockedAsync(bool locked)
        {
            ThrowIfDisposed();
            if (IsLocked == locked) return;
            IsLocked = locked;
            var action = locked ? "SessionLocked" : "SessionUnlocked";
            await AuditAsync(action).ConfigureAwait(false);
            SessionLockChanged?.Invoke(this, new SessionLockChangedEventArgs(locked, _clock.UtcNow));
        }

        private Task AuditAsync(string action) => _compliance.AuditAsync(new AuditEntry
        {
            Module = "SessionLockService",
            Action = action,
            Result = "success",
            Timestamp = _clock.UtcNow,
        });

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SessionLockService));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
