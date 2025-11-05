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

namespace QidWorkerRole.UploadMasters.PartnerSchedule
{
    public class UploadPartnerSchedule
    {
        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        public Boolean PartnerScheduleUpload(DataSet dsFiles)
        {
            try
            {
                //DataSet dsFiles = new DataSet();
                //dsFiles = uploadMasterCommon.GetUploadedFileData(UploadMasterType.PartnerSchedule);
                string FilePath = "";

                foreach (DataRow dr in dsFiles.Tables[0].Rows)
                {
                    // to upadate retry count only.
                    uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                    UploadMasterCommon umc = new UploadMasterCommon();

                    if (umc.DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "PartnerSchedule", out FilePath))
                    {
                        ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath);
                    }
                    else
                    {
                        uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                        uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dr["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        continue;
                    }

                    umc.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                    umc.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                }
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
            }
            return false;
        }

        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath)
        {
            DataTable dataTablePartnerScheduleExcelData = new DataTable("dataTablePartnerScheduleExcelData");

            bool isBinaryReader = false;

            try
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(filepath).ToLower();
                decimal tempDecimalValue = 0;

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".XLS") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTablePartnerScheduleExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                uploadMasterCommon.RemoveEmptyRows(dataTablePartnerScheduleExcelData);

                foreach (DataColumn dataColumn in dataTablePartnerScheduleExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }
                string[] columnNames = dataTablePartnerScheduleExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating PartnerSchedule Master DataTable
                DataTable PartnerScheduleType = new DataTable("PartnerScheduleType");
                PartnerScheduleType.Columns.Add("PartnerScheduleIndex", System.Type.GetType("System.Int32"));
                PartnerScheduleType.Columns.Add("ScheduleID", System.Type.GetType("System.Int32"));
                PartnerScheduleType.Columns.Add("PartnerCode", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("FromtDt", System.Type.GetType("System.DateTime"));
                PartnerScheduleType.Columns.Add("ToDt", System.Type.GetType("System.DateTime"));
                PartnerScheduleType.Columns.Add("EquipmentNo", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("FlightID", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("Source", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("Dest", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("SchDeptTime", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("SchArrTime", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("Frequency", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
                PartnerScheduleType.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("IsActive", System.Type.GetType("System.Byte"));
                PartnerScheduleType.Columns.Add("CargoCapacity", System.Type.GetType("System.Decimal"));
                PartnerScheduleType.Columns.Add("Status", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("Isdomestic", System.Type.GetType("System.Int32"));
                PartnerScheduleType.Columns.Add("DeptTimeZone", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("ArrTimeZone", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("UOM", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("SchDeptDay", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("SchArrDay", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("AircraftType", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("TailNo", System.Type.GetType("System.String"));
                PartnerScheduleType.Columns.Add("ValidationDetailsPartnerSchedule", System.Type.GetType("System.String"));

                #endregion PartnerSchedule Master

                string validationDetailsPartnerSchedule = string.Empty;
                DateTime tempDate;

                for (int i = 0; i < dataTablePartnerScheduleExcelData.Rows.Count; i++)
                {
                    validationDetailsPartnerSchedule = string.Empty;

                    #region Create row for PartnerScheduleType Data Table
                    DataRow dataRowPartnerScheduleType = PartnerScheduleType.NewRow();

                    dataRowPartnerScheduleType["PartnerScheduleIndex"] = i + 1;

                    #region [ScheduleID] [int] NOT NULL
                    dataRowPartnerScheduleType["ScheduleID"] = 0;
                    #endregion

                    #region[PartnerCode] [varchar] (20) NULL

                    if (columnNames.Contains("partner code*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["partner code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["PartnerCode"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Partner Code is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["partner code*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowPartnerScheduleType["PartnerCode"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Partner Code is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["PartnerCode"] = dataTablePartnerScheduleExcelData.Rows[i]["partner code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[FromtDt] [datetime] NULL

                    if (columnNames.Contains("fromt date*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["fromt date*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "From Date is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowPartnerScheduleType["FromtDt"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerScheduleExcelData.Rows[i]["fromt date*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTablePartnerScheduleExcelData.Rows[i]["fromt date*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowPartnerScheduleType["FromtDt"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowPartnerScheduleType["FromtDt"] = DBNull.Value;
                                        validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid From Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowPartnerScheduleType["FromtDt"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid From Date;";
                            }
                        }
                    }

                    #endregion

                    #region[ToDt] [datetime] NULL

                    if (columnNames.Contains("to date*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["to date*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "To From is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowPartnerScheduleType["ToDt"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerScheduleExcelData.Rows[i]["to date*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTablePartnerScheduleExcelData.Rows[i]["to date*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowPartnerScheduleType["ToDt"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowPartnerScheduleType["ToDt"] = DBNull.Value;
                                        validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid To Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowPartnerScheduleType["ToDt"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid To Date;";
                            }
                        }
                    }

                    #endregion

                    #region[EquipmentNo] [varchar] (15)  NULL

                    if (columnNames.Contains("equipmentno*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["equipmentno*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["EquipmentNo"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "EquipmentNo is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["equipmentno*"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowPartnerScheduleType["EquipmentNo"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "EquipmentNo is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["EquipmentNo"] = dataTablePartnerScheduleExcelData.Rows[i]["equipmentno*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region[FlightID] [varchar] (10)  NULL
                    if (columnNames.Contains("flightid*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["flightid*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["FlightID"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "FlightNo is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["flightid*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerScheduleType["FlightID"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "FlightNo is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["FlightID"] = dataTablePartnerScheduleExcelData.Rows[i]["flightid*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[Source] [varchar] (5)  NULL
                    if (columnNames.Contains("source*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["source*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Source"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Source is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["source*"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowPartnerScheduleType["Source"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Source is more than 5 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Source"] = dataTablePartnerScheduleExcelData.Rows[i]["source*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[Dest] [varchar] (5)  NULL
                    if (columnNames.Contains("dest*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["dest*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Dest"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Destination is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["dest*"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowPartnerScheduleType["Dest"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Destination is more than 5 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Dest"] = dataTablePartnerScheduleExcelData.Rows[i]["dest*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[SchDeptTime] [varchar] (15)  NULL
                    if (columnNames.Contains("deptarture time*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["deptarture time*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["SchDeptTime"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Deptarture Time is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["deptarture time*"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowPartnerScheduleType["SchDeptTime"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Deptarture Time is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["SchDeptTime"] = dataTablePartnerScheduleExcelData.Rows[i]["deptarture time*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[SchArrTime] [varchar] (15)  NULL
                    if (columnNames.Contains("arrival time*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["arrival time*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["SchArrTime"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Arrival Time is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["arrival time*"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowPartnerScheduleType["SchArrTime"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Arrival Time is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["SchArrTime"] = dataTablePartnerScheduleExcelData.Rows[i]["arrival time*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[Frequency] [varchar] (15)  NULL
                    if (columnNames.Contains("frequency*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["frequency*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Frequency"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Frequency is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["frequency*"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowPartnerScheduleType["Frequency"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Frequency is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Frequency"] = dataTablePartnerScheduleExcelData.Rows[i]["frequency*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[UpdatedOn] [datetime] NULL

                    if (columnNames.Contains("updatedon*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["updatedon*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "UpdatedOn Date is required;";
                        }
                        else
                        {
                            try
                            {
                                if (isBinaryReader)
                                {
                                    dataRowPartnerScheduleType["UpdatedOn"] = DateTime.FromOADate(Convert.ToDouble(dataTablePartnerScheduleExcelData.Rows[i]["updatedon*"].ToString().Trim()));
                                }
                                else
                                {
                                    if (DateTime.TryParse(dataTablePartnerScheduleExcelData.Rows[i]["updatedon*"].ToString().Trim(), out tempDate))
                                    {
                                        dataRowPartnerScheduleType["UpdatedOn"] = tempDate;
                                    }
                                    else
                                    {
                                        dataRowPartnerScheduleType["UpdatedOn"] = DBNull.Value;
                                        validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid UpdatedOn Date;";
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                dataRowPartnerScheduleType["ToDt"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid To Date;";
                            }
                        }
                    }
                    #endregion

                    #region[UpdatedBy] [varchar] (100)  NULL
                    if (columnNames.Contains("updatedby*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["updatedby*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["UpdatedBy"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "UpdatedBy is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["updatedby*"].ToString().Trim().Trim(',').Length > 100)
                            {
                                dataRowPartnerScheduleType["UpdatedBy"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "UpdatedBy is more than 100 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["UpdatedBy"] = dataTablePartnerScheduleExcelData.Rows[i]["updatedby*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[IsActive] [bit] NULL

                    if (columnNames.Contains("isactive*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["isactive*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["IsActive"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "IsActive is required;";
                        }
                        else
                        {
                            switch (dataTablePartnerScheduleExcelData.Rows[i]["isactive*"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerScheduleType["IsActive"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerScheduleType["IsActive"] = 0;
                                    break;
                                default:
                                    dataRowPartnerScheduleType["IsActive"] = DBNull.Value;
                                    validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "IsActive is Invalid;";
                                    break;
                            }
                        }
                    }

                    #endregion

                    #region[CargoCapacity] [decimal] (18, 2) NULL

                    if (columnNames.Contains("cargocapacity*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["cargocapacity*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["CargoCapacity"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "CargoCapacity is required;";
                        }
                        else
                        {
                            try
                            {
                                tempDecimalValue = 0;
                                if (decimal.TryParse(dataTablePartnerScheduleExcelData.Rows[i]["cargocapacity*"].ToString().Trim().Trim(','), out tempDecimalValue))
                                {
                                    dataRowPartnerScheduleType["CargoCapacity"] = tempDecimalValue;
                                }
                                else
                                {
                                    dataRowPartnerScheduleType["CargoCapacity"] = DBNull.Value;
                                    validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid CargoCapacity;";
                                }

                            }
                            catch (Exception)
                            {
                                dataRowPartnerScheduleType["CargoCapacity"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Invalid CargoCapacity;";
                            }
                        }
                    }

                    #endregion

                    #region[Status] [varchar] (10)  NULL
                    if (columnNames.Contains("status*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["status*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Status"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Status is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["status*"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerScheduleType["Status"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Status is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Status"] = dataTablePartnerScheduleExcelData.Rows[i]["status*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[Isdomestic] [int] NULL
                    if (columnNames.Contains("isdomestic*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["isdomestic*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Isdomestic"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "isdomestic is required;";
                        }
                        else
                        {
                            switch (dataTablePartnerScheduleExcelData.Rows[i]["isdomestic*"].ToString().ToUpper().Trim().Trim(','))
                            {
                                case "Y":
                                case "YES":
                                case "TRUE":
                                case "1":
                                    dataRowPartnerScheduleType["Isdomestic"] = 1;
                                    break;
                                case "N":
                                case "NO":
                                case "FALSE":
                                case "0":
                                    dataRowPartnerScheduleType["Isdomestic"] = 0;
                                    break;
                                default:
                                    dataRowPartnerScheduleType["Isdomestic"] = DBNull.Value;
                                    validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "Isdomestic is Invalid;";
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region[DeptTimeZone] [varchar] (10)  NULL
                    if (columnNames.Contains("depttimezone"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["depttimezone"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["DeptTimeZone"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["depttimezone"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerScheduleType["DeptTimeZone"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "DeptTimeZone is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["DeptTimeZone"] = dataTablePartnerScheduleExcelData.Rows[i]["depttimezone"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[ArrTimeZone] [varchar] (10)  NULL
                    if (columnNames.Contains("arrtimezone"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["arrtimezone"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ArrTimeZone"] = DBNull.Value;
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["arrtimezone"].ToString().Trim().Trim(',').Length > 10)
                            {
                                dataRowPartnerScheduleType["ArrTimeZone"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "ArrTimeZone is more than 10 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["ArrTimeZone"] = dataTablePartnerScheduleExcelData.Rows[i]["arrtimezone"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion

                    #region[UOM] [varchar] (3)  NULL
                    if (columnNames.Contains("uom*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["uom*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["UOM"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "UOM is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["uom*"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowPartnerScheduleType["UOM"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "UOM is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["UOM"] = dataTablePartnerScheduleExcelData.Rows[i]["uom*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region[SchDeptDay] [varchar](3) Not Null
                    if (columnNames.Contains("schdeptday*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["schdeptday*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["SchDeptDay"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "SchDeptDay is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["schdeptday*"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowPartnerScheduleType["SchDeptDay"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "SchDeptDay is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["SchDeptDay"] = dataTablePartnerScheduleExcelData.Rows[i]["schdeptday*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region[SchArrDay] [varchar](3) Not Null
                    if (columnNames.Contains("scharrday*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["schdeptday*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["SchArrDay"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "SchArrDay is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["scharrday*"].ToString().Trim().Trim(',').Length > 3)
                            {
                                dataRowPartnerScheduleType["SchArrDay"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "SchArrDay is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["SchArrDay"] = dataTablePartnerScheduleExcelData.Rows[i]["scharrday*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region[AircraftType] [varchar](15)  Null
                    if (columnNames.Contains("aircrafttype*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["aircrafttype*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["AircraftType"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "AircraftType is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["aircrafttype*"].ToString().Trim().Trim(',').Length > 15)
                            {
                                dataRowPartnerScheduleType["AircraftType"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "AircraftType is more than 15 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["AircraftType"] = dataTablePartnerScheduleExcelData.Rows[i]["aircrafttype*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion

                    #region[TailNo] [varchar](20)  Null
                    if (columnNames.Contains("tailno*"))
                    {
                        if (dataTablePartnerScheduleExcelData.Rows[i]["tailno*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["TailNo"] = DBNull.Value;
                            validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "TailNo is required;";
                        }
                        else
                        {
                            if (dataTablePartnerScheduleExcelData.Rows[i]["tailno*"].ToString().Trim().Trim(',').Length > 20)
                            {
                                dataRowPartnerScheduleType["TailNo"] = DBNull.Value;
                                validationDetailsPartnerSchedule = validationDetailsPartnerSchedule + "TailNo is more than 20 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["TailNo"] = dataTablePartnerScheduleExcelData.Rows[i]["tailno*"].ToString().Trim().Trim(',');
                            }
                        }
                    }

                    #endregion
                    #endregion Create row for PartnerScheduleType Data Table

                    dataRowPartnerScheduleType["ValidationDetailsPartnerSchedule"] = validationDetailsPartnerSchedule;
                    PartnerScheduleType.Rows.Add(dataRowPartnerScheduleType);
                }

                string errorInSp = string.Empty;
                ValidateAndInsertPartnerSchedule(srNotblMasterUploadSummaryLog, PartnerScheduleType, errorInSp);
                return true;
            }
            catch (Exception exception)
            {
                clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                return false;
            }
            finally
            {
                dataTablePartnerScheduleExcelData = null;
            }
        }

        public DataSet ValidateAndInsertPartnerSchedule(int srNotblMasterUploadSummaryLog, DataTable partnerScheduleType, string errorInSp)
        {
            DataSet dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] { 
                                                                      new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@PartnerScheduleType", partnerScheduleType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                SQLServer sQLServer = new SQLServer();
                dataSetResult = sQLServer.SelectRecords("uspUploadPartnerSchedule", sqlParameters);

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
