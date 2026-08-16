// SPEC-DERIVED-PHASE2G  HALT #9/#10/#11/#12/#13
// Gap #186 closed by ServiceRegistrations patch. Gap #187: verify expired auto-clear behavior.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly SemaphoreSlim _gate = new(1, 1);
        public event EventHandler<SnoozeChangedEventArgs>? SnoozeChanged;
        public SnoozeService(ISecureStore secureStore, IComplianceCore compliance, IClock clock, IAppLogger logger){_store=secureStore??throw new ArgumentNullException(nameof(secureStore));_compliance=compliance??throw new ArgumentNullException(nameof(compliance));_clock=clock??throw new ArgumentNullException(nameof(clock));_logger=logger??throw new ArgumentNullException(nameof(logger));}
        public async Task<SnoozeState?> GetAsync(Guid taskId)
        {
            if(taskId==Guid.Empty)throw new ArgumentException("Task id required.",nameof(taskId));
            SnoozeChangedEventArgs? changed = null;
            SnoozeState? result;
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var map=await LoadAsync().ConfigureAwait(false);
                if(!map.TryGetValue(taskId,out var state)) result=null;
                else if(state.SnoozedUntilUtc<=_clock.UtcNow)
                {
                    var previous = new Dictionary<Guid,SnoozeState>(map);
                    map.Remove(taskId);
                    await SaveMapAsync(map).ConfigureAwait(false);
                    try
                    {
                        await AuditAsync("SnoozeExpired",taskId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await RestoreMapAsync(previous, hadPersistedMap: true, ex).ConfigureAwait(false);
                        throw;
                    }
                    changed=new SnoozeChangedEventArgs(taskId,null,SnoozeChangeKind.Expired);
                    result=null;
                }
                else result=state;
            }
            finally { _gate.Release(); }
            if(changed is not null) SnoozeChanged?.Invoke(this,changed);
            return result;
        }
        public async Task SnoozeAsync(Guid taskId, DateTimeOffset untilUtc, SnoozeReason reason)
        {
            if(taskId==Guid.Empty)throw new ArgumentException("Task id required.",nameof(taskId));
            if(untilUtc<=_clock.UtcNow)throw new ArgumentOutOfRangeException(nameof(untilUtc));
            if(!Enum.IsDefined(typeof(SnoozeReason),reason))throw new ArgumentException("Invalid snooze reason.",nameof(reason));
            SnoozeChangedEventArgs? changed = null;
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var hadPersistedMap = await _store.ExistsAsync(StorageKey).ConfigureAwait(false);
                var map=await LoadAsync().ConfigureAwait(false);
                var previous = new Dictionary<Guid,SnoozeState>(map);
                var now=_clock.UtcNow;
                var state=new SnoozeState(taskId,untilUtc,reason,map.TryGetValue(taskId,out var old)?old.CreatedAtUtc:now,now);
                map[taskId]=state;
                await SaveMapAsync(map).ConfigureAwait(false);
                try
                {
                    await AuditAsync("SnoozeCreated",taskId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await RestoreMapAsync(previous, hadPersistedMap, ex).ConfigureAwait(false);
                    throw;
                }
                changed=new SnoozeChangedEventArgs(taskId,state,SnoozeChangeKind.Created);
            }
            finally { _gate.Release(); }
            if(changed is not null) SnoozeChanged?.Invoke(this,changed);
        }
        public async Task ClearAsync(Guid taskId)
        {
            if(taskId==Guid.Empty)throw new ArgumentException("Task id required.",nameof(taskId));
            var changed = false;
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var map=await LoadAsync().ConfigureAwait(false);
                if(map.TryGetValue(taskId, out _))
                {
                    var previous = new Dictionary<Guid,SnoozeState>(map);
                    map.Remove(taskId);
                    await SaveMapAsync(map).ConfigureAwait(false);
                    try
                    {
                        await AuditAsync("SnoozeCleared",taskId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await RestoreMapAsync(previous, hadPersistedMap: true, ex).ConfigureAwait(false);
                        throw;
                    }
                    changed=true;
                }
            }
            finally { _gate.Release(); }
            if(changed) SnoozeChanged?.Invoke(this,new SnoozeChangedEventArgs(taskId,null,SnoozeChangeKind.Cleared));
        }
        public async Task<IReadOnlyList<SnoozeState>> GetAllAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var map=await LoadAsync().ConfigureAwait(false);
                return map.Values.Where(s=>s.SnoozedUntilUtc>_clock.UtcNow).ToList();
            }
            finally { _gate.Release(); }
        }
        private async Task<Dictionary<Guid,SnoozeState>> LoadAsync()=>await _store.LoadAsync<Dictionary<Guid,SnoozeState>>(StorageKey)??new Dictionary<Guid,SnoozeState>();
        private Task SaveMapAsync(Dictionary<Guid,SnoozeState> map)=>_store.SaveAsync(StorageKey,map);
        private async Task RestoreMapAsync(Dictionary<Guid,SnoozeState> previous, bool hadPersistedMap, Exception auditException)
        {
            try
            {
                if (hadPersistedMap)
                    await SaveMapAsync(previous).ConfigureAwait(false);
                else
                    await _store.DeleteAsync(StorageKey).ConfigureAwait(false);
            }
            catch (Exception rollbackException)
            {
                _logger.LogError(rollbackException, "SnoozeService failed to restore state after audit failure: {0}: {1}; original: {2}: {3}", rollbackException.GetType().Name, rollbackException.Message, auditException.GetType().Name, auditException.Message);
            }
        }
        private Task AuditAsync(string action, Guid taskId)=>_compliance.AuditAsync(new AuditEntry{Module="SnoozeService",Action=action,Result="success",TargetId=taskId,Timestamp=_clock.UtcNow});
    }
}
