# Architecture.md v1.0.2 Final Promotion Manifest (Gap #110)

> **Status:** NOT YET APPLIED - owner sign-off required.
> **Format:** Option B per HALT-Msg2 #9 - prose-only manifest (full document rewrite avoided to preserve v1.0.1 to v1.0.2 audit trail).
> **Trigger:** Upon owner approval of `Architecture.v1.0.2-delta.md` (7 bundled changes).
> **Author:** DSW - **Date:** 2026-05-29

## Section 1 Scope

This manifest documents the exact prose insertions and section bumps required to promote Architecture.md from v1.0.1 to v1.0.2 once owner approves the amendment. Codex Phase 5B compile gate verifies this promotion has been applied OR explicitly notes non-application as **Gap #113**.

## Section 2 Document Header Updates

- Update header line: `"Source of Truth Document - v1.0.1"` -> `"v1.0.2"`
- Update `Last Updated` date field to date of owner approval
- Update Section 0 Disaster Recovery Rebuild Preamble version reference if present

## Section 3 Change Applications

### Change 1 - Section 3.3 Enums folder (REQUIRED)

Locate the `Enums/` subdirectory tree under `src/TaskTree.Core/` in Section 3.3 prose folder listing. Insert `ReminderReason.cs` as the 6th file (between `BugSeverity.cs` and the closing tree marker).

### Change 2 - Section 4.3 ReminderScheduler (OPTIONAL)

If approved: insert new paragraph at end of "Performance Target" subsection with Cadence semantic prose (see Architecture.v1.0.2-delta.md Section 3 Change 2).

### Change 3 - Section 4.1 TrayHost (REQUIRED IF PHASE 1E)

Insert new paragraph at end of "Handoff Notes" subsection with internal-raise testing pattern prose.

### Change 4 - Section 4.7 IOrchestrator (REQUIRED IF PHASE 1F)

Insert NEW subsection Section 4.7 (renumbering existing Section 4.7 onward if any) with full 2-method declaration + 4 invariants prose.

### Change 5 - Section 3.3 Enums folder (REQUIRED IF PHASE 1G)

Insert `ReminderDeliveryTier.cs` after `ReminderReason.cs` (covered in Change 1 diff if applied as combined change).

### Change 6 - Section 4.9 IReminderDeliveryService (REQUIRED IF PHASE 1G)

Insert NEW subsection Section 4.9 with 2-method declaration + 4 invariants prose.

### Change 7 - Section 3.3 tests folder (REQUIRED IF PHASE 2A)

Locate the `tests/` subdirectory tree in Section 3.3 prose. Insert `tests/TaskTree.TestSupport/` as the 10th directory entry with comment "added v1.0.2 (shared test helpers)".

## Section 4 Version History Append

Add row to Section 18 Version History table:

| Version | Date | Author | Changes |
|---|---|---|---|
| 1.0.2 | TBD | Dominic Sarria-Wiley | Section 3.3 Enums folder gains ReminderReason.cs + ReminderDeliveryTier.cs. OPTIONAL Section 4.3 formalizes Cadence clamp. Section 4.1 TrayHost blesses internal-raise pattern. NEW Section 4.7 IOrchestrator subsection. NEW Section 4.9 IReminderDeliveryService subsection. Section 3.3 tests folder adds TaskTree.TestSupport project. All 7 changes purely additive; no module API surface impact. |

## Section 5 Application Order

Recommended order:

1. Apply Change 7 first (small Section 3.3 tests addition).
2. Apply Changes 1 and 5 to Section 3.3 Enums folder (small additions).
3. Apply Change 2 if approved to Section 4.3 prose.
4. Apply Change 3 to Section 4.1 prose.
5. Apply Changes 4 and 6 as new subsections Section 4.7 and Section 4.9.
6. Update document header version to v1.0.2 and Last Updated date.
7. Append Section 18 Version History row.

## Section 6 Phase 5B Compile Gate Verification

Codex must verify:

1. Architecture.md Document Version reads `1.0.2`.
2. Section 3.3 Enums folder contains `ReminderReason.cs` + `ReminderDeliveryTier.cs` entries.
3. Section 3.3 tests folder contains `TaskTree.TestSupport/` entry.
4. Section 4.1 contains internal-raise testing pattern paragraph.
5. Section 4.3 contains Cadence semantic paragraph IF Change 2 was approved; ELSE log as deferred-to-v1.0.3 (OK per Gap #101).
6. Section 4.7 IOrchestrator subsection exists with 4 invariants prose.
7. Section 4.9 IReminderDeliveryService subsection exists with 4 invariants prose.
8. Section 18 Version History contains v1.0.2 row.

## Section 7 Non-Application Documentation

If owner approves only some changes (e.g., all REQUIRED but defers OPTIONAL Change 2), the application status table must record per-change accept/defer/reject status:

| Change | Status | Date | Notes |
|---|---|---|---|
| 1 | TBD | TBD | |
| 2 | TBD | TBD | |
| 3 | TBD | TBD | |
| 4 | TBD | TBD | |
| 5 | TBD | TBD | |
| 6 | TBD | TBD | |
| 7 | TBD | TBD | |

Phase 5F sign-off gate enforces ALL REQUIRED changes (1, 3, 4, 5, 6, 7) are applied.

## Section 8 Known Limitations

1. This manifest assumes Architecture.md v1.0.1 is the current published version; if owner has made interim edits the manifest may need adjustment.
2. Section numbering for new Section 4.7 and Section 4.9 subsections assumes no other Section 4 subsections have been added since v1.0.1 - Codex must verify and adjust numbering if needed.
3. Change 2 OPTIONAL status: Gap #101 indicates implicit deferral to v1.0.3; this manifest still documents the Change 2 prose for completeness in case owner chooses to include in v1.0.2 promotion.
4. Architecture.md v1.0.2 promotion does not require any code changes - this is a documentation-only amendment.
