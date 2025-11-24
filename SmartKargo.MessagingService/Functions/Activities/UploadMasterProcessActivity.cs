using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole.UploadMasters;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity invoked by UploadMasterProcessOrchestrator.
    /// </summary>
    public class UploadMasterProcessActivity
    {
        private readonly ILogger<UploadMasterProcessActivity> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        public UploadMasterProcessActivity(
            ILogger<UploadMasterProcessActivity> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uploadMasterCommon = uploadMasterCommon ?? throw new ArgumentNullException(nameof(uploadMasterCommon));
        }

        /// <summary>
        /// Durable Activity entry point.
        /// </summary>
        [Function(nameof(UploadMasterProcessActivity))]
        public async Task RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(UploadMasterProcessActivity), Input = input });

            _logger.LogInformation("UploadMasterProcessActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _uploadMasterCommon.UploadMasters().ConfigureAwait(false);

                _logger.LogInformation("UploadMasterProcessActivity completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("UploadMasterProcessActivity was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadMasterProcessActivity failed.");
                throw;
            }
        }
    }
}
