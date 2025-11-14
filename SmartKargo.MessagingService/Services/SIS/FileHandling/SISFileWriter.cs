//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Ionic.Zip;
//using log4net.Config;
//using log4net.Appender;
//using QidWorkerRole.SIS.DAL;
//using QidWorkerRole.SIS.FileHandling.Idec.Write;
//using QidWorkerRole.SIS.FileHandling.Xml.Write;
//using QidWorkerRole.SIS.Model.SupportingModels;
//using log4net;

//namespace QidWorkerRole.SIS.FileHandling
//{
//    public class SISFileWriter
//    {
//        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
//        const string BlobContainerName = "SIS";
//        GenericFunction objGenericFunction = new GenericFunction();
//        string LogFilePath = string.Empty;

//        /// <summary>
//        /// Generates File for SIS.
//        /// </summary>
//        /// <param name="listInvoiceHeaderID">List of Invoice Header Ids to be included in the file.</param>
//        /// <param name="AirlineCode">Billing Airline Code</param>
//        /// <param name="isIdec">ISIDEC : True, ISXML: False</param>
//        /// <returns>SIS IS-IDEC or IS-XML File</returns>
//        public bool GenerateSISFile(List<int> listInvoiceHeaderID, string airlineCode, string updatedBy, out string filePath, bool isIdec = false)
//        {
//            // listInvoiceHeaderID = new List<int> { 1, 2 };
//            try
//            {
//                string newFilePath = GetOutputFileName(airlineCode, isIdec);
//                string newFileName = Path.GetFileName(newFilePath);
//                string logFileName = Path.GetFileName(newFilePath) + "-" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".log";

//                LogFilePath = ChangeLogFileNameLocation(logFileName);

//                Logger.InfoFormat("======================= Start of File Generation: {0} ===============================", newFileName);

//                CreateDBData createDBData = new CreateDBData();
//                createDBData.UpdateBatchSequenceNumbers(listInvoiceHeaderID);

//                //Logger.Info("Batch Number, Sequence Number and Breakdown Serial Numbers updated.");

//                ReadDBData getDBData = new ReadDBData();
//                FileData fileData = getDBData.GetFileData(listInvoiceHeaderID, airlineCode);

//                if (fileData.InvoiceList.Count() > 0)
//                {

//                    if (GenerateFile(fileData, newFilePath, isIdec, out filePath))
//                    {
//                        Logger.InfoFormat("======================================================================================================");
//                        Logger.InfoFormat("\t Total {0} Invoice(s) included and there details are as follows:", fileData.InvoiceList.Count());
//                        Logger.InfoFormat("======================================================================================================");

//                        foreach (var inv in fileData.InvoiceList)
//                        {
//                            Logger.InfoFormat("\t Invoice Number: {0}", inv.InvoiceNumber);

//                            Logger.InfoFormat("\t \t {0} Air Way Bill(s)    : {1}", inv.AirWayBillList.Count(), string.Join(",", inv.AirWayBillList.Select(awb => awb.AWBSerialNumber.ToString() + awb.AWBCheckDigit.ToString())));

//                            Logger.InfoFormat("\t \t {0} Rejection Memo(s)  : {1}", inv.RejectionMemoList.Count(), string.Join(",", inv.RejectionMemoList.Select(rm => rm.RejectionMemoNumber.ToString())));

//                            Logger.InfoFormat("\t \t {0} Billing Memo(s)    : {1}", inv.BillingMemoList.Count(), string.Join(",", inv.BillingMemoList.Select(bm => bm.BillingMemoNumber.ToString())));

//                            Logger.InfoFormat("\t \t {0} Credit Memo(s)     : {1}", inv.CreditMemoList.Count(), string.Join(",", inv.CreditMemoList.Select(cm => cm.CreditMemoNumber.ToString())));
//                            Logger.InfoFormat("------------------------------------------------------------------------------------------------------");
//                        }

//                        Logger.InfoFormat("======================= End of File Generation: {0} ===============================", newFileName);

//                        char[] splitChar = { '\\' };
//                        string[] fileName = filePath.Split(splitChar);

//                        // Upload File to Blob
//                        MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(filePath));
//                        String FileUrl = objGenericFunction.UploadToBlob(memoryStream, fileName[fileName.Length - 1], BlobContainerName);

//                        // Upload Log File to Blob
//                        string logFileBlobUrl = string.Empty;

//                        byte[] bytes;
//                        using (FileStream fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
//                        {
//                            int index = 0;
//                            long fileLength = fs.Length;
//                            if (fileLength > Int32.MaxValue)
//                            {
//                                throw new IOException("File too long");
//                            }
//                            int count = (int)fileLength;
//                            bytes = new byte[count];
//                            while (count > 0)
//                            {
//                                int n = fs.Read(bytes, index, count);
//                                if (n == 0)
//                                {
//                                    throw new InvalidOperationException("End of file reached before expected");
//                                }
//                                index += n;
//                                count -= n;
//                            }
//                        }

//                        MemoryStream logFileMemoryStream = new MemoryStream(bytes);
//                        logFileBlobUrl = objGenericFunction.UploadToBlob(logFileMemoryStream, logFileName, BlobContainerName);

//                        QidWorkerRole.SIS.DAL.UpdateDBData updateDBData = new QidWorkerRole.SIS.DAL.UpdateDBData();
//                        updateDBData.UpdateDataAfterFileGenerated(fileData.FileTotal.FileHeaderID, fileName[fileName.Length - 1], FileUrl, logFileBlobUrl, updatedBy);

//                        return true;
//                    }
//                    else
//                    {
//                        Logger.Error("Problem in File Generation");
//                        filePath = LogFilePath;
//                        return false;
//                    }

//                }
//                else
//                {
//                    Logger.Error("Zero Invoices found for the file.");
//                    filePath = LogFilePath;
//                    return false;
//                }
//            }
//            catch (Exception exception)
//            {
//                clsLog.WriteLogAzure(exception);
//                filePath = LogFilePath;
//                return false;
//            }
//        }

//        /// <summary>
//        /// Generates IS-IDEC or IS-XML File for the given File Data.
//        /// </summary>
//        /// <param name="fileData">File Data</param>
//        /// <returns>IS-IDEC or IS-XML File.</returns>
//        public bool GenerateFile(FileData fileData, string writeFilePath, bool isIdec, out string filePath)
//        {
//            try
//            {
//                // Generates IS-IDEC File for the given File Data.
//                if (isIdec)
//                {
//                    //Logger.Info("IDEC File Generation Start.");

//                    //Logger.InfoFormat("idecFilePath: {0}", writeFilePath);

//                    var idecFileWriter = new IdecFileWriter();

//                    idecFileWriter.WriteIdecFile(fileData, writeFilePath);

//                    filePath = ZipOutputFile(writeFilePath);

//                    //Logger.Info("IDEC File Generation End.");

//                    return true;
//                }
//                // Generates IS-XML File for the given File Data.
//                else
//                {
//                    //Logger.Info("XML File Generation Start.");

//                    //Logger.InfoFormat("xmlFilePath: {0}", writeFilePath);

//                    var xmlFileWriter = new XmlFileWriter();

//                    xmlFileWriter.Init(writeFilePath);

//                    xmlFileWriter.WriteXMLFile(fileData.InvoiceList);

//                    filePath = ZipOutputFile(writeFilePath);

//                    //Logger.Info("XML File Generation End.");

//                    return true;
//                }
                
//            }
//            catch (Exception exception)
//            {
//                filePath = string.Empty;
//                clsLog.WriteLogAzure(exception);
//                filePath = LogFilePath;
//                return false;
//            }
            
//        }

//        /// <summary>
//        /// Get the File Name with complete File Path.
//        /// </summary>
//        /// <param name="airlineCode">Airline Code</param>
//        /// <param name="isIsIdec">ISIDEC : True, ISXML: False</param>
//        /// <returns>File Name with complete File Paht.</returns>
//        private string GetOutputFileName(string airlineCode, bool isIsIdec = true)
//        {
//            string applicationBasePath = AppDomain.CurrentDomain.BaseDirectory;
//            string basePath = applicationBasePath + @"\SISFilesGenerated\";

//            if (!Directory.Exists(basePath))
//            {
//                Directory.CreateDirectory(basePath);
//            }

//            ReadDBData getDBData = new ReadDBData();

//            DateTime currentOpenBillingPeriod = getDBData.GetCurrentOpenBillingPeriod(DateTime.UtcNow, "I");

//            return isIsIdec ? Path.Combine(basePath, "CIDECF-" + airlineCode +
//                                           currentOpenBillingPeriod.Year.ToString().PadLeft(4, '0') +
//                                           currentOpenBillingPeriod.Month.ToString().PadLeft(2, '0') +
//                                           currentOpenBillingPeriod.Day.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Year.ToString().PadLeft(4, '0') +
//                                           DateTime.UtcNow.Month.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Day.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Hour.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Minute.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Second.ToString().PadLeft(2, '0') + ".DAT")
//                            : Path.Combine(basePath, "CXMLF-" + airlineCode +
//                                           currentOpenBillingPeriod.Year.ToString().PadLeft(4, '0') +
//                                           currentOpenBillingPeriod.Month.ToString().PadLeft(2, '0') +
//                                           currentOpenBillingPeriod.Day.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Year.ToString().PadLeft(4, '0') +
//                                           DateTime.UtcNow.Month.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Day.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Hour.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Minute.ToString().PadLeft(2, '0') +
//                                           DateTime.UtcNow.Second.ToString().PadLeft(2, '0') + ".XML");
//        }

//        /// <summary>
//        /// Generates Zip file for the given file.
//        /// </summary>
//        /// <param name="outputFileName">Output File Name with complete File Path.</param>
//        /// <returns>Zip File</returns>
//        public string ZipOutputFile(string outputFileName)
//        {
//            try
//            {
//                var zipFileName = string.Format("{0}.ZIP", Path.Combine(Path.GetDirectoryName(outputFileName), Path.GetFileNameWithoutExtension(outputFileName)));
//                using (var zipFile = new ZipFile())
//                {
//                    zipFile.AddFile(outputFileName, ""); //Note : the second parameter will add file in the same directory
//                    zipFile.Save(zipFileName);

//                    //Delete source files
//                    if (System.IO.File.Exists(outputFileName))
//                        System.IO.File.Delete(outputFileName);

//                    return zipFileName;
//                }
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//                return string.Empty;
//            }
//        }

//        public static string ChangeLogFileNameLocation(string fileName)
//        {
//            //var config = XmlConfigurator.Configure();  //CEBV4-7795
//            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();

//            string logFileLocation = string.Empty;



//            foreach (IAppender a in h.Root.Appenders)
//            {
//                if (a is FileAppender)
//                {
//                    FileAppender fa = (FileAppender)a;
//                    // Programmatically set this to the desired location here
//                    string FileLocationinWebConfig = fa.File;

//                    logFileLocation = FileLocationinWebConfig + fileName;

//                    fa.File = logFileLocation;
//                    fa.ActivateOptions();
//                    break;
//                }
//            }

//            return logFileLocation;
//        }
//    }
//}