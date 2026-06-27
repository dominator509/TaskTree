// SPEC-DERIVED-PHASE3B  HALT #1
// Architecture.md Section 9.1.1 updater state machine.
// Gap #222: Architecture Section 3.3 Enums must add UpdaterState.cs if Phase 3B ships.

namespace TaskTree.Core.Enums
{
    /// <summary>AutoUpdater state-machine states from Architecture.md Section 9.1.1.</summary>
    public enum UpdaterState
    {
        Idle = 0,
        Checking = 1,
        Downloading = 2,
        Verifying = 3,
        Staging = 4,
        Applying = 5,
        Applied = 6,
        Failed = 7,
    }
}
