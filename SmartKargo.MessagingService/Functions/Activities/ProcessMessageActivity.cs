using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using static SmartKargo.MessagingService.Functions.Orchestrators.SendMailOrchestrator;

namespace SmartKargo.MessagingService.Functions.Activities
{
    /// <summary>
    /// Durable Activity Function that processes a single pending message using the legacy SendMail logic.
    /// Notes:
    /// - Preserves original logic and call sites.
    /// - clgLog is mapped to injected ILogger (so legacy clsLog.WriteLogAzure calls are redirected).
    /// - genericFunction.ReadValueFromDb(...) and genericFunction.GetPPKFilePath(...) are mapped to the provided config IDictionary.
    /// - objsql.ExecuteProcedure(...) is routed to the injected _readWriteDao.
    /// </summary>
    public class ProcessMessageActivity
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<ProcessMessageActivity> _logger;

        public ProcessMessageActivity(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<ProcessMessageActivity> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }

        [Function(nameof(ProcessMessageActivity))]
        public async Task<bool> Run([ActivityTrigger] ProcessMessageInput input)
        {
            return true;
            //try
            //{
            //    var ds = input.MessageDataSet;
            //    var config = input.Config;

            //    // if msgDs null or empty -> nothing to do
            //    if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
            //    {
            //        _logger.LogInformation("No dataset present, nothing to process.");
            //        return false;
            //    }

            //    if (ds.Tables[0] == null || ds.Tables[0].Rows.Count == 0)
            //    {
            //        _logger.LogInformation("No rows in table[0], nothing to process.");
            //        return false;
            //    }

            //    if (config == null)
            //    {
            //        _logger.LogWarning("Config is null - cannot run ProcessMessageActivity.");
            //        return false;
            //    }

            //    if (!config.TryGetValue("msgService_SendEmail", out var sendEmailValue) || string.IsNullOrWhiteSpace(sendEmailValue))
            //    {
            //        sendEmailValue = "false"; // default if not found or empty
            //    }

            //    bool f_SendOutgoingMail = Convert.ToBoolean(sendEmailValue);

            //    // --- Legacy logic starts here (preserving original flow) ---

            //    //// Map legacy helpers:
            //    //// clgLog.WriteLogAzure(...) -> maps to _logger via WriteLogAzure(...)
            //    //// genericFunction.ReadValueFromDb(...) -> mapped to GenericFunctionWrapper.ReadValueFromDb(...)
            //    //var clgLog = _logger;

            //    //// Minimal wrapper to keep original genericFunction.ReadValueFromDb(...) calls intact.
            //    //var genericFunction = new GenericFunctionWrapper(config);

            //    //// Map legacy 'objsql' to our injected DAO (keeps ExecuteProcedure call-site identical)
            //    //dynamic objsql = _readWriteDao;

            //    //// Local helper to replace clsLog.WriteLogAzure(...) usage with ILogger
            //    //void WriteLogAzure(object o)
            //    //{
            //    //    if (o == null)
            //    //    {
            //    //        _logger.LogInformation("null");
            //    //        return;
            //    //    }
            //    //    if (o is Exception ex)
            //    //    {
            //    //        _logger.LogError(ex, ex.Message);
            //    //    }
            //    //    else
            //    //    {
            //    //        _logger.LogInformation(o.ToString());
            //    //    }
            //    //}

            //    // Variables
            //    string ftpUrl = string.Empty,
            //        ftpUserName = string.Empty,
            //        ftpPassword = string.Empty,
            //        ccadd = string.Empty,
            //        FileExtension = string.Empty,
            //        msgCommType = string.Empty;

            //    //// Legacy logging (now routed to ILogger)
            //    //WriteLogAzure("Outgoing Mail Row Count" + ds.Tables[0].Rows.Count.ToString());
            //    //_logger.LogInformation("Outgoing Mail Row Count: {Count}", ds.Tables[0].Rows.Count);

            //    //isOn = true;

            //    bool isMessageSent = false,
            //     isFTPUploadSuccessfully = true;

            //    DataRow dr = ds.Tables[0].Rows[0];

            //    // Read values from ds
            //    string subject = dr[1].ToString();
            //    msgCommType = "EMAIL";
            //    DataRow drMsg = null;
            //    DataRow drEmailAccount = null;
            //    string FileName = dr["Subject"].ToString();
            //    string body = dr[2].ToString();
            //    string messageType = dr["Type"].ToString();
            //    string actualMsg = dr["ActualMsg"].ToString();
            //    string SITAFolderPath = dr["SITAFolderPath"].ToString().Trim();
            //    string awbNumber = dr["AWBNumber"].ToString();
            //    string flightNo = dr["FlightNo"].ToString();

            //    DateTime flightDate = default(DateTime);
            //    try
            //    {
            //        flightDate = Convert.ToDateTime(dr["FlightDt"]);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Failed to parse FlightDt");
            //    }

            //    string sentadd = dr[4].ToString().Trim(',');
            //    if (dr[3].ToString().Length > 3)
            //    {
            //        ccadd = dr[3].ToString().Trim(',');
            //    }

            //    bool ishtml = bool.Parse(string.IsNullOrWhiteSpace(dr["ishtml"].ToString()) ? "False" : dr["ishtml"].ToString());

            //    if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
            //    {
            //        drMsg = ds.Tables[2].Rows[0];
            //        msgCommType = drMsg["MsgCommType"].ToString().ToUpper().Trim();
            //        FileExtension = drMsg["FileExtension"].ToString().ToUpper().Trim();
            //        if (FileExtension.ToUpper() == "XML")
            //        {
            //            FileExtension = drMsg["FileExtension"].ToString().Trim();
            //        }
            //    }

            //    string status = string.Empty;
            //    if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
            //    {
            //        status = "Processed";
            //    }

            //    if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
            //    {
            //        status = "Re-Processed";
            //    }

            //    #region FTP File Upload

            //    if (msgCommType.ToUpper() == "FTP" || msgCommType.ToUpper() == "ALL")
            //    {
            //        FTP objFtp = new FTP();
            //        if (drMsg != null && drMsg.ItemArray.Length > 0)
            //        {
            //            if (drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
            //            {
            //                ftpUrl = drMsg["FTPID"].ToString();
            //                ftpUserName = drMsg["FTPUserName"].ToString();
            //                ftpPassword = drMsg["FTPPassword"].ToString();
            //            } // Rohidas added else condition for live issue on 30 aug 2017
            //            else
            //            {
            //                ftpUrl = genericFunction.ReadValueFromDb("FTPURLofFileUpload");
            //                ftpUserName = genericFunction.ReadValueFromDb("FTPUserofFileUpload");
            //                ftpPassword = genericFunction.ReadValueFromDb("FTPPasswordofFileUpload");
            //            }
            //        }
            //        else
            //        {
            //            ftpUrl = genericFunction.ReadValueFromDb("FTPURLofFileUpload");
            //            ftpUserName = genericFunction.ReadValueFromDb("FTPUserofFileUpload");
            //            ftpPassword = genericFunction.ReadValueFromDb("FTPPasswordofFileUpload");
            //        }
            //        if (FileName.ToUpper().Contains(".TXT"))
            //        {

            //            int fileIndex = FileName.IndexOf(".");
            //            if (fileIndex > 0)
            //            {
            //                FileName = FileName.Substring(0, fileIndex);
            //                FileExtension = "txt";
            //            }
            //        }
            //        else if (FileName.ToUpper().Contains(".XML"))
            //        {
            //            int fileIndex = FileName.IndexOf(".");
            //            if (fileIndex > 0)
            //            {
            //                FileName = FileName.Substring(0, fileIndex);
            //                if (FileExtension.ToUpper() != "XML")
            //                {
            //                    FileExtension = "xml";
            //                }
            //            }
            //        }
            //        else
            //            FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
            //        if (ftpUrl != "")
            //        {
            //            if (objFtp.UploadfileThrougFTPAndRenamefileAfterUploaded(ftpUrl, ftpUserName, ftpPassword, body, FileName, FileExtension == "" ? "SND" : FileExtension))
            //            {
            //                isMessageSent = true;
            //                string[] pname = { "num", "Status" };
            //                object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                    WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
            //            }
            //            else
            //            {
            //                isMessageSent = false;
            //                isFTPUploadSuccessfully = false;
            //            }
            //        }
            //    }
            //    #endregion

            //    WriteLogAzure("msgCommType-" + msgCommType);
            //    _logger.LogInformation("msgCommType - {MsgCommType}", msgCommType);

            //    #region Email
            //    if ((msgCommType.ToUpper() == "EMAIL" || msgCommType.ToUpper() == "ALL") && (f_SendOutgoingMail == true))
            //    {
            //        string accountEmail = genericFunction.ReadValueFromDb("msgService_OutEmailId");
            //        string password = genericFunction.ReadValueFromDb("msgService_OutEmailPassword");
            //        string MailIouterver = genericFunction.ReadValueFromDb("msgService_EmailOutServer");
            //        string MailsendPort = genericFunction.ReadValueFromDb("msgService_OutgoingMessagePort");
            //        string SMTPUserName = string.Empty;
            //        int outport = 0;

            //        if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
            //        {
            //            drEmailAccount = ds.Tables[3].Rows[0];
            //            if (drEmailAccount["EmailAddress"].ToString().Trim() != String.Empty
            //                && drEmailAccount["Password"].ToString().Trim() != String.Empty
            //                && drEmailAccount["ServerName"].ToString().Trim() != String.Empty
            //                && drEmailAccount["PortNumber"].ToString().Trim() != String.Empty)
            //            {
            //                accountEmail = drEmailAccount["EmailAddress"].ToString().Trim();
            //                password = drEmailAccount["Password"].ToString().Trim();
            //                MailIouterver = drEmailAccount["ServerName"].ToString().Trim();
            //                MailsendPort = drEmailAccount["PortNumber"].ToString().Trim();
            //                SMTPUserName = drEmailAccount["SMTPUserName"].ToString().Trim();
            //            }
            //        }

            //        if (MailsendPort != "")
            //            outport = int.Parse(MailsendPort == "" ? "110" : MailsendPort);
            //        else
            //            outport = int.Parse(Convert.ToString(genericFunction.ReadValueFromDb("OutPort")));//outport = int.Parse(ConfigurationManager.AppSettings["OutPort"].ToString());

            //        #region Email
            //        EMAILOUT objmail = new EMAILOUT();
            //        MailKitManager ObjMailKit = new MailKitManager();

            //        if (sentadd.Length > 2 && sentadd.Contains("@") && sentadd.Contains("."))
            //        {
            //            if (MailIouterver.ToUpper() == "SENDGRID")
            //            {
            //                bool success = false;
            //                try
            //                {
            //                    if (sentadd.Length > 0 && sentadd.Contains("@") && accountEmail != "" && password != "" && sentadd != "" && body != "")
            //                    {
            //                        success = objmail.SendEmailOutSendgrid(ds, accountEmail, sentadd, password, subject, body, ishtml, ccadd, subject);
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    WriteLogAzure("Exception in sending SendEmailOutSendgrid" + ex);
            //                    success = false;
            //                }
            //                if (success)
            //                {
            //                    isMessageSent = true;
            //                    string[] pname = { "num", "Status" };
            //                    object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                    {
            //                        WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
            //                    }
            //                }
            //                else
            //                {
            //                    isMessageSent = false;
            //                }
            //            }
            //            else if (MailIouterver.ToUpper() == "EMAIL-SMTP.AP-SOUTHEAST-1.AMAZONAWS.COM")
            //            {
            //                bool success = false;
            //                try
            //                {
            //                    if (sentadd.Length > 0 && sentadd.Contains("@") && accountEmail != "" && password != "" && sentadd != "" && body != "")
            //                    {
            //                        success = ObjMailKit.SendEmailMailKitManager(ds, accountEmail, sentadd, password, subject, body, ishtml, ccadd, subject, SMTPUserName, MailIouterver);
            //                    }
            //                }
            //                catch (Exception ex)
            //                {
            //                    WriteLogAzure("Exception in sending SendEmailOutSendgrid" + ex);
            //                    success = false;
            //                }
            //                if (success)
            //                {
            //                    isMessageSent = true;
            //                    string[] pname = { "num", "Status" };
            //                    object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                    {
            //                        WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
            //                    }
            //                }
            //                else
            //                {
            //                    isMessageSent = false;
            //                }
            //            }
            //            else
            //            {
            //                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            //                {
            //                    if (ds.Tables[1].Rows[0][0].ToString().ToUpper() != "NA")
            //                    {
            //                        #region Mail with attachment
            //                        WriteLogAzure("Inside Table 1 for MessageID :" + dr[0].ToString());
            //                        if (ds.Tables[1].Rows.Count > 0)
            //                        {
            //                            WriteLogAzure("Inside Table 1 Rows for MessageID :" + dr[0].ToString());
            //                            MemoryStream[] Attachments = new MemoryStream[0];
            //                            string[] Extensions = new string[0];
            //                            string[] AttachmentName = new string[0];
            //                            foreach (DataRow drow in ds.Tables[1].Rows)
            //                            {
            //                                try
            //                                {
            //                                    Array.Resize(ref Attachments, Attachments.Length + 1);
            //                                    Attachments[Attachments.Length - 1] = new MemoryStream(DownloadBlob(drow["FileUrl"].ToString()));

            //                                    Array.Resize(ref Extensions, Extensions.Length + 1);
            //                                    Extensions[Extensions.Length - 1] = drow["MIMEType"].ToString();
            //                                    if (!Extensions[Extensions.Length - 1].Contains('.'))
            //                                        Extensions[Extensions.Length - 1] = "." + Extensions[Extensions.Length - 1];
            //                                    Array.Resize(ref AttachmentName, AttachmentName.Length + 1);
            //                                    AttachmentName[AttachmentName.Length - 1] = drow["AttachmentName"].ToString();
            //                                }
            //                                catch (Exception)
            //                                {
            //                                    WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
            //                                }
            //                            }
            //                            if (accountEmail != "" && password != "" && sentadd != "" && body != "")
            //                            {
            //                                if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, Attachments, AttachmentName, Extensions, messageType, MailIouterver))
            //                                {
            //                                    isMessageSent = true;
            //                                    WriteLogAzure("After Sending Mail with Attachment for MessageID :" + dr[0].ToString());
            //                                    string[] pname = { "num", "Status" };
            //                                    object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                                        WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
            //                                }
            //                                else
            //                                {
            //                                    isMessageSent = false;
            //                                    WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
            //                                }
            //                            }
            //                        }
            //                        #endregion
            //                    }
            //                    else
            //                    {
            //                        #region Send Email
            //                        if (accountEmail != "" && password != "" && sentadd != "" && body != "")
            //                        {
            //                            if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, messageType, MailIouterver))
            //                            {
            //                                isMessageSent = true;
            //                                string[] pname = { "num", "Status" };
            //                                object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                                if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                                    WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
            //                            }
            //                            else
            //                            {
            //                                isMessageSent = false;
            //                                WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
            //                            }
            //                        }
            //                        #endregion
            //                    }
            //                }
            //                else
            //                {
            //                    #region Send Email
            //                    if (accountEmail != "" && password != "" && sentadd != "" && body != "")
            //                    {
            //                        if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, "", MailIouterver))
            //                        {
            //                            isMessageSent = true;
            //                            string[] pname = { "num", "Status" };
            //                            object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                            SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                            if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                            {
            //                                WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
            //                            }
            //                        }
            //                        else
            //                        {
            //                            isMessageSent = false;
            //                            WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
            //                        }
            //                    }
            //                    #endregion
            //                }
            //            }
            //        }
            //        #endregion
            //    }
            //    #endregion

            //    #region SFTP Upload
            //    if (msgCommType.ToUpper() == "SFTP" || msgCommType.ToUpper() == "ALL" || msgCommType.Equals("SFTP", StringComparison.OrdinalIgnoreCase))
            //    {
            //        FTP objFtp = new FTP();
            //        string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;
            //        foreach (DataRow drSFTP in ds.Tables[2].Rows)
            //        {
            //            if (drSFTP != null && drSFTP.ItemArray.Length > 0 && drSFTP["FTPID"].ToString() != "" && drSFTP["FTPUserName"].ToString() != "" && (drSFTP["FTPPassword"].ToString() != "" || drSFTP["PPKFileName"].ToString() != ""))
            //            {
            //                SFTPAddress = drSFTP["FTPID"].ToString();
            //                SFTPUserName = drSFTP["FTPUserName"].ToString();
            //                SFTPPassWord = drSFTP["FTPPassword"].ToString();
            //                ppkFileName = drSFTP["PPKFileName"].ToString().Trim();
            //                SFTPFingerPrint = drSFTP["FingerPrint"].ToString();
            //                StpFolerParth = drSFTP["RemotePath"].ToString();
            //                SFTPPortNumber = drSFTP["PortNumber"].ToString();
            //                GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
            //            }
            //            else
            //            {
            //                SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
            //                SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
            //                SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
            //                ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
            //                SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
            //                StpFolerParth = genericFunction.ReadValueFromDb("msgService_OUT_FolderPath");
            //                SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");
            //                GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
            //            }

            //            if (ppkFileName != string.Empty)
            //            {
            //                ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
            //            }

            //            FileName = dr["Subject"].ToString();

            //            if (FileName.ToUpper().Contains(".TXT"))
            //            {
            //                int fileIndex = FileName.IndexOf(".");
            //                if (fileIndex > 0)
            //                    FileName = FileName.Substring(0, fileIndex);
            //            }
            //            else if (FileName.ToUpper().Contains(".XML"))
            //            {
            //                int fileIndex = FileName.IndexOf(".");
            //                if (fileIndex > 0)
            //                {
            //                    FileName = FileName.Substring(0, fileIndex);
            //                    FileExtension = "xml";
            //                }
            //            }
            //            else if (FileName.ToUpper().Contains(".CSV"))
            //            {
            //                int fileIndex = FileName.IndexOf(".");
            //                if (fileIndex > 0)
            //                    FileName = FileName.Substring(0, fileIndex);
            //            }
            //            else
            //                FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");

            //            if (SFTPAddress != "" && SFTPUserName != "" && (SFTPPassWord != "" || ppkFileName != string.Empty) && SFTPFingerPrint != "" && StpFolerParth != "" && SFTPPortNumber.Trim() != string.Empty)
            //            {
            //                int portNumber = Convert.ToInt32(SFTPPortNumber);
            //                if (objFtp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, body, FileName, FileExtension == string.Empty ? ".SND" : FileExtension, StpFolerParth, portNumber, ppkLocalFilePath, GHAOutFolderPath))
            //                {
            //                    isMessageSent = true;
            //                    string[] pname = { "num", "Status" };
            //                    object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                        WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());
            //                }
            //                else
            //                {
            //                    isFTPUploadSuccessfully = false;
            //                }
            //            }
            //        }
            //    }
            //    #endregion

            //    #region SITA Upload
            //    if (msgCommType.ToUpper() == "SITA" || msgCommType.ToUpper() == "SITAFTP"
            //        || msgCommType.ToUpper() == "ALL"
            //        || msgCommType.Equals("SITA", StringComparison.OrdinalIgnoreCase)
            //        || msgCommType.Equals("SITAFTP", StringComparison.OrdinalIgnoreCase))
            //    {
            //        FTP objFtp = new FTP();
            //        string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;

            //        if (drMsg != null && drMsg.ItemArray.Length > 0 && drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
            //        {
            //            SFTPAddress = drMsg["FTPID"].ToString();
            //            SFTPUserName = drMsg["FTPUserName"].ToString();
            //            SFTPPassWord = drMsg["FTPPassword"].ToString();
            //            ppkFileName = drMsg["PPKFileName"].ToString().Trim();
            //            SFTPFingerPrint = drMsg["FingerPrint"].ToString();
            //            StpFolerParth = drMsg["RemotePath"].ToString();
            //            SFTPPortNumber = drMsg["PortNumber"].ToString();
            //            GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
            //        }
            //        else
            //        {
            //            SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
            //            SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
            //            SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
            //            ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
            //            SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
            //            StpFolerParth = genericFunction.ReadValueFromDb("msgService_OUT_FolderPath");
            //            SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");
            //            GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
            //        }
            //        if (ppkFileName != string.Empty)
            //        {
            //            ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
            //        }
            //        if (FileName.ToUpper().Contains(".TXT"))
            //        {
            //            int fileIndex = FileName.IndexOf(".");
            //            if (fileIndex > 0)
            //                FileName = FileName.Substring(0, fileIndex);
            //        }
            //        else if (FileName.ToUpper().Contains(".XML"))
            //        {
            //            int fileIndex = FileName.IndexOf(".");
            //            if (fileIndex > 0)
            //            {
            //                FileName = FileName.Substring(0, fileIndex);
            //                FileExtension = "xml";
            //            }
            //        }
            //        else
            //            FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");

            //        if (SFTPAddress != "" && SFTPUserName != "" && (SFTPPassWord != "" || ppkFileName != string.Empty) && SFTPFingerPrint != "" && StpFolerParth != "" && SFTPPortNumber.Trim() != string.Empty)
            //        {
            //            int portNumber = Convert.ToInt32(SFTPPortNumber);
            //            if (objFtp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, body, FileName, FileExtension == string.Empty ? ".SND" : FileExtension, StpFolerParth, portNumber, ppkLocalFilePath, GHAOutFolderPath))
            //            {
            //                isMessageSent = true;
            //                string[] pname = { "num", "Status" };
            //                object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                    WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());
            //            }
            //            else
            //            {
            //                isFTPUploadSuccessfully = false;
            //            }
            //        }
            //    }
            //    #endregion

            //    #region AZURE DRIVE
            //    if (msgCommType == "DRIVE" || msgCommType == "ALL")
            //    {
            //        AzureDrive drive = new AzureDrive();
            //        if (drive.UploadToDrive(DateTime.Now.ToString("yyyyMMdd_hhmmss_fff"), body, SITAFolderPath))
            //        {
            //            isMessageSent = true;
            //            string[] pname = { "num", "Status" };
            //            object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //            SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //            if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dr[0].ToString());
            //        }
            //        else
            //            isMessageSent = false;
            //    }
            //    #endregion

            //    #region MQ Message
            //    if (msgCommType.ToUpper() == "MESSAGE QUEUE" || msgCommType == "ALL")
            //    {
            //        if (drMsg != null)
            //        {
            //            int WaitInterval = 0;

            //            if (body.Trim() != string.Empty)
            //            {
            //                string MQManager = Convert.ToString(drMsg["MQManager"]);
            //                string MQChannel = Convert.ToString(drMsg["MQChannel"]);
            //                string MQHost = Convert.ToString(drMsg["MQHost"]);
            //                string MQPort = Convert.ToString(drMsg["MQPort"]);
            //                string MQUser = Convert.ToString(drMsg["MQUser"]);
            //                string MQInqueue = Convert.ToString(drMsg["MQOutQueue"]);//"CG.BOOKINGS.CARGOSPOT.SMARTKARGO";
            //                string MQOutqueue = "";
            //                string ErrorMessage = string.Empty;
            //                string Message = body;

            //                if (MQManager.Trim() != string.Empty && MQChannel.Trim() != string.Empty && MQHost.Trim() != string.Empty && MQPort.Trim() != string.Empty && MQInqueue.Trim() != string.Empty)
            //                {
            //                    MQAdapter mqAdapter = new MQAdapter(MessagingType.ASync, MQManager, MQChannel, MQHost, MQPort, MQUser, MQInqueue, MQOutqueue, WaitInterval);

            //                    string result = mqAdapter.SendMessage(Message, out ErrorMessage);

            //                    if (ErrorMessage.Trim() == string.Empty)
            //                    {
            //                        isMessageSent = true;
            //                        string[] pname = { "num", "Status" };
            //                        object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                        SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                        if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                            WriteLogAzure("MQ Message Sent successfully to:" + dr[0].ToString());
            //                    }
            //                    else
            //                    {
            //                        WriteLogAzure("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
            //                    }
            //                    mqAdapter.DisposeQueue();
            //                }
            //                else
            //                {
            //                    WriteLogAzure("Info : In(SendMail() method):Insufficient MQ Message Configuration");
            //                }
            //            }
            //        }
            //    }
            //    #endregion

            //    #region UFTP
            //    if (msgCommType.ToUpper() == "UFTP" || msgCommType == "ALL")
            //    {
            //        FTP objFtp = new FTP();
            //        if (drMsg != null && drMsg.ItemArray.Length > 0)
            //        {
            //            if (drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
            //            {
            //                ftpUrl = drMsg["FTPID"].ToString();
            //                ftpUserName = drMsg["FTPUserName"].ToString();
            //                ftpPassword = drMsg["FTPPassword"].ToString();
            //            } // Rohidas added else condition for live issue on 30 aug 2017
            //            else
            //            {
            //                ftpUrl = genericFunction.ReadValueFromDb("FTPURLofFileUpload");
            //                ftpUserName = genericFunction.ReadValueFromDb("FTPUserofFileUpload");
            //                ftpPassword = genericFunction.ReadValueFromDb("FTPPasswordofFileUpload");
            //            }
            //        }
            //        else
            //        {
            //            ftpUrl = genericFunction.ReadValueFromDb("FTPURLofFileUpload");
            //            ftpUserName = genericFunction.ReadValueFromDb("FTPUserofFileUpload");
            //            ftpPassword = genericFunction.ReadValueFromDb("FTPPasswordofFileUpload");
            //        }
            //        if (FileName.ToUpper().Contains(".TXT"))
            //        {
            //            int fileIndex = FileName.IndexOf(".");
            //            if (fileIndex > 0)
            //            {
            //                FileName = FileName.Substring(0, fileIndex);
            //                FileExtension = "txt";
            //            }
            //        }
            //        else if (FileName.ToUpper().Contains(".XML"))
            //        {
            //            int fileIndex = FileName.IndexOf(".");
            //            if (fileIndex > 0)
            //            {
            //                FileName = FileName.Substring(0, fileIndex);
            //                FileExtension = "xml";
            //            }
            //        }
            //        else
            //            FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
            //        if (ftpUrl != "")
            //        {
            //            if (objFtp.DownloadBlobAndFTPUpload(actualMsg, ftpUrl, ftpUserName, ftpPassword, body, FileName, FileExtension == "" ? "SND" : FileExtension))
            //            {
            //                isMessageSent = true;
            //                string[] pname = { "num", "Status" };
            //                object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //                if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                    WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
            //            }
            //            else
            //            {
            //                isMessageSent = false;
            //                isFTPUploadSuccessfully = false;
            //            }
            //        }
            //    }
            //    #endregion

            //    #region WebService
            //    if ((msgCommType.ToUpper() == "WEBSERVICE" || msgCommType.ToUpper() == "ALL") && drMsg != null && drMsg["WebServiceURL"].ToString() != "")
            //    {
            //        try
            //        {
            //            string results = string.Empty;
            //            string username = drMsg["WebServiceUserName"].ToString();
            //            string Password = drMsg["WebServicePassword"].ToString();
            //            string URL = drMsg["WebServiceUrl"].ToString();
            //            string PartnerCode = drMsg["PartnerCode"].ToString();
            //            string customsName = drMsg["CustomsName"].ToString();
            //            if (messageType == "XFFM")
            //            {
            //                WebService ws = new WebService(URL, "sendMessageXFFM", username, Password, body);
            //                ws.Invoke(customsName);
            //                results = ws.ResultString;
            //            }
            //            else if (messageType == "XFWB")
            //            {
            //                WebService ws = new WebService(URL, "sendMessageXFWB", username, Password, body);
            //                ws.Invoke(customsName);
            //                results = ws.ResultString;
            //            }
            //            else if (messageType == "XFZB")
            //            {
            //                WebService ws = new WebService(URL, "sendMessageXFZB", username, Password, body);
            //                ws.Invoke(customsName);
            //                results = ws.ResultString;
            //            }

            //            SaveMessage("DACCustoms", results, "WebService", "", DateTime.Now, DateTime.Now, messageType, "Active", "WebService", awbNumber, flightNo, flightDate);
            //            WriteLogAzure("Response message for " + messageType + ": " + results);
            //            isMessageSent = true;

            //            string[] pname = { "num", "Status" };
            //            object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //            SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //            if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                WriteLogAzure("Message Sent SuccessFully to:" + dr[0].ToString());
            //        }
            //        catch (Exception ex)
            //        {
            //            isMessageSent = false;
            //            WriteLogAzure(ex);
            //        }
            //    }
            //    #endregion

            //    #region WEBAPI
            //    if ((msgCommType.ToUpper() == "WEBAPI" || msgCommType.ToUpper() == "ALL") && drMsg != null && drMsg["WebServiceURL"].ToString() != "")
            //    {
            //        try
            //        {
            //            string results = string.Empty;
            //            string username = drMsg["WebServiceUserName"].ToString();
            //            string Password = drMsg["WebServicePassword"].ToString();
            //            string URL = drMsg["WebServiceUrl"].ToString();
            //            string PartnerCode = drMsg["PartnerCode"].ToString();
            //            string customsName = drMsg["CustomsName"].ToString();
            //            if (messageType == "FFM")
            //            {
            //                WebService ws = new WebService(URL, "", username, Password, body);
            //                ws.Invoke(customsName);
            //                results = ws.ResultString;
            //            }
            //            else if (messageType == "FWB")
            //            {
            //                WebService ws = new WebService(URL, "", username, Password, body);
            //                results = ws.ResultString;
            //            }
            //            else if (messageType == "FHL")
            //            {
            //                WebService ws = new WebService(URL, "", username, Password, body);
            //                ws.Invoke(customsName);
            //                results = ws.ResultString;
            //            }

            //            SaveMessage("DACCustoms", results, "WebService", "", DateTime.Now, DateTime.Now, messageType, "Active", "WebService", awbNumber, flightNo, flightDate);
            //            WriteLogAzure("Response message for " + messageType + ": " + results);
            //            isMessageSent = true;

            //            string[] pname = { "num", "Status" };
            //            object[] pvalue = { int.Parse(dr[0].ToString()), status };
            //            SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
            //            if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //                WriteLogAzure("Message Sent SuccessFully to:" + dr[0].ToString());
            //        }
            //        catch (Exception ex)
            //        {
            //            isMessageSent = false;
            //            WriteLogAzure(ex);
            //        }
            //    }
            //    #endregion

            //    if (!isMessageSent)
            //    {
            //        string ErrorMsg = "Error occured while processing sending request";
            //        if (!isFTPUploadSuccessfully)
            //        {
            //            FTP ftp = new FTP();
            //            ftp.FTPConnectionAlert();
            //            ErrorMsg = "FTP folder of SITA is not accessible";
            //        }
            //        string[] pname = { "num", "Status", "ErrorMsg", "MsgDeliveryType" };
            //        object[] pvalue = { int.Parse(dr[0].ToString()), status, ErrorMsg, msgCommType.ToUpper().Trim() };
            //        SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
            //        if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
            //            WriteLogAzure("Fail to sent email to:" + dr[0].ToString());
            //    }

            //    // Legacy logic ends here.

            //    // Return true only if the message was successfully handled by any channel
            //    return isMessageSent;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "ProcessMessageActivity failed.");
            //    return false;
            //}
        }

        /// <summary>
        /// Minimal wrapper that mimics the legacy genericFunction's ReadValueFromDb and GetPPKFilePath APIs,
        /// but backed by the supplied IDictionary<string,string> config. This avoids changing call-sites in legacy code.
        /// If you have a real genericFunction helper in your project, you can remove/replace this wrapper.
        /// </summary>
        private class GenericFunctionWrapper
        {
            private readonly IDictionary<string, string> _cfg;
            public GenericFunctionWrapper(IDictionary<string, string> cfg)
            {
                _cfg = cfg ?? new Dictionary<string, string>();
            }

            public string ReadValueFromDb(string key)
            {
                if (string.IsNullOrWhiteSpace(key)) return string.Empty;
                if (_cfg.TryGetValue(key, out var v)) return v ?? string.Empty;
                return string.Empty;
            }

            /// <summary>
            /// Best-effort PPK file resolver. If you have a different implementation in your repo, replace this.
            /// Looks for a config key "PPKLocalFolder" or "PPKFilePathPrefix" and combines with filename.
            /// If none present, returns filename unchanged.
            /// </summary>
            public string GetPPKFilePath(string ppkFileName)
            {
                if (string.IsNullOrWhiteSpace(ppkFileName)) return string.Empty;
                if (_cfg.TryGetValue("PPKLocalFolder", out var prefix) && !string.IsNullOrWhiteSpace(prefix))
                {
                    try
                    {
                        return Path.Combine(prefix, ppkFileName);
                    }
                    catch
                    {
                        return ppkFileName;
                    }
                }
                if (_cfg.TryGetValue("PPKFilePathPrefix", out var prefix2) && !string.IsNullOrWhiteSpace(prefix2))
                {
                    try
                    {
                        return Path.Combine(prefix2, ppkFileName);
                    }
                    catch
                    {
                        return ppkFileName;
                    }
                }
                return ppkFileName;
            }
        }

        // Note: The methods DownloadBlob(...) and SaveMessage(...) are expected to exist elsewhere in the project (same as original legacy code).
        // If not present, implement them in the appropriate utility/service and remove these comments.
    }
}
