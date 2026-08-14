// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Core/Abstractions/IAutoUpdater.cs
//  Purpose: Manifest poll, signature + hash verification, staging, offline import per Architecture §4.7 / §9.1.
//  Architecture.md References: §4.7, §9.1
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 2 — Interfaces)
//  D1 anti-drift: header cites Architecture.md sections.
//  D2 anti-drift: signatures match Architecture.md verbatim (or SPEC-DERIVED with owner approval).
//  D10 anti-drift: XML doc on every public member.
// ─────────────────────────────────────────────────────────────────────────────

using System.Threading.Tasks;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions;

/// <summary>
/// Coordinates the application self-update lifecycle: poll the manifest URL,
/// verify Ed25519 signature + SHA-256 hash, stage the package, and apply or
/// roll back per the state machine in §9.1.1.
/// </summary>
public interface IAutoUpdater
{
    /// <summary>Polls the configured update manifest endpoint.</summary>
    /// <returns>The latest <see cref="UpdateManifest"/>, or <c>null</c> if no newer version is available.</returns>
    Task<UpdateManifest?> CheckAsync();

    /// <summary>
    /// Verifies the Ed25519 signature on the manifest and the SHA-256 hash of
    /// the downloaded payload (§9.1.3).
    /// </summary>
    /// <param name="manifest">The manifest being verified.</param>
    /// <param name="payload">The downloaded package bytes.</param>
    /// <returns><c>true</c> if both signature and hash check out; otherwise <c>false</c>.</returns>
    Task<bool> VerifyAsync(UpdateManifest manifest, byte[] payload);

    /// <summary>Applies a staged update through the Windows MSIX installer.</summary>
    /// <param name="manifest">The manifest associated with the staged package.</param>
    Task ApplyAsync(UpdateManifest manifest);

    /// <summary>
    /// Imports a manifest + package from a local file path (offline / air-gapped
    /// scenarios, §9.1.4). The same signature + hash verification is enforced.
    /// </summary>
    /// <param name="filePath">Path to the local update bundle.</param>
    /// <returns>The verified manifest.</returns>
    Task<UpdateManifest> ImportLocalAsync(string filePath);

    /// <summary>Gets or sets the update channel (stable or beta) per §9.1.</summary>
    UpdateChannel Channel { get; set; }

    /// <summary>Gets or sets whether the auto-updater is active.</summary>
    bool Enabled { get; set; }
}
