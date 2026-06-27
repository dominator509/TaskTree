// ============================================================================
// File: src/TaskTree.Modules.TrayHost/Properties/AssemblyInfo.cs
// Purpose: Grant the unit-test assembly access to internal Raise*() methods
//          on TrayHost (HALT #5 — P1E-AC2 satisfaction).
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1E
//   HALT #5  Internal Raise*() methods accessible only via InternalsVisibleTo
//   HALT #15 Architecture v1.0.2 will bless this pattern in §4.1 (Gap #60)
// Precedent: Phase 1D Msg 2 established the same InternalsVisibleTo pattern
// for ReminderScheduler.Tests; this is its second consumer.
// SCOPE: ONLY the test assembly named below. Cross-Phase Gap #58.
// ============================================================================

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TaskTree.Modules.TrayHost.Tests")]
