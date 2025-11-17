using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
namespace QidWorkerRole.UploadMasters.Taxline
{
    public class UploadTaxLine
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadTaxLine> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadTaxLine(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadTaxLine> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;
        }
        #endregion
        public async Task<bool> TaxLineMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.TaxLine);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "TaxLineMasterUploadFile", out uploadFilePath))
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
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Message: " + ex.Message + " \nStackTrace: " + ex.StackTrace);
                return false;
            }
        }


        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableTaxLineExcelData = new DataTable("dataTableTaxLineExcelData");

            bool isBinaryReader = false;

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
                dataTableTaxLineExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommon.RemoveEmptyRows(dataTableTaxLineExcelData);

                foreach (DataColumn dataColumn in dataTableTaxLineExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                #region Creating TaxLineType DataTable

                DataTable TaxLineType = new DataTable("TaxLineType");
                TaxLineType.Columns.Add("TaxIndex", System.Type.GetType("System.Int32"));
                TaxLineType.Columns.Add("TAXId", System.Type.GetType("System.Int32"));
                TaxLineType.Columns.Add("TaxCode", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("TaxName", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("TaxType", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("ValidFrom", System.Type.GetType("System.DateTime"));
                TaxLineType.Columns.Add("ValidTo", System.Type.GetType("System.DateTime"));
                TaxLineType.Columns.Add("LocationLevel", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("Location", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("OriginLevel", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("Origin", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("DestinationLevel", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("Destination", System.Type.GetType("System.String"));

                TaxLineType.Columns.Add("Currency", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("GLCode", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("AppliedAt", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("AddInTotal", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("TaxPercent", System.Type.GetType("System.Decimal"));
                TaxLineType.Columns.Add("MinimumCharge", System.Type.GetType("System.Decimal"));
                TaxLineType.Columns.Add("Maximum", System.Type.GetType("System.Decimal"));
                TaxLineType.Columns.Add("AppliedOn", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                TaxLineType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                TaxLineType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));

                #endregion

                #region Creating TaxLineRemarkType DataTable

                DataTable TaxLineRemarkType = new DataTable("TaxLineRemarkType");
                TaxLineRemarkType.Columns.Add("TaxIndex", System.Type.GetType("System.Int32"));
                TaxLineRemarkType.Columns.Add("TaxLineSrNo", System.Type.GetType("System.Int32"));
                TaxLineRemarkType.Columns.Add("Comments", System.Type.GetType("System.String"));
                TaxLineRemarkType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                TaxLineRemarkType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                #region Creating TaxLineParamType DataTable

                DataTable TaxLineParamType = new DataTable("TaxLineParamType");
                TaxLineParamType.Columns.Add("TaxIndex", System.Type.GetType("System.Int32"));
                TaxLineParamType.Columns.Add("TaxSrNo", System.Type.GetType("System.Int32"));
                TaxLineParamType.Columns.Add("ParamName", System.Type.GetType("System.String"));
                TaxLineParamType.Columns.Add("ParamValue", System.Type.GetType("System.String"));
                TaxLineParamType.Columns.Add("IsInclude", System.Type.GetType("System.Byte"));
                TaxLineParamType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                TaxLineParamType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                int taxLineSrNo = 0;
                string validationDetailsTaxLine = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;
                string uLDType = string.Empty;


                for (int i = 0; i < dataTableTaxLineExcelData.Rows.Count; i++)
                {
                    taxLineSrNo = 0;
                    validationDetailsTaxLine = string.Empty;
                    tempDecimalValue = 0;
                    uLDType = string.Empty;


                    if (dataTableTaxLineExcelData.Rows[i]["taxcode"].ToString().Trim().Trim(',').Equals(string.Empty) && dataTableTaxLineExcelData.Rows[i]["taxname"].ToString().Trim().Trim(',').Equals(string.Empty))
                        break;
                    #region Create row for RateLineType Data Table

                    DataRow dataRowRateLineType = TaxLineType.NewRow();

                    dataRowRateLineType["TaxIndex"] = i + 1;

                    #region RateLineSrNo

                    if (dataTableTaxLineExcelData.Rows[i]["taxid"] == null)
                    {
                        dataRowRateLineType["TAXId"] = taxLineSrNo;
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["taxid"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["TAXId"] = taxLineSrNo;
                    }
                    else
                    {
                        if (int.TryParse(dataTableTaxLineExcelData.Rows[i]["taxid"].ToString().Trim().Trim(','), out taxLineSrNo))
                        {
                            dataRowRateLineType["TAXId"] = taxLineSrNo;
                        }
                        else
                        {
                            dataRowRateLineType["TAXId"] = taxLineSrNo;
                            validationDetailsTaxLine = validationDetailsTaxLine + "Invalid TaxId Value;";
                        }
                    }

                    #endregion

                    #region TaxCode

                    if (dataTableTaxLineExcelData.Rows[i]["taxcode"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TaxCode not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["taxcode"].ToString().Trim().Trim(',').Length > 30)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TaxCode is more than 30 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["TaxCode"] = dataTableTaxLineExcelData.Rows[i]["taxcode"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region TaxName

                    if (dataTableTaxLineExcelData.Rows[i]["taxname"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TaxName not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["taxname"].ToString().Trim().Trim(',').Length > 30)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TaxName is more than 30 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["TaxName"] = dataTableTaxLineExcelData.Rows[i]["taxname"].ToString().Trim().Trim(',');
                    }

                    #endregion
                    #region TaxType

                    if (dataTableTaxLineExcelData.Rows[i]["taxtype"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TaxType not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["taxtype"].ToString().Trim().Trim(',').Length > 30)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TaxType is more than 20 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["TaxType"] = dataTableTaxLineExcelData.Rows[i]["taxtype"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region ValidFrom

                    if (dataTableTaxLineExcelData.Rows[i]["validfrom"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "ValidFrom not found;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowRateLineType["ValidFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTableTaxLineExcelData.Rows[i]["validfrom"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableTaxLineExcelData.Rows[i]["validfrom"].ToString().Trim(), out tempDate))
                                {
                                    dataRowRateLineType["ValidFrom"] = tempDate;
                                }
                                else
                                {
                                    dataRowRateLineType["ValidFrom"] = DateTime.Now;
                                    validationDetailsTaxLine = validationDetailsTaxLine + "Invalid ValidFrom;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowRateLineType["ValidFrom"] = DateTime.Now;
                            validationDetailsTaxLine = validationDetailsTaxLine + "Invalid ValidFrom;";
                        }

                    }

                    #endregion

                    #region ValidTo

                    if (dataTableTaxLineExcelData.Rows[i]["validto"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "ValidTo not found;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowRateLineType["ValidTo"] = DateTime.FromOADate(Convert.ToDouble(dataTableTaxLineExcelData.Rows[i]["validto"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableTaxLineExcelData.Rows[i]["validto"].ToString().Trim(), out tempDate))
                                {
                                    dataRowRateLineType["ValidTo"] = tempDate;
                                }
                                else
                                {
                                    dataRowRateLineType["ValidTo"] = DateTime.Now;
                                    validationDetailsTaxLine = validationDetailsTaxLine + "Invalid ValidTo;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowRateLineType["ValidTo"] = DateTime.Now;
                            validationDetailsTaxLine = validationDetailsTaxLine + "Invalid ValidTo;";
                        }

                    }
                    #endregion

                    #region LocationLevel

                    if (dataTableTaxLineExcelData.Rows[i]["locationlevel"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["LocationLevel"] = string.Empty;
                    }
                    else
                    {
                        dataRowRateLineType["LocationLevel"] = dataTableTaxLineExcelData.Rows[i]["locationlevel"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region Location

                    if (dataTableTaxLineExcelData.Rows[i]["location"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Location not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["location"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Location is more than 5 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Location"] = dataTableTaxLineExcelData.Rows[i]["location"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region OriginLevel

                    if (dataTableTaxLineExcelData.Rows[i]["originlevel"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["OriginLevel"] = string.Empty;
                    }
                    else
                    {
                        dataRowRateLineType["OriginLevel"] = dataTableTaxLineExcelData.Rows[i]["originlevel"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region Origin

                    if (dataTableTaxLineExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Origin not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Origin is more than 5 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Origin"] = dataTableTaxLineExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region DestinationLeve

                    if (dataTableTaxLineExcelData.Rows[i]["destinationlevel"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["DestinationLevel"] = string.Empty;
                    }
                    else
                    {
                        dataRowRateLineType["DestinationLevel"] = dataTableTaxLineExcelData.Rows[i]["destinationlevel"].ToString().Trim().Trim(',');
                    }

                    #endregion

                    #region Destination

                    if (dataTableTaxLineExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Destination not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Destination is more than 5 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Destination"] = dataTableTaxLineExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region Currency

                    if (dataTableTaxLineExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Currency not found;";
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Currency is more than 5 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["Currency"] = dataTableTaxLineExcelData.Rows[i]["currency"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion

                    #region GLCode

                    if (dataTableTaxLineExcelData.Rows[i]["glcode"].ToString().Trim().Trim(',').Length > 20)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "GLCode is more than 20 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["GLCode"] = dataTableTaxLineExcelData.Rows[i]["glcode"].ToString().Trim().Trim(',');
                    }

                    #endregion
                    #region AppliedAt

                    if (dataTableTaxLineExcelData.Rows[i]["appliedat"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "AppliedAt is more than 5 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["AppliedAt"] = dataTableTaxLineExcelData.Rows[i]["appliedat"].ToString().Trim().Trim(',');
                    }

                    #endregion
                    #region AddInTotal

                    switch (dataTableTaxLineExcelData.Rows[i]["addintotal"].ToString().Trim().Trim(',').ToUpper())
                    {
                        case "1":
                            dataRowRateLineType["AddInTotal"] = 1;
                            break;
                        case "0":
                            dataRowRateLineType["AddInTotal"] = 0;
                            break;
                        default:
                            dataRowRateLineType["AddInTotal"] = 0;
                            break;
                    }

                    #endregion

                    #region TaxPercent

                    if (dataTableTaxLineExcelData.Rows[i]["taxpercent"] == null)
                    {
                        dataRowRateLineType["TaxPercent"] = tempDecimalValue;
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["taxpercent"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["TaxPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableTaxLineExcelData.Rows[i]["taxpercent"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowRateLineType["TaxPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowRateLineType["TaxPercent"] = tempDecimalValue;
                            validationDetailsTaxLine = validationDetailsTaxLine + "Invalid TaxPercent;";
                        }
                    }

                    #endregion

                    #region MinimumCharge

                    if (dataTableTaxLineExcelData.Rows[i]["minimumcharge"] == null)
                    {
                        dataRowRateLineType["MinimumCharge"] = tempDecimalValue;
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["minimumcharge"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["MinimumCharge"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableTaxLineExcelData.Rows[i]["minimumcharge"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowRateLineType["MinimumCharge"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowRateLineType["MinimumCharge"] = tempDecimalValue;
                            validationDetailsTaxLine = validationDetailsTaxLine + "Invalid MinimumCharge;";
                        }
                    }

                    #endregion

                    #region Maximum

                    if (dataTableTaxLineExcelData.Rows[i]["maximum"] == null)
                    {
                        dataRowRateLineType["Maximum"] = tempDecimalValue;
                    }
                    else if (dataTableTaxLineExcelData.Rows[i]["maximum"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowRateLineType["Maximum"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableTaxLineExcelData.Rows[i]["maximum"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowRateLineType["Maximum"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowRateLineType["Maximum"] = tempDecimalValue;
                            validationDetailsTaxLine = validationDetailsTaxLine + "Invalid Maximum;";
                        }
                    }

                    #endregion

                    #region AppliedOn

                    if (dataTableTaxLineExcelData.Rows[i]["appliedon"].ToString().Trim().Trim(',').Length > 20)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "AppliedOn is more than 20 Chars;";
                    }
                    else
                    {
                        dataRowRateLineType["AppliedOn"] = dataTableTaxLineExcelData.Rows[i]["appliedon"].ToString().Trim().Trim(',').ToUpper();
                    }

                    #endregion


                    dataRowRateLineType["UpdatedBy"] = string.Empty;
                    dataRowRateLineType["UpdatedOn"] = DateTime.Now;

                    // will be updated at the end of all validations.
                    dataRowRateLineType["ValidationDetails"] = string.Empty;

                    #endregion

                    #region Create row for TaxLineRemarkType Data Table

                    DataRow dataRowTaxLineRemarkType = TaxLineRemarkType.NewRow();

                    if (dataTableTaxLineExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Length > 250)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Remarks is more than 250 Chars;";
                    }
                    else
                    {
                        dataRowTaxLineRemarkType["TaxIndex"] = i + 1;
                        dataRowTaxLineRemarkType["TaxLineSrNo"] = taxLineSrNo;
                        dataRowTaxLineRemarkType["Comments"] = dataTableTaxLineExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').ToUpper();
                        dataRowTaxLineRemarkType["UpdatedBy"] = string.Empty;
                        dataRowTaxLineRemarkType["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region Create rows for TaxLineParamType Data Table

                    #region FlightCarrier

                    DataRow dataRowRateLineParamTypeFlightCarrier = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "FlightCarrier is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeFlightCarrier["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeFlightCarrier["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeFlightCarrier["ParamName"] = "FlightCarrier";
                        dataRowRateLineParamTypeFlightCarrier["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeFlightCarrier["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieflightcarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeFlightCarrier["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeFlightCarrier["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region IssueCarrier

                    DataRow dataRowRateLineParamTypeIssueCarrier = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "IssueCarrier is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeIssueCarrier["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeIssueCarrier["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeIssueCarrier["ParamName"] = "IssueCarrier";
                        dataRowRateLineParamTypeIssueCarrier["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeIssueCarrier["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieissuecarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeIssueCarrier["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeIssueCarrier["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region CountrySource

                    DataRow dataRowRateLineParamTypeCountrySource = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["countrysource"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "CountrySource is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeCountrySource["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeCountrySource["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeCountrySource["ParamName"] = "CountrySource";
                        dataRowRateLineParamTypeCountrySource["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["countrysource"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeCountrySource["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["iecountrysource"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeCountrySource["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeCountrySource["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region CountryDestination

                    DataRow dataRowRateLineParamTypeCountryDestination = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["countrydestination"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "CountryDestination is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeCountryDestination["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeCountryDestination["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeCountryDestination["ParamName"] = "CountryDestination";
                        dataRowRateLineParamTypeCountryDestination["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["countrydestination"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeCountryDestination["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["iecountrydestination"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeCountryDestination["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeCountryDestination["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region Flight#

                    DataRow dataRowRateLineParamTypeFlight = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["flight#"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "Flight# is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeFlight["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeFlight["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeFlight["ParamName"] = "FlightNum";
                        dataRowRateLineParamTypeFlight["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["flight#"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeFlight["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieflight#"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeFlight["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeFlight["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region DaysOfWeek

                    DataRow dataRowRateLineParamTypeDaysOfWeek = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "DaysOfWeek is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeDaysOfWeek["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeDaysOfWeek["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeDaysOfWeek["ParamName"] = "DaysOfWeek";
                        dataRowRateLineParamTypeDaysOfWeek["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeDaysOfWeek["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["iedaysofweek"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeDaysOfWeek["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeDaysOfWeek["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region AgentCode

                    DataRow dataRowRateLineParamTypeAgentCode = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "AgentCode is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeAgentCode["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeAgentCode["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeAgentCode["ParamName"] = "AgentCode";
                        dataRowRateLineParamTypeAgentCode["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeAgentCode["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieagentcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeAgentCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeAgentCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion





                    #region ShipperCode

                    DataRow dataRowRateLineParamTypeShipperCode = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "ShipperCode is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeShipperCode["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeShipperCode["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeShipperCode["ParamName"] = "ShipperCode";
                        dataRowRateLineParamTypeShipperCode["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeShipperCode["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieshippercode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeShipperCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeShipperCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region CommodityCode

                    DataRow dataRowRateLineParamTypeCommodityCode = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["commoditycode"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "CommodityCode is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeCommodityCode["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeCommodityCode["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeCommodityCode["ParamName"] = "CommCode";
                        dataRowRateLineParamTypeCommodityCode["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["commoditycode"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeCommodityCode["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["iecommoditycode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeCommodityCode["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeCommodityCode["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region ProductType

                    DataRow dataRowRateLineParamTypeProductType = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "ProductType is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeProductType["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeProductType["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeProductType["ParamName"] = "ProductType";
                        dataRowRateLineParamTypeProductType["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeProductType["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieproducttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeProductType["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeProductType["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region SHC

                    DataRow dataRowRateLineParamTypeSHC = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["shc"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "SHC is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeSHC["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeSHC["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeSHC["ParamName"] = "HandlingCode";
                        dataRowRateLineParamTypeSHC["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["shc"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeSHC["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieshc"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeSHC["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeSHC["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region EquipmentType

                    DataRow dataRowRateLineParamTypeEquipmentType = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "IEEquipmentType is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeEquipmentType["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeEquipmentType["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeEquipmentType["ParamName"] = "EquipType";
                        dataRowRateLineParamTypeEquipmentType["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeEquipmentType["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ieequipmenttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeEquipmentType["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeEquipmentType["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region TransitStation

                    DataRow dataRowRateLineParamTypeTransitStation = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "TransitStation is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeTransitStation["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeTransitStation["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeTransitStation["ParamName"] = "TransitStation";
                        dataRowRateLineParamTypeTransitStation["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeTransitStation["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["ietransitstation"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeTransitStation["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeTransitStation["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #region SPAMarkup%

                    DataRow dataRowRateLineParamTypeSPAMarkup = TaxLineParamType.NewRow();
                    if (dataTableTaxLineExcelData.Rows[i]["spamarkup%"].ToString().Trim().Trim(',').Length > 4000)
                    {
                        validationDetailsTaxLine = validationDetailsTaxLine + "SPAMarkup% is more than 4000 Chars;";
                    }
                    else
                    {
                        dataRowRateLineParamTypeSPAMarkup["TaxIndex"] = i + 1;
                        dataRowRateLineParamTypeSPAMarkup["TaxSrNo"] = taxLineSrNo;
                        dataRowRateLineParamTypeSPAMarkup["ParamName"] = "OCCodes";
                        dataRowRateLineParamTypeSPAMarkup["ParamValue"] = dataTableTaxLineExcelData.Rows[i]["spamarkup%"].ToString().Trim().Trim(',');
                        dataRowRateLineParamTypeSPAMarkup["IsInclude"] = dataTableTaxLineExcelData.Rows[i]["iespamarkup%"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowRateLineParamTypeSPAMarkup["UpdatedBy"] = string.Empty;
                        dataRowRateLineParamTypeSPAMarkup["UpdatedOn"] = DateTime.Now;
                    }

                    #endregion

                    #endregion




                    if (validationDetailsTaxLine.Equals(string.Empty))
                    {
                        dataRowRateLineType["ValidationDetails"] = string.Empty;

                        TaxLineRemarkType.Rows.Add(dataRowTaxLineRemarkType);

                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeFlightCarrier);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeIssueCarrier);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeCountrySource);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeCountryDestination);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeFlight);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeDaysOfWeek);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeAgentCode);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeShipperCode);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeCommodityCode);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeProductType);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeSHC);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeEquipmentType);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeTransitStation);
                        TaxLineParamType.Rows.Add(dataRowRateLineParamTypeSPAMarkup);


                    }
                    else
                    {
                        dataRowRateLineType["ValidationDetails"] = validationDetailsTaxLine;
                    }

                    TaxLineType.Rows.Add(dataRowRateLineType);

                }

                // Database Call to Validate & Insert Rate Line Master
                string errorInSp = string.Empty;
                await ValidateAndInsertTaxLineMaster(srNotblMasterUploadSummaryLog, TaxLineType, TaxLineRemarkType,
                                                                               TaxLineParamType,
                                                                               errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableTaxLineExcelData = null;
            }
        }
        public async Task<DataSet> ValidateAndInsertTaxLineMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableTaxLineType, DataTable dataTableTaxLineRemarkType,
                                                                                  DataTable dataTableTaxLineParamType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@TaxLineTableType", dataTableTaxLineType),
                    new SqlParameter("@TaxLineRemarkTableType", dataTableTaxLineRemarkType),
                    new SqlParameter("@TaxLineParamTableType", dataTableTaxLineParamType),
                    new SqlParameter("@Error", errorInSp)
                ];

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspUploadTaxLineMaster", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspUploadTaxLineMaster", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Message: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                return dataSetResult;
            }
        }
    }
}
