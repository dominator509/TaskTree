// SPEC-DERIVED-PHASE2B  HALT #8/#9/#10
// SPEC-DERIVED-PHASE2C  Builder integration
// SPEC-DERIVED-PHASE2E  Msg 2 SettingsViewModel ownership (Gap #157 closure path)

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.UI.ViewModels
{
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        private const int MaxTitleLength = 200;
        private readonly ITaskEngine _taskEngine;
        private readonly IClock _clock;
        private readonly IAppLogger _logger;

        [ObservableProperty] private ObservableCollection<TaskNode> tasks = new();
        [ObservableProperty] private string newTaskTitle = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        public TaskBuilderViewModel Builder { get; }
        public SettingsViewModel Settings { get; }

        public MainWindowViewModel(ITaskEngine taskEngine, IClock clock, IAppLogger logger, ISettingsService settingsService)
        {
            _taskEngine = taskEngine ?? throw new ArgumentNullException(nameof(taskEngine));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Builder = new TaskBuilderViewModel(taskEngine, clock, logger);
            Builder.TaskCreated += OnBuilderTaskCreated;
            Settings = new SettingsViewModel(settingsService ?? throw new ArgumentNullException(nameof(settingsService)), logger);
        }

        public async Task InitializeAsync()
        {
            await RefreshAsync();
            await Settings.InitializeAsync();
        }

        private void OnBuilderTaskCreated(object? sender, TaskBuilderViewModel.TaskBuilderCreatedEventArgs e)
        {
            if (!Tasks.Any(t => t.Id == e.Node.Id)) Tasks.Add(e.Node);
            StatusMessage = "Task created";
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                IsBusy = true;
                var tree = await _taskEngine.GetTreeAsync();
                Tasks.Clear();
                if (tree is not null) foreach (var node in tree) if (node is not null) Tasks.Add(node);
                StatusMessage = $"Loaded {Tasks.Count} tasks";
            }
            catch (Exception ex) { _logger.LogError($"RefreshAsync failed: {ex.GetType().Name}: {ex.Message}"); StatusMessage = "Refresh failed - see log"; }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task QuickAddAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) { StatusMessage = "Title required"; return; }
            var trimmed = NewTaskTitle.Trim();
            if (trimmed.Length > MaxTitleLength) { StatusMessage = $"Title too long (max {MaxTitleLength})"; return; }
            try
            {
                IsBusy = true;
                var node = new TaskNode { Id = Guid.NewGuid(), Title = trimmed, Priority = Priority.Normal, Status = TaskStatus.Active, Deadline = null, CreatedAt = _clock.UtcNow, ModifiedAt = _clock.UtcNow };
                await _taskEngine.AddAsync(node);
                Tasks.Add(node);
                NewTaskTitle = string.Empty;
                StatusMessage = "Task added";
            }
            catch (Exception ex) { _logger.LogError($"QuickAddAsync failed: {ex.GetType().Name}: {ex.Message}"); StatusMessage = "Add failed - see log"; }
            finally { IsBusy = false; }
        }
    }
}
