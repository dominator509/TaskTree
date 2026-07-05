// SPEC-DERIVED-PHASE1F-MSG2  HALT-Msg2 #1/#3-#14
// SPEC-DERIVED-PHASE1G-MSG2  Gap #93/#95 7-param ctor patch
// SPEC-DERIVED-PHASE1H  Gap #95 backfill (partial offline implementation)
// 15-test plan in PHASE1H-DERIVATIONS section 25; see docs/compile P5B-R015.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Orchestrator;
using TaskTree.TestSupport;

namespace TaskTree.Orchestrator.Tests
{
    [TestClass]
    public class OrchestratorTests
    {
        [TestMethod, TestCategory("Offline")]
        public void Constructor_NullDependencies_ThrowArgumentNullException()
        {
            var h = Harness.Create();

            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(null!, h.ReminderScheduler.Object, h.Compliance.Object, h.TrayHost.Object, h.ReminderDelivery.Object, h.SettingsService.Object, h.SessionLock.Object, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, null!, h.Compliance.Object, h.TrayHost.Object, h.ReminderDelivery.Object, h.SettingsService.Object, h.SessionLock.Object, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, null!, h.TrayHost.Object, h.ReminderDelivery.Object, h.SettingsService.Object, h.SessionLock.Object, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, h.Compliance.Object, null!, h.ReminderDelivery.Object, h.SettingsService.Object, h.SessionLock.Object, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, h.Compliance.Object, h.TrayHost.Object, null!, h.SettingsService.Object, h.SessionLock.Object, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, h.Compliance.Object, h.TrayHost.Object, h.ReminderDelivery.Object, null!, h.SessionLock.Object, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, h.Compliance.Object, h.TrayHost.Object, h.ReminderDelivery.Object, h.SettingsService.Object, null!, h.Logger.Object, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, h.Compliance.Object, h.TrayHost.Object, h.ReminderDelivery.Object, h.SettingsService.Object, h.SessionLock.Object, null!, h.Clock));
            Assert.ThrowsException<ArgumentNullException>(() => new Orchestrator(h.TaskEngine.Object, h.ReminderScheduler.Object, h.Compliance.Object, h.TrayHost.Object, h.ReminderDelivery.Object, h.SettingsService.Object, h.SessionLock.Object, h.Logger.Object, null!));
        }

        [TestMethod, TestCategory("Offline")]
        public async Task StartAsync_StartsDependenciesAndAuditsStartup()
        {
            var h = Harness.Create();
            var orchestrator = h.Create();

            await orchestrator.StartAsync(CancellationToken.None);

            h.TrayHost.Verify(x => x.Initialize(), Times.Once);
            h.SessionLock.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
            h.ReminderScheduler.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
            h.ReminderDelivery.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(1, h.AuditEntries.Count);
            Assert.AreEqual("Orchestrator", h.AuditEntries[0].Module);
            Assert.AreEqual("Startup", h.AuditEntries[0].Action);
            Assert.AreEqual("success", h.AuditEntries[0].Result);
        }

        [TestMethod, TestCategory("Offline")]
        public async Task StopAsync_AfterStart_UnsubscribesStopsAndAuditsShutdown()
        {
            var h = Harness.Create();
            var orchestrator = h.Create();

            await orchestrator.StartAsync(CancellationToken.None);
            await orchestrator.StopAsync();

            Assert.AreEqual(0, h.TrayHostShowTreeSubscriptions);
            Assert.AreEqual(0, h.TrayHostAddTaskSubscriptions);
            Assert.AreEqual(0, h.TrayHostExitSubscriptions);
            Assert.AreEqual(0, h.SessionLockSubscriptions);
            h.SessionLock.Verify(x => x.StopAsync(), Times.Once);
            h.ReminderDelivery.Verify(x => x.StopAsync(), Times.Once);
            h.ReminderScheduler.Verify(x => x.StopAsync(), Times.Once);
            h.TrayHost.Verify(x => x.Dispose(), Times.Once);
            Assert.AreEqual(2, h.AuditEntries.Count);
            Assert.AreEqual("Shutdown", h.AuditEntries[1].Action);
            Assert.AreEqual("success", h.AuditEntries[1].Result);
        }

        private sealed class Harness
        {
            private static readonly DateTimeOffset TestNow = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

            private Harness()
            {
                TaskEngine = new Mock<ITaskEngine>(MockBehavior.Strict);
                ReminderScheduler = new Mock<IReminderScheduler>(MockBehavior.Strict);
                Compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
                TrayHost = new Mock<ITrayHost>(MockBehavior.Strict);
                ReminderDelivery = new Mock<IReminderDeliveryService>(MockBehavior.Strict);
                SettingsService = new Mock<ISettingsService>(MockBehavior.Strict);
                SessionLock = new Mock<ISessionLockService>(MockBehavior.Strict);
                Logger = new Mock<IAppLogger>(MockBehavior.Loose);
                Clock = new FakeClock(TestNow);

                ReminderScheduler.SetupProperty(x => x.Cadence, TimeSpan.FromSeconds(30));
                ReminderScheduler.SetupAdd(x => x.ReminderDue += It.IsAny<EventHandler<ReminderEvent>>());
                ReminderScheduler.SetupRemove(x => x.ReminderDue -= It.IsAny<EventHandler<ReminderEvent>>());
                ReminderScheduler.Setup(x => x.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
                ReminderScheduler.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

                ReminderDelivery.Setup(x => x.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
                ReminderDelivery.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

                SessionLock.SetupGet(x => x.IsLocked).Returns(false);
                SessionLock.SetupAdd(x => x.SessionLockChanged += It.IsAny<EventHandler<SessionLockChangedEventArgs>>()).Callback<EventHandler<SessionLockChangedEventArgs>>(_ => SessionLockSubscriptions++);
                SessionLock.SetupRemove(x => x.SessionLockChanged -= It.IsAny<EventHandler<SessionLockChangedEventArgs>>()).Callback<EventHandler<SessionLockChangedEventArgs>>(_ => SessionLockSubscriptions--);
                SessionLock.Setup(x => x.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
                SessionLock.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

                TrayHost.SetupAdd(x => x.ShowTreeRequested += It.IsAny<EventHandler>()).Callback<EventHandler>(_ => TrayHostShowTreeSubscriptions++);
                TrayHost.SetupRemove(x => x.ShowTreeRequested -= It.IsAny<EventHandler>()).Callback<EventHandler>(_ => TrayHostShowTreeSubscriptions--);
                TrayHost.SetupAdd(x => x.AddTaskRequested += It.IsAny<EventHandler>()).Callback<EventHandler>(_ => TrayHostAddTaskSubscriptions++);
                TrayHost.SetupRemove(x => x.AddTaskRequested -= It.IsAny<EventHandler>()).Callback<EventHandler>(_ => TrayHostAddTaskSubscriptions--);
                TrayHost.SetupAdd(x => x.ExitRequested += It.IsAny<EventHandler>()).Callback<EventHandler>(_ => TrayHostExitSubscriptions++);
                TrayHost.SetupRemove(x => x.ExitRequested -= It.IsAny<EventHandler>()).Callback<EventHandler>(_ => TrayHostExitSubscriptions--);
                TrayHost.Setup(x => x.Initialize());
                TrayHost.Setup(x => x.Dispose());

                Compliance.Setup(x => x.AuditAsync(It.IsAny<AuditEntry>()))
                    .Callback<AuditEntry>(AuditEntries.Add)
                    .Returns(Task.CompletedTask);
            }

            public Mock<ITaskEngine> TaskEngine { get; }
            public Mock<IReminderScheduler> ReminderScheduler { get; }
            public Mock<IComplianceCore> Compliance { get; }
            public Mock<ITrayHost> TrayHost { get; }
            public Mock<IReminderDeliveryService> ReminderDelivery { get; }
            public Mock<ISettingsService> SettingsService { get; }
            public Mock<ISessionLockService> SessionLock { get; }
            public Mock<IAppLogger> Logger { get; }
            public FakeClock Clock { get; }
            public List<AuditEntry> AuditEntries { get; } = new();
            public int TrayHostShowTreeSubscriptions { get; private set; }
            public int TrayHostAddTaskSubscriptions { get; private set; }
            public int TrayHostExitSubscriptions { get; private set; }
            public int SessionLockSubscriptions { get; private set; }

            public static Harness Create() => new();

            public Orchestrator Create(
                ITaskEngine? taskEngine = null,
                IReminderScheduler? reminderScheduler = null,
                IComplianceCore? compliance = null,
                ITrayHost? trayHost = null,
                IReminderDeliveryService? reminderDelivery = null,
                ISettingsService? settingsService = null,
                ISessionLockService? sessionLock = null,
                IAppLogger? logger = null,
                IClock? clock = null)
            {
                return new Orchestrator(
                    taskEngine ?? TaskEngine.Object,
                    reminderScheduler ?? ReminderScheduler.Object,
                    compliance ?? Compliance.Object,
                    trayHost ?? TrayHost.Object,
                    reminderDelivery ?? ReminderDelivery.Object,
                    settingsService ?? SettingsService.Object,
                    sessionLock ?? SessionLock.Object,
                    logger ?? Logger.Object,
                    clock ?? Clock);
            }
        }
    }
}
