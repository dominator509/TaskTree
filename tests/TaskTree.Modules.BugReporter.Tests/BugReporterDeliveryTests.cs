// SPEC-DERIVED-PHASE3E  HALT #23

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.BugReporter;
using TaskTree.TestSupport;

namespace TaskTree.Modules.BugReporter.Tests
{
    [TestClass]
    public class BugReporterDeliveryTests
    {
        private static readonly DateTimeOffset T = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        private static BugReport Report(BugSeverity severity, bool redacted = true) => new(
            Guid.NewGuid(), T, BugReportType.UserSubmitted, severity, "t",
            new BugReportDescription("e", "a"),
            new BugReportEnvironment("o", "v", "b", UpdateChannel.Stable),
            Guid.NewGuid(), new string('A', 64), Array.Empty<BugReportAttachment>(), redacted);

        private static BugReporter Reporter(BugReportQueue queue)
        {
            var compliance = new Mock<IComplianceCore>();
            compliance.Setup(c => c.RedactPhi(It.IsAny<string>())).Returns<string>(s => s);
            var clock = new FakeClock(T);
            var router = new DeliveryRouter(
                new EmailDeliveryAdapter(),
                new GitHubIssueAdapter(),
                new FileDropAdapter(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))),
                new BugReportRateLimiter(),
                clock);
            return new BugReporter(queue, new RedactionPipeline(compliance.Object), new CrashCaptureHook(), clock, new Mock<IAppLogger>().Object, router);
        }

        [TestMethod]
        public async Task FlushQueueAsync_FileDropSuccess_RetainsReportUntilRetention()
        {
            var queue = new BugReportQueue(new InMemorySecureStore());
            await queue.EnqueueAsync(Report(BugSeverity.Trivial));

            Assert.AreEqual(1, await Reporter(queue).FlushQueueAsync());
            Assert.AreEqual(1, await queue.CountAsync());
            Assert.AreEqual(0, (await queue.GetPendingAsync()).Count);
        }

        [TestMethod]
        public async Task FlushQueueAsync_ExternalStubFailure_KeepsReport()
        {
            var queue = new BugReportQueue(new InMemorySecureStore());
            await queue.EnqueueAsync(Report(BugSeverity.Critical));

            Assert.AreEqual(0, await Reporter(queue).FlushQueueAsync());
            Assert.AreEqual(1, await queue.CountAsync());
            Assert.AreEqual(1, (await queue.GetPendingAsync()).Count);
        }

        [TestMethod]
        public async Task FlushQueueAsync_EmptyQueue_ReturnsZero()
        {
            var queue = new BugReportQueue(new InMemorySecureStore());
            Assert.AreEqual(0, await Reporter(queue).FlushQueueAsync());
        }

        [TestMethod]
        public async Task FlushQueueAsync_UnredactedReport_KeepsReport()
        {
            var queue = new BugReportQueue(new InMemorySecureStore());
            await queue.EnqueueAsync(Report(BugSeverity.Trivial, false));

            Assert.AreEqual(0, await Reporter(queue).FlushQueueAsync());
            Assert.AreEqual(1, await queue.CountAsync());
            Assert.AreEqual(1, (await queue.GetPendingAsync()).Count);
        }
    }
}
