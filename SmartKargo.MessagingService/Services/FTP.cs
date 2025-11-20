using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.DAL;
using QidWorkerRole.SIS.FileHandling;
using QidWorkerRole.UploadMasters;
using QueueManager;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using WinSCP;
using DbEntity = QidWorkerRole.SIS.DAL;
using ModelClass = QidWorkerRole.SIS.Model;
using Zipfile = Ionic.Zip.ZipFile;


namespace QidWorkerRole
{
    public class FTP
    {
        //SCMExceptionHandlingWorkRole scmeception = new SCMExceptionHandlingWorkRole();
        //SIS.SISBAL objSISBAL = new SIS.SISBAL();

        private readonly ISqlDataHelperDao _readWriteDao;
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<FTP> _staticLogger => _loggerFactory?.CreateLogger<FTP>();
        // static shared logger
        private readonly ILogger<FTP> _logger;       // instance logger
        private readonly EMAILOUT _emailOut;
        private readonly GenericFunction _genericFunction;
        private readonly cls_SCMBL _cls_SCMBL;
        private readonly AppConfig _appConfig;
        private readonly SIS.SISBAL _sISBAL;
        private readonly UploadMasterCommon _uploadMasterCommon;
        private readonly CreateDBData _createDBData;
        private readonly ReadDBData _readDBData;
        private readonly SISFileReader _sisFileReader;
        private readonly UpdateDBData _updateDBData;

        #region Constructor
        public FTP(
            ILogger<FTP> logger,
            EMAILOUT emailOut,
            GenericFunction genericFunction,
            cls_SCMBL cls_SCMBL,
            AppConfig appConfig,
            ISqlDataHelperFactory sqlDataHelperFactory,
            SIS.SISBAL sISBAL,
            ILoggerFactory loggerFactory,
            UploadMasterCommon uploadMasterCommon,
            CreateDBData createDBData,
            ReadDBData readDBData,
            SISFileReader sisFileReader,
            UpdateDBData updateDBData
            )
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _emailOut = emailOut;
            _genericFunction = genericFunction;
            _cls_SCMBL = cls_SCMBL;
            _appConfig = appConfig;
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _sISBAL = sISBAL;
            _uploadMasterCommon = uploadMasterCommon;
            _createDBData = createDBData;
            _readDBData = readDBData;
            _sisFileReader = sisFileReader;
            _updateDBData = updateDBData;
        }
        #endregion Constructor

        #region Save Saveon72FTP
        /*Not in use*/
        /// <summary>
        /// Saves FTP message 
        /// </summary>
        /// <param name="Message">Message to be stored on FTP.</param>
        /// <param name="FileName">Name to be given to file on FTP.</param>
        /// <returns>True if File saved successfully.</returns>
        public bool Saveon72FTP(string Message, string FileName)
        {
            try
            {   //Find FTP address to save file. Write code to fetch FTP address from database.
                //Save file.
                FtpWebRequest myFtpWebRequest;
                FtpWebResponse myFtpWebResponse;
                StreamWriter myStreamWriter;

                string UserName = "AAuser";
                string Secret = "AAuser";
                string FTPPath = " ftp://7.167.41.153:8897/";
                myFtpWebRequest = (FtpWebRequest)WebRequest.Create(FTPPath + "/" + FileName + ".msg");
                //myFtpWebRequest = (FtpWebRequest)WebRequest.Create(FTPPath + "/TestFTP.msg");

                if (UserName != "")
                {
                    myFtpWebRequest.Credentials = new NetworkCredential(UserName, Secret);
                }

                myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                myFtpWebRequest.UseBinary = true;

                myStreamWriter = new StreamWriter(myFtpWebRequest.GetRequestStream());
                myStreamWriter.Write(Message);
                myStreamWriter.Close();

                myFtpWebResponse = (FtpWebResponse)myFtpWebRequest.GetResponse();


                myFtpWebResponse.Close();

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error on FTP upload:", ex);
                _logger.LogError(ex, "Error on FTP upload: {0}", ex.Message);
                FTPConnectionAlert();
                return (false);

            }
            return (true);
        }
        #endregion Save FTP

        #region Save FTP
        /// <summary>
        /// Saves FTP message 
        /// </summary>
        /// <param name="Message">Message to be stored on FTP.</param>
        /// <param name="FileName">Name to be given to file on FTP.</param>
        /// <returns>True if File saved successfully.</returns>
        public bool SaveFTP(string FTPPath, string UserName, string Password, string Message, string FileName)
        {
            try
            {   //Find FTP address to save file. Write code to fetch FTP address from database.

                //Save file.
                FtpWebRequest myFtpWebRequest;
                FtpWebResponse myFtpWebResponse;
                StreamWriter myStreamWriter;

                myFtpWebRequest = (FtpWebRequest)WebRequest.Create(FTPPath + "/" + FileName + ".snd");
                //myFtpWebRequest = (FtpWebRequest)WebRequest.Create(FTPPath + "/TestFTP.msg");

                if (UserName != "")
                {
                    myFtpWebRequest.Credentials = new NetworkCredential(UserName, Password);
                }

                myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                myFtpWebRequest.UseBinary = true;

                myStreamWriter = new StreamWriter(myFtpWebRequest.GetRequestStream());
                myStreamWriter.Write(Message);
                myStreamWriter.Close();

                myFtpWebResponse = (FtpWebResponse)myFtpWebRequest.GetResponse();


                myFtpWebResponse.Close();

            }
            catch (Exception ex)
            {
                FTPConnectionAlert();
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return (false);
            }
            return (true);
        }
        public bool SaveFTP(string FTPPath, string UserName, string Password, string Message, string FileName, string fileextension)
        {
            try
            {   //Find FTP address to save file. Write code to fetch FTP address from database.

                //Save file.
                FtpWebRequest myFtpWebRequest;
                FtpWebResponse myFtpWebResponse;
                StreamWriter myStreamWriter;

                myFtpWebRequest = (FtpWebRequest)WebRequest.Create(FTPPath + "/" + FileName + "." + fileextension);
                //myFtpWebRequest = (FtpWebRequest)WebRequest.Create(FTPPath + "/TestFTP.msg");

                if (UserName != "")
                {
                    myFtpWebRequest.Credentials = new NetworkCredential(UserName, Password);
                }

                myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                myFtpWebRequest.UseBinary = true;

                myStreamWriter = new StreamWriter(myFtpWebRequest.GetRequestStream());
                myStreamWriter.Write(Message);
                myStreamWriter.Close();

                myFtpWebResponse = (FtpWebResponse)myFtpWebRequest.GetResponse();


                myFtpWebResponse.Close();

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("save ON FTP:" + ex.ToString() + FileName);
                _logger.LogError("save ON FTP: {0}", ex.ToString() + FileName);
                FTPConnectionAlert();
                return (false);
            }
            return (true);
        }




        #endregion Save FTP


        #region FTP Folders List
        public static void FTPFoldersList(string ftpurl,
            string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)
                WebRequest.Create(ftpurl);

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new System.Net.NetworkCredential(username,
                password);

            try
            {
                FtpWebResponse response = (FtpWebResponse)
                    request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                Console.WriteLine(reader.ReadToEnd());

                Console.WriteLine("Directory list complete " +
                    "with status: {0}", response.StatusDescription);

                reader.Close();
                response.Close();
            }
            catch (WebException ex)
            {

                // Console.WriteLine(e.ToString());
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        #endregion FTP Short List

        #region FTP Files List
        /// <summary>
        /// Returns list of files from given FTP folder.
        /// </summary>
        /// <param name="ftpurl">FTP URL with folder path </param>
        /// <param name="username">Username for FTP.</param>
        /// <param name="password">Password for FTP.</param>
        /// <returns>String array containing names of files on FTP folder.</returns>
        public string[] FTPFilesList(string ftpurl, string username, string password)
        {
            string filesList = "";
            string[] arrayfilesList = null;
            try
            {
                FtpWebRequest request = (FtpWebRequest)
                    WebRequest.Create(ftpurl);

                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username,
                    password);
                FtpWebResponse response = (FtpWebResponse)
                    request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                filesList = reader.ReadToEnd();

                //Separate file names into an array.
                if (filesList != null && filesList.Contains("\r\n"))
                {
                    arrayfilesList = filesList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                }
                reader.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                arrayfilesList = null;
                FTPConnectionAlert();
            }
            return (arrayfilesList);
        }

        /// <summary>
        /// Send alert eamil if service fails to connect FTP
        /// </summary>
        public void FTPConnectionAlert()
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();

                //string fromEmailId = genericFunction.ReadValueFromDb("msgService_OutEmailId");
                //string toEmailId = genericFunction.ReadValueFromDb("msgService_FTPAlertEmailID");
                //string password = genericFunction.ReadValueFromDb("msgService_OutEmailPassword");
                string fromEmailId = ConfigCache.Get("msgService_OutEmailId");
                string toEmailId = ConfigCache.Get("msgService_FTPAlertEmailID");
                string password = ConfigCache.Get("msgService_OutEmailPassword");

                string subject = "FTP Connection Status";
                string body = "FTP folder of SITA is not accessible.";
                bool isBodyHTML = false;
                //int outmailport = Convert.ToInt32(genericFunction.ReadValueFromDb("msgService_OutgoingMessagePort"));
                int outmailport = Convert.ToInt32(ConfigCache.Get("msgService_OutgoingMessagePort"));

                string CCEmailID = string.Empty;

                if (fromEmailId != "" && password != "" && toEmailId != "")
                {
                    //EMAILOUT emailOut = new EMAILOUT();
                    _emailOut.sendMail(fromEmailId, toEmailId, password, subject, body, isBodyHTML, outmailport, CCEmailID, "");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        #endregion FTP Files List

        #region DeleteFTPFile
        /// <summary>
        /// Returns list of files from given FTP folder.
        /// </summary>
        /// <param name="ftpfilepath">FTP URL with file path.</param>
        /// <param name="username">Username for FTP.</param>
        /// <param name="password">Password for FTP.</param>        
        /// <returns>True if file successfully deleted.</returns>
        public bool DeleteFTPFile(string ftpfilepath,
            string username, string password)
        {
            bool success = false;
            try
            {
                FtpWebRequest request = (FtpWebRequest)
                    WebRequest.Create(ftpfilepath);

                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(username,
                    password);
                FtpWebResponse response = (FtpWebResponse)
                    request.GetResponse();


                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                success = true;
                string filesList = reader.ReadToEnd();

                reader.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                success = false;
                FTPConnectionAlert();
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return (success);
        }
        #endregion DeleteFTPFile

        #region Read FTP File
        /// <summary>
        /// Reads contents of FTP file.
        /// </summary>
        /// <param name="FileName">Name of the file from which data is to be read.</param>
        /// <returns>Contents of file as string.</returns>
        public string ReadFTPFile(string FTPFileName, string UserName, string Password)
        {
            string strFilesContents = null;
            try
            {
                //CREATE AN FTP REQUEST WITH THE DOMAIN AND CREDENTIALS
                System.Net.FtpWebRequest tmpReq =
                    (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(FTPFileName);
                tmpReq.Credentials = new System.Net.NetworkCredential(UserName, Password);

                //GET THE FTP RESPONSE
                using (System.Net.WebResponse tmpRes = tmpReq.GetResponse())
                {
                    //GET THE STREAM TO READ THE RESPONSE FROM
                    using (System.IO.Stream tmpStream = tmpRes.GetResponseStream())
                    {
                        //CREATE A TXT READER (COULD BE BINARY OR ANY OTHER TYPE YOU NEED)
                        using (System.IO.TextReader tmpReader = new System.IO.StreamReader(tmpStream))
                        {
                            //STORE THE FILE CONTENTS INTO A STRING
                            strFilesContents = tmpReader.ReadToEnd();
                            tmpReader.Close();
                        }
                        tmpStream.Close();
                    }
                    tmpRes.Close();
                }
            }
            catch (Exception ex)
            {
                strFilesContents = "Error: " + ex.Message;
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                FTPConnectionAlert();
            }
            return strFilesContents;
        }
        #endregion Read FTP File

        #region SFTP Upload

        /**Commented this as this method no reference*/
        //public bool SaveSFTPUpload(string SFTPPath, string UserName, string Password, string FingerPrint, string Message, string FileName)
        //{
        //    try
        //    {
        //        // Setup session options
        //        SessionOptions sessionOptions = new SessionOptions
        //        {
        //            Protocol = Protocol.Sftp,
        //            HostName = SFTPPath,
        //            UserName = UserName,
        //            Password = Password,
        //            SshHostKeyFingerprint = FingerPrint

        //        };

        //        using (Session session = new Session())
        //        {
        //            // Connect
        //            session.DisableVersionCheck = true;
        //            session.ExecutablePath = null;// @"E:\D Drive\swapnil\QID\Dot Net\SCM\VS 2015 - Message Service Rework\AzureCloudServiceWorkerRole_25JUN2015\QidWorkerRole\bin\Release";

        //            session.Open(sessionOptions);

        //            // Upload files
        //            TransferOptions transferOptions = new TransferOptions();
        //            transferOptions.TransferMode = TransferMode.Binary;
        //            transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

        //            TransferOperationResult transferResult;

        //            string fileName;

        //            fileName = FileName;
        //            fileName = Path.ChangeExtension(fileName, ".txt");
        //            fileName = Path.Combine(Path.GetTempPath(), fileName);

        //            File.WriteAllText(fileName, Message);
        //            transferResult = session.PutFiles(fileName, "/Upload/", false, transferOptions);
        //            File.Delete(fileName);
        //            // Throw on any error
        //            transferResult.Check();

        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("SFTP File: " + FileName + " Error on UPload . Error: " + ex.Message);
        //        return false;
        //    }

        //}

        public bool SaveSFTPUpload(string SFTPAddress, string UserName, string Password, string FingerPrint, string Message, string FileName, string FileExtension, string RemotePath, int portNumber, string ppkLocalFilePath, string GHAOutFolderPath = "")
        {
            bool status = false;
            string fileName = string.Empty;
            try
            {

                //SessionOptions sessionOptions;
                //if (ppkLocalFilePath != string.Empty)
                //{
                //    sessionOptions = new SessionOptions
                //    {
                //        Protocol = Protocol.Sftp,
                //        HostName = SFTPAddress,
                //        UserName = UserName,
                //        SshPrivateKeyPath = ppkLocalFilePath,
                //        PortNumber = portNumber,
                //        SshHostKeyFingerprint = FingerPrint
                //    };
                //}
                //else
                //{
                //    sessionOptions = new SessionOptions
                //    {
                //        Protocol = Protocol.Sftp,
                //        HostName = SFTPAddress,
                //        UserName = UserName,
                //        Password = Password,
                //        PortNumber = portNumber,
                //        SshHostKeyFingerprint = FingerPrint
                //    };
                //}

                WinSCP.SessionOptions sessionOptions = new WinSCP.SessionOptions
                {
                    Protocol = WinSCP.Protocol.Sftp,
                    HostName = SFTPAddress,
                    UserName = UserName,
                    PortNumber = portNumber,
                    SshHostKeyFingerprint = FingerPrint,
                    SshPrivateKeyPath = !string.IsNullOrEmpty(ppkLocalFilePath) ? ppkLocalFilePath : null,
                    Password = string.IsNullOrEmpty(ppkLocalFilePath) ? Password : null
                };

                using (WinSCP.Session session = new WinSCP.Session())
                {
                    session.DisableVersionCheck = true;
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                    TransferOperationResult transferResult;
                    TransferOperationResult transferResultGHA;
                    fileName = FileName;
                    fileName = Path.ChangeExtension(fileName, FileExtension);
                    //fileName = Path.Combine(Path.GetTempPath(), fileName);
                    if (RemotePath.Length < 1 || RemotePath == "")
                        RemotePath = "/";

                    //GenericFunction genericFunction = new GenericFunction();
                    //if (genericFunction.ReadValueFromDb("MSServiceType") != string.Empty && genericFunction.ReadValueFromDb("MSServiceType").ToUpper() == "WINDOWSSERVICE")

                    var mSServiceType = ConfigCache.Get("MSServiceType");
                    if (mSServiceType != string.Empty && mSServiceType.ToUpper() == "WINDOWSSERVICE")

                    {
                        fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                        File.WriteAllText(fileName, Message);
                    }
                    else
                    {
                        File.WriteAllText(fileName, Message);
                    }
                    transferResult = session.PutFiles(fileName, RemotePath + "/", false, transferOptions);
                    if (GHAOutFolderPath.Trim() != string.Empty)
                    {
                        transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
                    }

                    var uploadManifestToBlob = ConfigCache.Get("UploadManifestToBlob");
                    var m_AMF_REQ_Container = ConfigCache.Get("M_AMF_REQ_Container");
                    var h_AMF_REQ_Container = ConfigCache.Get("H_AMF_REQ_Container");

                    //if (genericFunction.ReadValueFromDb("UploadManifestToBlob") != string.Empty && Convert.ToBoolean(genericFunction.ReadValueFromDb("UploadManifestToBlob")))
                    if (uploadManifestToBlob != string.Empty && Convert.ToBoolean(uploadManifestToBlob))

                    {
                        Stream messageStream = GenerateStreamFromString(Message);
                        if (Message.ToUpper().Contains("MASTERMANIFESTREQUEST"))
                        {
                            _genericFunction.UploadToBlob(messageStream, System.IO.Path.GetFileName(fileName), m_AMF_REQ_Container);
                        }
                        else if (Message.ToUpper().Contains("HOUSEMANIFESTREQUEST"))
                        {
                            _genericFunction.UploadToBlob(messageStream, System.IO.Path.GetFileName(fileName), h_AMF_REQ_Container);
                        }
                    }
                    // clsLog.WriteLogAzure("fileName1:- " + fileName + "RemotePath:- " + RemotePath);
                    _logger.LogInformation("fileName1:- {0} RemotePath:- {1}", fileName, RemotePath);
                    File.Delete(fileName);
                    // Throw on any error
                    transferResult.Check();
                    session.Close();
                }
                status = true;
                // clsLog.WriteLogAzure("Files Successfully Save in  Inbox for : " + FileName);
                _logger.LogInformation("Files Successfully Save in  Inbox for : {0}", FileName);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error on Files  uploading on SFTP for : " + ex.ToString());
                _logger.LogError("Error on Files  uploading on SFTP for : {0}", ex);
                status = false;
            }
            return status;
        }
        #endregion

        #region SFTP Download
        public async Task<bool> SFTPDownload(string SFTPPath, string RemotePath, string LocalPath, string UserName, string Password, string FingerPrint, int portNumber, string ppkLocalFilePath, string messageType, string archivalPath)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                //SessionOptions sessionOptions;

                WinSCP.SessionOptions sessionOptions = new WinSCP.SessionOptions
                {
                    Protocol = WinSCP.Protocol.Sftp,
                    HostName = SFTPPath,
                    UserName = UserName,
                    PortNumber = portNumber,
                    SshHostKeyFingerprint = FingerPrint,
                    SshPrivateKeyPath = !string.IsNullOrEmpty(ppkLocalFilePath) ? ppkLocalFilePath : null,
                    Password = string.IsNullOrEmpty(ppkLocalFilePath) ? Password : null
                };

                //if (ppkLocalFilePath != string.Empty)
                //{
                //    sessionOptions = new SessionOptions
                //    {
                //        Protocol = Protocol.Sftp,
                //        HostName = SFTPPath,
                //        UserName = UserName,
                //        PortNumber = portNumber,
                //        SshHostKeyFingerprint = FingerPrint,
                //        SshPrivateKeyPath = ppkLocalFilePath,
                //    };
                //}
                //else
                //{
                //    sessionOptions = new SessionOptions
                //    {
                //        Protocol = Protocol.Sftp,
                //        HostName = SFTPPath,
                //        UserName = UserName,
                //        PortNumber = portNumber,
                //        SshHostKeyFingerprint = FingerPrint,
                //        Password = Password,
                //    };
                //}
                using (WinSCP.Session session = new WinSCP.Session())
                {
                    session.Open(sessionOptions);
                    session.GetFileInfo(RemotePath);

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    #region : File Mask :
                    string pathToMask = RemotePath + "/", downloadFileName = string.Empty;
                    bool isDownloadExactFile = false;
                    DateTime? lastWriteTime = null;
                    if (messageType == MessageData.MessageTypeName.CARGOLOADXML)
                    {
                        transferOptions.FileMask = pathToMask + "*.xml";
                    }
                    else if (messageType == MessageData.MessageTypeName.EXCHANGERATESFROMTOUPLOAD)
                    {
                        RemoteDirectoryInfo directoryInfo = session.ListDirectory(RemotePath);

                        RemoteFileInfo latest =
                            directoryInfo.Files
                                .Where(file => !file.IsDirectory)
                                .OrderByDescending(file => file.LastWriteTime)
                                .FirstOrDefault();

                        if (latest == null)
                        {
                            // clsLog.WriteLogAzure("File not found at " + RemotePath);
                            _logger.LogInformation("File not found at {0}", RemotePath);
                            return false;
                        }
                        RemotePath = RemotePath + "/" + latest.Name;
                        lastWriteTime = latest.LastWriteTime;
                        downloadFileName = latest.Name;
                        isDownloadExactFile = true;
                    }
                    else if (messageType == MessageData.MessageTypeName.SCHEDULEUPLOAD)
                    {
                        transferOptions.FileMask = pathToMask + "*.ssim;" + pathToMask + "*.sim;" + pathToMask + "*.dat";
                    }
                    else if (messageType == MessageData.MessageTypeName.SSM)
                    {
                        //transferOptions.FileMask = "*.txt;*.ssm";
                        transferOptions.FileMask = pathToMask + "*.txt;" + pathToMask + "*.ssm;" + pathToMask + "*.rcv";
                    }
                    else if (messageType == MessageData.MessageTypeName.ASM)
                    {
                        transferOptions.FileMask = pathToMask + "*.txt;" + pathToMask + "*.asm;" + pathToMask + "*.rcv";
                    }
                    else if (messageType == MessageData.MessageTypeName.PHCUSTOMREGISTRY)
                    {
                        transferOptions.FileMask = pathToMask + "*.xls;" + pathToMask + "*.xlsb;" + pathToMask + "*.xlsx";
                    }
                    else if (messageType == MessageData.MessageTypeName.SISFILES)
                    {
                        transferOptions.FileMask = pathToMask + "*.ZIP;" + pathToMask + "*.ZIP;" + pathToMask + "*.ZIP";
                    }
                    else if (messageType == MessageData.MessageTypeName.EXCELUPLOADBOOKINGFFR)
                    {
                        transferOptions.FileMask = pathToMask + "*.xls;" + pathToMask + "*.xlsx";
                    }
                    #endregion Add Mask

                    #region : Read ASM/SSM in order to timestamp :
                    if (messageType == "ASM" || messageType == "SSM")
                    {
                        string msgBody = string.Empty;
                        TransferOperationResult transferResultASMSSM;
                        RemoteDirectoryInfo remoteDirectoryInfo = session.ListDirectory(RemotePath);
                        var sortedFilesList =
                            remoteDirectoryInfo.Files
                                    .Where(file => !file.IsDirectory)
                                    .Where(file => file.Name.ToUpper().EndsWith(".TXT") || file.Name.ToUpper().EndsWith("." + messageType) || file.Name.ToUpper().EndsWith(".RCV"))
                                    .OrderBy(file => file.Name)
                                    .OrderBy(file => file.LastWriteTime);

                        if (sortedFilesList.Count() == 0)
                            return true;

                        transferResultASMSSM = session.GetFiles(RemotePath, "/", false, transferOptions);

                        foreach (var file in sortedFilesList)
                        {
                            var item1 = transferResultASMSSM.Transfers.First(i => i.FileName == RemotePath + "/" + file.Name);
                            var streamReader = new StreamReader(item1.Destination, Encoding.UTF8);
                            msgBody = streamReader.ReadToEnd();

                            if (messageType == "SSM")
                            {
                                try
                                {
                                    // clsLog.WriteLogAzure("SSM File Processing Start for file name- " + item1.FileName + "");
                                    _logger.LogInformation("SSM File Processing Start for file name- {0}" + item1.FileName);

                                    int loopcount = 0;
                                    int j = 0;
                                    string Msg_Body = "";

                                    string[] SSMFileList = msgBody.Split(new string[] { "SSM" }, StringSplitOptions.RemoveEmptyEntries);

                                    int Total = SSMFileList.Length;
                                    int Count = Total / 300;

                                    // clsLog.WriteLogAzure("Total No. of Records in SSM File is- " + item1.FileName + " =  " + Convert.ToString(Total) + " & Total Character is " + msgBody.Length + "");
                                    // clsLog.WriteLogAzure("For Loop will be Run  " + item1.FileName + " = " + Convert.ToString(Count) + "  Times");
                                    _logger.LogInformation("Total No. of Records in SSM File is- {0} =  {1} & Total Character is {2}", item1.FileName, Convert.ToString(Total), msgBody.Length);
                                    _logger.LogInformation("For Loop will be Run {0} = {1} Times", item1.FileName, Convert.ToString(Count));

                                    if (Count > 0)
                                    {
                                        // clsLog.WriteLogAzure("For Loop Start for file Name- " + item1.FileName + "");
                                        _logger.LogInformation("For Loop Start for file Name- {0}", item1.FileName);

                                        for (int i = 0; i < Count; i++)
                                        {
                                            for (j = loopcount; j < 300 + loopcount; j++)
                                            {
                                                if (SSMFileList[j].Contains("\r\nUTC\r\n"))
                                                {
                                                    Msg_Body = Msg_Body + SSMFileList[j].Replace("\r\nUTC\r\n", "SSM\r\nUTC\r\n").TrimStart();
                                                }
                                                else if (SSMFileList[j].Contains("\nUTC\n"))
                                                {
                                                    Msg_Body = Msg_Body + SSMFileList[j].Replace("\nUTC\n", "SSM\nUTC\r\n").TrimStart();
                                                }
                                                else if (SSMFileList[j].Contains("\r\nLT\r\n"))
                                                {
                                                    Msg_Body = Msg_Body + SSMFileList[j].Replace("\r\nLT\r\n", "SSM\r\nLT\r\n").TrimStart();
                                                }
                                                else if (SSMFileList[j].Contains("\nLT\n"))
                                                {
                                                    Msg_Body = Msg_Body + SSMFileList[j].Replace("\nLT\n", "SSM\nLT\n").TrimStart();
                                                }
                                                else
                                                {
                                                    Msg_Body = Msg_Body + SSMFileList[j].Replace("UTC", "SSM\nUTC").TrimStart();
                                                }
                                            }

                                            // clsLog.WriteLogAzure("File Name- " + item1.FileName + "  " + i + "  loop message body length is : " + Msg_Body.Length);
                                            _logger.LogInformation("File Name- {0} {1} loop message body length is {2}", item1.FileName, i, Msg_Body.Length);

                                            if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + item1.FileName, Msg_Body, "SITASFTP", "", DateTime.UtcNow, DateTime.UtcNow, messageType, "Active", "SITA"))
                                            {
                                                Msg_Body = "";
                                                loopcount = j;
                                                // clsLog.WriteLogAzure(messageType + " Messages Saved to Inbox: " + item1.FileName + " " + file.LastWriteTime.ToString());
                                                _logger.LogInformation(messageType + " Messages Saved to Inbox: {0} {1}" + item1.FileName, file.LastWriteTime);
                                            }
                                            else
                                            {
                                                Msg_Body = "";
                                                // clsLog.WriteLogAzure("Error occured while saving to inbox: " + messageType + ": " + item1.FileName + " " + file.LastWriteTime.ToString());
                                                _logger.LogInformation("Error occured while saving to inbox: {0}: {1} {2}", messageType, item1.FileName, file.LastWriteTime);
                                            }

                                        }

                                        for (int k = loopcount; k < Total; k++)
                                        {
                                            if (SSMFileList[k].Contains("\r\nUTC\r\n"))
                                            {
                                                Msg_Body = Msg_Body + SSMFileList[k].Replace("\r\nUTC\r\n", "SSM\r\nUTC\r\n").TrimStart();
                                            }
                                            else if (SSMFileList[k].Contains("\nUTC\n"))
                                            {
                                                Msg_Body = Msg_Body + SSMFileList[k].Replace("\nUTC\n", "SSM\nUTC\r\n").TrimStart();
                                            }
                                            else if (SSMFileList[k].Contains("\r\nLT\r\n"))
                                            {
                                                Msg_Body = Msg_Body + SSMFileList[k].Replace("\r\nLT\r\n", "SSM\r\nLT\r\n").TrimStart();
                                            }
                                            else if (SSMFileList[k].Contains("\nLT\n"))
                                            {
                                                Msg_Body = Msg_Body + SSMFileList[k].Replace("\nLT\n", "SSM\nLT\n").TrimStart();
                                            }
                                            else
                                            {
                                                Msg_Body = Msg_Body + SSMFileList[k].Replace("UTC", "SSM\nUTC").TrimStart();
                                            }
                                        }

                                        // clsLog.WriteLogAzure("File Name - " + item1.FileName + " - Remaining No. of Records in SSM File : " + Convert.ToString(Total - loopcount));
                                        // clsLog.WriteLogAzure("File Name - " + item1.FileName + " - Last loop message body length is : " + Msg_Body.Length);
                                        _logger.LogInformation("File Name - {0} - Remaining No. of Records in SSM File : {1}", item1.FileName, Convert.ToString(Total - loopcount));
                                        _logger.LogInformation("File Name - {0} - Last loop message body length is : {1}", item1.FileName, Msg_Body.Length);

                                        if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + item1.FileName, Msg_Body, "SITASFTP", "", DateTime.UtcNow, DateTime.UtcNow, messageType, "Active", "SITA"))
                                        {
                                            Msg_Body = "";
                                            // clsLog.WriteLogAzure(messageType + " Messages Saved to Inbox: " + item1.FileName + " " + file.LastWriteTime.ToString());
                                            _logger.LogInformation("{0} Messages Saved to Inbox: {1} {2}", messageType, item1.FileName, file.LastWriteTime);
                                        }
                                        else
                                        {
                                            Msg_Body = "";
                                            // clsLog.WriteLogAzure("Error occured while saving to inbox: " + messageType + ": " + item1.FileName + " " + file.LastWriteTime.ToString());
                                            _logger.LogWarning("Error occured while saving to inbox: {0}: {1} {2}", messageType, item1.FileName, file.LastWriteTime.ToString());
                                        }

                                    }
                                    else
                                    {
                                        // clsLog.WriteLogAzure("File Name :- " + item1.FileName + " - Total No Of Records is : " + Total);
                                        // clsLog.WriteLogAzure("File Name - " + item1.FileName + " - Total body Length is : " + msgBody.Length);
                                        _logger.LogInformation($"File Name :- {item1.FileName} - Total No Of Records is : {Total}");
                                        _logger.LogInformation($"File Name - {item1.FileName} - Total body Length is : {msgBody.Length}");

                                        if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + item1.FileName, msgBody, "SITASFTP", "", DateTime.UtcNow, DateTime.UtcNow, messageType, "Active", "SITA"))
                                        {
                                            Msg_Body = "";
                                            // clsLog.WriteLogAzure(messageType + " Messages Saved to Inbox: " + item1.FileName + " " + file.LastWriteTime.ToString());
                                            _logger.LogInformation("{0} Messages Saved to Inbox: {1} {2}", messageType, item1.FileName, file.LastWriteTime);
                                        }
                                        else
                                        {
                                            Msg_Body = "";
                                            // clsLog.WriteLogAzure("Error occured while saving to inbox: " + messageType + ": " + item1.FileName + " " + file.LastWriteTime.ToString());
                                            _logger.LogWarning("Error occured while saving to inbox: {0}: {1} {2}", messageType, item1.FileName, file.LastWriteTime);

                                        }
                                    }

                                    if (archivalPath.Trim() != string.Empty && archivalPath.Contains("/"))
                                    {
                                        TransferOperationResult transferOperationResultPutFiles;
                                        transferOperationResultPutFiles = session.PutFiles(item1.Destination, archivalPath.Trim(), false, null);
                                        if (transferOperationResultPutFiles.Transfers.Count < 1 || transferOperationResultPutFiles.Failures.Count > 0)
                                            // clsLog.WriteLogAzure("Error on archival: " + messageType + ": " + item1.FileName);
                                            _logger.LogInformation("Error on archival: {0} : {1}", messageType, item1.FileName);
                                        if (transferOperationResultPutFiles.Transfers.Count > 0 && transferOperationResultPutFiles.Transfers[0].Error != null)
                                            // clsLog.WriteLogAzure("Error on archival: " + messageType + ": " + item1.FileName + ": "
                                            //     + Convert.ToString(transferOperationResultPutFiles.Transfers[0].Error == null ? "" : transferOperationResultPutFiles.Transfers[0].Error.Message));
                                            _logger.LogInformation($"Error on archival: {messageType} : {item1.FileName} : {Convert.ToString(transferOperationResultPutFiles.Transfers[0].Error == null ? "" : transferOperationResultPutFiles.Transfers[0].Error.Message)}");
                                    }
                                    // clsLog.WriteLogAzure("Removing file from SFTP: " + item1.FileName);
                                    _logger.LogInformation("Removing file from SFTP: {0}", item1.FileName);
                                    session.RemoveFiles(item1.FileName);

                                }
                                catch (Exception ex)
                                {

                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

                                    #region : SSM Failed alert :
                                    string SSM_FailedAlertEmailID = "";
                                    string fileName = string.Empty;

                                    //SSM_FailedAlertEmailID = genericFunction.ReadValueFromDb("SSMAlert");
                                    SSM_FailedAlertEmailID = ConfigCache.Get("SSMAlert");

                                    fileName = item1.FileName;

                                    if (SSM_FailedAlertEmailID != "")
                                    {
                                        await _genericFunction.SaveMessageOutBox("SSM Failed alert", "Hi,\r\n\r\n" + "Below SSM file are getting failed during processing. " + "\r\n" + "File Name :- " + fileName + "\r\n\r\nThanks.", "", SSM_FailedAlertEmailID, "", 0);
                                    }

                                    #endregion SSM Failed alert

                                    return false;
                                }
                            }
                            else
                            {

                                if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + item1.FileName, msgBody, "SITASFTP", "", DateTime.UtcNow, DateTime.UtcNow, messageType, "Active", "SITA"))
                                {
                                    if (archivalPath.Trim() != string.Empty && archivalPath.Contains("/"))
                                    {
                                        TransferOperationResult transferOperationResultPutFiles;
                                        transferOperationResultPutFiles = session.PutFiles(item1.Destination, archivalPath.Trim(), false, null);
                                        if (transferOperationResultPutFiles.Transfers.Count < 1 || transferOperationResultPutFiles.Failures.Count > 0)
                                            // clsLog.WriteLogAzure("Error on archival: " + messageType + ": " + item1.FileName);
                                            _logger.LogInformation("Error on archival: {0} : {1}", messageType, item1.FileName);
                                        if (transferOperationResultPutFiles.Transfers.Count > 0 && transferOperationResultPutFiles.Transfers[0].Error != null)
                                            // clsLog.WriteLogAzure("Error on archival: " + messageType + ": " + item1.FileName + ": "
                                            //     + Convert.ToString(transferOperationResultPutFiles.Transfers[0].Error == null ? "" : transferOperationResultPutFiles.Transfers[0].Error.Message));
                                            _logger.LogInformation("Error on archival: {0} : {1} : {2}", messageType, item1.FileName
                                                , Convert.ToString(transferOperationResultPutFiles.Transfers[0].Error == null ? "" : transferOperationResultPutFiles.Transfers[0].Error.Message));
                                    }
                                    // clsLog.WriteLogAzure("Removing file from SFTP: " + item1.FileName);
                                    _logger.LogInformation("Removing file from SFTP: {0}", item1.FileName);
                                    session.RemoveFiles(item1.FileName);
                                }
                                else
                                {
                                    // clsLog.WriteLogAzure("Error occured while saving to inbox: " + messageType + ": " + item1.FileName + " " + file.LastWriteTime.ToString());
                                    _logger.LogWarning("Error occured while saving to inbox: {0} : {1} {2}", messageType, item1.FileName, file.LastWriteTime);
                                }

                            }


                            streamReader.Dispose();
                        }
                        return true;
                    }
                    #endregion Read ASM/SSM in order to timestamp

                    TransferOperationResult transferResult;
                    if (isDownloadExactFile)
                        transferResult = session.GetFiles(RemotePath, DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + downloadFileName, false, transferOptions);
                    else
                        transferResult = session.GetFiles(RemotePath, "/", false, transferOptions);
                    transferResult.Check();/// Throw on any error
                    int x = 0;
                    //genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup");
                    //genericFunction.GetConfigurationValues("SISFileAutomation")
                    //genericFunction.ReadValueFromDb("UploadManifestToBlob")
                    //genericFunction.ReadValueFromDb("M_AMF_RES_Container")
                    //genericFunction.ReadValueFromDb("H_AMF_RES_Container")

                    var keepSITAFTPMessageBackup = ConfigCache.Get("KeepSITAFTPMessageBackup");
                    var sISFileAutomation = ConfigCache.Get("SISFileAutomation");
                    var uploadManifestToBlob = ConfigCache.Get("UploadManifestToBlob");
                    var m_AMF_RES_Container = ConfigCache.Get("M_AMF_RES_Container");
                    var h_AMF_RES_Container = ConfigCache.Get("H_AMF_RES_Container");

                    foreach (TransferEventArgs trn in transferResult.Transfers)
                    {
                        x++;
                        string strMessage = string.Empty;

                        if (!string.IsNullOrEmpty(_genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")) && Convert.ToBoolean(keepSITAFTPMessageBackup))
                        {
                            session.PutFiles(trn.Destination, "/" + RemotePath + "/Backup/", false, null);
                        }
                        var streamReader = new StreamReader(trn.Destination, Encoding.UTF8);
                        strMessage = streamReader.ReadToEnd();
                        string extension = Path.GetExtension(trn.FileName);

                        //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

                        if (messageType == MessageData.MessageTypeName.DIMS_Cubiscan || messageType == MessageData.MessageTypeName.JPEG_Cubiscan)
                        {
                            if (messageType == MessageData.MessageTypeName.DIMS_Cubiscan)
                            {
                                streamReader.Close();
                                await ProcessFile(x, trn.Destination, trn.FileName);
                                string FlNamewithoutExt = Path.GetFileNameWithoutExtension(trn.FileName);
                                Stream messageStream = GenerateStreamFromString(strMessage);
                                string url = _genericFunction.UploadToBlob(messageStream, (FlNamewithoutExt + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + extension), "DimensionFiles"); //change file name and append utc time to file name
                                // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                                _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                                session.RemoveFiles(trn.FileName);
                                continue;
                            }

                            if (messageType == MessageData.MessageTypeName.JPEG_Cubiscan)
                            {
                                Stream messageStream = GenerateStreamFromString(strMessage);
                                bool flag = false;
                                string strContainer = "epouch";
                                string DocumentName = "Acceptance Pictures";
                                extension = "JPG";
                                string Documentfilename = "";
                                string DocumentType = "PIC";
                                string fileExtention = Path.GetExtension(trn.Destination).ToLower();
                                string onlyFileName = Path.GetFileName(trn.FileName);
                                string[] ArrayFileName = onlyFileName.Split('-');
                                string awbNumber = ArrayFileName[1];
                                Documentfilename = awbNumber.Substring(0, 3) + awbNumber.Substring(3, 8) + DocumentType + 1;
                                byte[] bytes = System.IO.File.ReadAllBytes(trn.Destination);
                                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes, 0, bytes.Length);
                                byte[] byteValue = ms.ToArray();
                                string FileUrl = _genericFunction.UploadToBlob(ms, awbNumber.Substring(0, 3) + awbNumber.Substring(3, 8) + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + fileExtention, strContainer);
                                flag = await _genericFunction.UploadDocumentsOnEpouch(awbNumber.Substring(0, 3) + awbNumber.Substring(3, 8), DocumentName, "", "1", extension, new byte[0], Documentfilename, FileUrl);
                                // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                                _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                                session.RemoveFiles(trn.FileName);
                                continue;
                            }
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.SCHEDULEUPLOAD.ToString().ToUpper() && _uploadMasterCommon.IsFileValid(UploadMasterType.FlightSchedule, trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)))
                        {
                            streamReader.Close();
                            string FileName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.FlightSchedule);
                            await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.FlightSchedule, UserName);
                            // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                            _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                            session.RemoveFiles(trn.FileName);
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.FLIGHTCAPACITY.ToString().ToUpper() && _uploadMasterCommon.IsFileValid(UploadMasterType.FlightCapacity, trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)))
                        {
                            streamReader.Close();
                            string FileName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.FlightCapacity);
                            await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.FlightCapacity, UserName);
                            // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                            _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                            session.RemoveFiles(trn.FileName);
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.EXCHANGERATESFROMTOUPLOAD.ToString().ToUpper())
                        {
                            streamReader.Close();
                            string FileName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            FileName = FileName.Replace("-", "_");
                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.ExchangeRateFromTo);

                            if (_uploadMasterCommon.IsFileValid(UploadMasterType.ExchangeRateFromTo, FileName))
                                await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.ExchangeRateFromTo, UserName, lastWriteTime);
                            else
                                await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.ExchangeRateFromTo, UserName, lastWriteTime, "Invalid File");

                            if (File.Exists(trn.Destination))
                                File.Delete(trn.Destination);
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.FLIGHTPAXINFORMATIONUPLOAD.ToString().ToUpper() && _uploadMasterCommon.IsFileValid(UploadMasterType.FlightPaxInformation, trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)))
                        {
                            streamReader.Close();
                            string FileName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.FlightPaxInformation);
                            await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.FlightPaxInformation, UserName);
                            // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                            _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                            session.RemoveFiles(trn.FileName);
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.FLIGHTPAXFORECASTUPLOAD.ToString().ToUpper() && _uploadMasterCommon.IsFileValid(UploadMasterType.FlightPaxForecast, trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)))
                        {
                            streamReader.Close();
                            string FileName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.FlightPaxForecast);
                            await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.FlightPaxForecast, UserName);
                            // {clsLog.WriteLogAzure}("Removing file from SFTP: " + trn.FileName);
                            _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                            session.RemoveFiles(trn.FileName);
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.PHCUSTOMREGISTRY.ToString().ToUpper())
                        {
                            streamReader.Close();
                            string FileName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            FileName = FileName.Replace("-", "_");
                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.PHCustomRegistry);

                            if (_uploadMasterCommon.IsFileValid(UploadMasterType.ExchangeRateFromTo, FileName))
                                await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.PHCustomRegistry, UserName, lastWriteTime, "", trn.Destination);
                            else
                                await UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.PHCustomRegistry, UserName, lastWriteTime, "Invalid File", trn.Destination);

                            // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                            _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                            session.RemoveFiles(trn.FileName);
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.CARGOLOADXML.ToString().ToUpper())
                        {
                            //cls_SCMBL cls_scmbl = new cls_SCMBL();
                            await _cls_SCMBL.StoreXMLMessage(strMessage, trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1));

                            // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                            _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                            session.RemoveFiles(trn.FileName);
                        }
                        else if (messageType == MessageData.MessageTypeName.SISFILES && sISFileAutomation.Equals("True", StringComparison.OrdinalIgnoreCase))
                        {
                            streamReader.Close();
                            if (extension.Equals(".ZIP", StringComparison.OrdinalIgnoreCase))
                            {
                                string FileName = string.Empty, containerName = string.Empty;
                                FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + extension;
                                if (!FileName.Contains("CXML"))
                                {
                                    session.RemoveFiles(trn.FileName);
                                }
                                else
                                {
                                    FileStream fileStream = new FileStream(trn.Destination, FileMode.Open);
                                    string FileBlobPath = _genericFunction.UploadToBlob(fileStream, FileName, "SIS");
                                    if (!string.IsNullOrEmpty(FileBlobPath))
                                    {
                                        bool IsUpdatedInSK = UpdateSISFileInSK(FileName, FileBlobPath);
                                        if (IsUpdatedInSK)
                                            session.RemoveFiles(trn.FileName);
                                    }
                                }
                            }
                            continue;
                        }
                        else if (messageType.ToUpper() == MessageData.MessageTypeName.EXCELUPLOADBOOKINGFFR.ToString().ToUpper() && _uploadMasterCommon.IsFileValid(UploadMasterType.ExcelUploadBookingFFR, trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)))
                        {
                            string destinationUploadDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\TempUpload";
                            if (!Directory.Exists(destinationUploadDirectory))
                            {
                                Directory.CreateDirectory(destinationUploadDirectory);
                            }

                            // clsLog.WriteLogAzure("messageType= " + messageType);
                            _logger.LogInformation("messageType= {0}", messageType);
                            streamReader.Close();
                            string FileName = string.Empty, containerName = string.Empty;
                            FileName = Path.GetFileNameWithoutExtension(trn.FileName.Substring(trn.FileName.LastIndexOf('/') + 1)) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                            string destinationPath = Path.Combine(destinationUploadDirectory, FileName);
                            File.Copy(trn.Destination, destinationPath, overwrite: true);

                            DataSet dsContainerName = await _uploadMasterCommon.GetUploadMasterConfiguration(UploadMasterType.ExcelUploadBookingFFR);
                            // clsLog.WriteLogAzure("trn.Destination 1= " + trn.Destination);
                            _logger.LogInformation("trn.Destination 1= {0}", trn.Destination);
                            if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                            {
                                // clsLog.WriteLogAzure("trn.Destination 2= " + trn.Destination);
                                _logger.LogInformation("trn.Destination 2= {0}", trn.Destination);
                                containerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                                Stream messageStream = GenerateStreamFromString(strMessage);
                                UploadFileToBlob(dsContainerName, FileName, strMessage, UploadMasterType.ExcelUploadBookingFFR, UserName, null, "", destinationPath);
                                // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                                _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                                session.RemoveFiles(trn.FileName);
                            }
                            string[] files = Directory.GetFiles(destinationUploadDirectory);
                            foreach (string file in files)
                            {
                                if (file.Contains(FileName))
                                {
                                    File.Delete(file);
                                }
                            }
                        }
                        else
                        {
                            if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + trn.FileName, strMessage, "SITASFTP", "", DateTime.UtcNow, DateTime.UtcNow, messageType, "Active", "SITA"))
                            {
                                // clsLog.WriteLogAzure("Files Successfully Save in  Inbox for : " + trn.FileName);
                                _logger.LogInformation("Files Successfully Save in  Inbox for : {0}", trn.FileName);
                                //if (_genericFunction.ReadValueFromDb("UploadManifestToBlob") != string.Empty && Convert.ToBoolean(genericFunction.ReadValueFromDb("UploadManifestToBlob")))
                                if (uploadManifestToBlob != string.Empty && Convert.ToBoolean(uploadManifestToBlob))
                                {
                                    Stream messageStream = GenerateStreamFromString(strMessage);
                                    if (strMessage.ToUpper().Contains("MASTERMANIFESTRESPONSE"))
                                    {
                                        _genericFunction.UploadToBlob(messageStream, System.IO.Path.GetFileName(trn.FileName), m_AMF_RES_Container);
                                    }
                                    else if (strMessage.ToUpper().Contains("HOUSEMANIFESTRESPONSE"))
                                    {
                                        _genericFunction.UploadToBlob(messageStream, System.IO.Path.GetFileName(trn.FileName), h_AMF_RES_Container);
                                    }
                                }
                                // clsLog.WriteLogAzure("Removing file from SFTP: " + trn.FileName);
                                _logger.LogInformation("Removing file from SFTP: {0}", trn.FileName);
                                session.RemoveFiles(trn.FileName);
                            }
                            else
                            {
                                // clsLog.WriteLogAzure("Error on File Reading: " + trn.FileName);
                                _logger.LogWarning("Error on File Reading: {0}", trn.FileName);
                            }
                        }
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }

        }

        internal async Task UploadDataDumpFileToSFTP(string dataDumpFolderPath, string ZipFileName)
        {
            // clsLog.WriteLogAzure("dataDumpFolderPath: " + dataDumpFolderPath);
            _logger.LogInformation("dataDumpFolderPath: {0}", dataDumpFolderPath);
            try
            {
                int SleepSecond = 0; int count = 0;

                //SleepSecond = Convert.ToInt32(ConfigurationManager.AppSettings["SleepSecond"]);
                SleepSecond = Convert.ToInt32(_appConfig.Polling.SleepSeconds);


                string fileName = string.Empty;
                fileName = ZipFileName;
                string dirZIPFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataDumpCompressedFiles");
                dirZIPFile = Path.Combine(dirZIPFile, fileName);
                dirZIPFile = Path.Combine(dirZIPFile, fileName);

                // clsLog.WriteLogAzure("ZIP File Path: " + dirZIPFile);
                _logger.LogInformation("ZIP File Path: {0}", dirZIPFile);

                if (File.Exists(dirZIPFile))
                {
                Retry:
                    if (!SFTPUploadFile(dirZIPFile, dataDumpFolderPath, ZipFileName))
                    {
                        if (count <= 2)
                        {
                            count += 1;
                            System.Threading.Thread.Sleep(SleepSecond);
                            // clsLog.WriteLogAzure("DataDump Retry: Transfer To SFTP: Retry Count " + count);
                            _logger.LogWarning("DataDump Retry: Transfer To SFTP: Retry Count {0}", count);
                            goto Retry;
                        }
                        else
                        {
                            //string dataDumpAlertEmailID = "";
                            //dataDumpAlertEmailID = Convert.ToString(ConfigurationManager.AppSettings["DataDumpAlertEmailID"]);
                            string dataDumpAlertEmailID = _appConfig.Alert.DataDumpAlertEmailID;

                            //GenericFunction _genericFunction = new GenericFunction();

                            await _genericFunction.SaveMessageOutBox("Data dump alert", "Hi,\r\n\r\n" + fileName + "\r\nError: Failed to upload the data dump file to SFTP after 3 attempts.\r\n\r\nThanks."
                            , "", dataDumpAlertEmailID, "", 0);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task<string> UploadFileToBlob(DataSet dsContainerName, string FileName, string strMessage, string uploadMasterType, string UserName = "", DateTime? LastWriteTime = null, string ErrorMessage = "", string filePathToUpload = "")
        {
            string url = string.Empty;
            try
            {
                string ContainerName = string.Empty;
                int ProgressStatus = 0;
                int RecordCount = 0;
                int SuccessCount = 0;
                int FailCount = 0;
                bool IsProcessed = false;
                string FolderName = string.Empty;
                string ProcessMethod = string.Empty;
                string Station = string.Empty;
                string Status = "Process will start shortly";

                //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

                //GenericFunction genericFunction = new GenericFunction();

                //string BlobName = genericFunction.ReadValueFromDb("BlobStorageName");

                string BlobName = ConfigCache.Get("BlobStorageName");

                IsProcessed = ErrorMessage.Trim() == string.Empty ? false : true;

                if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                {
                    ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                }
                DataSet? dsSerialNumber = await _uploadMasterCommon.InsertMasterSummaryLog(0, FileName, uploadMasterType, UserName, RecordCount,
                                                   SuccessCount, FailCount, Station, Status, ProgressStatus,
                                                   BlobName, ContainerName, FolderName, ProcessMethod, ErrorMessage,
                                                   IsProcessed, LastWriteTime);

                if (dsSerialNumber != null && dsSerialNumber.Tables.Count > 0 && dsSerialNumber.Tables[0].Columns.Contains("RatesAlreadyUploaded"))
                {
                    return string.Empty;
                }

                Stream messageStream = GenerateStreamFromString(strMessage);
                url = _genericFunction.UploadToBlob(messageStream, FileName, ContainerName, filePathToUpload);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return url;
        }

        public async Task<bool> ProcessFile(int x, string filepath, string filename)
        {
            DataTable dataTableCubiScanExcelData = new DataTable("dataTableCubiScanExcelData");

            //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();

            //SQLServer dtb = new SQLServer();
            try
            {
                // filename = "apa_20160217_100948.csv";
                // filepath = "C:\\Users\\priyanka\\Desktop\\UTDS\\cubiscan\\cub\\apa_20160217_100948.csv";
                FileStream fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read);

                string fileExtention = Path.GetExtension(filepath).ToLower();

                string onlyFileName = Path.GetFileName(filepath);


                DataTable AWDDIMSData = new DataTable("AWBData");

                AWDDIMSData.Columns.Add("APA_DATE", typeof(string));
                AWDDIMSData.Columns.Add("APA_TIME", typeof(string));
                AWDDIMSData.Columns.Add("DIMSOURCE", typeof(string));
                AWDDIMSData.Columns.Add("DEVICENAME", typeof(string));

                AWDDIMSData.Columns.Add("BARCODE", typeof(string));
                AWDDIMSData.Columns.Add("SHIPMENT", typeof(string));
                AWDDIMSData.Columns.Add("LENGTH_GROSS", typeof(string));
                AWDDIMSData.Columns.Add("WIDTH_GROSS", typeof(string));
                AWDDIMSData.Columns.Add("HEIGHT_GROSS", typeof(string));

                AWDDIMSData.Columns.Add("WEIGHT_GROSS", typeof(string));

                AWDDIMSData.Columns.Add("PAL_ID", typeof(string));
                AWDDIMSData.Columns.Add("PAL_TOTALHEIGHT", typeof(string));
                AWDDIMSData.Columns.Add("PAL_WEIGHT", typeof(string));
                AWDDIMSData.Columns.Add("PAL_SHORTNAME", typeof(string));



                AWDDIMSData.Columns.Add("LENGTH_NET", typeof(string));
                AWDDIMSData.Columns.Add("WIDTH_NET", typeof(string));
                AWDDIMSData.Columns.Add("HEIGHT_NET", typeof(string));
                AWDDIMSData.Columns.Add("WEIGHT_NET", typeof(string));

                AWDDIMSData.Columns.Add("CUBEVOL_GROSS", typeof(string));
                AWDDIMSData.Columns.Add("CUBEVOL_NET", typeof(string));

                AWDDIMSData.Columns.Add("LENGTHUNIT", typeof(string));
                AWDDIMSData.Columns.Add("VOLUNIT", typeof(string));
                AWDDIMSData.Columns.Add("WEIGHTUNIT", typeof(string));

                AWDDIMSData.Columns.Add("FNAMEOVV", typeof(string));
                AWDDIMSData.Columns.Add("FNAMEPHA", typeof(string));
                AWDDIMSData.Columns.Add("FNAMEPHB", typeof(string));
                AWDDIMSData.Columns.Add("BRANCHID", typeof(string));
                AWDDIMSData.Columns.Add("COMPANY_ID", typeof(int));
                AWDDIMSData.Columns.Add("Ismerged", typeof(Boolean));
                AWDDIMSData.Columns.Add("InputFileName", typeof(string));
                AWDDIMSData.Columns.Add("UTCInsertDate", typeof(DateTime));

                fileStream.Close();
                DataTable dt = readCSVFile(filepath, AWDDIMSData, onlyFileName);
                //Pass this datatable to SP
                bool flag = true;

                //string[] PName = new string[] { "CubiScanDataTypeData" };
                //object[] PValues = new object[] { dt };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.Structured };

                SqlParameter[] sqlParameters =
                [
                    new SqlParameter("@CubiScanDataTypeData", SqlDbType.Structured)
                    {
                        Value = dt
                    }
                ];

                //flag = dtb.InsertData("spInsertAWBDIMSData", PName, PType, PValues);

                flag = await _readWriteDao.ExecuteNonQueryAsync("spInsertAWBDIMSData", sqlParameters);
                if (!flag)
                {
                    // clsLog.WriteLogAzure("Error in Updating DIMs Data Update:");
                    _logger.LogWarning("Error in Updating DIMs Data Update:");
                }

                //loop to upload files on blob and tblepouch
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(e.Message);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }


            return true;
        }

        public DataTable readCSVFile(string csvPath, DataTable AWDDIMSData, string filename)
        {

            string csvData = File.ReadAllText(csvPath);
            int rowcount = 0;
            try
            {
                foreach (string row in csvData.Split('\n'))
                {

                    if (!string.IsNullOrEmpty(row))
                    {
                        if (rowcount != 0)
                        {
                            AWDDIMSData.Rows.Add();
                            int i = 0;

                            //Execute a loop over the columns.  
                            foreach (string cell in row.Split(','))
                            {
                                AWDDIMSData.Rows[AWDDIMSData.Rows.Count - 1][i] = cell;
                                i++;
                            }

                            AWDDIMSData.Rows[AWDDIMSData.Rows.Count - 1][i] = "false";
                            AWDDIMSData.Rows[AWDDIMSData.Rows.Count - 1][i + 1] = filename;
                            AWDDIMSData.Rows[AWDDIMSData.Rows.Count - 1][i + 2] = System.DateTime.UtcNow;
                        }
                    }


                    rowcount++;
                }


            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(e.Message); 
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return AWDDIMSData;
        }

        #endregion

        public bool UpdateSISFileInSK(string FileName, string BlobPath)
        {
            bool IsAddedInSIS = false;
            try
            {
                string zipFileName = Path.GetFileName(FileName);
                string strAzulInvList = string.Empty;
                FileInfo fileInfo = new FileInfo(zipFileName);

                if (fileInfo.Extension.ToUpper().Equals(".ZIP"))
                {
                    string receivedZipFileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                    //DbEntity.CreateDBData createDBData = new QidWorkerRole.SIS.DAL.CreateDBData();

                    // If it is validation file (response file from SIS)
                    if (receivedZipFileName.ToUpper().Contains("_VAL") && (receivedZipFileName.Length == 35 || receivedZipFileName.Length == 36))
                    {
                        // check for original Invoice file name in database for which validation report is received.

                        //DbEntity.ReadDBData readDBData = new SIS.DAL.ReadDBData();

                        int fileStatusId = -9;
                        int receivablesFileID = 0;

                        if (receivedZipFileName.ToUpper().Contains("CXMLF-") && receivedZipFileName.Length == 35)
                        {
                            fileStatusId = _readDBData.IsOriginalFileExists(receivedZipFileName.Substring(0, 31).ToUpper() + ".ZIP", ref receivablesFileID);
                            if (fileStatusId == 2)
                            {
                                IsAddedInSIS = _createDBData.CreateReceivedISValidationFileHeaderData(receivedZipFileName + ".ZIP", BlobPath, DBNull.Value.ToString(), receivablesFileID, "SISAutomation");
                            }
                        }
                    }
                    // If it is normal invoice file (output file from SIS)
                    else
                    {
                        ModelClass.SupportingModels.FileData fileData = new ModelClass.SupportingModels.FileData
                        {
                            FileHeader = new ModelClass.FileHeaderClass
                            {
                                AirlineCode = receivedZipFileName.Substring(6, 3),
                                VersionNumber = 36,
                                FileInOutDirection = 0, // 0 for Incomming file
                                FileName = receivedZipFileName + ".XML",
                                FilePath = BlobPath,
                                FileStatusId = 4, // This is payables file and its status will be 4 i.e IS Validated.
                                CreatedBy = "SISAutomation",
                                CreatedOn = DateTime.UtcNow,
                                LastUpdatedBy = "SISAutomation",
                                ReadWriteOnSFTP = DateTime.UtcNow,
                                IsProcessed = 0
                            }
                        };
                        IsAddedInSIS = _createDBData.InsertSISFileHeaderData(fileData, out int newFileHeaderId);
                    }
                }
                return IsAddedInSIS;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return IsAddedInSIS;
            }
        }
        public async Task SISFilesReadProcess()
        {
            try
            {
                string appDomainCurrentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (!Directory.Exists(appDomainCurrentDomainBaseDirectory + @"\Logs"))
                    Directory.CreateDirectory(appDomainCurrentDomainBaseDirectory + @"\Logs");
                if (!Directory.Exists(appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\ZipFiles\ValidationReport"))
                    Directory.CreateDirectory(appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\ZipFiles\ValidationReport");
                if (!Directory.Exists(appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\UnZipFiles\ValidationReport"))
                    Directory.CreateDirectory(appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\UnZipFiles\ValidationReport");

                string tempLogFilePath = appDomainCurrentDomainBaseDirectory + @"\Logs\";
                string zipFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\ZipFiles\";
                string unZipFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\UnZipFiles\";
                string zipValidationReportFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\ZipFiles\ValidationReport\";
                string unZipValidationReportFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\UnZipFiles\ValidationReport\";

                //DbEntity.ReadDBData readDBData = new SIS.DAL.ReadDBData();
                string FileName = string.Empty;
                try
                {
                    // SIS Payables File
                    List<DbEntity.FileHeader> FileHeaderList = _readDBData.GetUnProcessedSISFiles();

                    //DbEntity.CreateDBData createDBData = new QidWorkerRole.SIS.DAL.CreateDBData();

                    foreach (var fileHeader in FileHeaderList)
                    {
                        // clsLog.WriteLogAzure("Read start: " + " (" + fileHeader.FileName + ")");
                        _logger.LogInformation("Read start: ({1})", fileHeader.FileName);
                        string zipFileName = Path.GetFileName(fileHeader.FilePath);
                        string strAzulInvList = string.Empty;
                        FileInfo fileInfo = new FileInfo(zipFileName);
                        Stream dataStream = Download_FromBlob(fileHeader.FilePath);
                        if (fileInfo.Extension.ToUpper().Equals(".ZIP"))
                        {
                            string receivedZipFileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                            // If it is normal invoice file (output file from SIS)
                            if (!receivedZipFileName.ToUpper().Contains("_VAL") && receivedZipFileName.Length != 35)
                            {
                                if (ValidateFileName(receivedZipFileName))
                                {
                                    DeleteContentsOfDirectory(zipFilePath);

                                    string path = Path.Combine(zipFilePath, zipFileName);
                                    using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                                    {
                                        dataStream.CopyTo(outputFileStream);
                                    }
                                    //FileUploadControl.SaveAs(zipFilePath + zipFileName);

                                    string zipFileNameWithoutExtention = Path.GetFileNameWithoutExtension(zipFilePath + zipFileName);

                                    DeleteContentsOfDirectory(unZipFilePath);

                                    var filePathToRead = ExtractInputFile(zipFilePath + zipFileName, unZipFilePath);
                                    if (!string.IsNullOrWhiteSpace(filePathToRead))
                                    {
                                        string extractedFileExtention = Path.GetExtension(filePathToRead).ToUpper();
                                        string extractedFileName = Path.GetFileName(filePathToRead);

                                        if (extractedFileExtention.Equals(".XML") || extractedFileExtention.Equals(".DAT"))
                                        {
                                            string unZippedFileNameWithoutExtention = Path.GetFileNameWithoutExtension(filePathToRead);

                                            if (zipFileNameWithoutExtention.ToLower().Equals(unZippedFileNameWithoutExtention.ToLower()))
                                            {
                                                //QidWorkerRole.SIS.FileHandling.SISFileReader sISFileReader = new SIS.FileHandling.SISFileReader();

                                                string logFilePath = string.Empty;
                                                bool success = false;
                                                (success, logFilePath) = await _sisFileReader.ReadSISFile(filePathToRead, "Qidadmin", logFilePath);

                                                //if (await _sisFileReader.ReadSISFile(filePathToRead, "Qidadmin", out logFilePath))
                                                if (success)
                                                {
                                                    //DbEntity.UpdateDBData
                                                    //ShowMessage(ref lblStatus, skResourceManager.GetString("msgUploadStatusFUS", skCultureInfo) + " (" + zipFileName + ") " + skResourceManager.GetString("msgUploadStatusFUS2", skCultureInfo), MessageType.SuccessMessage);
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrWhiteSpace(logFilePath))
                                                    {
                                                        // clsLog.WriteLogAzure("Upload status: Problem in File " + " (" + zipFileName + ") Upload. Please see downloaded log file for Errors ");
                                                        _logger.LogInformation($"Upload status: Problem in File {zipFileName} Upload. Please see downloaded log file for Errors ");
                                                        char[] splitChar = { '\\' };
                                                        string[] logFilePathSplit = logFilePath.Split(splitChar);

                                                        // temp path to copy log file to avoide in use exception
                                                        string destinationLogFilePath = tempLogFilePath + "\\" + logFilePathSplit[logFilePathSplit.Length - 1] + ".log";
                                                        File.Copy(logFilePath, destinationLogFilePath);
                                                    }
                                                    else
                                                    {
                                                        // clsLog.WriteLogAzure("Upload status: Problem in File " + " (" + zipFileName + ") Upload");
                                                        _logger.LogWarning($"Upload status: Problem in File ({zipFileName}) Upload");
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                // clsLog.WriteLogAzure("Upload status: Difference in ZIP and unZip file Names. Please upload a file with same name only.");
                                                _logger.LogInformation("Upload status: Difference in ZIP and unZip file Names. Please upload a file with same name only.");
                                            }
                                        }
                                        else
                                        {
                                            // clsLog.WriteLogAzure("Upload status: Unsupported extension of the file (" + extractedFileName + ") inside ZIP file.");
                                            _logger.LogWarning($"Upload status: Unsupported extension of the file ({extractedFileName}) inside ZIP file.");
                                        }
                                    }
                                    else
                                    {
                                        // clsLog.WriteLogAzure("Upload status: Problem in File Upload, File Path Not Found.");
                                        _logger.LogWarning("Upload status: Problem in File Upload, File Path Not Found.");
                                    }
                                }
                                else
                                {
                                    // clsLog.WriteLogAzure("Upload status: Invalid File Name. Please refer document for valid file naming conventions. Filename:" + zipFileName);// Format for XML: CXMLT-CCCYYYYMMPP.ZIP, Format for IDEC: CIDECT-CCCYYYYMMPP.ZIP";
                                    _logger.LogWarning("Upload status: Invalid File Name. Please refer document for valid file naming conventions. Filename: {0}" + zipFileName);// Format for XML: CXMLT-CCCYYYYMMPP.ZIP, Format for IDEC: CIDECT-CCCYYYYMMPP.ZIP";
                                }
                            }
                        }
                        else
                        {
                            // clsLog.WriteLogAzure("Upload status: Please upload a file having '.ZIP' extension only");
                            _logger.LogWarning("Upload status: Please upload a file having '.ZIP' extension only");
                        }
                        // clsLog.WriteLogAzure("Read end: " + " (" + fileHeader.FileName + ")");
                        _logger.LogInformation($"Read end: ({fileHeader.FileName})");
                    }
                }
                catch (Exception exception)
                {
                    // clsLog.WriteLogAzure("Error while uploading SIS Invoice file", exception);
                    _logger.LogError("Error while uploading SIS Invoice file {0}", exception);
                }
                try
                {
                    // SIS Validation Report File
                    List<DbEntity.ISValidationFileHeader> ISValidationFileHeaderList = _readDBData.GetUnProcessedSISValidationFiles();
                    foreach (var fileHeader in ISValidationFileHeaderList)
                    {
                        FileName = fileHeader.FileName;
                        // clsLog.WriteLogAzure("Read Validation File Start: " + " (" + FileName + ")");
                        _logger.LogInformation($"Read Validation File Start: ({FileName})");
                        string zipFileName = Path.GetFileName(FileName);
                        string strAzulInvList = string.Empty;
                        FileInfo fileInfo = new FileInfo(zipFileName);
                        Stream dataStream = Download_FromBlob(fileHeader.FilePath);
                        if (fileInfo.Extension.ToUpper().Equals(".ZIP"))
                        {
                            string receivedZipFileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                            // If it is validation file (response file from SIS)
                            if (receivedZipFileName.ToUpper().Contains("_VAL") && (receivedZipFileName.Length == 35 || receivedZipFileName.Length == 36))
                            {
                                // check for original Invoice file name in database for which validation report is received.
                                //QidWorkerRole.SIS.DAL.ReadDBData readDBData = new SIS.DAL.ReadDBData();
                                int fileStatusId = -9;
                                int receivablesFileID = 0;

                                if (receivedZipFileName.ToUpper().Contains("CXMLF-") && receivedZipFileName.Length == 35)
                                {
                                    fileStatusId = _readDBData.IsOriginalFileExists(receivedZipFileName.Substring(0, 31).ToUpper() + ".ZIP", ref receivablesFileID);
                                }
                                else if (receivedZipFileName.ToUpper().Contains("CIDECF-") && receivedZipFileName.Length == 36)
                                {
                                    fileStatusId = _readDBData.IsOriginalFileExists(receivedZipFileName.Substring(0, 32).ToUpper() + ".ZIP", ref receivablesFileID);
                                }
                                else
                                {
                                    fileStatusId = -8;
                                }
                                // clsLog.WriteLogAzure("Validation file Org status: " + " (" + fileStatusId + ")");
                                _logger.LogInformation($"Validation file Org status: ({fileStatusId})");
                                switch (fileStatusId)
                                {
                                    case 2:
                                        // Validation file upload code start.
                                        DeleteContentsOfDirectory(zipValidationReportFilePath);

                                        using (var fileStream = new FileStream(zipValidationReportFilePath + zipFileName, FileMode.Create))
                                        {
                                            dataStream.CopyTo(fileStream);
                                            // clsLog.WriteLogAzure("dataStream.CopyTo(fileStream): " + " (" + zipValidationReportFilePath + zipFileName + ")");
                                            _logger.LogInformation($"dataStream.CopyTo(fileStream): ({zipValidationReportFilePath + zipFileName})");
                                        }
                                        //FileUploadControl.SaveAs(zipValidationReportFilePath + zipFileName);

                                        string zipFileNameWithoutExtention = Path.GetFileNameWithoutExtension(zipValidationReportFilePath + zipFileName);

                                        DeleteContentsOfDirectory(unZipValidationReportFilePath);

                                        var ValidationFilePathToRead = ExtractInputValidationFile(zipValidationReportFilePath + zipFileName, unZipValidationReportFilePath + zipFileNameWithoutExtention);

                                        int rejectionOnValidationFailure = 0; //ViewState["RejectionOnValidationFailure"] != null ? Convert.ToInt32(ViewState["RejectionOnValidationFailure"].ToString()) : 0; // update it on load of this page
                                        bool onlineCorrectionAllowed = false; //ViewState["OnlineCorrectionAllowed"] != null ? Convert.ToBoolean(ViewState["OnlineCorrectionAllowed"].ToString()) : false; // update it on load of this page

                                        //QidWorkerRole.SIS.FileHandling.SISFileReader sISFileReader = new SIS.FileHandling.SISFileReader();

                                        string logValFilePath = string.Empty;
                                        strAzulInvList = string.Empty;
                                        int resultValidationFileUpload = _sisFileReader.ReadSISValidationReportFile(unZipValidationReportFilePath + "\\" + receivedZipFileName, zipValidationReportFilePath + zipFileName, "AutomationMsgService", rejectionOnValidationFailure, onlineCorrectionAllowed, ref logValFilePath, receivablesFileID, ref strAzulInvList);

                                        switch (resultValidationFileUpload)
                                        {
                                            case 1:
                                                //ShowMessage(ref lblStatus, skResourceManager.GetString("msgISValidationReportFile1", skCultureInfo) + " (" + zipFileName + ") " + skResourceManager.GetString("msgISValidationReportFile2", skCultureInfo), MessageType.SuccessMessage);
                                                //AccountingInterfaceBAL accountingInterfaceBAL = new AccountingInterfaceBAL();
                                                //objSISBAL.SaveInterlineBillingInterfaceDataI243(receivablesFileID, 1, "Qidadmin",DateTime.Now);

                                                string strMsgKey = string.Empty;
                                                await _sISBAL.CreateInterlineAuditLog("UploadISValidationReport", Convert.ToString(receivablesFileID), "Qidadmin", DateTime.Now, strMsgKey);
                                                break;
                                            case 2:
                                                //ShowMessage(ref lblStatus, skResourceManager.GetString("msgReportFileR1NotFound", skCultureInfo) + " " + zipFileName + ".", MessageType.ErrorMessage);
                                                // clsLog.WriteLogAzure("Upload status: Problem in File Upload, IS Validation Summary Report File R1 not found inside. FileName: " + zipFileName);
                                                _logger.LogInformation("Upload status: Problem in File Upload, IS Validation Summary Report File R1 not found inside. FileName: {0}", zipFileName);
                                                break;
                                            default:
                                                break;
                                        }

                                        break;
                                    case 1:
                                        //ShowMessage(ref lblStatus, "msgFileNotUploadToSIS", MessageType.ValidationMessage);
                                        // clsLog.WriteLogAzure("Upload status: Problem in File Upload, Validation File is not uploaded to SIS, FileName: " + zipFileName);
                                        _logger.LogInformation("Upload status: Problem in File Upload, Validation File is not uploaded to SIS, FileName: {0}", zipFileName);
                                        break;
                                    case 3:
                                        break;
                                    case 4:
                                        //ShowMessage(ref lblStatus, "msgAlreadyUploadedForInvList", MessageType.ErrorMessage);
                                        // clsLog.WriteLogAzure("Upload status: Problem in File Upload, Validation report file is already uploaded for Invoice File, FileName: " + zipFileName);
                                        _logger.LogInformation("Upload status: Problem in File Upload, Validation report file is already uploaded for Invoice File, FileName: {0}", zipFileName);
                                        break;
                                    case 8:
                                        //ShowMessage(ref lblStatus, "msgReportFileNamingConvent", MessageType.ErrorMessage);
                                        // clsLog.WriteLogAzure("Upload status: Invalid File Name.Please refer document for valid Validation report file naming conventions. FileName: " + zipFileName);
                                        _logger.LogInformation("Upload status: Invalid File Name.Please refer document for valid Validation report file naming conventions. FileName: {0}", zipFileName);
                                        break;
                                    default:
                                        //ShowMessage(ref lblStatus, "msgInvFileNotFound", MessageType.ErrorMessage);
                                        // clsLog.WriteLogAzure("Upload status: Problem in File Upload, Invoice File not found for uploading Validation FileName: " + zipFileName);
                                        _logger.LogInformation("Upload status: Problem in File Upload, Invoice File not found for uploading Validation FileName: {0}", zipFileName);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // clsLog.WriteLogAzure("Upload status: Please upload a file having '.ZIP' extension only");
                            _logger.LogWarning("Upload status: Please upload a file having '.ZIP' extension only");
                        }

                        // clsLog.WriteLogAzure("Validation file read End: " + " (" + FileName + ")");
                        _logger.LogInformation($"Validation file read End:({FileName})");
                    }

                }
                catch (Exception exception)
                {
                    // clsLog.WriteLogAzure("Error while uploading SIS Validation file", exception);
                    _logger.LogError("Error while uploading SIS Validation file {0}", exception);
                }
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure("Error while uploading SIS file", exception);
                _logger.LogError("Error while uploading SIS file {0}", exception);
            }
        }

        public string ExtractInputFile(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                using (Zipfile zipFile = Zipfile.Read(sourceDirectory))
                {
                    zipFile.ExtractAll(destinationDirectory);
                }

                var fileNameWithoutExtention = Path.GetFileNameWithoutExtension(sourceDirectory);
                var newFileNamePath = Directory.GetFiles(destinationDirectory, "*" + fileNameWithoutExtention + ".*").FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(newFileNamePath))
                {
                    return destinationDirectory + Path.GetFileName(newFileNamePath);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return string.Empty;
            }
        }
        /// <summary>
        /// Unzip Validation file.
        /// </summary>
        /// <param name="sourceDirectory">Input File Name with complete File Path.</param>
        /// <returns>Extracted File Directory</returns>
        public string ExtractInputValidationFile(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                using (Zipfile zipFile = Zipfile.Read(sourceDirectory))
                {
                    zipFile.ExtractAll(destinationDirectory);
                }

                return destinationDirectory;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("ExtractInputValidationFile ", ex);
                _logger.LogError("ExtractInputValidationFile {0}", ex);
                return destinationDirectory;
            }
        }


        public void DeleteContentsOfDirectory(string directoryPath)
        {
            try
            {
                System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                // delete all .zip files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.zip"))
                {
                    file.Delete();
                }

                // delete all .ZIP files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.ZIP"))
                {
                    file.Delete();
                }

                // delete all .dat files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.dat"))
                {
                    file.Delete();
                }

                // delete all .DAT files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.DAT"))
                {
                    file.Delete();
                }

                // delete all .XML files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.XML"))
                {
                    file.Delete();
                }

                // delete all .xml files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.xml"))
                {
                    file.Delete();
                }

                // delete all .CSV files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.CSV"))
                {
                    file.Delete();
                }

                // delete all .csv files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.csv"))
                {
                    file.Delete();
                }

                // delete all .TXT files.
                foreach (FileInfo file in directoryInfo.GetFiles("*.TXT"))
                {
                    if (!file.Name.Equals("TempFileToKeepFolderAfterAppReset.txt"))
                    {
                        file.Delete();
                    }
                }

                // delete all folders.
                foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                {
                    if (!dir.Name.Equals("ValidationReport"))
                    {
                        dir.Delete(true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public bool ValidateFileName(string fileNameWithoutExtention)
        {
            try
            {
                fileNameWithoutExtention = fileNameWithoutExtention.ToUpper();

                if (fileNameWithoutExtention.Substring(0, 1).Equals("C"))
                {
                    // XML: CXMLT-CCCYYYYMMPP
                    if (fileNameWithoutExtention.Length == 17)
                    {
                        string[] fileNameParts = fileNameWithoutExtention.Split('-');
                        if (!string.IsNullOrWhiteSpace(fileNameParts[0]))
                        {
                            if (fileNameParts[0].Equals("CXMLT"))
                            {
                                if (!string.IsNullOrWhiteSpace(fileNameParts[1]))
                                {
                                    if (fileNameParts[1].Length == 11)
                                    {
                                        if (Convert.ToInt32(fileNameParts[1].Substring(9, 2)) > 4 && Convert.ToInt32(fileNameParts[1].Substring(9, 2)) < 0)
                                        {
                                            return false;
                                        }
                                        else
                                        {
                                            DateTime tempdate;
                                            if (DateTime.TryParse(fileNameParts[1].Substring(3, 4) + '-' + fileNameParts[1].Substring(7, 2) + '-' + fileNameParts[1].Substring(9, 2), out tempdate))
                                            {
                                                return true;
                                            }
                                            else { return false; }
                                        }
                                    }
                                    else { return false; }
                                }
                                else { return false; }
                            }
                            else { return false; }
                        }
                        else { return false; }
                    }

                    // IDEC: CIDECT-CCCYYYYMMPP
                    else if (fileNameWithoutExtention.Length == 18)
                    {
                        string[] fileNameParts = fileNameWithoutExtention.Split('-');
                        if (!string.IsNullOrWhiteSpace(fileNameParts[0]))
                        {
                            if (fileNameParts[0].Equals("CIDECT"))
                            {
                                if (!string.IsNullOrWhiteSpace(fileNameParts[1]))
                                {
                                    if (fileNameParts[1].Length == 11)
                                    {
                                        if (Convert.ToInt32(fileNameParts[1].Substring(9, 2)) > 4 && Convert.ToInt32(fileNameParts[1].Substring(9, 2)) < 0)
                                        { return false; }
                                        else
                                        {
                                            DateTime tempdate;
                                            if (DateTime.TryParse(fileNameParts[1].Substring(3, 4) + '-' + fileNameParts[1].Substring(7, 2) + '-' + fileNameParts[1].Substring(9, 2), out tempdate))
                                            {
                                                return true;
                                            }
                                            else { return false; }
                                        }
                                    }
                                    else { return false; }
                                }
                                else { return false; }
                            }
                            else { return false; }
                        }
                        else { return false; }
                    }
                    else { return false; }
                }
                else { return false; }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public Stream Download_FromBlob(string filenameOrUrl)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                string containerName = "";
                string str = filenameOrUrl;
                string StorageName = _genericFunction.GetStorageName();
                string StorageKey = _genericFunction.GetStorageKey();
                if (filenameOrUrl.Contains('/'))
                {
                    //filenameOrUrl = filenameOrUrl.ToLower();
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }

                //Stream downloadStream = null;
                //containerName = containerName.ToLower();
                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(StorageName, StorageKey);
                //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //string sas = GetSASUrl(containerName, storageAccount);
                //StorageCredentialsSharedAccessSignature sasCreds = new StorageCredentialsSharedAccessSignature(sas);
                //CloudBlobClient sasBlobClient = new CloudBlobClient(storageAccount.BlobEndpoint,
                //new StorageCredentialsSharedAccessSignature(sas));
                //CloudBlob blob = sasBlobClient.GetBlobReference(containerName + @"/" + filenameOrUrl);

                Stream downloadStream = null;
                containerName = containerName.ToLower();

                // Enforce TLS 1.2
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create BlobServiceClient using account name + key
                string accountUrl = $"https://{StorageName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new Azure.Storage.StorageSharedKeyCredential(StorageName, StorageKey));

                // Generate SAS token using your existing GetSASUrl (updated overload)
                string sas = GetSASUrl(containerName, blobServiceClient);  // Updated method

                // Construct full SAS URI for the blob
                string blobSasUriString = $"{accountUrl}/{containerName}/{filenameOrUrl}?{sas}";
                Uri blobSasUri = new Uri(blobSasUriString);

                // Create BlobClient using SAS URI
                BlobClient blob = new BlobClient(blobSasUri);

                //CloudBlobContainer container = sasBlobClient.GetContainerReference(containerName);
                //CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(filenameOrUrl);

                //// provide the file download location below            
                //Stream file = File.OpenWrite(@"D:\" + filenameOrUrl);                 

                //cloudBlockBlob.DownloadToStream(file);
                try
                {
                    downloadStream = blob.OpenRead();
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure("Error on File Uploading FTP for :" + ex.Message.ToString());
                    _logger.LogError("Error on File Uploading FTP for : {0}", ex.Message.ToString());
                    return null;
                }
                return downloadStream;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error on File Uploading FTP for :" + ex.Message.ToString());
                _logger.LogError("Error on File Uploading FTP for : {0}" + ex.Message);
                return null;
            }
        }

        public async Task<bool> SITASFTPDownloadFile()
        {
            // clsLog.WriteLogAzure("Step-1: In SITASFTPDownloadFile()");
            _logger.LogError("Step-1: In SITASFTPDownloadFile()");
            bool status = false;
            //GenericFunction genericFunction = new GenericFunction();
            string ppkLocalFilePath = string.Empty;

            try
            {
                #region Process local RCV files
                //foreach (string fileName in Directory.GetFiles(@"D:\New", "*.RCV"))
                //{
                //    string message = File.ReadAllText(fileName);
                //    int indx = fileName.LastIndexOf("\\");
                //    string name = "MSG:/PROD/EDI_OTH/IN/" + fileName.Substring(indx + 2);
                //    if (genericFunction.SaveIncomingMessageInDatabase("MSG:" + name, message, "SITASFTP", "", DateTime.Now, DateTime.Now, "SITA", "Active", "SITA"))
                //    {
                //        File.Delete(fileName);
                //        clsLog.WriteLogAzure("Files Successfully Save in  Inbox for : " + fileName.Substring(indx + 2));
                //    }
                //    else
                //    {
                //        clsLog.WriteLogAzure("Error on File Reading" + fileName.Substring(indx + 2));
                //    }
                //}
                #endregion Process local RCV files

                //string SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
                //string SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
                //string SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
                //string ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
                //string SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
                //string StpCIMPINFolerParth = genericFunction.ReadValueFromDb("msgService_IN_FolderPath");
                //string StpGHAMCTINFolerParth = genericFunction.ReadValueFromDb("msgService_INGHAMCT_FolderPath");
                //string SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");

                string SFTPAddress = ConfigCache.Get("msgService_IN_SITAFTP");
                string SFTPUserName = ConfigCache.Get("msgService_IN_SITAUser");
                string SFTPPassWord = ConfigCache.Get("msgService_IN_SITAPWD");
                string ppkFileName = ConfigCache.Get("PPKFileName");
                string SFTPFingerPrint = ConfigCache.Get("msgService_IN_SFTPFingerPrint");
                string StpCIMPINFolerParth = ConfigCache.Get("msgService_IN_FolderPath");
                string StpGHAMCTINFolerParth = ConfigCache.Get("msgService_INGHAMCT_FolderPath");
                string SFTPPortNumber = ConfigCache.Get("msgService_IN_SITAPort");

                // clsLog.WriteLogAzure("Step-2: Get Configuations");
                _logger.LogInformation("Step-2: Get Configuations");

                if (SFTPAddress.Trim() != "" && SFTPUserName.Trim() != "" && (SFTPPassWord.Trim() != "" || ppkFileName.Trim() != "") && SFTPFingerPrint != "" && StpCIMPINFolerParth != "" && SFTPPortNumber != "")
                {
                    // clsLog.WriteLogAzure("Step-3: Start Connection");
                    _logger.LogInformation("Step-3: Start Connection");

                    int portNumber = Convert.ToInt32(SFTPPortNumber.Trim());

                    if (ppkFileName != string.Empty)
                    {
                        ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                    }
                    // Setup session options

                    //SessionOptions sessionOptions;
                    //if (ppkLocalFilePath.Trim() != string.Empty)
                    //{
                    //    sessionOptions = new SessionOptions
                    //    {
                    //        Protocol = Protocol.Sftp,
                    //        HostName = SFTPAddress,
                    //        UserName = SFTPUserName,
                    //        SshPrivateKeyPath = ppkLocalFilePath,
                    //        PortNumber = portNumber,
                    //        SshHostKeyFingerprint = SFTPFingerPrint
                    //    };
                    //}
                    //else
                    //{
                    //    sessionOptions = new SessionOptions
                    //    {
                    //        Protocol = Protocol.Sftp,
                    //        HostName = SFTPAddress,
                    //        UserName = SFTPUserName,
                    //        Password = SFTPPassWord,
                    //        PortNumber = portNumber,
                    //        SshHostKeyFingerprint = SFTPFingerPrint
                    //    };
                    //}

                    WinSCP.SessionOptions sessionOptions = new WinSCP.SessionOptions
                    {
                        Protocol = WinSCP.Protocol.Sftp,
                        HostName = SFTPAddress,
                        UserName = SFTPUserName,
                        PortNumber = portNumber,
                        SshHostKeyFingerprint = SFTPFingerPrint,
                        SshPrivateKeyPath = !string.IsNullOrEmpty(ppkLocalFilePath) ? ppkLocalFilePath : null,
                        Password = string.IsNullOrEmpty(ppkLocalFilePath) ? SFTPPassWord : null
                    };

                    // clsLog.WriteLogAzure("Step-3: Created Session Object");
                    _logger.LogInformation("Step-3: Created Session Object");

                    using (WinSCP.Session session = new WinSCP.Session())
                    {
                        // Connect
                        session.Open(sessionOptions);

                        // clsLog.WriteLogAzure("Step-4: Connected to the: " + SFTPAddress.Trim());
                        _logger.LogInformation("Step-4: Connected to the: {0}", SFTPAddress.Trim());

                        if (StpCIMPINFolerParth.Trim() != string.Empty)
                        {
                            session.GetFileInfo(StpCIMPINFolerParth);

                            TransferOptions transferOptions = new TransferOptions();
                            transferOptions.TransferMode = TransferMode.Binary;

                            string pathToMask = StpCIMPINFolerParth + "/";
                            transferOptions.FileMask = pathToMask + "*.rcv;" + pathToMask + "*.RCV";

                            TransferOperationResult transferResult;
                            transferResult = session.GetFiles(StpCIMPINFolerParth, "/", false, transferOptions);

                            // Throw on any error
                            transferResult.Check();

                            bool isFilesAvailable = false;
                            //genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")
                            var keepSITAFTPMessageBackup = ConfigCache.Get("KeepSITAFTPMessageBackup");

                            foreach (TransferEventArgs trn in transferResult.Transfers)
                            {
                                isFilesAvailable = true;
                                // clsLog.WriteLogAzure("Step-4.1: Files Exists");
                                _logger.LogInformation("Step-4.1: Files Exists");

                                // clsLog.WriteLogAzure("Step-5: File Name Received: " + trn.FileName);
                                _logger.LogInformation("Step-5: File Name Received: {0}", trn.FileName);

                                string strMessage = string.Empty;


                                //if (!string.IsNullOrEmpty(genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")) && Convert.ToBoolean(genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")))
                                if (!string.IsNullOrEmpty(keepSITAFTPMessageBackup) && Convert.ToBoolean(keepSITAFTPMessageBackup))

                                {
                                    session.PutFiles(trn.Destination, "/" + StpCIMPINFolerParth.Replace("/", string.Empty) + "Backup/", false, null);
                                }
                                var streamReader = new StreamReader(trn.Destination, Encoding.UTF8);
                                strMessage = streamReader.ReadToEnd();

                                if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + trn.FileName, strMessage, "SITASFTP", "", DateTime.Now, DateTime.Now, "SITA", "Active", "SITA"))
                                {
                                    session.RemoveFiles(trn.FileName);
                                    // clsLog.WriteLogAzure("Step-6: Files Successfully Save in  Inbox for : " + trn.FileName);
                                    _logger.LogInformation("Step-6: Files Successfully Save in  Inbox for : {0}", trn.FileName);
                                }
                                else
                                {
                                    // clsLog.WriteLogAzure("Step-7: Error on File Reading: " + trn.FileName);
                                    _logger.LogWarning("Step-7: Error on File Reading: {0}", trn.FileName);
                                }
                                streamReader.Dispose();
                            }

                            if (!isFilesAvailable)
                            {
                                // clsLog.WriteLogAzure("Step-8: No file available in folder");
                                _logger.LogInformation("Step-8: No file available in folder");
                            }
                        }
                        ///MCT GHA FIle  Download from
                        if (StpGHAMCTINFolerParth.Trim() != string.Empty)
                        {
                            TransferOptions transferGhaMCTOptions = new TransferOptions();
                            transferGhaMCTOptions.TransferMode = TransferMode.Binary;

                            TransferOperationResult transferGHAMCtResult;
                            transferGHAMCtResult = session.GetFiles(StpGHAMCTINFolerParth, "/", false, transferGhaMCTOptions);
                            transferGHAMCtResult.Check();

                            //genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")
                            var keepSITAFTPMessageBackup = ConfigCache.Get("KeepSITAFTPMessageBackup");

                            foreach (TransferEventArgs trnGHAMCTIN in transferGHAMCtResult.Transfers)
                            {
                                string strMessage = string.Empty;

                                //if (!string.IsNullOrEmpty(genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")) && Convert.ToBoolean(genericFunction.ReadValueFromDb("KeepSITAFTPMessageBackup")))
                                if (!string.IsNullOrEmpty(keepSITAFTPMessageBackup) && Convert.ToBoolean(keepSITAFTPMessageBackup))

                                {
                                    session.PutFiles(trnGHAMCTIN.Destination, "/" + StpGHAMCTINFolerParth.Replace("/", string.Empty) + "Backup/", false, null);
                                }

                                var streamReader = new StreamReader(trnGHAMCTIN.Destination, Encoding.UTF8);
                                strMessage = streamReader.ReadToEnd();

                                if (await _genericFunction.SaveIncomingMessageInDatabase("MSG:" + trnGHAMCTIN.FileName, strMessage, "SITASFTP", "", DateTime.Now, DateTime.Now, "SITA", "Active", "SITA"))
                                {
                                    session.RemoveFiles(trnGHAMCTIN.FileName);
                                    // clsLog.WriteLogAzure("Files Successfully Save in tblInbox for : " + trnGHAMCTIN.FileName);
                                    _logger.LogInformation("Files Successfully Save in tblInbox for : {0}", trnGHAMCTIN.FileName);
                                }
                                else
                                {
                                    // clsLog.WriteLogAzure("Error on File Reading" + trnGHAMCTIN.FileName);
                                    _logger.LogWarning("Error on File Reading {0}", trnGHAMCTIN.FileName);
                                }
                                streamReader.Dispose();
                            }
                            session.Close();
                        }
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                status = false;
            }
            return status;
        }


        #region Below Method used for FTP for Specific AGI Message

        /// <summary>
        /// Below Method used uplpoad the file throug  FTP and also rename the file after  uploaded the file in the folder
        /// </summary>
        /// <param name="strFileText"></param>
        /// <returns></returns>
        public bool UploadfileThrougFTPAndRenamefileAfterUploaded(string ftpURL, string ftpUserName, string ftpPassword, string messageBody, string fileName, string fileExtension)
        {
            bool status = false;
            try
            {
                //GenericFunction gf = new GenericFunction();
                //fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
                string tempFileName = string.Empty;
                if (fileExtension.Trim().ToUpper() == "TXT")
                {
                    tempFileName = fileName + ".SND";
                }
                else
                {
                    tempFileName = fileName + ".txt";
                }
                string OriginalfileName = fileName + "." + fileExtension;
                string requestUri = ftpURL + "/" + tempFileName;

                //Upload the file on OutFolder

                FtpWebRequest myFtpWebRequest;
                FtpWebResponse myFtpWebResponse;
                StreamWriter myStreamWriter;

                myFtpWebRequest = (FtpWebRequest)WebRequest.Create(ftpURL + "/" + tempFileName);
                myFtpWebRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);


                myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                myFtpWebRequest.UseBinary = true;

                myStreamWriter = new StreamWriter(myFtpWebRequest.GetRequestStream());
                myStreamWriter.Write(messageBody);
                myStreamWriter.Close();
                myFtpWebResponse = (FtpWebResponse)myFtpWebRequest.GetResponse();
                myFtpWebResponse.Close();

                //Rename file in the folder
                FtpWebRequest renameRequest = (FtpWebRequest)WebRequest.Create(requestUri);
                renameRequest.UseBinary = true;
                renameRequest.UsePassive = true;
                renameRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                renameRequest.KeepAlive = true;
                renameRequest.Method = WebRequestMethods.Ftp.Rename;
                renameRequest.RenameTo = OriginalfileName;
                FtpWebResponse renameResponse = (FtpWebResponse)renameRequest.GetResponse();
                renameResponse.Close();
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                //scmeception.logexception(ref ex);
                // clsLog.WriteLogAzure("Error on File Uploading FTP for :" + ex.Message.ToString());
                _logger.LogError("Error on File Uploading FTP for : {0}", ex.Message);
                FTPConnectionAlert();
            }
            return status;
        }
        #endregion

        #region Download from Blob and upload to FTP
        public bool DownloadBlobAndFTPUpload(string BlobURL, string ftpURL, string ftpUserName, string ftpPassword, string messageBody, string fileName, string fileExtension)
        {
            bool status = false;
            WebClient wb = new WebClient();
            try
            {
                string filepath = BlobURL;



                //GenericFunction gf = new GenericFunction();

                //fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
                string tempFileName = fileName + ".txt";
                string OriginalfileName = fileName + "." + fileExtension.ToUpper();
                string requestUri = ftpURL + "/" + tempFileName;

                Stream data = DownloadFromBlob(BlobURL);



                //Upload the file on OutFolder

                FtpWebRequest myFtpWebRequest;
                //FtpWebResponse myFtpWebResponse;
                //StreamWriter myStreamWriter;

                myFtpWebRequest = (FtpWebRequest)WebRequest.Create(ftpURL + "/" + tempFileName);
                myFtpWebRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);


                myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                myFtpWebRequest.UseBinary = true;

                //myStreamWriter = new StreamWriter(myFtpWebRequest.GetRequestStream());
                Stream uploadStream = myFtpWebRequest.GetRequestStream();
                int bufferLength = 16000;
                byte[] buffer = new byte[bufferLength];
                int contentLength = data.Read(buffer, 0, bufferLength);

                while (contentLength != 0)
                {
                    uploadStream.Write(buffer, 0, contentLength);
                    contentLength = data.Read(buffer, 0, bufferLength);
                }
                uploadStream.Close();
                data.Close();
                myFtpWebRequest = null;

                //Rename file in the folder
                FtpWebRequest renameRequest = (FtpWebRequest)WebRequest.Create(requestUri);
                renameRequest.UseBinary = true;
                renameRequest.UsePassive = true;
                renameRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                renameRequest.KeepAlive = true;
                renameRequest.Method = WebRequestMethods.Ftp.Rename;
                renameRequest.RenameTo = OriginalfileName;
                FtpWebResponse renameResponse = (FtpWebResponse)renameRequest.GetResponse();
                renameResponse.Close();

                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                //scmeception.logexception(ref ex);
                // clsLog.WriteLogAzure("Error on File Uploading FTP for :" + ex.Message.ToString());
                _logger.LogError("Error on File Uploading FTP for : {0}", ex.Message);
                FTPConnectionAlert();
            }
            finally
            {
                if (wb != null)
                    wb.Dispose();
            }
            return status;
        }
        public Stream DownloadFromBlob(string filenameOrUrl)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();

                string containerName = "";
                string str = filenameOrUrl;
                string StorageName = _genericFunction.GetStorageName();
                string StorageKey = _genericFunction.GetStorageKey();
                if (filenameOrUrl.Contains('/'))
                {
                    //filenameOrUrl = filenameOrUrl.ToLower();
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }
                //Stream downloadStream = null;
                //containerName = containerName.ToLower();
                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(StorageName, StorageKey);
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //string sas = GetSASUrl(containerName, storageAccount);
                //StorageCredentialsSharedAccessSignature sasCreds = new StorageCredentialsSharedAccessSignature(sas);
                //CloudBlobClient sasBlobClient = new CloudBlobClient(storageAccount.BlobEndpoint,
                //new StorageCredentialsSharedAccessSignature(sas));
                //CloudBlob blob = sasBlobClient.GetBlobReference(containerName + @"/" + filenameOrUrl);

                Stream downloadStream = null;
                containerName = containerName.ToLower();

                // Enforce TLS 1.2
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create BlobServiceClient using account name + key
                string accountUrl = $"https://{StorageName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new Azure.Storage.StorageSharedKeyCredential(StorageName, StorageKey));

                // Generate SAS token using your existing GetSASUrl (updated overload)
                string sas = GetSASUrl(containerName, blobServiceClient);  // Updated method

                // Construct full SAS URI for the blob
                string blobSasUriString = $"{accountUrl}/{containerName}/{filenameOrUrl}?{sas}";
                Uri blobSasUri = new Uri(blobSasUriString);

                // Create BlobClient using SAS URI
                BlobClient blob = new BlobClient(blobSasUri);

                try
                {
                    downloadStream = blob.OpenRead();
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure("Error on File Uploading FTP for :" + ex.Message.ToString());
                    _logger.LogError("Error on File Uploading FTP for : {0}", ex);
                    return null;
                }
                return downloadStream;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error on File Uploading FTP for :" + ex.Message.ToString());
                _logger.LogError("Error on File Uploading FTP for : {0}", ex.Message);
                return null;
            }

        }

        //public static string GetSASUrl(string containerName, CloudStorageAccount storageAccount)
        //{
        //    GenericFunction genericFunction = new GenericFunction();
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        //    container.CreateIfNotExist();

        //    BlobContainerPermissions containerPermissions = new BlobContainerPermissions();

        //    string sasactivetime = genericFunction.GetConfigurationValues("BlobStorageactiveSASTime");
        //    double _SaSactiveTime = string.IsNullOrWhiteSpace(sasactivetime) ? 5 : Convert.ToDouble(sasactivetime);

        //    containerPermissions.SharedAccessPolicies.Add("defaultpolicy", new SharedAccessPolicy()
        //    {
        //        SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-1),
        //        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(_SaSactiveTime),
        //        Permissions = SharedAccessPermissions.Write | SharedAccessPermissions.Read | SharedAccessPermissions.List
        //    });

        //    string IsBlobPrivate = genericFunction.GetConfigurationValues("IsBlobPrivate");
        //    IsBlobPrivate = string.IsNullOrWhiteSpace(sasactivetime) ? "NA" : IsBlobPrivate.Trim();
        //    if (IsBlobPrivate == "1")
        //    {
        //        containerPermissions.PublicAccess = BlobContainerPublicAccessType.Off;
        //    }
        //    else
        //    {
        //        containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
        //    }
        //    container.SetPermissions(containerPermissions);
        //    string sas = container.GetSharedAccessSignature(new SharedAccessPolicy(), "defaultpolicy");
        //    return sas;
        //}

        public string GetSASUrl(string containerName, BlobServiceClient blobServiceClient)
        {
            try
            {
                // 1. Get (or create) the container
                BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
                container.CreateIfNotExists();               // same as container.CreateIfNotExist()

                // 2. Read configuration values
                string sasactivetime = ConfigCache.Get("BlobStorageactiveSASTime");
                double _SaSactiveTime = string.IsNullOrWhiteSpace(sasactivetime) ? 5 : Convert.ToDouble(sasactivetime);

                string isBlobPrivate = ConfigCache.Get("IsBlobPrivate");
                isBlobPrivate = string.IsNullOrWhiteSpace(isBlobPrivate) ? "NA" : isBlobPrivate.Trim();

                // 3. Build the shared-access-policy
                var policy = new BlobSignedIdentifier
                {
                    Id = "defaultpolicy",
                    AccessPolicy = new BlobAccessPolicy
                    {
                        StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
                        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_SaSactiveTime),
                        Permissions = "rwl" // CORRECT: string with r=read, w=write, l=list
                    }
                };

                // 4. Apply public-access setting
                PublicAccessType publicAccess = (isBlobPrivate == "1")
                    ? PublicAccessType.None
                    : PublicAccessType.BlobContainer;   // Container = full public read

                // 5. Set permissions + policy in ONE call
                container.SetAccessPolicy(
                    permissions: new[] { policy },
                    accessType: publicAccess);

                // 6. Generate the SAS token for the *container* using the stored policy
                BlobSasBuilder sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    Resource = "c",                     // container-level SAS
                    Identifier = "defaultpolicy"        // use the stored policy
                };

                // The SDK adds the leading '?' – we strip it to match the old behaviour
                string sas = container.GenerateSasUri(sasBuilder).Query.TrimStart('?');
                return sas;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }

        }

        #endregion

        /*Not in use*/
        //public bool QuickSFTPUpload()
        //{
        //    string SFTPAddress = string.Empty, UserName = string.Empty, Password = string.Empty, FingerPrint = string.Empty, Message = string.Empty, FileName = string.Empty, FileExtension = string.Empty, RemotePath = string.Empty, ppkLocalFilePath = string.Empty, GHAOutFolderPath = string.Empty;
        //    int portNumber;
        //    bool status = false;
        //    string msgStatus = string.Empty;
        //    string fileName = string.Empty;
        //    GenericFunction genericFunction = new GenericFunction();
        //    SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
        //    UserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
        //    Password = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
        //    FingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
        //    portNumber = Convert.ToInt32(genericFunction.ReadValueFromDb("msgService_IN_SITAPort"));
        //    try
        //    {
        //        SessionOptions sessionOptions;

        //        sessionOptions = new SessionOptions
        //        {
        //            Protocol = Protocol.Sftp,
        //            HostName = SFTPAddress,
        //            UserName = UserName,
        //            Password = Password,
        //            PortNumber = portNumber,
        //            SshHostKeyFingerprint = FingerPrint
        //        };

        //        using (Session session = new Session())
        //        {
        //            session.DisableVersionCheck = true;
        //            session.Open(sessionOptions);

        //            // Upload files
        //            TransferOptions transferOptions = new TransferOptions();
        //            transferOptions.TransferMode = TransferMode.Binary;
        //            transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

        //            TransferOperationResult transferResult;
        //            TransferOperationResult transferResultGHA;

        //            ///////////*****/////////////////
        //            bool isOn = false;
        //            SQLServer objsql = new SQLServer();
        //            do
        //            {
        //                //string ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty, ccadd = string.Empty, msgCommType = string.Empty;
        //                string ccadd = string.Empty, msgCommType = string.Empty;
        //                isOn = false;
        //                if (session.Opened)
        //                {
        //                    DataSet ds = null;
        //                    ds = objsql.SelectRecords("spMailtoSend");
        //                    if (ds != null)
        //                    {
        //                        if (ds.Tables.Count > 0)
        //                        {
        //                            if (ds.Tables[0].Rows.Count > 0)
        //                            {
        //                                isOn = true;
        //                                bool isMessageSent = false;
        //                                DataRow dr = ds.Tables[0].Rows[0];
        //                                string subject = dr[1].ToString();
        //                                msgCommType = "EMAIL";
        //                                DataRow drMsg = null;
        //                                FileName = dr["Subject"].ToString();
        //                                Message = dr[2].ToString();
        //                                string sentadd = dr[4].ToString().Trim(',');
        //                                if (dr[3].ToString().Length > 3)
        //                                    ccadd = dr[3].ToString().Trim(',');
        //                                bool ishtml = bool.Parse(dr["ishtml"].ToString() == "" ? "False" : dr["ishtml"].ToString());

        //                                if (ds.Tables[2].Rows.Count > 0)
        //                                {
        //                                    drMsg = ds.Tables[2].Rows[0];
        //                                    msgCommType = drMsg["MsgCommType"].ToString().ToUpper().Trim();
        //                                    FileExtension = drMsg["FileExtension"].ToString().ToUpper().Trim();
        //                                }

        //                                if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
        //                                    msgStatus = "Processed";
        //                                if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
        //                                    msgStatus = "Re-Processed";

        //                                #region SFTP Upload
        //                                if (msgCommType.ToUpper() == "SFTP" || msgCommType.ToUpper() == "ALL" || msgCommType.Equals("SFTP", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    if (drMsg != null && drMsg.ItemArray.Length > 0 && drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && (drMsg["FTPPassword"].ToString() != "" || drMsg["PPKFileName"].ToString() != ""))
        //                                    {
        //                                        RemotePath = drMsg["RemotePath"].ToString();
        //                                        //GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
        //                                    }
        //                                    if (FileName.ToUpper().Contains(".TXT"))
        //                                    {
        //                                        int fileIndex = FileName.IndexOf(".");
        //                                        if (fileIndex > 0)
        //                                            FileName = FileName.Substring(0, fileIndex);

        //                                    }
        //                                    else if (FileName.ToUpper().Contains(".XML"))
        //                                    {
        //                                        int fileIndex = FileName.IndexOf(".");
        //                                        if (fileIndex > 0)
        //                                        {
        //                                            FileName = FileName.Substring(0, fileIndex);
        //                                            FileExtension = "XML";
        //                                        }
        //                                    }
        //                                    else
        //                                        FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");


        //                                    fileName = FileName;
        //                                    fileName = Path.ChangeExtension(fileName, FileExtension);
        //                                    if (RemotePath.Length < 1 || RemotePath == "")
        //                                        RemotePath = "/";
        //                                    File.WriteAllText(fileName, Message);
        //                                    transferResult = session.PutFiles(fileName, RemotePath + "/", false, transferOptions);
        //                                    if (GHAOutFolderPath.Trim() != string.Empty)
        //                                    {
        //                                        transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
        //                                    }
        //                                    File.Delete(fileName);
        //                                    // Throw on any error
        //                                    transferResult.Check();
        //                                    isMessageSent = true;

        //                                }
        //                                #endregion

        //                                #region SITA Upload
        //                                if (msgCommType.ToUpper() == "SITA" || msgCommType.ToUpper() == "SITAFTP"
        //                                    || msgCommType.ToUpper() == "ALL"
        //                                    || msgCommType.Equals("SITA", StringComparison.OrdinalIgnoreCase)
        //                                    || msgCommType.Equals("SITAFTP", StringComparison.OrdinalIgnoreCase))
        //                                {
        //                                    FTP objFtp = new FTP();
        //                                    string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty;
        //                                    GHAOutFolderPath = string.Empty;

        //                                    StpFolerParth = genericFunction.ReadValueFromDb("msgService_OUT_FolderPath");

        //                                    if (FileName.ToUpper().Contains(".TXT"))
        //                                    {
        //                                        int fileIndex = FileName.IndexOf(".");
        //                                        if (fileIndex > 0)
        //                                            FileName = FileName.Substring(0, fileIndex);

        //                                    }
        //                                    else if (FileName.ToUpper().Contains(".XML"))
        //                                    {
        //                                        int fileIndex = FileName.IndexOf(".");
        //                                        if (fileIndex > 0)
        //                                        {
        //                                            FileName = FileName.Substring(0, fileIndex);
        //                                            FileExtension = "XML";
        //                                        }
        //                                    }
        //                                    else
        //                                        FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");


        //                                    fileName = FileName;
        //                                    fileName = Path.ChangeExtension(fileName, FileExtension);
        //                                    if (RemotePath.Length < 1 || RemotePath == "")
        //                                        RemotePath = "/";
        //                                    File.WriteAllText(fileName, Message);
        //                                    transferResult = session.PutFiles(fileName, RemotePath + "/", false, transferOptions);
        //                                    if (GHAOutFolderPath.Trim() != string.Empty)
        //                                    {
        //                                        transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
        //                                    }
        //                                    File.Delete(fileName);
        //                                    // Throw on any error
        //                                    transferResult.Check();
        //                                    isMessageSent = true;

        //                                }
        //                                #endregion

        //                                if (isMessageSent)
        //                                {
        //                                    string[] pname = { "num", "Status" };
        //                                    object[] pvalue = { int.Parse(dr[0].ToString()), msgStatus };
        //                                    SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
        //                                    if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
        //                                        clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                        isOn = false;
        //                }

        //            } while (isOn);
        //            ///////////*****/////////////////

        //            //fileName = FileName;
        //            //fileName = Path.ChangeExtension(fileName, FileExtension);
        //            ////fileName = Path.Combine(Path.GetTempPath(), fileName);
        //            //if (RemotePath.Length < 1 || RemotePath == "")
        //            //    RemotePath = "/";
        //            //File.WriteAllText(fileName, Message);
        //            //transferResult = session.PutFiles(fileName, RemotePath + "/", false, transferOptions);
        //            //if (GHAOutFolderPath.Trim() != string.Empty)
        //            //{
        //            //    transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
        //            //}
        //            //File.Delete(fileName);
        //            //// Throw on any error
        //            //transferResult.Check();

        //            session.Close();
        //        }
        //        status = true;
        //        clsLog.WriteLogAzure("Files Successfully Save in  Inbox for : " + FileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error on Files  uploading on SFTP for : " + ex.ToString());
        //        status = false;
        //    }
        //    return status;
        //}

        /// <summary>
        /// Method to Upload All SITA messages to the SITA server
        /// </summary>
        public async Task SITAUpload(DataTable dtSITAMessages, string SITAAddress, string SITAUserName, string SITAPassWord, string SITAFingerPrint, string SITAFolerParth, int portNumber, string GHAOutFolderPath, string ppkLocalFilePath)
        {
            string fileName = string.Empty, fileExtension = string.Empty, messageBody = string.Empty, status = string.Empty;
            try
            {
                WinSCP.SessionOptions sessionOptions;
                if (ppkLocalFilePath != string.Empty)
                {
                    sessionOptions = new WinSCP.SessionOptions
                    {
                        Protocol = WinSCP.Protocol.Sftp,
                        HostName = SITAAddress,
                        UserName = SITAUserName,
                        SshPrivateKeyPath = ppkLocalFilePath,
                        PortNumber = portNumber,
                        SshHostKeyFingerprint = SITAFingerPrint
                    };
                }
                else
                {
                    sessionOptions = new WinSCP.SessionOptions
                    {
                        Protocol = WinSCP.Protocol.Sftp,
                        HostName = SITAAddress,
                        UserName = SITAUserName,
                        Password = SITAPassWord,
                        PortNumber = portNumber,
                        SshHostKeyFingerprint = SITAFingerPrint
                    };
                }
                using (WinSCP.Session session = new WinSCP.Session())
                {
                    session.DisableVersionCheck = true;
                    session.Open(sessionOptions);

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                    TransferOperationResult transferResult;
                    TransferOperationResult transferResultGHA;

                    for (int i = 0; i < dtSITAMessages.Rows.Count; i++)
                    {
                        DataRow drSITAMessage = dtSITAMessages.Rows[i];
                        fileExtension = drSITAMessage["FileExtension"].ToString().ToUpper().Trim();
                        fileName = drSITAMessage["Subject"].ToString();
                        messageBody = drSITAMessage["Body"].ToString();
                        if (fileName.ToUpper().Contains(".TXT"))
                        {
                            int fileIndex = fileName.IndexOf(".");
                            if (fileIndex > 0)
                                fileName = fileName.Substring(0, fileIndex);

                        }
                        else if (fileName.ToUpper().Contains(".XML"))
                        {
                            int fileIndex = fileName.IndexOf(".");
                            if (fileIndex > 0)
                            {
                                fileName = fileName.Substring(0, fileIndex);
                                fileExtension = "xml";
                            }
                        }
                        else
                            fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");

                        fileName = Path.ChangeExtension(fileName, fileExtension);
                        if (SITAFolerParth.Length < 1 || SITAFolerParth == string.Empty)
                            SITAFolerParth = "/";
                        File.WriteAllText(fileName, messageBody);
                        transferResult = session.PutFiles(fileName, SITAFolerParth + "/", false, transferOptions);
                        if (GHAOutFolderPath.Trim() != string.Empty)
                        {
                            transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
                            transferResultGHA.Check();
                            if (!transferResultGHA.IsSuccess)
                            {
                                // clsLog.WriteLogAzure("SITA server connection aborted while uploading the file");
                                _logger.LogInformation("SITA server connection aborted while uploading the file");
                                break;
                            }
                        }

                        #region : Update message status :
                        if (drSITAMessage["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || drSITAMessage["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || drSITAMessage["STATUS"].ToString().Length < 1)
                            status = "Processed";
                        if (drSITAMessage["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                            status = "Re-Processed";

                        //string[] pname = { "num", "Status" };
                        //object[] pvalue = { int.Parse(drSITAMessage["SrNo"].ToString()), status };
                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };

                        SqlParameter[] sqlParameters = [
                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(drSITAMessage["SrNo"].ToString()) },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                        ];


                        //SQLServer sqlServer = new SQLServer();
                        //if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))

                        if (await _readWriteDao.ExecuteNonQueryAsync("spMailSent", sqlParameters))
                            // clsLog.WriteLogAzure("File uploaded successfully to SITA server: " + drSITAMessage["SrNo"].ToString());
                            _logger.LogInformation("File uploaded successfully to SITA server: {0}", drSITAMessage["SrNo"]);
                        #endregion Update message status

                        File.Delete(fileName);
                        transferResult.Check();
                        if (!transferResult.IsSuccess)
                        {
                            // clsLog.WriteLogAzure("SITA server connection aborted while uploading the file");
                            _logger.LogInformation("SITA server connection aborted while uploading the file");
                            break;
                        }
                    }

                    session.Close();
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task SFTPUpload(DataTable dtSFTPMessages, string SITAAddress, string SITAUserName, string SITAPassWord, string SITAFingerPrint, string SITAFolerParth, int portNumber, string GHAOutFolderPath, string ppkLocalFilePath)
        {
            string fileName = string.Empty, fileExtension = string.Empty, messageBody = string.Empty, status = string.Empty;
            try
            {
                WinSCP.SessionOptions sessionOptions;
                if (ppkLocalFilePath != string.Empty)
                {
                    sessionOptions = new WinSCP.SessionOptions
                    {
                        Protocol = WinSCP.Protocol.Sftp,
                        HostName = SITAAddress,
                        UserName = SITAUserName,
                        SshPrivateKeyPath = ppkLocalFilePath,
                        PortNumber = portNumber,
                        SshHostKeyFingerprint = SITAFingerPrint
                    };
                }
                else
                {
                    sessionOptions = new WinSCP.SessionOptions
                    {
                        Protocol = WinSCP.Protocol.Sftp,
                        HostName = SITAAddress,
                        UserName = SITAUserName,
                        Password = SITAPassWord,
                        PortNumber = portNumber,
                        SshHostKeyFingerprint = SITAFingerPrint
                    };
                }
                using (WinSCP.Session session = new WinSCP.Session())
                {
                    session.DisableVersionCheck = true;
                    session.Open(sessionOptions);

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                    TransferOperationResult transferResult;
                    TransferOperationResult transferResultGHA;

                    for (int i = 0; i < dtSFTPMessages.Rows.Count; i++)
                    {
                        DataRow drSITAMessage = dtSFTPMessages.Rows[i];
                        fileExtension = drSITAMessage["FileExtension"].ToString().ToUpper().Trim();
                        fileName = drSITAMessage["Subject"].ToString();
                        messageBody = drSITAMessage["Body"].ToString();
                        if (fileName.ToUpper().Contains(".TXT"))
                        {
                            int fileIndex = fileName.IndexOf(".");
                            if (fileIndex > 0)
                                fileName = fileName.Substring(0, fileIndex);

                        }
                        else if (fileName.ToUpper().Contains(".XML"))
                        {
                            int fileIndex = fileName.IndexOf(".");
                            if (fileIndex > 0)
                            {
                                fileName = fileName.Substring(0, fileIndex);
                                fileExtension = "xml";
                            }
                        }
                        else
                            fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");

                        fileName = Path.ChangeExtension(fileName, fileExtension);
                        if (SITAFolerParth.Length < 1 || SITAFolerParth == string.Empty)
                            SITAFolerParth = "/";
                        File.WriteAllText(fileName, messageBody);
                        transferResult = session.PutFiles(fileName, SITAFolerParth + "/", false, transferOptions);
                        if (GHAOutFolderPath.Trim() != string.Empty)
                        {
                            transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
                            transferResultGHA.Check();
                            if (!transferResultGHA.IsSuccess)
                            {
                                // clsLog.WriteLogAzure("SITA server connection aborted while uploading the file");
                                _logger.LogInformation("SITA server connection aborted while uploading the file");
                                break;
                            }
                        }

                        #region : Update message status :
                        if (drSITAMessage["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || drSITAMessage["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || drSITAMessage["STATUS"].ToString().Length < 1)
                            status = "Processed";
                        if (drSITAMessage["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                            status = "Re-Processed";

                        //string[] pname = { "num", "Status" };
                        //object[] pvalue = { int.Parse(drSITAMessage["SrNo"].ToString()), status };
                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };

                        SqlParameter[] sqlParameters = [
                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(drSITAMessage["SrNo"].ToString()) },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                        ];


                        ///SQLServer sqlServer = new SQLServer();
                        //if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                        if (await _readWriteDao.ExecuteNonQueryAsync("spMailSent", sqlParameters))
                            // clsLog.WriteLogAzure("File uploaded successfully to SITA server: " + drSITAMessage["SrNo"].ToString());
                            _logger.LogInformation("File uploaded successfully to SITA server: {0}", drSITAMessage["SrNo"]);
                        #endregion Update message status

                        File.Delete(fileName);
                        transferResult.Check();
                        if (!transferResult.IsSuccess)
                        {
                            // clsLog.WriteLogAzure("SITA server connection aborted while uploading the file");
                            _logger.LogWarning("SITA server connection aborted while uploading the file");
                            break;
                        }
                    }

                    session.Close();
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task SFTPUpload(DataTable dtMessagesToSend)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                string SFTPAddress = string.Empty, SFTPUserName = string.Empty, SFTPPassWord = string.Empty, SFTPFingerPrint = string.Empty, SFTPFolerParth = string.Empty, SFTPPortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;

                if (dtMessagesToSend.Rows.Count > 0)
                {
                    DataRow drMsg = dtMessagesToSend.Rows[0];

                    if (drMsg != null && drMsg.ItemArray.Length > 0 && drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && (drMsg["FTPPassword"].ToString() != "" || drMsg["PPKFileName"].ToString() != ""))
                    {
                        SFTPAddress = drMsg["FTPID"].ToString();
                        SFTPUserName = drMsg["FTPUserName"].ToString();
                        SFTPPassWord = drMsg["FTPPassword"].ToString();
                        ppkFileName = drMsg["PPKFileName"].ToString().Trim();
                        SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                        SFTPFolerParth = drMsg["RemotePath"].ToString();
                        SFTPPortNumber = drMsg["PortNumber"].ToString();
                        //GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");
                        GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");

                    }
                    else
                    {
                        //SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
                        //SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
                        //SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
                        //ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
                        //SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
                        //SFTPFolerParth = genericFunction.ReadValueFromDb("msgService_OUT_FolderPath");
                        //SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");
                        //GHAOutFolderPath = genericFunction.ReadValueFromDb("msgService_OUTGHAMCT_FolderPath");

                        SFTPAddress = ConfigCache.Get("msgService_IN_SITAFTP");
                        SFTPUserName = ConfigCache.Get("msgService_IN_SITAUser");
                        SFTPPassWord = ConfigCache.Get("msgService_IN_SITAPWD");
                        ppkFileName = ConfigCache.Get("PPKFileName");
                        SFTPFingerPrint = ConfigCache.Get("msgService_IN_SFTPFingerPrint");
                        SFTPFolerParth = ConfigCache.Get("msgService_OUT_FolderPath");
                        SFTPPortNumber = ConfigCache.Get("msgService_IN_SITAPort");
                        GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");
                    }

                    if (ppkFileName != string.Empty)
                    {
                        ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                    }
                    if (SFTPAddress != "" && SFTPUserName != "" && (SFTPPassWord != "" || ppkFileName != string.Empty) && SFTPFingerPrint != "" && SFTPFolerParth != "" && SFTPPortNumber.Trim() != string.Empty)
                    {
                        int portNumber = Convert.ToInt32(SFTPPortNumber.Trim());
                        WinSCP.SessionOptions sessionOptions;
                        if (ppkLocalFilePath != string.Empty)
                        {
                            sessionOptions = new WinSCP.SessionOptions
                            {
                                Protocol = WinSCP.Protocol.Sftp,
                                HostName = SFTPAddress,
                                UserName = SFTPUserName,
                                SshPrivateKeyPath = ppkLocalFilePath,
                                PortNumber = portNumber,
                                SshHostKeyFingerprint = SFTPFingerPrint
                            };
                        }
                        else
                        {
                            sessionOptions = new WinSCP.SessionOptions
                            {
                                Protocol = WinSCP.Protocol.Sftp,
                                HostName = SFTPAddress,
                                UserName = SFTPUserName,
                                Password = SFTPPassWord,
                                PortNumber = portNumber,
                                SshHostKeyFingerprint = SFTPFingerPrint
                            };
                        }
                        using (WinSCP.Session session = new WinSCP.Session())
                        {
                            session.DisableVersionCheck = true;
                            session.Open(sessionOptions);

                            TransferOptions transferOptions = new TransferOptions();
                            transferOptions.TransferMode = TransferMode.Binary;
                            transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                            TransferOperationResult transferResult;
                            TransferOperationResult transferResultGHA;

                            if (SFTPFolerParth.Length < 1 || SFTPFolerParth == "")
                                SFTPFolerParth = "/";

                            string fileName, fileExtension, messageBody, status = string.Empty;

                            //genericFunction.ReadValueFromDb("MSServiceType")
                            //genericFunction.ReadValueFromDb("UploadManifestToBlob")

                            var mSServiceType = ConfigCache.Get("MSServiceType");
                            var uploadManifestToBlob = ConfigCache.Get("UploadManifestToBlob");
                            for (int i = 0; i < dtMessagesToSend.Rows.Count; i++)
                            {
                                fileName = string.Empty;
                                fileExtension = string.Empty;
                                messageBody = string.Empty;
                                fileExtension = dtMessagesToSend.Rows[i]["FileExtension"].ToString().Trim();
                                fileName = dtMessagesToSend.Rows[i]["Subject"].ToString();
                                messageBody = dtMessagesToSend.Rows[i]["Body"].ToString();

                                if (fileName.ToUpper().Contains(".TXT"))
                                {
                                    int fileIndex = fileName.IndexOf(".");
                                    if (fileIndex > 0)
                                        fileName = fileName.Substring(0, fileIndex);

                                }
                                else if (fileName.ToUpper().Contains(".XML"))
                                {
                                    int fileIndex = fileName.IndexOf(".");
                                    if (fileIndex > 0)
                                    {
                                        fileName = fileName.Substring(0, fileIndex);
                                        fileExtension = "xml";
                                    }
                                }
                                else
                                    fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");

                                fileName = Path.ChangeExtension(fileName, fileExtension);

                                //if (genericFunction.ReadValueFromDb("MSServiceType") != string.Empty && genericFunction.ReadValueFromDb("MSServiceType").ToUpper() == "WINDOWSSERVICE")
                                if (mSServiceType != string.Empty && mSServiceType.ToUpper() == "WINDOWSSERVICE")
                                {
                                    fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                                    File.WriteAllText(fileName, messageBody);
                                }
                                else
                                {
                                    File.WriteAllText(fileName, messageBody);
                                }
                                transferResult = session.PutFiles(fileName, SFTPFolerParth + "/", false, transferOptions);
                                if (GHAOutFolderPath.Trim() != string.Empty)
                                {
                                    transferResultGHA = session.PutFiles(fileName, GHAOutFolderPath + "/", false, transferOptions);
                                }

                                if (uploadManifestToBlob != string.Empty && Convert.ToBoolean(uploadManifestToBlob))
                                {
                                    Stream messageStream = GenerateStreamFromString(messageBody);
                                    if (messageBody.ToUpper().Contains("MASTERMANIFESTREQUEST"))
                                    {
                                        _genericFunction.UploadToBlob(messageStream, System.IO.Path.GetFileName(fileName), ConfigCache.Get("M_AMF_REQ_Container"));
                                    }
                                    else if (messageBody.ToUpper().Contains("HOUSEMANIFESTREQUEST"))
                                    {
                                        _genericFunction.UploadToBlob(messageStream, System.IO.Path.GetFileName(fileName), ConfigCache.Get("H_AMF_REQ_Container"));
                                    }
                                }
                                // clsLog.WriteLogAzure("fileName1:- " + fileName + "RemotePath:- " + SFTPFolerParth);
                                _logger.LogInformation($"fileName1:- {fileName} RemotePath:- {SFTPFolerParth}");
                                File.Delete(fileName);
                                // Throw on any error
                                transferResult.Check();

                                if (dtMessagesToSend.Rows[i]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[i]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[i]["STATUS"].ToString().Length < 1)
                                    status = "Processed";
                                if (dtMessagesToSend.Rows[i]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                                    status = "Re-Processed";

                                //string[] pname = { "num", "Status" };
                                //object[] pvalue = { int.Parse(dtMessagesToSend.Rows[i]["Srno"].ToString()), status };
                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };

                                SqlParameter[] sqlParameters = [
                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dtMessagesToSend.Rows[i]["Srno"].ToString()) },
                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                ];

                                //SQLServer sqlServer = new SQLServer();
                                //if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))

                                if (await _readWriteDao.ExecuteNonQueryAsync("spMailSent", sqlParameters))
                                    // clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dtMessagesToSend.Rows[i]["Srno"].ToString());
                                    _logger.LogInformation("File uploaded on sftp successfully to:{0}", dtMessagesToSend.Rows[i]["Srno"]);
                            }
                            session.Close();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public bool SFTPUploadFile(string filePath, string SFTPFolderPath, string ZipFileName)
        {
            bool isSuccess = true;
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                string SFTPAddress = string.Empty, SFTPUserName = string.Empty, SFTPPassWord = string.Empty, SFTPFingerPrint = string.Empty, SFTPPortNumber = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;

                //SFTPAddress = genericFunction.ReadValueFromDb("msgService_IN_SITAFTP");
                //SFTPUserName = genericFunction.ReadValueFromDb("msgService_IN_SITAUser");
                //SFTPPassWord = genericFunction.ReadValueFromDb("msgService_IN_SITAPWD");
                //ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
                //SFTPFingerPrint = genericFunction.ReadValueFromDb("msgService_IN_SFTPFingerPrint");
                //SFTPPortNumber = genericFunction.ReadValueFromDb("msgService_IN_SITAPort");

                SFTPAddress = ConfigCache.Get("msgService_IN_SITAFTP");
                SFTPUserName = ConfigCache.Get("msgService_IN_SITAUser");
                SFTPPassWord = ConfigCache.Get("msgService_IN_SITAPWD");
                ppkFileName = ConfigCache.Get("PPKFileName");
                SFTPFingerPrint = ConfigCache.Get("msgService_IN_SFTPFingerPrint");
                SFTPPortNumber = ConfigCache.Get("msgService_IN_SITAPort");

                if (ppkFileName != string.Empty)
                {
                    ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                }
                if (SFTPAddress != "" && SFTPUserName != "" && (SFTPPassWord != "" || ppkFileName != string.Empty) && SFTPFingerPrint != "" && SFTPPortNumber.Trim() != string.Empty)
                {
                    int portNumber = Convert.ToInt32(SFTPPortNumber.Trim());
                    WinSCP.SessionOptions sessionOptions;
                    if (ppkLocalFilePath != string.Empty)
                    {
                        sessionOptions = new WinSCP.SessionOptions
                        {
                            Protocol = WinSCP.Protocol.Sftp,
                            HostName = SFTPAddress,
                            UserName = SFTPUserName,
                            SshPrivateKeyPath = ppkLocalFilePath,
                            PortNumber = portNumber,
                            SshHostKeyFingerprint = SFTPFingerPrint
                        };
                    }
                    else
                    {
                        sessionOptions = new WinSCP.SessionOptions
                        {
                            Protocol = WinSCP.Protocol.Sftp,
                            HostName = SFTPAddress,
                            UserName = SFTPUserName,
                            Password = SFTPPassWord,
                            PortNumber = portNumber,
                            SshHostKeyFingerprint = SFTPFingerPrint
                        };
                    }
                    // clsLog.WriteLogAzure("Connecting to DataDump Folder");
                    _logger.LogInformation("Connecting to DataDump Folder");
                    using (WinSCP.Session session = new WinSCP.Session())
                    {
                        session.DisableVersionCheck = true;
                        session.Open(sessionOptions);

                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;
                        transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                        TransferOperationResult transferResult;

                        if (SFTPFolderPath.Length < 1 || SFTPFolderPath == "")
                            SFTPFolderPath = "/";
                        transferResult = session.PutFiles(filePath, SFTPFolderPath + "/", false, transferOptions);
                        transferResult.Check();
                        if (!transferResult.IsSuccess)
                        {
                            // clsLog.WriteLogAzure("SITA server connection aborted while uploading the file");
                            _logger.LogWarning("SITA server connection aborted while uploading the file");
                        }
                        session.Close();
                        // clsLog.WriteLogAzure("File Uploaded Successfully: " + filePath);
                        _logger.LogInformation("File Uploaded Successfully: {0}", filePath);
                    }
                    File.Delete(filePath);
                    // clsLog.WriteLogAzure("File deleted from local folder: " + filePath);
                    _logger.LogInformation("File deleted from local folder: {0}", filePath);

                    #region : Data dump success alert :

                    //string dataDumpAlertEmailID = Convert.ToString(ConfigurationManager.AppSettings["DataDumpAlertEmailID"]);
                    string dataDumpAlertEmailID = _appConfig.Alert.DataDumpAlertEmailID;

                    string fileName = string.Empty;
                    fileName = ZipFileName;

                    if (dataDumpAlertEmailID != "")
                        _genericFunction.SaveMessageOutBox("Data dump alert", "Hi,\r\n\r\n" + fileName + "\r\nData dump file uploaded successfully.\r\n\r\nThanks."
                            , "", dataDumpAlertEmailID, "", 0);
                    #endregion Data dump success alert
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return isSuccess;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public async Task FTPUpload(DataTable dtMessagesToSend)
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                string ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty, messageBody = string.Empty, status = string.Empty;
                ftpUrl = dtMessagesToSend.Rows[0]["FTPID"].ToString();
                ftpUserName = dtMessagesToSend.Rows[0]["FTPUserName"].ToString();
                ftpPassword = dtMessagesToSend.Rows[0]["FTPPassword"].ToString();
                if (ftpUrl == string.Empty || ftpUserName == string.Empty || ftpPassword == string.Empty)
                {
                    //ftpUrl = genericFunction.ReadValueFromDb("FTPURLofFileUpload");
                    //ftpUserName = genericFunction.ReadValueFromDb("FTPUserofFileUpload");
                    //ftpPassword = genericFunction.ReadValueFromDb("FTPPasswordofFileUpload");

                    ftpUrl = ConfigCache.Get("FTPURLofFileUpload");
                    ftpUserName = ConfigCache.Get("FTPUserofFileUpload");
                    ftpPassword = ConfigCache.Get("FTPPasswordofFileUpload");
                }
                ///Upload the file on OutFolder
                FtpWebRequest myFtpWebRequest;
                FtpWebResponse myFtpWebResponse;
                StreamWriter myStreamWriter;
                FtpWebRequest renameRequest;
                FtpWebResponse renameResponse;

                for (int i = 0; i < dtMessagesToSend.Rows.Count; i++)
                {
                    string fileName = string.Empty, fileExtension = string.Empty;
                    fileName = dtMessagesToSend.Rows[i]["Subject"].ToString();
                    fileExtension = dtMessagesToSend.Rows[i]["FileExtension"].ToString();
                    messageBody = dtMessagesToSend.Rows[i]["body"].ToString();
                    status = dtMessagesToSend.Rows[i]["STATUS"].ToString();

                    #region : Set file name :
                    if (fileName.ToUpper().Contains(".TXT"))
                    {
                        int fileIndex = fileName.IndexOf(".");
                        if (fileIndex > 0)
                        {
                            fileName = fileName.Substring(0, fileIndex);
                            fileExtension = "txt";
                        }
                    }
                    else if (fileName.ToUpper().Contains(".XML"))
                    {
                        int fileIndex = fileName.IndexOf(".");
                        if (fileIndex > 0)
                        {
                            fileName = fileName.Substring(0, fileIndex);
                            fileExtension = "xml";
                        }
                    }
                    else
                    {
                        fileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
                    }
                    #endregion Set file name

                    string tempFileName = fileName + ".txt";
                    string OriginalfileName = fileName + "." + fileExtension.ToUpper();
                    string requestUri = ftpUrl + "/" + tempFileName;
                    myFtpWebRequest = (FtpWebRequest)WebRequest.Create(ftpUrl + "/" + tempFileName);
                    myFtpWebRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

                    myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    myFtpWebRequest.UseBinary = true;

                    myStreamWriter = new StreamWriter(myFtpWebRequest.GetRequestStream());
                    myStreamWriter.Write(messageBody);
                    myStreamWriter.Close();
                    myFtpWebResponse = (FtpWebResponse)myFtpWebRequest.GetResponse();
                    myFtpWebResponse.Close();

                    ///Rename file in the folder
                    renameRequest = (FtpWebRequest)WebRequest.Create(requestUri);
                    renameRequest.UseBinary = true;
                    renameRequest.UsePassive = true;
                    renameRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
                    renameRequest.KeepAlive = true;
                    renameRequest.Method = WebRequestMethods.Ftp.Rename;
                    renameRequest.RenameTo = OriginalfileName;
                    renameResponse = (FtpWebResponse)renameRequest.GetResponse();
                    renameResponse.Close();

                    if (status.Equals("Failed", StringComparison.OrdinalIgnoreCase) || status.Equals("Active", StringComparison.OrdinalIgnoreCase) || status.Length < 1)
                        status = "Processed";
                    if (status.Equals("Processed", StringComparison.OrdinalIgnoreCase))
                        status = "Re-Processed";

                    //string[] pname = { "num", "Status" };
                    //object[] pvalue = { int.Parse(dtMessagesToSend.Rows[i]["Srno"].ToString()), status };
                    //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };

                    SqlParameter[] sqlParameters = [
                        new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dtMessagesToSend.Rows[i]["Srno"].ToString()) },
                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                    ];

                    //SQLServer sqlServer = new SQLServer();
                    //if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                    if (await _readWriteDao.ExecuteNonQueryAsync("spMailSent", sqlParameters))
                    {
                        // clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dtMessagesToSend.Rows[i]["Srno"].ToString());
                        _logger.LogInformation("uploaded on ftp successfully to: {0}", dtMessagesToSend.Rows[i]["Srno"]);
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task SendMQMessage(DataTable dtMessagesToSend)
        {
            try
            {
                if (dtMessagesToSend.Rows.Count > 0)
                {
                    DataRow drMsg = dtMessagesToSend.Rows[0];
                    string MQManager = Convert.ToString(drMsg["MQManager"]);
                    string MQChannel = Convert.ToString(drMsg["MQChannel"]);
                    string MQHost = Convert.ToString(drMsg["MQHost"]);
                    string MQPort = Convert.ToString(drMsg["MQPort"]);
                    string MQUser = Convert.ToString(drMsg["MQUser"]);
                    string MQInqueue = Convert.ToString(drMsg["MQOutQueue"]);//"CG.BOOKINGS.CARGOSPOT.SMARTKARGO";
                    string MQOutqueue = "";
                    string ErrorMessage = string.Empty;

                    if (MQManager.Trim() != string.Empty && MQChannel.Trim() != string.Empty && MQHost.Trim() != string.Empty && MQPort.Trim() != string.Empty && MQInqueue.Trim() != string.Empty)
                    {
                        MQAdapter mqAdapter = new MQAdapter(MessagingType.ASync, MQManager, MQChannel, MQHost, MQPort, MQUser, MQInqueue, MQOutqueue, 0);
                        string messageBody = string.Empty;
                        for (int i = 0; i < dtMessagesToSend.Rows.Count; i++)
                        {
                            messageBody = dtMessagesToSend.Rows[i]["body"].ToString();

                            string result = mqAdapter.SendMessage(messageBody, out ErrorMessage);

                            if (ErrorMessage.Trim() == string.Empty)
                            {
                                string status = string.Empty;
                                if (dtMessagesToSend.Rows[i]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[i]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[i]["STATUS"].ToString().Length < 1)
                                    status = "Processed";
                                if (dtMessagesToSend.Rows[i]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                                    status = "Re-Processed";

                                //string[] pname = { "num", "Status" };
                                //object[] pvalue = { int.Parse(dtMessagesToSend.Rows[i]["Srno"].ToString()), status };
                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };

                                SqlParameter[] sqlParameters = [
                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dtMessagesToSend.Rows[i]["Srno"].ToString()) },
                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                ];

                                //SQLServer sqlServer = new SQLServer();
                                //if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))

                                if (await _readWriteDao.ExecuteNonQueryAsync("spMailSent", sqlParameters))
                                    // clsLog.WriteLogAzure("MQ Message Sent successfully to:" + dtMessagesToSend.Rows[i]["Srno"].ToString());
                                    _logger.LogInformation("MQ Message Sent successfully to: {0}", dtMessagesToSend.Rows[i]["Srno"]);
                            }
                            else
                            {
                                // clsLog.WriteLogAzure("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
                                _logger.LogWarning("Fail to send MQMessage : ErrorMessage : {0}", ErrorMessage);
                                // Console.WriteLine("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
                            }
                            //TO DO : below statement is to be removed
                            //Console.ReadLine();
                        }
                        mqAdapter.DisposeQueue();
                    }
                    else
                    {
                        // clsLog.WriteLogAzure("Info : In(SendMail() method):Insufficient MQ Message Configuration");
                        _logger.LogWarning("Info : In(SendMail() method):Insufficient MQ Message Configuration");
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public void ZIPandSFPTUpload(DataSet dsOmanBIDataDump)
        {
            string content = string.Empty, fileName = string.Empty;
            try
            {
                string dirContainer = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataDumpFiles");
                string dirZIPFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataDumpCompressedFiles");
                dirZIPFile = Path.Combine(dirZIPFile, dsOmanBIDataDump.Tables[0].Rows[0]["ZIPFileName"].ToString());

                if (!Directory.Exists(dirContainer))
                    Directory.CreateDirectory(dirContainer);

                System.IO.DirectoryInfo di = new DirectoryInfo(dirContainer);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                if (!Directory.Exists(dirZIPFile))
                    Directory.CreateDirectory(dirZIPFile);

                dirZIPFile = Path.Combine(dirZIPFile, dsOmanBIDataDump.Tables[0].Rows[0]["ZIPFileName"].ToString());

                for (int i = 1; i < dsOmanBIDataDump.Tables.Count; i++)
                {
                    DataTable dt = new DataTable();
                    dt = dsOmanBIDataDump.Tables[i];
                    fileName = dt.Rows[0]["FileName"].ToString();
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        content = dt.Rows[j][0].ToString();
                        if (!File.Exists(Path.Combine(dirContainer, fileName)))
                            File.WriteAllText(Path.Combine(dirContainer, fileName), content);
                        else
                            File.AppendAllText(Path.Combine(dirContainer, fileName), "\r\n" + content);
                    }
                }

                if (File.Exists(dirZIPFile))
                {
                    // clsLog.WriteLogAzure(dirZIPFile + " already exist");
                    _logger.LogInformation($"{dirZIPFile} already exist");
                    File.Delete(dirZIPFile);
                    // clsLog.WriteLogAzure(dirZIPFile + " file removed");
                    _logger.LogInformation($"{dirZIPFile} file removed");
                }

                ZipFile.CreateFromDirectory(dirContainer, dirZIPFile);
                // clsLog.WriteLogAzure(dirZIPFile);
                _logger.LogInformation(dirZIPFile);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                //if (!SFTPUploadFile(dirZIPFile, dataDumpFolderPath))
                //{
                //    bool isRetry = true;
                //    clsLog.WriteLogAzure("Start Retry DataDump");
                //    SQLServer OmanBIDataDump = new SQLServer();
                //    SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@isRetry", isRetry) };
                //    dsOmanBIDataDump = OmanBIDataDump.SelectRecords("BI.ExportOmanBIDataDump", sqlParameter);
                //    clsLog.WriteLogAzure("End Retry DataDump");
                //}
                //clsLog.WriteLogAzure("After SFTPUploadFile() call");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task UploadSISReceivableFileonSFTP(DataTable dtSFTPDetails)
        {
            try
            {
                string appDomainCurrentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (!Directory.Exists(appDomainCurrentDomainBaseDirectory + @"\SISFilesUpload"))
                    Directory.CreateDirectory(appDomainCurrentDomainBaseDirectory + @"\SISFilesUpload");

                string zipFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesUpload";

                //GenericFunction genericFunction = new GenericFunction();

                string msgCommType = string.Empty, SFTPAddress = string.Empty, SFTPUserName = string.Empty, SFTPPassWord = string.Empty, SFTPFingerPrint = string.Empty, SFTPFolerPath = string.Empty, SISOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty,
                    FileExtension = string.Empty;
                int SFTPPortNumber = 22;
                foreach (DataRow drSFTPRow in dtSFTPDetails.Rows)
                {
                    SFTPAddress = drSFTPRow["FTPID"].ToString();
                    SFTPUserName = drSFTPRow["FTPUsername"].ToString();
                    SFTPPassWord = drSFTPRow["FTPPassword"].ToString();
                    SFTPFingerPrint = drSFTPRow["FingerPrint"].ToString();
                    SFTPFolerPath = drSFTPRow["RemotePath"].ToString();
                    SFTPPortNumber = Convert.ToInt32(drSFTPRow["PortNumber"].ToString() == string.Empty ? SFTPPortNumber.ToString() : drSFTPRow["PortNumber"].ToString());
                    msgCommType = drSFTPRow["Messagetype"].ToString();
                    ppkFileName = drSFTPRow["PPKFileName"].ToString();
                    FileExtension = drSFTPRow["FileExtension"].ToString();
                    if (ppkFileName != string.Empty)
                    {
                        ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                    }

                    WinSCP.SessionOptions sessionOptions;
                    if (ppkLocalFilePath != string.Empty)
                    {
                        sessionOptions = new WinSCP.SessionOptions
                        {
                            Protocol = WinSCP.Protocol.Sftp,
                            HostName = SFTPAddress,
                            UserName = SFTPUserName,
                            SshPrivateKeyPath = ppkLocalFilePath,
                            PortNumber = SFTPPortNumber,
                            SshHostKeyFingerprint = SFTPFingerPrint
                        };
                    }
                    else if (msgCommType.Equals("FTPS"))
                    {
                        sessionOptions = new WinSCP.SessionOptions
                        {
                            Protocol = WinSCP.Protocol.Ftp,
                            HostName = SFTPAddress,
                            UserName = SFTPUserName,
                            Password = SFTPPassWord,
                            PortNumber = SFTPPortNumber,
                            FtpSecure = FtpSecure.Explicit

                        };
                    }
                    else
                    {
                        sessionOptions = new WinSCP.SessionOptions
                        {
                            Protocol = WinSCP.Protocol.Sftp,
                            HostName = SFTPAddress,
                            UserName = SFTPUserName,
                            Password = SFTPPassWord,
                            PortNumber = SFTPPortNumber,
                            SshHostKeyFingerprint = SFTPFingerPrint
                        };
                    }
                    using (WinSCP.Session session = new WinSCP.Session())
                    {
                        session.DisableVersionCheck = true;
                        session.Open(sessionOptions);

                        // Upload files
                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;

                        TransferOperationResult transferResult;
                        TransferOperationResult transferResultGHA;

                        //DbEntity.ReadDBData readDBData = new SIS.DAL.ReadDBData();

                        // SIS Receivable File
                        List<DbEntity.FileHeader> FileHeaderList = _readDBData.GetUnProcessedSISReceivableFiles();
                        string FileName = string.Empty;
                        foreach (var fileHeader in FileHeaderList)
                        {
                            string zipFileName = Path.GetFileName(fileHeader.FilePath);
                            string strAzulInvList = string.Empty;
                            FileInfo fileInfo = new FileInfo(zipFileName);
                            Stream dataStream = Download_FromBlob(fileHeader.FilePath);
                            if (fileInfo.Extension.ToUpper().Equals(".ZIP"))
                            {
                                string receivedZipFileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                                if (!receivedZipFileName.ToUpper().Contains("_VAL") && receivedZipFileName.Length == 31)
                                {
                                    DeleteContentsOfDirectory(zipFilePath);

                                    string path = Path.Combine(zipFilePath, zipFileName);
                                    try
                                    {
                                        using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                                        {
                                            dataStream.CopyTo(outputFileStream);
                                        }
                                        FileName = path;
                                        FileName = Path.ChangeExtension(FileName, FileExtension);
                                        if (SFTPFolerPath.Length < 1 || SFTPFolerPath == "")
                                            SFTPFolerPath = "/";


                                        transferResult = session.PutFiles(FileName, SFTPFolerPath + "/", false, transferOptions);
                                        if (SISOutFolderPath.Trim() != string.Empty)
                                        {
                                            transferResultGHA = session.PutFiles(FileName, SISOutFolderPath + "/", false, transferOptions);
                                        }

                                        // Throw on any error
                                        transferResult.Check();
                                        bool isUploaded = false;
                                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                                        {
                                            //Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                                            isUploaded = true;
                                        }
                                        if (isUploaded)
                                        {
                                            //DbEntity.UpdateDBData updateDBData = new SIS.DAL.UpdateDBData();
                                            await _updateDBData.UpdateStatusToFileUploaded(fileHeader.FileHeaderID, "SISAutomation");

                                            // clsLog.WriteLogAzure("FileName:- " + FileName + "RemotePath:- " + SFTPFolerPath);
                                            _logger.LogInformation("FileName:- {0} RemotePath:- {1}", FileName, SFTPFolerPath);
                                            File.Delete(FileName);
                                        }
                                        //session.Close();




                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(string.Format("Error in UploadSISReceivableFileonSFTP for File{0}, Exceiption : {1}", FileName, ex.Message));
                                        _logger.LogError(string.Format("Error in UploadSISReceivableFileonSFTP for File{0}, Exceiption : {1}", FileName, ex.Message));
                                    }
                                }
                            }
                        }
                        session.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error in UploadSISFileonSFTP : " + ex.Message);
                _logger.LogError(ex, "Error in UploadSISFileonSFTP : {0}", ex.Message);
            }

        }
    }
}
