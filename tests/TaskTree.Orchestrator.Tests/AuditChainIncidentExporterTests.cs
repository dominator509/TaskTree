using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Models;
using TaskTree.Core.Security;
using TaskTree.Orchestrator;

namespace TaskTree.Orchestrator.Tests;

[TestClass]
public sealed class AuditChainIncidentExporterTests
{
    [TestMethod, TestCategory("Offline")]
    public void GetLastKnownGoodPrefix_StopsBeforeFirstCorruptEntry()
    {
        var first = Entry(1, HashChain.GenesisPrevHash);
        first.Hash = HashChain.ComputeHash(first.PrevHash, first);
        var second = Entry(2, first.Hash);
        second.Hash = HashChain.ComputeHash(second.PrevHash, second);
        second.Result = "tampered";

        var prefix = AuditChainIncidentExporter.GetLastKnownGoodPrefix(new[] { first, second });

        Assert.AreEqual(1, prefix.Count);
        Assert.AreEqual(first.Hash, prefix[0].Hash);
    }

    [TestMethod, TestCategory("Offline")]
    public void Export_WritesAtomicJsonPrefix()
    {
        var root = Path.Combine(Path.GetTempPath(), "TaskTreeAuditExport", Guid.NewGuid().ToString("N"));
        try
        {
            var entry = Entry(1, HashChain.GenesisPrevHash);
            entry.Hash = HashChain.ComputeHash(entry.PrevHash, entry);

            var path = AuditChainIncidentExporter.Export(
                new List<AuditEntry> { entry },
                root,
                new DateTimeOffset(2026, 8, 16, 12, 0, 0, TimeSpan.Zero));

            Assert.IsTrue(File.Exists(path));
            var exported = JsonSerializer.Deserialize<List<AuditEntry>>(File.ReadAllText(path));
            Assert.IsNotNull(exported);
            Assert.AreEqual(1, exported!.Count);
            Assert.IsFalse(File.Exists(path + ".tmp"));
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
        }
    }

    private static AuditEntry Entry(long seq, string previousHash) => new()
    {
        Seq = seq,
        Timestamp = new DateTimeOffset(2026, 8, 16, 12, 0, 0, TimeSpan.Zero),
        Actor = "synthetic-actor",
        Module = "SyntheticModule",
        Action = "SyntheticAction",
        TargetId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Result = "success",
        PrevHash = previousHash,
    };
}
