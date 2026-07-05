// SPEC-DERIVED-PHASE2C  HALT #9/#10/#11/#12/#13/#14/#16
// Roadmap P2C-AC1/AC2/AC3. Minimal PHI-like heuristic only; see Gap #129.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.UI.ViewModels
{
    public sealed partial class TaskBuilderViewModel : ObservableObject
    {
        private const int MaxTitleLength = 200;
        private const int MaxPatientTextLength = 160;
        private const int MaxLabHintLength = 120;
        private const int MaxDeliveryHintLength = 120;
        private static readonly Regex SevenDigits = new(@"\d{7,}", RegexOptions.Compiled);
        private static readonly Regex DateLike = new(@"\d{1,2}/\d{1,2}(/\d{2,4})?", RegexOptions.Compiled);

        private readonly ITaskEngine _taskEngine;
        private readonly IClock _clock;
        private readonly IAppLogger _logger;

        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private Priority priority = Priority.Normal;
        [ObservableProperty] private DateTimeOffset? deadline;
        [ObservableProperty] private string patientText = string.Empty;
        [ObservableProperty] private string labHint = string.Empty;
        [ObservableProperty] private string deliveryHint = string.Empty;
        [ObservableProperty] private bool requiresLabReview;
        [ObservableProperty] private bool requiresDeliveryCoordination;
        [ObservableProperty] private DateTimeOffset? labDueAtUtc;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        public event EventHandler<TaskBuilderCreatedEventArgs>? TaskCreated;

        public TaskBuilderViewModel(ITaskEngine taskEngine, IClock clock, IAppLogger logger)
        {
            _taskEngine = taskEngine ?? throw new ArgumentNullException(nameof(taskEngine));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        private async Task CreateTaskAsync()
        {
            if (!Validate()) return;
            try
            {
                IsBusy = true;
                var metadata = BuildMetadata();
                var node = new TaskNode
                {
                    Id = Guid.NewGuid(),
                    Title = Title.Trim(),
                    Priority = Priority,
                    Status = TaskStatus.Active,
                    Deadline = Deadline,
                    CreatedAt = _clock.UtcNow,
                    ModifiedAt = _clock.UtcNow,
                    Metadata = metadata,
                };
                await _taskEngine.AddAsync(node);
                TaskCreated?.Invoke(this, new TaskBuilderCreatedEventArgs(node, metadata));
                ResetFields();
                StatusMessage = "Task created";
            }
            catch (Exception ex)
            {
                _logger.LogError($"TaskBuilder create failed: {ex.GetType().Name}: {ex.Message}");
                StatusMessage = "Create failed - see log";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private void ResetForm() => ResetFields();

        private TaskMetadata BuildMetadata() => new(
            PatientText.Trim(), LabHint.Trim(), DeliveryHint.Trim(),
            RequiresLabReview, RequiresDeliveryCoordination, LabDueAtUtc);

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Title)) { StatusMessage = "Title required"; return false; }
            if (Title.Trim().Length > MaxTitleLength) { StatusMessage = "Title too long (max 200)"; return false; }
            if (PatientText.Length > MaxPatientTextLength) { StatusMessage = "Patient text too long (max 160)"; return false; }
            if (LabHint.Length > MaxLabHintLength) { StatusMessage = "Lab hint too long (max 120)"; return false; }
            if (DeliveryHint.Length > MaxDeliveryHintLength) { StatusMessage = "Delivery hint too long (max 120)"; return false; }
            if (ContainsPhiLike(PatientText) || ContainsPhiLike(LabHint) || ContainsPhiLike(DeliveryHint))
            { StatusMessage = "Metadata contains disallowed PHI-like pattern"; return false; }
            if (RequiresLabReview && LabDueAtUtc is null)
                StatusMessage = "Lab review flagged without due time";
            return true;
        }

        private static bool ContainsPhiLike(string value)
            => !string.IsNullOrEmpty(value) && (value.Contains('@') || SevenDigits.IsMatch(value) || DateLike.IsMatch(value));

        private void ResetFields()
        {
            Title = string.Empty; Priority = Priority.Normal; Deadline = null;
            PatientText = string.Empty; LabHint = string.Empty; DeliveryHint = string.Empty;
            RequiresLabReview = false; RequiresDeliveryCoordination = false; LabDueAtUtc = null;
        }

        public sealed class TaskBuilderCreatedEventArgs : EventArgs
        {
            public TaskBuilderCreatedEventArgs(TaskNode node, TaskMetadata metadata)
            { Node = node ?? throw new ArgumentNullException(nameof(node)); Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata)); }
            public TaskNode Node { get; }
            public TaskMetadata Metadata { get; }
        }
    }
}
