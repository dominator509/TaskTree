// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/ISecureStore.cs
//  Purpose: Encrypted local persistence (AES-256-GCM JSON) per Architecture §4.5, §10.3.
//  Architecture.md References: §4.5, §10.3, §10.7
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System.Threading.Tasks;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Encrypted key/value persistence layer. All payloads are serialized to JSON
/// and protected at rest with AES-256-GCM using a DPAPI-wrapped master key per
/// Architecture.md §10.3.
/// </summary>
/// <remarks>
/// Every read MUST verify the GCM authentication tag (§10.7). A failed verify
/// MUST throw <see cref="System.Security.Cryptography.CryptographicException"/>.
/// </remarks>
public interface ISecureStore
{
    /// <summary>Loads and decrypts the payload stored under <paramref name="key"/>.</summary>
    /// <typeparam name="T">Concrete reference type to deserialize into.</typeparam>
    /// <param name="key">Storage key (caller-defined).</param>
    /// <returns>The deserialized value, or <c>null</c> if no record exists.</returns>
    Task<T?> LoadAsync<T>(string key) where T : class;

    /// <summary>Serializes and encrypts <paramref name="value"/> under <paramref name="key"/>.</summary>
    /// <typeparam name="T">Concrete reference type.</typeparam>
    /// <param name="key">Storage key (caller-defined).</param>
    /// <param name="value">The value to persist.</param>
    Task SaveAsync<T>(string key, T value) where T : class;

    /// <summary>Returns whether a record exists for <paramref name="key"/>.</summary>
    /// <param name="key">Storage key (caller-defined).</param>
    Task<bool> ExistsAsync(string key);

    /// <summary>Removes the record (including its integrity tag) for <paramref name="key"/>.</summary>
    /// <param name="key">Storage key (caller-defined).</param>
    Task DeleteAsync(string key);
}
