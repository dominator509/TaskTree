// SPEC-DERIVED-PHASE3C  HALT #21
// Gap #221: positive valid-signature import test remains pending real Ed25519 test vector/key.

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
        private static UpdateManifest Manifest(byte[] payload, string sig="not-base64", string? hash=null) => new("1.0.1", UpdateChannel.Stable, new DateTimeOffset(2026,6,1,0,0,0,TimeSpan.Zero), "1.0.0", 100, new UpdatePackageInfo("u", hash ?? new HashVerifier().ComputeSha256Hex(payload), payload.Length), new UpdateSignatureInfo("Ed25519","k",sig), "n");
        private static string Bundle(bool manifest=true, bool package=true, bool validJson=true, string? hash=null){var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".zip");var payload=Encoding.UTF8.GetBytes("abc");using var zip=ZipFile.Open(path,ZipArchiveMode.Create);if(manifest){var e=zip.CreateEntry("update.manifest.json");using var s=e.Open();using var w=new StreamWriter(s);w.Write(validJson?JsonSerializer.Serialize(Manifest(payload,hash:hash),new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase}):"not-json");}if(package){var p=zip.CreateEntry("package.msix");using var s=p.Open();s.Write(payload);}return path;}
        private static OfflineImportService Svc()=>new(new ManifestSigner(),new HashVerifier(),new StagingService(Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N")),new HashVerifier()),new Mock<IAppLogger>(MockBehavior.Loose).Object);
        [TestMethod] public async Task ImportAsync_MissingBundle_Throws(){await Assert.ThrowsExceptionAsync<FileNotFoundException>(()=>Svc().ImportAsync(Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".zip")));}
        [TestMethod] public async Task ImportAsync_MissingManifest_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle(manifest:false)));}
        [TestMethod] public async Task ImportAsync_MissingPackage_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle(package:false)));}
        [TestMethod] public async Task ImportAsync_InvalidManifestJson_Throws(){await Assert.ThrowsExceptionAsync<JsonException>(()=>Svc().ImportAsync(Bundle(validJson:false)));}
        [TestMethod] public async Task ImportAsync_InvalidSignature_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle()));}
        [TestMethod] public async Task ImportAsync_HashMismatch_Throws(){await Assert.ThrowsExceptionAsync<InvalidOperationException>(()=>Svc().ImportAsync(Bundle(hash:new string('B',64))));}
        [TestMethod] public void ImportAsync_ValidBundle_StagesPackage_PENDING_KEY(){Assert.Inconclusive("Gap #221: Positive offline import requires deterministic Ed25519 test vector/public key.");}
    }
}
