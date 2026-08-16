// SPEC-DERIVED-PHASE2B  HALT #7 (minimal code-behind per MVVM)
using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;
using System.Windows;
using TaskTree.Core.Enums;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            viewModel.Settings.PropertyChanged += OnSettingsPropertyChanged;
            Closed += OnClosed;
            ApplyTheme(viewModel.Settings.ThemePreference);
        }

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.ThemePreference) && sender is SettingsViewModel settings)
            {
                var preference = settings.ThemePreference;
                if (Dispatcher.CheckAccess()) ApplyTheme(preference);
                else Dispatcher.BeginInvoke(new Action(() => ApplyTheme(preference)));
            }
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.Settings.PropertyChanged -= OnSettingsPropertyChanged;
            Closed -= OnClosed;
        }

        private static void ApplyTheme(ThemePreference preference)
        {
            var application = Application.Current;
            if (application is null) return;

            var useDarkTheme = preference == ThemePreference.Dark ||
                preference == ThemePreference.System && IsSystemDarkTheme();
            var themeName = useDarkTheme
                ? "DarkThemeResources.xaml"
                : "ThemeResources.xaml";
            var themeUri = "/TaskTree.UI;component/Themes/" + themeName;

            foreach (var dictionary in application.Resources.MergedDictionaries
                         .Where(IsTaskTreeThemeDictionary)
                         .ToList())
            {
                application.Resources.MergedDictionaries.Remove(dictionary);
            }

            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative),
            });
        }

        private static bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsTaskTreeThemeDictionary(ResourceDictionary dictionary)
            => dictionary.Source?.OriginalString.Contains("/TaskTree.UI;component/Themes/", StringComparison.OrdinalIgnoreCase) == true;
    }
}
