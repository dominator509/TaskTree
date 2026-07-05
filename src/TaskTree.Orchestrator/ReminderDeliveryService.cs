// SPEC-DERIVED-PHASE1G-MSG2  ReminderDeliveryService baseline
// SPEC-DERIVED-PHASE2G  HALT #15/#16 snooze skip before tier cascade
// Gap #190/#191/#193: verify with real scheduler due events and document audit vocabulary.

using System;
using System.Threading;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator
{
    public sealed class ReminderDeliveryService : IReminderDeliveryService
    {
        private readonly IReminderScheduler _scheduler; private readonly ToastTier1Adapter _tier1; private readonly ToastTier2Adapter _tier2; private readonly ToastTier3Adapter _tier3; private readonly IClock _clock; private readonly IAppLogger _logger; private readonly IComplianceCore _compliance; private readonly ISnoozeService _snooze;
        private bool _running;
        public ReminderDeliveryService(IReminderScheduler scheduler, ToastTier1Adapter tier1, ToastTier2Adapter tier2, ToastTier3Adapter tier3, IClock clock, IAppLogger logger, IComplianceCore compliance, ISnoozeService snooze){_scheduler=scheduler;_tier1=tier1;_tier2=tier2;_tier3=tier3;_clock=clock;_logger=logger;_compliance=compliance;_snooze=snooze;}
        public Task StartAsync(CancellationToken ct){if(_running)throw new InvalidOperationException("ReminderDeliveryService already running.");_scheduler.ReminderDue+=OnReminderDue;_running=true;return Task.CompletedTask;}
        public Task StopAsync(){if(!_running)return Task.CompletedTask;_scheduler.ReminderDue-=OnReminderDue;_running=false;return Task.CompletedTask;}
        private async void OnReminderDue(object? sender, ReminderEvent evt){try{var active=await _snooze.GetAsync(evt.TaskId);if(active is not null){_logger.LogInformation("Reminder delivery skipped: task is snoozed.");await _compliance.AuditAsync(new AuditEntry{Module="ReminderDelivery",Action="DeliverySkippedSnoozed",Result="success",TargetId=evt.TaskId,Timestamp=_clock.UtcNow});return;}if(_tier1.TryDeliver(evt)){await AuditAsync("DeliveredViaTier1",evt);return;}if(_tier2.TryDeliver(evt)){await AuditAsync("DeliveredViaTier2",evt);return;}if(_tier3.TryDeliver(evt)){await AuditAsync("DeliveredViaTier3",evt);return;}await AuditAsync("DeliveryFailedAllTiers",evt);}catch(Exception ex){_logger.LogError(ex,"Reminder delivery failed: {0}: {1}",ex.GetType().Name,ex.Message);}}
        private Task AuditAsync(string action, ReminderEvent evt)=>_compliance.AuditAsync(new AuditEntry{Module="ReminderDelivery",Action=action,Result="success",TargetId=evt.TaskId,Timestamp=_clock.UtcNow});
    }
}
