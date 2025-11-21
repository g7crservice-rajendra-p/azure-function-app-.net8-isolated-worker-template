
using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;


namespace QidWorkerRole.UploadMasters.Agent
{
    /// <summary>
    /// Class to Upload Agent (General Info only) Master File.
    /// </summary>
    public class UploadAgentMasterGeneralInfo
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadAgentMasterGeneralInfo> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;
        #region Constructor
        public UploadAgentMasterGeneralInfo(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadAgentMasterGeneralInfo> logger,
            Func<UploadMasterCommon> uploadMasterCommonFactory
            )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod Agent Master (General Info only).
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<Boolean> AgentMasterUploadGeneralInfo(DataSet dataSetFileData)
        {
            try
            {
                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        //if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "AgentMasterGeneralInfoUploadFile", out uploadFilePath))
                        if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "AgentMasterGeneralInfoUploadFile", out uploadFilePath))
                        {
                            //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                            //uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);

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
        /// Method to Process Agent (General Info only) Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Agent Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public async Task<bool> ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableAgentGeneralInfoExcelData = new DataTable("dataTableAgentGeneralInfoExcelData");

            bool isBinaryReader = false;

            try
            {
                //New latest package ExcelDatareader 3.8.0 and ExcelDataReader.Dataset 3.8.0 isnstalled so old code commeneted and added new Code
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);



                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".xlsb") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                 : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTableAgentGeneralInfoExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                //uploadMasterCommon.RemoveEmptyRows(dataTableAgentGeneralInfoExcelData);
                _uploadMasterCommonFactory().RemoveEmptyRows(dataTableAgentGeneralInfoExcelData);

                foreach (DataColumn dataColumn in dataTableAgentGeneralInfoExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableAgentGeneralInfoExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating AgentType DataTable

                DataTable AgentType = new DataTable("AgentType");
                AgentType.Columns.Add("AgentIndex", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("AgentCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IATAAgentCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AgentName", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ValidFrom", System.Type.GetType("System.DateTime"));
                AgentType.Columns.Add("ValidTo", System.Type.GetType("System.DateTime"));
                AgentType.Columns.Add("CustomerCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("Station", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AirlineCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("Country", System.Type.GetType("System.String"));
                AgentType.Columns.Add("City", System.Type.GetType("System.String"));
                AgentType.Columns.Add("EORINo", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AgentTypeId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("SalesId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("HoldingCompany", System.Type.GetType("System.String"));
                AgentType.Columns.Add("GSATypeId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("CommTypeId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("Fixed", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("Percentage", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("Remarks", System.Type.GetType("System.String"));
                AgentType.Columns.Add("CurrencyCodeId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("CurrencyNumber", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("SettlCurr", System.Type.GetType("System.String"));
                AgentType.Columns.Add("CassId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("ReportingPeriodId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("CrdtLmt", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("BillingPeriod", System.Type.GetType("System.String"));
                AgentType.Columns.Add("NorrmalCommPercent", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("BillToId", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("AgentAccountCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("TolarancePercent", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("TolaranceValue", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("MaxValue", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("ContactPerson", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("OfficeAddress1", System.Type.GetType("System.String"));
                AgentType.Columns.Add("State", System.Type.GetType("System.String"));
                AgentType.Columns.Add("MobileNumber", System.Type.GetType("System.String"));
                AgentType.Columns.Add("PostalZIP", System.Type.GetType("System.String"));
                AgentType.Columns.Add("Phone1", System.Type.GetType("System.String"));
                AgentType.Columns.Add("FAX", System.Type.GetType("System.String"));
                AgentType.Columns.Add("Email", System.Type.GetType("System.String"));
                AgentType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                AgentType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                AgentType.Columns.Add("Remark", System.Type.GetType("System.String"));
                AgentType.Columns.Add("PersonContact", System.Type.GetType("System.String"));
                AgentType.Columns.Add("CurrentAWBno", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ControllingLocator", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AccountCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("TDSOnCommision", System.Type.GetType("System.String"));
                AgentType.Columns.Add("TDSOnFreight", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ControllingLocatorCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BuildTo", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AccountMail", System.Type.GetType("System.String"));
                AgentType.Columns.Add("SalesMail", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AgentType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("PanCardNumber", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ServiceTaxNumber", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ValidBG", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsFOC", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("CurrencyCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("threshold", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AgentReferenceCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("TresholdLimitDays", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("RatePreference", System.Type.GetType("System.String"));
                AgentType.Columns.Add("GLCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AutoGenerateAgentInvoice", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("AllowedStations", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ExcludeFromFBL", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("IACCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("CCSFCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("isKnownShipper", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("InvoiceDueDays", System.Type.GetType("System.Int32"));
                AgentType.Columns.Add("AgentCreditAmount", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("AgentInvoiceBalance", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("AgentCreditRemaining", System.Type.GetType("System.Decimal"));
                AgentType.Columns.Add("TAXExemption", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("DefaultPayMode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("StockAlertDate", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AllowedPayMode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("DBA", System.Type.GetType("System.String"));
                AgentType.Columns.Add("KnownShipperExpDt", System.Type.GetType("System.DateTime"));
                AgentType.Columns.Add("SSPCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsConsole", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("ParticipationType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("OfficeAddress2", System.Type.GetType("System.String"));
                AgentType.Columns.Add("SITAAddress", System.Type.GetType("System.String"));
                AgentType.Columns.Add("KnownShipperValidFrom", System.Type.GetType("System.DateTime"));
                AgentType.Columns.Add("OPSMail", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsSameAddress", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("BillingAddress1", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingAddress2", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingCity", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingState", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingZipCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingCountry", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingContactPerson", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BillingPhNo", System.Type.GetType("System.String"));
                AgentType.Columns.Add("UOM", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsCASSAgent", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("AdditionalCredit", System.Type.GetType("System.Single"));
                AgentType.Columns.Add("AgentTypeATADTD", System.Type.GetType("System.String"));
                AgentType.Columns.Add("StockController", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsGSA", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("Latitude", System.Type.GetType("System.String"));
                AgentType.Columns.Add("Longitude", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IsRA3Designated", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("OpsFromTime", System.Type.GetType("System.String"));
                AgentType.Columns.Add("OpsToTime", System.Type.GetType("System.String"));
                AgentType.Columns.Add("DealPLIAppliedTo", System.Type.GetType("System.String"));
                AgentType.Columns.Add("Website", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ShipperType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("BusinessType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("IndustryFocus", System.Type.GetType("System.String"));
                AgentType.Columns.Add("SecurityType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ClerkIdentificationCode", System.Type.GetType("System.String"));
                AgentType.Columns.Add("POMandatory", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("KnownShipperNumber", System.Type.GetType("System.String"));
                AgentType.Columns.Add("RequestKnownShipper", System.Type.GetType("System.Byte"));
                AgentType.Columns.Add("Notification", System.Type.GetType("System.String"));
                AgentType.Columns.Add("InvoiceType", System.Type.GetType("System.String"));
                AgentType.Columns.Add("ValidationDetailsAgent", System.Type.GetType("System.String"));
                AgentType.Columns.Add("AllowedCarriers", System.Type.GetType("System.String"));

                #endregion Creating AgentType DataTable

                string validationDetailsAgent = string.Empty;
                DateTime tempDate;
                short tempIntValue = 0;
                decimal tempDecimalValue = 0;
                //float tempFloatValue = 0;

                for (int i = 0; i < dataTableAgentGeneralInfoExcelData.Rows.Count; i++)
                {
                    validationDetailsAgent = string.Empty;

                    #region Create row for AgentType Data Table

                    DataRow dataRowAgentType = AgentType.NewRow();

                    dataRowAgentType["AgentIndex"] = i + 1;
                    dataRowAgentType["SerialNumber"] = 0;

                    #region AgentCode* [varchar] (20) NULL

                    if (columnNames.Contains("agent_code*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentCode"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " AGENT_CODE is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["AgentCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " AGENT_CODE is more than 20 Chars;";
                            }
                            else
                            {
                                if (AgentType.Select("AgentCode = ''").Length == 0)
                                {
                                    if (AgentType.Select(string.Format("AgentCode = '{0}'", dataTableAgentGeneralInfoExcelData.Rows[i]["agent_code*"].ToString().Trim().Trim(','))).Length == 0)
                                    {
                                        dataRowAgentType["AgentCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["agent_code*"].ToString().Trim().Trim(',');
                                    }
                                    else
                                    {
                                        dataRowAgentType["AgentCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["agent_code*"].ToString().Trim().Trim(',');
                                        validationDetailsAgent = validationDetailsAgent + " Duplicate AGENT_CODE;";
                                    }
                                }
                                else
                                {
                                    dataRowAgentType["AgentCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["agent_code*"].ToString().Trim().Trim(',');
                                }
                            }
                        }
                    }

                    #endregion AgentCode

                    #region AgentName* [varchar] (125) NULL

                    if (columnNames.Contains("agent_name*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentName"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " AgentName is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_name*"].ToString().Trim().Trim(',').Length > 125)
                            {
                                dataRowAgentType["AgentName"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " AgentName is more than 125 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AgentName"] = dataTableAgentGeneralInfoExcelData.Rows[i]["agent_name*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AgentName

                    #region AllowedCarriers* [varchar] (50) NULL

                    if (columnNames.Contains("allowed_carriers*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["allowed_carriers*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AllowedCarriers"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " Allowed Carriers is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["allowed_carriers*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["AllowedCarriers"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " Allowed Carriers is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AllowedCarriers"] = dataTableAgentGeneralInfoExcelData.Rows[i]["allowed_carriers*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AllowedCarriers

                    #region CustomerCode [varchar] (20) NULL

                    if (columnNames.Contains("customer_code"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["customer_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CustomerCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["customer_code"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["CustomerCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " CustomerCode is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["CustomerCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["customer_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion CustomerCode

                    #region IATAAgentCode [varchar] (20) NULL

                    if (columnNames.Contains("iata_agent_code"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["iata_agent_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IATAAgentCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["iata_agent_code"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["IATAAgentCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " IATAAgentCode is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["IATAAgentCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["iata_agent_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion IATAAgentCode

                    #region Station* [varchar] (10) NULL

                    if (columnNames.Contains("airport_code*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["airport_code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Station"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " AIRPORT_CODE is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["airport_code*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["Station"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " AIRPORT_CODE is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["Station"] = dataTableAgentGeneralInfoExcelData.Rows[i]["airport_code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Station*

                    #region ShipperType [varchar] (20) NULL

                    dataRowAgentType["ShipperType"] = "Company";

                    #endregion ShipperType

                    #region BusinessType [varchar] (20) NULL
                    
                    dataRowAgentType["BusinessType"] = "AIR";

                    #endregion BusinessType

                    #region IndustryFocus [varchar] (20) NULL

                    dataRowAgentType["IndustryFocus"] = "AC";

                    #endregion IndustryFocus

                    #region OPSMail [varchar] (80) NULL

                    if (columnNames.Contains("ops_email"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["ops_email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["OPSMail"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["ops_email"].ToString().Trim().Trim(',').Length > 80)
                            {
                                dataRowAgentType["OPSMail"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " OPS_EMAIL is more than 80 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["OPSMail"] = dataTableAgentGeneralInfoExcelData.Rows[i]["ops_email"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion OPSMail

                    #region AccountMail [varchar] (500) NULL
                    
                    dataRowAgentType["AccountMail"] = DBNull.Value;

                    #endregion AccountMail

                    #region SalesMail [varchar] (500) NULL

                    dataRowAgentType["SalesMail"] = DBNull.Value;

                    #endregion SalesMail

                    #region PanCardNumber [varchar] (50) NULL

                    dataRowAgentType["PanCardNumber"] = DBNull.Value;

                    #endregion PanCardNumber

                    #region ServiceTaxNumber [varchar] (50) NULL

                    dataRowAgentType["ServiceTaxNumber"] = DBNull.Value;

                    #endregion ServiceTaxNumber

                    #region Website [varchar] (100) NULL

                    dataRowAgentType["Website"] = DBNull.Value;

                    #endregion Website

                    #region SITAAddress [varchar] (80) NULL

                    dataRowAgentType["SITAAddress"] = DBNull.Value;

                    #endregion SITAAddress

                    #region DBA [varchar] (100) NULL

                    dataRowAgentType["DBA"] = DBNull.Value;

                    #endregion DBA

                    #region ContactPerson [int] NULL

                    if (columnNames.Contains("contact_person_no"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["contact_person_no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["ContactPerson"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralInfoExcelData.Rows[i]["contact_person_no"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["ContactPerson"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid CONTACT_PERSON_NO;";
                                dataRowAgentType["ContactPerson"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion ContactPerson

                    #region MobileNumber [varchar] (50) NULL

                    if (columnNames.Contains("mobile_number"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["MobileNumber"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["MobileNumber"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " MOBILE_NUMBER is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["MobileNumber"] = dataTableAgentGeneralInfoExcelData.Rows[i]["mobile_number"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion MobileNumber

                    #region Notification [varchar] (350) NULL

                    dataRowAgentType["Notification"] = DBNull.Value;

                    #endregion Notification

                    #region OfficeAddress1* [varchar] (125) NULL

                    if (columnNames.Contains("address1*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["OfficeAddress1"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " ADDRESS1 is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',').Length > 125)
                            {
                                dataRowAgentType["OfficeAddress1"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " ADDRESS1 is more than 125 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["OfficeAddress1"] = dataTableAgentGeneralInfoExcelData.Rows[i]["address1*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion OfficeAddress1*

                    #region OfficeAddress2 [varchar] (125) NULL

                    if (columnNames.Contains("address2"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["address2"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["OfficeAddress2"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["address2"].ToString().Trim().Trim(',').Length > 125)
                            {
                                dataRowAgentType["OfficeAddress2"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " ADDRESS2 is more than 125 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["OfficeAddress2"] = dataTableAgentGeneralInfoExcelData.Rows[i]["address2"].ToString().Trim();
                            }
                        }
                    }

                    #endregion OfficeAddress2

                    #region City* [varchar] (50) NULL

                    if (columnNames.Contains("city*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["City"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " CITY is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["City"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " CITY is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["City"] = dataTableAgentGeneralInfoExcelData.Rows[i]["city*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion City*

                    #region State* [varchar] (20) NULL

                    if (columnNames.Contains("state*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["state*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["State"] = DBNull.Value;
                            validationDetailsAgent = validationDetailsAgent + " State is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["state*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["State"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " STATE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["State"] = dataTableAgentGeneralInfoExcelData.Rows[i]["state*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion State

                    #region Country* [varchar] (20) NULL

                    if (columnNames.Contains("country*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Country"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " COUNTRY is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["Country"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " COUNTRY is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["Country"] = dataTableAgentGeneralInfoExcelData.Rows[i]["country*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Country*

                    #region PostalZIP* [varchar] (20) NULL

                    if (columnNames.Contains("zip_code*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["zip_code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["PostalZIP"] = DBNull.Value;
                            validationDetailsAgent = validationDetailsAgent + " ZIP Code is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["zip_code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["PostalZIP"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " ZIP_CODE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["PostalZIP"] = dataTableAgentGeneralInfoExcelData.Rows[i]["zip_code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion PostalZIP

                    #region Phone1* [varchar] (20) NULL

                    if (columnNames.Contains("phone_no*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Phone1"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " PHONE_NO is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["Phone1"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " PHONE_NO is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["Phone1"] = dataTableAgentGeneralInfoExcelData.Rows[i]["phone_no*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Phone1*

                    #region FAX [varchar] (50) NULL

                    if (columnNames.Contains("fax"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["fax"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["FAX"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["fax"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["FAX"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " FAX is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["FAX"] = dataTableAgentGeneralInfoExcelData.Rows[i]["fax"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion FAX

                    #region AgentTypeATADTD [varchar] (10) NULL

                    dataRowAgentType["AgentTypeATADTD"] = "ATA";

                    #endregion AgentTypeATADTD

                    #region IsSameAddress [bit] NULL

                    if (columnNames.Contains("is_billing_same_as_mailing_address"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_billing_same_as_mailing_address"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IsSameAddress"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_billing_same_as_mailing_address"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                    dataRowAgentType["IsSameAddress"] = 1;
                                    break;
                                case "N":
                                    dataRowAgentType["IsSameAddress"] = 0;
                                    break;
                                default:
                                    dataRowAgentType["IsSameAddress"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + " IS_BILLING_SAME_AS_MAILING_ADDRESS is Invalid;";
                                    break;
                            }
                        }
                    }

                    if (dataRowAgentType["IsSameAddress"].ToString().Equals("1"))
                    {
                        dataRowAgentType["BillingAddress1"] = dataRowAgentType["OfficeAddress1"];
                        dataRowAgentType["BillingAddress2"] = dataRowAgentType["OfficeAddress2"];
                        dataRowAgentType["BillingCity"] = dataRowAgentType["City"];
                        dataRowAgentType["BillingState"] = dataRowAgentType["State"];
                        dataRowAgentType["BillingZipCode"] = dataRowAgentType["PostalZIP"];
                        dataRowAgentType["BillingCountry"] = dataRowAgentType["Country"];
                        dataRowAgentType["BillingContactPerson"] = dataRowAgentType["PersonContact"];
                        dataRowAgentType["BillingPhNo"] = dataRowAgentType["Phone1"];
                    }
                    else
                    {
                        #region BillingAddress1 [varchar] (125) NULL

                        if (columnNames.Contains("billing_address1"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_address1"].ToString().Trim().Equals(string.Empty))
                            {
                                dataRowAgentType["BillingAddress1"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_address1"].ToString().Trim().Length > 125)
                                {
                                    dataRowAgentType["BillingAddress1"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_ADDRESS1 is more than 125 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingAddress1"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_address1"].ToString().Trim();
                                }
                            }
                        }

                        #endregion BillingAddress1

                        #region BillingAddress2 [varchar] (125) NULL

                        if (columnNames.Contains("billing_address2"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_address2"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingAddress2"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_address2"].ToString().Trim().Trim(',').Length > 125)
                                {
                                    dataRowAgentType["BillingAddress2"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_ADDRESS2 is more than 125 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingAddress2"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_address2"].ToString().Trim();
                                }
                            }
                        }

                        #endregion BillingAddress2

                        #region BillingCity [varchar] (50) NULL

                        if (columnNames.Contains("billing_city"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingCity"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',').Length > 50)
                                {
                                    dataRowAgentType["BillingCity"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_CITY is more than 50 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingCity"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_city"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingCity

                        #region BillingState [varchar] (20) NULL

                        if (columnNames.Contains("billing_state"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingState"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',').Length > 20)
                                {
                                    dataRowAgentType["BillingState"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_STATE is more than 20 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingState"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_state"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingState

                        #region BillingCountry [varchar] (30) NULL

                        if (columnNames.Contains("billing_country"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingCountry"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',').Length > 30)
                                {
                                    dataRowAgentType["BillingCountry"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_COUNTRY is more than 30 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingCountry"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_country"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingCountry

                        #region BillingZipCode [varchar] (20) NULL

                        if (columnNames.Contains("billing_zip_code"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingZipCode"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',').Length > 20)
                                {
                                    dataRowAgentType["BillingZipCode"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_ZIP_CODE is more than 20 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingZipCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_zip_code"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingZipCode

                        #region BillingContactPerson [varchar] (100) NULL

                        if (columnNames.Contains("billing_contact_person"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingContactPerson"] = DBNull.Value;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',').Length > 100)
                                {
                                    dataRowAgentType["BillingContactPerson"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_CONTACT_PERSON is more than 100 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingContactPerson"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_contact_person"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingContactPerson

                        #region BillingPhNo [varchar] (20) NULL

                        if (columnNames.Contains("billing_phone_no"))
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',').Equals(string.Empty))
                            {
                                dataRowAgentType["BillingPhNo"] = string.Empty;
                            }
                            else
                            {
                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',').Length > 20)
                                {
                                    dataRowAgentType["BillingPhNo"] = string.Empty;
                                    validationDetailsAgent = validationDetailsAgent + " BILLING_PHONE_NO is more than 20 Chars;";
                                }
                                else
                                {
                                    dataRowAgentType["BillingPhNo"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_phone_no"].ToString().Trim().Trim(',');
                                }
                            }
                        }

                        #endregion BillingPhNo
                    }

                    #endregion IsSameAddress

                    #region ValidFrom* [datetime] NULL

                    if (columnNames.Contains("valid_from*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["valid_from*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsAgent = validationDetailsAgent + " ValidFrom is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAgentType["ValidFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTableAgentGeneralInfoExcelData.Rows[i]["valid_from*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAgentGeneralInfoExcelData.Rows[i]["valid_from*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAgentType["ValidFrom"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAgentType["ValidFrom"] = DateTime.Now;
                                        validationDetailsAgent = validationDetailsAgent + "Invalid ValidFrom;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAgentType["ValidFrom"] = DateTime.Now;
                                validationDetailsAgent = validationDetailsAgent + "Invalid ValidFrom;";
                            }
                        }
                    }

                    #endregion ValidFrom*

                    #region ValidTo* [datetime] NULL,

                    if (columnNames.Contains("valid_to*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["valid_to*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsAgent = validationDetailsAgent + " ValidTo is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowAgentType["ValidTo"] = DateTime.FromOADate(Convert.ToDouble(dataTableAgentGeneralInfoExcelData.Rows[i]["valid_to*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAgentGeneralInfoExcelData.Rows[i]["valid_to*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowAgentType["ValidTo"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowAgentType["ValidTo"] = DateTime.Now;
                                        validationDetailsAgent = validationDetailsAgent + "Invalid ValidTo;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAgentType["ValidTo"] = DateTime.Now;
                                validationDetailsAgent = validationDetailsAgent + "Invalid ValidTo;";
                            }
                        }
                    }

                    #endregion ValidTo*

                    #region ParticipationType* [varchar] (10) NULL

                    if (columnNames.Contains("participation_type*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["participation_type*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["ParticipationType"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " PARTICIPATION_TYPE is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["participation_type*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["ParticipationType"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " PARTICIPATION_TYPE is more than 10 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["participation_type*"].ToString().ToUpper().Trim().Trim(','))
                                {
                                    case "A":
                                    case "AS":
                                    case "NC":
                                        dataRowAgentType["ParticipationType"] = dataTableAgentGeneralInfoExcelData.Rows[i]["participation_type*"].ToString().ToUpper().Trim().Trim(',');
                                        break;
                                    default:
                                        dataRowAgentType["ParticipationType"] = string.Empty;
                                        validationDetailsAgent = validationDetailsAgent + " PARTICIPATION_TYPE is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion ParticipationType*

                    #region StockController* [varchar] (50) NULL

                    if (columnNames.Contains("is_stock_controller*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_stock_controller*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["StockController"] = DBNull.Value;
                            validationDetailsAgent = validationDetailsAgent + " IS_STOCK_CONTROLLER is required;";
                        }
                        else
                        {
                            switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_stock_controller*"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "YES":
                                case "Y":
                                    if (dataTableAgentGeneralInfoExcelData.Rows[i]["stock_controller_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                                    {
                                        dataRowAgentType["StockController"] = DBNull.Value;
                                        validationDetailsAgent = validationDetailsAgent + " IS_STOCK_CONTROLLER is set to Yes hence, STOCK_CONTROLLER_CODE is required;";
                                    }
                                    else
                                    {
                                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["stock_controller_code"].ToString().Trim().Trim(',').Length > 50)
                                        {
                                            dataRowAgentType["StockController"] = string.Empty;
                                            validationDetailsAgent = validationDetailsAgent + " STOCK_CONTROLLER_CODE is more than 50 Chars;";
                                        }
                                        else
                                        {
                                            dataRowAgentType["StockController"] = dataTableAgentGeneralInfoExcelData.Rows[i]["stock_controller_code"].ToString().Trim().Trim(',');
                                        }
                                    }
                                    break;
                                case "NO":
                                case "N":
                                    dataRowAgentType["StockController"] = DBNull.Value;
                                    break;
                                default:
                                    validationDetailsAgent = validationDetailsAgent + " Invalid IS_STOCK_CONTROLLER;";
                                    break;
                            }
                        }
                    }

                    #endregion StockController

                    #region IsGSA [bit] NULL

                    if (columnNames.Contains("is_gsa"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_gsa"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IsGSA"] = 0;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_gsa"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowAgentType["IsGSA"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " IS_GSA is more than 3 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_gsa"].ToString().Trim().ToLower())
                                {
                                    case "yes":
                                    case "y":
                                        dataRowAgentType["IsGSA"] = 1;
                                        break;
                                    case "no":
                                    case "n":
                                        dataRowAgentType["IsGSA"] = 0;
                                        break;
                                    default:
                                        dataRowAgentType["IsGSA"] = 0;
                                        validationDetailsAgent = validationDetailsAgent + " Invalid IS_GSA;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion IsGSA

                    #region IsRA3Designated [bit] NULL

                    if (columnNames.Contains("is_ra3_designated"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_ra3_designated"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IsRA3Designated"] = 0;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_ra3_designated"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowAgentType["IsRA3Designated"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " IS_RA3_DESIGNATED is more than 3 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_ra3_designated"].ToString().Trim().ToLower())
                                {
                                    case "yes":
                                    case "y":
                                        dataRowAgentType["IsRA3Designated"] = 1;
                                        break;
                                    case "no":
                                    case "n":
                                        dataRowAgentType["IsRA3Designated"] = 0;
                                        break;
                                    default:
                                        dataRowAgentType["IsRA3Designated"] = 0;
                                        validationDetailsAgent = validationDetailsAgent + " Invalid IS_RA3_DESIGNATED;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion IsRA3Designated

                    #region BuildTo* [varchar] (25) NULL

                    if (columnNames.Contains("bill_to*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["bill_to*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["BuildTo"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " BILL_TO is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["bill_to*"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowAgentType["BuildTo"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " BILL_TO is more than 25 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["bill_to*"].ToString().ToLower().Trim().Trim(','))
                                {
                                    case "self":
                                        dataRowAgentType["BuildTo"] = dataTableAgentGeneralInfoExcelData.Rows[i]["bill_to*"].ToString().Trim().Trim(',');
                                        break;
                                    case "controlling locator":
                                        dataRowAgentType["BuildTo"] = dataTableAgentGeneralInfoExcelData.Rows[i]["bill_to*"].ToString().Trim().Trim(',');

                                        #region ControllingLocatorCode [varchar] (25) NULL

                                        if (columnNames.Contains("billing_controller_code"))
                                        {
                                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_controller_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                                            {
                                                dataRowAgentType["ControllingLocatorCode"] = DBNull.Value;
                                                validationDetailsAgent = validationDetailsAgent + " Bill_TO is set to 'Controlling Locator' hence BILLING_CONTROLLER_CODE is required;";
                                            }
                                            else
                                            {
                                                if (dataTableAgentGeneralInfoExcelData.Rows[i]["billing_controller_code"].ToString().Trim().Trim(',').Length > 25)
                                                {
                                                    dataRowAgentType["ControllingLocatorCode"] = DBNull.Value;
                                                    validationDetailsAgent = validationDetailsAgent + " BILLING_CONTROLLER_CODE is more than 25 Chars;";
                                                }
                                                else
                                                {
                                                    dataRowAgentType["ControllingLocatorCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["billing_controller_code"].ToString().Trim().Trim(',');
                                                }
                                            }
                                        }
                                        else
                                        {
                                            validationDetailsAgent = validationDetailsAgent + " Bill_TO is set to 'Controlling Locator' hence BILLING_CONTROLLER_CODE column is required with value;";
                                        }

                                        #endregion ControllingLocatorCode

                                        break;
                                    default:
                                        dataRowAgentType["BuildTo"] = string.Empty;
                                        validationDetailsAgent = validationDetailsAgent + " BILL_TO is Invalid;";
                                        dataRowAgentType["ControllingLocatorCode"] = DBNull.Value;
                                        break;
                                }
                            }
                        }
                    }

                    #endregion BuildTo*

                    #region AdditionalCredit [float] NULL

                    dataRowAgentType["AdditionalCredit"] = DBNull.Value;

                    #endregion AdditionalCredit

                    #region GLCode [varchar] (10) NULL

                    if (columnNames.Contains("gl_code"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["gl_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["GLCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["gl_code"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["GLCode"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " GL_CODE is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["GLCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["gl_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion GLCode

                    #region BillType* [varchar] (50) NULL

                    if (columnNames.Contains("bill_type*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["bill_type*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["BillType"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " BILL_TYPE is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["bill_type*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["BillType"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " BILL_TYPE is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["BillType"] = dataTableAgentGeneralInfoExcelData.Rows[i]["bill_type*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion BillType*

                    #region ClerkIdentificationCode [varchar] (30) NULL

                    dataRowAgentType["ClerkIdentificationCode"] = DBNull.Value;

                    #endregion ClerkIdentificationCode

                    #region NorrmalCommPercent [decimal] (18, 2) NULL

                    if (columnNames.Contains("commission_percent"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["commission_percent"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["NorrmalCommPercent"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralInfoExcelData.Rows[i]["commission_percent"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["NorrmalCommPercent"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["NorrmalCommPercent"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid COMMISSION_PERCENT;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["NorrmalCommPercent"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid COMMISSION_PERCENT;";
                            }
                        }
                    }

                    #endregion NorrmalCommPercent

                    #region threshold [varchar] (20) NULL

                    if (columnNames.Contains("stock_threshold_alert"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["stock_threshold_alert"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["threshold"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["stock_threshold_alert"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["threshold"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " STOCK_THRESHOLD_ALERT is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["threshold"] = dataTableAgentGeneralInfoExcelData.Rows[i]["stock_threshold_alert"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion threshold

                    #region InvoiceType [varchar] (200) NULL

                    dataRowAgentType["InvoiceType"] = "CASH";

                    #endregion InvoiceType

                    #region CurrencyCode* [varchar] (20) NULL

                    if (columnNames.Contains("currency_code*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["currency_code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CurrencyCode"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " CURRENCY_CODE is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["currency_code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["CurrencyCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " CURRENCY_CODE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["CurrencyCode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["currency_code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion CurrencyCode*

                    #region AgentType [varchar] (50) NULL

                    if (columnNames.Contains("agent_type"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_type"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentType"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_type"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["AgentType"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " AGENT_TYPE is more than 50 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["agent_type"].ToString().Trim().ToLower())
                                {
                                    case "domestic":
                                    case "international":
                                        dataRowAgentType["AgentType"] = dataTableAgentGeneralInfoExcelData.Rows[i]["agent_type"].ToString().Trim();
                                        break;
                                    default:
                                        dataRowAgentType["AgentType"] = DBNull.Value;
                                        validationDetailsAgent = validationDetailsAgent + " Invalid AGENT_TYPE;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion AgentType

                    #region DealPLIAppliedTo [varchar] (10) NULL

                    if (columnNames.Contains("deal_pli_applied_to"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["deal_pli_applied_to"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["DealPLIAppliedTo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["deal_pli_applied_to"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["DealPLIAppliedTo"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " DEAL_PLI_APPLIED_TO is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["DealPLIAppliedTo"] = dataTableAgentGeneralInfoExcelData.Rows[i]["deal_pli_applied_to"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion DealPLIAppliedTo

                    #region InvoiceDueDays* [int] NULL

                    if (columnNames.Contains("invoice_due_days*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["invoice_due_days*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["InvoiceDueDays"] = DBNull.Value;
                            validationDetailsAgent = validationDetailsAgent + " INVOICE_DUE_DAYS is required;";
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralInfoExcelData.Rows[i]["invoice_due_days*"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["InvoiceDueDays"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid INVOICE_DUE_DAYS;";
                                dataRowAgentType["InvoiceDueDays"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion InvoiceDueDays

                    #region DefaultPayMode [varchar] (10) NULL

                    if (columnNames.Contains("default_pay_mode"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["default_pay_mode"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["DefaultPayMode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["default_pay_mode"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["DefaultPayMode"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " DEFAULT_PAY_MODE is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["DefaultPayMode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["default_pay_mode"].ToString().Trim().Trim(',');
                            }
                        }
                    }





                    #endregion DefaultPayMode

                    #region AllowedPayMode [varchar] (30) NULL

                    if (columnNames.Contains("allowed_pay_mode"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["allowed_pay_mode"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AllowedPayMode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["allowed_pay_mode"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["AllowedPayMode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " ALLOWED_PAY_MODE is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AllowedPayMode"] = dataTableAgentGeneralInfoExcelData.Rows[i]["allowed_pay_mode"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AllowedPayMode

                    #region IsFOC* [bit] NULL

                    if (columnNames.Contains("is_foc*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_foc*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IsFOC"] = 0;
                            validationDetailsAgent = validationDetailsAgent + " IS_FOC is required;";
                        }
                        else
                        {
                            switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_foc*"].ToString().Trim().ToLower())
                            {
                                case "n":
                                case "no":
                                    dataRowAgentType["IsFOC"] = 0;
                                    break;
                                case "yes":
                                case "y":
                                    dataRowAgentType["IsFOC"] = 1;
                                    break;
                                default:
                                    dataRowAgentType["IsFOC"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + " Invalid IS_FOC;";
                                    break;
                            }
                        }
                    }

                    #endregion IsFOC

                    #region ValidBG [varchar] (10) NULL

                    if (columnNames.Contains("validate_credit"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["validate_credit"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["ValidBG"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["validate_credit"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["ValidBG"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " VALIDATE_CREDIT is more than 10 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["validate_credit"].ToString().ToUpper().Trim().Trim(','))
                                {
                                    case "Y":
                                    case "YES":
                                        dataRowAgentType["ValidBG"] = "Yes";
                                        break;
                                    case "N":
                                    case "NO":
                                        dataRowAgentType["ValidBG"] = "No";
                                        break;
                                    default:
                                        dataRowAgentType["ValidBG"] = string.Empty;
                                        validationDetailsAgent = validationDetailsAgent + " VALIDATE_CREDIT is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion ValidBG

                    #region IsCASSAgent [bit] NULL

                    if (columnNames.Contains("is_cass_agent"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_cass_agent"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IsCASSAgent"] = 0;
                        }
                        else
                        {
                            switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_cass_agent"].ToString().Trim().ToLower())
                            {
                                case "yes":
                                case "y":
                                    dataRowAgentType["IsCASSAgent"] = 1;
                                    if (string.IsNullOrWhiteSpace(dataRowAgentType["IATAAgentCode"].ToString()))
                                    {
                                        validationDetailsAgent = validationDetailsAgent + " IATA_AGENT_CODE is required when IS_CASS_AGENT is true;";
                                    }
                                    break;
                                case "no":
                                case "n":
                                    dataRowAgentType["IsCASSAgent"] = 0;
                                    break;
                                default:
                                    dataRowAgentType["IsCASSAgent"] = 0;
                                    validationDetailsAgent = validationDetailsAgent + " Invalid IS_CASS_AGENT;";
                                    break;
                            }
                        }
                    }

                    #endregion IsCASSAgent

                    #region IsConsole [bit] NULL

                    dataRowAgentType["IsConsole"] = DBNull.Value;

                    #endregion IsConsole

                    #region IsActive* [bit] NULL

                    if (columnNames.Contains("is_active*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_active*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IsActive"] = DBNull.Value;
                            validationDetailsAgent = validationDetailsAgent + " IS_ACTIVE is required;";
                        }
                        else
                        {
                            switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_active*"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                    dataRowAgentType["IsActive"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                    dataRowAgentType["IsActive"] = 0;
                                    break;
                                default:
                                    dataRowAgentType["IsActive"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + " IS_ACTIVE is Invalid;";
                                    break;
                            }                            
                        }
                    }

                    #endregion IsActive*

                    #region POMandatory* [bit] NULL

                    dataRowAgentType["POMandatory"] = 0;

                    #endregion POMandatory

                    #region RatePreference* [varchar] (10) NULL

                    if (columnNames.Contains("rate_line_preference*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["rate_line_preference*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["RatePreference"] = DBNull.Value;
                            validationDetailsAgent = validationDetailsAgent + " RATE_LINE_PREFERENCE is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["rate_line_preference*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["RatePreference"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " RATE_LINE_PREFERENCE is more than 10 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralInfoExcelData.Rows[i]["rate_line_preference*"].ToString().ToLower().Trim().Trim(','))
                                {
                                    case "iata":
                                    case "mkt":
                                    case "as agreed":
                                        dataRowAgentType["RatePreference"] = dataTableAgentGeneralInfoExcelData.Rows[i]["rate_line_preference*"].ToString().Trim().Trim(',');
                                        break;
                                    case "select":
                                        dataRowAgentType["RatePreference"] = "AS Agreed";
                                        break;
                                    default:
                                        dataRowAgentType["RatePreference"] = DBNull.Value;
                                        validationDetailsAgent = validationDetailsAgent + " RATE_LINE_PREFERENCE is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion RatePreference

                    #region TAXExemption [bit] NULL

                    if (columnNames.Contains("is_vat_exemption*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_vat_exemption*"].ToString().Trim().ToLower().Equals("y"))
                        {
                            dataRowAgentType["TAXExemption"] = 1;
                        }
                        else
                        {
                            dataRowAgentType["TAXExemption"] = 0;
                        }
                    }

                    #endregion TAXExemption

                    #region AutoGenerateAgentInvoice [bit] NULL

                    if (columnNames.Contains("is_auto_generate_invoice*"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["is_auto_generate_invoice*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AutoGenerateAgentInvoice"] = 0;
                        }
                        else
                        {
                            switch (dataTableAgentGeneralInfoExcelData.Rows[i]["is_auto_generate_invoice*"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                    dataRowAgentType["AutoGenerateAgentInvoice"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                    dataRowAgentType["AutoGenerateAgentInvoice"] = 0;
                                    break;
                                default:
                                    dataRowAgentType["AutoGenerateAgentInvoice"] = 0;
                                    break;
                            }
                        }
                    }

                    #endregion AutoGenerateAgentInvoice

                    #region Remarks [varchar] (50) NULL

                    if (columnNames.Contains("remarks"))
                    {
                        if (dataTableAgentGeneralInfoExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Remark"] = DBNull.Value;
                            dataRowAgentType["Remarks"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralInfoExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["Remark"] = DBNull.Value;
                                dataRowAgentType["Remarks"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " REMARKS is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["Remark"] = dataTableAgentGeneralInfoExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',');
                                dataRowAgentType["Remarks"] = dataRowAgentType["Remark"].ToString();
                            }
                        }
                    }

                    #endregion Remark

                    
                    #region Due to only General Tab fields, these columns are ignored.

                    /*

                    #region AirlineCode [varchar] (10) NULL

                    if (columnNames.Contains("airline_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["airline_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AirlineCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["airline_code"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["AirlineCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " AIRLINE_CODE is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AirlineCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["airline_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AirlineCode

                    #region EORINo [varchar] (30) NULL

                    if (columnNames.Contains("eori_no"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["eori_no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["EORINo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["eori_no"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["EORINo"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " EORI_NO is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["EORINo"] = dataTableAgentGeneralTabExcelData.Rows[i]["eori_no"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion EORINo

                    #region AgentTypeId [int] NULL

                    if (columnNames.Contains("agent_type_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_type_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentTypeId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["agent_type_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["AgentTypeId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid AGENT_TYPE_ID;";
                                dataRowAgentType["AgentTypeId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion AgentTypeId

                    #region SalesId [int] NULL

                    if (columnNames.Contains("sales_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["sales_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["SalesId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["sales_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["SalesId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid SALES_ID;";
                                dataRowAgentType["SalesId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion SalesId

                    #region HoldingCompany [varchar] (30) NULL

                    if (columnNames.Contains("holding_company"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["holding_company"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["HoldingCompany"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["holding_company"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["HoldingCompany"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " HOLDING_COMPANY is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["HoldingCompany"] = dataTableAgentGeneralTabExcelData.Rows[i]["holding_company"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion HoldingCompany

                    #region GSATypeId [int] NULL

                    if (columnNames.Contains("gsa_type_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["gsa_type_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["GSATypeId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["gsa_type_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["GSATypeId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid GSA_TYPE_ID;";
                                dataRowAgentType["GSATypeId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion GSATypeId

                    #region CommTypeId [int] NULL

                    if (columnNames.Contains("comm_type_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["comm_type_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CommTypeId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["comm_type_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["CommTypeId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid COMM_TYPE_ID;";
                                dataRowAgentType["CommTypeId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion CommTypeId

                    #region Fixed [decimal] (18, 2) NULL

                    if (columnNames.Contains("fixed"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["fixed"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Fixed"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["fixed"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["Fixed"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["Fixed"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid FIXED;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["Fixed"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid FIXED;";
                            }
                        }
                    }

                    #endregion Fixed

                    #region Percentage [decimal] (18, 2) NULL

                    if (columnNames.Contains("percentage"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["percentage"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Percentage"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["percentage"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["Percentage"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["Percentage"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid PERCENTAGE;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["Percentage"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid PERCENTAGE;";
                            }
                        }
                    }

                    #endregion Percentage

                    #region CurrencyCodeId [int] NULL

                    if (columnNames.Contains("currency_code_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["currency_code_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CurrencyCodeId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["currency_code_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["CurrencyCodeId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid CURRENCY_CODE_ID;";
                                dataRowAgentType["CurrencyCodeId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion CurrencyCodeId

                    #region CurrencyNumber [int] NULL

                    if (columnNames.Contains("currency_number"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["currency_number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CurrencyNumber"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["currency_number"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["CurrencyNumber"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid CURRENCY_NUMBER;";
                                dataRowAgentType["CurrencyNumber"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion CurrencyNumber

                    #region SettlCurr [varchar] (20) NULL

                    if (columnNames.Contains("settl_curr"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["settl_curr"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["SettlCurr"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["settl_curr"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["SettlCurr"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " SETTL_CURR is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["SettlCurr"] = dataTableAgentGeneralTabExcelData.Rows[i]["settl_curr"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion SettlCurr

                    #region CassId [int] NULL

                    if (columnNames.Contains("cass_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["cass_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CassId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["cass_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["CassId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid CASS_ID;";
                                dataRowAgentType["CassId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion CassId

                    #region ReportingPeriodId [int] NULL

                    if (columnNames.Contains("reporting_period_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["reporting_period_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["ReportingPeriodId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["reporting_period_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["ReportingPeriodId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid REPORTING_PERIOD_ID;";
                                dataRowAgentType["ReportingPeriodId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion ReportingPeriodId

                    #region CrdtLmt [int] NULL

                    if (columnNames.Contains("crdt_lmt"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["crdt_lmt"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CrdtLmt"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["crdt_lmt"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["CrdtLmt"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid CRDT_LMT;";
                                dataRowAgentType["CrdtLmt"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion CrdtLmt

                    #region BillingPeriod [varchar] (50) NULL

                    if (columnNames.Contains("billing_period"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["billing_period"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["BillingPeriod"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["billing_period"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["BillingPeriod"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " BILLING_PERIOD is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["BillingPeriod"] = dataTableAgentGeneralTabExcelData.Rows[i]["billing_period"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion BillingPeriod

                    #region BillToId [int] NULL

                    if (columnNames.Contains("billto_id"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["billto_id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["BillToId"] = DBNull.Value;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["billto_id"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["BillToId"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Invalid BILLTO_ID;";
                                dataRowAgentType["BillToId"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion BillToId

                    #region AgentAccountCode [varchar] (30) NULL

                    if (columnNames.Contains("agent_account_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_account_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentAccountCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_account_code"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["AgentAccountCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " AGENT_ACCOUNT_CODE is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AgentAccountCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["agent_account_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AgentAccountCode

                    #region TolarancePercent [decimal] (18, 2) NULL

                    if (columnNames.Contains("tolarance%"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["tolarance%"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["TolarancePercent"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["tolarance%"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["TolarancePercent"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["TolarancePercent"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid TOLARANCE%;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["TolarancePercent"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid TOLARANCE%;";
                            }
                        }
                    }

                    #endregion TolarancePercent

                    #region TolaranceValue [decimal] (18, 2) NULL

                    if (columnNames.Contains("tolarance_value"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["tolarance_value"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["TolaranceValue"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["tolarance_value"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["TolaranceValue"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["TolaranceValue"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid TOLARANCE_VALUE;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["TolaranceValue"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid TOLARANCE_VALUE;";
                            }
                        }
                    }

                    #endregion TolaranceValue

                    #region MaxValue [decimal] (18, 2) NULL

                    if (columnNames.Contains("max_value"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["max_value"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["MaxValue"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["max_value"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["MaxValue"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["MaxValue"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid MAX_VALUE;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["MaxValue"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid MAX_VALUE;";
                            }
                        }
                    }

                    #endregion MaxValue

                    #region UpdatedBy [varchar] (100) NULL

                    dataRowAgentType["UpdatedBy"] = string.Empty;

                    #endregion UpdatedBy

                    #region UpdatedOn [datetime] NULL

                    dataRowAgentType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region PersonContact [varchar] (100) NULL

                    if (columnNames.Contains("contact_person"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["PersonContact"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAgentType["PersonContact"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " CONTACT_PERSON is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["PersonContact"] = dataTableAgentGeneralTabExcelData.Rows[i]["contact_person"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion PersonContact

                    #region CurrentAWBno [varchar] (15) NULL

                    if (columnNames.Contains("current_awbno"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["current_awbno"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CurrentAWBno"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["current_awbno"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowAgentType["CurrentAWBno"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " CURRENT_AWBNO is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["CurrentAWBno"] = dataTableAgentGeneralTabExcelData.Rows[i]["current_awbno"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion CurrentAWBno

                    #region ControllingLocator* [varchar] (50) NULL

                    if (columnNames.Contains("is_controlling_locator*"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["is_controlling_locator*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["ControllingLocator"] = string.Empty;
                            validationDetailsAgent = validationDetailsAgent + " IS_CONTROLLING_LOCATOR is required;";
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["is_controlling_locator*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["ControllingLocator"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " IS_CONTROLLING_LOCATOR is more than 50 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralTabExcelData.Rows[i]["is_controlling_locator*"].ToString().ToUpper().Trim().Trim(','))
                                {
                                    case "Y":
                                        dataRowAgentType["ControllingLocator"] = "YES";
                                        break;
                                    case "N":
                                        dataRowAgentType["ControllingLocator"] = "NO";
                                        break;
                                    default:
                                        dataRowAgentType["ControllingLocator"] = string.Empty;
                                        validationDetailsAgent = validationDetailsAgent + " IS_CONTROLLING_LOCATOR is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion ControllingLocator*

                    #region AccountCode [varchar] (25) NULL

                    if (columnNames.Contains("account_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["account_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AccountCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["account_code"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowAgentType["AccountCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " ACCOUNT_CODE is more than 25 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AccountCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["account_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AccountCode

                    #region TDSOnCommision [varchar] (25) NULL

                    if (columnNames.Contains("tds_on_commision"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["tds_on_commision"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["TDSOnCommision"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["tds_on_commision"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowAgentType["TDSOnCommision"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " TDS_ON_COMMISION is more than 25 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["TDSOnCommision"] = dataTableAgentGeneralTabExcelData.Rows[i]["tds_on_commision"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion TDSOnCommision

                    #region TDSOnFreight [varchar] (25) NULL

                    if (columnNames.Contains("tds_on_freight"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["tds_on_freight"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["TDSOnFreight"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["tds_on_freight"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowAgentType["TDSOnFreight"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " TDS_ON_FREIGHT is more than 25 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["TDSOnFreight"] = dataTableAgentGeneralTabExcelData.Rows[i]["tds_on_freight"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion TDSOnFreight

                    #region AgentReferenceCode [varchar] (15) NULL

                    if (columnNames.Contains("agent_reference_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_reference_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentReferenceCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_reference_code"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowAgentType["AgentReferenceCode"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " AGENT_REFERENCE_CODE is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AgentReferenceCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["agent_reference_code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AgentReferenceCode

                    #region TresholdLimitDays [int] NULL

                    if (columnNames.Contains("treshold_limit_days"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["treshold_limit_days"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["TresholdLimitDays"] = 0;
                        }
                        else
                        {
                            tempIntValue = 0;
                            if (Int16.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["treshold_limit_days"].ToString().Trim().Trim(','), out tempIntValue))
                            {
                                dataRowAgentType["TresholdLimitDays"] = tempIntValue;
                            }
                            else
                            {
                                validationDetailsAgent = validationDetailsAgent + " Credit: Invalid TRESHOLD_LIMIT_DAYS;";
                                dataRowAgentType["TresholdLimitDays"] = DBNull.Value;
                            }
                        }
                    }

                    #endregion TresholdLimitDays

                    #region AllowedStations [varchar] (50) NULL

                    if (columnNames.Contains("allowed_stations"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["allowed_stations"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AllowedStations"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["allowed_stations"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAgentType["AllowedStations"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " ALLOWED_STATIONS is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["AllowedStations"] = dataTableAgentGeneralTabExcelData.Rows[i]["allowed_stations"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AllowedStations

                    #region ExcludeFromFBL [bit] NULL

                    if (columnNames.Contains("exclude_from_fbl"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["exclude_from_fbl"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["ExcludeFromFBL"] = 0;
                        }
                        else
                        {
                            switch (dataTableAgentGeneralTabExcelData.Rows[i]["exclude_from_fbl"].ToString().Trim().ToLower())
                            {
                                case "true":
                                case "1":
                                case "y":
                                    dataRowAgentType["ExcludeFromFBL"] = 1;
                                    break;
                                case "false":
                                case "0":
                                case "n":
                                    dataRowAgentType["ExcludeFromFBL"] = 0;
                                    break;
                                default:
                                    dataRowAgentType["ExcludeFromFBL"] = 0;
                                    validationDetailsAgent = validationDetailsAgent + " Invalid EXCLUDE_FROM_FBL;";
                                    break;
                            }
                        }
                    }

                    #endregion ExcludeFromFBL

                    #region IACCode [varchar] (500) NULL

                    if (columnNames.Contains("iac_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["iac_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["IACCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["iac_code"].ToString().Trim().Trim(',').Length > 500)
                            {
                                dataRowAgentType["IACCode"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " IAC_CODE is more than 500 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["IACCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["iac_code"].ToString().Trim();
                            }
                        }
                    }

                    #endregion IACCode

                    #region CCSFCode [varchar] (1000) NULL

                    if (columnNames.Contains("ccsf_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["ccsf_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["CCSFCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["ccsf_code"].ToString().Trim().Trim(',').Length > 1000)
                            {
                                dataRowAgentType["CCSFCode"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " CCSF_CODE is more than 1000 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["CCSFCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["ccsf_code"].ToString().Trim();
                            }
                        }
                    }

                    #endregion CCSFCode

                    #region isKnownShipper [bit] NULL

                    if (columnNames.Contains("is_known_shipper"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["is_known_shipper"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["isKnownShipper"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["is_known_shipper"].ToString().Trim().Trim(',').Length > 1)
                            {
                                dataRowAgentType["isKnownShipper"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " IS_KNOWN_SHIPPER is more than 1 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralTabExcelData.Rows[i]["is_known_shipper"].ToString().Trim().ToLower())
                                {
                                    case "n":
                                        dataRowAgentType["isKnownShipper"] = 0;
                                        break;
                                    case "y":
                                        dataRowAgentType["isKnownShipper"] = 1;
                                        break;
                                    default:
                                        dataRowAgentType["isKnownShipper"] = DBNull.Value;
                                        validationDetailsAgent = validationDetailsAgent + " Invalid IS_KNOWN_SHIPPER;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion isKnownShipper

                    #region AgentCreditAmount [decimal] (18, 2) NULL

                    if (columnNames.Contains("agent_credit_amount"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_credit_amount"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentCreditAmount"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["agent_credit_amount"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["AgentCreditAmount"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["AgentCreditAmount"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid AGENT_CREDIT_AMOUNT;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["AgentCreditAmount"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid AGENT_CREDIT_AMOUNT;";
                            }
                        }
                    }

                    #endregion AgentCreditAmount

                    #region AgentInvoiceBalance [decimal] (18, 2) NULL

                    if (columnNames.Contains("agent_invoice_balance"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_invoice_balance"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentInvoiceBalance"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["agent_invoice_balance"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["AgentInvoiceBalance"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["AgentInvoiceBalance"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid AGENT_INVOICE_BALANCE;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["AgentInvoiceBalance"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid AGENT_INVOICE_BALANCE;";
                            }
                        }
                    }

                    #endregion AgentInvoiceBalance

                    #region AgentCreditRemaining [decimal] (18, 2) NULL

                    if (columnNames.Contains("agent_credit_remaining"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["agent_credit_remaining"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["AgentCreditRemaining"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["agent_credit_remaining"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowAgentType["AgentCreditRemaining"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowAgentType["AgentCreditRemaining"] = DBNull.Value;
                                    validationDetailsAgent = validationDetailsAgent + "Invalid AGENT_CREDIT_REMAINING;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowAgentType["AgentCreditRemaining"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid AGENT_CREDIT_REMAINING;";
                            }
                        }
                    }

                    #endregion AgentCreditRemaining

                    #region StockAlertDate [varchar] (20) NULL

                    if (columnNames.Contains("stock_alert_date"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["stock_alert_date"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["StockAlertDate"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["stock_alert_date"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAgentType["StockAlertDate"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " STOCK_ALERT_DATE is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["StockAlertDate"] = dataTableAgentGeneralTabExcelData.Rows[i]["stock_alert_date"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion StockAlertDate

                    #region KnownShipperExpDt [datetime] NULL

                    if (columnNames.Contains("known_shipper_valid_to"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["KnownShipperExpDt"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader && !dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Contains(@"\")
                                                   && !dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Contains("/")
                                                   && !dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim().Trim(',').Contains("-"))
                                {
                                    tempDate = DateTime.FromOADate(Convert.ToDouble(dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim()));
                                    if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                    {
                                        dataRowAgentType["KnownShipperExpDt"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dataRowAgentType["KnownShipperExpDt"] = tempDate;
                                    }
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_to"].ToString().Trim(), out tempDate))
                                    {
                                        if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                        {
                                            dataRowAgentType["KnownShipperExpDt"] = DBNull.Value;
                                        }
                                        else
                                        {
                                            dataRowAgentType["KnownShipperExpDt"] = tempDate;
                                        }
                                    }
                                    else
                                    {
                                        dataRowAgentType["KnownShipperExpDt"] = DBNull.Value;
                                        validationDetailsAgent = validationDetailsAgent + "Invalid KNOWN_SHIPPER_VALID_TO;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAgentType["KnownShipperExpDt"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid KNOWN_SHIPPER_VALID_TO;";
                            }
                        }
                    }

                    #endregion KnownShipperExpDt

                    #region SSPCode [varchar] (40) NULL

                    if (columnNames.Contains("ssp_code"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["ssp_code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["SSPCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["ssp_code"].ToString().Trim().Trim(',').Length > 40)
                            {
                                dataRowAgentType["SSPCode"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " SSP_CODE is more than 40 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["SSPCode"] = dataTableAgentGeneralTabExcelData.Rows[i]["ssp_code"].ToString().Trim();
                            }
                        }
                    }

                    #endregion SSPCode

                    #region KnownShipperValidFrom [datetime] NULL

                    if (columnNames.Contains("known_shipper_valid_from"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["KnownShipperValidFrom"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader && !dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Contains(@"\")
                                                   && !dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Contains("/")
                                                   && !dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim().Trim(',').Contains("-"))
                                {
                                    tempDate = DateTime.FromOADate(Convert.ToDouble(dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim()));
                                    if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                    {
                                        dataRowAgentType["KnownShipperValidFrom"] = DBNull.Value;
                                    }
                                    else
                                    {
                                        dataRowAgentType["KnownShipperValidFrom"] = tempDate;
                                    }
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_valid_from"].ToString().Trim(), out tempDate))
                                    {
                                        if (tempDate.Day == 1 && tempDate.Month == 1 && tempDate.Year == 1900)
                                        {
                                            dataRowAgentType["KnownShipperValidFrom"] = DBNull.Value;
                                        }
                                        else
                                        {
                                            dataRowAgentType["KnownShipperValidFrom"] = tempDate;
                                        }
                                    }
                                    else
                                    {
                                        dataRowAgentType["KnownShipperValidFrom"] = DBNull.Value;
                                        validationDetailsAgent = validationDetailsAgent + "Invalid KNOWN_SHIPPER_VALID_FROM;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowAgentType["KnownShipperValidFrom"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + "Invalid KNOWN_SHIPPER_VALID_FROM;";
                            }
                        }
                    }

                    #endregion KnownShipperValidFrom

                    #region UOM [varchar] (3) NULL

                    if (columnNames.Contains("uom"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["UOM"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowAgentType["UOM"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " UOM is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["UOM"] = dataTableAgentGeneralTabExcelData.Rows[i]["uom"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion UOM

                    #region Latitude [varchar] (30) NULL

                    if (columnNames.Contains("latitude"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["latitude"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Latitude"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["latitude"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["Latitude"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " LATITUDE is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["Latitude"] = dataTableAgentGeneralTabExcelData.Rows[i]["latitude"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Latitude

                    #region Longitude [varchar] (30) NULL

                    if (columnNames.Contains("longitude"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["longitude"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["Longitude"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["longitude"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["Longitude"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " LONGITUDE is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["Longitude"] = dataTableAgentGeneralTabExcelData.Rows[i]["longitude"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion Longitude

                    #region OpsFromTime [varchar] (10) NULL

                    if (columnNames.Contains("ops_from_time"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["ops_from_time"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["OpsFromTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["ops_from_time"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["OpsFromTime"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " OPS_FROM_TIME is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["OpsFromTime"] = dataTableAgentGeneralTabExcelData.Rows[i]["ops_from_time"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion OpsFromTime

                    #region OpsToTime [varchar] (10) NULL

                    if (columnNames.Contains("ops_to_time"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["ops_to_time"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["OpsToTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["ops_to_time"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["OpsToTime"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " OPS_TO_TIME is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["OpsToTime"] = dataTableAgentGeneralTabExcelData.Rows[i]["ops_to_time"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion OpsToTime

                    #region SecurityType [varchar] (10) NULL

                    if (columnNames.Contains("security_type"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["security_type"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["SecurityType"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["security_type"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAgentType["SecurityType"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " SECURITY_TYPE is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["SecurityType"] = dataTableAgentGeneralTabExcelData.Rows[i]["security_type"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion SecurityType

                    #region KnownShipperNumber [varchar] (30) NULL

                    if (columnNames.Contains("known_shipper_number"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["KnownShipperNumber"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_number"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAgentType["KnownShipperNumber"] = string.Empty;
                                validationDetailsAgent = validationDetailsAgent + " KNOWN_SHIPPER_NUMBER is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAgentType["KnownShipperNumber"] = dataTableAgentGeneralTabExcelData.Rows[i]["known_shipper_number"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion KnownShipperNumber

                    #region RequestKnownShipper [bit] NULL

                    if (columnNames.Contains("request_known_shipper"))
                    {
                        if (dataTableAgentGeneralTabExcelData.Rows[i]["request_known_shipper"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAgentType["RequestKnownShipper"] = 0;
                        }
                        else
                        {
                            if (dataTableAgentGeneralTabExcelData.Rows[i]["request_known_shipper"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowAgentType["RequestKnownShipper"] = DBNull.Value;
                                validationDetailsAgent = validationDetailsAgent + " REQUEST_KNOWN_SHIPPER is more than 3 Chars;";
                            }
                            else
                            {
                                switch (dataTableAgentGeneralTabExcelData.Rows[i]["request_known_shipper"].ToString().Trim().ToLower())
                                {
                                    case "true":
                                    case "1":
                                    case "y":
                                        dataRowAgentType["RequestKnownShipper"] = 1;
                                        break;
                                    default:
                                        dataRowAgentType["RequestKnownShipper"] = 0;
                                        validationDetailsAgent = validationDetailsAgent + " Invalid REQUEST_KNOWN_SHIPPER;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion RequestKnownShipper

                    */

                    #endregion Due to only General Tab fields, these columns are ignored.

                    #endregion Create row for AgentType Data Table

                    dataRowAgentType["ValidationDetailsAgent"] = validationDetailsAgent;

                    AgentType.Rows.Add(dataRowAgentType);
                }

                // Database Call to Validate & Insert/Update Agent Master
                string errorInSp = string.Empty;
                DataSet ?dataSetResult = new DataSet();

                dataSetResult = await ValidateAndInsertUpdateAgentMasterGeneralInfo(srNotblMasterUploadSummaryLog, AgentType, errorInSp);

                #region Send Messages after Agent Upload

                if (dataSetResult != null)
                {
                    if (dataSetResult.Tables.Count > 2)
                    {
                        DataSet dsMQMessageDetailAll = new DataSet();
                        dsMQMessageDetailAll.Tables.Add(dataSetResult.Tables[0].Copy());
                        dsMQMessageDetailAll.Tables.Add(dataSetResult.Tables[1].Copy());

                        string toEmailAddress = string.Empty;
                        string MsgCommType = string.Empty;
                        string strEmailid = string.Empty;
                        string MessageVersion = string.Empty;
                        string PatnerSitaID = string.Empty;
                        string OriginSenderAddress = string.Empty;
                        string MessageID = string.Empty;
                        int MessageIDInt = 0;
                        string SITAHeaderType = string.Empty;
                        string userName = string.Empty;


                        if (dataSetResult.Tables[2].Rows.Count > 0)
                        {
                            toEmailAddress = dataSetResult.Tables[2].Rows[0]["PartnerEmailiD"].ToString().Trim();
                            MsgCommType = dataSetResult.Tables[2].Rows[0]["MsgCommType"].ToString().Trim().ToUpper();
                            strEmailid = dataSetResult.Tables[2].Rows[0]["PartnerEmailiD"].ToString();
                            MessageVersion = dataSetResult.Tables[2].Rows[0]["MessageVersion"].ToString();
                            PatnerSitaID = dataSetResult.Tables[2].Rows[0]["PatnerSitaID"].ToString();
                            OriginSenderAddress = dataSetResult.Tables[2].Rows[0]["OriginSenderAddress"].ToString();
                            MessageID = dataSetResult.Tables[2].Rows[0]["MessageID"].ToString();
                            SITAHeaderType = dataSetResult.Tables[2].Rows[0]["SITAHeaderType"].ToString();
                        }

                        dataSetResult = null;

                        string MQMessage = string.Empty;
                        DataSet dsMQMessageDetail = dsMQMessageDetailAll.Copy();
                        dsMQMessageDetail.Tables[0].TableName = "SingleAgentInfo";
                        dsMQMessageDetail.Tables[1].TableName = "SingleAgentContactInfo";

                        string SitaMessageHeader = string.Empty;

                        #region Creating TblOutboxType DataTable

                        DataTable TblOutboxType = new DataTable("TblOutboxType");
                        TblOutboxType.Columns.Add("OutBoxIndex", System.Type.GetType("System.Int32"));
                        TblOutboxType.Columns.Add("Srno", System.Type.GetType("System.Int32"));
                        TblOutboxType.Columns.Add("subject", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("body", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("FromiD", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("ToiD", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("RecievedOn", System.Type.GetType("System.DateTime"));
                        TblOutboxType.Columns.Add("SendOn", System.Type.GetType("System.DateTime"));
                        TblOutboxType.Columns.Add("isProcessed", System.Type.GetType("System.Byte"));
                        TblOutboxType.Columns.Add("STATUS", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("Type", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("CreatedOn", System.Type.GetType("System.DateTime"));
                        TblOutboxType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                        TblOutboxType.Columns.Add("ishtml", System.Type.GetType("System.Byte"));
                        TblOutboxType.Columns.Add("Error", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("ToMobileNo", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("isSMSProcessed", System.Type.GetType("System.Byte"));
                        TblOutboxType.Columns.Add("SMSText", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("isInternal", System.Type.GetType("System.Byte"));
                        TblOutboxType.Columns.Add("IsBlog", System.Type.GetType("System.Byte"));
                        TblOutboxType.Columns.Add("SendToFTP", System.Type.GetType("System.Byte"));
                        TblOutboxType.Columns.Add("MsgDeliveryType", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("MsgCategory", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("CreatedBy", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("AWBNumber", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("FlightDestination", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("FlightOrigin", System.Type.GetType("System.String"));
                        TblOutboxType.Columns.Add("FlightNumber", System.Type.GetType("System.String"));

                        #endregion Creating TblOutboxType DataTable

                        DataRow dataRowTblOutboxType = TblOutboxType.NewRow();

                        for (int i = 0; i < dsMQMessageDetailAll.Tables[0].Rows.Count; i++)
                        {
                            userName = dsMQMessageDetailAll.Tables[0].Rows[i]["UpdatedBy"].ToString();
                            dsMQMessageDetail.Tables[0].Rows.Clear();
                            dsMQMessageDetail.Tables[1].Rows.Clear();

                            DataRow dataRowSingleAgentInfo = dsMQMessageDetailAll.Tables[0].Copy().Rows[i];
                            if (dataRowSingleAgentInfo != null)
                            {
                                dsMQMessageDetail.Tables[0].Rows.Add(dataRowSingleAgentInfo.ItemArray);
                            }

                            DataRow dataRowAgentContactInfo = dsMQMessageDetailAll.Tables[1].Select(string.Format("AgentCode = '{0}'", dsMQMessageDetailAll.Tables[0].Rows[i]["AgentCode"].ToString())).FirstOrDefault();
                            if (dataRowAgentContactInfo != null)
                            {
                                dsMQMessageDetail.Tables[1].Rows.Add(dataRowAgentContactInfo.ItemArray);
                            }

                            MQMessage = string.Empty;
                            if (dsMQMessageDetail != null)
                            {
                                MQMessage = CreateMQMessage(dsMQMessageDetail);

                                SitaMessageHeader = string.Empty;
                                ///Generate Message header if message type is SITA
                                if (MsgCommType == "ALL" || MsgCommType == "SITA")
                                {

                                    if (!string.IsNullOrWhiteSpace(MessageID))
                                    {
                                        if (int.TryParse(MessageID, out MessageIDInt))
                                        {
                                            MessageID = (MessageIDInt + i).ToString();
                                        }
                                    }

                                    SitaMessageHeader = MakeMailMessageFormat(PatnerSitaID, OriginSenderAddress, MessageID, SITAHeaderType);
                                }

                                ///If Message communication type is "SITA" then generate SITA Message Header
                                if (SitaMessageHeader.Trim() != string.Empty)
                                {
                                    #region Create and Add row for TblOutboxType Data Table

                                    dataRowTblOutboxType = TblOutboxType.NewRow();
                                    dataRowTblOutboxType["OutBoxIndex"] = i + 1;
                                    dataRowTblOutboxType["Srno"] = 0;

                                    // subject [VARCHAR](50) NULL
                                    dataRowTblOutboxType["subject"] = "MQ Message";

                                    // body [NVARCHAR](MAX) NULL
                                    dataRowTblOutboxType["body"] = SitaMessageHeader.ToString() + "\r\n" + MQMessage;

                                    dataRowTblOutboxType["FromiD"] = string.Empty; // FromiD [VARCHAR](50) NULL
                                    dataRowTblOutboxType["ToiD"] = strEmailid; // ToiD [VARCHAR](500) NULL
                                    dataRowTblOutboxType["RecievedOn"] = DateTime.Now; // RecievedOn [DATETIME] NULL
                                    dataRowTblOutboxType["SendOn"] = DateTime.Now; // SendOn [DATETIME] NULL
                                    dataRowTblOutboxType["isProcessed"] = "0"; // isProcessed [BIT] NULL
                                    dataRowTblOutboxType["STATUS"] = "Active"; // STATUS [VARCHAR](20) NULL
                                    dataRowTblOutboxType["Type"] = MessageData.MessageTypeName.SK2CS; // Type [VARCHAR](50) NULL
                                    dataRowTblOutboxType["CreatedOn"] = DateTime.Now; // CreatedOn [DATETIME] NULL
                                    dataRowTblOutboxType["UpdatedOn"] = DateTime.Now; // UpdatedOn [DATETIME] NULL
                                    dataRowTblOutboxType["ishtml"] = 0; // ishtml [BIT] NULL
                                    dataRowTblOutboxType["Error"] = string.Empty; // Error [VARCHAR](200) NULL
                                    dataRowTblOutboxType["ToMobileNo"] = string.Empty; // ToMobileNo [VARCHAR](100) NULL
                                    dataRowTblOutboxType["isSMSProcessed"] = 0; // isSMSProcessed [BIT] NULL
                                    dataRowTblOutboxType["SMSText"] = string.Empty; // SMSText [VARCHAR](300) NULL
                                    dataRowTblOutboxType["isInternal"] = 0; // isInternal [BIT] NULL
                                    dataRowTblOutboxType["IsBlog"] = 0; // IsBlog [BIT] NULL
                                    dataRowTblOutboxType["SendToFTP"] = 0; // SendToFTP [BIT] NULL
                                    dataRowTblOutboxType["MsgDeliveryType"] = string.Empty; // MsgDeliveryType [VARCHAR](20) NULL
                                    dataRowTblOutboxType["MsgCategory"] = string.Empty; // MsgCategory [VARCHAR](20) NULL
                                    dataRowTblOutboxType["CreatedBy"] = userName; // CreatedBy [VARCHAR](30) NULL
                                    dataRowTblOutboxType["AWBNumber"] = string.Empty; // AWBNumber [VARCHAR](12) NULL
                                    dataRowTblOutboxType["FlightDestination"] = string.Empty; // FlightDestination [VARCHAR](3) NULL
                                    dataRowTblOutboxType["FlightOrigin"] = string.Empty; // FlightOrigin [VARCHAR](3) NULL
                                    dataRowTblOutboxType["FlightNumber"] = string.Empty; // FlightNumber [VARCHAR](10) NULL

                                    TblOutboxType.Rows.Add(dataRowTblOutboxType);

                                    #endregion Create row for TblOutboxType Data Table
                                }

                                ///If message communication type is "EMAIL" then send message to selected email id's from configuration table.
                                if ((MsgCommType == "ALL" || MsgCommType == "EMAIL") && toEmailAddress.Trim() != string.Empty)
                                {
                                    #region Create and Add row for TblOutboxType Data Table

                                    dataRowTblOutboxType = TblOutboxType.NewRow();
                                    dataRowTblOutboxType["OutBoxIndex"] = i + 1;
                                    dataRowTblOutboxType["Srno"] = 0;

                                    // subject [VARCHAR](50) NULL
                                    dataRowTblOutboxType["subject"] = "MQ Message";

                                    // body [NVARCHAR](MAX) NULL
                                    dataRowTblOutboxType["body"] = MQMessage;

                                    dataRowTblOutboxType["FromiD"] = string.Empty; // FromiD [VARCHAR](50) NULL
                                    dataRowTblOutboxType["ToiD"] = toEmailAddress; // ToiD [VARCHAR](500) NULL
                                    dataRowTblOutboxType["RecievedOn"] = DateTime.Now; // RecievedOn [DATETIME] NULL
                                    dataRowTblOutboxType["SendOn"] = DateTime.Now; // SendOn [DATETIME] NULL
                                    dataRowTblOutboxType["isProcessed"] = "0"; // isProcessed [BIT] NULL
                                    dataRowTblOutboxType["STATUS"] = "Active"; // STATUS [VARCHAR](20) NULL
                                    dataRowTblOutboxType["Type"] = MessageData.MessageTypeName.SK2CS; // Type [VARCHAR](50) NULL
                                    dataRowTblOutboxType["CreatedOn"] = DateTime.Now; // CreatedOn [DATETIME] NULL
                                    dataRowTblOutboxType["UpdatedOn"] = DateTime.Now; // UpdatedOn [DATETIME] NULL
                                    dataRowTblOutboxType["ishtml"] = 0; // ishtml [BIT] NULL
                                    dataRowTblOutboxType["Error"] = string.Empty; // Error [VARCHAR](200) NULL
                                    dataRowTblOutboxType["ToMobileNo"] = string.Empty; // ToMobileNo [VARCHAR](100) NULL
                                    dataRowTblOutboxType["isSMSProcessed"] = 0; // isSMSProcessed [BIT] NULL
                                    dataRowTblOutboxType["SMSText"] = string.Empty; // SMSText [VARCHAR](300) NULL
                                    dataRowTblOutboxType["isInternal"] = 0; // isInternal [BIT] NULL
                                    dataRowTblOutboxType["IsBlog"] = 0; // IsBlog [BIT] NULL
                                    dataRowTblOutboxType["SendToFTP"] = 0; // SendToFTP [BIT] NULL
                                    dataRowTblOutboxType["MsgDeliveryType"] = string.Empty; // MsgDeliveryType [VARCHAR](20) NULL
                                    dataRowTblOutboxType["MsgCategory"] = string.Empty; // MsgCategory [VARCHAR](20) NULL
                                    dataRowTblOutboxType["CreatedBy"] = userName; // CreatedBy [VARCHAR](30) NULL
                                    dataRowTblOutboxType["AWBNumber"] = string.Empty; // AWBNumber [VARCHAR](12) NULL
                                    dataRowTblOutboxType["FlightDestination"] = string.Empty; // FlightDestination [VARCHAR](3) NULL
                                    dataRowTblOutboxType["FlightOrigin"] = string.Empty; // FlightOrigin [VARCHAR](3) NULL
                                    dataRowTblOutboxType["FlightNumber"] = string.Empty; // FlightNumber [VARCHAR](10) NULL

                                    TblOutboxType.Rows.Add(dataRowTblOutboxType);

                                    #endregion Create row for TblOutboxType Data Table
                                }

                                ///If message communication type is "MESSAGE QUEUE" then send messsage to specified QUEUE
                                if (MsgCommType == "ALL" || MsgCommType == "MESSAGE QUEUE")
                                {
                                    #region Create and Add row for TblOutboxType Data Table

                                    dataRowTblOutboxType = TblOutboxType.NewRow();
                                    dataRowTblOutboxType["OutBoxIndex"] = i + 1;
                                    dataRowTblOutboxType["Srno"] = 0;

                                    // subject [VARCHAR](50) NULL
                                    dataRowTblOutboxType["subject"] = "MQ Message";

                                    // body [NVARCHAR](MAX) NULL
                                    dataRowTblOutboxType["body"] = MQMessage;

                                    dataRowTblOutboxType["FromiD"] = string.Empty; // FromiD [VARCHAR](50) NULL
                                    dataRowTblOutboxType["ToiD"] = "MQAgent"; // ToiD [VARCHAR](500) NULL
                                    dataRowTblOutboxType["RecievedOn"] = DateTime.Now; // RecievedOn [DATETIME] NULL
                                    dataRowTblOutboxType["SendOn"] = DateTime.Now; // SendOn [DATETIME] NULL
                                    dataRowTblOutboxType["isProcessed"] = "0"; // isProcessed [BIT] NULL
                                    dataRowTblOutboxType["STATUS"] = "Active"; // STATUS [VARCHAR](20) NULL
                                    dataRowTblOutboxType["Type"] = MessageData.MessageTypeName.SK2CS; // Type [VARCHAR](50) NULL
                                    dataRowTblOutboxType["CreatedOn"] = DateTime.Now; // CreatedOn [DATETIME] NULL
                                    dataRowTblOutboxType["UpdatedOn"] = DateTime.Now; // UpdatedOn [DATETIME] NULL
                                    dataRowTblOutboxType["ishtml"] = 0; // ishtml [BIT] NULL
                                    dataRowTblOutboxType["Error"] = string.Empty; // Error [VARCHAR](200) NULL
                                    dataRowTblOutboxType["ToMobileNo"] = string.Empty; // ToMobileNo [VARCHAR](100) NULL
                                    dataRowTblOutboxType["isSMSProcessed"] = 0; // isSMSProcessed [BIT] NULL
                                    dataRowTblOutboxType["SMSText"] = string.Empty; // SMSText [VARCHAR](300) NULL
                                    dataRowTblOutboxType["isInternal"] = 0; // isInternal [BIT] NULL
                                    dataRowTblOutboxType["IsBlog"] = 0; // IsBlog [BIT] NULL
                                    dataRowTblOutboxType["SendToFTP"] = 0; // SendToFTP [BIT] NULL
                                    dataRowTblOutboxType["MsgDeliveryType"] = string.Empty; // MsgDeliveryType [VARCHAR](20) NULL
                                    dataRowTblOutboxType["MsgCategory"] = string.Empty; // MsgCategory [VARCHAR](20) NULL
                                    dataRowTblOutboxType["CreatedBy"] = userName; // CreatedBy [VARCHAR](30) NULL
                                    dataRowTblOutboxType["AWBNumber"] = string.Empty; // AWBNumber [VARCHAR](12) NULL
                                    dataRowTblOutboxType["FlightDestination"] = string.Empty; // FlightDestination [VARCHAR](3) NULL
                                    dataRowTblOutboxType["FlightOrigin"] = string.Empty; // FlightOrigin [VARCHAR](3) NULL
                                    dataRowTblOutboxType["FlightNumber"] = string.Empty; // FlightNumber [VARCHAR](10) NULL

                                    TblOutboxType.Rows.Add(dataRowTblOutboxType);

                                    #endregion Create row for TblOutboxType Data Table
                                }
                            }
                        }

                        if (TblOutboxType.Rows.Count > 0)
                        {
                            // Database Call to Insert TblOutbox [Messages]
                            errorInSp = string.Empty;
                            BulkInsertToTblOutbox(TblOutboxType, errorInSp);
                        }
                    }
                }

                #endregion Send Messages after Agent Upload

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
                dataTableAgentGeneralInfoExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Agent Master (General Info only).
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="agentType"> Agent Master Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet?> ValidateAndInsertUpdateAgentMasterGeneralInfo(int srNotblMasterUploadSummaryLog, DataTable agentType, string errorInSp)
        {
            DataSet ?dataSetResult = new DataSet();
            try
            {
                //SqlParameter[] sqlParameters = new SqlParameter[] { 
                //                                                      new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                //                                                      new SqlParameter("@AgentTableType", agentType),
                //                                                      new SqlParameter("@Error", errorInSp)
                //                                                  };
                var sqlParameters = new[]
                {
                    new SqlParameter("@SrNotblMasterUploadSummaryLog", srNotblMasterUploadSummaryLog),
                    new SqlParameter("@AgentTableType", agentType),
                    new SqlParameter
                    {
                        ParameterName = "@Error",
                        SqlDbType = System.Data.SqlDbType.NVarChar,
                        Size = 500,
                        Direction = System.Data.ParameterDirection.Output,
                        Value = errorInSp
                    }
                };


                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("Masters.uspUploadAgentMasterGeneralInfo", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspUploadAgentMasterGeneralInfo", sqlParameters);


                return dataSetResult;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} Stack Trace: {StackTrace}", exception.Message, exception.StackTrace);
                return dataSetResult;
            }
        }

        /// <summary>
        /// Method to generate MQMessage, Added by prashantz
        /// </summary>
        public string CreateMQMessage(DataSet dsMQMessageDetail)
        {
            var sb = new StringBuilder();
            try
            {
                if (dsMQMessageDetail != null)
                {

                    System.Collections.Generic.Dictionary<string, string> notifications = new System.Collections.Generic.Dictionary<string, string>();
                    string[] NotificationType = new string[0];

                    #region : C Record :

                    if (dsMQMessageDetail.Tables.Count > 0 && dsMQMessageDetail.Tables[0].Rows.Count > 0)
                    {
                        DataRow drC = dsMQMessageDetail.Tables[0].Rows[0];
                        sb.Append("C");
                        sb.Append(drC["AgentCode"].ToString().PadRight(9));//sb.Append(Util.PadRightTruncate(OrganizationId.ToString(), 9, ' '));
                        sb.Append(drC["AgentName"].ToString().PadRight(80));//sb.Append(Util.PadRightTruncate(OrganizationName, 80, ' '));
                        sb.Append(drC["OfficeAddress1"].ToString().PadRight(128));//sb.Append(Util.PadRightTruncate(shipaddAddress1, 128, ' '));
                        sb.Append(drC["OfficeAddress2"].ToString().PadRight(128));//sb.Append(Util.PadRightTruncate(shipaddAddress2, 128, ' '));
                        sb.Append(drC["City"].ToString().PadRight(25));//sb.Append(Util.PadRightTruncate(shipaddCity, 25, ' '));
                        sb.Append(drC["State"].ToString().PadRight(9));//sb.Append(Util.PadRightTruncate(shipaddState, 9, ' '));                        
                        sb.Append(drC["PostalZIP"].ToString().PadRight(10));//sb.Append(Util.PadRightTruncate(shipaddState, 9, ' '));
                        sb.Append(drC["Country"].ToString().PadRight(2));//sb.Append(Util.PadRightTruncate(shipaddZip, 10, ' '));
                        sb.Append("B");
                        sb.Append(drC["BillingAddress1"].ToString().PadRight(128));//sb.Append(Util.PadRightTruncate(shipaddCountry, 2, ' '));
                        sb.Append(drC["BillingAddress2"].ToString().PadRight(128));//sb.Append("B");
                        sb.Append(drC["BillingCity"].ToString().PadRight(25));//sb.Append(Util.PadRightTruncate(billaddAddress1, 128, ' '));
                        sb.Append(drC["BillingState"].ToString().PadRight(9));//sb.Append(Util.PadRightTruncate(billaddAddress2, 128, ' '));
                        sb.Append(drC["BillingZipCode"].ToString().PadRight(10));//sb.Append(Util.PadRightTruncate(billaddCity, 25, ' '));
                        sb.Append(drC["BillingCountry"].ToString().PadRight(2));//sb.Append(Util.PadRightTruncate(billaddState, 9, ' '));
                        sb.Append(drC["BusinessType"].ToString().PadRight(3));//sb.Append(Util.PadRightTruncate(billaddZip, 10, ' '));
                        sb.Append(drC["STATUS"].ToString().PadRight(1));//sb.Append(Util.PadRightTruncate(billaddCountry, 2, ' '));
                        sb.Append(drC["IsKnownShipper"].ToString().PadRight(1));//sb.Append(Util.PadRightTruncate(BusinessType, 3, ' '));
                        sb.Append(drC["AllowedStations"].ToString().PadRight(3));//sb.Append(Util.PadRightTruncate(Status, 1, ' '));
                        sb.Append(drC["Phone1"].ToString().PadRight(18));//sb.Append(Util.PadRightTruncate(Security, 1, ' '));
                        sb.Append(drC["CustomerCode"].ToString().PadRight(14));//sb.Append(Util.PadRightTruncate(AirportIdentifier, 3, ' '));
                        sb.Append(drC["ClerkIdentificationCode"].ToString().PadRight(10));//sb.Append(Util.PadRightTruncate(CorporatePhone, 18, ' '));
                        sb.Append(drC["IACCode"].ToString().PadRight(20));//sb.Append(Util.PadRightTruncate(BillingAcctNumber, 14, ' '));
                        sb.Append(drC["IACExpirationDate"].ToString().PadRight(8));//sb.Append(Util.PadRightTruncate(ClerkIdentification, 10, ' '));
                        sb.Append(drC["CCSFType"].ToString().PadRight(1));//sb.Append(Util.PadRightTruncate(SecurityId, 20, ' '));
                        sb.Append(drC["CCSFExpiryDate"].ToString().PadRight(8));//sb.Append(Util.PadRightTruncate(SecurityExpirationString, 8, ' '));
                        sb.Append(drC["EmployeeNumber"].ToString().PadRight(12));//sb.Append(Util.PadRightTruncate(EmployeeNumber, 12, ' '));                        
                        sb.Append(drC["CCSFCode"].ToString().PadRight(25));//sb.Append(Util.PadRightTruncate(CcsfExpirationString, 8, ' '));

                        //Compile Notifications at Agent Level to OpsEmail address.
                        if (drC["Notification"].ToString() != "" && drC["Email"].ToString() != "")
                        {
                            string[] notif = drC["Notification"].ToString().Trim().Split(',');
                            if (notif != null)
                            {
                                foreach (var item in notif)
                                {
                                    if (notifications.ContainsKey(item))
                                    {   //Check if notification type already present in dictionary.
                                        notifications[item] = notifications[item].ToString() + "," + drC["Email"].ToString().Trim();
                                    }
                                    else
                                    {   //Add new notification in dictionary
                                        notifications.Add(item, drC["Email"].ToString().Trim());
                                        //Set array to find out unique notification types.
                                        Array.Resize(ref NotificationType, NotificationType.Length + 1);
                                        NotificationType[NotificationType.Length - 1] = item;
                                    }
                                }
                            }
                        }

                    }

                    #endregion

                    #region : P Record :

                    if (dsMQMessageDetail.Tables.Count > 1 && dsMQMessageDetail.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow drP in dsMQMessageDetail.Tables[1].Rows)
                        {
                            sb.Append("\r\nP");
                            sb.Append(drP["AgentCode"].ToString().PadRight(9));//sb.Append(Util.PadRightTruncate(OrganizationId.ToString(), 9, ' '));
                            sb.Append(drP["SrNo"].ToString().PadRight(9));//sb.Append(Util.PadRightTruncate(ContactId.ToString(), 9, ' '));
                            sb.Append(drP["FirestName"].ToString().PadRight(20));//sb.Append(Util.PadRightTruncate(FirstName, 20, ' '));
                            sb.Append(drP["LastName"].ToString().PadRight(20));//sb.Append(Util.PadRightTruncate(LastName, 20, ' '));
                            sb.Append(drP["Title"].ToString().PadRight(80));//sb.Append(Util.PadRightTruncate(Title, 80, ' '));
                            sb.Append(drP["Phone"].ToString().PadRight(18));//sb.Append(Util.PadRightTruncate(OfficePhone, 18, ' '));
                            sb.Append(drP["Mobile"].ToString().PadRight(18));//sb.Append(Util.PadRightTruncate(MobilePhone, 18, ' '));
                            sb.Append(drP["Fax"].ToString().PadRight(18));//sb.Append(Util.PadRightTruncate(FaxPhone, 18, ' '));
                            sb.Append(drP["HomePhone"].ToString().PadRight(18));//sb.Append(Util.PadRightTruncate(HomePhone, 18, ' '));
                            sb.Append(drP["BirthDate"].ToString().PadRight(8));//sb.Append(Util.PadRightTruncate(BirthdayString, 8, ' '));
                            sb.Append(drP["Email"].ToString().PadRight(80));//sb.Append(Util.PadRightTruncate(EmailAddress, 80, ' '));

                            //Compile line for Notifications
                            if (drP["Notification"].ToString() != "" && drP["Email"].ToString() != "")
                            {
                                string[] notif = drP["Notification"].ToString().Trim().Split(',');
                                if (notif != null)
                                {
                                    foreach (var item in notif)
                                    {
                                        if (notifications.ContainsKey(item))
                                        {   //Check if notification type already present in dictionary.
                                            notifications[item] = notifications[item].ToString() + "," + drP["Email"].ToString().Trim();
                                        }
                                        else
                                        {   //Add new notification in dictionary
                                            notifications.Add(item, drP["Email"].ToString().Trim());
                                            //Set array to find out unique notification types.
                                            Array.Resize(ref NotificationType, NotificationType.Length + 1);
                                            NotificationType[NotificationType.Length - 1] = item;
                                        }
                                    }
                                }
                            }

                        }
                    }

                    #endregion

                    #region : M Record :

                    if (notifications != null && notifications.Count > 0)
                    {
                        sb.Append("\r\nM");
                        for (int iCount = 0; iCount < NotificationType.Length; iCount++)
                        {
                            sb.Append(NotificationType[iCount].ToString() + ":" +
                                notifications[NotificationType[iCount].ToString()].ToString() + ";");
                        }
                    }

                    //if (dsMQMessageDetail.Tables.Count > 0 && dsMQMessageDetail.Tables[0].Rows.Count > 0)
                    //{
                    //    DataRow drC = dsMQMessageDetail.Tables[0].Rows[0];

                    //    sb.Append(drC["Notification"].ToString().Replace(",", ":;"));
                    //}

                    #endregion
                }
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} Stack Trace: {StackTrace}", exception.Message, exception.StackTrace);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Method used Generate Sita Format Address
        /// </summary>
        /// <param name="SitaAddress"></param>
        /// <param name="OriginSitaAddress"></param>
        /// <param name="strMessageID"></param>
        /// <param name="strSITAHeaderType"></param>
        /// <returns></returns>
        public string MakeMailMessageFormat(string SitaAddress = "", string OriginSitaAddress = "", string strMessageID = "", string strSITAHeaderType = "")
        {
            var strbuilder = new StringBuilder();
            try
            {
                if (strSITAHeaderType == "")
                    strSITAHeaderType = "TYPEB";

                var strsitaAddress = SitaAddress.Split(',');
                var strOriginSitaAddress = OriginSitaAddress.Split(',');
                string strSitaAddress = string.Empty;
                if (strSITAHeaderType.ToUpper() == "TYPE1")
                {
                    foreach (var sitaID in strsitaAddress)
                        if (sitaID != "")
                            strbuilder.Append("QD " + sitaID + "\r\n");

                    if (OriginSitaAddress != "")
                        strbuilder.Append("." + OriginSitaAddress);
                }
                else
                {
                    strbuilder.Append("=HEADER\r\n");
                    strbuilder.Append("=SND," + String.Format("{0:yyyy/M/d HH:mm}", DateTime.Now) + "\r\n");
                    strbuilder.Append("=PRIORITY\r\n");
                    strbuilder.Append("QK\r\n");
                    strbuilder.Append("=DESTINATION TYPE B\r\n");
                    foreach (var sitaID in strsitaAddress)
                        if (sitaID != "")
                            strbuilder.Append("STX," + sitaID + "\r\n");
                    strbuilder.Append("=ORIGIN\r\n");
                    strbuilder.Append("" + OriginSitaAddress + "\r\n");
                    strbuilder.Append("=MSGID\r\n");
                    if (strMessageID != "")
                        strbuilder.Append("" + strMessageID + "\r\n");
                    else
                        strbuilder.Append("" + String.Format("{0:yyyyMd}", DateTime.Now) + "\r\n");
                    strbuilder.Append("=TEXT");
                }

            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} Stack Trace: {StackTrace}", exception.Message, exception.StackTrace);
            }
            return strbuilder.ToString();
        }

        /// <summary>
        /// Method to Call Stored Procedure for Inserting rows into tblOutbox.
        /// </summary>
        /// <param name="tblOutboxType"> tblOutbox Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public async Task<DataSet?> BulkInsertToTblOutbox(DataTable tblOutboxType, string errorInSp)
        {
            DataSet ?dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {   new SqlParameter("@TblOutboxTableType", tblOutboxType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspBulkInsertToTblOutbox", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspBulkInsertToTblOutbox", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                //clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError("Message: {Message} Stack Trace: {StackTrace}", exception.Message, exception.StackTrace);
                return dataSetResult;
            }
        }
    }
}
