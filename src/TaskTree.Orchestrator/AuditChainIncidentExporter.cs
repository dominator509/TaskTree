using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using TaskTree.Core.Models;
using TaskTree.Core.Security;

namespace TaskTree.Orchestrator;

/// <summary>
/// Writes a bounded last-known-good audit-chain prefix for startup integrity
/// incidents. This is an internal incident path and is not part of the core
/// module contracts.
/// </summary>
internal static class AuditChainIncidentExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    internal static IReadOnlyList<AuditEntry> GetLastKnownGoodPrefix(IReadOnlyList<AuditEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var prefix = new List<AuditEntry>(entries.Count);
        long expectedSeq = 1;
        var expectedPrevHash = HashChain.GenesisPrevHash;
        foreach (var entry in entries)
        {
            if (entry is null || entry.PrevHash is null || entry.Hash is null || entry.Seq != expectedSeq ||
                !string.Equals(entry.PrevHash, expectedPrevHash, StringComparison.Ordinal) ||
                !string.Equals(entry.Hash, HashChain.ComputeHash(entry.PrevHash, entry), StringComparison.Ordinal))
            {
                break;
            }

            prefix.Add(entry);
            expectedSeq++;
            expectedPrevHash = entry.Hash;
        }

        return prefix;
    }

    internal static string Export(IReadOnlyList<AuditEntry> entries, string root, DateTimeOffset exportedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Export root is required.", nameof(root));

        var prefix = GetLastKnownGoodPrefix(entries);
        Directory.CreateDirectory(root);

        var timestamp = exportedAtUtc.UtcDateTime.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture);
        var path = Path.Combine(root, $"audit-chain-last-known-good-{timestamp}-{Guid.NewGuid():N}.json");
        var temporaryPath = path + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(prefix, JsonOptions));
            File.Move(temporaryPath, path);
            return path;
        }
        catch
        {
            try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
            throw;
        }
    }
}
