// ============================================================================
// File: src/TaskTree.Core/Enums/ReminderDeliveryTier.cs
// Architecture §7 Tier 1/2/3 Reminder Fallback Chain
// Roadmap Sub-Phase 1G (P1G-AC1 decision tree return value)
// SPEC-DERIVED-PHASE1G  HALT #9 (Gap #81 — Arch v1.0.2 5th change pending Msg 2)
// ============================================================================

namespace TaskTree.Core.Enums
{
    /// <summary>Identifies which reminder-delivery tier was selected per §7 fallback chain.</summary>
    public enum ReminderDeliveryTier
    {
        /// <summary>Windows Toast Notification - preferred per §7.</summary>
        WindowsToast = 0,
        /// <summary>WPF custom toast window fallback per §7.</summary>
        WpfCustom = 1,
        /// <summary>NotifyIcon balloon + icon flash - universal fallback per §7.</summary>
        TrayBalloon = 2,
    }
}
