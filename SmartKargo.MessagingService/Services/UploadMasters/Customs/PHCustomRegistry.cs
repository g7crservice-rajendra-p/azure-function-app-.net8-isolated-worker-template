using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Globalization;

namespace QidWorkerRole.UploadMasters.ExchangeRatesFromTo
{
    public class PHCustomRegistry
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<PHCustomRegistry> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;

        #region Constructor
        public PHCustomRegistry(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<PHCustomRegistry> logger,
            Func<UploadMasterCommon> uploadMasterCommonFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion


        public async Task<Boolean> PHCustomRegistyUpload(DataSet dsFiles)
        {
            try
            {
                string FilePath = "";

                foreach (DataRow dr in dsFiles.Tables[0].Rows)
                {
                    ///to upadate retry count only.
                    await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "", 0, 0, 0, 1, "", 1, 1);

                    if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "PHCustomRegistry", out FilePath))
                    {
                        await ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath);
                    }

                    else
                    {
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                        await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(Convert.ToInt32(dr["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
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

        public async Task<bool> ProcessFile(int srnoTBLMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTablePHRegistery = new DataTable("dataTablePHRegistery");
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
                    dataTablePHRegistery = iExcelDataReader.AsDataSet().Tables[0];

                    // Free resources (IExcelDataReader is IDisposable)
                    iExcelDataReader.Close();

                    _uploadMasterCommonFactory().RemoveEmptyRows(dataTablePHRegistery);
                }
                else
                {
                    // clsLog.WriteLogAzure("Invalid file: " + filepath);
                    _logger.LogWarning("Invalid file: {filePath}", filepath);
                    return false;
                }
                foreach (DataColumn dataColumn in dataTablePHRegistery.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToUpper().Trim();
                }

                #region Creating dataTableExchangeRates DataTable
                DataTable dataTablePHCustomRegistery = new DataTable();
                dataTablePHCustomRegistery.Columns.Add("FileRowNo", System.Type.GetType("System.Int32"));
                dataTablePHCustomRegistery.Columns.Add("FlightNo", System.Type.GetType("System.String"));
                dataTablePHCustomRegistery.Columns.Add("Origin", System.Type.GetType("System.String"));
                dataTablePHCustomRegistery.Columns.Add("ETA", System.Type.GetType("System.String"));
                dataTablePHCustomRegistery.Columns.Add("REGNO", System.Type.GetType("System.String"));
                dataTablePHCustomRegistery.Columns.Add("DateOfArrival", System.Type.GetType("System.String"));
                dataTablePHCustomRegistery.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));
                #endregion

                string validationDetailsExchangeRates = string.Empty;
                //DateTime tempDate;


                for (int i = 0; i < dataTablePHRegistery.Rows.Count; i++)
                {
                    validationDetailsExchangeRates = string.Empty;

                    DataRow dataRowExchangeRates = dataTablePHCustomRegistery.NewRow();

                    dataRowExchangeRates["FileRowNo"] = i + 1;

                    #region : FlightNo :
                    if (dataTablePHRegistery.Rows[i]["FlightNo"].ToString().Trim().Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Flight Number not found;";
                    }
                    else if (dataTablePHRegistery.Rows[i]["FlightNo"].ToString().Trim().Length < 2)
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Invalid flight;";
                    }
                    else
                    {
                        dataRowExchangeRates["FlightNo"] = dataTablePHRegistery.Rows[i]["FlightNo"].ToString().Trim().ToUpper();
                    }
                    #endregion From

                    #region : Origin :
                    if (dataTablePHRegistery.Rows[i]["Origin"].ToString().Trim().Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Origin not found;";
                    }
                    else if (dataTablePHRegistery.Rows[i]["Origin"].ToString().Trim().Length < 2)
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "Origin not found;";
                    }
                    else
                    {
                        dataRowExchangeRates["Origin"] = dataTablePHRegistery.Rows[i]["Origin"].ToString().Trim().ToUpper();
                    }
                    #endregion To

                    #region : ETA :
                    if (dataTablePHRegistery.Rows[i]["ETA"].ToString().Trim().Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "ETA not found;";
                    }
                    else
                    {
                        //if (isBinaryReader)
                        //{
                        dataRowExchangeRates["ETA"] = dataTablePHRegistery.Rows[i]["ETA"].ToString();
                        //}                         
                    }
                    #endregion Effective_Date

                    #region : RegNo :
                    if (dataTablePHRegistery.Rows[i]["RegNo"] == null)
                    {
                        dataRowExchangeRates["RegNo"] = 1;
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "RegNo Not found;";
                    }
                    else if (dataTablePHRegistery.Rows[i]["RegNo"].ToString().Trim().Equals(string.Empty))
                    {
                        dataRowExchangeRates["RegNo"] = 1;
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "RegNo Not found;";
                    }
                    else
                    {
                        dataRowExchangeRates["RegNo"] = dataTablePHRegistery.Rows[i]["RegNo"].ToString().Trim();

                    }
                    #endregion RegNo

                    #region : Arrival_Date :
                    if (dataTablePHRegistery.Rows[i]["DateOfArrival"].ToString().Trim().Equals(string.Empty))
                    {
                        validationDetailsExchangeRates = validationDetailsExchangeRates + "DateOfArrival not found;";
                    }
                    else
                    {
                        if (isBinaryReader)
                        {
                            dataRowExchangeRates["DateOfArrival"] = DateTime.FromOADate(Convert.ToDouble(dataTablePHRegistery.Rows[i]["DateOfArrival"].ToString().Trim()));
                        }
                        else
                        {
                            DateTime tempDate;
                            if (DateTime.TryParseExact(dataTablePHRegistery.Rows[i]["DateOfArrival"].ToString().Trim().Replace('-', '/'), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                                dataRowExchangeRates["DateOfArrival"] = dataTablePHRegistery.Rows[i]["DateOfArrival"].ToString().Trim();
                            else
                                validationDetailsExchangeRates = validationDetailsExchangeRates + "Invalid Date Format";
                        }
                    }
                    #endregion Effective_Date

                    dataRowExchangeRates["ValidationDetails"] = validationDetailsExchangeRates;
                    dataTablePHCustomRegistery.Rows.Add(dataRowExchangeRates);
                }

                string errorInSp = string.Empty;

                await ValidateAndInsertPHCustomRegistry(srnoTBLMasterUploadSummaryLog, dataTablePHCustomRegistery, errorInSp);

                return true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
            finally
            {
                dataTablePHRegistery = null;
            }
        }

        public async Task<DataSet?> ValidateAndInsertPHCustomRegistry(int srNotblMasterUploadSummaryLog, DataTable dataTableExchangeRates, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {
                                                                      new SqlParameter("@SrNoTBLMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@DataTablePhRegistry", dataTableExchangeRates),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                sqlParameters[2].Direction = ParameterDirection.Output;
                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("masters.uspUploadPHRegistry", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("masters.uspUploadPHRegistry", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return dataSetResult;
            }
        }

    }
}
