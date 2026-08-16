using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Modules.TrayHost;
using TaskTree.TestSupport;

namespace TaskTree.Modules.TrayHost.Tests;

[TestClass]
public sealed class HotkeyManagerConcurrencyTests
{
    [TestMethod]
    public async Task ConcurrentSetConfigAsync_ExecutesPersistenceSerially()
    {
        var store = new BlockingSecureStore();
        var compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
        compliance.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);
        var manager = new HotkeyManager(
            new Mock<IAppLogger>().Object,
            compliance.Object,
            store,
            new FakeClock(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero)));
        try
        {
            var first = manager.SetConfigAsync(new HotkeyConfig(true, false, false, false, 0x41));
            await store.FirstLoadStarted.Task;
            var second = manager.SetConfigAsync(new HotkeyConfig(false, true, false, false, 0x42));

            await Task.Delay(50);
            Assert.IsFalse(second.IsCompleted);

            store.ReleaseFirstLoad.TrySetResult(true);
            await Task.WhenAll(first, second);
            Assert.AreEqual(1, store.MaximumConcurrentLoads);
        }
        finally
        {
            manager.Dispose();
        }
    }

    private sealed class BlockingSecureStore : ISecureStore
    {
        private readonly Dictionary<string, object?> _values = new(StringComparer.OrdinalIgnoreCase);
        private int _activeLoads;
        private int _loadCount;

        public TaskCompletionSource<bool> FirstLoadStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseFirstLoad { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int MaximumConcurrentLoads { get; private set; }

        public async Task<T?> LoadAsync<T>(string key) where T : class
        {
            var active = Interlocked.Increment(ref _activeLoads);
            MaximumConcurrentLoads = Math.Max(MaximumConcurrentLoads, active);
            try
            {
                if (Interlocked.Increment(ref _loadCount) == 1)
                {
                    FirstLoadStarted.TrySetResult(true);
                    await ReleaseFirstLoad.Task.ConfigureAwait(false);
                }

                return _values.TryGetValue(key, out var value) ? value as T : null;
            }
            finally
            {
                Interlocked.Decrement(ref _activeLoads);
            }
        }

        public Task SaveAsync<T>(string key, T value) where T : class
        {
            _values[key] = value;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key) => Task.FromResult(_values.ContainsKey(key));

        public Task DeleteAsync(string key)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }
    }
}
