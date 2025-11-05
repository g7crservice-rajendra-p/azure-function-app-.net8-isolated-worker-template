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

namespace QidWorkerRole.UploadMasters.Pax
{
    class UploadScedulePaxData
    {

        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod ScedulePaxData Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public Boolean PaxMasterUpload()
        {
            try
            {
                DataSet dataSetFileData = new DataSet();
                dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.PaxUpload);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "PaxUploadFile", out uploadFilePath))
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
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
        /// Method to Process Other Charges Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Other Charges Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTablePaxExcelData = new DataTable("dataTablePaxExcelData");
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

                dataTablePaxExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTablePaxExcelData);

                #region Creating OtherChargesMasterType DataTable

                DataTable PaxDataTable = new DataTable("PaxMasterType");
                PaxDataTable.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                PaxDataTable.Columns.Add("FlightId", System.Type.GetType("System.Int32"));
                PaxDataTable.Columns.Add("FlightFrom", System.Type.GetType("System.String"));
                PaxDataTable.Columns.Add("FlightTo", System.Type.GetType("System.String"));
                PaxDataTable.Columns.Add("FlightDate", System.Type.GetType("System.String"));
                PaxDataTable.Columns.Add("PaxCount", System.Type.GetType("System.Int32"));
                PaxDataTable.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                PaxDataTable.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                PaxDataTable.Columns.Add("ValidationErrors", System.Type.GetType("System.String"));

                #endregion


                string validationDetails = string.Empty;
                DateTime tempDate = DateTime.Now;

                for (int i = 0; i < dataTablePaxExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;

                    int SerialNumber = 0;
                    string FlightId = string.Empty;
                    string FlightFrom = string.Empty;
                    string FlightTo = string.Empty;

                    #region Create row for PAX master Data Table

                    // Maintained for Indexing purpose
                    SerialNumber = i + 1;

                    #region FlightId
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(dataTablePaxExcelData.Rows[i]["Flight No"].ToString().Trim().Trim(',')))
                        {
                            FlightId = dataTablePaxExcelData.Rows[i]["Flight No"].ToString().Trim().ToUpper().Trim(',');
                        }

                    #endregion FlightId

                        #region Flight From

                        if (!string.IsNullOrWhiteSpace(dataTablePaxExcelData.Rows[i]["Flight From"].ToString().Trim().Trim(',')))
                        {
                            FlightFrom = dataTablePaxExcelData.Rows[i]["Flight From"].ToString().Trim().ToUpper().Trim(',');

                        }

                        #endregion Flight From

                        #region Flight To

                        if (!string.IsNullOrWhiteSpace(dataTablePaxExcelData.Rows[i]["Flight To"].ToString().Trim().Trim(',')))
                        {
                            FlightTo = dataTablePaxExcelData.Rows[i]["Flight To"].ToString().Trim().ToUpper().Trim(',');

                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                        #endregion Flight To

                    for (int j = 3; j < dataTablePaxExcelData.Columns.Count; j++)
                    {
                        try
                        {
                            DataColumn dataColumn = dataTablePaxExcelData.Columns[j];

                            if (DateTime.TryParse(dataColumn.ColumnName.ToString(), out tempDate))
                            {
                                DataRow dataRowPaxType = PaxDataTable.NewRow();

                                dataRowPaxType["SerialNumber"] = SerialNumber;
                                dataRowPaxType["FlightId"] = FlightId;
                                dataRowPaxType["FlightFrom"] = FlightFrom;
                                dataRowPaxType["FlightTo"] = FlightTo;
                                dataRowPaxType["UpdatedOn"] = DateTime.Now;
                                dataRowPaxType["UpdatedBy"] = string.Empty;
                                dataRowPaxType["FlightDate"] = Convert.ToDateTime(tempDate).ToString("dd/MM/yyyy");
                                dataRowPaxType["PaxCount"] = (string.IsNullOrWhiteSpace(dataTablePaxExcelData.Rows[i][j].ToString().Trim()) == true ? 0 : Convert.ToInt16(dataTablePaxExcelData.Rows[i][j].ToString().Trim()));
                                dataRowPaxType["ValidationErrors"] = string.Empty;
                                PaxDataTable.Rows.Add(dataRowPaxType);
                            }
                            else
                            {
                                DataRow dataRowPaxType = PaxDataTable.NewRow();
                                dataRowPaxType["SerialNumber"] = SerialNumber;
                                dataRowPaxType["FlightId"] = FlightId;
                                dataRowPaxType["FlightFrom"] = FlightFrom;
                                dataRowPaxType["FlightTo"] = FlightTo;
                                dataRowPaxType["PaxCount"] = 0;
                                dataRowPaxType["ValidationErrors"] = "Invalid Flight Date";
                                PaxDataTable.Rows.Add(dataRowPaxType);


                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = ErrorMessage + ex.Message;
                            DataRow dataRowPaxType = PaxDataTable.NewRow();
                            dataRowPaxType["SerialNumber"] = SerialNumber;
                            dataRowPaxType["FlightId"] = FlightId;
                            dataRowPaxType["FlightFrom"] = FlightFrom;
                            dataRowPaxType["FlightTo"] = FlightTo;
                            dataRowPaxType["PaxCount"] = 0;
                            dataRowPaxType["ValidationErrors"] = ErrorMessage;
                            PaxDataTable.Rows.Add(dataRowPaxType);
                        }
                    }

                    #endregion Create row for PAX master Data Table


                }

                // Database Call to Validate & Insert Pax Data Master
                string errorInSp = string.Empty;
                ValidateAndInsertPaxData(srNotblMasterUploadSummaryLog, PaxDataTable, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTablePaxExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Pax Data Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="dataTableOtherChargesMasterType"> UploadPaxTable Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public DataSet ValidateAndInsertPaxData(int srNotblMasterUploadSummaryLog, DataTable dataTablePaxData,
                                                                                              string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                    new SqlParameter("@UploadPaxTable", dataTablePaxData),
                                                                    new SqlParameter("@Error", errorInSp)
                                                                  };



                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadPaxData", sqlParameters);

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
