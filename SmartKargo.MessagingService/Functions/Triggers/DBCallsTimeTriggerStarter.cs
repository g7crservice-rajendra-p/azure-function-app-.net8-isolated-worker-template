using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;
using System.Net;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class DBCallsTimeTriggerStarter
    {
        private readonly ILogger<DBCallsTimeTriggerStarter> _logger;

        public DBCallsTimeTriggerStarter(ILogger<DBCallsTimeTriggerStarter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Cron: "0 0 */1 * * *" -> at second=0 minute=0 every 1 hours (e.g., 00:00, 01:00, 02:00...)
        [Function(nameof(DBCallsTimeTriggerStarter))]
        public async Task Run(
            [TimerTrigger("0 0 */1 * * *", RunOnStartup = false)] TimerInfo timer,
            [DurableClient] DurableTaskClient client,
            CancellationToken cancellationToken)
        {
            // make instance id traceable: functionName + utc ticks
            string instanceId = $"DBCallsOrchestrator-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}".Substring(0, 64);

            try
            {
                _logger.LogInformation("DBCallsTimeTriggerStarter fired at {UtcNow}. Preparing to start orchestration '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);

                if (timer.IsPastDue)
                {
                    _logger.LogWarning("Timer trigger '{TriggerName}' is past due — possible scale/restart/throttling.",
                        nameof(DBCallsTimeTriggerStarter));
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
                    ScheduledBy = nameof(DBCallsTimeTriggerStarter)
                };

                _logger.LogInformation("Starting orchestration '{OrchName}' with InstanceId '{InstanceId}'.", nameof(DBCallsOrchestrator), instanceId);

                // Build StartOrchestrationOptions (InstanceId and optional StartAt)
                var startOptions = new StartOrchestrationOptions(InstanceId: instanceId);

                // Call the overload that accepts (orchestratorName, input, options, cancellationToken)
                await client.ScheduleNewOrchestrationInstanceAsync(
                    nameof(DBCallsOrchestrator),
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
                _logger.LogError(ex, "Failed to start DBCallsOrchestrator at {UtcNow}. InstanceId: '{InstanceId}'.",
                    DateTime.UtcNow, instanceId);
                throw;
            }
        }
    }
}
