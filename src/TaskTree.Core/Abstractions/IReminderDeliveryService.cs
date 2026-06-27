// SPEC-DERIVED-PHASE1G-MSG2  HALT-Msg2 #1 (Gap #86/#94)
using System.Threading;
using System.Threading.Tasks;
namespace TaskTree.Core.Abstractions
{
    public interface IReminderDeliveryService
    {
        Task StartAsync(CancellationToken ct);
        Task StopAsync();
    }
}
