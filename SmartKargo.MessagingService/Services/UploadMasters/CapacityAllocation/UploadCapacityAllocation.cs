using System;
using System.Data;
using System.IO;
using Excel;
using QID.DataAccess;
using System.Data.SqlClient;

namespace QidWorkerRole.UploadMasters.CapacityAllocation
{
    /// <summary>
    /// Class to Upload Capacity Allocation File.
    /// </summary>
    public class UploadCapacityAllocation
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod Capacity Allocation.
        /// </summary>
        /// <param name="dataSetFileData"> tblMasterUploadSummaryLog entry </param>
        /// <returns> True when Success and False when Fails </returns>
        public bool CapacityAllocation(DataSet dataSetFileData)
        {
            try
            {
                UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                
                string filePath = string.Empty;

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        filePath = string.Empty;

                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "CapacityAllocation", out filePath))
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), filePath);
                        }
                        else
                        {
                            uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }

                        //uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// Method to Process Capacity Allocation Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Capacity Allocation Upload File Path  </param>
        /// <returns> True when Success and False when Failed </returns>
        public Boolean ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableCapacityAllocationExcelData = new DataTable("dataTableCapacityAllocationExcelData");
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
                dataTableCapacityAllocationExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableCapacityAllocationExcelData);

                foreach (DataColumn dataColumn in dataTableCapacityAllocationExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                #region Creating CapacityType DataTable

                DataTable CapacityType = new DataTable("CapacityType");
                CapacityType.Columns.Add("CapacityIndex", System.Type.GetType("System.Int32"));
	            CapacityType.Columns.Add("ReferenceID", System.Type.GetType("System.Int32"));
	            CapacityType.Columns.Add("Origin", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("Destination", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("FlightNumber", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("fromDate", System.Type.GetType("System.DateTime"));
	            CapacityType.Columns.Add("ToDate", System.Type.GetType("System.DateTime"));
	            CapacityType.Columns.Add("daysOfWeek", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("Capacity", System.Type.GetType("System.Decimal"));
	            CapacityType.Columns.Add("Volume", System.Type.GetType("System.Decimal"));
	            CapacityType.Columns.Add("AllotmentId", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("AgentCode", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("ProductType", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("SHCCode", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("CommodityCode", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("ReleaseBeforeDeparture", System.Type.GetType("System.Int32"));
	            CapacityType.Columns.Add("NoShowrate", System.Type.GetType("System.Decimal"));
	            CapacityType.Columns.Add("PercentOver", System.Type.GetType("System.Int32"));
	            CapacityType.Columns.Add("Carrier", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("ShipmentOrigin", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("ShipmentDest", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("ULDType", System.Type.GetType("System.String"));
	            CapacityType.Columns.Add("ULDCount", System.Type.GetType("System.Int32"));
	            CapacityType.Columns.Add("AutoConfirmUpto", System.Type.GetType("System.Int32"));
	            CapacityType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));
                CapacityType.Columns.Add("BillingAgentCode", System.Type.GetType("System.String"));
                CapacityType.Columns.Add("Currency", System.Type.GetType("System.String"));
                CapacityType.Columns.Add("MinUnutilizedPerForNoShow", System.Type.GetType("System.String"));
                CapacityType.Columns.Add("FromTime", System.Type.GetType("System.String"));
                CapacityType.Columns.Add("ToTime", System.Type.GetType("System.String"));
                CapacityType.Columns.Add("RateLineId", System.Type.GetType("System.Int32"));

                #endregion Creating CostType DataTable

                string validationDetails = string.Empty;
                DateTime tempDate;
                string tempDaysOfWeek = string.Empty;
                decimal tempDecimalValue = 0;
                int tempIntValue = 0;
                string tempString = string.Empty;

                for (int i = 0; i < dataTableCapacityAllocationExcelData.Rows.Count; i++)
                {
                    validationDetails = string.Empty;
                    tempDecimalValue = 0;

                    #region Create row for CapacityType DataTable

                    DataRow dataRowCapacityType = CapacityType.NewRow();

                    #region CapacityIndex INT NOT NULL

                    dataRowCapacityType["CapacityIndex"] = i + 1;

                    #endregion CapacityIndex

                    #region ReferenceID INT NULL

                    dataRowCapacityType["ReferenceID"] = DBNull.Value;

                    #endregion ReferenceID

                    #region Origin [VARCHAR](20) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["origin"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "Origin is required; ";
                        dataRowCapacityType["Origin"] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTableCapacityAllocationExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetails = validationDetails + "Origin is more than 20 Chars; ";
                            dataRowCapacityType["Origin"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowCapacityType["Origin"] = dataTableCapacityAllocationExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }                    

                    #endregion Origin

                    #region Destination [VARCHAR](20) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["destination"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "Destination is required; ";
                        dataRowCapacityType["Destination"] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTableCapacityAllocationExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetails = validationDetails + "Destination is more than 20 Chars; ";
                            dataRowCapacityType["Destination"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowCapacityType["Destination"] = dataTableCapacityAllocationExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion Destination

                    #region FlightNumber [VARCHAR](20) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["flightnumber"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "FlightNumber is required; ";
                        dataRowCapacityType["FlightNumber"] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTableCapacityAllocationExcelData.Rows[i]["flightnumber"].ToString().Trim().Trim(',').Length > 20)
                        {
                            validationDetails = validationDetails + "FlightNumber is more than 20 Chars; ";
                            dataRowCapacityType["FlightNumber"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowCapacityType["FlightNumber"] = dataTableCapacityAllocationExcelData.Rows[i]["flightnumber"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion FlightNumber

                    #region fromDate [DATETIME] NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["fromdate"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " FromDate is required;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowCapacityType["fromDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableCapacityAllocationExcelData.Rows[i]["fromdate"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["fromdate"].ToString().Trim(), out tempDate))
                                {
                                    dataRowCapacityType["fromDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowCapacityType["fromDate"] = DateTime.MinValue;
                                    validationDetails = validationDetails + " Invalid FromDate;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowCapacityType["fromDate"] = DateTime.MinValue;
                            validationDetails = validationDetails + "Invalid FromDate;";
                        }
                    }

                    #endregion fromDate

                    #region ToDate [DATETIME] NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["todate"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + " ToDate is required;";
                    }
                    else
                    {
                        try
                        {
                            if (isBinaryReader)
                            {
                                dataRowCapacityType["ToDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableCapacityAllocationExcelData.Rows[i]["todate"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["todate"].ToString().Trim(), out tempDate))
                                {
                                    dataRowCapacityType["ToDate"] = tempDate;
                                }
                                else
                                {
                                    dataRowCapacityType["ToDate"] = DateTime.MinValue;
                                    validationDetails = validationDetails + " Invalid ToDate;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowCapacityType["ToDate"] = DateTime.MinValue;
                            validationDetails = validationDetails + "Invalid ToDate;";
                        }
                    }

                    #endregion ToDate

                    #region daysOfWeek [VARCHAR](30) NULL

                    tempDaysOfWeek = string.Empty;
                    if (dataTableCapacityAllocationExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',').Length > 30)
                    {
                        dataRowCapacityType["daysOfWeek"] = DBNull.Value;
                        validationDetails = validationDetails + "Days Of Week is more than 30 Chars;";
                    }
                    else
                    {
                        tempDaysOfWeek = dataTableCapacityAllocationExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',');
                        if (string.IsNullOrWhiteSpace(tempDaysOfWeek))
                        {
                            dataRowCapacityType["daysOfWeek"] = "1111111";
                        }
                        else
                        {
                            if(int.TryParse(tempDaysOfWeek.Replace(",",""), out tempIntValue))
                            {
                                if (tempIntValue > 1111111)
                                {
                                    dataRowCapacityType["daysOfWeek"] = DBNull.Value;
                                    validationDetails = validationDetails + "Invalid Days Of Week; ";
                                }
                                else
                                {
                                    dataRowCapacityType["daysOfWeek"] = tempIntValue.ToString().PadLeft(7, '0');
                                }
                            }
                            else
                            {
                                dataRowCapacityType["daysOfWeek"] = DBNull.Value;
                                validationDetails = validationDetails + "Invalid Days Of Week; ";
                            }
                        }
                                                
                    }

                    #endregion daysOfWeek

                    #region Capacity [DECIMAL](18, 3) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["capacity"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["Capacity"] = tempDecimalValue;
                        validationDetails = validationDetails + " Capacity is required;";
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["capacity"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCapacityType["Capacity"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCapacityType["Capacity"] = tempDecimalValue;
                            validationDetails = validationDetails + " Invalid Capacity;";
                        }
                    }

                    #endregion Capacity

                    #region Volume [DECIMAL](18, 3) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["volume"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["Volume"] = tempDecimalValue;
                        validationDetails = validationDetails + " Volume is required;";
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["volume"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCapacityType["Volume"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCapacityType["Volume"] = tempDecimalValue;
                            validationDetails = validationDetails + " Invalid Volume;";
                        }
                    }

                    #endregion Volume

                    #region AllotmentId [VARCHAR](35) NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["allotmentid"].ToString().Trim().Trim(',')))
                    {
                        validationDetails = validationDetails + "AllotmentId is required; ";
                        dataRowCapacityType["AllotmentId"] = DBNull.Value;
                    }
                    else
                    {
                        if (dataTableCapacityAllocationExcelData.Rows[i]["allotmentid"].ToString().Trim().Trim(',').Length > 35)
                        {
                            validationDetails = validationDetails + "AllotmentId is more than 35 Chars; ";
                            dataRowCapacityType["AllotmentId"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowCapacityType["AllotmentId"] = dataTableCapacityAllocationExcelData.Rows[i]["allotmentid"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion AllotmentId

                    #region AgentCode [NVARCHAR](50) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',').Length > 50)
                    {
                        validationDetails = validationDetails + "AgentCode is more than 50 Chars; ";
                        dataRowCapacityType["AgentCode"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["AgentCode"] = dataTableCapacityAllocationExcelData.Rows[i]["agentcode"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion AgentCode

                    #region ProductType [NVARCHAR](30) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 30)
                    {
                        validationDetails = validationDetails + "ProductType is more than 30 Chars; ";
                        dataRowCapacityType["ProductType"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["ProductType"] = dataTableCapacityAllocationExcelData.Rows[i]["producttype"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion ProductType

                    #region SHCCode [NVARCHAR](100) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["shccode"].ToString().Trim().Trim(',').Length > 100)
                    {
                        validationDetails = validationDetails + "SHCCode is more than 100 Chars; ";
                        dataRowCapacityType["SHCCode"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["SHCCode"] = dataTableCapacityAllocationExcelData.Rows[i]["shccode"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion SHCCode

                    #region CommodityCode [NVARCHAR](100) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["commoditycode"].ToString().Trim().Trim(',').Length > 100)
                    {
                        validationDetails = validationDetails + "CommodityCode is more than 100 Chars; ";
                        dataRowCapacityType["CommodityCode"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["CommodityCode"] = dataTableCapacityAllocationExcelData.Rows[i]["commoditycode"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion CommodityCode

                    #region ReleaseBeforeDeparture [INT] NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["releasebeforedeparture"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["ReleaseBeforeDeparture"] = 0;
                    }
                    else
                    {
                        tempString = string.Empty;
                        tempString = dataTableCapacityAllocationExcelData.Rows[i]["releasebeforedeparture"].ToString().Trim().Trim(',');
                        switch (tempString.ToLower())
                        {
                            case "y":
                                dataRowCapacityType["ReleaseBeforeDeparture"] = 1;
                                break;
                            case "n":
                                dataRowCapacityType["ReleaseBeforeDeparture"] = 0;
                                break;
                            default:
                                tempIntValue = 0;
                                if(int.TryParse(tempString, out tempIntValue))
                                {
                                    dataRowCapacityType["ReleaseBeforeDeparture"] = tempIntValue;
                                }
                                else
                                {
                                    validationDetails = validationDetails + " Invalid ReleaseBeforeDeparture;";
                                }
                                break;
                        }
                    }

                    #endregion ReleaseBeforeDeparture

                    #region NoShowrate [DECIMAL](18, 3) NULL

                    tempDecimalValue = 0;
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["noshowrate"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["NoShowrate"] = tempDecimalValue;
                    }
                    else
                    {
                        if (decimal.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["noshowrate"].ToString().Trim().Trim(','), out tempDecimalValue))
                        {
                            dataRowCapacityType["NoShowrate"] = tempDecimalValue;
                        }
                        else
                        {
                            dataRowCapacityType["NoShowrate"] = tempDecimalValue;
                            validationDetails = validationDetails + " Invalid NoShowrate;";
                        }
                    }

                    #endregion NoShowrate

                    #region PercentOver [INT] NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["percentover"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["PercentOver"] = 0;
                    }
                    else
                    {
                        tempIntValue = 0;
                        if(int.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["percentover"].ToString().Trim().Trim(','), out tempIntValue))
                        {
                            dataRowCapacityType["PercentOver"] = tempIntValue;
                        }
                        else
                        {
                            dataRowCapacityType["PercentOver"] = tempIntValue;
                            validationDetails = validationDetails + " Invalid PercentOver;";
                        }
                    }

                    #endregion PercentOver

                    #region Carrier [NVARCHAR](10) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["carrier"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetails = validationDetails + "Carrier is more than 10 Chars; ";
                        dataRowCapacityType["Carrier"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["Carrier"] = dataTableCapacityAllocationExcelData.Rows[i]["carrier"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion Carrier

                    #region ShipmentOrigin [NVARCHAR](10) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["shipmentorigin"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetails = validationDetails + "ShipmentOrigin is more than 10 Chars; ";
                        dataRowCapacityType["ShipmentOrigin"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["ShipmentOrigin"] = dataTableCapacityAllocationExcelData.Rows[i]["shipmentorigin"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion ShipmentOrigin

                    #region ShipmentDest [NVARCHAR](10) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["shipmentdest"].ToString().Trim().Trim(',').Length > 10)
                    {
                        validationDetails = validationDetails + "ShipmentDest is more than 10 Chars; ";
                        dataRowCapacityType["ShipmentDest"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["ShipmentDest"] = dataTableCapacityAllocationExcelData.Rows[i]["shipmentdest"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion ShipmentDest

                    #region ULDType [NVARCHAR](100) NULL

                    if (dataTableCapacityAllocationExcelData.Rows[i]["uldtype"].ToString().Trim().Trim(',').Length > 100)
                    {
                        validationDetails = validationDetails + "ULDType is more than 100 Chars; ";
                        dataRowCapacityType["ULDType"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["ULDType"] = dataTableCapacityAllocationExcelData.Rows[i]["uldtype"].ToString().Trim().ToUpper().Trim(',');
                    }

                    #endregion ULDType

                    #region ULDCount [INT] NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["uldcount"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["ULDCount"] = 0;
                    }
                    else
                    {
                        tempIntValue = 0;
                        if (int.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["uldcount"].ToString().Trim().Trim(','), out tempIntValue))
                        {
                            dataRowCapacityType["ULDCount"] = tempIntValue;
                        }
                        else
                        {
                            dataRowCapacityType["ULDCount"] = tempIntValue;
                            validationDetails = validationDetails + " Invalid ULDCount;";
                        }
                    }

                    #endregion ULDCount

                    #region AutoConfirmUpto [INT] NULL

                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["autoconfirmupto"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["AutoConfirmUpto"] = 0;
                    }
                    else
                    {
                        tempIntValue = 0;
                        if (int.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["autoconfirmupto"].ToString().Trim().Trim(','), out tempIntValue))
                        {
                            dataRowCapacityType["AutoConfirmUpto"] = tempIntValue;
                        }
                        else
                        {
                            dataRowCapacityType["AutoConfirmUpto"] = tempIntValue;
                            validationDetails = validationDetails + " Invalid AutoConfirmUpto;";
                        }
                    }

                    #endregion AutoConfirmUpto

                    #region BillingAgentCode [VARCHAR(35)] NULL
                    if (dataTableCapacityAllocationExcelData.Rows[i]["billingagentcode"].ToString().Trim().Trim(',').Length > 50)
                    {
                        validationDetails = validationDetails + "BillingAgentCode is more than 50 Chars; ";
                        dataRowCapacityType["BillingAgentCode"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["BillingAgentCode"] = dataTableCapacityAllocationExcelData.Rows[i]["billingagentcode"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion BillingAgentCode

                    #region Currency [VARCHAR(3)] NULL
                    if (dataTableCapacityAllocationExcelData.Rows[i]["currency"].ToString().Trim().Trim(',').Length > 3)
                    {
                        validationDetails = validationDetails + "Currency code is more than 3 Chars; ";
                        dataRowCapacityType["Currency"] = DBNull.Value;
                    }
                    else
                    {
                        dataRowCapacityType["Currency"] = dataTableCapacityAllocationExcelData.Rows[i]["currency"].ToString().Trim().ToUpper().Trim(',');
                    }
                    #endregion Currency

                    #region MinUnutilizedPerForNoShow [VARCHAR(35)] NULL
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["minunutilizedperfornoshow"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["MinUnutilizedPerForNoShow"] = 0;
                    }
                    else
                    {
                        tempIntValue = 0;
                        if (int.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["minunutilizedperfornoshow"].ToString().Trim().Trim(','), out tempIntValue))
                        {
                            dataRowCapacityType["MinUnutilizedPerForNoShow"] = tempIntValue;
                        }
                        else
                        {
                            dataRowCapacityType["MinUnutilizedPerForNoShow"] = tempIntValue;
                            validationDetails = validationDetails + " Invalid MinUnutilizedPerForNoShow;";
                        }
                    }
                    #endregion MinUnutilizedPerForNoShow

                    #region FromTime [VARCHAR(10)] NULL
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["fromtime"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["FromTime"] = DBNull.Value;
                    }
                    else if(dataTableCapacityAllocationExcelData.Rows[i]["fromtime"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetails = validationDetails + "FromTime is in incorrect format; ";
                        dataRowCapacityType["FromTime"] = DBNull.Value;
                    }
                    else
                    {
                        string FromTimeHours = dataTableCapacityAllocationExcelData.Rows[i]["fromtime"].ToString().Split(':')[0];
                        string FromTimeMinutes = dataTableCapacityAllocationExcelData.Rows[i]["fromtime"].ToString().Split(':')[1];
                        int FromTimeHR = 0;
                        int FromTimeMN = 0;
                        if (int.TryParse(FromTimeHours, out FromTimeHR))
                        {
                            if (FromTimeHR < 0 && FromTimeHR > 23)
                            {
                                validationDetails = validationDetails + " Invalid FromTime;";
                                dataRowCapacityType["FromTime"] = DBNull.Value;
                            }
                            
                        }
                        if (int.TryParse(FromTimeMinutes, out FromTimeMN))
                        {
                            if (FromTimeMN < 0 && FromTimeMN > 59)
                            {
                                validationDetails = validationDetails + " Invalid FromTime;";
                                dataRowCapacityType["FromTime"] = DBNull.Value;
                            }

                        }
                        if (FromTimeHR >= 0 && FromTimeMN >= 0)
                        {
                            dataRowCapacityType["FromTime"] = FromTimeHours + ":" + FromTimeMinutes;
                        }
                        else
                        {
                            validationDetails = validationDetails + " Invalid FromTime;";
                            dataRowCapacityType["FromTime"] = DBNull.Value;
                        }
                        
                    }
                    #endregion FromTime

                    #region ToTime [VARCHAR(10)] NULL
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["totime"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["ToTime"] = DBNull.Value;
                    }
                    else if (dataTableCapacityAllocationExcelData.Rows[i]["totime"].ToString().Trim().Trim(',').Length > 5)
                    {
                        validationDetails = validationDetails + "ToTime is in incorrect format; ";
                        dataRowCapacityType["ToTime"] = DBNull.Value;
                    }
                    else
                    {
                        string ToTimeHours = dataTableCapacityAllocationExcelData.Rows[i]["totime"].ToString().Split(':')[0];
                        string ToTimeMinutes = dataTableCapacityAllocationExcelData.Rows[i]["totime"].ToString().Split(':')[1];
                        int ToTimeHR = 0;
                        int ToTimeMN = 0;
                        if (int.TryParse(ToTimeHours, out ToTimeHR))
                        {
                            if (ToTimeHR < 0 && ToTimeHR > 23)
                            {
                                validationDetails = validationDetails + " Invalid ToTime;";
                                dataRowCapacityType["ToTime"] = DBNull.Value;
                            }

                        }
                        if (int.TryParse(ToTimeMinutes, out ToTimeMN))
                        {
                            if (ToTimeMN < 0 && ToTimeMN > 59)
                            {
                                validationDetails = validationDetails + " Invalid ToTime;";
                                dataRowCapacityType["ToTime"] = DBNull.Value;
                            }

                        }
                        if (ToTimeHR >= 0 && ToTimeMN >= 0)
                        {
                            dataRowCapacityType["ToTime"] = ToTimeHours + ":" + ToTimeMinutes;
                        }
                        else
                        {
                            validationDetails = validationDetails + " Invalid ToTime;";
                            dataRowCapacityType["ToTime"] = DBNull.Value;
                        }

                    }
                    #endregion ToTime

                    #region RateLineId [INT] NULL
                    if (string.IsNullOrWhiteSpace(dataTableCapacityAllocationExcelData.Rows[i]["ratelineid"].ToString().Trim().Trim(',')))
                    {
                        dataRowCapacityType["RateLineId"] = 0;
                    }
                    else
                    {
                        tempIntValue = 0;
                        if (int.TryParse(dataTableCapacityAllocationExcelData.Rows[i]["ratelineid"].ToString().Trim().Trim(','), out tempIntValue))
                        {
                            dataRowCapacityType["RateLineId"] = tempIntValue;
                        }
                        else
                        {
                            dataRowCapacityType["RateLineId"] = tempIntValue;
                            validationDetails = validationDetails + " Invalid RateLineId;";
                        }
                    }
                    #endregion RateLineId

                    #region ValidationDetails VARCHAR(MAX) NULL

                    dataRowCapacityType["ValidationDetails"] = validationDetails;

                    #endregion ValidationDetails

                    #endregion Create row for CapacityType DataTable

                    CapacityType.Rows.Add(dataRowCapacityType);
                }

                // Database Call to Validate & Insert Capacity Allocation
                ValidateAndInsertCapacityAllocation(srNotblMasterUploadSummaryLog, CapacityType);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableCapacityAllocationExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Capacity Allocation.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> tblMasterUploadSummaryLog Primay Key </param>
        /// <param name="dataTableCapacityAllocation"> CapacityType DataTable  </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        private DataSet ValidateAndInsertCapacityAllocation(int srNotblMasterUploadSummaryLog, DataTable dataTableCapacityAllocation)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParams = new SqlParameter[] { new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                new SqlParameter("@CapacityTableType", dataTableCapacityAllocation)
                                                              };

                dataSetResult = sqlServer.SelectRecords("Masters.uspUploadCapacityMaster", sqlParams);
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
            }
            return dataSetResult;
        }
    }
}
