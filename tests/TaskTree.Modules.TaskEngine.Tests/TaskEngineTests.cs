// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Modules.TaskEngine.Tests/TaskEngineTests.cs
//  Purpose: 14 unit tests for TaskEngine per Architecture §4.2 and Roadmap P1A-AC1..AC5 + 7 PHASE1A derivations.
//  Architecture.md References: §4.2, §10.5, §15, §12
//  Roadmap.md References: Phase 1A — TaskEngine (Msg 2 of 2)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: all test data is synthetic, non-PHI-shaped.
//  D10 anti-drift: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.TaskEngine;
using TaskTree.TestSupport;
using TaskStatus = TaskTree.Core.Enums.TaskStatus;

namespace TaskTree.Modules.TaskEngine.Tests;

/// <summary>
/// Verifies <see cref="TaskEngine"/> behavior per Architecture §4.2 and Roadmap
/// P1A-AC1..AC5, plus all 7 derivations recorded in
/// docs/spec-derivations/PHASE1A-DERIVATIONS.md.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1A: this test file inherits the 7 behavioral derivations
/// declared in TaskEngine.cs and additionally introduces test infrastructure
/// conventions (shared FakeClock + InMemorySecureStore; Moq-mocked
/// IComplianceCore + IAppLogger) recorded as derivation §8 in
/// docs/spec-derivations/PHASE1A-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class TaskEngineTests
{
    // ─── Test infrastructure (private nested types) ─────────────────────────

    private sealed class TestContext
    {
        public TaskEngine Engine { get; init; } = default!;
        public InMemorySecureStore Store { get; init; } = default!;
        public FakeClock Clock { get; init; } = default!;
        public List<AuditEntry> AuditEntries { get; init; } = default!;
        public Mock<IComplianceCore> Compliance { get; init; } = default!;
        public Mock<IAppLogger> Logger { get; init; } = default!;
    }

    private static TestContext CreateEngine()
    {
        var store = new InMemorySecureStore(preserveObjectReferences: true);
        var clock = new FakeClock();
        var audit = new List<AuditEntry>();
        var compliance = new Mock<IComplianceCore>();
        compliance.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>()))
                  .Callback<AuditEntry>(e => audit.Add(e))
                  .Returns(Task.CompletedTask);
        var logger = new Mock<IAppLogger>();
        var engine = new TaskEngine(store, clock, logger.Object, compliance.Object);
        return new TestContext
        {
            Engine = engine,
            Store = store,
            Clock = clock,
            AuditEntries = audit,
            Compliance = compliance,
            Logger = logger,
        };
    }

    private static TaskNode NewNode(string title, Priority pri = Priority.Normal, DateTimeOffset? deadline = null) =>
        new() { Title = title, Priority = pri, Deadline = deadline, Status = TaskStatus.Active };

    // ─── Tests ──────────────────────────────────────────────────────────────

    /// <summary>P1A-AC1: AddAsync persists and raises TaskAdded.</summary>
    [TestMethod]
    public async Task AddAsync_RootNode_PersistsAndRaisesTaskAdded()
    {
        var ctx = CreateEngine();
        TaskNode? raised = null;
        ctx.Engine.TaskAdded += (_, n) => raised = n;

        var added = await ctx.Engine.AddAsync(NewNode("synthetic-root"));

        Assert.IsNotNull(raised);
        Assert.AreEqual(added.Id, raised!.Id);
        var tree = await ctx.Engine.GetTreeAsync();
        Assert.AreEqual(1, tree.Count);
    }

    /// <summary>P1A-AC1: AddAsync with parentId appends to parent's Children.</summary>
    [TestMethod]
    public async Task AddAsync_WithParent_AppendsChild()
    {
        var ctx = CreateEngine();
        var parent = await ctx.Engine.AddAsync(NewNode("synthetic-parent"));
        var child = await ctx.Engine.AddAsync(NewNode("synthetic-child"), parent.Id);

        var tree = await ctx.Engine.GetTreeAsync();
        Assert.AreEqual(1, tree.Count);
        Assert.AreEqual(parent.Id, tree[0].Id);
        Assert.AreEqual(1, tree[0].Children.Count);
        Assert.AreEqual(child.Id, tree[0].Children[0].Id);
    }

    /// <summary>Derivation 2: AddAsync with non-existent parentId throws InvalidOperationException.</summary>
    [TestMethod]
    public async Task AddAsync_NonExistentParent_Throws()
    {
        var ctx = CreateEngine();
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await ctx.Engine.AddAsync(NewNode("orphan"), Guid.NewGuid()));
    }

    /// <summary>Derivation 3: AddAsync auto-assigns Guid.NewGuid when Id is empty.</summary>
    [TestMethod]
    public async Task AddAsync_EmptyId_AssignsNewGuid()
    {
        var ctx = CreateEngine();
        var node = NewNode("synthetic-empty-id");
        Assert.AreEqual(Guid.Empty, node.Id);

        var added = await ctx.Engine.AddAsync(node);

        Assert.AreNotEqual(Guid.Empty, added.Id);
    }

    /// <summary>Derivation 3: AddAsync preserves the caller-supplied Id when provided.</summary>
    [TestMethod]
    public async Task AddAsync_PreservesCallerId_WhenProvided()
    {
        var ctx = CreateEngine();
        var explicitId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var node = NewNode("synthetic-fixed-id");
        node.Id = explicitId;

        var added = await ctx.Engine.AddAsync(node);

        Assert.AreEqual(explicitId, added.Id);
    }

    /// <summary>P1A-AC2: UpdateAsync raises TaskCompleted on Active→Done transition.</summary>
    [TestMethod]
    public async Task UpdateAsync_ActiveToDone_RaisesTaskCompleted()
    {
        var ctx = CreateEngine();
        TaskNode? completed = null;
        ctx.Engine.TaskCompleted += (_, n) => completed = n;
        var node = await ctx.Engine.AddAsync(NewNode("synthetic-active"));

        node.Status = TaskStatus.Done;
        await ctx.Engine.UpdateAsync(node);

        Assert.IsNotNull(completed);
        Assert.AreEqual(node.Id, completed!.Id);
    }

    /// <summary>Extra: UpdateAsync does NOT raise TaskCompleted on idempotent Done→Done.</summary>
    [TestMethod]
    public async Task UpdateAsync_DoneToDone_DoesNotRaiseTaskCompleted()
    {
        var ctx = CreateEngine();
        var node = await ctx.Engine.AddAsync(NewNode("synthetic-done"));
        node.Status = TaskStatus.Done;
        await ctx.Engine.UpdateAsync(node);

        int completedCount = 0;
        ctx.Engine.TaskCompleted += (_, _) => completedCount++;
        // No status change; should NOT re-raise.
        await ctx.Engine.UpdateAsync(node);

        Assert.AreEqual(0, completedCount);
    }

    /// <summary>Derivation 7: UpdateAsync refreshes ModifiedAt while preserving CreatedAt.</summary>
    [TestMethod]
    public async Task UpdateAsync_UpdatesModifiedAtTimestamp()
    {
        var ctx = CreateEngine();
        var node = await ctx.Engine.AddAsync(NewNode("synthetic-tstamp"));
        var originalCreated = node.CreatedAt;
        var originalModified = node.ModifiedAt;

        ctx.Clock.UtcNow = ctx.Clock.UtcNow.AddMinutes(5);
        node.Title = "synthetic-tstamp-updated";
        var updated = await ctx.Engine.UpdateAsync(node);

        Assert.AreEqual(originalCreated, updated.CreatedAt);
        Assert.IsTrue(updated.ModifiedAt > originalModified);
    }

    /// <summary>Derivation 1 / P1A-AC4: DeleteAsync cascades to all descendants.</summary>
    [TestMethod]
    public async Task DeleteAsync_CascadeRemovesDescendants()
    {
        var ctx = CreateEngine();
        var parent = await ctx.Engine.AddAsync(NewNode("synthetic-cascade-root"));
        var c1 = await ctx.Engine.AddAsync(NewNode("synthetic-cascade-c1"), parent.Id);
        var c2 = await ctx.Engine.AddAsync(NewNode("synthetic-cascade-c2"), parent.Id);
        var gc = await ctx.Engine.AddAsync(NewNode("synthetic-cascade-gc"), c1.Id);

        await ctx.Engine.DeleteAsync(parent.Id);

        var tree = await ctx.Engine.GetTreeAsync();
        Assert.AreEqual(0, tree.Count);
    }

    /// <summary>P1A-AC3: GetOverdueAsync returns past-deadline non-Done nodes.</summary>
    [TestMethod]
    public async Task GetOverdueAsync_ReturnsPastDeadlineNonDone()
    {
        var ctx = CreateEngine();
        var past = DateTimeOffset.Parse("2025-12-01T00:00:00Z");
        await ctx.Engine.AddAsync(NewNode("synthetic-overdue", deadline: past));
        var future = DateTimeOffset.Parse("2026-12-01T00:00:00Z");
        await ctx.Engine.AddAsync(NewNode("synthetic-future", deadline: future));

        var overdue = await ctx.Engine.GetOverdueAsync(ctx.Clock.UtcNow);

        Assert.AreEqual(1, overdue.Count);
        Assert.AreEqual("synthetic-overdue", overdue[0].Title);
    }

    /// <summary>P1A-AC3: GetOverdueAsync excludes nodes without a deadline (and Done nodes).</summary>
    [TestMethod]
    public async Task GetOverdueAsync_ExcludesNodesWithoutDeadlineAndDone()
    {
        var ctx = CreateEngine();
        await ctx.Engine.AddAsync(NewNode("synthetic-no-deadline"));    // deadline null
        var past = DateTimeOffset.Parse("2025-12-01T00:00:00Z");
        var doneNode = await ctx.Engine.AddAsync(NewNode("synthetic-done-overdue", deadline: past));
        doneNode.Status = TaskStatus.Done;
        await ctx.Engine.UpdateAsync(doneNode);

        var overdue = await ctx.Engine.GetOverdueAsync(ctx.Clock.UtcNow);

        Assert.AreEqual(0, overdue.Count);
    }

    /// <summary>Derivation 5: GetTreeAsync assembles hierarchy from flat storage.</summary>
    [TestMethod]
    public async Task GetTreeAsync_AssemblesHierarchyFromFlatStorage()
    {
        var ctx = CreateEngine();
        var root = await ctx.Engine.AddAsync(NewNode("synthetic-tree-root"));
        var c1 = await ctx.Engine.AddAsync(NewNode("synthetic-tree-c1"), root.Id);
        await ctx.Engine.AddAsync(NewNode("synthetic-tree-gc1"), c1.Id);
        await ctx.Engine.AddAsync(NewNode("synthetic-tree-c2"), root.Id);

        var tree = await ctx.Engine.GetTreeAsync();

        Assert.AreEqual(1, tree.Count);
        Assert.AreEqual(root.Id, tree[0].Id);
        Assert.AreEqual(2, tree[0].Children.Count);
        var firstChild = tree[0].Children.First(n => n.Id == c1.Id);
        Assert.AreEqual(1, firstChild.Children.Count);
    }

    /// <summary>Derivation 4: AddAsync writes a TaskAdded audit entry via IComplianceCore.</summary>
    [TestMethod]
    public async Task AddAsync_WritesAuditEntry()
    {
        var ctx = CreateEngine();
        var added = await ctx.Engine.AddAsync(NewNode("synthetic-audit-add"));

        Assert.AreEqual(1, ctx.AuditEntries.Count);
        Assert.AreEqual("TaskAdded", ctx.AuditEntries[0].Action);
        Assert.AreEqual(added.Id, ctx.AuditEntries[0].TargetId);
        Assert.AreEqual(nameof(TaskEngine), ctx.AuditEntries[0].Module);
    }

    /// <summary>Derivation 4 + 1: DeleteAsync writes one audit entry per cascade-deleted node.</summary>
    [TestMethod]
    public async Task DeleteAsync_WritesAuditEntryPerDeletedNode()
    {
        var ctx = CreateEngine();
        var root = await ctx.Engine.AddAsync(NewNode("synthetic-audit-cascade-root"));
        await ctx.Engine.AddAsync(NewNode("synthetic-audit-cascade-c1"), root.Id);
        await ctx.Engine.AddAsync(NewNode("synthetic-audit-cascade-c2"), root.Id);
        ctx.AuditEntries.Clear();  // discard the 3 TaskAdded entries — interested in deletes only

        await ctx.Engine.DeleteAsync(root.Id);

        var deletes = ctx.AuditEntries.Where(e => e.Action == "TaskDeleted").ToList();
        Assert.AreEqual(3, deletes.Count);
    }
}
