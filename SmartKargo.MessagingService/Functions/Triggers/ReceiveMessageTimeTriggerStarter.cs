using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class ReceiveMessageTimeTriggerStarterStarter
{
    private readonly ILogger _logger;
    private readonly StartupReadiness _readiness;

    public ReceiveMessageTimeTriggerStarterStarter(ILoggerFactory loggerFactory, StartupReadiness readiness)
    {
        _logger = loggerFactory.CreateLogger<ReceiveMessageTimeTriggerStarterStarter>();
        _readiness = readiness;
    }

    [Function(nameof(ReceiveMessageTimeTriggerStarterStarter))]
    public async Task Run([TimerTrigger("*/60 * * * * *", RunOnStartup = false)] TimerInfo receiveMessageTimer, [DurableClient] DurableTaskClient client)
    {
        var instanceId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation("ReceiveMessageTimeTriggerStarter fired at {Now}. Started ReceiveMessageOrchestrator with instance ID '{InstanceId}'.",
                DateTime.UtcNow, instanceId);

            if (receiveMessageTimer.IsPastDue)
            {
                _logger.LogWarning("'{TriggerName}' is past due — possible scale, restart, or throttling delay.", 
                    "ReceiveMessageTimeTriggerStarterStarter");
            }

            // Wait for the service to become ready (ConfigCache).
            if (!_readiness.IsReady)
            {
                await _readiness.WaitForReadyAsync(TimeSpan.FromSeconds(30));
            }

            await client.ScheduleNewOrchestrationInstanceAsync(nameof(ReceiveMessageOrchestrator), new StartOrchestrationOptions(InstanceId: instanceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ReceiveMessageOrchestrator at {Now}. InstanceId (if generated): '{InstanceId}'.",
                DateTime.UtcNow, instanceId ?? "<none>");
            throw;
        }
    }
}