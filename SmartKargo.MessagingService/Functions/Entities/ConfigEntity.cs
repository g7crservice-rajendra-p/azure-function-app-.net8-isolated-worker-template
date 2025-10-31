using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Data.DbConstants;
using System.Data;

namespace SmartKargo.MessagingService.Functions.Entities
{
    /// <summary>
    /// Durable entity that manages application configuration stored in the database.
    /// Loads configuration into memory, caches it, and supports refresh operations.
    /// </summary>
    public sealed class ConfigEntity : TaskEntity<ConfigState>
    {
        #region Private Fields

        // Logger for structured diagnostics
        private readonly ILogger<ConfigEntity> _logger;

        // Lazy initialization of DAO to avoid premature DB connection
        private readonly Lazy<ISqlDataHelperDao> _lazyDao;

        // Minimum interval between config refresh operations
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigEntity"/> class.
        /// </summary>
        /// <param name="daoFactory">Factory for creating SQL data helper instances.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public ConfigEntity(ISqlDataHelperFactory daoFactory, ILogger<ConfigEntity> logger)
        {
            ArgumentNullException.ThrowIfNull(daoFactory);
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;

            // Lazy initialization ensures DAO is only created when first accessed
            _lazyDao = new Lazy<ISqlDataHelperDao>(() =>
            {
                // TODO: Add logging
                _logger.LogInformation("Initializing SQL Data Helper DAO (read-only).");
                return daoFactory.Create(readOnly: true);
            });

            // TODO: Add logging
            _logger.LogInformation("ConfigEntity initialized successfully with lazy DAO.");
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Accessor for the lazily-initialized DAO instance.
        /// </summary>
        private ISqlDataHelperDao Dao => _lazyDao.Value;

        #endregion

        #region Public Types

        /// <summary>
        /// Optional parameters for refresh operations.
        /// </summary>
        public class RefreshOptions
        {
            public bool ForceRefresh { get; set; } = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes configuration data from the database.
        /// Uses caching to avoid unnecessary DB calls.
        /// </summary>
        /// <param name="options">Optional refresh parameters.</param>
        public async Task RefreshAsync(RefreshOptions? options = null)
        {
            options ??= new RefreshOptions();

            // Check if cached data is still valid
            if (!options.ForceRefresh &&
                DateTime.UtcNow - State.LastRefreshTime < RefreshInterval &&
                State.Config.Count > 0)
            {
                // TODO: Add logging
                _logger.LogInformation("ConfigEntity: Cache valid, skipping DB refresh.");
                return;
            }

            try
            {
                // TODO: Add logging
                _logger.LogInformation(
                    "ConfigEntity: Refreshing configuration from DB. ForceRefresh={ForceRefresh}",
                    options.ForceRefresh
                );

                // Execute stored procedure using DAO
                var dataSet = await Dao.SelectRecords(StoredProcedures.GetMessageConfiguration);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    // TODO: Add logging
                    _logger.LogWarning("ConfigEntity: No configuration data retrieved from DB.");
                    return;
                }

                // Convert to dictionary
                var newConfig = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var key = row["Parameter"]?.ToString()?.Trim();
                    var value = row["Value"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        newConfig[key] = value ?? string.Empty;
                    }
                }

                // Update state
                State.Config = newConfig;
                State.LastRefreshTime = DateTime.UtcNow;

                // TODO: Add logging
                _logger.LogInformation("ConfigEntity: Refreshed {Count} configuration entries.", State.Config.Count);
            }
            catch (Exception ex)
            {
                // TODO: Add logging
                _logger.LogError(ex, "ConfigEntity: Error occurred while refreshing configuration.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single configuration value by key.
        /// </summary>
        public string? Get(string key)
        {
            if (State.Config.TryGetValue(key, out var value))
            {
                return value;
            }

            // TODO: Add logging
            _logger.LogWarning("ConfigEntity: Key '{Key}' not found in configuration.", key);
            return null;
        }
        

        /// <summary>
        /// Retrieves all configuration key-value pairs (copy).
        /// </summary>
        public IDictionary<string, string> GetAll() =>
            new Dictionary<string, string>(State.Config);

        #endregion

        #region Durable Entity Entry Point

        /// <summary>
        /// Entry point for Durable Entity dispatcher.
        /// </summary>
        [Function(nameof(ConfigEntity))]
        public static Task RunEntityAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        {
            // Note: Logging can't occur here since this is static context
            return dispatcher.DispatchAsync<ConfigEntity>();
        }

        #endregion
    }
}
