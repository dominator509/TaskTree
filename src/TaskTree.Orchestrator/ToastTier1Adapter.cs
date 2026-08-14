// ============================================================================
// File: src/TaskTree.Orchestrator/ToastTier1Adapter.cs
// Architecture §7 Tier 1 (Windows Toast Notification) - preferred path
// Roadmap Sub-Phase 1G P1G-AC3; live Windows toast capability probe
// SPEC-DERIVED-PHASE1G  HALT #5 (Gap #79), HALT #8 (ctor IAppLogger)
// Phase 5E keeps this dependency-free: WinRT toast activation is available only
// for an installed package identity, so the adapter fails closed when absent.
// ============================================================================

using System;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator
{
    /// <summary>Tier 1 adapter - Windows Toast Notification API.</summary>
    public sealed class ToastTier1Adapter : IDisposable
    {
        private readonly IAppLogger _logger;
        private bool _disposed;

        public ToastTier1Adapter(IAppLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Attempts the native toast path. This build has no packaged WinRT
        /// projection, so it reports unavailable and lets the delivery router
        /// fall through to the next tier.
        /// </summary>
        public bool TryDeliver(ReminderEvent evt)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(evt);
            _logger.LogDebug("Tier 1 toast unavailable: package identity is not provided by this desktop build.");
            return false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ToastTier1Adapter));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
