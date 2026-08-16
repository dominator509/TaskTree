// SPEC-DERIVED-PHASE3B  HALT #12/#13/#14/#15/#16
// Architecture.md Section 9.1.4 staging path and Section 9.1.3 hash verification.
// Gap #228/#229/#230: real Windows path and filesystem permission/tamper behavior need Phase 5C validation.

using System;
using System.IO;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Stages verified update packages on disk.</summary>
    public sealed class StagingService
    {
        private readonly string _stagingRoot;
        private readonly HashVerifier _hashVerifier;
        public StagingService() : this(GetDefaultStagingRoot(), new HashVerifier()) { }
        public StagingService(string stagingRoot, HashVerifier hashVerifier)
        {
            _stagingRoot = string.IsNullOrWhiteSpace(stagingRoot) ? throw new ArgumentException("Staging root required.", nameof(stagingRoot)) : stagingRoot;
            _hashVerifier = hashVerifier ?? throw new ArgumentNullException(nameof(hashVerifier));
        }
        public async Task<string> StageAsync(UpdateManifest manifest, byte[] payload)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            if (payload is null) throw new ArgumentNullException(nameof(payload));
            if (manifest.Package is null) throw new ArgumentException("Manifest package is required.", nameof(manifest));
            if (payload.LongLength != manifest.Package.SizeBytes) throw new InvalidOperationException("Payload size does not match manifest.");
            if (!_hashVerifier.VerifySha256(payload, manifest.Package.Sha256)) throw new InvalidOperationException("Payload hash does not match manifest.");
            ValidateVersionForStaging(manifest.Version);
            Directory.CreateDirectory(_stagingRoot);
            var path = Path.Combine(_stagingRoot, $"TaskTree-{manifest.Version}.msix");
            var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            var promoted = false;
            try
            {
                await File.WriteAllBytesAsync(temporaryPath, payload).ConfigureAwait(false);
                File.Move(temporaryPath, path, overwrite: true);
                promoted = true;

                var stagedBytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
                if (!_hashVerifier.VerifySha256(stagedBytes, manifest.Package.Sha256))
                    throw new InvalidOperationException("Staged file hash verification failed.");
            }
            catch
            {
                TryDelete(temporaryPath);
                if (promoted) TryDelete(path);
                throw;
            }
            return path;
        }

        private static void ValidateVersionForStaging(string? version)
        {
            if (string.IsNullOrWhiteSpace(version) || version.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || version.Contains('/') || version.Contains('\\'))
                throw new InvalidOperationException("Manifest version is not safe for a staged file name.");
        }
        private static string GetDefaultStagingRoot()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(local, "TaskTree", "updates");
        }
        private static void TryDelete(string path){try{if(File.Exists(path))File.Delete(path);}catch{}}
    }
}
