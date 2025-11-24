using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class UploadMasterProcessTimeTriggerStarter
    {
        private readonly ILogger<UploadMasterProcessTimeTriggerStarter> _logger;
        private readonly StartupReadiness _readiness;

        public UploadMasterProcessTimeTriggerStarter(ILogger<UploadMasterProcessTimeTriggerStarter> logger, StartupReadiness readiness)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _readiness = readiness ?? throw new ArgumentNullException(nameof(readiness));
        }

        // Cron: "0 0 */7 * * *" -> at second=0 minute=0 every 7 hours (e.g., 00:00, 07:00, 14:00...)
        [Function(nameof(UploadMasterProcessTimeTriggerStarter))]
        public async Task Run(
            [TimerTrigger("0 0 */7 * * *", RunOnStartup = false)] TimerInfo timer,
            [DurableClient] DurableTaskClient client,
            CancellationToken cancellationToken)
        {
            // make instance id traceable: functionName + utc ticks
            string instanceId = $"UploadMasterProcessOrchestrator-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}".Substring(0, 64);

            try
            {
                _logger.LogInformation("UploadMasterProcessTimeTriggerStarter fired at {UtcNow}. Preparing to start orchestration '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);

                if (timer.IsPastDue)
                {
                    _logger.LogWarning("Timer trigger '{TriggerName}' is past due — possible scale/restart/throttling.",
                        nameof(UploadMasterProcessTimeTriggerStarter));
                }

                // Wait for startup readiness with cancellation and a concrete timeout policy
                TimeSpan waitTimeout = TimeSpan.FromSeconds(30);
                if (!_readiness.IsReady)
                {
                    _logger.LogInformation("Service not ready yet. Waiting up to {TimeoutSeconds}s for readiness.", waitTimeout.TotalSeconds);
                    try
                    {
                        await _readiness.WaitForReadyAsync(waitTimeout, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Timeout or host is shutting down
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogWarning("Function host is shutting down while waiting for readiness. Aborting orchestration start.");
                            return;
                        }

                        _logger.LogWarning("Timeout waiting for readiness after {TimeoutSeconds}s. Skipping orchestration start.", waitTimeout.TotalSeconds);

                        // Decide: return (skip start) or continue anyway. Here we skip to avoid starting when not ready.
                        return;
                    }
                }

                // Prepare input for orchestration (if any)
                var orchestrationInput = new
                {
                    TriggeredAtUtc = DateTime.UtcNow,
                    ScheduledBy = nameof(UploadMasterProcessTimeTriggerStarter)
                };

                _logger.LogInformation("Starting orchestration '{OrchName}' with InstanceId '{InstanceId}'.", nameof(UploadMasterProcessOrchestrator), instanceId);

                // Build StartOrchestrationOptions (InstanceId and optional StartAt)
                var startOptions = new StartOrchestrationOptions(InstanceId: instanceId);

                // Call the overload that accepts (orchestratorName, input, options, cancellationToken)
                await client.ScheduleNewOrchestrationInstanceAsync(
                    nameof(UploadMasterProcessOrchestrator),
                    orchestrationInput,
                    startOptions,
                    cancellationToken
                );

                _logger.LogInformation("Orchestration '{InstanceId}' scheduled successfully.", instanceId);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Operation cancelled while attempting to schedule orchestration '{InstanceId}'.", instanceId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start UploadMasterProcessOrchestrator at {UtcNow}. InstanceId: '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);
                throw;
            }
        }
    }
}
