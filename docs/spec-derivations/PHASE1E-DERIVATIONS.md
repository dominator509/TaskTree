# PHASE1E-DERIVATIONS.md — Phase 1E Msg 1 (TrayHost HIGH-stub)

> **Scope:** TrayHost + HotkeyInterop production stubs + InternalsVisibleTo assembly attribute.
> **Companion docs:** Architecture.md v1.0.1 (v1.0.2 amendment pending — scope expanding per HALT #15), Roadmap.md v1.0.0, HANDOFF.md v1.0.18.
> **Owner-approved HALT batch:** 15 items, batch-resolved per proposals.

---

## §1 Summary Table

| # | Item | Resolution | Files Affected | LOAD-BEARING? |
|---|---|---|---|---|
| 1 | `HotkeyInterop` public surface | Hybrid A+C — PInvoke sigs + stubbed wrappers + impl `BuildModifierFlags` | HotkeyInterop.cs | No |
| 2 | `TrayHost` ctor deps | `(IAppLogger, IComplianceCore)` — 3rd audit injection | TrayHost.cs | **YES — Gap #56** |
| 3 | Audit timing in stub | `_compliance` stored; no AuditAsync calls | TrayHost.cs | No (Gap #57) |
| 4 | Throw message format | Canonical `"HIGH: ... — Codex Phase 5E"` for greppable closure | TrayHost.cs + HotkeyInterop.cs | No |
| 5 | P1E-AC2 raise mechanism | 3 `internal Raise*()` methods + `InternalsVisibleTo` | TrayHost.cs + AssemblyInfo.cs | No (Gap #58 + Arch v1.0.2) |
| 6 | `HotkeyInterop` visibility | `public static class` | HotkeyInterop.cs | No |
| 7 | `TrayHost` shape | `public sealed class TrayHost : ITrayHost, IDisposable` | TrayHost.cs | No |
| 8 | `Dispose` | Real idempotent (no Win32 owned in stub) | TrayHost.cs | No |
| 9 | `ShowBalloon` param validation | Validate first, then throw NotImplementedException | TrayHost.cs | No |
| 10 | `Initialize` idempotency | None in stub | TrayHost.cs | No (Gap #59) |
| 11 | Default hotkey constant | Deferred to Phase 2A `HotkeyConfig` | (negative) | No |
| 12 | `_initialized` field | Declared; Codex Phase 5E sets to true | TrayHost.cs | No |
| 13 | Msg structure | Msg 1 = stubs + docs; Msg 2 = tests + Architecture v1.0.2 update | (process) | No |
| 14 | `find-spec-derivations.ps1` | Add `PHASE1E = 3` | tools/find-spec-derivations.ps1 | No |
| 15 | Architecture v1.0.2 scope | Extend to bless internal-raise pattern in §4.1 | docs/Architecture.v1.0.2-delta.md (Msg 2) | No (Gap #60) |

---

## §2 — Item #1: `HotkeyInterop` Public Surface

- **Trigger:** Roadmap §1E names the file; Architecture §4.1 doesn't enumerate its members.
- **Architecture silence:** No interface declared; no methods enumerated.
- **Options considered:**
  - **A.** Static utility class wrapping Win32 PInvoke with public methods.
  - **B.** Instance class implementing future `IHotkeyInterop` interface (over-engineered).
  - **C.** Static class with PInvoke signatures only; wrappers throw `NotImplementedException`.
- **Resolution:** Hybrid A + C.
- **Rationale:** PInvoke declarations compile in chat-only env (only execute on call); `BuildModifierFlags` is pure logic safely implemented now. Avoids inventing `IHotkeyInterop` for a 1-implementation scenario.
- **Files affected:** `HotkeyInterop.cs`.
- **Gap for Handoff (Codex/Claude Code):** None for Phase 1E. Phase 5E Codex implements `Register`/`Unregister` bodies by replacing the throw statement with a real `RegisterHotKey`/`UnregisterHotKey` PInvoke call followed by Win32 error checking (e.g., `if (!RegisterHotKey(...)) throw new Win32Exception(Marshal.GetLastWin32Error());`). PInvoke signatures present so Codex doesn't need to redeclare `DllImport`.

---

## §3 — Item #2: `TrayHost` Constructor Dependencies

- **Trigger:** Phase 1F DI registration needs a concrete ctor shape.
- **Architecture silence:** §4.1 declares interface; no ctor shape specified.
- **Options considered:**
  - **A.** Parameterless (punts DI to Phase 1F).
  - **B.** `(IAppLogger)` minimal.
  - **C.** `(IAppLogger, IClock)` — clock not needed in stub.
  - **D.** `(IAppLogger, IComplianceCore)` — audit posture per §10.5 schema.
- **Resolution:** Option D.
- **Rationale:** User-facing tray events are HIPAA-relevant per §10.5 `actor / module / action / result`. Parallels established TaskEngine R6 + ReminderScheduler HALT-Msg2 #2 LOAD-BEARING pattern. TrayHost becomes the **third** audit-injection module.
- **Files affected:** `TrayHost.cs` ctor + `_compliance` field.
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #56 — LOAD-BEARING Phase 1F.** `ServiceRegistrations.cs` MUST inject `IComplianceCore` as the 2nd ctor parameter. Phase 1F now has **three** audit-injection LOAD-BEARING flags (TaskEngine + ReminderScheduler + TrayHost). Failure to inject surfaces at composition root resolution.

---

## §4 — Item #3: Audit Timing in Stub State

- **Trigger:** Item #2 establishes `IComplianceCore` injection, but `Initialize`/`ShowBalloon` are HIGH-stubbed.
- **Architecture silence:** §10.5 schema present; timing implicit.
- **Options considered:**
  - **A.** Audit on `ShowBalloon` only (also stubbed; pointless).
  - **B.** No audit calls in stub; defer to Codex Phase 5E live wiring.
  - **C.** Audit on event raise (when test mechanism manually fires).
- **Resolution:** Option B.
- **Rationale:** Stub auditing creates fake chain entries indistinguishable from real ones; muddies forensics. Defer to live wiring.
- **Files affected:** `TrayHost.cs` (`_compliance` field present; no `AuditAsync` calls).
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #57 — Phase 5E.** Live TrayHost handlers MUST call `_compliance.AuditAsync(new AuditEntry { Module = "TrayHost", Action = "ShowTreeRequested" | "AddTaskRequested" | "ExitRequested", Result = "success", Timestamp = clock.UtcNow })` on every event raise. Add to Phase 5E TrayHost implementation checklist.

---

## §5 — Item #4: Throw Message Canonical Text

- **Trigger:** Roadmap mandates `NotImplementedException` pattern.
- **Architecture silence:** None — Roadmap dictates.
- **Resolution:** Three canonical messages, all beginning with `"HIGH: "` and ending with `"— Codex Phase 5E"`.
  - `"HIGH: NotifyIcon + RegisterHotKey require live env — Codex Phase 5E"` (Initialize)
  - `"HIGH: NotifyIcon balloon requires live env — Codex Phase 5E"` (ShowBalloon)
  - `"HIGH: RegisterHotKey PInvoke requires live message loop — Codex Phase 5E"` (HotkeyInterop.Register)
  - `"HIGH: UnregisterHotKey PInvoke requires live message loop — Codex Phase 5E"` (HotkeyInterop.Unregister)
- **Files affected:** `TrayHost.cs`, `HotkeyInterop.cs`.
- **Gap for Handoff (Codex/Claude Code):** Phase 5E gap closure uses `Select-String -Pattern "HIGH:.*Codex Phase 5E"` to enumerate all 4 stubs needing implementation. Replace the `throw new NotImplementedException(...)` lines with real implementation while preserving signature.

---

## §6 — Item #5: Event Manual-Raise Mechanism (P1E-AC2)

- **Trigger:** P1E-AC2: "Events raise correctly via reflection test."
- **Architecture silence:** No test mechanism specified.
- **Options considered:**
  - **A.** Internal `RaiseShowTreeRequested()`, `RaiseAddTaskRequested()`, `RaiseExitRequested()` + `InternalsVisibleTo`.
  - **B.** Pure reflection via `EventInfo.GetRaiseMethod()` (brittle; auto-generated raise methods often null in C#).
  - **C.** Public `Raise*ForTesting()` methods (pollutes public API).
- **Resolution:** Option A.
- **Rationale:** Parallels Phase 1D Msg 2 HALT-Msg2 #1 InternalsVisibleTo precedent (now 2nd consumer of the pattern). Reflection adds brittleness without benefit; public methods violate §1.4 "Quiet by Default."
- **Files affected:** `TrayHost.cs` (3 internal methods) + **NEW** `Properties/AssemblyInfo.cs`.
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #58 — Architecture v1.0.2 amendment expansion (per Item #15).** §4.1 prose should explicitly bless internal-raise OR explicitly allow reflection-only. Phase 5B compile gate verifies `InternalsVisibleTo` scope = exactly 1 attribute line targeting `TaskTree.Modules.TrayHost.Tests` only.

---

## §7 — Item #6: `HotkeyInterop` Class Visibility

- **Trigger:** Standard implementation choice.
- **Architecture silence:** No public/internal designation.
- **Options:** A) `public static class` · B) `internal static class`.
- **Resolution:** Option A.
- **Rationale:** Future Phase 2A `HotkeyManager` will consume; `BuildModifierFlags` is broad-utility pure logic.
- **Files affected:** `HotkeyInterop.cs`.
- **Gap for Handoff:** None.

---

## §8 — Item #7: `TrayHost` Class Shape

- **Resolution:** `public sealed class TrayHost : ITrayHost, IDisposable`.
- **Rationale:** Consistent with all prior Phase 1 modules (TaskEngine, SecureStore, ComplianceCore, ReminderScheduler).
- **Files affected:** `TrayHost.cs`.
- **Gap for Handoff:** None.

---

## §9 — Item #8: `IDisposable` Contract

- **Options considered:**
  - **A.** Stub `Dispose()` with `NotImplementedException`.
  - **B.** Real `Dispose()` — sets `_disposed = true`; logger call protected.
- **Resolution:** Option B.
- **Rationale:** No Win32 resources owned in stub state (NotifyIcon not created until Initialize, which throws). Dispose can be implemented and tested now without Codex.
- **Files affected:** `TrayHost.cs`.
- **Gap for Handoff (Codex/Claude Code):** Codex Phase 5E extends `Dispose` to:
  1. Call `HotkeyInterop.Unregister(_hwnd, _hotkeyId)` if `_initialized`.
  2. Dispose the `NotifyIcon` instance.
  3. Then run the existing `_logger.LogInformation` + `_disposed = true` body.
  Existing stub body is forward-compatible — Codex inserts new disposals BEFORE the existing lines.

---

## §10 — Item #9: `ShowBalloon` Param Validation

- **Options:** A) No validation; immediate throw · B) Validate first, then throw NotImplementedException · C) Silent suppress per Quiet pillar.
- **Resolution:** Option B.
- **Rationale:** Validation is pure C# and testable NOW (Phase 1E Msg 2) without Codex. Fails fast on caller bugs.
- **Files affected:** `TrayHost.cs`.
- **Gap for Handoff (Codex/Claude Code):** Phase 1E Msg 2 must include tests for:
  - `null` title → `ArgumentNullException`.
  - `null` message → `ArgumentNullException`.
  - Empty string title/message → `ArgumentException`.
  - Whitespace-only title/message → `ArgumentException`.
  - Valid inputs → `NotImplementedException` (proves validation passes before stub throw).

---

## §11 — Item #10: `Initialize` Idempotency Check

- **Trigger:** D9 (testable behavior) might suggest idempotency check.
- **Options:** A) No check (stub always throws) · B) Check `_initialized` first; throw `InvalidOperationException` if true, else NotImplementedException.
- **Resolution:** Option A.
- **Rationale:** Pre-engineering behavior for a method that always throws is meaningless. Codex Phase 5E adds the check alongside live impl.
- **Files affected:** `TrayHost.cs`.
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #59 — Phase 5E.** Codex `Initialize()` must begin with:
  ```csharp
  ThrowIfDisposed();
  if (_initialized) throw new InvalidOperationException("TrayHost already initialized.");
  ```
  before NotifyIcon creation. Add to Phase 5E TrayHost implementation checklist.

---

## §12 — Item #11: Default Hotkey Constant

- **Trigger:** Architecture §13 specifies Ctrl+Alt+T.
- **Options:** A) Add `DefaultHotkeyDescription` constant in TrayHost/HotkeyInterop · B) Defer to Phase 2A `HotkeyConfig`.
- **Resolution:** Option B.
- **Rationale:** Phase 2A `HotkeyConfig` per Roadmap is the canonical home. Adding here creates drift on later migration.
- **Files affected:** None (negative).
- **Gap for Handoff (Codex/Claude Code):** Phase 2A `HotkeyConfig` must define a default descriptor matching §13:
  ```csharp
  public static readonly HotkeyDescriptor Default = new(Ctrl: true, Alt: true, Shift: false, Win: false, VirtualKey: VK_T);
  ```

---

## §13 — Item #12: `_initialized` Field Declaration

- **Resolution:** Declare `private bool _initialized;` field; not set in stub (Initialize throws before assignment).
- **Rationale:** Codex Phase 5E adds `_initialized = true;` at end of implemented `Initialize()` — minimal diff. Field declaration is forward-compat anti-drift.
- **Files affected:** `TrayHost.cs`.
- **Gap for Handoff:** Static-analysis warning (IDE0052 "unused private member") may surface; acceptable per HIGH-stub doctrine. Codex Phase 5E closes the warning when wiring real Initialize.

---

## §14 — Item #13: Msg Structure (Msg 1 vs Msg 2 Split)

- **Resolution:** Msg 1 = production stubs + AssemblyInfo + derivations + HANDOFF delta + PowerShell tool update. Msg 2 = test files + csproj patch + Architecture v1.0.2 delta update.
- **Rationale:** Mirrors Phase 1D precedent exactly.
- **Files affected:** None (process).
- **Gap for Handoff:** None.

---

## §15 — Item #14: `find-spec-derivations.ps1` Update

- **Resolution:** Add `'SPEC-DERIVED-PHASE1E' = 3` to expected hashtable.
- **Rationale:** 3 .cs files in Msg 1 carry the marker (TrayHost.cs, HotkeyInterop.cs, AssemblyInfo.cs).
- **Files affected:** `tools/find-spec-derivations.ps1`.
- **Gap for Handoff (Codex/Claude Code):** Phase 1E Msg 2 will bump `PHASE1E` to 4 (adding TrayHostTests.cs's marker hit via substring of PHASE1E-MSG2) and add `PHASE1E-MSG2` bucket. Parallels Phase 1D pattern.

---

## §16 — Item #15: Architecture v1.0.2 Scope Expansion

- **Trigger:** Phase 1D Msg 1 already proposed v1.0.2 for `ReminderReason.cs` §3.3 addition + optional Cadence clamp formalization. Phase 1E Item #5 introduces the second consumer of the internal-raise pattern.
- **Options:** A) Bundle into existing pending v1.0.2 · B) Keep v1.0.2 narrow; defer to v1.0.3.
- **Resolution:** Option A.
- **Rationale:** Consolidates pending amendment into one owner approval. Three additive non-breaking changes:
  1. `§3.3` — add `ReminderReason.cs` to Enums folder (Phase 1D Msg 1).
  2. `§4.3` (optional) — formalize `Cadence` `[1s, 5min]` clamp (Phase 1D Msg 1).
  3. `§4.1` — bless internal-raise pattern: *"The TrayHost module MAY expose internal Raise*() methods accessible only via [InternalsVisibleTo(...)] to its Tests assembly for AC P1E-AC2 satisfaction. No production caller may use these methods."* (Phase 1E Msg 1).
- **Files affected:** `docs/Architecture.v1.0.2-delta.md` (updated by Phase 1E Msg 2).
- **Gap for Handoff (Codex/Claude Code):** **Cross-Phase Gap #60 — Architecture v1.0.2 delta document.** Phase 1E Msg 2 emits the updated v1.0.2 delta with the §4.1 prose addition. Owner approval still required before Phase 5F sign-off.

---

## §17 Files Produced This Msg

| Path | Purpose | Marker |
|---|---|---|
| `src/TaskTree.Modules.TrayHost/TrayHost.cs` | HIGH-stub `ITrayHost` impl | `PHASE1E` |
| `src/TaskTree.Modules.TrayHost/HotkeyInterop.cs` | PInvoke sigs + `BuildModifierFlags` impl | `PHASE1E` |
| `src/TaskTree.Modules.TrayHost/Properties/AssemblyInfo.cs` | `InternalsVisibleTo` grant | `PHASE1E` |

Tests for TrayHost + HotkeyInterop: **deferred to Phase 1E Msg 2.**
Architecture v1.0.2 delta document update: **deferred to Phase 1E Msg 2.**

---

## §18 SPEC-DERIVED-PHASE1E Marker Inventory

| File | Marker count |
|---|---|
| `TrayHost.cs` | 1 |
| `HotkeyInterop.cs` | 1 |
| `Properties/AssemblyInfo.cs` | 1 |
| **Total distinct .cs files** | **3** |

`tools/find-spec-derivations.ps1` updated to assert `SPEC-DERIVED-PHASE1E = 3`. Msg 2 will introduce `SPEC-DERIVED-PHASE1E-MSG2` bucket + bump `PHASE1E` count via substring match (paralleling Phase 1D Msg 2 inventory mechanics).

---

## §19 Cross-Phase Gaps Introduced (rows 56-60)

| # | Gap | Source | Target Phase | Action |
|---|---|---|---|---|
| 56 | Inject `IComplianceCore` into `TrayHost` ctor | HALT #2 | Phase 1F | **LOAD-BEARING — 3rd audit injection** (TaskEngine + ReminderScheduler + TrayHost) |
| 57 | Live TrayHost handlers MUST call `AuditAsync` on each event | HALT #3 | Phase 5E | Codex: `Module="TrayHost"`, `Action="<EventName>"`, `Result="success"` |
| 58 | `InternalsVisibleTo` scope = Tests-only | HALT #5 | Phase 5B | Compile gate verifies 1 attribute line |
| 59 | `Initialize` idempotency check needed | HALT #10 | Phase 5E | Codex adds `if (_initialized) throw InvalidOperationException` |
| 60 | Architecture v1.0.2 must bless internal-raise pattern | HALT #15 | Architecture v1.0.2 | Phase 1E Msg 2 emits updated delta document |

---

## §20 Phase 1F Composition Root Checklist Additions

Appended to ReminderScheduler §19 checklist (Phase 1D Msg 2):

10. **NEW:** Register `ITrayHost` → `TrayHost` as **singleton** with `(IAppLogger, IComplianceCore)` injected (Gap #56).
11. **NEW:** Orchestrator subscribes to `ShowTreeRequested`, `AddTaskRequested`, `ExitRequested` AFTER `TrayHost.Initialize()` succeeds.
12. **NEW:** Orchestrator MUST NOT call internal `Raise*()` methods (`InternalsVisibleTo` scope enforces) (Gap #58).

**LOAD-BEARING:** Phase 1F now has **three** audit-injection flags (TaskEngine + ReminderScheduler + TrayHost). All three must hold or HIPAA audit chain silently breaks for the affected module.

---

## §21 Phase 5E Verification Additions (TrayHost-specific)

1. **`Initialize()` implementation:**
   - `ThrowIfDisposed()` first.
   - `if (_initialized) throw new InvalidOperationException("TrayHost already initialized.")` (Gap #59).
   - Create `NotifyIcon` (H.NotifyIcon.Wpf 2.0+ per §12).
   - Register Ctrl+Alt+T via `HotkeyInterop.Register(messageOnlyHwnd, hotkeyId, HotkeyInterop.BuildModifierFlags(ctrl: true, alt: true, shift: false, win: false), VK_T)`.
   - Wire NotifyIcon click → `ShowTreeRequested?.Invoke(this, EventArgs.Empty)`.
   - Wire context-menu items → `AddTaskRequested` / `ExitRequested`.
   - Audit each event raise via `_compliance.AuditAsync` (Gap #57).
   - Set `_initialized = true` at end.

2. **`ShowBalloon(title, message)` implementation:**
   - Param validation already present — leave intact.
   - Call `_notifyIcon.ShowBalloonTip(title, message, ToolTipIcon.Info)`.
   - Audit via `_compliance.AuditAsync` (Module="TrayHost", Action="BalloonShown").

3. **`HotkeyInterop.Register`/`Unregister` implementation:**
   - Replace `NotImplementedException` with:
     ```csharp
     if (!RegisterHotKey(hWnd, id, modifiers, virtualKey))
         throw new Win32Exception(Marshal.GetLastWin32Error());
     ```

4. **`TrayHost.Dispose()` extension:**
   - Before existing `_logger.LogInformation` line, dispose NotifyIcon + call `HotkeyInterop.Unregister`.
   - Wrap in try/catch to preserve idempotent contract.

5. **Audit posture (Gap #57):** every Show/Add/Exit handler invokes `_compliance.AuditAsync`.

6. **`InternalsVisibleTo` scope verification (Gap #58):** confirm exactly 1 attribute line targeting Tests assembly.

---

## §22 Architecture v1.0.2 Scope Expansion

The v1.0.2 amendment now bundles three additive non-breaking changes:

1. **§3.3** — Add `ReminderReason.cs` to Enums folder (Phase 1D Msg 1).
2. **§4.3** (optional) — Formalize `Cadence` `[1s, 5min]` clamp (Phase 1D Msg 1).
3. **§4.1** (NEW per HALT #15) — Bless TrayHost internal-raise pattern:
   > *"The TrayHost module MAY expose internal Raise*() methods accessible only via [InternalsVisibleTo(...)] to its Tests assembly for AC P1E-AC2 satisfaction. No production caller may use these methods."*

Phase 1E Msg 2 will produce the consolidated v1.0.2 delta document. All three changes remain non-breaking and additive.

---

## §23 Known Limitations

1. PInvoke declarations are present but untested in chat env — Codex Phase 5E first execution surfaces any signature drift (none expected; declarations match Microsoft Docs `RegisterHotKey`/`UnregisterHotKey`).
2. `_compliance` field is reserved but unused in Phase 1E stub. Static-analysis warning (IDE0052) may surface; acceptable.
3. `_initialized` field is reserved but unused in Phase 1E stub. Same caveat.
4. `ShowBalloon` validation may reject inputs that Phase 5E live impl would accept (e.g., max-length truncation policy). Codex may need to adjust validation when implementing live balloon.
5. No test coverage in Msg 1 — deferred to Msg 2.
6. `ModNoRepeat` (0x4000) always set in `BuildModifierFlags`. Win10+ required; Architecture §11 confirms min-build Win10 1809. Safe for v1.0.
7. ID parameter for `Register`/`Unregister` is `int` per Win32 contract; no validation in stub. Phase 5E should bound the ID range to avoid collisions if multiple hotkeys ever needed (Phase 2A concern).
