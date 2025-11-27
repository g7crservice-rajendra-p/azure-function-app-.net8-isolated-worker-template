using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Services;
using System.Net;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class HealthCheckHttpTriggerStarter
    {
        private readonly ILogger<HealthCheckHttpTriggerStarter> _logger;

        public HealthCheckHttpTriggerStarter(ILogger<HealthCheckHttpTriggerStarter> logger)
        {
            _logger = logger;
        }

        [Function("Health")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            CancellationToken cancellationToken)
        {
            string env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")
                           ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           ?? "Production";

            bool warmupSuccess = await ConfigEntityWarmup.WarmupFromEntityAsync(client, _logger, cancellationToken);

            if (!warmupSuccess)
            {
                _logger.LogWarning("Skipping orchestration because config warmup failed.");
                return req.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }
            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                status = "Healthy",
                environment = env,
                timeUtc = DateTime.UtcNow,
                config = ConfigCache.Snapshot()
            }, cancellationToken);

            return response;
        }
    }
}
