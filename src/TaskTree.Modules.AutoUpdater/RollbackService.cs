// SPEC-DERIVED-PHASE3C  HALT #17/#18/#19/#20
// Architecture.md Section 9.1.5 rollback strategy.
// Gap #241/#242/#243: live MSIX rollback, rollback directory, and selection rule require Phase 5E/Architecture documentation.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Finds rollback packages and exposes the Phase 5E MSIX rollback stub.</summary>
    public sealed class RollbackService
    {
        private readonly string _rollbackRoot;
        public RollbackService() : this(GetDefaultRollbackRoot()) { }
        public RollbackService(string rollbackRoot) => _rollbackRoot = string.IsNullOrWhiteSpace(rollbackRoot) ? throw new ArgumentException("Rollback root required.", nameof(rollbackRoot)) : rollbackRoot;
        public Task<string?> FindLastKnownGoodAsync()
        {
            if (!Directory.Exists(_rollbackRoot)) return Task.FromResult<string?>(null);
            var newest = Directory.GetFiles(_rollbackRoot, "*.msix", SearchOption.TopDirectoryOnly)
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault();
            return Task.FromResult(newest?.FullName);
        }
        public Task RollbackAsync() => throw new NotImplementedException("HIGH: MSIX rollback restore requires Add-AppxPackage - Codex Phase 5E");
        private static string GetDefaultRollbackRoot()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "TaskTree", "updates", "rollback");
        }
    }
}
