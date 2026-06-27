# PHASE2E-DERIVATIONS.md - Phase 2E SettingsService

> Scope: TaskTreeSettings + ISettingsService + SettingsService + DI registration + tests.
> Owner-approved HALT batch: 22 items.

## Section 1 Summary

All 22 HALT items approved as proposed. Settings are persisted through ISecureStore key `settings/app`, remain explicitly non-PHI, expose theme preference without runtime theme switching, and audit save/reset events only.

## Section 2 Files Produced

| Path | Purpose | Marker |
|---|---|---|
| src/TaskTree.Core/Enums/ThemePreference.cs | NEW enum | PHASE2E |
| src/TaskTree.Core/Models/TaskTreeSettings.cs | NEW settings model | PHASE2E |
| src/TaskTree.Core/Abstractions/ISettingsService.cs | NEW abstraction | PHASE2E |
| src/TaskTree.Modules.Settings/TaskTree.Modules.Settings.csproj | NEW project | csproj |
| src/TaskTree.Modules.Settings/SettingsService.cs | NEW implementation | PHASE2E |
| src/TaskTree.App/Bootstrap/ServiceRegistrations.cs | PATCH register ISettingsService | PHASE1F/PHASE1G/PHASE2A/PHASE2E |
| tests/TaskTree.Modules.Settings.Tests/TaskTree.Modules.Settings.Tests.csproj | NEW test project | csproj |
| tests/TaskTree.Modules.Settings.Tests/SettingsServiceTests.cs | NEW 12 tests | PHASE2E |
| docs/spec-derivations/PHASE2E-DERIVATIONS.md | This registry | md |
| docs/HANDOFF-v1.0.32-delta.md | Phase 2E delta | md |
| tools/find-spec-derivations.ps1 | PHASE2E=6 | script |

## Section 3 Marker Inventory

PHASE2E = 6 distinct .cs files. Grand total: 97 -> 103.

## Section 4 Cross-Phase Gaps Introduced (148-157)

| # | Gap | Target | Action |
|---|---|---|---|
| 148 | Add src/TaskTree.Modules.Settings/ to Architecture Section 3.3 | Architecture v1.0.3/Phase 5F | Amend folder tree |
| 149 | Add ISettingsService subsection to Architecture Section 4 if stable | Architecture v1.0.3 | Formalize interface |
| 150 | Add ThemePreference.cs to Architecture Section 3.3 Enums | Architecture v1.0.3 | Amend enum list |
| 151 | Verify settings backward compatibility as fields are added | Phase 5C | Old/new settings serialization tests |
| 152 | Include settings model in PHI-surface review | Phase 4A/5F | Compliance review |
| 153 | SettingsService audit injection consumer registration | Closed in Msg 1 | ServiceRegistrations patch emitted |
| 154 | Move SettingsChangedEventArgs if event usage expands | Later refactor | Currently EventArgs.Empty |
| 155 | Compliance policy documents SettingsSaved/SettingsReset | Phase 4A | Audit vocabulary |
| 156 | Verify corrupt settings store fallback | Phase 5C | Simulated corrupt SecureStore test |
| 157 | Settings UI surface deferred | Later Phase 2E/2F/2G | Add UI if Roadmap requires |

## Section 5 Carry-Forward Gaps

#141, #142, #138, #146, #147 remain active. Runtime theme switching and app-level resource merge are deferred because SettingsService only persists preference in Phase 2E.

## Section 6 Known Limitations

1. Runtime theme switching is not applied; ThemePreference is persisted only.
2. Settings UI is deferred.
3. SettingsChanged currently exposes EventArgs.Empty; richer event args deferred (Gap #154).
4. Corrupt settings fallback needs Phase 5C verification (Gap #156).
5. Architecture amendments required for new module/interface/enum (Gaps #148-#150).
