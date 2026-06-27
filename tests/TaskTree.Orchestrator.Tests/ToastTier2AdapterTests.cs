// SPEC-DERIVED-PHASE2F  HALT #17 headless-safe test
// Gap #178: visible ReminderToast hide-on-lock test deferred to Phase 5C WPF integration.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Orchestrator;

namespace TaskTree.Orchestrator.Tests
{
    [TestClass]
    public class ToastTier2AdapterTests
    {
        [TestMethod]
        public void TryDeliver_WhenSessionLocked_ReturnsFalse()
        {
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var session = new Mock<ISessionLockService>(MockBehavior.Strict);
            session.SetupGet(s => s.IsLocked).Returns(true);
            session.SetupAdd(s => s.SessionLockChanged += It.IsAny<System.EventHandler<SessionLockChangedEventArgs>>());
            var adapter = new ToastTier2Adapter(logger.Object, session.Object);
            Assert.IsFalse(adapter.TryDeliver(new ReminderEvent()));
        }
    }
}
