using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TaskTree.Modules.AutoUpdater;

internal static class MsixPackageInstaller
{
    public static async Task InstallAsync(string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath))
            throw new ArgumentException("MSIX package path is required.", nameof(packagePath));
        if (!File.Exists(packagePath))
            throw new FileNotFoundException("MSIX package was not found.", packagePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add("Add-AppxPackage -Path $args[0] -ForceApplicationShutdown");
        startInfo.ArgumentList.Add(packagePath);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start PowerShell for MSIX installation.");
        await process.WaitForExitAsync().ConfigureAwait(false);
        if (process.ExitCode == 0) return;

        var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
        throw new InvalidOperationException(
            $"Add-AppxPackage failed with exit code {process.ExitCode}: {error.Trim()}");
    }

    public static string GetDefaultStagedPath(string version)
    {
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Version is required.", nameof(version));
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(local, "TaskTree", "updates", $"TaskTree-{version}.msix");
    }
}
