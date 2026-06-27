# PHASE2G-DERIVATIONS.md - SnoozeService + EscalationPolicy

> Scope: ISnoozeService + SnoozeService + EscalationPolicyEvaluator + ReminderDeliveryService integration.
> Owner-approved HALT batch: 24 items.

## Section 1 Summary
All 24 HALT items approved as proposed. Snooze state persists under `snooze/state`, ReminderDeliveryService checks active snooze before tier cascade, and Phase 2 is marked COMPLETE.

## Section 2 Files Produced
16 artifacts emitted per HALT #23, including Core models/enums/abstraction, Snooze module, ReminderDeliveryService/ServiceRegistrations patches, tests, derivations, HANDOFF, and tooling.

## Section 3 Marker Inventory
PHASE2G = 11 distinct .cs files. Grand total: 115 -> 126.

## Section 4 Cross-Phase Gaps Introduced (180-193)
| # | Gap | Target | Action |
|---|---|---|---|
| 180 | Add ISnoozeService Architecture Section 4 subsection | Architecture v1.0.3 | Document interface |
| 181 | Add src/TaskTree.Modules.Snooze/ to Architecture Section 3.3 | Architecture v1.0.3/Phase 5F | Amend folder tree |
| 182 | Add SnoozeReason.cs to Architecture Enums | Architecture v1.0.3 | Amend enum list |
| 183 | Verify snooze-store serialization/backward compatibility | Phase 5C | Storage tests |
| 184 | Final escalation policy semantics require clinical/operational review | Phase 4A/5E | Policy review |
| 185 | Promote evaluator to service if escalation becomes configurable/audited | Future HALT | Refactor if needed |
| 186 | SnoozeService audit injection registration | Closed by Msg | ServiceRegistrations patch |
| 187 | Expired snooze auto-clear audit/storage churn verification | Phase 5C | Integration tests |
| 188 | Compliance policy documents Snooze audit vocabulary | Phase 4A | Audit vocabulary |
| 189 | Move SnoozeChangeKind to Core.Enums if reused broadly | Future refactor | Reassess usage |
| 190 | Verify snooze skip with real scheduler due events | Phase 5C | Integration tests |
| 191 | Compliance policy documents DeliverySkippedSnoozed | Phase 4A | Audit vocabulary |
| 192 | ReminderToast Snooze button/callback deferred | Phase 5E/future UI | Add callback/button |
| 193 | ReminderDeliveryService test backfill includes snooze skip | Phase 5C | Backfill skeleton |

## Section 5 Phase 2 Completion
Phase 2 COMPLETE: 2A through 2G emitted. Remaining work is primarily Architecture v1.0.3 amendments, Phase 4A compliance policy, Phase 5B compile/test churn, Phase 5C integration/backfill, and Phase 5E live OS/WPF hooks.
