// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Enums/TaskStatus.cs
//  Purpose: Lifecycle status of a task per Architecture §1.1 / §13 / §4.2 usage and Roadmap P1A-AC2.
//  Architecture.md References: §1.1, §13, §4.2, Roadmap P1A-AC2
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 4 — Enums)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: values match Architecture.md verbatim where specified (or SPEC-DERIVED-MSG4).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

namespace TaskTree.Core.Enums;

/// <summary>
/// Lifecycle status of a task in the tree.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG4: enumeration not specified verbatim in Architecture.md;
/// derived from documented usage and approved by human owner on 2026-05-26.
/// See docs/spec-derivations/PHASE0-MSG4-DERIVATIONS.md.
/// <para>
/// Architecture.md does not enumerate task statuses; the binary
/// "in-flight vs terminal" split is sufficient for every documented behavior:
/// §13 step 10 ("user marks complete"), Roadmap P1A-AC2
/// ("<c>UpdateAsync</c> raises <c>TaskCompleted</c> when status = <c>Done</c>"),
/// Roadmap 1A ("<c>GetOverdueAsync</c> returns past-deadline non-<c>Done</c>
/// nodes"). Dismissal/deletion is handled by <c>ITaskEngine.DeleteAsync</c>
/// per §4.2, not by a third status value.
/// </para>
/// </remarks>
public enum TaskStatus
{
    /// <summary>Task is in flight (not yet complete).</summary>
    Active = 0,

    /// <summary>Task is complete (terminal state).</summary>
    Done = 1,
}
