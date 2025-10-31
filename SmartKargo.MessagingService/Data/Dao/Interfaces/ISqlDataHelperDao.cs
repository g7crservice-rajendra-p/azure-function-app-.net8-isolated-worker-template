using Microsoft.Data.SqlClient;
using System.Data;

namespace SmartKargo.MessagingService.Data.Dao.Interfaces
{
    public interface ISqlDataHelperDao
    {
        Task<DataSet?> SelectRecords(string sql,
            SqlParameter[]? parameters = null,
            int commandTimeout = 30,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default);

        Task<string?> GetStringByProcedureAsync(
           string procedureName,
           SqlParameter[]? parameters = null,
           int commandTimeout = 30,
           CommandType commandType = CommandType.StoredProcedure,
           CancellationToken cancellationToken = default);

        Task<int> GetIntegerByProcedureAsync(
            string procedureName,
            SqlParameter[]? parameters = null,
            int commandTimeout = 30,
            CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default);

        Task<DataSet?> GetDatasetAsync(string query, CancellationToken cancellationToken = default);

        Task<bool> ExecuteNonQueryAsync(string procedureName,
          SqlParameter[]? parameters = null,
          int commandTimeout = 30,
          CommandType commandType = CommandType.StoredProcedure,
          CancellationToken cancellationToken = default);
    }
}
