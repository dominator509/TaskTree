using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests;

[TestClass]
public sealed class AutoUpdaterEligibilityTests
{
    [TestMethod]
    public async Task ApplyAsync_RejectsOlderManifestBeforeInstaller()
    {
        var previousVersion = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION");
        var root = Path.Combine(Path.GetTempPath(), "TaskTreeUpdaterEligibility", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION", "1.0.0");
            var packagePath = Path.Combine(root, "older.msix");
            var payload = new byte[] { 1, 2, 3 };
            File.WriteAllBytes(packagePath, payload);
            var manifest = Manifest("0.9.0", UpdateChannel.Stable, "0.1.0", packagePath, payload);
            var updater = new AutoUpdater(
                new ManifestSigner(),
                new HashVerifier(),
                new UpdaterStateMachine(new TestClock()),
                new StagingService(root, new HashVerifier()),
                new VersionEligibilityEvaluator(),
                stagingRoot: root);

            var error = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => updater.ApplyAsync(manifest));

            StringAssert.Contains(error.Message, "not eligible");
            Assert.AreEqual(UpdaterState.Idle, updater.StateMachine.Current);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION", previousVersion);
            TryDelete(root);
        }
    }

    [TestMethod]
    public async Task ApplyAsync_RejectsWrongChannelBeforeInstaller()
    {
        var previousVersion = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION");
        try
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION", "1.0.0");
            var manifest = Manifest("1.1.0", UpdateChannel.Beta, "1.0.0", Path.Combine(Path.GetTempPath(), "missing.msix"), Array.Empty<byte>());
            var updater = new AutoUpdater();

            var error = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => updater.ApplyAsync(manifest));

            StringAssert.Contains(error.Message, "not eligible");
            Assert.AreEqual(UpdaterState.Idle, updater.StateMachine.Current);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION", previousVersion);
        }
    }

    private static UpdateManifest Manifest(string version, UpdateChannel channel, string minimumVersion, string packagePath, byte[] payload) =>
        new(
            version,
            channel,
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
            minimumVersion,
            100,
            new UpdatePackageInfo(new Uri(packagePath).AbsoluteUri, new HashVerifier().ComputeSha256Hex(payload), payload.Length),
            new UpdateSignatureInfo("Ed25519", "synthetic", "not-base64"),
            "Synthetic update manifest");

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch { }
    }

    private sealed class TestClock : TaskTree.Core.Abstractions.IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
    }
}
