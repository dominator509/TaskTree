// ============================================================================
// File: src/TaskTree.Core/Abstractions/IOrchestrator.cs
// Architecture §3.2 dependency graph; §3.3 Abstractions folder
// ----------------------------------------------------------------------------
// SPEC-DERIVED-PHASE1F
//   HALT #1  Finalize 2-method surface (StartAsync + StopAsync) — Phase 0 stub was empty
//   HALT #16 Architecture v1.0.2 amendment formalizes §4 IOrchestrator prose (Gap #63)
// This patch is a Phase 0 derivation completion (interface stub was empty).
// Public surface MUST match concrete Orchestrator class for DI resolution to work.
// ============================================================================

using System.Threading;
using System.Threading.Tasks;

namespace TaskTree.Core.Abstractions
{
    /// <summary>Coordinates lifecycle and event wiring across all TaskTree modules per Architecture §3.2.</summary>
    /// <remarks>Phase 1F finalizes the 2-method surface. Architecture v1.0.2 amendment (Gap #63) formalizes §4 prose.</remarks>
    public interface IOrchestrator
    {
        /// <summary>Starts: verifies chain integrity (§10.7), subscribes events, initializes TrayHost, starts ReminderScheduler, audits Startup.</summary>
        Task StartAsync(CancellationToken ct);

        /// <summary>Stops in reverse: unsubscribes handlers, stops ReminderScheduler (5s bound per Gap #38), disposes TrayHost, audits Shutdown.</summary>
        Task StopAsync();
    }
}
