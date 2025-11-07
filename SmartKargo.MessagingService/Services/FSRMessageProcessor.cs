using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole
{
    public class FSRMessageProcessor
    {
        public FSRMessageProcessor()
        {

        }

        public bool DecodeFSR(string message, string messageFromAddress, string pimaAddress)
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
                            RelayFSA(awbPrefix, awbNumber, messageFromAddress, pimaAddress);
                        }
                    }
                }
                flag = true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return flag;
        }

        private void RelayFSA(string awbPrefix, string awbNumber, string messageFromAddress, string pimaAddress)
        {
            try
            {
                SQLServer sqlServer = new SQLServer();
                string[] paramName = new string[] { "AWBPrefix", "AWBNumber", "MessageFromAddress", "PIMAAddress" };
                object[] paramValue = new object[] { awbPrefix, awbNumber, messageFromAddress, pimaAddress };
                SqlDbType[] paramType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                DataSet ds = sqlServer.SelectRecords("Messaging.uspRelayFSAOnFSR", paramName, paramValue, paramType);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
    }
}
