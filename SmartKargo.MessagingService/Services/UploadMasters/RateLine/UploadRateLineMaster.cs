using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.RateLine
{
    /// <summary>
    /// Class to Upload Rate Line Master File.
    /// </summary>
    public class UploadRateLineMaster
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadRateLineMaster> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadRateLineMaster(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadRateLineMaster> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;

        }
        #endregion



        /// <summary>
        /// Method to Uplaod Rate Line Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<bool> RateLineMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.RateLine);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "RateLineMasterUploadFile", out uploadFilePath))
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath, Convert.ToString(dataRowFileData["ContainerName"]));
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
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to Process Rate Line Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Rate Line Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath, string ContainerName)
        {
            DataTable dataTableRateLineExcelData = new DataTable("dataTableRateLineExcelData");

            bool isBinaryReader = false;


            try
            {
                if (ContainerName.Trim().ToLower() != "approvedrateswebapi")
                {
                    FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                    IExcelDataReader iExcelDataReader = null;
                    string fileExtention = Path.GetExtension(filepath).ToLower();

                    isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") ? true : false;

                    iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                      : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                    // DataSet - Create column names from first row
                    iExcelDataReader.IsFirstRowAsColumnNames = true;
                    dataTableRateLineExcelData = iExcelDataReader.AsDataSet().Tables[0];

                    // Free resources (IExcelDataReader is IDisposable)
                    iExcelDataReader.Close();
                }
                else////***************************////
                {
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filepath, false))
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                        IEnumerable<Row> rows = sheetData.Descendants<Row>();
                        int columnCount = 0;
                        foreach (Cell cell in rows.ElementAt(0))
                        {
                            if (cell.InnerText.Trim() != "")
                            {
                                columnCount++;
                                dataTableRateLineExcelData.Columns.Add(cell.InnerText.Trim(), System.Type.GetType("System.String"));
                            }
                        }
                        int count = -1;
                        foreach (Row r in sheetData.Elements<Row>())
                        {
                            if (count >= 0)
                            {
                                DataRow row = dataTableRateLineExcelData.NewRow();
                                for (int i = 0; i < columnCount; i++)
                                {

                                    row[i] = r.ChildElements[i].InnerText;
                                }
                                dataTableRateLineExcelData.Rows.Add(row);
                            }
                            count++;
                        }
                    }
                }
                ////***************************////

                _uploadMasterCommon.RemoveEmptyRows(dataTableRateLineExcelData);

                foreach (DataColumn dataColumn in dataTableRateLineExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                #region Creating RateLineType DataTable

                DataTable RateLineType = new DataTable("RateLineType");
                RateLineType.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RateLineType.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RateLineType.Columns.Add("RateCardType", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("RateLineNum", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("OriginLevel", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("Origin", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("DestinationLevel", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("Destination", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("ContrRef", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("Currency", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("Status", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("StartDate", System.Type.GetType("System.DateTime"));
                RateLineType.Columns.Add("EndDate", System.Type.GetType("System.DateTime"));
                RateLineType.Columns.Add("RateBase", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("AgentCommPercent", System.Type.GetType("System.Decimal"));
                RateLineType.Columns.Add("MaxDiscountPercent", System.Type.GetType("System.Decimal"));
                RateLineType.Columns.Add("ServiceTax", System.Type.GetType("System.Decimal"));
                RateLineType.Columns.Add("TDSPercent", System.Type.GetType("System.Decimal"));
                RateLineType.Columns.Add("IsALLIn", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("isTact", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("IsULD", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("IsHeavy", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("isSpecial", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("GLCode", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("RateType", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("UOM", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("IsWtSlab", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("IsPrime", System.Type.GetType("System.Byte"));
                RateLineType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RateLineType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                RateLineType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));

                #endregion

                #region Creating RateLineRemarkType DataTable

                DataTable RateLineRemarkType = new DataTable("RateLineRemarkType");
                RateLineRemarkType.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RateLineRemarkType.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RateLineRemarkType.Columns.Add("Comments", System.Type.GetType("System.String"));
                RateLineRemarkType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RateLineRemarkType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                #region Creating RateLineParamType DataTable

                DataTable RateLineParamType = new DataTable("RateLineParamType");
                RateLineParamType.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RateLineParamType.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RateLineParamType.Columns.Add("ParamName", System.Type.GetType("System.String"));
                RateLineParamType.Columns.Add("ParamValue", System.Type.GetType("System.String"));
                RateLineParamType.Columns.Add("IsInclude", System.Type.GetType("System.Byte"));
                RateLineParamType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RateLineParamType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                #region Creating RateLineSlabType DataTable

                DataTable RateLineSlabType = new DataTable("RateLineSlabType");
                RateLineSlabType.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RateLineSlabType.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RateLineSlabType.Columns.Add("SlabName", System.Type.GetType("System.String"));
                RateLineSlabType.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                RateLineSlabType.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                RateLineSlabType.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                RateLineSlabType.Columns.Add("Basedon", System.Type.GetType("System.String"));
                RateLineSlabType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RateLineSlabType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                DataTable RateLineSlabTypeTemp = new DataTable("RateLineSlabTypeTemp");
                RateLineSlabTypeTemp.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RateLineSlabTypeTemp.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RateLineSlabTypeTemp.Columns.Add("SlabName", System.Type.GetType("System.String"));
                RateLineSlabTypeTemp.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                RateLineSlabTypeTemp.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                RateLineSlabTypeTemp.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                RateLineSlabTypeTemp.Columns.Add("Basedon", System.Type.GetType("System.String"));
                RateLineSlabTypeTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RateLineSlabTypeTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                #region Creating RatelineULDSlabType DataTable

                DataTable RatelineULDSlabType = new DataTable("RatelineULDSlabType");
                RatelineULDSlabType.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RatelineULDSlabType.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RatelineULDSlabType.Columns.Add("ULDType", System.Type.GetType("System.String"));
                RatelineULDSlabType.Columns.Add("SlabName", System.Type.GetType("System.String"));
                RatelineULDSlabType.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                RatelineULDSlabType.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                RatelineULDSlabType.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                RatelineULDSlabType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RatelineULDSlabType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                DataTable RatelineULDSlabTypeTemp = new DataTable("RatelineULDSlabTypeTemp");
                RatelineULDSlabTypeTemp.Columns.Add("RateIndex", System.Type.GetType("System.Int32"));
                RatelineULDSlabTypeTemp.Columns.Add("RateLineSrNo", System.Type.GetType("System.Int32"));
                RatelineULDSlabTypeTemp.Columns.Add("ULDType", System.Type.GetType("System.String"));
                RatelineULDSlabTypeTemp.Columns.Add("SlabName", System.Type.GetType("System.String"));
                RatelineULDSlabTypeTemp.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                RatelineULDSlabTypeTemp.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                RatelineULDSlabTypeTemp.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                RatelineULDSlabTypeTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                RatelineULDSlabTypeTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                int rateLineSrNo = 0;
                string validationDetailsRateLine = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;
                string uLDType = string.Empty;
                decimal tempDecimalWtValue = 0;
                decimal tempDecimalChargValue = 0;

                int columnNameStart;
                string[] strArrColName;

                for (int i = 0; i < dataTableRateLineExcelData.Rows.Count; i++)
                {
                    rateLineSrNo = 0;
                    validationDetailsRateLine = string.Empty;
                    tempDecimalValue = 0;
                    uLDType = string.Empty;
                    tempDecimalWtValue = 0;
                    tempDecimalChargValue = 0;

                    #region Create row for RateLineType Data Table

                    DataRow dataRowRateLineType = RateLineType.NewRow();

                    dataRowRateLineType["RateIndex"] = i + 1;

                    #region RateLineSrNo

                    if (dataTableRateLineExcelData.Rows[i]["rateid"] == null)
                    {
                        dataRowRateLineType["RateLineSrNo"] = rateLineSrNo;
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["rateid"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["RateLineSrNo"] = rateLineSrNo;
                    }
                    else
                    {
                        if (int.TryParse(dataTableRateLineExcelData.Rows[i]["rateid"].ToString().Trim().Trim(','), out rateLineSrNo))
                        {
                            dataRowRateLineType["RateLineSrNo"] = rateLineSrNo;
                        }
                        else
                        {
                            dataRowRateLineType["RateLineSrNo"] = rateLineSrNo;
                            validationDetailsRateLine = validationDetailsRateLine + "Invalid RateId Value;";
                        }
                    }

                    #endregion

                    #region RateCardType

                    if (dataTableRateLineExcelData.Rows[i]["ratecardtype"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Rate Card not found;";
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["ratecardtype"].ToString().Trim().Trim(',').Length > 20)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "RateCardType is more than 20 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["RateCardType"] = dataTableRateLineExcelData.Rows[i]["ratecardtype"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    dataRowRateLineType["RateLineNum"] = DBNull.Value;

                    #region OriginLevel

                    if (dataTableRateLineExcelData.Rows[i]["originlevel"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["OriginLevel"] = string.Empty;
                    }
                    else
                    {
                        dataRowRateLineType["OriginLevel"] = dataTableRateLineExcelData.Rows[i]["originlevel"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region Origin

                    if (dataTableRateLineExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Origin not found;";
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 20)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Origin is more than 20 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Origin"] = dataTableRateLineExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region DestinationLeve

                    if (dataTableRateLineExcelData.Rows[i]["destinationlevel"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["DestinationLevel"] = string.Empty;
                    }
                    else
                    {
                        dataRowRateLineType["DestinationLevel"] = dataTableRateLineExcelData.Rows[i]["destinationlevel"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region Destination

                    if (dataTableRateLineExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Destination not found;";
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 20)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Destination is more than 20 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Destination"] = dataTableRateLineExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region ContrRef

                    if (dataTableRateLineExcelData.Rows[i]["contr ref"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["ContrRef"] = DBNull.Value;
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["contr ref"].ToString().Trim().Trim(',').Length > 30)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Contr Ref is more than 30 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["ContrRef"] = dataTableRateLineExcelData.Rows[i]["contr ref"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region Currency

                    if (dataTableRateLineExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Currency not found;";
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Currency is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Currency"] = dataTableRateLineExcelData.Rows[i]["currency"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region Status

                    if (dataTableRateLineExcelData.Rows[i]["status"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Status is more than 10 Chars;";
                    }
                    else
                    {
                        switch (dataTableRateLineExcelData.Rows[i]["status"].ToString().Trim().Trim(',').ToUpper())
                        {
                            case "ACTIVE":
                                dataRowRateLineType["Status"] = "ACT";
                                break;
                            case "INACTIVE":
                                dataRowRateLineType["Status"] = "INA";
                                break;
                            case "DRAFT":
                                dataRowRateLineType["Status"] = "DRF";
                                break;
                            default:
                                dataRowRateLineType["Status"] = string.Empty;
                                validationDetailsRateLine = validationDetailsRateLine + "Invalid Status;";
                                break;
                        }
                    }

                    #endregion

                    #region StartDate

                    if (dataTableRateLineExcelData.Rows[i]["validfrom"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "ValidFrom not found;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowRateLineType["StartDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableRateLineExcelData.Rows[i]["validfrom"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableRateLineExcelData.Rows[i]["validfrom"].ToString().Trim(), out tempDate))
                                {
                                    dataRowRateLineType["StartDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowRateLineType["StartDate"] = DateTime.Now;
                                    validationDetailsRateLine = validationDetailsRateLine + "Invalid ValidFrom;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowRateLineType["StartDate"] = DateTime.Now;
                            validationDetailsRateLine = validationDetailsRateLine + "Invalid ValidFrom;";
                        }

                    }

                    #endregion

                    #region EndDate

                    if (dataTableRateLineExcelData.Rows[i]["validto"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "ValidTo not found;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowRateLineType["EndDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableRateLineExcelData.Rows[i]["validto"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableRateLineExcelData.Rows[i]["validto"].ToString().Trim(), out tempDate))
                                {
                                    dataRowRateLineType["EndDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowRateLineType["EndDate"] = DateTime.Now;
                                    validationDetailsRateLine = validationDetailsRateLine + "Invalid ValidTo;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowRateLineType["EndDate"] = DateTime.Now;
                            validationDetailsRateLine = validationDetailsRateLine + "Invalid ValidTo;";
                        }

                    }

                    #endregion

                    #region RateBase

                    if (dataTableRateLineExcelData.Rows[i]["ratebase"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "RateBase not found;";
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["ratebase"].ToString().Trim().Trim(',').Length > 3)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "RateBase is more than 3 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["RateBase"] = dataTableRateLineExcelData.Rows[i]["ratebase"].ToString().Trim().Trim(',').ToUpper();
                    }

                    #endregion

                    #region AgentCommPercent

                    if (dataTableRateLineExcelData.Rows[i]["agentcommission"] == null)
                    {
                        dataRowRateLineType["AgentCommPercent"] = tempDecimalValue;
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["agentcommission"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["AgentCommPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableRateLineExcelData.Rows[i]["agentcommission"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowRateLineType["AgentCommPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowRateLineType["AgentCommPercent"] = tempDecimalValue;
                            validationDetailsRateLine = validationDetailsRateLine + "Invalid AgentCommission;";
                        }
                    }

                    #endregion

                    #region MaxDiscountPercent

                    if (dataTableRateLineExcelData.Rows[i]["discount"] == null)
                    {
                        dataRowRateLineType["MaxDiscountPercent"] = tempDecimalValue;
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["discount"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["MaxDiscountPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableRateLineExcelData.Rows[i]["discount"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowRateLineType["MaxDiscountPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowRateLineType["MaxDiscountPercent"] = tempDecimalValue;
                            validationDetailsRateLine = validationDetailsRateLine + "Invalid Discount;";
                        }
                    }

                    #endregion

                    #region ServiceTax

                    if (dataTableRateLineExcelData.Rows[i]["tax%"] == null)
                    {
                        dataRowRateLineType["ServiceTax"] = tempDecimalValue;
                    }
                    else if (dataTableRateLineExcelData.Rows[i]["tax%"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["ServiceTax"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableRateLineExcelData.Rows[i]["tax%"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowRateLineType["ServiceTax"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowRateLineType["ServiceTax"] = tempDecimalValue;
                            validationDetailsRateLine = validationDetailsRateLine + "Invalid Tax%;";
                        }
                    }

                    #endregion

                    dataRowRateLineType["TDSPercent"] = DBNull.Value;

                    #region IsALLIn

                    switch (dataTableRateLineExcelData.Rows[i]["allinrateflag"].ToString().Trim().Trim(',').ToUpper())
                    {
                        case "Y":
                            dataRowRateLineType["IsALLIn"] = 1;
                            break;
                        case "1":
                            dataRowRateLineType["IsALLIn"] = 1;
                            break;
                        default:
                            dataRowRateLineType["IsALLIn"] = 0;
                            break;
                    }

                    #endregion

                    #region isTact

                    switch (dataTableRateLineExcelData.Rows[i]["tactflag"].ToString().Trim().Trim(',').ToUpper())
                    {
                        case "Y":
                            dataRowRateLineType["isTact"] = 1;
                            break;
                        case "1":
                            dataRowRateLineType["isTact"] = 1;
                            break;
                        default:
                            dataRowRateLineType["isTact"] = 0;
                            break;
                    }

                    #endregion

                    // Update this flag after Reading ULD Rate Slabs
                    dataRowRateLineType["IsULD"] = 0;

                    #region IsHeavy

                    switch (dataTableRateLineExcelData.Rows[i]["heavyflag"].ToString().Trim().Trim(',').ToUpper())
                    {
                        case "Y":
                            dataRowRateLineType["IsHeavy"] = 1;
                            break;
                        case "1":
                            dataRowRateLineType["IsHeavy"] = 1;
                            break;
                        default:
                            dataRowRateLineType["IsHeavy"] = 0;
                            break;
                    }

                    #endregion

                    dataRowRateLineType["isSpecial"] = 0;

                    #region GLCode

                    if (dataTableRateLineExcelData.Rows[i]["glaccount"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "GLAccount is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["GLCode"] = dataTableRateLineExcelData.Rows[i]["glaccount"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region RateType

                    switch (dataTableRateLineExcelData.Rows[i]["ratetype"].ToString().Trim().Trim(',').ToUpper())
                    {
                        case "DOMESTIC":
                            dataRowRateLineType["RateType"] = "DOM";
                            break;
                        case "INTERNATIONAL":
                            dataRowRateLineType["RateType"] = "INT";
                            break;
                        default:
                            dataRowRateLineType["RateType"] = string.Empty;
                            break;
                    }

                    #endregion

                    #region UOM

                    if (dataTableRateLineExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Length > 3)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "UOM is more than 3 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["UOM"] = dataTableRateLineExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').ToUpper();
                    }

                    #endregion

                    // Update this flag after Reading Rate Slabs
                    dataRowRateLineType["IsWtSlab"] = 0;

                    #region IsPrime

                    switch (dataTableRateLineExcelData.Rows[i]["primeflag"].ToString().Trim().Trim(',').ToUpper())
                    {
                        case "Y":
                            dataRowRateLineType["IsPrime"] = 1;
                            break;
                        case "1":
                            dataRowRateLineType["IsPrime"] = 1;
                            break;
                        default:
                            dataRowRateLineType["IsPrime"] = 0;
                            break;
                    }

                    #endregion

                    dataRowRateLineType["UpdatedBy"] = string.Empty;
                    dataRowRateLineType["UpdatedOn"] = DateTime.Now;

                    // will be updated at the end of all validations.
                    dataRowRateLineType["ValidationDetails"] = string.Empty;

                    #endregion

                    #region Create row for RateLineRemarkType Data Table

                    DataRow dataRowRateLineRemarkType = RateLineRemarkType.NewRow();

                    if (dataTableRateLineExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Length > 250)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Remarks is more than 250 Chars;";
                    }
                    else
                    {
                        dataRowRateLineRemarkType["RateIndex"] = i + 1;
                        dataRowRateLineRemarkType["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineRemarkType["Comments"] = dataTableRateLineExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').ToUpper();
                        dataRowRateLineRemarkType["UpdatedBy"] = string.Empty;
                        dataRowRateLineRemarkType["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region Create rows for RateLineParamType Data Table

                    #region CommodityCode

                    DataRow dataRowRateLineParamTypeCommCode = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["commoditycode"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "CommodityCode is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeCommCode["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeCommCode["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeCommCode["ParamName"] = "CommCode";
                        dataRowRateLineParamTypeCommCode["ParamValue"] = dataTableRateLineExcelData.Rows[i]["commoditycode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeCommCode["IsInclude"] = dataTableRateLineExcelData.Rows[i]["iecommcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeCommCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeCommCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region ProductType

                    DataRow dataRowRateLineParamTypeProductType = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "ProductType is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeProductType["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeProductType["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeProductType["ParamName"] = "ProductType";
                        dataRowRateLineParamTypeProductType["ParamValue"] = dataTableRateLineExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeProductType["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieproducttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeProductType["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeProductType["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region Flight#

                    DataRow dataRowRateLineParamTypeFlight = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["flight#"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Flight# is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeFlight["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeFlight["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeFlight["ParamName"] = "FlightNum";
                        dataRowRateLineParamTypeFlight["ParamValue"] = dataTableRateLineExcelData.Rows[i]["flight#"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeFlight["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieflight#"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeFlight["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeFlight["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region AgentCode

                    DataRow dataRowRateLineParamTypeAgentCode = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "AgentCode is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeAgentCode["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeAgentCode["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeAgentCode["ParamName"] = "AgentCode";
                        dataRowRateLineParamTypeAgentCode["ParamValue"] = dataTableRateLineExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeAgentCode["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieagentcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeAgentCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeAgentCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region IssueCarrier

                    DataRow dataRowRateLineParamTypeIssueCarrier = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "IssueCarrier is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeIssueCarrier["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeIssueCarrier["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeIssueCarrier["ParamName"] = "IssueingCarrier";
                        dataRowRateLineParamTypeIssueCarrier["ParamValue"] = dataTableRateLineExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeIssueCarrier["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieissuecarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeIssueCarrier["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeIssueCarrier["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region FlightCarrier

                    DataRow dataRowRateLineParamTypeFlightCarrier = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "FlightCarrier is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeFlightCarrier["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeFlightCarrier["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeFlightCarrier["ParamName"] = "FlightCarrier";
                        dataRowRateLineParamTypeFlightCarrier["ParamValue"] = dataTableRateLineExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeFlightCarrier["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieflightcarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeFlightCarrier["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeFlightCarrier["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region SHC

                    DataRow dataRowRateLineParamTypeSHC = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["shc"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "SHC is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeSHC["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeSHC["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeSHC["ParamName"] = "HandlingCode";
                        dataRowRateLineParamTypeSHC["ParamValue"] = dataTableRateLineExcelData.Rows[i]["shc"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeSHC["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieshc"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeSHC["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeSHC["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region DaysOfWeek

                    DataRow dataRowRateLineParamTypeDaysOfWeek = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "DaysOfWeek is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeDaysOfWeek["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeDaysOfWeek["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeDaysOfWeek["ParamName"] = "DaysOfWeek";
                        dataRowRateLineParamTypeDaysOfWeek["ParamValue"] = dataTableRateLineExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeDaysOfWeek["IsInclude"] = dataTableRateLineExcelData.Rows[i]["iedaysofweek"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeDaysOfWeek["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeDaysOfWeek["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region DepartureIntervalFromTo

                    DataRow dataRowRateLineParamTypeDepartureIntervalFromTo = RateLineParamType.NewRow();
                    if ((dataTableRateLineExcelData.Rows[i]["departureintervalfrom"].ToString().Trim().Trim(',').Length +
                         dataTableRateLineExcelData.Rows[i]["departureintervalto"].ToString().Trim().Trim(',').Length) > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "DepartureIntervalFrom - DepartureIntervalTo is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeDepartureIntervalFromTo["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeDepartureIntervalFromTo["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeDepartureIntervalFromTo["ParamName"] = "DepInterval";
                        if (dataTableRateLineExcelData.Rows[i]["departureintervalfrom"].ToString().Trim().Trim(',').Equals(string.Empty) &&
                           dataTableRateLineExcelData.Rows[i]["departureintervalto"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowRateLineParamTypeDepartureIntervalFromTo["ParamValue"] = string.Empty;
                        }
                        else
                        {
                            dataRowRateLineParamTypeDepartureIntervalFromTo["ParamValue"] = dataTableRateLineExcelData.Rows[i]["departureintervalfrom"].ToString().Trim().Trim(',') + "-" + dataTableRateLineExcelData.Rows[i]["departureintervalto"].ToString().Trim().Trim(',');
                        }
                        dataRowRateLineParamTypeDepartureIntervalFromTo["IsInclude"] = dataTableRateLineExcelData.Rows[i]["iedepinterval"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeDepartureIntervalFromTo["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeDepartureIntervalFromTo["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region SPAMarkup%

                    DataRow dataRowRateLineParamTypeSPAMarkup = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["spamarkup%"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "SPAMarkup% is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeSPAMarkup["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeSPAMarkup["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeSPAMarkup["ParamName"] = "SPAMarkup";
                        dataRowRateLineParamTypeSPAMarkup["ParamValue"] = dataTableRateLineExcelData.Rows[i]["spamarkup%"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeSPAMarkup["IsInclude"] = 1;
                        dataRowRateLineParamTypeSPAMarkup["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeSPAMarkup["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region EquipmentType

                    DataRow dataRowRateLineParamTypeEquipmentType = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "EquipmentType is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeEquipmentType["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeEquipmentType["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeEquipmentType["ParamName"] = "Equipment Type";
                        dataRowRateLineParamTypeEquipmentType["ParamValue"] = dataTableRateLineExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeEquipmentType["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieequipmenttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeEquipmentType["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeEquipmentType["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region TransitStation

                    DataRow dataRowRateLineParamTypeTransitStation = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "TransitStation is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeTransitStation["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeTransitStation["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeTransitStation["ParamName"] = "TransitStation";
                        dataRowRateLineParamTypeTransitStation["ParamValue"] = dataTableRateLineExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeTransitStation["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ietransitstation"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeTransitStation["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeTransitStation["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region ShipperCode

                    DataRow dataRowRateLineParamTypeShipperCode = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "ShipperCode is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeShipperCode["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeShipperCode["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeShipperCode["ParamName"] = "ShipperCode";
                        dataRowRateLineParamTypeShipperCode["ParamValue"] = dataTableRateLineExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeShipperCode["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieshippercode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeShipperCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeShipperCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region BillingAgentCode

                    DataRow dataRowRateLineParamTypeBillingAgentCode = RateLineParamType.NewRow();
                    if (dataTableRateLineExcelData.Rows[i]["BillingAgentCode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetailsRateLine = validationDetailsRateLine + "Billing Agent Code is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeBillingAgentCode["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeBillingAgentCode["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeBillingAgentCode["ParamName"] = "BillingAgentCode";
                        dataRowRateLineParamTypeBillingAgentCode["ParamValue"] = dataTableRateLineExcelData.Rows[i]["billingagentcode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeBillingAgentCode["IsInclude"] = dataTableRateLineExcelData.Rows[i]["iebillingagentcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeBillingAgentCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeBillingAgentCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion
                    int DefaultIndex = 51;
                    DataRow dataRowRateLineParamTypeFlightType = RateLineParamType.NewRow();

                    if (dataTableRateLineExcelData.Columns.Contains("FlightType") && dataTableRateLineExcelData.Columns.Contains("ieFlightType"))
                    {
                        #region FlightType
                        DefaultIndex = 53;
                        dataRowRateLineParamTypeFlightType["RateIndex"] = i + 1;
                        dataRowRateLineParamTypeFlightType["RateLineSrNo"] = rateLineSrNo;
                        dataRowRateLineParamTypeFlightType["ParamName"] = "FlightType";
                        dataRowRateLineParamTypeFlightType["ParamValue"] = dataTableRateLineExcelData.Rows[i]["FlightType"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeFlightType["IsInclude"] = dataTableRateLineExcelData.Rows[i]["ieFlightType"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeFlightType["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeFlightType["UpdatedOn"] = DateTime.Now;

                        #endregion
                    }
                    #endregion

                    RateLineSlabTypeTemp.Rows.Clear();
                    RatelineULDSlabTypeTemp.Rows.Clear();

                    for (int j = DefaultIndex; j < dataTableRateLineExcelData.Columns.Count; j++)
                    {
                        DataColumn dataColumn = dataTableRateLineExcelData.Columns[j];

                        DataRow dataRowRateLineSlabTypeTemp = RateLineSlabTypeTemp.NewRow();
                        DataRow dataRowRatelineULDSlabTypeTemp = RatelineULDSlabTypeTemp.NewRow();

                        #region MIN Slab

                        if (dataColumn.ColumnName.Equals("min"))
                        {
                            if (dataRowRateLineType["RateBase"].Equals("WB") || dataRowRateLineType["RateBase"].Equals("WP") || dataRowRateLineType["RateBase"].Equals("FC") || dataRowRateLineType["RateBase"].Equals("PB") || dataRowRateLineType["RateBase"].Equals("RC") || dataRowRateLineType["RateBase"].Equals("WK") || dataRowRateLineType["RateBase"].Equals("WS"))
                            {
                                tempDecimalValue = 0;
                                if (!dataTableRateLineExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                {
                                    if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                    {
                                        if (tempDecimalValue >= 0)
                                        {
                                            dataRowRateLineSlabTypeTemp["RateIndex"] = i + 1;
                                            dataRowRateLineSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                            dataRowRateLineSlabTypeTemp["SlabName"] = "M";
                                            dataRowRateLineSlabTypeTemp["Weight"] = 0;
                                            dataRowRateLineSlabTypeTemp["Charge"] = tempDecimalValue;
                                            dataRowRateLineSlabTypeTemp["Cost"] = 0;
                                            dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                            dataRowRateLineSlabTypeTemp["UpdatedBy"] = string.Empty;
                                            dataRowRateLineSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                            if (j < dataTableRateLineExcelData.Columns.Count - 1)
                                            {
                                                if (dataTableRateLineExcelData.Columns[j + 1].ColumnName.Equals("min_basedon"))
                                                {
                                                    switch (dataTableRateLineExcelData.Rows[i][j + 1].ToString().ToUpper().Trim())
                                                    {
                                                        case "IATA":
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "I";
                                                            break;
                                                        case "MKT":
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "M";
                                                            break;
                                                        default:
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                                            break;
                                                    }

                                                    j = j + 1;
                                                }
                                            }

                                            if (tempDecimalValue > 0)
                                            {
                                                dataRowRateLineType["IsWtSlab"] = 1;
                                            }

                                            RateLineSlabTypeTemp.Rows.Add(dataRowRateLineSlabTypeTemp);
                                            dataRowRateLineSlabTypeTemp = RateLineSlabTypeTemp.NewRow();
                                        }
                                    }
                                    else
                                    {
                                        validationDetailsRateLine = validationDetailsRateLine + "Invalid MIN Value.;";
                                    }
                                }
                            }
                        }

                        #endregion
                        #region FLAT (F) CHARGE

                        else if (dataColumn.ColumnName.Equals("f"))
                        {
                            if (dataRowRateLineType["RateBase"].Equals("WB") || dataRowRateLineType["RateBase"].Equals("WP") || dataRowRateLineType["RateBase"].Equals("FC") || dataRowRateLineType["RateBase"].Equals("PB") || dataRowRateLineType["RateBase"].Equals("RC") || dataRowRateLineType["RateBase"].Equals("WK") || dataRowRateLineType["RateBase"].Equals("WS"))
                            {
                                tempDecimalValue = 0;
                                if (!dataTableRateLineExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                {
                                    if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                    {
                                        if (tempDecimalValue > 0)
                                        {
                                            dataRowRateLineSlabTypeTemp["RateIndex"] = i + 1;
                                            dataRowRateLineSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                            dataRowRateLineSlabTypeTemp["SlabName"] = "F";
                                            dataRowRateLineSlabTypeTemp["Weight"] = 0;
                                            dataRowRateLineSlabTypeTemp["Charge"] = tempDecimalValue;
                                            dataRowRateLineSlabTypeTemp["Cost"] = 0;
                                            dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                            dataRowRateLineSlabTypeTemp["UpdatedBy"] = string.Empty;
                                            dataRowRateLineSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                            //if (j < dataTableRateLineExcelData.Columns.Count - 1)
                                            //{
                                            //    if (dataTableRateLineExcelData.Columns[j + 1].ColumnName.Equals("min_basedon"))
                                            //    {
                                            //        switch (dataTableRateLineExcelData.Rows[i][j + 1].ToString().ToUpper().Trim())
                                            //        {
                                            //            case "IATA":
                                            //                dataRowRateLineSlabTypeTemp["Basedon"] = "I";
                                            //                break;
                                            //            case "MKT":
                                            //                dataRowRateLineSlabTypeTemp["Basedon"] = "M";
                                            //                break;
                                            //            default:
                                            //                dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                            //                break;
                                            //        }

                                            //        j = j + 1;
                                            //    }
                                            //}

                                            if (tempDecimalValue > 0)
                                            {
                                                dataRowRateLineType["IsWtSlab"] = 1;
                                            }

                                            RateLineSlabTypeTemp.Rows.Add(dataRowRateLineSlabTypeTemp);
                                            dataRowRateLineSlabTypeTemp = RateLineSlabTypeTemp.NewRow();
                                        }
                                    }
                                    else
                                    {
                                        validationDetailsRateLine = validationDetailsRateLine + "Invalid F Value.;";
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Normal Slab

                        else if (dataColumn.ColumnName.Equals("n"))
                        {
                            if (dataRowRateLineType["RateBase"].Equals("WB") || dataRowRateLineType["RateBase"].Equals("WP") || dataRowRateLineType["RateBase"].Equals("FC") || dataRowRateLineType["RateBase"].Equals("PB") || dataRowRateLineType["RateBase"].Equals("RC") || dataRowRateLineType["RateBase"].Equals("WK") || dataRowRateLineType["RateBase"].Equals("WS"))
                            {
                                tempDecimalValue = 0;
                                if (!dataTableRateLineExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                {
                                    if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                    {
                                        if (tempDecimalValue >= 0)
                                        {
                                            dataRowRateLineSlabTypeTemp["RateIndex"] = i + 1;
                                            dataRowRateLineSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                            dataRowRateLineSlabTypeTemp["SlabName"] = "N";
                                            dataRowRateLineSlabTypeTemp["Weight"] = 0;
                                            dataRowRateLineSlabTypeTemp["Charge"] = tempDecimalValue;
                                            dataRowRateLineSlabTypeTemp["Cost"] = 0;
                                            dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                            dataRowRateLineSlabTypeTemp["UpdatedBy"] = string.Empty;
                                            dataRowRateLineSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                            if (j < dataTableRateLineExcelData.Columns.Count - 1)
                                            {
                                                if (dataTableRateLineExcelData.Columns[j + 1].ColumnName.Equals("n_basedon"))
                                                {
                                                    switch (dataTableRateLineExcelData.Rows[i][j + 1].ToString().ToUpper().Trim())
                                                    {
                                                        case "IATA":
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "I";
                                                            break;
                                                        case "MKT":
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "M";
                                                            break;
                                                        default:
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                                            break;
                                                    }

                                                    j = j + 1;
                                                }
                                            }

                                            if (tempDecimalValue > 0)
                                            {
                                                dataRowRateLineType["IsWtSlab"] = 1;
                                            }

                                            RateLineSlabTypeTemp.Rows.Add(dataRowRateLineSlabTypeTemp);
                                            dataRowRateLineSlabTypeTemp = RateLineSlabTypeTemp.NewRow();
                                        }
                                    }
                                    else
                                    {
                                        validationDetailsRateLine = validationDetailsRateLine + "Invalid N Value.;";
                                    }
                                }
                            }
                        }

                        #endregion

                        #region ULD Slabs

                        else if (dataColumn.ColumnName.StartsWith("u_"))
                        {
                            uLDType = string.Empty;
                            uLDType = dataColumn.ColumnName.Replace("u_", "");
                            if (!uLDType.Equals(string.Empty))
                            {
                                uLDType = uLDType.ToUpper();
                                tempDecimalValue = 0;
                                if (!dataTableRateLineExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                {
                                    if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                    {
                                        if (tempDecimalValue >= 0)
                                        {
                                            dataRowRatelineULDSlabTypeTemp["RateIndex"] = i + 1;
                                            dataRowRatelineULDSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                            dataRowRatelineULDSlabTypeTemp["ULDType"] = uLDType;
                                            dataRowRatelineULDSlabTypeTemp["SlabName"] = "M";
                                            dataRowRatelineULDSlabTypeTemp["Weight"] = 0;
                                            dataRowRatelineULDSlabTypeTemp["Charge"] = tempDecimalValue;
                                            dataRowRatelineULDSlabTypeTemp["Cost"] = 0;
                                            dataRowRatelineULDSlabTypeTemp["UpdatedBy"] = string.Empty;
                                            dataRowRatelineULDSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                            RatelineULDSlabTypeTemp.Rows.Add(dataRowRatelineULDSlabTypeTemp);
                                            dataRowRatelineULDSlabTypeTemp = RatelineULDSlabTypeTemp.NewRow();

                                            if (tempDecimalValue > 0)
                                            {
                                                dataRowRateLineType["IsULD"] = 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j].ToString() + ".;";
                                    }
                                }

                                if (j < dataTableRateLineExcelData.Columns.Count - 1)
                                {
                                    if (dataTableRateLineExcelData.Columns[j + 1].ColumnName.StartsWith("ovpwt"))
                                    {
                                        if (!dataTableRateLineExcelData.Rows[i][j + 1].ToString().Trim().Equals(string.Empty) &&
                                            !dataTableRateLineExcelData.Rows[i][j + 2].ToString().Trim().Equals(string.Empty))
                                        {
                                            tempDecimalWtValue = 0;
                                            if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j + 1].ToString().Trim(), out tempDecimalWtValue))
                                            {
                                                tempDecimalChargValue = 0;
                                                if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j + 2].ToString().Trim(), out tempDecimalChargValue))
                                                {
                                                    if (tempDecimalWtValue >= 0 && tempDecimalChargValue >= 0)
                                                    {
                                                        dataRowRatelineULDSlabTypeTemp["RateIndex"] = i + 1;
                                                        dataRowRatelineULDSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                                        dataRowRatelineULDSlabTypeTemp["ULDType"] = uLDType;
                                                        dataRowRatelineULDSlabTypeTemp["SlabName"] = "OverPivot";
                                                        dataRowRatelineULDSlabTypeTemp["Weight"] = tempDecimalWtValue;
                                                        dataRowRatelineULDSlabTypeTemp["Charge"] = tempDecimalChargValue;
                                                        dataRowRatelineULDSlabTypeTemp["Cost"] = 0;
                                                        dataRowRatelineULDSlabTypeTemp["UpdatedBy"] = string.Empty;
                                                        dataRowRatelineULDSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                                        RatelineULDSlabTypeTemp.Rows.Add(dataRowRatelineULDSlabTypeTemp);
                                                        dataRowRatelineULDSlabTypeTemp = RatelineULDSlabTypeTemp.NewRow();

                                                        if (tempDecimalWtValue > 0 && tempDecimalChargValue > 0)
                                                        {
                                                            dataRowRateLineType["IsULD"] = 1;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j + 2].ToString() + ".;";
                                                }
                                            }
                                            else
                                            {
                                                validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j + 1].ToString() + ".;";
                                            }
                                        }

                                        if (j < dataTableRateLineExcelData.Columns.Count - 3)
                                        {
                                            if (dataTableRateLineExcelData.Columns[j + 3].ColumnName.StartsWith("flatwt"))
                                            {
                                                if (!dataTableRateLineExcelData.Rows[i][j + 3].ToString().Trim().Equals(string.Empty) &&
                                                    !dataTableRateLineExcelData.Rows[i][j + 4].ToString().Trim().Equals(string.Empty))
                                                {
                                                    tempDecimalWtValue = 0;
                                                    if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j + 3].ToString().Trim(), out tempDecimalWtValue))
                                                    {
                                                        tempDecimalChargValue = 0;
                                                        if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j + 4].ToString().Trim(), out tempDecimalChargValue))
                                                        {
                                                            if (tempDecimalWtValue >= 0 && tempDecimalChargValue >= 0)
                                                            {
                                                                dataRowRatelineULDSlabTypeTemp["RateIndex"] = i + 1;
                                                                dataRowRatelineULDSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                                                dataRowRatelineULDSlabTypeTemp["ULDType"] = uLDType;
                                                                dataRowRatelineULDSlabTypeTemp["SlabName"] = "F";
                                                                dataRowRatelineULDSlabTypeTemp["Weight"] = tempDecimalWtValue;
                                                                dataRowRatelineULDSlabTypeTemp["Charge"] = tempDecimalChargValue;
                                                                dataRowRatelineULDSlabTypeTemp["Cost"] = 0;
                                                                dataRowRatelineULDSlabTypeTemp["UpdatedBy"] = string.Empty;
                                                                dataRowRatelineULDSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                                                RatelineULDSlabTypeTemp.Rows.Add(dataRowRatelineULDSlabTypeTemp);
                                                                dataRowRatelineULDSlabTypeTemp = RatelineULDSlabTypeTemp.NewRow();

                                                                if (tempDecimalWtValue > 0 && tempDecimalChargValue > 0)
                                                                {
                                                                    dataRowRateLineType["IsULD"] = 1;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j + 4].ToString() + ".;";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j + 3].ToString() + ".;";
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (dataTableRateLineExcelData.Columns[j + 1].ColumnName.StartsWith("flatwt"))
                                    {
                                        if (!dataTableRateLineExcelData.Rows[i][j + 1].ToString().Trim().Equals(string.Empty) &&
                                            !dataTableRateLineExcelData.Rows[i][j + 2].ToString().Trim().Equals(string.Empty))
                                        {
                                            tempDecimalWtValue = 0;
                                            if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j + 1].ToString().Trim(), out tempDecimalWtValue))
                                            {
                                                tempDecimalChargValue = 0;
                                                if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j + 2].ToString().Trim(), out tempDecimalChargValue))
                                                {
                                                    if (tempDecimalWtValue >= 0 && tempDecimalChargValue >= 0)
                                                    {
                                                        dataRowRatelineULDSlabTypeTemp["RateIndex"] = i + 1;
                                                        dataRowRatelineULDSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                                        dataRowRatelineULDSlabTypeTemp["ULDType"] = uLDType;
                                                        dataRowRatelineULDSlabTypeTemp["SlabName"] = "F";
                                                        dataRowRatelineULDSlabTypeTemp["Weight"] = tempDecimalWtValue;
                                                        dataRowRatelineULDSlabTypeTemp["Charge"] = tempDecimalChargValue;
                                                        dataRowRatelineULDSlabTypeTemp["Cost"] = 0;
                                                        dataRowRatelineULDSlabTypeTemp["UpdatedBy"] = string.Empty;
                                                        dataRowRatelineULDSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                                        RatelineULDSlabTypeTemp.Rows.Add(dataRowRatelineULDSlabTypeTemp);
                                                        dataRowRatelineULDSlabTypeTemp = RatelineULDSlabTypeTemp.NewRow();

                                                        if (tempDecimalWtValue > 0 && tempDecimalChargValue > 0)
                                                        {
                                                            dataRowRateLineType["IsULD"] = 1;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j + 2].ToString() + ".;";
                                                }
                                            }
                                            else
                                            {
                                                validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j + 1].ToString() + ".;";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Name: " + dataColumn.ColumnName + ".;";
                            }
                        }

                        #endregion

                        #region Weight Slabs

                        else
                        {
                            if (dataColumn.ColumnName.Contains("_basedon") ||
                                dataColumn.ColumnName.StartsWith("ovpwt") ||
                                dataColumn.ColumnName.StartsWith("ovpcharge") ||
                                dataColumn.ColumnName.StartsWith("flatwt") ||
                                dataColumn.ColumnName.StartsWith("flatcharge"))
                            { }
                            else
                            {
                                strArrColName = dataColumn.ColumnName.Split('_');
                                columnNameStart = 0;
                                if (Int32.TryParse(strArrColName[0], out columnNameStart) || (strArrColName[0].ToString().ToUpper() == "F" && Int32.TryParse(strArrColName[1], out columnNameStart)))
                                {
                                    if ((strArrColName.Length == 1 && columnNameStart > 0) || (strArrColName[0].ToString().ToUpper() == "F" && strArrColName.Length == 2 && columnNameStart > 0))
                                    {
                                        tempDecimalValue = 0;
                                        if (!dataTableRateLineExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                        {
                                            if (decimal.TryParse(dataTableRateLineExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                            {
                                                if (tempDecimalValue > 0)
                                                {
                                                    dataRowRateLineType["IsWtSlab"] = 1;

                                                    dataRowRateLineSlabTypeTemp["RateIndex"] = i + 1;
                                                    dataRowRateLineSlabTypeTemp["RateLineSrNo"] = rateLineSrNo;
                                                    if (strArrColName[0].ToString().ToUpper() == "F")
                                                    {
                                                        dataRowRateLineSlabTypeTemp["SlabName"] = "F";
                                                    }
                                                    else
                                                    {
                                                        switch (dataRowRateLineType["RateBase"].ToString())
                                                        {
                                                            case "FC":
                                                            case "WK":
                                                                dataRowRateLineSlabTypeTemp["SlabName"] = "F";
                                                                break;
                                                            default:
                                                                dataRowRateLineSlabTypeTemp["SlabName"] = "Q";
                                                                break;
                                                        }
                                                    }

                                                    dataRowRateLineSlabTypeTemp["Weight"] = columnNameStart;
                                                    dataRowRateLineSlabTypeTemp["Charge"] = tempDecimalValue;
                                                    dataRowRateLineSlabTypeTemp["Cost"] = 0;

                                                    if (j < dataTableRateLineExcelData.Columns.Count - 1)
                                                    {
                                                        if (dataTableRateLineExcelData.Columns[j + 1].ColumnName.Equals(string.Format("{0}_basedon", columnNameStart)) || dataTableRateLineExcelData.Columns[j + 1].ColumnName.Equals(string.Format("f_{0}_basedon", columnNameStart)))
                                                        {
                                                            switch (dataTableRateLineExcelData.Rows[i][j + 1].ToString().ToUpper().Trim())
                                                            {
                                                                case "IATA":
                                                                    dataRowRateLineSlabTypeTemp["Basedon"] = "I";
                                                                    break;
                                                                case "MKT":
                                                                    dataRowRateLineSlabTypeTemp["Basedon"] = "M";
                                                                    break;
                                                                default:
                                                                    dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                                                    break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dataRowRateLineSlabTypeTemp["Basedon"] = "B";
                                                    }

                                                    dataRowRateLineSlabTypeTemp["UpdatedBy"] = string.Empty;
                                                    dataRowRateLineSlabTypeTemp["UpdatedOn"] = DateTime.Now;

                                                    RateLineSlabTypeTemp.Rows.Add(dataRowRateLineSlabTypeTemp);
                                                    dataRowRateLineSlabTypeTemp = RateLineSlabTypeTemp.NewRow();

                                                    if (tempDecimalValue > 0)
                                                    {
                                                        dataRowRateLineType["IsWtSlab"] = 1;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Value: " + dataTableRateLineExcelData.Rows[i][j].ToString() + ".;";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    validationDetailsRateLine = validationDetailsRateLine + "Invalid Column Name: " + dataColumn.ColumnName + ".;";
                                }
                            }
                        }

                        #endregion

                    }

                    if (validationDetailsRateLine.Equals(string.Empty))
                    {
                        dataRowRateLineType["ValidationDetails"] = string.Empty;

                        RateLineRemarkType.Rows.Add(dataRowRateLineRemarkType);

                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeCommCode);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeProductType);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeFlight);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeAgentCode);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeIssueCarrier);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeFlightCarrier);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeSHC);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeDaysOfWeek);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeDepartureIntervalFromTo);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeSPAMarkup);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeEquipmentType);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeTransitStation);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeShipperCode);
                        RateLineParamType.Rows.Add(dataRowRateLineParamTypeBillingAgentCode);
                        if (dataTableRateLineExcelData.Columns.Contains("FlightType") && dataTableRateLineExcelData.Columns.Contains("ieFlightType"))
                        {
                            RateLineParamType.Rows.Add(dataRowRateLineParamTypeFlightType);
                        }
                        foreach (DataRow dataRowRateLineSlabTypeTemp in RateLineSlabTypeTemp.Rows)
                        {
                            RateLineSlabType.Rows.Add(dataRowRateLineSlabTypeTemp.ItemArray);
                        }

                        foreach (DataRow dataRowRatelineULDSlabTypeTemp in RatelineULDSlabTypeTemp.Rows)
                        {
                            RatelineULDSlabType.Rows.Add(dataRowRatelineULDSlabTypeTemp.ItemArray);
                        }
                    }
                    else
                    {
                        dataRowRateLineType["ValidationDetails"] = validationDetailsRateLine;
                    }

                    RateLineType.Rows.Add(dataRowRateLineType);

                }

                // Database Call to Validate & Insert Rate Line Master
                string errorInSp = string.Empty;
                await ValidateAndInsertRateLineMaster(srNotblMasterUploadSummaryLog, RateLineType,
                                                                               RateLineRemarkType,
                                                                               RateLineParamType,
                                                                               RateLineSlabType,
                                                                               RatelineULDSlabType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableRateLineExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Rate Line Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="dataTableRateLineType"> Rate Line Table Type </param>
        /// <param name="dataTableRateLineRemarkType"> Rate Line Remark Table Type </param>
        /// <param name="dataTableRateLineParamType"> Rate Line Parameter Table Type </param>
        /// <param name="dataTableRateLineSlabType"> Rate Line Slab Table Type </param>
        /// <param name="dataTableRatelineULDSlabType"> Rate Line ULD Slab Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet?> ValidateAndInsertRateLineMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableRateLineType,
                                                                                          DataTable dataTableRateLineRemarkType,
                                                                                          DataTable dataTableRateLineParamType,
                                                                                          DataTable dataTableRateLineSlabType,
                                                                                          DataTable dataTableRatelineULDSlabType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@RateLineTableType", dataTableRateLineType),
                    new SqlParameter("@RateLineRemarkTableType", dataTableRateLineRemarkType),
                    new SqlParameter("@RateLineParamTableType", dataTableRateLineParamType),
                    new SqlParameter("@RateLineSlabTableType", dataTableRateLineSlabType),
                    new SqlParameter("@RatelineULDSlabTableType", dataTableRatelineULDSlabType),
                    new SqlParameter("@Error", errorInSp)
                ];

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspUploadRateLineMaster", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspUploadRateLineMaster", sqlParameters);

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
