// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Enums/Priority.cs
//  Purpose: Task priority bands per Architecture §5.3 cadence table.
//  Architecture.md References: §5.3, §1.1, §2 F2
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 4 — Enums)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: values match Architecture.md verbatim where specified (or SPEC-DERIVED-MSG4).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

namespace TaskTree.Core.Enums;

/// <summary>
/// Priority bands (1–5) driving the reminder cadence per Architecture §5.3.
/// </summary>
/// <remarks>
/// Backing integers (1..5) are normative — they match the §5.3 cadence table
/// row order.
/// </remarks>
public enum Priority
{
    /// <summary>P1 — repeat every 5 min; escalates after 15 min overdue (§5.3).</summary>
    Critical = 1,

    /// <summary>P2 — initial 30 min before deadline; repeat every 15 min (§5.3).</summary>
    High = 2,

    /// <summary>P3 — initial 1 hour before deadline; repeat every 30 min (§5.3).</summary>
    Normal = 3,

    /// <summary>P4 — initial 4 hours before deadline; repeat every 2 hours (§5.3).</summary>
    Low = 4,

    /// <summary>P5 — at deadline; repeat every 8 hours; silent badge only (§5.3).</summary>
    Trivial = 5,
}
