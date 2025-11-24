using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity executed by DBCallsOrchestrator.
    /// </summary>
    public class DBCallsActivity
    {
        private readonly ILogger<DBCallsActivity> _logger;
        private readonly Cls_BL _cls_BL;

        public DBCallsActivity(
            ILogger<DBCallsActivity> logger,
            Cls_BL cls_BL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cls_BL = cls_BL ?? throw new ArgumentNullException(nameof(cls_BL));
        }

        /// <summary>
        /// Durable Activity Function entry point.
        /// </summary>
        [Function(nameof(DBCallsActivity))]
        public async Task<object?> RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(DBCallsActivity), Input = input });

            _logger.LogInformation("DBCallsActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await _cls_BL.DBCalls().ConfigureAwait(false);

                _logger.LogInformation("DBCallsActivity completed successfully.");

                return result; // return to orchestrator (can be null or DTO)
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DBCallsActivity cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DBCallsActivity failed.");
                throw;
            }
        }
    }
}