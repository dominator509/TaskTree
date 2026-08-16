// SPEC-DERIVED-PHASE3E  HALT #13/#14
// Architecture.md Section 9.2.6 rate limit max 5/minute and 50/day.
// Gap #270/#271: in-memory rate limiter and outbound/filedrop semantics are derived.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>In-memory outbound bug report rate limiter.</summary>
    public sealed class BugReportRateLimiter
    {
        private readonly List<DateTimeOffset> _sent = new();
        private readonly object _gate = new();
        public bool CanSend(DateTimeOffset nowUtc)
        {
            lock (_gate)
            {
                Prune(nowUtc);
                return IsWithinLimit(nowUtc);
            }
        }
        public void RecordSend(DateTimeOffset nowUtc){lock (_gate){Prune(nowUtc);_sent.Add(nowUtc);}}
        private bool IsWithinLimit(DateTimeOffset nowUtc) => _sent.Count(x => x > nowUtc.AddMinutes(-1)) < 5 && _sent.Count(x => x.Date == nowUtc.Date) < 50;
        private void Prune(DateTimeOffset nowUtc)=>_sent.RemoveAll(x=>x < nowUtc.AddDays(-1));
    }
}
