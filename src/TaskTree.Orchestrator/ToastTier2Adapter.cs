// SPEC-DERIVED-PHASE1G  HALT #13
// SPEC-DERIVED-PHASE2B  ReminderToast integration
// SPEC-DERIVED-PHASE2F  HALT #14/#15 (session lock suppression)
// Gap #176: constructor changed for ISessionLockService; tests/factories must update.
// Gap #178: visible toast hide on lock needs real WPF integration test.

using System;
using System.Windows;
using System.Windows.Threading;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.UI.ViewModels;
using TaskTree.UI.Views;

namespace TaskTree.Orchestrator
{
    public sealed class ToastTier2Adapter
    {
        private readonly IAppLogger _logger;
        private readonly ISessionLockService _sessionLock;
        private ReminderToast? _window;
        private ToastViewModel? _viewModel;
        private DispatcherTimer? _closeTimer;

        public ToastTier2Adapter(IAppLogger logger, ISessionLockService sessionLock)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionLock = sessionLock ?? throw new ArgumentNullException(nameof(sessionLock));
            _sessionLock.SessionLockChanged += OnSessionLockChanged;
        }

        public bool TryDeliver(ReminderEvent evt)
        {
            if (_sessionLock.IsLocked)
            {
                _logger.LogWarning("Tier2 suppressed: session is locked.");
                return false;
            }
            if (Application.Current is null)
            {
                _logger.LogWarning("Tier2 unavailable: no WPF Application context.");
                return false;
            }
            _viewModel ??= new ToastViewModel();
            _window ??= new ReminderToast(_viewModel);
            _viewModel.UpdateContent(evt);
            _closeTimer ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _closeTimer.Tick -= OnCloseTimerTick;
            _closeTimer.Tick += OnCloseTimerTick;
            _closeTimer.Stop(); _closeTimer.Start();
            try { if (!_window.IsVisible) _window.Show(); else _window.Visibility = Visibility.Visible; return true; }
            catch (Exception ex) { _logger.LogError($"Tier2 WPF toast failed: {ex.GetType().Name}: {ex.Message}"); return false; }
        }

        private void OnSessionLockChanged(object? sender, SessionLockChangedEventArgs e)
        {
            if (!e.IsLocked) return;
            try { _closeTimer?.Stop(); _window?.Hide(); }
            catch (Exception ex) { _logger.LogError($"Tier2 hide-on-lock failed: {ex.GetType().Name}: {ex.Message}"); }
        }

        private void OnCloseTimerTick(object? sender, EventArgs e)
        { _closeTimer?.Stop(); if (_window is not null) _window.Hide(); }
    }
}
