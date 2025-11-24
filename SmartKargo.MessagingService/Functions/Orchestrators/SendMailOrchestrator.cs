using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class SendMailOrchestrator
    {
        [Function(nameof(SendMailOrchestrator))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(SendMailOrchestrator));
            string instanceId = context.InstanceId;

            logger.LogInformation("SendMailOrchestrator started. InstanceId: {InstanceId}", instanceId);

            var input = context.GetInput<object?>();

            try
            {
                logger.LogInformation("Calling SendMailActivity...");

                await context.CallActivityAsync(nameof(SendMailActivity), input);

                logger.LogInformation("SendMailActivity completed for InstanceId: {InstanceId}", instanceId);

            }
            catch (TaskFailedException ex)
            {
                logger.LogError(ex, "Activity SendMailActivity failed in InstanceId: {InstanceId}", instanceId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in orchestrator InstanceId: {InstanceId}", instanceId);
                throw;
            }
            finally
            {
                logger.LogInformation("SendMailOrchestrator finished (may replay). InstanceId: {InstanceId}", instanceId);
            }
        }
    }
}
