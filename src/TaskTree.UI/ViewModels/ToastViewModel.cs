// SPEC-DERIVED-PHASE2B  HALT #13/#14/#15
// SPEC-DERIVED-PHASE2D  HALT #4/#7 (Gap #120 closure: hard-coded brushes replaced by PriorityBrushProvider)

using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.UI.Themes;

namespace TaskTree.UI.ViewModels
{
    /// <summary>Tier 2 reminder toast ViewModel. Priority-aware title + themed priority brush.</summary>
    public sealed partial class ToastViewModel : ObservableObject
    {
        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private string message = string.Empty;
        [ObservableProperty] private Brush priorityColor = PriorityBrushProvider.TrivialBrush;
        [ObservableProperty] private bool isVisible;
        [ObservableProperty] private string reasonLabel = string.Empty;

        public void UpdateContent(ReminderEvent evt)
        {
            string nextTitle;
            if (evt.Reason == ReminderReason.Overdue)
            {
                nextTitle = evt.Priority == Priority.Trivial
                    ? "Trivial task - review when ready"
                    : $"{evt.Priority} task overdue";
            }
            else
            {
                nextTitle = $"{evt.Priority} task due";
            }

            Title = nextTitle;
            Message = $"Task {evt.TaskId} fired at {evt.FiredAtUtc:HH:mm:ss} UTC";
            ReasonLabel = (evt.Reason == ReminderReason.Overdue && evt.Priority == Priority.Trivial)
                ? string.Empty
                : $"({evt.Reason})";
            PriorityColor = PriorityBrushProvider.GetBrush(evt.Priority);
        }
    }
}
