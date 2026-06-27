// SPEC-DERIVED-PHASE1H  HALT #5 (Gap #98 - Phase 1A R7 promotion at 8th-consumer trigger)
// Option B project re-evaluation alongside FakeClock at Phase 2A per Gap #97.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;

namespace TaskTree.Core.Tests.TestDoubles
{
    /// <summary>Thread-safe in-memory ISecureStore for offline tests using JSON round-trip.</summary>
    public sealed class InMemorySecureStore : ISecureStore
    {
        private readonly Dictionary<string, byte[]> _store = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _gate = new(1, 1);

        public async Task<T?> LoadAsync<T>(string key) where T : class
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!_store.TryGetValue(key, out var bytes)) return null;
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            finally { _gate.Release(); }
        }

        public async Task SaveAsync<T>(string key, T value) where T : class
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                _store[key] = System.Text.Encoding.UTF8.GetBytes(json);
            }
            finally { _gate.Release(); }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { return _store.ContainsKey(key); }
            finally { _gate.Release(); }
        }

        public async Task DeleteAsync(string key)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { _store.Remove(key); }
            finally { _gate.Release(); }
        }
    }
}
