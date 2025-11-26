using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Functions.Triggers;

public class BlobUploadTriggerStarter
{
    private readonly ILogger _logger;
    AppConfig _appConfig;

    public BlobUploadTriggerStarter(
        ILoggerFactory loggerFactory, 
        AppConfig appConfig
    )
    {
        _logger = loggerFactory.CreateLogger<BlobUploadTriggerStarter>();
        _appConfig = appConfig;
    }

    /// <summary>
    /// Production-ready BlobTrigger Function
    /// </summary>
    [Function(nameof(BlobUploadTriggerStarter))]
    public async Task Run(
        //[BlobTrigger("inbound/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
        [BlobTrigger("incoming/{name}", Connection = "AzureWebJobsStorage")] BlobClient sourceBlobClient,
        string name,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var instanceId = Guid.NewGuid().ToString();
        //var blobSize = blobStream.Length;

        try
        {
            _logger.LogInformation("BlobTrigger fired for '{Name}'", name);

            // This is your SAME storage account connection string
            string storageConn = _appConfig.AppLogging.ConnectionString;

            // Destination container is different
            BlobClient destBlobClient = new BlobClient(storageConn, "processed", name);

            // Idempotency: skip if already exists
            if (await destBlobClient.ExistsAsync())
            {
                _logger.LogInformation("Skipping — '{Name}' already exists in destination.", name);
                return;
            }

            // Perform server-side copy
            var copyOp = await destBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

            _logger.LogInformation(
                "Started copy for '{Name}'.",
                name
            );


            //_logger.LogInformation(
            //    "BlobUploadTriggerStarter fired at {Now}. Blob '{BlobName}', Size: {BlobSize} bytes. Instance ID: '{InstanceId}'.",
            //    DateTime.UtcNow, name, blobSize, instanceId);

            //// 1. Readiness handling (same pattern as your timer trigger)
            //if (!_readiness.IsReady)
            //{
            //    _logger.LogInformation("System not yet ready — waiting for StartupReadiness...");
            //    await _readiness.WaitForReadyAsync(TimeSpan.FromSeconds(30));
            //}

            //// 2. Read blob bytes if required
            //byte[] blobBytes;
            //using (var ms = new MemoryStream())
            //{
            //    await blobStream.CopyToAsync(ms);
            //    blobBytes = ms.ToArray();
            //}

            //// 3. Create input for orchestrator
            //var orchestratorInput = new BlobProcessInput
            //{
            //    FileName = name,
            //    BlobSize = blobSize,
            //    UploadedOnUtc = DateTime.UtcNow,
            //    Content = blobBytes
            //};

            //// 4. Trigger orchestrator
            //await client.ScheduleNewOrchestrationInstanceAsync(
            //    nameof(ProcessBlobOrchestrator),
            //    orchestratorInput,
            //    new StartOrchestrationOptions(instanceId));

            //_logger.LogInformation(
            //    "Started ProcessBlobOrchestrator with Instance ID '{InstanceId}' for blob '{BlobName}'.",
            //    instanceId, name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed in BlobUploadTriggerStarter at {Now}. Blob '{BlobName}', InstanceId: '{InstanceId}'.",
                DateTime.UtcNow, name, instanceId);

            throw; // Important for retry semantics
        }
    }
}
