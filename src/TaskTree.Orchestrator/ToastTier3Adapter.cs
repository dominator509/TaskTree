// ============================================================================
// File: src/TaskTree.Orchestrator/ToastTier3Adapter.cs
// Architecture §7 Tier 3 - universal fallback (NotifyIcon balloon + icon flash)
// Roadmap Sub-Phase 1G P1G-AC3 universal fallback
// SPEC-DERIVED-PHASE1G  HALT #5 (Gap #79), HALT #8 (ctor ITrayHost + IAppLogger)
// Phase 5E depends on Phase 1E TrayHost.ShowBalloon live (Gap #57).
// ============================================================================

using System;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator
{
    /// <summary>Tier 3 adapter - NotifyIcon balloon + icon flash. Universal fallback.</summary>
    public sealed class ToastTier3Adapter : IDisposable
    {
        private readonly ITrayHost _trayHost;
        private readonly IAppLogger _logger;
        private bool _disposed;

        public ToastTier3Adapter(ITrayHost trayHost, IAppLogger logger)
        {
            _trayHost = trayHost ?? throw new ArgumentNullException(nameof(trayHost));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool TryDeliver(ReminderEvent evt)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(evt);
            try
            {
                _trayHost.ShowBalloon("TaskTree reminder", "A task reminder is due.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Tier 3 reminder delivery failed: {0}: {1}", ex.GetType().Name, ex.Message);
                return false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ToastTier3Adapter));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
