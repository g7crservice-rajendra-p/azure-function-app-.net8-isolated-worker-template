using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;

namespace QidWorkerRole.UploadMasters.FlightSchedule
{
    public class UploadFlightSchedule
    {
        //UploadMasterCommon _uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadFlightSchedule> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public UploadFlightSchedule(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadFlightSchedule> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;

        }
        #endregion

        /*Not in use*/
        //public void updateFileDirect(string FilePath)
        //{
        //    byte[] downloadStream;
        //    string FileName = string.Empty, carrier = string.Empty;
        //    downloadStream = File.ReadAllBytes(FilePath);
        //    ProcessFile(0, FilePath, FileName, out carrier);
        //}


        public async Task<bool> GetUploadFlightSchedule(DataSet dsFiles)
        {
            try
            {

                //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

                string carrier = string.Empty;
                //DataSet dsFiles = new DataSet();
                //dsFiles = uploadMasterCommon.GetUploadedFileData(UploadMasterType.FlightSchedule);

                if (dsFiles != null && dsFiles.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsFiles.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        string FilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "Schedule", out FilePath))
                        {
                            // clsLog.WriteLogAzure("Schedule File Path:" + FilePath);
                            _logger.LogInformation("Schedule File Path: {filePath}", FilePath);
                            DataSet dsSSIMUpdateResult = new DataSet();

                            if (!await ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath, Convert.ToString(dr["FileName"]), out carrier))
                                return false;

                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Daily Flight Schedule start", 0, 0, 0, 1, "", 1);
                            dsSSIMUpdateResult = await SSIMUpdate(carrier);
                            if (dsSSIMUpdateResult != null && dsSSIMUpdateResult.Tables.Count > 0)
                            {
                                foreach (DataTable dt in dsSSIMUpdateResult.Tables)
                                {
                                    if (dt.Columns.Contains("SuccessStatus") && !Convert.ToBoolean(dsSSIMUpdateResult.Tables[0].Rows[0]["SuccessStatus"].ToString()))
                                        return false;
                                }
                            }

                            await GetRefereshSchedule();
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Daily Flight Schedule End", 0, 0, 0, 1, "", 1);
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1);
                            await RefreshCapacity();
                        }
                        else
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dr["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        private async Task<bool> RefreshCapacity()
        {
            try
            {
                string flightID = string.Empty, source = string.Empty, dest = string.Empty;
                DateTime flightDate = DateTime.UtcNow, toDate = DateTime.UtcNow;
                bool isSSIMUploaded = true;

                //SQLServer sqlServer = new SQLServer();

                SqlParameter[] sqlParameters = [
                    new SqlParameter("FlightID", flightID)
                    , new SqlParameter("FlightDate", flightDate)
                    , new SqlParameter("Source", source)
                    , new SqlParameter("Dest", dest)
                    , new SqlParameter("ToDate", toDate)
                    , new SqlParameter("IsSSIMUploaded", isSSIMUploaded)
                ];
                //sqlServer.SelectRecords("uspRefreshCapacity", sqlParameters);
                await _readWriteDao.SelectRecords("uspRefreshCapacity", sqlParameters);
                return true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;
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

        public async Task<bool> ProcessFile(int MasterLogId, string FilePath, string FileName, out string carrier)
        {
            carrier = string.Empty;
            string creationDateTime = string.Empty;
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

                DataSet? ds = new DataSet();
                FileStream stream = File.Open(FilePath, FileMode.Open, FileAccess.Read);
                stream.Position = 0;
                StreamReader sr = new StreamReader(stream);
                int count = 0;
                string Line = "";
                DataTable dt = GetFlightScheduleTable();

                do
                {
                    Line = sr.ReadLine();

                    if (Line.StartsWith("3"))
                    {
                        count++;
                        dt.Rows.Add(
                                count
                              , Line.Substring(14, 7)
                              , Line.Substring(21, 7)
                              , Line.Substring(2, 3).Trim() + Line.Substring(5, 4).Trim() + Line.Substring(1, 1).Trim()
                              , Line.Substring(36, 3)
                              , Line.Substring(54, 3)
                              , Line.Substring(39, 4)
                              , Line.Substring(57, 4)
                              , Line.Substring(28, 7).Replace(' ', '0').Replace('2', '1').Replace('3', '1').Replace('4', '1').Replace('5', '1').Replace('6', '1').Replace('7', '1') // AirlineFrequency
                              , Line.Substring(172, 20)
                              , Line.Substring(61, 9)
                              , Line.Substring(43, 9)
                              , Line.Substring(2, 3).Trim()
                              , Line.Substring(72, 3)
                              , Line.Substring(192, 1).Trim()
                              , Line.Substring(193, 1).Trim()
                              , Line.Substring(9, 2).Trim()
                              , Line.Substring(11, 2).Trim()
                              , Line.Substring(13, 1).Trim()
                              );
                    }
                    else if (Line.StartsWith("2"))
                    {
                        carrier = Line.Substring(2, 3).Trim();
                        creationDateTime = Line.Substring(28, 7).Trim();
                        creationDateTime = creationDateTime + " " + Line.Substring(190, 2).Trim() + ":" + Line.Substring(192, 2).Trim();
                    }

                } while (sr.Peek() != -1);
                dt.EndInit();
                dt.AcceptChanges();
                sr.Close();

                SqlParameter[] sqlParams = [
                new SqlParameter("@FlightScheduleSSIM", dt) ,
                new SqlParameter("@SrNo", MasterLogId) ,
                new SqlParameter("@SSIMCreationDateTime", creationDateTime)
                ];

                //SQLServer sqlServer = new SQLServer();
                //ds = sqlServer.SelectRecords("uspImportAirlineScheduleFromSSIM", sqlParams);
                ds = await _readWriteDao.SelectRecords("uspImportAirlineScheduleFromSSIM", sqlParams);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Columns.Contains("IsOldSSIM") && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["IsOldSSIM"].ToString().ToUpper() == "TRUE")
                        return false;
                }

                //string filepath = @ConfigurationManager.AppSettings["DownLoadFilePath"].ToString() + "\\SSIMUploadLog\\"+FileName;
                //string filepath = @Convert.ToString(genericFunction.ReadValueFromDb("DownLoadFilePath")) + "\\SSIMUploadLog\\" + FileName;

                string downLoadFilePath = ConfigCache.Get("DownLoadFilePath");
                string filepath = @Convert.ToString(downLoadFilePath) + "\\SSIMUploadLog\\" + FileName;


                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return false;

                _uploadMasterCommon.ExportDataSet(ds, filepath);
            }
            catch (Exception ex)
            {
                carrier = string.Empty;
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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

        //            //clsLog.WriteLogAzure(ex);
        //            CloudBlob blob = blobClient.GetBlobReference(str);
        //            downloadStream = blob.DownloadByteArray();
        //        }

        //        return downloadStream;

        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
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
        //        //clsLog.WriteLogAzure(ex);
        //        return false;
        //    }

        //}

        private async Task<bool> GetRefereshSchedule()
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //sqlServer.SelectRecords("uspRefreshAirlineScheduleRouteForecast", 900);
                await _readWriteDao.SelectRecords("uspRefreshAirlineScheduleRouteForecast", commandTimeout: 900);
                return true;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;

        }

        public async Task<DataSet> SSIMUpdate(string carrier)
        {
            DataSet? dsResult = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();

                SqlParameter[] sqlParameter = [
                    new  SqlParameter("@Carrier", carrier)
                ];
                //dsResult = sqlServer.SelectRecords("sp_UpdateDatesSSIM", sqlParameter);
                dsResult = await _readWriteDao.SelectRecords("sp_UpdateDatesSSIM", sqlParameter);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsResult;
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
