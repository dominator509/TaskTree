// ============================================================================
// File: tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs
// Promotion trigger: PHASE1A §8 ("promote on 2nd+ consumer; canonical at 4th")
// 4 consumers as of Phase 1D Msg 1:
//   1. TaskEngineTests           (Phase 1A)
//   2. ComplianceCoreTests       (Phase 1C)
//   3. AuditChainWriterTests     (Phase 1C)
//   4. ReminderSchedulerTests    (Phase 1D Msg 2 — pending)
// ----------------------------------------------------------------------------
// Phase 1D HALT #1 — Option A (promote into existing TaskTree.Core.Tests project)
// Option B (new TaskTree.TestSupport project) re-evaluated on 5th consumer.
// See PHASE1D-DERIVATIONS.md §21 and HANDOFF v1.0.16 Cross-Phase Gap #40.
// ============================================================================

using System;
using TaskTree.Core.Abstractions;

namespace TaskTree.Core.Tests.TestDoubles
{
    /// <summary>
    /// Deterministic <see cref="IClock"/> for unit tests. Time advances only
    /// when callers explicitly mutate <see cref="UtcNow"/> or call
    /// <see cref="Advance"/>.
    /// </summary>
    public sealed class FakeClock : IClock
    {
        private static readonly DateTimeOffset DefaultEpoch =
            new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>Creates a clock pinned at 2026-01-01T00:00:00Z.</summary>
        public FakeClock() : this(DefaultEpoch) { }

        /// <summary>Creates a clock pinned at <paramref name="initial"/>.</summary>
        public FakeClock(DateTimeOffset initial)
        {
            UtcNow = initial;
        }

        /// <inheritdoc />
        public DateTimeOffset UtcNow { get; set; }

        /// <summary>Advances the clock by <paramref name="delta"/>.</summary>
        public void Advance(TimeSpan delta) => UtcNow += delta;
    }
}
