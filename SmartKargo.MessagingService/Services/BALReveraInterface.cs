//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Logging;
//using SmartKargo.MessagingService.Data.Dao.Interfaces;
//using System.Configuration;
//using System.Data;
//namespace QidWorkerRole
//{
//    public class BALReveraInterface
//    {
//        #region Variables
//        public static string ConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
//        //SQLServer db = new SQLServer();

//        private readonly ISqlDataHelperDao _readWriteDao;
//        private readonly ILogger<EMAILOUT> _logger;

//        #endregion
//        public BALReveraInterface(ISqlDataHelperFactory sqlDataHelperFactory,
//            ILogger<EMAILOUT> logger)
//        {
//            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
//            _logger = logger;
//        }
//        #region GetAWBInfo
//        public async Task<DataSet> GetAWBInfo(DateTime FromDate, DateTime ToDate)
//        {
//            DataSet? dsGetAWBInfo = new DataSet("BALReveraInterface_dsGetAWBInfo");
//            try
//            {

//                //string[] Pname = new string[2];
//                //Pname[0] = "FromDate";
//                //Pname[1] = "ToDate";

//                //object[] Pvalue = new object[2];
//                //Pvalue[0] = FromDate;
//                //Pvalue[1] = ToDate;

//                //SqlDbType[] Ptype = new SqlDbType[2];
//                //Ptype[0] = SqlDbType.DateTime;
//                //Ptype[1] = SqlDbType.DateTime;

//                //dsGetAWBInfo = db.SelectRecords("SPReveraAWBInfomation", Pname, Pvalue, Ptype);

//                SqlParameter[] parameters =
//                {
//                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = FromDate },
//                    new SqlParameter("@ToDate", SqlDbType.DateTime)   { Value = ToDate }
//                };

//                dsGetAWBInfo = await _readWriteDao.SelectRecords("SPReveraAWBInfomation", parameters);

//                if (dsGetAWBInfo != null && dsGetAWBInfo.Tables.Count > 0 && dsGetAWBInfo.Tables[0].Rows.Count > 0)
//                {
//                    return dsGetAWBInfo;
//                }

//            }
//            catch (Exception ex)
//            {

//                if (dsGetAWBInfo != null)
//                    dsGetAWBInfo.Dispose();
//                //clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetAWBInfo");
//                _logger.LogError(ex, "Error on GetAWBInfo.");
//            }
//            return (null);
//        }
//        #endregion

//        #region FlownInfo
//        public async Task<DataSet> GetFlownInfo(DateTime FromDate, DateTime ToDate)
//        {
//            DataSet? dsFlownInfo = new DataSet("BALReveraInterface_dsFlownInfo");
//            try
//            {
//                //string[] Pname = new string[2];
//                //Pname[0] = "FromDate";
//                //Pname[1] = "ToDate";

//                //object[] Pvalue = new object[2];
//                //Pvalue[0] = FromDate;
//                //Pvalue[1] = ToDate;

//                //SqlDbType[] Ptype = new SqlDbType[2];
//                //Ptype[0] = SqlDbType.DateTime;
//                //Ptype[1] = SqlDbType.DateTime;
//                //dsFlownInfo = db.SelectRecords("SPReveraFlownInformation", Pname, Pvalue, Ptype);

//                SqlParameter[] parameters =
//                {
//                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = FromDate },
//                    new SqlParameter("@ToDate", SqlDbType.DateTime)   { Value = ToDate }
//                };

//                dsFlownInfo = await _readWriteDao.SelectRecords("SPReveraFlownInformation", parameters);

//                if (dsFlownInfo != null && dsFlownInfo.Tables.Count > 0 && dsFlownInfo.Tables[0].Rows.Count > 0)
//                {
//                    return dsFlownInfo;
//                }

//            }
//            catch (Exception ex)
//            {
//                if (dsFlownInfo != null)
//                    dsFlownInfo.Dispose();
//                //clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetFlownInfo");
//                _logger.LogError(ex, "Error on GetFlownInfo.");

//            }
//            return (null);
//        }
//        #endregion

//        #region FltManifest
//        public async Task<DataSet> GetFltManifestDetails(DateTime FromDate, DateTime ToDate)
//        {
//            DataSet? dsFltManifest = new DataSet("BALReveraInterface_dsFltManifest");

//            try
//            {
//                //string[] Pname = new string[2];
//                //Pname[0] = "FromDate";
//                //Pname[1] = "ToDate";

//                //object[] Pvalue = new object[2];
//                //Pvalue[0] = FromDate;
//                //Pvalue[1] = ToDate;

//                //SqlDbType[] Ptype = new SqlDbType[2];
//                //Ptype[0] = SqlDbType.DateTime;
//                //Ptype[1] = SqlDbType.DateTime;
//                //dsFltManifest = db.SelectRecords("SP_ReveraManifestInterface", Pname, Pvalue, Ptype);
                
//                SqlParameter[] parameters =
//                {
//                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = FromDate },
//                    new SqlParameter("@ToDate", SqlDbType.DateTime)   { Value = ToDate }
//                };

//                dsFltManifest = await _readWriteDao.SelectRecords("SP_ReveraManifestInterface", parameters);

//                if (dsFltManifest != null && dsFltManifest.Tables.Count > 0 && dsFltManifest.Tables[0].Rows.Count > 0)
//                {
//                    return dsFltManifest;
//                }

//            }
//            catch (Exception ex)
//            {
//                if (dsFltManifest != null)
//                    dsFltManifest.Dispose();
//                //clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetFltManifestDetails");
//                _logger.LogError(ex, "Error on GetFltManifestDetails.");

//            }
//            return (null);


//        }
//        #endregion

//        #region GetReveraFileName
//        public async Task<DataTable> GetReveraFileName(DateTime CurrentDate, string ReveraFileType, string TransactionID, int TotalRecord, float CheckSum)
//        {
//            DataSet? dsFileName = new DataSet("BALReveraInterface_dsFileName");
//            try
//            {
//                //string[] Pname = new string[5];
//                //Pname[0] = "CurrentDate";
//                //Pname[1] = "ReveraFileType";
//                //Pname[2] = "transID";
//                //Pname[3] = "totRec";
//                //Pname[4] = "CHKSUM";

//                //object[] Pvalue = new object[5];
//                //Pvalue[0] = CurrentDate;
//                //Pvalue[1] = ReveraFileType;
//                //Pvalue[2] = TransactionID;
//                //Pvalue[3] = TotalRecord;
//                //Pvalue[4] = CheckSum;

//                //SqlDbType[] Ptype = new SqlDbType[5];
//                //Ptype[0] = SqlDbType.DateTime;
//                //Ptype[1] = SqlDbType.VarChar;
//                //Ptype[2] = SqlDbType.VarChar;
//                //Ptype[3] = SqlDbType.Int;
//                //Ptype[4] = SqlDbType.Float;
//                //dsFileName = db.SelectRecords("SPGetOrSetReveraFileNumber", Pname, Pvalue, Ptype);


//                SqlParameter[] parameters =
//                {
//                    new SqlParameter("@CurrentDate", SqlDbType.DateTime) { Value = CurrentDate },
//                    new SqlParameter("@ReveraFileType", SqlDbType.VarChar) { Value = ReveraFileType },
//                    new SqlParameter("@transID", SqlDbType.VarChar) { Value = TransactionID },
//                    new SqlParameter("@totRec", SqlDbType.Int) { Value = TotalRecord },
//                    new SqlParameter("@CHKSUM", SqlDbType.Float) { Value = CheckSum }
//                };

//                dsFileName = await _readWriteDao.SelectRecords("SPGetOrSetReveraFileNumber", parameters);

//                if (dsFileName != null && dsFileName.Tables.Count > 0 && dsFileName.Tables[0].Rows.Count > 0)
//                {
//                    return dsFileName.Tables[0];
//                }

//            }
//            catch (Exception ex)
//            {
//                if (dsFileName != null)
//                    dsFileName.Dispose();

//                //clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetReveraFileName");
//                _logger.LogError(ex, "Error on GetReveraFileName.");
//            }
//            return (null);
//        }
//        #endregion


//        //Added by Dhaval Kumar for frmInterfaceAuditLog.aspx

//        #region Get Revera Interface Audit log
//        public async Task<DataSet> GetReveraAuditLog(DateTime FromDate, DateTime ToDate)
//        {
//            DataSet? dsReveraAuditLog = new DataSet("BALReveraInterface_dsReveraAuditLog");

//            try
//            {
//                //string[] Pname = new string[2];
//                //Pname[0] = "FromDate";
//                //Pname[1] = "ToDate";

//                //object[] Pvalue = new object[2];
//                //Pvalue[0] = FromDate;
//                //Pvalue[1] = ToDate;

//                //SqlDbType[] Ptype = new SqlDbType[2];
//                //Ptype[0] = SqlDbType.DateTime;
//                //Ptype[1] = SqlDbType.DateTime;
//                //dsReveraAuditLog = db.SelectRecords("SP_ReveraAuditLog", Pname, Pvalue, Ptype);

//                SqlParameter[] parameters =
//                {
//                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = FromDate },
//                    new SqlParameter("@ToDate", SqlDbType.DateTime)   { Value = ToDate }
//                };

//                dsReveraAuditLog = await _readWriteDao.SelectRecords("SP_ReveraAuditLog", parameters);

//                if (dsReveraAuditLog != null && dsReveraAuditLog.Tables.Count > 0 && dsReveraAuditLog.Tables[0].Rows.Count > 0)
//                {
//                    return dsReveraAuditLog;
//                }
//            }
//            catch (Exception ex)
//            {
//                if (dsReveraAuditLog != null)
//                    dsReveraAuditLog.Dispose();
//                //clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetReveraAuditLog");
//                _logger.LogError(ex, "Error on GetReveraAuditLog.");

//            }
//            return (null);
//        }
//        #endregion

//        #region Get Revera Interface Audit log Indepth Details
//        public async Task<DataSet> GetReveraAuditLogDetails(string FileName, string UniqueID)
//        {
//            DataSet? dsAuditLogDetails = new DataSet("BALReveraInterface_dsAuditLogDetails");

//            try
//            {
//                //string[] Pname = new string[2];
//                //Pname[0] = "Fname";
//                //Pname[1] = "UniqID";

//                //object[] Pvalue = new object[2];
//                //Pvalue[0] = FileName;
//                //Pvalue[1] = UniqueID;

//                //SqlDbType[] Ptype = new SqlDbType[2];
//                //Ptype[0] = SqlDbType.VarChar;
//                //Ptype[1] = SqlDbType.VarChar;

//                SqlParameter[] parameters =
//                {
//                    new SqlParameter("@Fname", SqlDbType.VarChar) { Value = FileName },
//                    new SqlParameter("@UniqID", SqlDbType.VarChar) { Value = UniqueID }
//                };
//                dsAuditLogDetails = await _readWriteDao.SelectRecords("SP_ReveraAuditLogDetails", parameters);

//                if (dsAuditLogDetails != null && dsAuditLogDetails.Tables.Count > 0 && dsAuditLogDetails.Tables[0].Rows.Count > 0)
//                {
//                    return dsAuditLogDetails;
//                }
//            }
//            catch (Exception ex)
//            {
//                if (dsAuditLogDetails != null)
//                    dsAuditLogDetails.Dispose();
//                //clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetReveraAuditLogDetails");
//                _logger.LogError(ex, "Error on GetReveraAuditLogDetails.");

//            }
//            return (null);
//        }
//        #endregion
//        //----------------------------END----------------------------------------

//    }
//}