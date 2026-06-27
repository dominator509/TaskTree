// ============================================================================
// File: src/TaskTree.Orchestrator/ToastTier1Adapter.cs
// Architecture §7 Tier 1 (Windows Toast Notification) - preferred path
// Roadmap Sub-Phase 1G P1G-AC3 HIGH-stub
// SPEC-DERIVED-PHASE1G  HALT #5 (Gap #79), HALT #8 (ctor IAppLogger)
// Phase 5E Codex replaces with Microsoft.Toolkit.Uwp.Notifications / Windows.UI.Notifications.
// ============================================================================

using System;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator
{
    /// <summary>Tier 1 adapter - Windows Toast Notification API. HIGH-stub.</summary>
    public sealed class ToastTier1Adapter : IDisposable
    {
        private readonly IAppLogger _logger;
        private bool _disposed;

        public ToastTier1Adapter(IAppLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>HIGH-stub. Always throws. ReminderDeliveryService catches and falls through.</summary>
        public bool TryDeliver(ReminderEvent evt)
        {
            ThrowIfDisposed();
            throw new NotImplementedException(
                "HIGH: Windows Toast API requires live env - Codex Phase 5E");
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
