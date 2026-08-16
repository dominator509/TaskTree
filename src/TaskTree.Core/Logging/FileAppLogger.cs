// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Logging/FileAppLogger.cs
//  Purpose: File-backed JSON-lines logger per Architecture §3.3 / §12; implements IAppLogger (Msg 2).
//  Architecture.md References: §3.3, §12, §10.2, §10.5, P0-AC5
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 5 — Security + Logging Primitives)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: behavior matches Architecture.md verbatim where specified (or SPEC-DERIVED-MSG5).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskTree.Core.Abstractions;

namespace TaskTree.Core.Logging;

/// <summary>
/// File-backed JSON-lines logger implementing <see cref="IAppLogger"/>.
/// Writes one canonical JSON object per line per Roadmap P0-AC5.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG5: surface not specified verbatim in Architecture.md;
/// derived from documented usage and approved by human owner on 2026-05-26.
/// See docs/spec-derivations/PHASE0-MSG5-DERIVATIONS.md.
/// <para>
/// Architecture.md is silent on the FileAppLogger surface beyond "writes
/// valid JSON" (Roadmap P0-AC5). Derivations recorded in
/// PHASE0-MSG5-DERIVATIONS.md §3:
/// - Constructor parameters: log directory (caller-injected; default in DI
///   resolves to <c>%LOCALAPPDATA%\TaskTree\logs\</c>), file name (default
///   <c>tasktree.log</c>), max size (default 10 MB).
/// - Rotation: size-based; on threshold the current file is renamed to
///   <c>tasktree.log.YYYYMMDD-HHmmss</c> and a new file starts.
///   No purge in v1.0 (documented limitation; §16 honest candidate).
/// - JSON-line schema: <c>{ timestamp, level, message, args, exception }</c>.
/// - Thread safety: lock per append; sufficient for §15 perf targets.
/// </para>
/// <para>
/// PHI redaction: NOT performed here — callers redact via
/// <c>IComplianceCore.RedactPhi</c> (§9.2.3) BEFORE invoking the logger,
/// per the <see cref="IAppLogger"/> contract (PHASE0-MSG2-DERIVATIONS §3).
/// </para>
/// </remarks>
public sealed class FileAppLogger : IAppLogger
{
    /// <summary>Default log file name.</summary>
    public const string DefaultLogFileName = "tasktree.log";

    /// <summary>Default size threshold (10 MB) that triggers rotation.</summary>
    public const long DefaultMaxFileSizeBytes = 10L * 1024L * 1024L;

    private readonly string _logDirectory;
    private readonly string _logFileName;
    private readonly long _maxFileSizeBytes;
    private readonly string _logFilePath;
    private readonly object _writeLock = new();

    /// <summary>Initializes a new file-backed JSON-lines logger.</summary>
    /// <param name="logDirectory">Directory that will contain the log file. Created if absent.</param>
    /// <param name="logFileName">Log file name. Defaults to <see cref="DefaultLogFileName"/>.</param>
    /// <param name="maxFileSizeBytes">Size threshold that triggers rotation. Defaults to <see cref="DefaultMaxFileSizeBytes"/>.</param>
    public FileAppLogger(string logDirectory, string logFileName = DefaultLogFileName, long maxFileSizeBytes = DefaultMaxFileSizeBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(logFileName);
        if (maxFileSizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFileSizeBytes));

        _logDirectory = logDirectory;
        _logFileName = logFileName;
        _maxFileSizeBytes = maxFileSizeBytes;
        _logFilePath = Path.Combine(_logDirectory, _logFileName);
        Directory.CreateDirectory(_logDirectory);
    }

    /// <summary>Gets the absolute path of the active log file.</summary>
    public string LogFilePath => _logFilePath;

    /// <inheritdoc />
    public void LogDebug(string message, params object?[] args)
        => Write("debug", message, args, null);

    /// <inheritdoc />
    public void LogInformation(string message, params object?[] args)
        => Write("information", message, args, null);

    /// <inheritdoc />
    public void LogWarning(string message, params object?[] args)
        => Write("warning", message, args, null);

    /// <inheritdoc />
    public void LogError(Exception? exception, string message, params object?[] args)
        => Write("error", message, args, exception);

    private void Write(string level, string message, object?[]? args, Exception? exception)
    {
        var entry = new LogLine
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level     = level,
            Message   = message ?? string.Empty,
            Args      = args is { Length: > 0 } ? args : null,
            Exception = exception?.ToString(),
        };
        string json = JsonSerializer.Serialize(entry, JsonOptions);

        lock (_writeLock)
        {
            RotateIfNeeded_NoLock();
            File.AppendAllText(_logFilePath, json + Environment.NewLine, Encoding.UTF8);
        }
    }

    private void RotateIfNeeded_NoLock()
    {
        if (!File.Exists(_logFilePath)) return;
        var fi = new FileInfo(_logFilePath);
        if (fi.Length < _maxFileSizeBytes) return;
        string suffix = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        string rotated = Path.Combine(_logDirectory, $"{_logFileName}.{suffix}");
        var collision = 0;
        while (File.Exists(rotated))
        {
            collision++;
            rotated = Path.Combine(_logDirectory, $"{_logFileName}.{suffix}-{collision}");
        }
        File.Move(_logFilePath, rotated, overwrite: false);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed class LogLine
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object?[]? Args { get; set; }
        public string? Exception { get; set; }
    }
}
