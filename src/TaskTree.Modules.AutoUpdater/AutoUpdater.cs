// SPEC-DERIVED-PHASE3A  AutoUpdater core verification
// SPEC-DERIVED-PHASE3B  state/staging collaborators
// SPEC-DERIVED-PHASE3C  HALT #11/#12 ImportLocalAsync integration
// Architecture.md Sections 4.7 and 9.1.4-9.1.6.
// Gap #236/#237: constructor overload changed; offline import not represented in state graph.

using System;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Core.Logging;

namespace TaskTree.Modules.AutoUpdater
{
    public sealed class AutoUpdater : IAutoUpdater
    {
        private readonly ManifestSigner _manifestSigner;
        private readonly HashVerifier _hashVerifier;
        private readonly OfflineImportService _offlineImportService;
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
        public Task<UpdateManifest?> CheckAsync(){StateMachine.TransitionTo(UpdaterState.Checking);StateMachine.TransitionTo(UpdaterState.Idle);return Task.FromResult<UpdateManifest?>(null);}
        public Task<bool> VerifyAsync(UpdateManifest manifest, byte[] payload)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            if (payload is null) throw new ArgumentNullException(nameof(payload));
            if (manifest.Package is null || manifest.Signature is null) return Task.FromResult(false);
            if (payload.LongLength != manifest.Package.SizeBytes) return Task.FromResult(false);
            if (!_hashVerifier.VerifySha256(payload, manifest.Package.Sha256)) return Task.FromResult(false);
            return Task.FromResult(_manifestSigner.VerifyManifestSignature(manifest));
        }
        public Task ApplyAsync(UpdateManifest manifest) => throw new NotImplementedException("HIGH: Add-AppxPackage - Codex Phase 5E");
        public async Task<UpdateManifest> ImportLocalAsync(string filePath) => (await _offlineImportService.ImportAsync(filePath).ConfigureAwait(false)).Manifest;
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
