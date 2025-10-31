using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SmartKargo.MessagingService.Logging;

namespace SmartKargo.MessagingService.Extensions
{
    public static class SerilogConfigExtension
    {
        public static FunctionsApplicationBuilder AddSerilogConfiguration(this FunctionsApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // Get all configurations that implement ISerilogConfiguration
            var configurations = new List<ISerilogConfiguration>
        {
            new ConsoleSerilogConfiguration(),
            new AzureBlobSerilogConfiguration()
            // Add new configurations here
        };

            var loggerConfiguration = new LoggerConfiguration();

            foreach (var config in configurations)
            {
                loggerConfiguration = config.Configure(loggerConfiguration, builder.Configuration, builder.Environment);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(Log.Logger, dispose: true);
            });

            builder.Services.AddSingleton<IHostedService, SerilogFlushService>();

            return builder;
        }
    }
}
