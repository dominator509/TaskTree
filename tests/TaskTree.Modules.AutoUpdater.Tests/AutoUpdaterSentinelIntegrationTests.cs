using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;
using TaskTree.TestSupport;

namespace TaskTree.Modules.AutoUpdater.Tests;

[TestClass]
public sealed class AutoUpdaterSentinelIntegrationTests
{
    private const string TestPublicKeyBase64 = "SmLibYaFoZZTvKbfgIRDpvjWztHTSfLXldqSLQGafyU=";
    private const string TestSignatureBase64 = "8rECRwvb/uFcp/MydWhEReGDGaWqQcEdWSFlwUKsO5kAsiynW9HnqNKhbC+pbq6Knf8Dvyqi2Ug+N2LlrfqLAQ==";

    [TestMethod, TestCategory("Offline")]
    public async Task ApplyAsync_LeavesSentinelAfterInstallerSucceeds()
    {
        var root = TempRoot();
        var packagePath = Path.Combine(root, "TaskTree-1.0.1.msix");
        var payload = Encoding.UTF8.GetBytes("abc");
        await File.WriteAllBytesAsync(packagePath, payload);
        var sentinel = new SentinelService(Path.Combine(root, "sentinel.lock"));
        var installedPath = string.Empty;
        var updater = CreateUpdater(root, sentinel, path =>
        {
            installedPath = path;
            return Task.CompletedTask;
        });

        await updater.ApplyAsync(Manifest(payload));

        Assert.AreEqual(packagePath, installedPath);
        Assert.IsTrue(await sentinel.ExistsAsync());
        Assert.AreEqual("1.0.1", (await sentinel.ReadAsync())!.Version);
    }

    [TestMethod, TestCategory("Offline")]
    public async Task ApplyAsync_ClearsSentinelWhenInstallerFails()
    {
        var root = TempRoot();
        var packagePath = Path.Combine(root, "TaskTree-1.0.1.msix");
        var payload = Encoding.UTF8.GetBytes("abc");
        await File.WriteAllBytesAsync(packagePath, payload);
        var sentinel = new SentinelService(Path.Combine(root, "sentinel.lock"));
        var updater = CreateUpdater(root, sentinel, _ => Task.FromException(new InvalidOperationException("synthetic install failure")));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => updater.ApplyAsync(Manifest(payload)));

        Assert.IsFalse(await sentinel.ExistsAsync());
        Assert.AreEqual(TaskTree.Core.Enums.UpdaterState.Idle, updater.StateMachine.Current);
    }

    [TestMethod, TestCategory("Offline")]
    public async Task ApplyAsync_StateChangedObserverFailure_ResetsStateMachine()
    {
        var root = TempRoot();
        var packagePath = Path.Combine(root, "TaskTree-1.0.1.msix");
        var payload = Encoding.UTF8.GetBytes("abc");
        await File.WriteAllBytesAsync(packagePath, payload);
        var sentinel = new SentinelService(Path.Combine(root, "sentinel.lock"));
        var machine = new UpdaterStateMachine(new FakeClock());
        machine.StateChanged += (_, args) =>
        {
            if (args.Current == UpdaterState.Applied)
                throw new InvalidOperationException("synthetic observer failure");
        };
        var updater = CreateUpdater(root, sentinel, _ => Task.CompletedTask, machine);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => updater.ApplyAsync(Manifest(payload)));

        Assert.AreEqual(UpdaterState.Idle, machine.Current);
    }

    private static AutoUpdater CreateUpdater(string root, SentinelService sentinel, Func<string, Task> installer, UpdaterStateMachine? stateMachine = null) =>
        new(
            new ManifestSigner(TestPublicKeyBase64),
            new HashVerifier(),
            stateMachine ?? new UpdaterStateMachine(new FakeClock()),
            new StagingService(root, new HashVerifier()),
            new VersionEligibilityEvaluator(),
            null,
            root,
            sentinel,
            installer);

    private static UpdateManifest Manifest(byte[] payload) =>
        new(
            "1.0.1",
            UpdateChannel.Stable,
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
            "1.0.0",
            100,
            new UpdatePackageInfo("u", new HashVerifier().ComputeSha256Hex(payload), payload.Length),
            new UpdateSignatureInfo("Ed25519", "k", TestSignatureBase64),
            "n");

    private static string TempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "TaskTreeUpdaterSentinelTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
