// SPEC-DERIVED-PHASE1F
// SPEC-DERIVED-PHASE1G-MSG2
// SPEC-DERIVED-PHASE2B
// SPEC-DERIVED-PHASE2E
// SPEC-DERIVED-PHASE2F  HALT #11/#12/#13 (session lock wiring)

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.UI.ViewModels;
using TaskTree.UI.Views;

namespace TaskTree.Orchestrator
{
    public sealed class Orchestrator : IOrchestrator, IDisposable
    {
        private readonly ITaskEngine _taskEngine; private readonly IReminderScheduler _reminderScheduler; private readonly IComplianceCore _compliance; private readonly ITrayHost _trayHost; private readonly IReminderDeliveryService _reminderDeliveryService; private readonly ISettingsService _settingsService; private readonly ISessionLockService _sessionLock; private readonly IAppLogger _logger; private readonly IClock _clock; private readonly IAutoUpdater? _autoUpdater; private readonly IBugReporter? _bugReporter; private readonly TimeSpan _updatePollInterval; private readonly string _auditIncidentRoot;
        private readonly SemaphoreSlim _gate = new(1,1); private EventHandler? _showTreeHandler,_addTaskHandler,_exitHandler,_autoLogoffHandler; private EventHandler<SessionLockChangedEventArgs>? _sessionLockHandler; private bool _running,_disposed,_mainWindowHiddenBySessionLock; private MainWindow? _mainWindow; private MainWindowViewModel? _mainWindowViewModel; private CancellationTokenSource? _updatePollingCts; private Task? _updatePollingTask;
        public Orchestrator(ITaskEngine taskEngine, IReminderScheduler reminderScheduler, IComplianceCore compliance, ITrayHost trayHost, IReminderDeliveryService reminderDeliveryService, ISettingsService settingsService, ISessionLockService sessionLock, IAppLogger logger, IClock clock, IAutoUpdater? autoUpdater = null, IBugReporter? bugReporter = null, TimeSpan? updatePollInterval = null, string? auditIncidentRoot = null)
        { _taskEngine=taskEngine??throw new ArgumentNullException(nameof(taskEngine)); _reminderScheduler=reminderScheduler??throw new ArgumentNullException(nameof(reminderScheduler)); _compliance=compliance??throw new ArgumentNullException(nameof(compliance)); _trayHost=trayHost??throw new ArgumentNullException(nameof(trayHost)); _reminderDeliveryService=reminderDeliveryService??throw new ArgumentNullException(nameof(reminderDeliveryService)); _settingsService=settingsService??throw new ArgumentNullException(nameof(settingsService)); _sessionLock=sessionLock??throw new ArgumentNullException(nameof(sessionLock)); _logger=logger??throw new ArgumentNullException(nameof(logger)); _clock=clock??throw new ArgumentNullException(nameof(clock)); _autoUpdater=autoUpdater; _bugReporter=bugReporter; _updatePollInterval=updatePollInterval??TimeSpan.FromHours(24); if(_updatePollInterval<=TimeSpan.Zero)throw new ArgumentOutOfRangeException(nameof(updatePollInterval),"Update poll interval must be positive."); _auditIncidentRoot=string.IsNullOrWhiteSpace(auditIncidentRoot)?GetDefaultAuditIncidentRoot():auditIncidentRoot; }
        public async Task StartAsync(CancellationToken ct)
        {
            ThrowIfDisposed();
            await _gate.WaitAsync(ct).ConfigureAwait(false);
            var trayInitializationAttempted = false;
            var sessionLockStartAttempted = false;
            var schedulerStartAttempted = false;
            var deliveryStartAttempted = false;
            var startupBegan = false;
            try
            {
                if (_running) throw new InvalidOperationException("Orchestrator already running.");
                startupBegan = true;

                await VerifyAuditChainAtStartupAsync().ConfigureAwait(false);
                await FlushBugReportQueueAtStartupAsync().ConfigureAwait(false);

                _showTreeHandler = OnShowTreeRequested;
                _addTaskHandler = (s, e) => _logger.LogInformation("AddTaskRequested");
                _exitHandler = (s, e) =>
                {
                    _logger.LogInformation("ExitRequested");
                    if (System.Windows.Application.Current is not null) System.Windows.Application.Current.Shutdown();
                };
                _sessionLockHandler = OnSessionLockChanged;
                _autoLogoffHandler = OnAutoLogoffTriggered;
                _trayHost.ShowTreeRequested += _showTreeHandler;
                _trayHost.AddTaskRequested += _addTaskHandler;
                _trayHost.ExitRequested += _exitHandler;
                _sessionLock.SessionLockChanged += _sessionLockHandler;
                _compliance.AutoLogoffTriggered += _autoLogoffHandler;

                trayInitializationAttempted = true;
                _trayHost.Initialize();
                _compliance.StartIdleMonitor(TimeSpan.FromMinutes(15));

                // Mark each start before awaiting it: an implementation may have
                // acquired resources and then fail during its start operation.
                sessionLockStartAttempted = true;
                await _sessionLock.StartAsync(ct).ConfigureAwait(false);
                schedulerStartAttempted = true;
                await _reminderScheduler.StartAsync(ct).ConfigureAwait(false);
                deliveryStartAttempted = true;
                await _reminderDeliveryService.StartAsync(ct).ConfigureAwait(false);
                await _compliance.AuditAsync(new AuditEntry
                {
                    Module = "Orchestrator",
                    Action = "Startup",
                    Result = "success",
                    Timestamp = _clock.UtcNow,
                }).ConfigureAwait(false);
                _running = true;
                StartUpdatePolling();
            }
            catch
            {
                if (!startupBegan) throw;
                _running = false;
                await CleanupFailedStartAsync(
                    trayInitializationAttempted,
                    sessionLockStartAttempted,
                    schedulerStartAttempted,
                    deliveryStartAttempted).ConfigureAwait(false);
                throw;
            }
            finally { _gate.Release(); }
        }

        public async Task StopAsync()
        {
            ThrowIfDisposed();
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!_running) return;
                _running = false;

                var failures = new List<Exception>();
                await StopUpdatePollingAsync(failures).ConfigureAwait(false);
                UnsubscribeHandlers(failures);
                CloseMainWindow();
                await TryStopAsync("SessionLock", _sessionLock.StopAsync, failures).ConfigureAwait(false);
                await TryStopAsync("ReminderDeliveryService", _reminderDeliveryService.StopAsync, failures).ConfigureAwait(false);
                await TryStopAsync("ReminderScheduler", _reminderScheduler.StopAsync, failures).ConfigureAwait(false);
                TryCleanup("TrayHost.Dispose", _trayHost.Dispose, failures);
                await TryStopAsync("Shutdown audit", () => _compliance.AuditAsync(new AuditEntry
                {
                    Module = "Orchestrator",
                    Action = "Shutdown",
                    Result = "success",
                    Timestamp = _clock.UtcNow,
                }), failures).ConfigureAwait(false);

                if (failures.Count > 0)
                    throw new AggregateException("Orchestrator shutdown encountered one or more failures.", failures);
            }
            finally { _gate.Release(); }
        }
        private async void OnShowTreeRequested(object? sender, EventArgs e){try{_mainWindowViewModel??=new MainWindowViewModel(_taskEngine,_clock,_logger,_settingsService);_mainWindow??=new MainWindow(_mainWindowViewModel);await _mainWindowViewModel.InitializeAsync().ConfigureAwait(true);if(!_mainWindow.IsVisible)_mainWindow.Show();else{_mainWindow.Visibility=System.Windows.Visibility.Visible;if(_mainWindow.WindowState==System.Windows.WindowState.Minimized)_mainWindow.WindowState=System.Windows.WindowState.Normal;_mainWindow.Activate();}_mainWindowHiddenBySessionLock=false;}catch(Exception ex){_logger.LogError(ex,"OnShowTreeRequested failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private void OnSessionLockChanged(object? sender, SessionLockChangedEventArgs e){try{var window=_mainWindow;if(window is null)return;if(window.Dispatcher.CheckAccess())ApplySessionLockState(e);else window.Dispatcher.BeginInvoke(new Action(()=>ApplySessionLockState(e)));}catch(Exception ex){_logger.LogError(ex,"OnSessionLockChanged failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private void OnAutoLogoffTriggered(object? sender, EventArgs e){try{var window=_mainWindow;if(window is null)return;if(window.Dispatcher.CheckAccess())HideMainWindowForAutoLogoff();else window.Dispatcher.BeginInvoke(new Action(HideMainWindowForAutoLogoff));}catch(Exception ex){_logger.LogError(ex,"OnAutoLogoffTriggered failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private void HideMainWindowForAutoLogoff(){try{if(_mainWindow is null || !_mainWindow.IsVisible)return;_mainWindow.Hide();_mainWindowHiddenBySessionLock=true;_logger.LogInformation("MainWindow hidden due to compliance auto-logoff.");}catch(Exception ex){_logger.LogError(ex,"Auto-logoff window hide failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private void ApplySessionLockState(SessionLockChangedEventArgs e){try{if(e.IsLocked && _mainWindow is not null && _mainWindow.IsVisible){_mainWindow.Hide();_mainWindowHiddenBySessionLock=true;_logger.LogInformation("MainWindow hidden due to session lock.");}else if(!e.IsLocked && _mainWindowHiddenBySessionLock){_mainWindowHiddenBySessionLock=false;_logger.LogInformation("Session unlocked; MainWindow remains hidden until user reopens.");}}catch(Exception ex){_logger.LogError(ex,"OnSessionLockChanged dispatcher callback failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private async Task CleanupFailedStartAsync(bool trayInitializationAttempted, bool sessionLockStartAttempted, bool schedulerStartAttempted, bool deliveryStartAttempted)
        {
            var failures = new List<Exception>();
            UnsubscribeHandlers(failures);
            CloseMainWindow();
            if (deliveryStartAttempted)
                await TryStopAsync("ReminderDeliveryService startup cleanup", _reminderDeliveryService.StopAsync, failures).ConfigureAwait(false);
            if (schedulerStartAttempted)
                await TryStopAsync("ReminderScheduler startup cleanup", _reminderScheduler.StopAsync, failures).ConfigureAwait(false);
            if (sessionLockStartAttempted)
                await TryStopAsync("SessionLock startup cleanup", _sessionLock.StopAsync, failures).ConfigureAwait(false);
            if (trayInitializationAttempted)
                TryCleanup("TrayHost startup cleanup", _trayHost.Dispose, failures);
            LogCleanupFailures("startup", failures);
        }

        private async Task VerifyAuditChainAtStartupAsync()
        {
            var chainValid = false;
            try
            {
                chainValid = await _compliance.VerifyChainIntegrityAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Audit chain verification failed at startup: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
            }

            if (chainValid) return;

            IReadOnlyList<AuditEntry>? chain = null;
            try
            {
                chain = await _compliance.GetAuditChainAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Unable to load the audit chain for startup integrity handling: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
            }

            var exportPath = TryExportLastKnownGoodChain(chain);
            ShowAuditIntegrityWarning(exportPath);
            try { _logger.LogError(null, "Audit chain verification failed at startup."); } catch { }
            try
            {
                await _compliance.AuditAsync(new AuditEntry
                {
                    Module = "Orchestrator",
                    Action = "ChainVerifyFailedAtStartup",
                    Result = "failure",
                    Timestamp = _clock.UtcNow,
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Unable to audit startup chain verification failure: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
            }
        }

        private string? TryExportLastKnownGoodChain(IReadOnlyList<AuditEntry>? chain)
        {
            if (chain is null) return null;

            try
            {
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrWhiteSpace(local)) return null;
                return AuditChainIncidentExporter.Export(
                    chain,
                    _auditIncidentRoot,
                    _clock.UtcNow);
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Unable to export the last-known-good audit chain: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
                return null;
            }
        }

        private static string GetDefaultAuditIncidentRoot()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "TaskTree", "audit-incidents");
        }

        private void ShowAuditIntegrityWarning(string? exportPath)
        {
            var message = exportPath is null
                ? "TaskTree detected an audit-chain integrity failure. The last-known-good audit export could not be written."
                : $"TaskTree detected an audit-chain integrity failure. A last-known-good audit export was written to:\n\n{exportPath}";
            try
            {
                var application = System.Windows.Application.Current;
                if (application is null) return;

                void Show() => System.Windows.MessageBox.Show(
                    message,
                    "TaskTree Audit Integrity Warning",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                if (application.Dispatcher.CheckAccess()) Show();
                else application.Dispatcher.Invoke(Show);
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Unable to show the audit integrity warning: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
            }
        }

        private async Task FlushBugReportQueueAtStartupAsync()
        {
            if (_bugReporter is null) return;
            try
            {
                await _bugReporter.FlushQueueAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try { _logger.LogError(ex, "Bug-report queue flush failed at startup: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
            }
        }

        private void StartUpdatePolling()
        {
            if (_autoUpdater is null || _updatePollingTask is not null) return;
            _updatePollingCts = new CancellationTokenSource();
            _updatePollingTask = UpdatePollingLoopAsync(_updatePollingCts.Token);
        }

        private async Task UpdatePollingLoopAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(_updatePollInterval);
            try
            {
                while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
                {
                    try
                    {
                        var manifest = await _autoUpdater!.CheckAsync().ConfigureAwait(false);
                        if (manifest is not null)
                            _logger.LogInformation("TaskTree update available: {0}", manifest.Version);
                    }
                    catch (Exception ex)
                    {
                        try { _logger.LogError(ex, "Updater poll failed: {0}: {1}", ex.GetType().Name, ex.Message); } catch { }
                    }
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        }

        private async Task StopUpdatePollingAsync(List<Exception> failures)
        {
            var cts = _updatePollingCts;
            var task = _updatePollingTask;
            if (cts is null || task is null) return;

            cts.Cancel();
            var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(4))).ConfigureAwait(false);
            if (completed != task)
            {
                try { _logger.LogWarning("Updater poll did not stop within the shutdown budget; allowing its bounded request to finish."); } catch { }
                _ = task.ContinueWith(t =>
                {
                    _ = t.Exception;
                    cts.Dispose();
                    if (ReferenceEquals(_updatePollingTask, t))
                    {
                        _updatePollingTask = null;
                        _updatePollingCts = null;
                    }
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return;
            }

            try { await task.ConfigureAwait(false); }
            catch (Exception ex) { failures.Add(ex); }
            finally
            {
                cts.Dispose();
                _updatePollingTask = null;
                _updatePollingCts = null;
            }
        }

        private void UnsubscribeHandlers(List<Exception> failures)
        {
            var showTreeHandler = _showTreeHandler;
            if (showTreeHandler is not null)
                TryCleanup("ShowTreeRequested unsubscribe", () => _trayHost.ShowTreeRequested -= showTreeHandler, failures);
            var addTaskHandler = _addTaskHandler;
            if (addTaskHandler is not null)
                TryCleanup("AddTaskRequested unsubscribe", () => _trayHost.AddTaskRequested -= addTaskHandler, failures);
            var exitHandler = _exitHandler;
            if (exitHandler is not null)
                TryCleanup("ExitRequested unsubscribe", () => _trayHost.ExitRequested -= exitHandler, failures);
            var sessionLockHandler = _sessionLockHandler;
            if (sessionLockHandler is not null)
                TryCleanup("SessionLockChanged unsubscribe", () => _sessionLock.SessionLockChanged -= sessionLockHandler, failures);
            var autoLogoffHandler = _autoLogoffHandler;
            if (autoLogoffHandler is not null)
                TryCleanup("AutoLogoffTriggered unsubscribe", () => _compliance.AutoLogoffTriggered -= autoLogoffHandler, failures);
            _showTreeHandler = null;
            _addTaskHandler = null;
            _exitHandler = null;
            _sessionLockHandler = null;
            _autoLogoffHandler = null;
        }

        private void CloseMainWindow()
        {
            try { _mainWindow?.Close(); }
            catch (Exception ex) { _logger.LogError(ex, "MainWindow.Close failed: {0}", ex.Message); }
            finally
            {
                _mainWindow = null;
                _mainWindowViewModel = null;
                _mainWindowHiddenBySessionLock = false;
            }
        }

        private async Task TryStopAsync(string operation, Func<Task> stop, List<Exception> failures)
        {
            try { await stop().ConfigureAwait(false); }
            catch (Exception ex)
            {
                failures.Add(ex);
                try { _logger.LogError(ex, "{0} failed during orchestrator cleanup: {1}", operation, ex.Message); } catch { }
            }
        }

        private void TryCleanup(string operation, Action cleanup, List<Exception> failures)
        {
            try { cleanup(); }
            catch (Exception ex)
            {
                failures.Add(ex);
                try { _logger.LogError(ex, "{0} failed during orchestrator cleanup: {1}", operation, ex.Message); } catch { }
            }
        }

        private void LogCleanupFailures(string phase, List<Exception> failures)
        {
            foreach (var failure in failures)
            {
                try { _logger.LogError(failure, "Orchestrator {0} cleanup failed: {1}", phase, failure.Message); } catch { }
            }
        }

        private void ThrowIfDisposed(){if(_disposed)throw new ObjectDisposedException(nameof(Orchestrator));}
        public void Dispose(){if(_disposed)return;try{StopAsync().GetAwaiter().GetResult();}catch{}try{_gate.Dispose();}catch{} _disposed=true;}
    }
}
