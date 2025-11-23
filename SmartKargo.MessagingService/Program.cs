using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SmartKargo.MessagingService.Extensions;
using static SmartKargo.MessagingService.Extensions.DependencyInjectionAndValidator;

var builder = FunctionsApplication.CreateBuilder(args);

// --------------------
// Environment Setup
// --------------------
var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Production";
Console.WriteLine($"Starting SmartKargo Messaging Service - Environment: {environment}");

// --------------------
// Configuration & Dependency Injection
// --------------------
builder.AddApiConfiguration()
       .AddDependencyInjectionConfiguration();

// Optional: enable WebApplication pipeline (if using HTTP triggers with controllers)
builder.ConfigureFunctionsWebApplication();

// --------------------
// Logging (Serilog)
// --------------------
try
{
    builder.AddSerilogConfiguration(); // Custom extension for Serilog setup
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to initialize Serilog: {ex.Message}");
    throw;
}


//// --------------------
//// Application Insights
//// --------------------
//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();


// --------------------
// Build & Run with Safety
// --------------------
try
{
    var host = builder.Build();

    Log.Information("SmartKargo Messaging Service Function App starting up...");
    DiConstructorDependencyValidator.Validate(host, logger: host.Services.GetService<ILoggerFactory>()?.CreateLogger("DIValidator"), throwOnMissing: false);
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SmartKargo Messaging Service failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}