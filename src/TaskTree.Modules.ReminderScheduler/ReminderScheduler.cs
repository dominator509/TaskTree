// ============================================================================
// File: src/TaskTree.Modules.ReminderScheduler/ReminderScheduler.cs
// Implements: IReminderScheduler (Architecture §4.3); Cadence binding §5.3
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1D
//   HALT #2/#3/#6/#9/#10/#11/#13 (Msg 1)
// SPEC-DERIVED-PHASE1D-MSG2 (test-support patches; public surface unchanged)
//   HALT-Msg2 #2  TickOnceAsync promoted to `internal` for direct test invocation
//   HALT-Msg2 #11 StopWaitTimeout promoted to `internal static` (mutable for test #8)
// Internal access granted ONLY to TaskTree.Modules.ReminderScheduler.Tests
// via Properties/AssemblyInfo.cs. Gap #48: production MUST NOT mutate StopWaitTimeout.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskStatus = TaskTree.Core.Enums.TaskStatus;

namespace TaskTree.Modules.ReminderScheduler
{
    public sealed class ReminderScheduler : IReminderScheduler, IDisposable
    {
        private static readonly TimeSpan MinCadence = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxCadence = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DefaultCadence = TimeSpan.FromSeconds(30);

        // HALT-Msg2 #11 — internal mutable for bounded-wait test only.
        internal static TimeSpan StopWaitTimeout = TimeSpan.FromSeconds(5);

        private readonly IClock _clock;
        private readonly ITaskEngine _taskEngine;
        private readonly IComplianceCore _compliance;
        private readonly IAppLogger _logger;
        private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
        private readonly Dictionary<Guid, DateTimeOffset> _lastFiredUtc = new();

        private TimeSpan _cadence = DefaultCadence;
        private bool _running;
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _internalCts;
        private Task? _loopTask;
        private bool _disposed;

        public ReminderScheduler(IClock clock, ITaskEngine taskEngine, IComplianceCore compliance, IAppLogger logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _taskEngine = taskEngine ?? throw new ArgumentNullException(nameof(taskEngine));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler<ReminderEvent>? ReminderDue;

        public TimeSpan Cadence
        {
            get => _cadence;
            set
            {
                if (value < MinCadence || value > MaxCadence)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Cadence must be between {MinCadence} and {MaxCadence}.");
                _cadence = value;
            }
        }

        public async Task StartAsync(CancellationToken ct)
        {
            ThrowIfDisposed();
            await _lifecycleGate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_running) throw new InvalidOperationException("ReminderScheduler is already running.");
                _internalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _timer = new PeriodicTimer(_cadence);
                _loopTask = RunLoopAsync(_internalCts.Token);
                _running = true;
                _logger.LogInformation($"ReminderScheduler started with cadence {_cadence}.");
            }
            finally { _lifecycleGate.Release(); }
        }

        public async Task StopAsync()
        {
            ThrowIfDisposed();
            await _lifecycleGate.WaitAsync().ConfigureAwait(false);
            CancellationTokenSource? ctsToDispose = null;
            PeriodicTimer? timerToDispose = null;
            Task? loopToAwait = null;
            try
            {
                if (!_running) return;
                _internalCts?.Cancel();
                ctsToDispose = _internalCts;
                timerToDispose = _timer;
                loopToAwait = _loopTask;
                _running = false;
                _timer = null;
                _internalCts = null;
                _loopTask = null;
            }
            finally { _lifecycleGate.Release(); }

            if (loopToAwait is not null)
            {
                var completed = await Task.WhenAny(loopToAwait, Task.Delay(StopWaitTimeout)).ConfigureAwait(false);
                if (completed != loopToAwait)
                    _logger.LogWarning($"ReminderScheduler.StopAsync: in-flight tick did not complete within {StopWaitTimeout}; abandoning wait.");
            }

            timerToDispose?.Dispose();
            ctsToDispose?.Dispose();
            _logger.LogInformation("ReminderScheduler stopped.");
        }

        private async Task RunLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var ok = await _timer!.WaitForNextTickAsync(ct).ConfigureAwait(false);
                        if (!ok) break;
                    }
                    catch (OperationCanceledException) { break; }

                    try { await TickOnceAsync(ct).ConfigureAwait(false); }
                    catch (Exception ex) { _logger.LogError(ex, "ReminderScheduler tick failed: {0}: {1}", ex.GetType().Name, ex.Message); }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "ReminderScheduler loop terminated unexpectedly: {0}: {1}", ex.GetType().Name, ex.Message); }
        }

        // HALT-Msg2 #2 — internal for direct test invocation. Gap #47: production MUST NOT call.
        internal async Task TickOnceAsync(CancellationToken ct)
        {
            var nowUtc = _clock.UtcNow;
            var tree = await _taskEngine.GetTreeAsync().ConfigureAwait(false);
            if (tree is null) return;

            foreach (var node in tree)
            {
                if (ct.IsCancellationRequested) break;
                if (node is null) continue;
                if (node.Status == TaskStatus.Done) continue;

                try
                {
                    DateTimeOffset? lastFired = _lastFiredUtc.TryGetValue(node.Id, out var t) ? t : null;
                    if (!CadencePolicy.ShouldFire(node, lastFired, nowUtc, out var reason)) continue;

                    _lastFiredUtc[node.Id] = nowUtc;

                    var evt = new ReminderEvent
                    {
                        TaskId = node.Id,
                        FiredAtUtc = nowUtc,
                        Reason = reason,
                        Priority = node.Priority,
                    };

                    ReminderDue?.Invoke(this, evt);

                    await _compliance.AuditAsync(new AuditEntry
                    {
                        Module = "ReminderScheduler",
                        Action = "ReminderFired",
                        TargetId = node.Id,
                        Result = "success",
                        Timestamp = nowUtc,
                    }).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ReminderScheduler: node {0} processing failed: {1}: {2}", node.Id, ex.GetType().Name, ex.Message);
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ReminderScheduler));
        }

        public void Dispose()
        {
            if (_disposed) return;
            try { StopAsync().GetAwaiter().GetResult(); }
            catch (Exception ex)
            {
                try { _logger.LogWarning($"ReminderScheduler.Dispose StopAsync threw: {ex.Message}"); } catch { }
            }
            try { _lifecycleGate.Dispose(); } catch { }
            try { _internalCts?.Dispose(); } catch { }
            try { _timer?.Dispose(); } catch { }
            _disposed = true;
        }
    }
}
