// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Modules.ComplianceCore.Tests/AuditChainWriterTests.cs
//  Purpose: 2 unit tests for AuditChainWriter sequence policy per Derivation 4 / §10.5.
//  Architecture.md References: §10.5, §10.7
//  Roadmap.md References: Phase 1C — Msg 5 of 5 (closes 13-test plan)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: all test data is synthetic, non-PHI-shaped.
//  D10 anti-drift: XML doc on every test class.
//  NOTE: inline InMemorySecureStore + FakeClock follow PHASE1A §8 conventions
//    (also used in ComplianceCoreTests Msg 4 — 2nd consumer; promotion to
//    shared TestDoubles location deferred to Phase 1D Msg 1).
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Core.Security;
using TaskTree.Modules.ComplianceCore;

namespace TaskTree.Modules.ComplianceCore.Tests;

/// <summary>
/// Verifies <see cref="AuditChainWriter"/> sequence policy (Derivation 4) per §10.5.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1C: this test class verifies derivation §4 (sequence
/// numbering policy) from PHASE1C-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class AuditChainWriterTests
{
    // ─── Inline test doubles (same pattern as ComplianceCoreTests Msg 4) ──

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
    }

    private sealed class InMemorySecureStore : ISecureStore
    {
        private readonly Dictionary<string, object?> _data = new();
        public Task<T?> LoadAsync<T>(string key) where T : class
            => Task.FromResult(_data.TryGetValue(key, out var v) ? (T?)v : null);
        public Task SaveAsync<T>(string key, T value) where T : class
        {
            _data[key] = value;
            return Task.CompletedTask;
        }
        public Task<bool> ExistsAsync(string key) => Task.FromResult(_data.ContainsKey(key));
        public Task DeleteAsync(string key) { _data.Remove(key); return Task.CompletedTask; }
    }

    private static AuditChainWriter CreateWriter()
    {
        var store = new InMemorySecureStore();
        var clock = new FakeClock();
        var logger = new Mock<IAppLogger>();
        return new AuditChainWriter(store, clock, logger.Object);
    }

    private static AuditEntry NewEntry(string action, long callerSuppliedSeq) => new()
    {
        Seq = callerSuppliedSeq,   // caller pre-populates Seq; the writer should OVERWRITE it.
        Actor = "synthetic-actor",
        Module = "TestModule",
        Action = action,
        TargetId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Result = "success",
    };

    // ─── Tests ────────────────────────────────────────────────────────────

    /// <summary>
    /// Derivation 4: AppendAsync OVERWRITES caller-supplied Seq with the next
    /// monotonic value (lastSeq+1). Caller passing Seq=999 must end up with
    /// the writer-assigned 1, 2, 3 sequence — proving the security invariant
    /// that monotonicity cannot be controlled from outside the writer.
    /// </summary>
    [TestMethod]
    public async Task AppendAsync_AssignsMonotonicallyIncreasingSeq()
    {
        var writer = CreateWriter();

        // All three callers attempt to inject Seq=999. The writer must ignore them.
        await writer.AppendAsync(NewEntry("synthetic-1", callerSuppliedSeq: 999));
        await writer.AppendAsync(NewEntry("synthetic-2", callerSuppliedSeq: 999));
        await writer.AppendAsync(NewEntry("synthetic-3", callerSuppliedSeq: 999));

        var chain = await writer.GetAllAsync();
        Assert.AreEqual(3, chain.Count);
        Assert.AreEqual(1L, chain[0].Seq);
        Assert.AreEqual(2L, chain[1].Seq);
        Assert.AreEqual(3L, chain[2].Seq);
    }

    /// <summary>
    /// Derivation 4: First entry's PrevHash is the empty-string sentinel
    /// <see cref="HashChain.GenesisPrevHash"/>, per PHASE0-MSG5 §2.
    /// </summary>
    [TestMethod]
    public async Task AppendAsync_FirstEntry_UsesGenesisPrevHash()
    {
        var writer = CreateWriter();

        await writer.AppendAsync(NewEntry("synthetic-genesis", callerSuppliedSeq: 0));

        var chain = await writer.GetAllAsync();
        Assert.AreEqual(1, chain.Count);
        Assert.AreEqual(HashChain.GenesisPrevHash, chain[0].PrevHash);
        Assert.AreEqual(string.Empty, chain[0].PrevHash);  // explicit equality with documented sentinel
    }
}
