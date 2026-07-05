// SPEC-DERIVED-PHASE2B  HALT #11/#12 (Gap #80 closure - replaces Phase 1G Tier 2 WPF window)

using System;
using System.Windows;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Views
{
    public partial class ReminderToast : Window
    {
        public ReminderToast(ToastViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            ShowActivated = false;
            Loaded += (s, e) =>
            {
                var workingArea = SystemParameters.WorkArea;
                Left = workingArea.Right - Width - 20;
                Top = workingArea.Bottom - Height - 20;
            };
        }
    }
}
