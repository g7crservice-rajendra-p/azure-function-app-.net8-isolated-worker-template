using log4net;
using log4net.Appender;
using Microsoft.Extensions.Logging;
using QidWorkerRole.SIS.FileHandling.Idec.Read;
using QidWorkerRole.SIS.FileHandling.ISValidationReport;
using QidWorkerRole.SIS.FileHandling.Xml.Read;
using QidWorkerRole.SIS.FileHandling.Xml.Read.SupportingModels;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using DbEntity = QidWorkerRole.SIS.DAL;
using ModelClass = QidWorkerRole.SIS.Model;

namespace QidWorkerRole.SIS.FileHandling
{
    public class SISFileReader
    {
        private readonly ILogger<SISFileReader> _logger;
         private static ILoggerFactory? _loggerFactory;
        private static ILogger<SISFileReader> _staticLogger => _loggerFactory?.CreateLogger<SISFileReader>();

        private readonly GenericFunction _genericFunction;
        private readonly SISBAL _sISBAL;
        public SISFileReader(
            ILogger<SISFileReader> logger,
            GenericFunction genericFunction,
            SISBAL sISBAL,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _genericFunction = genericFunction;
            _sISBAL = sISBAL;
        }

        // For Logging.
        //private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //GenericFunction objGenericFunction = new GenericFunction();
        //Cls_BL objBL = new Cls_BL();
        //SISBAL bojSISBAL = new SISBAL();

        /// <summary>
        /// Read SIS File.
        /// </summary>
        /// <param name="filePath"> File Path to be read.</param>
        /// <param name="logFilePath"> Log File Path. </param>
        /// <returns>true if successful, false if unsuccessful.</returns>
        public async Task<bool> ReadSISFile(string filePath, string CreatedBy, out string logFilePath)
        {
            string appDomainCurrentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string zipFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\ZipFiles\";
            string unZipFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\UnZipFiles\";
            const string BlobContainerName = "SIS";

            // Initialize file name property in all the log messages.
            ThreadContext.Properties["FilePath"] = filePath;

            string fileExtention = Path.GetExtension(filePath);
            string fileName = Path.GetFileName(filePath);

            string logFileName = fileName + "-" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".log";

            logFilePath = null;
            logFilePath = ChangeLogFileNameLocation(logFileName);           

            try
            {
                // Logger.InfoFormat("======================= Start of Uploading File: {0} ===============================", fileName);
                _logger.LogInformation("================================ Start of Uploading File: {0} ===============================", fileName);
                SIS.DAL.ReadDBData readDBData = new DbEntity.ReadDBData();

                if (readDBData.IsFileAlreadyExistsWithSameName(fileName))
                {
                    // Logger.Info("\t\t File with Same Name Already exist. Please upload file with different file name.");
                    // Logger.InfoFormat("======================= End of Uploading File: {0} ===============================", fileName);
                    _logger.LogInformation("\t\t File with Same Name Already exist. Please upload file with different file name.");
                    _logger.LogInformation("================================ End of Uploading File: {0} ===============================", fileName);
                    return false;
                }
                else
                {
                    ModelClass.SupportingModels.FileData fileData = null;

                    if (fileExtention != null && fileExtention.ToUpper().Equals(".DAT"))
                    {
                        IdecFileReader idecFileReader = new IdecFileReader();

                        fileData = new ModelClass.SupportingModels.FileData();

                        #region FileHeader

                        fileData.FileHeader = new ModelClass.FileHeaderClass();

                        if (fileName != null)
                        {
                            fileData.FileHeader.AirlineCode = fileName.Substring(7, 3);
                            fileData.FileHeader.VersionNumber = 0320;
                            fileData.FileHeader.FileInOutDirection = 0; // 0 for Incomming file
                            fileData.FileHeader.FileName = fileName;
                        }
                        fileData.FileHeader.FileStatusId = 4; // This is payables file and its status will be 4 i.e IS Validated.
                        fileData.FileHeader.CreatedBy = CreatedBy;
                        fileData.FileHeader.CreatedOn = DateTime.UtcNow;
                        fileData.FileHeader.LastUpdatedBy = "FileReaderSISAutomation";

                        #endregion

                        fileData.FileTotal = new ModelClass.FileTotal();
                        fileData.InvoiceList = new List<ModelClass.Invoice>();

                        foreach (var invoice in idecFileReader.Read(filePath))
                        {
                            if (invoice != null)
                            {
                                SIS.DAL.ReadDBData readDBDataForInvoice = new DbEntity.ReadDBData();
                                if (readDBDataForInvoice.IsInvoiceAlreadyExists(invoice))
                                {
                                    // Logger.InfoFormat("\t Invoice Number {0} is alerady exist between {1} (Billing Airline) & {2} (Billed Airline) in {3} (Billing Year). Hence, NOT considered while uploading this File. Please upload Invoice with different Invoice Number.", invoice.InvoiceNumber, invoice.BillingAirline, invoice.BilledAirline, invoice.BillingYear);
                                    _logger.LogInformation("\t Invoice Number {0} is alerady exist between {1} (Billing Airline) & {2} (Billed Airline) in {3} (Billing Year). Hence, NOT considered while uploading this File. Please upload Invoice with different Invoice Number.", invoice.InvoiceNumber, invoice.BillingAirline, invoice.BilledAirline, invoice.BillingYear);
                                }
                                else
                                {
                                    invoice.InvoiceStatusId = 5; // This is payables invoice received and its status will be 5 i.e. IS-Validated.
                                    invoice.IsReceivedFromFile = true; // Invoice is received from file.
                                    invoice.CreatedBy = CreatedBy;
                                    invoice.CreatedOn = DateTime.UtcNow;
                                    invoice.LastUpdatedBy = "FileReaderSISAutomation";
                                    invoice.IsSIS = true;
                                    fileData.InvoiceList.Add(invoice);
                                }
                            }
                        }
                    }

                    if (fileExtention != null && fileExtention.ToUpper().Equals(".XML"))
                    {
                        XmlFileReader xmlFileReader = new XmlFileReader(filePath);

                        TransmissionHeader transmissionHeader = xmlFileReader.ReadTransmissionHeader();

                        fileData = new ModelClass.SupportingModels.FileData();

                        #region FileHeader

                        fileData.FileHeader = new ModelClass.FileHeaderClass();

                        if (fileName != null)
                        {
                            fileData.FileHeader.AirlineCode = fileName.Substring(6, 3);
                            fileData.FileHeader.VersionNumber = 36;
                            fileData.FileHeader.FileInOutDirection = 0; // 0 for Incomming file
                            fileData.FileHeader.FileName = fileName;
                        }
                        fileData.FileHeader.FileStatusId = 4; // This is payables file and its status will be 4 i.e IS Validated.
                        fileData.FileHeader.CreatedBy = CreatedBy;
                        fileData.FileHeader.CreatedOn = DateTime.UtcNow;
                        fileData.FileHeader.LastUpdatedBy = "FileReaderSISAutomation";

                        #endregion

                        fileData.FileTotal = new ModelClass.FileTotal();
                        fileData.InvoiceList = new List<ModelClass.Invoice>();

                        foreach (var invoice in xmlFileReader.ReadInvoice())
                        {
                            if (invoice != null)
                            {
                                SIS.DAL.ReadDBData readDBDataForInvoice = new DbEntity.ReadDBData();
                                if (readDBDataForInvoice.IsInvoiceAlreadyExists(invoice))
                                {
                                    // Logger.InfoFormat("\t Invoice Number {0} is alerady exist between {1} (Billing Airline) & {2} (Billed Airline) in {3} (Billing Year). Hence, NOT considered while uploading this File. Please upload Invoice with different Invoice Number.", invoice.InvoiceNumber, invoice.BillingAirline, invoice.BilledAirline, invoice.BillingYear);
                                    _logger.LogInformation("\t Invoice Number {0} is alerady exist between {1} (Billing Airline) & {2} (Billed Airline) in {3} (Billing Year). Hence, NOT considered while uploading this File. Please upload Invoice with different Invoice Number.", invoice.InvoiceNumber, invoice.BillingAirline, invoice.BilledAirline, invoice.BillingYear);
                                }
                                else
                                {
                                    invoice.InvoiceStatusId = 5; // This is payables invoice received and its status will be 5 i.e. IS-Validated.
                                    invoice.IsReceivedFromFile = true; // Invoice is received from file.
                                    invoice.CreatedBy = CreatedBy;
                                    invoice.CreatedOn = DateTime.UtcNow;
                                    invoice.LastUpdatedBy = "FileReaderSISAutomation";
                                    invoice.IsSIS = true;
                                    fileData.InvoiceList.Add(invoice);
                                }
                            }
                        }

                        TransmissionSummary transmissionSummary = xmlFileReader.ReadTransmissionSummary();
                    }

                    if (fileData != null)
                    {
                        if (fileData.InvoiceList.Count > 0)
                        {
                            QidWorkerRole.SIS.DAL.CreateDBData createDBData = new QidWorkerRole.SIS.DAL.CreateDBData();

                            int newFileHeaderId;

                            if (createDBData.InsertReceivedFileData(fileData, CreatedBy, out newFileHeaderId))
                            {
                                // Logger.InfoFormat("======================================================================================================");
                                // Logger.InfoFormat("\t Total {0} Invoice(s) uploaded and there details are as follows:", fileData.InvoiceList.Count());
                                // Logger.InfoFormat("======================================================================================================");
                                _logger.LogInformation("======================================================================================================");
                                _logger.LogInformation("\t Total {0} Invoice(s) uploaded and there details are as follows:", fileData.InvoiceList.Count());
                                _logger.LogInformation("======================================================================================================");
                                foreach (var inv in fileData.InvoiceList)
                                {
                                    // Logger.InfoFormat("\t Invoice Number: {0}", inv.InvoiceNumber);
                                    

                                    // Logger.InfoFormat("\t \t {0} Air Way Bill(s)    : {1}", inv.AirWayBillList.Count(), string.Join(",", inv.AirWayBillList.Select(awb => awb.AWBSerialNumber.ToString() + awb.AWBCheckDigit.ToString())));

                                    // Logger.InfoFormat("\t \t {0} Rejection Memo(s)  : {1}", inv.RejectionMemoList.Count(), string.Join(",", inv.RejectionMemoList.Select(rm => rm.RejectionMemoNumber.ToString())));

                                    // Logger.InfoFormat("\t \t {0} Billing Memo(s)    : {1}", inv.BillingMemoList.Count(), string.Join(",", inv.BillingMemoList.Select(bm => bm.BillingMemoNumber.ToString())));

                                    // Logger.InfoFormat("\t \t {0} Credit Memo(s)     : {1}", inv.CreditMemoList.Count(), string.Join(",", inv.CreditMemoList.Select(cm => cm.CreditMemoNumber.ToString())));
                                    // Logger.InfoFormat("------------------------------------------------------------------------------------------------------");
                                    _logger.LogInformation("\t Invoice Number: {0}", inv.InvoiceNumber);
                                    

                                    _logger.LogInformation("\t \t {0} Air Way Bill(s)    : {1}", inv.AirWayBillList.Count(), string.Join(",", inv.AirWayBillList.Select(awb => awb.AWBSerialNumber.ToString() + awb.AWBCheckDigit.ToString())));

                                    _logger.LogInformation("\t \t {0} Rejection Memo(s)  : {1}", inv.RejectionMemoList.Count(), string.Join(",", inv.RejectionMemoList.Select(rm => rm.RejectionMemoNumber.ToString())));

                                    _logger.LogInformation("\t \t {0} Billing Memo(s)    : {1}", inv.BillingMemoList.Count(), string.Join(",", inv.BillingMemoList.Select(bm => bm.BillingMemoNumber.ToString())));

                                    _logger.LogInformation("\t \t {0} Credit Memo(s)     : {1}", inv.CreditMemoList.Count(), string.Join(",", inv.CreditMemoList.Select(cm => cm.CreditMemoNumber.ToString())));
                                    _logger.LogInformation("------------------------------------------------------------------------------------------------------");
                                }

                                // Logger.InfoFormat("======================= End of Uploading File: {0} ===============================", fileName);
                                _logger.LogInformation("================================ End of Uploading File: {0} ===============================", fileName);
                                // Upload original ZIP file to Blob.
                                //MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(zipFilePath + Path.GetFileNameWithoutExtension(filePath) + ".ZIP"));
                                
                               // string FileBlobUrl = objGenericFunction.UploadToBlob(memoryStream, Path.GetFileNameWithoutExtension(filePath) + ".ZIP", BlobContainerName);

                                // Upload Log File to Blob
                                string logFileBlobUrl = string.Empty;
                                if (!string.IsNullOrWhiteSpace(logFileName) && !string.IsNullOrWhiteSpace(logFilePath))
                                {
                                    byte[] bytes;
                                    using (FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        int index = 0;
                                        long fileLength = fs.Length;
                                        if (fileLength > Int32.MaxValue)
                                        {
                                            throw new IOException("File too long");
                                        }
                                        int count = (int)fileLength;
                                        bytes = new byte[count];
                                        while (count > 0)
                                        {
                                            int n = fs.Read(bytes, index, count);
                                            if (n == 0)
                                            {
                                                throw new InvalidOperationException("End of file reached before expected");
                                            }
                                            index += n;
                                            count -= n;
                                        }
                                    }

                                    MemoryStream logFileMemoryStream = new MemoryStream(bytes);
                                    logFileBlobUrl = _genericFunction.UploadToBlob(logFileMemoryStream, logFileName, BlobContainerName);

                                    if (newFileHeaderId > 0)
                                    {
                                        QidWorkerRole.SIS.DAL.UpdateDBData updateDBData = new QidWorkerRole.SIS.DAL.UpdateDBData();
                                        updateDBData.UpdateReceivedFileHeaderData(newFileHeaderId, DBNull.Value.ToString(), logFileBlobUrl);

                                        string strMsg = "";
                                        DataSet ds = await _sISBAL.InterlineMatchPayablesAWBs(newFileHeaderId.ToString(), CreatedBy, strMsg);
                                    }

                                    return true;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // Logger.Info("0 Invoices found in the file to upload.");
                            // Logger.InfoFormat("======================= End of Uploading File: {0} ===============================", fileName);
                            _logger.LogInformation("0 Invoices found in the file to upload.");
                            _logger.LogInformation("================================ End of Uploading File: {0} ===============================", fileName);
                            return false;
                        }
                    }
                    else
                    {
                        // Logger.Info("No data found in the file to upload.");
                        // Logger.InfoFormat("======================= End of Uploading File: {0} ===============================", fileName);
                        _logger.LogWarning("No data found in the file to upload.");
                        _logger.LogInformation("================================ End of Uploading File: {0} ===============================", fileName);
                        return false;
                    }
                }
            }
            catch (Exception exception)
            {
                // Logger.InfoFormat("Exception Occured in Read SIS File. Message: {0}, StackTrace: {1}", exception.Message, exception.StackTrace);
                _logger.LogError("Exception Occured in Read SIS File. Message: {0}, StackTrace: {1}", exception.Message, exception.StackTrace);

                return false;
            }
        }

        public static string ChangeLogFileNameLocation(string fileName)
        {
            try
            {
                //var config = XmlConfigurator.Configure(); //CEBV4-7795
                log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
    
                string logFileLocation = string.Empty;           
    
                foreach (IAppender a in h.Root.Appenders)
                {
                    if (a is FileAppender)
                    {
                        FileAppender fa = (FileAppender)a;
                        // Programmatically set this to the desired location here
                        string FileLocationinWebConfig = fa.File;
                        
                        logFileLocation = FileLocationinWebConfig + fileName;
    
                        fa.File = logFileLocation;
                        fa.ActivateOptions();
                        break;
                    }
                }            
    
            return logFileLocation;
            }
            catch (System.Exception ex)
            {
                _staticLogger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Read & Upload SIS Validation Report R1 & R2 File
        /// </summary>
        /// <param name="validationFileDirectoryPath">Validation File Directory Path</param>
        /// <param name="UserName">User Name</param>
        /// <param name="rejectionOnValidationFailure">Rejection On Validation Failure Flag</param>
        /// <param name="onlineCorrectionAllowed">Online Correction Allowed Flag</param>
        /// <returns></returns>
        public int ReadSISValidationReportFile(string validationFileDirectoryPath, string zipFileNameWithPath, string UserName, int rejectionOnValidationFailure
                                                , bool onlineCorrectionAllowed, ref string logValFilePath, int receivablesFileID
            ,ref string strAzulOracleInvList)
        {
            try
            {
                string appDomainCurrentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string zipValidatioinReportFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\ZipFiles\ValidatioinReport\";
                string unZipValidatioiReportFilePath = appDomainCurrentDomainBaseDirectory + @"\SISFilesReceived\UnZipFiles\ValidatioinReport\";
                const string BlobContainerName = "SIS";
    
               
                // Initialize file name property in all the log messages.
                ThreadContext.Properties["FilePath"] = zipFileNameWithPath;
    
                string valFileName = Path.GetFileName(zipFileNameWithPath);
    
                // clsLog.WriteLogAzure("ReadSISValidationReportFile: " + " (" + valFileName + ")");
                _logger.LogInformation("ReadSISValidationReportFile: ({0})" ,  valFileName );
    
                string logValFileName = valFileName + "-" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".log";
    
                logValFilePath = ChangeLogFileNameLocation(logValFileName);
    
                // _logger.LogInformation("======================= Start of Uploading IS Validation File: {0} ===============================", valFileName);
                _logger.LogInformation("======================= Start of Uploading IS Validation File: {0} ===============================", valFileName);
    
                var ValidationFilePathR1 = Directory.GetFiles(validationFileDirectoryPath, "*_VAL_R1.CSV");
    
                if (ValidationFilePathR1 != null)
                {
                    if (ValidationFilePathR1.Count() == 1)
    	            {
                        if (!string.IsNullOrWhiteSpace(ValidationFilePathR1[0]))
                        {
                            List<ModelClass.ISValidationReport.ISValidationSummaryReport> listISValidationSummaryReport = new List<ModelClass.ISValidationReport.ISValidationSummaryReport>();
                            
                            ISValidationReportReader iSValidationReportReader = new ISValidationReportReader();                        
                            listISValidationSummaryReport = iSValidationReportReader.ReadISValidationSummaryReportR1(ValidationFilePathR1[0]);
                            // clsLog.WriteLogAzure("iSValidationReportReader.ReadISValidationSummaryReportR1: " + " (" + ValidationFilePathR1[0] + ")");
                            _logger.LogInformation("iSValidationReportReader.ReadISValidationSummaryReportR1: ({0})", ValidationFilePathR1[0]);
                            if (listISValidationSummaryReport.Count > 0)
                            {
                                #region For R2
    
                                var ValidationFilePathR2 = Directory.GetFiles(validationFileDirectoryPath, "*_VAL_R2.CSV");
                                List<ModelClass.ISValidationReport.ISValidationDetailErrorReport> listISValidationDetailErrorReport = new List<ModelClass.ISValidationReport.ISValidationDetailErrorReport>();
                                if (ValidationFilePathR2 != null)
                                {
                                    if (ValidationFilePathR2.Count() == 1)
                                    {
                                        if (!string.IsNullOrWhiteSpace(ValidationFilePathR2[0]))
                                        {
                                            listISValidationDetailErrorReport = iSValidationReportReader.ReadISValidationDetailErrorReportR2(ValidationFilePathR2[0]);
                                        }
                                        else
                                        {
                                            // Back R2 list to be passed to upload method.
                                        }
                                    }
                                    else
                                    {
                                        // Back R2 list to be passed to upload method.
                                    }
                                }
                                else
                                {
                                    // Back R2 list to be passed to upload method.
                                }
    
                                #endregion
    
                                DbEntity.CreateDBData createDBData = new DbEntity.CreateDBData();
                                if (createDBData.UpdateStatusAndInsertISValidationReportR1R2(listISValidationSummaryReport, listISValidationDetailErrorReport, UserName, rejectionOnValidationFailure, onlineCorrectionAllowed,ref strAzulOracleInvList))
                                {
                                    // Logger.InfoFormat("\t \t {0} record(s) in  IS Validation Summary Report.", listISValidationSummaryReport.Count());
    
                                    // Logger.InfoFormat("\t \t {0} record(s) in  IS Validation Details Report.", listISValidationDetailErrorReport.Count());
    
                                    // Logger.InfoFormat("========================= End of Uploading IS Validation File: {0} ===============================", valFileName);
                                    _logger.LogInformation("\t \t {0} record(s) in  IS Validation Summary Report.", listISValidationSummaryReport.Count());
    
                                    _logger.LogInformation("\t \t {0} record(s) in  IS Validation Details Report.", listISValidationDetailErrorReport.Count());
    
                                    _logger.LogInformation("========================= End of Uploading IS Validation File: {0} ===============================", valFileName);
    
                                    // Upload original ZIP file to Blob.
                                    //MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(zipFileNameWithPath));
                                    //string valFileBlobUrl = objGenericFunction.UploadToBlob(memoryStream, Path.GetFileNameWithoutExtension(zipFileNameWithPath) + ".ZIP", BlobContainerName);
    
                                    // Upload Log File to Blob
                                    string logValFileBlobUrl = string.Empty;
                                    if (!string.IsNullOrWhiteSpace(logValFileName) && !string.IsNullOrWhiteSpace(logValFilePath))
                                    {
                                        byte[] bytes;
                                        using (FileStream fs = new FileStream(logValFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            int index = 0;
                                            long fileLength = fs.Length;
                                            if (fileLength > Int32.MaxValue)
                                            {
                                                throw new IOException("File too long");
                                            }
                                            int count = (int)fileLength;
                                            bytes = new byte[count];
                                            while (count > 0)
                                            {
                                                int n = fs.Read(bytes, index, count);
                                                if (n == 0)
                                                {
                                                    throw new InvalidOperationException("End of file reached before expected");
                                                }
                                                index += n;
                                                count -= n;
                                            }
                                        }
    
                                        MemoryStream logFileMemoryStream = new MemoryStream(bytes);
                                        logValFileBlobUrl = _genericFunction.UploadToBlob(logFileMemoryStream, logValFileName, BlobContainerName);
                                        DbEntity.CreateDBData createDBDatav = new DbEntity.CreateDBData();
                                        createDBDatav.CreateReceivedISValidationFileHeaderData(Path.GetFileNameWithoutExtension(zipFileNameWithPath) + ".ZIP", DBNull.Value.ToString(), logValFileBlobUrl, receivablesFileID, UserName);
                                    }
                                    return 1; // validation report uploaded & statuses updated successfully... success message to user.
                                }
                                else
                                {
                                    return 0; // Problem in uploading & status update validation report... error message to user.
                                }
                            }
                            else
                            {
                                return 2; // no validation report R1 found... error message to user.
                            }
                        }
                        else
                        {
                            return 2; // no validation report R1 found... error message to user.
                        }
    	            }
                    else
                    {
                        return 2; // no validation report R1 found... error message to user.
                    }
                }
                else
                {
                    return 2; // no validation report R1 found... error message to user.
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }
    }
}