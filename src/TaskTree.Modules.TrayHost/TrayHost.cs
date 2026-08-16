// ============================================================================
// File: src/TaskTree.Modules.TrayHost/TrayHost.cs
// Module: TaskTree.Modules.TrayHost
// Implements: TaskTree.Core.Abstractions.ITrayHost (Architecture §4.1)
// Default hotkey: Architecture §13 Ctrl+Alt+T
// Audit schema: Architecture §10.5
// Roadmap: Sub-Phase 1E and Phase 5E live wiring
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1E
//   HALT #2  ctor (IAppLogger, IComplianceCore) — 3rd LOAD-BEARING audit injection (Gap #56)
//   HALT #3  _compliance is used for user-intent audit events
//   HALT #5  Internal Raise*() methods for P1E-AC2 satisfaction (InternalsVisibleTo) — Gap #58
//   HALT #7  public sealed class TrayHost : ITrayHost, IDisposable
//   HALT #8  Real idempotent Dispose (no Win32 resources owned in stub state)
//   HALT #9  ShowBalloon validates params before calling the native adapter
//   HALT #10 Initialize is idempotent
// Public API surface remains unchanged.
// See: docs/spec-derivations/PHASE1E-DERIVATIONS.md
// ============================================================================

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using H.NotifyIcon;
using TaskTree.Core.Abstractions;

namespace TaskTree.Modules.TrayHost
{
    /// <summary>
    /// Tray icon, context menu, and global hotkey host.
    /// </summary>
    /// <remarks>
    /// The native resources are created on the WPF dispatcher that owns the
    /// application and are released by <see cref="Dispose"/>.
    /// </remarks>
    public sealed class TrayHost : ITrayHost, IDisposable
    {
        private readonly IAppLogger _logger;

        private readonly IComplianceCore _compliance;
        private readonly HotkeyManager? _hotkeyManager;
        private TaskbarIcon? _taskbarIcon;
        private HwndSource? _hotkeyWindow;
        private bool _hotkeyRegistered;
        private bool _trayOwnsHotkeyRegistration;
        private bool _initialized;
        private bool _disposed;

        private const int WmHotkey = 0x0312;
        private static readonly IntPtr HwndMessage = new(-3);

        /// <summary>
        /// Creates a new TrayHost. All dependencies required; nulls throw
        /// <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <remarks>
        /// LOAD-BEARING for Phase 1F: <c>IComplianceCore</c> must be registered
        /// and injected. Cross-Phase Gap #56 (third audit-injection flag after
        /// TaskEngine R6 and ReminderScheduler HALT-Msg2 #2). Phase 5E will use
        /// <c>_compliance.AuditAsync</c> on every Show/Add/Exit event (Gap #57).
        /// </remarks>
        public TrayHost(IAppLogger logger, IComplianceCore compliance)
            : this(logger, compliance, hotkeyManager: null)
        {
        }

        /// <summary>
        /// Creates a tray host with the persisted hotkey manager used by the
        /// application composition root. The legacy two-argument constructor
        /// remains available for isolated callers and tests.
        /// </summary>
        public TrayHost(IAppLogger logger, IComplianceCore compliance, HotkeyManager? hotkeyManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
            _hotkeyManager = hotkeyManager;
        }

        /// <inheritdoc />
        public event EventHandler? ShowTreeRequested;

        /// <inheritdoc />
        public event EventHandler? AddTaskRequested;

        /// <inheritdoc />
        public event EventHandler? ExitRequested;

        /// <inheritdoc />
        public void Initialize()
        {
            ThrowIfDisposed();
            if (_initialized) return;
            if (Application.Current is null)
                throw new InvalidOperationException("TrayHost requires a WPF Application context.");

            try
            {
                _taskbarIcon = new TaskbarIcon
                {
                    ToolTipText = "TaskTree",
                    IconSource = new GeneratedIconSource
                    {
                        Text = "T",
                        Foreground = Brushes.White,
                        Background = Brushes.DarkSlateGray,
                        FontWeight = FontWeights.Bold,
                    },
                };
                _taskbarIcon.ContextMenu = CreateContextMenu();
                _taskbarIcon.TrayLeftMouseDoubleClick += OnTrayShowTree;
                _taskbarIcon.TrayBalloonTipClicked += OnTrayBalloonClicked;
                _taskbarIcon.ForceCreate();

                var sourceParameters = new HwndSourceParameters("TaskTree.Hotkey")
                {
                    ParentWindow = HwndMessage,
                    WindowStyle = 0,
                };
                _hotkeyWindow = new HwndSource(sourceParameters);
                _hotkeyWindow.AddHook(WindowMessageFilter);
                if (_hotkeyManager is null)
                {
                    HotkeyInterop.Register(
                        _hotkeyWindow.Handle,
                        HotkeyManager.HotkeyId,
                        HotkeyInterop.BuildModifierFlags(ctrl: true, alt: true, shift: false, win: false),
                        0x54);
                    _trayOwnsHotkeyRegistration = true;
                }
                else
                {
                    _hotkeyManager.InitializeAsync(_hotkeyWindow.Handle).GetAwaiter().GetResult();
                    _trayOwnsHotkeyRegistration = false;
                }
                _hotkeyRegistered = true;
                _initialized = true;
                RecordAudit("TrayInitialized");
            }
            catch
            {
                DisposeNativeResources();
                throw;
            }
        }

        /// <inheritdoc />
        public void ShowBalloon(string title, string message)
        {
            ThrowIfDisposed();
            if (title is null) throw new ArgumentNullException(nameof(title));
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty or whitespace.", nameof(title));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty or whitespace.", nameof(message));
            if (!_initialized || _taskbarIcon is null)
                throw new InvalidOperationException("TrayHost must be initialized before showing a balloon.");

            var taskbarIcon = _taskbarIcon;
            void ShowOnDispatcher()
            {
                taskbarIcon.ShowNotification(
                    title,
                    message,
                    H.NotifyIcon.Core.NotificationIcon.Info,
                    largeIcon: true,
                    sound: false,
                    respectQuietTime: true,
                    realtime: true,
                    timeout: TimeSpan.FromSeconds(8));
                RecordAudit("TrayBalloonShown");
            }

            if (taskbarIcon.Dispatcher.CheckAccess()) ShowOnDispatcher();
            else taskbarIcon.Dispatcher.Invoke(ShowOnDispatcher);
        }

        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            var showTree = new MenuItem { Header = "Open Task Tree" };
            showTree.Click += OnTrayShowTree;
            var addTask = new MenuItem { Header = "Add Task" };
            addTask.Click += OnTrayAddTask;
            var exit = new MenuItem { Header = "Exit" };
            exit.Click += OnTrayExit;
            menu.Items.Add(showTree);
            menu.Items.Add(addTask);
            menu.Items.Add(new Separator());
            menu.Items.Add(exit);
            return menu;
        }

        private void OnTrayShowTree(object? sender, EventArgs e)
        {
            RaiseShowTreeRequested();
            RecordAudit("ShowTreeRequested");
        }

        private void OnTrayAddTask(object? sender, RoutedEventArgs e)
        {
            RaiseAddTaskRequested();
            RecordAudit("AddTaskRequested");
        }

        private void OnTrayExit(object? sender, RoutedEventArgs e)
        {
            RaiseExitRequested();
            RecordAudit("ExitRequested");
        }

        private void OnTrayBalloonClicked(object? sender, RoutedEventArgs e)
        {
            RaiseShowTreeRequested();
            RecordAudit("TrayBalloonClicked");
        }

        private IntPtr WindowMessageFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmHotkey && wParam.ToInt32() == HotkeyManager.HotkeyId)
            {
                RaiseShowTreeRequested();
                RecordAudit("GlobalHotkeyPressed");
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void RecordAudit(string action)
        {
            _ = RecordAuditAsync(action);
        }

        private async System.Threading.Tasks.Task RecordAuditAsync(string action)
        {
            try
            {
                await _compliance.AuditAsync(new TaskTree.Core.Models.AuditEntry
                {
                    Module = "TrayHost",
                    Action = action,
                    Result = "success",
                    Timestamp = DateTimeOffset.UtcNow,
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TrayHost audit failed: {0}", ex.Message);
            }
        }

        // --------------------------------------------------------------------
        // Test-only manual event raise (HALT #5 / Gap #58)
        // Accessible only to TaskTree.Modules.TrayHost.Tests via InternalsVisibleTo
        // (see Properties/AssemblyInfo.cs). Production callers MUST NOT use these.
        // --------------------------------------------------------------------

        /// <summary>Test-only raise of <see cref="ShowTreeRequested"/>.</summary>
        internal void RaiseShowTreeRequested() => ShowTreeRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>Test-only raise of <see cref="AddTaskRequested"/>.</summary>
        internal void RaiseAddTaskRequested() => AddTaskRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>Test-only raise of <see cref="ExitRequested"/>.</summary>
        internal void RaiseExitRequested() => ExitRequested?.Invoke(this, EventArgs.Empty);

        // --------------------------------------------------------------------
        // IDisposable (HALT #8 — real, idempotent)
        // --------------------------------------------------------------------

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TrayHost));
        }

        /// <summary>
        /// Idempotent disposal of the tray icon, message-only window, and hotkey.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            DisposeNativeResources();
            try { _logger.LogInformation("TrayHost disposed."); }
            catch { }
            _disposed = true;
        }

        private void DisposeNativeResources()
        {
            if (_hotkeyManager is not null)
            {
                try { _hotkeyManager.Dispose(); }
                catch (Exception ex) { _logger.LogWarning("Managed hotkey disposal failed: {0}", ex.Message); }
            }
            if (_hotkeyRegistered && _trayOwnsHotkeyRegistration && _hotkeyWindow is not null)
            {
                try { HotkeyInterop.Unregister(_hotkeyWindow.Handle, HotkeyManager.HotkeyId); }
                catch (Exception ex) { _logger.LogWarning("Hotkey unregister failed: {0}", ex.Message); }
            }
            _hotkeyRegistered = false;
            _trayOwnsHotkeyRegistration = false;
            if (_hotkeyWindow is not null)
            {
                _hotkeyWindow.RemoveHook(WindowMessageFilter);
                _hotkeyWindow.Dispose();
                _hotkeyWindow = null;
            }
            if (_taskbarIcon is not null)
            {
                _taskbarIcon.TrayLeftMouseDoubleClick -= OnTrayShowTree;
                _taskbarIcon.TrayBalloonTipClicked -= OnTrayBalloonClicked;
                _taskbarIcon.Dispose();
                _taskbarIcon = null;
            }
            _initialized = false;
        }
    }
}
