using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.AircraftPattern
{
    public class UploadAircraftLoadingPattern
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadAircraftLoadingPattern> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;

        #region Constructor
        public UploadAircraftLoadingPattern(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadAircraftLoadingPattern> logger, 
            Func<UploadMasterCommon> uploadmasterCommonFactory
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommonFactory = uploadmasterCommonFactory;
        }
        #endregion
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        public async Task<Boolean> AircraftPatternUpload()
        {
            try
            {
                DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.AircraftLoadingPattern);
                dataSetFileData = await _uploadMasterCommonFactory().GetUploadedFileData(UploadMasterType.AircraftLoadingPattern);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);
                       await  _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        //if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                        if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "AircraftLoadPattern", out uploadFilePath))
                        {
                            //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                           // uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                            await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} \nStackTrace: {StackTrace}", exception.Message, exception.StackTrace);
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
            DataTable dataTableAircraftLoadingPatternExcelData = new DataTable("dataTableAircraftLoadingPatternData");
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

                dataTableAircraftLoadingPatternExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                //uploadMasterCommon.RemoveEmptyRows(dataTableAircraftLoadingPatternExcelData);
                _uploadMasterCommonFactory().RemoveEmptyRows(dataTableAircraftLoadingPatternExcelData);

                #region Creating AircraftConfig
                DataTable AircraftConfig = new DataTable("AircraftLoadingPattern");
                AircraftConfig.Columns.Add("ReferenceIndex", System.Type.GetType("System.Int32"));
                AircraftConfig.Columns.Add("PatternID", System.Type.GetType("System.Int32"));
                AircraftConfig.Columns.Add("AircraftType", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("EquipmentType", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("PatternName", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("FromDate", System.Type.GetType("System.DateTime"));
                AircraftConfig.Columns.Add("ToDate", System.Type.GetType("System.DateTime"));
                AircraftConfig.Columns.Add("OverPlan", System.Type.GetType("System.Int32"));
                AircraftConfig.Columns.Add("FlightNo", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("BulkWeight", System.Type.GetType("System.Decimal"));
                AircraftConfig.Columns.Add("BulkVolume", System.Type.GetType("System.Decimal"));
                AircraftConfig.Columns.Add("UOM", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("IsDefault", System.Type.GetType("System.Byte"));
                AircraftConfig.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
                AircraftConfig.Columns.Add("LoadingPattern", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("LoadingRestriction", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("CreatedBy", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("CreatedOn", System.Type.GetType("System.DateTime"));
                AircraftConfig.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                AircraftConfig.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                AircraftConfig.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));

                #endregion

                #region Creating AircraftCompartment table
                DataTable AircraftCompartment = new DataTable("AircraftCompartment");
                AircraftCompartment.Columns.Add("ReferenceIndex", System.Type.GetType("System.Int32"));
                AircraftCompartment.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                AircraftCompartment.Columns.Add("PatternID", System.Type.GetType("System.Int32"));
                AircraftCompartment.Columns.Add("AircraftType", System.Type.GetType("System.String"));
                AircraftCompartment.Columns.Add("CompartmentID", System.Type.GetType("System.Int32"));
                AircraftCompartment.Columns.Add("CompartmentName", System.Type.GetType("System.String"));
                AircraftCompartment.Columns.Add("PalletPosition", System.Type.GetType("System.String"));
                AircraftCompartment.Columns.Add("CompartmentPosition", System.Type.GetType("System.String"));
                AircraftCompartment.Columns.Add("IsBulk", System.Type.GetType("System.Byte"));
                #endregion
                string validationDetails = string.Empty;
                DateTime tempDate = DateTime.Now;
                foreach (DataColumn dataColumn in dataTableAircraftLoadingPatternExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }
                string[] columnNames = dataTableAircraftLoadingPatternExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                for (int i = 0; i < dataTableAircraftLoadingPatternExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;
                    string AircraftType = string.Empty;
                    int PatternId = 0;
                    int SerialNumber = 0;

                    #region Create row for Aircraft Loading Pattern Data Table

                    // Maintained for Indexing purpose
                    SerialNumber = i + 1;
                    DataRow dataRowAircraftType = AircraftConfig.NewRow();
                    dataRowAircraftType["ReferenceIndex"] = i + 1;
                    #region[patternid]

                    if (columnNames.Contains("patternid"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["patternid"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["PatternID"] = Convert.ToInt32(dataTableAircraftLoadingPatternExcelData.Rows[i]["patternid"].ToString().Trim().Trim(','));
                            PatternId = Convert.ToInt32(dataTableAircraftLoadingPatternExcelData.Rows[i]["patternid"].ToString().Trim().Trim(','));
                        }
                        else
                        {
                            dataRowAircraftType["PatternID"] = 0;
                        }
                    }

                    #endregion

                    #region[AircraftType]

                    if (columnNames.Contains("aircrafttype"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["aircrafttype"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["AircraftType"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["aircrafttype"].ToString().Trim().Trim(',');
                            AircraftType = dataTableAircraftLoadingPatternExcelData.Rows[i]["aircrafttype"].ToString().Trim().Trim(',');
                        }
                        else
                        {
                            validationDetails = validationDetails + "AircraftType is required;";
                        }
                    }

                    #endregion

                    #region[PatternName]
                    if (columnNames.Contains("patternname"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["patternname"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["PatternName"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["patternname"].ToString().Trim().Trim(',');
                        }
                        else
                        {
                            validationDetails = validationDetails + "PatternName is required;";
                        }
                    }

                    #endregion

                    #region[EquipmentNo]
                    if (columnNames.Contains("equipmentno"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["equipmentno"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["EquipmentType"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["equipmentno"].ToString().Trim().Trim(',');
                        }
                        else
                        {
                            dataRowAircraftType["EquipmentType"] = string.Empty;
                        }
                    }

                    #endregion

                    #region[FromtDt] [datetime] NULL

                    if (columnNames.Contains("fromdate"))
                    {
                        if (dataTableAircraftLoadingPatternExcelData.Rows[i]["fromdate"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["FromDate"] = System.DateTime.Now;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAircraftType["FromDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableAircraftLoadingPatternExcelData.Rows[i]["fromdate"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAircraftLoadingPatternExcelData.Rows[i]["fromdate"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAircraftType["FromDate"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAircraftType["FromDate"] = DBNull.Value;
                                        validationDetails = validationDetails + "Invalid From Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAircraftType["FromDate"] = DBNull.Value;
                                validationDetails = validationDetails + "Invalid From Date;";
                            }
                        }
                    }

                    #endregion

                    #region[ToDate] [datetime] NULL

                    if (columnNames.Contains("todate"))
                    {
                        if (dataTableAircraftLoadingPatternExcelData.Rows[i]["todate"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["ToDate"] = System.DateTime.Now;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAircraftType["ToDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableAircraftLoadingPatternExcelData.Rows[i]["todate"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAircraftLoadingPatternExcelData.Rows[i]["todate"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAircraftType["ToDate"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAircraftType["ToDate"] = DBNull.Value;
                                        validationDetails = validationDetails + "Invalid To Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAircraftType["ToDate"] = DBNull.Value;
                                validationDetails = validationDetails + "Invalid To Date;";
                            }
                        }
                    }

                    #endregion

                    #region[overplan]
                    if (columnNames.Contains("overplan"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["overplan"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["OverPlan"] = Convert.ToInt32(dataTableAircraftLoadingPatternExcelData.Rows[i]["overplan"].ToString().Trim().Trim(','));
                        }
                        else
                        {
                            dataRowAircraftType["OverPlan"] = 0;
                        }
                    }

                    #endregion

                    #region[FlightNo]
                    if (columnNames.Contains("flightno"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["flightno"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["FlightNo"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["flightno"].ToString().Trim().Trim(',');
                        }
                        else
                        {
                            dataRowAircraftType["FlightNo"] = string.Empty;
                        }
                    }

                    #endregion

                    #region[BulkWeight]
                    if (columnNames.Contains("bulkweight"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["bulkweight"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["BulkWeight"] = Convert.ToDecimal((dataTableAircraftLoadingPatternExcelData.Rows[i]["bulkweight"].ToString().Trim().Trim(',')));
                        }
                        else
                        {
                            dataRowAircraftType["BulkWeight"] = 0;
                        }
                    }

                    #endregion

                    #region[BulkVolume]
                    if (columnNames.Contains("bulkvolume"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["bulkvolume"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["BulkVolume"] = Convert.ToDecimal((dataTableAircraftLoadingPatternExcelData.Rows[i]["bulkvolume"].ToString().Trim().Trim(',')));
                        }
                        else
                        {
                            dataRowAircraftType["BulkVolume"] = 0;
                        }
                    }

                    #endregion

                    #region[UOM]
                    if (columnNames.Contains("uom"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["UOM"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["uom"].ToString().Trim().Trim(',');
                        }
                        else
                        {
                            dataRowAircraftType["UOM"] = "K";
                        }
                    }

                    #endregion

                    #region[IsDefault]
                    if (columnNames.Contains("isdefault"))
                    {
                        try
                        {
                            if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["isdefault"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAircraftType["IsDefault"] = Convert.ToBoolean(dataTableAircraftLoadingPatternExcelData.Rows[i]["isdefault"].ToString().Trim().Trim(',').ToLower());
                            }
                            else
                            {
                                dataRowAircraftType["IsDefault"] = false;
                            }
                        }
                        catch (Exception)
                        {
                            dataRowAircraftType["IsDefault"] = false;
                            validationDetails = validationDetails + "Invalid IsDefault";
                        }

                    }

                    #endregion

                    #region[IsActive]
                    if (columnNames.Contains("isactive"))
                    {
                        try
                        {
                            if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["isactive"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAircraftType["IsActive"] = Convert.ToBoolean(dataTableAircraftLoadingPatternExcelData.Rows[i]["isactive"].ToString().Trim().ToLower());
                            }
                            else
                            {
                                dataRowAircraftType["IsActive"] = false;
                            }
                        }
                        catch (Exception)
                        {
                            dataRowAircraftType["IsActive"] = false;
                            validationDetails = validationDetails + "Invalid IsActive";
                        }

                    }

                    #endregion

                    #region[LoadingRestriction]
                    if (columnNames.Contains("loadingrestriction"))
                    {

                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["loadingrestriction"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["LoadingRestriction"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["loadingrestriction"].ToString().Trim();
                        }
                        else
                        {
                            dataRowAircraftType["LoadingRestriction"] = string.Empty;
                        }
                    }

                    #endregion

                    #region[CreatedBy] [varchar] (100)  NULL
                    if (columnNames.Contains("createdby"))
                    {
                        if (!dataTableAircraftLoadingPatternExcelData.Rows[i]["createdby"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["CreatedBy"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["createdby"].ToString().Trim().Trim(',');

                        }
                        else
                        {
                            dataRowAircraftType["CreatedBy"] = string.Empty;

                        }
                    }
                    #endregion

                    #region[CreatedOn] [datetime] NULL

                    if (columnNames.Contains("createdon"))
                    {
                        if (dataTableAircraftLoadingPatternExcelData.Rows[i]["createdon"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["CreatedOn"] = System.DateTime.Now;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAircraftType["CreatedOn"] = DateTime.FromOADate(Convert.ToDouble(dataTableAircraftLoadingPatternExcelData.Rows[i]["createdon"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAircraftLoadingPatternExcelData.Rows[i]["createdon"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAircraftType["CreatedOn"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAircraftType["CreatedOn"] = DBNull.Value;
                                        validationDetails = validationDetails + "Invalid UpdatedOn Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAircraftType["CreatedOn"] = DBNull.Value;
                                validationDetails = validationDetails + "Invalid CreatedOn;";
                            }
                        }
                    }
                    #endregion

                    #region[UpdatedOn] [datetime] NULL

                    if (columnNames.Contains("updatedon"))
                    {
                        if (dataTableAircraftLoadingPatternExcelData.Rows[i]["updatedon"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["UpdatedOn"] = System.DateTime.Now;

                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAircraftType["UpdatedOn"] = DateTime.FromOADate(Convert.ToDouble(dataTableAircraftLoadingPatternExcelData.Rows[i]["updatedon"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAircraftLoadingPatternExcelData.Rows[i]["updatedon"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAircraftType["UpdatedOn"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAircraftType["UpdatedOn"] = DBNull.Value;
                                        validationDetails = validationDetails + "Invalid UpdatedOn Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAircraftType["UpdatedOn"] = DBNull.Value;
                                validationDetails = validationDetails + "Invalid UpdatedOn;";
                            }
                        }
                    }
                    #endregion

                    #region[UpdatedBy] [varchar] (100)  NULL
                    if (columnNames.Contains("updatedby"))
                    {
                        if (dataTableAircraftLoadingPatternExcelData.Rows[i]["updatedby"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAircraftType["UpdatedBy"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableAircraftLoadingPatternExcelData.Rows[i]["updatedby"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAircraftType["UpdatedBy"] = DBNull.Value;
                                validationDetails = validationDetails + "UpdatedBy is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAircraftType["UpdatedBy"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["updatedby"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[LoadingPattern]

                    string LoadingPattern = string.Empty;
                    LoadingPattern = dataTableAircraftLoadingPatternExcelData.Rows[i]["PalletPosition1"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["ContainerPosition1"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["PalletPosition2"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["ContainerPosition2"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["PalletPosition3"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["ContainerPosition3"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["PalletPosition4"].ToString().Trim().Trim(',');
                    LoadingPattern = LoadingPattern + dataTableAircraftLoadingPatternExcelData.Rows[i]["ContainerPosition4"].ToString().Trim().Trim(',');


                    if (!LoadingPattern.Equals(string.Empty))
                    {
                        dataRowAircraftType["LoadingPattern"] = LoadingPattern;
                    }
                    else
                    {
                        dataRowAircraftType["LoadingPattern"] = string.Empty;
                    }


                    #endregion
                    int k = 1;
                    for (int j = 0; j < 4; j++)
                    {

                        try
                        {
                            DataRow dataRowAircraftCompartment = AircraftCompartment.NewRow();

                            dataRowAircraftCompartment["ReferenceIndex"] = i + 1;
                            dataRowAircraftCompartment["SerialNumber"] = 0;
                            dataRowAircraftCompartment["PatternID"] = PatternId;
                            dataRowAircraftCompartment["AircraftType"] = AircraftType;
                            dataRowAircraftCompartment["CompartmentID"] = k;
                            dataRowAircraftCompartment["CompartmentName"] = dataTableAircraftLoadingPatternExcelData.Rows[i]["CompartmentName" + k].ToString();
                            string PalletPosition = dataTableAircraftLoadingPatternExcelData.Rows[i]["PalletPosition" + k].ToString();

                            int checkStartWithNumber = string.IsNullOrEmpty(PalletPosition) == true ? 0 : GetLeadingNumber(PalletPosition);
                            if (checkStartWithNumber < 0)
                            {
                                validationDetails = validationDetails + "Invalid PalletPosition" + k;
                            }
                            dataRowAircraftCompartment["PalletPosition"] = string.IsNullOrWhiteSpace(PalletPosition) == true ? string.Empty : PalletPosition;
                            string ContainerPosition = dataTableAircraftLoadingPatternExcelData.Rows[i]["ContainerPosition" + k].ToString();

                            checkStartWithNumber = string.IsNullOrEmpty(ContainerPosition)==true?0:GetLeadingNumber(ContainerPosition);
                            if (checkStartWithNumber < 0)
                            {
                                validationDetails = validationDetails + "Invalid ContainerPosition" + k;
                            }
                            dataRowAircraftCompartment["CompartmentPosition"] = string.IsNullOrWhiteSpace(ContainerPosition) == true ? string.Empty : ContainerPosition;
                            dataRowAircraftCompartment["IsBulk"] = false;
                            AircraftCompartment.Rows.Add(dataRowAircraftCompartment);
                            k++;



                            //}
                        }
                        catch (Exception ex)
                        {
                            validationDetails = validationDetails + ex.Message;

                        }
                    }
                    dataRowAircraftType["ValidationDetails"] = validationDetails;
                    AircraftConfig.Rows.Add(dataRowAircraftType);

                    #endregion Create row for Aircraft Loading Pattern Data Table


                }

                // Database Call to Validate & Insert Aircraft Loading Pattern
                string errorInSp = string.Empty;
                ValidateAndInsertAircraftLoadingPattern(srNotblMasterUploadSummaryLog, AircraftConfig, AircraftCompartment, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} Stack Trace: {StackTrace}", exception.Message, exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableAircraftLoadingPatternExcelData = null;
            }
        }

        /// <summary>
        /// Method to insert Aircraft Loading Pattern data tables
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"></param>
        /// <param name="dataTableAircraftPattern"></param>
        /// <param name="datatableAircraftCompartment"></param>
        /// <param name="errorInSp"></param>
        /// <returns>Result</returns>
        public async Task<DataSet?> ValidateAndInsertAircraftLoadingPattern(int srNotblMasterUploadSummaryLog, DataTable dataTableAircraftPattern, DataTable datatableAircraftCompartment,
                                                                                              string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                    new SqlParameter("@AircraftLoadingPatternType", dataTableAircraftPattern),
                                                                    new SqlParameter("@AircraftCompartmentType", datatableAircraftCompartment),
                                                                    new SqlParameter("@Error", errorInSp)
                                                                  };



                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspUploadAircraftLoadingPattern", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspUploadAircraftLoadingPattern", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                ///clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} Stack Trace: {StackTrace}", exception.Message, exception.StackTrace);
                return dataSetResult;
            }
        }

        public int GetLeadingNumber(string input)
        {
            char[] chars = input.ToCharArray();
            int lastValid = -1;

            for (int i = 0; i < chars.Length; i++)
            {
                if (Char.IsDigit(chars[i]))
                {
                    lastValid = i;
                }
                else
                {
                    break;
                }
            }

            if (lastValid >= 0)
            {
                return int.Parse(new string(chars, 0, lastValid + 1));
            }
            else
            {
                return -1;
            }
        }
    }
}
