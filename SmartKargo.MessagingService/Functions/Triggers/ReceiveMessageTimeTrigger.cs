using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class ReceiveMessageTimeTrigger
{
    private readonly ILogger _logger;
    private readonly StartupReadiness _readiness;

    public ReceiveMessageTimeTrigger(ILoggerFactory loggerFactory, StartupReadiness readiness)
    {
        _logger = loggerFactory.CreateLogger<ReceiveMessageTimeTrigger>();
        _readiness = readiness;
    }

    [Function(nameof(ReceiveMessageTimeTrigger))]
    public async Task Run([TimerTrigger("*/30 * * * * *", RunOnStartup = true)] TimerInfo receiveMessageTimer, [DurableClient] DurableTaskClient client)
    {
        var instanceId = Guid.NewGuid().ToString();

        try
        {
            if (!_readiness.IsReady)
            {
                await _readiness.WaitForReadyAsync(TimeSpan.FromSeconds(30));
            }

            var config = ConfigCache.Snapshot();
            _logger.LogInformation($"ReceiveMessageTimeTrigger Timer trigger function activated for instanceId: {instanceId}");

            await client.ScheduleNewOrchestrationInstanceAsync(nameof(ReceiveMessageOrchestrator), new StartOrchestrationOptions(InstanceId: instanceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"ReceiveMessageTimeTrigger: queue trigger function exception for instanceId: {instanceId}. Details: {ex.Message}");
        }
    }
}