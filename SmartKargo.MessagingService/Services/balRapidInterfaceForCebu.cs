using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using QID.DataAccess;
using System.Data.SqlClient;
using System.Configuration;
using QidWorkerRole;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace QidWorkerRole
{
    class balRapidInterfaceForCebu
    {
        public static Dictionary<string, string> objDictionary = null;
        public static Dictionary<string, string> objUploadDictionary = null;
        public DataSet InsertRapidInterfaceData(DateTime updatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate, string CTMFileName, string FlownFileName)
        {
            objDictionary = new Dictionary<string, string>();
            objUploadDictionary = new Dictionary<string, string>();
            DataSet ds = new DataSet();
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                //SqlDataAdapter objDA = null;
                //DataSet objDs = null;
                //string XML = string.Empty;
                //DataTable objDT = null;
                //DataSet dsDomestic = null;

                clsLog.WriteLog("----------------------------------------------------------------------------------------------------------------------");
                clsLog.WriteLog("Schedular run on ::" + System.DateTime.Now);
                objDictionary.Add("File Names ", "Status");
                #region "SAP XML for Collecions"
                //string Command = "Exec BI.usp_getSAPCollectionDetails '" + FromDate.ToString("MM/dd/yyyy") + "', '" + ToDate.ToString("MM/dd/yyyy") + "', '" + ExecutedOn.AddDays(0).ToString("MM/dd/yyyy") + "'";

                //SQLServer da = new SQLServer();

                string[] pName = new string[6];
                pName[0] = "UserName";
                pName[1] = "UpdatedOn";
                pName[2] = "FromDate";
                pName[3] = "Todate";
                pName[4] = "CTMBatchIDForTxtFile";
                pName[5] = "FlownBatchIDForTxtFile";


                object[] pValue = new object[6];
                pValue[0] = strUserName;
                pValue[1] = updatedOn;
                pValue[2] = dtFromDate;
                pValue[3] = dtToDate;
                pValue[4] = CTMFileName;
                pValue[5] = FlownFileName;

                SqlDbType[] pType = new SqlDbType[6];
                pType[0] = SqlDbType.VarChar;
                pType[1] = SqlDbType.DateTime;
                pType[2] = SqlDbType.DateTime;
                pType[3] = SqlDbType.DateTime;
                pType[4] = SqlDbType.VarChar;
                pType[5] = SqlDbType.VarChar;



                //Clientname = objBAL.GetClientName();
                //if (Clientname.Contains("CEBU"))
                //{
                ds = SelectRecords("SP_RapidInterfaceInsertAWBFile_CEBU", pName, pValue, pType);
                //}
                //else
                //{
                //    ds = da.SelectRecords("SP_RapidInterfaceInsert", pName, pValue, pType);

                //}

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
                clsLog.WriteLogAzure(ex);                
                return ds;
            }
        }
        public DataSet GetRapidInterfaceData(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;


                ds = SelectRecords("SP_RapidInterfaceSelectAWBFile", QueryPname, QueryValue, QueryType);

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
                clsLog.WriteLogAzure(ex);                
                return ds;
            }
        }


        public DataSet InsertRapidFlownTransaction(DateTime UpdatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[4];
                PName[0] = "UserName";
                PName[1] = "UpdatedOn";
                PName[2] = "FromDate";
                PName[3] = "Todate";


                object[] PValue = new object[4];
                PValue[0] = strUserName;
                PValue[1] = UpdatedOn;
                PValue[2] = dtFromDate;
                PValue[3] = dtToDate;




                SqlDbType[] PType = new SqlDbType[4];
                PType[0] = SqlDbType.VarChar;
                PType[1] = SqlDbType.DateTime;
                PType[2] = SqlDbType.DateTime;
                PType[3] = SqlDbType.DateTime;




                ds = da.SelectRecords("uspInsertRapidFlownCebu", PName, PValue, PType);


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
                clsLog.WriteLogAzure(ex);
                
                return ds;
            }
        }

        public DataSet GetRapidFlownTransaction(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;




                ds = da.SelectRecords("uspgetRapidFlownCebu", QueryPname, QueryValue, QueryType);


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
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        public DataSet InsertRapidExportSales(DateTime UpdatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[5];
                PName[0] = "UserName";
                PName[1] = "UpdatedOn";
                PName[2] = "FromDate";
                PName[3] = "Todate";
                PName[4] = "FrontEndCall";


                object[] PValue = new object[5];
                PValue[0] = strUserName;
                PValue[1] = UpdatedOn;
                PValue[2] = dtFromDate;
                PValue[3] = dtToDate;
                PValue[4] = 0;




                SqlDbType[] PType = new SqlDbType[5];
                PType[0] = SqlDbType.VarChar;
                PType[1] = SqlDbType.DateTime;
                PType[2] = SqlDbType.DateTime;
                PType[3] = SqlDbType.DateTime;
                PType[4] = SqlDbType.Int;



                ds = da.SelectRecords("uspInsertRapidExportSalesCebu", PName, PValue, PType);


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
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }
        public DataSet GetRapidExportSalesTransaction(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;




                ds = da.SelectRecords("uspgetRapidExportSalesCebu", QueryPname, QueryValue, QueryType);



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
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        public DataSet InsertRapidPXCCARecordsCebu(DateTime UpdatedOn, string strUserName, DateTime dtFromDate, DateTime dtToDate)
        {

            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] PName = new string[4];
                PName[0] = "UserName";
                PName[1] = "UpdatedOn";
                PName[2] = "FromDate";
                PName[3] = "Todate";


                object[] PValue = new object[4];
                PValue[0] = strUserName;
                PValue[1] = UpdatedOn;
                PValue[2] = dtFromDate;
                PValue[3] = dtToDate;




                SqlDbType[] PType = new SqlDbType[4];
                PType[0] = SqlDbType.VarChar;
                PType[1] = SqlDbType.DateTime;
                PType[2] = SqlDbType.DateTime;
                PType[3] = SqlDbType.DateTime;




                ds = da.SelectRecords("uspInsertRapidPXCCARecordsCebu", PName, PValue, PType);


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
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }
        public DataSet GetRapidPXCCARecordsTransaction(string strFileName)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[1];
                object[] QueryValue = new object[1];
                SqlDbType[] QueryType = new SqlDbType[1];

                QueryPname[0] = "BatchIDForTxtFile";

                QueryType[0] = SqlDbType.VarChar;

                QueryValue[0] = strFileName;


                ds = da.SelectRecords("uspgetRapidPXCCARecordscebu", QueryPname, QueryValue, QueryType);



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
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }

        #region Get String By Procedure Multiple Param

        /// <summary>
        /// Gets String result from database based on StoredProcedure.
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="queryPName"></param>
        /// <param name="queryPValues"></param>
        /// <param name="queryPTypes"></param>
        /// <returns>Single string data returned by query.</returns>
        public string GetStringByProcedure(string procedureName, string[] queryPName, object[] queryPValues, SqlDbType[] queryPTypes)
        {
            DataSet ds = null;
            try
            {
                string strGetString = "";
                ds = SelectRecords(procedureName, queryPName, queryPValues, queryPTypes);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            strGetString = ds.Tables[0].Rows[0][0].ToString();
                        }
                    }
                    ds.Dispose();
                }
                else
                {
                    strGetString = "";
                }
                return (strGetString);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return (null);
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }

        }
        #endregion
        public DataSet SelectRecords(string selectProcedure, string[] queryPName, object[] queryValues, SqlDbType[] queryTypes)
        {
            DataSet dataSet = new DataSet();
            SqlCommand cmd = null;
            SqlDataAdapter adapter = null;
            StringBuilder sbParam = new StringBuilder();
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            SqlConnection _con = new SqlConnection();
            try
            {
                _con.ConnectionString = connectionString;
                if (_con.State != ConnectionState.Open)
                    _con.Open();
                adapter = new SqlDataAdapter();
                cmd = new SqlCommand
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = selectProcedure,
                    Connection = _con
                };
                adapter.SelectCommand = cmd;
                adapter.SelectCommand.CommandTimeout = 10000;

                //Add parameters to command.
                sbParam.Append("EXEC " + selectProcedure + " ");
                for (int i = 0; i < queryPName.Length; i++)
                {
                    cmd.Parameters.Add("@" + queryPName[i], queryTypes[i]).Value = queryValues[i];
                    switch (Convert.ToString(queryTypes[i]))
                    {
                        case "VarChar":
                        case "NVarChar":
                        case "DateTime":
                        case "Date":
                            sbParam.Append("@" + queryPName[i] + "='" + queryValues[i] + "',");
                            break;
                        case "Bit":
                            int isBool = Convert.ToBoolean(queryValues[i]) ? 1 : 0;
                            sbParam.Append("@" + queryPName[i] + "=" + isBool + ",");
                            break;
                        default:
                            sbParam.Append("@" + queryPName[i] + "=" + queryValues[i] + ",");
                            break;
                    }
                }

                adapter.Fill(dataSet);

                {
                    bool blnFlag = false;

                    for (int intCount = 0; intCount < dataSet.Tables.Count; intCount++)
                    {
                        if (dataSet.Tables[intCount].Rows.Count > 0)
                        {
                            blnFlag = true;
                            break;
                        }
                    }

                    if (blnFlag == false)
                    {
                        if (_con.State == ConnectionState.Open)
                            _con.Close();

                        string strArchival = GetConnectionStringArchival();
                        if (!string.IsNullOrEmpty(strArchival))
                        {
                            _con.ConnectionString = strArchival;
                            cmd.Connection = _con;
                            adapter.Fill(dataSet);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                dataSet = null;
                clsLog.WriteLogAzure(ex);
            }
            finally
            {
                if (adapter != null)
                    adapter.Dispose();

                if (cmd != null)
                    cmd.Dispose();

                if (_con.State == ConnectionState.Open)
                    _con.Close();
            }
            return dataSet;
        }
        private string GetConnectionStringArchival()
        {
            try
            {
                string strcon = Convert.ToString(ConfigurationManager.ConnectionStrings["ConStrArchival"]);
                return (strcon);
            }
            catch (Exception)
            {
                return ("");
            }
        }

        public DataSet getInterfaceType()
        {
            try
            {
                SQLServer da = new SQLServer();
                return da.GetDataset("SELECT DISTINCT(InterfaceType) FROM Log.InterfacedFileDetails");
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return null;

            }
        }
        #endregion

        public DataSet GetInterfaceDetails(string strFileType, DateTime dtFromDate, DateTime dtToDate, string strMsgKey)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                //Add Parameters

                da.AddParameters("@FromDate", SqlDbType.DateTime, ParameterDirection.Input, dtFromDate);
                da.AddParameters("@ToDate", SqlDbType.DateTime, ParameterDirection.Input, dtToDate);
                da.AddParameters("@InterfaceType", SqlDbType.VarChar, ParameterDirection.Input, strFileType);

                da.FillDataset("[dbo].[uspGetInterfaceDetais]", ds, null, null);
                return ds;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return ds;
            }
        }


        public void SetInterfaceDetails(string fileType, string fileName, string fileUrl, string createdBy, DateTime createdDate)
        {

            try
            {
                SQLServer da = new SQLServer();

                string[] pName = new string[5];
                pName[0] = "FileType";
                pName[1] = "FileName";
                pName[2] = "FileURL";
                pName[3] = "CreatedBy";
                pName[4] = "CreatedDate";

                object[] pValue = new object[5];
                pValue[0] = fileType;
                pValue[1] = fileName;
                pValue[2] = fileUrl;
                pValue[3] = createdBy;
                pValue[4] = createdDate;

                SqlDbType[] pType = new SqlDbType[5];
                pType[0] = SqlDbType.VarChar;
                pType[1] = SqlDbType.VarChar;
                pType[2] = SqlDbType.VarChar;
                pType[3] = SqlDbType.VarChar;
                pType[4] = SqlDbType.DateTime;

                da.SelectRecords("uspSetInterfaceDetails", pName, pValue, pType);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        public DataSet GetMissingAWBFlownDetails(DateTime updatedOn, DateTime dtFromDate, DateTime dtToDate)
        {
            DataSet ds = new DataSet();
            try
            {
                SQLServer da = new SQLServer();

                string[] QueryPname = new string[3];
                object[] QueryValue = new object[3];
                SqlDbType[] QueryType = new SqlDbType[3];

                QueryPname[0] = "UpdatedOn";
                QueryPname[1] = "FromDate";
                QueryPname[2] = "Todate";

                QueryValue[0] = updatedOn;
                QueryValue[1] = dtFromDate;
                QueryValue[2] = dtToDate;

                QueryType[0] = SqlDbType.DateTime;
                QueryType[1] = SqlDbType.DateTime;
                QueryType[2] = SqlDbType.DateTime;


                ds = SelectRecords("uspRapidAWBFlownException", QueryPname, QueryValue, QueryType);

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
                clsLog.WriteLogAzure(ex);
                return ds;
            }

        }

    }
}
