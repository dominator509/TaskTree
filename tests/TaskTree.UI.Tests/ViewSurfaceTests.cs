// Architecture.md §4.4; Roadmap Phase 2B/2E runtime XAML smoke coverage.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TaskTree.Core.Enums;
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

        [STATestMethod, TestCategory("Offline")]
        public void MainWindow_ThemePreference_AppliesRuntimeResourceDictionary()
        {
            var applicationWasCreated = Application.Current is null;
            var application = Application.Current ?? new Application();
            var engine = new Mock<ITaskEngine>(MockBehavior.Loose);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var settings = new Mock<ISettingsService>(MockBehavior.Loose);
            settings.Setup(s => s.GetAsync()).ReturnsAsync(TaskTreeSettings.Default);
            var viewModel = new MainWindowViewModel(engine.Object, new FakeClock(), logger.Object, settings.Object);
            var mainWindow = new MainWindow(viewModel);

            try
            {
                viewModel.Settings.ThemePreference = ThemePreference.Dark;
                var darkBrush = (SolidColorBrush)application.Resources["TaskTreeWindowBackgroundBrush"];
                Assert.AreEqual(Color.FromRgb(0x1E, 0x1E, 0x1E), darkBrush.Color);

                Task.Run(() => viewModel.Settings.ThemePreference = ThemePreference.Light).GetAwaiter().GetResult();
                mainWindow.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { }));
                var lightBrush = (SolidColorBrush)application.Resources["TaskTreeWindowBackgroundBrush"];
                Assert.AreEqual(Color.FromRgb(0xF7, 0xF7, 0xF7), lightBrush.Color);
            }
            finally
            {
                mainWindow.Close();
                if (applicationWasCreated) application.Shutdown();
            }
        }
    }
}
