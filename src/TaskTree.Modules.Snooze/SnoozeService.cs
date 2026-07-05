// SPEC-DERIVED-PHASE2G  HALT #9/#10/#11/#12/#13
// Gap #186 closed by ServiceRegistrations patch. Gap #187: verify expired auto-clear behavior.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.Snooze
{
    public sealed class SnoozeService : ISnoozeService
    {
        private const string StorageKey = "snooze/state";
        private readonly ISecureStore _store; private readonly IComplianceCore _compliance; private readonly IClock _clock; private readonly IAppLogger _logger;
        public event EventHandler<SnoozeChangedEventArgs>? SnoozeChanged;
        public SnoozeService(ISecureStore secureStore, IComplianceCore compliance, IClock clock, IAppLogger logger){_store=secureStore??throw new ArgumentNullException(nameof(secureStore));_compliance=compliance??throw new ArgumentNullException(nameof(compliance));_clock=clock??throw new ArgumentNullException(nameof(clock));_logger=logger??throw new ArgumentNullException(nameof(logger));}
        public async Task<SnoozeState?> GetAsync(Guid taskId){if(taskId==Guid.Empty)throw new ArgumentException("Task id required.",nameof(taskId));var map=await LoadAsync();if(!map.TryGetValue(taskId,out var state))return null;if(state.SnoozedUntilUtc<=_clock.UtcNow){map.Remove(taskId);await SaveMapAsync(map);await AuditAsync("SnoozeExpired",taskId);SnoozeChanged?.Invoke(this,new SnoozeChangedEventArgs(taskId,null,SnoozeChangeKind.Expired));return null;}return state;}
        public async Task SnoozeAsync(Guid taskId, DateTimeOffset untilUtc, SnoozeReason reason){if(taskId==Guid.Empty)throw new ArgumentException("Task id required.",nameof(taskId));if(untilUtc<=_clock.UtcNow)throw new ArgumentOutOfRangeException(nameof(untilUtc));if(!Enum.IsDefined(typeof(SnoozeReason),reason))throw new ArgumentException("Invalid snooze reason.",nameof(reason));var map=await LoadAsync();var now=_clock.UtcNow;var state=new SnoozeState(taskId,untilUtc,reason,map.TryGetValue(taskId,out var old)?old.CreatedAtUtc:now,now);map[taskId]=state;await SaveMapAsync(map);await AuditAsync("SnoozeCreated",taskId);SnoozeChanged?.Invoke(this,new SnoozeChangedEventArgs(taskId,state,SnoozeChangeKind.Created));}
        public async Task ClearAsync(Guid taskId){if(taskId==Guid.Empty)throw new ArgumentException("Task id required.",nameof(taskId));var map=await LoadAsync();if(!map.Remove(taskId))return;await SaveMapAsync(map);await AuditAsync("SnoozeCleared",taskId);SnoozeChanged?.Invoke(this,new SnoozeChangedEventArgs(taskId,null,SnoozeChangeKind.Cleared));}
        public async Task<IReadOnlyList<SnoozeState>> GetAllAsync(){var map=await LoadAsync();return map.Values.Where(s=>s.SnoozedUntilUtc>_clock.UtcNow).ToList();}
        private async Task<Dictionary<Guid,SnoozeState>> LoadAsync()=>await _store.LoadAsync<Dictionary<Guid,SnoozeState>>(StorageKey)??new Dictionary<Guid,SnoozeState>();
        private Task SaveMapAsync(Dictionary<Guid,SnoozeState> map)=>_store.SaveAsync(StorageKey,map);
        private Task AuditAsync(string action, Guid taskId)=>_compliance.AuditAsync(new AuditEntry{Module="SnoozeService",Action=action,Result="success",TargetId=taskId,Timestamp=_clock.UtcNow});
    }
}
