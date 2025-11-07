using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Data.DbConstants;
using System.Data;

namespace SmartKargo.MessagingService.Functions.Activities;

/// <summary>
/// Durable Activity Function that fetches pending email messages from the database.
/// </summary>
public class FetchPendingMessagesActivity
{
    private readonly ISqlDataHelperDao _readWriteDao;
    private readonly ILogger<FetchPendingMessagesActivity> _logger;

    public FetchPendingMessagesActivity(
        ISqlDataHelperFactory sqlDataHelperFactory,
        ILogger<FetchPendingMessagesActivity> logger)
    {
        _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
        _logger = logger;
    }

    /// <summary>
    /// Fetches pending messages using the MailtoSend stored procedure.
    /// </summary>
    /// <param name="_">Unused input parameter required by Durable Functions.</param>
    /// <returns>DataSet containing pending messages, or null if none found.</returns>
    [Function(nameof(FetchPendingMessagesActivity))]
    public async Task<DataSet?> RunAsync([ActivityTrigger] object? _)
    {
        try
        {
            _logger.LogInformation("Fetching pending messages using stored procedure: {StoredProcedure}.", StoredProcedures.MailtoSend);

            var dbRes = await _readWriteDao.SelectRecords(StoredProcedures.MailtoSend);

            _logger.LogInformation("Successfully fetched pending messages. Table count: {TableCount} in activity '{ActivityName}", dbRes?.Tables.Count ?? 0, nameof(FetchPendingMessagesActivity));

            return dbRes;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while executing stored procedure '{StoredProcedure}' in activity '{ActivityName}'.",
                StoredProcedures.MailtoSend,
                nameof(FetchPendingMessagesActivity));

            throw;
        }
    }
}
