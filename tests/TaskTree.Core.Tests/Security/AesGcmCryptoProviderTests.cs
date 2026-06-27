// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Core.Tests/Security/AesGcmCryptoProviderTests.cs
//  Purpose: Verifies AES-256-GCM round-trip and tamper-detection per Architecture §10.3 / §10.7 and Roadmap P0-AC3.
//  Architecture.md References: §10.3, §10.7, P0-AC3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 6 — Test Skeletons + 5 Primitive Tests)
//  D1: header cites Architecture.md sections.
//  D6: all test data is synthetic, non-PHI-shaped.
//  D10: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Security;

namespace TaskTree.Core.Tests.Security;

/// <summary>
/// Verifies AES-256-GCM round-trip integrity and that any tampering with the
/// ciphertext is detected at decrypt time.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG6: test naming convention (MethodName_Scenario_ExpectedOutcome)
/// and test category vocabulary (Offline default / Live / Integration) not
/// specified verbatim in Architecture.md; derived from documented usage and
/// approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG6-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class AesGcmCryptoProviderTests
{
    /// <summary>Encrypt → Decrypt round-trips the original plaintext byte-for-byte (P0-AC3).</summary>
    [TestMethod]
    public void Encrypt_Decrypt_RoundTripsPlaintext()
    {
        var provider = new AesGcmCryptoProvider();
        var key = new byte[AesGcmCryptoProvider.KeySize];
        RandomNumberGenerator.Fill(key);
        var plaintext = Encoding.UTF8.GetBytes("synthetic-payload-msg6-roundtrip");

        var encrypted = provider.Encrypt(plaintext, key);
        var decrypted = provider.Decrypt(encrypted, key);

        CollectionAssert.AreEqual(plaintext, decrypted);
    }

    /// <summary>Tampered ciphertext byte → Decrypt throws CryptographicException per §10.7 integrity controls.</summary>
    [TestMethod]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        var provider = new AesGcmCryptoProvider();
        var key = new byte[AesGcmCryptoProvider.KeySize];
        RandomNumberGenerator.Fill(key);
        var plaintext = Encoding.UTF8.GetBytes("synthetic-payload-msg6-tamper");
        var encrypted = provider.Encrypt(plaintext, key);

        // Flip one bit inside the ciphertext middle (after nonce, before tag).
        encrypted[AesGcmCryptoProvider.NonceSize + 1] ^= 0x01;

        Assert.ThrowsException<CryptographicException>(() => provider.Decrypt(encrypted, key));
    }
}
