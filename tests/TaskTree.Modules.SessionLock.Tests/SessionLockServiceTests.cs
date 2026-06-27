// SPEC-DERIVED-PHASE2F  HALT #16 (12 tests)

using System;
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
        private static (SessionLockService svc, Mock<IComplianceCore> comp) Build(){var comp=new Mock<IComplianceCore>(MockBehavior.Strict);comp.Setup(c=>c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);var log=new Mock<IAppLogger>(MockBehavior.Loose);return(new SessionLockService(new FakeClock(),comp.Object,log.Object),comp);}
        [TestMethod] public void Constructor_NullArgs_Throw(){var c=new FakeClock();var comp=new Mock<IComplianceCore>().Object;var log=new Mock<IAppLogger>().Object;Assert.ThrowsException<ArgumentNullException>(()=>new SessionLockService(null!,comp,log));Assert.ThrowsException<ArgumentNullException>(()=>new SessionLockService(c,null!,log));Assert.ThrowsException<ArgumentNullException>(()=>new SessionLockService(c,comp,null!));}
        [TestMethod] public async Task StartAsync_WhenNotRunning_SetsRunningAndAudits(){var(svc,comp)=Build();await svc.StartAsync(CancellationToken.None);comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLockStarted")),Times.Once);}
        [TestMethod] public async Task StartAsync_CalledTwice_ThrowsInvalidOperationException(){var(svc,_)=Build();await svc.StartAsync(CancellationToken.None);await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>svc.StartAsync(CancellationToken.None));}
        [TestMethod] public async Task StopAsync_WhenNotRunning_IsNoOp(){var(svc,comp)=Build();await svc.StopAsync();comp.Verify(c=>c.AuditAsync(It.IsAny<AuditEntry>()),Times.Never);}
        [TestMethod] public async Task StopAsync_WhenRunning_AuditsStopped(){var(svc,comp)=Build();await svc.StartAsync(CancellationToken.None);await svc.StopAsync();comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLockStopped")),Times.Once);}
        [TestMethod] public async Task RaiseLockedForTestsAsync_SetsIsLockedTrue(){var(svc,_)=Build();await svc.RaiseLockedForTestsAsync();Assert.IsTrue(svc.IsLocked);}
        [TestMethod] public async Task RaiseLockedForTestsAsync_RaisesEvent(){var(svc,_)=Build();var raised=false;svc.SessionLockChanged+=(s,e)=>raised=e.IsLocked;await svc.RaiseLockedForTestsAsync();Assert.IsTrue(raised);}
        [TestMethod] public async Task RaiseLockedForTestsAsync_AuditsLocked(){var(svc,comp)=Build();await svc.RaiseLockedForTestsAsync();comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLocked")),Times.Once);}
        [TestMethod] public async Task RaiseUnlockedForTestsAsync_SetsIsLockedFalse(){var(svc,_)=Build();await svc.RaiseLockedForTestsAsync();await svc.RaiseUnlockedForTestsAsync();Assert.IsFalse(svc.IsLocked);}
        [TestMethod] public async Task DuplicateLockEvent_Suppressed(){var(svc,comp)=Build();await svc.RaiseLockedForTestsAsync();await svc.RaiseLockedForTestsAsync();comp.Verify(c=>c.AuditAsync(It.Is<AuditEntry>(e=>e.Action=="SessionLocked")),Times.Once);}
        [TestMethod] public void Dispose_CalledTwice_DoesNotThrow(){var(svc,_)=Build();svc.Dispose();svc.Dispose();}
        [TestMethod] public async Task Dispose_ThenStartAsync_ThrowsObjectDisposedException(){var(svc,_)=Build();svc.Dispose();await Assert.ThrowsExceptionAsync<ObjectDisposedException>(()=>svc.StartAsync(CancellationToken.None));}
    }
}
