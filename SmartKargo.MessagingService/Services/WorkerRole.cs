//using System;
//using System.Diagnostics;
//using System.Threading;
//using Microsoft.WindowsAzure.ServiceRuntime;
//using System.Configuration;
//using System.Data;
//using QID.DataAccess;
//using System.Net.Mail;
//using System.IO;
//using WinSCP;
//using System.Text;
//using QidWorkerRole.UploadMasters;

//namespace QidWorkerRole
//{
//    public class WorkerRole : RoleEntryPoint
//    {
//        #region :: Variable Declaration ::

//        bool isWorkerRoleRunning = true;
//        DateTime dtLastupdatedTime = DateTime.Now;

//        #region : Revera Global Variables :
//        public static string ConStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
//        string ReveraLastProcessedDateTime = string.Empty;
//        SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();
//        static DataSet dsConfig = null;
//        BALReveraInterface objBAL = new BALReveraInterface();

//        GenericFunction genericFunction = new GenericFunction();
//        DateTime DailyExecutionDateTime;
//        int DayDiff = 0;

//        string FTPCommonPath = string.Empty;

//        string SFTPFingerPrint = string.Empty;
//        string FTPUserName = string.Empty;
//        string FTPPassword = string.Empty;
//        string FTPHost = string.Empty;

//        string FileAWB_Path = string.Empty;
//        string FileFlown_Path = string.Empty;
//        string FileManifest_Path = string.Empty;

//        string Mail_From = string.Empty;
//        string Mail_To = string.Empty;
//        string Mail_Subject = string.Empty;
//        string Mail_Body = string.Empty;

//        string AWBFileName = string.Empty;
//        string FlownFileName = string.Empty;
//        string FlightFileName = string.Empty;
//        string AWBFileStatus = string.Empty;
//        string AWBFileStatusColor = string.Empty;
//        string FlownFileStatus = string.Empty;
//        string FlownFileStatusColor = string.Empty;
//        string FilghtFileStatus = string.Empty;
//        string FilghtFileStatusColor = string.Empty;
//        string AWBFTPStatus = string.Empty;
//        string FlownFTPStatus = string.Empty;
//        string FlightFTPStatus = string.Empty;
//        string UploadStatus = string.Empty;
//        string UploadStatusColor = string.Empty;
//        #endregion : Revera Global Variables
//        #endregion : Variable Declaration

//        #region :: Public Methods ::

//        public override void Run()
//        {

//            try
//            {
//                AzureDrive objAzureDrive = new AzureDrive();
//                Cls_BL objbl = new Cls_BL();
//                FTP fTP = new FTP();
//                bool isReadMessageQueue = false;
//                if (!bool.TryParse(genericFunction.GetConfigurationValues("ReadMessageQueue"), out isReadMessageQueue))
//                {
//                    isReadMessageQueue = false;
//                }
//                DataSet dsUploadMasters = new DataSet();

//                while (isWorkerRoleRunning)
//                {
//                    objbl.ReadMailFromMailBox();
//                    dsUploadMasters = objbl.DBCalls();
//                    objbl.SendMail();
//                    fTP.SITASFTPDownloadFile();
//                    objbl.FTPListener();
//                    objAzureDrive.ReadFromSITADrive();



//                    if (isReadMessageQueue)
//                    {
//                        objbl.ReceiveMQMessage();
//                    }
//                    if (dsUploadMasters != null && dsUploadMasters.Tables.Count > 0 && dsUploadMasters.Tables[0].Rows.Count > 0)
//                    {
//                        UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
//                        uploadMasterCommon.UploadMasters(dsUploadMasters);
//                    }

//                    #region SAP Interface Process
//                    bool IsEnabled = genericFunction.ReadValueFromDb("EnableSAPInterface") == "" ? false : Convert.ToBoolean(genericFunction.ReadValueFromDb("EnableSAPInterface"));

//                    if (IsEnabled)
//                    {
//                        SAPProcess();
//                    }
//                    #endregion

//                    clsLog.WriteLogAzure("Worker role sleep " + DateTime.Now);
//                    Thread.Sleep(1000);
//                }
//            }
//            catch (Exception objEx)
//            {
//                //SCMExceptionHandling.logexception(ref objEx);
//                clsLog.WriteLogAzure(objEx);
//            }
//            Trace.WriteLine("Starting processing of messages");
//        }

//        public override bool OnStart()
//        {
//            try
//            {
//                isWorkerRoleRunning = true;
//                clsLog.WriteLogAzure("In OnStart");
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//            return base.OnStart();
//        }

//        public override void OnStop()
//        {
//            try
//            {
//                isWorkerRoleRunning = false;
//                clsLog.WriteLogAzure("Service Stoped @ " + DateTime.Now.ToString());

//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//            base.OnStop();
//        }

//        public void RunMessagingProcessForTest()
//        {
//            try
//            {
//                AzureDrive objAzureDrive = new AzureDrive();
//                Cls_BL objbl = new Cls_BL();
//                FTP fTP = new FTP();
//                bool isReadMessageQueue = false;

             
//                if (!bool.TryParse(genericFunction.GetConfigurationValues("ReadMessageQueue"), out isReadMessageQueue))
//                {
//                    isReadMessageQueue = false;
//                }
//                DataSet dsUploadMasters = new DataSet();

//                while (isWorkerRoleRunning)
//                {
//                    //objbl.ReadMailFromMailBox();
//                    dsUploadMasters = objbl.DBCalls();
//                    //objbl.SendMail();
//                    //fTP.SITASFTPDownloadFile();
//                    //objbl.FTPListener();
//                    //objAzureDrive.ReadFromSITADrive();

//                    //if (isReadMessageQueue)
//                    //{
//                    //    objbl.ReceiveMQMessage();
//                    //}
//                    //SQLServer sqlServerUplodedFile = new SQLServer();
//                    //dsUploadMasters = sqlServerUplodedFile.SelectRecords("uspGetUplodedFileTest");
//                    //if (dsUploadMasters != null && dsUploadMasters.Tables.Count > 0 && dsUploadMasters.Tables[0].Rows.Count > 0)
//                    //{
//                    //    UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
//                    //    uploadMasterCommon.UploadMasters(dsUploadMasters);
//                    //}

//                    //#region SAP Interface Process
//                    //bool IsEnabled = genericFunction.ReadValueFromDb("EnableSAPInterface") == "" ? false : Convert.ToBoolean(genericFunction.ReadValueFromDb("EnableSAPInterface"));

//                    //if (IsEnabled)
//                    //{
//                    //    SAPProcess();
//                    //}
//                    //#endregion

//                    //clsLog.WriteLogAzure("Worker role sleep " + DateTime.Now);
//                    //Thread.Sleep(1000);
//                }
//            }
//            catch (Exception objEx)
//            {
//                //SCMExceptionHandling.logexception(ref objEx);
//                clsLog.WriteLogAzure(objEx);
//            }
//        }
//        #endregion : Public Methods

//        #region :: Private Methods ::
//        /// <summary>
//        /// Used to do the default initialization of Revera Download Process
//        /// </summary>
//        private void ReveraInitialization()
//        {
//            // To get the Date and generate the "From Date" and "To Date"
//            DateTime CurrentDate = DateTime.Now.AddDays(DayDiff);
//            DateTime FrmDate = Convert.ToDateTime(CurrentDate.ToString("MM/dd/yyyy"));
//            DateTime ToDate = Convert.ToDateTime(CurrentDate.ToString("MM/dd/yyyy"));

//            // Function to Create folder in Bin( If already exist clear folder)
//            createOrCleanFolder();

//            // Function to Generate Files for the Revera
//            getAWBInformationFile(FrmDate, ToDate);
//            getAWBFlownInformationFile(FrmDate, ToDate);
//            getFlightManifestInformationFile(FrmDate, ToDate);

//            // Function to rename Revera file and copy to Revera Folder
//            setFolderForFiles(CurrentDate);

//            // Function to Upload Files to FTP Server
//            uploadFilesToFTPServer();
//            // Function to send mail with Revera file attachment.
//            sendReferenceMail(CurrentDate.ToString(), FileAWB_Path, FileFlown_Path, FileManifest_Path);

//            // Function to get the Last Run time of the .exe file
//            SetLastRun();
//        }

//        /// <summary>
//        /// Process to download the getAWBInformation
//        /// </summary>
//        private void getAWBInformationFile(DateTime FrmDate, DateTime ToDate)
//        {
//            string filename = "AWBInfo";
//            string tmpTransactionID = string.Empty;
//            int tmpTotalRecord = 0;
//            float tmpCheckSum = 0.0F;

//            DataSet dsAWB = new DataSet("FrmERPInterface_dsAWB");

//            try
//            {

//                dsAWB = objBAL.GetAWBInfo(FrmDate, ToDate);

//                if (dsAWB != null)
//                {
//                    tmpTransactionID = dsAWB.Tables[1].Rows[0][0].ToString();
//                    tmpTotalRecord = Convert.ToInt32(dsAWB.Tables[1].Rows[0][1]);
//                    tmpCheckSum = float.Parse(dsAWB.Tables[1].Rows[0][2].ToString());
//                    AWBFileStatus = "Data Available";
//                    AWBFileStatusColor = "Green";
//                    CreateTxtOrExcelFileForRevera(dsAWB, filename, tmpTransactionID, tmpTotalRecord, tmpCheckSum, FrmDate);
//                }
//                else
//                {
//                    tmpTransactionID = "0";
//                    tmpTotalRecord = 0;
//                    tmpCheckSum = 0.0F;
//                    AWBFileStatus = "Data Not Available";
//                    AWBFileStatusColor = "Red";
//                    CreateTxtOrExcelFileForRevera(dsAWB, filename, tmpTransactionID, tmpTotalRecord, tmpCheckSum, FrmDate);
//                }

//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//            finally
//            {
//                if (dsAWB != null)
//                    dsAWB.Dispose();
//            }
//        }

//        /// <summary>
//        /// Process to download the AWBFlownInformation
//        /// </summary>
//        private void getAWBFlownInformationFile(DateTime FrmDate, DateTime ToDate)
//        {
//            string filename = "FlownInfo";
//            string tmpTransactionID = string.Empty;
//            int tmpTotalRecord = 0;
//            float tmpCheckSum = 0.0F;
//            DataSet dsFlown = new DataSet("FrmERPInterface_dsFlown");

//            try
//            {
//                dsFlown = objBAL.GetFlownInfo(FrmDate, ToDate);
//                if (dsFlown != null)
//                {
//                    tmpTransactionID = dsFlown.Tables[1].Rows[0][0].ToString();
//                    tmpTotalRecord = Convert.ToInt32(dsFlown.Tables[1].Rows[0][1]);
//                    tmpCheckSum = float.Parse(dsFlown.Tables[1].Rows[0][2].ToString());
//                    FlownFileStatus = "Data Available";
//                    FlownFileStatusColor = "Green";
//                    CreateTxtOrExcelFileForRevera(dsFlown, filename, tmpTransactionID, tmpTotalRecord, tmpCheckSum, FrmDate);
//                }
//                else
//                {
//                    tmpTransactionID = "0";
//                    tmpTotalRecord = 0;
//                    tmpCheckSum = 0.0F;
//                    FlownFileStatus = "Data Not Available";
//                    FlownFileStatusColor = "Red";
//                    CreateTxtOrExcelFileForRevera(dsFlown, filename, tmpTransactionID, tmpTotalRecord, tmpCheckSum, FrmDate);

//                }
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//            finally
//            {
//                if (dsFlown != null)
//                    dsFlown.Dispose();
//            }
//        }

//        /// <summary>
//        /// Process to download the FltManifest
//        /// </summary>
//        private void getFlightManifestInformationFile(DateTime FrmDate, DateTime ToDate)
//        {
//            string tmpTransactionID = "0";
//            int tmpTotalRecord = 0;
//            float tmpCheckSum = 0.0F;
//            DataSet dsFlt = new DataSet("FrmERPInterface_dsFlt");

//            try
//            {
//                dsFlt = objBAL.GetFltManifestDetails(FrmDate, ToDate);
//                if (dsFlt != null)
//                {
//                    tmpTransactionID = dsFlt.Tables[1].Rows[0][0].ToString();
//                    tmpTotalRecord = Convert.ToInt32(dsFlt.Tables[1].Rows[0][1]);
//                    tmpCheckSum = float.Parse(dsFlt.Tables[1].Rows[0][2].ToString());
//                    FilghtFileStatus = "Data Available";
//                    FilghtFileStatusColor = "Green";

//                }
//                else
//                {
//                    tmpTransactionID = "0";
//                    tmpTotalRecord = 0;
//                    tmpCheckSum = 0.0F;
//                    FilghtFileStatus = "Data Not Available";
//                    FilghtFileStatusColor = "Red";
//                }
//                StringBuilder sb = new StringBuilder();
//                if (dsFlt != null && dsFlt.Tables.Count > 0 && dsFlt.Tables[0].Rows.Count > 0)
//                {
//                    sb.Append(dsFlt.Tables[0].Rows[0][0].ToString());
//                }
//                DataTable dsFile = new DataTable("FrmERPInterface_dsFile1");
//                string fileType = "FltManifest";
//                dsFile = objBAL.GetReveraFileName(FrmDate, fileType, tmpTransactionID, tmpTotalRecord, tmpCheckSum);

//                fileType = dsFile.Rows[0][0].ToString();

//                StreamWriter SW;

//                SW = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Temp\" + "tmp_" + fileType + ".txt", true);
//                SW.Write(sb.ToString());
//                SW.Close();
//                //}


//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//            finally
//            {
//                if (dsFlt != null)
//                    dsFlt.Dispose();
//            }
//        }

//        /// <summary>
//        /// Function CreateTxtOrExcelFileForRevera
//        /// </summary>
//        void CreateTxtOrExcelFileForRevera(DataSet dsRevera, string fileType, string TransactionID, int TotalRecords, float CheckSum, DateTime GenDate, string strExtension = ".txt")
//        {
//            try
//            {
//                StringBuilder SB = new StringBuilder();
//                // Check File name
//                DataTable dsFile = new DataTable("FrmERPInterface_dsFile");

//                dsFile = objBAL.GetReveraFileName(GenDate, fileType, TransactionID, TotalRecords, CheckSum);

//                fileType = dsFile.Rows[0][0].ToString();

//                if (dsRevera != null)
//                {
//                    /**********************************************************/
//                    for (int intRow = 0; intRow < dsRevera.Tables[0].Rows.Count; intRow++)
//                    {
//                        for (int intCol = 0; intCol < dsRevera.Tables[0].Columns.Count; intCol++)
//                        {
//                            if (intCol == 0)
//                                SB.Append(dsRevera.Tables[0].Rows[intRow][intCol].ToString());
//                            else
//                            {
//                                if (strExtension != ".xls")
//                                    SB.Append("|" + dsRevera.Tables[0].Rows[intRow][intCol].ToString());
//                                else
//                                    SB.Append("\t" + dsRevera.Tables[0].Rows[intRow][intCol].ToString());
//                            }

//                        }
//                        SB.Append("\n");
//                    }
//                }


//                StreamWriter SW;
//                SW = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Temp\" + "tmp_" + fileType + ".txt", true);

//                SW.Write(SB.ToString());
//                SW.Close();

//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//        }

//        /// <summary>
//        /// To set folder as per the downloaded file
//        /// </summary>
//        private void setFolderForFiles(DateTime CurDate)
//        {
//            int GenDayDiff = 0;
//            GenDayDiff = Convert.ToInt32(dsConfig.Tables[0].Select("Parameter='Revera_GenerationDate'")[0]["Value"]);
//            CurDate = DateTime.Now.AddDays(GenDayDiff);
//            if (Directory.Exists("Temp"))
//            {
//                string[] FilesList = Directory.GetFiles("Temp");
//                foreach (string FlInf in FilesList)
//                {
//                    string FileName = string.Empty;
//                    FileInfo CurFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + FlInf);
//                    if (FlInf.Contains("flight_manifest"))
//                    {//flight_manifest_15052001
//                        FileName = "flight_manifest_" + CurDate.ToString("yy") + CurDate.ToString("MM") + CurDate.ToString("dd") + "01.txt";
//                        CurFile.MoveTo("ReveraFiles/" + FileName);
//                    }
//                    if (FlInf.Contains("HAAWB_AUDIT"))
//                    {//HAAWB_AUDIT20-May-15_001
//                        FileName = "HAAWB_AUDIT" + CurDate.ToString("dd") + "-" + CurDate.ToString("MMM") + "-" + CurDate.ToString("yy") + "_001.txt";
//                        CurFile.MoveTo("ReveraFiles/" + FileName);
//                    }
//                    if (FlInf.Contains("HAFLT_AUDIT"))
//                    {//HAFLT_AUDIT20-May-15_001
//                        FileName = "HAFLT_AUDIT" + CurDate.ToString("dd") + "-" + CurDate.ToString("MMM") + "-" + CurDate.ToString("yy") + "_001.txt";
//                        CurFile.MoveTo("ReveraFiles/" + FileName);
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Create new folder for Files or Clear the existing folder
//        /// </summary>
//        private void createOrCleanFolder()
//        {
//            // For Cleaning Revera Directory
//            if (Directory.Exists("ReveraFiles"))
//            {
//                string[] FilesList = Directory.GetFiles("ReveraFiles");
//                foreach (string FlInf in FilesList)
//                {
//                    File.Delete(FlInf);
//                }
//            }
//            else
//            {
//                Directory.CreateDirectory("ReveraFiles");
//            }

//            // For Cleaning Temp Directory
//            if (Directory.Exists("Temp"))
//            {
//                string[] FilesList = Directory.GetFiles("Temp");
//                foreach (string FlInf in FilesList)
//                {
//                    File.Delete(FlInf);
//                }
//            }
//            else
//            {
//                Directory.CreateDirectory("Temp");
//            }
//        }

//        /// <summary>
//        /// Process the Revera files for Uploading to FTP server
//        /// </summary>
//        private void uploadFilesToFTPServer()
//        {

//            // Set Common Path from App.config File
//            FTPCommonPath = dsConfig.Tables[0].Select("Parameter='Revera_CommonPath'")[0]["Value"].ToString();

//            if (Directory.Exists("ReveraFiles"))
//            {
//                string[] FilesList = Directory.GetFiles("ReveraFiles");
//                foreach (string fname in FilesList)
//                {
//                    string FileName = string.Empty;
//                    string UploadLocation = string.Empty;
//                    bool FTPStatus = true;
//                    FileName = Directory.GetCurrentDirectory() + @"\ReveraFiles\" + fname.Substring(fname.LastIndexOf("\\") + 1);
//                    if (fname.Contains("flight_manifest"))
//                    {
//                        UploadLocation = FTPCommonPath + @"to_cra_manifest/";
//                        FileManifest_Path = FileName;
//                        FlightFileName = FileManifest_Path.Substring(FileManifest_Path.LastIndexOf("\\") + 1);
//                        FTPStatus = SaveSFTPUpload(FileName, UploadLocation);
//                        if (FTPStatus) FlightFTPStatus = "Done"; else FlightFTPStatus = "Error";
//                    }
//                    if (fname.Contains("HAAWB_AUDIT"))
//                    {
//                        UploadLocation = FTPCommonPath + @"to_cra_awb/";
//                        FileAWB_Path = FileName;
//                        AWBFileName = FileManifest_Path.Substring(FileManifest_Path.LastIndexOf("\\") + 1);
//                        FTPStatus = SaveSFTPUpload(FileName, UploadLocation);
//                        if (FTPStatus) AWBFTPStatus = "Done"; else AWBFTPStatus = "Error";
//                    }
//                    if (fname.Contains("HAFLT_AUDIT"))
//                    {
//                        UploadLocation = FTPCommonPath + @"to_cra_flown/";
//                        FileFlown_Path = FileName;
//                        FlownFileName = FileFlown_Path.Substring(FileFlown_Path.LastIndexOf("\\") + 1);
//                        FTPStatus = SaveSFTPUpload(FileName, UploadLocation);
//                        if (FTPStatus) FlownFTPStatus = "Done"; else FlownFTPStatus = "Error";
//                    }
//                }
//                if (FlightFTPStatus == "Done" && AWBFTPStatus == "Done" && FlownFTPStatus == "Done")
//                {
//                    UploadStatus = "UPLOADED SUCCESSFULLY";
//                    UploadStatusColor = "#009900";
//                }
//                else
//                {
//                    UploadStatus = "UPLOADING FAILED";
//                    UploadStatusColor = "Red";
//                }
//            }
//        }

//        /// <summary>
//        /// Function to Upload the files to Secured FTP server
//        /// </summary>
//        public bool SaveSFTPUpload(string LocFilePath, string UploadDirPath)
//        {
//            SFTPFingerPrint = dsConfig.Tables[0].Select("Parameter='Revera_SecureFTP_FingerPrint'")[0]["Value"].ToString();
//            FTPHost = dsConfig.Tables[0].Select("Parameter='Revera_SecureFTP_ServerHost'")[0]["Value"].ToString();
//            FTPUserName = dsConfig.Tables[0].Select("Parameter='Revera_SecureFTP_UserName'")[0]["Value"].ToString();
//            FTPPassword = dsConfig.Tables[0].Select("Parameter='Revera_SecureFTP_Password'")[0]["Value"].ToString();
//            string ppkFileName = genericFunction.ReadValueFromDb("PPKFileName");
//            string ppkLocalFilePath = string.Empty;

//            try
//            {
//                if (ppkFileName != string.Empty)
//                {
//                    ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
//                }
//                // Setup session options
//                SessionOptions sessionOptions;
//                if (ppkLocalFilePath != string.Empty)
//                {
//                    sessionOptions = new SessionOptions
//                    {
//                        Protocol = Protocol.Sftp,
//                        HostName = FTPHost,
//                        UserName = FTPUserName,
//                        SshPrivateKeyPath = ppkLocalFilePath,
//                        SshHostKeyFingerprint = SFTPFingerPrint
//                    };
//                }
//                else
//                {
//                    sessionOptions = new SessionOptions
//                    {
//                        Protocol = Protocol.Sftp,
//                        HostName = FTPHost,
//                        UserName = FTPUserName,
//                        Password = FTPPassword,
//                        SshHostKeyFingerprint = SFTPFingerPrint
//                    };
//                }

//                using (Session session = new Session())
//                {
//                    // Connect
//                    session.Open(sessionOptions);

//                    // Upload files
//                    TransferOptions transferOptions = new TransferOptions();
//                    transferOptions.TransferMode = TransferMode.Binary;
//                    transferOptions.FilePermissions = null; //This is default
//                    transferOptions.PreserveTimestamp = false;

//                    TransferOperationResult transferResult;

//                    transferResult = session.PutFiles(LocFilePath, UploadDirPath, false, transferOptions);

//                    // Throw on any error
//                    transferResult.Check();

//                }
//                return true;
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//                return false;
//            }

//        }

//        /// <summary>
//        /// Process to send mail for Reference with attachment
//        /// </summary>
//        private void sendReferenceMail(string FileDate, String Attachment_1, string Attachment_2, string Attachment_3)
//        {
//            try
//            {
//                // Get the UserName and Password
//                string UserID = dsConfig.Tables[0].Select("Parameter='Revera_Mail_Username'")[0]["Value"].ToString();
//                string Password = dsConfig.Tables[0].Select("Parameter='Revera_Mail_password'")[0]["Value"].ToString();
//                Mail_From = dsConfig.Tables[0].Select("Parameter='Revera_Mail_From'")[0]["Value"].ToString();
//                Mail_To = dsConfig.Tables[0].Select("Parameter='Revera_Mail_To'")[0]["Value"].ToString();

//                // Declaration
//                MailMessage mail = new MailMessage();
//                SmtpClient SmtpServer = new SmtpClient("smtpout.secureserver.net", 80);
//                Mail_Body = "<html><head></head><body><b style=\"color:#214CC1;font-size:19px;font-family:Georgia;\">&nbsp;&nbsp;&nbsp;Hello Vishal,</b><br><br><b style=\"color:#214CC1;font-size:19px;font-family:Georgia;\">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Please find the Revera files in the attachment.</b><br><br><table style=\"border:1px solid #000000;margin:0px;padding:0px;font-family:Georgia;margin-left:15px\" width=\"600px\"><tbody><tr><td align=\"center\" colspan=\"2\"><img src=cid:CompanyLogo><div class=\"a6S\" dir=\"ltr\" style=\"opacity: 0.01; left: 252px; top: 130px;\"><div id=\":zx\" class=\"T-I J-J5-Ji aQv T-I-ax7 L3 a5q\" role=\"button\" tabindex=\"0\" aria-label=\"Download attachment \" data-tooltip-class=\"a1V\" data-tooltip=\"Download\"><div class=\"aSK J-J5-Ji aYr\"></div></div></div></td></tr><tr><td style=\"height:5px;background-color:#0096ff;font-size:23px;color:White\" colspan=\"3\" align=\"center\"><b>\"REVERA FILE STATUS\"</b></td></tr><tr><td style=\"height:5px;font-size:23px;color:White;font-family:Times New Roman\" colspan=\"2\" align=\"center\" ><b style=\"color:" + UploadStatusColor + "\">" + UploadStatus + "</b></td></tr><tr style=\"background-color:#aad4ff\"><td style=\"height:30px\"><b>&nbsp;HAAWB_AUDIT File Name :</b></td><td>&nbsp;" + AWBFileName + "</td></tr><tr style=\"background-color:#e1eef4\"><td style=\"height:30px\"><b>&nbsp;HAAWB_AUDIT Status :</b></td><td style=\"color:" + AWBFileStatusColor + ";\">&nbsp;" + AWBFileStatus + "</td></tr><tr style=\"background-color:#aad4ff\"><td style=\"height:30px\"><b>&nbsp;HAFLT_AUDIT File Name:</b></td><td>&nbsp;" + FlownFileName + "</td></tr><tr style=\"background-color:#e1eef4\"><td style=\"height:30px\"><b>&nbsp;HAFLT_AUDIT Status :</b></td><td style=\"color:" + FlownFileStatusColor + ";\">&nbsp;" + FlownFileStatus + "</td></tr><tr style=\"background-color:#aad4ff\"><td style=\"height:30px\"><b>&nbsp;FLIGHT_MANIFEST File Name:</b></td><td>&nbsp;" + FlightFileName + "</td></tr><tr style=\"background-color:#e1eef4\"><td style=\"height:30px\"><b>&nbsp;FLIGHT_MANIFEST Status :</b></td><td style=\"color:" + FilghtFileStatusColor + ";\">&nbsp;" + FilghtFileStatus + "</td></tr>	</tbody></table><br><br><b>&nbsp;&nbsp;&nbsp; Thanks,</b><br><b>&nbsp;&nbsp;&nbsp;&nbsp;SMARTKARGO</b><br><br><b>&nbsp;&nbsp;&nbsp;&nbsp;NOTE: This is auto generated email. Please do not reply.</b></body></html>	";
//                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(Mail_Body, null, "text/html");
//                LinkedResource headerImageLink = new LinkedResource(Directory.GetCurrentDirectory() + @"\..\..\Images\SmartKargo_Logo.png");
//                headerImageLink.ContentId = "CompanyLogo";
//                htmlView.LinkedResources.Add(headerImageLink);
//                mail.AlternateViews.Add(htmlView);

//                Mail_Subject = "Revera Files Dated:" + DateTime.Now.ToString("MM/dd/yyyy");

//                // Configuration
//                mail.From = new MailAddress(Mail_From, "Revera Updates");
//                mail.To.Add(Mail_To);
//                mail.Subject = Mail_Subject;
//                mail.IsBodyHtml = true;
//                mail.Body = htmlView.ToString();

//                //Attach file
//                mail.Attachments.Add(new Attachment(Attachment_1.ToString()));
//                mail.Attachments.Add(new Attachment(Attachment_2.ToString()));
//                mail.Attachments.Add(new Attachment(Attachment_3.ToString()));
//                //SmtpServer.UseDefaultCredentials = true;
//                SmtpServer.Credentials = new System.Net.NetworkCredential(UserID, Password);
//                //SmtpServer.EnableSsl = true;
//                SmtpServer.Send(mail);
//                // Dispose the SMTP Server
//                SmtpServer.Dispose();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }

//        /// <summary>
//        ///  Process to get the Configuration from the Configuration Table
//        /// </summary>
//        private bool GetMasterDetails()
//        {
//            bool Status = false;

//            try
//            {
//                dsConfig = null;
//                SQLServer objSQL = new SQLServer();
//                dsConfig = objSQL.SelectRecords("sp_GetMasterConfiguration", "SearchType", "GetConfig", SqlDbType.VarChar);

//                if (dsConfig != null && dsConfig.Tables.Count > 0 && dsConfig.Tables[0].Rows.Count > 0)
//                    Status = true;

//                return Status;
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//                return Status = false;
//            }
//        }

//        /// <summary>
//        /// Process to Update Last Run Date in tblConfiguration
//        /// </summary>
//        private void SetLastRun()
//        {
//            try
//            {
//                SQLServer objSQL = new SQLServer();
//                objSQL.ExecuteProcedure("sp_GetMasterConfiguration");
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//        }

//        /// <summary>
//        /// Method to call SAP interface process
//        /// </summary>
//        public void SAPProcess()
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(DailyExecutionDateTime.ToString()))
//                    DailyExecutionDateTime = Convert.ToDateTime(genericFunction.ReadValueFromDb("DailySAPInterfaceDateTime"));
                
//                DateTime currentDateTime = DateTime.Now;

//                if (Convert.ToDateTime(currentDateTime) >= Convert.ToDateTime(DailyExecutionDateTime))
//                    ProcessSAPInterface(currentDateTime, "Daily");

//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//        }

//        /// <summary>
//        /// Method To Process SAP Interface for VIETJET VJ-711
//        /// </summary>
//        public void ProcessSAPInterface(DateTime CurrentDate, string ProcessType = "Daily")
//        {
//            try
//            {
//                SAPInterfaceProcessor objSAPInterface = new SAPInterfaceProcessor();
//                DateTime dtFrmDate;
//                DateTime dtToDate;
//                string updatedby = "SmartKargoA";
//                DateTime updatedon;
//                int ExecutionHour = CurrentDate.Hour;

//                if (ProcessType == "Daily")
//                {
//                    DateTime tmpCurrentDate = Convert.ToDateTime(CurrentDate.ToShortDateString());
//                    dtFrmDate = Convert.ToDateTime(tmpCurrentDate.AddDays(-1));
//                    dtToDate = Convert.ToDateTime(tmpCurrentDate.AddDays(-1));
//                    updatedon = Convert.ToDateTime(tmpCurrentDate);

//                    objSAPInterface.GenerateSAPInterface(dtFrmDate, dtToDate, updatedby, updatedon);
                   
//                }
               
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//        }
//        #endregion : Private Methods
//    }
//}

