// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.SecureStore/SecureStore.cs
//  Purpose: Encrypted per-key local persistence implementing ISecureStore per §4.5 / §10.3 / §10.7.
//  Architecture.md References: §4.5, §10.3, §10.7, §3.3
//  Roadmap.md References: Phase 1B — SecureStore + MasterKeyManager (patched Msg 2: depends on IMasterKeyManager)
//  D1 anti-drift: header cites Architecture.md sections.
//  D5 anti-drift: no TODOs; all behavior implemented per PHASE1B-DERIVATIONS.md.
//  D7 anti-drift: no hardcoded paths — storageDirectory is constructor-injected.
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;

namespace TaskTree.Modules.SecureStore;

/// <summary>
/// Encrypted per-key local persistence implementing <see cref="ISecureStore"/>
/// per Architecture §4.5 / §10.3 / §10.7.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1B: surface not specified verbatim in Architecture.md;
/// derived from documented usage (§4.5 / §10.3) and approved by human owner
/// on 2026-05-26. See docs/spec-derivations/PHASE1B-DERIVATIONS.md.
/// <para>
/// Derivations applied here:
/// (2) One file per key under <see cref="StorageDirectory"/>; keys containing
///     '/' are sanitized via '/'→'__'. File contents = AesGcm wire format
///     <c>nonce ‖ ciphertext ‖ tag</c> from <see cref="ICryptoProvider"/>
///     (PHASE0-MSG2-DERIVATIONS §2). Plaintext = <see cref="JsonSerializer"/>
///     default-options serialization.
/// (3) Constructor (storageDirectory, IMasterKeyManager, ICryptoProvider, IAppLogger).
///     Directory created on construction; not hardcoded.
/// (4) <see cref="JsonSerializer"/> default options; converters added globally
///     in Phase 1F composition root if needed.
/// (5) Single <see cref="SemaphoreSlim"/>(1,1) gate guarding all operations.
/// </para>
/// <para>
/// Patched in Phase 1B Msg 2: constructor parameter type changed from concrete
/// <c>MasterKeyManager</c> to <see cref="IMasterKeyManager"/> per
/// PHASE1B-DERIVATIONS §7. Promotion preserves Derivation 3's design intent —
/// the SecureStore is still constructed with a key provider; only the type is
/// now an interface so that the store can be tested in isolation from DPAPI.
/// </para>
/// </remarks>
public sealed class SecureStore : ISecureStore
{
    /// <summary>File extension used for every stored value's encrypted blob.</summary>
    public const string FileExtension = ".bin";

    /// <summary>Sanitizer replacement for the forward-slash character in storage keys.</summary>
    public const string SlashReplacement = "__";

    private readonly string _storageDirectory;
    private readonly IMasterKeyManager _keyManager;
    private readonly ICryptoProvider _crypto;
    private readonly IAppLogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Initializes a new <see cref="SecureStore"/>. The storage directory is
    /// caller-injected (DI Phase 1F resolves to
    /// <c>%LOCALAPPDATA%\TaskTree\store\</c>) — NOT hardcoded here, per D7.
    /// </summary>
    /// <param name="storageDirectory">Directory that will contain per-key encrypted files.</param>
    /// <param name="keyManager">Master-key provider (<see cref="IMasterKeyManager"/>).</param>
    /// <param name="crypto">AES-256-GCM provider (Phase 0 Msg 5).</param>
    /// <param name="logger">Logger for store activity.</param>
    public SecureStore(string storageDirectory, IMasterKeyManager keyManager, ICryptoProvider crypto, IAppLogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageDirectory);
        _storageDirectory = storageDirectory;
        _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));
        _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Directory.CreateDirectory(_storageDirectory);
    }

    /// <summary>Gets the absolute path of the storage directory.</summary>
    public string StorageDirectory => _storageDirectory;

    private static string SanitizeKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        // Known keys use '/' as a logical separator. Normalize both Windows
        // separator forms before validating the resulting single-segment name.
        var sanitized = key
            .Replace("/", SlashReplacement, StringComparison.Ordinal)
            .Replace("\\", SlashReplacement, StringComparison.Ordinal);

        if (sanitized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Storage key contains invalid filename characters.", nameof(key));

        return sanitized;
    }

    private string FilePathFor(string key) =>
        Path.Combine(_storageDirectory, SanitizeKey(key) + FileExtension);

    /// <inheritdoc />
    public async Task<T?> LoadAsync<T>(string key) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var path = FilePathFor(key);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!File.Exists(path)) return null;

            var blob = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            var masterKey = await _keyManager.GetOrCreateMasterKeyAsync().ConfigureAwait(false);
            // Decrypt throws CryptographicException on tag mismatch per §10.7.
            var plaintext = _crypto.Decrypt(blob, masterKey);
            var json = Encoding.UTF8.GetString(plaintext);
            return JsonSerializer.Deserialize<T>(json);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync<T>(string key, T value) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        var path = FilePathFor(key);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = JsonSerializer.Serialize(value);
            var plaintext = Encoding.UTF8.GetBytes(json);
            var masterKey = await _keyManager.GetOrCreateMasterKeyAsync().ConfigureAwait(false);
            var blob = _crypto.Encrypt(plaintext, masterKey);

            var tmp = path + ".tmp";
            await File.WriteAllBytesAsync(tmp, blob).ConfigureAwait(false);
            File.Move(tmp, path, overwrite: true);
            _logger.LogInformation("SecureStore wrote key {0} ({1} bytes)", key, blob.Length);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            return File.Exists(FilePathFor(key));
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var path = FilePathFor(key);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                _logger.LogInformation("SecureStore deleted key {0}", key);
            }
        }
        finally
        {
            _gate.Release();
        }
    }
}
