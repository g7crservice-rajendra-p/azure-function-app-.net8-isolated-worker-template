using Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;

namespace QidWorkerRole.UploadMasters.FlightScheduleExcel
{
    public class UploadFlightScheduleExcel
    {
        //UploadMasterCommon _uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadFlightScheduleExcel> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadFlightScheduleExcel(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadFlightScheduleExcel> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;

        }
        #endregion
        public async Task<bool> GetUploadFlightSchedule(DataSet dataSetFileData)
        {
            if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                {
                    // to upadate retry count only.
                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                    string FilePath = "";
                    if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]), "FlightScheduleExcel", out FilePath))
                    {
                        await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), FilePath, Convert.ToString(dataRowFileData["FileName"]));
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Daily Flight Schedule start", 0, 0, 0, 1, "", 1);
                        await SSIMUpdate();
                    }
                    else
                    {
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                        await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                    }
                    await GetRefereshSchedule();
                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Daily Flight Schedule End", 0, 0, 0, 1, "", 1);
                    await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                }
            }
            return true;
        }

        public DataTable GetFlightScheduleTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("RowID", typeof(string));
            dt.Columns.Add("FromDate", typeof(string));
            dt.Columns.Add("ToDate", typeof(string));
            dt.Columns.Add("FlightID", typeof(string));
            dt.Columns.Add("Source", typeof(string));
            dt.Columns.Add("Dest", typeof(string));
            dt.Columns.Add("ScheduleDepttime", typeof(string));
            dt.Columns.Add("SchArrtime", typeof(string));
            dt.Columns.Add("frequency", typeof(string));
            dt.Columns.Add("EquipmentNo", typeof(string));
            dt.Columns.Add("ArrTimeZone", typeof(string));
            dt.Columns.Add("DeptTimeZone", typeof(string));
            dt.Columns.Add("FlightPrefix", typeof(string));
            dt.Columns.Add("AircraftType", typeof(string));
            dt.Columns.Add("UTCDeptDay", typeof(string));
            dt.Columns.Add("UTCArrDay", typeof(string));
            dt.Columns.Add("Itinerary", typeof(string));
            dt.Columns.Add("LegSeqNo", typeof(string));
            dt.Columns.Add("FlightType", typeof(string));

            return dt;
        }

        public async Task<bool> ProcessFile(int MasterLogId, string FilePath, string FileName)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

                DataTable dataTableFlightScheduleExcelData = new DataTable("dataTableFlightScheduleExcelData");
                DataSet? ds = new DataSet();
                bool isBinaryReader = false;

                FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read);

                IExcelDataReader iExcelDataReader = null;
                string fileExtention = Path.GetExtension(FilePath).ToLower();

                isBinaryReader = fileExtention.Equals(".xls") || fileExtention.Equals(".XLS") ? true : false;

                iExcelDataReader = isBinaryReader ? ExcelReaderFactory.CreateBinaryReader(fileStream) // for Reading from a binary Excel file ('97-2003 format; *.xls)
                                                  : ExcelReaderFactory.CreateOpenXmlReader(fileStream); // for Reading from a OpenXml Excel file (2007 format; *.xlsx)

                // DataSet - Create column names from first row
                iExcelDataReader.IsFirstRowAsColumnNames = true;
                dataTableFlightScheduleExcelData = iExcelDataReader.AsDataSet().Tables[0];

                // Free resources (IExcelDataReader is IDisposable)
                iExcelDataReader.Close();

                _uploadMasterCommon.RemoveEmptyRows(dataTableFlightScheduleExcelData);

                foreach (DataColumn dataColumn in dataTableFlightScheduleExcelData.Columns)
                {
                    dataColumn.ColumnName = dataColumn.ColumnName.ToLower().Trim();
                }
                string[] columnNames = dataTableFlightScheduleExcelData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                #region Creating FlightSchedule Master DataTable
                DataTable FlightScheduleType = GetFlightScheduleTable();
                #endregion FlightSchedule Master

                string validationDetailsFlightSchedule = string.Empty;
                DateTime tempDate;

                for (int i = 0; i < dataTableFlightScheduleExcelData.Rows.Count; i++)
                {
                    validationDetailsFlightSchedule = string.Empty;

                    if (String.IsNullOrEmpty(dataTableFlightScheduleExcelData.Rows[i]["from date*"].ToString()) || String.IsNullOrEmpty(dataTableFlightScheduleExcelData.Rows[i]["to date*"].ToString()))
                        break;

                    #region Create row for FlightScheduleType Data Table
                    DataRow dataRowFlightScheduleType = FlightScheduleType.NewRow();

                    dataRowFlightScheduleType["RowID"] = i + 1;

                    #region[FromtDt] [datetime] NULL

                    if (columnNames.Contains("from date*"))
                    {

                        try
                        {
                            //dataRowFlightScheduleType["FromDate"]=dataTableFlightScheduleExcelData.Rows[i]["from date*"].ToString();
                            if (isBinaryReader)
                            {
                                dataRowFlightScheduleType["FromDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableFlightScheduleExcelData.Rows[i]["from date*"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableFlightScheduleExcelData.Rows[i]["from date*"].ToString().Trim(), out tempDate))
                                {
                                    dataRowFlightScheduleType["FromDate"] = tempDate.ToString("MM-dd-yyyy");
                                }
                                else
                                {
                                    dataRowFlightScheduleType["FromDate"] = DBNull.Value;
                                    validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Invalid From Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowFlightScheduleType["FromtDt"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Invalid From Date;";
                        }
                    }

                    #endregion

                    #region[ToDt] [datetime] NULL

                    if (columnNames.Contains("to date*"))
                    {
                        try
                        {
                            //dataRowFlightScheduleType["ToDate"] = dataTableFlightScheduleExcelData.Rows[i]["to date*"].ToString();

                            if (isBinaryReader)
                            {
                                dataRowFlightScheduleType["ToDate"] = DateTime.FromOADate(Convert.ToDouble(dataTableFlightScheduleExcelData.Rows[i]["to date*"].ToString().Trim()));
                            }
                            else
                            {
                                if (DateTime.TryParse(dataTableFlightScheduleExcelData.Rows[i]["to date*"].ToString().Trim(), out tempDate))
                                {
                                    dataRowFlightScheduleType["ToDate"] = tempDate.ToString("MM-dd-yyyy");
                                }
                                else
                                {
                                    dataRowFlightScheduleType["ToDate"] = DBNull.Value;
                                    validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Invalid To Date;";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            dataRowFlightScheduleType["ToDt"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Invalid To Date;";
                        }
                    }

                    #endregion
                    #region[FlightID] [varchar] (10)  NULL
                    string FlightId = string.Empty;
                    if (columnNames.Contains("flight id*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["flight id*"].ToString().Trim().Trim(',').Length > 10)
                        {
                            dataRowFlightScheduleType["FlightID"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "FlightNo is more than 10 Chars;";
                        }
                        else
                        {
                            FlightId = dataTableFlightScheduleExcelData.Rows[i]["flight id*"].ToString().Trim().Trim(',');
                            dataRowFlightScheduleType["FlightID"] = FlightId;
                        }

                    }
                    #endregion

                    #region[Source] [varchar] (5)  NULL
                    if (columnNames.Contains("source*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["source*"].ToString().Trim().Trim(',').Length > 5)
                        {
                            dataRowFlightScheduleType["Source"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Source is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["Source"] = dataTableFlightScheduleExcelData.Rows[i]["source*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region[Dest] [varchar] (5)  NULL
                    if (columnNames.Contains("dest*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["dest*"].ToString().Trim().Trim(',').Length > 5)
                        {
                            dataRowFlightScheduleType["Dest"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Destination is more than 5 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["Dest"] = dataTableFlightScheduleExcelData.Rows[i]["dest*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region[SchDeptTime] [varchar] (15)  NULL
                    if (columnNames.Contains("schedule dept time*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["schedule dept time*"].ToString().Trim().Trim(',').Length > 15)
                        {
                            dataRowFlightScheduleType["ScheduleDepttime"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Deptarture Time is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["ScheduleDepttime"] = dataTableFlightScheduleExcelData.Rows[i]["schedule dept time*"].ToString().Trim().Trim(',').PadLeft(4, '0');
                        }
                    }
                    #endregion

                    #region[SchArrTime] [varchar] (15)  NULL
                    if (columnNames.Contains("sch arr time*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["sch arr time*"].ToString().Trim().Trim(',').Length > 15)
                        {
                            dataRowFlightScheduleType["SchArrtime"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Arrival Time is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["SchArrtime"] = dataTableFlightScheduleExcelData.Rows[i]["sch arr time*"].ToString().Trim().Trim(',').PadLeft(4, '0');
                        }
                    }
                    #endregion

                    #region[Frequency] [varchar] (15)  NULL
                    if (columnNames.Contains("frequency*"))
                    {
                        if (dataTableFlightScheduleExcelData.Rows[i]["frequency*"].ToString().Trim().Trim(',').Length > 15)
                        {
                            dataRowFlightScheduleType["frequency"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Frequency is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["frequency"] = dataTableFlightScheduleExcelData.Rows[i]["frequency*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion


                    #region[EquipmentNo] [varchar] (15)  NULL

                    if (columnNames.Contains("equipment no*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["equipment no*"].ToString().Trim().Trim(',').Length > 15)
                        {
                            dataRowFlightScheduleType["EquipmentNo"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "EquipmentNo is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["EquipmentNo"] = dataTableFlightScheduleExcelData.Rows[i]["equipment no*"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion

                    #region[DeptTimeZone] [varchar] (10)  NULL
                    if (columnNames.Contains("dept time zone*"))
                    {
                        if (dataTableFlightScheduleExcelData.Rows[i]["dept time zone*"].ToString().Trim().Trim(',').Length > 10)
                        {
                            dataRowFlightScheduleType["DeptTimeZone"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "DeptTimeZone is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["DeptTimeZone"] = dataTableFlightScheduleExcelData.Rows[i]["dept time zone*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region[ArrTimeZone] [varchar] (10)  NULL
                    if (columnNames.Contains("arr time zone*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["arr time zone*"].ToString().Trim().Trim(',').Length > 10)
                        {
                            dataRowFlightScheduleType["ArrTimeZone"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "ArrTimeZone is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["ArrTimeZone"] = dataTableFlightScheduleExcelData.Rows[i]["arr time zone*"].ToString().Trim().Trim(',');
                        }
                    }
                    #endregion

                    #region[FlightPrefix] [varchar] (3)  NULL
                    if (columnNames.Contains("flight id*"))
                    {
                        if (dataTableFlightScheduleExcelData.Rows[i]["flight id*"].ToString().Trim().Substring(0, 2).Trim(',').Length > 3)
                        {
                            dataRowFlightScheduleType["FlightPrefix"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Flight ID* is more than 3 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["FlightPrefix"] = dataTableFlightScheduleExcelData.Rows[i]["flight id*"].ToString().Trim().Substring(0, 2).Trim(',');
                        }
                    }
                    #endregion
                    #region[AircraftType] [varchar](15)  Null
                    if (columnNames.Contains("aircraft type*"))
                    {
                        if (dataTableFlightScheduleExcelData.Rows[i]["aircraft type*"].ToString().Trim().Trim(',').Length > 15)
                        {
                            dataRowFlightScheduleType["AircraftType"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "AircraftType is more than 15 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["AircraftType"] = dataTableFlightScheduleExcelData.Rows[i]["aircraft type*"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion
                    #region[UTC Dept Day] [varchar](3) Not Null
                    if (columnNames.Contains("utc dept day*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["utc dept day*"].ToString().Trim().Trim(',').Length > 3)
                        {
                            dataRowFlightScheduleType["UTCDeptDay"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "UTC Dept Day is more than 3 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["UTCDeptDay"] = dataTableFlightScheduleExcelData.Rows[i]["utc dept day*"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion

                    #region[UTCArrDay] [varchar](3) Not Null
                    if (columnNames.Contains("utc arr day*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["utc arr day*"].ToString().Trim().Trim(',').Length > 3)
                        {
                            dataRowFlightScheduleType["UTCArrDay"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "UTCArrDay is more than 3 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["UTCArrDay"] = dataTableFlightScheduleExcelData.Rows[i]["utc arr day*"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion



                    #region[Itinerary] [varchar](20)  Null
                    if (columnNames.Contains("itinerary"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["itinerary"].ToString().Trim().Trim(',').Length > 10)
                        {
                            dataRowFlightScheduleType["Itinerary"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "Itinerary is more than 10 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["Itinerary"] = dataTableFlightScheduleExcelData.Rows[i]["itinerary"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion

                    #region[Leg Seq No] [varchar](20)  Null
                    if (columnNames.Contains("leg seq no*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["leg seq no*"].ToString().Trim().Trim(',').Length > 10)
                        {
                            dataRowFlightScheduleType["LegSeqNo"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "leg seq no* is more than 1 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["LegSeqNo"] = dataTableFlightScheduleExcelData.Rows[i]["leg seq no*"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion
                    #region[Flight Type*] [varchar](20)  Null
                    if (columnNames.Contains("flight type*"))
                    {

                        if (dataTableFlightScheduleExcelData.Rows[i]["flight type*"].ToString().Trim().Trim(',').Length > 10)
                        {
                            dataRowFlightScheduleType["FlightType"] = DBNull.Value;
                            validationDetailsFlightSchedule = validationDetailsFlightSchedule + "leg seq no* is more than 1 Chars;";
                        }
                        else
                        {
                            dataRowFlightScheduleType["FlightType"] = dataTableFlightScheduleExcelData.Rows[i]["flight type*"].ToString().Trim().Trim(',');
                        }
                    }

                    #endregion
                    #endregion Create row for PartnerScheduleType Data Table
                    FlightScheduleType.Rows.Add(dataRowFlightScheduleType);
                }


                SqlParameter[] sqlParams = [
                new SqlParameter("@FlightScheduleSSIM", FlightScheduleType) ,
                new SqlParameter("@SrNo", MasterLogId),
                new SqlParameter("@UploadType", "Excel"),
                ];

                //SQLServer sqlServer = new SQLServer();
                //ds = sqlServer.SelectRecords("uspImportAirlineScheduleFromSSIM", sqlParams);
                ds = await _readWriteDao.SelectRecords("uspImportAirlineScheduleFromSSIM", sqlParams);

                //string filepath = @ConfigurationManager.AppSettings["DownLoadFilePath"].ToString() + "\\SSIMUploadLog\\"+FileName;
                //string filepath = @Convert.ToString(genericFunction.ReadValueFromDb("DownLoadFilePath")) + "\\SSIMUploadLog\\" + FileName;

                string downloadFilePath = ConfigCache.Get("DownLoadFilePath");
                string filepath = @Convert.ToString(downloadFilePath) + "\\SSIMUploadLog\\" + FileName;
                _uploadMasterCommon.ExportDataSet(ds, filepath);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return false;
            }
            return true;
        }

        /*Not in use*/
        //public byte[] DownloadFromBlob(string filenameOrUrl)
        //{
        //    try
        //    {
        //        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
        //        string containerName = "";
        //        string str = filenameOrUrl;
        //        if (filenameOrUrl.Contains('/'))
        //        {
        //            filenameOrUrl = filenameOrUrl.ToLower();
        //            containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
        //            filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
        //        }
        //        byte[] downloadStream = null;
        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(uploadMasterCommon.getStorageName(), uploadMasterCommon.getStorageKey());
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        try
        //        {
        //            CloudBlob blob = blobClient.GetBlobReference(string.Format("{0}/{1}", containerName, filenameOrUrl));
        //            downloadStream = blob.DownloadByteArray();
        //        }
        //        catch (Exception ex)
        //        {

        //            clsLog.WriteLogAzure(ex);
        //            CloudBlob blob = blobClient.GetBlobReference(str);
        //            downloadStream = blob.DownloadByteArray();
        //        }

        //        return downloadStream;

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return null;
        //    }

        //}

        /*Not in use*/
        //public bool UploadBlob(Stream stream, string fileName, string containerName)
        //{
        //    try
        //    {
        //        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(uploadMasterCommon.getStorageName(), uploadMasterCommon.getStorageKey());// "NUro8/C7+kMqtwOwLbe6agUvA83s+8xSTBqrkMwSjPP6MAxVkdtsLDGjyfyEqQIPv6JHEEf5F5s4a+DFPsSQfg==");
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
        //        blobContainer.CreateIfNotExist();
        //        CloudBlob blob = blobContainer.GetBlobReference(fileName);
        //        blob.Properties.ContentType = "";
        //        blob.UploadFromStream(stream);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return false;
        //    }

        //}

        private async Task<bool> GetRefereshSchedule()
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //sqlServer.SelectRecords("uspRefreshAirlineScheduleRouteForecast", 900);
                await _readWriteDao.SelectRecords("uspRefreshAirlineScheduleRouteForecast", null, 900);
                return true;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return false;

        }

        public async Task SSIMUpdate()
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //sqlServer.SelectRecords("uspRefreshScheduleForSSIS", 900);
                await _readWriteDao.SelectRecords("uspRefreshScheduleForSSIS", null, 900);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        /*Not in use*/
        //private void ExportDataSet(DataSet ds, string destination)
        //{
        //    using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
        //    {
        //        var workbookPart = workbook.AddWorkbookPart();

        //        workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

        //        workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

        //        foreach (System.Data.DataTable table in ds.Tables)
        //        {

        //            var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
        //            var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
        //            sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

        //            DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
        //            string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

        //            uint sheetId = 1;
        //            if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
        //            {
        //                sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
        //            }

        //            DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
        //            sheets.Append(sheet);

        //            DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

        //            List<String> columns = new List<string>();
        //            foreach (System.Data.DataColumn column in table.Columns)
        //            {
        //                columns.Add(column.ColumnName);

        //                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
        //                headerRow.AppendChild(cell);
        //            }


        //            sheetData.AppendChild(headerRow);

        //            foreach (System.Data.DataRow dsrow in table.Rows)
        //            {
        //                DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
        //                foreach (String col in columns)
        //                {
        //                    DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
        //                    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
        //                    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
        //                    newRow.AppendChild(cell);
        //                }
        //                sheetData.AppendChild(newRow);
        //            }
        //        }
        //    }
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //}

    }
}
