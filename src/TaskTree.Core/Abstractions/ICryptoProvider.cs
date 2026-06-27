// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/ICryptoProvider.cs
//  Purpose: AES-256-GCM authenticated encryption primitive per Architecture §10.3 / §10.7. Concrete: AesGcmCryptoProvider (Msg 5).
//  Architecture.md References: §10.3, §10.7, §3.3
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Authenticated symmetric encryption primitive backing
/// <c>AesGcmCryptoProvider</c> (Phase 0 Msg 5) and <see cref="ISecureStore"/>.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG2: surface not specified verbatim in Architecture.md; derived from
/// documented usage and approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG2-DERIVATIONS.md.
/// <para>
/// Wire format produced by <see cref="Encrypt(byte[], byte[], byte[])"/> and
/// consumed by <see cref="Decrypt(byte[], byte[], byte[])"/>: 12-byte nonce ‖
/// ciphertext ‖ 16-byte GCM tag, packed contiguously into a single
/// <c>byte[]</c>. Single-blob output keeps <see cref="ISecureStore"/> writes
/// atomic per §10.7.
/// </para>
/// </remarks>
public interface ICryptoProvider
{
    /// <summary>
    /// Encrypts <paramref name="plaintext"/> with AES-256-GCM, generating a fresh
    /// 12-byte nonce per call.
    /// </summary>
    /// <param name="plaintext">Raw bytes to encrypt.</param>
    /// <param name="key">32-byte AES-256 key.</param>
    /// <param name="associatedData">
    /// Optional associated data bound into the GCM tag (not encrypted; integrity
    /// only). May be <c>null</c>.
    /// </param>
    /// <returns>A single byte[] containing <c>nonce ‖ ciphertext ‖ tag</c>.</returns>
    byte[] Encrypt(byte[] plaintext, byte[] key, byte[]? associatedData = null);

    /// <summary>
    /// Decrypts a payload previously produced by
    /// <see cref="Encrypt(byte[], byte[], byte[])"/>, verifying the GCM tag.
    /// </summary>
    /// <param name="ciphertextWithNonceAndTag">
    /// Single byte[] in <c>nonce ‖ ciphertext ‖ tag</c> layout.
    /// </param>
    /// <param name="key">32-byte AES-256 key.</param>
    /// <param name="associatedData">
    /// Associated data supplied at encryption time (must match exactly).
    /// </param>
    /// <returns>The recovered plaintext.</returns>
    byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key, byte[]? associatedData = null);
}
