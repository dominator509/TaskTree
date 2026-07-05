// SPEC-DERIVED-PHASE3A  HALT #20
// Gap #221: positive vector coverage lives in OfflineImportServiceTests; production key remains owner-owned.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests
{
    [TestClass]
    public class ManifestSignerTests
    {
        private static UpdateManifest Manifest(string alg = "Ed25519", string sig = "not-base64") => new(
            Version: "1.0.1",
            Channel: UpdateChannel.Stable,
            Released: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
            MinPreviousVersion: "1.0.0",
            RolloutPercent: 100,
            Package: new UpdatePackageInfo("https://updates.example.invalid/tasktree.msix", new string('A', 64), 3),
            Signature: new UpdateSignatureInfo(alg, "tasktree-stable-2026", sig),
            Notes: "Synthetic test manifest");

        [TestMethod]
        public void VerifyManifestSignature_UnsupportedAlg_ReturnsFalse()
        {
            Assert.IsFalse(new ManifestSigner().VerifyManifestSignature(Manifest(alg: "RSA")));
        }

        [TestMethod]
        public void VerifyManifestSignature_InvalidBase64Signature_ReturnsFalse()
        {
            Assert.IsFalse(new ManifestSigner().VerifyManifestSignature(Manifest()));
        }

        [TestMethod]
        public void VerifyManifestSignature_TamperedManifest_ReturnsFalse()
        {
            var manifest = Manifest(sig: Convert.ToBase64String(new byte[64]));
            Assert.IsFalse(new ManifestSigner().VerifyManifestSignature(manifest with { Version = "9.9.9" }));
        }

        [TestMethod]
        public void BuildCanonicalSigningPayload_SameManifest_SameBytes()
        {
            var signer = new ManifestSigner();
            CollectionAssert.AreEqual(signer.BuildCanonicalSigningPayload(Manifest()), signer.BuildCanonicalSigningPayload(Manifest()));
        }
    }
}
