// SPEC-DERIVED-PHASE3C  HALT #21
// Gap #221: positive valid-signature import backfilled with a fixed Ed25519 test vector; production key remains owner-owned.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests
{
    [TestClass]
    public class OfflineImportServiceTests
    {
        private const string TestPublicKeyBase64 = "SmLibYaFoZZTvKbfgIRDpvjWztHTSfLXldqSLQGafyU=";
        private const string TestSignatureBase64 = "8rECRwvb/uFcp/MydWhEReGDGaWqQcEdWSFlwUKsO5kAsiynW9HnqNKhbC+pbq6Knf8Dvyqi2Ug+N2LlrfqLAQ==";

        private static UpdateManifest Manifest(byte[] payload, string sig="not-base64", string? hash=null) => new("1.0.1", UpdateChannel.Stable, new DateTimeOffset(2026,6,1,0,0,0,TimeSpan.Zero), "1.0.0", 100, new UpdatePackageInfo("u", hash ?? new HashVerifier().ComputeSha256Hex(payload), payload.Length), new UpdateSignatureInfo("Ed25519","k",sig), "n");
        private static string Bundle(bool manifest=true, bool package=true, bool validJson=true, string? hash=null, UpdateManifest? signedManifest=null, byte[]? payloadOverride=null){var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".zip");var payload=payloadOverride??Encoding.UTF8.GetBytes("abc");using var zip=ZipFile.Open(path,ZipArchiveMode.Create);if(manifest){var e=zip.CreateEntry("update.manifest.json");using var s=e.Open();using var w=new StreamWriter(s);w.Write(validJson?JsonSerializer.Serialize(signedManifest??Manifest(payload,hash:hash),new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase}):"not-json");}if(package){var p=zip.CreateEntry("package.msix");using var s=p.Open();s.Write(payload);}return path;}
        private static OfflineImportService Svc(ManifestSigner? signer=null, string? stagingRoot=null)=>new(signer??new ManifestSigner(),new HashVerifier(),new StagingService(stagingRoot??Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")),new HashVerifier()),new Mock<IAppLogger>(MockBehavior.Loose).Object);
        private static (UpdateManifest manifest, ManifestSigner signer, byte[] payload) SignedManifest()
        {
            var payload = Encoding.UTF8.GetBytes("abc");
            return (Manifest(payload, sig: TestSignatureBase64), new ManifestSigner(TestPublicKeyBase64), payload);
        }
        [TestMethod] public async Task ImportAsync_MissingBundle_Throws(){await Assert.ThrowsExceptionAsync<FileNotFoundException>(()=>Svc().ImportAsync(Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".zip")));}
        [TestMethod] public async Task ImportAsync_MissingManifest_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle(manifest:false)));}
        [TestMethod] public async Task ImportAsync_MissingPackage_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle(package:false)));}
        [TestMethod] public async Task ImportAsync_InvalidManifestJson_Throws(){await Assert.ThrowsExceptionAsync<JsonException>(()=>Svc().ImportAsync(Bundle(validJson:false)));}
        [TestMethod] public async Task ImportAsync_InvalidSignature_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle()));}
        [TestMethod] public async Task ImportAsync_MissingPackageMetadata_FailsClosed(){var (manifest,signer,payload)=SignedManifest();var broken=manifest with { Package=null! };var ex=await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc(signer).ImportAsync(Bundle(signedManifest:broken,payloadOverride:payload)));StringAssert.Contains(ex.Message,"package metadata");}
        [TestMethod] public async Task ImportAsync_SizeMismatch_FailsClosed(){var (manifest,signer,payload)=SignedManifest();var ex=await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc(signer).ImportAsync(Bundle(signedManifest:manifest,payloadOverride:Encoding.UTF8.GetBytes("abcd"))));StringAssert.Contains(ex.Message,"size");}
        [TestMethod] public async Task ImportAsync_HashMismatch_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle(hash:new string('B',64))));}
        [TestMethod] public async Task ImportAsync_ValidBundle_StagesPackage()
        {
            var (manifest, signer, payload) = SignedManifest();
            var stagingRoot = Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"));
            var result = await Svc(signer, stagingRoot).ImportAsync(Bundle(signedManifest: manifest, payloadOverride: payload));
            Assert.AreEqual(manifest.Version, result.Manifest.Version);
            Assert.IsTrue(File.Exists(result.StagedPackagePath));
            CollectionAssert.AreEqual(payload, File.ReadAllBytes(result.StagedPackagePath));
        }
    }
}
