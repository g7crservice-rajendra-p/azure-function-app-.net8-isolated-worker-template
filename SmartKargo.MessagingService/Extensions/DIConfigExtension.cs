using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmartKargo.MessagingService.Data.Dao.Implementations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;

namespace SmartKargo.MessagingService.Extensions
{
    public static class NativeInjectorBootStrapper
    {
        public static void RegisterServices(FunctionsApplicationBuilder builder)
        {
            // Application
            //builder.Services.AddScoped<IEncryptionService, EncryptionService>();

            // Data
            builder.Services.AddScoped<ISqlDataHelperDao, SqlDataHelperDao>(); // For direct injection (optional)
            builder.Services.AddScoped<ISqlDataHelperFactory, SqlDataHelperFactory>();
        }
    }
    public static class DependencyInjectionConfigExtension
    {
        public static FunctionsApplicationBuilder AddDependencyInjectionConfiguration(this FunctionsApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            NativeInjectorBootStrapper.RegisterServices(builder);

            return builder;
        }
    }
}
