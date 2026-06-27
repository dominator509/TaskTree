// SPEC-DERIVED-PHASE3A  HALT #11/#12/#13
// Architecture.md Section 9.1.3 package SHA-256 verification.
// Gap #216/#217: hash casing accepted; Phase 3B must re-check after download/staging.

using System;
using System.Security.Cryptography;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Computes and verifies SHA-256 package hashes.</summary>
    public sealed class HashVerifier
    {
        /// <summary>Computes an uppercase 64-character SHA-256 hex digest.</summary>
        public string ComputeSha256Hex(byte[] payload)
        {
            if (payload is null) throw new ArgumentNullException(nameof(payload));
            return Convert.ToHexString(SHA256.HashData(payload));
        }

        /// <summary>Returns true when payload SHA-256 equals expected HEX_64, accepting upper/lowercase.</summary>
        public bool VerifySha256(byte[] payload, string expectedHex)
        {
            if (payload is null) throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrWhiteSpace(expectedHex) || expectedHex.Length != 64) return false;
            foreach (var c in expectedHex)
            {
                var ok = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!ok) return false;
            }
            return string.Equals(ComputeSha256Hex(payload), expectedHex, StringComparison.OrdinalIgnoreCase);
        }
    }
}
