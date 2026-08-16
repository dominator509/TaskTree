// SPEC-DERIVED-PHASE3D  HALT #9/#10/#11/#12
// Architecture.md Section 9.2.5 encrypted local queue via SecureStore; Section 9.2 dedup by fingerprint.
// Gap #249/#250/#251: queue key/surface/dedup boundary are derived and require Phase 5C validation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Encrypted local bug report queue backed by ISecureStore.</summary>
    public sealed class BugReportQueue
    {
        private const string StorageKey = "bugreports/queue";
        private readonly ISecureStore _store;
        private readonly SemaphoreSlim _gate = new(1, 1);
        public BugReportQueue(ISecureStore store) => _store = store ?? throw new ArgumentNullException(nameof(store));
        public async Task EnqueueAsync(BugReport report)
        {
            if (report is null) throw new ArgumentNullException(nameof(report));
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var list = await LoadAsync().ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(report.Fingerprint) && list.Any(r => string.Equals(r.Fingerprint, report.Fingerprint, StringComparison.OrdinalIgnoreCase))) return;
                list.Add(report);
                await _store.SaveAsync(StorageKey, list).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }
        public async Task<IReadOnlyList<BugReport>> GetAllAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { return await LoadAsync().ConfigureAwait(false); }
            finally { _gate.Release(); }
        }
        public async Task RemoveAsync(Guid id)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var list = await LoadAsync().ConfigureAwait(false);
                if (list.RemoveAll(r => r.Id == id) > 0) await _store.SaveAsync(StorageKey, list).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }
        public async Task<int> CountAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { return (await LoadAsync().ConfigureAwait(false)).Count; }
            finally { _gate.Release(); }
        }
        private async Task<List<BugReport>> LoadAsync() => await _store.LoadAsync<List<BugReport>>(StorageKey).ConfigureAwait(false) ?? new List<BugReport>();
    }
}
