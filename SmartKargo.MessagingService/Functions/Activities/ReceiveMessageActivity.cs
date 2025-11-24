using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity invoked by ReceiveMessageOrchestrator.
    /// </summary>
    public class ReceiveMessageActivity
    {
        private readonly ILogger<ReceiveMessageActivity> _logger;
        private readonly Cls_BL _cls_BL;
        private readonly FTP _fTP;
        private readonly AzureDrive _azureDrive;

        public ReceiveMessageActivity(
            ILogger<ReceiveMessageActivity> logger,
            Cls_BL cls_BL,
            FTP fTP,
            AzureDrive azureDrive)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cls_BL = cls_BL ?? throw new ArgumentNullException(nameof(cls_BL));
            _fTP = fTP ?? throw new ArgumentNullException(nameof(fTP));
            _azureDrive = azureDrive ?? throw new ArgumentNullException(nameof(azureDrive));
        }

        /// <summary>
        /// Durable Activity entry point.
        /// </summary>
        [Function(nameof(ReceiveMessageActivity))]
        public async Task RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(ReceiveMessageActivity), Input = input });

            _logger.LogInformation("ReceiveMessageActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Step 1: Read Mailbox
                _logger.LogInformation("Reading mailbox...");
                await _cls_BL.ReadMailFromMailBox().ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                // Step 2: Download Files via SFTP
                _logger.LogInformation("Downloading files from SFTP...");
                await _fTP.SITASFTPDownloadFile().ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                // Step 3: Read from Azure Drive
                _logger.LogInformation("Processing files from Azure Drive...");
                await _azureDrive.ReadFromSITADrive().ConfigureAwait(false);

                _logger.LogInformation("ReceiveMessageActivity completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("ReceiveMessageActivity was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReceiveMessageActivity failed.");
                throw;
            }
        }
    }
}