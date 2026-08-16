// File: src/TaskTree.Modules.AutoUpdater/Properties/AssemblyInfo.cs
// Purpose: expose the internal MSIX path/process guard to offline tests only.
// Architecture.md References: §9.1.5; Roadmap Phase 5C validation.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TaskTree.Modules.AutoUpdater.Tests")]
