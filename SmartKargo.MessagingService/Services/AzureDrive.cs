using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.File;
using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using Microsoft.WindowsAzure.StorageClient;

namespace QidWorkerRole
{
    public class AzureDrive
    {
        string storageAccountName = string.Empty;
        string storageAccountKey = string.Empty;
        string shareDriveName = string.Empty;
        string RootFolder = string.Empty;
        string RootFolder_IN = "in";
        string RootFolder_OUT = "out";
        string BackupFolder = "QidMessageBackup";
        SCMExceptionHandlingWorkRole scmeception = new SCMExceptionHandlingWorkRole();
        StorageCredentials objCrd = null;
        CloudStorageAccount storageAccount = null;
        CloudFileClient fileClient = null;
        CloudFileShare shareDrive = null;
        CloudFileDirectory dirDrive = null;
        CloudFileDirectory fldrRroot = null;
        CloudFileDirectory fldrBackup = null;
        CloudFileDirectory fldrOut = null;
        public AzureDrive()
        {

        }

        /// <summary>
        /// Below Method used to Read file from shared Drive and save in the database
        /// </summary>
        public void ReadFromSITADrive()
        {
            try
            {
                clsLog.WriteLogAzure("In ReadFromSITADrive()");
                GenericFunction genericFunction = new GenericFunction();
                storageAccountName = genericFunction.ReadValueFromDb("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
                storageAccountKey = genericFunction.ReadValueFromDb("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
                shareDriveName = genericFunction.ReadValueFromDb("SHARE_DRIVE_VM_DRIVE_NAME");
                RootFolder = genericFunction.ReadValueFromDb("SHARE_DRIVE_VM_ROOT_FOLDER");
                if (storageAccountName != "" && storageAccountKey != "" && shareDriveName != "" && RootFolder != "")
                {
                    StorageCredentials objCrd = new StorageCredentials(storageAccountName, storageAccountKey);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    CloudStorageAccount storageAccount = new CloudStorageAccount(objCrd, true);
                    CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
                    CloudFileShare shareDrive = fileClient.GetShareReference(shareDriveName);
                    if (shareDrive.Exists())
                    {
                        CloudFileDirectory dirDrive = shareDrive.GetRootDirectoryReference();
                        CloudFileDirectory fldrRroot = dirDrive.GetDirectoryReference(RootFolder);
                        CloudFileDirectory fldrBackup = dirDrive.GetDirectoryReference(BackupFolder);
                        CloudFileDirectory fldrIN = fldrRroot.GetDirectoryReference(RootFolder_IN);
                        IEnumerable<IListFileItem> fileList = fldrIN.ListFilesAndDirectories();
                        foreach (var fileItem in fileList)
                        {
                            if (fileItem is CloudFile)
                            {
                                CloudFile fileIn = fileItem as CloudFile;
                                if (fileIn.Exists())
                                {
                                    string strMessage = fileIn.DownloadTextAsync().Result;
                                    CloudFile fileBackup = fldrBackup.GetFileReference(DateTime.Now.ToString("yyyyMMdd_HHmm") + "_" + fileIn.Name);
                                    genericFunction.SaveIncomingMessageInDatabase("MSG:" + fileIn.Name, strMessage, "SITAftp", "", DateTime.Now, DateTime.Now, "SITA", "Active", "SITA");
                                    fileBackup.Create(1);
                                    fileBackup.UploadText(strMessage);
                                    fileIn.Delete();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

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
                GenericFunction genericFunction = new GenericFunction();
                storageAccountName = genericFunction.ReadValueFromDb("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
                storageAccountKey = genericFunction.ReadValueFromDb("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
                shareDriveName = genericFunction.ReadValueFromDb("SHARE_DRIVE_VM_DRIVE_NAME");

                if (folderPath.Trim() == string.Empty)
                    RootFolder = genericFunction.ReadValueFromDb("SHARE_DRIVE_VM_ROOT_FOLDER");
                else
                    RootFolder = folderPath;

                if (storageAccountName != "" && storageAccountKey != "" && shareDriveName != "")
                {
                    objCrd = new StorageCredentials(storageAccountName, storageAccountKey);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    storageAccount = new CloudStorageAccount(objCrd, true);
                    fileClient = storageAccount.CreateCloudFileClient();
                    var blobClient = storageAccount.CreateCloudBlobClient();
                    shareDrive = fileClient.GetShareReference(shareDriveName);
                    if (shareDrive.Exists())
                    {
                        dirDrive = shareDrive.GetRootDirectoryReference();
                        fldrRroot = dirDrive.GetDirectoryReference(RootFolder);
                        fldrOut = fldrRroot.GetDirectoryReference(RootFolder_OUT);
                        fldrBackup = dirDrive.GetDirectoryReference(BackupFolder);
                        string strfileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".SND";
                        CloudFile fileOut = fldrOut.GetFileReference(strfileName);
                        fileOut.Create(1);
                        fileOut.UploadTextAsync(MsgBody);
                        CloudFile fileBackup = fldrBackup.GetFileReference(DateTime.Now.ToString("yyyyMMdd_HHmm_fff") + ".SND");
                        fileBackup.Create(1);
                        fileBackup.UploadTextAsync(MsgBody);
                        flag = true;
                    }
                }

            }
            catch (Exception objEx)
            {
                //scmeception.logexception(ref objEx);
                clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + objEx.Message.ToString());
                flag = false;
            }
            return flag;
        }

        public void DRIVEUpload(System.Data.DataTable dtMessagesToSend)
        {
            try
            {
                string status = "Active";
                try
                {
                    GenericFunction genericFunction = new GenericFunction();
                    storageAccountName = genericFunction.ReadValueFromDb("SHARE_DRIVE_STORAGE_ACCOUNT_NAME");
                    storageAccountKey = genericFunction.ReadValueFromDb("SHARE_DRIVE_STORAGE_ACCOUNT_KEY");
                    shareDriveName = genericFunction.ReadValueFromDb("SHARE_DRIVE_VM_DRIVE_NAME");
                    RootFolder = genericFunction.ReadValueFromDb("SHARE_DRIVE_VM_ROOT_FOLDER");
                    if (storageAccountName != "" && storageAccountKey != "" && shareDriveName != "")
                    {
                        objCrd = new StorageCredentials(storageAccountName, storageAccountKey);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        storageAccount = new CloudStorageAccount(objCrd, true);
                        fileClient = storageAccount.CreateCloudFileClient();
                        var blobClient = storageAccount.CreateCloudBlobClient();
                        shareDrive = fileClient.GetShareReference(shareDriveName);
                        if (shareDrive.Exists())
                        {
                            dirDrive = shareDrive.GetRootDirectoryReference();
                            fldrRroot = dirDrive.GetDirectoryReference(RootFolder);
                            fldrOut = fldrRroot.GetDirectoryReference(RootFolder_OUT);
                            fldrBackup = dirDrive.GetDirectoryReference(BackupFolder);
                            string messageBody = string.Empty;

                            for (int i = 0; i < dtMessagesToSend.Rows.Count; i++)
                            {
                                messageBody = dtMessagesToSend.Rows[i]["body"].ToString();
                                string strfileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff") + ".SND";
                                CloudFile fileOut = fldrOut.GetFileReference(strfileName);
                                fileOut.Create(1);
                                fileOut.UploadTextAsync(messageBody);
                                CloudFile fileBackup = fldrBackup.GetFileReference(DateTime.Now.ToString("yyyyMMdd_HHmm_fff") + ".SND");
                                fileBackup.Create(1);
                                fileBackup.UploadTextAsync(messageBody);

                                if (dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || dtMessagesToSend.Rows[0]["STATUS"].ToString().Length < 1)
                                    status = "Processed";
                                if (dtMessagesToSend.Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                                    status = "Re-Processed";
                                string[] pname = { "num", "Status" };
                                object[] pvalue = { int.Parse(dtMessagesToSend.Rows[0]["STATUS"].ToString()), status };
                                SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                SQLServer sqlServer = new SQLServer();
                                if (sqlServer.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                    clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dtMessagesToSend.Rows[0]["STATUS"].ToString());
                            }
                        }
                    }
                }
                catch (Exception objEx)
                {
                    //scmeception.logexception(ref objEx);
                    clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + objEx.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
    }
}
