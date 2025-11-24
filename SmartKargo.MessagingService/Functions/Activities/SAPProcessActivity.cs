using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QidWorkerRole;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity invoked by SAPProcessOrchestrator.
    /// </summary>
    public class SAPProcessActivity
    {
        private readonly ILogger<SAPProcessActivity> _logger;
        private readonly SAPInterfaceProcessor _sAPInterfaceProcessor;

        public SAPProcessActivity(
            ILogger<SAPProcessActivity> logger,
            SAPInterfaceProcessor sAPInterfaceProcessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sAPInterfaceProcessor = sAPInterfaceProcessor ?? throw new ArgumentNullException(nameof(sAPInterfaceProcessor));
        }

        /// <summary>
        /// Durable Activity entry point.
        /// </summary>
        [Function(nameof(SAPProcessActivity))]
        public async Task RunAsync(
            [ActivityTrigger] object? input,
            CancellationToken cancellationToken)
        {
            using var scope = _logger.BeginScope(new { Activity = nameof(SAPProcessActivity), Input = input });

            _logger.LogInformation("SAPProcessActivity started.");

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                string dailyExecutionDateTime = ConfigCache.Get("DailySAPInterfaceDateTime");

                if (string.IsNullOrWhiteSpace(dailyExecutionDateTime))
                {
                    _logger.LogWarning("Config value 'DailySAPInterfaceDateTime' is missing. Skipping SAP interface generation.");
                    return;
                }

                if (!DateTime.TryParse(dailyExecutionDateTime, out DateTime scheduledTime))
                {
                    _logger.LogWarning("Invalid 'DailySAPInterfaceDateTime' format in config. Value: {Value}", dailyExecutionDateTime);
                    return;
                }

                DateTime now = DateTime.UtcNow;

                // Only proceed if current UTC time has passed the scheduled config time
                if (now >= scheduledTime)
                {
                    DateTime currentDate = now.Date;                 // today's date (UTC)
                    DateTime dtFromDate = currentDate.AddDays(-1);   // yesterday
                    DateTime dtToDate = currentDate.AddDays(-1);     // yesterday
                    DateTime updatedOn = currentDate;
                    const string updatedBy = "SmartKargoA";

                    _logger.LogInformation(
                        "Executing SAP interface generation for date: {Date}.",
                        dtToDate.ToString("yyyy-MM-dd"));

                    await _sAPInterfaceProcessor
                        .GenerateSAPInterface(dtFromDate, dtToDate, updatedBy, updatedOn)
                        .ConfigureAwait(false);

                    _logger.LogInformation("SAP interface generation completed successfully.");
                }
                else
                {
                    _logger.LogInformation(
                        "Current time ({Now}) has not reached scheduled time ({Scheduled}). Skipping SAP interface generation.",
                        now, scheduledTime);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SAPProcessActivity was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SAPProcessActivity failed.");
                throw;
            }
        }
    }
}