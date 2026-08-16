// SPEC-DERIVED-PHASE2F  HALT #5/#6/#7/#8/#9/#10
// Gap #169: Real Windows session event hook deferred to Codex Phase 5E.
// Gap #171/#172: Compliance policy must document SessionLock audit vocabulary.

using System;
using System.Runtime.InteropServices;
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
        private readonly SemaphoreSlim _stateGate = new(1, 1);
        private readonly SemaphoreSlim _lifecycleOperationGate = new(1, 1);
        private readonly object _lifecycleGate = new();
        private bool _running;
        private bool _disposed;
        private Timer? _sessionMonitorTimer;

        private const uint DesktopSwitchDesktop = 0x0100;
        private const int ErrorAccessDenied = 5;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint desiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseDesktop(IntPtr desktop);

        public event EventHandler<SessionLockChangedEventArgs>? SessionLockChanged;
        public bool IsLocked { get; private set; }

        public SessionLockService(IClock clock, IComplianceCore compliance, IAppLogger logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _compliance.AutoLogoffTriggered += OnAutoLogoffTriggered;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await _lifecycleOperationGate.WaitAsync(ct).ConfigureAwait(false);
            Timer? startedTimer = null;
            try
            {
                lock (_lifecycleGate)
                {
                    ThrowIfDisposed();
                    if (_running) throw new InvalidOperationException("SessionLockService already running.");
                    _running = true;
                    startedTimer = new Timer(CheckSessionState, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    _sessionMonitorTimer = startedTimer;
                }

                try
                {
                    await AuditAsync("SessionLockStarted").ConfigureAwait(false);
                }
                catch
                {
                    lock (_lifecycleGate)
                    {
                        if (ReferenceEquals(_sessionMonitorTimer, startedTimer))
                        {
                            _sessionMonitorTimer = null;
                            _running = false;
                        }
                    }

                    startedTimer.Dispose();
                    throw;
                }
            }
            finally
            {
                _lifecycleOperationGate.Release();
            }
        }

        public async Task StopAsync()
        {
            await _lifecycleOperationGate.WaitAsync().ConfigureAwait(false);
            try
            {
                Timer? timer;
                lock (_lifecycleGate)
                {
                    ThrowIfDisposed();
                    if (!_running) return;
                    timer = _sessionMonitorTimer;
                    _sessionMonitorTimer = null;
                    _running = false;
                }

                timer?.Dispose();
                await AuditAsync("SessionLockStopped").ConfigureAwait(false);
            }
            finally
            {
                _lifecycleOperationGate.Release();
            }
        }

        internal async Task RaiseLockedForTestsAsync() => await SetLockedAsync(true).ConfigureAwait(false);
        internal async Task RaiseUnlockedForTestsAsync() => await SetLockedAsync(false).ConfigureAwait(false);

        private async Task SetLockedAsync(bool locked)
        {
            ThrowIfDisposed();
            await _stateGate.WaitAsync().ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();
                if (IsLocked == locked) return;

                IsLocked = locked;
                var action = locked ? "SessionLocked" : "SessionUnlocked";
                await AuditAsync(action).ConfigureAwait(false);

                // Disposal may race a timer callback that was already in flight.
                // Do not publish a post-disposal UI state transition.
                lock (_lifecycleGate)
                {
                    if (!_disposed)
                        SessionLockChanged?.Invoke(this, new SessionLockChangedEventArgs(locked, _clock.UtcNow));
                }
            }
            finally
            {
                _stateGate.Release();
            }
        }

        private void CheckSessionState(object? state)
        {
            lock (_lifecycleGate)
            {
                if (_disposed || !_running) return;
            }
            var desktop = OpenInputDesktop(0, false, DesktopSwitchDesktop);
            if (desktop != IntPtr.Zero)
            {
                CloseDesktop(desktop);
                _ = ApplyObservedLockStateAsync(false);
                return;
            }

            if (Marshal.GetLastWin32Error() == ErrorAccessDenied)
                _ = ApplyObservedLockStateAsync(true);
        }

        private async Task ApplyObservedLockStateAsync(bool locked)
        {
            try
            {
                await SetLockedAsync(locked).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session switch handling failed: {0}: {1}", ex.GetType().Name, ex.Message);
            }
        }

        private void OnAutoLogoffTriggered(object? sender, EventArgs e)
        {
            _ = ApplyObservedLockStateAsync(locked: true);
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
            if (Volatile.Read(ref _disposed)) throw new ObjectDisposedException(nameof(SessionLockService));
        }

        public void Dispose()
        {
            _lifecycleOperationGate.Wait();
            try
            {
                Timer? timer;
                lock (_lifecycleGate)
                {
                    if (_disposed) return;
                    Volatile.Write(ref _disposed, true);
                    _running = false;
                    timer = _sessionMonitorTimer;
                    _sessionMonitorTimer = null;
                }

                timer?.Dispose();
                _compliance.AutoLogoffTriggered -= OnAutoLogoffTriggered;
            }
            finally
            {
                _lifecycleOperationGate.Release();
            }
        }
    }
}
