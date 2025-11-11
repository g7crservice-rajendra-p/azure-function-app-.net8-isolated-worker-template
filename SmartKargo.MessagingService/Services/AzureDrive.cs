using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Auth;
//using Microsoft.WindowsAzure.Storage.File;
//using Microsoft.WindowsAzure.StorageClient;
//using QID.DataAccess;
//using System;
//using System.Collections.Generic;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Data;
using System.Net;
using System.Text;

namespace QidWorkerRole
{
    public class AzureDrive
    {
        //string storageAccountName = string.Empty;
        //string storageAccountKey = string.Empty;
        //string shareDriveName = string.Empty;
        //string RootFolder = string.Empty;
        //string RootFolder_IN = "in";
        //string RootFolder_OUT = "out";
        //string BackupFolder = "QidMessageBackup";
        //SCMExceptionHandlingWorkRole scmeception = new SCMExceptionHandlingWorkRole();
        //StorageCredentials objCrd = null;
        //CloudStorageAccount storageAccount = null;
        //CloudFileClient fileClient = null;
        //CloudFileShare shareDrive = null;
        //CloudFileDirectory dirDrive = null;
        //CloudFileDirectory fldrRroot = null;
        //CloudFileDirectory fldrBackup = null;
        //CloudFileDirectory fldrOut = null;

        string RootFolder_IN = "in";
        string RootFolder_OUT = "out";
        string BackupFolder = "QidMessageBackup";

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<AzureDrive> _logger;
        private readonly GenericFunction _genericFunction;
        public AzureDrive(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<AzureDrive> logger,
            GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }

        /*Deprecated*/
        //public void ReadFromSITADrive()
        //{
        //    try
        //    {
        //        clsLog.WriteLogAzure("In ReadFromSITADrive()");
        //        GenericFunction genericFunction = new GenericFunction();
        //        storageAccountName = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
        //        storageAccountKey = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
        //        shareDriveName = ConfigCache.Get("SHARE_DRIVE_VM_DRIVE_NAME");
        //        RootFolder = ConfigCache.Get("SHARE_DRIVE_VM_ROOT_FOLDER");
        //        if (storageAccountName != "" && storageAccountKey != "" && shareDriveName != "" && RootFolder != "")
        //        {
        //            StorageCredentials objCrd = new StorageCredentials(storageAccountName, storageAccountKey);
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //            CloudStorageAccount storageAccount = new CloudStorageAccount(objCrd, true);
        //            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
        //            CloudFileShare shareDrive = fileClient.GetShareReference(shareDriveName);
        //            if (shareDrive.Exists())
        //            {
        //                CloudFileDirectory dirDrive = shareDrive.GetRootDirectoryReference();
        //                CloudFileDirectory fldrRroot = dirDrive.GetDirectoryReference(RootFolder);
        //                CloudFileDirectory fldrBackup = dirDrive.GetDirectoryReference(BackupFolder);
        //                CloudFileDirectory fldrIN = fldrRroot.GetDirectoryReference(RootFolder_IN);
        //                IEnumerable<IListFileItem> fileList = fldrIN.ListFilesAndDirectories();
        //                foreach (var fileItem in fileList)
        //                {
        //                    if (fileItem is CloudFile)
        //                    {
        //                        CloudFile fileIn = fileItem as CloudFile;
        //                        if (fileIn.Exists())
        //                        {
        //                            string strMessage = fileIn.DownloadTextAsync().Result;
        //                            CloudFile fileBackup = fldrBackup.GetFileReference(DateTime.Now.ToString("yyyyMMdd_HHmm") + "_" + fileIn.Name);
        //                            genericFunction.SaveIncomingMessageInDatabase("MSG:" + fileIn.Name, strMessage, "SITAftp", "", DateTime.Now, DateTime.Now, "SITA", "Active", "SITA");
        //                            fileBackup.Create(1);
        //                            fileBackup.UploadText(strMessage);
        //                            fileIn.Delete();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}


        /// <summary>
        /// Below Method used to Read file from shared Drive and save in the database
        /// </summary>
        /// 
        public void ReadFromSITADrive()
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();

                // clsLog.WriteLogAzure("In ReadFromSITADrive()");
                _logger.LogInformation("In ReadFromSITADrive()");

                var storageAccountName = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
                var storageAccountKey = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
                var shareDriveName = ConfigCache.Get("SHARE_DRIVE_VM_DRIVE_NAME");
                var RootFolder = ConfigCache.Get("SHARE_DRIVE_VM_ROOT_FOLDER");

                if (!string.IsNullOrEmpty(storageAccountName) &&
                    !string.IsNullOrEmpty(storageAccountKey) &&
                    !string.IsNullOrEmpty(shareDriveName) &&
                    !string.IsNullOrEmpty(RootFolder))
                {
                    // Keep TLS setting (optional but retained from original)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    // Build service client using account name/key
                    var serviceUri = new Uri($"https://{storageAccountName}.file.core.windows.net");
                    var credential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                    var serviceClient = new ShareServiceClient(serviceUri, credential);

                    // Get share client
                    var shareClient = serviceClient.GetShareClient(shareDriveName);

                    // Check whether share exists
                    if (shareClient.Exists().Value)
                    {
                        var rootDir = shareClient.GetRootDirectoryClient();
                        var fldrRroot = rootDir.GetSubdirectoryClient(RootFolder);
                        var fldrBackup = rootDir.GetSubdirectoryClient(BackupFolder);
                        var fldrIN = fldrRroot.GetSubdirectoryClient(RootFolder_IN);

                        // Enumerate files and directories (synchronous pageable)
                        Pageable<ShareFileItem> fileList = fldrIN.GetFilesAndDirectories();

                        foreach (ShareFileItem fileItem in fileList)
                        {
                            // If it's a file (not a directory)
                            if (!fileItem.IsDirectory)
                            {
                                var fileClient = fldrIN.GetFileClient(fileItem.Name);

                                if (fileClient.Exists().Value)
                                {
                                    // Download file content to string (synchronous)
                                    var downloadResponse = fileClient.Download();
                                    string strMessage;
                                    using (var stream = downloadResponse.Value.Content)
                                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        strMessage = reader.ReadToEnd();
                                    }

                                    // Save incoming message in DB (same as original order)
                                    _genericFunction.SaveIncomingMessageInDatabase(
                                        "MSG:" + fileItem.Name,
                                        strMessage,
                                        "SITAftp",
                                        "",
                                        DateTime.Now,
                                        DateTime.Now,
                                        "SITA",
                                        "Active",
                                        "SITA");

                                    // Create a backup file and upload text (preserve naming format)
                                    var backupFileName = DateTime.Now.ToString("yyyyMMdd_HHmm") + "_" + fileItem.Name;
                                    var fileBackup = fldrBackup.GetFileClient(backupFileName);

                                    // Upload requires creating the file with correct length first
                                    byte[] contentBytes = Encoding.UTF8.GetBytes(strMessage ?? string.Empty);
                                    long contentLength = contentBytes.Length;

                                    // Ensure backup directory exists (GetSubdirectoryClient doesn't auto-create)
                                    // (If directories are guaranteed to exist, these calls are harmless; otherwise they create them.)
                                    try { fldrBackup.CreateIfNotExists(); } catch { /* ignore/create failures */ }

                                    // Create backup file with appropriate length and upload content
                                    fileBackup.Create(contentLength);
                                    using (var ms = new MemoryStream(contentBytes))
                                    {
                                        // Upload will write from offset 0
                                        fileBackup.Upload(ms);
                                    }

                                    // Delete original file
                                    fileClient.Delete();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,"Error on ReadFromSITADrive");
            }
        }

        /*Deprecated*/
        //public bool UploadToDrive(string FileName, string MsgBody, string folderPath)
        //{
        //    bool flag = false;
        //    try
        //    {
        //        GenericFunction genericFunction = new GenericFunction();
        //        var storageAccountName = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
        //        var storageAccountKey = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
        //        var shareDriveName = ConfigCache.Get("SHARE_DRIVE_VM_DRIVE_NAME");

        //        if (folderPath.Trim() == string.Empty)
        //            RootFolder = ConfigCache.Get("SHARE_DRIVE_VM_ROOT_FOLDER");
        //        else
        //            RootFolder = folderPath;

        //        if (storageAccountName != "" && storageAccountKey != "" && shareDriveName != "")
        //        {
        //            objCrd = new StorageCredentials(storageAccountName, storageAccountKey);
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //            storageAccount = new CloudStorageAccount(objCrd, true);
        //            fileClient = storageAccount.CreateCloudFileClient();
        //            var blobClient = storageAccount.CreateCloudBlobClient();
        //            shareDrive = fileClient.GetShareReference(shareDriveName);
        //            if (shareDrive.Exists())
        //            {
        //                dirDrive = shareDrive.GetRootDirectoryReference();
        //                fldrRroot = dirDrive.GetDirectoryReference(RootFolder);
        //                fldrOut = fldrRroot.GetDirectoryReference(RootFolder_OUT);
        //                fldrBackup = dirDrive.GetDirectoryReference(BackupFolder);
        //                string strfileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".SND";
        //                CloudFile fileOut = fldrOut.GetFileReference(strfileName);
        //                fileOut.Create(1);
        //                fileOut.UploadTextAsync(MsgBody);
        //                CloudFile fileBackup = fldrBackup.GetFileReference(DateTime.Now.ToString("yyyyMMdd_HHmm_fff") + ".SND");
        //                fileBackup.Create(1);
        //                fileBackup.UploadTextAsync(MsgBody);
        //                flag = true;
        //            }
        //        }

        //    }
        //    catch (Exception objEx)
        //    {
        //        //scmeception.logexception(ref objEx);
        //        clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + objEx.Message.ToString());
        //        flag = false;
        //    }
        //    return flag;
        //}

        /// <summary>
        /// File Upload on Share Drive
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="MsgBody"></param>
        /// <returns></returns>
        public bool UploadToDrive(string FileName, string MsgBody, string folderPath)
        {
            bool flag = false;
            try
            {
                var storageAccountName = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
                var storageAccountKey = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
                var shareDriveName = ConfigCache.Get("SHARE_DRIVE_VM_DRIVE_NAME");
                var RootFolder = string.Empty;

                if (folderPath.Trim() == string.Empty)
                {
                    RootFolder = ConfigCache.Get("SHARE_DRIVE_VM_ROOT_FOLDER");
                }
                else
                {
                    RootFolder = folderPath;
                }

                if (!string.IsNullOrEmpty(storageAccountName) &&
                    !string.IsNullOrEmpty(storageAccountKey) &&
                    !string.IsNullOrEmpty(shareDriveName))
                {
                    // Keep TLS setting from original
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    // Build the service client using account name/key
                    var serviceUri = new Uri($"https://{storageAccountName}.file.core.windows.net");
                    var credential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                    var serviceClient = new ShareServiceClient(serviceUri, credential);

                    // Get share client
                    var shareClient = serviceClient.GetShareClient(shareDriveName);

                    // Check whether share exists
                    if (shareClient.Exists().Value)
                    {
                        // Root directory client
                        var dirDrive = shareClient.GetRootDirectoryClient();

                        // Subdirectories (note: CreateIfNotExists used to be safe)
                        var fldrRroot = dirDrive.GetSubdirectoryClient(RootFolder);
                        var fldrOut = fldrRroot.GetSubdirectoryClient(RootFolder_OUT);
                        var fldrBackup = dirDrive.GetSubdirectoryClient(BackupFolder);

                        // Ensure directories exist (harmless if they already do)
                        try { fldrRroot.CreateIfNotExists(); } catch { /* ignore */ }
                        try { fldrOut.CreateIfNotExists(); } catch { /* ignore */ }
                        try { fldrBackup.CreateIfNotExists(); } catch { /* ignore */ }

                        // Compose filenames (preserve original timestamp formats)
                        string strfileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".SND";
                        string backupFileName = DateTime.Now.ToString("yyyyMMdd_HHmm_fff") + ".SND";

                        // Prepare content bytes
                        byte[] contentBytes = Encoding.UTF8.GetBytes(MsgBody ?? string.Empty);
                        long contentLength = contentBytes.Length;

                        // Create and upload main file
                        var fileOut = fldrOut.GetFileClient(strfileName);
                        fileOut.Create(contentLength); // create file with the required length
                        using (var ms = new MemoryStream(contentBytes))
                        {
                            fileOut.Upload(ms); // upload content
                        }

                        // Create and upload backup file
                        var fileBackup = fldrBackup.GetFileClient(backupFileName);
                        fileBackup.Create(contentLength);
                        using (var ms2 = new MemoryStream(contentBytes))
                        {
                            fileBackup.Upload(ms2);
                        }

                        flag = true;
                    }
                }
            }
            catch (Exception objEx)
            {
                // preserve original logging style/message
                // clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + objEx.Message.ToString());
                _logger.LogError(objEx, "uploaded on Azure Share Drive successfully to: {Message}" , objEx.Message.ToString());
                flag = false;
            }
            return flag;
        }

        /*Deprecated*/
        //public void DRIVEUpload(System.Data.DataTable dtMessagesToSend)
        //{
        //    try
        //    {
        //        string status = "Active";
        //        try
        //        {
        //            GenericFunction genericFunction = new GenericFunction();
        //            storageAccountName = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
        //            storageAccountKey = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
        //            shareDriveName = ConfigCache.Get("SHARE_DRIVE_VM_DRIVE_NAME");
        //            RootFolder = ConfigCache.Get("SHARE_DRIVE_VM_ROOT_FOLDER");
        //            if (storageAccountName != "" && storageAccountKey != "" && shareDriveName != "")
        //            {
        //                objCrd = new StorageCredentials(storageAccountName, storageAccountKey);
        //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //                storageAccount = new CloudStorageAccount(objCrd, true);
        //                fileClient = storageAccount.CreateCloudFileClient();
        //                var blobClient = storageAccount.CreateCloudBlobClient();
        //                shareDrive = fileClient.GetShareReference(shareDriveName);
        //                if (shareDrive.Exists())
        //                {
        //                    dirDrive = shareDrive.GetRootDirectoryReference();
        //                    fldrRroot = dirDrive.GetDirectoryReference(RootFolder);
        //                    fldrOut = fldrRroot.GetDirectoryReference(RootFolder_OUT);
        //                    fldrBackup = dirDrive.GetDirectoryReference(BackupFolder);
        //                    string messageBody = string.Empty;

        //                    for (int i = 0; i < dtMessagesToSend.Rows.Count; i++)
        //                    {
        //                        messageBody = dtMessagesToSend.Rows[i]["body"].ToString();
        //                        string strfileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".SND";
        //                        CloudFile fileOut = fldrOut.GetFileReference(strfileName);
        //                        fileOut.Create(1);
        //                        fileOut.UploadTextAsync(messageBody);
        //                        CloudFile fileBackup = fldrBackup.GetFileReference(DateTime.Now.ToString("yyyyMMdd_HHmm_fff") + ".SND");
        //                        fileBackup.Create(1);
        //                        fileBackup.UploadTextAsync(messageBody);

        //                        if (dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[0]["STATUS"].ToString().Length < 1)
        //                            status = "Processed";
        //                        if (dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
        //                            status = "Re-Processed";
        //                        string[] pname = { "num", "Status" };
        //                        object[] pvalue = { int.Parse(dtMessagesToSend.Rows[0]["STATUS"].ToString()), status };
        //                        SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
        //                        SQLServer sqlServer = new SQLServer();
        //                        if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
        //                            clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dtMessagesToSend.Rows[0]["STATUS"].ToString());
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception objEx)
        //        {
        //            //scmeception.logexception(ref objEx);
        //            clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + objEx.Message.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        public async Task DRIVEUpload(System.Data.DataTable dtMessagesToSend)
        {
            try
            {
                string status = "Active";
                try
                {
                    var storageAccountName = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
                    var storageAccountKey = ConfigCache.Get("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
                    var shareDriveName = ConfigCache.Get("SHARE_DRIVE_VM_DRIVE_NAME");
                    var RootFolder = ConfigCache.Get("SHARE_DRIVE_VM_ROOT_FOLDER");

                    if (!string.IsNullOrEmpty(storageAccountName) &&
                        !string.IsNullOrEmpty(storageAccountKey) &&
                        !string.IsNullOrEmpty(shareDriveName))
                    {
                        // retain TLS setting from original code
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        // build service client
                        var serviceUri = new Uri($"https://{storageAccountName}.file.core.windows.net");
                        var credential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                        var serviceClient = new ShareServiceClient(serviceUri, credential);

                        // get share client
                        var shareClient = serviceClient.GetShareClient(shareDriveName);

                        // check share exists
                        if (shareClient.Exists().Value)
                        {
                            // root and subdirectory clients (assumes BackupFolder, RootFolder_OUT exist as class fields)
                            var dirDrive = shareClient.GetRootDirectoryClient();
                            var fldrRroot = dirDrive.GetSubdirectoryClient(RootFolder);
                            var fldrOut = fldrRroot.GetSubdirectoryClient(RootFolder_OUT);
                            var fldrBackup = dirDrive.GetSubdirectoryClient(BackupFolder);

                            // ensure directories exist (safe no-op if already present)
                            try { fldrRroot.CreateIfNotExistsAsync().GetAwaiter().GetResult(); } catch { /* ignore */ }
                            try { fldrOut.CreateIfNotExistsAsync().GetAwaiter().GetResult(); } catch { /* ignore */ }
                            try { fldrBackup.CreateIfNotExistsAsync().GetAwaiter().GetResult(); } catch { /* ignore */ }

                            string messageBody = string.Empty;

                            for (int i = 0; i < dtMessagesToSend.Rows.Count; i++)
                            {
                                messageBody = dtMessagesToSend.Rows[i]["body"].ToString();
                                string strfileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".SND";
                                var fileOut = fldrOut.GetFileClient(strfileName);

                                // prepare bytes
                                byte[] contentBytes = Encoding.UTF8.GetBytes(messageBody ?? string.Empty);
                                long contentLength = contentBytes.Length;

                                // create file with required length and upload content (blocking)
                                fileOut.CreateAsync(contentLength).GetAwaiter().GetResult();
                                using (var ms = new MemoryStream(contentBytes))
                                {
                                    fileOut.UploadAsync(ms).GetAwaiter().GetResult();
                                }

                                // backup file
                                var fileBackup = fldrBackup.GetFileClient(DateTime.Now.ToString("yyyyMMdd_HHmm_fff") + ".SND");
                                fileBackup.CreateAsync(contentLength).GetAwaiter().GetResult();
                                using (var ms2 = new MemoryStream(contentBytes))
                                {
                                    fileBackup.UploadAsync(ms2).GetAwaiter().GetResult();
                                }

                                // Preserve original status update logic (note: original code referenced Rows[0] inside loop)
                                if (dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase)
                                    || dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase)
                                    || dtMessagesToSend.Rows[0]["STATUS"].ToString().Length < 1)
                                {
                                    status = "Processed";
                                }

                                if (dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                                {
                                    status = "Re-Processed";
                                }

                                //string[] pname = { "num", "Status" };
                                //object[] pvalue = { int.Parse(dtMessagesToSend.Rows[0]["STATUS"].ToString()), status };
                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };

                                //SQLServer sqlServer = new SQLServer();
                                //if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                //    clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dtMessagesToSend.Rows[0]["STATUS"].ToString());

                                SqlParameter[] parameters =
                                {
                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dtMessagesToSend.Rows[0]["STATUS"].ToString()) },
                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                };
                                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent",parameters);
                                if (dbRes)
                                {
                                    // clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dtMessagesToSend.Rows[0]["STATUS"].ToString());
                                    _logger.LogInformation("uploaded on Azure Share Drive successfully to: {status}" , dtMessagesToSend.Rows[0]["STATUS"].ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception objEx)
                {
                    // clsLog.WriteLogAzure("Failed to upload on Azure Share Drive for:" + objEx.Message.ToString());
                    _logger.LogError(objEx, "Failed to upload on Azure Share Drive for: {message}" , objEx.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in DRIVEUpload");
            }
        }
    }
}
