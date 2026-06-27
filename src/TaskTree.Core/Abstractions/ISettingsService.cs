// SPEC-DERIVED-PHASE2E  HALT #3/#10
// Gap #149: Architecture Section 4 should add ISettingsService subsection if stable.

using System;
using System.Threading.Tasks;
using TaskTree.Core.Models;

namespace TaskTree.Core.Abstractions
{
    public interface ISettingsService
    {
        event EventHandler? SettingsChanged;
        Task<TaskTreeSettings> GetAsync();
        Task SaveAsync(TaskTreeSettings settings);
        Task ResetAsync();
    }
}
