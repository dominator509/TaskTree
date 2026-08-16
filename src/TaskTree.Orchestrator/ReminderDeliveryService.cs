// SPEC-DERIVED-PHASE1G-MSG2  ReminderDeliveryService baseline
// SPEC-DERIVED-PHASE2G  HALT #15/#16 snooze skip before tier cascade
// Gap #190/#191/#193: verify with real scheduler due events and document audit vocabulary.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator
{
    public sealed class ReminderDeliveryService : IReminderDeliveryService
    {
        private readonly IReminderScheduler _scheduler; private readonly ToastTier1Adapter _tier1; private readonly ToastTier2Adapter _tier2; private readonly ToastTier3Adapter _tier3; private readonly IClock _clock; private readonly IAppLogger _logger; private readonly IComplianceCore _compliance; private readonly ISnoozeService _snooze;
        private readonly object _stateGate = new();
        private readonly HashSet<Task> _inFlight = new();
        private CancellationTokenSource? _deliveryCts;
        private bool _running;
        public ReminderDeliveryService(IReminderScheduler scheduler, ToastTier1Adapter tier1, ToastTier2Adapter tier2, ToastTier3Adapter tier3, IClock clock, IAppLogger logger, IComplianceCore compliance, ISnoozeService snooze){_scheduler=scheduler;_tier1=tier1;_tier2=tier2;_tier3=tier3;_clock=clock;_logger=logger;_compliance=compliance;_snooze=snooze;}
        public Task StartAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            lock(_stateGate)
            {
                if(_running)throw new InvalidOperationException("ReminderDeliveryService already running.");
                var cts=CancellationTokenSource.CreateLinkedTokenSource(ct);
                try
                {
                    _deliveryCts=cts;
                    _running=true;
                    _scheduler.ReminderDue+=OnReminderDue;
                }
                catch
                {
                    _deliveryCts=null;
                    _running=false;
                    cts.Dispose();
                    throw;
                }
            }
            return Task.CompletedTask;
        }
        public async Task StopAsync()
        {
            CancellationTokenSource? cts;
            Task[] pending;
            lock(_stateGate)
            {
                if(!_running)return;
                _running=false;
                _scheduler.ReminderDue-=OnReminderDue;
                cts=_deliveryCts;
                _deliveryCts=null;
                cts?.Cancel();
                pending=new Task[_inFlight.Count];
                _inFlight.CopyTo(pending);
            }
            try
            {
                if(pending.Length>0) await Task.WhenAll(pending).ConfigureAwait(false);
            }
            finally { cts?.Dispose(); }
        }
        private void OnReminderDue(object? sender, ReminderEvent evt)
        {
            TaskCompletionSource<bool> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Task delivery;
            lock(_stateGate)
            {
                if(!_running || _deliveryCts is null)return;
                delivery=DeliverTrackedAsync(evt,_deliveryCts.Token,ready.Task);
                _inFlight.Add(delivery);
            }
            ready.SetResult(true);
            delivery.ContinueWith(RemoveCompletedDelivery,CancellationToken.None,TaskContinuationOptions.ExecuteSynchronously,TaskScheduler.Default);
        }
        private async Task DeliverTrackedAsync(ReminderEvent evt,CancellationToken ct,Task ready)
        {
            await ready.ConfigureAwait(false);
            await DeliverAsync(evt,ct).ConfigureAwait(false);
        }
        private void RemoveCompletedDelivery(Task delivery)
        {
            lock(_stateGate) _inFlight.Remove(delivery);
        }
        private async Task DeliverAsync(ReminderEvent evt,CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                var active=await _snooze.GetAsync(evt.TaskId).ConfigureAwait(false);
                ct.ThrowIfCancellationRequested();
                if(active is not null)
                {
                    _logger.LogInformation("Reminder delivery skipped: task is snoozed.");
                    ct.ThrowIfCancellationRequested();
                    await _compliance.AuditAsync(new AuditEntry{Module="ReminderDelivery",Action="DeliverySkippedSnoozed",Result="success",TargetId=evt.TaskId,Timestamp=_clock.UtcNow}).ConfigureAwait(false);
                    return;
                }
                if(_tier1.TryDeliver(evt)){ct.ThrowIfCancellationRequested();await AuditAsync("DeliveredViaTier1",evt).ConfigureAwait(false);return;}
                if(_tier2.TryDeliver(evt)){ct.ThrowIfCancellationRequested();await AuditAsync("DeliveredViaTier2",evt).ConfigureAwait(false);return;}
                if(_tier3.TryDeliver(evt)){ct.ThrowIfCancellationRequested();await AuditAsync("DeliveredViaTier3",evt).ConfigureAwait(false);return;}
                ct.ThrowIfCancellationRequested();
                await AuditAsync("DeliveryFailedAllTiers",evt).ConfigureAwait(false);
            }
            catch(OperationCanceledException) when(ct.IsCancellationRequested)
            {
                _logger.LogDebug("Reminder delivery canceled during service shutdown.");
            }
            catch(Exception ex){_logger.LogError(ex,"Reminder delivery failed: {0}: {1}",ex.GetType().Name,ex.Message);}
        }
        private Task AuditAsync(string action, ReminderEvent evt)=>_compliance.AuditAsync(new AuditEntry{Module="ReminderDelivery",Action=action,Result="success",TargetId=evt.TaskId,Timestamp=_clock.UtcNow});
    }
}
