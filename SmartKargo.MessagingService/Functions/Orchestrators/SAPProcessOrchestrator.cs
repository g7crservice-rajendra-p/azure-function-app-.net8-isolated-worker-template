using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class SAPProcessOrchestrator
    {
        [Function(nameof(SAPProcessOrchestrator))]
        public static async Task<object?> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(SAPProcessOrchestrator));
            string instanceId = context.InstanceId;

            logger.LogInformation(
                "SAPProcessOrchestrator started. InstanceId: {InstanceId}",
                instanceId);

            var input = context.GetInput<object?>();

            try
            {
                logger.LogInformation(
                    "Calling SAPProcessActivity for InstanceId: {InstanceId}",
                    instanceId);

                // Simple activity call (no retry)
                var result = await context.CallActivityAsync<object?>(
                    nameof(SAPProcessActivity),
                    input);

                logger.LogInformation(
                    "SAPProcessActivity completed successfully. InstanceId: {InstanceId}",
                    instanceId);

                return result;
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(
                    ex,
                    "SAPProcessActivity failed. InstanceId: {InstanceId}",
                    instanceId);

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception in SAPProcessOrchestrator. InstanceId: {InstanceId}",
                    instanceId);

                throw;
            }
            finally
            {
                logger.LogInformation(
                    "SAPProcessOrchestrator finished (may replay). InstanceId: {InstanceId}",
                    instanceId);
            }
        }
    }
}