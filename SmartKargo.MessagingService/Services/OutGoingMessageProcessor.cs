using System;
using System.Linq;
using System.Data;
using QID.DataAccess;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Configuration;
using System.Net;

namespace QidWorkerRole
{

    public class OutGoingMessageProcessor
    {
        GenericFunction genericFunction = new GenericFunction();
        string accountEmail = string.Empty;
        string password = string.Empty;
        string MailInServer = string.Empty;
        string SFTPAddress = string.Empty;
        string SFTPUserName = string.Empty;
        string SFTPPassWord = string.Empty;
        public bool f_SendOutgoingMail = true;
        public string BlobKey = string.Empty;
        public string BlobName = string.Empty;

        SCMExceptionHandlingWorkRole scmexception = new SCMExceptionHandlingWorkRole();
        public OutGoingMessageProcessor()
        {

        }

        #region OutGoing Method to send Message to Sita and Mail and Upload File to FTP and Azzure Drive

        /// <summary>
        /// 
        /// </summary>

        public void SendOutGoingMessageToSKClient()
        {

            try
            {

                int outport = 0;
                bool isOn = false, isMessageSent = false, ishtml = false;
                f_SendOutgoingMail = Convert.ToBoolean(genericFunction.ReadValueFromDb("msgService_SendEmail"));
                SQLServer objsql = new SQLServer();

                do
                {
                    isMessageSent = false;
                    ishtml = false;
                    string MailsendPort = string.Empty, subject = string.Empty, msgCommType = string.Empty, FileName = string.Empty, body = string.Empty, sentadd = string.Empty, ccadd = string.Empty, status = string.Empty, FileExtension = string.Empty, ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty;
                    status = "Processed";
                    isOn = false;
                    DataSet ds = null;
                    ds = objsql.SelectRecords("spMailtoSend");
                    if (ds != null && ds.Tables.Count > 0)
                    {

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            isOn = true;
                            isMessageSent = false;
                            DataRow dr = ds.Tables[0].Rows[0];
                            subject = dr[1].ToString();
                            msgCommType = "EMAIL";
                            DataRow drMsg = null;
                            FileName = dr["Subject"].ToString();
                            body = dr[2].ToString();
                            sentadd = dr[4].ToString().Trim(',');
                            if (dr[3].ToString().Length > 3)
                                ccadd = dr[3].ToString().Trim(',');
                            ishtml = Convert.ToBoolean(dr["ishtml"].ToString() == "" ? "False" : dr["ishtml"].ToString());

                            if (ds.Tables[2].Rows.Count > 0)
                            {
                                drMsg = ds.Tables[2].Rows[0];
                                msgCommType = drMsg["MsgCommType"].ToString().ToUpper().Trim();
                                FileExtension = drMsg["FileExtension"].ToString().ToUpper().Trim() == "" ? "SND" : drMsg["FileExtension"].ToString().ToUpper();
                            }

                            if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
                                status = "Processed";
                            if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                                status = "Re-Processed";

                            #region FTP File Upload

                            if (msgCommType.ToUpper() == "FTP" || msgCommType == "ALL")
                            {

                                FTP objFtp = new FTP();
                                if (drMsg != null && drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
                                {
                                    ftpUrl = drMsg["FTPID"].ToString();
                                    ftpUserName = drMsg["FTPUserName"].ToString();
                                    ftpPassword = drMsg["FTPPassword"].ToString();

                                }
                                else
                                {
                                    ftpUrl = genericFunction.ReadValueFromDb("FTPURLofFileUpload");
                                    ftpUserName = genericFunction.ReadValueFromDb("FTPUserofFileUpload");
                                    ftpPassword = genericFunction.ReadValueFromDb("FTPPasswordofFileUpload");
                                }
                                if (FileName.ToUpper().Contains(".TXT"))
                                {
                                    int fileIndex = FileName.IndexOf(".");
                                    if (fileIndex > 0)
                                    {
                                        FileName = FileName.Substring(0, fileIndex);
                                        FileExtension = FileName.Substring(fileIndex, 3);
                                    }
                                }
                                else
                                    FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
                                if (ftpUrl != "" && ftpUserName != "" && ftpPassword != "")
                                {
                                    if (objFtp.UploadfileThrougFTPAndRenamefileAfterUploaded(ftpUrl, ftpUserName, ftpPassword, body, FileName, FileExtension))
                                    {
                                        isMessageSent = true;
                                        string[] pname = { "num", "Status" };
                                        object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                        SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                        if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                            clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
                                    }
                                    else
                                        isMessageSent = false;
                                }


                            }
                            #endregion

                            #region Email
                            if ((msgCommType == "EMAIL" || msgCommType == "ALL") && (f_SendOutgoingMail == true))
                            {

                                accountEmail = genericFunction.ReadValueFromDb("msgService_EmailId");
                                password = genericFunction.ReadValueFromDb("msgService_EmailPassword");
                                string MailIouterver = genericFunction.ReadValueFromDb("msgService_EmailOutServer");
                                MailsendPort = genericFunction.ReadValueFromDb("msgService_OutgoingMessagePort");

                                if (MailsendPort != "")
                                    outport = int.Parse(MailsendPort == "" ? "110" : MailsendPort);
                                else
                                    outport = int.Parse(Convert.ToString(genericFunction.ReadValueFromDb("OutPort")));//ConfigurationManager.AppSettings["OutPort"].ToString()
                                #region Email
                                EMAILOUT objmail = new EMAILOUT();
                                if (sentadd.Length > 2 && sentadd.Contains("@") && sentadd.Contains("."))
                                {

                                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                                    {

                                        if (ds.Tables[1].Rows[0][0].ToString().ToUpper() != "NA")
                                        {
                                            #region Mail with attachment
                                            clsLog.WriteLogAzure("Inside Table 1 for MessageID :" + dr[0].ToString());
                                            if (ds.Tables[1].Rows.Count > 0)
                                            {
                                                clsLog.WriteLogAzure("Inside Table 1 Rows for MessageID :" + dr[0].ToString());
                                                MemoryStream[] Attachments = new MemoryStream[0];
                                                string[] Extensions = new string[0];
                                                string[] AttachmentName = new string[0];
                                                foreach (DataRow drow in ds.Tables[1].Rows)
                                                {
                                                    try
                                                    {
                                                        Array.Resize(ref Attachments, Attachments.Length + 1);
                                                        Attachments[Attachments.Length - 1] = new MemoryStream(DownloadBlob(drow["FileUrl"].ToString()));

                                                        Array.Resize(ref Extensions, Extensions.Length + 1);
                                                        Extensions[Extensions.Length - 1] = drow["MIMEType"].ToString();
                                                        if (!Extensions[Extensions.Length - 1].Contains('.'))
                                                            Extensions[Extensions.Length - 1] = "." + Extensions[Extensions.Length - 1];
                                                        Array.Resize(ref AttachmentName, AttachmentName.Length + 1);
                                                        AttachmentName[AttachmentName.Length - 1] = drow["AttachmentName"].ToString();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        clsLog.WriteLogAzure(ex);
                                                        clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());

                                                    }

                                                }
                                                if (accountEmail != "" && password != "" && sentadd != "" && body != "")
                                                {
                                                    if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, Attachments, AttachmentName, Extensions))
                                                    {
                                                        isMessageSent = true;
                                                        clsLog.WriteLogAzure("After Sending Mail with Attachment for MessageID :" + dr[0].ToString());
                                                        string[] pname = { "num", "Status" };
                                                        object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                        SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                        if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                            clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());

                                                    }
                                                    else
                                                    {
                                                        isMessageSent = false;
                                                        clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());

                                                    }
                                                }


                                            }

                                            #endregion Mail with attachment
                                        }
                                        else
                                        {
                                            #region Send Email
                                            if (accountEmail != "" && password != "" && sentadd != "" && body != "")
                                            {
                                                if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, ""))
                                                {
                                                    isMessageSent = true;
                                                    string[] pname = { "num", "Status" };
                                                    object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                        clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());

                                                }
                                                else
                                                {
                                                    isMessageSent = false;
                                                    clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());

                                                }
                                            }
                                            #endregion send email
                                        }
                                    }
                                    else
                                    {
                                        #region Send Email
                                        if (accountEmail != "" && password != "" && sentadd != "" && body != "")
                                        {

                                            if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, ""))
                                            {
                                                isMessageSent = true;
                                                string[] pname = { "num", "Status" };
                                                object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                {
                                                    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                }
                                            }
                                            else
                                            {
                                                isMessageSent = false;
                                                clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());

                                            }
                                        }
                                        #endregion send email
                                    }
                                }
                                #endregion

                            }

                            #endregion

                            #region SFTP Upload
                            if (msgCommType.ToUpper() == "SFTP" || msgCommType.ToUpper() == "ALL" || msgCommType.Equals("SFTP", StringComparison.OrdinalIgnoreCase))
                            {
                                FTP objFtp = new FTP();
                                string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty;
                                if (drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "" && drMsg["FingerPrint"].ToString() != "" && drMsg["RemotePath"].ToString() != "")
                                {
                                    SFTPAddress = drMsg["FTPID"].ToString();
                                    SFTPUserName = drMsg["FTPUserName"].ToString();
                                    SFTPPassWord = drMsg["FTPPassword"].ToString();
                                    SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                                    StpFolerParth = drMsg["RemotePath"].ToString();
                                }
                                else
                                {
                                    SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
                                    SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
                                    SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
                                    SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
                                    StpFolerParth = genericFunction.ReadValueFromDb("msgService_OUT_FolderPath");
                                    SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");
                                }
                                if (FileName.ToUpper().Contains(".TXT"))
                                {
                                    int fileIndex = FileName.IndexOf(".");
                                    if (fileIndex > 0)
                                        FileName = FileName.Substring(0, fileIndex);

                                }
                                else if (FileName.ToUpper().Contains(".XML"))
                                {
                                    int fileIndex = FileName.IndexOf(".");
                                    if (fileIndex > 0)
                                    {
                                        FileName = FileName.Substring(0, fileIndex);
                                        FileExtension = "XML";
                                    }
                                }
                                else
                                    FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");


                                if (SFTPAddress != "" && SFTPUserName != "" && SFTPPassWord != "" && SFTPFingerPrint != "" && StpFolerParth != "" && SFTPPortNumber.Trim() != string.Empty)
                                {
                                    int portNumber = Convert.ToInt32(SFTPPortNumber);
                                    if (objFtp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, body, FileName, FileExtension, StpFolerParth, portNumber, string.Empty))
                                    {
                                        isMessageSent = true;
                                        string[] pname = { "num", "Status" };
                                        object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                        SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                        if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                            clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());

                                    }
                                }

                            }
                            #endregion

                            #region AZURE DRIVE
                            if (msgCommType == "DRIVE" || msgCommType == "ALL")
                            {
                                AzureDrive drive = new AzureDrive();
                                if (drive.UploadToDrive(DateTime.Now.ToString("yyyyMMdd_hhmmss_fff"), body,""))
                                {
                                    isMessageSent = true;
                                    string[] pname = { "num", "Status" };
                                    object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                        clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dr[0].ToString());

                                }
                                else
                                    isMessageSent = false;

                            }
                            #endregion

                            if (!isMessageSent)
                            {
                                string[] pname = { "num", "Status", "ErrorMsg" };
                                object[] pvalue = { int.Parse(dr[0].ToString()), status, "Error occured while processing sending request." };
                                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar };
                                if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                    clsLog.WriteLogAzure("Email not Sent successfully to:" + dr[0].ToString());
                            }
                        }

                    }
                    else
                        isOn = false;

                } while (isOn);

                objsql = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                clsLog.WriteLogAzure("Error found:", ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numChars"></param>
        /// <returns></returns>
        //private string GetRandomNumbers(int numChars)
        //{
        //    string[] chars = {
        //                         "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "P", "Q", "R",
        //                         "S",
        //                         "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        //                     };
        //    var rnd = new Random();
        //    string random = string.Empty;
        //    for (int i = 0; i < numChars; i++)
        //    {
        //        random += chars[rnd.Next(0, 33)];
        //    }
        //    return random;
        //}


        #region AzzureDownloadBlob

        private byte[] DownloadBlobX(string filenameOrUrl)
        {
            try
            {

                string containerName = ""; //container must be lowercase, no special characters
                if (filenameOrUrl.Contains('/'))
                {
                    filenameOrUrl = filenameOrUrl.ToLower();
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }

                byte[] downloadStream = null;
                StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey("hawaiianstorage", "wcafYuM5usLvBUfQ642acJs41ZCOe6ZlGIFt3PFT2xooLfwTiZpKS+Fs73m7cmfwUN1BAxBYfLcpBsicwoRe8A==");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);

                //get a reference to the blob
                CloudBlob blob = blobClient.GetBlobReference("/attachments/2.png");//(string.Format("{0}/{1}", containerName, filenameOrUrl));

                //write the file to the http response
                //blob.DownloadToStream(downloadStream);
                downloadStream = blob.DownloadByteArray();
                //FetchAttributes();

                return downloadStream;

            }
            catch (Exception)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                return null;
            }

        }

        private byte[] DownloadBlob(string filenameOrUrl)
        {
            try
            {

                string containerName = ""; //container must be lowercase, no special characters
                if (filenameOrUrl.Contains('/'))
                {
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    containerName = containerName.ToLower();
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }

                byte[] downloadStream = null;
                StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), genericFunction.GetStorageKey());
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);

                //get a reference to the blob
                //containerName = "attachments";
                //filenameOrUrl = "2.jpg";
                CloudBlob blob = blobClient.GetBlobReference(string.Format("{0}/{1}", containerName, filenameOrUrl));

                //write the file to the http response
                //blob.DownloadToStream(downloadStream);
                downloadStream = blob.DownloadByteArray();
                //FetchAttributes();

                return downloadStream;

            }
            catch (Exception)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                return null;
            }

        }

        private string getStorageName()
        {
            try
            {
                if (string.IsNullOrEmpty(BlobName))
                {
                    BlobName = genericFunction.ReadValueFromDb("BlobStorageName");
                }
            }
            catch (Exception)
            {
                //SCMExceptionHandling.logexception(ref objEx);
            }
            return BlobName;
        }

        #endregion



        #endregion
    }
}
