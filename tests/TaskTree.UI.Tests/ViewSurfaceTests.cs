// Architecture.md §4.4; Roadmap Phase 2B/2E runtime XAML smoke coverage.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.TestSupport;
using TaskTree.UI.ViewModels;
using TaskTree.UI.Views;

namespace TaskTree.UI.Tests
{
    [TestClass]
    public sealed class ViewSurfaceTests
    {
        [STATestMethod, TestCategory("Offline")]
        public void SettingsAndMainWindow_XamlLoads()
        {
            var engine = new Mock<ITaskEngine>(MockBehavior.Loose);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var settings = new Mock<ISettingsService>(MockBehavior.Loose);
            settings.Setup(s => s.GetAsync()).ReturnsAsync(TaskTreeSettings.Default);
            var viewModel = new MainWindowViewModel(engine.Object, new FakeClock(), logger.Object, settings.Object);

            var settingsView = new SettingsView();
            settingsView.DataContext = viewModel.Settings;
            var mainWindow = new MainWindow(viewModel);

            try
            {
                Assert.AreSame(viewModel, mainWindow.DataContext);
            }
            finally
            {
                mainWindow.Close();
            }
        }
    }
}
