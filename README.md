# TaskTree — Next Chat Handoff Bundle (Complete)

This bundle contains **everything a fresh chat session needs** to resume the TaskTree build at **Phase 1D Msg 1**. No manual delta-merging required.

## What's in this bundle

| File | Version | Purpose |
|---|---|---|
| `docs/Architecture.md` | **v1.0.1** | Locked source-of-truth for structure, naming, patterns, dependencies, technology. Includes the Phase 0 Msg 6 amendment (added TrayHost.Tests + UI.Tests to §3.3). |
| `docs/Roadmap.md` | **v1.0.0** | Locked source-of-truth for build order, phasing, acceptance criteria. All 6 phases + 32 sub-phases. |
| `docs/HANDOFF.md` | **v1.0.15** (consolidated) | Live state tracker. Self-sufficient — covers all 14 prior deltas, all decisions, all open questions, 31-row cross-phase gap table, ready-to-paste continuation prompt. |
| `README.md` | — | This file |

## How to use this bundle

### Step 1 — Open a fresh chat

Start a new chat session with Claude Opus (or comparable agent).

### Step 2 — Attach the 3 docs

Attach all three files from `docs/`:
- `Architecture.md` (v1.0.1)
- `Roadmap.md` (v1.0.0)
- `HANDOFF.md` (v1.0.15)

### Step 3 — Paste the Continuation Prompt

Open `HANDOFF.md` and scroll to the section titled **"Continuation Prompt (ready-to-paste for fresh chat)"**. Copy the entire code block inside that section and paste it as your first chat message.

The agent will:
1. Read `HANDOFF.md` to determine current state
2. Confirm Phase 0 + 1A + 1B + 1C are complete (23/35 = 65.7% progress)
3. Begin Phase 1D Msg 1 with the HALT protocol, surfacing **FakeClock promotion as HALT item #1**

## Critical reminders for the next chat

### 🛑 HALT item #1 for Phase 1D Msg 1

Promote `FakeClock` from inline private nested class (currently in `TaskEngineTests`, `ComplianceCoreTests`, `AuditChainWriterTests`) to a shared `TestDoubles` project or namespace. Phase 1D ReminderScheduler tests will be the **4th consumer**; this is the canonical promotion trigger per:
- `PHASE1C-DERIVATIONS.md` §Implementation note
- `PHASE1A-DERIVATIONS.md` §8 re-validation guidance

Two viable promotion targets:
- `tests/TaskTree.Core.Tests/TestDoubles/FakeClock.cs` (smallest diff)
- New `tests/TaskTree.TestSupport/` project (cleaner long-term home)

### 🚨 LOAD-BEARING open questions (block Phase 5F sign-off)

- **Q10**: Source for the §9.2.3 PHI "common-name list". Current synthetic 10-name sentinel set (`Pat Test`, `Sam Sample`, etc.) provides **ZERO production coverage** for names — **HIPAA breach risk if shipped as-is**. Owner must provide real source list before Phase 5F sign-off.

- **Q11**: Support-email allowlist seed for `PhiRedactor`. DI default is empty unless owner populates at Phase 1F composition root. Owner must populate OR explicitly accept empty default before Phase 5F sign-off.

### ⚠️ CRITICAL Phase 1F flags (when wiring DI in `ServiceRegistrations.cs`)

| # | Flag | Source |
|---|---|---|
| 1 | MUST inject `IComplianceCore` into `TaskEngine` constructor (else HIPAA audit chain silently breaks) | PHASE1A §4 |
| 2 | MUST register `MasterKeyManager` as singleton `IMasterKeyManager` (else compile failure on `SecureStore` ctor) | PHASE1B §7 |
| 3 | MUST pass canonical paths: `%LOCALAPPDATA%\TaskTree\keys\`, `\store\`, `\logs\` | PHASE1B §1, §3 + PHASE0-MSG5 §3 |
| 4 | SHOULD populate Q11 allowlist or explicitly accept empty default | PHASE1C §7 |

`HANDOFF.md` §"Consolidated Gap Summary" has the full **31-row cross-phase flag table**.

## State at handoff

- **Phase 0** ✅ (scaffold; 6 Msgs; 16 derivations)
- **Phase 1A** ✅ (TaskEngine; 14 tests; 8 derivations including test infra)
- **Phase 1B** ✅ (SecureStore + MasterKeyManager + IMasterKeyManager; 12 tests; 7 derivations)
- **Phase 1C** ✅ (ComplianceCore + PhiRedactor + AuditChainWriter; 13 tests; 8 derivations)
- **Phase 1D** ⬚ ← next
- **Progress: 23 of 35 → 65.7%**

## Total spec-derived items tracked

**38 derivations across 11 registries.** The per-phase derivations registries (`PHASE0-MSG2/3/4/5/6-DERIVATIONS.md` and `PHASE1A/1B/1C-DERIVATIONS.md`) were delivered in prior Msg bundles — they live in the assembled repo at `docs/spec-derivations/`. 

`HANDOFF.md` §"Consolidated Gap Summary" lists every derivation with its registry of origin, so the next chat has full visibility without needing the individual registries re-attached.

## Build verification

After Phase 5A repo stitching, run:

```powershell
pwsh -File tools/find-spec-derivations.ps1 -Root .
```

Expected marker counts (Phase 1C complete):
- SPEC-DERIVED-MSG2 = 4
- SPEC-DERIVED-MSG3 = 4
- SPEC-DERIVED-MSG4 = 2
- SPEC-DERIVED-MSG5 = 3
- SPEC-DERIVED-MSG6 = 3
- SPEC-DERIVED-PHASE1A = 2
- SPEC-DERIVED-PHASE1B = 5
- SPEC-DERIVED-PHASE1C = 6

Total: **29 distinct `.cs` files carry markers**.

---

End of README.
