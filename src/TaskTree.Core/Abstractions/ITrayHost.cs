// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/ITrayHost.cs
//  Purpose: System-tray icon, context menu, hotkey hook per Architecture §4.1. Live wiring deferred to Codex Phase 5E.
//  Architecture.md References: §4.1, §7
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Owns the Windows notification-area icon, its context menu, and the global
/// hotkey hook. Surfaces user intent as events per Architecture.md §4.1.
/// </summary>
/// <remarks>
/// The runtime implementation depends on <c>H.NotifyIcon.Wpf</c> and Win32
/// <c>RegisterHotKey</c> PInvoke; per Roadmap 1E those concrete bindings are
/// stubbed and finalized by Codex during Phase 5E.
/// </remarks>
public interface ITrayHost : IDisposable
{
    /// <summary>Raised when the user requests the full task-tree window.</summary>
    event EventHandler ShowTreeRequested;

    /// <summary>Raised when the user requests the quick add-task entry point.</summary>
    event EventHandler AddTaskRequested;

    /// <summary>Raised when the user requests application exit.</summary>
    event EventHandler ExitRequested;

    /// <summary>Initializes the tray icon, context menu, and hotkey registration.</summary>
    void Initialize();

    /// <summary>Shows a transient tray balloon (Tier 3 reminder fallback per §7).</summary>
    /// <param name="title">Balloon title text.</param>
    /// <param name="message">Balloon body text.</param>
    void ShowBalloon(string title, string message);
}
