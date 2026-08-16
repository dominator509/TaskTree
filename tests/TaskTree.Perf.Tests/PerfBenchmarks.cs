// SPEC-DERIVED-PHASE4B  HALT #5/#7/#8/#9/#10/#11/#12/#15/#17/#18/#19
// Architecture.md Section 15 performance targets. Roadmap Phase 4B performance benchmarks.
// Phase 5C closure: module-backed measurements run here; installed UI and host metrics remain Live.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Core.Security;
using TaskTree.Modules.BugReporter;
using TaskTree.Modules.ComplianceCore;
using TaskTree.Modules.ReminderScheduler;
using TaskTree.Modules.SecureStore;
using TaskTree.Modules.TaskEngine;
using TaskTree.TestSupport;

namespace TaskTree.Perf.Tests
{
    /// <summary>
    /// MSTest performance benchmarks for Architecture.md Section 15.
    /// Module-backed cases exercise the real implementations with deterministic test doubles.
    /// Installed UI, idle resource, and network metrics remain Phase 5E/5F live validation.
    /// </summary>
    [TestClass]
    public class PerfBenchmarks
    {
        [TestMethod]
        [TestCategory("Performance")]
        public async Task TaskEngine_AddUpdateDelete_SmokeUnderTarget()
        {
            var (engine, _, _) = CreateTaskEngine();
            const int iterations = 100;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                var node = await engine.AddAsync(new TaskNode { Title = $"Synthetic task {i:000}" });
                node.Title = $"Synthetic task updated {i:000}";
                await engine.UpdateAsync(node);
                await engine.DeleteAsync(node.Id);
            }
            sw.Stop();
            var average = sw.Elapsed.TotalMilliseconds / iterations;
            Console.WriteLine($"TaskEngine CRUD average ms: {average:F4}");
            Assert.IsTrue(average < 50d, "TaskEngine CRUD exceeded the 50 ms target.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task TaskEngine_Fetch1000Nodes_SmokeUnderTarget()
        {
            var (engine, _, _) = CreateTaskEngine();
            for (var i = 0; i < 1_000; i++)
                await engine.AddAsync(new TaskNode { Title = $"Synthetic task {i:0000}" });

            await engine.GetTreeAsync();
            var sw = Stopwatch.StartNew();
            var tree = await engine.GetTreeAsync();
            sw.Stop();
            Console.WriteLine($"TaskEngine 1000-node fetch ms: {sw.Elapsed.TotalMilliseconds:F4}");
            Assert.AreEqual(1_000, tree.Count);
            Assert.IsTrue(sw.Elapsed.TotalMilliseconds < 100d, "TaskEngine 1000-node fetch exceeded the 100 ms target.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task ReminderScheduler_TickEvaluation1000Tasks_SmokeUnderTarget()
        {
            var clock = new FakeClock(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
            var compliance = new NoOpCompliance();
            var nodes = new List<TaskNode>(1_000);
            for (var i = 0; i < 1_000; i++)
            {
                nodes.Add(new TaskNode
                {
                    Id = Guid.NewGuid(),
                    Title = $"Synthetic future task {i:0000}",
                    Deadline = clock.UtcNow.AddHours(2),
                    Priority = Priority.Normal,
                });
            }

            var taskEngine = new Mock<ITaskEngine>();
            taskEngine.Setup(e => e.GetTreeAsync())
                .Returns(Task.FromResult<IReadOnlyList<TaskNode>>(nodes));
            using var scheduler = new ReminderScheduler(clock, taskEngine.Object, compliance, CreateLogger());
            var fired = 0;
            scheduler.ReminderDue += (_, _) => fired++;
            // Warm the first-call JIT path so the threshold measures steady-state
            // task evaluation, matching the explicit warmup used by storage cases.
            await scheduler.TickOnceAsync(CancellationToken.None);
            var sw = Stopwatch.StartNew();
            await scheduler.TickOnceAsync(CancellationToken.None);
            sw.Stop();
            Console.WriteLine($"ReminderScheduler 1000-task tick ms: {sw.Elapsed.TotalMilliseconds:F4}");
            Assert.AreEqual(0, fired);
            Assert.IsTrue(sw.Elapsed.TotalMilliseconds < 10d, "ReminderScheduler 1000-task tick exceeded the 10 ms target.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task SecureStore_WriteReadSmallPayload_SmokeUnderTarget()
        {
            var root = CreateTempRoot();
            try
            {
                var store = CreateSecureStore(root);
                var payload = new BytePayload { Data = Encoding.UTF8.GetBytes(new string('S', 16 * 1024)) };
                await store.SaveAsync("perf/warmup", new BytePayload { Data = new byte[1024] });
                _ = await store.LoadAsync<BytePayload>("perf/warmup");
                var sw = Stopwatch.StartNew();
                await store.SaveAsync("perf/small", payload);
                var loaded = await store.LoadAsync<BytePayload>("perf/small");
                sw.Stop();
                Console.WriteLine($"SecureStore 16 KB save+load ms: {sw.Elapsed.TotalMilliseconds:F4}");
                Assert.IsNotNull(loaded);
                Assert.AreEqual(payload.Data.Length, loaded!.Data.Length);
                Assert.IsTrue(sw.Elapsed.TotalMilliseconds < 100d, "SecureStore 16 KB save+load exceeded the 100 ms target.");
            }
            finally { DeleteTempRoot(root); }
        }

        [TestMethod]
        [TestCategory("Performance")]
        [TestCategory("Stress")]
        public async Task SecureStore_WriteReadTenMbPayload_RecordsElapsed()
        {
            var root = CreateTempRoot();
            try
            {
                var store = CreateSecureStore(root);
                var payload = new BytePayload { Data = Encoding.UTF8.GetBytes(new string('S', 10 * 1024 * 1024)) };
                await store.SaveAsync("perf/warmup", new BytePayload { Data = new byte[1024] });
                _ = await store.LoadAsync<BytePayload>("perf/warmup");
                var saveSw = Stopwatch.StartNew();
                await store.SaveAsync("perf/ten-megabytes", payload);
                saveSw.Stop();
                var loadSw = Stopwatch.StartNew();
                var loaded = await store.LoadAsync<BytePayload>("perf/ten-megabytes");
                loadSw.Stop();
                Console.WriteLine($"SecureStore 10 MB save ms: {saveSw.Elapsed.TotalMilliseconds:F4}");
                Console.WriteLine($"SecureStore 10 MB load ms: {loadSw.Elapsed.TotalMilliseconds:F4}");
                Assert.IsNotNull(loaded);
                Assert.AreEqual(payload.Data.Length, loaded!.Data.Length);
                Assert.IsTrue(saveSw.Elapsed.TotalMilliseconds < 100d, "SecureStore 10 MB save exceeded the 100 ms target.");
                Assert.IsTrue(loadSw.Elapsed.TotalMilliseconds < 100d, "SecureStore 10 MB load exceeded the 100 ms target.");
            }
            finally { DeleteTempRoot(root); }
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task Audit_Write1000_AverageUnder20Ms_Smoke()
        {
            var writer = new AuditChainWriter(new InMemorySecureStore(preserveObjectReferences: true), new FakeClock(), CreateLogger());
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 1_000; i++)
            {
                await writer.AppendAsync(new AuditEntry
                {
                    Actor = "syntheticSid",
                    Module = "PerfBenchmarks",
                    Action = "SyntheticAudit",
                    TargetId = Guid.Empty,
                    Result = "success",
                    Timestamp = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(i),
                });
            }
            sw.Stop();
            var average = sw.Elapsed.TotalMilliseconds / 1_000d;
            Console.WriteLine($"AuditChainWriter append average ms: {average:F4}");
            Assert.IsTrue(await writer.VerifyAsync());
            Assert.IsTrue(average < 20d, "AuditChainWriter append exceeded the 20 ms target.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public async Task BugReporter_SubmitQueued_SmokeUnderTarget()
        {
            var compliance = new Mock<IComplianceCore>(MockBehavior.Loose);
            compliance.Setup(c => c.RedactPhi(It.IsAny<string>())).Returns<string>(value => value);
            var queue = new BugReportQueue(new InMemorySecureStore());
            var reporter = new BugReporter(
                queue,
                new RedactionPipeline(compliance.Object),
                new CrashCaptureHook(),
                new FakeClock(),
                CreateLogger());

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 20; i++)
            {
                await reporter.SubmitAsync(new BugReport(
                    Guid.Empty,
                    default,
                    BugReportType.UserSubmitted,
                    BugSeverity.Normal,
                    $"Synthetic report {i:00}",
                    new BugReportDescription("Synthetic expected", $"Synthetic actual {i:00}"),
                    new BugReportEnvironment("Synthetic OS", "Synthetic app", "Synthetic build", UpdateChannel.Stable),
                    Guid.Empty,
                    string.Empty,
                    Array.Empty<BugReportAttachment>(),
                    false));
            }
            sw.Stop();
            var average = sw.Elapsed.TotalMilliseconds / 20d;
            Console.WriteLine($"BugReporter submit+queue average ms: {average:F4}");
            Assert.AreEqual(20, await queue.CountAsync());
            Assert.IsTrue(average < 50d, "BugReporter submit+queue exceeded the 50 ms target.");
        }

        [TestMethod]
        [TestCategory("Performance")]
        [TestCategory("Live")]
        public void LiveMetrics_RamCpuTrayUi_NeedPhase5E()
        {
            Assert.Inconclusive("Gap #302/#311: Idle RAM, CPU, tray, and warm WPF latency require installed live Windows validation in Phase 5E/5F.");
        }

        private static (TaskEngine Engine, FakeClock Clock, IComplianceCore Compliance) CreateTaskEngine()
        {
            var clock = new FakeClock(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
            var compliance = new NoOpCompliance();
            return (
                new TaskEngine(new InMemorySecureStore(preserveObjectReferences: true), clock, CreateLogger(), compliance),
                clock,
                compliance);
        }

        private static IAppLogger CreateLogger() => new NoOpLogger();

        private static SecureStore CreateSecureStore(string root)
        {
            var logger = CreateLogger();
            var keyRoot = Path.Combine(root, "keys");
            var storeRoot = Path.Combine(root, "store");
            return new SecureStore(storeRoot, new MasterKeyManager(keyRoot, logger), new AesGcmCryptoProvider(), logger);
        }

        private static string CreateTempRoot()
        {
            var root = Path.Combine(Path.GetTempPath(), "TaskTreePerf", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static void DeleteTempRoot(string root)
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }

        private sealed class BytePayload
        {
            public byte[] Data { get; set; } = Array.Empty<byte>();
        }

        private sealed class NoOpLogger : IAppLogger
        {
            public void LogDebug(string message, params object?[] args) { }
            public void LogInformation(string message, params object?[] args) { }
            public void LogWarning(string message, params object?[] args) { }
            public void LogError(Exception? exception, string message, params object?[] args) { }
        }

        private sealed class NoOpCompliance : IComplianceCore
        {
            public event EventHandler? AutoLogoffTriggered
            {
                add { }
                remove { }
            }

            public Task AuditAsync(AuditEntry entry) => Task.CompletedTask;

            public Task<IReadOnlyList<AuditEntry>> GetAuditChainAsync()
                => Task.FromResult<IReadOnlyList<AuditEntry>>(Array.Empty<AuditEntry>());

            public Task<bool> VerifyChainIntegrityAsync() => Task.FromResult(true);

            public void StartIdleMonitor(TimeSpan timeout) { }

            public string RedactPhi(string text) => text ?? string.Empty;
        }
    }
}
