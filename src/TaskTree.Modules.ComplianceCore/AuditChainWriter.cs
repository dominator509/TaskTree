// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.ComplianceCore/AuditChainWriter.cs
//  Purpose: SHA-256 hash-chained audit log writer per Architecture §10.5 / §10.7; consumed by ComplianceCore.
//  Architecture.md References: §10.5, §10.7, §4.6, §4.5
//  Roadmap.md References: Phase 1C — ComplianceCore baseline (Msg 3 of 5)
//  D1 anti-drift: header cites Architecture.md sections.
//  D5 anti-drift: no TODOs; all behavior implemented per PHASE1C-DERIVATIONS.md §2-§5.
//  D7 anti-drift: no hardcoded paths — storage key is documented constant; SecureStore is constructor-injected.
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Core.Security;

namespace TaskTree.Modules.ComplianceCore;

/// <summary>
/// SHA-256 hash-chained audit log writer per Architecture §10.5 + §10.7.
/// Persists entries via <see cref="ISecureStore"/> under the key
/// <see cref="StorageKey"/> (sanitized to <c>audit__chain.bin</c> on disk per
/// PHASE1B §2). Consumed by <c>ComplianceCore.AuditAsync</c>.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1C: surface + storage + sequence + verify behavior not
/// specified verbatim in Architecture.md; derived from §10.5 / §10.7 and
/// approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE1C-DERIVATIONS.md §2-§5.
/// <para>
/// Derivations applied here:
/// (2) Class surface: ctor(ISecureStore, IClock, IAppLogger); methods
///     AppendAsync/GetAllAsync/VerifyAsync; SemaphoreSlim(1,1) gate matching
///     PHASE1A §6 + PHASE1B §5 conventions.
/// (3) Storage: flat List&lt;AuditEntry&gt; in private DTO AuditChainDto
///     under <see cref="StorageKey"/> "audit/chain"; entire chain rewritten
///     on every append (ISecureStore is key/value, not append-only).
/// (4) Sequence policy: <see cref="AppendAsync"/> overwrites entry.Seq with
///     lastSeq+1 (security invariant — caller cannot pre-set Seq); first
///     entry Seq=1; PrevHash = <see cref="HashChain.GenesisPrevHash"/> for
///     first entry; Hash computed via <see cref="HashChain.ComputeHash"/>
///     AFTER all other fields are set.
/// (5) <see cref="VerifyAsync"/> returns bool only — NO side effects in
///     Phase 1C; the §10.9 "user-visible warning + export" is Phase 2B/2E +
///     Phase 4A scope.
/// </para>
/// <para>
/// Defensive implementation note (NOT a new derivation; follows PHASE1A §7
/// pattern): if entry.Timestamp == default when <see cref="AppendAsync"/>
/// is called, it is set to <c>clock.UtcNow</c>. Callers (e.g. TaskEngine per
/// PHASE1A §4) are expected to pre-populate Timestamp; this fallback guards
/// against forgetful callers without overriding properly-set values.
/// </para>
/// </remarks>
public sealed class AuditChainWriter
{
    /// <summary>SecureStore key under which the audit chain is persisted per Derivation 3.</summary>
    public const string StorageKey = "audit/chain";

    private readonly ISecureStore _store;
    private readonly IClock _clock;
    private readonly IAppLogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Initializes a new <see cref="AuditChainWriter"/>. All three
    /// dependencies are required.
    /// </summary>
    /// <param name="store">Encrypted persistence; the chain is stored under <see cref="StorageKey"/>.</param>
    /// <param name="clock">Injectable time source for the defensive timestamp fallback.</param>
    /// <param name="logger">Logger for append/verify lifecycle messages.</param>
    public AuditChainWriter(ISecureStore store, IClock clock, IAppLogger logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Appends <paramref name="entry"/> to the hash-chained audit log per
    /// §10.5. The writer overwrites <see cref="AuditEntry.Seq"/> with the
    /// next monotonic value, sets <see cref="AuditEntry.PrevHash"/> from the
    /// previous entry, and computes <see cref="AuditEntry.Hash"/> via
    /// <see cref="HashChain.ComputeHash"/> before persisting.
    /// </summary>
    /// <param name="entry">
    /// The entry to append. Caller pre-populates Actor/Module/Action/TargetId/
    /// Result/Timestamp; the writer assigns Seq/PrevHash/Hash.
    /// </param>
    public async Task AppendAsync(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var dto = await _store.LoadAsync<AuditChainDto>(StorageKey).ConfigureAwait(false)
                      ?? new AuditChainDto();

            // Derivation 4: writer-assigned monotonic Seq (overwrites any caller value).
            long nextSeq = dto.Entries.Count == 0 ? 1L : dto.Entries[^1].Seq + 1;
            entry.Seq = nextSeq;

            // Defensive timestamp fallback per PHASE1A §7 pattern.
            if (entry.Timestamp == default)
                entry.Timestamp = _clock.UtcNow;

            // Derivation 4: PrevHash = last entry's Hash, or GenesisPrevHash for first.
            string prevHash = dto.Entries.Count == 0
                ? HashChain.GenesisPrevHash
                : dto.Entries[^1].Hash;
            entry.PrevHash = prevHash;

            // Compute Hash AFTER all other fields are set
            // (HashChain.ComputeHash omits Hash from the §10.5 formula).
            entry.Hash = HashChain.ComputeHash(prevHash, entry);

            dto.Entries.Add(entry);
            await _store.SaveAsync(StorageKey, dto).ConfigureAwait(false);
            _logger.LogInformation(
                "AuditChainWriter appended entry Seq={0} Action={1}",
                entry.Seq, entry.Action);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Returns the full audit chain in append order. A defensive copy is
    /// returned so callers cannot mutate persisted state.
    /// </summary>
    public async Task<IReadOnlyList<AuditEntry>> GetAllAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var dto = await _store.LoadAsync<AuditChainDto>(StorageKey).ConfigureAwait(false);
            if (dto is null) return Array.Empty<AuditEntry>();
            return dto.Entries.ToList();   // defensive copy
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Verifies the chain integrity per §10.5 hash formula using
    /// <see cref="HashChain.VerifyChain"/>. Returns <c>true</c> for an
    /// untampered chain (and for the empty chain). Returns <c>false</c> on
    /// any hash/prev-hash mismatch.
    /// </summary>
    /// <remarks>
    /// Phase 1C: bool-only return per Derivation 5. The §10.9 "user-visible
    /// warning + export" behavior is Phase 2B/2E (UI) + Phase 4A (export
    /// tool) scope. Phase 2B will likely require a <c>ChainVerifyFailed</c>
    /// event on <see cref="IComplianceCore"/> via §Governance amendment —
    /// see PHASE1C-DERIVATIONS.md §5.
    /// </remarks>
    public async Task<bool> VerifyAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var dto = await _store.LoadAsync<AuditChainDto>(StorageKey).ConfigureAwait(false);
            if (dto is null) return true;   // empty chain is valid
            return HashChain.VerifyChain(dto.Entries);
        }
        finally
        {
            _gate.Release();
        }
    }

    // Storage DTO — wraps List<AuditEntry> for System.Text.Json round-trip
    // via ISecureStore. Per PHASE1B §2 the key "audit/chain" sanitizes to
    // audit__chain.bin on disk.
    private sealed class AuditChainDto
    {
        public List<AuditEntry> Entries { get; set; } = new();
    }
}
