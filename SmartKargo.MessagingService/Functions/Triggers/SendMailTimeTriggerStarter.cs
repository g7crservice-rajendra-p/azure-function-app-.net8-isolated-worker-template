using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class SendMailTimeTriggerStarter
{
    private readonly ILogger _logger;
    private readonly StartupReadiness _readiness;

    public SendMailTimeTriggerStarter(ILoggerFactory loggerFactory, StartupReadiness readiness)
    {
        _logger = loggerFactory.CreateLogger<SendMailTimeTriggerStarter>();
        _readiness = readiness;
    }

    [Function(nameof(SendMailTimeTriggerStarter))]
    public async Task Run([TimerTrigger("* */60 * * * *", RunOnStartup = false)] TimerInfo sendMailTimer, [DurableClient] DurableTaskClient client)
    {
        var instanceId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation("SendMailTimeTriggerStarter fired at {Now}. Started SendMailOrchestrator with instance ID '{InstanceId}'.",
            DateTime.UtcNow, instanceId);

            if (sendMailTimer.IsPastDue)
            {
                _logger.LogWarning("'{TriggerName}' is past due — possible scale, restart, or throttling delay.",
                    "SendMailTimeTriggerStarter");
            }

            // Wait for the service to become ready (ConfigCache).
            if (!_readiness.IsReady)
            {
                await _readiness.WaitForReadyAsync(TimeSpan.FromSeconds(30));
            }

            await client.ScheduleNewOrchestrationInstanceAsync(nameof(SendMailOrchestrator), new StartOrchestrationOptions(InstanceId: instanceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ReceiveMessageOrchestrator at {Now}. InstanceId (if generated): '{InstanceId}'.",
                DateTime.UtcNow, instanceId ?? "<none>");
            throw;
        }
    }
}