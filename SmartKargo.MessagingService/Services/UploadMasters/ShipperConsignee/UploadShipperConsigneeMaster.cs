using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.ShipperConsignee
{
    /// <summary>
    /// Class to Upload ShipperConsignee Master File.
    /// </summary>
    public class UploadShipperConsigneeMaster
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadShipperConsigneeMaster> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;

        #region Constructor
        public UploadShipperConsigneeMaster(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadShipperConsigneeMaster> logger,
            Func<UploadMasterCommon> uploadMasterCommonFactory
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion

        /// <summary>
        /// Method to Uplaod ShipperConsignee Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<bool> ShipperConsigneeMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.ShipperConsignee);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "ShipperConsigneeMasterUploadFile", out uploadFilePath))
                        {
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
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
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Message: " + ex.Message + " \nStackTrace: " + ex.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to Process ShipperConsignee Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> ShipperConsignee Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableShipperConsigneeExcelData = new DataTable("dataTableShipperConsigneeExcelData");

            bool isBinaryReader = false;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".csv") || fileExtention.Equals(".xlsb") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTableShipperConsigneeExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommonFactory().RemoveEmptyRows(dataTableShipperConsigneeExcelData);

                foreach (DataColumn dataColumn in dataTableShipperConsigneeExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableShipperConsigneeExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating AgentType DataTable

                DataTable ShipperConsigneeType = new DataTable("ShipperConsigneeType");
                ShipperConsigneeType.Columns.Add("AgentIndex", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("AgentCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IATAAgentCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AgentName", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ValidFrom", System.Type.GetType("System.DateTime"));
                ShipperConsigneeType.Columns.Add("ValidTo", System.Type.GetType("System.DateTime"));
                ShipperConsigneeType.Columns.Add("CustomerCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("Station", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AirlineCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("Country", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("City", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("EORINo", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AgentTypeId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("SalesId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("HoldingCompany", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("GSATypeId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("CommTypeId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("Fixed", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("Percentage", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("Remarks", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("CurrencyCodeId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("CurrencyNumber", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("SettlCurr", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("CassId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("ReportingPeriodId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("CrdtLmt", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("BillingPeriod", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("NorrmalCommPercent", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("BillToId", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("AgentAccountCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("TolarancePercent", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("TolaranceValue", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("MaxValue", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("ContactPerson", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("OfficeAddress1", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("State", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("MobileNumber", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("PostalZIP", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("Phone1", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("FAX", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("Email", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                ShipperConsigneeType.Columns.Add("Remark", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("PersonContact", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("CurrentAWBno", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ControllingLocator", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AccountCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("TDSOnCommision", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("TDSOnFreight", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ControllingLocatorCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BuildTo", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AccountMail", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("SalesMail", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AgentType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("PanCardNumber", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ServiceTaxNumber", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ValidBG", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsFOC", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("CurrencyCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("threshold", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AgentReferenceCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("TresholdLimitDays", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("RatePreference", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("GLCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AutoGenerateAgentInvoice", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("AllowedStations", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ExcludeFromFBL", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("IACCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("CCSFCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("isKnownShipper", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("InvoiceDueDays", System.Type.GetType("System.Int32"));
                ShipperConsigneeType.Columns.Add("AgentCreditAmount", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("AgentInvoiceBalance", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("AgentCreditRemaining", System.Type.GetType("System.Decimal"));
                ShipperConsigneeType.Columns.Add("TAXExemption", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("DefaultPayMode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("StockAlertDate", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AllowedPayMode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("DBA", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("KnownShipperExpDt", System.Type.GetType("System.DateTime"));
                ShipperConsigneeType.Columns.Add("SSPCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsConsole", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("ParticipationType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("OfficeAddress2", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("SITAAddress", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("KnownShipperValidFrom", System.Type.GetType("System.DateTime"));
                ShipperConsigneeType.Columns.Add("OPSMail", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsSameAddress", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("BillingAddress1", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingAddress2", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingCity", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingState", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingZipCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingCountry", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingContactPerson", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BillingPhNo", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("UOM", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsCASSAgent", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("AdditionalCredit", System.Type.GetType("System.Single"));
                ShipperConsigneeType.Columns.Add("AgentTypeATADTD", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("StockController", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsGSA", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("Latitude", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("Longitude", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IsRA3Designated", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("OpsFromTime", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("OpsToTime", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("DealPLIAppliedTo", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("Website", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ShipperType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("BusinessType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("IndustryFocus", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("SecurityType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ClerkIdentificationCode", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("POMandatory", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("KnownShipperNumber", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("RequestKnownShipper", System.Type.GetType("System.Byte"));
                ShipperConsigneeType.Columns.Add("Notification", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("InvoiceType", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("ValidationDetailsAgent", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("AllowedCarriers", System.Type.GetType("System.String"));
                ShipperConsigneeType.Columns.Add("InvoiceEntity", System.Type.GetType("System.String"));

                #endregion Creating AgentType DataTable

                string validationDetailsShipperConsignee = string.Empty;
                DateTime tempDate;

                for (int i = 0; i < dataTableShipperConsigneeExcelData.Rows.Count; i++)
                {
                    validationDetailsShipperConsignee = string.Empty;

                    #region Create row for ShipperConsigneeType Data Table

                    DataRow dataRowAShipperConsigneeType = ShipperConsigneeType.NewRow();

                    dataRowAShipperConsigneeType["AgentIndex"] = i + 1;
                    dataRowAShipperConsigneeType["SerialNumber"] = 0;

                    #region AgentCode* [varchar] (20) NULL

                    if (columnNames.Contains("account_code*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["account_code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["AgentCode"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ACCOUNT_CODE is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["account_code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAShipperConsigneeType["AgentCode"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ACCOUNT_CODE is more than 20 Chars;";
                            }
                            else
                            {
                                if (ShipperConsigneeType.Select("AgentCode = ''").Length == 0)
                                {
                                    if (ShipperConsigneeType.Select(string.Format("AgentCode = '{0}'", dataTableShipperConsigneeExcelData.Rows[i]["account_code*"].ToString().Trim().Trim(','))).Length == 0)
                                    {
                                        dataRowAShipperConsigneeType["AgentCode"] = dataTableShipperConsigneeExcelData.Rows[i]["account_code*"].ToString().Trim().Trim(',');
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["AgentCode"] = dataTableShipperConsigneeExcelData.Rows[i]["account_code*"].ToString().Trim().Trim(',');
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + " Duplicate ACCOUNT_CODE;";
                                    }
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["AgentCode"] = dataTableShipperConsigneeExcelData.Rows[i]["account_code*"].ToString().Trim().Trim(',');
                                }
                            }
                        }
                    }

                    #endregion AgentCode

                    #region IATAAgentCode [varchar] (20) NULL

                    if (columnNames.Contains("iata_acc_code"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["iata_acc_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["IATAAgentCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["iata_acc_code"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAShipperConsigneeType["IATAAgentCode"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " IATA_ACC_CODE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["IATAAgentCode"] = dataTableShipperConsigneeExcelData.Rows[i]["iata_acc_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion IATAAgentCode

                    #region AgentName* [varchar] (125) NULL

                    if (columnNames.Contains("account_name*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["account_name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["AgentName"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ACCOUNT_NAME is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["account_name*"].ToString().Trim().Trim(',').Length > 125)
                            {
                                dataRowAShipperConsigneeType["AgentName"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ACCOUNT_NAME is more than 125 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["AgentName"] = dataTableShipperConsigneeExcelData.Rows[i]["account_name*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AgentName

                    #region AllowedCarriers* [varchar] (50) NULL

                    if (columnNames.Contains("allowed_carriers*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["allowed_carriers*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["AllowedCarriers"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " Allowed Carriers is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["allowed_carriers*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAShipperConsigneeType["AllowedCarriers"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " Allowed Carriers is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["AllowedCarriers"] = dataTableShipperConsigneeExcelData.Rows[i]["allowed_carriers*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AllowedCarriers

                    #region ValidFrom* [datetime] NULL

                    if (columnNames.Contains("valid_from*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["valid_from*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " VALID_FROM is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAShipperConsigneeType["ValidFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTableShipperConsigneeExcelData.Rows[i]["valid_from*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableShipperConsigneeExcelData.Rows[i]["valid_from*"].ToString().Trim(),out tempDate))
                                    {
                                        dataRowAShipperConsigneeType["ValidFrom"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["ValidFrom"] = DateTime.Now;
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid VALID_FROM;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAShipperConsigneeType["ValidFrom"] = DateTime.Now;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid VALID_FROM;";
                            }
                        }
                    }

                    #endregion ValidFrom*

                    #region ValidTo* [datetime] NULL,

                    if (columnNames.Contains("valid_to*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["valid_to*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " VALID_TO is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAShipperConsigneeType["ValidTo"] = DateTime.FromOADate(Convert.ToDouble(dataTableShipperConsigneeExcelData.Rows[i]["valid_to*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableShipperConsigneeExcelData.Rows[i]["valid_to*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAShipperConsigneeType["ValidTo"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["ValidTo"] = DateTime.Now;
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid VALID_TO;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAShipperConsigneeType["ValidTo"] = DateTime.Now;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid VALID_TO;";
                            }
                        }
                    }

                    #endregion ValidTo*

                    #region CustomerCode [varchar] (20) NULL

                    if (columnNames.Contains("credit_acc_no"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["credit_acc_no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["CustomerCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["credit_acc_no"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowAShipperConsigneeType["CustomerCode"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " CREDIT_ACC_NO is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["CustomerCode"] = dataTableShipperConsigneeExcelData.Rows[i]["credit_acc_no"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion CustomerCode

                    #region Country* [varchar] (20) NULL

                    if (columnNames.Contains("country*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["Country"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " COUNTRY is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAShipperConsigneeType["Country"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " COUNTRY is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["Country"] = dataTableShipperConsigneeExcelData.Rows[i]["country*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Country*

                    #region City* [varchar] (50) NULL

                    if (columnNames.Contains("city*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["City"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " CITY is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAShipperConsigneeType["City"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " CITY is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["City"] = dataTableShipperConsigneeExcelData.Rows[i]["city*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion City*

                    #region OfficeAddress1* [varchar] (125) NULL

                    if (columnNames.Contains("address1*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["OfficeAddress1"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ADDRESS1 is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',').Length > 125)
                            {
                                dataRowAShipperConsigneeType["OfficeAddress1"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ADDRESS1 is more than 125 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["OfficeAddress1"] = dataTableShipperConsigneeExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion OfficeAddress1*

                    #region State [varchar] (20) NULL

                    if (columnNames.Contains("state"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["state"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["State"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["state"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAShipperConsigneeType["State"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " STATE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["State"] = dataTableShipperConsigneeExcelData.Rows[i]["state"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion State

                    #region MobileNumber [varchar] (50) NULL

                    if (columnNames.Contains("mobile_number"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["MobileNumber"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAShipperConsigneeType["MobileNumber"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " MOBILE_NUMBER is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["MobileNumber"] = dataTableShipperConsigneeExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion MobileNumber

                    #region PostalZIP [varchar] (20) NULL

                    if (columnNames.Contains("zip_code"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["zip_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["PostalZIP"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["zip_code"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAShipperConsigneeType["PostalZIP"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ZIP_CODE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["PostalZIP"] = dataTableShipperConsigneeExcelData.Rows[i]["zip_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion PostalZIP

                    #region Phone1* [varchar] (20) NULL

                    if (columnNames.Contains("phone_no*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["Phone1"] = string.Empty;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " PHONE_NO is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAShipperConsigneeType["Phone1"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " PHONE_NO is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["Phone1"] = dataTableShipperConsigneeExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Phone1*

                    #region FAX [varchar] (50) NULL

                    if (columnNames.Contains("fax"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["fax"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["FAX"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["fax"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAShipperConsigneeType["FAX"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " FAX is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["FAX"] = dataTableShipperConsigneeExcelData.Rows[i]["fax"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion FAX

                    #region Email [varchar] (500) NULL

                    if (columnNames.Contains("email"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["Email"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["email"].ToString().Trim().Trim(',').Length > 500)
                            {
                                dataRowAShipperConsigneeType["Email"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " EMAIL is more than 500 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["Email"] = dataTableShipperConsigneeExcelData.Rows[i]["email"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Email

                    #region UpdatedBy [varchar] (100) NULL

                    dataRowAShipperConsigneeType["UpdatedBy"] = string.Empty;

                    #endregion UpdatedBy

                    #region UpdatedOn [datetime] NULL

                    dataRowAShipperConsigneeType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region Remarks [varchar] (50) NULL

                    if (columnNames.Contains("remarks"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["Remark"] = DBNull.Value;
                            dataRowAShipperConsigneeType["Remarks"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAShipperConsigneeType["Remark"] = DBNull.Value;
                                dataRowAShipperConsigneeType["Remarks"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " REMARKS is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["Remark"] = dataTableShipperConsigneeExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',');
                                dataRowAShipperConsigneeType["Remarks"] = dataRowAShipperConsigneeType["Remark"].ToString();
                            }
                        }
                    }

                    #endregion Remark

                    #region PersonContact [varchar] (100) NULL

                    if (columnNames.Contains("contact_person"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["PersonContact"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAShipperConsigneeType["PersonContact"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " CONTACT_PERSON is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["PersonContact"] = dataTableShipperConsigneeExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion PersonContact

                    #region ControllingLocatorCode [varchar] (25) NULL

                    if (columnNames.Contains("agent_code"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["agent_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["ControllingLocatorCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["agent_code"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowAShipperConsigneeType["ControllingLocatorCode"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " AGENT_CODE is more than 25 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["ControllingLocatorCode"] = dataTableShipperConsigneeExcelData.Rows[i]["agent_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion ControllingLocatorCode

                    #region PanCardNumber [varchar] (50) NULL

                    if (columnNames.Contains("tin"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["tin"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["PanCardNumber"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["tin"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAShipperConsigneeType["PanCardNumber"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " TIN is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["PanCardNumber"] = dataTableShipperConsigneeExcelData.Rows[i]["tin"].ToString().Trim();
                            }
                        }
                    }

                    #endregion PanCardNumber

                    #region IACCode [varchar] (500) NULL

                    if (columnNames.Contains("iac_code"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["iac_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["IACCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["iac_code"].ToString().Trim().Trim(',').Length > 500)
                            {
                                dataRowAShipperConsigneeType["IACCode"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " IAC_CODE is more than 500 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["IACCode"] = dataTableShipperConsigneeExcelData.Rows[i]["iac_code"].ToString().Trim();
                            }
                        }
                    }

                    #endregion IACCode

                    #region CCSFCode [varchar] (1000) NULL

                    if (columnNames.Contains("ccsf_code"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["ccsf_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["CCSFCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["ccsf_code"].ToString().Trim().Trim(',').Length > 1000)
                            {
                                dataRowAShipperConsigneeType["CCSFCode"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " CCSF_CODE is more than 1000 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["CCSFCode"] = dataTableShipperConsigneeExcelData.Rows[i]["ccsf_code"].ToString().Trim();
                            }
                        }
                    }

                    #endregion CCSFCode

                    #region isKnownShipper [bit] NULL

                    if (columnNames.Contains("is_known_shipper"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["is_known_shipper"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["isKnownShipper"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["is_known_shipper"].ToString().Trim().Trim(',').Length > 1)
                            {
                                dataRowAShipperConsigneeType["isKnownShipper"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " IS_KNOWN_SHIPPER is more than 1 Chars;";
                            }
                            else
                            {
                                switch (dataTableShipperConsigneeExcelData.Rows[i]["is_known_shipper"].ToString().Trim().ToLower())
                                {
                                    case "n":
                                        dataRowAShipperConsigneeType["isKnownShipper"] = 0;
                                        break;
                                    case "y":
                                        dataRowAShipperConsigneeType["isKnownShipper"] = 1;
                                        break;
                                    default:
                                        dataRowAShipperConsigneeType["isKnownShipper"] = DBNull.Value;
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + " Invalid IS_KNOWN_SHIPPER;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion isKnownShipper

                    #region TAXExemption [bit] NULL

                    if (columnNames.Contains("vat_exemption"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["vat_exemption"].ToString().Trim().ToLower().Equals("y"))
                        {
                            dataRowAShipperConsigneeType["TAXExemption"] = 1;
                        }
                        else
                        {
                            dataRowAShipperConsigneeType["TAXExemption"] = 0;
                        }
                    }

                    #endregion TAXExemption

                    #region DBA [varchar] (100) NULL

                    if (columnNames.Contains("dba"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["dba"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["DBA"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["dba"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAShipperConsigneeType["DBA"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " DBA is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["DBA"] = dataTableShipperConsigneeExcelData.Rows[i]["dba"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion DBA

                    #region KnownShipperExpDt [datetime] NULL

                    if (columnNames.Contains("known_shipper_valid_to"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["KnownShipperExpDt"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader && !dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Contains(@"\")
                                                   && !dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Contains("/")
                                                   && !dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Contains("-"))
                                {
                                    tempDate = DateTime.FromOADate(Convert.ToDouble(dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim()));
                                    if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                    {
                                        dataRowAShipperConsigneeType["KnownShipperExpDt"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["KnownShipperExpDt"] = tempDate;
                                    }
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim(), out tempDate))
                                    {
                                        if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                        {
                                            dataRowAShipperConsigneeType["KnownShipperExpDt"] = DBNull.Value;
                                        }
                                        else
                                        {
                                            dataRowAShipperConsigneeType["KnownShipperExpDt"] = tempDate;
                                        }
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["KnownShipperExpDt"] = DBNull.Value;
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid KNOWN_SHIPPER_VALID_TO;";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                dataRowAShipperConsigneeType["KnownShipperExpDt"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid KNOWN_SHIPPER_VALID_TO;";
                            }
                        }
                    }

                    #endregion KnownShipperExpDt

                    #region SSPCode [varchar] (40) NULL

                    if (columnNames.Contains("ssp_code"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["ssp_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["SSPCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["ssp_code"].ToString().Trim().Trim(',').Length > 40)
                            {
                                dataRowAShipperConsigneeType["SSPCode"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " SSP_CODE is more than 40 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["SSPCode"] = dataTableShipperConsigneeExcelData.Rows[i]["ssp_code"].ToString().Trim();
                            }
                        }
                    }

                    #endregion SSPCode

                    #region IsConsole [bit] NULL

                    if (columnNames.Contains("is_console"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["is_console"].ToString().Trim().ToLower().Equals("y"))
                        {
                            dataRowAShipperConsigneeType["IsConsole"] = 1;
                        }
                        else
                        {
                            dataRowAShipperConsigneeType["IsConsole"] = 0;
                        }
                    }

                    #endregion IsConsole

                    #region ParticipationType [varchar] (10) NULL

                    if (columnNames.Contains("participation_type"))
                    {
                        switch (dataTableShipperConsigneeExcelData.Rows[i]["participation_type"].ToString().ToUpper().Trim().Trim(','))
                        {
                            case "S":
                            case "C":
                            case "SC":
                                dataRowAShipperConsigneeType["ParticipationType"] = dataTableShipperConsigneeExcelData.Rows[i]["participation_type"].ToString().ToUpper().Trim().Trim(',');
                                break;
                            default:
                                dataRowAShipperConsigneeType["ParticipationType"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " PARTICIPATION_TYPE is Invalid;";
                                break;
                        }                        
                    }

                    #endregion ParticipationType*

                    #region IsActive* [bit] NULL

                    if (columnNames.Contains("is_active*"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["is_active*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["IsActive"] = DBNull.Value;
                            validationDetailsShipperConsignee = validationDetailsShipperConsignee + " IS_ACTIVE is required;";
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["is_active*"].ToString().Trim().Trim(',').Length > 1)
                            {
                                dataRowAShipperConsigneeType["IsActive"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " IS_ACTIVE is more than 1 Chars;";
                            }
                            else
                            {
                                switch (dataTableShipperConsigneeExcelData.Rows[i]["is_active*"].ToString().ToUpper().Trim().Trim(','))
                                {
                                    case "Y":
                                        dataRowAShipperConsigneeType["IsActive"] = 1;
                                        break;
                                    case "N":
                                        dataRowAShipperConsigneeType["IsActive"] = 0;
                                        break;
                                    default:
                                        dataRowAShipperConsigneeType["IsActive"] = DBNull.Value;
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + " IS_ACTIVE is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion IsActive*

                    #region OfficeAddress2 [varchar] (125) NULL

                    if (columnNames.Contains("address2"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["address2"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["OfficeAddress2"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["address2"].ToString().Trim().Trim(',').Length > 125)
                            {
                                dataRowAShipperConsigneeType["OfficeAddress2"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " ADDRESS2 is more than 125 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["OfficeAddress2"] = dataTableShipperConsigneeExcelData.Rows[i]["address2"].ToString().Trim();
                            }
                        }
                    }

                    #endregion OfficeAddress2

                    #region KnownShipperValidFrom [datetime] NULL

                    if (columnNames.Contains("known_shipper_valid_from"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["KnownShipperValidFrom"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader && !dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Contains(@"\")
                                                   && !dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Contains("/")
                                                   && !dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Contains("-"))
                                {
                                    tempDate = DateTime.FromOADate(Convert.ToDouble(dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim()));
                                    if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                    {
                                        dataRowAShipperConsigneeType["KnownShipperValidFrom"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["KnownShipperValidFrom"] = tempDate;
                                    }
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableShipperConsigneeExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim(), out tempDate))
                                    {
                                        if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                        {
                                            dataRowAShipperConsigneeType["KnownShipperValidFrom"] = DBNull.Value;
                                        }
                                        else
                                        {
                                            dataRowAShipperConsigneeType["KnownShipperValidFrom"] = tempDate;
                                        }
                                    }
                                    else
                                    {
                                        dataRowAShipperConsigneeType["KnownShipperValidFrom"] = DBNull.Value;
                                        validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid KNOWN_SHIPPER_VALID_FROM;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAShipperConsigneeType["KnownShipperValidFrom"] = DBNull.Value;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + "Invalid KNOWN_SHIPPER_VALID_FROM;";
                            }
                        }
                    }

                    #endregion KnownShipperValidFrom

                    #region IsSameAddress [bit] NULL

                    if (columnNames.Contains("billing_address_same_as_mailing"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["billing_address_same_as_mailing"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["IsSameAddress"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableShipperConsigneeExcelData.Rows[i]["billing_address_same_as_mailing"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                    dataRowAShipperConsigneeType["IsSameAddress"] = 1;
                                    break;
                                case "N":
                                    dataRowAShipperConsigneeType["IsSameAddress"] = 0;
                                    break;
                                default:
                                    dataRowAShipperConsigneeType["IsSameAddress"] = DBNull.Value;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_ADDRESS_SAME_AS_MAILING is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion IsSameAddress

                    if (dataRowAShipperConsigneeType["IsSameAddress"].ToString().Equals("1"))
                    {
                        dataRowAShipperConsigneeType["BillingAddress1"] = dataRowAShipperConsigneeType["OfficeAddress1"];
                        dataRowAShipperConsigneeType["BillingAddress2"] = dataRowAShipperConsigneeType["OfficeAddress2"];
                        dataRowAShipperConsigneeType["BillingCity"] = dataRowAShipperConsigneeType["City"];
                        dataRowAShipperConsigneeType["BillingState"] = dataRowAShipperConsigneeType["State"];
                        dataRowAShipperConsigneeType["BillingZipCode"] = dataRowAShipperConsigneeType["PostalZIP"];
                        dataRowAShipperConsigneeType["BillingCountry"] = dataRowAShipperConsigneeType["Country"];
                        dataRowAShipperConsigneeType["BillingContactPerson"] = dataRowAShipperConsigneeType["PersonContact"];
                        dataRowAShipperConsigneeType["BillingPhNo"] = dataRowAShipperConsigneeType["Phone1"];
                    }
                    else
                    {
                        #region BillingAddress1 [varchar] (125) NULL

                        if (columnNames.Contains("billing_address1"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_address1"].ToString().Trim().Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingAddress1"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_address1"].ToString().Trim().Length > 125)
                                {
                                    dataRowAShipperConsigneeType["BillingAddress1"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_ADDRESS1 is more than 125 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingAddress1"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_address1"].ToString().Trim();
                                }
                            }
                        }

                        #endregion BillingAddress1

                        #region BillingAddress2 [varchar] (125) NULL

                        if (columnNames.Contains("billing_address2"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_address2"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingAddress2"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_address2"].ToString().Trim().Trim(',').Length > 125)
                                {
                                    dataRowAShipperConsigneeType["BillingAddress2"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_ADDRESS2 is more than 125 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingAddress2"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_address2"].ToString().Trim();
                                }
                            }
                        }

                        #endregion BillingAddress2

                        #region BillingCity [varchar] (50) NULL

                        if (columnNames.Contains("billing_city"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingCity"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',').Length > 50)
                                {
                                    dataRowAShipperConsigneeType["BillingCity"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_CITY is more than 50 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingCity"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingCity

                        #region BillingState [varchar] (20) NULL

                        if (columnNames.Contains("billing_state"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingState"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',').Length > 20)
                                {
                                    dataRowAShipperConsigneeType["BillingState"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_STATE is more than 20 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingState"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingState

                        #region BillingZipCode [varchar] (20) NULL

                        if (columnNames.Contains("billing_zip_code"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingZipCode"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',').Length > 20)
                                {
                                    dataRowAShipperConsigneeType["BillingZipCode"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_ZIP_CODE is more than 20 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingZipCode"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingZipCode

                        #region BillingCountry [varchar] (30) NULL

                        if (columnNames.Contains("billing_country"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingCountry"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',').Length > 30)
                                {
                                    dataRowAShipperConsigneeType["BillingCountry"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_COUNTRY is more than 30 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingCountry"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingCountry

                        #region BillingContactPerson [varchar] (100) NULL

                        if (columnNames.Contains("billing_contact_person"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingContactPerson"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',').Length > 100)
                                {
                                    dataRowAShipperConsigneeType["BillingContactPerson"] = DBNull.Value;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_CONTACT_PERSON is more than 100 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingContactPerson"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingContactPerson

                        #region BillingPhNo [varchar] (20) NULL

                        if (columnNames.Contains("billing_phone_no"))
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAShipperConsigneeType["BillingPhNo"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableShipperConsigneeExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',').Length > 20)
                                {
                                    dataRowAShipperConsigneeType["BillingPhNo"] = string.Empty;
                                    validationDetailsShipperConsignee = validationDetailsShipperConsignee + " BILLING_PHONE_NO is more than 20 Chars;";
                                }
                                else
                                {
                                    dataRowAShipperConsigneeType["BillingPhNo"] = dataTableShipperConsigneeExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingPhNo
                    }

                    #region Un-used fields in Agent Master for ShipperConsignee are set to NULL

                    #region Station [varchar] (10) NULL

                    if (columnNames.Contains("station"))
                    {
                        if (dataTableShipperConsigneeExcelData.Rows[i]["station"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAShipperConsigneeType["Station"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableShipperConsigneeExcelData.Rows[i]["station"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAShipperConsigneeType["Station"] = string.Empty;
                                validationDetailsShipperConsignee = validationDetailsShipperConsignee + " STATION is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAShipperConsigneeType["Station"] = dataTableShipperConsigneeExcelData.Rows[i]["station"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Station*

                    #region AirlineCode [varchar] (10) NULL

                    dataRowAShipperConsigneeType["AirlineCode"] = DBNull.Value;

                    #endregion AirlineCode

                    #region EORINo [varchar] (30) NULL

                    dataRowAShipperConsigneeType["EORINo"] = DBNull.Value;

                    #endregion EORINo

                    #region AgentTypeId [int] NULL

                    dataRowAShipperConsigneeType["AgentTypeId"] = DBNull.Value;

                    #endregion AgentTypeId

                    #region SalesId [int] NULL

                    dataRowAShipperConsigneeType["SalesId"] = DBNull.Value;

                    #endregion SalesId

                    #region HoldingCompany [varchar] (30) NULL

                    dataRowAShipperConsigneeType["HoldingCompany"] = DBNull.Value;

                    #endregion HoldingCompany

                    #region GSATypeId [int] NULL

                    dataRowAShipperConsigneeType["GSATypeId"] = DBNull.Value;

                    #endregion GSATypeId

                    #region CommTypeId [int] NULL

                    dataRowAShipperConsigneeType["CommTypeId"] = DBNull.Value;

                    #endregion CommTypeId

                    #region Fixed [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["Fixed"] = DBNull.Value;

                    #endregion Fixed

                    #region Percentage [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["Percentage"] = DBNull.Value;

                    #endregion Percentage

                    #region CurrencyCodeId [int] NULL

                    dataRowAShipperConsigneeType["CurrencyCodeId"] = DBNull.Value;

                    #endregion CurrencyCodeId

                    #region CurrencyNumber [int] NULL

                    dataRowAShipperConsigneeType["CurrencyNumber"] = DBNull.Value;

                    #endregion CurrencyNumber

                    #region SettlCurr [varchar] (20) NULL

                    dataRowAShipperConsigneeType["SettlCurr"] = DBNull.Value;

                    #endregion SettlCurr

                    #region CassId [int] NULL

                    dataRowAShipperConsigneeType["CassId"] = DBNull.Value;

                    #endregion CassId

                    #region ReportingPeriodId [int] NULL

                    dataRowAShipperConsigneeType["ReportingPeriodId"] = DBNull.Value;

                    #endregion ReportingPeriodId

                    #region CrdtLmt [int] NULL

                    dataRowAShipperConsigneeType["CrdtLmt"] = DBNull.Value;

                    #endregion CrdtLmt

                    #region BillingPeriod [varchar] (50) NULL

                    dataRowAShipperConsigneeType["BillingPeriod"] = DBNull.Value;

                    #endregion BillingPeriod

                    #region NorrmalCommPercent [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["NorrmalCommPercent"] = DBNull.Value;

                    #endregion NorrmalCommPercent

                    #region BillToId [int] NULL

                    dataRowAShipperConsigneeType["BillToId"] = DBNull.Value;

                    #endregion BillToId

                    #region AgentAccountCode [varchar] (30) NULL

                    dataRowAShipperConsigneeType["AgentAccountCode"] = DBNull.Value;

                    #endregion AgentAccountCode

                    #region TolarancePercent [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["TolarancePercent"] = DBNull.Value;

                    #endregion TolarancePercent

                    #region TolaranceValue [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["TolaranceValue"] = DBNull.Value;

                    #endregion TolaranceValue

                    #region MaxValue [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["MaxValue"] = DBNull.Value;

                    #endregion MaxValue

                    #region ContactPerson [int] NULL

                    dataRowAShipperConsigneeType["ContactPerson"] = DBNull.Value;

                    #endregion ContactPerson

                    #region CurrentAWBno [varchar] (15) NULL

                    dataRowAShipperConsigneeType["CurrentAWBno"] = DBNull.Value;

                    #endregion CurrentAWBno

                    #region ControllingLocator [varchar] (50) NULL

                    dataRowAShipperConsigneeType["ControllingLocator"] = DBNull.Value;

                    #endregion ControllingLocator*

                    #region AccountCode [varchar] (25) NULL

                    dataRowAShipperConsigneeType["AccountCode"] = DBNull.Value;

                    #endregion AccountCode

                    #region TDSOnCommision [varchar] (25) NULL

                    dataRowAShipperConsigneeType["TDSOnCommision"] = DBNull.Value;

                    #endregion TDSOnCommision

                    #region TDSOnFreight [varchar] (25) NULL

                    dataRowAShipperConsigneeType["TDSOnFreight"] = DBNull.Value;

                    #endregion TDSOnFreight

                    #region BuildTo [varchar] (25) NULL

                    dataRowAShipperConsigneeType["BuildTo"] = DBNull.Value;

                    #endregion BuildTo*

                    #region AccountMail [varchar] (500) NULL

                    dataRowAShipperConsigneeType["AccountMail"] = DBNull.Value;

                    #endregion AccountMail

                    #region SalesMail [varchar] (500) NULL

                    dataRowAShipperConsigneeType["SalesMail"] = DBNull.Value;

                    #endregion SalesMail

                    #region AgentType [varchar] (50) NULL

                    dataRowAShipperConsigneeType["AgentType"] = DBNull.Value;

                    #endregion AgentType

                    #region BillType [varchar] (50) NULL

                    dataRowAShipperConsigneeType["BillType"] = DBNull.Value;

                    #endregion BillType*

                    #region ServiceTaxNumber [varchar] (50) NULL

                    dataRowAShipperConsigneeType["ServiceTaxNumber"] = DBNull.Value;

                    #endregion ServiceTaxNumber

                    #region ValidBG [varchar] (10) NULL

                    dataRowAShipperConsigneeType["ValidBG"] = DBNull.Value;

                    #endregion ValidBG*

                    #region IsFOC [bit] NULL

                    dataRowAShipperConsigneeType["IsFOC"] = DBNull.Value;

                    #endregion IsFOC

                    #region threshold [varchar] (20) NULL

                    dataRowAShipperConsigneeType["threshold"] = DBNull.Value;

                    #endregion threshold

                    #region CurrencyCode [varchar] (20) NULL

                    dataRowAShipperConsigneeType["CurrencyCode"] = DBNull.Value;

                    #endregion CurrencyCode*

                    #region AgentReferenceCode [varchar] (15) NULL

                    dataRowAShipperConsigneeType["AgentReferenceCode"] = DBNull.Value;

                    #endregion AgentReferenceCode

                    #region TresholdLimitDays [int] NULL

                    dataRowAShipperConsigneeType["TresholdLimitDays"] = DBNull.Value;

                    #endregion TresholdLimitDays

                    #region RatePreference [varchar] (10) NULL

                    dataRowAShipperConsigneeType["RatePreference"] = DBNull.Value;

                    #endregion RatePreference

                    #region GLCode [varchar] (10) NULL

                    dataRowAShipperConsigneeType["GLCode"] = DBNull.Value;

                    #endregion GLCode

                    #region AutoGenerateAgentInvoice [bit] NULL

                    dataRowAShipperConsigneeType["AutoGenerateAgentInvoice"] = DBNull.Value;
                    
                    #endregion AutoGenerateAgentInvoice

                    #region AllowedStations [varchar] (50) NULL

                    dataRowAShipperConsigneeType["AllowedStations"] = DBNull.Value;

                    #endregion AllowedStations

                    #region ExcludeFromFBL [bit] NULL

                    dataRowAShipperConsigneeType["ExcludeFromFBL"] = DBNull.Value;

                    #endregion ExcludeFromFBL

                    #region InvoiceDueDays [int] NULL

                    dataRowAShipperConsigneeType["InvoiceDueDays"] = DBNull.Value;

                    #endregion InvoiceDueDays

                    #region AgentCreditAmount [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["AgentCreditAmount"] = DBNull.Value;

                    #endregion AgentCreditAmount

                    #region AgentInvoiceBalance [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["AgentInvoiceBalance"] = DBNull.Value;

                    #endregion AgentInvoiceBalance

                    #region AgentCreditRemaining [decimal] (18, 2) NULL

                    dataRowAShipperConsigneeType["AgentCreditRemaining"] = DBNull.Value;

                    #endregion AgentCreditRemaining

                    #region DefaultPayMode [varchar] (10) NULL

                    dataRowAShipperConsigneeType["DefaultPayMode"] = DBNull.Value;

                    #endregion DefaultPayMode

                    #region StockAlertDate [varchar] (20) NULL

                    dataRowAShipperConsigneeType["StockAlertDate"] = DBNull.Value;

                    #endregion StockAlertDate

                    #region AllowedPayMode [varchar] (30) NULL

                    dataRowAShipperConsigneeType["AllowedPayMode"] = DBNull.Value;

                    #endregion AllowedPayMode

                    #region SITAAddress [varchar] (80) NULL

                    dataRowAShipperConsigneeType["SITAAddress"] = DBNull.Value;

                    #endregion SITAAddress

                    #region OPSMail [varchar] (80) NULL

                    dataRowAShipperConsigneeType["OPSMail"] = DBNull.Value;

                    #endregion OPSMail

                    #region UOM [varchar] (3) NULL

                    dataRowAShipperConsigneeType["UOM"] = DBNull.Value;

                    #endregion UOM

                    #region IsCASSAgent [bit] NULL

                    dataRowAShipperConsigneeType["IsCASSAgent"] = DBNull.Value;

                    #endregion IsCASSAgent

                    #region AdditionalCredit [float] NULL

                    dataRowAShipperConsigneeType["AdditionalCredit"] = DBNull.Value;

                    #endregion AdditionalCredit

                    #region AgentTypeATADTD [varchar] (10) NULL

                    dataRowAShipperConsigneeType["AgentTypeATADTD"] = DBNull.Value;

                    #endregion AgentTypeATADTD

                    #region StockController [varchar] (50) NULL

                    dataRowAShipperConsigneeType["StockController"] = DBNull.Value;

                    #endregion StockController

                    #region IsGSA [bit] NULL

                    dataRowAShipperConsigneeType["IsGSA"] = DBNull.Value;

                    #endregion IsGSA

                    #region Latitude [varchar] (30) NULL

                    dataRowAShipperConsigneeType["Latitude"] = DBNull.Value;

                    #endregion Latitude

                    #region Longitude [varchar] (30) NULL

                    dataRowAShipperConsigneeType["Longitude"] = DBNull.Value;

                    #endregion Longitude

                    #region IsRA3Designated [bit] NULL

                    dataRowAShipperConsigneeType["IsRA3Designated"] = DBNull.Value;

                    #endregion IsRA3Designated

                    #region OpsFromTime [varchar] (10) NULL

                    dataRowAShipperConsigneeType["OpsFromTime"] = DBNull.Value;

                    #endregion OpsFromTime

                    #region OpsToTime [varchar] (10) NULL

                    dataRowAShipperConsigneeType["OpsToTime"] = DBNull.Value;

                    #endregion OpsToTime

                    #region DealPLIAppliedTo [varchar] (10) NULL

                    dataRowAShipperConsigneeType["DealPLIAppliedTo"] = DBNull.Value;

                    #endregion DealPLIAppliedTo

                    #region Website [varchar] (100) NULL

                    dataRowAShipperConsigneeType["Website"] = DBNull.Value;

                    #endregion Website

                    #region ShipperType [varchar] (20) NULL

                    dataRowAShipperConsigneeType["ShipperType"] = DBNull.Value;

                    #endregion ShipperType

                    #region BusinessType [varchar] (20) NULL

                    dataRowAShipperConsigneeType["BusinessType"] = DBNull.Value;

                    #endregion BusinessType

                    #region IndustryFocus [varchar] (20) NULL

                    dataRowAShipperConsigneeType["IndustryFocus"] = DBNull.Value;

                    #endregion IndustryFocus

                    #region SecurityType [varchar] (10) NULL

                    dataRowAShipperConsigneeType["SecurityType"] = DBNull.Value;

                    #endregion SecurityType

                    #region ClerkIdentificationCode [varchar] (30) NULL

                    dataRowAShipperConsigneeType["ClerkIdentificationCode"] = DBNull.Value;

                    #endregion ClerkIdentificationCode

                    #region POMandatory [bit] NULL

                    dataRowAShipperConsigneeType["POMandatory"] = DBNull.Value;

                    #endregion POMandatory

                    #region KnownShipperNumber [varchar] (30) NULL

                    dataRowAShipperConsigneeType["KnownShipperNumber"] = DBNull.Value;

                    #endregion KnownShipperNumber

                    #region RequestKnownShipper [bit] NULL

                    dataRowAShipperConsigneeType["RequestKnownShipper"] = DBNull.Value;

                    #endregion RequestKnownShipper

                    #region Notification [varchar] (350) NULL

                    dataRowAShipperConsigneeType["Notification"] = DBNull.Value;

                    #endregion Notification

                    #region InvoiceType [varchar] (10) NULL

                    dataRowAShipperConsigneeType["InvoiceType"] = DBNull.Value;

                    #endregion InvoiceType

                    #region InvoiceType [varchar] (10) NULL

                    dataRowAShipperConsigneeType["InvoiceEntity"] = DBNull.Value;

                    #endregion InvoiceType

                    #endregion Un-used fields in Agent Master for ShipperConsignee

                    #endregion Create row for ShipperConsigneeType Data Table

                    dataRowAShipperConsigneeType["ValidationDetailsAgent"] = validationDetailsShipperConsignee;
                    ShipperConsigneeType.Rows.Add(dataRowAShipperConsigneeType);
                }

                // Database Call to Validate & Insert/Update ShipperConsignee Master
                string errorInSp = string.Empty;
                await ValidateAndInsertUpdateShipperConsigneeMaster(srNotblMasterUploadSummaryLog, ShipperConsigneeType, errorInSp);

                return true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Message: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", ex.Message, ex.StackTrace);
                return false;
            }
            finally
            {
                dataTableShipperConsigneeExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting ShipperConsignee Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="agentType"> Agent Master Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet?> ValidateAndInsertUpdateShipperConsigneeMaster(int srNotblMasterUploadSummaryLog, DataTable shipperConsigneeType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = [ 
                    new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                    new SqlParameter("@ShipperConsigneeTableType", shipperConsigneeType),
                    new SqlParameter("@Error", errorInSp)
                ];

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspUploadShipperConsigneeMaster", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspUploadShipperConsigneeMaster", sqlParameters);

                return dataSetResult;
            }
            catch (Exception ex)
            {
                //  .WriteLogAzure("Message: " + ex.Message + " Stack Trace: " + ex.StackTrace);
                _logger.LogError("Message: {message} Stack Trace: {stackTrace}", ex.Message, ex.StackTrace);
                return dataSetResult;
            }
        }
    }
}
