// SPEC-DERIVED-PHASE2A HALT #12/#15 (closes Phase 1E Gap #11)
namespace TaskTree.Modules.TrayHost
{
    /// <summary>Persistent hotkey binding (record for value semantics + JSON round-trip).</summary>
    public sealed record HotkeyConfig(bool Ctrl, bool Alt, bool Shift, bool Win, int VirtualKey)
    {
        /// <summary>Architecture section 13 default: Ctrl+Alt+T (VK_T = 0x54).</summary>
        public static HotkeyConfig Default => new(Ctrl: true, Alt: true, Shift: false, Win: false, VirtualKey: 0x54);
    }
}
