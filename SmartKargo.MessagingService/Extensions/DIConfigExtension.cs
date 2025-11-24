using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QidWorkerRole;
using QidWorkerRole.SIS.DAL;
using QidWorkerRole.UploadMasters;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Implementations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SmartKargo.MessagingService.Extensions
{
    /// <summary>
    /// Combined auto-registration and constructor-dependency validator.
    /// </summary>
    public static class DependencyInjectionAndValidator
    {
        /// <summary>
        /// Register explicit services manual registrations and then auto-registers discovered concrete types.
        /// Conservative defaults:
        ///  - Scans entry assembly
        ///  - Filters by namespace filters (defaults include QidWorkerRole)
        ///  - Registers only types with ctor parameters or types named *Processor/*Service/*Manager/*Upload
        ///  - Skips POCO/model/EF generated types
        /// </summary>
        public static FunctionsApplicationBuilder AddDependencyInjectionConfiguration(
            this FunctionsApplicationBuilder builder,
            Assembly[]? assembliesToScan = null,
            string[]? namespaceFilters = null,
            ServiceLifetime lifetime = ServiceLifetime.Scoped,
            bool onlyRegisterCtorInjectables = true)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("DI:AutoRegistrar");

            // Preserve explicit registrations first
            NativeInjectorBootStrapper.RegisterServices(builder, logger);

            // Defaults
            assembliesToScan ??= [Assembly.GetExecutingAssembly()];
            namespaceFilters = [.. (namespaceFilters ?? ["QidWorkerRole"]).Where(s => !string.IsNullOrWhiteSpace(s))];

            // Gather already-registered types (best-effort)
            var existingRegisteredTypes = new HashSet<Type>(builder.Services
                .Select(sd => sd.ServiceType)
                .OfType<Type>());

            var discovered = new List<Type>();

            foreach (var asm in assembliesToScan.Distinct())
            {
                if (asm == null) continue;

                Type[] allTypes;
                try
                {
                    allTypes = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException rtlEx)
                {
                    logger?.LogWarning(rtlEx, "Assembly {Assembly} failed to fully load; using available types.", asm.FullName);
                    allTypes = rtlEx.Types.Where(t => t != null).ToArray()!;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error enumerating types in assembly {Assembly}. Skipping it.", asm.FullName);
                    continue;
                }

                foreach (var t in allTypes)
                {
                    if (t == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (!t.IsClass || t.IsAbstract)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(t.Namespace))
                        {
                            continue;
                        }

                        // Namespace match
                        if (!namespaceFilters.Any(f => t.Namespace!.Contains(f, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        // Skip compiler-generated and iterator state machine classes
                        if (t.Name.StartsWith("<") ||
                            t.CustomAttributes.Any(a => a.AttributeType == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) ||
                            (t.IsNested && t.Name.Contains("d__")))
                        {
                            continue;
                        }

                        if (t.Namespace?.Contains(".CustomConverters", StringComparison.OrdinalIgnoreCase) == true
                             || t.Name.EndsWith("Converter", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Skip DTO/model/EF artifact
                        if (IsPocoOrModel(t))
                        {
                            continue;
                        }

                        // Heuristics: either has ctor params or matches service-like naming
                        var hasCtorParams = t.GetConstructors().Any(c => c.GetParameters().Length > 0);
                        var looksLikeService = t.Name.EndsWith("Processor", StringComparison.OrdinalIgnoreCase)
                                              || t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase)
                                              || t.Name.EndsWith("Manager", StringComparison.OrdinalIgnoreCase)
                                              || t.Name.EndsWith("Upload", StringComparison.OrdinalIgnoreCase)
                                              || t.Name.EndsWith("Handler", StringComparison.OrdinalIgnoreCase);

                        if (onlyRegisterCtorInjectables && !hasCtorParams && !looksLikeService)
                        {
                            continue;
                        }

                        if (existingRegisteredTypes.Contains(t))
                        {
                            continue;
                        }

                        // Skip EF DbContext-like names and known special types
                        if (t.Name.EndsWith("Entities", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        /*Ignored this class DI as this object created explicitly
                         baecause ctor expecting string filepath during initialization */
                        if (t.Name.Contains("XmlFileReader"))
                        {
                            continue;
                        }

                        discovered.Add(t);
                    }
                    catch (Exception ex)
                    {
                        // Don't break the loop for a single bad type; log and continue
                        logger?.LogError(ex, "Skipping type {TypeName} due to exception during discovery.", t?.FullName);
                    }
                }
            }

            // Register discovered types as themselves
            var distinct = discovered.Distinct().ToList();
            foreach (var t in distinct)
            {
                try
                {
                    switch (lifetime)
                    {
                        case ServiceLifetime.Singleton:
                            builder.Services.AddSingleton(t);
                            break;
                        case ServiceLifetime.Transient:
                            builder.Services.AddTransient(t);
                            break;
                        default:
                            builder.Services.AddScoped(t);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Failed to register discovered type {TypeName}.", t.FullName);
                }
            }

            // Register the discovered type list for later validation (store as readonly list)
            try
            {
                var ro = new ReadOnlyCollection<Type>(distinct);
                builder.Services.AddSingleton<IReadOnlyCollection<Type>>(ro);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Could not persist discovered type list into DI container.");
            }

            return builder;
        }

        private static bool IsPocoOrModel(Type t)
        {
            if (t == null)
            {
                return false;
            }

            var n = t.Name ?? string.Empty;
            var ns = t.Namespace ?? string.Empty;

            if (n.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("Model", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("Entity", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("Record", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("Context", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("Designer", StringComparison.OrdinalIgnoreCase)
                || n.EndsWith("Repository", StringComparison.OrdinalIgnoreCase) // optional heuristic
               )
            {
                return true;
            }

            if (ns.IndexOf(".Model", StringComparison.OrdinalIgnoreCase) >= 0
                || ns.IndexOf(".Models", StringComparison.OrdinalIgnoreCase) >= 0
                || (ns.IndexOf(".DAL", StringComparison.OrdinalIgnoreCase) >= 0 && n.EndsWith("Model", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (t.FullName != null && t.FullName.Contains("Designer", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        // Explicit service registrations (wrapped once with proper logging)
        private static void RegisterExplicitServices(
            IServiceCollection services,
            ILogger? logger)
        {
            try
            {
                // HttpClient for EMAILOUT
                services.AddHttpClient<EMAILOUT>();

                // Singleton startup readiness
                services.AddSingleton<StartupReadiness>();

                // Data layer explicit mappings
                services.AddScoped<ISqlDataHelperDao, SqlDataHelperDao>();
                services.AddScoped<ISqlDataHelperFactory, SqlDataHelperFactory>();

                // These Func<T> factory registrations allow certain components to create new scoped
                // instances on-demand *inside the same DI scope* without requiring their own constructors
                // to take a direct dependency on the concrete type.

                //
                //  1. **Lazy resolution** – instance is created only when needed, avoiding heavy
                //     constructor cost or early initialization.
                //
                //  2. **Multiple instances per scope** – calling the Func<T> multiple times returns
                //     separate scoped objects if required by the consuming logic.
                //
                //  3. **Avoiding circular dependencies** – Func<T> breaks dependency cycles that
                //     would otherwise occur if the service were injected directly.
                //
                //  4. **Cleaner orchestration patterns** – workflows or processors that need to
                //     instantiate UploadMasterCommon / cls_SCMBL / XFSUMessageProcessor / MailKitManager
                //     dynamically (e.g., based on runtime conditions) use the delegate instead of
                //     manual service locator patterns.
                services.AddScoped<Func<UploadMasterCommon>>(sp => () => sp.GetRequiredService<UploadMasterCommon>());
                services.AddScoped<Func<cls_SCMBL>>(sp => () => sp.GetRequiredService<cls_SCMBL>());
                services.AddScoped<Func<XFSUMessageProcessor>>(sp => () => sp.GetRequiredService<XFSUMessageProcessor>());
                services.AddScoped<Func<MailKitManager>>(sp => () => sp.GetRequiredService<MailKitManager>());


                logger?.LogInformation("Explicit DI registrations completed successfully.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during explicit DI registrations.");
                throw;
            }
        }


        /// <summary>
        /// Your manual registrations remain here. Keep adding explicit interface->implementation mappings as needed.
        /// </summary>
        public static class NativeInjectorBootStrapper
        {
            // Keep same signature as before
            public static void RegisterServices(FunctionsApplicationBuilder builder, ILogger? logger)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                // Ensure app config loaded (AddApiConfiguration must be called BEFORE AddDependencyInjectionConfiguration)
                var appConfig = builder.Configuration.Get<AppConfig>()
                              ?? throw new InvalidOperationException("AppConfig not loaded.");

                // Read the entity framework connection string from strongly typed AppConfig
                var efConnectionString = appConfig.Database?.EfConnectionString;
                if (string.IsNullOrWhiteSpace(efConnectionString))
                {
                    throw new InvalidOperationException("Missing entity framework connection string in AppConfig: ConnectionStrings:EntityFramework");
                }

                // Register a transient factory that creates a new SISDBEntities each time the Func is invoked
                builder.Services.AddSingleton<Func<SISDBEntities>>(
                    sp => () => new SISDBEntities(efConnectionString)
                );

                RegisterExplicitServices(builder.Services, logger);
            }
        }

        /// <summary>
        /// Validator that inspects constructor parameter types and compares them with registered service types.
        /// DOES NOT instantiate services (safe for production/dev checks).
        /// </summary>
        public static class DiConstructorDependencyValidator
        {
            /// <summary>
            /// Validate constructor dependencies for auto-registered candidates.
            /// Only runs automatically in Development unless throwOnMissing=true.
            /// </summary>
            /// <param name="host">Built host</param>
            /// <param name="logger">optional logger</param>
            /// <param name="throwOnMissing">set true in CI to fail fast</param>
            public static void Validate(IHost host, ILogger? logger = null, bool throwOnMissing = false)
            {
                if (host == null) throw new ArgumentNullException(nameof(host));

                var env = host.Services.GetService<IHostEnvironment>();
                if (env != null && !env.IsDevelopment() && !throwOnMissing)
                {
                    logger?.LogInformation("Skipping DI constructor validation (not Development and not forced).");
                    return;
                }

                logger ??= host.Services.GetService<ILoggerFactory>()?.CreateLogger("DiConstructorDependencyValidator");

                // Try to get previously stored candidate list; fallback to scanning assembly conservatively
                var candidates = host.Services.GetService<IReadOnlyCollection<Type>>()
                               ?? GetCandidatesFromAssembly(Assembly.GetExecutingAssembly(),
                                    ["QidWorkerRole"]);

                // Obtain registered service types from IServiceCollection if registered; otherwise partial fallback
                var registeredFromCollection = GetRegisteredTypesFromServiceProvider(host.Services);

                var missing = new List<string>();

                foreach (var t in candidates)
                {
                    if (t == null) continue;
                    var ctor = t.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
                    if (ctor == null) continue;

                    foreach (var p in ctor.GetParameters())
                    {
                        var depType = p.ParameterType;
                        if (IsFrameworkSatisfied(depType))
                            continue;

                        // Exact-match or assignable relationship with a registered type counts as satisfied
                        var satisfied = registeredFromCollection.Contains(depType)
                                        || registeredFromCollection.Any(rt => depType.IsAssignableFrom(rt) || rt.IsAssignableFrom(depType));

                        if (!satisfied)
                        {
                            missing.Add($"{t.FullName} -> missing registration for constructor parameter '{p.Name}' : {depType.FullName}");
                        }
                    }
                }

                if (missing.Any())
                {
                    logger?.LogError("DI constructor validation found {Count} missing registrations.", missing.Count);
                    foreach (var m in missing)
                    {
                        logger?.LogError(m);
                    }

                    if (throwOnMissing)
                    {
                        throw new InvalidOperationException("DI constructor validation failed. See logs for missing registrations.");
                    }
                }
                else
                {
                    logger?.LogInformation("DI constructor validation passed. No missing registrations detected for examined candidates.");
                }
            }

            // Try to recover service types from provider (best-effort)
            private static HashSet<Type> GetRegisteredTypesFromServiceProvider(IServiceProvider provider)
            {
                var set = new HashSet<Type>();

                try
                {
                    // If IServiceCollection was registered into DI (some hosts do), prefer it
                    var sc = provider.GetService<IServiceCollection>();
                    if (sc != null)
                    {
                        foreach (var sd in sc)
                            if (sd.ServiceType is Type t) set.Add(t);
                    }
                }
                catch
                {
                    // ignore and fallback
                }

                // Fallback: include common framework registrations
                set.Add(typeof(IServiceProvider));
                set.Add(typeof(IHostEnvironment));
                set.Add(typeof(Microsoft.Extensions.Configuration.IConfiguration));
                set.Add(typeof(Microsoft.Extensions.Logging.ILoggerFactory));
                set.Add(typeof(Microsoft.Extensions.Logging.ILogger<>).GetGenericTypeDefinition());

                return set;
            }

            private static bool IsFrameworkSatisfied(Type t)
            {
                if (t == typeof(IServiceProvider)
                    || t == typeof(IHostEnvironment)
                    || t == typeof(Microsoft.Extensions.Configuration.IConfiguration))
                {
                    return true;
                }

                if (t == typeof(Microsoft.Extensions.Logging.ILogger))
                {
                    return true;
                }

                if (t.IsGenericType)
                {
                    var gd = t.GetGenericTypeDefinition();
                    if (gd == typeof(Microsoft.Extensions.Logging.ILogger<>)
                        || gd == typeof(Microsoft.Extensions.Options.IOptions<>))
                        return true;
                }

                return false;
            }

            private static IReadOnlyCollection<Type> GetCandidatesFromAssembly(Assembly asm, string[] namespaceFilters)
            {
                namespaceFilters = (namespaceFilters ?? Array.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                var list = new List<Type>();
                Type[] all;
                try { all = asm.GetTypes(); } catch (ReflectionTypeLoadException ex) { all = ex.Types.Where(tt => tt != null).ToArray()!; }

                foreach (var t in all)
                {
                    if (t == null) continue;
                    if (!t.IsClass || t.IsAbstract || t.Namespace == null) continue;
                    if (!namespaceFilters.Any(f => t.Namespace.Contains(f, StringComparison.OrdinalIgnoreCase))) continue;
                    if (t.Name.EndsWith("Dto", StringComparison.OrdinalIgnoreCase)
                        || t.Name.EndsWith("Model", StringComparison.OrdinalIgnoreCase)
                        || t.Name.EndsWith("Record", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (t.GetConstructors().Any(c => c.GetParameters().Length > 0)
                        || t.Name.EndsWith("Processor", StringComparison.OrdinalIgnoreCase)
                        || t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase)
                        || t.Name.EndsWith("Manager", StringComparison.OrdinalIgnoreCase)
                        || t.Name.EndsWith("Upload", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(t);
                    }
                }

                return list.Distinct().ToList().AsReadOnly();
            }
        }
    }
}
