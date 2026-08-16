// SPEC-DERIVED-PHASE3F  HALT #19
// Architecture.md Section 9.2.5: successful submissions retain for 7 days;
// failed submissions retain for 30 days before local purge.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.BugReporter;
using TaskTree.TestSupport;

namespace TaskTree.Modules.BugReporter.Tests
{
    [TestClass]
    public class BugReportRetentionTests
    {
        private static readonly DateTimeOffset T = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        private static BugReport Report(DateTimeOffset timestamp) => new(
            Guid.NewGuid(), timestamp, BugReportType.UserSubmitted, BugSeverity.Normal, "title",
            new BugReportDescription("expected", "actual"),
            new BugReportEnvironment("os", "app", "build", UpdateChannel.Stable),
            Guid.NewGuid(), Guid.NewGuid().ToString("N"), Array.Empty<BugReportAttachment>(), true);

        [TestMethod]
        public async Task DeliveredReport_IsPendingUntilSevenDayPurge()
        {
            var queue = new BugReportQueue(new InMemorySecureStore());
            var report = Report(T);
            await queue.EnqueueAsync(report);
            await queue.RecordDeliveryResultAsync(report.Id, T.AddHours(1), delivered: true);

            Assert.AreEqual(0, (await queue.GetPendingAsync()).Count);
            Assert.AreEqual(0, await queue.PurgeExpiredAsync(T.AddDays(7).AddMinutes(-1)));
            Assert.AreEqual(1, await queue.CountAsync());
            Assert.AreEqual(1, await queue.PurgeExpiredAsync(T.AddDays(7).AddHours(2)));
            Assert.AreEqual(0, await queue.CountAsync());
        }

        [TestMethod]
        public async Task FailedReport_IsRetainedUntilThirtyDayPurge()
        {
            var queue = new BugReportQueue(new InMemorySecureStore());
            var report = Report(T);
            await queue.EnqueueAsync(report);
            await queue.RecordDeliveryResultAsync(report.Id, T.AddHours(1), delivered: false);

            Assert.AreEqual(1, (await queue.GetPendingAsync()).Count);
            Assert.AreEqual(0, await queue.PurgeExpiredAsync(T.AddDays(30).AddMinutes(-1)));
            Assert.AreEqual(1, await queue.CountAsync());
            Assert.AreEqual(1, await queue.PurgeExpiredAsync(T.AddDays(30).AddMinutes(1)));
            Assert.AreEqual(0, await queue.CountAsync());
        }

        [TestMethod]
        public async Task RetentionMetadata_PersistsAcrossQueueInstances()
        {
            var store = new InMemorySecureStore();
            var report = Report(T);
            var first = new BugReportQueue(store);
            await first.EnqueueAsync(report);
            await first.RecordDeliveryResultAsync(report.Id, T.AddHours(1), delivered: true);

            var second = new BugReportQueue(store);
            Assert.AreEqual(0, (await second.GetPendingAsync()).Count);
            Assert.AreEqual(0, await second.PurgeExpiredAsync(T.AddDays(7).AddMinutes(-1)));
            Assert.AreEqual(1, await second.PurgeExpiredAsync(T.AddDays(7).AddHours(2)));
        }
    }
}
