using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Services;
using System.Data;

namespace QidWorkerRole.UploadMasters.FlightCapacity
{
    public class FlightCapacity
    {
        private readonly ILogger<FlightCapacity> _logger;
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;


        #region Constructor
        public FlightCapacity(
            ILogger<FlightCapacity> logger,
            Func<UploadMasterCommon> uploadMasterCommonFactory
         )
        {
            _logger = logger;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion
        public async Task UploadFlightCapacity(DataSet dsFiles)
        {
            try
            {
                //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                //DataSet dsFiles = new DataSet();
                //dsFiles = uploadMasterCommon.GetUploadedFileData(UploadMasterType.FlightCapacity);
                string FilePath = "";

                if (dsFiles != null && dsFiles.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsFiles.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommonFactory().UpdateUploadMastersStatus(Convert.ToInt32(dr["SrNo"]), "Process End", 0, 0, 0, 1, "", 1, 1);

                        //UploadMasterCommon umc = new UploadMasterCommon();
                        if (_uploadMasterCommonFactory().DoDownloadBLOB(Convert.ToString(dr["FileName"]), Convert.ToString(dr["ContainerName"]), "CapacityAllocation", out FilePath))
                        {
                            await ProcessFile(Convert.ToInt32(dr["SrNo"]), FilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task ProcessFile(int SerialNumber, string FilePath)
        {
            #region : Variables Declaration :

            //UploadMasterCommon _uploadMasterCommon = new UploadMasterCommon();
            //Cls_BL cls_BL = new Cls_BL();

            //FilePath = "D:\\Project Documents\\Upload Master\\Flight Capacity\\CargoSPOT_capacity12.csv";//To be removed
            DataSet result = new DataSet();
            DataSet FinalRes = new DataSet();
            string res = string.Empty;
            int RecordCount = 0, FailedCount = 0, SuccessCount = 0;
            StreamReader srCSV = new StreamReader(FilePath);
            DataSet dsres = new DataSet();
            DataTable dt = new DataTable();
            DataSet dsSerialNumber = new DataSet();
            string ErrorMessage = "", MasterValue = string.Empty;
            bool IsSuccess = true;
            DateTime UploadStartTime = DateTime.Now;
            string FileName = string.Empty, UserName = string.Empty, Station = string.Empty;

            //GenericFunction genericFunction = new GenericFunction();

            #endregion
            try
            {
                string? str = srCSV.ReadLine();
                string[] arrCsvElement = str.Replace(" ", "").Split(',');

                for (int i = 0; i < arrCsvElement.Length; i++)
                    dt.Columns.Add(new DataColumn(arrCsvElement[i], typeof(string)));

                while ((str = srCSV.ReadLine()) != null)
                {
                    arrCsvElement = str.Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < arrCsvElement.Length; i++)
                        dr[i] = arrCsvElement[i];
                    dt.Rows.Add(dr);
                }
                result.Tables.Add(dt);

                _uploadMasterCommonFactory().RemoveEmptyRows(result.Tables[0]);

                DataTable dtUploadMasterDetailLog = new DataTable("UploadMasterDetailLog");
                dtUploadMasterDetailLog = CreateUploadMasterDetailLogDataTable();
                FinalRes = CreateCapacityTransactionDataTable(result.Clone());

                for (int i = 0; i < result.Tables[0].Rows.Count; i++)
                {
                    ErrorMessage = string.Empty;
                    string FltOrigDate = string.Empty;

                    RecordCount++;

                    #region : Validate Data :
                    FltOrigDate = (string)(result.Tables[0].Rows[i]["FltOrigDate"].ToString());
                    if (!CanBeCasted(FltOrigDate, "System.DateTime", 1, 0))
                        ErrorMessage = ErrorMessage + " Flt Orig Date is mandatory;";

                    string FltNo = string.Empty;
                    FltNo = (string)(result.Tables[0].Rows[i]["Flt"].ToString());
                    if (!CanBeCasted(FltNo, "", 1, 0))
                        ErrorMessage = ErrorMessage + " FltNo is mandatory;";

                    string Origin = string.Empty;
                    Origin = (string)(result.Tables[0].Rows[i]["Orig"].ToString());
                    if (!CanBeCasted(Origin, "", 1, 0))
                        ErrorMessage = ErrorMessage + " Orig is mandatory;";

                    string Dest = string.Empty;
                    Dest = (string)(result.Tables[0].Rows[i]["Dest"].ToString());
                    if (!CanBeCasted(Dest, "", 1, 0))
                        ErrorMessage = ErrorMessage + " Dest is mandatory;";

                    string ReportWeight = string.Empty;
                    ReportWeight = (string)(result.Tables[0].Rows[i]["ReportWeight"].ToString());
                    if (!CanBeCasted(ReportWeight, "System.float", 0, 0))
                        ErrorMessage = ErrorMessage + "Invalid Report Weight ;";

                    string ReportVolume = string.Empty;
                    ReportVolume = (string)(result.Tables[0].Rows[i]["ReportVolume"].ToString());
                    if (!CanBeCasted(ReportVolume, "System.float", 0, 0))
                        ErrorMessage = ErrorMessage + "Invalid Report Volume ;";

                    string ULDCartforBGE = string.Empty;
                    ULDCartforBGE = (string)(result.Tables[0].Rows[i]["ULD/CartforBGE"].ToString());
                    if (!CanBeCasted(ReportVolume, "System.int", 0, 0))
                        ErrorMessage = ErrorMessage + "Invalid Pax Container;";
                    #endregion

                    #region : Set Success/Fail Count :
                    MasterValue = "FltNo: " + FltNo + "- FltDate: " + FltOrigDate + "- Origin: " + Origin + "- Dest: " + Dest;
                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        result.Tables[0].Rows[i].Delete();
                        result.AcceptChanges();
                        IsSuccess = false;
                        FailedCount++;
                        i--;
                    }
                    else
                    {
                        FinalRes.Tables[0].ImportRow(result.Tables[0].Rows[i]);
                        IsSuccess = true;
                        SuccessCount++;
                    }

                    result.AcceptChanges();
                    #endregion

                    #region : Add Log into Data Table :
                    DataRow drLog = dtUploadMasterDetailLog.NewRow();
                    string[] arrColumnNames = { "UploadSummarySrNo", "MasterKey", "ErrorDescription", "IsSuccess", "UploadedOn" };
                    drLog["UploadSummarySrNo"] = SerialNumber;
                    drLog["MasterKey"] = MasterValue;
                    drLog["ErrorDescription"] = ErrorMessage;
                    drLog["IsSuccess"] = IsSuccess;
                    drLog["UploadedOn"] = UploadStartTime;
                    dtUploadMasterDetailLog.Rows.Add(drLog);
                    #endregion
                }

                FinalRes.Tables[0].EndLoadData();
                FinalRes.Tables[0].AcceptChanges();
                FinalRes.Tables[0].Columns[11].ColumnName = "ULDCartforBGE";

                dsres = await _uploadMasterCommonFactory().InsertCapacityFile(FinalRes.Tables[0], UploadStartTime, UserName);

                #region : Download excel file for reference :
                //string filepath = @ConfigurationManager.AppSettings["DownLoadFilePath"].ToString() + "\\CapacityUploadLog\\" + FileName;
                //string filepath = @Convert.ToString(genericFunction.ReadValueFromDb("DownLoadFilePath")) + "\\CapacityUploadLog\\" + FileName;

                string downLoadFilePath = ConfigCache.Get("DownLoadFilePath");
                string filepath = @Convert.ToString(downLoadFilePath) + "\\CapacityUploadLog\\" + FileName;

                _uploadMasterCommonFactory().ExportDataSet(FinalRes.Tables[0], filepath);
                #endregion

                if (await _uploadMasterCommonFactory().InsertMasterDetailsLog(dtUploadMasterDetailLog, FinalRes.Tables[0], UploadStartTime, UserName))
                {
                    DataSet dsContainerName = await _uploadMasterCommonFactory().GetUploadMasterConfiguration(UploadMasterType.FlightCapacity);
                    string ContainerName = string.Empty;
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();

                    //string BlobName = genericFunction.ReadValueFromDb("BlobStorageName");
                    string BlobName = ConfigCache.Get("BlobStorageName");

                    await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(SerialNumber, RecordCount, SuccessCount, FailedCount, "Process Completed", 1, string.Empty, string.Empty, true);
                    await _uploadMasterCommonFactory().UpdateUploadMastersStatus(SerialNumber, "Process Start", RecordCount, SuccessCount
                        , FailedCount, 1, string.Empty, 1);
                    await _uploadMasterCommonFactory().UpdateUploadMastersStatus(SerialNumber, "Process End", 0, 0, 0, 1, string.Empty, 1);
                }
                else
                    await _uploadMasterCommonFactory().UpdateUploadMasterSummaryLog(SerialNumber, 0, 0, 0, "Process Failed", 1, string.Empty, string.Empty, true);

            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            finally
            {
                RecordCount = 0;
                FailedCount = 0;
                SuccessCount = 0;
                ErrorMessage = string.Empty;
                SerialNumber = 0;
                if (srCSV != null)
                    srCSV.Dispose();
                if (dsSerialNumber != null)
                    dsSerialNumber.Dispose();
            }
        }

        private DataSet CreateCapacityTransactionDataTable(DataSet ClonedDataSet)
        {
            DataSet dsCapacityTransaction = new DataSet();
            try
            {

                if (ClonedDataSet.Tables[0].Columns.Count == 6
                    || ClonedDataSet.Tables[0].Columns.Count == 7 || ClonedDataSet.Tables[0].Columns.Count == 8)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("DataSource", typeof(string));
                    dt.Columns.Add("FltOrigDate", typeof(DateTime));
                    dt.Columns.Add("Flt", typeof(string));
                    dt.Columns.Add("Orig", typeof(string));
                    dt.Columns.Add("Dest", typeof(string));
                    dt.Columns.Add("ReportWeight", typeof(decimal));
                    dt.Columns.Add("ReportVolume", typeof(decimal));
                    dt.Columns.Add("FSW", typeof(string));
                    dt.Columns.Add("OBW", typeof(string));
                    dt.Columns.Add("FSV", typeof(string));
                    dt.Columns.Add("OBV", typeof(string));
                    dt.Columns.Add("ULD/CartforBGE", typeof(string));
                    dsCapacityTransaction.Tables.Add(dt);
                }
                else
                {
                    dsCapacityTransaction = ClonedDataSet;
                    dsCapacityTransaction.Tables[0].Columns[0].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[1].DataType = typeof(DateTime);
                    dsCapacityTransaction.Tables[0].Columns[2].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[3].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[4].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[5].DataType = typeof(decimal);
                    dsCapacityTransaction.Tables[0].Columns[6].DataType = typeof(decimal);
                    dsCapacityTransaction.Tables[0].Columns[7].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[8].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[9].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[10].DataType = typeof(string);
                    dsCapacityTransaction.Tables[0].Columns[11].DataType = typeof(string);
                }

                dsCapacityTransaction.Tables[0].EndInit();
                return dsCapacityTransaction;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        private DataTable CreateUploadMasterDetailLogDataTable()
        {
            DataTable dtUploadMasterDetailLog = new DataTable("UploadMasterDetailLog");
            try
            {
                string[] arrColumnNames = { "UploadSummarySrNo", "MasterKey", "ErrorDescription", "IsSuccess", "UploadedOn" };
                for (int i = 0; i < arrColumnNames.Length; i++)
                {
                    dtUploadMasterDetailLog.Columns.Add(new DataColumn(arrColumnNames[i], typeof(string)));
                }
                return dtUploadMasterDetailLog;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        private bool CanBeCasted(Object value, string ExpectedDataType, int minlen, int maxlen)
        {
            bool canBeConverted = true;
            try
            {
                switch (ExpectedDataType)
                {
                    case "System.int":
                        try
                        {
                            var convertedValue = Convert.ToInt32(value);
                        }
                        catch (Exception)
                        {
                            canBeConverted = false;
                        }
                        break;
                    case "System.float":
                        try
                        {
                            var convertedValue = Convert.ToDouble(value);
                        }
                        catch (Exception)
                        {
                            canBeConverted = false;
                        }
                        break;

                    case "System.decimal":
                        try
                        {

                            var convertedValue = Convert.ToDecimal(value);
                        }
                        catch (Exception)
                        {
                            canBeConverted = false;
                        }
                        break;


                    case "System.DateTime":
                        try
                        {
                            var convertedValue = Convert.ToDateTime(value);
                        }
                        catch (Exception)
                        {
                            canBeConverted = false;
                        }
                        break;
                }
                if (canBeConverted == true)
                {
                    int len = value.ToString().Trim().Length;
                    if (minlen != 0 && len < minlen)
                        return false;
                    else if (maxlen != 0 && len > maxlen)
                        return false;
                }
                return true;
            }

            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }

        }
    }
}
