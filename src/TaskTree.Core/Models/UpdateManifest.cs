// SPEC-DERIVED-MSG3
// SPEC-DERIVED-PHASE3A  HALT #1/#2/#3/#4/#5
// Architecture.md Section 9.1.2 manifest schema; Section 9.1.3 signature/integrity.
// Gap #209/#210/#211/#212: schema patch, nested DTO derivation, casing compatibility, version comparison deferred.

using System;
using TaskTree.Core.Enums;

namespace TaskTree.Core.Models
{
    /// <summary>Signed update manifest matching Architecture.md Section 9.1.2.</summary>
    public sealed record UpdateManifest(
        string Version,
        UpdateChannel Channel,
        DateTimeOffset Released,
        string MinPreviousVersion,
        int RolloutPercent,
        UpdatePackageInfo Package,
        UpdateSignatureInfo Signature,
        string Notes);

    /// <summary>Package metadata embedded in the signed update manifest.</summary>
    public sealed record UpdatePackageInfo(
        string Url,
        string Sha256,
        long SizeBytes);

    /// <summary>Signature metadata embedded in the signed update manifest.</summary>
    public sealed record UpdateSignatureInfo(
        string Alg,
        string PublicKeyId,
        string Value);
}
