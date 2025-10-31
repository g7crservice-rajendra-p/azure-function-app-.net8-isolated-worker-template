using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Orchestrators;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class ReceiveMessageTimeTrigger
{
    private readonly ILogger _logger;

    public ReceiveMessageTimeTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ReceiveMessageTimeTrigger>();
    }

    [Function(nameof(ReceiveMessageTimeTrigger))]
    public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = false)] TimerInfo receiveMessageTimer, [DurableClient] DurableTaskClient client)
    {
        var instanceId = Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation($"ReceiveMessageTimeTrigger Timer trigger function activated for instanceId: {instanceId}");

            await client.ScheduleNewOrchestrationInstanceAsync(nameof(ReceiveMessageOrchestrator), new StartOrchestrationOptions(InstanceId: instanceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"ReceiveMessageTimeTrigger: queue trigger function exception for instanceId: {instanceId}. Details: {ex.Message}");
        }
    }
}