// SPEC-DERIVED-PHASE1F
// SPEC-DERIVED-PHASE1G-MSG2
// SPEC-DERIVED-PHASE2B
// SPEC-DERIVED-PHASE2E
// SPEC-DERIVED-PHASE2F  HALT #11/#12/#13 (session lock wiring)

using System;
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
        private readonly ITaskEngine _taskEngine; private readonly IReminderScheduler _reminderScheduler; private readonly IComplianceCore _compliance; private readonly ITrayHost _trayHost; private readonly IReminderDeliveryService _reminderDeliveryService; private readonly ISettingsService _settingsService; private readonly ISessionLockService _sessionLock; private readonly IAppLogger _logger; private readonly IClock _clock;
        private readonly SemaphoreSlim _gate = new(1,1); private EventHandler? _showTreeHandler,_addTaskHandler,_exitHandler; private EventHandler<SessionLockChangedEventArgs>? _sessionLockHandler; private bool _running,_disposed,_mainWindowHiddenBySessionLock; private MainWindow? _mainWindow; private MainWindowViewModel? _mainWindowViewModel;
        public Orchestrator(ITaskEngine taskEngine, IReminderScheduler reminderScheduler, IComplianceCore compliance, ITrayHost trayHost, IReminderDeliveryService reminderDeliveryService, ISettingsService settingsService, ISessionLockService sessionLock, IAppLogger logger, IClock clock)
        { _taskEngine=taskEngine??throw new ArgumentNullException(nameof(taskEngine)); _reminderScheduler=reminderScheduler??throw new ArgumentNullException(nameof(reminderScheduler)); _compliance=compliance??throw new ArgumentNullException(nameof(compliance)); _trayHost=trayHost??throw new ArgumentNullException(nameof(trayHost)); _reminderDeliveryService=reminderDeliveryService??throw new ArgumentNullException(nameof(reminderDeliveryService)); _settingsService=settingsService??throw new ArgumentNullException(nameof(settingsService)); _sessionLock=sessionLock??throw new ArgumentNullException(nameof(sessionLock)); _logger=logger??throw new ArgumentNullException(nameof(logger)); _clock=clock??throw new ArgumentNullException(nameof(clock)); }
        public async Task StartAsync(CancellationToken ct){ThrowIfDisposed();await _gate.WaitAsync(ct).ConfigureAwait(false);try{if(_running)throw new InvalidOperationException("Orchestrator already running.");_showTreeHandler=OnShowTreeRequested;_addTaskHandler=(s,e)=>_logger.LogInformation("AddTaskRequested");_exitHandler=(s,e)=>{_logger.LogInformation("ExitRequested");if(System.Windows.Application.Current is not null)System.Windows.Application.Current.Shutdown();};_sessionLockHandler=OnSessionLockChanged;_trayHost.ShowTreeRequested+=_showTreeHandler;_trayHost.AddTaskRequested+=_addTaskHandler;_trayHost.ExitRequested+=_exitHandler;_sessionLock.SessionLockChanged+=_sessionLockHandler;_trayHost.Initialize();_compliance.StartIdleMonitor(TimeSpan.FromMinutes(15));await _sessionLock.StartAsync(ct).ConfigureAwait(false);await _reminderScheduler.StartAsync(ct).ConfigureAwait(false);await _reminderDeliveryService.StartAsync(ct).ConfigureAwait(false);await _compliance.AuditAsync(new AuditEntry{Module="Orchestrator",Action="Startup",Result="success",Timestamp=_clock.UtcNow}).ConfigureAwait(false);_running=true;}finally{_gate.Release();}}
        public async Task StopAsync(){ThrowIfDisposed();await _gate.WaitAsync().ConfigureAwait(false);try{if(!_running)return;if(_showTreeHandler is not null)_trayHost.ShowTreeRequested-=_showTreeHandler;if(_addTaskHandler is not null)_trayHost.AddTaskRequested-=_addTaskHandler;if(_exitHandler is not null)_trayHost.ExitRequested-=_exitHandler;if(_sessionLockHandler is not null)_sessionLock.SessionLockChanged-=_sessionLockHandler;try{_mainWindow?.Close();_mainWindow=null;_mainWindowViewModel=null;}catch(Exception ex){_logger.LogError(ex,"MainWindow.Close failed: {0}",ex.Message);}await _sessionLock.StopAsync().ConfigureAwait(false);await _reminderDeliveryService.StopAsync().ConfigureAwait(false);await _reminderScheduler.StopAsync().ConfigureAwait(false);_trayHost.Dispose();await _compliance.AuditAsync(new AuditEntry{Module="Orchestrator",Action="Shutdown",Result="success",Timestamp=_clock.UtcNow}).ConfigureAwait(false);_running=false;}finally{_gate.Release();}}
        private async void OnShowTreeRequested(object? sender, EventArgs e){try{_mainWindowViewModel??=new MainWindowViewModel(_taskEngine,_clock,_logger,_settingsService);_mainWindow??=new MainWindow(_mainWindowViewModel);await _mainWindowViewModel.InitializeAsync().ConfigureAwait(true);if(!_mainWindow.IsVisible)_mainWindow.Show();else{_mainWindow.Visibility=System.Windows.Visibility.Visible;if(_mainWindow.WindowState==System.Windows.WindowState.Minimized)_mainWindow.WindowState=System.Windows.WindowState.Normal;_mainWindow.Activate();}_mainWindowHiddenBySessionLock=false;}catch(Exception ex){_logger.LogError(ex,"OnShowTreeRequested failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private void OnSessionLockChanged(object? sender, SessionLockChangedEventArgs e){try{if(e.IsLocked && _mainWindow is not null && _mainWindow.IsVisible){_mainWindow.Hide();_mainWindowHiddenBySessionLock=true;_logger.LogInformation("MainWindow hidden due to session lock.");}else if(!e.IsLocked && _mainWindowHiddenBySessionLock){_mainWindowHiddenBySessionLock=false;_logger.LogInformation("Session unlocked; MainWindow remains hidden until user reopens.");}}catch(Exception ex){_logger.LogError(ex,"OnSessionLockChanged failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private void ThrowIfDisposed(){if(_disposed)throw new ObjectDisposedException(nameof(Orchestrator));}
        public void Dispose(){if(_disposed)return;try{StopAsync().GetAwaiter().GetResult();}catch{}try{_gate.Dispose();}catch{} _disposed=true;}
    }
}
