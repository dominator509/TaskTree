// =============================================================================
// TaskTree — FakeClock.cs (PROMOTED from inline private nested classes)
// Implements: Architecture.md §12 (IClock test-double), Phase 1A R7 promotion trigger
// Phase:      1D Msg 1 (HALT #1 Option B)
// SPEC-DERIVED-PHASE1D §1 (FakeClock promotion as canonical 4th-consumer trigger)
// PRIOR INLINE COPIES (must be deleted at Phase 5A repo stitch — see G1D-9):
//   - tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs
//   - tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs
//   - tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs
// =============================================================================
using System;
using TaskTree.Core.Abstractions;

namespace TaskTree.TestSupport.Clocks;

/// <summary>
/// Deterministic <see cref="IClock"/> implementation for tests.
/// Time advances only when <see cref="Advance"/> or <see cref="SetTo"/> is called.
/// </summary>
public sealed class FakeClock : IClock
{
    private DateTimeOffset _now;

    public FakeClock() : this(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)) { }

    public FakeClock(DateTimeOffset start) { _now = start; }

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _now;

    /// <summary>Advance the fake clock by the specified duration.</summary>
    public void Advance(TimeSpan by)
    {
        if (by < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(by), by, "FakeClock cannot move backwards.");
        _now = _now.Add(by);
    }

    /// <summary>Set the fake clock to an absolute time (must be >= current value).</summary>
    public void SetTo(DateTimeOffset newNow)
    {
        if (newNow < _now)
            throw new ArgumentOutOfRangeException(nameof(newNow), newNow, "FakeClock cannot move backwards.");
        _now = newNow;
    }
}
