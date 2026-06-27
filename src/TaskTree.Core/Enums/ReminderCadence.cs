// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Enums/ReminderCadence.cs
//  Purpose: Cadence band that produced a reminder firing per §5.3.
//  Architecture.md References: §5.3, §4.3, §2G, Roadmap P1D
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 4 — Enums)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: values match Architecture.md verbatim where specified (or SPEC-DERIVED-MSG4).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

namespace TaskTree.Core.Enums;

/// <summary>
/// Cadence band that produced a reminder firing — mirrors §5.3 cadence table
/// rows by priority.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG4: enumeration not specified verbatim in Architecture.md;
/// derived from documented usage and approved by human owner on 2026-05-26.
/// See docs/spec-derivations/PHASE0-MSG4-DERIVATIONS.md.
/// <para>
/// Architecture.md §5.3 describes cadence bands by priority but does not name
/// enum values. Values mirror <see cref="Priority"/> (Msg 4) 1:1 —
/// Critical/High/Normal/Low/Trivial — because §5.3 binds cadence directly to
/// priority. This is NOT the same type as <see cref="Priority"/>:
/// <see cref="Priority"/> is the user-assigned task attribute;
/// <see cref="ReminderCadence"/> is the scheduler output recorded on
/// <c>ReminderEvent.Cadence</c> (Msg 3) so consumers can route by which §5.3
/// band actually fired.
/// </para>
/// </remarks>
public enum ReminderCadence
{
    /// <summary>Priority-1 cadence band (§5.3 row 1).</summary>
    Critical = 1,

    /// <summary>Priority-2 cadence band (§5.3 row 2).</summary>
    High = 2,

    /// <summary>Priority-3 cadence band (§5.3 row 3).</summary>
    Normal = 3,

    /// <summary>Priority-4 cadence band (§5.3 row 4).</summary>
    Low = 4,

    /// <summary>Priority-5 cadence band (§5.3 row 5).</summary>
    Trivial = 5,
}
