// =============================================================================
// TaskTree - PlaceholderReminderDeliveryService.cs
// Implements: IReminderDeliveryService (HALT #4 Option A - Phase 1F placeholder)
// Phase:      1F Msg 1
// SPEC-DERIVED-PHASE1F §3
// REPLACED by Phase 1G ReminderDeliveryService router (Tier 1/2/3 decision tree).
// =============================================================================
using System;
using System.Threading.Tasks;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator;

internal sealed class PlaceholderReminderDeliveryService : IReminderDeliveryService
{
    private readonly IAppLogger _logger;

    public PlaceholderReminderDeliveryService(IAppLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task DeliverAsync(ReminderEvent reminder)
    {
        if (reminder is null) return Task.CompletedTask;
        try
        {
            _logger.LogInformation(
                "[Placeholder] ReminderDue: NodeId={NodeId} Priority={Priority} Reason={Reason} FiredAt={FiredAt}",
                reminder.NodeId, reminder.Priority, reminder.Reason, reminder.FiredAt);
        }
        catch
        {
            // D5: never throw from delivery hot path; scheduler must keep ticking.
        }
        return Task.CompletedTask;
    }
}
