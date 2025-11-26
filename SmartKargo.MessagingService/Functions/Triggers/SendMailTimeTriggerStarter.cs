using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class SendMailTimeTriggerStarter
    {
        private readonly ILogger<SendMailTimeTriggerStarter> _logger;

        public SendMailTimeTriggerStarter(ILogger<SendMailTimeTriggerStarter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Cron: "0 0 */6 * * *" -> at second=0 minute=0 every 6 hours (e.g., 00:00, 06:00, 12:00...)
        [Function(nameof(SendMailTimeTriggerStarter))]
        public async Task Run(
            [TimerTrigger("0 0 */6 * * *", RunOnStartup = false)] TimerInfo timer,
            [DurableClient] DurableTaskClient client,
            CancellationToken cancellationToken)
        {
            // make instance id traceable: functionName + utc ticks
            string instanceId = $"SendMailOrchestrator-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}".Substring(0, 64);

            try
            {
                _logger.LogInformation("SendMailTimeTriggerStarter fired at {UtcNow}. Preparing to start orchestration '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);

                if (timer.IsPastDue)
                {
                    _logger.LogWarning("Timer trigger '{TriggerName}' is past due — possible scale/restart/throttling.",
                        nameof(SendMailTimeTriggerStarter));
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
                    ScheduledBy = nameof(SendMailTimeTriggerStarter)
                };

                _logger.LogInformation("Starting orchestration '{OrchName}' with InstanceId '{InstanceId}'.", nameof(SendMailOrchestrator), instanceId);

                // Build StartOrchestrationOptions (InstanceId and optional StartAt)
                var startOptions = new StartOrchestrationOptions(InstanceId: instanceId);

                // Call the overload that accepts (orchestratorName, input, options, cancellationToken)
                await client.ScheduleNewOrchestrationInstanceAsync(
                    nameof(SendMailOrchestrator),
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
                _logger.LogError(ex, "Failed to start SendMailOrchestrator at {UtcNow}. InstanceId: '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);
                throw;
            }
        }
    }
}
