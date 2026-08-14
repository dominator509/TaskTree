// SPEC-DERIVED-PHASE2A HALT #19 (10 tests for P2A-AC1/AC2/AC3)

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.TestSupport;

namespace TaskTree.Modules.TrayHost.Tests
{
    [TestClass]
    public class HotkeyManagerTests
    {
        private static readonly DateTimeOffset TestEpoch = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        private static (HotkeyManager mgr, FakeClock clock, InMemorySecureStore store, Mock<IComplianceCore> compliance, Mock<IAppLogger> logger) Build()
        {
            var clock = new FakeClock(TestEpoch);
            var store = new InMemorySecureStore();
            var compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
            compliance.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var mgr = new HotkeyManager(logger.Object, compliance.Object, store, clock);
            return (mgr, clock, store, compliance, logger);
        }

        [TestMethod, TestCategory("Offline")]
        public void Constructor_NullArgs_Throw()
        {
            var clock = new FakeClock(TestEpoch);
            var store = new InMemorySecureStore();
            var compliance = new Mock<IComplianceCore>().Object;
            var logger = new Mock<IAppLogger>().Object;
            Assert.ThrowsException<ArgumentNullException>(() => new HotkeyManager(null!, compliance, store, clock));
            Assert.ThrowsException<ArgumentNullException>(() => new HotkeyManager(logger, null!, store, clock));
            Assert.ThrowsException<ArgumentNullException>(() => new HotkeyManager(logger, compliance, null!, clock));
            Assert.ThrowsException<ArgumentNullException>(() => new HotkeyManager(logger, compliance, store, null!));
        }

        [TestMethod, TestCategory("Offline")]
        public async Task GetDefaultConfigAsync_ReturnsCtrlAltT()
        {
            var (mgr, _, _, _, _) = Build();
            try
            {
                var cfg = await mgr.GetDefaultConfigAsync();
                Assert.IsTrue(cfg.Ctrl);
                Assert.IsTrue(cfg.Alt);
                Assert.IsFalse(cfg.Shift);
                Assert.IsFalse(cfg.Win);
                Assert.AreEqual(0x54, cfg.VirtualKey);
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task GetCurrentConfigAsync_NoPriorConfig_ReturnsDefault()
        {
            var (mgr, _, _, _, _) = Build();
            try
            {
                var cfg = await mgr.GetCurrentConfigAsync();
                Assert.AreEqual(HotkeyConfig.Default, cfg);
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task GetCurrentConfigAsync_AfterSet_ReturnsSaved()
        {
            var (mgr, _, _, _, _) = Build();
            try
            {
                var custom = new HotkeyConfig(Ctrl: false, Alt: false, Shift: true, Win: false, VirtualKey: 0x41);
                await mgr.SetConfigAsync(custom);
                var loaded = await mgr.GetCurrentConfigAsync();
                Assert.AreEqual(custom, loaded);
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task SetConfigAsync_ValidConfig_PersistsAndAudits()
        {
            var (mgr, _, _, compliance, _) = Build();
            try
            {
                var custom = new HotkeyConfig(Ctrl: true, Alt: false, Shift: false, Win: false, VirtualKey: 0x41);
                var result = await mgr.SetConfigAsync(custom);
                Assert.AreEqual(HotkeyManager.HotkeyRegistrationResult.Success, result);
                compliance.Verify(c => c.AuditAsync(It.Is<AuditEntry>(e =>
                    e.Module == "HotkeyManager" && e.Action == "HotkeyConfigChanged" && e.Result == "success")),
                    Times.Once);
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task SetConfigAsync_InvalidConfig_ReturnsInvalidConfig_NoAudit()
        {
            var (mgr, _, _, compliance, _) = Build();
            try
            {
                var invalid = new HotkeyConfig(Ctrl: false, Alt: false, Shift: false, Win: false, VirtualKey: 0x41);
                var result = await mgr.SetConfigAsync(invalid);
                Assert.AreEqual(HotkeyManager.HotkeyRegistrationResult.InvalidConfig, result);
                compliance.Verify(c => c.AuditAsync(It.IsAny<AuditEntry>()), Times.Never);
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task SetConfigAsync_RaisesHotkeyChangedEvent()
        {
            var (mgr, _, _, _, _) = Build();
            try
            {
                HotkeyManager.HotkeyChangedEventArgs? captured = null;
                mgr.HotkeyChanged += (s, e) => captured = e;
                var newCfg = new HotkeyConfig(Ctrl: true, Alt: true, Shift: true, Win: false, VirtualKey: 0x42);
                await mgr.SetConfigAsync(newCfg);
                Assert.IsNotNull(captured);
                Assert.AreEqual(newCfg, captured!.NewConfig);
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public async Task InitializeAsync_RequiresWindowHandle()
        {
            var (mgr, _, _, _, _) = Build();
            try
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(
                    async () => await mgr.InitializeAsync(IntPtr.Zero));
            }
            finally { mgr.Dispose(); }
        }

        [TestMethod, TestCategory("Offline")]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var (mgr, _, _, _, _) = Build();
            mgr.Dispose();
            mgr.Dispose();
        }

        [TestMethod, TestCategory("Offline")]
        public async Task Dispose_ThenSetConfigAsync_ThrowsObjectDisposedException()
        {
            var (mgr, _, _, _, _) = Build();
            mgr.Dispose();
            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(
                async () => await mgr.SetConfigAsync(HotkeyConfig.Default));
        }
    }
}
