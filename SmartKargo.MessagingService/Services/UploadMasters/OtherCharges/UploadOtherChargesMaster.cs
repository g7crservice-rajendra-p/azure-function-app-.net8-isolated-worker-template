using Excel;
using QID.DataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace QidWorkerRole.UploadMasters.OtherCharges
{
    /// <summary>
    /// Class to Upload Other Charges Master File.
    /// </summary>
    public class UploadOtherChargesMaster
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod OtherCharges Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public Boolean OtherChargesMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.OtherCharges);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "OtherChargesMasterUploadFile", out uploadFilePath))
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
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
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
            DataTable dataTableOtherChargesExcelData = new DataTable("dataTableOtherChargesExcelData");

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
                dataTableOtherChargesExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableOtherChargesExcelData);

                foreach (DataColumn dataColumn in dataTableOtherChargesExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                #region Creating OtherChargesMasterType DataTable

                DataTable OtherChargesMasterType = new DataTable("OtherChargesMasterType");
                OtherChargesMasterType.Columns.Add("ReferenceID", System.Type.GetType("System.Int16"));
                OtherChargesMasterType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                OtherChargesMasterType.Columns.Add("ChargeHeadCode", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("ChargeHeadName", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("ParticipationType", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("Refundable", System.Type.GetType("System.Byte"));
                OtherChargesMasterType.Columns.Add("StartDate", System.Type.GetType("System.DateTime"));
                OtherChargesMasterType.Columns.Add("EndDate", System.Type.GetType("System.DateTime"));
                OtherChargesMasterType.Columns.Add("LocationLevel", System.Type.GetType("System.Int32"));
                OtherChargesMasterType.Columns.Add("Location", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("OriginLevel", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("Origin", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("DestinationLevel", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("Destination", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("Currency", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("PaymentType", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("ChargeType", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("DiscountPercent", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("CommPercent", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("ServiceTax", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("TDSPercent", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("Type", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("Stage", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("InterlineCarrier", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("ChargeHeadBasis", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("MinimumCharge", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("PerUnitCharge", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("WeightType", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("Status", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                OtherChargesMasterType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("ViaStation", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("chargedAt", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("IsOnAWB", System.Type.GetType("System.Byte"));
                OtherChargesMasterType.Columns.Add("ChargeCode", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("BasedOn", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("BaseRate", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("MinimumCost", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("PerUnitCost", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("GLCode", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("MaxValue", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("MaxCost", System.Type.GetType("System.Decimal"));
                OtherChargesMasterType.Columns.Add("OCCode", System.Type.GetType("System.String"));
                OtherChargesMasterType.Columns.Add("IgnoreCCSF", System.Type.GetType("System.Byte"));
                OtherChargesMasterType.Columns.Add("IsPackaging", System.Type.GetType("System.Byte"));
                OtherChargesMasterType.Columns.Add("isEditable", System.Type.GetType("System.Byte"));
                OtherChargesMasterType.Columns.Add("UOM", System.Type.GetType("System.String"));
                
                OtherChargesMasterType.Columns.Add("validationDetails", System.Type.GetType("System.String"));

                #endregion

                #region Creating OtherChargesParamType DataTable

                DataTable OtherChargesParamType = new DataTable("OtherChargesParamType");
                OtherChargesParamType.Columns.Add("ReferenceID", System.Type.GetType("System.Int16"));
                OtherChargesParamType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                OtherChargesParamType.Columns.Add("ChargeHeadSrNo", System.Type.GetType("System.Int32"));
                OtherChargesParamType.Columns.Add("ParamName", System.Type.GetType("System.String"));
                OtherChargesParamType.Columns.Add("ParamValue", System.Type.GetType("System.String"));
                OtherChargesParamType.Columns.Add("IsInclude", System.Type.GetType("System.Byte"));
                OtherChargesParamType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                OtherChargesParamType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                #endregion

                #region Creating OtherChargeSlabsType DataTable

                DataTable OtherChargeSlabsType = new DataTable("OtherChargeSlabsType");
                OtherChargeSlabsType.Columns.Add("ReferenceID", System.Type.GetType("System.Int16"));
                OtherChargeSlabsType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                OtherChargeSlabsType.Columns.Add("ChargeHeadSrNo", System.Type.GetType("System.Int32"));
                OtherChargeSlabsType.Columns.Add("SlabName", System.Type.GetType("System.String"));
                OtherChargeSlabsType.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                OtherChargeSlabsType.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                OtherChargeSlabsType.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                OtherChargeSlabsType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                OtherChargeSlabsType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                DataTable OtherChargeSlabsTypeTemp = new DataTable("OtherChargeSlabsTypeTemp");
                OtherChargeSlabsTypeTemp.Columns.Add("ReferenceID", System.Type.GetType("System.Int16"));
                OtherChargeSlabsTypeTemp.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                OtherChargeSlabsTypeTemp.Columns.Add("ChargeHeadSrNo", System.Type.GetType("System.Int32"));
                OtherChargeSlabsTypeTemp.Columns.Add("SlabName", System.Type.GetType("System.String"));
                OtherChargeSlabsTypeTemp.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                OtherChargeSlabsTypeTemp.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                OtherChargeSlabsTypeTemp.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                OtherChargeSlabsTypeTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                OtherChargeSlabsTypeTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                #endregion

                #region Creating ULDOCSlabsType DataTable

                DataTable ULDOCSlabsType = new DataTable("ULDOCSlabsType");
                ULDOCSlabsType.Columns.Add("ReferenceID", System.Type.GetType("System.Int16"));
                ULDOCSlabsType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                ULDOCSlabsType.Columns.Add("OCSrNo", System.Type.GetType("System.Int32"));
                ULDOCSlabsType.Columns.Add("ULDType", System.Type.GetType("System.String"));
                ULDOCSlabsType.Columns.Add("SlabName", System.Type.GetType("System.String"));
                ULDOCSlabsType.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                ULDOCSlabsType.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                ULDOCSlabsType.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                ULDOCSlabsType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                ULDOCSlabsType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                DataTable ULDOCSlabsTypeTemp = new DataTable("ULDOCSlabsTypeTemp");
                ULDOCSlabsTypeTemp.Columns.Add("ReferenceID", System.Type.GetType("System.Int16"));
                ULDOCSlabsTypeTemp.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                ULDOCSlabsTypeTemp.Columns.Add("OCSrNo", System.Type.GetType("System.Int32"));
                ULDOCSlabsTypeTemp.Columns.Add("ULDType", System.Type.GetType("System.String"));
                ULDOCSlabsTypeTemp.Columns.Add("SlabName", System.Type.GetType("System.String"));
                ULDOCSlabsTypeTemp.Columns.Add("Weight", System.Type.GetType("System.Decimal"));
                ULDOCSlabsTypeTemp.Columns.Add("Charge", System.Type.GetType("System.Decimal"));
                ULDOCSlabsTypeTemp.Columns.Add("Cost", System.Type.GetType("System.Decimal"));
                ULDOCSlabsTypeTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                ULDOCSlabsTypeTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));

                #endregion

                string validationDetails = string.Empty;
                int serialNumberOC = 0;
                DateTime tempDate;
                decimal tempDecimalValue = 0;
                int intColumnName = 0;

                for (int i = 0; i < dataTableOtherChargesExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;
                    serialNumberOC = 0;
                    tempDecimalValue = 0;

                    #region Create row for OtherChargesMaster Data Table

                    DataRow dataRowOtherChargesMasterType = OtherChargesMasterType.NewRow();

                    // Maintained for Indexing purpose
                    dataRowOtherChargesMasterType["ReferenceID"] = i + 1;

                    #region SerialNumber INT

                    if (dataTableOtherChargesExcelData.Rows[i]["ocid"] == null)
                    {
                        dataRowOtherChargesMasterType["SerialNumber"] = serialNumberOC;
                    }
                    else if (dataTableOtherChargesExcelData.Rows[i]["ocid"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowOtherChargesMasterType["SerialNumber"] = serialNumberOC;
                    }
                    else
                    {
                        if (int.TryParse(dataTableOtherChargesExcelData.Rows[i]["ocid"].ToString().Trim().Trim(','), out serialNumberOC))
                        {
                            dataRowOtherChargesMasterType["SerialNumber"] = serialNumberOC;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["SerialNumber"] = serialNumberOC;
                            validationDetails = validationDetails + "Invalid OCID;";
                        }
                    }

                    #endregion SerialNumber

                    #region OCCode [varchar] (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["occode"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "OCCode is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["occode"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + "OCCode is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["ChargeHeadCode"] = dataTableOtherChargesExcelData.Rows[i]["occode"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion OCCode

                    #region ChargeDescription [varchar] (100) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["chargedescription"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "ChargeDescription is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["chargedescription"].ToString().Trim().Trim(',').Length > 100)
                        {
                            validationDetails = validationDetails + "ChargeDescription is more than 100 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["ChargeHeadName"] = dataTableOtherChargesExcelData.Rows[i]["chargedescription"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion ChargeDescription

                    #region ParticipationType [varchar] (10) NULL

                    dataRowOtherChargesMasterType["ParticipationType"] = "N";

                    #endregion ParticipationType

                    #region Refundable [bit] NULL

                    dataRowOtherChargesMasterType["Refundable"] = 0;

                    #endregion Refundable

                    #region StartDate [datetime] NULL

                    if (dataTableOtherChargesExcelData.Rows[i]["startdate"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetails = validationDetails + "StartDate is required;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowOtherChargesMasterType["StartDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableOtherChargesExcelData.Rows[i]["startdate"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableOtherChargesExcelData.Rows[i]["startdate"].ToString().Trim(), out tempDate))
                                {
                                    dataRowOtherChargesMasterType["StartDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowOtherChargesMasterType["StartDate"] = DateTime.MinValue;
                                    validationDetails = validationDetails + "Invalid StartDate;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowOtherChargesMasterType["StartDate"] = DateTime.MinValue;
                            validationDetails = validationDetails + "Invalid StartDate;";
                        }
                    }

                    #endregion StartDate

                    #region EndDate [datetime] NULL

                    if (dataTableOtherChargesExcelData.Rows[i]["enddate"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        validationDetails = validationDetails + "EndDate is required;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowOtherChargesMasterType["EndDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableOtherChargesExcelData.Rows[i]["enddate"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableOtherChargesExcelData.Rows[i]["enddate"].ToString().Trim(), out tempDate))
                                {
                                    dataRowOtherChargesMasterType["EndDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowOtherChargesMasterType["EndDate"] = DateTime.MaxValue;
                                    validationDetails = validationDetails + "Invalid EndDate;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowOtherChargesMasterType["EndDate"] = DateTime.MaxValue;
                            validationDetails = validationDetails + "Invalid EndDate;";
                        }
                    }

                    #endregion EndDate

                    #region Level [int] NULL

                    if (dataTableOtherChargesExcelData.Rows[i]["level"].ToString().Trim().Trim(',').Equals(string.Empty))
                    {
                        dataRowOtherChargesMasterType["LocationLevel"] = DBNull.Value;
                    }
                    else
                    {
                        switch (dataTableOtherChargesExcelData.Rows[i]["level"].ToString().Trim().Trim(','))
                        {
                            case "Airport":
                                dataRowOtherChargesMasterType["LocationLevel"] = 0;
                                break;
                            case "City":
                                dataRowOtherChargesMasterType["LocationLevel"] = 1;
                                break;
                            case "Region":
                                dataRowOtherChargesMasterType["LocationLevel"] = 2;
                                break;
                            case "Country":
                                dataRowOtherChargesMasterType["LocationLevel"] = 3;
                                break;
                            case "Zone":
                                dataRowOtherChargesMasterType["LocationLevel"] = 4;
                                break;
                            default:
                                dataRowOtherChargesMasterType["LocationLevel"] = DBNull.Value;
                                break;
                        }
                    }

                    #endregion Level

                    #region Location [varchar] (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["location"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["Location"] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["location"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + "Location is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["Location"] = dataTableOtherChargesExcelData.Rows[i]["location"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Location

                    #region OriginLevel [varchar] (100) NULL is INT in table but will convert in SP

                    if (dataTableOtherChargesExcelData.Rows[i]["originlevel"].ToString().Trim().Trim(',').Length > 100)
                    {
                        dataRowOtherChargesMasterType["OriginLevel"] = dataTableOtherChargesExcelData.Rows[i]["originlevel"].ToString().Trim().ToUpper().Trim(',').Substring(0, 99);
                    }
                    else
                    {
                        dataRowOtherChargesMasterType["OriginLevel"] = dataTableOtherChargesExcelData.Rows[i]["originlevel"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion OriginLevel

                    #region Origin [varchar] (15) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["origin"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "Origin is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 15)
                        {
                            validationDetails = validationDetails + "Origin is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["Origin"] = dataTableOtherChargesExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Origin

                    #region DestinationLevel [varchar] (100) NULL is INT in table but will convert in SP

                    if (dataTableOtherChargesExcelData.Rows[i]["destinationlevel"].ToString().Trim().Trim(',').Length > 100)
                    {
                        dataRowOtherChargesMasterType["DestinationLevel"] = dataTableOtherChargesExcelData.Rows[i]["destinationlevel"].ToString().Trim().ToUpper().Trim(',').Substring(0, 99);
                    }
                    else
                    {
                        dataRowOtherChargesMasterType["DestinationLevel"] = dataTableOtherChargesExcelData.Rows[i]["destinationlevel"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion DestinationLevel

                    #region Destination [varchar] (15) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["destination"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "Destination is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 15)
                        {
                            validationDetails = validationDetails + "Destination is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["Destination"] = dataTableOtherChargesExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Destination

                    #region Currency [varchar] (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["currency"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "Currency is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + "Currency is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["Currency"] = dataTableOtherChargesExcelData.Rows[i]["currency"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Currency

                    #region PaymentType [varchar] (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["paymenttype"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "PaymentType is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["paymenttype"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + "PaymentType is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["PaymentType"] = dataTableOtherChargesExcelData.Rows[i]["paymenttype"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion PaymentType

                    #region ChargeType [varchar] (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["chargetype"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "chargetype is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["chargetype"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + "chargetype is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["ChargeType"] = dataTableOtherChargesExcelData.Rows[i]["chargetype"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion ChargeType

                    #region DiscountPercent [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["discount%"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["DiscountPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["discount%"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["DiscountPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["DiscountPercent"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Discount%;";
                        }
                    }

                    #endregion DiscountPercent

                    #region CommPercent [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["agentcommission%"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["CommPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["agentcommission%"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["CommPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["CommPercent"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid AgentCommission%;";
                        }
                    }

                    #endregion CommPercent

                    #region ServiceTax [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["tax%"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["ServiceTax"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["tax%"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["ServiceTax"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["ServiceTax"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid Tax%;";
                        }
                    }

                    #endregion ServiceTax

                    #region TDSPercent [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["tds %"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["TDSPercent"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["tds %"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["TDSPercent"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["TDSPercent"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid TDS %;";
                        }
                    }

                    #endregion TDSPercent

                    #region Type [varchar] (5) NULL

                    dataRowOtherChargesMasterType["Type"] = DBNull.Value;

                    #endregion Type

                    #region Stage [varchar] (5) NULL

                    dataRowOtherChargesMasterType["Stage"] = DBNull.Value;

                    #endregion Stage

                    #region InterlineCarrier [varchar] (500) NULL

                    dataRowOtherChargesMasterType["InterlineCarrier"] = DBNull.Value;

                    #endregion InterlineCarrier

                    #region ChargeHeadBasis [varchar] (15) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["chargeheadbasis"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "ChargeHeadBasis is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["chargeheadbasis"].ToString().Trim().Trim(',').Length > 15)
                        {
                            validationDetails = validationDetails + "ChargeHeadBasis is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["ChargeHeadBasis"] = dataTableOtherChargesExcelData.Rows[i]["chargeheadbasis"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion ChargeHeadBasis

                    #region MinimumCharge [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["minimumcharge"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["MinimumCharge"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["minimumcharge"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["MinimumCharge"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["MinimumCharge"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid MinimumCharge;";
                        }
                    }

                    #endregion MinimumCharge

                    #region PerUnitCharge [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["perunitcharge"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["PerUnitCharge"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["perunitcharge"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["PerUnitCharge"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["PerUnitCharge"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid PerUnitCharge;";
                        }
                    }

                    #endregion PerUnitCharge

                    #region WeightType [varchar] (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["weighttype"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["WeightType"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["weighttype"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + "weightType is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["WeightType"] = dataTableOtherChargesExcelData.Rows[i]["weighttype"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion WeightType

                    #region Status [varchar] (5) NULL

                    dataRowOtherChargesMasterType["Status"] = string.Empty;

                    #endregion Status

                    #region UpdatedOn [datetime] NULL

                    dataRowOtherChargesMasterType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region UpdatedBy [varchar] (30) NULL

                    dataRowOtherChargesMasterType["UpdatedBy"] = string.Empty;

                    #endregion UpdatedBy

                    #region ViaStation [varchar] (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["viastation"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["ViaStation"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["viastation"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + "ViaStation is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["ViaStation"] = dataTableOtherChargesExcelData.Rows[i]["viastation"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion ViaStation

                    #region chargedAt [varchar] (5) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["chargedat"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "chargedAt is required;";
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["chargedat"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationDetails = validationDetails + "chargedAt is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["chargedAt"] = dataTableOtherChargesExcelData.Rows[i]["chargedat"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion chargedAt

                    #region IsOnAWB [bit] NULL

                    dataRowOtherChargesMasterType["IsOnAWB"] = 0;

                    #endregion IsOnAWB

                    #region ChargeCode [varchar] (5) NULL

                    if (dataTableOtherChargesExcelData.Columns.Contains("ChargeCode"))
                    {
                        if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["ChargeCode"].ToString().Trim().Trim(',')))
                        {
                            validationDetails = validationDetails + "ChargeCode is required;";
                        }
                        else
                        {
                            if (dataTableOtherChargesExcelData.Rows[i]["ChargeCode"].ToString().Trim() == "Normal" || dataTableOtherChargesExcelData.Rows[i]["ChargeCode"].ToString().Trim() == "All-In")
                            {
                                dataRowOtherChargesMasterType["ChargeCode"] = dataTableOtherChargesExcelData.Rows[i]["ChargeCode"].ToString().Trim().Trim(',') == "All-In" ? "A" : "N";
                            }
                            else
                            {
                                validationDetails = validationDetails + "ChargeCode value should be Normal OR All-In;";
                            }
                        }
                    }
                    else
                    {
                        dataRowOtherChargesMasterType["ChargeCode"] = "N";
                    }
                    #endregion ChargeCode

                    #region BasedOn [varchar] (10) NULL

                    dataRowOtherChargesMasterType["BasedOn"] = DBNull.Value;

                    #endregion BasedOn

                    #region BaseRate [decimal] (18, 2) NULL

                    dataRowOtherChargesMasterType["BaseRate"] = DBNull.Value;

                    #endregion BaseRate

                    #region MinimumCost [decimal] (18, 2) NULL

                    dataRowOtherChargesMasterType["MinimumCost"] = DBNull.Value;

                    #endregion MinimumCost

                    #region PerUnitCost [decimal] (18, 2) NULL

                    dataRowOtherChargesMasterType["PerUnitCost"] = DBNull.Value;

                    #endregion PerUnitCost

                    #region GLCode [varchar] (10) NULL

                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["glaccountcode"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["GLCode"] = string.Empty;
                    }
                    else
                    {
                        if (dataTableOtherChargesExcelData.Rows[i]["glaccountcode"].ToString().Trim().Trim(',').Length > 10)
                        {
                            validationDetails = validationDetails + "GLAccountCode is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["GLCode"] = dataTableOtherChargesExcelData.Rows[i]["glaccountcode"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion GLCode

                    #region MaxValue [decimal] (18, 2) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i]["maxcharge"].ToString().Trim().Trim(',')))
                    {
                        dataRowOtherChargesMasterType["MaxValue"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i]["maxcharge"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowOtherChargesMasterType["MaxValue"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowOtherChargesMasterType["MaxValue"] = tempDecimalValue;
                            validationDetails = validationDetails + "Invalid MaxCharge;";
                        }
                    }

                    #endregion MaxValue

                    #region MaxCost [decimal] (18, 2) NULL

                    dataRowOtherChargesMasterType["MaxCost"] = DBNull.Value;

                    #endregion MaxCost

                    #region OCCode [varchar] (10) NULL

                    dataRowOtherChargesMasterType["OCCode"] = dataRowOtherChargesMasterType["ChargeHeadCode"];

                    #endregion OCCode

                    #region IgnoreCCSF [bit] NULL

                    dataRowOtherChargesMasterType["IgnoreCCSF"] = DBNull.Value;

                    #endregion IgnoreCCSF

                    #region IsPackaging [bit] NULL
                    if (dataTableOtherChargesExcelData.Columns.Contains("misccharges"))
                    {
                        switch (dataTableOtherChargesExcelData.Rows[i]["misccharges"].ToString().Trim().Trim(','))
                        {
                            case "Y":
                                dataRowOtherChargesMasterType["IsPackaging"] = 1;
                                break;
                            case "1":
                                dataRowOtherChargesMasterType["IsPackaging"] = 1;
                                break;
                            default:
                                dataRowOtherChargesMasterType["IsPackaging"] = 0;
                                break;
                        }
                    }
                    //dataRowOtherChargesMasterType["IsPackaging"] = DBNull.Value;

                    #endregion IsPackaging

                    #region isEditable [bit] NULL

                    dataRowOtherChargesMasterType["isEditable"] = DBNull.Value;

                    #endregion isEditable

                    #region UOM [varchar] (3) NULL

                    dataRowOtherChargesMasterType["UOM"] = DBNull.Value;

                    #endregion UOM

                   

                    #endregion Create row for OtherChargesMaster Data Table

                    #region Create rows for OtherChargesParamType Data Table

                    #region Handler

                    DataRow dataRowOtherChargesParamTypeHandler = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["handler"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Handler is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeHandler["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeHandler["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeHandler["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeHandler["ParamName"] = "Handler";
                        dataRowOtherChargesParamTypeHandler["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["handler"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeHandler["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["iehandler"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeHandler["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeHandler["UpdatedBy"] = string.Empty;
                    }

                    #endregion Handler

                    #region EquipType

                    DataRow dataRowOtherChargesParamTypeEquipType = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "EquipmentType is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeEquipType["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeEquipType["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeEquipType["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeEquipType["ParamName"] = "EquipType";
                        dataRowOtherChargesParamTypeEquipType["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeEquipType["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieequiptype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeEquipType["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeEquipType["UpdatedBy"] = string.Empty;
                    }

                    #endregion EquipType

                    #region CommCode

                    DataRow dataRowOtherChargesParamTypeCommCode = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["iatacommcode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "IATACommCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeCommCode["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeCommCode["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeCommCode["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeCommCode["ParamName"] = "CommCode";
                        dataRowOtherChargesParamTypeCommCode["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["iatacommcode"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeCommCode["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieiatacommcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeCommCode["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeCommCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion CommCode

                    #region ProductType

                    DataRow dataRowOtherChargesParamTypeProductType = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "ProductType is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeProductType["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeProductType["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeProductType["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeProductType["ParamName"] = "ProductType";
                        dataRowOtherChargesParamTypeProductType["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeProductType["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieproducttype"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeProductType["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeProductType["UpdatedBy"] = string.Empty;
                    }

                    #endregion ProductType

                    #region FlightNum

                    DataRow dataRowOtherChargesParamTypeFlightNum = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["flight#"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Flight# is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeFlightNum["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeFlightNum["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeFlightNum["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeFlightNum["ParamName"] = "FlightNum";
                        dataRowOtherChargesParamTypeFlightNum["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["flight#"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeFlightNum["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieflightnum"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeFlightNum["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeFlightNum["UpdatedBy"] = string.Empty;
                    }

                    #endregion FlightNum

                    #region ShippingAgentCode

                    DataRow dataRowOtherChargesParamTypeAgentCode = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["shippingagentcode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "ShippingAgentCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeAgentCode["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeAgentCode["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeAgentCode["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeAgentCode["ParamName"] = "AgentCode";
                        dataRowOtherChargesParamTypeAgentCode["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["shippingagentcode"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeAgentCode["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieshippingagentcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeAgentCode["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeAgentCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion ShippingAgentCode

                    #region HandlingCode

                    DataRow dataRowOtherChargesParamTypeHandlingCode = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["shc"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "SHC is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeHandlingCode["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeHandlingCode["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeHandlingCode["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeHandlingCode["ParamName"] = "HandlingCode";
                        dataRowOtherChargesParamTypeHandlingCode["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["shc"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeHandlingCode["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieshc"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeHandlingCode["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeHandlingCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion HandlingCode

                    #region DaysOfWeek

                    DataRow dataRowOtherChargesParamTypeDaysOfWeek = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["dayofweek"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Dayofweek is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeDaysOfWeek["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeDaysOfWeek["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeDaysOfWeek["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeDaysOfWeek["ParamName"] = "DaysOfWeek";
                        dataRowOtherChargesParamTypeDaysOfWeek["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["dayofweek"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeDaysOfWeek["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["iedaysofweek"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeDaysOfWeek["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeDaysOfWeek["UpdatedBy"] = string.Empty;
                    }

                    #endregion DaysOfWeek

                    #region FlightCarrier

                    DataRow dataRowOtherChargesParamTypeFlightCarrier = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["flight carrier"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Flight Carrier is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeFlightCarrier["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeFlightCarrier["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeFlightCarrier["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeFlightCarrier["ParamName"] = "FlightCarrier";
                        dataRowOtherChargesParamTypeFlightCarrier["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["flight carrier"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeFlightCarrier["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieflightcarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeFlightCarrier["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeFlightCarrier["UpdatedBy"] = string.Empty;
                    }

                    #endregion FlightCarrier

                    #region IssueCarrier

                    DataRow dataRowOtherChargesParamTypeIssueCarrier = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["issue carrier"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Issue Carrier is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeIssueCarrier["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeIssueCarrier["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeIssueCarrier["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeIssueCarrier["ParamName"] = "IssueCarrier";
                        dataRowOtherChargesParamTypeIssueCarrier["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["issue carrier"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeIssueCarrier["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieissueingcarrier"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeIssueCarrier["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeIssueCarrier["UpdatedBy"] = string.Empty;
                    }

                    #endregion IssueCarrier

                    #region ShipperCode

                    DataRow dataRowOtherChargesParamTypeShipperCode = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "ShipperCode is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeShipperCode["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeShipperCode["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeShipperCode["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeShipperCode["ParamName"] = "ShipperCode";
                        dataRowOtherChargesParamTypeShipperCode["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeShipperCode["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieshippercode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeShipperCode["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeShipperCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion ShipperCode

                    #region AirlineCode

                    DataRow dataRowOtherChargesParamTypeAirlineCode = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["airline code"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Airline Code is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeAirlineCode["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeAirlineCode["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeAirlineCode["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeAirlineCode["ParamName"] = "AirlineCode";
                        dataRowOtherChargesParamTypeAirlineCode["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["airline code"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeAirlineCode["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieairlinecode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeAirlineCode["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeAirlineCode["UpdatedBy"] = string.Empty;
                    }

                    #endregion AirlineCode

                    #region CountryDestination

                    DataRow dataRowOtherChargesParamTypeCountryDestination = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["countrydestination"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "CountryDestination is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeCountryDestination["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeCountryDestination["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeCountryDestination["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeCountryDestination["ParamName"] = "CountryDestination";
                        dataRowOtherChargesParamTypeCountryDestination["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["countrydestination"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeCountryDestination["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["iecountrydestination"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeCountryDestination["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeCountryDestination["UpdatedBy"] = string.Empty;
                    }

                    #endregion CountryDestination

                    #region CountrySource

                    DataRow dataRowOtherChargesParamTypeCountrySource = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["countrysource"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "CountrySource is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeCountrySource["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeCountrySource["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeCountrySource["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeCountrySource["ParamName"] = "CountrySource";
                        dataRowOtherChargesParamTypeCountrySource["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["countrysource"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeCountrySource["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["iecountrysource"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeCountrySource["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeCountrySource["UpdatedBy"] = string.Empty;
                    }

                    #endregion CountrySource

                    #region POrigin

                    DataRow dataRowOtherChargesParamTypeSOURCE = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["porigin"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "POrigin is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeSOURCE["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeSOURCE["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeSOURCE["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeSOURCE["ParamName"] = "SOURCE";
                        dataRowOtherChargesParamTypeSOURCE["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["porigin"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeSOURCE["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["ieporigin"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeSOURCE["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeSOURCE["UpdatedBy"] = string.Empty;
                    }

                    #endregion POrigin

                    #region Destination

                    DataRow dataRowOtherChargesParamTypeDestination = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["pdestination"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "PDestination is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeDestination["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeDestination["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeDestination["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeDestination["ParamName"] = "Destination";
                        dataRowOtherChargesParamTypeDestination["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["pdestination"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeDestination["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["iepdestination"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeDestination["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeDestination["UpdatedBy"] = string.Empty;
                    }

                    #endregion Destination

                    #region BillingAgentCode

                    DataRow dataRowOtherChargesParamTypeBillingAgentCode = OtherChargesParamType.NewRow();
                    if (dataTableOtherChargesExcelData.Rows[i]["billingagentcode"].ToString().Trim().Trim(',').Length > 500)
                    {
                        validationDetails = validationDetails + "Billing Agent Code is more than 500 Chars;";
                    }
                    else
                    {
                        dataRowOtherChargesParamTypeBillingAgentCode["ReferenceID"] = i + 1;
                        dataRowOtherChargesParamTypeBillingAgentCode["SerialNumber"] = 0;
                        dataRowOtherChargesParamTypeBillingAgentCode["ChargeHeadSrNo"] = serialNumberOC;
                        dataRowOtherChargesParamTypeBillingAgentCode["ParamName"] = "BillingAgentCode";
                        dataRowOtherChargesParamTypeBillingAgentCode["ParamValue"] = dataTableOtherChargesExcelData.Rows[i]["billingagentcode"].ToString().Trim().Trim(',');
                        dataRowOtherChargesParamTypeBillingAgentCode["IsInclude"] = dataTableOtherChargesExcelData.Rows[i]["iebillingagentcode"].ToString().ToUpper().Trim().Trim(',').Equals("I") ? 1 : 0;
                        dataRowOtherChargesParamTypeBillingAgentCode["UpdatedOn"] = DateTime.Now;
                        dataRowOtherChargesParamTypeBillingAgentCode["UpdatedBy"] = string.Empty;

                    }

                    #endregion BillingAgentCode

                    #endregion Create rows for OtherChargesParamType Data Table

                    OtherChargeSlabsTypeTemp.Rows.Clear();
                    ULDOCSlabsTypeTemp.Rows.Clear();

                    for (int j = 62; j < dataTableOtherChargesExcelData.Columns.Count; j++)
                    {
                        DataColumn dataColumn = dataTableOtherChargesExcelData.Columns[j];

                        DataRow dataRowOtherChargeSlabsTypeTemp = OtherChargeSlabsTypeTemp.NewRow();
                        DataRow dataRowULDOCSlabsTypeTemp = ULDOCSlabsTypeTemp.NewRow();

                        if (dataColumn.ColumnName.Equals("min"))
                        {
                            #region MIN Slab

                            tempDecimalValue = 0;
                            if (!string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim()))
                            {
                                if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                {
                                    if (tempDecimalValue > 0)
                                    {
                                        dataRowOtherChargeSlabsTypeTemp["ReferenceID"] = i + 1;
                                        dataRowOtherChargeSlabsTypeTemp["SerialNumber"] = 0;
                                        dataRowOtherChargeSlabsTypeTemp["ChargeHeadSrNo"] = serialNumberOC;
                                        dataRowOtherChargeSlabsTypeTemp["SlabName"] = "M";
                                        dataRowOtherChargeSlabsTypeTemp["Weight"] = 0;
                                        dataRowOtherChargeSlabsTypeTemp["Charge"] = tempDecimalValue;
                                        dataRowOtherChargeSlabsTypeTemp["Cost"] = 0;
                                        dataRowOtherChargeSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                        dataRowOtherChargeSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                        OtherChargeSlabsTypeTemp.Rows.Add(dataRowOtherChargeSlabsTypeTemp);
                                        dataRowOtherChargeSlabsTypeTemp = OtherChargeSlabsTypeTemp.NewRow();
                                    }
                                }
                                else
                                {
                                    validationDetails = validationDetails + "Invalid MIN Value.;";
                                }
                            }

                            #endregion MIN Slab
                        }
                        else if (dataColumn.ColumnName.Equals("n"))
                        {
                            #region N Slab

                            tempDecimalValue = 0;
                            if (!string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim()))
                            {
                                if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                {
                                    if (tempDecimalValue > 0)
                                    {
                                        dataRowOtherChargeSlabsTypeTemp["ReferenceID"] = i + 1;
                                        dataRowOtherChargeSlabsTypeTemp["SerialNumber"] = 0;
                                        dataRowOtherChargeSlabsTypeTemp["ChargeHeadSrNo"] = serialNumberOC;
                                        dataRowOtherChargeSlabsTypeTemp["SlabName"] = "N";
                                        dataRowOtherChargeSlabsTypeTemp["Weight"] = 0;
                                        dataRowOtherChargeSlabsTypeTemp["Charge"] = tempDecimalValue;
                                        dataRowOtherChargeSlabsTypeTemp["Cost"] = 0;
                                        dataRowOtherChargeSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                        dataRowOtherChargeSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                        OtherChargeSlabsTypeTemp.Rows.Add(dataRowOtherChargeSlabsTypeTemp);
                                        dataRowOtherChargeSlabsTypeTemp = OtherChargeSlabsTypeTemp.NewRow();
                                    }
                                }
                                else
                                {
                                    validationDetails = validationDetails + "Invalid MIN Value.;";
                                }
                            }

                            #endregion N Slab
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(dataColumn.ColumnName.ToString()))
                            {
                                intColumnName = 0;
                                if (!dataColumn.ColumnName.StartsWith("l") && Int32.TryParse(dataColumn.ColumnName.ToString(), out intColumnName))
                                {
                                    #region Weight Slabs

                                    tempDecimalValue = 0;
                                    if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                    {
                                        if (intColumnName > 0 && tempDecimalValue > 0)
                                        {
                                            dataRowOtherChargeSlabsTypeTemp["ReferenceID"] = i + 1;
                                            dataRowOtherChargeSlabsTypeTemp["SerialNumber"] = 0;
                                            dataRowOtherChargeSlabsTypeTemp["ChargeHeadSrNo"] = serialNumberOC;
                                            dataRowOtherChargeSlabsTypeTemp["SlabName"] = "Q";
                                            dataRowOtherChargeSlabsTypeTemp["Weight"] = intColumnName;
                                            dataRowOtherChargeSlabsTypeTemp["Charge"] = tempDecimalValue;
                                            dataRowOtherChargeSlabsTypeTemp["Cost"] = 0;
                                            dataRowOtherChargeSlabsTypeTemp["UpdatedOn"] = DateTime.Now;
                                            dataRowOtherChargeSlabsTypeTemp["UpdatedBy"] = string.Empty;

                                            OtherChargeSlabsTypeTemp.Rows.Add(dataRowOtherChargeSlabsTypeTemp);
                                            dataRowOtherChargeSlabsTypeTemp = OtherChargeSlabsTypeTemp.NewRow();
                                        }
                                    }

                                    #endregion Weight Slabs
                                }
                                else
                                {
                                    #region ULD Slabs

                                    if (!string.IsNullOrWhiteSpace(dataColumn.ColumnName.ToString()) &&
                                        !string.IsNullOrWhiteSpace(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim()))
                                    {
                                        if (dataColumn.ColumnName.ToString().Length < 7)
                                        {
                                            tempDecimalValue = 0;
                                            if (decimal.TryParse(dataTableOtherChargesExcelData.Rows[i][j].ToString().Trim(), out tempDecimalValue))
                                            {
                                                if (tempDecimalValue > 0)
                                                {
                                                    dataRowULDOCSlabsTypeTemp["ReferenceID"] = i + 1;
                                                    dataRowULDOCSlabsTypeTemp["SerialNumber"] = 0;
                                                    dataRowULDOCSlabsTypeTemp["OCSrNo"] = serialNumberOC;
                                                    dataRowULDOCSlabsTypeTemp["ULDType"] = dataColumn.ColumnName.ToString();
                                                    dataRowULDOCSlabsTypeTemp["SlabName"] = "M";
                                                    dataRowULDOCSlabsTypeTemp["Weight"] = 0;
                                                    dataRowULDOCSlabsTypeTemp["Charge"] = tempDecimalValue;
                                                    dataRowULDOCSlabsTypeTemp["Cost"] = 0;
                                                    dataRowULDOCSlabsTypeTemp["UpdatedBy"] = string.Empty;
                                                    dataRowULDOCSlabsTypeTemp["UpdatedOn"] = DateTime.Now;

                                                    ULDOCSlabsTypeTemp.Rows.Add(dataRowULDOCSlabsTypeTemp);
                                                    dataRowULDOCSlabsTypeTemp = ULDOCSlabsTypeTemp.NewRow();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            validationDetails = validationDetails + "ULD Slab Column Name '" + dataColumn.ColumnName.ToString() + "' is more than 6 Chars;";
                                        }
                                    }

                                    #endregion ULD Slabs
                                }
                            }
                        }
                    }

                    if (validationDetails.Equals(string.Empty))
                    {
                        dataRowOtherChargesMasterType["validationDetails"] = string.Empty;

                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeHandler);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeEquipType);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeCommCode);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeProductType);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeFlightNum);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeAgentCode);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeHandlingCode);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeDaysOfWeek);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeFlightCarrier);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeIssueCarrier);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeShipperCode);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeAirlineCode);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeCountryDestination);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeCountrySource);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeSOURCE);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeDestination);
                        OtherChargesParamType.Rows.Add(dataRowOtherChargesParamTypeBillingAgentCode);

                        foreach (DataRow dataRowOtherChargeSlabsTypeTemp in OtherChargeSlabsTypeTemp.Rows)
                        {
                            OtherChargeSlabsType.Rows.Add(dataRowOtherChargeSlabsTypeTemp.ItemArray);
                        }

                        foreach (DataRow dataRowULDOCSlabsTypeTemp in ULDOCSlabsTypeTemp.Rows)
                        {
                            ULDOCSlabsType.Rows.Add(dataRowULDOCSlabsTypeTemp.ItemArray);
                        }
                    }
                    else
                    {
                        dataRowOtherChargesMasterType["validationDetails"] = validationDetails;
                    }

                    OtherChargesMasterType.Rows.Add(dataRowOtherChargesMasterType);
                }

                // Database Call to Validate & Insert Other Charges Master
                string errorInSp = string.Empty;
                ValidateAndInsertOtherChargesMaster(srNotblMasterUploadSummaryLog, OtherChargesMasterType,
                                                                                   OtherChargesParamType,
                                                                                   OtherChargeSlabsType,
                                                                                   ULDOCSlabsType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableOtherChargesExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting OtherCharges Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="dataTableOtherChargesMasterType"> Other Charges Master Table Type </param>
        /// <param name="dataTableOtherChargesParamsType"> Other Charges Params Table Type </param>
        /// <param name="dataTableOtherChargeSlabsType"> Other Charge Slabs Table Type </param>
        /// <param name="dataTableULDOCSlabsType"> ULD OC Slabs Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public DataSet ValidateAndInsertOtherChargesMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableOtherChargesMasterType,
                                                                                              DataTable dataTableOtherChargesParamsType,
                                                                                              DataTable dataTableOtherChargeSlabsType,
                                                                                              DataTable dataTableULDOCSlabsType,
                                                                                              string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                    new SqlParameter("@OtherChargesMasterTableType", dataTableOtherChargesMasterType),
                                                                    new SqlParameter("@OtherChargesParamsTableType", dataTableOtherChargesParamsType),
                                                                    new SqlParameter("@OtherChargeSlabsTableType", dataTableOtherChargeSlabsType),
                                                                    new SqlParameter("@ULDOCSlabsTableType", dataTableULDOCSlabsType),
                                                                    new SqlParameter("@Error", errorInSp)
                                                                  };



                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadOtherChargesMaster", sqlParameters);

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
