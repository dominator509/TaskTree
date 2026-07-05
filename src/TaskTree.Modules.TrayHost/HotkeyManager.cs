// SPEC-DERIVED-PHASE2A HALT #10/#11/#13/#14/#17/#18
// Architecture section 4.1 + 10.5 + 13; Roadmap P2A-AC1/AC2/AC3 (HIGH)
// LOAD-BEARING: 5th IComplianceCore consumer (Gap #107)

using System;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.TrayHost
{
    public sealed class HotkeyManager : IDisposable
    {
        public enum HotkeyRegistrationResult { Success = 0, ConflictDetected = 1, InvalidConfig = 2, NotInitialized = 3 }

        public sealed class HotkeyChangedEventArgs : EventArgs
        {
            public HotkeyConfig OldConfig { get; init; } = HotkeyConfig.Default;
            public HotkeyConfig NewConfig { get; init; } = HotkeyConfig.Default;
        }

        private const string StorageKey = "hotkeys/config";

        private readonly IAppLogger _logger;
        private readonly IComplianceCore _compliance;
        private readonly ISecureStore _secureStore;
        private readonly IClock _clock;
        private bool _disposed;
        private bool _initialized;

        public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;

        public HotkeyManager(IAppLogger logger, IComplianceCore compliance, ISecureStore secureStore, IClock clock)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _secureStore = secureStore ?? throw new ArgumentNullException(nameof(secureStore));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>HIGH-stub per Roadmap section 2A. Codex Phase 5E wires real RegisterHotKey.</summary>
        public Task InitializeAsync(IntPtr messageOnlyHwnd)
        {
            ThrowIfDisposed();
            throw new NotImplementedException("HIGH: RegisterHotKey + message-only HWND require live env - Codex Phase 5E");
        }

        public async Task<HotkeyConfig> GetCurrentConfigAsync()
        {
            ThrowIfDisposed();
            var loaded = await _secureStore.LoadAsync<HotkeyConfig>(StorageKey).ConfigureAwait(false);
            return loaded ?? HotkeyConfig.Default;
        }

        public Task<HotkeyConfig> GetDefaultConfigAsync()
        {
            ThrowIfDisposed();
            return Task.FromResult(HotkeyConfig.Default);
        }

        public async Task<HotkeyRegistrationResult> SetConfigAsync(HotkeyConfig config)
        {
            ThrowIfDisposed();
            if (config is null) return HotkeyRegistrationResult.InvalidConfig;
            if (!config.Ctrl && !config.Alt && !config.Shift && !config.Win)
                return HotkeyRegistrationResult.InvalidConfig;

            var oldConfig = await GetCurrentConfigAsync().ConfigureAwait(false);
            await _secureStore.SaveAsync(StorageKey, config).ConfigureAwait(false);

            try
            {
                await _compliance.AuditAsync(new AuditEntry
                {
                    Module = "HotkeyManager",
                    Action = "HotkeyConfigChanged",
                    Result = "success",
                    Timestamp = _clock.UtcNow,
                }).ConfigureAwait(false);
            }
            catch (Exception ex) { _logger.LogError(ex, "HotkeyManager audit failed: {0}", ex.Message); }

            HotkeyChanged?.Invoke(this, new HotkeyChangedEventArgs { OldConfig = oldConfig, NewConfig = config });
            return HotkeyRegistrationResult.Success;
        }

        private void ThrowIfDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(HotkeyManager)); }

        public void Dispose()
        {
            if (_disposed) return;
            try { _logger.LogInformation("HotkeyManager disposed."); } catch { }
            _disposed = true;
        }
    }
}
