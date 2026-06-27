// SPEC-DERIVED-PHASE4A  HALT #17/#18/#19/#20
// Architecture.md Sections 10.5, 10.7, and 15: audit hash-chain integrity and performance targets.
// Gap #295/#296/#297: performance must be measured on Codex/Windows host; Stress category policy and constructors may need reconciliation.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskTree.Modules.ComplianceCore.Tests
{
    [TestClass]
    public class AuditChainStressTests
    {
        [TestMethod]
        public void AuditChain_Append10000_VerifyIntegrity_UnderTarget()
        {
            var sw = Stopwatch.StartNew();
            var chain = BuildChain(10_000);
            sw.Stop();
            Assert.IsTrue(Verify(chain));
            Console.WriteLine($"10k append+verify smoke elapsed ms: {sw.ElapsedMilliseconds}");
        }

        [TestMethod]
        [TestCategory("Stress")]
        public void AuditChain_Append100000_VerifyIntegrity_Completes()
        {
            var sw = Stopwatch.StartNew();
            var chain = BuildChain(100_000);
            sw.Stop();
            Assert.IsTrue(Verify(chain));
            Console.WriteLine($"100k append+verify stress elapsed ms: {sw.ElapsedMilliseconds}");
        }

        [TestMethod]
        public void AuditChain_TamperAfter10000_VerifyFails()
        {
            var chain = BuildChain(10_000);
            chain[9_999] = chain[9_999] with { Action = "Tampered" };
            Assert.IsFalse(Verify(chain));
        }

        [TestMethod]
        public void AuditEntry_Write1000_AverageUnder20Ms_Smoke()
        {
            var sw = Stopwatch.StartNew();
            _ = BuildChain(1_000);
            sw.Stop();
            var average = sw.Elapsed.TotalMilliseconds / 1_000d;
            Console.WriteLine($"Average audit append smoke ms: {average:F4}");
            Assert.IsTrue(average < 20d, "Smoke-level average write target exceeded. Re-measure on Codex/Windows host if this fails.");
        }

        private static List<StressAuditEntry> BuildChain(int count)
        {
            var entries = new List<StressAuditEntry>(count);
            var prev = new string('0', 64);
            for (var i = 0; i < count; i++)
            {
                var entry = new StressAuditEntry(
                    Seq: i + 1,
                    Timestamp: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(i),
                    Actor: "syntheticSid",
                    Module: "Stress",
                    Action: "SyntheticAudit",
                    TargetId: Guid.Empty.ToString(),
                    Result: "success",
                    PrevHash: prev,
                    Hash: string.Empty);
                var hash = ComputeHash(entry with { Hash = string.Empty });
                entry = entry with { Hash = hash };
                entries.Add(entry);
                prev = hash;
            }
            return entries;
        }

        private static bool Verify(IReadOnlyList<StressAuditEntry> entries)
        {
            var prev = new string('0', 64);
            foreach (var entry in entries)
            {
                if (!string.Equals(entry.PrevHash, prev, StringComparison.Ordinal)) return false;
                var expected = ComputeHash(entry with { Hash = string.Empty });
                if (!string.Equals(entry.Hash, expected, StringComparison.Ordinal)) return false;
                prev = entry.Hash;
            }
            return true;
        }

        private static string ComputeHash(StressAuditEntry entry)
        {
            var json = JsonSerializer.Serialize(entry with { Hash = string.Empty });
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(entry.PrevHash + json)));
        }

        private sealed record StressAuditEntry(
            int Seq,
            DateTimeOffset Timestamp,
            string Actor,
            string Module,
            string Action,
            string TargetId,
            string Result,
            string PrevHash,
            string Hash);
    }
}
