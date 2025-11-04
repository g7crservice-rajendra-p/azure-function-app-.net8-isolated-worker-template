using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class PrewarmConfigOrchestrator
    {
        [Function(nameof(PrewarmConfigOrchestrator))]
        public static async Task Run([OrchestrationTrigger] TaskOrchestrationContext ctx)
        {
            // Replay-safe call that will call ConfigEntity.GetAll() and RefreshAsync() if needed.
            await ConfigCache.EnsureInitializedFromOrchestratorAsync(ctx);
        }
    }
}
