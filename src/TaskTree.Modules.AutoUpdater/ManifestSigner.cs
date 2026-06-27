// SPEC-DERIVED-PHASE3A  HALT #6/#7/#8/#9/#10
// Architecture.md Section 9.1.3 Ed25519 signature verification and public-key pinning.
// Gap #214/#215/#221: production key and canonicalization/test vectors required before release.

using System;
using System.Text.Json;
using NSec.Cryptography;
using TaskTree.Core.Models;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Builds canonical manifest signing payloads and verifies Ed25519 signatures.</summary>
    public sealed class ManifestSigner
    {
        internal const string EmbeddedPublicKeyBase64 = "PHASE3A_DEV_PUBLIC_KEY_PLACEHOLDER";

        private static readonly JsonSerializerOptions CanonicalOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        /// <summary>Builds canonical UTF-8 JSON payload excluding the manifest signature object.</summary>
        public byte[] BuildCanonicalSigningPayload(UpdateManifest manifest)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            var payload = new
            {
                manifest.Version,
                manifest.Channel,
                manifest.Released,
                manifest.MinPreviousVersion,
                manifest.RolloutPercent,
                manifest.Package,
                manifest.Notes,
            };
            return JsonSerializer.SerializeToUtf8Bytes(payload, CanonicalOptions);
        }

        /// <summary>Verifies the embedded Ed25519 signature. Returns false for malformed placeholder key/signature.</summary>
        public bool VerifyManifestSignature(UpdateManifest manifest)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            if (manifest.Signature is null) return false;
            if (!string.Equals(manifest.Signature.Alg, "Ed25519", StringComparison.Ordinal)) return false;

            try
            {
                var publicKeyBytes = Convert.FromBase64String(EmbeddedPublicKeyBase64);
                var signatureBytes = Convert.FromBase64String(manifest.Signature.Value);
                var algorithm = SignatureAlgorithm.Ed25519;
                using var publicKey = PublicKey.Import(algorithm, publicKeyBytes, KeyBlobFormat.RawPublicKey);
                return algorithm.Verify(publicKey, BuildCanonicalSigningPayload(manifest), signatureBytes);
            }
            catch
            {
                return false;
            }
        }
    }
}
