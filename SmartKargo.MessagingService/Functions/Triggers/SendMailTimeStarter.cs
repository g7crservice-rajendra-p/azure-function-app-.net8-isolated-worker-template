using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class SendMailTimeStarter
{
    private readonly ILogger _logger;

    public SendMailTimeStarter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SendMailTimeStarter>();
    }

    [Function("SendMailTimeStarter")]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("SendMailTimeStarter fired at {Now}. Starting SendEmailOrchestrator...", DateTime.UtcNow);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}