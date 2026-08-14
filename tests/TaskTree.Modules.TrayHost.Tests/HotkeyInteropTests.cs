// ============================================================================
// File: tests/TaskTree.Modules.TrayHost.Tests/HotkeyInteropTests.cs
// Covers: Architecture §4.1 (PInvoke), §13 default Ctrl+Alt+T; Roadmap P1E-AC3
// SPEC-DERIVED-PHASE1E-MSG2  HALT-Msg2 #5/#10/#11
// Test count: 6. Gap #61.
// ============================================================================

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskTree.Modules.TrayHost.Tests
{
    [TestClass]
    public class HotkeyInteropTests
    {
        [DataTestMethod, TestCategory("Offline")]
        [DataRow(false, false, false, false, 0x4000u)]
        [DataRow(true,  false, false, false, 0x4002u)]
        [DataRow(false, true,  false, false, 0x4001u)]
        [DataRow(false, false, true,  false, 0x4004u)]
        [DataRow(false, false, false, true,  0x4008u)]
        [DataRow(true,  true,  false, false, 0x4003u)]
        [DataRow(true,  false, true,  true,  0x400Eu)]
        [DataRow(true,  true,  true,  true,  0x400Fu)]
        public void BuildModifierFlags_MatrixCoverage_ReturnsCorrectFlags(
            bool ctrl, bool alt, bool shift, bool win, uint expected)
        {
            Assert.AreEqual(expected, HotkeyInterop.BuildModifierFlags(ctrl, alt, shift, win));
        }

        [TestMethod, TestCategory("Offline")]
        public void ModifierConstants_HaveCorrectWin32Values()
        {
            Assert.AreEqual(0x0001u, HotkeyInterop.ModAlt);
            Assert.AreEqual(0x0002u, HotkeyInterop.ModControl);
            Assert.AreEqual(0x0004u, HotkeyInterop.ModShift);
            Assert.AreEqual(0x0008u, HotkeyInterop.ModWin);
            Assert.AreEqual(0x4000u, HotkeyInterop.ModNoRepeat);
        }

        [TestMethod, TestCategory("Offline")]
        public void BuildModifierFlags_AlwaysSetsNoRepeat()
        {
            for (int i = 0; i < 16; i++)
            {
                bool ctrl  = (i & 1) != 0;
                bool alt   = (i & 2) != 0;
                bool shift = (i & 4) != 0;
                bool win   = (i & 8) != 0;
                var result = HotkeyInterop.BuildModifierFlags(ctrl, alt, shift, win);
                Assert.AreEqual(HotkeyInterop.ModNoRepeat, result & HotkeyInterop.ModNoRepeat);
            }
        }

        [TestMethod, TestCategory("Offline")]
        public void BuildModifierFlags_DefaultHotkey_CtrlAltOnly_Returns0x4003()
        {
            Assert.AreEqual(
                0x4003u,
                HotkeyInterop.BuildModifierFlags(ctrl: true, alt: true, shift: false, win: false));
        }

        [TestMethod, TestCategory("Offline")]
        public void Register_RequiresWindowHandle()
        {
            Assert.ThrowsException<ArgumentException>(
                () => HotkeyInterop.Register(IntPtr.Zero, 1, 0u, 0x54));
        }

        [TestMethod, TestCategory("Offline")]
        public void Unregister_RequiresWindowHandle()
        {
            Assert.ThrowsException<ArgumentException>(
                () => HotkeyInterop.Unregister(IntPtr.Zero, 1));
        }
    }
}
