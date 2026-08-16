// SPEC-DERIVED-PHASE3C  HALT #1/#2/#3/#4/#5/#6/#7/#8/#9/#10
// Architecture.md Sections 9.1.4-9.1.6: offline import uses same signature + hash verification pipeline.
// Gap #232/#233/#234/#235: ZIP bundle format, result type, constructor/factory, and entry names are Phase 3C-derived.

using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Imports an offline update ZIP bundle containing update.manifest.json and package.msix.</summary>
    public sealed class OfflineImportService
    {
        private const string ManifestEntryName = "update.manifest.json";
        private const string PackageEntryName = "package.msix";
        private readonly ManifestSigner _manifestSigner;
        private readonly HashVerifier _hashVerifier;
        private readonly StagingService _stagingService;
        private readonly IAppLogger _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>Creates the offline import service.</summary>
        public OfflineImportService(ManifestSigner manifestSigner, HashVerifier hashVerifier, StagingService stagingService, IAppLogger logger)
        {
            _manifestSigner = manifestSigner ?? throw new ArgumentNullException(nameof(manifestSigner));
            _hashVerifier = hashVerifier ?? throw new ArgumentNullException(nameof(hashVerifier));
            _stagingService = stagingService ?? throw new ArgumentNullException(nameof(stagingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Imports, verifies, and stages an offline update bundle.</summary>
        public async Task<OfflineImportResult> ImportAsync(string bundlePath)
        {
            if (string.IsNullOrWhiteSpace(bundlePath)) throw new ArgumentException("Bundle path required.", nameof(bundlePath));
            if (!File.Exists(bundlePath)) throw new FileNotFoundException("Offline update bundle not found.", bundlePath);
            try
            {
                using var archive = ZipFile.OpenRead(bundlePath);
                var manifestEntry = archive.GetEntry(ManifestEntryName) ?? throw new InvalidOperationException("Offline update bundle is missing update.manifest.json.");
                var packageEntry = archive.GetEntry(PackageEntryName) ?? throw new InvalidOperationException("Offline update bundle is missing package.msix.");
                await using var manifestStream = manifestEntry.Open();
                var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(manifestStream, JsonOptions).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("Offline update manifest could not be parsed.");
                if (manifest.Package is null)
                    throw new InvalidOperationException("Offline update manifest is missing package metadata.");
                if (!_manifestSigner.VerifyManifestSignature(manifest))
                {
                    _logger.LogWarning("Offline update manifest signature verification failed.");
                    throw new InvalidOperationException("Offline update manifest signature verification failed.");
                }
                if (manifest.Package.SizeBytes < 0 || packageEntry.Length != manifest.Package.SizeBytes)
                    throw new InvalidOperationException("Offline update package size does not match manifest metadata.");
                await using var packageStream = packageEntry.Open();
                using var memory = new MemoryStream();
                await packageStream.CopyToAsync(memory).ConfigureAwait(false);
                var payload = memory.ToArray();
                if (!_hashVerifier.VerifySha256(payload, manifest.Package.Sha256))
                    throw new InvalidOperationException("Offline update package hash verification failed.");
                var stagedPath = await _stagingService.StageAsync(manifest, payload).ConfigureAwait(false);
                return new OfflineImportResult(manifest, stagedPath);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogError(ex, "Offline update bundle is not a valid ZIP: {0}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>Result of a verified offline update import.</summary>
    public sealed record OfflineImportResult(UpdateManifest Manifest, string StagedPackagePath);
}
