// SPEC-DERIVED-PHASE2D  HALT #4/#5/#6
// Gap #143: Keep these RGB values synchronized with ThemeResources.xaml until Phase 5C resource lookup/converter decision.
// Gap #144: Palette values may be revised by design review later.

using System.Windows.Media;
using TaskTree.Core.Enums;

namespace TaskTree.UI.Themes
{
    /// <summary>Central priority-to-brush mapping for WPF UI components.</summary>
    public static class PriorityBrushProvider
    {
        public static readonly SolidColorBrush CriticalBrush = Frozen(0xD1, 0x34, 0x38);
        public static readonly SolidColorBrush HighBrush = Frozen(0xFF, 0x8C, 0x00);
        public static readonly SolidColorBrush NormalBrush = Frozen(0xFF, 0xD7, 0x00);
        public static readonly SolidColorBrush LowBrush = Frozen(0x4F, 0x9D, 0xE8);
        public static readonly SolidColorBrush TrivialBrush = Frozen(0x8A, 0x88, 0x86);

        public static Brush GetBrush(Priority priority) => priority switch
        {
            Priority.Critical => CriticalBrush,
            Priority.High => HighBrush,
            Priority.Normal => NormalBrush,
            Priority.Low => LowBrush,
            Priority.Trivial => TrivialBrush,
            _ => TrivialBrush,
        };

        private static SolidColorBrush Frozen(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}
