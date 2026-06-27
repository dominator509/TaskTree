// =============================================================================
// TaskTree - App.xaml.cs
// Phase:      1F Msg 2
// =============================================================================
using System;
using System.Threading;
using System.Windows;
using TaskTree.App.Bootstrap;

namespace TaskTree.App;

public partial class App : Application
{
    private readonly CompositionRoot _root = new();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            await _root.StartAsync(CancellationToken.None).ConfigureAwait(true);
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
        try { await _root.DisposeAsync().ConfigureAwait(true); }
        catch { }
        base.OnExit(e);
    }
}
