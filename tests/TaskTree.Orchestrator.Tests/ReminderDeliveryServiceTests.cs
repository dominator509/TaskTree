// SPEC-DERIVED-PHASE2G  HALT #20 skeleton/plan
// Gap #193: Phase 5C ReminderDeliveryService backfill must include snooze skip behavior.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Orchestrator;
using TaskTree.TestSupport;

namespace TaskTree.Orchestrator.Tests
{
    [TestClass]
    public class ReminderDeliveryServiceTests
    {
        [TestMethod]
        public async Task OnReminderDue_WhenSnoozed_SkipsTierCascade_AuditsDeliverySkippedSnoozed()
        {
            var taskId = Guid.NewGuid();
            var clock = new FakeClock(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var scheduler = new Mock<IReminderScheduler>(MockBehavior.Strict);
            var compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
            var snooze = new Mock<ISnoozeService>(MockBehavior.Strict);
            var sessionLock = new Mock<ISessionLockService>(MockBehavior.Loose);
            var trayHost = new Mock<ITrayHost>(MockBehavior.Loose);
            var auditWritten = new TaskCompletionSource<AuditEntry>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<ReminderEvent>? reminderHandler = null;

            scheduler.SetupAdd(x => x.ReminderDue += It.IsAny<EventHandler<ReminderEvent>>())
                .Callback<EventHandler<ReminderEvent>>(h => reminderHandler += h);
            scheduler.SetupRemove(x => x.ReminderDue -= It.IsAny<EventHandler<ReminderEvent>>())
                .Callback<EventHandler<ReminderEvent>>(h => reminderHandler -= h);
            scheduler.SetupProperty(x => x.Cadence, TimeSpan.FromSeconds(30));

            snooze.Setup(x => x.GetAsync(taskId)).ReturnsAsync(
                new SnoozeState(taskId, clock.UtcNow.AddMinutes(10), SnoozeReason.UserRequested, clock.UtcNow, clock.UtcNow));
            compliance.Setup(x => x.AuditAsync(It.IsAny<AuditEntry>()))
                .Callback<AuditEntry>(entry => auditWritten.TrySetResult(entry))
                .Returns(Task.CompletedTask);
            sessionLock.SetupGet(x => x.IsLocked).Returns(false);

            var service = new ReminderDeliveryService(
                scheduler.Object,
                new ToastTier1Adapter(logger.Object),
                new ToastTier2Adapter(logger.Object, sessionLock.Object),
                new ToastTier3Adapter(trayHost.Object, logger.Object),
                clock,
                logger.Object,
                compliance.Object,
                snooze.Object);

            await service.StartAsync(CancellationToken.None);
            Assert.IsNotNull(reminderHandler);

            reminderHandler!.Invoke(this, new ReminderEvent
            {
                TaskId = taskId,
                FiredAtUtc = clock.UtcNow,
                Deadline = clock.UtcNow.AddMinutes(5),
                Priority = Priority.Normal,
                Reason = ReminderReason.Initial,
            });

            var completed = await Task.WhenAny(auditWritten.Task, Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.AreSame(auditWritten.Task, completed);
            var audit = await auditWritten.Task;

            Assert.AreEqual("ReminderDelivery", audit.Module);
            Assert.AreEqual("DeliverySkippedSnoozed", audit.Action);
            Assert.AreEqual(taskId, audit.TargetId);
            Assert.AreEqual("success", audit.Result);
            logger.Verify(x => x.LogError(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<object?[]>()), Times.Never);
            await service.StopAsync();
            Assert.IsNull(reminderHandler);
        }

        [TestMethod]
        public async Task StopAsync_WaitsForInFlightDelivery_AndIgnoresCallbacksAfterStop()
        {
            var taskId = Guid.NewGuid();
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var scheduler = new Mock<IReminderScheduler>(MockBehavior.Strict);
            var compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
            var snooze = new Mock<ISnoozeService>(MockBehavior.Strict);
            var sessionLock = new Mock<ISessionLockService>(MockBehavior.Loose);
            EventHandler<ReminderEvent>? reminderHandler = null;
            var lookupStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var lookupRelease = new TaskCompletionSource<SnoozeState?>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.SetupAdd(x => x.ReminderDue += It.IsAny<EventHandler<ReminderEvent>>())
                .Callback<EventHandler<ReminderEvent>>(h => reminderHandler += h);
            scheduler.SetupRemove(x => x.ReminderDue -= It.IsAny<EventHandler<ReminderEvent>>())
                .Callback<EventHandler<ReminderEvent>>(h => reminderHandler -= h);
            snooze.Setup(x => x.GetAsync(taskId))
                .Callback(() => lookupStarted.TrySetResult(true))
                .Returns(() => lookupRelease.Task);
            compliance.Setup(x => x.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);
            sessionLock.SetupGet(x => x.IsLocked).Returns(false);

            var service = new ReminderDeliveryService(
                scheduler.Object,
                new ToastTier1Adapter(logger.Object),
                new ToastTier2Adapter(logger.Object, sessionLock.Object),
                new ToastTier3Adapter(new Mock<ITrayHost>(MockBehavior.Loose).Object, logger.Object),
                new FakeClock(),
                logger.Object,
                compliance.Object,
                snooze.Object);

            await service.StartAsync(CancellationToken.None);
            var reminder = new ReminderEvent { TaskId = taskId, FiredAtUtc = DateTimeOffset.UtcNow };
            var registeredHandler = reminderHandler!;
            registeredHandler.Invoke(this, reminder);
            Assert.AreSame(lookupStarted.Task, await Task.WhenAny(lookupStarted.Task, Task.Delay(TimeSpan.FromSeconds(2))));

            var stopTask = service.StopAsync();
            Assert.IsFalse(stopTask.IsCompleted);
            lookupRelease.SetResult(null);
            await stopTask;

            registeredHandler.Invoke(this, reminder);
            await Task.Delay(50);
            snooze.Verify(x => x.GetAsync(taskId), Times.Once);
        }
    }
}
