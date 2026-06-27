// ============================================================================
// File: src/TaskTree.Modules.TrayHost/HotkeyInterop.cs
// Module: TaskTree.Modules.TrayHost
// Architecture: §4.1 (RegisterHotKey PInvoke wrapper); §13 default hotkey
// Roadmap: Sub-Phase 1E (HIGH-stub); future consumer Sub-Phase 2A HotkeyManager
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1E
//   HALT #1  Hybrid A+C — PInvoke sigs present, Register/Unregister stubbed,
//            BuildModifierFlags implemented (pure logic, no Win32 call)
//   HALT #6  public static class — consumable by future Phase 2A HotkeyManager
// PInvoke declarations compile but only execute on call; safe in chat-only env.
// Codex Phase 5E replaces Register/Unregister bodies with live PInvoke calls.
// ============================================================================

using System;
using System.Runtime.InteropServices;

namespace TaskTree.Modules.TrayHost
{
    /// <summary>
    /// Win32 <c>RegisterHotKey</c> / <c>UnregisterHotKey</c> PInvoke wrappers.
    /// Phase 1E HIGH-stub — PInvoke declarations are present and
    /// <see cref="BuildModifierFlags"/> is implemented; <see cref="Register"/>
    /// and <see cref="Unregister"/> throw <see cref="NotImplementedException"/>
    /// pending Codex Phase 5E live message loop.
    /// </summary>
    public static class HotkeyInterop
    {
        // ---- Win32 modifier flags (per RegisterHotKey docs) -----------------

        /// <summary>MOD_ALT flag (0x0001).</summary>
        public const uint ModAlt = 0x0001;

        /// <summary>MOD_CONTROL flag (0x0002).</summary>
        public const uint ModControl = 0x0002;

        /// <summary>MOD_SHIFT flag (0x0004).</summary>
        public const uint ModShift = 0x0004;

        /// <summary>MOD_WIN flag (0x0008).</summary>
        public const uint ModWin = 0x0008;

        /// <summary>MOD_NOREPEAT flag (0x4000) — Win10+ single-fire per press.</summary>
        public const uint ModNoRepeat = 0x4000;

        // ---- PInvoke signatures (HALT #1) -----------------------------------

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // ---- Wrappers (HIGH-stubbed per Roadmap) ----------------------------

        /// <summary>
        /// Registers a global hotkey. HIGH-stub — throws until Codex Phase 5E
        /// provides a live message-only window HWND and replaces this body
        /// with a real <c>RegisterHotKey</c> PInvoke call.
        /// </summary>
        public static void Register(IntPtr hWnd, int id, uint modifiers, uint virtualKey)
        {
            throw new NotImplementedException(
                "HIGH: RegisterHotKey PInvoke requires live message loop — Codex Phase 5E");
        }

        /// <summary>
        /// Unregisters a previously registered global hotkey. HIGH-stub.
        /// </summary>
        public static void Unregister(IntPtr hWnd, int id)
        {
            throw new NotImplementedException(
                "HIGH: UnregisterHotKey PInvoke requires live message loop — Codex Phase 5E");
        }

        // ---- Pure-logic helpers (implemented now) ---------------------------

        /// <summary>
        /// Combines modifier booleans into a Win32 <c>RegisterHotKey</c>
        /// <c>fsModifiers</c> flag value. <see cref="ModNoRepeat"/> is always
        /// set per Architecture §4.1 modern hotkey behavior on Windows 10+
        /// (single-fire per press; Architecture §11 min-build = Win10 1809).
        /// </summary>
        /// <param name="ctrl">Include Ctrl modifier.</param>
        /// <param name="alt">Include Alt modifier.</param>
        /// <param name="shift">Include Shift modifier.</param>
        /// <param name="win">Include Win modifier.</param>
        /// <returns>fsModifiers flag value suitable for <c>RegisterHotKey</c>.</returns>
        public static uint BuildModifierFlags(bool ctrl, bool alt, bool shift, bool win)
        {
            uint flags = ModNoRepeat;
            if (ctrl) flags |= ModControl;
            if (alt) flags |= ModAlt;
            if (shift) flags |= ModShift;
            if (win) flags |= ModWin;
            return flags;
        }
    }
}
