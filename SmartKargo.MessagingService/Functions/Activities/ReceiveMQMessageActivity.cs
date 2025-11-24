using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity invoked by ReceiveMQMessageOrchestrator.
    /// </summary>
    public class ReceiveMQMessageActivity
    {
        private readonly ILogger<ReceiveMQMessageActivity> _logger;
        private readonly Cls_BL _cls_BL;

        public ReceiveMQMessageActivity(
            ILogger<ReceiveMQMessageActivity> logger,
            Cls_BL cls_BL)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cls_BL = cls_BL ?? throw new ArgumentNullException(nameof(cls_BL));
        }

        /// <summary>
        /// Durable Activity entry point.
        /// </summary>
        [Function(nameof(ReceiveMQMessageActivity))]
        public async Task RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(ReceiveMQMessageActivity), Input = input });

            _logger.LogInformation("ReceiveMQMessageActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Execute MQ message receive logic
                await _cls_BL.ReceiveMQMessage().ConfigureAwait(false);

                _logger.LogInformation("ReceiveMQMessageActivity completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("ReceiveMQMessageActivity was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReceiveMQMessageActivity failed.");
                throw;
            }
        }
    }
}