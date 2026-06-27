// SPEC-DERIVED-PHASE2G  HALT #1/#14
// Gap #180: Architecture Section 4 should add ISnoozeService subsection.
// Gap #189: Move SnoozeChangeKind to Core.Enums if reused broadly.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions
{
    public interface ISnoozeService
    {
        event EventHandler<SnoozeChangedEventArgs>? SnoozeChanged;
        Task<SnoozeState?> GetAsync(Guid taskId);
        Task SnoozeAsync(Guid taskId, DateTimeOffset untilUtc, Enums.SnoozeReason reason);
        Task ClearAsync(Guid taskId);
        Task<IReadOnlyList<SnoozeState>> GetAllAsync();
    }

    public enum SnoozeChangeKind { Created = 0, Cleared = 1, Expired = 2 }

    public sealed class SnoozeChangedEventArgs : EventArgs
    {
        public SnoozeChangedEventArgs(Guid taskId, SnoozeState? state, SnoozeChangeKind kind)
        { TaskId = taskId; State = state; Kind = kind; }
        public Guid TaskId { get; }
        public SnoozeState? State { get; }
        public SnoozeChangeKind Kind { get; }
    }
}
