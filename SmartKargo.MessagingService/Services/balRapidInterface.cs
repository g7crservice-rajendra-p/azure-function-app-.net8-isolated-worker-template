using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole
{
    class balRapidInterface
    {
        //static public Dictionary<string, string> objDictionary = null;
        //static public Dictionary<string, string> objUploadDictionary = null;
        //static string BlobKey = String.Empty;
        //static string BlobName = String.Empty;

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<balRapidInterface> _logger;

        public balRapidInterface(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<balRapidInterface> logger
         )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }

        public async Task<DataSet?> InsertRapidInterfaceData(DateTime updatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate, string CTMFileName, string FlownFileName)
        {

            //objDictionary = new Dictionary<string, string>();
            //objUploadDictionary = new Dictionary<string, string>();

            DataSet? ds = new DataSet();
            try
            {
                //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();

                //string XML = string.Empty;

                //clsLog.WriteLog("----------------------------------------------------------------------------------------------------------------------");
                //clsLog.WriteLog("Schedular run on ::" + DateTime.Now);
                //objDictionary.Add("File Names ", "Status");

                #region "SAP XML for Collecions"
                //string Command = "Exec BI.usp_getSAPCollectionDetails '" + FromDate.ToString("MM/dd/yyyy") + "', '" + ToDate.ToString("MM/dd/yyyy") + "', '" + ExecutedOn.AddDays(0).ToString("MM/dd/yyyy") + "'";

                //SQLServer da = new SQLServer();

                //string[] pName = new string[6];
                //pName[0] = "UserName";
                //pName[1] = "UpdatedOn";
                //pName[2] = "FromDate";
                //pName[3] = "Todate";
                //pName[4] = "CTMBatchIDForTxtFile";
                //pName[5] = "FlownBatchIDForTxtFile";

                //object[] pValue = new object[6];
                //pValue[0] = strUserName;
                //pValue[1] = updatedOn;
                //pValue[2] = dtFromDate;
                //pValue[3] = dtToDate;
                //pValue[4] = CTMFileName;
                //pValue[5] = FlownFileName;

                //SqlDbType[] pType = new SqlDbType[6];
                //pType[0] = SqlDbType.VarChar;
                //pType[1] = SqlDbType.DateTime;
                //pType[2] = SqlDbType.DateTime;
                //pType[3] = SqlDbType.DateTime;
                //pType[4] = SqlDbType.VarChar;
                //pType[5] = SqlDbType.VarChar;

                //ds = SelectRecords("SP_RapidInterfaceInsert", pName, pValue, pType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = strUserName },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = updatedOn },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = dtFromDate },
                    new SqlParameter("@Todate", SqlDbType.DateTime) { Value = dtToDate },
                    new SqlParameter("@CTMBatchIDForTxtFile", SqlDbType.VarChar) { Value = CTMFileName },
                    new SqlParameter("@FlownBatchIDForTxtFile", SqlDbType.VarChar) { Value = FlownFileName }
                };

                // async DAO call
                ds = await _readWriteDao.SelectRecords("SP_RapidInterfaceInsert", parameters);

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        return ds;
                    }
                }

                return ds;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in InsertRapidInterfaceData");
                return ds;
            }
        }
        public async Task<DataSet?> GetRapidInterfaceData(string strFileName)
        {
            DataSet? ds = new();
            try
            {
                //SQLServer da = new SQLServer();

                //string[] QueryPname = new string[1];
                //object[] QueryValue = new object[1];
                //SqlDbType[] QueryType = new SqlDbType[1];

                //QueryPname[0] = "BatchIDForTxtFile";
                //QueryType[0] = SqlDbType.VarChar;
                //QueryValue[0] = strFileName;
                //ds = SelectRecords("SP_RapidInterfaceSelect", QueryPname, QueryValue, QueryType);


                SqlParameter[] parameters =
                {
                    new SqlParameter("@BatchIDForTxtFile", SqlDbType.VarChar) { Value = strFileName }
                };

                ds = await _readWriteDao.SelectRecords("SP_RapidInterfaceSelect", parameters);

                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in GetRapidInterfaceData");
                return ds;
            }
        }


        public async Task<DataSet?> InsertRapidFlownTransaction(DateTime UpdatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet? ds = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();

                //string[] PName = new string[4];
                //PName[0] = "UserName";
                //PName[1] = "UpdatedOn";
                //PName[2] = "FromDate";
                //PName[3] = "Todate";

                //object[] PValue = new object[4];
                //PValue[0] = strUserName;
                //PValue[1] = UpdatedOn;
                //PValue[2] = dtFromDate;
                //PValue[3] = dtToDate;

                //SqlDbType[] PType = new SqlDbType[4];
                //PType[0] = SqlDbType.VarChar;
                //PType[1] = SqlDbType.DateTime;
                //PType[2] = SqlDbType.DateTime;
                //PType[3] = SqlDbType.DateTime;
                //ds = da.SelectRecords("SP_RapidFlownInsert", PName, PValue, PType);


                SqlParameter[] parameters =
                {
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = strUserName },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = UpdatedOn },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = dtFromDate },
                    new SqlParameter("@Todate", SqlDbType.DateTime) { Value = dtToDate }
                };
                ds = await _readWriteDao.SelectRecords("SP_RapidFlownInsert", parameters);

                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "InsertRapidFlownTransaction");
                return ds;
            }
        }

        public async Task<DataSet?> GetRapidFlownTransaction(string strFileName)
        {
            DataSet? ds = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] QueryPname = new string[1];
                //object[] QueryValue = new object[1];
                //SqlDbType[] QueryType = new SqlDbType[1];

                //QueryPname[0] = "BatchIDForTxtFile";
                //QueryType[0] = SqlDbType.VarChar;
                //QueryValue[0] = strFileName;
                //ds = da.SelectRecords("SP_RapidFlownSelect", QueryPname, QueryValue, QueryType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@BatchIDForTxtFile", SqlDbType.VarChar) { Value = strFileName }
                };

                ds = await _readWriteDao.SelectRecords("SP_RapidFlownSelect", parameters);

                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in GetRapidFlownTransaction");
                return ds;
            }
        }

        public async Task<DataSet?> InsertRapidCTMTransaction(DateTime UpdatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet? ds = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();

                //string[] PName = new string[4];
                //PName[0] = "UserName";
                //PName[1] = "UpdatedOn";
                //PName[2] = "FromDate";
                //PName[3] = "Todate";

                //object[] PValue = new object[4];
                //PValue[0] = strUserName;
                //PValue[1] = UpdatedOn;
                //PValue[2] = dtFromDate;
                //PValue[3] = dtToDate;

                //SqlDbType[] PType = new SqlDbType[4];
                //PType[0] = SqlDbType.VarChar;
                //PType[1] = SqlDbType.DateTime;
                //PType[2] = SqlDbType.DateTime;
                //PType[3] = SqlDbType.DateTime;
                //ds = da.SelectRecords("SP_RapidCTMInsert", PName, PValue, PType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = strUserName },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = UpdatedOn },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = dtFromDate },
                    new SqlParameter("@Todate", SqlDbType.DateTime) { Value = dtToDate }
                };
                ds = await _readWriteDao.SelectRecords("SP_RapidCTMInsert", parameters);

                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in InsertRapidCTMTransaction");
                return ds;
            }
        }

        public async Task<DataSet?> GetRapidCTMTransaction(string strFileName)
        {
            DataSet? ds = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] QueryPname = new string[1];
                //object[] QueryValue = new object[1];
                //SqlDbType[] QueryType = new SqlDbType[1];

                //QueryPname[0] = "BatchIDForTxtFile";
                //QueryType[0] = SqlDbType.VarChar;
                //QueryValue[0] = strFileName;
                //ds = da.SelectRecords("SP_RapidCTMSelect", QueryPname, QueryValue, QueryType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@BatchIDForTxtFile", SqlDbType.VarChar) { Value = strFileName }
                };

                ds = await _readWriteDao.SelectRecords("SP_RapidCTMSelect", parameters);


                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in GetRapidCTMTransaction");
                return ds;
            }
        }


        public async Task<DataSet?> InsertRapidCCAPXTransaction(DateTime UpdatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet? ds = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();

                //string[] PName = new string[4];
                //PName[0] = "UserName";
                //PName[1] = "UpdatedOn";
                //PName[2] = "FromDate";
                //PName[3] = "Todate";


                //object[] PValue = new object[4];
                //PValue[0] = strUserName;
                //PValue[1] = UpdatedOn;
                //PValue[2] = dtFromDate;
                //PValue[3] = dtToDate;

                //SqlDbType[] PType = new SqlDbType[4];
                //PType[0] = SqlDbType.VarChar;
                //PType[1] = SqlDbType.DateTime;
                //PType[2] = SqlDbType.DateTime;
                //PType[3] = SqlDbType.DateTime;

                //ds = da.SelectRecords("UspInsertRapidPXCCARecordsOman", PName, PValue, PType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = strUserName },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = UpdatedOn },
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = dtFromDate },
                    new SqlParameter("@Todate", SqlDbType.DateTime) { Value = dtToDate }
                };

                ds = await _readWriteDao.SelectRecords("UspInsertRapidPXCCARecordsOman", parameters);

                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in InsertRapidCCAPXTransaction");
                return ds;
            }
        }

        public async Task<DataSet?> GetRapidCCAPXTransaction(string strFileName)
        {
            DataSet? ds = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] QueryPname = new string[1];
                //object[] QueryValue = new object[1];
                //SqlDbType[] QueryType = new SqlDbType[1];

                //QueryPname[0] = "BatchIDForTxtFile";

                //QueryType[0] = SqlDbType.VarChar;

                //QueryValue[0] = strFileName;

                //ds = da.SelectRecords("uspGetRapidPXCCARecordsOman", QueryPname, QueryValue, QueryType);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@BatchIDForTxtFile", SqlDbType.VarChar) { Value = strFileName }
                };

                ds = await _readWriteDao.SelectRecords("uspGetRapidPXCCARecordsOman", parameters);


                if (ds != null)
                {
                    if (ds.Tables != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            return (ds);
                        }
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in GetRapidCCAPXTransaction");
                return ds;
            }
        }


        /*Duplicated function instead use SqlDataHelperDao's GetStringByProcedure*/
        //#region Get String By Procedure Multiple Param

        ///// <summary>
        ///// Gets String result from database based on StoredProcedure.
        ///// </summary>
        ///// <param name="procedureName"></param>
        ///// <param name="queryPName"></param>
        ///// <param name="queryPValues"></param>
        ///// <param name="queryPTypes"></param>
        ///// <returns>Single string data returned by query.</returns>
        //public string GetStringByProcedure(string procedureName, string[] queryPName, object[] queryPValues, SqlDbType[] queryPTypes)
        //{
        //    DataSet ds = null;
        //    try
        //    {
        //        string strGetString = "";
        //        ds = SelectRecords(procedureName, queryPName, queryPValues, queryPTypes);
        //        if (ds != null)
        //        {
        //            if (ds.Tables.Count > 0)
        //            {
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    strGetString = ds.Tables[0].Rows[0][0].ToString();
        //                }
        //            }
        //            ds.Dispose();
        //        }
        //        else
        //        {
        //            strGetString = "";
        //        }
        //        return (strGetString);
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return (null);
        //    }
        //    finally
        //    {
        //        if (ds != null)
        //            ds.Dispose();
        //    }

        //}
        //#endregion

        /*Duplicated function instead use SqlDataHelperDao's SelectRecords*/

        //public DataSet SelectRecords(string selectProcedure, string[] queryPName, object[] queryValues, SqlDbType[] queryTypes)
        //{
        //    DataSet dataSet = new DataSet();
        //    SqlCommand cmd = null;
        //    SqlDataAdapter adapter = null;
        //    StringBuilder sbParam = new StringBuilder();
        //    string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //    SqlConnection _con = new SqlConnection();
        //    try
        //    {
        //        _con.ConnectionString = connectionString;
        //        if (_con.State != ConnectionState.Open)
        //            _con.Open();
        //        adapter = new SqlDataAdapter();
        //        cmd = new SqlCommand
        //        {
        //            CommandType = CommandType.StoredProcedure,
        //            CommandText = selectProcedure,
        //            Connection = _con
        //        };
        //        adapter.SelectCommand = cmd;
        //        adapter.SelectCommand.CommandTimeout = 10000;

        //        //Add parameters to command.
        //        sbParam.Append("EXEC " + selectProcedure + " ");
        //        for (int i = 0; i < queryPName.Length; i++)
        //        {
        //            cmd.Parameters.Add("@" + queryPName[i], queryTypes[i]).Value = queryValues[i];
        //            switch (Convert.ToString(queryTypes[i]))
        //            {
        //                case "VarChar":
        //                case "NVarChar":
        //                case "DateTime":
        //                case "Date":
        //                    sbParam.Append("@" + queryPName[i] + "='" + queryValues[i] + "',");
        //                    break;
        //                case "Bit":
        //                    int isBool = Convert.ToBoolean(queryValues[i]) ? 1 : 0;
        //                    sbParam.Append("@" + queryPName[i] + "=" + isBool + ",");
        //                    break;
        //                default:
        //                    sbParam.Append("@" + queryPName[i] + "=" + queryValues[i] + ",");
        //                    break;
        //            }
        //        }

        //        adapter.Fill(dataSet);

        //        {
        //            bool blnFlag = false;

        //            for (int intCount = 0; intCount < dataSet.Tables.Count; intCount++)
        //            {
        //                if (dataSet.Tables[intCount].Rows.Count > 0)
        //                {
        //                    blnFlag = true;
        //                    break;
        //                }
        //            }

        //            if (blnFlag == false)
        //            {
        //                if (_con.State == ConnectionState.Open)
        //                    _con.Close();

        //                string strArchival = GetConnectionStringArchival();
        //                if (!string.IsNullOrEmpty(strArchival))
        //                {
        //                    _con.ConnectionString = strArchival;
        //                    cmd.Connection = _con;
        //                    adapter.Fill(dataSet);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        dataSet = null;
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    finally
        //    {
        //        if (adapter != null)
        //            adapter.Dispose();

        //        if (cmd != null)
        //            cmd.Dispose();

        //        if (_con.State == ConnectionState.Open)
        //            _con.Close();
        //    }
        //    return dataSet;
        //}
        //private string GetConnectionStringArchival()
        //{
        //    try
        //    {
        //        string strcon = Convert.ToString(ConfigurationManager.ConnectionStrings["ConStrArchival"]);
        //        return (strcon);
        //    }
        //    catch (Exception)
        //    {
        //        return ("");
        //    }
        //}

        /*Not in use*/
        //public DataSet getInterfaceType()
        //{
        //    try
        //    {
        //        SQLServer da = new SQLServer();
        //        return da.GetDataset("SELECT DISTINCT(InterfaceType) FROM Log.InterfacedFileDetails");
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return null;

        //    }
        //}
        #endregion

        /*Not in use*/
        //public DataSet GetInterfaceDetails(string strFileType, DateTime dtFromDate, DateTime dtToDate, string strMsgKey)
        //{
        //    DataSet ds = new DataSet();
        //    try
        //    {
        //        SQLServer da = new SQLServer();

        //        //Add Parameters

        //        da.AddParameters("@FromDate", SqlDbType.DateTime, ParameterDirection.Input, dtFromDate);
        //        da.AddParameters("@ToDate", SqlDbType.DateTime, ParameterDirection.Input, dtToDate);
        //        da.AddParameters("@InterfaceType", SqlDbType.VarChar, ParameterDirection.Input, strFileType);

        //        da.FillDataset("[dbo].[uspGetInterfaceDetais]", ds, null, null);
        //        return ds;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return ds;
        //    }
        //}

        /*Not in use*/

        //public void SetInterfaceDetails(string fileType, string fileName, string fileUrl, string createdBy, DateTime createdDate)
        //{

        //    try
        //    {
        //        SQLServer da = new SQLServer();

        //        string[] pName = new string[5];
        //        pName[0] = "FileType";
        //        pName[1] = "FileName";
        //        pName[2] = "FileURL";
        //        pName[3] = "CreatedBy";
        //        pName[4] = "CreatedDate";

        //        object[] pValue = new object[5];
        //        pValue[0] = fileType;
        //        pValue[1] = fileName;
        //        pValue[2] = fileUrl;
        //        pValue[3] = createdBy;
        //        pValue[4] = createdDate;

        //        SqlDbType[] pType = new SqlDbType[5];
        //        pType[0] = SqlDbType.VarChar;
        //        pType[1] = SqlDbType.VarChar;
        //        pType[2] = SqlDbType.VarChar;
        //        pType[3] = SqlDbType.VarChar;
        //        pType[4] = SqlDbType.DateTime;

        //        da.SelectRecords("uspSetInterfaceDetails", pName, pValue, pType);
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        /*Not in use*/

        //public static bool UploadBlob(Stream stream, string fileName)
        //{
        //    try
        //    {
        //        string containerName = "blobstorage";

        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());// "NUro8/C7+kMqtwOwLbe6agUvA83s+8xSTBqrkMwSjPP6MAxVkdtsLDGjyfyEqQIPv6JHEEf5F5s4a+DFPsSQfg==");
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, useHttps: true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
        //        blobContainer.CreateIfNotExist();
        //        CloudBlob blob = blobContainer.GetBlobReference(fileName);
        //        blob.Properties.ContentType = "";
        //        blob.UploadFromStream(stream);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return false;
        //    }
        //}

        /*Not in use*/

        //private static string getStorageKey()
        //{
        //    try
        //    {
        //        if (String.IsNullOrEmpty(BlobKey))
        //        {

        //            BlobKey = GetMasterConfiguration("BlobStorageKey");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    return BlobKey;
        //}

        /*Not in use*/

        //private static string getStorageName()
        //{
        //    try
        //    {
        //        if (String.IsNullOrEmpty(BlobName))
        //        {

        //            BlobName = GetMasterConfiguration("BlobStorageName");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    return BlobName;
        //}

        /*Not in use*/

        //public static string GetMasterConfiguration(string Parameter)
        //{
        //    string ParameterValue = string.Empty;

        //    balRapidInterface da = new balRapidInterface();
        //    string[] QName = new string[] { "PType" };
        //    object[] QValues = new object[] { Parameter };
        //    SqlDbType[] QType = new SqlDbType[] { SqlDbType.VarChar };
        //    ParameterValue = da.GetStringByProcedure("spGetSystemParameter", QName, QValues, QType);
        //    if (ParameterValue == null)
        //        ParameterValue = "";
        //    da = null;
        //    QName = null;
        //    QValues = null;
        //    QType = null;

        //    return ParameterValue;
        //}

    }
}

