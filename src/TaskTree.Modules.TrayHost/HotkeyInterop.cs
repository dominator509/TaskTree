// ============================================================================
// File: src/TaskTree.Modules.TrayHost/HotkeyInterop.cs
// Module: TaskTree.Modules.TrayHost
// Architecture: §4.1 (RegisterHotKey PInvoke wrapper); §13 default hotkey
// Roadmap: Sub-Phase 1E; consumed by the Phase 2A HotkeyManager and TrayHost
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1E
//   HALT #1  Hybrid A+C — PInvoke signatures and pure modifier logic are
//            retained; Register/Unregister now call the declared Win32 API.
//   HALT #6  public static class — consumable by future Phase 2A HotkeyManager
// PInvoke calls are isolated here so callers can keep their platform boundary
// small and deterministic.
// ============================================================================

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TaskTree.Modules.TrayHost
{
    /// <summary>
    /// Win32 <c>RegisterHotKey</c> / <c>UnregisterHotKey</c> PInvoke wrappers.
    /// <see cref="BuildModifierFlags"/> is pure and deterministic; the wrapper
    /// methods validate arguments and surface native registration failures.
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

        // ---- Win32 wrappers --------------------------------------------------

        /// <summary>
        /// Registers a global hotkey against a message-only or top-level window.
        /// </summary>
        public static void Register(IntPtr hWnd, int id, uint modifiers, uint virtualKey)
        {
            if (hWnd == IntPtr.Zero) throw new ArgumentException("A window handle is required.", nameof(hWnd));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (virtualKey == 0 || virtualKey > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(virtualKey));
            if (!RegisterHotKey(hWnd, id, modifiers, virtualKey))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "RegisterHotKey failed.");
        }

        /// <summary>
        /// Unregisters a previously registered global hotkey.
        /// </summary>
        public static void Unregister(IntPtr hWnd, int id)
        {
            if (hWnd == IntPtr.Zero) throw new ArgumentException("A window handle is required.", nameof(hWnd));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (!UnregisterHotKey(hWnd, id))
            {
                var error = Marshal.GetLastWin32Error();
                const int HotKeyNotRegistered = 1409;
                if (error != HotKeyNotRegistered)
                    throw new Win32Exception(error, "UnregisterHotKey failed.");
            }
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
