// ============================================================================
// File: tests/TaskTree.Modules.TrayHost.Tests/TrayHostTests.cs
// Covers: Architecture §4.1; Roadmap P1E-AC1/AC2/AC3
// SPEC-DERIVED-PHASE1E-MSG2  HALT-Msg2 #1/#3/#4/#5/#6/#7/#8/#9
// Test count: 10. Gaps covered: #57 (compile-enforced), #58, #61, #62.
// Synthetic data only per D6.
// ============================================================================

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;

namespace TaskTree.Modules.TrayHost.Tests
{
    [TestClass]
    public class TrayHostTests
    {
        private static (TrayHost trayHost, Mock<IComplianceCore> compliance, Mock<IAppLogger> logger) Build()
        {
            // HALT-Msg2 #3 — Strict: any AuditAsync call fails the test (Gap #57 enforcement).
            var compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var trayHost = new TrayHost(logger.Object, compliance.Object);
            return (trayHost, compliance, logger);
        }

        [TestMethod, TestCategory("Offline")]
        public void Constructor_NullArgs_Throw()
        {
            var compliance = new Mock<IComplianceCore>().Object;
            var logger = new Mock<IAppLogger>().Object;
            Assert.ThrowsException<ArgumentNullException>(() => new TrayHost(null!, compliance));
            Assert.ThrowsException<ArgumentNullException>(() => new TrayHost(logger, null!));
        }

        [TestMethod, TestCategory("Offline")]
        public void Initialize_RequiresWpfApplicationContext()
        {
            var (trayHost, _, _) = Build();
            try
            {
                var ex = Assert.ThrowsException<InvalidOperationException>(() => trayHost.Initialize());
                StringAssert.Contains(ex.Message, "WPF Application");
            }
            finally { trayHost.Dispose(); }
        }

        [DataTestMethod, TestCategory("Offline")]
        [DataRow(null,    "msg",   typeof(ArgumentNullException))]
        [DataRow("title", null,    typeof(ArgumentNullException))]
        [DataRow("",      "msg",   typeof(ArgumentException))]
        [DataRow("title", "",      typeof(ArgumentException))]
        [DataRow("   ",   "msg",   typeof(ArgumentException))]
        [DataRow("title", "   ",   typeof(ArgumentException))]
        public void ShowBalloon_InvalidParams_Throws(string title, string message, Type expectedExceptionType)
        {
            var (trayHost, _, _) = Build();
            try
            {
                try
                {
                    trayHost.ShowBalloon(title, message);
                    Assert.Fail($"Expected {expectedExceptionType.Name}");
                }
                catch (AssertFailedException) { throw; }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, expectedExceptionType);
                }
            }
            finally { trayHost.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void ShowBalloon_BeforeInitialize_ThrowsInvalidOperationException()
        {
            var (trayHost, _, _) = Build();
            try
            {
                var ex = Assert.ThrowsException<InvalidOperationException>(
                    () => trayHost.ShowBalloon("Valid Title", "Valid Message"));
                StringAssert.Contains(ex.Message, "initialized");
            }
            finally { trayHost.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void RaiseShowTreeRequested_FiresEvent_WithCorrectSenderAndArgs()
        {
            var (trayHost, _, _) = Build();
            try
            {
                int count = 0;
                object? capturedSender = null;
                EventArgs? capturedArgs = null;
                trayHost.ShowTreeRequested += (s, e) => { count++; capturedSender = s; capturedArgs = e; };
                trayHost.RaiseShowTreeRequested();
                Assert.AreEqual(1, count);
                Assert.AreSame(trayHost, capturedSender);
                Assert.AreSame(EventArgs.Empty, capturedArgs);
            }
            finally { trayHost.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void RaiseAddTaskRequested_FiresEvent_WithCorrectSenderAndArgs()
        {
            var (trayHost, _, _) = Build();
            try
            {
                int count = 0;
                object? capturedSender = null;
                EventArgs? capturedArgs = null;
                trayHost.AddTaskRequested += (s, e) => { count++; capturedSender = s; capturedArgs = e; };
                trayHost.RaiseAddTaskRequested();
                Assert.AreEqual(1, count);
                Assert.AreSame(trayHost, capturedSender);
                Assert.AreSame(EventArgs.Empty, capturedArgs);
            }
            finally { trayHost.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void RaiseExitRequested_FiresEvent_WithCorrectSenderAndArgs()
        {
            var (trayHost, _, _) = Build();
            try
            {
                int count = 0;
                object? capturedSender = null;
                EventArgs? capturedArgs = null;
                trayHost.ExitRequested += (s, e) => { count++; capturedSender = s; capturedArgs = e; };
                trayHost.RaiseExitRequested();
                Assert.AreEqual(1, count);
                Assert.AreSame(trayHost, capturedSender);
                Assert.AreSame(EventArgs.Empty, capturedArgs);
            }
            finally { trayHost.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void RaiseEvents_NoSubscribers_DoesNotThrow()
        {
            var (trayHost, _, _) = Build();
            try
            {
                trayHost.RaiseShowTreeRequested();
                trayHost.RaiseAddTaskRequested();
                trayHost.RaiseExitRequested();
            }
            finally { trayHost.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var (trayHost, _, _) = Build();
            trayHost.Dispose();
            trayHost.Dispose();
        }

        [TestMethod, TestCategory("Offline")]
        public void Dispose_ThenAnyPublicMethod_ThrowsObjectDisposedException()
        {
            var (trayHost, _, _) = Build();
            trayHost.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => trayHost.Initialize());
            Assert.ThrowsException<ObjectDisposedException>(() => trayHost.ShowBalloon("title", "message"));
        }
    }
}
