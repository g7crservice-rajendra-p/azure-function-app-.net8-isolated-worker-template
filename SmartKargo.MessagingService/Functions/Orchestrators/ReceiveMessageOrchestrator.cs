using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Orchestrators;

public static class ReceiveMessageOrchestrator
{
    [Function(nameof(ReceiveMessageOrchestrator))]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(ReceiveMessageOrchestrator));
        logger.LogInformation("Saying hello.");
        var outputs = new List<string>();


        // Initialize the process-level configuration cache on this host instance (if not already loaded).
        var config = await ConfigCache.EnsureInitializedFromOrchestratorAsync(context);


        // Two-way call to the entity which returns a value - awaits the response
        //int currentValue = await context.Entities.CallEntityAsync<int>(entityId, "Get");
        //var entityId = new EntityInstanceId(nameof(ConfigEntity), "Config");

        //// Two-way call to the entity which returns a value - awaits the response
        //ConfigState currentValue = await context.Entities.CallEntityAsync<ConfigState>(entityId, "RefreshAsync");

        //var currentValue1 = await context.Entities.CallEntityAsync<IDictionary<string, string>>(entityId, "GetAll");

        //string value = await context.Entities.CallEntityAsync<string>(entityId, "Get", "ACASAutomation");



        // Replace name and input with values relevant for your Durable Functions Activity
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        Console.WriteLine(outputs);
        //return outputs;
    }

    [Function(nameof(SayHello))]
    public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("SayHello");
        logger.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }
}