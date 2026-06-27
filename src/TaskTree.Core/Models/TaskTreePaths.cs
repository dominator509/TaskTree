// =============================================================================
// TaskTree - TaskTreePaths.cs
// Implements: Architecture.md v1.0.3 §3.3 + §10.3 amendment (HALT #7 Option B)
// Phase:      1F Msg 1
// SPEC-DERIVED-PHASE1F §2 - see docs/spec-derivations/PHASE1F-DERIVATIONS.md
// =============================================================================
using System;
using System.IO;

namespace TaskTree.Core.Models;

/// <summary>
/// Canonical filesystem paths used by TaskTree. Registered as a singleton in
/// ServiceRegistrations and injected into consumers (MasterKeyManager,
/// SecureStore, FileAppLogger, AutoUpdater, BugReporter).
/// </summary>
public sealed class TaskTreePaths
{
    public string RootDir { get; }
    public string KeyDir { get; }
    public string MasterKeyPath { get; }
    public string StorageDir { get; }
    public string LogDir { get; }
    public string UpdatesDir { get; }
    public string BugReportsDir { get; }
    public string SentinelPath { get; }

    public TaskTreePaths()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskTree"))
    {
    }

    public TaskTreePaths(string rootDir)
    {
        if (string.IsNullOrWhiteSpace(rootDir))
            throw new ArgumentException("Root directory must be non-empty.", nameof(rootDir));

        RootDir       = rootDir;
        KeyDir        = Path.Combine(RootDir, "keys");
        MasterKeyPath = Path.Combine(KeyDir, "master.bin");
        StorageDir    = Path.Combine(RootDir, "store");
        LogDir        = Path.Combine(RootDir, "logs");
        UpdatesDir    = Path.Combine(RootDir, "updates");
        BugReportsDir = Path.Combine(RootDir, "bugreports");
        SentinelPath  = Path.Combine(RootDir, "sentinel.lock");
    }

    /// <summary>
    /// Creates any missing directories. Called at app startup by CompositionRoot
    /// (Phase 1F Msg 2) before module construction.
    /// </summary>
    public void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(RootDir);
        Directory.CreateDirectory(KeyDir);
        Directory.CreateDirectory(StorageDir);
        Directory.CreateDirectory(LogDir);
        Directory.CreateDirectory(UpdatesDir);
        Directory.CreateDirectory(BugReportsDir);
    }
}
