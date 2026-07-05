// SPEC-DERIVED-PHASE2A HALT #6 (Gap #97 - Option B promotion)
// Relocated from tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs.
// Phase 5A MUST delete the old location (Gap #103).

using System;

namespace TaskTree.TestSupport
{
    /// <summary>
    /// Compatibility alias for the promoted <see cref="Clocks.FakeClock"/> test clock.
    /// </summary>
    public sealed class FakeClock : Clocks.FakeClock
    {
        private static readonly DateTimeOffset DefaultEpoch = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>Initializes the clock at the shared synthetic default epoch.</summary>
        public FakeClock() : this(DefaultEpoch) { }

        /// <summary>Initializes the clock at a specific synthetic timestamp.</summary>
        public FakeClock(DateTimeOffset initial) : base(initial) { }
    }
}
