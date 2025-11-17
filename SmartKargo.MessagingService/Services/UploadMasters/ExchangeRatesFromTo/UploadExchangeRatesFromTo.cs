using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Globalization;


namespace QidWorkerRole.UploadMasters.ExchangeRatesFromTo
{
    public class UploadExchangeRatesFromTo
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadExchangeRatesFromTo> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadExchangeRatesFromTo(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadExchangeRatesFromTo> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;
        }
        #endregion

        public async Task<Boolean> ExchangeRatesFromTo(DataSet dsFiles)
        {
            try
            {
                string FilePath = "";

                foreach (DataRow dr in dsFiles.Tables[0].Rows)
                {
                    ///to upadate retry count only.
                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "", 0, 0, 0, 1, "", 1, 1);

                    if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "ExchangeRatesFromTo", out FilePath))
                    {
                        ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath);
                    }
                    else
                    {
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                        await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dr["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;
        }

        public bool ProcessFile(int srnoTBLMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableExchangeRatesFromTo = new DataTable("dataTableExchangeRatesFromTo");
            bool isBinaryReader = false;
            StreamReader srCSV = new StreamReader(filepath);
            try
            {
                string fileExtention = Path.GetExtension(filepath).ToLower();
                if (fileExtention.Equals(".csv"))
                {
                    string str = srCSV.ReadLine();
                    string[] arrCsvElement = str.Replace(" ", "").Split(',');
                    for (int i = 0; i < arrCsvElement.Length; i++)
                    {
                        dataTableExchangeRatesFromTo.Columns.Add(new DataColumn(arrCsvElement[i], typeof(string)));
                    }
                    while ((str = srCSV.ReadLine()) != null)
                    {
                        arrCsvElement = str.Split(',');
                        DataRow dr = dataTableExchangeRatesFromTo.NewRow();
                        for (int i = 0; i < arrCsvElement.Length; i++)
                        {
                            dr[i] = arrCsvElement[i];
                        }
                        dataTableExchangeRatesFromTo.Rows.Add(dr);
                    }
                }
                else if (fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") || fileExtention.Equals(".xlsx"))
                {
                    FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                    IExcelDataReader iExcelDataReader = null;

                    isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") ? true : false;

                    iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                      : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                    // DataSet - Create column names from first row
                    iExcelDataReader.IsFirstRowAsColumnNames = true;
                    dataTableExchangeRatesFromTo = iExcelDataReader.AsDataSet().Tables[0];

                    // Free resources (IExcelDataReader is IDisposable)
                    iExcelDataReader.Close();

                    _uploadMasterCommon.RemoveEmptyRows(dataTableExchangeRatesFromTo);
                }
                else
                {
                    // clsLog.WriteLogAzure("Invalid file: " + filepath);
                    _logger.LogWarning("Invalid file: {filePath}", filepath);
                    return false;
                }
                foreach (DataColumn dataColumn in dataTableExchangeRatesFromTo.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToUpper().Trim();
                }

                #region Creating dataTableExchangeRates DataTable
                DataTable dataTableExchangeRates = new DataTable();
                dataTableExchangeRates.Columns.Add("FileRowNo", System.Type.GetType("System.Int32"));
                dataTableExchangeRates.Columns.Add("From", System.Type.GetType("System.String"));
                dataTableExchangeRates.Columns.Add("To", System.Type.GetType("System.String"));
                dataTableExchangeRates.Columns.Add("Effective_Date", System.Type.GetType("System.DateTime"));
                dataTableExchangeRates.Columns.Add("Exchange_Rate", System.Type.GetType("System.Decimal"));
                dataTableExchangeRates.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));
                #endregion

                string validationDetailsExchangeRates = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;

                for (int i = 0; i < dataTableExchangeRatesFromTo.Rows.Count; i++)
                {
                    validationDetailsExchangeRates = string.Empty;
                    tempDecimalValue = 0;
                    DataRow dataRowExchangeRates = dataTableExchangeRates.NewRow();

                    dataRowExchangeRates["FileRowNo"] = i + 1;

                    #region : From :
                    if (dataTableExchangeRatesFromTo.Rows[i]["FROM"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "From currency not found;";
                    }
                    else if (dataTableExchangeRatesFromTo.Rows[i]["FROM"].ToString().Trim().Trim(',').Length > 4)
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Invalid From currency;";
                    }
                    else
                    {
                        dataRowExchangeRates["From"] = dataTableExchangeRatesFromTo.Rows[i]["FROM"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion From

                    #region : To :
                    if (dataTableExchangeRatesFromTo.Rows[i]["TO"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "To currency not found;";
                    }
                    else if (dataTableExchangeRatesFromTo.Rows[i]["TO"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "From - Currency is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowExchangeRates["To"] = dataTableExchangeRatesFromTo.Rows[i]["TO"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion To

                    #region : Effective_Date :
                    if (dataTableExchangeRatesFromTo.Rows[i]["EFFECTIVE_DATE"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Effective_Date not found;";
                    }
                    else
                    {
                        if (isBinaryReader)
                        {
                            dataRowExchangeRates["Effective_Date"] = DateTime.FromOADate(Convert.ToDouble(dataTableExchangeRatesFromTo.Rows[i]["EFFECTIVE_DATE"].ToString().Trim()));
                        }
                        else
                        {
                            if (DateTime.TryParseExact(dataTableExchangeRatesFromTo.Rows[i]["EFFECTIVE_DATE"].ToString().Trim().Replace('-', '/'), "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                            {
                                dataRowExchangeRates["Effective_Date"] = tempDate;
                            }
                            //tempDate = DateTime.ParseExact(dataTableExchangeRatesFromTo.Rows[i]["EFFECTIVE_DATE"].ToString().Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            //if (DateTime.TryParse(tempDate.ToString(), out tempDate))
                            //{
                            //    dataRowExchangeRates["Effective_Date"] = tempDate;
                            //}
                            else
                            {
                                validationDetailsExchangeRates = validationDetailsExchangeRates + "Invalid ValidFrom;";
                            }
                        }
                    }
                    #endregion Effective_Date

                    #region : Exchange_Rate :
                    if (dataTableExchangeRatesFromTo.Rows[i]["EXCHANGE_RATE"] == null)
                    {
                        dataRowExchangeRates["Exchange_Rate"] = 1;
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Exchange_Rate Not found;";
                    }
                    else if (dataTableExchangeRatesFromTo.Rows[i]["EXCHANGE_RATE"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowExchangeRates["Exchange_Rate"] = 1;
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Exchange_Rate Not found;";
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableExchangeRatesFromTo.Rows[i]["EXCHANGE_RATE"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowExchangeRates["Exchange_Rate"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowExchangeRates["Exchange_Rate"] = 1;
                            validationDetailsExchangeRates = validationDetailsExchangeRates + "Exchange_Rate Not found;";
                        }
                    }
                    #endregion Exchange_Rate

                    dataRowExchangeRates["ValidationDetails"] = validationDetailsExchangeRates;
                    dataTableExchangeRates.Rows.Add(dataRowExchangeRates);
                }

                string errorInSp = string.Empty;
                ValidateAndInsertExchangeRatesFromTo(srnoTBLMasterUploadSummaryLog, dataTableExchangeRates, errorInSp);

                return true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); return false;
            }
            finally
            {
                if (srCSV != null)
                    srCSV.Dispose();

                dataTableExchangeRatesFromTo = null;
            }
        }

        public async Task<DataSet?> ValidateAndInsertExchangeRatesFromTo(int srNotblMasterUploadSummaryLog, DataTable dataTableExchangeRates, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {
                                                                      new SqlParameter("@SrNoTBLMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@DataTableExchangeRatesFromTo", dataTableExchangeRates),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                sqlParameters[2].Direction = ParameterDirection.Output;
                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("masters.uspUploadCurrencyExchangeRatesFromTo", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("masters.uspUploadCurrencyExchangeRatesFromTo", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); return dataSetResult;
            }
        }
    }
}
