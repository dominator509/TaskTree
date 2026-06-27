// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Core.Tests/Security/HashChainTests.cs
//  Purpose: Verifies SHA-256 hash chain determinism and tamper-detection per Architecture §10.5 / §10.7 and Roadmap P0-AC4.
//  Architecture.md References: §10.5, §10.7, P0-AC4
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 6 — Test Skeletons + 5 Primitive Tests)
//  D1: header cites Architecture.md sections.
//  D6: all test data is synthetic, non-PHI-shaped.
//  D10: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Models;
using TaskTree.Core.Security;

namespace TaskTree.Core.Tests.Security;

/// <summary>
/// Verifies that <see cref="HashChain.ComputeHash"/> is deterministic and that
/// <see cref="HashChain.VerifyChain"/> detects post-hash mutation.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG6: test naming convention (MethodName_Scenario_ExpectedOutcome)
/// and test category vocabulary (Offline default / Live / Integration) not
/// specified verbatim in Architecture.md; derived from documented usage and
/// approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG6-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class HashChainTests
{
    private static AuditEntry MakeEntry(long seq, string prevHash, string action) => new()
    {
        Seq = seq,
        Timestamp = DateTimeOffset.Parse("2026-01-01T00:00:00Z").AddSeconds(seq),
        Actor = "synthetic-actor",
        Module = "TestModule",
        Action = action,
        TargetId = Guid.Parse("00000000-0000-0000-0000-00000000000" + seq),
        Result = "success",
        PrevHash = prevHash,
    };

    /// <summary>ComputeHash returns the same 64-char lowercase hex for identical inputs (§10.5 reproducibility).</summary>
    [TestMethod]
    public void ComputeHash_SameInput_IsDeterministic()
    {
        var entry = MakeEntry(1, HashChain.GenesisPrevHash, "TestAction");

        var h1 = HashChain.ComputeHash(HashChain.GenesisPrevHash, entry);
        var h2 = HashChain.ComputeHash(HashChain.GenesisPrevHash, entry);

        Assert.AreEqual(h1, h2);
        Assert.AreEqual(64, h1.Length);
    }

    /// <summary>VerifyChain returns false when any entry is mutated after hashing (P0-AC4).</summary>
    [TestMethod]
    public void VerifyChain_TamperedEntry_ReturnsFalse()
    {
        var entry1 = MakeEntry(1, HashChain.GenesisPrevHash, "FirstAction");
        entry1.Hash = HashChain.ComputeHash(entry1.PrevHash, entry1);

        var entry2 = MakeEntry(2, entry1.Hash, "SecondAction");
        entry2.Hash = HashChain.ComputeHash(entry2.PrevHash, entry2);

        var entries = new List<AuditEntry> { entry1, entry2 };
        Assert.IsTrue(HashChain.VerifyChain(entries));

        entry2.Action = "TamperedAction";   // mutate AFTER hashing
        Assert.IsFalse(HashChain.VerifyChain(entries));
    }
}
