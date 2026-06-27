// SPEC-DERIVED-PHASE3F  HALT #4/#5/#6/#7/#8/#18
// Roadmap Phase 3F; Architecture.md Section 9.1 AutoUpdater offline integration gate.
// Gap #214/#221 retained: positive Ed25519 verification awaits real key/test vector.
// Gap #279: Phase 5E must replace no-op/stub updater operations with live HTTP/MSIX flow.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.AutoUpdater;
using TaskTree.TestSupport;

namespace TaskTree.Modules.AutoUpdater.Tests
{
    [TestClass]
    public class Phase3IntegrationTests
    {
        private static readonly DateTimeOffset T = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        private static UpdateManifest Manifest(byte[] payload) => new("1.0.1", UpdateChannel.Stable, T, "1.0.0", 100, new UpdatePackageInfo("https://updates.example.invalid/tasktree.msix", new HashVerifier().ComputeSha256Hex(payload), payload.Length), new UpdateSignatureInfo("Ed25519", "tasktree-stable-2026", "not-base64"), "Synthetic integration manifest");
        private static string TempRoot() => Path.Combine(Path.GetTempPath(), "TaskTreeP3F", Guid.NewGuid().ToString("N"));

        [TestMethod]
        public void StateMachine_HappyPath_ReachesApplied()
        {
            var machine = new UpdaterStateMachine(new FakeClock(T));
            machine.TransitionTo(UpdaterState.Checking);
            machine.TransitionTo(UpdaterState.Downloading);
            machine.TransitionTo(UpdaterState.Verifying);
            machine.TransitionTo(UpdaterState.Staging);
            machine.TransitionTo(UpdaterState.Applying);
            machine.TransitionTo(UpdaterState.Applied);
            Assert.AreEqual(UpdaterState.Applied, machine.Current);
        }

        [TestMethod]
        public void StateMachine_VerifyFailure_ReachesFailed()
        {
            var machine = new UpdaterStateMachine(new FakeClock(T));
            machine.TransitionTo(UpdaterState.Checking);
            machine.TransitionTo(UpdaterState.Downloading);
            machine.TransitionTo(UpdaterState.Verifying);
            machine.TransitionTo(UpdaterState.Failed);
            Assert.AreEqual(UpdaterState.Failed, machine.Current);
        }

        [TestMethod]
        public async Task StagingService_ValidPayload_StagesAndVerifiesFile()
        {
            var payload = Encoding.UTF8.GetBytes("synthetic package");
            var staged = await new StagingService(TempRoot(), new HashVerifier()).StageAsync(Manifest(payload), payload);
            Assert.IsTrue(File.Exists(staged));
            Assert.IsTrue(new HashVerifier().VerifySha256(await File.ReadAllBytesAsync(staged), Manifest(payload).Package.Sha256));
        }

        [TestMethod]
        public void VersionEligibility_NewerVersionWithinRollout_IsEligible()
        {
            var payload = Encoding.UTF8.GetBytes("synthetic package");
            Assert.IsTrue(new VersionEligibilityEvaluator().IsEligible(Manifest(payload), "1.0.0", 50));
        }

        [TestMethod]
        public async Task OfflineImport_InvalidSignature_DoesNotStage()
        {
            var root = TempRoot();
            Directory.CreateDirectory(root);
            var bundle = Path.Combine(root, "bundle.zip");
            var payload = Encoding.UTF8.GetBytes("synthetic package");
            using (var zip = System.IO.Compression.ZipFile.Open(bundle, System.IO.Compression.ZipArchiveMode.Create))
            {
                var manifestEntry = zip.CreateEntry("update.manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                    writer.Write(System.Text.Json.JsonSerializer.Serialize(Manifest(payload), new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
                var packageEntry = zip.CreateEntry("package.msix");
                using var stream = packageEntry.Open();
                stream.Write(payload);
            }
            var stagingRoot = Path.Combine(root, "stage");
            var svc = new OfflineImportService(new ManifestSigner(), new HashVerifier(), new StagingService(stagingRoot, new HashVerifier()), new TestLogger());
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => svc.ImportAsync(bundle));
            Assert.IsFalse(Directory.Exists(stagingRoot));
        }

        [TestMethod]
        public async Task Sentinel_CreateReadClear_RoundTripsManifest()
        {
            var sentinel = new SentinelService(Path.Combine(TempRoot(), "sentinel.lock"));
            var manifest = Manifest(Encoding.UTF8.GetBytes("synthetic package"));
            await sentinel.CreateAsync(manifest);
            Assert.IsTrue(await sentinel.ExistsAsync());
            Assert.AreEqual(manifest.Version, (await sentinel.ReadAsync())!.Version);
            await sentinel.ClearAsync();
            Assert.IsFalse(await sentinel.ExistsAsync());
        }

        [TestMethod]
        public async Task Rollback_FindLastKnownGood_ReturnsNewestMsix()
        {
            var root = TempRoot(); Directory.CreateDirectory(root);
            var oldFile = Path.Combine(root, "old.msix"); var newFile = Path.Combine(root, "new.msix");
            File.WriteAllText(oldFile, "old"); File.WriteAllText(newFile, "new");
            File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow.AddMinutes(-10));
            File.SetLastWriteTimeUtc(newFile, DateTime.UtcNow);
            Assert.AreEqual(newFile, await new RollbackService(root).FindLastKnownGoodAsync());
        }

        [TestMethod]
        public async Task ApplyAsync_RemainsPhase5EStub()
        {
            var ex = await Assert.ThrowsExceptionAsync<NotImplementedException>(() => new AutoUpdater().ApplyAsync(Manifest(Array.Empty<byte>())));
            StringAssert.Contains(ex.Message, "Phase 5E");
        }

        private sealed class TestLogger : TaskTree.Core.Abstractions.IAppLogger
        {
            public void LogInformation(string message) { }
            public void LogWarning(string message) { }
            public void LogError(string message) { }
        }
    }
}
