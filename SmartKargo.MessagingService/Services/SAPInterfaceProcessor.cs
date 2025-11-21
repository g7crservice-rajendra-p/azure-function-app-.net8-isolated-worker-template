using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.Net;
using System.Text;

namespace QidWorkerRole
{
    class SAPInterfaceProcessor
    {
        //private string SFTPFingerPrint = string.Empty;

        //private string StpFolerParth = string.Empty;

        //private string SFTPPortNumber = string.Empty;

        //private string GHAOutFolderPath = string.Empty;

        //private string ppkFileName = string.Empty;

        //private string ppkLocalFilePath = string.Empty;

        //public string SFTPAddress = string.Empty;

        //public string SFTPUserName = string.Empty;

        //public string SFTPPassWord = string.Empty;

        //private GenericFunction genericFunction = new GenericFunction();

        //private FTP objftp = new FTP();

        //private int portNumber = 0;

        //private DataSet dsSAP = new DataSet();

        //private DataRow drMsg = null;


        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<SAPInterfaceProcessor> _logger;
         private static ILoggerFactory? _loggerFactory;
        private static ILogger<SAPInterfaceProcessor> _staticLogger => _loggerFactory?.CreateLogger<SAPInterfaceProcessor>();

        private readonly FTP _fTP;
        private readonly GenericFunction _genericFunction;

        public SAPInterfaceProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<SAPInterfaceProcessor> logger,
            ILoggerFactory loggerFactory,
            FTP fTP,
            GenericFunction genericFunction
         )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _loggerFactory = loggerFactory;
            _fTP = fTP;
            _genericFunction = genericFunction;

            //dsSAP = GetSAPConfiguration();
            //if (dsSAP != null)
            //{
            //    if (dsSAP.Tables.Count > 0 && dsSAP.Tables[0].Rows.Count > 0)
            //    {
            //        drMsg = dsSAP.Tables[0].Rows[0];
            //        SFTPAddress = drMsg["FTPID"].ToString();
            //        SFTPUserName = drMsg["FTPUserName"].ToString();
            //        SFTPPassWord = drMsg["FTPPassword"].ToString();
            //        ppkFileName = drMsg["PPKFileName"].ToString().Trim();
            //        SFTPFingerPrint = drMsg["FingerPrint"].ToString();
            //        StpFolerParth = drMsg["RemotePath"].ToString();
            //        SFTPPortNumber = drMsg["PortNumber"].ToString();
            //        GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
            //    }
            //}
            //if (ppkFileName != string.Empty)
            //{
            //    ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
            //}
            //if (SFTPPortNumber != string.Empty)
            //{
            //    portNumber = Convert.ToInt32(SFTPPortNumber);
            //}
        }

        public async Task GenerateSAPInterface(DateTime fromDate, DateTime toDate, string updatedby, DateTime updatedon)
        {
            //SQLServer objsql = new SQLServer();
            byte[] byteArray = null;

            DataSet? dsGetSapInfo = new DataSet("BALReveraInterface_dsGetSAPInfo");
            try
            {
                //string[] pname = new string[4];
                //pname[0] = "FromDate";
                //pname[1] = "ToDate";
                //pname[2] = "updatedby";
                //pname[3] = "updatedon";

                //object[] pvalue = new object[4];
                //pvalue[0] = fromDate;
                //pvalue[1] = toDate;
                //pvalue[2] = updatedby;
                //pvalue[3] = updatedon;

                //SqlDbType[] ptype = new SqlDbType[4];
                //ptype[0] = SqlDbType.DateTime;
                //ptype[1] = SqlDbType.DateTime;
                //ptype[2] = SqlDbType.VarChar;
                //ptype[3] = SqlDbType.DateTime;

                SqlParameter[] parameters =
                [
                    new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = fromDate },
                    new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = toDate },
                    new SqlParameter("@updatedby", SqlDbType.VarChar) { Value = updatedby },
                    new SqlParameter("@updatedon", SqlDbType.DateTime) { Value = updatedon }
                ];

                //dsGetSapInfo = objsql.SelectRecords("USPSAPInterface", pname, pvalue, ptype);
                dsGetSapInfo = await _readWriteDao.SelectRecords("USPSAPInterface", parameters);

                if (dsGetSapInfo != null && dsGetSapInfo.Tables.Count > 0 && dsGetSapInfo.Tables[0].Rows.Count > 0)
                {
                    string isFileGenerated = string.Empty;
                    isFileGenerated = dsGetSapInfo.Tables[0].Rows[0]["StatusMsg"].ToString();

                    if (!isFileGenerated.Contains("FILESALREADYGENERATED"))
                    {
                        if (dsGetSapInfo.Tables[1] != null && dsGetSapInfo.Tables[1].Rows.Count > 0)
                        {
                            #region Code to Convert Data Table to json File

                            for (int index = 1; index < dsGetSapInfo.Tables.Count; index++)
                            {
                                DataSet objDs = new DataSet("objDsSAPInterfaceJson");
                                DataTable objDT = new DataTable("objDTSAPInterfaceJson");
                                string jsonString = string.Empty;
                                string? BlobFileURL = string.Empty;
                                string FileNameFormat = "SAP_" + DateTime.Now.ToString("ddMMyyyy_hh.mm");

                                objDT = dsGetSapInfo.Tables[index].Copy();
                                objDs.Tables.Add(objDT);
                                //Convert Datatable data into Json String
                                jsonString = JsonConvert.SerializeObject(objDT);
                                // Convert Json String to Stream
                                byteArray = Encoding.ASCII.GetBytes(jsonString.ToString());
                                MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(jsonString.ToString()));
                                //  upload to Blob
                                BlobFileURL = UploadToBlob(mStream, FileNameFormat + ".json", "sapinterface");

                                /**Fetch SFTP information and upload*/
                                string SFTPAddress = string.Empty;
                                string SFTPUserName = string.Empty;
                                string SFTPPassWord = string.Empty;
                                string SFTPFingerPrint = string.Empty;
                                string StpFolerParth = string.Empty;
                                string ppkFileName = string.Empty;
                                string ppkLocalFilePath = string.Empty;
                                string SFTPPortNumber = string.Empty;
                                int portNumber = 0;

                                DataSet? dsSAP = await GetSAPConfiguration();

                                if (dsSAP != null)
                                {
                                    if (dsSAP.Tables.Count > 0 && dsSAP.Tables[0].Rows.Count > 0)
                                    {
                                        var drMsg = dsSAP.Tables[0].Rows[0];

                                        SFTPAddress = drMsg["FTPID"].ToString();
                                        SFTPUserName = drMsg["FTPUserName"].ToString();
                                        SFTPPassWord = drMsg["FTPPassword"].ToString();
                                        SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                                        StpFolerParth = drMsg["RemotePath"].ToString();
                                        ppkFileName = drMsg["PPKFileName"].ToString().Trim();
                                        SFTPPortNumber = drMsg["PortNumber"].ToString();
                                    }
                                }

                                if (ppkFileName != string.Empty)
                                {
                                    ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                                }
                                if (SFTPPortNumber != string.Empty)
                                {
                                    portNumber = Convert.ToInt32(SFTPPortNumber);
                                }

                                //  upload to sftp
                                //if (objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, jsonString, FileNameFormat, ".json", StpFolerParth, portNumber, ppkLocalFilePath))
                                if (_fTP.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, jsonString, FileNameFormat, ".json", StpFolerParth, portNumber, ppkLocalFilePath))

                                {    //Save last SAP generated date

                                    //SQLServer sQLServer = new SQLServer();
                                    //sQLServer.ExecuteProcedure("usp_LastUpdateBITableLog");

                                    await _readWriteDao.ExecuteNonQueryAsync("usp_LastUpdateBITableLog");

                                    // save log details
                                    await SetSAPFileLog(FileNameFormat, BlobFileURL, DateTime.Now);
                                }

                            }
                            #endregion
                        }

                    }
                    else
                    {
                        // clsLog.WriteLogAzure(isFileGenerated);
                        _logger.LogWarning(isFileGenerated);
                    }

                }
            }
            catch (Exception ex)
            {
                if (dsGetSapInfo != null)
                    dsGetSapInfo.Dispose();
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }


        public async Task SetSAPFileLog(string FileName, string FileURL, DateTime CreatedOn)
        {

            try
            {
                //SQLServer sQLServer = new SQLServer();
                //sQLServer.SelectRecords("SP_SetSAPFileLog", new string[3]
                //{
                //"FileName",
                //"FileURL",
                //"CreatedOn"
                //}, new object[3]
                //{
                //FileName,
                //FileURL,
                //CreatedOn
                //}, new SqlDbType[3]
                //{
                //SqlDbType.VarChar,
                //SqlDbType.VarChar,
                //SqlDbType.DateTime
                //});

                SqlParameter[] parameters =
                [
                    new SqlParameter("@FileName", SqlDbType.VarChar) { Value = FileName },
                    new SqlParameter("@FileURL", SqlDbType.VarChar) { Value = FileURL },
                    new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = CreatedOn }
                ];
                await _readWriteDao.SelectRecords("SP_SetSAPFileLog", parameters);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

        }

        public static string? UploadToBlob(Stream stream, string fileName, string containerName)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                //string BlobName = Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageName")) == "" ? "" : Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageName"));
                //string BlobKey = Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageKey")) == "" ? "" : Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageKey"));

                string BlobName = ConfigCache.Get("BlobStorageName");
                string BlobKey = ConfigCache.Get("BlobStorageKey");

                containerName = containerName.ToLower();

                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(BlobName, BlobKey);
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //CloudBlobClient sasBlobClient = new CloudBlobClient(storageAccount.BlobEndpoint, cred);
                //CloudBlob blob = sasBlobClient.GetBlobReference(containerName + @"/" + fileName);
                //blob.Properties.ContentType = "";
                //blob.Metadata["FileName"] = fileName;
                //blob.UploadFromStream(stream);
                //return "https://" + BlobName + ".blob.core.windows.net/" + containerName + "/" + fileName;


                if (string.IsNullOrWhiteSpace(BlobName) || string.IsNullOrWhiteSpace(BlobKey))
                    return null;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var blobServiceClient = new BlobServiceClient(
                    new Uri($"https://{BlobName}.blob.core.windows.net"),
                    new StorageSharedKeyCredential(BlobName, BlobKey)
                );

                BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);

                BlobClient blob = container.GetBlobClient(fileName);
                blob.SetHttpHeaders(new BlobHttpHeaders { ContentType = "" });
                blob.SetMetadata(new Dictionary<string, string> { { "FileName", fileName } });

                stream.Position = 0;
                blob.Upload(stream, overwrite: true);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }



        public async Task<DataSet?> GetSAPConfiguration()
        {
            DataSet? result = new DataSet();
            try
            {
                ///SQLServer sQLServer = new SQLServer();
                //result = sQLServer.SelectRecords("spGetSAPConfiguration", new string[1]
                //{
                //    "MessageType"
                //}, new object[1]
                //{
                //"SAPInterface"
                //}, new SqlDbType[1]
                //{
                //SqlDbType.VarChar
                //});

                SqlParameter[] parameters = [
                    new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = "SAPInterface" }
                ];
                result = await _readWriteDao.SelectRecords("spGetSAPConfiguration", parameters);
                return result;
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return result;
            }
        }

    }
}
