using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.ExchangeRates
{
    public class UploadExchangeRates
    {
        //UploadMasterCommon _uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadExchangeRates> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadExchangeRates(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadExchangeRates> logger,
            UploadMasterCommon uploadMasterCommon
         )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;
        }
        #endregion

        public async Task<bool> UpdateExchangeRateUpload(DataSet dataSetFileData)
        {
            try
            {

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "UploadExchangeRates", out uploadFilePath))
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", exception.Message, exception.StackTrace);
                return false;
            }
        }
        public async Task<bool> ProcessFile(int srnoTBLMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableExchangeRatesExcelData = new DataTable("dataTableExchangeRates");
            bool isBinaryReader = false;
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
                    dataTableExchangeRatesExcelData = iExcelDataReader.AsDataSet().Tables[0];

                    // Free resources (IExcelDataReader is IDisposable)
                    iExcelDataReader.Close();

                    _uploadMasterCommon.RemoveEmptyRows(dataTableExchangeRatesExcelData);
                }
                else
                {
                    //clsLog.WriteLogAzure("Invalid file: " + filepath);
                    _logger.LogWarning("Invalid file: {filePath}", filepath);
                    return false;
                }
                foreach (DataColumn dataColumn in dataTableExchangeRatesExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToUpper().Trim();
                }

                #region Creating dataTableMSRRatesType DataTable
                DataTable ExchangeRateType = new DataTable("ExchnageRatesType");
                ExchangeRateType.Columns.Add("CurrencyCode", System.Type.GetType("System.String"));
                ExchangeRateType.Columns.Add("CurrencyIATARate", System.Type.GetType("System.Decimal"));
                ExchangeRateType.Columns.Add("ValidFrom", System.Type.GetType("System.DateTime"));
                ExchangeRateType.Columns.Add("ValidTo", System.Type.GetType("System.DateTime"));
                ExchangeRateType.Columns.Add("Type", System.Type.GetType("System.String"));
                ExchangeRateType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                ExchangeRateType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                ExchangeRateType.Columns.Add("FileRowNo", System.Type.GetType("System.Int32"));
                ExchangeRateType.Columns.Add("ValidationErrors", System.Type.GetType("System.String"));
                #endregion

                string validationDetailsExchangeRate = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;

                for (int i = 0; i < dataTableExchangeRatesExcelData.Rows.Count; i++)
                {
                    validationDetailsExchangeRate = string.Empty;
                    if (dataTableExchangeRatesExcelData.Rows[i]["currencycode"].ToString().Trim().Trim(',').Equals(string.Empty) && dataTableExchangeRatesExcelData.Rows[i]["currencyiatarate"].ToString().Trim().Trim(',').Equals(string.Empty))
                        break;
                    DataRow dataRowExchangeRate = ExchangeRateType.NewRow();

                    #region FileRowNo
                    dataRowExchangeRate["FileRowNo"] = i + 1;
                    #endregion

                    #region CurrencyCode
                    if (dataTableExchangeRatesExcelData.Rows[i]["currencycode"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsExchangeRate = validationDetailsExchangeRate + "CurrencyCode not found;";
                    }
                    else if (dataTableExchangeRatesExcelData.Rows[i]["currencycode"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsExchangeRate = validationDetailsExchangeRate + "CurrencyCode is more than 5 Chars;";
                    }
                    else
                    {
                        dataRowExchangeRate["CurrencyCode"] = dataTableExchangeRatesExcelData.Rows[i]["currencycode"].ToString().Trim().Trim(',');
                    }
                    #endregion

                    #region CurrencyIATARate

                    if (dataTableExchangeRatesExcelData.Rows[i]["currencyiatarate"] == null)
                    {
                        dataRowExchangeRate["CurrencyIATARate"] = tempDecimalValue;
                    }
                    else if (dataTableExchangeRatesExcelData.Rows[i]["currencyiatarate"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowExchangeRate["CurrencyIATARate"] = tempDecimalValue;
                        validationDetailsExchangeRate = validationDetailsExchangeRate + "CurrencyIATARate not found;";
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableExchangeRatesExcelData.Rows[i]["currencyiatarate"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowExchangeRate["CurrencyIATARate"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowExchangeRate["CurrencyIATARate"] = tempDecimalValue;
                            validationDetailsExchangeRate = validationDetailsExchangeRate + "Invalid CurrencyIATARate;";
                        }
                    }

                    #endregion

                    #region ValidFrom

                    if (dataTableExchangeRatesExcelData.Rows[i]["validfrom"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsExchangeRate = validationDetailsExchangeRate + "ValidFrom not found;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowExchangeRate["ValidFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTableExchangeRatesExcelData.Rows[i]["validfrom"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableExchangeRatesExcelData.Rows[i]["validfrom"].ToString().Trim(), out tempDate))
                                {
                                    dataRowExchangeRate["ValidFrom"] = tempDate;
                                }
                                else
                                {
                                    dataRowExchangeRate["ValidFrom"] = DateTime.Now;
                                    validationDetailsExchangeRate = validationDetailsExchangeRate + "Invalid ValidFrom;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowExchangeRate["ValidFrom"] = DateTime.Now;
                            validationDetailsExchangeRate = validationDetailsExchangeRate + "Invalid ValidFrom;";
                        }

                    }

                    #endregion

                    #region ValidTo

                    if (dataTableExchangeRatesExcelData.Rows[i]["validto"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsExchangeRate = validationDetailsExchangeRate + "ValidTo not found;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowExchangeRate["ValidTo"] = DateTime.FromOADate(Convert.ToDouble(dataTableExchangeRatesExcelData.Rows[i]["validto"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableExchangeRatesExcelData.Rows[i]["validto"].ToString().Trim(), out tempDate))
                                {
                                    dataRowExchangeRate["ValidTo"] = tempDate;
                                }
                                else
                                {
                                    dataRowExchangeRate["ValidTo"] = DateTime.Now;
                                    validationDetailsExchangeRate = validationDetailsExchangeRate + "Invalid ValidTo;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowExchangeRate["ValidTo"] = DateTime.Now;
                            validationDetailsExchangeRate = validationDetailsExchangeRate + "Invalid ValidTo;";
                        }

                    }
                    #endregion

                    #region Type
                    if (dataTableExchangeRatesExcelData.Rows[i]["type"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowExchangeRate["Type"] = dataTableExchangeRatesExcelData.Rows[i]["type"].ToString().Trim().Trim(',');
                        // validationDetailsExchangeRate = validationDetailsExchangeRate + "Type not found;";
                    }
                    else if (dataTableExchangeRatesExcelData.Rows[i]["type"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetailsExchangeRate = validationDetailsExchangeRate + "Type is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowExchangeRate["Type"] = dataTableExchangeRatesExcelData.Rows[i]["type"].ToString().Trim().Trim(',');
                    }
                    #endregion

                    #region Updated BY

                    dataRowExchangeRate["UpdatedBy"] = string.Empty;
                    #endregion

                    #region UpdatedON
                    dataRowExchangeRate["UpdatedOn"] = DateTime.Now;
                    #endregion

                    #region Validation Errors
                    dataRowExchangeRate["ValidationErrors"] = validationDetailsExchangeRate;
                    #endregion

                    ExchangeRateType.Rows.Add(dataRowExchangeRate);
                }

                string errorInSp = string.Empty;
                await ValidateAndInsertExchangeRates(srnoTBLMasterUploadSummaryLog, ExchangeRateType, errorInSp);

                return true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
            finally
            {
                dataTableExchangeRatesExcelData = null;
            }
        }

        public async Task<DataSet?> ValidateAndInsertExchangeRates(int srNotblMasterUploadSummaryLog, DataTable dataTableMSRRatesType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@ExchangeRateTableTypeInput", dataTableMSRRatesType),
                    new SqlParameter("@Error", errorInSp)
                ];

                sqlParameters[2].Direction = ParameterDirection.Output;

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("sp_AddExchangeRateDetails", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("sp_AddExchangeRateDetails", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return dataSetResult;
            }
        }
    }
}
