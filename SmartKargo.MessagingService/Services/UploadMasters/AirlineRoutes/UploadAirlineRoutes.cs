using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using QID.DataAccess;
using System.Threading;
using System.IO;
using Excel;
using System.Data.SqlClient;
using System.Globalization;

namespace QidWorkerRole.UploadMasters.AirlineRoutes
{
    class UploadAirlineRoutes
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod AirlineRoutes Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public Boolean AirlineRoutesUpload()
        {
            try
            {
                DataSet dataSetFileData = new DataSet();
                dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.AirlineRoutes);

                DateTime ProcessStart = System.DateTime.UtcNow;
                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", System.DateTime.Now, System.DateTime.Now, 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "AirlineRoutesFile", out uploadFilePath))
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", ProcessStart, ProcessStart, 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                            ProcessStart = System.DateTime.Now;
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", ProcessStart, System.DateTime.Now, 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", System.DateTime.Now, System.DateTime.Now, 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to uploa Airline Route file
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableAirlineRoutesExcelData = new DataTable("dataTableAirlineRoutesExcelData");
            //DataSet dsTemp = new DataSet("ds_tempPax");
            bool isBinaryReader = false;
            string ErrorMessage = string.Empty;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)


                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;

                dataTableAirlineRoutesExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();


                #region Creating AirlineRoutesTable DataTable

                DataTable AirlineRouteDataTable = new DataTable("AirlineRouteTableType");
                AirlineRouteDataTable.Columns.Add("FileRowNo", System.Type.GetType("System.Int32"));
                AirlineRouteDataTable.Columns.Add("Id", System.Type.GetType("System.Int32"));
                AirlineRouteDataTable.Columns.Add("Source", System.Type.GetType("System.String"));
                AirlineRouteDataTable.Columns.Add("Destination", System.Type.GetType("System.String"));
                AirlineRouteDataTable.Columns.Add("ExpRoute", System.Type.GetType("System.String"));
                AirlineRouteDataTable.Columns.Add("IsActive", System.Type.GetType("System.Boolean"));
                AirlineRouteDataTable.Columns.Add("ValidationErrors", System.Type.GetType("System.String"));
                #endregion

                foreach (DataColumn dataColumn in dataTableAirlineRoutesExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }
                string validationDetails = string.Empty;

                for (int i = 0; i < dataTableAirlineRoutesExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;

                    #region Create row for Airline Routes master Data Table
                    DataRow dataRowAirlineRoutes = AirlineRouteDataTable.NewRow();
                    // Maintained for Indexing purpose
                   

                    #region FileRowNo
                    dataRowAirlineRoutes["FileRowNo"] = i + 1;
                    #endregion

                    #region ID
                    if (dataTableAirlineRoutesExcelData.Rows[i]["id"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowAirlineRoutes["Id"] = 0;
                    }

                    else
                    {
                        dataRowAirlineRoutes["Id"] = dataTableAirlineRoutesExcelData.Rows[i]["id"].ToString().Trim().Trim(',');
                    }
                    #endregion

                    #region Source
                    if (dataTableAirlineRoutesExcelData.Rows[i]["source"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetails = validationDetails + "Source not found;";
                    }
                    else if (dataTableAirlineRoutesExcelData.Rows[i]["source"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetails = validationDetails + "Source is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowAirlineRoutes["Source"] = dataTableAirlineRoutesExcelData.Rows[i]["source"].ToString().Trim().Trim(',');
                    }
                    #endregion

                    #region Destination
                    if (dataTableAirlineRoutesExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetails = validationDetails + "Destination not found;";
                    }
                    else if (dataTableAirlineRoutesExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetails = validationDetails + "Destination is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowAirlineRoutes["Destination"] = dataTableAirlineRoutesExcelData.Rows[i]["destination"].ToString().Trim().Trim(',');
                    }
                    #endregion

                    #region ExpRoute
                    if (dataTableAirlineRoutesExcelData.Rows[i]["exproute"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetails = validationDetails + "ExpRoute not found;";
                    }
                    else if (dataTableAirlineRoutesExcelData.Rows[i]["exproute"].ToString().Trim().Trim(',').Length > 50)
                    {
                        validationDetails = validationDetails + "ExpRoute is more than 50 Chars;";
                    }
                    else
                    {
                        dataRowAirlineRoutes["ExpRoute"] = dataTableAirlineRoutesExcelData.Rows[i]["exproute"].ToString().Trim().Trim(',');
                    }
                    #endregion

                    #region[IsActive]

                    try
                    {
                        if (!dataTableAirlineRoutesExcelData.Rows[i]["status"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            if (dataTableAirlineRoutesExcelData.Rows[i]["status"].ToString().ToLower().Equals("active"))
                            {
                                dataRowAirlineRoutes["IsActive"] = true;

                            }
                            else
                            {
                                dataRowAirlineRoutes["IsActive"] = true;
                            }

                        }
                        else
                        {
                            dataRowAirlineRoutes["IsActive"] = false;
                        }
                    }
                    catch (Exception)
                    {
                        dataRowAirlineRoutes["IsActive"] = false;
                        validationDetails = validationDetails + "Invalid IsActive";
                    }

                    #endregion
                    dataRowAirlineRoutes["ValidationErrors"] = validationDetails;

                    #endregion Create row for Airline Routes master Data Table

                    AirlineRouteDataTable.Rows.Add(dataRowAirlineRoutes);

                }

                // Database Call to Validate & Insert AirlineRoute Master
                string errorInSp = string.Empty;
                ValidateAndInsertAirlineRouteData(srNotblMasterUploadSummaryLog, AirlineRouteDataTable, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableAirlineRoutesExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting AirlineRoute Data Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="dataTableOtherChargesMasterType"> dataTableAirlineRouteData Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public DataSet ValidateAndInsertAirlineRouteData(int srNotblMasterUploadSummaryLog, DataTable dataTableAirlineRouteData,
                                                                                              string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                    new SqlParameter("@ExpectedRouteTableInput", dataTableAirlineRouteData),
                                                                    new SqlParameter("@Error", errorInSp)
                                                                  };



                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadAirlineRouteDetails", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return dataSetResult;
            }
        }

    }
}
