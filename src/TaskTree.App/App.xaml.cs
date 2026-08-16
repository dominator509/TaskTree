// =============================================================================
// TaskTree - App.xaml.cs
// Phase:      1F Msg 2
// =============================================================================
using System;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TaskTree.App.Bootstrap;
using TaskTree.Core.Abstractions;

namespace TaskTree.App;

public partial class App : Application
{
    private IServiceProvider? _services;
    private IOrchestrator? _orchestrator;
    private IBugReporter? _bugReporter;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            _services = CompositionRoot.BuildServiceProvider();
            _bugReporter = _services.GetRequiredService<IBugReporter>();
            _bugReporter.HookGlobalCrashHandler();
            _orchestrator = _services.GetRequiredService<IOrchestrator>();
            await _orchestrator.StartAsync(CancellationToken.None).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"TaskTree failed to start.\n\n{ex.GetType().Name}: {ex.Message}",
                "TaskTree - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_orchestrator is not null)
                await _orchestrator.StopAsync().ConfigureAwait(true);
        }
        catch { }
        finally
        {
            if (_services is IDisposable disposable)
                disposable.Dispose();
        }
        base.OnExit(e);
    }
}
