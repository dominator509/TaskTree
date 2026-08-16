// SPEC-DERIVED-PHASE3C  HALT #13/#14/#15/#16
// Architecture.md Section 9.1.5: sentinel file supports first-launch crash rollback.
// Gap #238/#239/#240: real Windows profile behavior, public surface, and sentinel content format need validation/documentation.

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Manages first-launch rollback sentinel file.</summary>
    public sealed class SentinelService
    {
        private readonly string _sentinelPath;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
        public SentinelService() : this(GetDefaultSentinelPath()) { }
        public SentinelService(string sentinelPath) => _sentinelPath = string.IsNullOrWhiteSpace(sentinelPath) ? throw new ArgumentException("Sentinel path required.", nameof(sentinelPath)) : sentinelPath;
        public async Task CreateAsync(UpdateManifest manifest)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            await _gate.WaitAsync().ConfigureAwait(false);
            var temporaryPath = _sentinelPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                var dir = Path.GetDirectoryName(_sentinelPath);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(manifest, JsonOptions)).ConfigureAwait(false);
                File.Move(temporaryPath, _sentinelPath, overwrite: true);
            }
            finally
            {
                TryDelete(temporaryPath);
                _gate.Release();
            }
        }
        public async Task<bool> ExistsAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { return File.Exists(_sentinelPath); }
            finally { _gate.Release(); }
        }
        public async Task ClearAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { if (File.Exists(_sentinelPath)) File.Delete(_sentinelPath); }
            finally { _gate.Release(); }
        }
        public async Task<UpdateManifest?> ReadAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!File.Exists(_sentinelPath)) return null;
                await using var stream = File.OpenRead(_sentinelPath);
                return await JsonSerializer.DeserializeAsync<UpdateManifest>(stream, JsonOptions).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }
        private static void TryDelete(string path){try{if(File.Exists(path))File.Delete(path);}catch{} }
        private static string GetDefaultSentinelPath()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "TaskTree", "sentinel.lock");
        }
    }
}
