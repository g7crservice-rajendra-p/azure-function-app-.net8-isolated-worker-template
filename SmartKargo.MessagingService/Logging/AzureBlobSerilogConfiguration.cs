using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using SmartKargo.MessagingService.Configurations;

namespace SmartKargo.MessagingService.Logging
{
    /// <summary>
    /// Configures Serilog to write application logs to Azure Blob Storage.
    /// Ensures asynchronous non-blocking writes, structured JSON logs,
    /// and safe fallback behavior when Azure logging is unavailable.
    /// </summary>
    public sealed class AzureBlobSerilogConfiguration : BaseSerilogConfiguration
    {
        // === Constants ===
        private const long DefaultBlobSizeLimitBytes = 100 * 1024 * 1024; // 100 MB per blob
        //private const int DefaultRetentionDays = 31; // Default retention: 1 month
        private const int DefaultFlushToDiskIntervalSec = 10; // Flush every 10 seconds
        private const int DefaultBatchPostingLimit = 100; // Batch upload count

        /// <summary>
        /// Configures Serilog to use Azure Blob Storage sink.
        /// Reads connection details from the application configuration.
        /// </summary>
        public override LoggerConfiguration Configure(
            LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            // Start from the base configuration (adds console, etc..)
            var baseConfig = base.Configure(loggerConfiguration, configuration, environment);

            // Retrieve the 'AppLogging' section specifically
            var appLoggingSection = configuration.GetSection(nameof(AppConfig.AppLogging));
            var appLogging = appLoggingSection.Get<AppLoggingConfig>();

            if (appLogging == null)
            {
                Console.Error.WriteLine("Azure Blob Storage logging configuration is missing.");
                return baseConfig;
            }

            string blobConnectionString = appLogging.ConnectionString.Trim();
            string blobContainerName = appLogging.ContainerName ?? "app-logs";

            //int retentionDays = appLogging.RetentionDays > 0 ? appLogging.RetentionDays : DefaultRetentionDays;

            // Validate Azure connection string
            if (string.IsNullOrWhiteSpace(blobConnectionString))
            {
                Console.Error.WriteLine("Azure Blob Storage connection string is missing. Blob logging disabled.");
                return baseConfig;
            }

            try
            {
                // Base prefix (only app + environment)
                string blobBasePrefix = $"{environment.ApplicationName}/{environment.EnvironmentName}";
                //string blobName = $"{environment.ApplicationName}/{LogLevel}/log-{DateTime.UtcNow:yyyyMMdd}.txt";

                // Enable internal Serilog diagnostic messages (optional)
                Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog] {msg}"));


                // 1. INFORMATION & BELOW → info/
                baseConfig = baseConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level < LogEventLevel.Warning)
                    .WriteTo.Async(a => a.AzureBlobStorage(
                        connectionString: blobConnectionString,
                        storageContainerName: blobContainerName,
                        storageFileName: $"{blobBasePrefix}/info/{DateTime.UtcNow:yyyyMMdd}.txt",
                        restrictedToMinimumLevel: LogEventLevel.Verbose,
                        formatter: new JsonFormatter(renderMessage: true),
                        period: TimeSpan.FromSeconds(DefaultFlushToDiskIntervalSec),
                        batchPostingLimit: DefaultBatchPostingLimit,
                        blobSizeLimitBytes: DefaultBlobSizeLimitBytes,
                        // retainedBlobCountLimit: 90,
                        contentType: "application/text"
                    ))
                );

                // 2. WARNINGS ONLY → warnings/
                baseConfig = baseConfig.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.Async(a => a.AzureBlobStorage(
                        connectionString: blobConnectionString,
                        storageContainerName: blobContainerName,
                        storageFileName: $"{blobBasePrefix}/warnings/{DateTime.UtcNow:yyyyMMdd}.txt",
                        restrictedToMinimumLevel: LogEventLevel.Warning,
                        formatter: new JsonFormatter(renderMessage: true),
                        period: TimeSpan.FromSeconds(DefaultFlushToDiskIntervalSec),
                        batchPostingLimit: DefaultBatchPostingLimit,
                        blobSizeLimitBytes: DefaultBlobSizeLimitBytes,
                        // retainedBlobCountLimit: 90,
                        contentType: "application/text"
                    ))
                );

                // 3. ERRORS & FATAL → errors/
                baseConfig = baseConfig.WriteTo.Async(a => a.AzureBlobStorage(
                    connectionString: blobConnectionString,
                    storageContainerName: blobContainerName,
                    storageFileName: $"{blobBasePrefix}/errors/{DateTime.UtcNow:yyyyMMdd}.txt",
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    formatter: new JsonFormatter(renderMessage: true),
                    period: TimeSpan.FromSeconds(DefaultFlushToDiskIntervalSec),
                    batchPostingLimit: DefaultBatchPostingLimit,
                    blobSizeLimitBytes: DefaultBlobSizeLimitBytes,

                    /*After this limit is reached, the oldest blobs are deleted. Currently disabled by commenting*/
                    //retainedBlobCountLimit: retentionDays,

                    contentType: "application/text"
                ));
            }
            catch (Exception ex)
            {
                // Log configuration failure to console (startup-safe)
                Console.Error.WriteLine($"[Startup Error] Failed to configure Azure Blob Storage logging: {ex.Message}");
            }

            return baseConfig;
        }
    }
}
