# Phase 0 — Msg 3 Spec-Derived Model Shapes

**Status:** approved by Dominic Sarria-Wiley on 2026-05-26
**Grep marker:** `SPEC-DERIVED-MSG3`
**Total derived models:** 4 of 5 (AuditEntry is verbatim from §10.5)
**Companion docs:** Architecture.md v1.0.0, Roadmap.md v1.0.0, HANDOFF.md v1.0.2, `PHASE0-MSG2-DERIVATIONS.md`

---

## Why this file exists

Architecture.md is the single source of truth for model shapes. For four of
the five Phase-0 models, Architecture.md describes their *role* and (for
three) their JSON schemas, but the C# class layouts — including nested type
names and a handful of representation choices — are not specified verbatim.
Per Roadmap D8 (HALT protocol), the agent stopped, asked, and received owner
approval before generating shapes. This file records each derivation so any
future agent (Codex, Claude Code, human reviewer) can locate, audit, and
re-validate every approved-but-not-spec'd model shape.

## How to use this file at handoff

1. `grep -r "SPEC-DERIVED-MSG3" src/` — locates every model file carrying a derived shape.
2. `grep -r "SPEC-DERIVED-MSG3" docs/` — locates this registry.
3. Cross-check entries below against Phase 1A (TaskEngine using TaskNode),
   Phase 1C (AuditEntry usage), Phase 1D (ReminderEvent usage),
   Phase 3A (UpdateManifest deserialization), Phase 3D (BugReport pipeline).
4. Promotion path: amend Architecture.md, bump to v1.0.x, record a
   Superseded-by row.

---

## Derivation Registry

### 1. TaskNode

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Models/TaskNode.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all five models as proposed with all gap tracking thoroughly recorded for later handoffs" |
| Source-of-truth section(s) | §4.2 (`ITaskEngine.AddAsync(TaskNode, Guid? parentId)`), §1.1 (priority 1–5, deadlines, hierarchy), §2 (F2/F3/F4), §5.3 (priority drives cadence), Roadmap 2C (drag-drop mutates Children) |
| Spec gap | Architecture references `TaskNode` in every interface signature but never declares its field surface. |
| Derivation rationale | • `Id` + `ParentId` — hierarchy requires nullable parent (root = null) and stable identifier (CRUD per §4.2).<br>• `Title` — F2/F3 add/edit/display in tray and tree.<br>• `Priority` enum — F2 priority 1–5; binds to §5.3 cadence.<br>• `Deadline` nullable — F4 "Deadlines with date + time"; tasks without deadlines are explicitly supported (paper sticky-note replacement, §1.1).<br>• `Status` enum — F2 add/edit/delete + Roadmap P1A-AC2 ("Status = Done").<br>• `CreatedAt`/`ModifiedAt` — required for §10.5 audit context and §15 cadence calculations.<br>• `IList<TaskNode> Children` — §2C drag-drop reordering mutates in place.<br>• Mutable sealed class chosen over record because §2C mutations + System.Text.Json (§4.5 SecureStore wire format) prefer settable properties. |
| Approved surface | Full class as proposed in HALT message (Phase 0 Msg 3). |
| Downstream consumers | TaskEngine (Phase 1A), ReminderScheduler (Phase 1D, via `ReminderEvent.Node`), SecureStore persistence (Phase 1B), TreeViewUI binding (Phase 2B), DragDropBehavior (Phase 2C). |
| Re-validation tasks for handoff agent | - Confirm System.Text.Json round-trip preserves `Children` ordering (drag-drop persistence depends on this).<br>- Confirm Phase 1A serialization handles cyclic prevention (parent/child loops MUST NOT serialize infinitely).<br>- If a `Description`/`Notes` field is needed in UI (Phase 2B/2E), amend Architecture §4.2 first — do NOT silently add.<br>- Confirm `CreatedAt`/`ModifiedAt` are always sourced from `IClock` per Roadmap 1A anti-drift. |
| Risk if shape is wrong | Tree corruption on persistence round-trip; lost edits; UI binding mismatch. |
| Promotion path | Amend §4.2 per §Governance; bump to v1.0.x; add Superseded-by row. |

### 2. ReminderEvent

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Models/ReminderEvent.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all five models as proposed with all gap tracking thoroughly recorded for later handoffs" |
| Source-of-truth section(s) | §4.3 (`IReminderScheduler.ReminderDue` event), §5.3 (cadence bands), Roadmap P1D-AC3 ("includes node + reason"). |
| Spec gap | §4.3 names the event payload type but never declares fields. |
| Derivation rationale | • `Node` — P1D-AC3 explicit.<br>• `Reason` as `string` — P1D-AC3 says "reason"; not adding a new enum keeps §3.3 enum count at 5.<br>• `FiredAt` — needed for §10.5 audit correlation and §15 latency measurement.<br>• `Cadence` — captures which §5.3 band triggered the firing for §2G escalation decisions. |
| Approved surface | Full class as proposed in HALT message (Phase 0 Msg 3). |
| Downstream consumers | ReminderScheduler (Phase 1D), ReminderDeliveryService (Phase 1G), SnoozeService (Phase 2G), Tier 2 Toast XAML binding (Phase 2B). |
| Re-validation tasks for handoff agent | - Confirm `Reason` string values stay limited to {`Initial`, `Cadence`, `Overdue`, `Escalation`}. If a new value is needed (e.g., `Snoozed-resumed`), document it here before adding.<br>- Confirm Phase 2G Escalation flips a flag observable by the UI binding (currently `ReminderEvent` has no `IsEscalated` bool — escalation is conveyed via `Cadence` band; verify this is sufficient at integration time).<br>- If telemetry adds a unique event id later, append it as a new property — do NOT repurpose existing fields. |
| Risk if shape is wrong | Reminder UI cannot determine why a toast fired; escalation logic misroutes; audit log loses cadence context. |
| Promotion path | Amend §4.3 per §Governance. |

### 3. UpdateManifest (nested type names + Channel-as-string)

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Models/UpdateManifest.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all five models as proposed with all gap tracking thoroughly recorded for later handoffs" |
| Source-of-truth section(s) | §9.1.2 (JSON schema), §9.1.3 (signature model), §4.7 (`IAutoUpdater.Channel` typed as `UpdateChannel` enum). |
| Spec gap | §9.1.2 specifies the JSON schema verbatim but does not name nested C# types and uses `channel` as a string while §4.7 uses the `UpdateChannel` enum. |
| Derivation rationale | • Nested class names `PackageInfo` and `SignatureInfo` — chosen for clarity; alternative was anonymous `JsonElement`.<br>• `Channel` field as `string` — matches §9.1.2 JSON verbatim and avoids forcing a System.Text.Json converter for enum serialization. Conversion to `UpdateChannel` happens explicitly at the AutoUpdater boundary in Phase 3A.<br>• Mutable settable properties — required for System.Text.Json deserialization without `[JsonConstructor]` attribute (kept simple per §19 chat-first strategy). |
| Approved surface | As proposed in HALT message (Phase 0 Msg 3). |
| Downstream consumers | AutoUpdater (Phase 3A), ManifestSigner (Phase 3A), OfflineImportService (Phase 3C), UpdaterStateMachine (Phase 3B). |
| Re-validation tasks for handoff agent | - Confirm Phase 3A parses real manifests using the field order/casing from §9.1.2 (lowerCamelCase JSON ↔ PascalCase C#: `JsonNamingPolicy.CamelCase` or `[JsonPropertyName]` required).<br>- Confirm the string ↔ `UpdateChannel` enum conversion at the AutoUpdater boundary handles unknown channel values gracefully (reject manifest, do not crash).<br>- If §9.1.2 ever adds fields (e.g., `releaseNotesUrl`), add them as new optional properties — do NOT remove existing.<br>- Validate that the JSON property naming policy is set in `TaskTree.App.Bootstrap` (Phase 1F) so all manifest deserialization uses the same policy. |
| Risk if shape is wrong | Manifest parse failures; channel mismatch admits beta build to stable users (§9.1.6 T3); signature verify failure path. |
| Promotion path | Amend §9.1.2 per §Governance. |

### 4. BugReport (nested type names + Type-as-string)

| Field | Value |
| --- | --- |
| File | `src/TaskTree.Core/Models/BugReport.cs` |
| Approved by | Dominic Sarria-Wiley |
| Approval date | 2026-05-26 |
| Approval message | "Approve all five models as proposed with all gap tracking thoroughly recorded for later handoffs" |
| Source-of-truth section(s) | §9.2.1 (JSON schema), §9.2.2 (capture policy), §9.2.4 (routing), §4.8 (`IBugReporter.SubmitAsync(BugReport)`). |
| Spec gap | §9.2.1 specifies JSON schema verbatim but does not name nested C# types and the `type` field has no matching enum in §3.3 (only 5 enums: Priority, TaskStatus, ReminderCadence, UpdateChannel, BugSeverity). |
| Derivation rationale | • Nested class names `DescriptionInfo`, `EnvironmentInfo`, `AttachmentInfo` — clarity over `JsonElement`.<br>• `Type` field as `string` (`"crash"`, `"user_submitted"`, `"regression"`) — adding `BugType` enum would expand §3.3's 5-enum list, which would itself require a §3.3 amendment. Keeping `string` preserves the §3.3 contract verbatim.<br>• `Severity` stays the existing `BugSeverity` enum (already in §3.3).<br>• Mutable settable properties — System.Text.Json deserialization. |
| Approved surface | As proposed in HALT message (Phase 0 Msg 3). |
| Downstream consumers | BugReporter (Phase 3D), CrashCaptureHook (Phase 3D), RedactionPipeline (Phase 3D), EmailDeliveryAdapter / GitHubIssueAdapter / FileDropAdapter (Phase 3E). |
| Re-validation tasks for handoff agent | - Confirm Phase 3D `RedactionPipeline` visits every string field (`Title`, `Description.Expected`, `Description.Actual`, `Attachments[].Name`) per §9.2.3.<br>- Confirm Phase 3E routes by `BugSeverity` per §9.2.4 table.<br>- If a fourth `Type` value emerges (e.g., `"feedback"`), document it here BEFORE adding to the code.<br>- Confirm `Fingerprint` is deterministic across restarts (dedup per §9.2.1). |
| Risk if shape is wrong | PHI leakage (HIPAA breach §10.2); duplicate GitHub Issues; mis-routed severity. |
| Promotion path | Amend §9.2.1 per §Governance. |

---

## Cross-reference table

| Model | Path | Derived elements | Phases that consume it |
| --- | --- | --- | --- |
| `TaskNode` | `src/TaskTree.Core/Models/TaskNode.cs` | entire shape | Phase 1A, 1B, 2B, 2C |
| `ReminderEvent` | `src/TaskTree.Core/Models/ReminderEvent.cs` | entire shape | Phase 1D, 1G, 2G |
| `UpdateManifest` | `src/TaskTree.Core/Models/UpdateManifest.cs` | nested type names; `Channel` as `string` | Phase 3A, 3B, 3C |
| `BugReport` | `src/TaskTree.Core/Models/BugReport.cs` | nested type names; `Type` as `string` | Phase 3D, 3E |
| `AuditEntry` | `src/TaskTree.Core/Models/AuditEntry.cs` | (none — verbatim §10.5) | Phase 1C, 1F |

---

## Promotion / change procedure

If any of these 4 shapes needs to change after handoff:

1. Open a §Governance amendment against Architecture.md (bump to v1.0.x).
2. Add the formal model schema or nested type names to the appropriate §4.x
   or §9.x section.
3. Update this file with a "Superseded by Architecture.md v1.0.x §y.z on
   YYYY-MM-DD" row.
4. Update HANDOFF.md §Decisions Log + §Gap Summary.
5. Re-run the grep helper script (`tools/find-spec-derivations.ps1`) to
   confirm no orphan `SPEC-DERIVED-MSG3` markers remain.

---

## Document history

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 1.0.0 | 2026-05-26 | Dominic Sarria-Wiley (via Claude Opus) | Initial registry — 4 model shapes derived during Phase 0 Msg 3. |
