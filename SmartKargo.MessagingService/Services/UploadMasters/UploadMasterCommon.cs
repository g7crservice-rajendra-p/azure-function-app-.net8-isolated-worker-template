using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QidWorkerRole.UploadMasters.Agent;
using QidWorkerRole.UploadMasters.AircraftPattern;
using QidWorkerRole.UploadMasters.Airports;
using QidWorkerRole.UploadMasters.Booking;
using QidWorkerRole.UploadMasters.CapacityAllocation;
using QidWorkerRole.UploadMasters.CCAUpload;
using QidWorkerRole.UploadMasters.Collection;
using QidWorkerRole.UploadMasters.CostLine;
using QidWorkerRole.UploadMasters.DCM;
using QidWorkerRole.UploadMasters.ExchangeRates;
using QidWorkerRole.UploadMasters.ExchangeRatesFromTo;
using QidWorkerRole.UploadMasters.FlightBudget;
using QidWorkerRole.UploadMasters.FlightSchedule;
using QidWorkerRole.UploadMasters.FlightScheduleExcel;
using QidWorkerRole.UploadMasters.MSRRates;
using QidWorkerRole.UploadMasters.OtherCharges;
using QidWorkerRole.UploadMasters.PartnerMaster;
using QidWorkerRole.UploadMasters.PartnerSchedule;
using QidWorkerRole.UploadMasters.RateLine;
using QidWorkerRole.UploadMasters.RouteControl;
using QidWorkerRole.UploadMasters.ShipperConsignee;
using QidWorkerRole.UploadMasters.Taxline;
using QidWorkerRole.UploadMasters.UserMaster;
using QidWorkerRole.UploadMasters.Vendor;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;

namespace QidWorkerRole.UploadMasters
{
    public class UploadMasterCommon
    {
        /// <summary>
        /// To update Masters File Upload Status.
        /// </summary>
        /// <param name="UploadSummarySrNo"></param>
        /// <param name="STATUS"></param>
        /// <param name="RecordCount"></param>
        /// <param name="SuccessCount"></param>
        /// <param name="FailCount"></param>
        /// <param name="ProgressStatus"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="IsSuccess"></param>
        /// <param name="IsRetryCountUpdate"></param>

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UploadMasterCommon> _logger;

        //private readonly GenericFunction _genericFunction();
        private readonly Func<GenericFunction> _genericFunctionFactory;


        private readonly UploadVendorMaster _uploadVendorMaster;
        private readonly UploadRateLineMaster _uploadRateLineMaster;
        private readonly UploadAgentMasterGeneralInfo _uploadAgentMasterGeneralInfo;
        private readonly UploadAgentMaster _uploadAgentMaster;
        private readonly UploadAgentMasterUpdate _uploadAgentMasterUpdate;
        private readonly UploadShipperConsigneeMaster _uploadShipperConsigneeMaster;
        private readonly UploadOtherChargesMaster _uploadOtherChargesMaster;
        private readonly FlightCapacity.FlightCapacity _flightCapacity;
        private readonly UploadFlightSchedule _uploadFlightSchedule;
        private readonly UploadCapacityAllocation _uploadCapacityAllocation;
        private readonly UploadCostMaster _uploadCostMaster;
        private readonly UploadTaxLine _uploadTaxLine;
        private readonly UploadFlightBudget _uploadFlightBudget;
        private readonly UploadRouteControl _uploadRouteControl;
        private readonly UploadAirportsMaster _uploadAirportsMaster;
        private readonly UploadPartnerMaster _uploadPartnerMaster;
        private readonly UploadPartnerSchedule _uploadPartnerSchedule;
        private readonly UploadUserMaster _uploadUserMaster;
        private readonly UploadFlightScheduleExcel _uploadFlightScheduleExcel;
        private readonly UploadAircraftLoadingPattern _uploadAircraftLoadingPattern;
        private readonly FlightPaxInfo.FlightPaxInfo _flightPaxInfo;
        private readonly UploadExchangeRatesFromTo _uploadExchangeRatesFromTo;
        private readonly PHCustomRegistry _phCustomRegistry;
        private readonly InvoiceCollection _invoiceCollection;
        private readonly CCAUploadFile _ccaUploadFile;
        private readonly UploadMSRRates _uploadMSRRates;
        private readonly UploadExchangeRates _uploadExchangeRates;
        private readonly BookingExcelUpload _bookingExcelUpload;
        private readonly UploadDCM _uploadDCM;

        #region Constructor
        public UploadMasterCommon(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UploadMasterCommon> logger,

            //GenericFunction genericFunction,
            Func<GenericFunction> genericFunctionFactory,

            UploadVendorMaster uploadVendorMaster,
            UploadRateLineMaster uploadRateLineMaster,
            UploadAgentMasterGeneralInfo uploadAgentMasterGeneralInfo,
            UploadAgentMaster uploadAgentMaster,
            UploadAgentMasterUpdate uploadAgentMasterUpdate,
            UploadShipperConsigneeMaster uploadShipperConsigneeMaster,
            UploadOtherChargesMaster uploadOtherChargesMaster,
            FlightCapacity.FlightCapacity flightCapacity,
            UploadFlightSchedule uploadFlightSchedule,
            UploadCapacityAllocation uploadCapacityAllocation,
            UploadCostMaster uploadCostMaster,
            UploadTaxLine uploadTaxLine,
            UploadFlightBudget uploadFlightBudget,
            UploadRouteControl uploadRouteControl,
            UploadAirportsMaster uploadAirportsMaster,
            UploadPartnerMaster uploadPartnerMaster,
            UploadPartnerSchedule uploadPartnerSchedule,
            UploadUserMaster uploadUserMaster,
            UploadFlightScheduleExcel uploadFlightScheduleExcel,
            UploadAircraftLoadingPattern uploadAircraftLoadingPattern,
            FlightPaxInfo.FlightPaxInfo flightPaxInfo,
            UploadExchangeRatesFromTo uploadExchangeRatesFromTo,
            PHCustomRegistry phCustomRegistry,
            InvoiceCollection invoiceCollection,
            CCAUploadFile ccaUploadFile,
            UploadMSRRates uploadMSRRates,
            UploadExchangeRates uploadExchangeRates,
            BookingExcelUpload bookingExcelUpload,
            UploadDCM uploadDCM)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;

            //_genericFunction = genericFunction;
            _genericFunctionFactory = genericFunctionFactory;
            _uploadVendorMaster = uploadVendorMaster;
            _uploadRateLineMaster = uploadRateLineMaster;
            _uploadAgentMasterGeneralInfo = uploadAgentMasterGeneralInfo;
            _uploadAgentMaster = uploadAgentMaster;
            _uploadAgentMasterUpdate = uploadAgentMasterUpdate;
            _uploadShipperConsigneeMaster = uploadShipperConsigneeMaster;
            _uploadOtherChargesMaster = uploadOtherChargesMaster;
            _flightCapacity = flightCapacity;
            _uploadFlightSchedule = uploadFlightSchedule;
            _uploadCapacityAllocation = uploadCapacityAllocation;
            _uploadCostMaster = uploadCostMaster;
            _uploadTaxLine = uploadTaxLine;
            _uploadFlightBudget = uploadFlightBudget;
            _uploadRouteControl = uploadRouteControl;
            _uploadAirportsMaster = uploadAirportsMaster;
            _uploadPartnerMaster = uploadPartnerMaster;
            _uploadPartnerSchedule = uploadPartnerSchedule;
            _uploadUserMaster = uploadUserMaster;
            _uploadFlightScheduleExcel = uploadFlightScheduleExcel;
            _uploadAircraftLoadingPattern = uploadAircraftLoadingPattern;
            _flightPaxInfo = flightPaxInfo;
            _uploadExchangeRatesFromTo = uploadExchangeRatesFromTo;
            _phCustomRegistry = phCustomRegistry;
            _invoiceCollection = invoiceCollection;
            _ccaUploadFile = ccaUploadFile;
            _uploadMSRRates = uploadMSRRates;
            _uploadExchangeRates = uploadExchangeRates;
            _bookingExcelUpload = bookingExcelUpload;
            _uploadDCM = uploadDCM;
        }
        #endregion

        //public void UpdateUploadMastersStatus(int UploadSummarySrNo, string STATUS, int RecordCount, int SuccessCount, int FailCount,
        //                                      int ProgressStatus, string ErrorMessage, int IsSuccess, int IsRetryCountUpdate = 0)
        public async Task UpdateUploadMastersStatus(int UploadSummarySrNo, string STATUS, int RecordCount, int SuccessCount, int FailCount,
                                              int ProgressStatus, string ErrorMessage, int IsSuccess, int IsRetryCountUpdate = 0)
        {
            DataSet? dataSetResult = new DataSet();
            //SQLServer sqlServer = new SQLServer();

            try
            {
                SqlParameter[] sqlParameter = [
                    new SqlParameter("@UploadSummarySrNo", UploadSummarySrNo),
                      new SqlParameter("@STATUS", STATUS),
                      new SqlParameter("@RecordCount", RecordCount),
                      new SqlParameter("@SuccessCount", SuccessCount),
                      new SqlParameter("@FailCount", FailCount),
                      new SqlParameter("@ProgressStatus", ProgressStatus),
                      new SqlParameter("@ErrorMessage", ErrorMessage),
                      new SqlParameter("@IsSuccess", IsSuccess),
                      new SqlParameter("@IsRetryCountUpdate", IsRetryCountUpdate)
                ];

                //dataSetResult =  sqlServer.SelectRecords("Masters.uspUpdateUploadMastersStatus", sqlParameter);
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspUpdateUploadMastersStatus", sqlParameter);
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure(exception);
                _logger.LogError(exception, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// To update Masters File Upload Summary.
        /// </summary>
        /// <param name="SrNo"></param>
        /// <param name="RecordCount"></param>
        /// <param name="SuccessCount"></param>
        /// <param name="FailedCount"></param>
        /// <param name="Status"></param>
        /// <param name="ProgressStatus"></param>
        /// <param name="ProcessMethod"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="IsProcessed"></param>

        //public void UpdateUploadMasterSummaryLog(int SrNo, int RecordCount, int SuccessCount, int FailedCount, string Status,
        //                                         int ProgressStatus, string ProcessMethod, string ErrorMessage, bool IsProcessed)

        public async Task UpdateUploadMasterSummaryLog(int SrNo, int RecordCount, int SuccessCount, int FailedCount, string Status,
                                                int ProgressStatus, string ProcessMethod, string ErrorMessage, bool IsProcessed)
        {
            DataSet? dataSetResult = new DataSet();
            //SQLServer sqlServer = new SQLServer();
            try
            {
                SqlParameter[] sqlParameter = [
                    new SqlParameter("@SrNo", SrNo),
                    new SqlParameter("@RecordCount", RecordCount),
                    new SqlParameter("@SuccessCount", SuccessCount),
                    new SqlParameter("@FailedCount", FailedCount),
                    new SqlParameter("@Status", Status),
                    new SqlParameter("@ProgressStatus", ProgressStatus),
                    new SqlParameter("@ProcessMethod", ProcessMethod),
                    new SqlParameter("@ErrorMessage", ErrorMessage),
                    new SqlParameter("@IsProcessed", IsProcessed)
                ];

                //dataSetResult = sqlServer.SelectRecords("Masters.uspUpdateUploadMasterSummary", sqlParameter);
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspUpdateUploadMasterSummary", sqlParameter);
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure(exception);
                _logger.LogError(exception, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task<DataSet> GetUploadedFileData(string MasterType)
        {
            DataSet? ds = new DataSet();
            //SQLServer ObjSql = new SQLServer();

            try
            {
                SqlParameter[] sqlParams = new SqlParameter[]
                {
                    new SqlParameter("@MasterType", MasterType)
                };


                //ds = ObjSql.SelectRecords("uspGetUplodedFile", sqlParams);
                ds = await _readWriteDao.SelectRecords("uspGetUplodedFile", sqlParams);

                if (ds != null)
                {
                    return (ds);
                }
                else
                {
                    return (ds);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        /*Not in use*/
        //public bool IsFileExistOnBlob(string filename, String containerName, out byte[] downloadStream)
        //{
        //    try
        //    {
        //        containerName = containerName.ToLower();
        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        CloudBlob blob = blobClient.GetBlobReference(string.Format("{0}/{1}", containerName, filename));
        //        downloadStream = blob.DownloadByteArray();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        downloadStream = null;
        //        return false;
        //    }

        //}

        /*Not in use*/
        //public string getStorageKey()
        //{
        //    string BlobKey = "";
        //    try
        //    {
        //        //Cls_BL cls_BL = new Cls_BL();

        //        //GenericFunction genericFunction = new GenericFunction();
        //        BlobKey = _genericFunctionFactory().ReadValueFromDb("BlobStorageKey");
        //        //BlobKey = sqlServer.GetMasterConfiguration("BlobStorageKey");

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    return BlobKey;
        //}

        /*Not in use*/
        //public string getStorageName()
        //{
        //    string BlobName = "";

        //    try
        //    {
        //        //Cls_BL cls_BL = new Cls_BL();
        //        //GenericFunction genericFunction = new GenericFunction();
        //        BlobName = _genericFunctionFactory().ReadValueFromDb("BlobStorageName");
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    return BlobName;
        //}

        /// <summary>
        /// To insert Master Upload Summary Log data.
        /// </summary>
        /// <param name="SrNo"></param>
        /// <param name="FileName"></param>
        /// <param name="MasterType"></param>
        /// <param name="UploadedBy"></param>
        /// <param name="RecordCount"></param>
        /// <param name="SuccessCount"></param>
        /// <param name="FailedCount"></param>
        /// <param name="Station"></param>
        /// <param name="Status"></param>
        /// <param name="ProgressStatus"></param>
        /// <param name="BolbName"></param>
        /// <param name="ContainerName"></param>
        /// <param name="FolderName"></param>
        /// <param name="ProcessMethod"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="IsProcessed"></param>
        /// <returns></returns>
        internal async Task<DataSet?> InsertMasterSummaryLog(int SrNo, string FileName, string MasterType, string UploadedBy, int RecordCount,
                                                int SuccessCount, int FailedCount, string Station, string Status, int ProgressStatus,
                                                string BolbName, string ContainerName, string FolderName, string ProcessMethod, string ErrorMessage,
                                                bool IsProcessed, DateTime? LastWriteTime = null)
        {
            DataSet? dataSetResult = new DataSet();
            //SQLServer sqlServer = new SQLServer();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@SrNo", SrNo),
                                                                   new SqlParameter("@FileName", FileName),
                                                                   new SqlParameter("@MasterType", MasterType),
                                                                   new SqlParameter("@UploadedBy", UploadedBy),
                                                                   new SqlParameter("@RecordCount", RecordCount),
                                                                   new SqlParameter("@SuccessCount", SuccessCount),
                                                                   new SqlParameter("@FailedCount", FailedCount),
                                                                   new SqlParameter("@Station", Station),
                                                                   new SqlParameter("@Status", Status),
                                                                   new SqlParameter("@ProgressStatus", ProgressStatus),
                                                                   new SqlParameter("@BolbName", BolbName),
                                                                   new SqlParameter("@ContainerName", ContainerName),
                                                                   new SqlParameter("@FolderName", FolderName),
                                                                   new SqlParameter("@ProcessMethod", ProcessMethod),
                                                                   new SqlParameter("@ErrorMessage", ErrorMessage),
                                                                   new SqlParameter("@IsProcessed", IsProcessed),
                                                                   new SqlParameter("@LastWriteTime", LastWriteTime)
                                                                 };

                //dataSetResult = sqlServer.SelectRecords("Masters.uspInsertMasterUploadSummaryStatusLog", sqlParameter);
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspInsertMasterUploadSummaryStatusLog", sqlParameter);
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure(exception);
                _logger.LogError(exception, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dataSetResult;
        }

        internal async Task<DataSet?> GetUploadMasterMessageConfiguration(string MessageType)
        {
            DataSet? dsUploadMasterConfiguration = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                     new  SqlParameter("@MessageType",MessageType)
                };
                //dsUploadMasterConfiguration = sqlServer.SelectRecords("uspGetUploadMasterMessageConfiguration", sqlParameter);
                dsUploadMasterConfiguration = await _readWriteDao.SelectRecords("uspGetUploadMasterMessageConfiguration", sqlParameter);
                if (dsUploadMasterConfiguration != null && dsUploadMasterConfiguration.Tables.Count > 0)
                {
                    return dsUploadMasterConfiguration;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }

        }

        /// <summary>
        /// Method to get the container name for selected upload type
        /// Added by prashantz
        /// </summary>
        /// <param name="UploadType">Type of master to be uploaded</param>
        /// <returns></returns>
        public async Task<DataSet> GetUploadMasterConfiguration(string UploadType)
        {
            try
            {
                DataSet? ds = new DataSet();
                //SQLServer da = new SQLServer();

                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter("@UploadType",UploadType),
                };

                //ds = da.SelectRecords("uspGetUploadMasterConfiguration", sqlParams);
                ds = await _readWriteDao.SelectRecords("uspGetUploadMasterConfiguration", sqlParams);

                if (ds != null)
                {
                    return ds;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
            return null;
        }

        public bool DoDownloadBLOB(string filename, string containerName, string FolderName, out string FilePath)
        {

            try
            {
                //Cls_BL cls_BL = new Cls_BL();
                //GenericFunction genericFunction = new GenericFunction();

                //string BlobStorageName = _genericFunctionFactory().ReadValueFromDb("BlobStorageName");
                //string BlobStorageKey = _genericFunctionFactory().ReadValueFromDb("BlobStorageKey");

                ////  This is standard code to interact with Blob storage.
                //StorageCredentialsAccountAndKey creds = new StorageCredentialsAccountAndKey(BlobStorageName, BlobStorageKey);
                //CloudStorageAccount storageAccount = new CloudStorageAccount(creds, useHttps: true);
                //CloudBlobClient client = storageAccount.CreateCloudBlobClient();

                //client.RetryPolicy = RetryPolicies.Retry(10, TimeSpan.FromSeconds(5));

                //CloudBlobContainer container = client.GetContainerReference(containerName);
                //container.CreateIfNotExist();

                ////// In this case, we will not pass a key and only pass the resolver because
                ////// this policy will only be used for downloading / decrypting.
                ////var list = container.ListBlobs();
                ////Dictionary<string, DateTimeOffset?> blobNames = list.OfType<CloudBlockBlob>().Select(b => new { BlobName = b.Name, ModifiedDate = b.Properties.LastModified }).ToDictionary(t => t.BlobName, t => t.ModifiedDate);
                ////Dictionary<string, DateTimeOffset?> d = new Dictionary<string, DateTimeOffset?>();
                ////var blobName = blobNames.FirstOrDefault(x => x.Value == blobNames.Values.Max()).Key;
                //CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                //// Fetch container properties and write out their values.
                //blob.FetchAttributes();
                ////  create a local file
                //string filepath = @Convert.ToString(_genericFunctionFactory().ReadValueFromDb("DownLoadFilePath")) + "\\" + FolderName + "\\" + blob.Name.Trim();
                ////string filepath = @ConfigurationManager.AppSettings["DownLoadFilePath"].ToString() + "\\" + FolderName + "\\" + blob.Name.Trim();

                //FileInfo fiFilePath = new FileInfo(filepath);
                ////get last modified date 

                ////  create a local file
                //if (fiFilePath.Directory != null && !fiFilePath.Directory.Exists)
                //{
                //    fiFilePath.Directory.Create();
                //}
                //FileInfo[] filePaths = fiFilePath.Directory.GetFiles();
                //foreach (FileInfo filePath in filePaths)
                //    filePath.Delete();

                //File.Delete(filepath);
                //blob.DownloadToFile(filepath, null);
                //FilePath = fiFilePath.FullName;

                string accountName = ConfigCache.Get("BlobStorageName");
                string accountKey = ConfigCache.Get("BlobStorageKey");

                var sharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);

                // Configure retry policy: 10 retries, 5-second initial delay, exponential backoff
                var options = new BlobClientOptions
                {
                    Retry =
                    {
                        MaxRetries = 10,
                        Delay = TimeSpan.FromSeconds(5),
                        Mode = RetryMode.Exponential,  
                        // Optional: Cap max delay (e.g., 2 minutes) to prevent excessive waits
                        MaxDelay = TimeSpan.FromMinutes(2)
                    }
                };

                // BlobServiceClient with HTTPS enforced and retry options
                var blobServiceClient = new BlobServiceClient(
                    new Uri($"https://{accountName}.blob.core.windows.net"),
                    sharedKeyCredential,
                    options
                );

                // Get container and create if not exists
                BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
                container.CreateIfNotExists();

                // Get BlobClient (equivalent to CloudBlockBlob)
                BlobClient blob = container.GetBlobClient(filename);

                // Fetch blob attributes (equivalent to FetchAttributes())
                blob.GetProperties();  // Retrieves properties like LastModified, ContentType, etc.

                // Create local file path
                string downloadFilePath = ConfigCache.Get("DownLoadFilePath");
                string filepath = @Convert.ToString(downloadFilePath) + "\\" + FolderName + "\\" + blob.Name.Trim();


                FileInfo fiFilePath = new FileInfo(filepath);
                //get last modified date 

                //  create a local file
                if (fiFilePath.Directory != null && !fiFilePath.Directory.Exists)
                {
                    fiFilePath.Directory.Create();
                }

                FileInfo[] filePaths = fiFilePath.Directory.GetFiles();
                foreach (FileInfo filePath in filePaths)
                {
                    filePath.Delete();
                }

                File.Delete(filepath);

                // Download blob to local file (equivalent to DownloadToFile(filepath, null))
                blob.DownloadTo(filepath);

                FilePath = fiFilePath.FullName;
            }
            catch (Exception ex)
            {
                FilePath = string.Empty;
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
            return true;
        }

        internal async Task<DataSet> InsertCapacityFile(DataTable dt, DateTime Updatedon, string UpdatedBy)
        {
            try
            {
                DataSet? ds = new DataSet();
                //QID.DataAccess.SQLServer da = new QID.DataAccess.SQLServer();

                SqlParameter[] sqlParams = new SqlParameter[] {
                new SqlParameter("@tblcapacityTableType", dt),
                new SqlParameter("@UploadedOn", Updatedon),
                new SqlParameter("@UploadedBy", UpdatedBy)

            };

                //ds = da.SelectRecords("uspUploadCapcityFile", sqlParams);
                ds = await _readWriteDao.SelectRecords("uspUploadCapcityFile", sqlParams);

                if (ds != null)
                {
                    return (ds);
                }
                else
                {
                    return (ds);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return (null);
            }

        }

        internal async Task<DataSet> InsertMasterDetailsLog(int SerialNumber, string MasterValue, string ErrorMessage, bool IsSuccess, DateTime UploadStartTime)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] {
                     new SqlParameter("UploadSummarySrNo",SerialNumber)
                    ,new SqlParameter("MasterKey",MasterValue)
                    ,new SqlParameter("ErrorDescription",ErrorMessage)
                    ,new SqlParameter("IsSuccess",IsSuccess)
                    ,new SqlParameter("UploadedOn",UploadStartTime)
                };

                //SQLServer sqlServer = new SQLServer();
                DataSet? ds = new DataSet("Ds_DetailLogSrNo");


                //ds = sqlServer.SelectRecords("spAddUploadMasterDetails", sqlParameter);
                ds = await _readWriteDao.SelectRecords("spAddUploadMasterDetails", sqlParameter);

                return ds;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        internal async Task<bool> InsertMasterDetailsLog(DataTable dtUploadMasterDetailLog, DataTable dtCapacity, DateTime UploadStartTime, string UserName)
        {
            bool IsSuccess = false;
            try
            {
                //SQLServer sqlServer = new SQLServer();
                DataSet? dsUploadMasterDetailLog = new DataSet();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                      new SqlParameter("@dtUploadMasterDetailLog",dtUploadMasterDetailLog)
                    , new SqlParameter("@dtCapacity",dtCapacity)
                    , new SqlParameter("@UploadStartTime",UploadStartTime)
                    , new SqlParameter("@UserName",UserName)
                };
                //dsUploadMasterDetailLog = sqlServer.SelectRecords("uspUploadMasterDetailsLog", sqlParameter);
                dsUploadMasterDetailLog = await _readWriteDao.SelectRecords("uspUploadMasterDetailsLog", sqlParameter);
                if (dsUploadMasterDetailLog != null && dsUploadMasterDetailLog.Tables.Count > 0 && dsUploadMasterDetailLog.Tables[0].Rows.Count > 0)
                {
                    bool.TryParse(dsUploadMasterDetailLog.Tables[1].Rows[0]["Result"].ToString(), out IsSuccess);
                    return IsSuccess;
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return IsSuccess;
        }

        internal void ExportDataSet(DataSet ds, string destinationPath)
        {
            try
            {
                FileInfo fiFilePath = new FileInfo(destinationPath);
                //get last modified date 

                //  create a local file
                if (fiFilePath.Directory != null && !fiFilePath.Directory.Exists)
                {
                    fiFilePath.Directory.Create();
                }

                using (var workbook = SpreadsheetDocument.Create(fiFilePath.FullName + "Demo.xlsx", DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();

                    workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                    workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                    foreach (System.Data.DataTable table in ds.Tables)
                    {

                        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                        sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                        DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                        string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                        uint sheetId = 1;
                        if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                        {
                            sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                        }

                        DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                        sheets.Append(sheet);

                        DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                        List<String> columns = new List<string>();
                        foreach (System.Data.DataColumn column in table.Columns)
                        {
                            columns.Add(column.ColumnName);

                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                            headerRow.AppendChild(cell);
                        }


                        sheetData.AppendChild(headerRow);

                        foreach (System.Data.DataRow dsrow in table.Rows)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                            foreach (String col in columns)
                            {
                                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                                newRow.AppendChild(cell);
                            }

                            sheetData.AppendChild(newRow);
                        }

                    }
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

        }

        internal void ExportDataSet(DataTable table, string destinationPath)
        {
            try
            {
                FileInfo fiFilePath = new FileInfo(destinationPath);
                //get last modified date 

                //  create a local file
                if (fiFilePath.Directory != null && !fiFilePath.Directory.Exists)
                {
                    fiFilePath.Directory.Create();
                }

                using (var workbook = SpreadsheetDocument.Create(fiFilePath.FullName + "Demo.xlsx", DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = workbook.AddWorkbookPart();

                    workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                    workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();



                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }


                    sheetData.AppendChild(headerRow);

                    foreach (System.Data.DataRow dsrow in table.Rows)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

        }

        internal async Task AgentUpdateDateInterval(string nextDate)
        {
            try
            {
                DataSet? dsResult = new DataSet();
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                    new SqlParameter("NextDate", nextDate)
                };
                //dsResult = sqlServer.SelectRecords("uspAgentUpdateDateInterval", sqlParameter);
                dsResult = await _readWriteDao.SelectRecords("uspAgentUpdateDateInterval", sqlParameter);

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        internal async Task UploadMasters(DataSet dsUploadMasters)
        {

            try
            {
                string uploadType = string.Empty;
                int retryCount = 0;
                for (int i = 0; i < dsUploadMasters.Tables[0].Rows.Count; i++)
                {
                    DataTable dtUploadMasters = new DataTable();
                    DataSet dsUploadRecord = new DataSet();
                    dtUploadMasters = dsUploadMasters.Tables[0].Clone();
                    DataRow dr = dtUploadMasters.NewRow();
                    dr["URL"] = dsUploadMasters.Tables[0].Rows[i]["URL"];
                    dr["BlobName"] = dsUploadMasters.Tables[0].Rows[i]["BlobName"];
                    dr["ContainerName"] = dsUploadMasters.Tables[0].Rows[i]["ContainerName"];
                    dr["FileName"] = dsUploadMasters.Tables[0].Rows[i]["FileName"];
                    dr["SrNo"] = dsUploadMasters.Tables[0].Rows[i]["SrNo"];
                    dr["MasterType"] = dsUploadMasters.Tables[0].Rows[i]["MasterType"];
                    dr["RetryCount"] = dsUploadMasters.Tables[0].Rows[i]["RetryCount"];
                    dtUploadMasters.Rows.Add(dr);
                    dsUploadRecord.Tables.Add(dtUploadMasters);

                    uploadType = dsUploadMasters.Tables[0].Rows[i]["MasterType"].ToString();
                    retryCount = Convert.ToInt32(dsUploadMasters.Tables[0].Rows[i]["RetryCount"].ToString());
                    if (uploadType.ToUpper() == UploadMasterType.RateLine.ToUpper())
                    {
                        //RateLine.UploadRateLineMaster uploadRateLineMaster = new RateLine.UploadRateLineMaster();
                        await _uploadRateLineMaster.RateLineMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.AgentGeneralInfo.ToUpper())
                    {
                        //Agent.UploadAgentMasterGeneralInfo uploadAgentMasterGeneralInfo = new Agent.UploadAgentMasterGeneralInfo();
                        await _uploadAgentMasterGeneralInfo.AgentMasterUploadGeneralInfo(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.Agent.ToUpper())
                    {
                        //Agent.UploadAgentMaster uploadAgentMaster = new Agent.UploadAgentMaster();
                        await _uploadAgentMaster.AgentMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.AgentUpdate.ToUpper())
                    {
                        //UploadAgentMasterUpdate uploadAgentMasterUpdate = new Agent.UploadAgentMasterUpdate();
                        await _uploadAgentMasterUpdate.UpdateAgent(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.ShipperConsignee.ToUpper())
                    {
                        //ShipperConsignee.UploadShipperConsigneeMaster uploadShipperConsigneeMaster = new ShipperConsignee.UploadShipperConsigneeMaster();
                        await _uploadShipperConsigneeMaster.ShipperConsigneeMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.OtherCharges.ToUpper())
                    {
                        //OtherCharges.UploadOtherChargesMaster uploadOtherChargesMaster = new OtherCharges.UploadOtherChargesMaster();
                        await _uploadOtherChargesMaster.OtherChargesMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.FlightCapacity.ToUpper())
                    {
                        //FlightCapacity.FlightCapacity flightCapacity = new FlightCapacity.FlightCapacity();
                        await _flightCapacity.UploadFlightCapacity(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.FlightSchedule.ToUpper())
                    {
                        //FlightSchedule.UploadFlightSchedule uploadFlightSchedule = new FlightSchedule.UploadFlightSchedule();
                        if (!await _uploadFlightSchedule.GetUploadFlightSchedule(dsUploadRecord) && retryCount == 2)
                        {
                            //GenericFunction genericFunction = new GenericFunction();
                            //string uploadAlertEmailID = _genericFunctionFactory().GetConfigurationValues("SSIMUploadAlertEmailID");

                            string uploadAlertEmailID = ConfigCache.Get("SSIMUploadAlertEmailID");

                            await _genericFunctionFactory().SaveMessageOutBox("Flight Schedule"
                                , "Hi,\r\n\r\nSSIM Upload is failed, please contact to the team for more details.\r\n\r\nThanks,\r\n\r\nSmartKargo Team"
                                , "", uploadAlertEmailID, "", 0);
                        }
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.CapacityAllocation.ToUpper())
                    {
                        //CapacityAllocation.UploadCapacityAllocation uploadCapacityAllocation = new CapacityAllocation.UploadCapacityAllocation();
                        await _uploadCapacityAllocation.CapacityAllocation(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.CostMaster.ToUpper())
                    {
                        //CostLine.UploadCostMaster uploadCostMaster = new CostLine.UploadCostMaster();
                        await _uploadCostMaster.CostLineMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.TaxLine.ToUpper())
                    {
                        //Taxline.UploadTaxLine uploadTaxLine = new Taxline.UploadTaxLine();
                        await _uploadTaxLine.TaxLineMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.FlightBudget.ToUpper())
                    {
                        //FlightBudget.UploadFlightBudget uploadFlightBudget = new FlightBudget.UploadFlightBudget();
                        await _uploadFlightBudget.FlightBudgetUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.RouteControls.ToUpper())
                    {
                        //RouteControl.UploadRouteControl uploadRouteControl = new RouteControl.UploadRouteControl();
                        await _uploadRouteControl.RouteControlsMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.Airports.ToUpper())
                    {
                        //Airports.UploadAirportsMaster uploadAirports = new Airports.UploadAirportsMaster();
                        await _uploadAirportsMaster.UpdateAirports(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.Partners.ToUpper())
                    {
                        //PartnerMaster.UploadPartnerMaster uploadPartnerMaster = new PartnerMaster.UploadPartnerMaster();
                        await _uploadPartnerMaster.PartnerMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.PartnerSchedule.ToUpper())
                    {
                        //PartnerSchedule.UploadPartnerSchedule uploadPartnerSchedule = new PartnerSchedule.UploadPartnerSchedule();
                        await _uploadPartnerSchedule.PartnerScheduleUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.User.ToUpper())
                    {
                        //UserMaster.UploadUserMaster uploadUserMaster = new UserMaster.UploadUserMaster();
                        await _uploadUserMaster.UserMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.FlightScheduleExcel.ToUpper())
                    {
                        //FlightScheduleExcel.UploadFlightScheduleExcel uploadFlightScheduleExcel = new FlightScheduleExcel.UploadFlightScheduleExcel();
                        await _uploadFlightScheduleExcel.GetUploadFlightSchedule(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.AircraftLoadingPattern.ToUpper())
                    {
                        //AircraftPattern.UploadAircraftLoadingPattern uploadAircraftLoadingPattern = new AircraftPattern.UploadAircraftLoadingPattern();
                        await _uploadAircraftLoadingPattern.AircraftPatternUpload();
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.FlightPaxInformation.ToUpper()
                        || uploadType.ToUpper() == UploadMasterType.FlightPaxForecast.ToUpper())
                    {
                        //FlightPaxInfo.FlightPaxInfo FlightPaxInfo = new FlightPaxInfo.FlightPaxInfo();
                        await _flightPaxInfo.PaxMasterUpload(dsUploadRecord, uploadType);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.ExchangeRateFromTo.ToUpper())
                    {
                        //UploadExchangeRatesFromTo uploadExchangeRatesFromTo = new UploadExchangeRatesFromTo();
                        await _uploadExchangeRatesFromTo.ExchangeRatesFromTo(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.PHCustomRegistry.ToUpper())
                    {
                        //PHCustomRegistry phCustomRegistry = new PHCustomRegistry();
                        await _phCustomRegistry.PHCustomRegistyUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.Collection.ToUpper())
                    {
                        //Collection.InvoiceCollection uploadCollection = new Collection.InvoiceCollection();
                        await _invoiceCollection.UpdateInvoiceCollection(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.CreditDebitNotes.ToUpper())
                    {
                        //CCAUpload.CCAUploadFile uploadCCA = new CCAUpload.CCAUploadFile();
                        await _ccaUploadFile.UpdateCCAUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.MSRRates.ToUpper())
                    {
                        //MSRRates.UploadMSRRates uploadMSR = new MSRRates.UploadMSRRates();
                        await _uploadMSRRates.UpdateMSRupload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.ExchangeRate.ToUpper())
                    {
                        //UploadExchangeRates uploadExchangeRates = new UploadExchangeRates();
                        await _uploadExchangeRates.UpdateExchangeRateUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.Booking.ToUpper())
                    {
                        //BookingExcelUpload bookingExcelUpload = new BookingExcelUpload();
                        await _bookingExcelUpload.BookingUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.VendorMaster.ToUpper())
                    {
                        //UploadVendorMaster uploadVendorMaster = new UploadVendorMaster();

                        await _uploadVendorMaster.VendorMasterUpload(dsUploadRecord);
                    }
                    else if (uploadType.ToUpper() == UploadMasterType.DCM.ToUpper())
                    {
                        //UploadDCM uploadDCMMaster = new UploadDCM();
                        await _uploadDCM.DCMUpload(dsUploadRecord);
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task UploadMasters()
        {

            try
            {
                DataSet? dsUploadMasters = new DataSet();
                //SQLServer sqlServerUplodedFile = new SQLServer();
                //dsUploadMasters = sqlServerUplodedFile.SelectRecords("uspGetUplodedFile");
                dsUploadMasters = await _readWriteDao.SelectRecords("uspGetUplodedFile");

                if (dsUploadMasters != null && dsUploadMasters.Tables.Count > 0 && dsUploadMasters.Tables[0].Rows.Count > 0)
                {
                    string uploadType = string.Empty;
                    int retryCount = 0;
                    for (int i = 0; i < dsUploadMasters.Tables[0].Rows.Count; i++)
                    {
                        DataTable dtUploadMasters = new DataTable();
                        DataSet dsUploadRecord = new DataSet();
                        dtUploadMasters = dsUploadMasters.Tables[0].Clone();
                        DataRow dr = dtUploadMasters.NewRow();
                        dr["URL"] = dsUploadMasters.Tables[0].Rows[i]["URL"];
                        dr["BlobName"] = dsUploadMasters.Tables[0].Rows[i]["BlobName"];
                        dr["ContainerName"] = dsUploadMasters.Tables[0].Rows[i]["ContainerName"];
                        dr["FileName"] = dsUploadMasters.Tables[0].Rows[i]["FileName"];
                        dr["SrNo"] = dsUploadMasters.Tables[0].Rows[i]["SrNo"];
                        dr["MasterType"] = dsUploadMasters.Tables[0].Rows[i]["MasterType"];
                        dtUploadMasters.Rows.Add(dr);
                        dsUploadRecord.Tables.Add(dtUploadMasters);

                        uploadType = dsUploadMasters.Tables[0].Rows[i]["MasterType"].ToString();
                        retryCount = Convert.ToInt32(dsUploadMasters.Tables[0].Rows[i]["RetryCount"].ToString());
                        if (uploadType.ToUpper() == UploadMasterType.RateLine.ToUpper())
                        {
                            //RateLine.UploadRateLineMaster uploadRateLineMaster = new RateLine.UploadRateLineMaster();
                            await _uploadRateLineMaster.RateLineMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.AgentGeneralInfo.ToUpper())
                        {
                            //Agent.UploadAgentMasterGeneralInfo uploadAgentMasterGeneralInfo = new Agent.UploadAgentMasterGeneralInfo();
                            await _uploadAgentMasterGeneralInfo.AgentMasterUploadGeneralInfo(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.Agent.ToUpper())
                        {
                            //Agent.UploadAgentMaster uploadAgentMaster = new Agent.UploadAgentMaster();
                            await _uploadAgentMaster.AgentMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.AgentUpdate.ToUpper())
                        {
                            //UploadAgentMasterUpdate uploadAgentMasterUpdate = new Agent.UploadAgentMasterUpdate();
                            await _uploadAgentMasterUpdate.UpdateAgent(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.ShipperConsignee.ToUpper())
                        {
                            //ShipperConsignee.UploadShipperConsigneeMaster uploadShipperConsigneeMaster = new ShipperConsignee.UploadShipperConsigneeMaster();
                            await _uploadShipperConsigneeMaster.ShipperConsigneeMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.OtherCharges.ToUpper())
                        {
                            //OtherCharges.UploadOtherChargesMaster uploadOtherChargesMaster = new OtherCharges.UploadOtherChargesMaster();
                            await _uploadOtherChargesMaster.OtherChargesMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.FlightCapacity.ToUpper())
                        {
                            //FlightCapacity.FlightCapacity flightCapacity = new FlightCapacity.FlightCapacity();
                            await _flightCapacity.UploadFlightCapacity(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.FlightSchedule.ToUpper())
                        {
                            //FlightSchedule.UploadFlightSchedule uploadFlightSchedule = new FlightSchedule.UploadFlightSchedule();
                            if (!await _uploadFlightSchedule.GetUploadFlightSchedule(dsUploadRecord) && retryCount == 2)
                            {
                                //GenericFunction genericFunction = new GenericFunction();

                                //string uploadAlertEmailID = _genericFunctionFactory().GetConfigurationValues("SSIMUploadAlertEmailID");
                                string uploadAlertEmailID = ConfigCache.Get("SSIMUploadAlertEmailID");

                                await _genericFunctionFactory().SaveMessageOutBox("Flight Schedule"
                                     , "Hi,\r\n\r\nSSIM Upload is failed, please contact to the team for more details.\r\n\r\nThanks,\r\n\r\nSmartKargo Team"
                                     , "", uploadAlertEmailID, "", 0);
                            }
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.CapacityAllocation.ToUpper())
                        {
                            //CapacityAllocation.UploadCapacityAllocation uploadCapacityAllocation = new CapacityAllocation.UploadCapacityAllocation();
                            await _uploadCapacityAllocation.CapacityAllocation(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.CostMaster.ToUpper())
                        {
                            //CostLine.UploadCostMaster uploadCostMaster = new CostLine.UploadCostMaster();
                            await _uploadCostMaster.CostLineMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.TaxLine.ToUpper())
                        {
                            //Taxline.UploadTaxLine uploadTaxLine = new Taxline.UploadTaxLine();
                            await _uploadTaxLine.TaxLineMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.FlightBudget.ToUpper())
                        {
                            //FlightBudget.UploadFlightBudget uploadFlightBudget = new FlightBudget.UploadFlightBudget();
                            await _uploadFlightBudget.FlightBudgetUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.RouteControls.ToUpper())
                        {
                            //RouteControl.UploadRouteControl uploadRouteControl = new RouteControl.UploadRouteControl();
                            await _uploadRouteControl.RouteControlsMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.Airports.ToUpper())
                        {
                            //Airports.UploadAirportsMaster uploadAirports = new Airports.UploadAirportsMaster();
                            await _uploadAirportsMaster.UpdateAirports(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.Partners.ToUpper())
                        {
                            //PartnerMaster.UploadPartnerMaster uploadPartnerMaster = new PartnerMaster.UploadPartnerMaster();
                            await _uploadPartnerMaster.PartnerMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.PartnerSchedule.ToUpper())
                        {
                            //PartnerSchedule.UploadPartnerSchedule uploadPartnerSchedule = new PartnerSchedule.UploadPartnerSchedule();
                            await _uploadPartnerSchedule.PartnerScheduleUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.User.ToUpper())
                        {
                            //UserMaster.UploadUserMaster uploadUserMaster = new UserMaster.UploadUserMaster();
                            await _uploadUserMaster.UserMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.FlightScheduleExcel.ToUpper())
                        {
                            //FlightScheduleExcel.UploadFlightScheduleExcel uploadFlightScheduleExcel = new FlightScheduleExcel.UploadFlightScheduleExcel();
                            await _uploadFlightScheduleExcel.GetUploadFlightSchedule(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.AircraftLoadingPattern.ToUpper())
                        {
                            //AircraftPattern.UploadAircraftLoadingPattern uploadAircraftLoadingPattern = new AircraftPattern.UploadAircraftLoadingPattern();
                            await _uploadAircraftLoadingPattern.AircraftPatternUpload();
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.FlightPaxInformation.ToUpper()
                            || uploadType.ToUpper() == UploadMasterType.FlightPaxForecast.ToUpper())
                        {
                            //FlightPaxInfo.FlightPaxInfo FlightPaxInfo = new FlightPaxInfo.FlightPaxInfo();
                            await _flightPaxInfo.PaxMasterUpload(dsUploadRecord, uploadType);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.ExchangeRateFromTo.ToUpper())
                        {
                            //UploadExchangeRatesFromTo uploadExchangeRatesFromTo = new UploadExchangeRatesFromTo();
                            await _uploadExchangeRatesFromTo.ExchangeRatesFromTo(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.PHCustomRegistry.ToUpper())
                        {
                            //PHCustomRegistry phCustomRegistry = new PHCustomRegistry();
                            await _phCustomRegistry.PHCustomRegistyUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.Collection.ToUpper())
                        {
                            //Collection.InvoiceCollection uploadCollection = new Collection.InvoiceCollection();
                            await _invoiceCollection.UpdateInvoiceCollection(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.CreditDebitNotes.ToUpper())
                        {
                            //CCAUpload.CCAUploadFile uploadCCA = new CCAUpload.CCAUploadFile();
                            await _ccaUploadFile.UpdateCCAUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.MSRRates.ToUpper())
                        {
                            //MSRRates.UploadMSRRates uploadMSR = new MSRRates.UploadMSRRates();
                            await _uploadMSRRates.UpdateMSRupload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.ExchangeRate.ToUpper())
                        {
                            //UploadExchangeRates uploadExchangeRates = new UploadExchangeRates();
                            await _uploadExchangeRates.UpdateExchangeRateUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.Booking.ToUpper())
                        {
                            //BookingExcelUpload bookingExcelUpload = new BookingExcelUpload();
                            await _bookingExcelUpload.BookingUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.VendorMaster.ToUpper())
                        {
                            //UploadVendorMaster uploadVendorMaster = new UploadVendorMaster();

                            await _uploadVendorMaster.VendorMasterUpload(dsUploadRecord);
                        }
                        else if (uploadType.ToUpper() == UploadMasterType.DCM.ToUpper())
                        {
                            //UploadDCM uploadDCMMaster = new UploadDCM();
                            await _uploadDCM.DCMUpload(dsUploadRecord);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// Method to remove empty/null rows from DataTable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public DataTable RemoveEmptyRows(DataTable dataTable)
        {
            dataTable.AcceptChanges();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                if (string.IsNullOrWhiteSpace(string.Join("", dataRow.ItemArray.Select(r => r.ToString()).ToArray())))
                {
                    dataRow.Delete();
                }
            }
            dataTable.AcceptChanges();

            return dataTable;
        }

        /// <summary>
        /// Validate file on the basis of file extension
        /// </summary>
        /// <returns>'true' when file is valid other wise 'false'</returns>
        internal bool IsFileValid(string UploadType, string FileName, string TemplatePath = "")
        {
            try
            {
                string FileExtension = Path.GetExtension(FileName).ToLower();

                if (UploadType == UploadMasterType.RateLine.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }

                    string[] allowedExtensions = { ".xls", ".xlsb", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.OtherCharges.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }

                    string[] allowedExtensions = { ".xls", ".xlsx", ".csv" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.CostMaster.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }

                    string[] allowedExtensions = { ".xls", ".xlsx", ".csv" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.Agent.ToString() || UploadType == UploadMasterType.AgentGeneralInfo.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }

                    string[] allowedExtensions = { ".xls", ".xlsx", ".csv" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.ShipperConsignee.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }

                    string[] allowedExtensions = { ".xls", ".xlsx", ".csv" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.FlightBudget.ToString() || UploadType == UploadMasterType.RouteControls.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }

                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }

                else if (UploadType == UploadMasterType.FlightSchedule.ToString())
                {
                    string[] allowedExtensions = { ".sim", ".ssim", ".dat" };
                    return CheckFileExtension(FileExtension, allowedExtensions);
                }
                else if (UploadType == UploadMasterType.FlightCapacity.ToString())
                {
                    string[] allowedExtensions = { ".xls", ".xlsx", ".csv" };
                    return CheckFileExtension(FileExtension, allowedExtensions);
                }
                else if (UploadType == UploadMasterType.CapacityAllocation.ToString())
                {
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension, allowedExtensions);
                }
                else if (UploadType == UploadMasterType.AgentUpdate.ToString())
                {
                    string[] allowedExtensions = { ".txt", ".json" };
                    return CheckFileExtension(FileExtension, allowedExtensions);
                }
                else if (UploadType == UploadMasterType.Templates.ToString())
                {
                    if (TemplatePath.Contains(FileName))
                        return true;
                    else
                        return false;

                }
                else if (UploadType == UploadMasterType.TaxLine.ToString())
                {
                    string[] allowedExtensions = { ".xls", ".xlsb", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.Airports.ToString() || UploadType == UploadMasterType.Partners.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.PartnerSchedule.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.ExchangeRate.ToString())
                {
                    string[] allowedExtensions = { ".xls", ".xlsb", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }

                else if (UploadType == UploadMasterType.IATAExchangeRate.ToString())
                {
                    string[] allowedExtensions = { ".txt" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.FlightScheduleExcel.ToString())
                {
                    string[] allowedExtensions = { ".xls", ".xlsb", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.PaxUpload.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.AircraftLoadingPattern.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.AirlineRoutes.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.User.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.FlightPaxInformation.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".txt" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.FlightPaxForecast.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".txt" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.ExchangeRateFromTo.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx", ".csv" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.PHCustomRegistry.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx", ".xlsb" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.Collection.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.CreditDebitNotes.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.MSRRates.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
                else if (UploadType == UploadMasterType.ExcelUploadBookingFFR.ToString())
                {
                    string OnlyFileName = Path.GetFileNameWithoutExtension(FileName);
                    if (OnlyFileName.Contains("-"))
                    {
                        // clsLog.WriteLogAzure("Please remove '-' sign from file name: " + FileName);
                        _logger.LogInformation("Please remove '-' sign from file name: {0}", FileName);
                        return false;
                    }
                    string[] allowedExtensions = { ".xls", ".xlsx" };
                    return CheckFileExtension(FileExtension.ToLower(), allowedExtensions);
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;
        }

        /// <summary>
        /// Check file extension for the file being uploaded with allowed extensions(different for different masters)
        /// </summary>
        /// <returns>'true' when extesion matched orherwise 'false'</returns>
        private bool CheckFileExtension(string FileExtension, string[] allowedExtensions)
        {
            bool IsFileOK = false;
            try
            {
                for (int i = 0; i < allowedExtensions.Length; i++)
                {
                    if (FileExtension == allowedExtensions[i])
                    {
                        IsFileOK = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return IsFileOK;
        }
    }

    /// <summary>
    /// Contains all masters type to be uploaded
    /// Note: Upload type must match to upload type field from table "UploadMasterConfiguration"
    /// </summary>
    public struct UploadMasterType
    {
        public const string RateLine = "Rate Line";
        public const string Agent = "Agent";
        public const string AgentGeneralInfo = "Agent General Info";
        public const string ShipperConsignee = "Shipper/Consignee";
        public const string OtherCharges = "Other Charges";
        public const string FlightBudget = "Flight Budget";
        public const string RouteControls = "Route Controls";
        public const string CostMaster = "Cost Master";
        public const string FlightSchedule = "Flight Schedule";
        public const string CapacityAllocation = "Capacity Allocation";
        public const string FlightCapacity = "Flight Capacity";
        public const string AgentUpdate = "Agent Update";
        public const string Templates = "Templates";
        public const string TaxLine = "Tax Line";
        public const string Airports = "Airports";
        public const string Partners = "Partners";
        public const string PartnerSchedule = "Partner Schedule";
        public const string FlightScheduleExcel = "Flight Schedule Excel";
        public const string PaxUpload = "Pax Excel";
        public const string FlightPaxInformation = "Flight Pax Information";
        public const string FlightPaxForecast = "Flight Pax Forecast";
        public const string AircraftLoadingPattern = "Aircraft Loading Pattern";
        public const string AirlineRoutes = "Airline Routes";
        public const string User = "User";

        public const string ExchangeRateFromTo = "Exchange Rates FromTo";
        public const string ExchangeRate = "Exchange Rates";
        public const string Booking = "Booking";
        public const string IATAExchangeRate = "IATA Exchange Rates";
        public const string PHCustomRegistry = "PH Custom Registry";
        public const string Collection = "Collection";
        public const string CreditDebitNotes = "Credit/Debit Notes";
        public const string MSRRates = "MSR Rates";
        public const string SISPayables = "SIS Payables";
        public const string VendorMaster = "Vendor Master";
        public const string ExcelUploadBookingFFR = "Excel Upload Booking FFR";
        public const string DCM = "DCM";
    }
}
