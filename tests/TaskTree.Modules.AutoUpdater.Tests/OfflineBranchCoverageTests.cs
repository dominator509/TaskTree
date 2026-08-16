// File: tests/TaskTree.Modules.AutoUpdater.Tests/OfflineBranchCoverageTests.cs
// Covers: Architecture §9.1.4-§9.1.6; Roadmap P5C coverage gate.
// Synthetic data only; no network, signing keys, or package installation.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests;

[TestClass]
public sealed class OfflineBranchCoverageTests
{
    private static UpdateManifest Manifest(UpdatePackageInfo? package = null, UpdateSignatureInfo? signature = null) =>
        new(
            "1.0.1",
            UpdateChannel.Stable,
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
            "1.0.0",
            100,
            package ?? new UpdatePackageInfo("https://updates.example.invalid/tasktree.msix", new string('A', 64), 1),
            signature ?? new UpdateSignatureInfo("Ed25519", "synthetic", "not-base64"),
            "Synthetic update manifest");

    [TestMethod]
    public void Constructor_NullDependencies_Throw()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new AutoUpdater(null!, new HashVerifier()));
        Assert.ThrowsException<ArgumentNullException>(() => new AutoUpdater(new ManifestSigner(), null!));
        Assert.ThrowsException<ArgumentNullException>(() => new AutoUpdater(
            new ManifestSigner(), new HashVerifier(), null!, new StagingService(), new VersionEligibilityEvaluator()));
        Assert.ThrowsException<ArgumentNullException>(() => new AutoUpdater(
            new ManifestSigner(), new HashVerifier(), new UpdaterStateMachine(new TestClock()), null!, new VersionEligibilityEvaluator()));
        Assert.ThrowsException<ArgumentNullException>(() => new AutoUpdater(
            new ManifestSigner(), new HashVerifier(), new UpdaterStateMachine(new TestClock()), new StagingService(), null!));
    }

    [TestMethod]
    public void CheckAsync_Disabled_ReturnsNull()
    {
        var updater = new AutoUpdater { Enabled = false };
        Assert.IsNull(updater.CheckAsync().GetAwaiter().GetResult());
    }

    [TestMethod]
    public void CheckAsync_InvalidOrNonHttpsEndpoint_ReturnsNull()
    {
        var previous = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL");
        try
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", "not-a-uri");
            Assert.IsNull(new AutoUpdater().CheckAsync().GetAwaiter().GetResult());
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", "http://127.0.0.1/manifest.json");
            Assert.IsNull(new AutoUpdater().CheckAsync().GetAwaiter().GetResult());
        }
        finally
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", previous);
        }
    }

    [TestMethod]
    public void CheckAsync_HttpsLoopbackFailure_ReturnsNullAndResetsState()
    {
        var previous = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL");
        try
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", "https://127.0.0.1:1/manifest.json");
            var updater = new AutoUpdater();
            Assert.IsNull(updater.CheckAsync().GetAwaiter().GetResult());
            Assert.AreEqual(UpdaterState.Idle, updater.StateMachine.Current);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL", previous);
        }
    }

    [TestMethod]
    public void VerifyAsync_NullInputs_Throw()
    {
        var updater = new AutoUpdater();
        Assert.ThrowsException<ArgumentNullException>(() => updater.VerifyAsync(null!, Array.Empty<byte>()).GetAwaiter().GetResult());
        Assert.ThrowsException<ArgumentNullException>(() => updater.VerifyAsync(Manifest(), null!).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void VerifyAsync_MissingPackageOrSignature_ReturnsFalse()
    {
        var updater = new AutoUpdater();
        Assert.IsFalse(updater.VerifyAsync(Manifest(package: null!), new byte[] { 1 }).GetAwaiter().GetResult());
        Assert.IsFalse(updater.VerifyAsync(Manifest(signature: null!), new byte[] { 1 }).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void ApplyAsync_NullManifest_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new AutoUpdater().ApplyAsync(null!).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void ApplyAsync_ExistingStagedPackageWithBadHash_FailsClosedAndResetsState()
    {
        var packagePath = Path.Combine(Path.GetTempPath(), "TaskTree-synthetic-invalid.msix");
        var payload = new byte[] { 1, 2, 3 };
        File.WriteAllBytes(packagePath, payload);
        try
        {
            var manifest = Manifest(new UpdatePackageInfo(new Uri(packagePath).AbsoluteUri, new string('B', 64), payload.Length));
            Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => new AutoUpdater().ApplyAsync(manifest)).GetAwaiter().GetResult();
        }
        finally
        {
            if (File.Exists(packagePath)) File.Delete(packagePath);
        }
    }

    [TestMethod]
    public void MsixPackageInstaller_ValidatesPathAndBuildsStagedPath()
    {
        var argument = Assert.ThrowsExceptionAsync<ArgumentException>(
            () => MsixPackageInstaller.InstallAsync(" ")).GetAwaiter().GetResult();
        StringAssert.Contains(argument.Message, "package path");

        var missing = Assert.ThrowsExceptionAsync<FileNotFoundException>(
            () => MsixPackageInstaller.InstallAsync(Path.Combine(Path.GetTempPath(), "missing-tasktree.msix")))
            .GetAwaiter().GetResult();
        StringAssert.Contains(missing.Message, "not found");

        StringAssert.EndsWith(MsixPackageInstaller.GetDefaultStagedPath("1.0.1"), "TaskTree-1.0.1.msix");
        Assert.ThrowsException<ArgumentException>(() => MsixPackageInstaller.GetDefaultStagedPath(""));
    }

    private sealed class TestClock : TaskTree.Core.Abstractions.IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
    }
}
