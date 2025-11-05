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

namespace QidWorkerRole.UploadMasters.MSRRates
{
     class UploadMSRRates
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        public bool UpdateMSRupload(DataSet dataSetFileData)
        {
            try
            {

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "UploadMSRRates", out uploadFilePath))
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
        public bool ProcessFile(int srnoTBLMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableMSRRates = new DataTable("dataTableMSRRates");
            bool isBinaryReader = false;
            //StreamReader srCSV = new StreamReader(filepath);
            try
            {
                string fileExtention = Path.GetExtension(filepath).ToLower();
                 if (fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") || fileExtention.Equals(".xlsx"))
                {
                    FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                    IExcelDataReader iExcelDataReader = null;

                    isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") ? true : false;

                    iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                      : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                    // DataSet - Create column names from first row
                    iExcelDataReader.IsFirstRowAsColumnNames = true;
                    dataTableMSRRates = iExcelDataReader.AsDataSet().Tables[0];

                    // Free resources (IExcelDataReader is IDisposable)
                    iExcelDataReader.Close();

                    uploadMasterCommon.RemoveEmptyRows(dataTableMSRRates);
                }
                else
                {
                    clsLog.WriteLogAzure("Invalid file: " + filepath);
                    return false;
                }
                foreach (DataColumn dataColumn in dataTableMSRRates.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToUpper().Trim();
                }

                #region Creating dataTableMSRRatesType DataTable
                DataTable dataTableMSRRatesType = new DataTable();
                dataTableMSRRatesType.Columns.Add("FileRowNo", System.Type.GetType("System.Int32"));
                dataTableMSRRatesType.Columns.Add("Origin", System.Type.GetType("System.String"));
                dataTableMSRRatesType.Columns.Add("Destination", System.Type.GetType("System.String"));
                dataTableMSRRatesType.Columns.Add("Transit", System.Type.GetType("System.String"));
                dataTableMSRRatesType.Columns.Add("MSRRate", System.Type.GetType("System.Decimal"));
                dataTableMSRRatesType.Columns.Add("Currency", System.Type.GetType("System.String"));
                dataTableMSRRatesType.Columns.Add("ValidFrom", System.Type.GetType("System.DateTime"));
                dataTableMSRRatesType.Columns.Add("ValidTo", System.Type.GetType("System.DateTime"));
                dataTableMSRRatesType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));
                #endregion

                string validationDetailsMSRRates = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;

                for (int i = 0; i < dataTableMSRRates.Rows.Count; i++)
                {
                    validationDetailsMSRRates = string.Empty;
                    tempDecimalValue = 0;
                    DataRow dataRowMSRRatesExcel = dataTableMSRRatesType.NewRow();

                    dataRowMSRRatesExcel["FileRowNo"] = i + 1;

                    #region : Origin :
                    if (dataTableMSRRates.Rows[i]["Origin"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "Origin not found;";
                    }
                    else if (dataTableMSRRates.Rows[i]["Origin"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "Invalid Origin;";
                    }
                    else
                    {
                        dataRowMSRRatesExcel["Origin"] = dataTableMSRRates.Rows[i]["Origin"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion Origin

                    #region : Destination :
                    if (dataTableMSRRates.Rows[i]["Destination"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "Destination not found;";
                    }
                    else if (dataTableMSRRates.Rows[i]["Destination"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "Invalid Destination;";
                    }
                    else
                    {
                        dataRowMSRRatesExcel["Destination"] = dataTableMSRRates.Rows[i]["Destination"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion Destination

                     if (dataTableMSRRates.Rows[i]["Transit"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "Invalid Transit;";
                    }
                    else
                    {
                        dataRowMSRRatesExcel["Transit"] = dataTableMSRRates.Rows[i]["Transit"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #region : MSRRate :
                    if (dataTableMSRRates.Rows[i]["MSR"] == null)
                    {
                        dataRowMSRRatesExcel["MSRRate"] = 1;
                        validationDetailsMSRRates = validationDetailsMSRRates + "MSRRate Not found;";
                    }
                    else if (dataTableMSRRates.Rows[i]["MSR"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowMSRRatesExcel["MSRRate"] = 1;
                        validationDetailsMSRRates = validationDetailsMSRRates + "MSRRate Not found;";
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableMSRRates.Rows[i]["MSR"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowMSRRatesExcel["MSRRate"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowMSRRatesExcel["MSRRate"] = 1;
                            validationDetailsMSRRates = validationDetailsMSRRates + "MSR Not found;";
                        }
                    }
                    #endregion MSR

                    #region : Currency :
                    if (dataTableMSRRates.Rows[i]["Currency"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "currency not found;";
                    }
                    else if (dataTableMSRRates.Rows[i]["Currency"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "Currency is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowMSRRatesExcel["Currency"] = dataTableMSRRates.Rows[i]["Currency"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion Currency

                    #region : ValidFrom :
                    if (dataTableMSRRates.Rows[i]["Valid From"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "ValidFrom not found;";
                    }
                    else
                    {
                        if (isBinaryReader)
                        {
                            dataRowMSRRatesExcel["ValidFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTableMSRRates.Rows[i]["Valid From"].ToString().Trim()));
                        }
                        else
                        {

                            if (DateTime.TryParse(dataTableMSRRates.Rows[i]["Valid From"].ToString().Trim(), out tempDate))
                            {
                                dataRowMSRRatesExcel["ValidFrom"] = tempDate;
                            }
                            //tempDate = DateTime.ParseExact(dataTableMSRRates.Rows[i]["EFFECTIVE_DATE"].ToString().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            //if (DateTime.TryParse(tempDate.ToString(), out tempDate))
                            //{
                            //    dataRowMSRRatesExcel["Effective_Date"] = tempDate;
                            //}
                            else
                            {
                                validationDetailsMSRRates = validationDetailsMSRRates + "Invalid ValidFrom;";
                            }
                        }
                    }
                    #endregion ValidFrom

                    #region : ValidTo :
                    if (dataTableMSRRates.Rows[i]["Valid To"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsMSRRates = validationDetailsMSRRates + "ValidTo not found;";
                    }
                    else
                    {
                        if (isBinaryReader)
                        {
                            dataRowMSRRatesExcel["ValidTo"] = DateTime.FromOADate(Convert.ToDouble(dataTableMSRRates.Rows[i]["Valid To"].ToString().Trim()));
                        }
                        else
                        {
                            if (DateTime.TryParse(dataTableMSRRates.Rows[i]["Valid To"].ToString().Trim(), out tempDate))
                            {
                                dataRowMSRRatesExcel["ValidTo"] = tempDate;
                            }
                            //tempDate = DateTime.ParseExact(dataTableMSRRates.Rows[i]["EFFECTIVE_DATE"].ToString().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            //if (DateTime.TryParse(tempDate.ToString(), out tempDate))
                            //{
                            //    dataRowMSRRatesExcel["Effective_Date"] = tempDate;
                            //}
                            else
                            {
                                validationDetailsMSRRates = validationDetailsMSRRates + "Invalid ValidTo;";
                            }
                        }
                    }
                    #endregion ValidFrom



                    dataRowMSRRatesExcel["ValidationDetails"] = validationDetailsMSRRates;
                    dataTableMSRRatesType.Rows.Add(dataRowMSRRatesExcel);
                }

                string errorInSp = string.Empty;
                ValidateAndInsertMSRRates(srnoTBLMasterUploadSummaryLog, dataTableMSRRatesType, errorInSp);

                return true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return false;
            }
            finally
            {
                

                dataTableMSRRates = null;
            }
        }
        public DataSet ValidateAndInsertMSRRates(int srNotblMasterUploadSummaryLog, DataTable dataTableMSRRatesType, string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {
                                                                      new SqlParameter("@SrNoTBLMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@DataTableMSRRates", dataTableMSRRatesType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                sqlParameters[2].Direction = ParameterDirection.Output;
                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("Masters.uspUploadMSRRates", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return dataSetResult;
            }
        }
    }
}
