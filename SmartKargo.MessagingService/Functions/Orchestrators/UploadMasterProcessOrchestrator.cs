using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;

namespace SmartKargo.MessagingService.Functions.Orchestrators
{
    public static class UploadMasterProcessOrchestrator
    {
        [Function(nameof(UploadMasterProcessOrchestrator))]
        public static async Task<object?> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(UploadMasterProcessOrchestrator));
            string instanceId = context.InstanceId;

            logger.LogInformation(
                "UploadMasterProcessOrchestrator started. InstanceId: {InstanceId}",
                instanceId);

            var input = context.GetInput<object?>();

            try
            {
                logger.LogInformation(
                    "Calling UploadMasterProcessActivity for InstanceId: {InstanceId}",
                    instanceId);

                // Activity execution (no retry)
                var result = await context.CallActivityAsync<object?>(
                    nameof(UploadMasterProcessActivity),
                    input);

                logger.LogInformation(
                    "UploadMasterProcessActivity completed successfully. InstanceId: {InstanceId}",
                    instanceId);

                return result;
            }
            catch (TaskFailedException ex)
            {
                logger.LogError(
                    ex,
                    "UploadMasterProcessActivity failed. InstanceId: {InstanceId}",
                    instanceId);

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception in UploadMasterProcessOrchestrator. InstanceId: {InstanceId}",
                    instanceId);

                throw;
            }
            finally
            {
                logger.LogInformation(
                    "UploadMasterProcessOrchestrator finished (may replay). InstanceId: {InstanceId}",
                    instanceId);
            }
        }
    }
}