// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Enums/UpdateChannel.cs
//  Purpose: Auto-updater channel per Architecture §9.1 and §9.1.2.
//  Architecture.md References: §9.1, §9.1.2, §4.7
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 4 — Enums)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: values match Architecture.md verbatim where specified.
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

namespace TaskTree.Core.Enums;

/// <summary>
/// Update channel selected by the user for the auto-updater per §9.1.
/// </summary>
/// <remarks>
/// Wire-format note: §9.1.2 JSON manifest carries
/// <c>"channel":"stable"|"beta"</c> as a lower-case string;
/// <c>UpdateManifest.Channel</c> (Msg 3) is typed as <c>string</c> to match the
/// JSON verbatim, and conversion to this enum happens at the AutoUpdater
/// boundary in Phase 3A.
/// </remarks>
public enum UpdateChannel
{
    /// <summary>Production-quality release channel.</summary>
    Stable,

    /// <summary>Pre-release channel for early adopters.</summary>
    Beta,
}
