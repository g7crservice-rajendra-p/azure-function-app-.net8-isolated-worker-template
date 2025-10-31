using DurableTask.Core.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Entities;

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


        // Two-way call to the entity which returns a value - awaits the response
        //int currentValue = await context.Entities.CallEntityAsync<int>(entityId, "Get");
        var entityId = new EntityInstanceId(nameof(ConfigEntity), "Config");

        // Two-way call to the entity which returns a value - awaits the response
        ConfigState currentValue = await context.Entities.CallEntityAsync<ConfigState>(entityId, "RefreshAsync");

        var currentValue1 = await context.Entities.CallEntityAsync<IDictionary<string, string>>(entityId, "GetAll");

        string value = await context.Entities.CallEntityAsync<string>(entityId, "Get", "ACASAutomation");


        
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

    [Function("ReceiveMessageOrchestrator_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("ReceiveMessageOrchestrator_HttpStart");

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(ReceiveMessageOrchestrator));

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}