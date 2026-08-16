// Architecture.md §2, §9.1, §9.2, §10.10; Roadmap Phase 2E.

using System.Windows.Controls;

namespace TaskTree.UI.Views
{
    /// <summary>Reusable settings surface bound to <see cref="ViewModels.SettingsViewModel" />.</summary>
    public partial class SettingsView : UserControl
    {
        /// <summary>Initializes the settings view.</summary>
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
