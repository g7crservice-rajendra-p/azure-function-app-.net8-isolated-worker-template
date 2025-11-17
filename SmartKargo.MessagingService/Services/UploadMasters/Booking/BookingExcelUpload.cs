using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.Booking
{
    public class BookingExcelUpload
    {
        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<BookingExcelUpload> _logger;
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<Cls_BL> _staticLogger => _loggerFactory?.CreateLogger<Cls_BL>();
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public BookingExcelUpload(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<BookingExcelUpload> logger,
            UploadMasterCommon uploadMasterCommon,
            ILoggerFactory loggerFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;
            _loggerFactory = loggerFactory;
        }
        #endregion

        public async Task<Boolean> BookingUpload(DataSet dsFiles)
        {
            try
            {
                string FilePath = "", userName = "";
                
                foreach (DataRow dr in dsFiles.Tables[0].Rows)
                {
                    userName = dr["UploadedBy"].ToString();
                    // to upadate retry count only.
                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                    //UploadMasterCommon umc = new UploadMasterCommon();

                    //if (umc.DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "BookingExcelUpload", out FilePath))
                    if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "BookingExcelUpload", out FilePath))
                    {
                        ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath, userName);
                    }
                    else
                    {
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                        await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dr["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        continue;
                    }

                    //umc.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                    //umc.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);

                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                }
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                _logger.LogError("Message: {message} \nStackTrace: {stackTrace}", exception.Message, exception.StackTrace);
            }
            return false;
        }

        public bool ProcessFile(int srNotblMasterUploadSummaryLog, string filepath, string userName)
        {
            // clsLog.WriteLogAzure("Process Booking Excel File: " + filepath);
            _logger.LogInformation("Process Booking Excel File: {FilePath}", filepath);
            DataTable dataTableBookingExcelData = new DataTable();

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
                dataTableBookingExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommon.RemoveEmptyRows(dataTableBookingExcelData);

                foreach (DataColumn dataColumn in dataTableBookingExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }
                string[] columnNames = dataTableBookingExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Booking details data table
                DataTable BookingDetailsType = new DataTable();
                BookingDetailsType.Columns.Add("BookingIndex", System.Type.GetType("System.Int32"));
                BookingDetailsType.Columns.Add("AirwayBillPrefix", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("AirWayBillNumber", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShippingAgentCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("Origin", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("Destination", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("CommodityCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("CommodityDescription", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("Pieces", System.Type.GetType("System.Int32"));
                BookingDetailsType.Columns.Add("GrossWt", System.Type.GetType("System.Decimal"));
                BookingDetailsType.Columns.Add("UOM", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("Dims", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("DimsUOM", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperName", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperAddress", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperCity", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperState", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperCountry", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperPostalCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperTelephone", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ShipperEmail", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeName", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeAddress", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeCity", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeState", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeCountry", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneePostalCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeTelephone", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ConsigneeEmail", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("EORINumber", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("ProductType", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("PaymentMode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("SHC", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("FlightDate", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("FlightNumber", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("AllotmentCode", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("HandlingInfo", System.Type.GetType("System.String"));
                BookingDetailsType.Columns.Add("DVForCarriage", System.Type.GetType("System.Decimal"));
                BookingDetailsType.Columns.Add("DVForCustoms", System.Type.GetType("System.Decimal"));
                BookingDetailsType.Columns.Add("ValidationDetails", System.Type.GetType("System.String"));                
                #endregion Booking details data table

                
                string validationDetailsBooking = string.Empty;

                for (int i = 0; i < dataTableBookingExcelData.Rows.Count; i++)
                {
                    validationDetailsBooking = string.Empty;

                    #region Create row for Booking Data Table
                    DataRow dataRowPartnerScheduleType = BookingDetailsType.NewRow();
                    
                    dataRowPartnerScheduleType["BookingIndex"] = i + 1;

                    #region airwaybill prefix*

                    if (columnNames.Contains("airwaybill prefix*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["airwaybill prefix*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["AirwayBillPrefix"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Prefix is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["airwaybill prefix*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerScheduleType["AirwayBillPrefix"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Prefix is more than 3 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["AirwayBillPrefix"] = dataTableBookingExcelData.Rows[i]["airwaybill prefix*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion airwaybill prefix*

                    #region airwaybill number*

                    if (columnNames.Contains("airwaybill number*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["airwaybill number*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["AirWayBillNumber"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["airwaybill number*"].ToString().Trim().Trim(',').Length > 8)
                            {
                                dataRowPartnerScheduleType["AirWayBillNumber"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["AirWayBillNumber"] = dataTableBookingExcelData.Rows[i]["airwaybill number*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion airwaybill number*

                    #region shipping agent code*

                    if (columnNames.Contains("shipping agent code*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipping agent code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShippingAgentCode"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["shipping agent code*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowPartnerScheduleType["ShippingAgentCode"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["ShippingAgentCode"] = dataTableBookingExcelData.Rows[i]["shipping agent code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion shipping agent code*

                    #region origin*

                    if (columnNames.Contains("origin*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["origin*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Origin"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["origin*"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowPartnerScheduleType["Origin"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Origin"] = dataTableBookingExcelData.Rows[i]["origin*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion origin*

                    #region destination*

                    if (columnNames.Contains("destination*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["destination*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Destination"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["destination*"].ToString().Trim().Trim(',').Length > 5)
                            {
                                dataRowPartnerScheduleType["Destination"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Destination"] = dataTableBookingExcelData.Rows[i]["destination*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion destination*

                    #region commodity code*

                    if (columnNames.Contains("commodity code*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["commodity code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["CommodityCode"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["commodity code*"].ToString().Trim().Trim(',').Length > 50)
                            {
                                dataRowPartnerScheduleType["CommodityCode"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["CommodityCode"] = dataTableBookingExcelData.Rows[i]["commodity code*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion destination*

                    #region commodity description*

                    if (columnNames.Contains("commodity description*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["commodity description*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["CommodityDescription"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["commodity description*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerScheduleType["CommodityDescription"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["CommodityDescription"] = dataTableBookingExcelData.Rows[i]["commodity description*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion destination*

                    #region pieces*

                    if (columnNames.Contains("pieces*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["pieces*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Pieces"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["pieces*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerScheduleType["Pieces"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Pieces"] = dataTableBookingExcelData.Rows[i]["pieces*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion pieces*

                    #region gross wt*

                    if (columnNames.Contains("gross wt*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["gross wt*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["GrossWt"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["gross wt*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerScheduleType["GrossWt"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["GrossWt"] = dataTableBookingExcelData.Rows[i]["gross wt*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion pieces*

                    #region uom*
                    if (columnNames.Contains("uom*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["uom*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["UOM"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["uom*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerScheduleType["UOM"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["UOM"] = dataTableBookingExcelData.Rows[i]["uom*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion uom*

                    #region dims*
                    if (columnNames.Contains("dims*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["dims*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["Dims"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            if (dataTableBookingExcelData.Rows[i]["dims*"].ToString().Trim().Trim(',').Length > 200)
                            {
                                dataRowPartnerScheduleType["Dims"] = DBNull.Value;
                                validationDetailsBooking = validationDetailsBooking + "AWB Number is more than 8 Chars;";
                            }
                            else
                            {
                                dataRowPartnerScheduleType["Dims"] = dataTableBookingExcelData.Rows[i]["dims*"].ToString().Trim().Trim(',');
                            }
                        }
                    }
                    #endregion dims*

                    #region dims uom*
                    if (columnNames.Contains("dims uom*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["dims uom*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["DimsUOM"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["DimsUOM"] = dataTableBookingExcelData.Rows[i]["dims uom*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion dims uom*

                    #region shipper code
                    if (columnNames.Contains("shipper code"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperCode"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperCode"] = dataTableBookingExcelData.Rows[i]["shipper code"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper code

                    #region shipper name*
                    if (columnNames.Contains("shipper name*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperName"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperName"] = dataTableBookingExcelData.Rows[i]["shipper name*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper name

                    #region shipper address*
                    if (columnNames.Contains("shipper address*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper address*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperAddress"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperAddress"] = dataTableBookingExcelData.Rows[i]["shipper address*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper address*

                    #region shipper city*
                    if (columnNames.Contains("shipper city*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper city*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperCity"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperCity"] = dataTableBookingExcelData.Rows[i]["shipper city*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper city*

                    #region shipper state*
                    if (columnNames.Contains("shipper state*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper state*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperState"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperState"] = dataTableBookingExcelData.Rows[i]["shipper state*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper state*

                    #region shipper country*
                    if (columnNames.Contains("shipper country*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper country*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperCountry"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperCountry"] = dataTableBookingExcelData.Rows[i]["shipper country*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper country*

                    #region shipper postal code*
                    if (columnNames.Contains("shipper postal code*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper postal code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperPostalCode"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperPostalCode"] = dataTableBookingExcelData.Rows[i]["shipper postal code*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper postal code*

                    #region shipper telephone*
                    if (columnNames.Contains("shipper telephone*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper telephone*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperTelephone"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperTelephone"] = dataTableBookingExcelData.Rows[i]["shipper telephone*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper telephone*

                    #region shipper email
                    if (columnNames.Contains("shipper email"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shipper email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ShipperEmail"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ShipperEmail"] = dataTableBookingExcelData.Rows[i]["shipper email"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shipper email

                    #region consignee code
                    if (columnNames.Contains("consignee code"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeCode"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeCode"] = dataTableBookingExcelData.Rows[i]["consignee code"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee code

                    #region consignee name*
                    if (columnNames.Contains("consignee name*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee name*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeName"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeName"] = dataTableBookingExcelData.Rows[i]["consignee name*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion dims*

                    #region consignee address*
                    if (columnNames.Contains("consignee address*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee address*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeAddress"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeAddress"] = dataTableBookingExcelData.Rows[i]["consignee address*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee address*

                    #region consignee city*
                    if (columnNames.Contains("consignee city*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee city*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeCity"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeCity"] = dataTableBookingExcelData.Rows[i]["consignee city*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee city*

                    #region consignee state*
                    if (columnNames.Contains("consignee state*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee state*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeState"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeState"] = dataTableBookingExcelData.Rows[i]["consignee state*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee state*

                    #region consignee country*
                    if (columnNames.Contains("consignee country*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee country*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeCountry"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeCountry"] = dataTableBookingExcelData.Rows[i]["consignee country*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee country*

                    #region consignee postal code*
                    if (columnNames.Contains("consignee postal code*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee postal code*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneePostalCode"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneePostalCode"] = dataTableBookingExcelData.Rows[i]["consignee postal code*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee postal code*

                    #region consignee telephone*
                    if (columnNames.Contains("consignee telephone*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee telephone*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeTelephone"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeTelephone"] = dataTableBookingExcelData.Rows[i]["consignee telephone*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee telephone*

                    #region consignee email
                    if (columnNames.Contains("consignee email"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["consignee email"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ConsigneeEmail"] = DBNull.Value;
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ConsigneeEmail"] = dataTableBookingExcelData.Rows[i]["consignee email"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion consignee email

                    #region eori number
                    if (columnNames.Contains("eori number"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["eori number"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["EORINumber"] = "";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["EORINumber"] = dataTableBookingExcelData.Rows[i]["eori number"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion eori number

                    #region product type*
                    if (columnNames.Contains("product type*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["product type*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["ProductType"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["ProductType"] = dataTableBookingExcelData.Rows[i]["product type*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion product type*

                    #region payment mode*
                    if (columnNames.Contains("payment mode*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["payment mode*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["PaymentMode"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["PaymentMode"] = dataTableBookingExcelData.Rows[i]["payment mode*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion payment mode*

                    #region shc*
                    if (columnNames.Contains("shc*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["shc*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["SHC"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["SHC"] = dataTableBookingExcelData.Rows[i]["shc*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion shc*

                    #region flight date*
                    if (columnNames.Contains("flight date*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["flight date*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["FlightDate"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["FlightDate"] = dataTableBookingExcelData.Rows[i]["flight date*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion flight date*

                    #region flight number*
                    if (columnNames.Contains("flight number*"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["flight number*"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["FlightNumber"] = DBNull.Value;
                            validationDetailsBooking = validationDetailsBooking + "AWB Number is required;";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["FlightNumber"] = dataTableBookingExcelData.Rows[i]["flight number*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion flight number*

                    #region allotment code
                    if (columnNames.Contains("allotment code"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["allotment code"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["AllotmentCode"] = "";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["AllotmentCode"] = dataTableBookingExcelData.Rows[i]["allotment code"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion allotment code

                    #region handling info
                    if (columnNames.Contains("handling info"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["handling info"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["HandlingInfo"] = "";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["HandlingInfo"] = dataTableBookingExcelData.Rows[i]["handling info"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion handling info

                    #region dv for carriage
                    if (columnNames.Contains("dv for carriage"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["dv for carriage"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["DVForCarriage"] = "0";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["DVForCarriage"] = dataTableBookingExcelData.Rows[i]["dv for carriage"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion dv for carriage

                    #region dv for customs
                    if (columnNames.Contains("dv for customs"))
                    {
                        if (dataTableBookingExcelData.Rows[i]["dv for customs"].ToString().Trim().Trim(',').Equals(string.Empty))
                        {
                            dataRowPartnerScheduleType["DVForCustoms"] = "0";
                        }
                        else
                        {
                            dataRowPartnerScheduleType["DVForCustoms"] = dataTableBookingExcelData.Rows[i]["dv for customs"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion dv for customs

                    dataRowPartnerScheduleType["ValidationDetails"] = validationDetailsBooking;
                    
                    BookingDetailsType.Rows.Add(dataRowPartnerScheduleType);
                    #endregion Create row for Booking Data Table
                }

                //AddBookingDetailsToQueue(srNotblMasterUploadSummaryLog, BookingDetailsType, userName);
                return true;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError($"Message: {exception.Message} Stack Trace: {exception.StackTrace}");
                return false;
            }
            finally
            {
                dataTableBookingExcelData = null;
            }
        }

        //private void AddBookingDetailsToQueue(int srNotblMasterUploadSummaryLog, DataTable bookingDetailsType, string userName)
        //{
        //    try
        //    {
        //        clsLog.WriteLogAzure("AddBookingDetailsToQueue: " + srNotblMasterUploadSummaryLog.ToString());
        //        if (bookingDetailsType != null && bookingDetailsType.Rows.Count > 0)
        //        {
        //            // Your Azure Queue connection string
        //            string connectionString = "DefaultEndpointsProtocol=https;AccountName=djdevtestfilestrgerp;AccountKey=P9j5fScFqDVAfcZvQQIZEQZ8Xc+3JR+wlbW/K5MjWgj/w1mPsCEWFD90Rb7ZJPfDvD8dN8Rf+kG1+AStjP87rg==;EndpointSuffix=core.windows.net";

        //            // The name of the queue
        //            string queueName = "booking-excelupload-queue";

                    
        //            // Create a QueueClient to send messages
        //            QueueClient queueClient = new QueueClient(connectionString, queueName);
        //            // Ensure the queue exists
        //            //queueClient.CreateIfNotExists();

        //            clsLog.WriteLogAzure("Create a QueueClient to send messages: " + srNotblMasterUploadSummaryLog.ToString());

        //            if (queueClient.Exists())
        //            {
        //                for (int i = 0; i < bookingDetailsType.Rows.Count; i++)
        //                {
        //                    string jsonMessage = ConvertDataRowToJson(bookingDetailsType.Rows[i]);

        //                    clsLog.WriteLogAzure("Add the message to the queue: " + jsonMessage);

        //                    // Add the message to the queue
        //                    queueClient.SendMessage(jsonMessage);

        //                    clsLog.WriteLogAzure("Added message to the queue: " + srNotblMasterUploadSummaryLog.ToString());
        //                    Console.WriteLine("Message added to the queue successfully!");


        //                }
        //            }
        //            else
        //            {
        //                Console.WriteLine("Queue does not exist.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        //private void AddBookingDetailsToQueue(int srNotblMasterUploadSummaryLog, DataTable bookingDetailsType)
        //{
        //    try
        //    {
        //        if (bookingDetailsType != null && bookingDetailsType.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < bookingDetailsType.Rows.Count; i++)
        //            {
        //                string jsonMessage = ConvertDataRowToJson(bookingDetailsType.Rows[i]);

        //                // Your Azure Queue connection string
        //                string connectionString = "<YourAzureStorageConnectionString>";

        //                // The name of the queue
        //                string queueName = "booking-excelupload-queue";

        //                // Create a QueueClient to send messages
        //                QueueClient queueClient = new QueueClient(connectionString, queueName);

        //                // Ensure the queue exists
        //                queueClient.CreateIfNotExists();

        //                if (queueClient.Exists())
        //                {
        //                    // Add the message to the queue
        //                    queueClient.SendMessage(jsonMessage);
        //                    Console.WriteLine("Message added to the queue successfully!");
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Queue does not exist.");
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        public static string ConvertDataRowToJson(DataRow row)
        {
            string jsonString = string.Empty;
            try
            {
                // Convert DataRow to dictionary
                var rowDict = row.Table.Columns.Cast<DataColumn>()
                                   .ToDictionary(col => col.ColumnName, col => row[col]);

                // Serialize dictionary to JSON string
                jsonString = JsonConvert.SerializeObject(rowDict, Formatting.Indented);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}"); 
            }
            return jsonString;
        }

        public async Task<DataSet?> ValidateAndInsertPartnerSchedule(int srNotblMasterUploadSummaryLog, DataTable partnerScheduleType, string errorInSp)
        {
            DataSet? dataSetResult = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[] {
                                                                      new SqlParameter("@SrNotblMasterUploadSummaryLog",srNotblMasterUploadSummaryLog),
                                                                      new SqlParameter("@PartnerScheduleType", partnerScheduleType),
                                                                      new SqlParameter("@Error", errorInSp)
                                                                  };

                //SQLServer sQLServer = new SQLServer();
                //dataSetResult = sQLServer.SelectRecords("uspUploadPartnerSchedule", sqlParameters);
                dataSetResult = await _readWriteDao.SelectRecords("uspUploadPartnerSchedule", sqlParameters);

                return dataSetResult;
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Message: " + exception.Message + " Stack Trace: " + exception.StackTrace);
                _logger.LogError($"Message: {exception.Message} Stack Trace: {exception.StackTrace}");
                return dataSetResult;
            }
        }
    }
}
