using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SmartKargo.MessagingService.Logging
{
    public interface ISerilogConfiguration
    {
        LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration, IHostEnvironment environment);
    }
}
