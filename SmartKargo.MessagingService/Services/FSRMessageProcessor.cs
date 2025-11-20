
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole
{
    public class FSRMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FSRMessageProcessor> _logger;

        #region Constructor
        public FSRMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FSRMessageProcessor> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion

        public async Task<bool> DecodeFSR(string message, string messageFromAddress, string pimaAddress)
        {
            bool flag = false;
            string awbPrefix = string.Empty, awbNumber = string.Empty;
            try
            {
                string[] arrMsg = message.Split('$');
                if (arrMsg.Length > 1)
                {
                    if (arrMsg[1].Contains('-'))
                    {
                        awbPrefix = arrMsg[1].Split('-')[0];
                        awbNumber = arrMsg[1].Split('-')[1];
                        if (messageFromAddress.Trim().Length > 0)
                        {
                            await RelayFSA(awbPrefix, awbNumber, messageFromAddress, pimaAddress);
                        }
                    }
                }
                flag = true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        // void RelayFSA(string awbPrefix, string awbNumber, string messageFromAddress, string pimaAddress)
        private async Task RelayFSA(string awbPrefix, string awbNumber, string messageFromAddress, string pimaAddress)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //string[] paramName = new string[] { "AWBPrefix", "AWBNumber", "MessageFromAddress", "PIMAAddress" };
                //object[] paramValue = new object[] { awbPrefix, awbNumber, messageFromAddress, pimaAddress };
                //SqlDbType[] paramType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //DataSet ds = sqlServer.SelectRecords("Messaging.uspRelayFSAOnFSR", paramName, paramValue, paramType);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                  new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                  new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber },
                  new SqlParameter("@MessageFromAddress", SqlDbType.VarChar) { Value = messageFromAddress },
                  new SqlParameter("@PIMAAddress", SqlDbType.VarChar) { Value = pimaAddress }
                };

                DataSet? ds = await _readWriteDao.SelectRecords("Messaging.uspRelayFSAOnFSR", sqlParameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
    }
}
