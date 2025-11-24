using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Entities;
using System.Net;
using System.Web;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class ConfigEntityHttpTriggerStarter
    {
        private readonly ILogger<ConfigEntityHttpTriggerStarter> _logger;

        public ConfigEntityHttpTriggerStarter(ILogger<ConfigEntityHttpTriggerStarter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// HTTP endpoint to force-refresh the ConfigEntity.
        /// Usage:
        ///   POST /api/config/refresh?entityKey=config
        /// Blocks execution in Production environment.
        /// </summary>
        [Function("RefreshConfigEntity")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "config/refresh")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext context,
            CancellationToken cancellationToken)
        {
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            string entityKey = query["entityKey"] ?? "config";
            try
            {
                // Two-way call to the entity which returns a value - awaits the response
                var entityId = new EntityInstanceId(nameof(ConfigEntity), "config");

                // Signal the entity to run RefreshAsync with ForceRefresh = true
                var payload = new { ForceRefresh = true };

                _logger.LogInformation("Signaling ConfigEntity (key={EntityKey}) to refresh (force=true).", entityKey);

                await client.Entities.SignalEntityAsync(entityId, "RefreshAsync", payload).ConfigureAwait(false);

                var accepted = req.CreateResponse(HttpStatusCode.Accepted);
                await accepted.WriteAsJsonAsync(new
                {
                    Message = "ConfigEntity refresh signaled.",
                    Entity = nameof(ConfigEntity),
                    EntityKey = entityKey
                }, cancellationToken);

                return accepted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to signal ConfigEntity refresh for key {EntityKey}.", entityKey);
                var err = req.CreateResponse(HttpStatusCode.InternalServerError);
                await err.WriteStringAsync($"Failed to signal refresh: {ex.Message}", cancellationToken);
                return err;
            }
        }
    }
}
