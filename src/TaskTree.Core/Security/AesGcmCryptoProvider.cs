// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Security/AesGcmCryptoProvider.cs
//  Purpose: AES-256-GCM authenticated encryption per Architecture §10.3 / §10.7; implements ICryptoProvider (Msg 2).
//  Architecture.md References: §10.3, §10.7, §3.3, §4.5
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 5 — Security + Logging Primitives)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: behavior matches Architecture.md verbatim where specified (or SPEC-DERIVED-MSG5).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Security.Cryptography;
using TaskTree.Core.Abstractions;

namespace TaskTree.Core.Security;

/// <summary>
/// AES-256-GCM authenticated-encryption primitive implementing
/// <see cref="ICryptoProvider"/>.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG5: implementation details (nonce source, key-length guard,
/// tag-length constant) not specified verbatim in Architecture.md; derived
/// from documented usage and approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG5-DERIVATIONS.md.
/// <para>
/// Wire format produced by <see cref="Encrypt"/> and consumed by
/// <see cref="Decrypt"/> (NORMATIVE per ICryptoProvider registry
/// PHASE0-MSG2-DERIVATIONS §2): 12-byte nonce ‖ ciphertext ‖ 16-byte GCM tag,
/// packed contiguously into a single <c>byte[]</c>. Nonce is generated per
/// call from <see cref="RandomNumberGenerator.Fill(Span{byte})"/> — never
/// reused. Key MUST be exactly 32 bytes (AES-256); shorter or longer keys
/// throw <see cref="ArgumentException"/>. Tag length is 16 bytes
/// (<see cref="AesGcm.TagByteSizes"/>.MaxSize).
/// </para>
/// </remarks>
public sealed class AesGcmCryptoProvider : ICryptoProvider
{
    /// <summary>AES-GCM standard nonce size in bytes.</summary>
    public const int NonceSize = 12;

    /// <summary>AES-GCM tag size in bytes (max per .NET AesGcm).</summary>
    public const int TagSize = 16;

    /// <summary>AES-256 key size in bytes.</summary>
    public const int KeySize = 32;

    /// <summary>Initializes a new AES-256-GCM crypto provider.</summary>
    public AesGcmCryptoProvider() { }

    /// <summary>
    /// Encrypts <paramref name="plaintext"/> with AES-256-GCM per §10.3,
    /// producing the normative wire format <c>nonce ‖ ciphertext ‖ tag</c>.
    /// </summary>
    /// <param name="plaintext">Raw bytes to encrypt.</param>
    /// <param name="key">32-byte AES-256 key.</param>
    /// <param name="associatedData">Optional associated data bound into the GCM tag (integrity only).</param>
    /// <returns>Single <c>byte[]</c> in the normative wire format.</returns>
    public byte[] Encrypt(byte[] plaintext, byte[] key, byte[]? associatedData = null)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be exactly {KeySize} bytes for AES-256 (got {key.Length}).", nameof(key));

        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

        // Pack: nonce ‖ ciphertext ‖ tag
        var output = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce,      0, output, 0,                              NonceSize);
        Buffer.BlockCopy(ciphertext, 0, output, NonceSize,                      ciphertext.Length);
        Buffer.BlockCopy(tag,        0, output, NonceSize + ciphertext.Length,  TagSize);
        return output;
    }

    /// <summary>
    /// Decrypts a payload previously produced by <see cref="Encrypt"/>,
    /// verifying the GCM tag per §10.7 integrity controls.
    /// </summary>
    /// <param name="ciphertextWithNonceAndTag">Wire-format payload (<c>nonce ‖ ciphertext ‖ tag</c>).</param>
    /// <param name="key">32-byte AES-256 key.</param>
    /// <param name="associatedData">Associated data supplied at encryption time (must match exactly).</param>
    /// <returns>The recovered plaintext.</returns>
    /// <exception cref="CryptographicException">Thrown by <see cref="AesGcm.Decrypt"/> on tag mismatch.</exception>
    public byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key, byte[]? associatedData = null)
    {
        ArgumentNullException.ThrowIfNull(ciphertextWithNonceAndTag);
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != KeySize)
            throw new ArgumentException($"Key must be exactly {KeySize} bytes for AES-256 (got {key.Length}).", nameof(key));
        if (ciphertextWithNonceAndTag.Length < NonceSize + TagSize)
            throw new ArgumentException("Input too short to contain nonce + tag.", nameof(ciphertextWithNonceAndTag));

        int ctLen = ciphertextWithNonceAndTag.Length - NonceSize - TagSize;
        var nonce = new byte[NonceSize];
        var ciphertext = new byte[ctLen];
        var tag = new byte[TagSize];
        Buffer.BlockCopy(ciphertextWithNonceAndTag, 0,                 nonce,      0, NonceSize);
        Buffer.BlockCopy(ciphertextWithNonceAndTag, NonceSize,         ciphertext, 0, ctLen);
        Buffer.BlockCopy(ciphertextWithNonceAndTag, NonceSize + ctLen, tag,        0, TagSize);

        var plaintext = new byte[ctLen];
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
        return plaintext;
    }
}
