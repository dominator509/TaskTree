// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.TaskEngine/TaskEngine.cs
//  Purpose: Hierarchical task-tree CRUD implementing ITaskEngine per §4.2.
//  Architecture.md References: §4.2, §4.5, §4.6, §5.3, §10.5, §15
//  Roadmap.md References: Phase 1A — TaskEngine
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: ITaskEngine surface matches §4.2 verbatim.
//  D5 anti-drift: no TODOs; all behavior is implemented per PHASE1A-DERIVATIONS.md.
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Core.Enums;
using TaskStatus = TaskTree.Core.Enums.TaskStatus;

namespace TaskTree.Modules.TaskEngine;

/// <summary>
/// Hierarchical task-tree CRUD implementation per Architecture §4.2.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1A: 7 behavioral decisions not specified verbatim in
/// Architecture.md; derived from documented usage and approved by human
/// owner on 2026-05-26. See docs/spec-derivations/PHASE1A-DERIVATIONS.md.
/// <para>
/// Derivations applied here:
/// (1) DeleteAsync cascades to all descendants.
/// (2) AddAsync with non-existent parentId throws InvalidOperationException.
/// (3) AddAsync auto-assigns Guid.NewGuid() when node.Id == Guid.Empty.
/// (4) Cross-cutting audit dep: IComplianceCore injected per §4.6 even
///     though §3.2 module graph does not show the TaskEngine→ComplianceCore
///     edge (load-bearing rule is §4.6 "All modules emit audit events").
/// (5) Flat Dictionary&lt;Guid, TaskNode&gt; in SecureStore under key
///     "tasks/tree"; tree assembled at read time.
/// (6) Concurrency: single SemaphoreSlim(1,1) guarding all operations.
/// (7) Timestamps: CreatedAt set only when default; ModifiedAt always
///     refreshed on AddAsync/UpdateAsync.
/// </para>
/// </remarks>
public sealed class TaskEngine : ITaskEngine
{
    /// <summary>SecureStore key under which the flat task map is persisted.</summary>
    public const string StorageKey = "tasks/tree";

    private readonly ISecureStore _store;
    private readonly IClock _clock;
    private readonly IAppLogger _logger;
    private readonly IComplianceCore _compliance;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Initializes a new <see cref="TaskEngine"/>. All four dependencies are
    /// required; <paramref name="compliance"/> is mandatory per Derivation 4
    /// (§4.6 cross-cutting audit rule).
    /// </summary>
    public TaskEngine(ISecureStore store, IClock clock, IAppLogger logger, IComplianceCore compliance)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
    }

    /// <inheritdoc />
    public event EventHandler<TaskNode>? TaskAdded;

    /// <inheritdoc />
    public event EventHandler<TaskNode>? TaskCompleted;

    // Storage DTO — wraps Dictionary<Guid, TaskNode> for System.Text.Json
    // round-trip via ISecureStore (Derivation 5).
    private sealed class TaskMap
    {
        public Dictionary<Guid, TaskNode> Nodes { get; set; } = new();
    }

    private async Task<TaskMap> LoadMapAsync()
    {
        var map = await _store.LoadAsync<TaskMap>(StorageKey).ConfigureAwait(false);
        return map ?? new TaskMap();
    }

    private Task SaveMapAsync(TaskMap map) => _store.SaveAsync(StorageKey, map);

    private static TaskNode CloneForStorage(TaskNode src) => new()
    {
        Id = src.Id,
        ParentId = src.ParentId,
        Title = src.Title,
        Priority = src.Priority,
        Deadline = src.Deadline,
        Status = src.Status,
        CreatedAt = src.CreatedAt,
        ModifiedAt = src.ModifiedAt,
        Metadata = src.Metadata,
        // Children is reconstructed at read time; storage form omits nesting.
        Children = new List<TaskNode>(),
    };

    private async Task WriteAuditAsync(string action, Guid targetId, string result)
    {
        var entry = new AuditEntry
        {
            Timestamp = _clock.UtcNow,
            // AuditChainWriter supplies the current Windows user SID when the
            // caller does not provide an explicit actor.
            Actor = string.Empty,
            Module = nameof(TaskEngine),
            Action = action,
            TargetId = targetId,
            Result = result,
            // Seq / PrevHash / Hash are assigned by ComplianceCore in Phase 1C.
        };
        await _compliance.AuditAsync(entry).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new <see cref="TaskNode"/> to the tree. If <paramref name="parentId"/>
    /// is not <c>null</c>, the parent must exist or <see cref="InvalidOperationException"/>
    /// is thrown (Derivation 2). Auto-assigns <see cref="TaskNode.Id"/> when the
    /// supplied node has <see cref="Guid.Empty"/> (Derivation 3). Persists,
    /// audits, and raises <see cref="TaskAdded"/>.
    /// </summary>
    public async Task<TaskNode> AddAsync(TaskNode node, Guid? parentId = null)
    {
        ArgumentNullException.ThrowIfNull(node);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var map = await LoadMapAsync().ConfigureAwait(false);

            if (parentId.HasValue && !map.Nodes.ContainsKey(parentId.Value))
                throw new InvalidOperationException(
                    $"Cannot add node under parent {parentId.Value}: parent does not exist.");

            var now = _clock.UtcNow;
            if (node.Id == Guid.Empty) node.Id = Guid.NewGuid();
            node.ParentId = parentId;
            if (node.CreatedAt == default) node.CreatedAt = now;
            node.ModifiedAt = now;

            if (map.Nodes.ContainsKey(node.Id))
                throw new InvalidOperationException(
                    $"Cannot add node {node.Id}: identifier already exists.");

            map.Nodes[node.Id] = CloneForStorage(node);
            await SaveMapAsync(map).ConfigureAwait(false);
            await WriteAuditAsync("TaskAdded", node.Id, "success").ConfigureAwait(false);
            _logger.LogInformation("TaskEngine.AddAsync persisted node {0}", node.Id);
        }
        finally
        {
            _gate.Release();
        }

        TaskAdded?.Invoke(this, node);
        return node;
    }

    /// <summary>
    /// Updates an existing node. Preserves <see cref="TaskNode.CreatedAt"/>,
    /// refreshes <see cref="TaskNode.ModifiedAt"/>, and raises
    /// <see cref="TaskCompleted"/> only on the Active→Done transition
    /// (idempotent Done→Done updates do NOT re-raise).
    /// </summary>
    public async Task<TaskNode> UpdateAsync(TaskNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        bool fireCompleted = false;
        TaskNode persistedSnapshot;

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var map = await LoadMapAsync().ConfigureAwait(false);

            if (!map.Nodes.TryGetValue(node.Id, out var existing))
                throw new InvalidOperationException(
                    $"Cannot update node {node.Id}: not found.");

            var previousStatus = existing.Status;

            node.CreatedAt = existing.CreatedAt;
            node.ModifiedAt = _clock.UtcNow;

            map.Nodes[node.Id] = CloneForStorage(node);
            await SaveMapAsync(map).ConfigureAwait(false);
            await WriteAuditAsync("TaskUpdated", node.Id, "success").ConfigureAwait(false);
            _logger.LogInformation("TaskEngine.UpdateAsync persisted node {0}", node.Id);

            fireCompleted = previousStatus != TaskStatus.Done && node.Status == TaskStatus.Done;
            if (fireCompleted)
            {
                await WriteAuditAsync("TaskCompleted", node.Id, "success").ConfigureAwait(false);
            }

            persistedSnapshot = node;
        }
        finally
        {
            _gate.Release();
        }

        if (fireCompleted) TaskCompleted?.Invoke(this, persistedSnapshot);
        return persistedSnapshot;
    }

    /// <summary>
    /// Deletes the node and ALL its descendants (cascade — Derivation 1).
    /// Writes one audit entry per deleted node.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var map = await LoadMapAsync().ConfigureAwait(false);
            if (!map.Nodes.ContainsKey(id))
                throw new InvalidOperationException($"Cannot delete node {id}: not found.");

            // Build a child-index once for O(n) cascade computation.
            var childIndex = new Dictionary<Guid, List<Guid>>();
            foreach (var kvp in map.Nodes)
            {
                if (kvp.Value.ParentId is Guid pid)
                {
                    if (!childIndex.TryGetValue(pid, out var bucket))
                    {
                        bucket = new List<Guid>();
                        childIndex[pid] = bucket;
                    }
                    bucket.Add(kvp.Key);
                }
            }

            var toDelete = new List<Guid>();
            var stack = new Stack<Guid>();
            stack.Push(id);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                toDelete.Add(current);
                if (childIndex.TryGetValue(current, out var children))
                {
                    foreach (var c in children) stack.Push(c);
                }
            }

            foreach (var victim in toDelete)
                map.Nodes.Remove(victim);

            await SaveMapAsync(map).ConfigureAwait(false);

            foreach (var victim in toDelete)
                await WriteAuditAsync("TaskDeleted", victim, "success").ConfigureAwait(false);

            _logger.LogInformation("TaskEngine.DeleteAsync removed {0} node(s) rooted at {1}", toDelete.Count, id);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Returns a snapshot of the full hierarchical task tree, assembled from
    /// the flat storage representation (Derivation 5).
    /// </summary>
    public async Task<IReadOnlyList<TaskNode>> GetTreeAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var map = await LoadMapAsync().ConfigureAwait(false);
            if (map.Nodes.Count == 0) return Array.Empty<TaskNode>();

            // Clone every node into an in-memory tree (callers must not mutate storage).
            var clones = map.Nodes.Values.ToDictionary(n => n.Id, n => new TaskNode
            {
                Id = n.Id,
                ParentId = n.ParentId,
                Title = n.Title,
                Priority = n.Priority,
                Deadline = n.Deadline,
                Status = n.Status,
                CreatedAt = n.CreatedAt,
                ModifiedAt = n.ModifiedAt,
                Metadata = n.Metadata,
                Children = new List<TaskNode>(),
            });

            var roots = new List<TaskNode>();
            foreach (var node in clones.Values)
            {
                if (node.ParentId is Guid pid && clones.TryGetValue(pid, out var parent))
                {
                    parent.Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }
            return roots;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Returns nodes whose <see cref="TaskNode.Deadline"/> is non-null and
    /// strictly earlier than <paramref name="now"/>, AND whose
    /// <see cref="TaskNode.Status"/> is not <see cref="TaskStatus.Done"/>.
    /// </summary>
    public async Task<IReadOnlyList<TaskNode>> GetOverdueAsync(DateTimeOffset now)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var map = await LoadMapAsync().ConfigureAwait(false);
            var result = new List<TaskNode>();
            foreach (var n in map.Nodes.Values)
            {
                if (n.Status == TaskStatus.Done) continue;
                if (n.Deadline is DateTimeOffset dl && dl < now)
                {
                    // Return a clone to prevent caller mutation of storage state.
                    result.Add(new TaskNode
                    {
                        Id = n.Id,
                        ParentId = n.ParentId,
                        Title = n.Title,
                        Priority = n.Priority,
                        Deadline = n.Deadline,
                        Status = n.Status,
                        CreatedAt = n.CreatedAt,
                        ModifiedAt = n.ModifiedAt,
                        Metadata = n.Metadata,
                        Children = new List<TaskNode>(),
                    });
                }
            }
            return result;
        }
        finally
        {
            _gate.Release();
        }
    }
}
