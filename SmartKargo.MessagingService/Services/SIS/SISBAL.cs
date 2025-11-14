using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Collections.Specialized;
using System.Data;

namespace QidWorkerRole.SIS
{
    public class SISBAL
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<SISBAL> _logger;

        #region Constructor
        public SISBAL(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<SISBAL> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion
        public async Task<DataSet> InterlineMatchPayablesAWBs(string strFileHeaderId, string strUserName, string strMsgKey)
        {
            DataSet? ds = new DataSet();

            try
            {
                //SQLServer da = new SQLServer();

                //Add Parameters
                //da.AddParameters("@FileHeaderID", SqlDbType.Int, ParameterDirection.Input, strFileHeaderId);
                //da.AddParameters("@UserName", SqlDbType.VarChar, ParameterDirection.Input, strUserName);

                //da.AddParameters("@MessageKey", SqlDbType.VarChar, 200, ParameterDirection.Output, strMsgKey);

                //da.AddParameters("@ReturnValue", SqlDbType.Int, ParameterDirection.ReturnValue, null);

                SqlParameter[] parameters = new SqlParameter[] { 
                    new SqlParameter("@FileHeaderID", SqlDbType.Int) { Value = strFileHeaderId },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = strUserName },
                    new SqlParameter("@MessageKey", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output, Value = strMsgKey },
                    new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue, Value = DBNull.Value }

                };

                //da.FillDataset("[SIS].[uspInterlineMatchPayablesAWBs]", ds, null, null);
                await _readWriteDao.SelectRecords("[SIS].[uspInterlineMatchPayablesAWBs]", parameters);

                return ds;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }

        /*Not in use*/
        //public DataSet SaveInterlineBillingInterfaceDataI243(int fileHeaderID, int isSIS, string userName, DateTime createdOn)
        //{
        //    DataSet dataSet = new DataSet();
        //    try
        //    {
        //        //SQLServer sqlServer = new SQLServer();
        //        sqlServer.AddParameters("@FileHeaderID", SqlDbType.Int, ParameterDirection.Input, fileHeaderID);
        //        sqlServer.AddParameters("@IsSIS", SqlDbType.Int, ParameterDirection.Input, isSIS);
        //        sqlServer.AddParameters("@UserName", SqlDbType.VarChar, ParameterDirection.Input, userName);
        //        sqlServer.AddParameters("@CreatedOn", SqlDbType.DateTime, ParameterDirection.Input, createdOn);
                
        //        sqlServer.FillDataset("Interfaces.uspSaveInterlineBillingInterfaceDataI243", dataSet, null, null);
                
        //        return dataSet;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error in SaveInterlineBillingInterfaceDataI243 : " + ex.Message);
        //        return dataSet;
        //    }
        //}

        /// <summary>
        /// Method to add Interline Audit Log for File Generation
        /// </summary>
        /// <param name="strInvoiceHeaderID"></param>
        /// <param name="userName"></param>
        /// <param name="updatedOn"></param>
        /// <param name="strMsgKey"></param>
        /// <returns></returns>
        public async Task<DataSet?> CreateInterlineAuditLog(string action, string strInvoiceHeaderID, string userName, DateTime updatedOn, string strMsgKey)
        {
            DataSet resultDataSet = new DataSet();
            NameValueCollection returnValue;
            try
            {
                //SQLServer sqlServer = new SQLServer();

                //Add Parameters
                //sqlServer.AddParameters("@Action", SqlDbType.VarChar, ParameterDirection.Input, action);
                //sqlServer.AddParameters("@ListOfInvoiceIds", SqlDbType.VarChar, ParameterDirection.Input, strInvoiceHeaderID);
                //sqlServer.AddParameters("@UserName", SqlDbType.VarChar, ParameterDirection.Input, userName);
                //sqlServer.AddParameters("@UpdatedOn", SqlDbType.DateTime, ParameterDirection.Input, updatedOn);
                //sqlServer.AddParameters("@MessageKey", SqlDbType.VarChar, 200, ParameterDirection.Output, strMsgKey);

                //sqlServer.AddParameters("@ReturnValue", SqlDbType.Int, ParameterDirection.ReturnValue, null);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Action", SqlDbType.VarChar) { Value = action },
                    new SqlParameter("@ListOfInvoiceIds", SqlDbType.VarChar) { Value = strInvoiceHeaderID },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = userName },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = updatedOn },
                    new SqlParameter("@MessageKey", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output, Value = strMsgKey },
                    new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue, Value = DBNull.Value }
                };

                //returnValue = sqlServer.FillDataset("IBilling.CreateInterlineAuditLog", resultDataSet, null, null);
                resultDataSet = await _readWriteDao.SelectRecords("IBilling.CreateInterlineAuditLog", sqlParameters);
                //strMsgKey = returnValue["@MessageKey"] != null ? returnValue["@MessageKey"].ToString() : "";
                //string err = returnValue["@ReturnValue"].ToString();

                return resultDataSet;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure(exception);
                return resultDataSet;
            }
        }

    }
}
