using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Functions.Entities;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class HealthCheckHttpTrigger
{
    private readonly ILogger<HealthCheckHttpTrigger> _logger;
    private readonly AppConfig _appConfig;
    //private readonly ConfigReaderService _configReaderService;

    public HealthCheckHttpTrigger(ILogger<HealthCheckHttpTrigger> logger, AppConfig appConfig)
    {
        _logger = logger;
        _appConfig = appConfig;
    }

    [Function("HealthCheckHttpTrigger")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req, [DurableClient] DurableTaskClient client, FunctionContext context)
    {


        //return new OkObjectResult(_appConfig);

        try
        {
            _logger.LogTrace("Trace");
            _logger.LogDebug("Debug");
            _logger.LogInformation("Information");
            _logger.LogWarning("Warning");
            // var configReader = client.InstanceServices.GetRequiredService<ConfigReaderService>();
            var config = ConfigCache.Snapshot();

            var abc=ConfigCache.Get("ScreeningRequired");

            throw new InvalidOperationException("Invalid operation");

        }
        catch (Exception ex)
        {
            _logger.LogError(message: ex.ToString());
            throw;
        }

        //// Two-way call to the entity which returns a value - awaits the response
        var entityId = new EntityInstanceId(nameof(ConfigEntity), "config");

        _logger.LogDebug("C# HTTP trigger function processed a entityId.");



        // Call RefreshAsync
        EntityMetadata<ConfigState> entity = await client.Entities.GetEntityAsync<ConfigState>(entityId);
        if (entity != null && !entity.IncludesState)
        {
            //Optionally signal the entity to initialize
            await client.Entities.SignalEntityAsync(entityId, "RefreshAsync");
        }

        var currentValue = await client.Entities.GetEntityAsync<ConfigState>(entityId);
        var configState = currentValue.State.Config;

        return new OkObjectResult(_appConfig);
    }
}