// SPEC-DERIVED-PHASE2E  HALT #9/#10/#11/#12/#13/#14/#15/#16
// SettingsService persists non-PHI settings via ISecureStore and audits save/reset events.

using System;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.Settings
{
    public sealed class SettingsService : ISettingsService
    {
        private const string StorageKey = "settings/app";
        private readonly ISecureStore _secureStore;
        private readonly IComplianceCore _compliance;
        private readonly IClock _clock;
        private readonly IAppLogger _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public event EventHandler? SettingsChanged;

        public SettingsService(ISecureStore secureStore, IComplianceCore compliance, IClock clock, IAppLogger logger)
        {
            _secureStore = secureStore ?? throw new ArgumentNullException(nameof(secureStore));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TaskTreeSettings> GetAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var settings = await _secureStore.LoadAsync<TaskTreeSettings>(StorageKey).ConfigureAwait(false);
                if (settings is null)
                    return TaskTreeSettings.Default;

                Validate(settings);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettingsService.GetAsync failed; returning defaults: {0}: {1}", ex.GetType().Name, ex.Message);
                return TaskTreeSettings.Default;
            }
            finally { _gate.Release(); }
        }

        public async Task SaveAsync(TaskTreeSettings settings)
        {
            Validate(settings);
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                await _secureStore.SaveAsync(StorageKey, settings).ConfigureAwait(false);
                await _compliance.AuditAsync(new AuditEntry
                {
                    Module = "SettingsService",
                    Action = "SettingsSaved",
                    Result = "success",
                    Timestamp = _clock.UtcNow,
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SettingsService.SaveAsync failed: {0}: {1}", ex.GetType().Name, ex.Message);
                throw;
            }
            finally { _gate.Release(); }

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task ResetAsync()
        {
            var defaults = TaskTreeSettings.Default;
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                await _secureStore.SaveAsync(StorageKey, defaults).ConfigureAwait(false);
                await _compliance.AuditAsync(new AuditEntry
                {
                    Module = "SettingsService",
                    Action = "SettingsReset",
                    Result = "success",
                    Timestamp = _clock.UtcNow,
                }).ConfigureAwait(false);
            }
            finally { _gate.Release(); }

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void Validate(TaskTreeSettings? settings)
        {
            if (settings is null) throw new ArgumentNullException(nameof(settings));
            if (!Enum.IsDefined(typeof(ThemePreference), settings.ThemePreference))
                throw new ArgumentException("Invalid theme preference.", nameof(settings));
            if (settings.ReminderSnoozeMinutes < 1)
                throw new ArgumentOutOfRangeException(nameof(settings.ReminderSnoozeMinutes), "Reminder snooze must be >= 1 minute.");
            if (settings.ReminderSnoozeMinutes > 240)
                throw new ArgumentOutOfRangeException(nameof(settings.ReminderSnoozeMinutes), "Reminder snooze must be <= 240 minutes.");
        }
    }
}
