// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Modules.ComplianceCore.Tests/ComplianceCoreTests.cs
//  Purpose: 5 unit tests for ComplianceCore wiring per Architecture §4.6 / §10.5 and Roadmap P1C-AC1, AC2, AC5.
//  Architecture.md References: §4.6, §10.5, §10.7
//  Roadmap.md References: Phase 1C — Msg 4 of 5 (tests)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: all test data is synthetic, non-PHI-shaped.
//  D10 anti-drift: XML doc on every test class.
//  NOTE: inline InMemorySecureStore + FakeClock follow PHASE1A §8 conventions.
//    This is the 2nd consumer of FakeClock; promotion to shared location
//    deferred to Phase 1D Msg 1 (3rd consumer is the better trigger point).
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Modules.ComplianceCore;

namespace TaskTree.Modules.ComplianceCore.Tests;

/// <summary>
/// Verifies <see cref="ComplianceCore"/> wiring per §4.6 and the audit chain
/// pipeline per §10.5 (P1C-AC1, AC2, AC5, plus the <c>StartIdleMonitor</c>
/// stub per Roadmap 1C anti-drift D5).
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1C: this test class verifies derivations §1
/// (ComplianceCore class structure), §2 (AuditChainWriter surface),
/// §3 (audit storage), §4 (sequence policy), and §5 (verify side
/// effects) from PHASE1C-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class ComplianceCoreTests
{
    // ─── Inline test doubles (per PHASE1A §8 convention) ───────────────────

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
    }

    // InMemorySecureStore — same as PHASE1A §8 pattern, but EXPOSES the
    // internal dictionary publicly so the tamper test (#3) can mutate the
    // stored AuditChainDto via reflection. This is test-only; production
    // SecureStore (Phase 1B) round-trips through encrypted JSON and does not
    // expose state.
    private sealed class InMemorySecureStore : ISecureStore
    {
        public Dictionary<string, object?> Data { get; } = new();
        public Task<T?> LoadAsync<T>(string key) where T : class
            => Task.FromResult(Data.TryGetValue(key, out var v) ? (T?)v : null);
        public Task SaveAsync<T>(string key, T value) where T : class
        {
            Data[key] = value;
            return Task.CompletedTask;
        }
        public Task<bool> ExistsAsync(string key) => Task.FromResult(Data.ContainsKey(key));
        public Task DeleteAsync(string key) { Data.Remove(key); return Task.CompletedTask; }
    }

    private sealed class TestContext
    {
        public ComplianceCore Core { get; init; } = default!;
        public InMemorySecureStore Store { get; init; } = default!;
        public FakeClock Clock { get; init; } = default!;
        public PhiRedactor Redactor { get; init; } = default!;
        public AuditChainWriter Writer { get; init; } = default!;
    }

    private static TestContext CreateContext()
    {
        var store = new InMemorySecureStore();
        var clock = new FakeClock();
        var logger = new Mock<IAppLogger>();
        var redactor = new PhiRedactor();
        var writer = new AuditChainWriter(store, clock, logger.Object);
        var core = new ComplianceCore(store, clock, logger.Object, redactor, writer);
        return new TestContext { Core = core, Store = store, Clock = clock, Redactor = redactor, Writer = writer };
    }

    private static AuditEntry NewEntry(string action) => new()
    {
        Actor = "synthetic-actor",
        Module = "TestModule",
        Action = action,
        TargetId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Result = "success",
    };

    // ─── Tests ─────────────────────────────────────────────────────────────

    /// <summary>P1C-AC1: AuditAsync appends a valid chain entry that can be read back.</summary>
    [TestMethod]
    public async Task AuditAsync_AppendsValidChainEntry()
    {
        var ctx = CreateContext();

        await ctx.Core.AuditAsync(NewEntry("synthetic-action-1"));

        var chain = await ctx.Core.GetAuditChainAsync();
        Assert.AreEqual(1, chain.Count);
        Assert.AreEqual("synthetic-action-1", chain[0].Action);
        Assert.AreEqual(1L, chain[0].Seq);
        Assert.AreEqual("", chain[0].PrevHash);   // GenesisPrevHash for first entry
        Assert.AreEqual(64, chain[0].Hash.Length); // Hex SHA-256
    }

    /// <summary>P1C-AC1: VerifyChainIntegrityAsync returns true for an untampered multi-entry chain.</summary>
    [TestMethod]
    public async Task VerifyChainIntegrityAsync_ReturnsTrueForUntamperedChain()
    {
        var ctx = CreateContext();
        await ctx.Core.AuditAsync(NewEntry("synthetic-action-1"));
        await ctx.Core.AuditAsync(NewEntry("synthetic-action-2"));
        await ctx.Core.AuditAsync(NewEntry("synthetic-action-3"));

        bool ok = await ctx.Core.VerifyChainIntegrityAsync();

        Assert.IsTrue(ok);
    }

    /// <summary>
    /// P1C-AC2: VerifyChainIntegrityAsync returns false after a stored entry is
    /// mutated post-hash. Uses reflection on the private AuditChainDto to
    /// bypass the writer (which would correctly recompute Hash on a
    /// legitimate update).
    /// </summary>
    [TestMethod]
    public async Task VerifyChainIntegrityAsync_DetectsByteChange()
    {
        var ctx = CreateContext();
        await ctx.Core.AuditAsync(NewEntry("synthetic-action-1"));
        await ctx.Core.AuditAsync(NewEntry("synthetic-action-2"));
        Assert.IsTrue(await ctx.Core.VerifyChainIntegrityAsync());

        // Reach into the stored AuditChainDto and mutate the second entry's
        // Action field. The InMemorySecureStore holds the dto by reference, so
        // mutation propagates without re-saving.
        var stored = ctx.Store.Data["audit/chain"];
        Assert.IsNotNull(stored);
        var entriesProp = stored!.GetType().GetProperty("Entries");
        Assert.IsNotNull(entriesProp);
        var entries = (IList)entriesProp!.GetValue(stored)!;
        var second = (AuditEntry)entries[1]!;
        second.Action = "TamperedAction";   // mutate AFTER hashing

        bool ok = await ctx.Core.VerifyChainIntegrityAsync();
        Assert.IsFalse(ok);
    }

    /// <summary>P1C-AC5: persisted AuditEntry includes Actor/Module/Action/TargetId/Result + writer-assigned Seq/PrevHash/Hash.</summary>
    [TestMethod]
    public async Task AuditEntry_IncludesActorModuleActionResult()
    {
        var ctx = CreateContext();

        await ctx.Core.AuditAsync(NewEntry("synthetic-shape-check"));

        var chain = await ctx.Core.GetAuditChainAsync();
        Assert.AreEqual(1, chain.Count);
        var e = chain[0];

        Assert.AreEqual("synthetic-actor", e.Actor);
        Assert.AreEqual("TestModule", e.Module);
        Assert.AreEqual("synthetic-shape-check", e.Action);
        Assert.AreEqual("success", e.Result);
        Assert.AreNotEqual(Guid.Empty, e.TargetId);
        Assert.AreEqual(1L, e.Seq);
        Assert.AreNotEqual(default(DateTimeOffset), e.Timestamp);
        Assert.AreEqual("", e.PrevHash);
        Assert.AreEqual(64, e.Hash.Length);
    }

    /// <summary>Roadmap 1C Anti-Drift D5: StartIdleMonitor throws NotImplementedException (Deferred to Phase 2F).</summary>
    [TestMethod]
    public void StartIdleMonitor_ThrowsNotImplementedException()
    {
        var ctx = CreateContext();

        var ex = Assert.ThrowsException<NotImplementedException>(
            () => ctx.Core.StartIdleMonitor(TimeSpan.FromMinutes(15)));

        StringAssert.Contains(ex.Message, "Phase 2F");
    }
}
