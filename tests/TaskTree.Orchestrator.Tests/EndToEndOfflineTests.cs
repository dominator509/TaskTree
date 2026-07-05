// SPEC-DERIVED-PHASE1H  HALT #2/#3/#4/#5/#6/#7/#8/#9/#11
// Phase 5B/C backfill: offline provider coverage for the Phase 1H E2E skeleton.
// See PHASE1H-DERIVATIONS section 25 and docs/compile P5B-R016.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.ComplianceCore;
using TaskTree.Modules.ReminderScheduler;
using TaskTree.Modules.TaskEngine;
using TaskTree.TestSupport;

namespace TaskTree.Orchestrator.Tests
{
    [TestClass]
    public class EndToEndOfflineTests
    {
        private static readonly DateTimeOffset TestEpoch = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        [TestMethod, TestCategory("Offline")]
        public void OfflineProvider_ResolvesCoreRuntimeGraph()
        {
            using var provider = BuildOfflineProvider();

            Assert.IsNotNull(provider.GetRequiredService<IComplianceCore>());
            Assert.IsNotNull(provider.GetRequiredService<ITaskEngine>());
            Assert.IsNotNull(provider.GetRequiredService<IReminderScheduler>());
            Assert.IsNotNull(provider.GetRequiredService<IReminderDeliveryService>());
            Assert.IsNotNull(provider.GetRequiredService<IOrchestrator>());
        }

        [TestMethod, TestCategory("Offline")]
        public async Task TaskEngine_AddAsync_PersistsTaskAndKeepsAuditChainValid()
        {
            using var provider = BuildOfflineProvider();
            var engine = provider.GetRequiredService<ITaskEngine>();
            var compliance = provider.GetRequiredService<IComplianceCore>();

            var added = await engine.AddAsync(new TaskNode
            {
                Title = "synthetic task",
                Priority = Priority.Normal,
                Status = global::TaskTree.Core.Enums.TaskStatus.Active,
            });

            var tree = await engine.GetTreeAsync();
            var audit = await compliance.GetAuditChainAsync();

            Assert.AreEqual(added.Id, tree.Single().Id);
            Assert.AreEqual("TaskAdded", audit.Single().Action);
            Assert.IsTrue(await compliance.VerifyChainIntegrityAsync());
        }

        [TestMethod, TestCategory("Offline")]
        public async Task Orchestrator_StartStop_UsesOfflineGraphAndAuditsLifecycle()
        {
            using var provider = BuildOfflineProvider();
            var orchestrator = provider.GetRequiredService<IOrchestrator>();
            var compliance = provider.GetRequiredService<IComplianceCore>();

            await orchestrator.StartAsync(CancellationToken.None);
            await orchestrator.StopAsync();

            var actions = (await compliance.GetAuditChainAsync()).Select(x => x.Action).ToArray();
            CollectionAssert.Contains(actions, "Startup");
            CollectionAssert.Contains(actions, "Shutdown");
            Assert.IsTrue(await compliance.VerifyChainIntegrityAsync());
        }

        private static ServiceProvider BuildOfflineProvider()
        {
            var services = new ServiceCollection();
            var clock = new FakeClock(TestEpoch);
            var store = new InMemorySecureStore(preserveObjectReferences: true);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var trayHost = new Mock<ITrayHost>(MockBehavior.Loose);
            var sessionLock = new Mock<ISessionLockService>(MockBehavior.Loose);
            var settings = new Mock<ISettingsService>(MockBehavior.Loose);
            var snooze = new Mock<ISnoozeService>(MockBehavior.Loose);

            trayHost.Setup(x => x.Initialize());
            trayHost.Setup(x => x.Dispose());
            sessionLock.SetupGet(x => x.IsLocked).Returns(false);
            sessionLock.Setup(x => x.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            sessionLock.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);
            settings.Setup(x => x.GetAsync()).ReturnsAsync(TaskTreeSettings.Default);
            snooze.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync((SnoozeState?)null);

            services.AddSingleton<IClock>(clock);
            services.AddSingleton<ISecureStore>(store);
            services.AddSingleton<IAppLogger>(logger.Object);
            services.AddSingleton<ITrayHost>(trayHost.Object);
            services.AddSingleton<ISessionLockService>(sessionLock.Object);
            services.AddSingleton<ISettingsService>(settings.Object);
            services.AddSingleton<ISnoozeService>(snooze.Object);
            services.AddSingleton<PhiRedactor>(_ => new PhiRedactor(Array.Empty<string>()));
            services.AddSingleton<AuditChainWriter>();
            services.AddSingleton<IComplianceCore, ComplianceCore>();
            services.AddSingleton<ITaskEngine, TaskEngine>();
            services.AddSingleton<IReminderScheduler, ReminderScheduler>();
            services.AddSingleton<global::TaskTree.Orchestrator.ToastTier1Adapter>();
            services.AddSingleton<global::TaskTree.Orchestrator.ToastTier2Adapter>();
            services.AddSingleton<global::TaskTree.Orchestrator.ToastTier3Adapter>();
            services.AddSingleton<IReminderDeliveryService, global::TaskTree.Orchestrator.ReminderDeliveryService>();
            services.AddSingleton<IOrchestrator, global::TaskTree.Orchestrator.Orchestrator>();

            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}
