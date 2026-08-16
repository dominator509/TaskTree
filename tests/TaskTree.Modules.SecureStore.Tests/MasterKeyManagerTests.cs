// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Modules.SecureStore.Tests/MasterKeyManagerTests.cs
//  Purpose: Constructor + DPAPI lifecycle tests for MasterKeyManager.
//  Architecture.md References: §10.3, P1B-AC4
//  Roadmap.md References: Phase 1B — Msg 2 (tests); Roadmap §1B Codex Handoff Notes (Live tests deferred to Phase 5E)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: all test data is synthetic, non-PHI-shaped.
//  D10 anti-drift: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Modules.SecureStore;

namespace TaskTree.Modules.SecureStore.Tests;

/// <summary>
/// Tests for <see cref="MasterKeyManager"/>. One offline test verifies the
/// constructor contract (no DPAPI invoked). Four <c>[TestCategory("Live")]</c>
/// tests run real DPAPI wrap/unwrap on a Windows session per Roadmap §1B Codex
/// Handoff Notes — to be executed in Phase 5E.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1B: this test class verifies derivations §1, §6, and §7
/// from PHASE1B-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class MasterKeyManagerTests
{
    private sealed class TestPaths : IDisposable
    {
        public string KeyDir { get; }
        public TestPaths()
        {
            KeyDir = Path.Combine(Path.GetTempPath(), "TaskTreeKeyTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(KeyDir);
        }
        public void Dispose()
        {
            try { if (Directory.Exists(KeyDir)) Directory.Delete(KeyDir, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    // ─── Offline: constructor contract (no DPAPI invoked) ─────────────────

    /// <summary>Constructor creates the key directory and exposes KeyFilePath without touching DPAPI.</summary>
    [TestMethod]
    public void Constructor_CreatesDirectoryAndExposesKeyFilePath()
    {
        using var paths = new TestPaths();
        // Pre-delete to confirm the constructor recreates it.
        Directory.Delete(paths.KeyDir, recursive: true);
        Assert.IsFalse(Directory.Exists(paths.KeyDir));

        var logger = new Mock<IAppLogger>();
        var mgr = new MasterKeyManager(paths.KeyDir, logger.Object);

        Assert.IsTrue(Directory.Exists(paths.KeyDir));
        Assert.AreEqual(
            Path.Combine(paths.KeyDir, MasterKeyManager.DefaultKeyFileName),
            mgr.KeyFilePath);
    }

    /// <summary>Key file names cannot escape the injected key directory.</summary>
    [TestMethod]
    public void Constructor_PathTraversalKeyFileName_Throws()
    {
        using var paths = new TestPaths();
        var logger = new Mock<IAppLogger>();

        Assert.ThrowsException<ArgumentException>(() =>
            new MasterKeyManager(paths.KeyDir, logger.Object, Path.Combine(paths.KeyDir, "outside.bin")));
        Assert.ThrowsException<ArgumentException>(() =>
            new MasterKeyManager(paths.KeyDir, logger.Object, "..\\outside.bin"));
    }

    // ─── Live: real DPAPI tests (executed in Phase 5E) ────────────────────

    /// <summary>P1B-AC4 (Live): first GetOrCreateMasterKeyAsync generates and persists a wrapped key file.</summary>
    [TestMethod]
    [TestCategory("Live")]
    public async Task MasterKeyManager_FirstCall_GeneratesAndPersistsKey()
    {
        using var paths = new TestPaths();
        var logger = new Mock<IAppLogger>();
        var mgr = new MasterKeyManager(paths.KeyDir, logger.Object);

        var key = await mgr.GetOrCreateMasterKeyAsync();

        Assert.AreEqual(32, key.Length);
        Assert.IsTrue(File.Exists(mgr.KeyFilePath));
    }

    /// <summary>Derivation 6 (Live): subsequent calls return the cached key without re-reading the file.</summary>
    [TestMethod]
    [TestCategory("Live")]
    public async Task MasterKeyManager_SecondCall_ReturnsSameKey()
    {
        using var paths = new TestPaths();
        var logger = new Mock<IAppLogger>();
        var mgr = new MasterKeyManager(paths.KeyDir, logger.Object);

        var k1 = await mgr.GetOrCreateMasterKeyAsync();
        var k2 = await mgr.GetOrCreateMasterKeyAsync();

        CollectionAssert.AreEqual(k1, k2);
    }

    /// <summary>P1B-AC4 (Live): key persists across MasterKeyManager instances (DPAPI unwrap of the file).</summary>
    [TestMethod]
    [TestCategory("Live")]
    public async Task MasterKeyManager_AcrossInstances_KeyPersists()
    {
        using var paths = new TestPaths();
        var logger = new Mock<IAppLogger>();

        var first = new MasterKeyManager(paths.KeyDir, logger.Object);
        var k1 = await first.GetOrCreateMasterKeyAsync();

        // New instance, same directory — should unwrap the same key.
        var second = new MasterKeyManager(paths.KeyDir, logger.Object);
        var k2 = await second.GetOrCreateMasterKeyAsync();

        CollectionAssert.AreEqual(k1, k2);
    }

    /// <summary>Derivation 6 (Live): cache hit avoids touching the file system after first call.</summary>
    [TestMethod]
    [TestCategory("Live")]
    public async Task MasterKeyManager_CachesAfterFirstUnwrap()
    {
        using var paths = new TestPaths();
        var logger = new Mock<IAppLogger>();
        var mgr = new MasterKeyManager(paths.KeyDir, logger.Object);

        await mgr.GetOrCreateMasterKeyAsync();      // populates cache + creates file
        File.Delete(mgr.KeyFilePath);               // remove file; cache should still serve next call
        var key2 = await mgr.GetOrCreateMasterKeyAsync();

        Assert.AreEqual(32, key2.Length);
        Assert.IsFalse(File.Exists(mgr.KeyFilePath),
            "Cache should have served the second call without recreating the file.");
    }
}
