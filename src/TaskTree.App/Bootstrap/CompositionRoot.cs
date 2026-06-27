// ============================================================================
// File: src/TaskTree.App/Bootstrap/CompositionRoot.cs
// Architecture §3.3 paths; §10.3 keys path
// SPEC-DERIVED-PHASE1F  HALT #7 (CR/SR split), HALT #8 (path helpers); Gaps #4 + #5
// ============================================================================

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace TaskTree.App.Bootstrap
{
    /// <summary>Composition root: canonical %LOCALAPPDATA% path helpers + DI container build.</summary>
    public static class CompositionRoot
    {
        /// <summary>%LOCALAPPDATA%\TaskTree\keys\ (created if missing).</summary>
        public static string GetKeyDirectory() => EnsureDir(Path.Combine(LocalAppData, "TaskTree", "keys"));

        /// <summary>%LOCALAPPDATA%\TaskTree\store\ (created if missing).</summary>
        public static string GetStorageDirectory() => EnsureDir(Path.Combine(LocalAppData, "TaskTree", "store"));

        /// <summary>%LOCALAPPDATA%\TaskTree\logs\ (created if missing).</summary>
        public static string GetLogDirectory() => EnsureDir(Path.Combine(LocalAppData, "TaskTree", "logs"));

        /// <summary>Builds the full DI container with scope validation.</summary>
        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTaskTreeServices();
            return services.BuildServiceProvider(validateScopes: true);
        }

        private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static string EnsureDir(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
