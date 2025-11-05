using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using QID.DataAccess;
using System.Data.SqlClient;
using System.Configuration;
using QidWorkerRole;
namespace QidWorkerRole
{
    public class BALReveraInterface
    {
        #region Variables
        public static string ConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        SQLServer db = new SQLServer();

        #endregion

        #region GetAWBInfo
        public DataSet GetAWBInfo(DateTime FromDate, DateTime ToDate)
        {
            DataSet dsGetAWBInfo = new DataSet("BALReveraInterface_dsGetAWBInfo");
            try
            {
                string[] Pname = new string[2];
                Pname[0] = "FromDate";
                Pname[1] = "ToDate";

                object[] Pvalue = new object[2];
                Pvalue[0] = FromDate;
                Pvalue[1] = ToDate;

                SqlDbType[] Ptype = new SqlDbType[2];
                Ptype[0] = SqlDbType.DateTime;
                Ptype[1] = SqlDbType.DateTime;


                dsGetAWBInfo = db.SelectRecords("SPReveraAWBInfomation", Pname, Pvalue, Ptype);

                if (dsGetAWBInfo != null && dsGetAWBInfo.Tables.Count > 0 && dsGetAWBInfo.Tables[0].Rows.Count > 0)
                {
                    return dsGetAWBInfo;
                }

            }
            catch (Exception ex)
            {

                if (dsGetAWBInfo != null)
                    dsGetAWBInfo.Dispose();

                clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetAWBInfo");

            }
            return (null);
        }
        #endregion

        #region FlownInfo
        public DataSet GetFlownInfo(DateTime FromDate, DateTime ToDate)
        {
            DataSet dsFlownInfo = new DataSet("BALReveraInterface_dsFlownInfo");
            try
            {
                string[] Pname = new string[2];
                Pname[0] = "FromDate";
                Pname[1] = "ToDate";

                object[] Pvalue = new object[2];
                Pvalue[0] = FromDate;
                Pvalue[1] = ToDate;

                SqlDbType[] Ptype = new SqlDbType[2];
                Ptype[0] = SqlDbType.DateTime;
                Ptype[1] = SqlDbType.DateTime;


                dsFlownInfo = db.SelectRecords("SPReveraFlownInformation", Pname, Pvalue, Ptype);

                if (dsFlownInfo != null && dsFlownInfo.Tables.Count > 0 && dsFlownInfo.Tables[0].Rows.Count > 0)
                {
                    return dsFlownInfo;
                }

            }
            catch (Exception ex)
            {
                if (dsFlownInfo != null)
                    dsFlownInfo.Dispose();
                clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetFlownInfo");

            }
            return (null);
        }
        #endregion

        #region FltManifest
        public DataSet GetFltManifestDetails(DateTime FromDate, DateTime ToDate)
        {
            DataSet dsFltManifest = new DataSet("BALReveraInterface_dsFltManifest");

            try
            {
                string[] Pname = new string[2];
                Pname[0] = "FromDate";
                Pname[1] = "ToDate";

                object[] Pvalue = new object[2];
                Pvalue[0] = FromDate;
                Pvalue[1] = ToDate;

                SqlDbType[] Ptype = new SqlDbType[2];
                Ptype[0] = SqlDbType.DateTime;
                Ptype[1] = SqlDbType.DateTime;


                dsFltManifest = db.SelectRecords("SP_ReveraManifestInterface", Pname, Pvalue, Ptype);

                if (dsFltManifest != null && dsFltManifest.Tables.Count > 0 && dsFltManifest.Tables[0].Rows.Count > 0)
                {
                    return dsFltManifest;
                }

            }
            catch (Exception ex)
            {
                if (dsFltManifest != null)
                    dsFltManifest.Dispose();
                clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetFltManifestDetails");

            }
            return (null);


        }
        #endregion

        #region GetReveraFileName
        public DataTable GetReveraFileName(DateTime CurrentDate, string ReveraFileType, string TransactionID, int TotalRecord, float CheckSum)
        {
            DataSet dsFileName = new DataSet("BALReveraInterface_dsFileName");
            try
            {
                string[] Pname = new string[5];
                Pname[0] = "CurrentDate";
                Pname[1] = "ReveraFileType";
                Pname[2] = "transID";
                Pname[3] = "totRec";
                Pname[4] = "CHKSUM";


                object[] Pvalue = new object[5];
                Pvalue[0] = CurrentDate;
                Pvalue[1] = ReveraFileType;
                Pvalue[2] = TransactionID;
                Pvalue[3] = TotalRecord;
                Pvalue[4] = CheckSum;



                SqlDbType[] Ptype = new SqlDbType[5];
                Ptype[0] = SqlDbType.DateTime;
                Ptype[1] = SqlDbType.VarChar;
                Ptype[2] = SqlDbType.VarChar;
                Ptype[3] = SqlDbType.Int;
                Ptype[4] = SqlDbType.Float;



                dsFileName = db.SelectRecords("SPGetOrSetReveraFileNumber", Pname, Pvalue, Ptype);

                if (dsFileName != null && dsFileName.Tables.Count > 0 && dsFileName.Tables[0].Rows.Count > 0)
                {
                    return dsFileName.Tables[0];
                }

            }
            catch (Exception ex)
            {
                if (dsFileName != null)
                    dsFileName.Dispose();

                clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetReveraFileName");
            }
            return (null);
        }
        #endregion


        //Added by Dhaval Kumar for frmInterfaceAuditLog.aspx

        #region Get Revera Interface Audit log
        public DataSet GetReveraAuditLog(DateTime FromDate, DateTime ToDate)
        {
            DataSet dsReveraAuditLog = new DataSet("BALReveraInterface_dsReveraAuditLog");

            try
            {
                string[] Pname = new string[2];
                Pname[0] = "FromDate";
                Pname[1] = "ToDate";

                object[] Pvalue = new object[2];
                Pvalue[0] = FromDate;
                Pvalue[1] = ToDate;

                SqlDbType[] Ptype = new SqlDbType[2];
                Ptype[0] = SqlDbType.DateTime;
                Ptype[1] = SqlDbType.DateTime;


                dsReveraAuditLog = db.SelectRecords("SP_ReveraAuditLog", Pname, Pvalue, Ptype);

                if (dsReveraAuditLog != null && dsReveraAuditLog.Tables.Count > 0 && dsReveraAuditLog.Tables[0].Rows.Count > 0)
                {
                    return dsReveraAuditLog;
                }
            }
            catch (Exception ex)
            {
                if (dsReveraAuditLog != null)
                    dsReveraAuditLog.Dispose();
                clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetReveraAuditLog");

            }
            return (null);
        }
        #endregion

        #region Get Revera Interface Audit log Indepth Details
        public DataSet GetReveraAuditLogDetails(string FileName, string UniqueID)
        {
            DataSet dsAuditLogDetails = new DataSet("BALReveraInterface_dsAuditLogDetails");

            try
            {
                string[] Pname = new string[2];
                Pname[0] = "Fname";
                Pname[1] = "UniqID";

                object[] Pvalue = new object[2];
                Pvalue[0] = FileName;
                Pvalue[1] = UniqueID;

                SqlDbType[] Ptype = new SqlDbType[2];
                Ptype[0] = SqlDbType.VarChar;
                Ptype[1] = SqlDbType.VarChar;


                dsAuditLogDetails = db.SelectRecords("SP_ReveraAuditLogDetails", Pname, Pvalue, Ptype);

                if (dsAuditLogDetails != null && dsAuditLogDetails.Tables.Count > 0 && dsAuditLogDetails.Tables[0].Rows.Count > 0)
                {
                    return dsAuditLogDetails;
                }
            }
            catch (Exception ex)
            {
                if (dsAuditLogDetails != null)
                    dsAuditLogDetails.Dispose();
                clsLog.WriteLogAzure(ex, "BALReveraInterface", "GetReveraAuditLogDetails");

            }
            return (null);
        }
        #endregion
        //----------------------------END----------------------------------------

    }
}