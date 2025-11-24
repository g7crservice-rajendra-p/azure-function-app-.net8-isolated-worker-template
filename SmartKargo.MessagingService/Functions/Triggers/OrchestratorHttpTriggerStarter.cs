using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Web;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    public class OrchestratorHttpTriggerStarter
    {
        private readonly ILogger<OrchestratorHttpTriggerStarter> _logger;

        public OrchestratorHttpTriggerStarter(
            ILogger<OrchestratorHttpTriggerStarter> logger
        )
        {
            _logger = logger;
        }

        [Function("StartOrchestratorViaHttp")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = "start-orchestrator")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext context,
            CancellationToken cancellationToken)
        {
            string env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")
                         ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                         ?? "Production"; // default safe

            _logger.LogInformation("StartOrchestratorViaHttp called in environment: {Env}", env);

            //BLOCK ORCHESTRATION START IN PRODUCTION
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync(
                    "Starting orchestrators via HTTP is disabled in Production environment.",
                    cancellationToken);
                return forbidden;
            }

            //Allowed for Dev / Local / Staging
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            string? orchestratorName = query["name"];

            if (string.IsNullOrWhiteSpace(orchestratorName))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Missing ?name=OrchestratorName", cancellationToken);
                return badResponse;
            }

            _logger.LogInformation("HTTP request received to start orchestrator '{Orchestrator}'.", orchestratorName);

            // Read optional JSON body as input
            object? input = null;
            try
            {
                if (req.Body != null)
                {
                    input = await req.ReadFromJsonAsync<object>(cancellationToken: cancellationToken);
                }
            }
            catch
            {
                _logger.LogWarning("Input body was not valid JSON. Running orchestrator without input.");
            }

            string instanceId = Guid.NewGuid().ToString("N");

            try
            {
                _logger.LogInformation("Starting orchestrator '{Name}' with InstanceId '{InstanceId}'.",
                    orchestratorName, instanceId);

                // Build StartOrchestrationOptions (InstanceId and optional StartAt)
                var startOptions = new StartOrchestrationOptions(InstanceId: instanceId);

                await client.ScheduleNewOrchestrationInstanceAsync(orchestratorName, input, startOptions, cancellationToken);

                var response = req.CreateResponse(HttpStatusCode.Accepted);
                await response.WriteAsJsonAsync(new
                {
                    Message = "Orchestrator started.",
                    Orchestrator = orchestratorName,
                    InstanceId = instanceId
                }, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start orchestrator '{Orchestrator}'.", orchestratorName);

                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync(
                    $"Failed to start orchestrator '{orchestratorName}'. Error: {ex.Message}",
                    cancellationToken);

                return error;
            }
        }
    }
}