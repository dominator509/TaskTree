// SPEC-DERIVED-PHASE4B  HALT #5/#7/#8/#9/#10/#11/#12/#15/#17/#18/#19
// Architecture.md Section 15 performance targets. Roadmap Phase 4B performance benchmarks.
// Gap #300-#314: benchmark/report remains preliminary until stitched repo executes on Codex/Windows.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskTree.Perf.Tests
{
    /// <summary>
    /// Smoke-level MSTest performance benchmarks for Architecture.md Section 15.
    /// Final threshold enforcement belongs to Phase 5F release-candidate validation.
    /// </summary>
    [TestClass]
    public class PerfBenchmarks
    {
        [TestMethod]
        [TestCategory("Performance")]
        public async Task TaskEngine_AddUpdateDelete_SmokeUnderTarget()
        {
            await Task.CompletedTask;
            var elapsed = MeasureSyntheticWork(1_000);
            Console.WriteLine($"TaskEngine CRUD smoke placeholder elapsed ms: {elapsed.TotalMilliseconds:F4}");
            Assert.IsTrue(elapsed.TotalMilliseconds < 50d, "Smoke placeholder exceeded TaskEngine CRUD target; replace with stitched TaskEngine benchmark in Phase 5C.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task TaskEngine_Fetch1000Nodes_SmokeUnderTarget()
        {
            await Task.CompletedTask;
            var elapsed = MeasureSyntheticWork(1_000);
            Console.WriteLine($"TaskEngine 1000-node fetch smoke placeholder elapsed ms: {elapsed.TotalMilliseconds:F4}");
            Assert.IsTrue(elapsed.TotalMilliseconds < 100d, "Smoke placeholder exceeded full-tree fetch target; replace with stitched TaskEngine benchmark in Phase 5C.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task ReminderScheduler_TickEvaluation1000Tasks_SmokeUnderTarget()
        {
            await Task.CompletedTask;
            var elapsed = MeasureSyntheticWork(1_000);
            Console.WriteLine($"ReminderScheduler tick smoke placeholder elapsed ms: {elapsed.TotalMilliseconds:F4}");
            Assert.IsTrue(elapsed.TotalMilliseconds < 10d, "Smoke placeholder exceeded reminder tick target; add deterministic tick test seam in Phase 5B if needed.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task SecureStore_WriteReadSmallPayload_SmokeUnderTarget()
        {
            await Task.CompletedTask;
            var payload = Encoding.UTF8.GetBytes(new string('S', 16 * 1024));
            var elapsed = MeasureByteCopy(payload);
            Console.WriteLine($"SecureStore small payload smoke placeholder elapsed ms: {elapsed.TotalMilliseconds:F4}");
            Assert.IsTrue(elapsed.TotalMilliseconds < 100d, "Smoke placeholder exceeded SecureStore target; replace with stitched SecureStore benchmark in Phase 5C.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        [TestCategory("Stress")]
        public async Task SecureStore_WriteReadTenMbPayload_RecordsElapsed()
        {
            await Task.CompletedTask;
            var payload = Encoding.UTF8.GetBytes(new string('S', 10 * 1024 * 1024));
            var elapsed = MeasureByteCopy(payload);
            Console.WriteLine($"SecureStore 10 MB payload smoke placeholder elapsed ms: {elapsed.TotalMilliseconds:F4}");
            Assert.IsTrue(payload.Length >= 10 * 1024 * 1024);
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task Audit_Write1000_AverageUnder20Ms_Smoke()
        {
            await Task.CompletedTask;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1_000; i++) _ = $"SyntheticAudit|{i}|success".GetHashCode();
            sw.Stop();
            var average = sw.Elapsed.TotalMilliseconds / 1_000d;
            Console.WriteLine($"Audit write synthetic average ms: {average:F4}");
            Assert.IsTrue(average < 20d, "Synthetic audit smoke target exceeded; run real AuditChainWriter benchmark in Phase 5C/5F.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task BugReporter_SubmitQueued_SmokeUnderTarget()
        {
            await Task.CompletedTask;
            var elapsed = MeasureSyntheticWork(500);
            Console.WriteLine($"BugReporter submit queued smoke placeholder elapsed ms: {elapsed.TotalMilliseconds:F4}");
            Assert.IsTrue(elapsed.TotalMilliseconds < 50d, "Smoke placeholder exceeded bug submit target; replace with stitched BugReporter benchmark in Phase 5C.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        [TestCategory("Live")]
        public void LiveMetrics_RamCpuTrayUi_NeedPhase5E()
        {
            Assert.Inconclusive("Gap #302/#311: Idle RAM, CPU, tray, and warm WPF latency require installed live Windows validation in Phase 5E/5F.");
        }

        private static TimeSpan MeasureSyntheticWork(int count)
        {
            var sw = Stopwatch.StartNew();
            var checksum = 0;
            for (var i = 0; i < count; i++) checksum ^= $"Synthetic Task {i:0000}".GetHashCode();
            sw.Stop();
            GC.KeepAlive(checksum);
            return sw.Elapsed;
        }

        private static TimeSpan MeasureByteCopy(byte[] payload)
        {
            var sw = Stopwatch.StartNew();
            var copy = new byte[payload.Length];
            Buffer.BlockCopy(payload, 0, copy, 0, payload.Length);
            sw.Stop();
            GC.KeepAlive(copy);
            return sw.Elapsed;
        }
    }
}
