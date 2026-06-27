// SPEC-DERIVED-PHASE2B  HALT #7 (minimal code-behind per MVVM)
using System;
using System.Windows;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
