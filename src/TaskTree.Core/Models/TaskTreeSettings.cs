// SPEC-DERIVED-PHASE2E  HALT #1/#4/#8
// Non-PHI settings model. Gap #152 requires Phase 4A/5F PHI-surface review.

using TaskTree.Core.Enums;

namespace TaskTree.Core.Models
{
    public sealed record TaskTreeSettings(
        ThemePreference ThemePreference,
        bool StartWithWindows,
        bool MinimizeToTrayOnClose,
        bool EnableReminderSounds,
        int ReminderSnoozeMinutes,
        bool ShowCompletedTasks)
    {
        public static TaskTreeSettings Default => new(
            ThemePreference.System,
            StartWithWindows: false,
            MinimizeToTrayOnClose: true,
            EnableReminderSounds: false,
            ReminderSnoozeMinutes: 10,
            ShowCompletedTasks: false);
    }
}
