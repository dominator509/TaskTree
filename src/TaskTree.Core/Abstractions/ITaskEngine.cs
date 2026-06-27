// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/ITaskEngine.cs
//  Purpose: Hierarchical task tree CRUD per Architecture §4.2.
//  Architecture.md References: §4.2, §3.3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Performs create / read / update / delete operations against the hierarchical
/// task tree, persists mutations via <see cref="ISecureStore"/>, and raises
/// lifecycle events as documented in Architecture.md §4.2.
/// </summary>
/// <remarks>
/// Signatures are verbatim from Architecture.md §4.2 stub. Per Roadmap rule D2
/// they MUST NOT be renamed. Time-dependent operations (e.g. overdue lookup)
/// MUST use the injected <see cref="IClock"/> — never <c>DateTime.Now</c>.
/// </remarks>
public interface ITaskEngine
{
    /// <summary>
    /// Adds a new <see cref="TaskNode"/> to the tree, optionally under a parent.
    /// Persists the mutation and raises <see cref="TaskAdded"/>.
    /// </summary>
    /// <param name="node">The node to add (must have a populated identifier).</param>
    /// <param name="parentId">
    /// Identifier of the desired parent node, or <c>null</c> to add at the root.
    /// </param>
    /// <returns>The persisted node (may include server-side defaults).</returns>
    Task<TaskNode> AddAsync(TaskNode node, Guid? parentId = null);

    /// <summary>
    /// Updates an existing node. If the new status transitions to <c>Done</c>,
    /// implementations raise <see cref="TaskCompleted"/>.
    /// </summary>
    /// <param name="node">The node carrying the updated state.</param>
    /// <returns>The persisted node after the update is applied.</returns>
    Task<TaskNode> UpdateAsync(TaskNode node);

    /// <summary>Deletes a node by identifier, removing its subtree.</summary>
    /// <param name="id">Identifier of the node to delete.</param>
    Task DeleteAsync(Guid id);

    /// <summary>Returns a snapshot of the full hierarchical task tree.</summary>
    /// <returns>An ordered, read-only list of top-level nodes (children nested within).</returns>
    Task<IReadOnlyList<TaskNode>> GetTreeAsync();

    /// <summary>
    /// Returns tasks whose deadline has passed and which are not yet
    /// in a terminal status.
    /// </summary>
    /// <param name="now">Reference time (supplied via <see cref="IClock"/>).</param>
    /// <returns>An ordered, read-only list of overdue nodes.</returns>
    Task<IReadOnlyList<TaskNode>> GetOverdueAsync(DateTimeOffset now);

    /// <summary>Raised after a node is successfully added.</summary>
    event EventHandler<TaskNode> TaskAdded;

    /// <summary>Raised when a node transitions to a completed state.</summary>
    event EventHandler<TaskNode> TaskCompleted;
}
