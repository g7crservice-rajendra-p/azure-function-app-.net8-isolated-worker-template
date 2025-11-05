using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.IO;
using Excel;
using System.Data.SqlClient;
using QID.DataAccess;

namespace QidWorkerRole.UploadMasters.CostLine
{
    /// <summary>
    /// Class to Upload Cost Master File.
    /// </summary>
    public class UploadCostMaster
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod Cost Line Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public Boolean CostLineMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.CostMaster);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "CostMasterUploadFile", out uploadFilePath))
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath);
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " \n StackTrace: " + exception.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Method to Process Cost Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Cost Master Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableCostMasterExcelData = new DataTable("dataTableCostMasterExcelData");

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
                dataTableCostMasterExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableCostMasterExcelData);

                foreach (DataColumn dataColumn in dataTableCostMasterExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                #region Creating CostType DataTable

                DataTable CostType = new DataTable("CostType");
                CostType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostType.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostType.Columns.Add("VendorCode", System.Type.GetType("System.String"));
                CostType.Columns.Add("CostCode", System.Type.GetType("System.String"));
                CostType.Columns.Add("ChargeHeadCode", System.Type.GetType("System.String"));
                CostType.Columns.Add("ChargeHeadName", System.Type.GetType("System.String"));
                CostType.Columns.Add("StartDate", System.Type.GetType("System.DateTime"));
                CostType.Columns.Add("EndDate", System.Type.GetType("System.DateTime"));
                CostType.Columns.Add("LocationLevelInt", System.Type.GetType("System.Int32"));
                CostType.Columns.Add("LocationLevel", System.Type.GetType("System.String"));
                CostType.Columns.Add("Location", System.Type.GetType("System.String"));
                CostType.Columns.Add("OriginLevelInt", System.Type.GetType("System.Int32"));
                CostType.Columns.Add("OriginLevel", System.Type.GetType("System.String"));
                CostType.Columns.Add("Origin", System.Type.GetType("System.String"));
                CostType.Columns.Add("DestinationLevelInt", System.Type.GetType("System.Int32"));
                CostType.Columns.Add("DestinationLevel", System.Type.GetType("System.String"));
                CostType.Columns.Add("Destination", System.Type.GetType("System.String"));
                CostType.Columns.Add("CurrencyID", System.Type.GetType("System.Int32"));
                CostType.Columns.Add("Currency", System.Type.GetType("System.String"));
                CostType.Columns.Add("PaymentType", System.Type.GetType("System.String"));
                CostType.Columns.Add("CostType", System.Type.GetType("System.String"));
                CostType.Columns.Add("DiscountPercent", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("CommPercent", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("ServiceTax", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("TDSPercent", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("ChargeHeadBasis", System.Type.GetType("System.String"));
                CostType.Columns.Add("MinimumCharge", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("MaxValue", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("PerUnitCharge", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("WeightType", System.Type.GetType("System.String"));
                CostType.Columns.Add("Status", System.Type.GetType("System.String"));
                CostType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                CostType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                CostType.Columns.Add("ViaStation", System.Type.GetType("System.String"));
                CostType.Columns.Add("chargedAt", System.Type.GetType("System.String"));
                CostType.Columns.Add("BasedOn", System.Type.GetType("System.String"));
                CostType.Columns.Add("BaseRate", System.Type.GetType("System.Decimal"));
                CostType.Columns.Add("GLCode", System.Type.GetType("System.String"));
                CostType.Columns.Add("UOM", System.Type.GetType("System.String"));
                CostType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));
                CostType.Columns.Add("ConcessionPer", System.Type.GetType("System.String"));

                #endregion

                #region Creating CostRemarksType DataTable

                DataTable CostRemarksType = new DataTable("CostRemarksType");
                CostRemarksType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostRemarksType.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostRemarksType.Columns.Add("CostID", System.Type.GetType("System.Int32"));
                CostRemarksType.Columns.Add("Remark", System.Type.GetType("System.String"));
                CostRemarksType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                CostRemarksType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                #region Creating CostSlabsType DataTable

                DataTable CostSlabsType = new DataTable("CostSlabsType");
                CostSlabsType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostSlabsType.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostSlabsType.Columns.Add("CostID", System.Type.GetType("System.Int32"));
                CostSlabsType.Columns.Add("SlabName", System.Type.GetType("System.String"));
                CostSlabsType.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                CostSlabsType.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                CostSlabsType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                CostSlabsType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                DataTable CostSlabsTypeTemp = new DataTable("CostSlabsTypeTemp");
                CostSlabsTypeTemp.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostSlabsTypeTemp.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostSlabsTypeTemp.Columns.Add("CostID", System.Type.GetType("System.Int32"));
                CostSlabsTypeTemp.Columns.Add("SlabName", System.Type.GetType("System.String"));
                CostSlabsTypeTemp.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                CostSlabsTypeTemp.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                CostSlabsTypeTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                CostSlabsTypeTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                #endregion

                #region Creating CostULDSlabsType DataTable

                DataTable CostULDSlabsType = new DataTable("CostULDSlabsType");
                CostULDSlabsType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostULDSlabsType.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostULDSlabsType.Columns.Add("CostID", System.Type.GetType("System.Int32"));
                CostULDSlabsType.Columns.Add("ULDType", System.Type.GetType("System.String"));
                CostULDSlabsType.Columns.Add("SlabName", System.Type.GetType("System.String"));
                CostULDSlabsType.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                CostULDSlabsType.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                CostULDSlabsType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                CostULDSlabsType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                DataTable CostULDSlabsTypeTemp = new DataTable("CostULDSlabsTypeTemp");
                CostULDSlabsTypeTemp.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostULDSlabsTypeTemp.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostULDSlabsTypeTemp.Columns.Add("CostID", System.Type.GetType("System.Int32"));
                CostULDSlabsTypeTemp.Columns.Add("ULDType", System.Type.GetType("System.String"));
                CostULDSlabsTypeTemp.Columns.Add("SlabName", System.Type.GetType("System.String"));
                CostULDSlabsTypeTemp.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                CostULDSlabsTypeTemp.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                CostULDSlabsTypeTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                CostULDSlabsTypeTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                #region Creating CostParamsType DataTable

                DataTable CostParamsType = new DataTable("CostParamsType");
                CostParamsType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
                CostParamsType.Columns.Add("ID", System.Type.GetType("System.Int32"));
                CostParamsType.Columns.Add("CostID", System.Type.GetType("System.Int32"));
                CostParamsType.Columns.Add("ParamName", System.Type.GetType("System.String"));
                CostParamsType.Columns.Add("ParamValue", System.Type.GetType("System.String"));
                CostParamsType.Columns.Add("IsInclude", System.Type.GetType("System.Byte"));
                CostParamsType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                CostParamsType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                #endregion

                string validationDetails = string.Empty;
                int costID = 0;
                DateTime tempDate;
                string uLDType = string.Empty;
                decimal tempDecimalValue = 0;
                decimal tempDecimalWtValue = 0;
                decimal tempDecimalChargValue = 0;
                decimal columnNameStartDecimal;
                string[] strArrColName;

                for (int i = 0; i < dataTableCostMasterExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;
                    costID = 0;
                    tempDecimalValue = 0;

                    #region Create row for CostType DataTable

                    DataRow dataRowCostType = CostType.NewRow();

                    #region ReferenceID INT NOT NULL

                    dataRowCostType["ReferenceID"] = i + 1;

                    #endregion ReferenceID

                    #region ID int NULL

                    if (dataTableCostMasterExcelData.Rows[i]["cost id"] == null)
                    {
                        dataRowCostType["ID"] = costID;
                    }
                    else if (dataTableCostMasterExcelData.Rows[i]["cost id"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowCostType["ID"] = costID;
                    }
                    else
                    {
                        if (int.TryParse(dataTableCostMasterExcelData.Rows[i]["cost id"].ToString().Trim().Trim(','), out costID))
                        {
                            dataRowCostType["ID"] = costID;
                        }
                        else
                        {
                            dataRowCostType["ID"] = costID;
                            validationDetails = validationDetails + "Invalid Cost ID;";
                        }
                    }

                    #endregion ID

                    #region VendorCode varchar (50) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["vendor code"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Vendor Code is required;";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["vendor code"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + "Vendor Code is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["VendorCode"] = dataTableCostMasterExcelData.Rows[i]["vendor code"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion VendorCode

                    #region CostCode varchar (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["cost code"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Cost Code is required;";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["cost code"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + "Cost Code is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowCostType["CostCode"] = dataTableCostMasterExcelData.Rows[i]["cost code"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion CostCode

                    #region ChargeHeadCode varchar (10) NOT NULL

                    dataRowCostType["ChargeHeadCode"] = string.Empty;

                    #endregion ChargeHeadCode

                    #region ChargeHeadName varchar (100) NULL

                    dataRowCostType["ChargeHeadName"] = string.Empty;

                    #endregion ChargeHeadName

                    #region StartDate date NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["from date"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " From Date is required;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowCostType["StartDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableCostMasterExcelData.Rows[i]["from date"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableCostMasterExcelData.Rows[i]["from date"].ToString().Trim(), out tempDate))
                                {
                                    dataRowCostType["StartDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowCostType["StartDate"] = DateTime.MinValue;
                                    validationDetails = validationDetails + " Invalid From Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowCostType["StartDate"] = DateTime.MinValue;
                            validationDetails = validationDetails + "Invalid From Date;";
                        }
                    }

                    #endregion StartDate

                    #region EndDate date NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["to date"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " To Date is required;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowCostType["EndDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableCostMasterExcelData.Rows[i]["to date"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableCostMasterExcelData.Rows[i]["to date"].ToString().Trim(), out tempDate))
                                {
                                    dataRowCostType["EndDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowCostType["EndDate"] = DateTime.MinValue;
                                    validationDetails = validationDetails + " Invalid To Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowCostType["EndDate"] = DateTime.MinValue;
                            validationDetails = validationDetails + "Invalid To Date;";
                        }
                    }

                    #endregion EndDate

                    #region LocationLevelInt int NULL

                    dataRowCostType["LocationLevelInt"] = 0;

                    #endregion LocationLevelInt

                    #region LocationLevel VARCHAR(50) NULL

                    dataRowCostType["LocationLevel"] = string.Empty;

                    #endregion LocationLevel

                    #region Location varchar (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["location"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["Location"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["location"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + " Location is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Location"] = dataTableCostMasterExcelData.Rows[i]["location"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Location

                    #region OriginLevelInt int NULL

                    dataRowCostType["OriginLevelInt"] = 0;

                    #endregion OriginLevelInt

                    #region OriginLevel VARCHAR(50) NULL

                    dataRowCostType["OriginLevel"] = string.Empty;

                    #endregion OriginLevel

                    #region Origin varchar (15) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["origin"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "Origin is required;";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 15)
                        {
                            validationDetails = validationDetails + "Origin is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Origin"] = dataTableCostMasterExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Origin

                    #region DestinationLevelInt int NULL

                    dataRowCostType["DestinationLevelInt"] = 0;

                    #endregion DestinationLevelInt

                    #region DestinationLevel VARCHAR(50) NULL

                    dataRowCostType["DestinationLevel"] = string.Empty;

                    #endregion DestinationLevel

                    #region Destination varchar (15) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["destination"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Destination is required;";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 15)
                        {
                            validationDetails = validationDetails + " Destination is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Destination"] = dataTableCostMasterExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Destination

                    #region CurrencyID int NULL

                    dataRowCostType["CurrencyID"] = 0;

                    #endregion CurrencyID

                    #region Currency VARCHAR(50) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["currency"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Currency is required;";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationDetails = validationDetails + " Currency is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowCostType["Currency"] = dataTableCostMasterExcelData.Rows[i]["currency"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Currency

                    #region PaymentType varchar (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["payment type"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["PaymentType"] = "All";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["payment type"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + " Payment Type is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowCostType["PaymentType"] = dataTableCostMasterExcelData.Rows[i]["payment type"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion PaymentType

                    #region CostType varchar (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["cost type"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " Cost Type is required;";
                    }
                    else
                    {
                        switch (dataTableCostMasterExcelData.Rows[i]["cost type"].ToString().Trim().ToLower())
                        {
                            case "a":
                            case "awb":
                                dataRowCostType["CostType"] = "A";
                                break;
                            case "f":
                            case "flight":
                                dataRowCostType["CostType"] = "F";
                                break;
                            case "station":
                            case "s":
                                dataRowCostType["CostType"] = "S";
                                break;
                            default:
                                dataRowCostType["CostType"] = "";
                                validationDetails = validationDetails + " Invalid Cost Type: " + dataTableCostMasterExcelData.Rows[i]["cost type"].ToString() + ";";
                                break;
                        }
                    }

                    #endregion ChargeType

                    #region DiscountPercent decimal (18, 2) NULL

                    dataRowCostType["DiscountPercent"] = 0;

                    #endregion DiscountPercent

                    #region CommPercent decimal (18, 2) NULL

                    dataRowCostType["CommPercent"] = 0;

                    #endregion CommPercent

                    #region ServiceTax decimal (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["tax"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["ServiceTax"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i]["tax"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCostType["ServiceTax"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCostType["ServiceTax"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Tax;";
                        }
                    }

                    #endregion ServiceTax

                    #region TDSPercent decimal (18, 2) NULL

                    dataRowCostType["TDSPercent"] = 0;

                    #endregion TDSPercent

                    #region ChargeHeadBasis varchar (15) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["costheadbasis"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "CostHeadBasis is required;";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["costheadbasis"].ToString().Trim().Trim(',').Length > 15)
                        {
                            validationDetails = validationDetails + "ChargeHeadBasis is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowCostType["ChargeHeadBasis"] = dataTableCostMasterExcelData.Rows[i]["costheadbasis"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion ChargeHeadBasis

                    #region MinimumCharge decimal (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["minimum"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["MinimumCharge"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i]["minimum"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCostType["MinimumCharge"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCostType["MinimumCharge"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Minimum;";
                        }
                    }

                    #endregion MinimumCharge

                    #region MaxValue decimal (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["maximum"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["MaxValue"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i]["maximum"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCostType["MaxValue"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCostType["MaxValue"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Maximum;";
                        }
                    }

                    #endregion MaxValue

                    #region PerUnitCharge decimal (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["charge"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["PerUnitCharge"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i]["charge"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCostType["PerUnitCharge"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCostType["PerUnitCharge"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Charge;";
                        }
                    }

                    #endregion PerUnitCharge

                    #region WeightType varchar (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["weight type"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["WeightType"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["weight type"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + "Weight Type is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowCostType["WeightType"] = dataTableCostMasterExcelData.Rows[i]["weight type"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion WeightType

                    #region Status varchar (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["status"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["Status"] = "ACT";
                    }
                    else
                    {
                        switch (dataTableCostMasterExcelData.Rows[i]["status"].ToString().Trim().ToLower())
                        {
                            case "in-active":
                            case "ina":
                                dataRowCostType["Status"] = "INA";
                                break;
                            default:
                                dataRowCostType["Status"] = "ACT";
                                break;
                        }
                    }

                    #endregion Status

                    #region UpdatedOn datetime NULL

                    dataRowCostType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region UpdatedBy varchar (100) NULL

                    dataRowCostType["UpdatedBy"] = string.Empty;

                    #endregion UpdatedBy

                    #region ViaStation varchar (10) NULL

                    dataRowCostType["ViaStation"] = DBNull.Value;

                    #endregion ViaStation

                    #region chargedAt varchar (5) NULL

                    dataRowCostType["chargedAt"] = "All";

                    #endregion chargedAt

                    #region BasedOn varchar (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["applied on"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["BasedOn"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["applied on"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + "Applied On is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowCostType["BasedOn"] = dataTableCostMasterExcelData.Rows[i]["applied on"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion BasedOn

                    #region BaseRate decimal (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["base rate"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["BaseRate"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i]["base rate"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCostType["BaseRate"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCostType["BaseRate"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Base Rate;";
                        }
                    }

                    #endregion BaseRate

                    #region GLCode varchar (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["glcode"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["GLCode"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["glcode"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + " GLCode is more than 10 Chars; ";
                        }
                        else
                        {
                            dataRowCostType["GLCode"] = dataTableCostMasterExcelData.Rows[i]["glcode"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion GLCode

                    #region UOM varchar (3) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i]["uom"].ToString().Trim().Trim(',')))
                    {
                        dataRowCostType["UOM"] = "All";
                    }
                    else
                    {
                        if (dataTableCostMasterExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Length > 3)
                        {
                            validationDetails = validationDetails + "UOM is more than 3 Chars; ";
                        }
                        else
                        {
                            dataRowCostType["UOM"] = dataTableCostMasterExcelData.Rows[i]["uom"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion UOM

                    #region ValidationDetails VARCHAR(MAX) NULL

                    dataRowCostType["ValidationDetails"] = validationDetails;

                    #endregion ValidationDetails

                    #endregion

                    #region Create row for CostRemarksType DataTable

                    DataRow dataRowCostRemarksType = CostRemarksType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["remarks"].ToString().Trim().Length > 500)
                    {
                        validationDetails = validationDetails + "Remarks length is more than 500 chars;";
                    }
                    else
                    {
                        #region ReferenceID INT NOT NULL

                        dataRowCostRemarksType["ReferenceID"] = i + 1;

                        #endregion ReferenceID

                        #region ID int NULL

                        dataRowCostRemarksType["ID"] = 0;

                        #endregion ID

                        #region CostID int NULL

                        dataRowCostRemarksType["CostID"] = costID;

                        #endregion CostID

                        #region Remark varchar (500) NULL

                        dataRowCostRemarksType["Remark"] = dataTableCostMasterExcelData.Rows[i]["remarks"].ToString().Trim();

                        #endregion Remark

                        #region UpdatedBy varchar (100) NULL

                        dataRowCostRemarksType["UpdatedBy"] = string.Empty;

                        #endregion UpdatedBy

                        #region UpdatedOn datetime NOT NULL

                        dataRowCostRemarksType["UpdatedOn"] = DateTime.Now;

                        #endregion UpdatedOn
                    }

                    #endregion

                    #region Create rows for CostParamsType DataTable

                    #region Flight Carrier

                    DataRow dataRowCostParamsTypeFlightCarrier = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "FlightCarrier is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeFlightCarrier["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeFlightCarrier["ID"] = 0;
                        dataRowCostParamsTypeFlightCarrier["CostID"] = costID;
                        dataRowCostParamsTypeFlightCarrier["ParamName"] = "FlightCarrier";
                        dataRowCostParamsTypeFlightCarrier["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeFlightCarrier["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieflightcarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeFlightCarrier["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeFlightCarrier["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Issue Carrier

                    DataRow dataRowCostParamsTypeIssueCarrier = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "IssueCarrier is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeIssueCarrier["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeIssueCarrier["ID"] = 0;
                        dataRowCostParamsTypeIssueCarrier["CostID"] = costID;
                        dataRowCostParamsTypeIssueCarrier["ParamName"] = "IssueCarrier";
                        dataRowCostParamsTypeIssueCarrier["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeIssueCarrier["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieissuecarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeIssueCarrier["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeIssueCarrier["UpdatedBy"] = string.Empty;
                    }
                    #endregion

                    #region Airline Code

                    DataRow dataRowCostParamsTypeAirlineCode = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["airlinecode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "AirlineCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeAirlineCode["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeAirlineCode["ID"] = 0;
                        dataRowCostParamsTypeAirlineCode["CostID"] = costID;
                        dataRowCostParamsTypeAirlineCode["ParamName"] = "AirlineCode";
                        dataRowCostParamsTypeAirlineCode["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["airlinecode"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeAirlineCode["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieairlinecode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeAirlineCode["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeAirlineCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Origin

                    DataRow dataRowCostParamsTypeOrigin = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["origin1"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Origin1 is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeOrigin["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeOrigin["ID"] = 0;
                        dataRowCostParamsTypeOrigin["CostID"] = costID;
                        dataRowCostParamsTypeOrigin["ParamName"] = "Origin";
                        dataRowCostParamsTypeOrigin["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["origin1"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeOrigin["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieorigin"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeOrigin["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeOrigin["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Country Source

                    DataRow dataRowCostParamsTypeCountrySource = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["countrysource"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "CountrySource is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeCountrySource["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeCountrySource["ID"] = 0;
                        dataRowCostParamsTypeCountrySource["CostID"] = costID;
                        dataRowCostParamsTypeCountrySource["ParamName"] = "CountrySource";
                        dataRowCostParamsTypeCountrySource["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["countrysource"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeCountrySource["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["iecountrysource"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeCountrySource["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeCountrySource["UpdatedBy"] = string.Empty;
                    }


                    #endregion

                    #region Destination

                    DataRow dataRowCostParamsTypeDestination = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["destination1"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Destination1 is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeDestination["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeDestination["ID"] = 0;
                        dataRowCostParamsTypeDestination["CostID"] = costID;
                        dataRowCostParamsTypeDestination["ParamName"] = "Destination";
                        dataRowCostParamsTypeDestination["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["destination1"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeDestination["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["iedestination"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeDestination["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeDestination["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Country Destination

                    DataRow dataRowCostParamsTypeCountryDestination = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["countrydestination"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "CountryDestination is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeCountryDestination["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeCountryDestination["ID"] = 0;
                        dataRowCostParamsTypeCountryDestination["CostID"] = costID;
                        dataRowCostParamsTypeCountryDestination["ParamName"] = "CountryDestination";
                        dataRowCostParamsTypeCountryDestination["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["countrydestination"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeCountryDestination["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["iecountrydestination"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeCountryDestination["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeCountryDestination["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Flight Number

                    DataRow dataRowCostParamsTypeFlightNumber = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["flightnumber"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "FlightNumber is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeFlightNumber["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeFlightNumber["ID"] = 0;
                        dataRowCostParamsTypeFlightNumber["CostID"] = costID;
                        dataRowCostParamsTypeFlightNumber["ParamName"] = "FlightNum";
                        dataRowCostParamsTypeFlightNumber["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["flightnumber"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeFlightNumber["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieflightnumber"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeFlightNumber["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeFlightNumber["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Transit Station

                    DataRow dataRowCostParamsTypeTransitStation = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "TransitStation is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeTransitStation["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeTransitStation["ID"] = 0;
                        dataRowCostParamsTypeTransitStation["CostID"] = costID;
                        dataRowCostParamsTypeTransitStation["ParamName"] = "TransitStation";
                        dataRowCostParamsTypeTransitStation["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeTransitStation["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ietransitstation"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeTransitStation["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeTransitStation["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Days Of Week

                    DataRow dataRowCostParamsTypeDaysOfWeek = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "DaysOfWeek is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeDaysOfWeek["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeDaysOfWeek["ID"] = 0;
                        dataRowCostParamsTypeDaysOfWeek["CostID"] = costID;
                        dataRowCostParamsTypeDaysOfWeek["ParamName"] = "DaysOfWeek";
                        dataRowCostParamsTypeDaysOfWeek["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeDaysOfWeek["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["iedaysofweek"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeDaysOfWeek["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeDaysOfWeek["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Agent Code

                    DataRow dataRowCostParamsTypeAgentCode = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "AgentCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeAgentCode["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeAgentCode["ID"] = 0;
                        dataRowCostParamsTypeAgentCode["CostID"] = costID;
                        dataRowCostParamsTypeAgentCode["ParamName"] = "AgentCode";
                        dataRowCostParamsTypeAgentCode["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeAgentCode["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieagentcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeAgentCode["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeAgentCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Shipper Code

                    DataRow dataRowCostParamsTypeShipperCode = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "ShipperCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeShipperCode["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeShipperCode["ID"] = 0;
                        dataRowCostParamsTypeShipperCode["CostID"] = costID;
                        dataRowCostParamsTypeShipperCode["ParamName"] = "ShipperCode";
                        dataRowCostParamsTypeShipperCode["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeShipperCode["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieshippercode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeShipperCode["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeShipperCode["UpdatedBy"] = string.Empty;
                    }


                    #endregion

                    #region IATA Comm. Code

                    DataRow dataRowCostParamsTypeIATACommCode = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["iatacommcode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "IATACommCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeIATACommCode["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeIATACommCode["ID"] = 0;
                        dataRowCostParamsTypeIATACommCode["CostID"] = costID;
                        dataRowCostParamsTypeIATACommCode["ParamName"] = "IATACommCode";
                        dataRowCostParamsTypeIATACommCode["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["iatacommcode"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeIATACommCode["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieiatacommcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeIATACommCode["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeIATACommCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Product Type

                    DataRow dataRowCostParamsTypeProductType = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "ProductType is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeProductType["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeProductType["ID"] = 0;
                        dataRowCostParamsTypeProductType["CostID"] = costID;
                        dataRowCostParamsTypeProductType["ParamName"] = "ProductType";
                        dataRowCostParamsTypeProductType["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeProductType["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieproducttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeProductType["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeProductType["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region SPL Handling Code

                    DataRow dataRowCostParamsTypeSHCCode = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["shccode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "SHCCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeSHCCode["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeSHCCode["ID"] = 0;
                        dataRowCostParamsTypeSHCCode["CostID"] = costID;
                        dataRowCostParamsTypeSHCCode["ParamName"] = "HandlingCode";
                        dataRowCostParamsTypeSHCCode["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["shccode"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeSHCCode["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieshccode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeSHCCode["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeSHCCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Handler

                    DataRow dataRowCostParamsTypeHandler = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["handler"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Handler is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeHandler["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeHandler["ID"] = 0;
                        dataRowCostParamsTypeHandler["CostID"] = costID;
                        dataRowCostParamsTypeHandler["ParamName"] = "Handler";
                        dataRowCostParamsTypeHandler["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["handler"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeHandler["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["iehandler"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeHandler["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeHandler["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region Equipment Type

                    DataRow dataRowCostParamsTypeEquipmentType = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "EquipmentType is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeEquipmentType["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeEquipmentType["ID"] = 0;
                        dataRowCostParamsTypeEquipmentType["CostID"] = costID;
                        dataRowCostParamsTypeEquipmentType["ParamName"] = "EquipType";
                        dataRowCostParamsTypeEquipmentType["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeEquipmentType["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["ieequipmenttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeEquipmentType["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeEquipmentType["UpdatedBy"] = string.Empty;
                    }

                    #endregion

                    #region DepInterval

                    DataRow dataRowCostParamsTypeDepInterval = CostParamsType.NewRow();
                    if (dataTableCostMasterExcelData.Rows[i]["depinterval"].ToString().Trim().Trim(',').Length > 11)
                    {
                        validationDetails = validationDetails + "DepInterval is more than 10 Chars;";
                    }
                    else
                    {
                        dataRowCostParamsTypeDepInterval["ReferenceID"] = i + 1;
                        dataRowCostParamsTypeDepInterval["ID"] = 0;
                        dataRowCostParamsTypeDepInterval["CostID"] = costID;
                        dataRowCostParamsTypeDepInterval["ParamName"] = "DepInterval";
                        dataRowCostParamsTypeDepInterval["ParamValue"] = dataTableCostMasterExcelData.Rows[i]["depinterval"].ToString().Trim().Trim(',');
                        dataRowCostParamsTypeDepInterval["IsInclude"] = dataTableCostMasterExcelData.Rows[i]["iedepinterval"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowCostParamsTypeDepInterval["UpdatedOn"] = DateTime.Now;
                        dataRowCostParamsTypeDepInterval["UpdatedBy"] = string.Empty;
                    }

                    #endregion DepInterval

                    #endregion

                    CostSlabsTypeTemp.Rows.Clear();
                    CostULDSlabsTypeTemp.Rows.Clear();

                    for (int j = 64; j < dataTableCostMasterExcelData.Columns.Count; j++)
                    {
                        DataColumn dataColumn = dataTableCostMasterExcelData.Columns[j];

                        DataRow dataRowCostSlabsTypeTemp = CostSlabsTypeTemp.NewRow();
                        DataRow dataRowCostULDSlabsTypeTemp = CostULDSlabsTypeTemp.NewRow();

                        #region MIN Slab

                        if (dataColumn.ColumnName.Equals("min"))
                        {
                            tempDecimalValue = 0;
                            if (!string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i][j].ToString().Trim()))
                            {
                                if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                {
                                    if (tempDecimalValue > 0)
                                    {
                                        dataRowCostSlabsTypeTemp["ReferenceID"] = i + 1;
                                        dataRowCostSlabsTypeTemp["ID"] = 0;
                                        dataRowCostSlabsTypeTemp["CostID"] = costID;
                                        dataRowCostSlabsTypeTemp["SlabName"] = "M";
                                        dataRowCostSlabsTypeTemp["Weight"] = 0;
                                        dataRowCostSlabsTypeTemp["Charge"] = tempDecimalValue;
                                        dataRowCostSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                        dataRowCostSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                        CostSlabsTypeTemp.Rows.Add(dataRowCostSlabsTypeTemp);
                                        dataRowCostSlabsTypeTemp = CostSlabsTypeTemp.NewRow();
                                    }
                                }
                                else
                                {
                                    validationDetails = validationDetails + "Invalid MIN Value.;";
                                }
                            }
                        }
                        #endregion MIN Slab

                        #region N Slab

                        else if (dataColumn.ColumnName.Equals("n"))
                        {
                            tempDecimalValue = 0;
                            if (!string.IsNullOrWhiteSpace(dataTableCostMasterExcelData.Rows[i][j].ToString().Trim()))
                            {
                                if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                {
                                    if (tempDecimalValue > 0)
                                    {
                                        dataRowCostSlabsTypeTemp["ReferenceID"] = i + 1;
                                        dataRowCostSlabsTypeTemp["ID"] = 0;
                                        dataRowCostSlabsTypeTemp["CostID"] = costID;
                                        dataRowCostSlabsTypeTemp["SlabName"] = "N";
                                        dataRowCostSlabsTypeTemp["Weight"] = 0;
                                        dataRowCostSlabsTypeTemp["Charge"] = tempDecimalValue;
                                        dataRowCostSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                        dataRowCostSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                        CostSlabsTypeTemp.Rows.Add(dataRowCostSlabsTypeTemp);
                                        dataRowCostSlabsTypeTemp = CostSlabsTypeTemp.NewRow();
                                    }
                                }
                                else
                                {
                                    validationDetails = validationDetails + "Invalid MIN Value.;";
                                }
                            }
                        }

                        #endregion N Slab

                        #region ULD Slabs

                        else if (dataColumn.ColumnName.StartsWith("u_"))
                        {
                            uLDType = string.Empty;
                            uLDType = dataColumn.ColumnName.Replace("u_", "");
                            if (!uLDType.Equals(string.Empty))
                            {
                                uLDType = uLDType.ToUpper();
                                tempDecimalValue = 0;
                                if (!dataTableCostMasterExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                {
                                    if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                    {
                                        if (tempDecimalValue >= 0)
                                        {
                                            dataRowCostULDSlabsTypeTemp["ReferenceID"] = i + 1;
                                            dataRowCostULDSlabsTypeTemp["ID"] = 0;
                                            dataRowCostULDSlabsTypeTemp["COstID"] = costID;
                                            dataRowCostULDSlabsTypeTemp["ULDType"] = uLDType;
                                            dataRowCostULDSlabsTypeTemp["SlabName"] = "M";
                                            dataRowCostULDSlabsTypeTemp["Weight"] = 0;
                                            dataRowCostULDSlabsTypeTemp["Charge"] = tempDecimalValue;
                                            dataRowCostULDSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                            dataRowCostULDSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                            CostULDSlabsTypeTemp.Rows.Add(dataRowCostULDSlabsTypeTemp);
                                            dataRowCostULDSlabsTypeTemp = CostULDSlabsTypeTemp.NewRow();
                                        }
                                    }
                                    else
                                    {
                                        validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j].ToString() + ".;";
                                    }
                                }

                                if (j < dataTableCostMasterExcelData.Columns.Count - 1)
                                {
                                    if (dataTableCostMasterExcelData.Columns[j + 1].ColumnName.StartsWith("ovpwt"))
                                    {
                                        if (!dataTableCostMasterExcelData.Rows[i][j + 1].ToString().Trim().Equals(string.Empty) &&
                                            !dataTableCostMasterExcelData.Rows[i][j + 2].ToString().Trim().Equals(string.Empty))
                                        {
                                            tempDecimalWtValue = 0;
                                            if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j + 1].ToString().Trim(), out tempDecimalWtValue))
                                            {
                                                tempDecimalChargValue = 0;
                                                if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j + 2].ToString().Trim(), out tempDecimalChargValue))
                                                {
                                                    if (tempDecimalWtValue >= 0 && tempDecimalChargValue >= 0)
                                                    {
                                                        dataRowCostULDSlabsTypeTemp["ReferenceID"] = i + 1;
                                                        dataRowCostULDSlabsTypeTemp["ID"] = 0;
                                                        dataRowCostULDSlabsTypeTemp["COstID"] = costID;
                                                        dataRowCostULDSlabsTypeTemp["ULDType"] = uLDType;
                                                        dataRowCostULDSlabsTypeTemp["SlabName"] = "OverPivot";
                                                        dataRowCostULDSlabsTypeTemp["Weight"] = tempDecimalWtValue;
                                                        dataRowCostULDSlabsTypeTemp["Charge"] = tempDecimalChargValue;
                                                        dataRowCostULDSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                                        dataRowCostULDSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                                        CostULDSlabsTypeTemp.Rows.Add(dataRowCostULDSlabsTypeTemp);
                                                        dataRowCostULDSlabsTypeTemp = CostULDSlabsTypeTemp.NewRow();
                                                    }
                                                }
                                                else
                                                {
                                                    validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j + 2].ToString() + ".;";
                                                }
                                            }
                                            else
                                            {
                                                validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j + 1].ToString() + ".;";
                                            }
                                        }

                                        if (j < dataTableCostMasterExcelData.Columns.Count - 3)
                                        {
                                            if (dataTableCostMasterExcelData.Columns[j + 3].ColumnName.StartsWith("flatwt"))
                                            {
                                                if (!dataTableCostMasterExcelData.Rows[i][j + 3].ToString().Trim().Equals(string.Empty) &&
                                                    !dataTableCostMasterExcelData.Rows[i][j + 4].ToString().Trim().Equals(string.Empty))
                                                {
                                                    tempDecimalWtValue = 0;
                                                    if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j + 3].ToString().Trim(), out tempDecimalWtValue))
                                                    {
                                                        tempDecimalChargValue = 0;
                                                        if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j + 4].ToString().Trim(), out tempDecimalChargValue))
                                                        {
                                                            if (tempDecimalWtValue >= 0 && tempDecimalChargValue >= 0)
                                                            {
                                                                dataRowCostULDSlabsTypeTemp["ReferenceID"] = i + 1;
                                                                dataRowCostULDSlabsTypeTemp["ID"] = 0;
                                                                dataRowCostULDSlabsTypeTemp["COstID"] = costID;
                                                                dataRowCostULDSlabsTypeTemp["ULDType"] = uLDType;
                                                                dataRowCostULDSlabsTypeTemp["SlabName"] = "F";
                                                                dataRowCostULDSlabsTypeTemp["Weight"] = tempDecimalWtValue;
                                                                dataRowCostULDSlabsTypeTemp["Charge"] = tempDecimalChargValue;
                                                                dataRowCostULDSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                                                dataRowCostULDSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                                                CostULDSlabsTypeTemp.Rows.Add(dataRowCostULDSlabsTypeTemp);
                                                                dataRowCostULDSlabsTypeTemp = CostULDSlabsTypeTemp.NewRow();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j + 4].ToString() + ".;";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j + 3].ToString() + ".;";
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (dataTableCostMasterExcelData.Columns[j + 1].ColumnName.StartsWith("flatwt"))
                                    {
                                        if (!dataTableCostMasterExcelData.Rows[i][j + 1].ToString().Trim().Equals(string.Empty) &&
                                            !dataTableCostMasterExcelData.Rows[i][j + 2].ToString().Trim().Equals(string.Empty))
                                        {
                                            tempDecimalWtValue = 0;
                                            if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j + 1].ToString().Trim(), out tempDecimalWtValue))
                                            {
                                                tempDecimalChargValue = 0;
                                                if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j + 2].ToString().Trim(), out tempDecimalChargValue))
                                                {
                                                    if (tempDecimalWtValue >= 0 && tempDecimalChargValue >= 0)
                                                    {
                                                        dataRowCostULDSlabsTypeTemp["ReferenceID"] = i + 1;
                                                        dataRowCostULDSlabsTypeTemp["ID"] = 0;
                                                        dataRowCostULDSlabsTypeTemp["COstID"] = costID;
                                                        dataRowCostULDSlabsTypeTemp["ULDType"] = uLDType;
                                                        dataRowCostULDSlabsTypeTemp["SlabName"] = "F";
                                                        dataRowCostULDSlabsTypeTemp["Weight"] = tempDecimalWtValue;
                                                        dataRowCostULDSlabsTypeTemp["Charge"] = tempDecimalChargValue;
                                                        dataRowCostULDSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                                        dataRowCostULDSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                                        CostULDSlabsTypeTemp.Rows.Add(dataRowCostULDSlabsTypeTemp);
                                                        dataRowCostULDSlabsTypeTemp = CostULDSlabsTypeTemp.NewRow();
                                                    }
                                                }
                                                else
                                                {
                                                    validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j + 2].ToString() + ".;";
                                                }
                                            }
                                            else
                                            {
                                                validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j + 1].ToString() + ".;";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                validationDetails = validationDetails + "Invalid Column Name: " + dataColumn.ColumnName + ".;";
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
                                columnNameStartDecimal = 0;
                                if (Decimal.TryParse(strArrColName[0], out columnNameStartDecimal))
                                {
                                    if (strArrColName.Length == 1 && columnNameStartDecimal > 0)
                                    {
                                        tempDecimalValue = 0;
                                        if (!dataTableCostMasterExcelData.Rows[i][j].ToString().Trim().Equals(string.Empty))
                                        {
                                            if (decimal.TryParse(dataTableCostMasterExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                            {
                                                if (tempDecimalValue > 0)
                                                {
                                                    dataRowCostSlabsTypeTemp["ReferenceID"] = i + 1;
                                                    dataRowCostSlabsTypeTemp["ID"] = 0;
                                                    dataRowCostSlabsTypeTemp["CostID"] = costID;
                                                    dataRowCostSlabsTypeTemp["SlabName"] = "F";
                                                    dataRowCostSlabsTypeTemp["Weight"] = columnNameStartDecimal;
                                                    dataRowCostSlabsTypeTemp["Charge"] = tempDecimalValue;
                                                    dataRowCostSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                                    dataRowCostSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                                    CostSlabsTypeTemp.Rows.Add(dataRowCostSlabsTypeTemp);
                                                    dataRowCostSlabsTypeTemp = CostSlabsTypeTemp.NewRow();
                                                }
                                            }
                                            else
                                            {
                                                validationDetails = validationDetails + "Invalid Column Value: " + dataTableCostMasterExcelData.Rows[i][j].ToString() + ".;";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    validationDetails = validationDetails + "Invalid Column Name: " + dataColumn.ColumnName + ".;";
                                }
                            }
                        }

                        #endregion
                    }

                    if (validationDetails.Equals(string.Empty))
                    {
                        dataRowCostType["validationDetails"] = string.Empty;

                        CostRemarksType.Rows.Add(dataRowCostRemarksType);

                        CostParamsType.Rows.Add(dataRowCostParamsTypeFlightCarrier);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeIssueCarrier);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeAirlineCode);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeOrigin);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeCountrySource);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeDestination);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeCountryDestination);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeFlightNumber);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeTransitStation);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeDaysOfWeek);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeAgentCode);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeShipperCode);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeIATACommCode);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeProductType);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeSHCCode);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeHandler);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeEquipmentType);
                        CostParamsType.Rows.Add(dataRowCostParamsTypeDepInterval);

                        foreach (DataRow dataRowCostSlabsTypeTemp in CostSlabsTypeTemp.Rows)
                        {
                            CostSlabsType.Rows.Add(dataRowCostSlabsTypeTemp.ItemArray);
                        }

                        foreach (DataRow dataRowCostULDSlabsTypeTemp in CostULDSlabsTypeTemp.Rows)
                        {
                            CostULDSlabsType.Rows.Add(dataRowCostULDSlabsTypeTemp.ItemArray);
                        }
                    }
                    else
                    {
                        dataRowCostType["validationDetails"] = validationDetails;
                    }

                    CostType.Rows.Add(dataRowCostType);
                }

                // Database Call to Validate & Insert Cost Line Master
                string errorInSp = string.Empty;
                ValidateAndInsertCostMaster(srNotblMasterUploadSummaryLog, CostType, CostSlabsType, CostULDSlabsType, CostParamsType, CostRemarksType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableCostMasterExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Cost Line Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> tblMasterUploadSummaryLog Primay Key </param>
        /// <param name="dataTableCostType"> CostType DataTable </param>
        /// <param name="dataTableCostSlabsType"> CostSlabsType DataTable </param>
        /// <param name="dataTableCostULDSlabsType"> CostULDSlabsType DataTable </param>
        /// <param name="dataTableCostParamsType"> CostParamsType DataTable </param>
        /// <param name="dataTableCostRemarksType"> CostRemarksType DataTable </param>
        /// <param name="errorInSp"> Stored Procedure Out Parameter </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public DataSet ValidateAndInsertCostMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableCostType,
                                                                                      DataTable dataTableCostSlabsType,
                                                                                      DataTable dataTableCostULDSlabsType,
                                                                                      DataTable dataTableCostParamsType,
                                                                                      DataTable dataTableCostRemarksType,
                                                                                      string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog", srNotblMasterUploadSummaryLog),
                                                                    new SqlParameter("@CostTableType", dataTableCostType),
                                                                    new SqlParameter("@CostSlabsTableType", dataTableCostSlabsType),
                                                                    new SqlParameter("@CostULDSlabsTableType", dataTableCostULDSlabsType),
                                                                    new SqlParameter("@CostParamsTableType", dataTableCostParamsType),
                                                                    new SqlParameter("@CostRemarksTableType", dataTableCostRemarksType),
                                                                    new SqlParameter("@Error", errorInSp)
                                                                  };



                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("Masters.uspUploadCostMaster", sqlParameters);

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
