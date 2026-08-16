// SPEC-DERIVED-PHASE2E  Msg 2 Settings UI ViewModel
// Gap #157 closure path: Settings UI surface emitted.
// Gap #158/#159 closure: MainWindow applies the persisted theme preference at runtime.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.UI.ViewModels
{
    public sealed partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IAppLogger _logger;

        [ObservableProperty] private ThemePreference themePreference = ThemePreference.System;
        [ObservableProperty] private bool startWithWindows;
        [ObservableProperty] private bool minimizeToTrayOnClose = true;
        [ObservableProperty] private bool enableReminderSounds;
        [ObservableProperty] private int reminderSnoozeMinutes = 10;
        [ObservableProperty] private bool showCompletedTasks;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        public SettingsViewModel(ISettingsService settingsService, IAppLogger logger)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                Apply(await _settingsService.GetAsync().ConfigureAwait(false));
                StatusMessage = "Settings loaded";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettingsViewModel.LoadAsync failed: {0}: {1}", ex.GetType().Name, ex.Message);
                StatusMessage = "Settings load failed - see log";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;
                await _settingsService.SaveAsync(Build()).ConfigureAwait(false);
                StatusMessage = "Settings saved";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettingsViewModel.SaveAsync failed: {0}: {1}", ex.GetType().Name, ex.Message);
                StatusMessage = "Settings save failed - see log";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ResetAsync()
        {
            try
            {
                IsBusy = true;
                await _settingsService.ResetAsync().ConfigureAwait(false);
                Apply(TaskTreeSettings.Default);
                StatusMessage = "Settings reset";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettingsViewModel.ResetAsync failed: {0}: {1}", ex.GetType().Name, ex.Message);
                StatusMessage = "Settings reset failed - see log";
            }
            finally { IsBusy = false; }
        }

        private TaskTreeSettings Build() => new(
            ThemePreference,
            StartWithWindows,
            MinimizeToTrayOnClose,
            EnableReminderSounds,
            ReminderSnoozeMinutes,
            ShowCompletedTasks);

        private void Apply(TaskTreeSettings settings)
        {
            ThemePreference = settings.ThemePreference;
            StartWithWindows = settings.StartWithWindows;
            MinimizeToTrayOnClose = settings.MinimizeToTrayOnClose;
            EnableReminderSounds = settings.EnableReminderSounds;
            ReminderSnoozeMinutes = settings.ReminderSnoozeMinutes;
            ShowCompletedTasks = settings.ShowCompletedTasks;
        }
    }
}
