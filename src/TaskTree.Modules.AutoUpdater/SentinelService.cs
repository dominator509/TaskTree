// SPEC-DERIVED-PHASE3C  HALT #13/#14/#15/#16
// Architecture.md Section 9.1.5: sentinel file supports first-launch crash rollback.
// Gap #238/#239/#240: real Windows profile behavior, public surface, and sentinel content format need validation/documentation.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Manages first-launch rollback sentinel file.</summary>
    public sealed class SentinelService
    {
        private readonly string _sentinelPath;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
        public SentinelService() : this(GetDefaultSentinelPath()) { }
        public SentinelService(string sentinelPath) => _sentinelPath = string.IsNullOrWhiteSpace(sentinelPath) ? throw new ArgumentException("Sentinel path required.", nameof(sentinelPath)) : sentinelPath;
        public async Task CreateAsync(UpdateManifest manifest)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            var dir = Path.GetDirectoryName(_sentinelPath);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(_sentinelPath, JsonSerializer.Serialize(manifest, JsonOptions)).ConfigureAwait(false);
        }
        public Task<bool> ExistsAsync() => Task.FromResult(File.Exists(_sentinelPath));
        public Task ClearAsync(){ if (File.Exists(_sentinelPath)) File.Delete(_sentinelPath); return Task.CompletedTask; }
        public async Task<UpdateManifest?> ReadAsync()
        {
            if (!File.Exists(_sentinelPath)) return null;
            await using var stream = File.OpenRead(_sentinelPath);
            return await JsonSerializer.DeserializeAsync<UpdateManifest>(stream, JsonOptions).ConfigureAwait(false);
        }
        private static string GetDefaultSentinelPath()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "TaskTree", "sentinel.lock");
        }
    }
}
