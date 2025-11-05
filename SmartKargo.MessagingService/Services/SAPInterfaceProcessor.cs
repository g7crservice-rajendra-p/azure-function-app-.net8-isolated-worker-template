using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Newtonsoft.Json;
using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole
{
    class SAPInterfaceProcessor
    {
        private string SFTPFingerPrint = string.Empty;

        private string StpFolerParth = string.Empty;

        private string SFTPPortNumber = string.Empty;

        private string GHAOutFolderPath = string.Empty;

        private string ppkFileName = string.Empty;

        private string ppkLocalFilePath = string.Empty;

        public string SFTPAddress = string.Empty;

        public string SFTPUserName = string.Empty;

        public string SFTPPassWord = string.Empty;

        private GenericFunction genericFunction = new GenericFunction();

        private FTP objftp = new FTP();

        private int portNumber = 0;

        private DataSet dsSAP = new DataSet();

        private DataRow drMsg = null;

        public SAPInterfaceProcessor()
        {
            dsSAP = GetSAPConfiguration();
            if (dsSAP != null)
            {
                if (dsSAP.Tables.Count > 0 && dsSAP.Tables[0].Rows.Count > 0)
                {
                    drMsg = dsSAP.Tables[0].Rows[0];
                    SFTPAddress = drMsg["FTPID"].ToString();
                    SFTPUserName = drMsg["FTPUserName"].ToString();
                    SFTPPassWord = drMsg["FTPPassword"].ToString();
                    ppkFileName = drMsg["PPKFileName"].ToString().Trim();
                    SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                    StpFolerParth = drMsg["RemotePath"].ToString();
                    SFTPPortNumber = drMsg["PortNumber"].ToString();
                    GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
                }
            }
            if (ppkFileName != string.Empty)
            {
                ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
            }
            if (SFTPPortNumber != string.Empty)
            {
                portNumber = Convert.ToInt32(SFTPPortNumber);
            }
        }
        public void GenerateSAPInterface(DateTime fromDate, DateTime toDate, string updatedby, DateTime updatedon)
        {
            SQLServer objsql = new SQLServer();
            byte[] byteArray = null;

            DataSet dsGetSapInfo = new DataSet("BALReveraInterface_dsGetSAPInfo");
            try
            {
                string[] pname = new string[4];
                pname[0] = "FromDate";
                pname[1] = "ToDate";
                pname[2] = "updatedby";
                pname[3] = "updatedon";

                object[] pvalue = new object[4];
                pvalue[0] = fromDate;
                pvalue[1] = toDate;
                pvalue[2] = updatedby;
                pvalue[3] = updatedon;

                SqlDbType[] ptype = new SqlDbType[4];
                ptype[0] = SqlDbType.DateTime;
                ptype[1] = SqlDbType.DateTime;
                ptype[2] = SqlDbType.VarChar;
                ptype[3] = SqlDbType.DateTime;

                dsGetSapInfo = objsql.SelectRecords("USPSAPInterface", pname, pvalue, ptype);

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
                                String BlobFileURL = string.Empty;
                                String FileNameFormat = "SAP_" + DateTime.Now.ToString("ddMMyyyy_hh.mm");
                               
                                objDT = dsGetSapInfo.Tables[index].Copy();
                                objDs.Tables.Add(objDT);
                                //Convert Datatable data into Json String
                                jsonString = JsonConvert.SerializeObject(objDT);
                                // Convert Json String to Stream
                                byteArray = Encoding.ASCII.GetBytes(jsonString.ToString());
                                MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(jsonString.ToString()));
                                //  upload to Blob
                                BlobFileURL = UploadToBlob(mStream, FileNameFormat + ".json", "sapinterface");

                                //  upload to sftp
                                if (objftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, jsonString, FileNameFormat, ".json", StpFolerParth, portNumber, ppkLocalFilePath))
                                {    //Save last SAP generated date
                                    SQLServer sQLServer = new SQLServer();
                                    sQLServer.ExecuteProcedure("usp_LastUpdateBITableLog");
                                    // save log details
                                    SetSAPFileLog(FileNameFormat, BlobFileURL, DateTime.Now);
                                }

                            }
                            #endregion
                        }

                    }
                    else
                    {
                        clsLog.WriteLogAzure(isFileGenerated);
                    }

                }
            }
            catch (Exception ex)
            {
                if (dsGetSapInfo != null)
                    dsGetSapInfo.Dispose();
                clsLog.WriteLogAzure(ex);
            }
        }

       
        public void SetSAPFileLog(string FileName, string FileURL, DateTime CreatedOn)
        {

            try
            {
                SQLServer sQLServer = new SQLServer();
                sQLServer.SelectRecords("SP_SetSAPFileLog", new string[3]
                {
                "FileName",
                "FileURL",
                "CreatedOn"
                }, new object[3]
                {
                FileName,
                FileURL,
                CreatedOn
                }, new SqlDbType[3]
                {
                SqlDbType.VarChar,
                SqlDbType.VarChar,
                SqlDbType.DateTime
                });
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }

        }

        public static string UploadToBlob(Stream stream, string fileName, string containerName)
        {
            try
            {
                GenericFunction genericFunction = new GenericFunction();
                string BlobName = Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageName")) == "" ? "" : Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageName"));
                string BlobKey = Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageKey")) == "" ? "" : Convert.ToString(genericFunction.ReadValueFromDb("BlobStorageKey"));
                containerName = containerName.ToLower();
                StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(BlobName, BlobKey);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                CloudBlobClient sasBlobClient = new CloudBlobClient(storageAccount.BlobEndpoint, cred);
                CloudBlob blob = sasBlobClient.GetBlobReference(containerName + @"/" + fileName);
                blob.Properties.ContentType = "";
                blob.Metadata["FileName"] = fileName;
                blob.UploadFromStream(stream);
                return "https://" + BlobName + ".blob.core.windows.net/" + containerName + "/" + fileName;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return null;
            }
        }


       
        public DataSet GetSAPConfiguration()
        {
            DataSet result = new DataSet();
            try
            {
                SQLServer sQLServer = new SQLServer();
                result = sQLServer.SelectRecords("spGetSAPConfiguration", new string[1]
                {
                "MessageType"
                }, new object[1]
                {
                "SAPInterface"
                }, new SqlDbType[1]
                {
                SqlDbType.VarChar
                });
                return result;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                return result;
            }
        }

    }
}
