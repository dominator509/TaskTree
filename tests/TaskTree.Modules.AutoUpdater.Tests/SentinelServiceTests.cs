// SPEC-DERIVED-PHASE3C  HALT #22
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests;

[TestClass]
public sealed class SentinelServiceTests
{
    private static string PathForTest() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "sentinel.lock");

    private static UpdateManifest Manifest() => new(
        "1.0.1",
        UpdateChannel.Stable,
        new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
        "1.0.0",
        100,
        new UpdatePackageInfo("u", new string('A', 64), 1),
        new UpdateSignatureInfo("Ed25519", "k", "s"),
        "n");

    [TestMethod, TestCategory("Offline")]
    public async Task ExistsAsync_NoFile_ReturnsFalse() => Assert.IsFalse(await new SentinelService(PathForTest()).ExistsAsync());

    [TestMethod, TestCategory("Offline")]
    public async Task CreateAsync_WritesSentinel()
    {
        var path = PathForTest();
        await new SentinelService(path).CreateAsync(Manifest());
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod, TestCategory("Offline")]
    public async Task ExistsAsync_AfterCreate_ReturnsTrue()
    {
        var service = new SentinelService(PathForTest());
        await service.CreateAsync(Manifest());
        Assert.IsTrue(await service.ExistsAsync());
    }

    [TestMethod, TestCategory("Offline")]
    public async Task ReadAsync_AfterCreate_ReturnsManifest()
    {
        var service = new SentinelService(PathForTest());
        await service.CreateAsync(Manifest());
        Assert.AreEqual("1.0.1", (await service.ReadAsync())!.Version);
    }

    [TestMethod, TestCategory("Offline")]
    public async Task TryMarkLaunchAttemptAsync_FirstAttemptReturnsTrue_SecondReturnsFalse()
    {
        var service = new SentinelService(PathForTest());
        await service.CreateAsync(Manifest());
        Assert.IsTrue(await service.TryMarkLaunchAttemptAsync());
        Assert.IsFalse(await service.TryMarkLaunchAttemptAsync());
    }

    [TestMethod, TestCategory("Offline")]
    public async Task ClearAsync_RemovesSentinelAndLaunchAttempt()
    {
        var path = PathForTest();
        var service = new SentinelService(path);
        await service.CreateAsync(Manifest());
        Assert.IsTrue(await service.TryMarkLaunchAttemptAsync());
        await service.ClearAsync();
        Assert.IsFalse(await service.ExistsAsync());
        Assert.IsFalse(File.Exists(path + ".started"));
    }

    [TestMethod, TestCategory("Offline")]
    public async Task ClearAsync_NoFile_IsNoOp() => await new SentinelService(PathForTest()).ClearAsync();

    [TestMethod, TestCategory("Offline")]
    public async Task CreateAsync_ReplacesWithoutTemporaryArtifacts()
    {
        var path = PathForTest();
        var service = new SentinelService(path);
        await service.CreateAsync(Manifest());
        await service.CreateAsync(Manifest());
        var files = Directory.GetFiles(Path.GetDirectoryName(path)!);
        Assert.AreEqual(1, files.Length);
        Assert.AreEqual(path, files[0]);
    }
}
