// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Enums/BugSeverity.cs
//  Purpose: Bug severity bands driving §9.2.4 routing rules.
//  Architecture.md References: §9.2.4, §9.2.1, §4.8
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 4 — Enums)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: values match Architecture.md verbatim where specified (or SPEC-DERIVED-MSG4).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

namespace TaskTree.Core.Enums;

/// <summary>
/// Bug severity bands (1–5) per Architecture §9.2.4.
/// </summary>
/// <remarks>
/// Backing integers (1..5) are normative — they match the §9.2.4 routing
/// table row order. Routing summary:
/// 1 = Email + GitHub (critical),
/// 2 = Email + GitHub (high),
/// 3 = GitHub (bug),
/// 4 = GitHub (enhancement),
/// 5 = Local file drop only.
/// </remarks>
public enum BugSeverity
{
    /// <summary>S1 — email + GitHub Issue labelled <c>critical</c> (§9.2.4).</summary>
    Critical = 1,

    /// <summary>S2 — email + GitHub Issue labelled <c>high</c> (§9.2.4).</summary>
    High = 2,

    /// <summary>S3 — GitHub Issue labelled <c>bug</c> (§9.2.4).</summary>
    Normal = 3,

    /// <summary>S4 — GitHub Issue labelled <c>enhancement</c> (§9.2.4).</summary>
    Low = 4,

    /// <summary>S5 — local file drop only (§9.2.4).</summary>
    Trivial = 5,
}
