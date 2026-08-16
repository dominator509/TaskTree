// SPEC-DERIVED-PHASE2F  HALT #16 (12 tests)

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Modules.SessionLock;
using TaskTree.TestSupport;

namespace TaskTree.Modules.SessionLock.Tests
{
    [TestClass]
    public class SessionLockServiceTests
    {
        private static readonly bool[] ExpectedTransitionOrder = [true, false];
        private static (SessionLockService svc, Mock<IComplianceCore> comp) Build(){var comp=new Mock<IComplianceCore>(MockBehavior.Strict);comp.Setup(c=>c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);var log=new Mock<IAppLogger>(MockBehavior.Loose);return(new SessionLockService(new FakeClock(),comp.Object,log.Object),comp);}
        [TestMethod] public void Constructor_NullArgs_Throw(){var c=new FakeClock();var comp=new Mock<IComplianceCore>().Object;var log=new Mock<IAppLogger>().Object;Assert.ThrowsException<ArgumentNullException>(()=>new SessionLockService(null!,comp,log));Assert.ThrowsException<ArgumentNullException>(()=>new SessionLockService(c,null!,log));Assert.ThrowsException<ArgumentNullException>(()=>new SessionLockService(c,comp,null!));}
        [TestMethod] public async Task StartAsync_WhenNotRunning_SetsRunningAndAudits(){var(svc,comp)=Build();await svc.StartAsync(CancellationToken.None);comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLockStarted")),Times.Once);}
        [TestMethod]
        public async Task StartAsync_WhenStartupAuditFails_CleansUpAndAllowsRetry()
        {
            var comp = new Mock<IComplianceCore>(MockBehavior.Strict);
            var fail = true;
            comp.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>()))
                .Returns(() => fail
                    ? Task.FromException(new InvalidOperationException("audit unavailable"))
                    : Task.CompletedTask);
            var svc = new SessionLockService(new FakeClock(), comp.Object, new Mock<IAppLogger>().Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => svc.StartAsync(CancellationToken.None));

            fail = false;
            await svc.StartAsync(CancellationToken.None);
            await svc.StopAsync();
        }
        [TestMethod] public async Task StartAsync_CalledTwice_ThrowsInvalidOperationException(){var(svc,_)=Build();await svc.StartAsync(CancellationToken.None);await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>svc.StartAsync(CancellationToken.None));}
        [TestMethod] public async Task StopAsync_WhenNotRunning_IsNoOp(){var(svc,comp)=Build();await svc.StopAsync();comp.Verify(c=>c.AuditAsync(It.IsAny<AuditEntry>()),Times.Never);}
        [TestMethod] public async Task StopAsync_WhenRunning_AuditsStopped(){var(svc,comp)=Build();await svc.StartAsync(CancellationToken.None);await svc.StopAsync();comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLockStopped")),Times.Once);}
        [TestMethod] public async Task RaiseLockedForTestsAsync_SetsIsLockedTrue(){var(svc,_)=Build();await svc.RaiseLockedForTestsAsync();Assert.IsTrue(svc.IsLocked);}
        [TestMethod] public async Task RaiseLockedForTestsAsync_RaisesEvent(){var(svc,_)=Build();var raised=false;svc.SessionLockChanged+=(s,e)=>raised=e.IsLocked;await svc.RaiseLockedForTestsAsync();Assert.IsTrue(raised);}
        [TestMethod] public async Task RaiseLockedForTestsAsync_AuditsLocked(){var(svc,comp)=Build();await svc.RaiseLockedForTestsAsync();comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLocked")),Times.Once);}
        [TestMethod]
        public async Task Transition_WhenAuditFails_RollsBackStateAndAllowsRetry()
        {
            var comp = new Mock<IComplianceCore>(MockBehavior.Strict);
            var fail = true;
            comp.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>()))
                .Returns(() => fail
                    ? Task.FromException(new InvalidOperationException("audit unavailable"))
                    : Task.CompletedTask);
            var svc = new SessionLockService(new FakeClock(), comp.Object, new Mock<IAppLogger>().Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => svc.RaiseLockedForTestsAsync());
            Assert.IsFalse(svc.IsLocked);

            var raised = false;
            svc.SessionLockChanged += (_, args) => raised = args.IsLocked;
            fail = false;
            await svc.RaiseLockedForTestsAsync();

            Assert.IsTrue(svc.IsLocked);
            Assert.IsTrue(raised);
            comp.Verify(c => c.AuditAsync(It.Is<AuditEntry>(e => e.Action == "SessionLocked")), Times.Exactly(2));
        }
        [TestMethod] public async Task RaiseUnlockedForTestsAsync_SetsIsLockedFalse(){var(svc,_)=Build();await svc.RaiseLockedForTestsAsync();await svc.RaiseUnlockedForTestsAsync();Assert.IsFalse(svc.IsLocked);}
        [TestMethod] public async Task DuplicateLockEvent_Suppressed(){var(svc,comp)=Build();await svc.RaiseLockedForTestsAsync();await svc.RaiseLockedForTestsAsync();comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLocked")),Times.Once);}
        [TestMethod]
        public async Task ConcurrentTransitions_PublishInTransitionOrder()
        {
            var comp = new Mock<IComplianceCore>(MockBehavior.Strict);
            var lockAuditStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var lockAudit = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            comp.Setup(c => c.AuditAsync(It.Is<AuditEntry>(e => e.Action == "SessionLocked")))
                .Callback<AuditEntry>(_ => lockAuditStarted.TrySetResult(true))
                .Returns(lockAudit.Task);
            comp.Setup(c => c.AuditAsync(It.Is<AuditEntry>(e => e.Action == "SessionUnlocked")))
                .Returns(Task.CompletedTask);

            var svc = new SessionLockService(new FakeClock(), comp.Object, new Mock<IAppLogger>().Object);
            var transitions = new List<bool>();
            svc.SessionLockChanged += (_, args) => transitions.Add(args.IsLocked);

            var locking = svc.RaiseLockedForTestsAsync();
            await lockAuditStarted.Task;
            var unlocking = svc.RaiseUnlockedForTestsAsync();
            await Task.Delay(25);
            lockAudit.TrySetResult(null);

            await Task.WhenAll(locking, unlocking);

            CollectionAssert.AreEqual(ExpectedTransitionOrder, transitions);
        }
        [TestMethod]
        public async Task Dispose_DuringAuditedTransition_SuppressesChangeEvent()
        {
            var comp = new Mock<IComplianceCore>(MockBehavior.Strict);
            var auditStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var audit = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            comp.Setup(c => c.AuditAsync(It.Is<AuditEntry>(e => e.Action == "SessionLocked")))
                .Callback<AuditEntry>(_ => auditStarted.TrySetResult(true))
                .Returns(audit.Task);

            var svc = new SessionLockService(new FakeClock(), comp.Object, new Mock<IAppLogger>().Object);
            var raised = false;
            svc.SessionLockChanged += (_, _) => raised = true;

            var locking = svc.RaiseLockedForTestsAsync();
            await auditStarted.Task;
            svc.Dispose();
            audit.TrySetResult(null);

            await locking;

            Assert.IsFalse(raised);
        }

        [TestMethod]
        public async Task Dispose_DuringStartAudit_WaitsForLifecycleOperation()
        {
            var comp = new Mock<IComplianceCore>(MockBehavior.Strict);
            var auditStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var audit = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            comp.Setup(c => c.AuditAsync(It.Is<AuditEntry>(e => e.Action == "SessionLockStarted")))
                .Callback<AuditEntry>(_ => auditStarted.TrySetResult(true))
                .Returns(audit.Task);

            var svc = new SessionLockService(new FakeClock(), comp.Object, new Mock<IAppLogger>().Object);
            var starting = svc.StartAsync(CancellationToken.None);
            await auditStarted.Task;

            var disposing = Task.Run(svc.Dispose);
            await Task.Delay(25);
            Assert.IsFalse(disposing.IsCompleted);

            audit.TrySetResult(null);
            await starting;
            await disposing;

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(
                () => svc.StopAsync());
        }
        [TestMethod] public void Dispose_CalledTwice_DoesNotThrow(){var(svc,_)=Build();svc.Dispose();svc.Dispose();}
        [TestMethod] public async Task Dispose_ThenStartAsync_ThrowsObjectDisposedException(){var(svc,_)=Build();svc.Dispose();await Assert.ThrowsExceptionAsync<ObjectDisposedException>(()=>svc.StartAsync(CancellationToken.None));}
    }
}
