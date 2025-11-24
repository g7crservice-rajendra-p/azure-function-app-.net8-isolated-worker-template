using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class DBCallsOrchestrator
    {
        [Function(nameof(DBCallsOrchestrator))]
        public static async Task<object?> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(DBCallsOrchestrator));
            string instanceId = context.InstanceId;

            logger.LogInformation(
                "DBCallsOrchestrator started. InstanceId: {InstanceId}",
                instanceId);

            // Read orchestrator input (if provided)
            var input = context.GetInput<object?>();

            try
            {
                logger.LogInformation(
                    "Calling DBCallsActivity for InstanceId: {InstanceId}",
                    instanceId);

                // Single activity call (no retries)
                var result = await context.CallActivityAsync<object?>(
                    nameof(DBCallsActivity),
                    input);

                logger.LogInformation(
                    "DBCallsActivity completed successfully. InstanceId: {InstanceId}",
                    instanceId);

                return result;
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(
                    ex,
                    "DBCallsActivity failed. InstanceId: {InstanceId}",
                    instanceId);

                throw; // Fail orchestrator normally
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception in orchestrator. InstanceId: {InstanceId}",
                    instanceId);

                throw; // Fail orchestrator normally
            }
            finally
            {
                logger.LogInformation(
                    "DBCallsOrchestrator finished (may replay). InstanceId: {InstanceId}",
                    instanceId);
            }
        }
    }
}
