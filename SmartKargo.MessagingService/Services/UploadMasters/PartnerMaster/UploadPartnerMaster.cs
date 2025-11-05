using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QID.DataAccess;
using System.Threading;
using System.IO;
using Excel;
using System.Data.SqlClient;
using System.Globalization;

namespace QidWorkerRole.UploadMasters.PartnerMaster
{
    public class UploadPartnerMaster
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        public Boolean PartnerMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dsFiles = new DataSet();
                //dsFiles = uploadMasterCommon.GetUploadedFileData(UploadMasterType.Partners);
                string FilePath = "";

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "Partners", out FilePath))
                        {
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), FilePath);
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                            continue;
                        }

                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
            }
            return false;
        }

        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTablePartnerExcelData = new DataTable("dataTablePartnerExcelData");

            bool isBinaryReader = false;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);
                decimal tempDecimalValue = 0;

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".XLS") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTablePartnerExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTablePartnerExcelData);

                foreach (DataColumn dataColumn in dataTablePartnerExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTablePartnerExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating PartnerMasterType DataTable

                DataTable PartnerMasterType = new DataTable("PartnerMasterType");
                PartnerMasterType.Columns.Add("PartnerMasterIndex", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("SrNo", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("PartnerPrefix", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("PartnerCode", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("PartnerType", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("ZoneId", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("BillingCurrency", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("ListingCurrency", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("AccountCode", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("ControllingPartner", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("PartnerName", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("BillIdentifireID", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("InwardBilling", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("OutwardBilling", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("InwardOutward", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("From", System.Type.GetType("System.DateTime"));
                PartnerMasterType.Columns.Add("To", System.Type.GetType("System.DateTime"));
                PartnerMasterType.Columns.Add("OwnAWB", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("OALAWB", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("Percentage", System.Type.GetType("System.Decimal"));
                PartnerMasterType.Columns.Add("Value", System.Type.GetType("System.Decimal"));
                PartnerMasterType.Columns.Add("MaximumValue", System.Type.GetType("System.Int32"));
                PartnerMasterType.Columns.Add("CNoteType", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("CNoteValidation", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("DigitalSignature", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("IsSuspended", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("Language", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("SettlementMethod", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("RegistrationID", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("TaxRegistrationID", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("AdditionalTaxRegID", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("PartnerPresident", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("PartnerCFO", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("Country", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("City", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("PostalCode", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("LegalName", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("Address", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("SITAiD", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("EmailiD", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("validFrom", System.Type.GetType("System.DateTime"));
                PartnerMasterType.Columns.Add("validTo", System.Type.GetType("System.DateTime"));
                PartnerMasterType.Columns.Add("IsScheduled", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("AWBValidation", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("isActive", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("CreatedBy", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("CreatedOn", System.Type.GetType("System.DateTime"));
                PartnerMasterType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                PartnerMasterType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("Remarks", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("AcceptPartial", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("AcceptedTolerance", System.Type.GetType("System.Decimal"));
                PartnerMasterType.Columns.Add("OCApplied", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("IsAutoCustMsg", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("PartnerBillType", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("FRTApplied", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("BillingEvent", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("AutoGenerateInvoice", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("PartnerEndpoint", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("ValidateStock", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("ValidateTSA", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("ValidateCommodity", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("ValidateShipperAgent", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("isSIS", System.Type.GetType("System.Byte"));
                PartnerMasterType.Columns.Add("CarrierRegistrationNo", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("CompanyCode", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("WebServiceAddress", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("Token", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("Logo", System.Type.GetType("System.String"));
                PartnerMasterType.Columns.Add("ValidationDetailsPartnerMaster", System.Type.GetType("System.String"));

                #endregion Creating PartnerMasterType DataTable

                string validationDetailsPartnerMaster = string.Empty;

                DateTime tempDate;

                for (int i = 0; i < dataTablePartnerExcelData.Rows.Count; i++)
                {
                    validationDetailsPartnerMaster = string.Empty;

                    #region Create row for PartnerMasterType Data Table

                    DataRow dataRowPartnerMasterType = PartnerMasterType.NewRow();

                    dataRowPartnerMasterType["PartnerMasterIndex"] = i + 1;

                    #region [SrNo] [int] NOT NULL IDENTITY(1, 1)
                    dataRowPartnerMasterType["SrNo"] = 0;

                    #endregion

                    #region [PartnerPrefix] [varchar] (5) NULL

                    if (columnNames.Contains("partner prefix*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["partner prefix*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerPrefix"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Prefix is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["partner prefix*"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowPartnerMasterType["PartnerPrefix"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Prefix is more than 5 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerPrefix"] = dataTablePartnerExcelData.Rows[i]["partner prefix*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [PartnerCode] [varchar] (20) NULL

                    if (columnNames.Contains("partner code*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["partner code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerCode"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Code is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["partner code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowPartnerMasterType["PartnerCode"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Code is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerCode"] = dataTablePartnerExcelData.Rows[i]["partner code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [PartnerType] [varchar] (20) NULL

                    if (columnNames.Contains("partner type*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["partner type*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerType"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Type is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["partner type*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowPartnerMasterType["PartnerType"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Type is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerType"] = dataTablePartnerExcelData.Rows[i]["partner type*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [ZoneId] [varchar] (20) NULL

                    if (columnNames.Contains("zone / location id *"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["zone / location id *"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["ZoneId"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Zone / Location id  is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["zone / location id *"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowPartnerMasterType["ZoneId"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Zone / Location id is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["ZoneId"] = dataTablePartnerExcelData.Rows[i]["zone / location id *"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [BillingCurrency] [varchar] (10) NULL
                    if (columnNames.Contains("billing currency*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["billing currency*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["BillingCurrency"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Billing Currency is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["billing currency*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerMasterType["BillingCurrency"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Billing Currency is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["BillingCurrency"] = dataTablePartnerExcelData.Rows[i]["billing currency*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [ListingCurrency] [varchar] (10) NULL

                    if (columnNames.Contains("listing currency*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["listing currency*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["ListingCurrency"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Listing Currency is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["listing currency*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerMasterType["ListingCurrency"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Listing Currency is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["ListingCurrency"] = dataTablePartnerExcelData.Rows[i]["listing currency*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [AccountCode] [varchar] (10) NULL

                    if (columnNames.Contains("account code*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["account code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["AccountCode"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Account Code is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["account code*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerMasterType["AccountCode"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Account Code is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["AccountCode"] = dataTablePartnerExcelData.Rows[i]["account code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [ControllingPartner] [varchar] (10) NULL

                    dataRowPartnerMasterType["ControllingPartner"] = DBNull.Value;

                    #endregion

                    #region [PartnerName] [varchar] (100) NULL

                    if (columnNames.Contains("partner name*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["partner name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerName"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Name is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["partner name*"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowPartnerMasterType["PartnerName"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner Name is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerName"] = dataTablePartnerExcelData.Rows[i]["partner name*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [BillIdentifireID] [int] NULL
                    dataRowPartnerMasterType["BillIdentifireID"] = DBNull.Value;
                    #endregion

                    #region [InwardBilling] [int] NULL
                    dataRowPartnerMasterType["InwardBilling"] = DBNull.Value;
                    #endregion

                    #region [OutwardBilling] [int] NULL
                    dataRowPartnerMasterType["OutwardBilling"] = DBNull.Value;
                    #endregion

                    #region [InwardOutward] [int] NULL
                    dataRowPartnerMasterType["InwardOutward"] = DBNull.Value;
                    #endregion

                    #region [From] [datetime] NULL

                    dataRowPartnerMasterType["From"] = DBNull.Value;

                    #endregion

                    #region [To] [datetime] NULL

                    dataRowPartnerMasterType["To"] = DBNull.Value;

                    #endregion

                    #region [OwnAWB] [int] NULL

                    dataRowPartnerMasterType["OwnAWB"] = DBNull.Value;

                    #endregion

                    #region [OALAWB] [int] NULL

                    dataRowPartnerMasterType["OALAWB"] = DBNull.Value;

                    #endregion

                    #region [Percentage] [decimal] (18 2) NULL
                    dataRowPartnerMasterType["Percentage"] = DBNull.Value;
                    #endregion

                    #region [Value] [decimal] (18 2) NULL
                    dataRowPartnerMasterType["Value"] = DBNull.Value;
                    #endregion

                    #region [MaximumValue] [int] NULL
                    dataRowPartnerMasterType["MaximumValue"] = DBNull.Value;
                    #endregion

                    #region [CNoteType] [varchar] (20) NULL
                    dataRowPartnerMasterType["CNoteType"] = DBNull.Value;
                    #endregion

                    #region [CNoteValidation] [varchar] (20) NULL
                    dataRowPartnerMasterType["CNoteValidation"] = DBNull.Value;
                    #endregion

                    #region [DigitalSignature] [bit] NULL

                    if (columnNames.Contains("digital signature"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["digital signature"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["DigitalSignature"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["digital signature"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["DigitalSignature"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["DigitalSignature"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["DigitalSignature"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Digital Signature is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [IsSuspended] [bit] NULL

                    if (columnNames.Contains("issuspended"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["issuspended"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["IsSuspended"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["issuspended"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["IsSuspended"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["IsSuspended"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["IsSuspended"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "IsSuspended is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [Language] [varchar] (2) NULL

                    if (columnNames.Contains("language*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["language*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["Language"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Language is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["language*"].ToString().Trim().Trim(',').Length > 2)
                            {
                                dataRowPartnerMasterType["Language"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Language is more than 2 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["Language"] = dataTablePartnerExcelData.Rows[i]["language*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [SettlementMethod] [varchar] (50) NULL

                    if (columnNames.Contains("settlement method"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["settlement method"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["SettlementMethod"] = "ICH";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["settlement method"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowPartnerMasterType["SettlementMethod"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Settlement Method is more than 50 Chars;";
                            }
                            else
                            {
                                switch (dataTablePartnerExcelData.Rows[i]["settlement method"].ToString().Trim().Trim(',').ToUpper())
                                {
                                    case "ICH" :
                                        dataRowPartnerMasterType["SettlementMethod"] = "ICH";
                                        break;
                                    case "ACH" :
                                        dataRowPartnerMasterType["SettlementMethod"] = "ACH";
                                        break;
                                    case "BILATERAL" :
                                        dataRowPartnerMasterType["SettlementMethod"] = "BILATERAL";
                                        break;
                                    default:
                                        dataRowPartnerMasterType["SettlementMethod"] = DBNull.Value;
                                        validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Settlement Method is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion

                    #region [RegistrationID] [varchar] (50) NULL

                    if (columnNames.Contains("registration id*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["registration id*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["RegistrationID"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "RegistrationID is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["registration id*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowPartnerMasterType["RegistrationID"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "RegistrationID is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["RegistrationID"] = dataTablePartnerExcelData.Rows[i]["registration id*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [TaxRegistrationID] [varchar] (25) NULL

                    if (columnNames.Contains("tax registration id*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["tax registration id*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["TaxRegistrationID"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "TaxRegistrationID is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["tax registration id*"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowPartnerMasterType["TaxRegistrationID"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "TaxRegistrationID is more than 25 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["TaxRegistrationID"] = dataTablePartnerExcelData.Rows[i]["tax registration id*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [AdditionalTaxRegID] [varchar] (25) NULL

                    if (columnNames.Contains("additional tax reg id*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["additional tax reg id*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["AdditionalTaxRegID"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Additional Tax Regd ID is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["additional tax reg id*"].ToString().Trim().Trim(',').Length > 25)
                            {
                                dataRowPartnerMasterType["AdditionalTaxRegID"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Additional Tax Regd ID is more than 25 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["AdditionalTaxRegID"] = dataTablePartnerExcelData.Rows[i]["additional tax reg id*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [PartnerPresident] [varchar] (70) NULL

                    if (columnNames.Contains("partner president"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["partner president"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerPresident"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["partner president"].ToString().Trim().Trim(',').Length > 70)
                            {
                                dataRowPartnerMasterType["PartnerPresident"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner President is more than 70 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerPresident"] = dataTablePartnerExcelData.Rows[i]["partner president"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [PartnerCFO] [varchar] (70) NULL

                    if (columnNames.Contains("partner cfo"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["partner cfo"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerCFO"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["partner cfo"].ToString().Trim().Trim(',').Length > 70)
                            {
                                dataRowPartnerMasterType["PartnerCFO"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Partner CFO is more than 70 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerCFO"] = dataTablePartnerExcelData.Rows[i]["partner cfo"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [Country] [varchar] (2) NULL

                    if (columnNames.Contains("country*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["Country"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Country is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["country*"].ToString().Trim().Trim(',').Length > 2)
                            {
                                dataRowPartnerMasterType["Country"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Country is more than 2 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["Country"] = dataTablePartnerExcelData.Rows[i]["country*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [City] [varchar] (3) NULL

                    if (columnNames.Contains("city*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["City"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "City is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowPartnerMasterType["City"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "City is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["City"] = dataTablePartnerExcelData.Rows[i]["city*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [PostalCode] [varchar] (50) NULL

                    if (columnNames.Contains("postal code*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["postal code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PostalCode"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "PostalCode is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["postal code*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowPartnerMasterType["PostalCode"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "PostalCode is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PostalCode"] = dataTablePartnerExcelData.Rows[i]["postal code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [LegalName] [varchar] (100) NULL

                    if (columnNames.Contains("legal name*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["legal name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["LegalName"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Legal Name is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["legal name*"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowPartnerMasterType["LegalName"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Legal Name is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["LegalName"] = dataTablePartnerExcelData.Rows[i]["legal name*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [Address] [varchar] (200) NULL

                    if (columnNames.Contains("address*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["address*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["Address"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Address is required;";
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["address*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerMasterType["Address"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Address is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["Address"] = dataTablePartnerExcelData.Rows[i]["address*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [SITAiD] [varchar] (100) NULL

                    if (columnNames.Contains("sitaid"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["sitaid"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["SITAiD"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["sitaid"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerMasterType["SITAiD"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "SITAiD is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["SITAiD"] = dataTablePartnerExcelData.Rows[i]["sitaid"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [EmailiD] [varchar] (1000) NULL

                    if (columnNames.Contains("emailid"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["emailid"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["EmailiD"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["emailid"].ToString().Trim().Trim(',').Length > 1000)
                            {
                                dataRowPartnerMasterType["EmailiD"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "EmailiD is more than 1000 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["EmailiD"] = dataTablePartnerExcelData.Rows[i]["emailid"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [validFrom] [datetime] NULL

                    if (columnNames.Contains("valid from*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["valid from*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Valid From is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowPartnerMasterType["validFrom"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerExcelData.Rows[i]["valid from*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTablePartnerExcelData.Rows[i]["valid from*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowPartnerMasterType["validFrom"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowPartnerMasterType["validFrom"] = DBNull.Value;
                                        validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid Valid From;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowPartnerMasterType["validFrom"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid Valid From;";
                            }
                        }
                    }


                    #endregion

                    #region [validTo] [datetime] NULL

                    if (columnNames.Contains("valid to*"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["valid to*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Valid To is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowPartnerMasterType["validTo"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerExcelData.Rows[i]["valid to*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTablePartnerExcelData.Rows[i]["valid to*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowPartnerMasterType["validTo"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowPartnerMasterType["validTo"] = DBNull.Value;
                                        validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid Valid To;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowPartnerMasterType["validTo"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid Valid To;";
                            }
                        }
                    }

                    #endregion

                    #region [IsScheduled] [varchar] (20) NULL

                    if (columnNames.Contains("isscheduled"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["isscheduled"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["IsScheduled"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["isscheduled"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowPartnerMasterType["IsScheduled"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "IsScheduled is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["IsScheduled"] = dataTablePartnerExcelData.Rows[i]["isscheduled"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [AWBValidation] [varchar] (10) NULL

                    dataRowPartnerMasterType["AWBValidation"] = DBNull.Value;

                    #endregion

                    #region [isActive] [bit] NULL
                    if (columnNames.Contains("IsActive"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["IsActive"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["IsActive"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["IsActive"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["IsActive"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["IsActive"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["IsActive"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "IsActive is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [CreatedBy] [varchar] (100) NULL

                    if (columnNames.Contains("CreatedBy"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["CreatedBy"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["CreatedBy"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["CreatedBy"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowPartnerMasterType["CreatedBy"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "CreatedBy is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["CreatedBy"] = dataTablePartnerExcelData.Rows[i]["CreatedBy"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [CreatedOn] [datetime] NULL

                    if (columnNames.Contains("CreatedOn"))
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowPartnerMasterType["CreatedOn"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerExcelData.Rows[i]["CreatedOn"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTablePartnerExcelData.Rows[i]["CreatedOn"].ToString().Trim(), out tempDate))
                                {
                                    dataRowPartnerMasterType["CreatedOn"] = tempDate;
                                }
                                else
                                {
                                    dataRowPartnerMasterType["CreatedOn"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid CreatedOn Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowPartnerMasterType["CreatedOn"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid CreatedOn Date;";
                        }
                    }
                    #endregion

                    #region [UpdatedOn] [datetime] NUL
                    if (columnNames.Contains("UpdatedOn"))
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowPartnerMasterType["UpdatedOn"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerExcelData.Rows[i]["UpdatedOn"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTablePartnerExcelData.Rows[i]["UpdatedOn"].ToString().Trim(), out tempDate))
                                {
                                    dataRowPartnerMasterType["UpdatedOn"] = tempDate;
                                }
                                else
                                {
                                    dataRowPartnerMasterType["UpdatedOn"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid UpdatedOn Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowPartnerMasterType["UpdatedOn"] = DBNull.Value;
                            validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid UpdatedOn Date;";
                        }
                    }

                    #endregion

                    #region [UpdatedBy] [varchar] (100) NULL

                    if (columnNames.Contains("UpdatedBy"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["UpdatedBy"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["UpdatedBy"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["CreatedBy"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowPartnerMasterType["UpdatedBy"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "UpdatedBy is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["UpdatedBy"] = dataTablePartnerExcelData.Rows[i]["UpdatedBy"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [Remarks] [varchar] (1000) NULL
                    if (columnNames.Contains("Remarks"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["Remarks"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["Remarks"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["Remarks"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowPartnerMasterType["Remarks"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Remarks is more than 1000 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["Remarks"] = dataTablePartnerExcelData.Rows[i]["Remarks"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [AcceptPartial] [varchar] (1) NULL

                    if (columnNames.Contains("accept more / less pcs"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["accept more / less pcs"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["AcceptPartial"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["accept more / less pcs"].ToString().Trim().Trim(',').Length > 1)
                            {
                                dataRowPartnerMasterType["AcceptPartial"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "AcceptPartial is more than 1 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["AcceptPartial"] = dataTablePartnerExcelData.Rows[i]["accept more / less pcs"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [AcceptedTolerance] [decimal] (18 2) NULL

                    if (columnNames.Contains("tolerance %"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["tolerance %"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["AcceptedTolerance"] = DBNull.Value;
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTablePartnerExcelData.Rows[i]["tolerance %"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowPartnerMasterType["AcceptedTolerance"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowPartnerMasterType["AcceptedTolerance"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid Tolerance %;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowPartnerMasterType["AcceptedTolerance"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Invalid Tolerance %;";
                            }
                        }
                    }

                    #endregion

                    #region [OCApplied] [bit] NULL

                    if (columnNames.Contains("include other charges"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["include other charges"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["OCApplied"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["include other charges"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["OCApplied"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["OCApplied"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["OCApplied"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Include Other Charges Is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [IsAutoCustMsg] [bit] NULL

                    if (columnNames.Contains("auto generate customs"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["auto generate customs"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["IsAutoCustMsg"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["auto generate customs"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["IsAutoCustMsg"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["IsAutoCustMsg"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["IsAutoCustMsg"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "IsAutoCustMsg Is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [PartnerBillType] [varchar] (15) NULL

                    if (columnNames.Contains("billing type"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["billing type"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["PartnerBillType"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["billing type"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowPartnerMasterType["PartnerBillType"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "billing type is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["PartnerBillType"] = dataTablePartnerExcelData.Rows[i]["billing type"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                     
                    #endregion

                    #region [FRTApplied] [bit] NULL
                    dataRowPartnerMasterType["FRTApplied"] = DBNull.Value;
                    #endregion

                    #region [BillingEvent] [varchar] (2) NULL

                    if (columnNames.Contains("billing on"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["billing on"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["BillingEvent"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerExcelData.Rows[i]["billing on"].ToString().Trim().Trim(',').Length > 2)
                            {
                                dataRowPartnerMasterType["BillingEvent"] = DBNull.Value;
                                validationDetailsPartnerMaster = validationDetailsPartnerMaster + "Billing Event is more than 2 Chars;";
                            }
                            else
                            {
                                dataRowPartnerMasterType["BillingEvent"] = dataTablePartnerExcelData.Rows[i]["billing on"].ToString().Trim().Trim(',');
                            }
                        }
                    }


                    #endregion

                    #region [AutoGenerateInvoice] [bit] NULL

                    if (columnNames.Contains("auto generate invoice"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["auto generate invoice"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["AutoGenerateInvoice"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["auto generate invoice"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["AutoGenerateInvoice"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["AutoGenerateInvoice"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["AutoGenerateInvoice"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "AutoGenerate Invoice Is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [PartnerEndpoint] [varchar] (500) NULL
                          dataRowPartnerMasterType["PartnerEndpoint"] = DBNull.Value;
                    #endregion

                    #region [ValidateStock] [bit] NULL
                         dataRowPartnerMasterType["ValidateStock"] = DBNull.Value;
                    #endregion

                    #region [ValidateTSA] [bit] NULL
                          dataRowPartnerMasterType["ValidateTSA"] = DBNull.Value;
                    #endregion

                    #region [ValidateCommodity] [bit] NULL
                         dataRowPartnerMasterType["ValidateCommodity"] = DBNull.Value;
                    #endregion

                    #region [ValidateShipperAgent] [bit] NULL
                          dataRowPartnerMasterType["ValidateShipperAgent"] = DBNull.Value;
                    #endregion

                    #region [isSIS] [bit] NULL

                    if (columnNames.Contains("issis"))
                    {
                        if (dataTablePartnerExcelData.Rows[i]["issis"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerMasterType["isSIS"] = 0;
                        }
                        else
                        {
                            switch (dataTablePartnerExcelData.Rows[i]["issis"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerMasterType["isSIS"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerMasterType["isSIS"] = 0;
                                    break;
                                default:
                                    dataRowPartnerMasterType["isSIS"] = DBNull.Value;
                                    validationDetailsPartnerMaster = validationDetailsPartnerMaster + "IsSIS is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [CarrierRegistrationNo] [varchar] (50) NULL
                        dataRowPartnerMasterType["CarrierRegistrationNo"] = DBNull.Value;
                    #endregion

                    #region [CompanyCode] [varchar] (50) NULL
                         dataRowPartnerMasterType["CompanyCode"] = DBNull.Value;
                    #endregion

                    #region [WebServiceAddress] [varchar] (500) NULL
                         dataRowPartnerMasterType["WebServiceAddress"] = DBNull.Value;
                    #endregion

                    #region [Token] [nvarchar] (500) NULL
                        dataRowPartnerMasterType["Token"] = DBNull.Value;
                    #endregion

                    #region [Logo] [nvarchar] (30) NULL
                        dataRowPartnerMasterType["Logo"] = DBNull.Value;
                    #endregion

                    dataRowPartnerMasterType["validationDetailsPartnerMaster"] = validationDetailsPartnerMaster;

                    PartnerMasterType.Rows.Add(dataRowPartnerMasterType);

                    #endregion Create row for PartnerMasterType Data Table
                }

                // Database Call to Validate & Insert/Update AirportMaster Master
                string errorInSp = string.Empty;
                ValidateAndInsertPartnerMaster(srNotblMasterUploadSummaryLog, PartnerMasterType, errorInSp);//

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTablePartnerExcelData = null;
            }
        }

        public DataSet ValidateAndInsertPartnerMaster(int srNotblMasterUploadSummaryLog, DataTable shipperConsigneeType, string errorInSp)
        {
            // uspUploadPartnerMaster
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { 
                                                                      new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@PartnerMasterTableType", shipperConsigneeType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadPartnerMaster", sqlParameters);

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
