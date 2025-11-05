using Excel;
using Newtonsoft.Json;
using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QidWorkerRole.UploadMasters.Airports
{
    public class UploadAirportsMaster
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        public Boolean UpdateAirports(DataSet dsFiles)
        {
            try
            {
                //DataSet dsFiles = new DataSet();
                //dsFiles = uploadMasterCommon.GetUploadedFileData(UploadMasterType.Airports);
                string FilePath = "";
                
                foreach (DataRow dr in dsFiles.Tables[0].Rows)
                {
                    // to upadate retry count only.
                    uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                    if (uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "Airports", out FilePath))
                    {
                        ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath);
                    }
                    else
                    {
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                        uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dr["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        continue;
                    }

                    uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                    uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Message: " + ex.Message + " \nStackTrace: " + ex.StackTrace);
            }
            return false;
        }

        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTableAirpotsExcelData = new DataTable("dataTableAirpotsExcelData");

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
                dataTableAirpotsExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTableAirpotsExcelData);

                foreach (DataColumn dataColumn in dataTableAirpotsExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }

                string[] columnNames = dataTableAirpotsExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating AirportMasterType DataTable

                DataTable AirportMasterType = new DataTable("AirportMasterType");
                AirportMasterType.Columns.Add("AirportMasterIndex", System.Type.GetType("System.Int32"));
                AirportMasterType.Columns.Add("SerialNumber", System.Type.GetType("System.Int32"));
                AirportMasterType.Columns.Add("AirportCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("AirportName", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CityCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("RegionCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CountryCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CityGroup", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("AirportGroup", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("RegionGroup", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CountryGroup", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("IsTaxExempted", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("StationMailId", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ManagerName", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ManagerEmailId", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ShiftMobNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("LandlineNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ManagerMobNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("counter", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("SpotRateApprovalAuathorityMailId", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GHAName", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GHAAddress", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GHAPhoneNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GHAMobileNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GHAFAXNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GHAEmailID", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GSAName", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GSAAddress", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GSAPhoneNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GSAMobileNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GSAFAXNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GSAEmailID", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("APMName", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("APMAddress", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("APMPhoneNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("APMMobileNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("APMFAXNo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("APMEmailID", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("AdditionalInfo", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("TransitTime", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CutOffTime", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("BookingCurrrency", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("InvoiceCurrrency", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("InvoiceType", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("BookingType", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("isRFIDEnable", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("CityType", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("GLAccountCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("TimeZone", System.Type.GetType("System.DateTime"));
                AirportMasterType.Columns.Add("TimeZones", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("UTCTIMEDIFF", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("IsULDEnabled", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("Latitude", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("Longitude", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CustomAirportCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("CustomOfficeCode", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("DATEFORMAT", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("Aging1", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("Aging2", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("UOM", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ShowInMobileApp", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("MobilityCutOffTime", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("AirportAddress", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ProductType", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("IsOperational", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("AirPortType", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("DimsUOM", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("Freezer", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("Cooler", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("InternationalFrieght", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("CloakRoom", System.Type.GetType("System.Byte"));
                AirportMasterType.Columns.Add("LocationID", System.Type.GetType("System.String"));
                AirportMasterType.Columns.Add("ValidationDetailsAirportMaster", System.Type.GetType("System.String"));

                #endregion Creating AirportMasterType DataTable

                string validationDetailsAirport = string.Empty;

                for (int i = 0; i < dataTableAirpotsExcelData.Rows.Count; i++)
                {
                    validationDetailsAirport = string.Empty;

                    #region Create row for AirportMasterType Data Table

                    DataRow dataRowAirportMasterType = AirportMasterType.NewRow();

                    dataRowAirportMasterType["AirportMasterIndex"] = i + 1;
                    dataRowAirportMasterType["SerialNumber"] = 0;

                    #region [AirportCode] [varchar] (10)NULL

                    if (columnNames.Contains("airport code*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["airport code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["AirportCode"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + " Airport Code is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["airport code*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["AirportCode"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + " Airport Code is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["AirportCode"] = dataTableAirpotsExcelData.Rows[i]["airport code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AirportCode

                    #region [AirportName] [varchar] (50)NULL

                    if (columnNames.Contains("airport name*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["airport name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["AirportName"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Airport Name is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["airport name*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["AirportName"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Airport Name is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["AirportName"] = dataTableAirpotsExcelData.Rows[i]["airport name*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AirportCode

                    #region [CityCode] [varchar] (100)NULL

                    if (columnNames.Contains("city*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CityCode"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "City Code is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["city*"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["CityCode"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "City Code is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CityCode"] = dataTableAirpotsExcelData.Rows[i]["city*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [RegionCode] [varchar] (20)NULL
                    if (columnNames.Contains("region code*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["region code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["RegionCode"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Region Code is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["region code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAirportMasterType["RegionCode"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Region Code is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["RegionCode"] = dataTableAirpotsExcelData.Rows[i]["region code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [CountryCode] [varchar] (10) NULL

                    if (columnNames.Contains("country code*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["country code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CountryCode"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Country Code is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["country code*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["CountryCode"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Country Code is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CountryCode"] = dataTableAirpotsExcelData.Rows[i]["country code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion AirportCode

                    #region [CityGroup] [varchar] (10)NULL

                    if (columnNames.Contains("citygroup"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["citygroup"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CityGroup"] = string.Empty;

                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["citygroup"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["CityGroup"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "City Group is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CityGroup"] = dataTableAirpotsExcelData.Rows[i]["citygroup"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [AirportGroup] [varchar] (10)NULL

                    if (columnNames.Contains("airportgroup"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["airportgroup"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["AirportGroup"] = DBNull.Value;

                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["airportgroup"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["AirportGroup"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Airport Group is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["AirportGroup"] = dataTableAirpotsExcelData.Rows[i]["airportgroup"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [RegionGroup] [varchar] (10)NULL

                    if (columnNames.Contains("regiongroup"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["regiongroup"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["RegionGroup"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["regiongroup"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["RegionGroup"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Region Group is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["RegionGroup"] = dataTableAirpotsExcelData.Rows[i]["regiongroup"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [CountryGroup] [varchar] (10)NULL

                    if (columnNames.Contains("countrygroup"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["countrygroup"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CountryGroup"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["countrygroup"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["CountryGroup"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Country Group is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CountryGroup"] = dataTableAirpotsExcelData.Rows[i]["countrygroup"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [IsTaxExempted] [bit] NULL

                    if (columnNames.Contains("istaxexempted"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["istaxexempted"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["IsTaxExempted"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["istaxexempted"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["IsTaxExempted"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["IsTaxExempted"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["IsTaxExempted"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "IsTaxExempted is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [IsActive] [bit] NULL

                    if (columnNames.Contains("isactive"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["isactive"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["IsActive"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["isactive"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["IsActive"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["IsActive"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["IsActive"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + " Is Active is Invalid;";
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region [StationMailId] [varchar] (100)NULL

                    if (columnNames.Contains("station email id"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["station email id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["StationMailId"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["station email id"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["StationMailId"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Station Mail Id is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["StationMailId"] = dataTableAirpotsExcelData.Rows[i]["station email id"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [ManagerName] [varchar] (100)NULL

                    if (columnNames.Contains("mgr name"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["mgr name"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["ManagerName"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["mgr name"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["ManagerName"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Manager Name is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["ManagerName"] = dataTableAirpotsExcelData.Rows[i]["mgr name"].ToString().Trim();
                            }
                        }
                    }
                    #endregion

                    #region [ManagerEmailId] [varchar] (100)NULL

                    if (columnNames.Contains("mgr email id"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["mgr email id"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["ManagerEmailId"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["mgr email id"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["ManagerEmailId"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Manager Email Id is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["ManagerEmailId"] = dataTableAirpotsExcelData.Rows[i]["mgr email id"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [ShiftMobNo] [varchar] (50)NULL

                    if (columnNames.Contains("shift mob no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["shift mob no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["ShiftMobNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["shift mob no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["ShiftMobNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Shift Mobile No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["ShiftMobNo"] = dataTableAirpotsExcelData.Rows[i]["shift mob no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [LandlineNo] [varchar] (50)NULL

                    if (columnNames.Contains("landline no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["landline no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["LandlineNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["landline no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["LandlineNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Landline No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["LandlineNo"] = dataTableAirpotsExcelData.Rows[i]["landline no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [ManagerMobNo] [varchar] (50)NULL

                    if (columnNames.Contains("mgr mob no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["mgr mob no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["ManagerMobNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["mgr mob no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["ManagerMobNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Manager Mobile No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["ManagerMobNo"] = dataTableAirpotsExcelData.Rows[i]["mgr mob no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [counter] [varchar] (500)NULL
                    if (columnNames.Contains("counter time"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["counter time"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["counter"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["counter time"].ToString().Trim().Trim(',').Length > 500)
                            {
                                dataRowAirportMasterType["counter"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "counter time is more than 500 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["counter"] = dataTableAirpotsExcelData.Rows[i]["counter time"].ToString().Trim();
                            }
                        }
                    }
                    #endregion

                    #region [SpotRateApprovalAuathorityMailId] [varchar] (70)NULL

                    if (columnNames.Contains("spotrateapprovalauathoritymailid"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["spotrateapprovalauathoritymailid"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["SpotRateApprovalAuathorityMailId"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["spotrateapprovalauathoritymailid"].ToString().Trim().Trim(',').Length > 70)
                            {
                                dataRowAirportMasterType["SpotRateApprovalAuathorityMailId"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "SpotRate Approval Auathority MailId is more than 70 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["SpotRateApprovalAuathorityMailId"] = dataTableAirpotsExcelData.Rows[i]["spotrateapprovalauathoritymailid"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GHAName] [varchar] (100)NULL

                    if (columnNames.Contains("gha name"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gha name"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GHAName"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gha name"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["GHAName"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GHA Name is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GHAName"] = dataTableAirpotsExcelData.Rows[i]["gha name"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GHAAddress] [varchar] (200)NULL

                    if (columnNames.Contains("gha addr"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gha addr"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GHAAddress"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gha addr"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowAirportMasterType["GHAAddress"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GHA Address is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GHAAddress"] = dataTableAirpotsExcelData.Rows[i]["gha addr"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GHAPhoneNo] [varchar] (50)NULL

                    if (columnNames.Contains("gha ph no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gha ph no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GHAPhoneNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gha ph no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["GHAPhoneNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GHA Phone No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GHAPhoneNo"] = dataTableAirpotsExcelData.Rows[i]["gha ph no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GHAMobileNo] [varchar] (50)NULL

                    if (columnNames.Contains("gha mob no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gha mob no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GHAMobileNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gha mob no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["GHAMobileNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GHA Mobile No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GHAMobileNo"] = dataTableAirpotsExcelData.Rows[i]["gha mob no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GHAFAXNo] [varchar] (50)NULL

                    if (columnNames.Contains("gha fax no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gha fax no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GHAFAXNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gha fax no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["GHAFAXNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GHA Mobile No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GHAFAXNo"] = dataTableAirpotsExcelData.Rows[i]["gha fax no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GHAEmailID] [varchar] (100)NULL

                    if (columnNames.Contains("gha email"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gha email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GHAEmailID"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gha email"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["GHAEmailID"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GHA Email ID is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GHAEmailID"] = dataTableAirpotsExcelData.Rows[i]["gha email"].ToString().Trim();
                            }
                        }
                    }
                    #endregion

                    #region [GSAName] [varchar] (100)NULL

                    if (columnNames.Contains("gsa name"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gsa name"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GSAName"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gsa name"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["GSAName"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GSA Name is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GSAName"] = dataTableAirpotsExcelData.Rows[i]["gsa name"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GSAAddress] [varchar] (200)NULL

                    if (columnNames.Contains("gsa addr"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gsa addr"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GSAAddress"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gsa addr"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowAirportMasterType["GSAAddress"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GSA Address ID is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GSAAddress"] = dataTableAirpotsExcelData.Rows[i]["gsa addr"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GSAPhoneNo] [varchar] (50)NULL

                    if (columnNames.Contains("gsa ph no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gsa ph no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GSAPhoneNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gsa ph no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["GSAPhoneNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GSA Phone No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GSAPhoneNo"] = dataTableAirpotsExcelData.Rows[i]["gsa ph no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GSAMobileNo] [varchar] (50)NULL

                    if (columnNames.Contains("gsa mob no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gsa mob no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GSAMobileNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gsa mob no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["GSAMobileNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GSA Mobile No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GSAMobileNo"] = dataTableAirpotsExcelData.Rows[i]["gsa mob no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GSAFAXNo] [varchar] (50)NULL

                    if (columnNames.Contains("gsa fax no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gsa fax no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GSAFAXNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gsa fax no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["GSAFAXNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GSA FAX No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GSAFAXNo"] = dataTableAirpotsExcelData.Rows[i]["gsa fax no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [GSAEmailID] [varchar] (100)NULL

                    if (columnNames.Contains("gsa email"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gsa email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GSAEmailID"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gsa email"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["GSAEmailID"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GSA EmailId is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GSAEmailID"] = dataTableAirpotsExcelData.Rows[i]["gsa email"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [APMName] [varchar] (100)NULL

                    if (columnNames.Contains("apm name"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["apm name"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["APMName"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["apm name"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["APMName"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "APM Name is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["APMName"] = dataTableAirpotsExcelData.Rows[i]["apm name"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [APMAddress] [varchar] (200)NULL

                    if (columnNames.Contains("apm addr"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["apm addr"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["APMAddress"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["apm addr"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowAirportMasterType["APMAddress"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "APM Address is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["APMAddress"] = dataTableAirpotsExcelData.Rows[i]["apm addr"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [APMPhoneNo] [varchar] (50)NULL

                    if (columnNames.Contains("apm ph no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["apm ph no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["APMPhoneNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["apm ph no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["APMPhoneNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "APM Phone No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["APMPhoneNo"] = dataTableAirpotsExcelData.Rows[i]["apm ph no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [APMMobileNo] [varchar] (50)NULL

                    if (columnNames.Contains("apm mob no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["apm mob no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["APMMobileNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["apm mob no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["APMMobileNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "APM Mobile No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["APMMobileNo"] = dataTableAirpotsExcelData.Rows[i]["apm mob no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [APMFAXNo] [varchar] (50)NULL
                    if (columnNames.Contains("apm fax no"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["apm fax no"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["APMFAXNo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["apm fax no"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["APMFAXNo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "APM Fax No is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["APMFAXNo"] = dataTableAirpotsExcelData.Rows[i]["apm fax no"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [APMEmailID] [varchar] (100)NULL

                    if (columnNames.Contains("apm email"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["apm email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["APMEmailID"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["apm email"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowAirportMasterType["APMEmailID"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "APM EmailID is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["APMEmailID"] = dataTableAirpotsExcelData.Rows[i]["apm email"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [AdditionalInfo] [varchar] (500)NULL

                    if (columnNames.Contains("additional info"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["additional info"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["AdditionalInfo"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["additional info"].ToString().Trim().Trim(',').Length > 500)
                            {
                                dataRowAirportMasterType["AdditionalInfo"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Additional Info is more than 500 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["AdditionalInfo"] = dataTableAirpotsExcelData.Rows[i]["additional info"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [TransitTime] [varchar] (10)NULL

                    if (columnNames.Contains("transit time"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["transit time"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["TransitTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["transit time"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["TransitTime"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Transit Time is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["TransitTime"] = dataTableAirpotsExcelData.Rows[i]["transit time"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [CutOffTime] [varchar] (10)NULL

                    if (columnNames.Contains("cut off time"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["cut off time"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CutOffTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["cut off time"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["CutOffTime"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "CutOff Time is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CutOffTime"] = dataTableAirpotsExcelData.Rows[i]["cut off time"].ToString().Trim();
                            }
                        }
                    }

                    #endregion

                    #region [BookingCurrrency] [varchar] (10)NULL

                    if (columnNames.Contains("booking curr*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["booking curr*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["BookingCurrrency"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Booking Currrency is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["booking curr*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["BookingCurrrency"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Booking Currrency is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["BookingCurrrency"] = dataTableAirpotsExcelData.Rows[i]["booking curr*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [InvoiceCurrrency] [varchar] (10)NULL

                    if (columnNames.Contains("inv curr*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["inv curr*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["InvoiceCurrrency"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Invoice Currrency is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["inv curr*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["InvoiceCurrrency"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Invoice Currrency is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["InvoiceCurrrency"] = dataTableAirpotsExcelData.Rows[i]["inv curr*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [InvoiceType] [varchar] (10)NULL

                    if (columnNames.Contains("inv curr type*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["inv curr type*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["InvoiceType"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Invoice Currrency Type is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["inv curr type*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["InvoiceType"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Invoice Currrency Type is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["InvoiceType"] = dataTableAirpotsExcelData.Rows[i]["inv curr type*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [BookingType] [varchar] (10)NULL

                    if (columnNames.Contains("booking curr type*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["booking curr type*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["BookingType"] = string.Empty;
                            validationDetailsAirport = validationDetailsAirport + "Booking Currrency Type is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["booking curr type*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["BookingType"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "Booking Currrency Type is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["BookingType"] = dataTableAirpotsExcelData.Rows[i]["booking curr type*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [isRFIDEnable] [bit] NULL

                    if (columnNames.Contains("is rfid enable"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["is rfid enable"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["isRFIDEnable"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["is rfid enable"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["isRFIDEnable"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["isRFIDEnable"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["isRFIDEnable"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "Is RFID Enable is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [CityType] [varchar] (50)NULL

                    if (columnNames.Contains("is metro"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["is metro"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CityType"] = string.Empty;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["is metro"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["CityType"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "CityType is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CityType"] = dataTableAirpotsExcelData.Rows[i]["is metro"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [GLAccountCode] [varchar] (20)NULL

                    if (columnNames.Contains("gla acc code"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["gla acc code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["GLAccountCode"] = string.Empty;

                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["gla acc code"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAirportMasterType["GLAccountCode"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "GLA ccount Code is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["GLAccountCode"] = dataTableAirpotsExcelData.Rows[i]["gla acc code"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [TimeZone] [datetime] NULL
                    dataRowAirportMasterType["TimeZone"] = DBNull.Value;

                    #endregion

                    #region [TimeZones] [varchar] (150)NULL

                    if (columnNames.Contains("timezone*"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["timezone*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["TimeZones"] = DBNull.Value;
                            validationDetailsAirport = validationDetailsAirport + "TimeZone is required;";
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["timezone*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["TimeZones"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "TimeZone is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["TimeZones"] = dataTableAirpotsExcelData.Rows[i]["timezone*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [UTCTIMEDIFF] [varchar] (10)NULL

                    if (columnNames.Contains("utc time diff"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["utc time diff"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["UTCTIMEDIFF"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["utc time diff"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["UTCTIMEDIFF"] = string.Empty;
                                validationDetailsAirport = validationDetailsAirport + "UTC TIME DIFF is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["UTCTIMEDIFF"] = dataTableAirpotsExcelData.Rows[i]["utc time diff"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [IsULDEnabled] [bit] NULL

                    if (columnNames.Contains("is uld enabled"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["is uld enabled"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["IsULDEnabled"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["is uld enabled"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["IsULDEnabled"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["IsULDEnabled"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["IsULDEnabled"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "IsULDEnabled is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [Latitude] [varchar] (20)NULL

                    if (columnNames.Contains("latitude"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["latitude"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["Latitude"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["latitude"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAirportMasterType["Latitude"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Latitude is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["Latitude"] = dataTableAirpotsExcelData.Rows[i]["latitude"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [Longitude] [varchar] (20)NULL
                    if (columnNames.Contains("longitude"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["longitude"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["Longitude"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["longitude"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAirportMasterType["Longitude"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Longitude is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["Longitude"] = dataTableAirpotsExcelData.Rows[i]["longitude"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [CustomAirportCode] [varchar] (10)NULL

                    if (columnNames.Contains("customairportcode"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["customairportcode"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CustomAirportCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["customairportcode"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["CustomAirportCode"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Custom Airportcode is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CustomAirportCode"] = dataTableAirpotsExcelData.Rows[i]["customairportcode"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [CustomOfficeCode] [varchar] (10)NULL

                    if (columnNames.Contains("customofficecode"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["customofficecode"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CustomOfficeCode"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["customofficecode"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["CustomOfficeCode"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Custom Office code is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["CustomOfficeCode"] = dataTableAirpotsExcelData.Rows[i]["customofficecode"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [DATEFORMAT] [varchar] (20)NULL

                    if (columnNames.Contains("dateformat"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["dateformat"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["DATEFORMAT"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["dateformat"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowAirportMasterType["DATEFORMAT"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "dateformat is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["DATEFORMAT"] = dataTableAirpotsExcelData.Rows[i]["dateformat"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [Aging1] [varchar] (5)NULL

                    if (columnNames.Contains("aging1"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["aging1"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["Aging1"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["aging1"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowAirportMasterType["Aging1"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Aging1 is more than 5 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["Aging1"] = dataTableAirpotsExcelData.Rows[i]["aging1"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region [Aging2] [varchar] (5)NULL

                    if (columnNames.Contains("aging2"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["aging2"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["Aging2"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["aging2"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowAirportMasterType["Aging2"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Aging2 is more than 5 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["Aging2"] = dataTableAirpotsExcelData.Rows[i]["aging2"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [UOM] [varchar] (3)NULL

                    if (columnNames.Contains("uom"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["UOM"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["uom"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowAirportMasterType["UOM"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "UOM is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["UOM"] = dataTableAirpotsExcelData.Rows[i]["uom"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [ShowInMobileApp] [bit] NULL

                    if (columnNames.Contains("showinmobileapp"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["showinmobileapp"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["ShowInMobileApp"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["showinmobileapp"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "Yes":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["ShowInMobileApp"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["ShowInMobileApp"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["ShowInMobileApp"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "ShowInMobileApp is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [MobilityCutOffTime] [varchar] (10)NULL

                    if (columnNames.Contains("mobilitycutofftime"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["mobilitycutofftime"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["MobilityCutOffTime"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["mobilitycutofftime"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["MobilityCutOffTime"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "mobilitycutofftime is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["MobilityCutOffTime"] = dataTableAirpotsExcelData.Rows[i]["mobilitycutofftime"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [AirportAddress] [varchar] (200)NULL

                    if (columnNames.Contains("airportaddress"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["airportaddress"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["AirportAddress"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["airportaddress"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowAirportMasterType["AirportAddress"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Airport Address is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["AirportAddress"] = dataTableAirpotsExcelData.Rows[i]["airportaddress"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [ProductType] [varchar] (50)NULL

                    if (columnNames.Contains("producttype"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["ProductType"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowAirportMasterType["ProductType"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Product Type is more than 50 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["ProductType"] = dataTableAirpotsExcelData.Rows[i]["producttype"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [IsOperational] [bit] NULL

                    if (columnNames.Contains("isoperational"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["isoperational"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["IsOperational"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["isoperational"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["IsOperational"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["IsOperational"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["IsOperational"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "IsOperational is Invalid;";
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region [AirPortType] [varchar] (30)NULL

                    if (columnNames.Contains("airporttype"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["airporttype"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["AirPortType"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["airporttype"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAirportMasterType["AirPortType"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "airport type is more than 200 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["AirPortType"] = dataTableAirpotsExcelData.Rows[i]["airporttype"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [DimsUOM] [varchar] (10)NULL

                    if (columnNames.Contains("dimsuom"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["dimsuom"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["DimsUOM"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["dimsuom"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowAirportMasterType["DimsUOM"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Dims UOM is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["DimsUOM"] = dataTableAirpotsExcelData.Rows[i]["dimsuom"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region [Freezer] [bit] NULL

                    if (columnNames.Contains("freezer"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["freezer"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["Freezer"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["freezer"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["Freezer"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["Freezer"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["Freezer"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "freezer is Invalid;";
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region [Cooler] [bit] NULL

                    if (columnNames.Contains("cooler"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["cooler"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["Cooler"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["cooler"].ToString().Trim().Trim(',').Length > 1)
                            {
                                dataRowAirportMasterType["Cooler"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "Cooler is more than 1 Chars;";
                            }
                            else
                            {
                                switch (dataTableAirpotsExcelData.Rows[i]["cooler"].ToString().ToUpper().Trim().Trim(','))
                                {
                                    case "Y":
                                    case "YES":
                                    case "TRUE":
                                    case "1":
                                        dataRowAirportMasterType["Cooler"] = 1;
                                        break;
                                    case "N":
                                    case "NO":
                                    case "FALSE":
                                    case "0":
                                        dataRowAirportMasterType["Cooler"] = 0;
                                        break;
                                    default:
                                        dataRowAirportMasterType["Cooler"] = DBNull.Value;
                                        validationDetailsAirport = validationDetailsAirport + "Cooler is Invalid;";
                                        break;
                                }
                            }
                        }
                    }

                    #endregion

                    #region [InternationalFrieght] [bit] NULL

                    if (columnNames.Contains("internationalfrieght"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["internationalfrieght"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["InternationalFrieght"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["internationalfrieght"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["InternationalFrieght"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["InternationalFrieght"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["InternationalFrieght"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "InternationalFrieght is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region [CloakRoom] [bit] NULL

                    if (columnNames.Contains("cloakroom"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["cloakroom"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["CloakRoom"] = DBNull.Value;
                        }
                        else
                        {
                            switch (dataTableAirpotsExcelData.Rows[i]["cloakroom"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowAirportMasterType["CloakRoom"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowAirportMasterType["CloakRoom"] = 0;
                                    break;
                                default:
                                    dataRowAirportMasterType["CloakRoom"] = DBNull.Value;
                                    validationDetailsAirport = validationDetailsAirport + "cloakroom is Invalid;";
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region [LocationID] [varchar] (30) NULL

                    if (columnNames.Contains("locationid"))
                    {
                        if (dataTableAirpotsExcelData.Rows[i]["locationid"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowAirportMasterType["LocationID"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTableAirpotsExcelData.Rows[i]["locationid"].ToString().Trim().Trim(',').Length > 30)
                            {
                                dataRowAirportMasterType["LocationID"] = DBNull.Value;
                                validationDetailsAirport = validationDetailsAirport + "LocationID is more than 30 Chars;";
                            }
                            else
                            {
                                dataRowAirportMasterType["LocationID"] = dataTableAirpotsExcelData.Rows[i]["locationid"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #endregion Create row for AirportMasterType Data Table

                    dataRowAirportMasterType["ValidationDetailsAirportMaster"] = validationDetailsAirport;
                    AirportMasterType.Rows.Add(dataRowAirportMasterType);
                }

                // Database Call to Validate & Insert/Update AirportMaster Master
                string errorInSp = string.Empty;
                ValidateAndInsertUpdateAirportMaster(srNotblMasterUploadSummaryLog, AirportMasterType, errorInSp);

                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTableAirpotsExcelData = null;
            }
        }
        public DataSet ValidateAndInsertUpdateAirportMaster(int srNotblMasterUploadSummaryLog, DataTable shipperConsigneeType, string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { 
                                                                      new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@AirportsTableType", shipperConsigneeType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadAirportsMaster", sqlParameters);

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
