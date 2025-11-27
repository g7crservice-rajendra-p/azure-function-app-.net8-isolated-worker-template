using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QidWorkerRole;
using QidWorkerRole.BAL;
using QidWorkerRole.SIS;
using QidWorkerRole.SIS.DAL;
using QidWorkerRole.SIS.FileHandling;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.ISValidationReport;
using QidWorkerRole.UploadMasters;
using QidWorkerRole.UploadMasters.Agent;
using QidWorkerRole.UploadMasters.AircraftPattern;
using QidWorkerRole.UploadMasters.Airports;
using QidWorkerRole.UploadMasters.Booking;
using QidWorkerRole.UploadMasters.CapacityAllocation;
using QidWorkerRole.UploadMasters.CCAUpload;
using QidWorkerRole.UploadMasters.Collection;
using QidWorkerRole.UploadMasters.CostLine;
using QidWorkerRole.UploadMasters.DCM;
using QidWorkerRole.UploadMasters.ExchangeRates;
using QidWorkerRole.UploadMasters.ExchangeRatesFromTo;
using QidWorkerRole.UploadMasters.FlightBudget;
using QidWorkerRole.UploadMasters.FlightCapacity;
using QidWorkerRole.UploadMasters.FlightPaxInfo;
using QidWorkerRole.UploadMasters.FlightSchedule;
using QidWorkerRole.UploadMasters.FlightScheduleExcel;
using QidWorkerRole.UploadMasters.MSRRates;
using QidWorkerRole.UploadMasters.OtherCharges;
using QidWorkerRole.UploadMasters.PartnerMaster;
using QidWorkerRole.UploadMasters.PartnerSchedule;
using QidWorkerRole.UploadMasters.RateLine;
using QidWorkerRole.UploadMasters.RouteControl;
using QidWorkerRole.UploadMasters.ShipperConsignee;
using QidWorkerRole.UploadMasters.Taxline;
using QidWorkerRole.UploadMasters.UserMaster;
using QidWorkerRole.UploadMasters.Vendor;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Implementations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;

namespace SmartKargo.MessagingService.Extensions
{
    /// <summary>
    /// Centralized, explicit dependency registrations for production.
    /// This class intentionally avoids automatic assembly scanning and large
    /// commented blocks — keep registrations here minimal and explicit.
    /// </summary>
    public static class DependencyInjectionAndValidator
    {
        /// <summary>
        /// Register explicit services only. Call from Functions host startup.
        /// </summary>
        public static FunctionsApplicationBuilder AddDependencyInjectionConfiguration(
            this FunctionsApplicationBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            // Try to obtain a logger (best-effort). Avoid depending on a fully-built ServiceProvider for real work.
            var sp = builder.Services.BuildServiceProvider();
            var loggerFactory = sp?.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("DI:ExplicitRegistrar");

            try
            {
                NativeInjectorBootStrapper.RegisterServices(builder, logger);
                logger?.LogInformation("Dependency injection configuration applied successfully.");
                return builder;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during DI configuration.");
                throw;
            }
        }

        /// <summary>
        /// Minimal explicit registrations kept here.
        /// Add new registrations deliberately and keep scope/lifetime intentional.
        /// </summary>
        private static class NativeInjectorBootStrapper
        {
            public static void RegisterServices(FunctionsApplicationBuilder builder, ILogger? logger)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                // Ensure AppConfig is already registered and bound before calling this method
                var appConfig = builder.Configuration.Get<AppConfig>()
                                ?? throw new InvalidOperationException("AppConfig not loaded. Call AddApiConfiguration before AddDependencyInjectionConfiguration.");

                var efConnectionString = appConfig.Database?.EfConnectionString;
                if (string.IsNullOrWhiteSpace(efConnectionString))
                {
                    throw new InvalidOperationException("Missing Entity Framework connection string in AppConfig: ConnectionStrings:EntityFramework");
                }

                var services = builder.Services;

                // Register a factory for SISDBEntities so callers can obtain a fresh instance when needed.
                services.AddSingleton<Func<SISDBEntities>>(
                    sp => () => new SISDBEntities(efConnectionString)
                );

                // Core data layer registrations
                services.AddScoped<ISqlDataHelperDao, SqlDataHelperDao>();
                services.AddScoped<ISqlDataHelperFactory, SqlDataHelperFactory>();

                //HttpClient
                services.AddHttpClient<EMAILOUT>();

                services.AddScoped<ASM>();
                services.AddScoped<AWBDetailsAPI>();
                services.AddScoped<AzureDrive>();
                services.AddScoped<balRapidInterface>();
                services.AddScoped<balRapidInterfaceForCebu>();
                services.AddScoped<BookingExcelUpload>();
                services.AddScoped<CarditResiditManagement>();
                services.AddScoped<CGOProcessor>();
                services.AddScoped<CIMPMessageValidation>();
                services.AddScoped<Cls_BL>();
                services.AddScoped<cls_Encode_Decode>();
                services.AddScoped<cls_SCMBL>();
                services.AddScoped<CreateDBData>();
                services.AddScoped<CustomsImportBAL>();
                services.AddScoped<CustomsMessageProcessor>();
                services.AddScoped<ExchangeRateExpiryAlert>();
                services.AddScoped<FBLMessageProcessor>();
                services.AddScoped<FBRMessageProcessor>();
                services.AddScoped<FDMMessageProcessor>();
                services.AddScoped<FFAMessageProcessor>();
                services.AddScoped<FFMMessageProcessor>();
                services.AddScoped<FFRMessageProcessor>();
                services.AddScoped<FHLMessageProcessor>();
                services.AddScoped<FlightCapacity>();
                services.AddScoped<FlightPaxInfo>();
                services.AddScoped<FNAMessageProcessor>();
                services.AddScoped<FRPMessageProcessor>();
                services.AddScoped<FSBMessageProcessor>();
                services.AddScoped<FSRMessageProcessor>();
                services.AddScoped<FSUMessageProcessor>();
                services.AddScoped<FTP>();
                services.AddScoped<FWBMessageProcessor>();
                services.AddScoped<FWRMessageProcessor>();
                services.AddScoped<GenericFunction>();
                services.AddScoped<IdecFileReader>();
                services.AddScoped<InvoiceCollection>();
                services.AddScoped<ISValidationReportReader>();
                services.AddScoped<LDMMessageProcessor>();
                services.AddScoped<MailKitManager>();
                services.AddScoped<PHCustomRegistry>();
                services.AddScoped<PSNMessageProcessor>();
                services.AddScoped<RapidException>();
                services.AddScoped<RapidInterfaceMethods>();
                services.AddScoped<RateExpiryAlert>();
                services.AddScoped<ReadDBData>();
                services.AddScoped<SISBAL>();
                services.AddScoped<SMSOUT>();
                services.AddScoped<TcpIMAP>();
                services.AddScoped<UnDepartedAWBListAlert>();
                services.AddScoped<UpdateDBData>();
                services.AddScoped<UploadAgentMaster>();
                services.AddScoped<UploadAgentMasterGeneralInfo>();
                services.AddScoped<UploadAgentMasterUpdate>();
                services.AddScoped<UploadAircraftLoadingPattern>();
                services.AddScoped<UploadAirportsMaster>();
                services.AddScoped<UploadCapacityAllocation>();
                services.AddScoped<UploadCostMaster>();
                services.AddScoped<UploadDCM>();
                services.AddScoped<UploadExchangeRates>();
                services.AddScoped<UploadExchangeRatesFromTo>();
                services.AddScoped<UploadFlightBudget>();
                services.AddScoped<UploadFlightSchedule>();
                services.AddScoped<UploadFlightScheduleExcel>();
                services.AddScoped<UploadMasterCommon>();
                services.AddScoped<UploadMSRRates>();
                services.AddScoped<UploadOtherChargesMaster>();
                services.AddScoped<UploadPartnerMaster>();
                services.AddScoped<UploadPartnerSchedule>();
                services.AddScoped<UploadRateLineMaster>();
                services.AddScoped<UploadRouteControl>();
                services.AddScoped<UploadShipperConsigneeMaster>();
                services.AddScoped<UploadTaxLine>();
                services.AddScoped<UploadUserMaster>();
                services.AddScoped<UploadVendorMaster>();
                services.AddScoped<WebService>();
                services.AddScoped<XFBLMessageProcessor>();
                services.AddScoped<XFFMMessageProcessor>();
                services.AddScoped<XFFRMessageProcessor>();
                services.AddScoped<XFHLMessageProcessor>();
                services.AddScoped<XFNMMessageProcessor>();
                services.AddScoped<XFSUMessageProcessor>();
                services.AddScoped<XFWBMessageProcessor>();
                services.AddScoped<XFZBMessageProcessor>();
                services.AddScoped<CCAUploadFile>();
                services.AddScoped<EMAILOUT>();
                services.AddScoped<MVT>();
                services.AddScoped<SAPInterfaceProcessor>();
                services.AddScoped<SSM>();
                services.AddScoped<SISFileReader>();

                // Func<T> factories
                services.AddScoped<Func<UploadMasterCommon>>(sp => () => sp.GetRequiredService<UploadMasterCommon>());
                services.AddScoped<Func<cls_SCMBL>>(sp => () => sp.GetRequiredService<cls_SCMBL>());
                services.AddScoped<Func<XFSUMessageProcessor>>(sp => () => sp.GetRequiredService<XFSUMessageProcessor>());
                services.AddScoped<Func<MailKitManager>>(sp => () => sp.GetRequiredService<MailKitManager>());
                services.AddScoped<Func<Cls_BL>>(sp => () => sp.GetRequiredService<Cls_BL>());

                logger?.LogInformation("NativeInjectorBootStrapper: core services registered.");
            }
        }
    }
}
