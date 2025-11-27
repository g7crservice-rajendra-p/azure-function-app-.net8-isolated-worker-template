using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class FTPListenerTimeTriggerStarter
    {
        private readonly ILogger<FTPListenerTimeTriggerStarter> _logger;

        public FTPListenerTimeTriggerStarter(ILogger<FTPListenerTimeTriggerStarter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Cron: "0 0 */2 * * *" -> at second=0 minute=0 every 2 hours (e.g., 00:00, 02:00, 04:00...)
        [Function(nameof(FTPListenerTimeTriggerStarter))]
        public async Task Run(
            [TimerTrigger("0 0 */2 * * *", RunOnStartup = false)] TimerInfo timer,
            [DurableClient] DurableTaskClient client,
            CancellationToken cancellationToken)
        {
            // make instance id traceable: functionName + utc ticks
            string instanceId = $"FTPListenerOrchestrator-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}".Substring(0, 64);

            try
            {
                _logger.LogInformation("FTPListenerTimeTriggerStarter fired at {UtcNow}. Preparing to start orchestration '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);

                if (timer.IsPastDue)
                {
                    _logger.LogWarning("Timer trigger '{TriggerName}' is past due — possible scale/restart/throttling.",
                        nameof(FTPListenerTimeTriggerStarter));
                }

                //- During cold start / scale-out, this process may not have any config loaded.
                //- Activities and orchestrators depend on these settings for DB paths, URLs, etc.
                bool warmupSuccess = await ConfigEntityWarmup.WarmupFromEntityAsync(client, _logger, cancellationToken);
                if (!warmupSuccess)
                {
                    _logger.LogWarning("Skipping orchestration because config warmup failed.");
                    return;
                }

                // Prepare input for orchestration (if any)
                var orchestrationInput = new
                {
                    TriggeredAtUtc = DateTime.UtcNow,
                    ScheduledBy = nameof(FTPListenerTimeTriggerStarter)
                };

                _logger.LogInformation("Starting orchestration '{OrchName}' with InstanceId '{InstanceId}'.", nameof(FTPListenerOrchestrator), instanceId);

                // Build StartOrchestrationOptions (InstanceId and optional StartAt)
                var startOptions = new StartOrchestrationOptions(InstanceId: instanceId);

                // Call the overload that accepts (orchestratorName, input, options, cancellationToken)
                await client.ScheduleNewOrchestrationInstanceAsync(
                    nameof(FTPListenerOrchestrator),
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
                _logger.LogError(ex, "Failed to start FTPListenerOrchestrator at {UtcNow}. InstanceId: '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);
                throw;
            }
        }
    }
}
