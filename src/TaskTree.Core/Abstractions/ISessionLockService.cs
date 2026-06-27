// SPEC-DERIVED-PHASE2F  HALT #1/#2
// Gap #164: Architecture Section 4 should add ISessionLockService subsection if Phase 2F ships.
// Gap #165: Move SessionLockChangedEventArgs to Core.Models if session state grows.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTree.Core.Abstractions
{
    public interface ISessionLockService
    {
        event EventHandler<SessionLockChangedEventArgs>? SessionLockChanged;
        bool IsLocked { get; }
        Task StartAsync(CancellationToken ct);
        Task StopAsync();
    }

    public sealed class SessionLockChangedEventArgs : EventArgs
    {
        public SessionLockChangedEventArgs(bool isLocked, DateTimeOffset changedAtUtc)
        { IsLocked = isLocked; ChangedAtUtc = changedAtUtc; }
        public bool IsLocked { get; }
        public DateTimeOffset ChangedAtUtc { get; }
    }
}
