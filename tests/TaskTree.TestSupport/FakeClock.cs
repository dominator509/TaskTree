// SPEC-DERIVED-PHASE2A HALT #6 (Gap #97 - Option B promotion)
// Relocated from tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs.
// Phase 5A MUST delete the old location (Gap #103).

using System;
using TaskTree.Core.Abstractions;

namespace TaskTree.TestSupport
{
    public sealed class FakeClock : IClock
    {
        private static readonly DateTimeOffset DefaultEpoch = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public FakeClock() : this(DefaultEpoch) { }
        public FakeClock(DateTimeOffset initial) { UtcNow = initial; }

        public DateTimeOffset UtcNow { get; set; }

        public void Advance(TimeSpan delta) => UtcNow += delta;
    }
}
