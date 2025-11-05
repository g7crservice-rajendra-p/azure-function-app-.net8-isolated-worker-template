using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using QID.DataAccess;
using System.Threading;
using System.IO;
using Excel;
using System.Data.SqlClient;
using System.Globalization;

namespace QidWorkerRole.UploadMasters.RouteControl
{
    /// <summary>
    /// Class to Upload RouteControls Master File.
    /// </summary>
    public class UploadRouteControl
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        /// <summary>
        /// Method to Uplaod Route Controls Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public Boolean RouteControlsMasterUpload(DataSet dataSetFileData)
        {
            try
            {
                //DataSet dataSetFileData = new DataSet();
                //dataSetFileData = uploadMasterCommon.GetUploadedFileData(UploadMasterType.RouteControls);

                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              "RouteControlsMasterUploadFile", out uploadFilePath))
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
        /// Method to Process Route Controls Master Upload File.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Table Primary Key </param>
        /// <param name="filepath"> Route Controls Upload File Path </param>
        /// <returns> True when Success and False when Failed </returns>
        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableRouteControlsExcelData = new DataTable("dataTableRouteControlsExcelData");

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
                dataTableRouteControlsExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableRouteControlsExcelData);

                foreach (DataColumn dataColumn in dataTableRouteControlsExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableRouteControlsExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating RouteControlsMasterType DataTable

                DataTable dataTableRouteControlsMasterType = new DataTable("dataTableRouteControlsMasterType");
                dataTableRouteControlsMasterType.Columns.Add("RouteControlsIndex", System.Type.GetType("System.Int32"));
	            dataTableRouteControlsMasterType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
	            dataTableRouteControlsMasterType.Columns.Add("RouteID", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("RouteName", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("StartDate", System.Type.GetType("System.DateTime"));
	            dataTableRouteControlsMasterType.Columns.Add("EndDate", System.Type.GetType("System.DateTime"));
	            dataTableRouteControlsMasterType.Columns.Add("LocationLevel", System.Type.GetType("System.Int32"));
	            dataTableRouteControlsMasterType.Columns.Add("Location", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("OriginLevel", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("Origin", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("DestinationLevel", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("Destination", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("Type", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("Status", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
	            dataTableRouteControlsMasterType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("Remarks", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("MINIMUM_CAPACITY", System.Type.GetType("System.Decimal"));
                dataTableRouteControlsMasterType.Columns.Add("MINIMUM_VOLUME", System.Type.GetType("System.Decimal"));
	            dataTableRouteControlsMasterType.Columns.Add("ControlType", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("Yield", System.Type.GetType("System.String"));
	            dataTableRouteControlsMasterType.Columns.Add("ValidationErrorDetailsRouteControls", System.Type.GetType("System.String"));

                #endregion

                #region Creating RouteConfigParamsType DataTable

                DataTable dataTableRouteConfigParamsType = new DataTable("dataTableRouteConfigParamsType");
                dataTableRouteConfigParamsType.Columns.Add("RouteControlsIndex", System.Type.GetType("System.Int32"));
	            dataTableRouteConfigParamsType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
	            dataTableRouteConfigParamsType.Columns.Add("RouteSrNo", System.Type.GetType("System.Int32"));
	            dataTableRouteConfigParamsType.Columns.Add("ParamName", System.Type.GetType("System.String"));
	            dataTableRouteConfigParamsType.Columns.Add("ParamValue", System.Type.GetType("System.String"));
	            dataTableRouteConfigParamsType.Columns.Add("IsInclude", System.Type.GetType("System.Byte"));
                dataTableRouteConfigParamsType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
	            dataTableRouteConfigParamsType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));

                #endregion

                string validationErrorDetailsRouteControls = string.Empty;
                DateTime tempDate;
                decimal tempDecimalValue = 0;

                for (int i = 0; i < dataTableRouteControlsExcelData.Rows.Count; i++)
                {
                    validationErrorDetailsRouteControls = string.Empty;
                    tempDecimalValue = 0;

                    #region Create row for RouteControlsMasterType Data Table

                    DataRow dataRowRouteControlsMasterType = dataTableRouteControlsMasterType.NewRow();

                    #region RouteControlsIndex INT NULL

                    dataRowRouteControlsMasterType["RouteControlsIndex"] = i + 1;

                    #endregion RouteControlsIndex

                    #region SerialNumber INT NULL

                    dataRowRouteControlsMasterType["SerialNumber"] = DBNull.Value;

                    #endregion SerialNumber

                    #region RouteID VARCHAR(30) NULL

                    if (columnNames.Contains("routeid"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["routeid"].ToString().Trim().Trim(',').Length > 30)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "RouteID is more than 30 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["RouteID"] = dataTableRouteControlsExcelData.Rows[i]["routeid"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }

                    #endregion RouteID

                    #region RouteName VARCHAR(200) NULL

                    if (columnNames.Contains("routename"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["routename"].ToString().Trim().Trim(',').Length > 200)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "RouteName is more than 200 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["RouteName"] = dataTableRouteControlsExcelData.Rows[i]["routename"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion RouteName

                    #region StartDate DATETIME NULL

                    if (columnNames.Contains("startdate"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["startdate"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "StartDate not found;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowRouteControlsMasterType["StartDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableRouteControlsExcelData.Rows[i]["startdate"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableRouteControlsExcelData.Rows[i]["startdate"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowRouteControlsMasterType["StartDate"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowRouteControlsMasterType["StartDate"] = DateTime.Now;
                                        validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Invalid StartDate;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowRouteControlsMasterType["StartDate"] = DateTime.Now;
                                validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Invalid StartDate;";
                            }
                        }
                    }

                    #endregion StartDate

                    #region EndDate DATETIME NULL

                    if (columnNames.Contains("enddate"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["enddate"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "EndDate not found;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowRouteControlsMasterType["EndDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableRouteControlsExcelData.Rows[i]["enddate"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTableRouteControlsExcelData.Rows[i]["enddate"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowRouteControlsMasterType["EndDate"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowRouteControlsMasterType["EndDate"] = DateTime.Now;
                                        validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Invalid EndDate;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowRouteControlsMasterType["EndDate"] = DateTime.Now;
                                validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Invalid EndDate;";
                            }
                        }
                    }

                    #endregion EndDate

                    #region LocationLevel INT NULL

                    dataRowRouteControlsMasterType["LocationLevel"] = DBNull.Value;

                    #endregion LocationLevel

                    #region Location VARCHAR(5) NULL

                    dataRowRouteControlsMasterType["Location"] = DBNull.Value;

                    #endregion Location

                    #region OriginLevel VARCHAR(5) NULL

                    dataRowRouteControlsMasterType["OriginLevel"] = DBNull.Value;

                    #endregion OriginLevel

                    #region Origin VARCHAR(50) NULL

                    if (columnNames.Contains("origin"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Origin not found;";
                        }
                        else if (dataTableRouteControlsExcelData.Rows[i]["origin"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Origin is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["Origin"] = dataTableRouteControlsExcelData.Rows[i]["origin"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }
                    else
                    {
                        validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Origin column not found;";
                    }

                    #endregion Origin

                    #region DestinationLevel VARCHAR(5) NULL

                    dataRowRouteControlsMasterType["DestinationLevel"] = DBNull.Value;

                    #endregion DestinationLevel

                    #region Destination VARCHAR(50) NULL

                    if (columnNames.Contains("destination"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Destination not found;";
                        }
                        else if (dataTableRouteControlsExcelData.Rows[i]["destination"].ToString().Trim().Trim(',').Length > 50)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Destination is more than 50 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["Destination"] = dataTableRouteControlsExcelData.Rows[i]["destination"].ToString().Trim().ToUpper().Trim(',');
                        }
                    }
                    else
                    {
                        validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Destination column not found;";
                    }

                    #endregion Destination

                    #region Type VARCHAR(5) NULL

                    dataRowRouteControlsMasterType["Type"] = DBNull.Value;

                    #endregion Type

                    #region Status VARCHAR(5) NULL

                    if (columnNames.Contains("status"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["status"].ToString().Trim().Trim(',').Length > 5)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Status is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["Status"] = dataTableRouteControlsExcelData.Rows[i]["status"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion Status

                    #region UpdatedOn DATETIME NULL

                    dataRowRouteControlsMasterType["UpdatedOn"] = DateTime.Now;

                    #endregion UpdatedOn

                    #region UpdatedBy VARCHAR(100) NULL

                    dataRowRouteControlsMasterType["UpdatedBy"] = DBNull.Value;

                    #endregion UpdatedBy

                    #region Remarks VARCHAR(500) NULL

                    if (columnNames.Contains("remarks"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',').Length > 500)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Remarks is more than 500 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["Remarks"] = dataTableRouteControlsExcelData.Rows[i]["remarks"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion Remarks

                    #region MINIMUM_CAPACITY DECIMAL(18, 2) NULL

                    if (columnNames.Contains("min_capacity"))
                    {
                        tempDecimalValue = 0;
                        if (dataTableRouteControlsExcelData.Rows[i]["min_capacity"] == null)
                        {
                            dataRowRouteControlsMasterType["MINIMUM_CAPACITY"] = DBNull.Value;
                        }
                        else if (dataTableRouteControlsExcelData.Rows[i]["min_capacity"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowRouteControlsMasterType["MINIMUM_CAPACITY"] = tempDecimalValue;
                        }
                        else
                        {
                            if (decimal.TryParse(dataTableRouteControlsExcelData.Rows[i]["min_capacity"].ToString().Trim().Trim(','), out tempDecimalValue))
                            {
                                dataRowRouteControlsMasterType["MINIMUM_CAPACITY"] = tempDecimalValue;
                            }
                            else
                            {
                                dataRowRouteControlsMasterType["MINIMUM_CAPACITY"] = DBNull.Value;
                                validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Invalid MINIMUM_CAPACITY;";
                            }
                        }
                    }                    

                    #endregion MINIMUM_CAPACITY

                    #region MINIMUM_VOLUME DECIMAL(18, 2) NULL

                    if (columnNames.Contains("min_volume"))
                    {
                        tempDecimalValue = 0;
                        if (dataTableRouteControlsExcelData.Rows[i]["min_volume"] == null)
                        {
                            dataRowRouteControlsMasterType["MINIMUM_VOLUME"] = DBNull.Value;
                        }
                        else if (dataTableRouteControlsExcelData.Rows[i]["min_volume"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowRouteControlsMasterType["MINIMUM_VOLUME"] = tempDecimalValue;
                        }
                        else
                        {
                            if (decimal.TryParse(dataTableRouteControlsExcelData.Rows[i]["min_volume"].ToString().Trim().Trim(','), out tempDecimalValue))
                            {
                                dataRowRouteControlsMasterType["MINIMUM_VOLUME"] = tempDecimalValue;
                            }
                            else
                            {
                                dataRowRouteControlsMasterType["MINIMUM_VOLUME"] = DBNull.Value;
                                validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Invalid MINIMUM_VOLUME;";
                            }
                        }
                    } 

                    #endregion MINIMUM_VOLUME

                    #region ControlType VARCHAR(40) NULL

                    if (columnNames.Contains("controltype"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["controltype"].ToString().Trim().Trim(',').Length > 40)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "ControlType is more than 40 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["ControlType"] = dataTableRouteControlsExcelData.Rows[i]["controltype"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion ControlType

                    #region Yield VARCHAR(40) NULL

                    if (columnNames.Contains("yield"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["yield"].ToString().Trim().Trim(',').Length > 40)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Yield is more than 40 Chars;";
                        }
                        else
                        {
                            dataRowRouteControlsMasterType["Yield"] = dataTableRouteControlsExcelData.Rows[i]["yield"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion Yield

                    #endregion Create row for RouteControlsMasterType Data Table

                    #region Create rows for RouteConfigParamsType Data Table

                    #region IssueCarrier

                    DataRow RouteConfigParamsTypeIssueCarrier = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("issuecarrier"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "IssueCarrier is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeIssueCarrier["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeIssueCarrier["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeIssueCarrier["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeIssueCarrier["ParamName"] = "IssueCarrier";
                            RouteConfigParamsTypeIssueCarrier["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["issuecarrier"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeIssueCarrier["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieissuecarrier"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeIssueCarrier["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeIssueCarrier["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion IssueCarrier

                    #region FlightCarrier

                    DataRow RouteConfigParamsTypeFlightCarrier = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("flightcarrier"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "FlightCarrier is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeFlightCarrier["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeFlightCarrier["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeFlightCarrier["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeFlightCarrier["ParamName"] = "FlightCarrier";
                            RouteConfigParamsTypeFlightCarrier["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["flightcarrier"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeFlightCarrier["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieflightcarrier"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeFlightCarrier["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeFlightCarrier["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion FlightCarrier

                    #region Origin

                    DataRow RouteConfigParamsTypeOrigin = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("origindetail"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["origindetail"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "OriginDetail is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeOrigin["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeOrigin["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeOrigin["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeOrigin["ParamName"] = "Origin";
                            RouteConfigParamsTypeOrigin["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["origindetail"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeOrigin["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieorigin"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeOrigin["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeOrigin["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion Origin

                    #region Destination

                    DataRow RouteConfigParamsTypeDestination = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("destinationdetail"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["destinationdetail"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "DestinationDetail is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeDestination["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeDestination["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeDestination["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeDestination["ParamName"] = "Destination";
                            RouteConfigParamsTypeDestination["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["destinationdetail"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeDestination["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["iedestination"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeDestination["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeDestination["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion Destination

                    #region FlightNum

                    DataRow RouteConfigParamsTypeFlightNum = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("flightnum"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["flightnum"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "FlightNum is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeFlightNum["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeFlightNum["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeFlightNum["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeFlightNum["ParamName"] = "FlightNum";
                            RouteConfigParamsTypeFlightNum["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["flightnum"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeFlightNum["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieflightnum"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeFlightNum["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeFlightNum["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion FlightNum

                    #region ShipperCode

                    DataRow RouteConfigParamsTypeShipperCode = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("shippercode"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "ShipperCode is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeShipperCode["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeShipperCode["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeShipperCode["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeShipperCode["ParamName"] = "ShipperCode";
                            RouteConfigParamsTypeShipperCode["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["shippercode"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeShipperCode["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieshippercode"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeShipperCode["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeShipperCode["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion ShipperCode

                    #region AgentCode

                    DataRow RouteConfigParamsTypeAgentCode = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("agentcode"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "AgentCode is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeAgentCode["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeAgentCode["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeAgentCode["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeAgentCode["ParamName"] = "AgentCode";
                            RouteConfigParamsTypeAgentCode["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["agentcode"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeAgentCode["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieagentcode"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeAgentCode["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeAgentCode["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion AgentCode

                    #region Priority

                    DataRow RouteConfigParamsTypePriority = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("priority"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["priority"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "Priority is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypePriority["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypePriority["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypePriority["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypePriority["ParamName"] = "Priority";
                            RouteConfigParamsTypePriority["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["priority"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypePriority["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["iepriority"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypePriority["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypePriority["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion Priority

                    #region ProductType

                    DataRow RouteConfigParamsTypeProductType = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("producttype"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "ProductType is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeProductType["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeProductType["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeProductType["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeProductType["ParamName"] = "ProductType";
                            RouteConfigParamsTypeProductType["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeProductType["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieproducttype"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeProductType["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeProductType["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion ProductType

                    #region CommCode

                    DataRow RouteConfigParamsTypeCommCode = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("commcode"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["commcode"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "CommCode is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeCommCode["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeCommCode["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeCommCode["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeCommCode["ParamName"] = "CommCode";
                            RouteConfigParamsTypeCommCode["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["commcode"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeCommCode["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["iecommcode"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeCommCode["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeCommCode["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion CommCode

                    #region SHC

                    DataRow RouteConfigParamsTypeSHC = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("shc"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["shc"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "SHC is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeSHC["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeSHC["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeSHC["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeSHC["ParamName"] = "SHC";
                            RouteConfigParamsTypeSHC["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["shc"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeSHC["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieshc"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeSHC["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeSHC["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion SHC

                    #region ULD Category

                    DataRow RouteConfigParamsTypeULDCategory = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("uld category"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["uld category"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "ULD Category is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeULDCategory["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeULDCategory["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeULDCategory["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeULDCategory["ParamName"] = "ULD Category";
                            RouteConfigParamsTypeULDCategory["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["uld category"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeULDCategory["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieuldcategory"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeULDCategory["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeULDCategory["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion ULD Category

                    #region DaysOfWeek

                    DataRow RouteConfigParamsTypeDaysOfWeek = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("daysofweek"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "DaysOfWeek is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeDaysOfWeek["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeDaysOfWeek["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeDaysOfWeek["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeDaysOfWeek["ParamName"] = "DaysOfWeek";
                            RouteConfigParamsTypeDaysOfWeek["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["daysofweek"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeDaysOfWeek["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["iedaysofweek"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeDaysOfWeek["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeDaysOfWeek["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion DaysOfWeek

                    #region PaymentType

                    DataRow RouteConfigParamsTypePaymentType = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("paymenttype"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["paymenttype"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "PaymentType is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypePaymentType["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypePaymentType["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypePaymentType["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypePaymentType["ParamName"] = "PaymentType";
                            RouteConfigParamsTypePaymentType["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["paymenttype"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypePaymentType["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["iepaymenttype"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypePaymentType["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypePaymentType["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion PaymentType

                    #region EquipmentType

                    DataRow RouteConfigParamsTypeEquipmentType = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("equipmenttype"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "EquipmentType is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeEquipmentType["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeEquipmentType["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeEquipmentType["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeEquipmentType["ParamName"] = "EquipmentType";
                            RouteConfigParamsTypeEquipmentType["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["equipmenttype"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeEquipmentType["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ieequipmenttype"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeEquipmentType["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeEquipmentType["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion EquipmentType

                    #region TransitStation

                    DataRow RouteConfigParamsTypeTransitStation = dataTableRouteConfigParamsType.NewRow();

                    if (columnNames.Contains("transitstation"))
                    {
                        if (dataTableRouteControlsExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',').Length > 4000)
                        {
                            validationErrorDetailsRouteControls = validationErrorDetailsRouteControls + "TransitStation is more than 4000 Chars;";
                        }
                        else
                        {
                            RouteConfigParamsTypeTransitStation["RouteControlsIndex"] = i + 1;
                            RouteConfigParamsTypeTransitStation["SerialNumber"] = DBNull.Value;
                            RouteConfigParamsTypeTransitStation["RouteSrNo"] = DBNull.Value;
                            RouteConfigParamsTypeTransitStation["ParamName"] = "TransitStation";
                            RouteConfigParamsTypeTransitStation["ParamValue"] = dataTableRouteControlsExcelData.Rows[i]["transitstation"].ToString().Trim().Trim(',');
                            RouteConfigParamsTypeTransitStation["IsInclude"] = dataTableRouteControlsExcelData.Rows[i]["ietransitstation"].ToString().Trim().Trim(',').Equals("1") ? 1 : 0;
                            RouteConfigParamsTypeTransitStation["UpdatedOn"] = DateTime.Now;
                            RouteConfigParamsTypeTransitStation["UpdatedBy"] = string.Empty;
                        }
                    }

                    #endregion TransitStation

                    #endregion Create rows for RouteConfigParamsType Data Table

                    if (validationErrorDetailsRouteControls.Equals(string.Empty))
                    {
                        dataRowRouteControlsMasterType["ValidationErrorDetailsRouteControls"] = string.Empty;

                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeIssueCarrier);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeFlightCarrier);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeOrigin);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeDestination);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeFlightNum);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeShipperCode);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeAgentCode);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypePriority);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeProductType);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeCommCode);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeSHC);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeULDCategory);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeDaysOfWeek);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypePaymentType);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeEquipmentType);
                        dataTableRouteConfigParamsType.Rows.Add(RouteConfigParamsTypeTransitStation);
                    }
                    else
                    {
                        dataRowRouteControlsMasterType["ValidationErrorDetailsRouteControls"] = validationErrorDetailsRouteControls;
                    }

                    dataTableRouteControlsMasterType.Rows.Add(dataRowRouteControlsMasterType);

                }

                // Database Call to Validate & Insert Route Controls Master
                string errorInSp = string.Empty;
                ValidateAndInsertRouteControlsMaster(srNotblMasterUploadSummaryLog, dataTableRouteControlsMasterType, dataTableRouteConfigParamsType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableRouteControlsExcelData = null;
            }
        }

        /// <summary>
        /// Method to Call Stored Procedure for Validating & Inserting Route Controls Master.
        /// </summary>
        /// <param name="srNotblMasterUploadSummaryLog"> Master Summary Log Primary Key </param>
        /// <param name="dataTableRateLineType"> Route Controls Master Table Type </param>
        /// <param name="dataTableRateLineParamType"> Route Config Params Table Type </param>
        /// <param name="errorInSp"> Error Message from Stored Procedure </param>
        /// <returns> Selected Data Set from Stored Procedure </returns>
        public DataSet ValidateAndInsertRouteControlsMaster(int srNotblMasterUploadSummaryLog, DataTable dataTableRouteControlsMasterType,
                                                                                               DataTable dataTableRouteConfigParamsType, string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {   new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@RouteControlsMasterType", dataTableRouteControlsMasterType),
                                                                      new SqlParameter("@RouteConfigParamsType", dataTableRouteConfigParamsType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadRouteControlsMaster", sqlParameters);

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
