using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole.UploadMasters
{
    public class UploadMaster
    {
        /// <summary>
        /// Method to upload master files to blob
        /// </summary>
        internal void UploadMasterFile(string UploadType)
        {
            string ContainerName = string.Empty;
            UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
            try
            {
                if (UploadType == UploadMasterType.RateLine)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.RATELINE, ContainerName);
                }
                else if (UploadType == UploadMasterType.Agent)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.AGENT, ContainerName);
                }
                else if (UploadType == UploadMasterType.ShipperConsignee)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.SHIPPERCONSIGNEE, ContainerName);
                }
                else if (UploadType == UploadMasterType.OtherCharges)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.OTHERCHARGES, ContainerName);
                }
                else if (UploadType == UploadMasterType.FlightCapacity)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.FLIGHTCAPACITY, ContainerName);
                }
                else if (UploadType == UploadMasterType.FlightSchedule)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.SCHEDULEUPLOAD, ContainerName);
                }
                else if (UploadType == UploadMasterType.AgentUpdate)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.AGENTUPDATE, ContainerName);
                }
                else if (UploadType == UploadMasterType.CapacityAllocation)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.CAPACITYALLOCATION, ContainerName);
                }
                else if (UploadType == UploadMasterType.TaxLine)
                {
                    DataSet dsContainerName = uploadMasterCommon.GetUploadMasterConfiguration(UploadType);
                    if (dsContainerName != null && dsContainerName.Tables.Count > 0 && dsContainerName.Tables[0].Rows.Count > 0)
                    {
                        ContainerName = dsContainerName.Tables[0].Rows[0]["ContainerName"].ToString();
                    }
                    UploadMasters(UploadType, MessageData.MessageTypeName.TAXLINE, ContainerName);
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private void UploadMasters(string UploadType, string MessageType, string ContainerName)
        {
            try
            {
                UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                DataSet dsUploadMasterConfiguration = uploadMasterCommon.GetUploadMasterMessageConfiguration(MessageType);
                if (dsUploadMasterConfiguration != null && dsUploadMasterConfiguration.Tables.Count > 0 && dsUploadMasterConfiguration.Tables[0].Rows.Count > 0)
                {
                    string FilePath = dsUploadMasterConfiguration.Tables[0].Rows[0]["FileSharePath"].ToString().Trim();
                    if (FilePath != string.Empty)
                    {
                        if (UploadType == UploadMasterType.AgentUpdate)
                        {
                            GenericFunction genericFunction = new GenericFunction();
                            if (genericFunction.ReadValueFromDb("AgentUpdateDateInterval") == string.Empty || DateTime.ParseExact(genericFunction.ReadValueFromDb("AgentUpdateDateInterval"), genericFunction.ReadValueFromDb("SystemDateFormat"), null) < DateTime.Now)
                            {
                                uploadMasterCommon.AgentUpdateDateInterval(DateTime.Now.AddDays(1).ToString(genericFunction.ReadValueFromDb("SystemDateFormat") + " hh:mm:ss"));
                                Configuration.ConfigurationValues.Remove("AgentUpdateDateInterval");
                                Configuration.ConfigurationValues.Add("AgentUpdateDateInterval", DateTime.Now.AddDays(1).ToString(genericFunction.ReadValueFromDb("SystemDateFormat")));

                                UploadUpdateAgentMasterFileToBlob(UploadType, FilePath, ContainerName);
                            }
                        }
                        else
                        {
                            UploadMasterFileToBlob(UploadType, FilePath, ContainerName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private void UploadMasterFileToBlob(string UploadType, string FilePath, string ContainerName)
        {
            try
            {
                Cls_BL cls_BL = new Cls_BL();
                UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                string FileName = string.Empty;
                GenericFunction genericFunction = new GenericFunction();
                int ProgressStatus = 0;
                int RecordCount = 0;
                int SuccessCount = 0;
                int FailCount = 0;
                bool IsProcessed = false;
                string FolderName = string.Empty;
                string ProcessMethod = string.Empty;
                string ErrorMessage = string.Empty;
                string Status = "Process will start shortly";
                string BlobName = genericFunction.ReadValueFromDb("BlobStorageName");
                foreach (string file in Directory.GetFiles(FilePath))
                {
                    FileName = Path.GetFileNameWithoutExtension(file) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file);
                    FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    genericFunction.UploadMastersToBlob(fileStream, FileName, ContainerName);
                    fileStream.Close();
                    string UserName = string.Empty;
                    DateTime IT = System.DateTime.Now;
                    string Station = string.Empty;
                    DataSet dsSerialNumber = uploadMasterCommon.InsertMasterSummaryLog(0,           FileName,       UploadType,     UserName,       RecordCount,    SuccessCount,
                                                                                       FailCount,   Station,        Status,         ProgressStatus, BlobName,       ContainerName,
                                                                                       FolderName,  ProcessMethod,  ErrorMessage,   IsProcessed);

                    File.Delete(file);

                    if (dsSerialNumber != null && dsSerialNumber.Tables.Count > 0 && dsSerialNumber.Tables[0].Rows.Count > 0)
                    {
                        clsLog.WriteLogAzure("File uploaded successfully");
                    }
                    else
                    {
                        clsLog.WriteLogAzure("File already Uploaded");
                    }
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        private void UploadUpdateAgentMasterFileToBlob(string UploadType, string FilePath, string ContainerName)
        {
            try
            {
                Cls_BL cls_BL = new Cls_BL();
                UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                string FileName = string.Empty;
                GenericFunction genericFunction = new GenericFunction();
                int ProgressStatus = 0;
                int RecordCount = 0;
                int SuccessCount = 0;
                int FailCount = 0;
                bool IsProcessed = false;
                string FolderName = string.Empty;
                string ProcessMethod = string.Empty;
                string ErrorMessage = string.Empty;
                string Status = "Process will start shortly";
                string BlobName = genericFunction.ReadValueFromDb("BlobStorageName");
                foreach (string file in Directory.GetFiles(FilePath, "*.json"))
                {
                    FileName = Path.GetFileNameWithoutExtension(file) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file);
                    FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    genericFunction.UploadMastersToBlob(fileStream, FileName, ContainerName);
                    fileStream.Close();
                    string UserName = string.Empty;
                    DateTime IT = System.DateTime.Now;
                    string Station = string.Empty;
                    DataSet dsSerialNumber = uploadMasterCommon.InsertMasterSummaryLog(0,               FileName,       UploadType, UserName,       RecordCount,
                                                                                       SuccessCount,    FailCount,      Station,    Status,         ProgressStatus,
                                                                                       BlobName,        ContainerName,  FolderName, ProcessMethod,  ErrorMessage,
                                                                                       IsProcessed);

                    File.Delete(file);

                    if (dsSerialNumber != null && dsSerialNumber.Tables.Count > 0 && dsSerialNumber.Tables[0].Rows.Count > 0)
                    {
                        clsLog.WriteLogAzure("File uploaded successfully");
                    }
                    else
                    {
                        clsLog.WriteLogAzure("File already Uploaded");
                    }
                }

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

    }
}
