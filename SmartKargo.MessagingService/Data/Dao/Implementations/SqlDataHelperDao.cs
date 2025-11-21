using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace SmartKargo.MessagingService.Data.Dao.Implementations
{
    /// <summary>
    /// Provides high-performance SQL Server database access with support for:
    /// - Read-only and read-write connection strings
    /// - Archival fallback queries
    /// - Built-in retry logic
    /// - Async and disposable usage patterns
    /// 
    /// This class is stateless and thread-safe. Connection pooling is handled by ADO.NET.
    /// </summary>
    public sealed class SqlDataHelperDao : ISqlDataHelperDao, IDisposable, IAsyncDisposable
    {
        #region Private Fields

        private readonly string _connectionString;

        //private readonly string _archivalConnectionString;

        // Logger for structured diagnostics
        private readonly ILogger<SqlDataHelperDao> _logger;
        private bool _disposed;

        // Reuse retry provider instance to avoid reallocation overhead
        private static readonly SqlRetryLogicBaseProvider RetryProvider =
            SqlConfigurableRetryFactory.CreateExponentialRetryProvider(
                new SqlRetryLogicOption
                {
                    NumberOfTries = 3,
                    DeltaTime = TimeSpan.FromSeconds(2),
                    MaxTimeInterval = TimeSpan.FromSeconds(10)
                });

        private const int DefaultCommandTimeout = 30;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataHelperDao"/> class.
        /// </summary>
        /// <param name="appConfig">Configuration object that holds the application level variables</param>
        /// <param name="readOnly">If true, initializes with read-only connection string; otherwise, read-write.</param>
        public SqlDataHelperDao(AppConfig appConfig, ILogger<SqlDataHelperDao> logger, bool readOnly = false)
        {
            ArgumentNullException.ThrowIfNull(appConfig);
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;

            _connectionString = readOnly
                ? ValidateConnectionString(appConfig.Database.ReadOnlyConnectionString)
                : ValidateConnectionString(appConfig.Database.ReadWriteConnectionString);

            //_archivalConnectionString = ValidateConnectionString(appConfig.Database.ArchivalConnectionString);
        }

        #endregion

        #region Core Method - ValidateConnectionString

        /// <summary>
        /// Validates and normalizes a SQL Server connection string.
        /// Throws <see cref="ArgumentException"/> if the connection string is invalid.
        /// </summary>
        private string ValidateConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("Connection string validation failed: input is null or empty.");
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                _logger.LogDebug("Connection string validated and normalized successfully for data source: {DataSource}.", builder.DataSource);
                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid connection string format detected during validation.");
                throw new ArgumentException("Invalid connection string format.", nameof(connectionString), ex);
            }
        }

        #endregion

        #region Connection Factory

        /// <summary>
        /// Creates and opens a SQL connection asynchronously with retry logic.
        /// </summary>
        private async Task<SqlConnection> CreateOpenConnectionAsync(
            string connectionString,
            CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(connectionString)
            {
                RetryLogicProvider = RetryProvider
            };

            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("SQL connection opened successfully to {DataSource}.", connection.DataSource);
                return connection;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL connection open failed for data source: {DataSource}.", connection.DataSource);
                throw new InvalidOperationException("Failed to open SQL connection.", sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while opening SQL connection for data source: {DataSource}.", connection.DataSource);
                throw new InvalidOperationException("Unexpected error while opening SQL connection.", ex);
            }
        }

        #endregion

        #region Command Creation

        /// <summary>
        /// Creates and configures a <see cref="SqlCommand"/> for execution.
        /// </summary>
        private SqlCommand CreateCommand(
            SqlConnection connection,
            string sql,
            CommandType commandType,
            SqlParameter[]? parameters = null,
            int commandTimeout = DefaultCommandTimeout)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                _logger.LogWarning("Attempted to create SQL command with empty or null command text.");
                throw new ArgumentException("SQL command text cannot be null or empty.", nameof(sql));
            }

            var command = new SqlCommand(sql, connection)
            {
                CommandType = commandType,
                CommandTimeout = commandTimeout
            };

            if (parameters is { Length: > 0 })
            {
                command.Parameters.AddRange(parameters);
                _logger.LogDebug("Added {ParameterCount} parameters to SQL command: {CommandText}.", parameters.Length, sql);
            }
            else
            {
                _logger.LogTrace("Creating SQL command without parameters: {CommandText}.", sql);
            }

            _logger.LogInformation("SQL command created successfully. CommandType: {CommandType}, Timeout: {Timeout}s.",
                commandType, commandTimeout);

            return command;
        }

        #endregion


        #region Core Method - SelectRecords

        /// <summary>
        /// Executes a stored procedure or sql queries and retrieves results as a DataSet.
        /// Automatically falls back to the archival database if no records are returned.
        /// </summary>
        /// <param name="storedProcedureName">Stored procedure name.</param>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <param name="commandType">Command type, defaults to StoredProcedure.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="DataSet"/> containing the result set, or null in case of error.</returns>
        public async Task<DataSet?> SelectRecords(
            string storedProcedureName,
            SqlParameter[]? parameters = null,
            int commandTimeout = DefaultCommandTimeout,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storedProcedureName))
            {
                _logger.LogWarning("SelectRecords validation failed: SQL or stored procedure name is null or empty.");
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(storedProcedureName));
            }

            DataSet dataSet = new();

            try
            {
                // -----------------------------------
                // Primary Database Query Execution
                // -----------------------------------
                _logger.LogDebug("Executing primary database query: {SqlName} (CommandType: {CommandType})", storedProcedureName, commandType);

                using (var connection = await CreateOpenConnectionAsync(_connectionString, cancellationToken))
                using (var command = CreateCommand(connection, storedProcedureName, commandType, parameters, commandTimeout))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    dataSet.Tables.Add(dataTable);
                }

                _logger.LogInformation(
                    "Primary DB query executed successfully for {SqlName}. Records retrieved: {RecordCount}.",
                    storedProcedureName,
                    dataSet.Tables.Count > 0 ? dataSet.Tables[0].Rows.Count : 0);

                // -----------------------------------
                // Fallback to Archival if Empty
                // -----------------------------------

                /*
                 *
                bool hasRows = dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0;
                if (!hasRows)
                {
                    _logger.LogWarning("No records found in primary DB for {SqlName}. Switching to archival database.", storedProcedureName);

                    using (var archivalConnection = await CreateOpenConnectionAsync(_archivalConnectionString, cancellationToken))
                    using (var archivalCommand = CreateCommand(archivalConnection, storedProcedureName, commandType, parameters))
                    using (var archivalReader = await archivalCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var archivalTable = new DataTable();
                        archivalTable.Load(archivalReader);
                        dataSet.Tables.Add(archivalTable);
                    }

                    _logger.LogInformation(
                        "Archival DB query executed successfully for {SqlName}. Records retrieved: {RecordCount}.",
                        storedProcedureName,
                        dataSet.Tables.Count > 0 ? dataSet.Tables[^1].Rows.Count : 0);
                }
                */

                return dataSet;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "SQL exception occurred while executing {SqlName}. ErrorNumber: {ErrorNumber}, Procedure: {Procedure}, Message: {Message}",
                    storedProcedureName,
                    sqlEx.Number,
                    sqlEx.Procedure,
                    sqlEx.Message);
                return null;
            }
            catch (TimeoutException tex)
            {
                _logger.LogError(tex,
                    "SQL command timeout occurred for {SqlName}. Message: {Message}",
                    storedProcedureName,
                    tex.Message);
                return null;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SelectRecords operation was canceled for {SqlName}.", storedProcedureName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while executing {SqlName}. Message: {Message}",
                    storedProcedureName,
                    ex.Message);
                return null;
            }
        }


        #endregion

        #region GetStringByProcedure

        /// <summary>
        /// Executes a stored procedure and retrieves the first column of the first row as a string.
        /// Uses the provided SQL parameters for the procedure call.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameters">Optional SQL parameters for the stored procedure.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>
        /// The string value from the first column of the first row,
        /// or <c>null</c> if an error occurs, or an empty string if no data is found.
        /// </returns>
        public async Task<string?> GetStringByProcedureAsync(
            string procedureName,
            SqlParameter[]? parameters = null,
            int commandTimeout = DefaultCommandTimeout,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                _logger.LogWarning("Invalid stored procedure name provided to {MethodName}.", nameof(GetStringByProcedureAsync));
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(procedureName));
            }

            try
            {
                // -------------------------------------------
                // Execute stored procedure using SelectRecords
                // -------------------------------------------
                var ds = await SelectRecords(
                    procedureName,
                    parameters,
                    commandTimeout,
                    commandType,
                    cancellationToken).ConfigureAwait(false);

                if (ds is not null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    var result = ds.Tables[0].Rows[0][0]?.ToString() ?? string.Empty;

                    _logger.LogInformation(
                        "Stored procedure {ProcedureName} executed successfully. Result: {Result}",
                        procedureName, result);

                    return result;
                }

                _logger.LogInformation(
                    "Stored procedure {ProcedureName} executed successfully but returned no records.",
                    procedureName);

                return string.Empty;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "SQL exception occurred while executing stored procedure {ProcedureName}. ErrorNumber: {ErrorNumber}",
                    procedureName, sqlEx.Number);
            }
            catch (TimeoutException tex)
            {
                _logger.LogError(tex,
                    "Timeout occurred while executing stored procedure {ProcedureName}.",
                    procedureName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Operation canceled while executing stored procedure {ProcedureName}.",
                    procedureName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while executing stored procedure {ProcedureName}.",
                    procedureName);
            }
            return null;
        }

        #endregion

        #region GetIntegerByProcedure

        /// <summary>
        /// Executes a stored procedure and retrieves the first column of the first row as an integer.
        /// Uses strongly typed <see cref="SqlParameter"/>s for safety.
        /// </summary>
        /// <param name="procedureName">The stored procedure name to execute.</param>
        /// <param name="parameters">The SQL parameters for the stored procedure.</param>
        /// <returns>Single integer value returned by the query, or 0 if not found or error.</returns>
        public async Task<int> GetIntegerByProcedureAsync(
            string procedureName,
            SqlParameter[]? parameters = null,
            int commandTimeout = DefaultCommandTimeout,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default)
        {
            int result = 0;

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                _logger.LogWarning("Invalid stored procedure name passed to {Method}.", nameof(GetIntegerByProcedureAsync));
                throw new ArgumentException("Stored procedure name cannot be null or empty.", nameof(procedureName));
            }

            try
            {
                var ds = await SelectRecords(
                    procedureName,
                    parameters,
                    commandTimeout,
                    commandType,
                    cancellationToken).ConfigureAwait(false);

                if (ds is not null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0 &&
                    int.TryParse(ds.Tables[0].Rows[0][0]?.ToString(), out int parsed))
                {
                    result = parsed;
                    _logger.LogInformation("Stored procedure {ProcedureName} executed successfully. Result: {Result}", procedureName, result);
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "SQL exception while executing stored procedure {ProcedureName}. ErrorNumber: {ErrorNumber}",
                    procedureName, sqlEx.Number);
            }
            catch (TimeoutException tex)
            {
                _logger.LogError(tex,
                    "Timeout occurred while executing stored procedure {ProcedureName}.", procedureName);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Operation canceled while executing stored procedure {ProcedureName}.", procedureName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while executing stored procedure {ProcedureName}.", procedureName);
            }

            return result;
        }

        #endregion

        #region GetDataset

        /// <summary>
        /// Executes a SQL SELECT query and returns the results as a <see cref="DataSet"/>.
        /// Automatically applies retry logic, structured logging, and safety checks.
        /// </summary>
        /// <param name="query">The SELECT query to execute.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        /// A <see cref="DataSet"/> containing one DataTable with the query results,
        /// or <c>null</c> if the query is invalid or an error occurs.
        /// </returns>
        public async Task<DataSet?> GetDatasetAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            // -----------------------------------
            // Validation
            // -----------------------------------
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty or null SQL query passed to {MethodName}.", nameof(GetDatasetAsync));
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }

            // Prevent misuse (UPDATE/INSERT/DELETE should not be run in this method)
            var upperQuery = query.Trim().ToUpperInvariant();
            if (upperQuery.Contains("UPDATE ") || upperQuery.Contains("INSERT ") || upperQuery.Contains("DELETE "))
            {
                _logger.LogWarning("Unsafe query detected in {MethodName}. Only SELECT queries are allowed.", nameof(GetDatasetAsync));
                return null;
            }

            DataSet dataSet = new();

            try
            {
                // -----------------------------------
                // Open connection with retry provider
                // -----------------------------------

                using (var connection = await CreateOpenConnectionAsync(_connectionString, cancellationToken))
                using (var command = CreateCommand(connection, query, CommandType.Text, null, DefaultCommandTimeout))

                // -----------------------------------
                // Execute and load data
                // -----------------------------------
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    dataSet.Tables.Add(dataTable);
                }

                _logger.LogInformation("Query executed successfully via {MethodName}.", nameof(GetDatasetAsync));

                return dataSet;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "SQL exception while executing query in {MethodName}. ErrorNumber: {ErrorNumber}",
                    nameof(GetDatasetAsync),
                    sqlEx.Number);
            }
            catch (TimeoutException tex)
            {
                _logger.LogError(tex,
                    "Timeout occurred while executing query in {MethodName}.", nameof(GetDatasetAsync));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation canceled while executing query in {MethodName}.", nameof(GetDatasetAsync));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while executing query in {MethodName}.", nameof(GetDatasetAsync));
            }

            return null;
        }

        #endregion

        #region Core Method - ExecuteNonQuery

        /// <summary>
        /// Executes a stored procedure or SQL command that performs a non-query operation (INSERT, UPDATE, DELETE).
        /// Returns a success flag and includes structured diagnostic logging for visibility and troubleshooting.
        /// </summary>
        /// <param name="sql">Stored procedure name or SQL command text.</param>
        /// <param name="parameters">Optional SQL parameters.</param>
        /// <param name="commandType">Command type, defaults to StoredProcedure.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> if execution succeeds; otherwise, <c>false</c>.</returns>
        public async Task<bool> ExecuteNonQueryAsync(
            string procedureName,
            SqlParameter[]? parameters = null,
            int commandTimeout = DefaultCommandTimeout,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                _logger.LogWarning("Invalid procedure name passed to {MethodName}.", nameof(ExecuteNonQueryAsync));
                throw new ArgumentException("Procedure name cannot be null or empty.", nameof(procedureName));
            }

            //// Detect special case
            //bool isSpecialSp = string.Equals(procedureName, "spSaveFFRAWBRoute", StringComparison.OrdinalIgnoreCase);

            //// Build parameter log string (for debugging visibility)
            //if (isSpecialSp && parameters is { Length: > 0 })
            //{
            //    var paramDump = string.Join(", ", parameters.Select(p =>
            //        $"@{p.ParameterName}={p.Value ?? "NULL"} ({p.SqlDbType})"));
            //    _logger.LogDebug("Executing {ProcedureName} with parameters: {Parameters}", procedureName, paramDump);
            //}

            try
            {
                using var connection = await CreateOpenConnectionAsync(_connectionString, cancellationToken).ConfigureAwait(false);
                using var command = CreateCommand(connection, procedureName, commandType, parameters,commandTimeout);

                var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                //if (isSpecialSp)
                //{
                //    _logger.LogInformation("{ProcedureName} executed successfully.", procedureName);
                //}

                if (affectedRows > 0)
                {
                    _logger.LogInformation(
                        "Stored procedure {ProcedureName} executed successfully. Rows affected: {RowsAffected}.",
                        procedureName, affectedRows);
                }
                else
                {
                    _logger.LogWarning(
                        "Stored procedure {ProcedureName} executed but affected 0 rows.",
                        procedureName);
                }

                return true;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "SQL exception occurred while executing stored procedure {ProcedureName}. ErrorNumber: {ErrorNumber}, Message: {Message}",
                    procedureName, sqlEx.Number, sqlEx.Message);
            }
            catch (TimeoutException tex)
            {
                _logger.LogError(tex,
                    "Timeout occurred while executing stored procedure {ProcedureName}. Message: {Message}",
                    procedureName, tex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Execution canceled for stored procedure {ProcedureName}.", procedureName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error occurred while executing stored procedure {ProcedureName}. Message: {Message}",
                    procedureName, ex.Message);
            }

            return false;
        }

        #endregion

        #region Disposal Pattern

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            // TODO: Add debug-level logging for DAO disposal
        }

        private ValueTask DisposeAsyncCore()
        {
            _disposed = true;
            // TODO: Add debug-level logging for async DAO disposal
            return ValueTask.CompletedTask;
        }

        ~SqlDataHelperDao() => Dispose(disposing: false);

        #endregion
    }
}
