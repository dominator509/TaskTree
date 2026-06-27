// ============================================================================
// File: src/TaskTree.Orchestrator/Views/ToastTier2Window.xaml.cs
// Architecture §7 Tier 2 (WPF custom toast window)
// SPEC-DERIVED-PHASE1G  HALT #6/#7/#14 (Gap #50/#55 Trivial-Overdue soften)
// Phase 2B replaces with TaskTree.UI/Views/ReminderToast.xaml (Gap #80).
// ============================================================================

using System.Windows;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator.Views
{
    /// <summary>Tier 2 WPF custom toast window - bottom-right anchored, non-activating.</summary>
    public partial class ToastTier2Window : Window
    {
        public ToastTier2Window()
        {
            InitializeComponent();
            ShowActivated = false;
            Loaded += (s, e) =>
            {
                var workingArea = SystemParameters.WorkArea;
                Left = workingArea.Right - Width - 20;
                Top = workingArea.Bottom - Height - 20;
            };
        }

        /// <summary>
        /// Updates visible content. HALT #14 priority-aware title:
        /// Overdue+Trivial -> "Trivial task - review when ready" (Gap #50/#55 soften);
        /// Overdue+other -> "{Priority} task overdue"; else -> "{Priority} task due".
        /// </summary>
        public void UpdateContent(ReminderEvent evt)
        {
            string title;
            if (evt.Reason == ReminderReason.Overdue)
            {
                if (evt.Priority == Priority.Trivial)
                    title = "Trivial task - review when ready";
                else
                    title = $"{evt.Priority} task overdue";
            }
            else
            {
                title = $"{evt.Priority} task due";
            }

            string message = $"Task {evt.TaskId} fired at {evt.FiredAtUtc:HH:mm:ss} UTC (Reason: {evt.Reason})";
            TitleText.Text = title;
            MessageText.Text = message;
        }
    }
}
