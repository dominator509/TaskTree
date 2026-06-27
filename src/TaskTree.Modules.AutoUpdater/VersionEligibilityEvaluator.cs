// SPEC-DERIVED-PHASE3B  HALT #9/#10/#11
// Architecture.md Sections 9.1.2 and 9.1.4 version/minPreviousVersion/rollout checks.
// Gap #226/#227: System.Version comparison only; rollout bucket derivation remains caller-supplied.

using System;
using TaskTree.Core.Models;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Evaluates manifest eligibility against current version and rollout bucket.</summary>
    public sealed class VersionEligibilityEvaluator
    {
        public bool IsEligible(UpdateManifest manifest, string currentVersion, int installationRolloutBucket)
        {
            if (manifest is null) throw new ArgumentNullException(nameof(manifest));
            if (installationRolloutBucket < 1 || installationRolloutBucket > 100) return false;
            if (manifest.RolloutPercent < 1 || manifest.RolloutPercent > 100) return false;
            if (!Version.TryParse(manifest.Version, out var manifestVersion)) return false;
            if (!Version.TryParse(currentVersion, out var current)) return false;
            if (!Version.TryParse(manifest.MinPreviousVersion, out var minPrevious)) return false;
            if (manifestVersion <= current) return false;
            if (current < minPrevious) return false;
            return installationRolloutBucket <= manifest.RolloutPercent;
        }
    }
}
