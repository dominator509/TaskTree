// ============================================================================
// File: tests/TaskTree.Modules.ReminderScheduler.Tests/ReminderSchedulerTests.cs
// Covers: Architecture §4.3, §5.3, §15; Roadmap P1D-AC1/AC2/AC3/AC4
// SPEC-DERIVED-PHASE1D-MSG2  HALT-Msg2 #2/#5/#6/#7/#8/#9/#10/#11/#13
// Test count: 10. Gaps covered: #38/#39/#41/#42/#43/#48/#54/#55.
// Synthetic data only per D6.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.TestSupport;
using TaskStatus = TaskTree.Core.Enums.TaskStatus;

namespace TaskTree.Modules.ReminderScheduler.Tests
{
    [TestClass]
    public class ReminderSchedulerTests
    {
        private static (
            ReminderScheduler scheduler,
            FakeClock clock,
            Mock<ITaskEngine> engine,
            Mock<IComplianceCore> compliance,
            Mock<IAppLogger> logger)
        Build(List<TaskNode>? initialTree = null)
        {
            var clock = new FakeClock(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));
            var engine = new Mock<ITaskEngine>(MockBehavior.Strict);
            engine.Setup(e => e.GetTreeAsync())
                  .ReturnsAsync((IReadOnlyList<TaskNode>)(initialTree ?? new List<TaskNode>()));
            var compliance = new Mock<IComplianceCore>(MockBehavior.Loose);
            compliance.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var scheduler = new ReminderScheduler(clock, engine.Object, compliance.Object, logger.Object);
            return (scheduler, clock, engine, compliance, logger);
        }

        private static TaskNode MakeNode(Priority p, DateTimeOffset? deadline = null, TaskStatus status = TaskStatus.Active)
            => new()
            {
                Id = Guid.NewGuid(),
                Title = "synthetic-task",
                Priority = p,
                Deadline = deadline,
                Status = status,
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ModifiedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            };

        [TestMethod, TestCategory("Offline")]
        public void Constructor_NullArgs_Throw()
        {
            var clock = new FakeClock();
            var engine = new Mock<ITaskEngine>().Object;
            var compliance = new Mock<IComplianceCore>().Object;
            var logger = new Mock<IAppLogger>().Object;
            Assert.ThrowsException<ArgumentNullException>(() => new ReminderScheduler(null!, engine, compliance, logger));
            Assert.ThrowsException<ArgumentNullException>(() => new ReminderScheduler(clock, null!, compliance, logger));
            Assert.ThrowsException<ArgumentNullException>(() => new ReminderScheduler(clock, engine, null!, logger));
            Assert.ThrowsException<ArgumentNullException>(() => new ReminderScheduler(clock, engine, compliance, null!));
        }

        [TestMethod, TestCategory("Offline")]
        public void Cadence_InRangeBoundaryInclusive_Accepted()
        {
            var (s, _, _, _, _) = Build();
            try
            {
                s.Cadence = TimeSpan.FromSeconds(1); Assert.AreEqual(TimeSpan.FromSeconds(1), s.Cadence);
                s.Cadence = TimeSpan.FromMinutes(5); Assert.AreEqual(TimeSpan.FromMinutes(5), s.Cadence);
                s.Cadence = TimeSpan.FromSeconds(30); Assert.AreEqual(TimeSpan.FromSeconds(30), s.Cadence);
                s.Cadence = TimeSpan.FromMinutes(2.5); Assert.AreEqual(TimeSpan.FromMinutes(2.5), s.Cadence);
            }
            finally { s.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void Cadence_OutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var (s, _, _, _, _) = Build();
            try
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => s.Cadence = TimeSpan.FromMilliseconds(999));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => s.Cadence = TimeSpan.FromMinutes(5).Add(TimeSpan.FromMilliseconds(1)));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => s.Cadence = TimeSpan.Zero);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => s.Cadence = TimeSpan.FromHours(1));
            }
            finally { s.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task StartAsync_CalledTwice_ThrowsInvalidOperationException()
        {
            var (s, _, _, _, _) = Build();
            await s.StartAsync(CancellationToken.None);
            try
            {
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                    async () => await s.StartAsync(CancellationToken.None));
            }
            finally
            {
                await s.StopAsync();
                s.Dispose();
            }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task StartAsync_WhenStartupLogFails_CleansUpAndAllowsRetry()
        {
            var (scheduler, _, _, _, logger) = Build();
            var fail = true;
            logger.Setup(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object?[]>()))
                .Callback(() =>
                {
                    if (fail) throw new InvalidOperationException("log unavailable");
                });

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => scheduler.StartAsync(CancellationToken.None));

            fail = false;
            await scheduler.StartAsync(CancellationToken.None);
            await scheduler.StopAsync();
            scheduler.Dispose();
        }

        [TestMethod, TestCategory("Offline")]
        public async Task TickOnceAsync_FiresReminderAndAuditsAndPopulatesEvent()
        {
            var node = MakeNode(Priority.Critical, deadline: null);
            var tree = new List<TaskNode> { node };
            var (s, clock, _, compliance, _) = Build(tree);
            try
            {
                ReminderEvent? captured = null;
                int callCount = 0;
                s.ReminderDue += (_, e) => { captured = e; callCount++; };

                await s.TickOnceAsync(CancellationToken.None);

                Assert.AreEqual(1, callCount);
                Assert.IsNotNull(captured);
                Assert.AreEqual(node.Id, captured!.TaskId);
                Assert.AreEqual(clock.UtcNow, captured.FiredAtUtc);
                Assert.AreEqual(ReminderReason.Initial, captured.Reason);
                Assert.AreEqual(Priority.Critical, captured.Priority);
                compliance.Verify(c => c.AuditAsync(It.Is<AuditEntry>(e =>
                    e.Module == "ReminderScheduler" &&
                    e.Action == "ReminderFired" &&
                    e.TargetId == node.Id &&
                    e.Result == "success")), Times.Once);
            }
            finally { s.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task TickOnceAsync_DoneStatusSkipped_NoFireNoAudit()
        {
            var node = MakeNode(Priority.Critical, deadline: null, status: TaskStatus.Done);
            var tree = new List<TaskNode> { node };
            var (s, _, engine, compliance, _) = Build(tree);
            try
            {
                int callCount = 0;
                s.ReminderDue += (_, _) => callCount++;
                await s.TickOnceAsync(CancellationToken.None);
                Assert.AreEqual(0, callCount);
                compliance.Verify(c => c.AuditAsync(It.IsAny<AuditEntry>()), Times.Never);
                engine.Verify(e => e.GetTreeAsync(), Times.Once);
            }
            finally { s.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task StartAsync_RunsLoop_ProducesAtLeastOneTick()
        {
            var node = MakeNode(Priority.Critical, deadline: null);
            var tree = new List<TaskNode> { node };
            var (s, _, _, _, _) = Build(tree);
            try
            {
                s.Cadence = TimeSpan.FromSeconds(1);
                int callCount = 0;
                var tickObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                s.ReminderDue += (_, _) =>
                {
                    Interlocked.Increment(ref callCount);
                    tickObserved.TrySetResult(true);
                };
                await s.StartAsync(CancellationToken.None);
                var completed = await Task.WhenAny(tickObserved.Task, Task.Delay(TimeSpan.FromSeconds(3)));
                await s.StopAsync();
                Assert.AreSame(tickObserved.Task, completed, "Reminder loop did not produce a tick within the bounded test window.");
                Assert.IsTrue(callCount >= 1, $"Expected >= 1 tick; got {callCount}");
            }
            finally { s.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task StopAsync_BoundedWait_ReturnsBeforeStuckTickCompletes()
        {
            // HALT-Msg2 #11 / Gap #38
            var node = MakeNode(Priority.Critical, deadline: null);
            var tree = new List<TaskNode> { node };
            var clock = new FakeClock(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));
            var tickStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var engine = new Mock<ITaskEngine>(MockBehavior.Strict);
            engine.Setup(e => e.GetTreeAsync()).Returns(async () =>
            {
                tickStarted.TrySetResult(true);
                await Task.Delay(2000);
                return (IReadOnlyList<TaskNode>)tree;
            });
            var compliance = new Mock<IComplianceCore>(MockBehavior.Loose);
            compliance.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var s = new ReminderScheduler(clock, engine.Object, compliance.Object, logger.Object);

            var originalTimeout = ReminderScheduler.StopWaitTimeout;
            ReminderScheduler.StopWaitTimeout = TimeSpan.FromMilliseconds(200);
            try
            {
                s.Cadence = TimeSpan.FromSeconds(1);
                await s.StartAsync(CancellationToken.None);
                Assert.AreSame(
                    tickStarted.Task,
                    await Task.WhenAny(tickStarted.Task, Task.Delay(TimeSpan.FromSeconds(3))));
                var sw = Stopwatch.StartNew();
                await s.StopAsync();
                sw.Stop();
                Assert.IsTrue(sw.ElapsedMilliseconds < 800, $"StopAsync took {sw.ElapsedMilliseconds}ms");
                logger.Verify(l => l.LogWarning(It.Is<string>(msg =>
                    msg.Contains("StopAsync") || msg.Contains("did not complete"))), Times.AtLeastOnce);
            }
            finally
            {
                ReminderScheduler.StopWaitTimeout = originalTimeout;
                s.Dispose();
            }
        }

        [TestMethod, TestCategory("Offline")]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var (s, _, _, _, _) = Build();
            s.Dispose();
            s.Dispose();
        }

        [TestMethod, TestCategory("Offline")]
        public async Task Dispose_WhileRunning_StopsLoopAndPreventsRestart()
        {
            var (s, _, _, _, _) = Build();
            s.Cadence = TimeSpan.FromSeconds(1);
            await s.StartAsync(CancellationToken.None);
            s.Dispose();
            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(
                async () => await s.StartAsync(CancellationToken.None));
        }
    }
}
