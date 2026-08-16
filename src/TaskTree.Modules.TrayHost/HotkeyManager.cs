// SPEC-DERIVED-PHASE2A HALT #10/#11/#13/#14/#17/#18
// Architecture section 4.1 + 10.5 + 13; Roadmap P2A-AC1/AC2/AC3 (HIGH)
// LOAD-BEARING: 5th IComplianceCore consumer (Gap #107)

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.TrayHost
{
    /// <summary>Persists and registers the configured global hotkey.</summary>
    public sealed class HotkeyManager : IDisposable
    {
        // Shared with TrayHost's message filter; internal keeps the registration
        // identity out of the public contract while preventing drift.
        internal const int HotkeyId = 0x5455;

        /// <summary>Result values used when replacing a global hotkey binding.</summary>
        public enum HotkeyRegistrationResult
        {
            /// <summary>The binding was registered and persisted.</summary>
            Success = 0,
            /// <summary>The operating system rejected the binding because it conflicts with another registration.</summary>
            ConflictDetected = 1,
            /// <summary>The supplied configuration has no modifier or is otherwise invalid.</summary>
            InvalidConfig = 2,
            /// <summary>The manager has not yet been attached to a message-only window.</summary>
            NotInitialized = 3,
        }

        /// <summary>Describes a hotkey configuration change.</summary>
        public sealed class HotkeyChangedEventArgs : EventArgs
        {
            /// <summary>The previous binding.</summary>
            public HotkeyConfig OldConfig { get; init; } = HotkeyConfig.Default;
            /// <summary>The new binding.</summary>
            public HotkeyConfig NewConfig { get; init; } = HotkeyConfig.Default;
        }

        private const string StorageKey = "hotkeys/config";

        private readonly IAppLogger _logger;
        private readonly IComplianceCore _compliance;
        private readonly ISecureStore _secureStore;
        private readonly IClock _clock;
        private readonly SemaphoreSlim _operationGate = new(1, 1);
        private bool _disposed;
        private bool _initialized;
        private bool _hotkeyRegistered;
        private IntPtr _messageOnlyHwnd;

        /// <summary>Raised after a new binding is persisted.</summary>
        public event EventHandler<HotkeyChangedEventArgs>? HotkeyChanged;

        /// <summary>Creates a hotkey manager.</summary>
        public HotkeyManager(IAppLogger logger, IComplianceCore compliance, ISecureStore secureStore, IClock clock)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _secureStore = secureStore ?? throw new ArgumentNullException(nameof(secureStore));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>Registers the current configuration against a message-only window.</summary>
        public async Task InitializeAsync(IntPtr messageOnlyHwnd)
        {
            ThrowIfDisposed();
            if (messageOnlyHwnd == IntPtr.Zero)
                throw new ArgumentException("A message-only window handle is required.", nameof(messageOnlyHwnd));
            await _operationGate.WaitAsync().ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();
                if (_initialized) return;
                var config = await LoadCurrentConfigAsync().ConfigureAwait(false);
                try
                {
                    Register(messageOnlyHwnd, config);
                    _messageOnlyHwnd = messageOnlyHwnd;
                    _hotkeyRegistered = true;
                    _initialized = true;
                }
                catch (Win32Exception ex)
                {
                    _logger.LogWarning("Global hotkey registration failed: {0}", ex.Message);
                    throw;
                }
            }
            finally
            {
                _operationGate.Release();
            }
        }

        /// <summary>Loads the persisted hotkey configuration, or the default when none exists.</summary>
        public async Task<HotkeyConfig> GetCurrentConfigAsync()
        {
            await _operationGate.WaitAsync().ConfigureAwait(false);
            try
            {
                ThrowIfDisposed();
                return await LoadCurrentConfigAsync().ConfigureAwait(false);
            }
            finally
            {
                _operationGate.Release();
            }
        }

        /// <summary>Returns the default Ctrl+Alt+T binding.</summary>
        public Task<HotkeyConfig> GetDefaultConfigAsync()
        {
            ThrowIfDisposed();
            return Task.FromResult(HotkeyConfig.Default);
        }

        /// <summary>Validates, registers, persists, and audits a replacement hotkey binding.</summary>
        public async Task<HotkeyRegistrationResult> SetConfigAsync(HotkeyConfig config)
        {
            ThrowIfDisposed();
            if (config is null) return HotkeyRegistrationResult.InvalidConfig;
            if ((!config.Ctrl && !config.Alt && !config.Shift && !config.Win) ||
                config.VirtualKey is < 1 or > ushort.MaxValue)
                return HotkeyRegistrationResult.InvalidConfig;

            await _operationGate.WaitAsync().ConfigureAwait(false);
            HotkeyChangedEventArgs? changed = null;
            try
            {
                ThrowIfDisposed();
                var oldConfig = await LoadCurrentConfigAsync().ConfigureAwait(false);
                var nativeRegistrationChanged = false;
                if (_initialized)
                {
                    try
                    {
                        HotkeyInterop.Unregister(_messageOnlyHwnd, HotkeyId);
                        Register(_messageOnlyHwnd, config);
                        _hotkeyRegistered = true;
                        nativeRegistrationChanged = true;
                    }
                    catch (Win32Exception ex)
                    {
                        _logger.LogWarning("Global hotkey replacement failed: {0}", ex.Message);
                        try { Register(_messageOnlyHwnd, oldConfig); _hotkeyRegistered = true; } catch { }
                        return HotkeyRegistrationResult.ConflictDetected;
                    }
                }

                try
                {
                    await _secureStore.SaveAsync(StorageKey, config).ConfigureAwait(false);
                }
                catch
                {
                    if (nativeRegistrationChanged)
                        RestoreRegistration(oldConfig);
                    throw;
                }

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

                changed = new HotkeyChangedEventArgs { OldConfig = oldConfig, NewConfig = config };
            }
            finally
            {
                _operationGate.Release();
            }

            HotkeyChanged?.Invoke(this, changed!);
            return HotkeyRegistrationResult.Success;
        }

        private async Task<HotkeyConfig> LoadCurrentConfigAsync()
        {
            var loaded = await _secureStore.LoadAsync<HotkeyConfig>(StorageKey).ConfigureAwait(false);
            return loaded ?? HotkeyConfig.Default;
        }

        private void RestoreRegistration(HotkeyConfig oldConfig)
        {
            try { HotkeyInterop.Unregister(_messageOnlyHwnd, HotkeyId); } catch { }
            try
            {
                Register(_messageOnlyHwnd, oldConfig);
                _hotkeyRegistered = true;
            }
            catch (Exception ex)
            {
                _hotkeyRegistered = false;
                _logger.LogError(ex, "HotkeyManager failed to restore the previous binding: {0}", ex.Message);
            }
        }

        private void ThrowIfDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(HotkeyManager)); }

        private static void Register(IntPtr hwnd, HotkeyConfig config)
            => HotkeyInterop.Register(hwnd, HotkeyId, HotkeyInterop.BuildModifierFlags(config.Ctrl, config.Alt, config.Shift, config.Win), (uint)config.VirtualKey);

        /// <summary>Releases the registered global hotkey.</summary>
        public void Dispose()
        {
            _operationGate.Wait();
            try
            {
                if (_disposed) return;
                if (_hotkeyRegistered)
                {
                    try { HotkeyInterop.Unregister(_messageOnlyHwnd, HotkeyId); }
                    catch (Exception ex) { _logger.LogWarning("Global hotkey unregister failed: {0}", ex.Message); }
                    _hotkeyRegistered = false;
                }
                try { _logger.LogInformation("HotkeyManager disposed."); } catch { }
                _disposed = true;
            }
            finally
            {
                _operationGate.Release();
            }
        }
    }
}
