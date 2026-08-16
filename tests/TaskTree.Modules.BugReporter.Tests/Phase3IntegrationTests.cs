// SPEC-DERIVED-PHASE3F  HALT #9/#10/#11/#12/#19
// Roadmap Phase 3F; Architecture.md Section 9.2 BugReporter offline integration gate.
// Gap #257/#259 retained: real crash injection deferred to Phase 5E.
// Gap #263/#265/#274/#276 retained: live SMTP/GitHub adapters deferred to Phase 5E.

using System;
using System.IO;
using System.Linq;
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
    public class Phase3IntegrationTests
    {
        private static readonly DateTimeOffset T = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        private static BugReport Report(BugSeverity severity, string fp, bool redacted = false) => new(Guid.NewGuid(), default, BugReportType.UserSubmitted, severity, "SECRET title", new BugReportDescription("SECRET expected", "SECRET actual"), new BugReportEnvironment("SECRET os", "SECRET app", "SECRET build", UpdateChannel.Stable), Guid.Empty, fp, Array.Empty<BugReportAttachment>(), redacted);
        private static Mock<IComplianceCore> Redactor(){var m=new Mock<IComplianceCore>();m.Setup(x=>x.RedactPhi(It.IsAny<string>())).Returns<string>(s=>s.Replace("SECRET","[REDACTED]"));return m;}
        private static string Root()=>Path.Combine(Path.GetTempPath(),"TaskTreeP3F",Guid.NewGuid().ToString("N"));

        [TestMethod]
        public async Task SubmitAsync_RedactsAndQueuesReport()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            var reporter = new BugReporter(q, new RedactionPipeline(Redactor().Object), new CrashCaptureHook(), new FakeClock(T), new NullLogger());
            await reporter.SubmitAsync(Report(BugSeverity.Normal, string.Empty));
            var queued = (await q.GetAllAsync())[0];
            Assert.IsTrue(queued.Redacted);
            StringAssert.Contains(queued.Title, "[REDACTED]");
        }

        [TestMethod]
        public async Task SubmitAsync_DuplicateFingerprint_QueuesOnlyOnce()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            var reporter = new BugReporter(q, new RedactionPipeline(Redactor().Object), new CrashCaptureHook(), new FakeClock(T), new NullLogger());
            var fp = new string('A',64);
            await reporter.SubmitAsync(Report(BugSeverity.Normal, fp));
            await reporter.SubmitAsync(Report(BugSeverity.Normal, fp));
            Assert.AreEqual(1, await q.CountAsync());
        }

        [TestMethod]
        public async Task Queue_ConcurrentDistinctSubmissions_PreserveAllReports()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            var submissions = Enumerable.Range(0, 20)
                .Select(i => q.EnqueueAsync(Report(BugSeverity.Normal, i.ToString("X64"))))
                .ToArray();
            await Task.WhenAll(submissions);
            Assert.AreEqual(20, await q.CountAsync());
        }

        [TestMethod]
        public async Task FlushQueueAsync_TrivialReport_FileDropsAndRemovesFromQueue()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            await q.EnqueueAsync(Report(BugSeverity.Trivial, new string('B',64), redacted:true));
            var clock = new FakeClock(T);
            var router = new DeliveryRouter(new EmailDeliveryAdapter(), new GitHubIssueAdapter(), new FileDropAdapter(Root()), new BugReportRateLimiter(), clock);
            var reporter = new BugReporter(q, new RedactionPipeline(Redactor().Object), new CrashCaptureHook(), clock, new NullLogger(), router);
            Assert.AreEqual(1, await reporter.FlushQueueAsync());
            Assert.AreEqual(0, await q.CountAsync());
        }

        [TestMethod]
        public async Task FlushQueueAsync_CriticalReport_ExternalStubsKeepQueued()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            await q.EnqueueAsync(Report(BugSeverity.Critical, new string('C',64), redacted:true));
            var clock = new FakeClock(T);
            var router = new DeliveryRouter(new EmailDeliveryAdapter(), new GitHubIssueAdapter(), new FileDropAdapter(Root()), new BugReportRateLimiter(), clock);
            var reporter = new BugReporter(q, new RedactionPipeline(Redactor().Object), new CrashCaptureHook(), clock, new NullLogger(), router);
            Assert.AreEqual(0, await reporter.FlushQueueAsync());
            Assert.AreEqual(1, await q.CountAsync());
        }

        [TestMethod]
        public async Task FlushQueueAsync_ConcurrentCallsDeliverOnce()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            await q.EnqueueAsync(Report(BugSeverity.Trivial, new string('G',64), redacted:true));
            var clock = new FakeClock(T);
            var router = new DeliveryRouter(new EmailDeliveryAdapter(), new GitHubIssueAdapter(), new FileDropAdapter(Root()), new BugReportRateLimiter(), clock);
            var reporter = new BugReporter(q, new RedactionPipeline(Redactor().Object), new CrashCaptureHook(), clock, new NullLogger(), router);
            var results = await Task.WhenAll(reporter.FlushQueueAsync(), reporter.FlushQueueAsync());
            Assert.AreEqual(1, results.Sum());
            Assert.AreEqual(0, await q.CountAsync());
        }

        [TestMethod]
        public async Task FileDrop_DoesNotWriteUnredactedReport()
        {
            var root = Root();
            var result = await new FileDropAdapter(root).DeliverAsync(Report(BugSeverity.Trivial, new string('D',64), redacted:false));
            Assert.IsFalse(result.Success);
            Assert.IsFalse(Directory.Exists(root));
        }

        [TestMethod]
        public async Task CrashHook_ManualRaise_QueuesRedactedCrashReport()
        {
            var q = new BugReportQueue(new InMemorySecureStore());
            var hook = new CrashCaptureHook();
            var reporter = new BugReporter(q, new RedactionPipeline(Redactor().Object), hook, new FakeClock(T), new NullLogger());
            reporter.HookGlobalCrashHandler();
            hook.RaiseForTests(new InvalidOperationException("SECRET crash"));
            await Task.Delay(50);
            Assert.AreEqual(1, await q.CountAsync());
            Assert.IsTrue((await q.GetAllAsync())[0].Redacted);
        }

        [TestMethod]
        public void RateLimiter_BlocksSixthOutboundWithinMinute()
        {
            var limiter = new BugReportRateLimiter();
            for (var i=0;i<5;i++) limiter.RecordSend(T.AddSeconds(i));
            Assert.IsFalse(limiter.CanSend(T.AddSeconds(30)));
        }

        [TestMethod]
        public async Task DeliveryRouter_RoutesSeverityMatrix_Offline()
        {
            var router = new DeliveryRouter(new EmailDeliveryAdapter(), new GitHubIssueAdapter(), new FileDropAdapter(Root()), new BugReportRateLimiter(), new FakeClock(T));
            Assert.IsFalse((await router.DeliverAsync(Report(BugSeverity.Critical, new string('E',64), redacted:true))).Success);
            Assert.IsFalse((await router.DeliverAsync(Report(BugSeverity.Normal, new string('F',64), redacted:true))).Success);
            Assert.IsTrue((await router.DeliverAsync(Report(BugSeverity.Trivial, new string('0',64), redacted:true))).Success);
        }

        private sealed class NullLogger : IAppLogger
        {
            public void LogDebug(string message, params object?[] args) { }
            public void LogInformation(string message, params object?[] args) { }
            public void LogWarning(string message, params object?[] args) { }
            public void LogError(Exception? exception, string message, params object?[] args) { }
        }
    }
}
