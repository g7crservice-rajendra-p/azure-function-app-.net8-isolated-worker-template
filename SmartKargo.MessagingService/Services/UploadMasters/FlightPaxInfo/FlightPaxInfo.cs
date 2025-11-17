using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;

namespace QidWorkerRole.UploadMasters.FlightPaxInfo
{
    public class FlightPaxInfo
    {
        //UploadMasterCommon _uploadMasterCommon = new UploadMasterCommon();

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FlightPaxInfo> _logger;
        private readonly UploadMasterCommon _uploadMasterCommon;

        #region Constructor
        public FlightPaxInfo(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FlightPaxInfo> logger,
            UploadMasterCommon uploadMasterCommon)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _uploadMasterCommon = uploadMasterCommon;

        }
        #endregion

        /// <summary>
        /// Method to Uplaod ScedulePaxData Master.
        /// </summary>
        /// <returns> True when Success and False when Fails </returns>
        public async Task<bool> PaxMasterUpload(DataSet dataSetFileData, string uploadType)
        {
            try
            {
                string messageType = uploadType.ToUpper() == UploadMasterType.FlightPaxForecast.ToUpper() ? MessageData.MessageTypeName.FLIGHTPAXFORECASTUPLOAD : MessageData.MessageTypeName.FLIGHTPAXINFORMATIONUPLOAD;
                string folderName = uploadType.ToUpper() == UploadMasterType.FlightPaxForecast.ToUpper() ? uploadType.Replace(" ", "") : "FlightPaxInfo";
                if (dataSetFileData != null && dataSetFileData.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dataRowFileData in dataSetFileData.Tables[0].Rows)
                    {
                        // to upadate retry count only.
                        await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Update Retry Count", 0, 0, 0, 1, "", 1, 1);

                        string uploadFilePath = "";
                        if (_uploadMasterCommon.DoDownloadBLOB(Convert.ToString(dataRowFileData["FileName"]), Convert.ToString(dataRowFileData["ContainerName"]),
                                                              folderName, out uploadFilePath))
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 1, "", 1);
                            await ProcessFile(Convert.ToInt32(dataRowFileData["SrNo"]), uploadFilePath, messageType);
                        }
                        else
                        {
                            await _uploadMasterCommon.UpdateUploadMastersStatus(Convert.ToInt32(dataRowFileData["SrNo"]), "Process Start", 0, 0, 0, 0, "File Not Found!", 1);
                            await _uploadMasterCommon.UpdateUploadMasterSummaryLog(Convert.ToInt32(dataRowFileData["SrNo"]), 0, 0, 0, "Process Failed", 0, "W", "File Not Found!", true);
                        }
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

        private async Task ProcessFile(int srNo, string filePath, string messageType)
        {
            try
            {
                string[] PaxInfo = null;
                string[] lines = System.IO.File.ReadAllLines(filePath);
                string filename = Path.GetFileName(filePath);
                int recordCount = lines.Length;
                int successCount = 0;
                int failedCount = 0;
                DataTable dtFlightPaxInfo = CreatePaxDataTable();
                await _uploadMasterCommon.UpdateUploadMastersStatus(srNo, "Process Start", recordCount, successCount, failedCount, 1, string.Empty, 1);
                for (int i = 0; i < lines.Length; i++)
                {
                    DataRow paxDataRow = dtFlightPaxInfo.NewRow();
                    PaxInfo = lines[i].Trim().Split(',');
                    if (messageType == MessageData.MessageTypeName.FLIGHTPAXFORECASTUPLOAD)
                    {
                        if (PaxInfo.Length == 11 && i != 0)///Flight Pax Forecast
                        {
                            DateTime flightDate = new DateTime(Convert.ToInt32(PaxInfo[6].Substring(0, 4)), Convert.ToInt32(PaxInfo[6].Substring(5, 2)),
                            Convert.ToInt32(PaxInfo[6].Substring(8, 2)));
                            paxDataRow["SerialNumber"] = i + 1;
                            paxDataRow["DepartureDate"] = flightDate;
                            paxDataRow["Origin"] = PaxInfo[10].Trim();
                            paxDataRow["CarrierCode"] = PaxInfo[3].Trim();
                            int flightCode = 0;
                            if (int.TryParse(PaxInfo[7].Trim(), out flightCode))
                                PaxInfo[7] = flightCode.ToString().Length < 3 ? flightCode.ToString().PadLeft(3, '0') : flightCode.ToString();
                            paxDataRow["FlightID"] = PaxInfo[3].Trim() + PaxInfo[7].Trim();
                            paxDataRow["ExpectedInfants"] = "0";
                            paxDataRow["ExpectedAdults"] = PaxInfo[8].Trim() == string.Empty ? "0" : PaxInfo[8].Trim();
                            paxDataRow["ExpectedChild"] = "0";
                            paxDataRow["ExpectedTotalPax"] = PaxInfo[8].Trim() == string.Empty ? "0" : PaxInfo[8].Trim();
                            paxDataRow["ActualInfants"] = "0";
                            paxDataRow["ActualAdults"] = PaxInfo[8].Trim() == string.Empty ? "0" : PaxInfo[8].Trim();
                            paxDataRow["ActualChild"] = "0";
                            paxDataRow["ActualTotalPax"] = PaxInfo[8].Trim() == string.Empty ? "0" : PaxInfo[8].Trim();
                            paxDataRow["ExpectedBaggage"] = "0";
                            paxDataRow["ActualBaggage"] = "0";
                            paxDataRow["AircraftCode"] = PaxInfo[0].Trim();
                            paxDataRow["AircraftRegistration"] = PaxInfo[1].Trim();
                            paxDataRow["AircraftType"] = PaxInfo[2].Trim();
                            paxDataRow["DepartureTime"] = PaxInfo[4].Trim();
                            paxDataRow["Destination"] = PaxInfo[5].Trim();
                            paxDataRow["GroupBooked"] = PaxInfo[9].Trim() == string.Empty ? "0" : PaxInfo[9].Trim();
                            paxDataRow["FileName"] = filename;
                            paxDataRow["UpdatedBy"] = "Pax Upload";
                            paxDataRow["UpdatedOn"] = DateTime.UtcNow;
                            dtFlightPaxInfo.Rows.Add(paxDataRow);
                        }
                    }
                    else if (PaxInfo.Length == 27)///Flight Pax Information
                    {
                        DateTime dtDeparture = new DateTime(Convert.ToInt32(PaxInfo[0].Substring(0, 4)), Convert.ToInt32(PaxInfo[0].Substring(4, 2)),
                        Convert.ToInt32(PaxInfo[0].Substring(6, 2)));

                        paxDataRow["SerialNumber"] = i + 1;
                        paxDataRow["DepartureDate"] = dtDeparture;
                        paxDataRow["Origin"] = PaxInfo[4].Trim();
                        paxDataRow["CarrierCode"] = PaxInfo[1].Trim();
                        paxDataRow["FlightID"] = PaxInfo[1].Trim() + PaxInfo[2].Trim();
                        paxDataRow["ExpectedInfants"] = PaxInfo[9].Trim() == string.Empty ? "0" : PaxInfo[9].Trim();
                        paxDataRow["ExpectedAdults"] = PaxInfo[11].Trim() == string.Empty ? "0" : PaxInfo[11].Trim();
                        paxDataRow["ExpectedChild"] = PaxInfo[12].Trim() == string.Empty ? "0" : PaxInfo[12].Trim();
                        paxDataRow["ExpectedTotalPax"] = PaxInfo[5].Trim() == string.Empty ? "0" : PaxInfo[5].Trim();
                        paxDataRow["ActualInfants"] = PaxInfo[19].Trim() == string.Empty ? "0" : PaxInfo[19].Trim();
                        paxDataRow["ActualAdults"] = PaxInfo[21].Trim() == string.Empty ? "0" : PaxInfo[21].Trim();
                        paxDataRow["ActualChild"] = PaxInfo[22].Trim() == string.Empty ? "0" : PaxInfo[22].Trim();
                        paxDataRow["ActualTotalPax"] = PaxInfo[15].Trim() == string.Empty ? "0" : PaxInfo[15].Trim();
                        paxDataRow["UpdatedOn"] = DateTime.UtcNow;
                        paxDataRow["UpdatedBy"] = "Pax Upload";
                        paxDataRow["FileName"] = filename;
                        paxDataRow["ExpectedBaggage"] = PaxInfo[25].Trim() == string.Empty ? "0" : PaxInfo[25].Trim();
                        paxDataRow["ActualBaggage"] = PaxInfo[26].Trim() == string.Empty ? "0" : PaxInfo[26].Trim();
                        dtFlightPaxInfo.Rows.Add(paxDataRow);
                    }
                }
                if (dtFlightPaxInfo.Rows.Count > 0)
                {
                    SqlParameter[] sqlParams = [
                        new SqlParameter("@FlightPacInformation", dtFlightPaxInfo)
                        , new SqlParameter("@RecordCount", recordCount)
                        , new SqlParameter("@SrNo", srNo)
                    ];

                    //SQLServer sqlServer = new SQLServer();
                    //dsPaxResult = sqlServer.SelectRecords("uspSaveFlightPaxInformation", sqlParams);

                    DataSet? dsPaxResult = new DataSet();
                    dsPaxResult = await _readWriteDao.SelectRecords("uspSaveFlightPaxInformation", sqlParams);
                }
                else
                {
                    clsLog.WriteLogAzure(filename + ": PAX file is not processed due to fileds count mismatched filed count should be 27 in text file");
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        /*Not in use*/
        //public bool SaveFlightPaxInformation(object[] paxdata)
        //{
        //    bool isInsert;
        //    try
        //    {
        //        string[] paramNames = new string[15];
        //        SqlDbType[] dataTypes = new SqlDbType[15];
        //        int i = 0;

        //        //0
        //        paramNames.SetValue("DepartureDate", i);
        //        dataTypes.SetValue(SqlDbType.DateTime, i);
        //        i++;

        //        //1
        //        paramNames.SetValue("Origin", i);
        //        dataTypes.SetValue(SqlDbType.VarChar, i);
        //        i++;

        //        //2
        //        paramNames.SetValue("CarrierCode", i);
        //        dataTypes.SetValue(SqlDbType.VarChar, i);
        //        i++;

        //        //3
        //        paramNames.SetValue("FlightID", i);
        //        dataTypes.SetValue(SqlDbType.VarChar, i);
        //        i++;

        //        //4
        //        paramNames.SetValue("ExpectedInfants", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //5
        //        paramNames.SetValue("ExpectedAdults", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //6
        //        paramNames.SetValue("ExpectedChild", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //7
        //        paramNames.SetValue("ExpectedTotalPax", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //8
        //        paramNames.SetValue("ActualInfants", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //9
        //        paramNames.SetValue("ActualAdults", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //10
        //        paramNames.SetValue("ActualChild", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;

        //        //11
        //        paramNames.SetValue("ActualTotalPax", i);
        //        dataTypes.SetValue(SqlDbType.Int, i);
        //        i++;


        //        //12
        //        paramNames.SetValue("CreatedOn", i);
        //        dataTypes.SetValue(SqlDbType.DateTime, i);
        //        i++;

        //        //13
        //        paramNames.SetValue("CreatedBy", i);
        //        dataTypes.SetValue(SqlDbType.VarChar, i);
        //        i++;

        //        //14
        //        paramNames.SetValue("FileName", i);
        //        dataTypes.SetValue(SqlDbType.VarChar, i);

        //        SQLServer sqlServer = new SQLServer();
        //        isInsert = sqlServer.InsertData("uspSaveFlightPaxInformation", paramNames, dataTypes, paxdata);

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        isInsert = false;
        //    }

        //    return isInsert;

        //}

        private DataTable CreatePaxDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SerialNumber", typeof(string));
            dt.Columns.Add("DepartureDate", typeof(string));
            dt.Columns.Add("Origin", typeof(string));
            dt.Columns.Add("CarrierCode", typeof(string));
            dt.Columns.Add("FlightID", typeof(string));
            dt.Columns.Add("ExpectedInfants", typeof(string));
            dt.Columns.Add("ExpectedAdults", typeof(string));
            dt.Columns.Add("ExpectedChild", typeof(string));
            dt.Columns.Add("ExpectedTotalPax", typeof(string));
            dt.Columns.Add("ActualInfants", typeof(string));
            dt.Columns.Add("ActualAdults", typeof(string));
            dt.Columns.Add("ActualChild", typeof(string));
            dt.Columns.Add("ActualTotalPax", typeof(string));
            dt.Columns.Add("FileName", typeof(string));
            dt.Columns.Add("UpdatedOn", typeof(string));
            dt.Columns.Add("UpdatedBy", typeof(string));
            dt.Columns.Add("ExpectedBaggage", typeof(string));
            dt.Columns.Add("ActualBaggage", typeof(string));

            dt.Columns.Add("AircraftCode", typeof(string));
            dt.Columns.Add("AircraftRegistration", typeof(string));
            dt.Columns.Add("AircraftType", typeof(string));
            dt.Columns.Add("DepartureTime", typeof(string));
            dt.Columns.Add("Destination", typeof(string));
            dt.Columns.Add("GroupBooked", typeof(string));
            return dt;
        }
    }
}