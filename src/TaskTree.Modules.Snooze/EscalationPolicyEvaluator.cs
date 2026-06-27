// SPEC-DERIVED-PHASE2G  HALT #7/#8
// Gap #185: promote to IEscalationPolicyService if configuration/audit complexity grows.

using System;
using TaskTree.Core.Models;

namespace TaskTree.Modules.Snooze
{
    public static class EscalationPolicyEvaluator
    {
        public static bool ShouldEscalate(SnoozeState? snooze, DateTimeOffset nowUtc, EscalationPolicy policy, int repeatCount)
        {
            if (policy is null) throw new ArgumentNullException(nameof(policy));
            if (snooze is null) return false;
            if (snooze.SnoozedUntilUtc <= nowUtc) return true;
            return repeatCount >= policy.MaxRepeats;
        }
    }
}
