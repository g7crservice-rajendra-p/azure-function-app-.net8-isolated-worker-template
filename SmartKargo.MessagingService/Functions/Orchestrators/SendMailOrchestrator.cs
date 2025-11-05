using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Activities;
using SmartKargo.MessagingService.Functions.Entities;
using System.Data;

namespace SmartKargo.MessagingService.Functions.Orchestrators;

/// <summary>
/// Durable Orchestrator responsible for fetching pending mail messages (via an activity),
/// and handing each message to ProcessMessageActivity along with the configuration
/// read from the ConfigEntity.
/// 
/// Important: orchestration code must be deterministic. Activities and entity calls are fine here.
/// </summary>
public static class SendMailOrchestrator
{
    [Function(nameof(SendMailOrchestrator))]
    public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // Replay-safe logger so logs aren't duplicated during orchestration replay.
        ILogger logger = context.CreateReplaySafeLogger(nameof(SendMailOrchestrator));

        logger.LogInformation("SendMailOrchestrator started at orchestration time: {OrchestrationTime}", context.CurrentUtcDateTime);

        bool isOn;

        do
        {
            isOn = false;

            // 1) Fetch one batch (DataSet) of pending messages - activity executes stored procedure spMailtoSend.
            DataSet? ds = null;
            try
            {
                logger.LogDebug("Calling activity: {ActivityName} to fetch pending messages.", nameof(FetchPendingMessagesActivity));
                ds = await context.CallActivityAsync<DataSet?>(nameof(FetchPendingMessagesActivity));
            }
            catch (Exception ex)
            {
                // If the activity throws, log error and break to avoid hot-looping on a failing activity.
                logger.LogError(ex, "FetchPendingMessagesActivity failed. Aborting loop to avoid repeated failures.");
                break;
            }

            // If no dataset or no tables, then nothing to process — exit loop.
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
            {
                logger.LogInformation("No pending messages returned by {ActivityName}. Ending orchestrator loop.", nameof(FetchPendingMessagesActivity));
                break;
            }

            var table0 = ds.Tables[0];
            if (table0 == null || table0.Rows.Count == 0)
            {
                logger.LogInformation("Fetched DataSet contains zero rows. Ending orchestrator loop.");
                break;
            }

            // At least one row exists -> set isOn to true so loop continues if more messages are found.
            isOn = true;
            logger.LogInformation("Fetched {RowCount} pending message(s) in current batch.", table0.Rows.Count);

            //// Extract messageId from the first row (defensive parsing).
            //int messageId;
            //try
            //{
            //    var raw = table0.Rows[0][0];
            //    messageId = Convert.ToInt32(raw);
            //    logger.LogInformation("Processing message id: {MessageId}", messageId);
            //}
            //catch (Exception ex)
            //{
            //    // If ID parse fails, log warning and continue to next iteration (so we don't stop processing).
            //    logger.LogWarning(ex, "Failed to parse message id from the fetched row; skipping this row and continuing.");
            //    continue;
            //}

            //// Prepare input payload for the ProcessMessageActivity.
            //var input = new ProcessMessageInput
            //{
            //    // DataSet is serializable and is safe to pass via durable orchestration history.
            //    MessageDataSet = ds,

            //    // Pass a case-insensitive copy (or the original if already present) so activities can do insensitive lookups.
            //    Config = new Dictionary<string, string>(configFallback, StringComparer.OrdinalIgnoreCase)
            //};

            //// Call the ProcessMessageActivity and log the result.
            //try
            //{
            //    logger.LogDebug("Calling activity: {ActivityName} to process message {MessageId}.", nameof(ProcessMessageActivity), messageId);
            //    var processed = await context.CallActivityAsync<bool>(nameof(ProcessMessageActivity), input);

            //    if (processed)
            //    {
            //        logger.LogInformation("ProcessMessageActivity completed successfully for message id {MessageId}.", messageId);
            //    }
            //    else
            //    {
            //        // Activity returned false — application-level failure (e.g., couldn't send or store result). Log with warning.
            //        logger.LogWarning("ProcessMessageActivity returned false for message id {MessageId} — the message was not processed successfully. It may be retried later.", messageId);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Activity threw an exception. Log and continue to next message if possible.
            //    logger.LogError(ex, "ProcessMessageActivity threw an exception while processing message id {MessageId}. Continuing to next message.", messageId);

            //    // Depending on desired behavior, you may want to re-throw to fail orchestration, or continue as done here.
            //    // We continue to next message to keep processing other pending messages.
            //    continue;
            //}

            // Loop will continue if isOn remains true and FetchPendingMessagesActivity returns more rows in next iteration.
        }
        while (isOn);

        logger.LogInformation("SendMailOrchestrator completed at orchestration time: {OrchestrationTime}", context.CurrentUtcDateTime);
    }

    /// <summary>
    /// Payload passed into ProcessMessageActivity so the activity receives both the DataSet
    /// and configuration values read from the ConfigEntity (or empty fallback).
    /// 
    /// Notes:
    /// - The class is marked [Serializable] to ensure it can be stored safely in orchestration history.
    /// - Config is recommended to use a case-insensitive comparer so lookups like "msgService_SendEmail"
    ///   succeed regardless of case.
    /// </summary>
    [Serializable]
    public class ProcessMessageInput
    {
        // DataSet is serializable and will be passed through the orchestration history.
        public DataSet? MessageDataSet { get; set; }

        // Config dictionary read from ConfigEntity (may be empty). Use case-insensitive comparer recommended.
        public IDictionary<string, string>? Config { get; set; }
    }
}
