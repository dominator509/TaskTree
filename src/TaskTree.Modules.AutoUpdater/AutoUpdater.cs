// SPEC-DERIVED-PHASE3A  AutoUpdater core verification
// SPEC-DERIVED-PHASE3B  state/staging collaborators
// SPEC-DERIVED-PHASE3C  HALT #11/#12 ImportLocalAsync integration
// Architecture.md Sections 4.7 and 9.1.4-9.1.6.
// Gap #236/#237: constructor overload changed; offline import not represented in state graph.

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Core.Logging;

namespace TaskTree.Modules.AutoUpdater
{
    public sealed class AutoUpdater : IAutoUpdater
    {
        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly ManifestSigner _manifestSigner;
        private readonly HashVerifier _hashVerifier;
        private readonly OfflineImportService _offlineImportService;
        private readonly SemaphoreSlim _operationGate = new(1, 1);
        public AutoUpdater() : this(new ManifestSigner(), new HashVerifier(), new UpdaterStateMachine(new SystemClockAdapter()), new StagingService(), new VersionEligibilityEvaluator(), null) { }
        public AutoUpdater(ManifestSigner manifestSigner, HashVerifier hashVerifier)
            : this(manifestSigner, hashVerifier, new UpdaterStateMachine(new SystemClockAdapter()), new StagingService(), new VersionEligibilityEvaluator(), null) { }
        public AutoUpdater(ManifestSigner manifestSigner, HashVerifier hashVerifier, UpdaterStateMachine stateMachine, StagingService stagingService, VersionEligibilityEvaluator eligibilityEvaluator, OfflineImportService? offlineImportService = null)
        {
            _manifestSigner = manifestSigner ?? throw new ArgumentNullException(nameof(manifestSigner));
            _hashVerifier = hashVerifier ?? throw new ArgumentNullException(nameof(hashVerifier));
            StateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            StagingService = stagingService ?? throw new ArgumentNullException(nameof(stagingService));
            EligibilityEvaluator = eligibilityEvaluator ?? throw new ArgumentNullException(nameof(eligibilityEvaluator));
            _offlineImportService = offlineImportService ?? new OfflineImportService(_manifestSigner, _hashVerifier, StagingService, new NullAppLogger());
        }
        public UpdaterStateMachine StateMachine { get; }
        public StagingService StagingService { get; }
        public VersionEligibilityEvaluator EligibilityEvaluator { get; }
        public UpdateChannel Channel { get; set; } = UpdateChannel.Stable;
        public bool Enabled { get; set; } = true;
        public async Task<UpdateManifest?> CheckAsync()
        {
            if (!Enabled) return null;
            var endpoint = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_MANIFEST_URL");
            if (string.IsNullOrWhiteSpace(endpoint)) return null;
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                return null;

            await _operationGate.WaitAsync().ConfigureAwait(false);
            try
            {
                StateMachine.TransitionTo(UpdaterState.Checking);
                using var response = await HttpClient.GetAsync(uri).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) return null;
                await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(stream, JsonOptions).ConfigureAwait(false);
                if (manifest is null || manifest.Package is null || manifest.Signature is null) return null;
                if (manifest.Channel != Channel || !_manifestSigner.VerifyManifestSignature(manifest)) return null;
                var currentVersion = Environment.GetEnvironmentVariable("TASKTREE_UPDATE_CURRENT_VERSION") ?? "0.0.0";
                var bucket = int.TryParse(Environment.GetEnvironmentVariable("TASKTREE_UPDATE_ROLLOUT_BUCKET"), out var parsedBucket)
                    ? parsedBucket
                    : 100;
                return EligibilityEvaluator.IsEligible(manifest, currentVersion, bucket) ? manifest : null;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (StateMachine.Current == UpdaterState.Checking)
                    StateMachine.TransitionTo(UpdaterState.Idle);
                _operationGate.Release();
            }
        }
        public Task<bool> VerifyAsync(UpdateManifest manifest, byte[] payload)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            if (payload is null) throw new ArgumentNullException(nameof(payload));
            if (manifest.Package is null || manifest.Signature is null) return Task.FromResult(false);
            if (payload.LongLength != manifest.Package.SizeBytes) return Task.FromResult(false);
            if (!_hashVerifier.VerifySha256(payload, manifest.Package.Sha256)) return Task.FromResult(false);
            return Task.FromResult(_manifestSigner.VerifyManifestSignature(manifest));
        }
        public async Task ApplyAsync(UpdateManifest manifest)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            await _operationGate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (StateMachine.Current == UpdaterState.Idle)
                {
                    StateMachine.TransitionTo(UpdaterState.Checking);
                    StateMachine.TransitionTo(UpdaterState.Downloading);
                    var packagePath = await EnsurePackageStagedAsync(manifest).ConfigureAwait(false);
                    StateMachine.TransitionTo(UpdaterState.Verifying);
                    StateMachine.TransitionTo(UpdaterState.Staging);
                    StateMachine.TransitionTo(UpdaterState.Applying);
                    await MsixPackageInstaller.InstallAsync(packagePath).ConfigureAwait(false);
                    StateMachine.TransitionTo(UpdaterState.Applied);
                    StateMachine.TransitionTo(UpdaterState.Idle);
                    return;
                }
                var stagedPath = ResolvePackagePath(manifest);
                if (!File.Exists(stagedPath))
                    throw new FileNotFoundException("MSIX package is not staged.", stagedPath);
                var stagedPayload = await File.ReadAllBytesAsync(stagedPath).ConfigureAwait(false);
                if (!await VerifyAsync(manifest, stagedPayload).ConfigureAwait(false))
                    throw new InvalidOperationException("Staged update package failed signature or hash verification.");
                StateMachine.TransitionTo(UpdaterState.Applying);
                await MsixPackageInstaller.InstallAsync(stagedPath).ConfigureAwait(false);
                StateMachine.TransitionTo(UpdaterState.Applied);
                StateMachine.TransitionTo(UpdaterState.Idle);
            }
            catch
            {
                if (StateMachine.Current == UpdaterState.Checking || StateMachine.Current == UpdaterState.Downloading || StateMachine.Current == UpdaterState.Verifying || StateMachine.Current == UpdaterState.Staging || StateMachine.Current == UpdaterState.Applying)
                    StateMachine.TransitionTo(UpdaterState.Failed);
                if (StateMachine.Current == UpdaterState.Failed)
                    StateMachine.Reset();
                throw;
            }
            finally
            {
                _operationGate.Release();
            }
        }
        public async Task<UpdateManifest> ImportLocalAsync(string filePath) => (await _offlineImportService.ImportAsync(filePath).ConfigureAwait(false)).Manifest;
        private static string ResolvePackagePath(UpdateManifest manifest)
        {
            if (!string.IsNullOrWhiteSpace(manifest.Package?.Url) &&
                Uri.TryCreate(manifest.Package.Url, UriKind.Absolute, out var uri) &&
                uri.IsFile)
                return uri.LocalPath;
            return MsixPackageInstaller.GetDefaultStagedPath(manifest.Version);
        }

        private async Task<string> EnsurePackageStagedAsync(UpdateManifest manifest)
        {
            var stagedPath = ResolvePackagePath(manifest);
            if (File.Exists(stagedPath))
            {
                var stagedPayload = await File.ReadAllBytesAsync(stagedPath).ConfigureAwait(false);
                if (!await VerifyAsync(manifest, stagedPayload).ConfigureAwait(false))
                    throw new InvalidOperationException("Staged update package failed signature or hash verification.");
                return stagedPath;
            }
            if (!string.Equals(Environment.GetEnvironmentVariable("TASKTREE_UPDATE_DOWNLOAD_ENABLED"), "true", StringComparison.OrdinalIgnoreCase))
                throw new FileNotFoundException("MSIX package is not staged and network download is disabled.", stagedPath);
            if (!Uri.TryCreate(manifest.Package?.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Update package URL must use HTTPS.");

            var payload = await HttpClient.GetByteArrayAsync(uri).ConfigureAwait(false);
            if (!await VerifyAsync(manifest, payload).ConfigureAwait(false))
                throw new InvalidOperationException("Downloaded update package failed signature or hash verification.");
            var staged = await StagingService.StageAsync(manifest, payload).ConfigureAwait(false);
            return staged;
        }
        private sealed class SystemClockAdapter : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
        private sealed class NullAppLogger : IAppLogger
        {
            public void LogDebug(string message, params object?[] args) { }
            public void LogInformation(string message, params object?[] args) { }
            public void LogWarning(string message, params object?[] args) { }
            public void LogError(Exception? exception, string message, params object?[] args) { }
        }
    }
}
