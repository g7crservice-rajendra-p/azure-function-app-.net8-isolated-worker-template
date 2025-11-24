using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity invoked by SendMailOrchestrator (if any).
    /// </summary>
    public class SendMailActivity
    {
        private readonly ILogger<SendMailActivity> _logger;
        private readonly Cls_BL _cls_BL;

        public SendMailActivity(
            ILogger<SendMailActivity> logger,
            Cls_BL cls_BL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cls_BL = cls_BL ?? throw new ArgumentNullException(nameof(cls_BL));
        }

        /// <summary>
        /// Durable Activity entry point.
        /// </summary>
        [Function(nameof(SendMailActivity))]
        public async Task RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(SendMailActivity), Input = input });

            _logger.LogInformation("SendMailActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Execute email logic (no return value)
                await _cls_BL.SendMail().ConfigureAwait(false);

                _logger.LogInformation("SendMailActivity completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SendMailActivity was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMailActivity failed.");
                throw;
            }
        }
    }
}