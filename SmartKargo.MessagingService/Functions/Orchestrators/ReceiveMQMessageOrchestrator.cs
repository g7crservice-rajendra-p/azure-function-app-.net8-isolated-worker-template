using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class ReceiveMQMessageOrchestrator
    {
        [Function(nameof(ReceiveMQMessageOrchestrator))]
        public static async Task<object?> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(ReceiveMQMessageOrchestrator));
            string instanceId = context.InstanceId;

            logger.LogInformation(
                "ReceiveMQMessageOrchestrator started. InstanceId: {InstanceId}",
                instanceId);

            var input = context.GetInput<object?>();

            try
            {
                logger.LogInformation(
                    "Calling ReceiveMQMessageActivity for InstanceId: {InstanceId}",
                    instanceId);

                // Simple no-retry activity invocation
                var result = await context.CallActivityAsync<object?>(
                    nameof(ReceiveMQMessageActivity),
                    input);

                logger.LogInformation(
                    "ReceiveMQMessageActivity completed successfully. InstanceId: {InstanceId}",
                    instanceId);

                return result;
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(
                    ex,
                    "ReceiveMQMessageActivity failed. InstanceId: {InstanceId}",
                    instanceId);

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception in ReceiveMQMessageOrchestrator. InstanceId: {InstanceId}",
                    instanceId);

                throw;
            }
            finally
            {
                logger.LogInformation(
                    "ReceiveMQMessageOrchestrator finished (may replay). InstanceId: {InstanceId}",
                    instanceId);
            }
        }
    }
}
