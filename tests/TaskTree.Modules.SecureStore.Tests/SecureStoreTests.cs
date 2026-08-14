// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Modules.SecureStore.Tests/SecureStoreTests.cs
//  Purpose: Offline unit tests for SecureStore (real AesGcmCryptoProvider + Moq IMasterKeyManager).
//  Architecture.md References: §4.5, §10.7
//  Roadmap.md References: Phase 1B — Msg 2 (tests)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: all test data is synthetic, non-PHI-shaped.
//  D10 anti-drift: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Security;
using TaskTree.Modules.SecureStore;

namespace TaskTree.Modules.SecureStore.Tests;

/// <summary>
/// Offline tests for <see cref="SecureStore"/>: round-trip, tamper detection,
/// delete, exists, slash sanitization, atomic write semantics, and
/// missing-key null return. Uses real <see cref="AesGcmCryptoProvider"/> and
/// a Moq-mocked <see cref="IMasterKeyManager"/> to avoid DPAPI dependency.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1B: this test class inherits the 6 PHASE1B derivations
/// plus the §7 IMasterKeyManager promotion. See
/// docs/spec-derivations/PHASE1B-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class SecureStoreTests
{
    private sealed class TestPaths : IDisposable
    {
        public string StorageDir { get; }
        public TestPaths()
        {
            StorageDir = Path.Combine(Path.GetTempPath(), "TaskTreeStoreTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(StorageDir);
        }
        public void Dispose()
        {
            try { if (Directory.Exists(StorageDir)) Directory.Delete(StorageDir, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    private sealed class Harness
    {
        public SecureStore Store { get; init; } = default!;
        public TestPaths Paths { get; init; } = default!;
    }

    private static Harness CreateStore()
    {
        var paths = new TestPaths();
        var key = new byte[AesGcmCryptoProvider.KeySize];
        RandomNumberGenerator.Fill(key);
        var mockKey = new Mock<IMasterKeyManager>();
        mockKey.Setup(m => m.GetOrCreateMasterKeyAsync()).ReturnsAsync(key);
        var mockLog = new Mock<IAppLogger>();
        var crypto = new AesGcmCryptoProvider();
        var store = new SecureStore(paths.StorageDir, mockKey.Object, crypto, mockLog.Object);
        return new Harness { Store = store, Paths = paths };
    }

    /// <summary>Test fixture for round-trip serialization.</summary>
    public sealed class SamplePayload
    {
        /// <summary>Sample title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Sample integer.</summary>
        public int Count { get; set; }
    }

    /// <summary>P1B-AC1: LoadAsync returns null for a missing key.</summary>
    [TestMethod]
    public async Task LoadAsync_MissingKey_ReturnsNull()
    {
        var h = CreateStore();
        using var __ = h.Paths;

        var result = await h.Store.LoadAsync<SamplePayload>("does-not-exist");

        Assert.IsNull(result);
    }

    /// <summary>P1B-AC2: SaveAsync → LoadAsync round-trips the payload.</summary>
    [TestMethod]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsPayload()
    {
        var h = CreateStore();
        using var __ = h.Paths;
        var payload = new SamplePayload { Title = "synthetic-payload", Count = 42 };

        await h.Store.SaveAsync("synthetic-key", payload);
        var loaded = await h.Store.LoadAsync<SamplePayload>("synthetic-key");

        Assert.IsNotNull(loaded);
        Assert.AreEqual("synthetic-payload", loaded!.Title);
        Assert.AreEqual(42, loaded.Count);
    }

    /// <summary>P1B-AC3: Tampered ciphertext → LoadAsync throws the .NET 8 GCM authentication exception per §10.7.</summary>
    [TestMethod]
    public async Task LoadAsync_TamperedCiphertext_ThrowsCryptographicException()
    {
        var h = CreateStore();
        using var __ = h.Paths;
        await h.Store.SaveAsync("synthetic-tamper-key", new SamplePayload { Title = "x", Count = 1 });

        // Tamper: flip a byte inside the ciphertext middle.
        var filePath = Path.Combine(h.Paths.StorageDir, "synthetic-tamper-key.bin");
        var blob = File.ReadAllBytes(filePath);
        blob[AesGcmCryptoProvider.NonceSize + 1] ^= 0x01;
        File.WriteAllBytes(filePath, blob);

        await Assert.ThrowsExceptionAsync<AuthenticationTagMismatchException>(
            async () => await h.Store.LoadAsync<SamplePayload>("synthetic-tamper-key"));
    }

    /// <summary>P1B-AC5: DeleteAsync removes the on-disk file.</summary>
    [TestMethod]
    public async Task DeleteAsync_RemovesFile()
    {
        var h = CreateStore();
        using var __ = h.Paths;
        await h.Store.SaveAsync("synthetic-delete-key", new SamplePayload());
        Assert.IsTrue(await h.Store.ExistsAsync("synthetic-delete-key"));

        await h.Store.DeleteAsync("synthetic-delete-key");

        Assert.IsFalse(await h.Store.ExistsAsync("synthetic-delete-key"));
    }

    /// <summary>§4.5: ExistsAsync reflects file presence.</summary>
    [TestMethod]
    public async Task ExistsAsync_ReflectsFilePresence()
    {
        var h = CreateStore();
        using var __ = h.Paths;

        Assert.IsFalse(await h.Store.ExistsAsync("synthetic-exists-key"));
        await h.Store.SaveAsync("synthetic-exists-key", new SamplePayload());
        Assert.IsTrue(await h.Store.ExistsAsync("synthetic-exists-key"));
    }

    /// <summary>Derivation 2: Keys with '/' map to '__'-sanitized filenames.</summary>
    [TestMethod]
    public async Task SaveAsync_KeyWithSlash_WritesSanitizedFileName()
    {
        var h = CreateStore();
        using var __ = h.Paths;

        await h.Store.SaveAsync("synthetic/with/slash", new SamplePayload());

        var expected = Path.Combine(h.Paths.StorageDir, "synthetic__with__slash.bin");
        Assert.IsTrue(File.Exists(expected),
            $"Expected sanitized file '{expected}' but it was not created.");
    }

    /// <summary>Derivation 2: Atomic-write contract — the temp file does not remain after SaveAsync.</summary>
    [TestMethod]
    public async Task SaveAsync_WritesAtomically_NoTempFileRemains()
    {
        var h = CreateStore();
        using var __ = h.Paths;

        await h.Store.SaveAsync("synthetic-atomic-key", new SamplePayload());

        var finalPath = Path.Combine(h.Paths.StorageDir, "synthetic-atomic-key.bin");
        var tmpPath = finalPath + ".tmp";
        Assert.IsTrue(File.Exists(finalPath));
        Assert.IsFalse(File.Exists(tmpPath),
            "Temp file should be moved (renamed) atomically; nothing should remain at .tmp.");
    }
}
