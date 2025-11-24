using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class FTPListenerOrchestrator
    {
        [Function(nameof(FTPListenerOrchestrator))]
        public static async Task<object?> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(FTPListenerOrchestrator));
            string instanceId = context.InstanceId;

            logger.LogInformation(
                "FTPListenerOrchestrator started. InstanceId: {InstanceId}",
                instanceId);

            var input = context.GetInput<object?>();

            try
            {
                logger.LogInformation(
                    "Calling FTPListenerActivity for InstanceId: {InstanceId}",
                    instanceId);

                // Simple activity execution without retries
                var result = await context.CallActivityAsync<object?>(
                    nameof(FTPListenerActivity),
                    input);

                logger.LogInformation(
                    "FTPListenerActivity completed successfully. InstanceId: {InstanceId}",
                    instanceId);

                return result;
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(
                    ex,
                    "FTPListenerActivity failed. InstanceId: {InstanceId}",
                    instanceId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception in FTPListenerOrchestrator. InstanceId: {InstanceId}",
                    instanceId);
                throw;
            }
            finally
            {
                logger.LogInformation(
                    "FTPListenerOrchestrator finished (may replay). InstanceId: {InstanceId}",
                    instanceId);
            }
        }
    }
}
