using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests;

[TestClass]
public sealed class UpdaterOperationConcurrencyTests
{
    [TestMethod]
    public async Task ConcurrentChecks_AreSerializedAroundStateMachine()
    {
        var previousEndpoint = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL");
        try
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", "https://127.0.0.1:1/tasktree.json");
            var updater = new AutoUpdater();
            var firstCheckStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseFirstCheck = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstTransition = 0;

            updater.StateMachine.StateChanged += (_, args) =>
            {
                if (args.Current != UpdaterState.Checking || Interlocked.Exchange(ref firstTransition, 1) != 0)
                    return;

                firstCheckStarted.TrySetResult(true);
                releaseFirstCheck.Task.GetAwaiter().GetResult();
            };

            var firstCheck = Task.Run(() => updater.CheckAsync());
            await firstCheckStarted.Task;

            var secondCheck = Task.Run(() => updater.CheckAsync());
            await Task.Delay(50);
            Assert.IsFalse(secondCheck.IsCompleted);

            releaseFirstCheck.TrySetResult(true);
            await Task.WhenAll(firstCheck, secondCheck);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", previousEndpoint);
        }
    }
}
