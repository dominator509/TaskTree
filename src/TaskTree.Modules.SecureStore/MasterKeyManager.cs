// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.SecureStore/MasterKeyManager.cs
//  Purpose: Generate/wrap/cache the AES-256 master key (DPAPI user-scope) per §10.3.
//  Architecture.md References: §10.3, §4.5, §3.3
//  Roadmap.md References: Phase 1B — SecureStore + MasterKeyManager (patched Msg 2: implements IMasterKeyManager)
//  D1 anti-drift: header cites Architecture.md sections.
//  D5 anti-drift: no TODOs; all behavior implemented per PHASE1B-DERIVATIONS.md.
//  D7 anti-drift: no hardcoded paths/secrets — directory is constructor-injected.
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;

namespace TaskTree.Modules.SecureStore;

/// <summary>
/// Manages the per-install AES-256 master key per Architecture §10.3:
/// generated on first run via <see cref="RandomNumberGenerator"/>, wrapped
/// via DPAPI (CurrentUser scope), persisted to disk, and cached in memory
/// for the lifetime of the process.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1B: surface not specified verbatim in Architecture.md;
/// derived from documented usage (§10.3) and approved by human owner on
/// 2026-05-26. See docs/spec-derivations/PHASE1B-DERIVATIONS.md.
/// <para>
/// Derivations applied here:
/// (1) Constructor surface: (keyDirectory, IAppLogger, keyFileName = "master.bin").
/// (6) In-memory cache after first unwrap; SemaphoreSlim(1,1) for concurrent
///     first-call. No purge on idle — auto-logoff (Phase 2F) is responsible
///     for process-state wipe.
/// </para>
/// <para>
/// Patched in Phase 1B Msg 2: now implements <see cref="IMasterKeyManager"/>
/// (promotion path from PHASE1B-DERIVATIONS §1; recorded as §7). Sealing is
/// preserved — <see cref="IMasterKeyManager"/> is the abstraction seam.
/// </para>
/// </remarks>
public sealed class MasterKeyManager : IMasterKeyManager
{
    /// <summary>
    /// Default master-key file name per §10.3
    /// (full path: <c>%LOCALAPPDATA%\TaskTree\keys\master.bin</c>).
    /// </summary>
    public const string DefaultKeyFileName = "master.bin";

    private readonly string _keyDirectory;
    private readonly string _keyFilePath;
    private readonly IAppLogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private byte[]? _cachedKey;

    /// <summary>
    /// Initializes a new <see cref="MasterKeyManager"/>. The key directory is
    /// caller-injected (DI Phase 1F resolves to
    /// <c>%LOCALAPPDATA%\TaskTree\keys\</c> per §10.3) — NOT hardcoded here,
    /// per D7.
    /// </summary>
    /// <param name="keyDirectory">Directory that will contain the wrapped key file.</param>
    /// <param name="logger">Logger for key-lifecycle messages.</param>
    /// <param name="keyFileName">Wrapped-key file name. Defaults to <see cref="DefaultKeyFileName"/>.</param>
    public MasterKeyManager(string keyDirectory, IAppLogger logger, string keyFileName = DefaultKeyFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyFileName);
        if (Path.IsPathRooted(keyFileName) ||
            !string.Equals(Path.GetFileName(keyFileName), keyFileName, StringComparison.Ordinal) ||
            keyFileName is "." or ".." ||
            keyFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException("Key file name must be a single safe file name.", nameof(keyFileName));
        _keyDirectory = keyDirectory;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyFilePath = Path.Combine(_keyDirectory, keyFileName);
        Directory.CreateDirectory(_keyDirectory);
    }

    /// <summary>Gets the absolute path of the wrapped master-key file.</summary>
    public string KeyFilePath => _keyFilePath;

    /// <inheritdoc />
    public async Task<byte[]> GetOrCreateMasterKeyAsync()
    {
        if (_cachedKey is not null) return CopyCachedKey();

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_cachedKey is not null) return CopyCachedKey();   // double-check after lock

            if (File.Exists(_keyFilePath))
            {
                var wrapped = await File.ReadAllBytesAsync(_keyFilePath).ConfigureAwait(false);
                _cachedKey = ProtectedData.Unprotect(wrapped, optionalEntropy: null,
                                                    scope: DataProtectionScope.CurrentUser);
                if (_cachedKey.Length != 32)
                    throw new CryptographicException(
                        $"Master key file at {_keyFilePath} unwrapped to {_cachedKey.Length} bytes (expected 32).");
                _logger.LogInformation("MasterKeyManager loaded existing master key from {0}", _keyFilePath);
                return CopyCachedKey();
            }

            var fresh = new byte[32];
            RandomNumberGenerator.Fill(fresh);
            var wrappedNew = ProtectedData.Protect(fresh, optionalEntropy: null,
                                                   scope: DataProtectionScope.CurrentUser);

            // Atomic write: temp → move
            var tmp = _keyFilePath + ".tmp";
            await File.WriteAllBytesAsync(tmp, wrappedNew).ConfigureAwait(false);
            File.Move(tmp, _keyFilePath, overwrite: true);

            _cachedKey = fresh;
            _logger.LogInformation("MasterKeyManager generated new master key at {0}", _keyFilePath);
            return CopyCachedKey();
        }
        finally
        {
            _gate.Release();
        }
    }

    private byte[] CopyCachedKey() => (byte[])_cachedKey!.Clone();
}
