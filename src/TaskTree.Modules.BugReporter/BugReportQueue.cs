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
        private const string RetentionStorageKey = "bugreports/retention";
        internal static readonly TimeSpan SuccessRetention = TimeSpan.FromDays(7);
        internal static readonly TimeSpan FailureRetention = TimeSpan.FromDays(30);
        private readonly ISecureStore _store;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private sealed class RetentionRecord
        {
            public DateTimeOffset EnqueuedAtUtc { get; set; }
            public DateTimeOffset? LastAttemptAtUtc { get; set; }
            public DateTimeOffset? DeliveredAtUtc { get; set; }
        }

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
                var retention = await LoadRetentionAsync().ConfigureAwait(false);
                retention[report.Id] = new RetentionRecord
                {
                    EnqueuedAtUtc = report.Timestamp == default ? DateTimeOffset.UtcNow : report.Timestamp,
                };
                await SaveRetentionAsync(retention).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public async Task<IReadOnlyList<BugReport>> GetAllAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { return await LoadAsync().ConfigureAwait(false); }
            finally { _gate.Release(); }
        }

        /// <summary>Returns queued reports that have not completed a delivery.</summary>
        public async Task<IReadOnlyList<BugReport>> GetPendingAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var list = await LoadAsync().ConfigureAwait(false);
                var retention = await LoadRetentionAsync().ConfigureAwait(false);
                return list.Where(report =>
                    !retention.TryGetValue(report.Id, out var record) || !record.DeliveredAtUtc.HasValue).ToList();
            }
            finally { _gate.Release(); }
        }

        /// <summary>Records the outcome of a delivery attempt for retention and retry policy.</summary>
        public async Task RecordDeliveryResultAsync(Guid id, DateTimeOffset attemptedAtUtc, bool delivered)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var list = await LoadAsync().ConfigureAwait(false);
                var report = list.FirstOrDefault(candidate => candidate.Id == id);
                if (report is null) return;

                var retention = await LoadRetentionAsync().ConfigureAwait(false);
                if (!retention.TryGetValue(id, out var record))
                {
                    record = new RetentionRecord
                    {
                        EnqueuedAtUtc = report.Timestamp == default ? attemptedAtUtc : report.Timestamp,
                    };
                    retention[id] = record;
                }

                record.LastAttemptAtUtc = attemptedAtUtc;
                record.DeliveredAtUtc = delivered ? attemptedAtUtc : null;
                await SaveRetentionAsync(retention).ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        /// <summary>Purges delivered reports after 7 days and failed reports after 30 days.</summary>
        public async Task<int> PurgeExpiredAsync(DateTimeOffset nowUtc)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var list = await LoadAsync().ConfigureAwait(false);
                if (list.Count == 0) return 0;

                var retention = await LoadRetentionAsync().ConfigureAwait(false);
                var expired = new List<Guid>();
                foreach (var report in list)
                {
                    if (!retention.TryGetValue(report.Id, out var record))
                    {
                        record = new RetentionRecord
                        {
                            EnqueuedAtUtc = report.Timestamp == default ? nowUtc : report.Timestamp,
                        };
                        retention[report.Id] = record;
                    }

                    var anchor = record.DeliveredAtUtc ?? record.EnqueuedAtUtc;
                    var retentionPeriod = record.DeliveredAtUtc.HasValue ? SuccessRetention : FailureRetention;
                    if (nowUtc >= anchor + retentionPeriod)
                        expired.Add(report.Id);
                }

                if (expired.Count == 0)
                {
                    await SaveRetentionAsync(retention).ConfigureAwait(false);
                    return 0;
                }

                list.RemoveAll(report => expired.Contains(report.Id));
                foreach (var id in expired) retention.Remove(id);
                await _store.SaveAsync(StorageKey, list).ConfigureAwait(false);
                await SaveRetentionAsync(retention).ConfigureAwait(false);
                return expired.Count;
            }
            finally { _gate.Release(); }
        }

        public async Task RemoveAsync(Guid id)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var list = await LoadAsync().ConfigureAwait(false);
                if (list.RemoveAll(r => r.Id == id) > 0)
                {
                    await _store.SaveAsync(StorageKey, list).ConfigureAwait(false);
                    var retention = await LoadRetentionAsync().ConfigureAwait(false);
                    if (retention.Remove(id)) await SaveRetentionAsync(retention).ConfigureAwait(false);
                }
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
        private async Task<Dictionary<Guid, RetentionRecord>> LoadRetentionAsync() => await _store.LoadAsync<Dictionary<Guid, RetentionRecord>>(RetentionStorageKey).ConfigureAwait(false) ?? new Dictionary<Guid, RetentionRecord>();
        private Task SaveRetentionAsync(Dictionary<Guid, RetentionRecord> retention) => _store.SaveAsync(RetentionStorageKey, retention);
    }
}
