// SPEC-DERIVED-PHASE1H HALT #5 (Gap #98 - promoted test double)
// SPEC-DERIVED-PHASE2A HALT #7 (Gap #98 - Option B promotion)
// Relocated from tests/TaskTree.Core.Tests/TestDoubles/InMemorySecureStore.cs.
// Phase 5A MUST delete the old location (Gap #104).

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;

namespace TaskTree.TestSupport
{
    public sealed class InMemorySecureStore : ISecureStore
    {
        private readonly Dictionary<string, byte[]> _store = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _gate = new(1, 1);
        private readonly bool _preserveObjectReferences;

        public InMemorySecureStore(bool preserveObjectReferences = false)
        {
            _preserveObjectReferences = preserveObjectReferences;
        }

        public Dictionary<string, object?> Data { get; } = new(StringComparer.OrdinalIgnoreCase);

        public async Task<T?> LoadAsync<T>(string key) where T : class
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_preserveObjectReferences)
                {
                    return Data.TryGetValue(key, out var value) ? (T?)value : null;
                }

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
                if (_preserveObjectReferences)
                {
                    Data[key] = value;
                    return;
                }

                var json = System.Text.Json.JsonSerializer.Serialize(value);
                _store[key] = System.Text.Encoding.UTF8.GetBytes(json);
            }
            finally { _gate.Release(); }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try { return _preserveObjectReferences ? Data.ContainsKey(key) : _store.ContainsKey(key); }
            finally { _gate.Release(); }
        }

        public async Task DeleteAsync(string key)
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_preserveObjectReferences) Data.Remove(key);
                else _store.Remove(key);
            }
            finally { _gate.Release(); }
        }
    }
}
