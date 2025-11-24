using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity executed by FTPListenerOrchestrator.
    /// </summary>
    public class FTPListenerActivity
    {
        private readonly ILogger<FTPListenerActivity> _logger;
        private readonly Cls_BL _cls_BL;

        public FTPListenerActivity(
            ILogger<FTPListenerActivity> logger,
            Cls_BL cls_BL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cls_BL = cls_BL ?? throw new ArgumentNullException(nameof(cls_BL));
        }

        /// <summary>
        /// Durable Activity entry point.
        /// </summary>
        [Function(nameof(FTPListenerActivity))]
        public async Task RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(FTPListenerActivity), Input = input });

            _logger.LogInformation("FTPListenerActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // No result expected, simply execute the operation
                await _cls_BL.FTPListener().ConfigureAwait(false);

                _logger.LogInformation("FTPListenerActivity completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("FTPListenerActivity was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FTPListenerActivity failed.");
                throw;
            }
        }
    }
}
