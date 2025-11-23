using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.FlightBudget
{
    /// <summary>
    /// Class to Upload Flight Budget File.
    /// </summary>
    public class UploadFlightBudget
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadFlightBudget> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;

        #region Constructor
        public UploadFlightBudget(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadFlightBudget> logger,
            Func<UploadMasterCommon> uploadMasterCommonFactory
         )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion

        /// <summary>
        /// Method to Uplaod Flight Budget.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<bool> FlightBudgetUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.FlightBudget);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "FlightBudgetUploadFile", out uploadFilePath))
                        {
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
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

        /// <summary>
        /// Method to Process Flight Budget Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Flight Budget Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableFlightBudgetExcelData = new DataTable("dataTableFlightBudgetExcelData");

            bool isBinaryReader = false;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".XLS") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTableFlightBudgetExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommonFactory().RemoveEmptyRows(dataTableFlightBudgetExcelData);

                foreach (DataColumn dataColumn in dataTableFlightBudgetExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableFlightBudgetExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating FlightBudgetType DataTable

                DataTable FlightBudgetType = new DataTable("FlightBudgetType");
                FlightBudgetType.Columns.Add("FlightBudgetIndex", System.Type.GetType("System.Int32"));
                FlightBudgetType.Columns.Add("ID", System.Type.GetType("System.Int32"));
                FlightBudgetType.Columns.Add("FlightNo", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("Origin", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("Destination", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("AircraftType", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("No_Of_Flights", System.Type.GetType("System.Int32"));
                FlightBudgetType.Columns.Add("Frequency", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("WtCapacity", System.Type.GetType("System.Decimal"));
                FlightBudgetType.Columns.Add("CubMCapacity", System.Type.GetType("System.Decimal"));
                FlightBudgetType.Columns.Add("CargoDensity", System.Type.GetType("System.Decimal"));
                FlightBudgetType.Columns.Add("TargetTonnage", System.Type.GetType("System.Decimal"));
                FlightBudgetType.Columns.Add("UoM", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("TargetRevenues", System.Type.GetType("System.Decimal"));
                FlightBudgetType.Columns.Add("RevCurrency", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("TargetCost", System.Type.GetType("System.Decimal"));
                FlightBudgetType.Columns.Add("CostCurrency", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                FlightBudgetType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("ValidFrom", System.Type.GetType("System.DateTime"));
                FlightBudgetType.Columns.Add("ValidTo", System.Type.GetType("System.DateTime"));
                FlightBudgetType.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
                FlightBudgetType.Columns.Add("FlightDate", System.Type.GetType("System.DateTime"));
                FlightBudgetType.Columns.Add("FlightBudgetOrigin", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("FlightBudgetDestination", System.Type.GetType("System.String"));
                FlightBudgetType.Columns.Add("TargetUOM", System.Type.GetType("System.Byte"));
                FlightBudgetType.Columns.Add("ValidationDetailsFlightBudget", System.Type.GetType("System.String"));

                #endregion Creating FlightBudgetType DataTable

                string validationDetailsFlightBudget = string.Empty;
                int intValue;
                decimal tempDecimalValue = 0;
                DateTime tempDate;

                for (int i = 0; i < dataTableFlightBudgetExcelData.Rows.Count; i++)
                {
                    validationDetailsFlightBudget = string.Empty;

                    #region Create row for FlightBudgetType Data Table

                    DataRow dataRowFlightBudgetType = FlightBudgetType.NewRow();

                    #region FlightBudgetIndex [INT] NULL

                    dataRowFlightBudgetType["FlightBudgetIndex"] = i + 1;

                    #endregion FlightBudgetIndex

                    #region ID [INT] NULL

                    dataRowFlightBudgetType["ID"] = DBNull.Value;

                    #endregion ID

                    #region FlightNo [VARCHAR] (20) NULL

                    if (columnNames.Contains("flightno"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["flightno"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "FlightNo is more than 20 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["FlightNo"] = dataTableFlightBudgetExcelData.Rows[i]["flightno"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion FlightNo

                    #region Origin [VARCHAR] (50) NULL

                    if (columnNames.Contains("origin"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "Origin is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["Origin"] = dataTableFlightBudgetExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Origin

                    #region Destination [VARCHAR] (50) NULL

                    if (columnNames.Contains("destination"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "Destination is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["Destination"] = dataTableFlightBudgetExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Destination

                    #region AircraftType [VARCHAR] (50) NULL

                    if (columnNames.Contains("aircrafttype"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["aircrafttype"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "AircraftType is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["AircraftType"] = dataTableFlightBudgetExcelData.Rows[i]["aircrafttype"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion AircraftType

                    #region No_Of_Flights [INT] NULL

                    if (columnNames.Contains("no_of_flights"))
                    {
                        intValue = 0;
                        try
                        {
                            if (Int32.TryParse(dataTableFlightBudgetExcelData.Rows[i]["no_of_flights"].ToString().Trim().ToUpper().Trim(','), out intValue))
                            {
                                dataRowFlightBudgetType["No_Of_Flights"] = intValue;
                            }
                            else
                            {
                                dataRowFlightBudgetType["No_Of_Flights"] = DBNull.Value;
                                validationDetailsFlightBudget = validationDetailsFlightBudget + "No_Of_Flights must be int;";
                            }
                        }
                        catch
                        {
                            dataRowFlightBudgetType["No_Of_Flights"] = DBNull.Value;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "No_Of_Flights must be int;";
                        }
                    }

                    #endregion No_Of_Flights

                    #region Frequency [VARCHAR] (20) NULL

                    if (columnNames.Contains("frequency"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["frequency"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "Frequency is more than 20 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["Frequency"] = dataTableFlightBudgetExcelData.Rows[i]["frequency"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Frequency

                    #region WtCapacity [DECIMAL] (18, 2) NULL

                    if (columnNames.Contains("wtcapacity"))
                    {
                        tempDecimalValue = 0;
                        if (decimal.TryParse(dataTableFlightBudgetExcelData.Rows[i]["wtcapacity"].ToString().Trim(), out tempDecimalValue))
                        {
                            dataRowFlightBudgetType["WtCapacity"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowFlightBudgetType["WtCapacity"] = tempDecimalValue;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "WtCapacity must be decimal;";
                        }
                    }

                    #endregion WtCapacity

                    #region CubMCapacity [DECIMAL] (18, 2) NULL

                    if (columnNames.Contains("cubmcapacity"))
                    {
                        tempDecimalValue = 0;
                        if (decimal.TryParse(dataTableFlightBudgetExcelData.Rows[i]["cubmcapacity"].ToString().Trim(), out tempDecimalValue))
                        {
                            dataRowFlightBudgetType["CubMCapacity"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowFlightBudgetType["CubMCapacity"] = tempDecimalValue;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "CubMCapacity must be decimal;";
                        }
                    }

                    #endregion CubMCapacity

                    #region CargoDensity [DECIMAL] (18, 2) NULL

                    if (columnNames.Contains("cargodensity"))
                    {
                        tempDecimalValue = 0;
                        if (decimal.TryParse(dataTableFlightBudgetExcelData.Rows[i]["cargodensity"].ToString().Trim(), out tempDecimalValue))
                        {
                            dataRowFlightBudgetType["CargoDensity"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowFlightBudgetType["CargoDensity"] = tempDecimalValue;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "CargoDensity must be decimal;";
                        }
                    }

                    #endregion CargoDensity

                    #region TargetTonnage [DECIMAL] (18, 2) NULL

                    if (columnNames.Contains("targettonnage"))
                    {
                        tempDecimalValue = 0;
                        if (decimal.TryParse(dataTableFlightBudgetExcelData.Rows[i]["targettonnage"].ToString().Trim(), out tempDecimalValue))
                        {
                            dataRowFlightBudgetType["TargetTonnage"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowFlightBudgetType["TargetTonnage"] = tempDecimalValue;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "TargetTonnage must be decimal;";
                        }
                    }

                    #endregion TargetTonnage

                    #region UoM [VARCHAR] (3) NULL

                    if (columnNames.Contains("uom"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Length > 3)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "UoM is more than 3 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["UoM"] = dataTableFlightBudgetExcelData.Rows[i]["uom"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion UoM

                    #region TargetRevenues [DECIMAL] (18, 2) NULL

                    if (columnNames.Contains("targetrevenues"))
                    {
                        tempDecimalValue = 0;
                        if (decimal.TryParse(dataTableFlightBudgetExcelData.Rows[i]["targetrevenues"].ToString().Trim(), out tempDecimalValue))
                        {
                            dataRowFlightBudgetType["TargetRevenues"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowFlightBudgetType["TargetRevenues"] = tempDecimalValue;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "TargetRevenues must be decimal;";
                        }
                    }

                    #endregion TargetRevenues

                    #region RevCurrency [VARCHAR] (20) NULL

                    if (columnNames.Contains("revcurrency"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["revcurrency"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "RevCurrency is more than 20 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["RevCurrency"] = dataTableFlightBudgetExcelData.Rows[i]["revcurrency"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion RevCurrency

                    #region TargetCost [DECIMAL] (18, 2) NULL

                    if (columnNames.Contains("targetcost"))
                    {
                        tempDecimalValue = 0;
                        if (decimal.TryParse(dataTableFlightBudgetExcelData.Rows[i]["targetcost"].ToString().Trim(), out tempDecimalValue))
                        {
                            dataRowFlightBudgetType["TargetCost"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowFlightBudgetType["TargetCost"] = tempDecimalValue;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "TargetCost must be decimal;";
                        }
                    }

                    #endregion TargetCost

                    #region CostCurrency [VARCHAR] (20) NULL

                    if (columnNames.Contains("costcurrency"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["costcurrency"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "CostCurrency is more than 20 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["CostCurrency"] = dataTableFlightBudgetExcelData.Rows[i]["costcurrency"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion CostCurrency

                    #region UpdatedOn [DATETIME] NULL

                    dataRowFlightBudgetType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region UpdatedBy [VARCHAR] (100) NULL

                    dataRowFlightBudgetType["UpdatedBy"] = DBNull.Value;

                    #endregion UpdatedBy

                    #region ValidFrom [DATETIME] NULL

                    if (columnNames.Contains("validfrom"))
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowFlightBudgetType["ValidFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTableFlightBudgetExcelData.Rows[i]["validfrom"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableFlightBudgetExcelData.Rows[i]["validfrom"].ToString().Trim(), out tempDate))
                                {
                                    dataRowFlightBudgetType["ValidFrom"] = tempDate;
                                }
                                else
                                {
                                    dataRowFlightBudgetType["ValidFrom"] = DateTime.Now;
                                    validationDetailsFlightBudget = validationDetailsFlightBudget + "Invalid ValidFrom;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowFlightBudgetType["ValidFrom"] = DateTime.Now;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "Invalid ValidFrom;";
                        }
                    }

                    #endregion ValidFrom

                    #region ValidTo [DATETIME] NULL

                    if (columnNames.Contains("validto"))
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowFlightBudgetType["ValidTo"] = DateTime.FromOADate(Convert.ToDouble(dataTableFlightBudgetExcelData.Rows[i]["validto"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableFlightBudgetExcelData.Rows[i]["validto"].ToString().Trim(), out tempDate))
                                {
                                    dataRowFlightBudgetType["ValidTo"] = tempDate;
                                }
                                else
                                {
                                    dataRowFlightBudgetType["ValidTo"] = DateTime.Now;
                                    validationDetailsFlightBudget = validationDetailsFlightBudget + "Invalid ValidTo;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowFlightBudgetType["ValidTo"] = DateTime.Now;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "Invalid ValidTo;";
                        }
                    }

                    #endregion ValidTo

                    #region IsActive [BIT] NULL

                    if (columnNames.Contains("isactive"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["isactive"].ToString().Trim().ToLower().Equals("y"))
                        {
                            dataRowFlightBudgetType["IsActive"] = 1;
                        }
                        else
                        {
                            dataRowFlightBudgetType["IsActive"] = 0;
                        }
                    }

                    #endregion IsActive

                    #region FlightDate [DATETIME] NULL

                    if (columnNames.Contains("flightdate"))
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowFlightBudgetType["FlightDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableFlightBudgetExcelData.Rows[i]["flightdate"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableFlightBudgetExcelData.Rows[i]["flightdate"].ToString().Trim(), out tempDate))
                                {
                                    dataRowFlightBudgetType["FlightDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowFlightBudgetType["FlightDate"] = DateTime.Now;
                                    validationDetailsFlightBudget = validationDetailsFlightBudget + "Invalid FlightDate;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowFlightBudgetType["FlightDate"] = DateTime.Now;
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "Invalid FlightDate;";
                        }
                    }
                    #endregion FlightDate

                    #region FlightBudgetOrigin [VARCHAR](3) NULL

                    if (columnNames.Contains("flightbudgetorigin"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["flightbudgetorigin"].ToString().Trim().Trim(',').Length > 3)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "FlightBudgetOrigin is more than 3 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["FlightBudgetOrigin"] = dataTableFlightBudgetExcelData.Rows[i]["flightbudgetorigin"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion FlightBudgetOrigin

                    #region FlightBudgetDestination [VARCHAR](3) NULL

                    if (columnNames.Contains("flightbudgetdestination"))
                    {
                        if (dataTableFlightBudgetExcelData.Rows[i]["flightbudgetdestination"].ToString().Trim().Trim(',').Length > 3)
                        {
                            validationDetailsFlightBudget = validationDetailsFlightBudget + "FlightBudgetDestination is more than 3 Chars;";
                        }
                        else
                        {
                            dataRowFlightBudgetType["FlightBudgetDestination"] = dataTableFlightBudgetExcelData.Rows[i]["flightbudgetdestination"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion FlightBudgetDestination

                    #region TargetUOM [tinyINT] NULL

                    dataRowFlightBudgetType["TargetUOM"] = DBNull.Value;

                    #endregion TargetUOM

                    #region ValidationDetailsFlightBudget [VARCHAR](MAX) NULL

                    dataRowFlightBudgetType["ValidationDetailsFlightBudget"] = validationDetailsFlightBudget;

                    #endregion ValidationDetailsFlightBudget

                    FlightBudgetType.Rows.Add(dataRowFlightBudgetType);

                    #endregion Create row for FlightBudgetType Data Table
                }

                // Database Call to Validate & Insert/Update Flight Budget
                string errorInSp = string.Empty;
                DataSet? dataSetResult = new DataSet();
                dataSetResult = await ValidateAndInsertUpdateFlightBudgetMaster(srNotblMasterUploadSummaryLog, FlightBudgetType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", exception.Message, exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableFlightBudgetExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting FlightBudget Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="agentType"> Flight Budget Master Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet?> ValidateAndInsertUpdateFlightBudgetMaster(int srNotblMasterUploadSummaryLog, DataTable flightBudgetType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@FightBudgetTableType", flightBudgetType),
                    new SqlParameter("@Error", errorInSp)
                ];

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspUploadFlightBudgetMaster", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspUploadFlightBudgetMaster", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", exception.Message, exception.StackTrace);
                return dataSetResult;
            }
        }

    }
}
