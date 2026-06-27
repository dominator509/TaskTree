// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IClock.cs
//  Purpose: Injectable time source per Architecture §4.2 / §4.3 / §3.2.
//  Architecture.md References: §3.2, §4.2, §4.3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Injectable time source. Per Roadmap 1A anti-drift, all time-dependent
/// modules MUST read time exclusively through this abstraction and never via
/// <c>DateTime.Now</c>.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG2: surface not specified verbatim in Architecture.md; derived from
/// documented usage and approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG2-DERIVATIONS.md.
/// </remarks>
public interface IClock
{
    /// <summary>Gets the current UTC time as a <see cref="DateTimeOffset"/>.</summary>
    DateTimeOffset UtcNow { get; }
}
