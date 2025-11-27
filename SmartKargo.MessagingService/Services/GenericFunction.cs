#region Generic Function Class Description
/* GenericFunction Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              :   Generic Function class read text file, make xml sring and Get Record from  database and generate EDI  Message.
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-03-02
      * Approved By          :
      * Approved Date        :
      * Modified By          :  
      * Modified On          :   
      * Description          :   
     */
#endregion

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QidWorkerRole.UploadMasters;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System;
using System.Data;
using System.Net;
using System.Text;
using static QidWorkerRole.SCMExceptionHandlingWorkRole;


//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;
//using Azure.Core;

namespace QidWorkerRole
{
    public class GenericFunction
    {
        //SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();
        // string BlobKey = string.Empty;
        //string BlobName = string.Empty;
        static string BlobKey = string.Empty;
        static string BlobName = string.Empty;

        /// <summary>
        /// Enum created for blob container name(Container name should be in lower case).
        /// Note: While update this enum, make same changes in ProjectMartKargoManager/CommonUtility.cs
        /// </summary>
        public enum ContainerName { schedules, capacity, agentupdate };

        #region Connectionstring
        //string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        #endregion

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<GenericFunction> _logger;
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<GenericFunction>? _staticLogger => _loggerFactory?.CreateLogger<GenericFunction>();
        private readonly Func<UploadMasterCommon> _uploadMasterCommonFactory;



        #region Constructor
        public GenericFunction(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<GenericFunction> logger,
            ILoggerFactory loggerFactory,
            Func<UploadMasterCommon> uploadMasterCommonFactory)

        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _loggerFactory = loggerFactory;
            _uploadMasterCommonFactory = uploadMasterCommonFactory;
        }
        #endregion

        public async Task<bool> SaveMessageToOutbox(string subject, string message, string toEmailID, string sitaMessageHeader, string sftpHeaderSITAddress, string type = ""
            , string awbNumber = "", string flightNumber = "", string flightDate = "", string flightOrigin = "", string flightDestination = "", string carrierCode = ""
            , string error = "", string fromAddress = "", string msgCategory = "")
        {
            bool flag = false;
            try
            {
                if (sitaMessageHeader.Trim().Length > 0 && message.Trim().Length > 3)
                    await SaveMessageOutBox("SITA:" + type.Trim().ToUpper(), sitaMessageHeader.Trim() + "\r\n" + message.Trim(), "", "SITAFTP", flightOrigin, flightDestination, flightNumber, flightDate.ToString(), awbNumber);

                if (sftpHeaderSITAddress.Trim().Length > 0 && message.Trim().Length > 3)
                    await SaveMessageOutBox("SITA:" + type.Trim().ToUpper(), sftpHeaderSITAddress.Trim() + "\r\n" + message.Trim(), "", "SFTP", flightOrigin, flightDestination, flightNumber, flightDate.ToString(), awbNumber);

                if (toEmailID.Trim().Length > 0 && message.Trim().Length > 3)
                    await SaveMessageOutBox(type.Trim().ToUpper(), message.Trim(), "", toEmailID.Trim(), flightOrigin, flightDestination, flightNumber, flightDate.ToString(), awbNumber);

                flag = true;
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        public async Task<bool> SaveMessageOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, string FlightOrigin = "", string FlightDestination = "", string FlightNumber = "", string FlightDate = "", string AWBNo = "")
        {
            bool flag = false;
            try
            {
                //string procedure = "spInsertMsgToOutbox";
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] {
                //                                "Subject",
                //                                "Body",
                //                                "FromEmailID",
                //                                "ToEmailID",
                //                                "CreatedOn",
                //                                 "FlightNumber",
                //                                "FlightOrigin",
                //                                "FlightDestination",
                //                                "FlightDate",
                //                                "AWBNumber"};

                //object[] paramvalue = new object[] {
                //                               subject,
                //                                Msg,
                //                                FromEmailID,
                //                                ToEmailID,
                //                                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //                                FlightNumber,
                //                                FlightOrigin,
                //                                FlightDestination,
                //                                FlightDate,
                //                                AWBNo
                //                                };

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar
                //};
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("FromEmailID", SqlDbType.VarChar) { Value = FromEmailID },
                    new SqlParameter("ToEmailID", SqlDbType.VarChar) { Value = ToEmailID },
                    new SqlParameter("CreatedOn", SqlDbType.DateTime) { Value = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    new SqlParameter("FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin },
                    new SqlParameter("FlightDestination", SqlDbType.VarChar) { Value = FlightDestination },
                    new SqlParameter("FlightDate", SqlDbType.VarChar) { Value = FlightDate },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = AWBNo }
                };


                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
                flag = await _readWriteDao.ExecuteNonQueryAsync("spInsertMsgToOutbox", sqlParameters);
                if (!flag)
                {
                    //clsLog.WriteLogAzure("Error in addMsgToOutBox:" + dtb.LastErrorDescription);
                    // clsLog.WriteLogAzure("Error in addMsgToOutBox");
                    _logger.LogWarning("Error in addMsgToOutBox");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error:" + ex.Message);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        public async Task<bool> SaveMessageOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, string FlightOrigin, string FlightDestination, string FlightNumber, string FlightDate, string AWBNo, string CreatedBy, string Type)
        {
            bool flag = false;
            try
            {
                //string procedure = "spInsertMsgToOutbox";
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] {
                //                                "Subject",
                //                                "Body",
                //                                "FromEmailID",
                //                                "ToEmailID",
                //                                "CreatedOn",
                //                                 "FlightNumber",
                //                                "FlightOrigin",
                //                                "FlightDestination",
                //                                "FlightDate",
                //                                "AWBNumber",
                //                                "CreatedBy",
                //                                "Type"};

                //object[] paramvalue = new object[] {
                //                               subject,
                //                                Msg,
                //                                FromEmailID,
                //                                ToEmailID,
                //                                System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                //                                FlightNumber,
                //                                FlightOrigin,
                //                                FlightDestination,
                //                                FlightDate,
                //                                AWBNo,
                //                                CreatedBy,
                //                                Type
                //                                };

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar
                //};
                SqlParameter[] paramvalues = new SqlParameter[] {
                    new SqlParameter("Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("FromEmailID", SqlDbType.VarChar) { Value = FromEmailID },
                    new SqlParameter("ToEmailID", SqlDbType.VarChar) { Value = ToEmailID },
                    new SqlParameter("CreatedOn", SqlDbType.DateTime) { Value = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                    new SqlParameter("FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin },
                    new SqlParameter("FlightDestination", SqlDbType.VarChar) { Value = FlightDestination },
                    new SqlParameter("FlightDate", SqlDbType.VarChar) { Value = FlightDate },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = AWBNo },
                    new SqlParameter("CreatedBy", SqlDbType.VarChar) { Value = CreatedBy },
                    new SqlParameter("Type", SqlDbType.VarChar) { Value = Type }
                };

                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
                flag = await _readWriteDao.ExecuteNonQueryAsync("spInsertMsgToOutbox", paramvalues);
                if (!flag)
                {
                    //clsLog.WriteLogAzure("Error in addMsgToOutBox:" + dtb.LastErrorDescription);
                    // clsLog.WriteLogAzure("Error in addMsgToOutBox");
                    _logger.LogWarning("Error in addMsgToOutBox");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error:" + ex.Message);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        public async Task<int> DumpInterfaceInformation(string subject, string body, DateTime timeStamp, string messageType, string errorDesc, bool isBlog,
        string fromEmailId, string toEmailId, MemoryStream attachments, string attachmentExtension, string fileUrl, string isProcessed, string messageBoxType, string attachmentName)
        {
            int SerialNo = 0;
            try
            {
                //string procedure = "uspAddMessageAttachmentDetails";
                //SQLServer dtb = new SQLServer();
                DataSet? objDS = null;
                byte[] objBytes = null;

                if (attachments != null)
                    objBytes = attachments.ToArray();

                //string[] paramname = new string[] { "Subject",
                //                                "Body",
                //                                "TimeStamp",
                //                                "MessageType",
                //                                "ErrorDesc",
                //                                "IsBlog",
                //                                "FromId",
                //                                "ToId",
                //                                "Attachment",
                //                                "Extension",
                //                                "FileUrl",
                //                                "isProcessed",
                //                                "MessageBoxType",
                //                                "AttachmentName"
                //                            };

                //object[] paramvalue = new object[] {subject,
                //                                body,
                //                                timeStamp,
                //                                messageType,
                //                                errorDesc,
                //                                isBlog,
                //                                fromEmailId,
                //                                toEmailId,
                //                                objBytes,
                //                                attachmentExtension,
                //                                fileUrl,
                //                                isProcessed,
                //                                messageBoxType,
                //                                attachmentName
                //                            };

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.DateTime,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.Bit,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarBinary,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarChar,
                //                                SqlDbType.VarChar
                //                            };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("Body", SqlDbType.VarChar) { Value = body },
                    new SqlParameter("TimeStamp", SqlDbType.DateTime) { Value = timeStamp },
                    new SqlParameter("MessageType", SqlDbType.VarChar) { Value = messageType },
                    new SqlParameter("ErrorDesc", SqlDbType.VarChar) { Value = errorDesc },
                    new SqlParameter("IsBlog", SqlDbType.Bit) { Value = isBlog },
                    new SqlParameter("FromId", SqlDbType.VarChar) { Value = fromEmailId },
                    new SqlParameter("ToId", SqlDbType.VarChar) { Value = toEmailId },
                    new SqlParameter("Attachment", SqlDbType.VarBinary) { Value = objBytes ?? (object)DBNull.Value },
                    new SqlParameter("Extension", SqlDbType.VarChar) { Value = attachmentExtension },
                    new SqlParameter("FileUrl", SqlDbType.VarChar) { Value = fileUrl },
                    new SqlParameter("isProcessed", SqlDbType.VarChar) { Value = isProcessed },
                    new SqlParameter("MessageBoxType", SqlDbType.VarChar) { Value = messageBoxType },
                    new SqlParameter("AttachmentName", SqlDbType.VarChar) { Value = attachmentName }
                };

                //objDS = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);
                objDS = await _readWriteDao.SelectRecords("uspAddMessageAttachmentDetails", sqlParameters);

                if (objDS != null && objDS.Tables.Count > 0 && objDS.Tables[0].Rows.Count > 0)
                    SerialNo = Convert.ToInt32(objDS.Tables[0].Rows[0][0]);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return SerialNo;
        }

        internal async Task AIMSLoadPlan()
        {
            string fileName = string.Empty, fileURL = string.Empty, emailAddress = string.Empty, subject = string.Empty, body = string.Empty;
            try
            {
                DataSet? dsAIMSLoadPlan = new DataSet();
                //SQLServer ProcessAIMS = new SQLServer();
                //GenericFunction genericFunction = new GenericFunction();

                //dsAIMSLoadPlan = ProcessAIMS.SelectRecords("Messaging.uspProcessAIMS");
                dsAIMSLoadPlan = await _readWriteDao.GetDatasetAsync("Messaging.uspProcessAIMS");
                string partnerCode = string.Empty;
                if (dsAIMSLoadPlan != null && dsAIMSLoadPlan.Tables.Count > 0 && dsAIMSLoadPlan.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsAIMSLoadPlan.Tables[0].Rows.Count; i++)
                    {
                        partnerCode = dsAIMSLoadPlan.Tables[0].Rows[i]["Carrier"].ToString();
                        DataSet dsconfiguration = await GetSitaAddressandMessageVersion(partnerCode, "AIMSLOADPLAN", "AIR", "", "", "", string.Empty);
                        //DataSet dsconfiguration = await _genericFunction.GetSitaAddressandMessageVersion(partnerCode, "AIMSLOADPLAN", "AIR", "", "", "", string.Empty);
                        emailAddress = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        if (emailAddress.Trim() != string.Empty)
                        {
                            MemoryStream loadPlanDetails = new MemoryStream(Encoding.UTF8.GetBytes(""));
                            loadPlanDetails = new MemoryStream(Encoding.UTF8.GetBytes(dsAIMSLoadPlan.Tables[0].Rows[i]["LoadPlanDetails"].ToString()));
                            subject = dsAIMSLoadPlan.Tables[0].Rows[i]["Subject"].ToString();
                            body = dsAIMSLoadPlan.Tables[0].Rows[i]["Body"].ToString();
                            fileName = dsAIMSLoadPlan.Tables[0].Rows[i]["FileName"].ToString();
                            fileURL = UploadToBlob(loadPlanDetails, fileName, "aimsloadplan");
                            await DumpInterfaceInformation(subject, body, System.DateTime.UtcNow, "AIMSLOADPLAN", "", false
                                , "", emailAddress, loadPlanDetails, ".txt", fileURL, "0", "Outbox", fileName.Replace(".TXT", ""));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="Msg"></param>
        /// <param name="FromEmailID"></param>
        /// <param name="ToEmailID"></param>
        /// <param name="agent"></param>
        /// <param name="refNo"></param>
        /// <returns></returns>
        public async Task<bool> SaveMessageOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, string agent, int refNo)
        {
            bool flag = false;
            try
            {
                //string procedure = "spInsertMsgToOutbox";
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] {
                //                                "Subject",
                //                                "Body",
                //                                "FromEmailID",
                //                                "ToEmailID",
                //                                "CreatedOn",
                //                                "Agentcode",
                //                                "refNo"};

                //object[] paramvalue = new object[] {subject,
                //                                Msg,
                //                                FromEmailID,
                //                                ToEmailID,
                //                                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //                                agent,
                //                                refNo};

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.Int};

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("FromEmailID", SqlDbType.VarChar) { Value = FromEmailID },
                    new SqlParameter("ToEmailID", SqlDbType.VarChar) { Value = ToEmailID },
                    new SqlParameter("CreatedOn", SqlDbType.DateTime) { Value = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    new SqlParameter("Agentcode", SqlDbType.VarChar) { Value = agent },
                    new SqlParameter("refNo", SqlDbType.Int) { Value = refNo }

                };

                //flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);
                flag = await _readWriteDao.ExecuteNonQueryAsync("spInsertMsgToOutbox", sqlParameters);
            }
            catch (Exception ex)
            {
                //scm.logexception(ref ex);
                //clsLog.WriteLog("Error:"+ex.Message);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Body"></param>
        /// <param name="FromId"></param>
        /// <param name="ToID"></param>
        /// <returns></returns>
        public async Task<bool> SaveFFRMessageInOutBox(string Body, string FromId, string ToID)
        {
            //string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            //SQLServer dtb = new SQLServer();
            DataSet ds = new DataSet();
            DataSet? objDS = null;
            bool flag = false;
            try
            {
                //string[] Pname = new string[3];
                //object[] Pvalue = new object[3];
                //SqlDbType[] Ptype = new SqlDbType[3];

                //Pname[0] = "body";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = Body;

                //Pname[1] = "fromId";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = FromId;

                //Pname[2] = "toId";
                //Ptype[2] = SqlDbType.VarChar;
                //Pvalue[2] = ToID;

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("body", SqlDbType.VarChar) { Value = Body },
                    new SqlParameter("fromId", SqlDbType.VarChar) { Value = FromId },
                    new SqlParameter("toId", SqlDbType.VarChar) { Value = ToID }
                };

                //objDS = dtb.SelectRecords("spSaveFFRToOutbox", Pname, Pvalue, Ptype);
                objDS = await _readWriteDao.SelectRecords("spSaveFFRToOutbox", sqlParameters);

                if (objDS != null)
                {
                    if (objDS.Tables.Count > 0)
                    {
                        if (objDS.Tables[0].Rows.Count > 0)
                        {
                            flag = true;
                        }
                    }
                }
                //return Convert.ToString(objDS.Tables[0].Rows[0][0]);
                //else
                //    return string.Empty;
            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                flag = false;
            }
            finally
            {
                //dtb = null;
                objDS = null;
                //da = null;
            }
            GC.Collect();
            return flag;
        }

        public async Task AutoForwardEmail(int refno, string Origin)
        {
            try
            {
                DataSet? ds = new DataSet();
                //SQLServer da = new SQLServer();
                //string[] PName = new string[] { "Origin", "InboxID" };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int };
                //object[] PValue = new object[] { Origin, refno };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("Origin", SqlDbType.VarChar) { Value = Origin },
                    new SqlParameter("InboxID", SqlDbType.Int) { Value = refno }
                };

                //da.ExecuteProcedure("spAutoForwardMsgs", PName, PType, PValue);
                await _readWriteDao.ExecuteNonQueryAsync("spAutoForwardMsgs", sqlParameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex.Message); 
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strRecord"></param>
        public async Task<DataSet> GetEdiMessageFormat(string strMessageName, string strCarrierCode, string strProceName)
        {
            //       string strConnectionString = Global.GetConnectionString();
            //SQLServer da = new SQLServer();
            DataSet? ds = new DataSet();
            //string[] Pname = new string[2];
            //object[] Pvalue = new object[2];
            //SqlDbType[] Ptype = new SqlDbType[2];

            try
            {
                //Pname[0] = "MessageName";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = strMessageName;

                //Pname[1] = "CarrierCode";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = strCarrierCode;
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("MessageName", SqlDbType.VarChar) { Value = strMessageName },
                    new SqlParameter("CarrierCode", SqlDbType.VarChar) { Value = strCarrierCode }
                };

                //ds = da.SelectRecords(strProceName, Pname, Pvalue, Ptype);
                ds = await _readWriteDao.SelectRecords(strProceName, sqlParameters);

                return ds;

            }
            catch (Exception ex)
            {
                //scm.logexception(ref ex);
                //clsLog.WriteLogAzure(ex, "BLExpManifest", "GetAwbTabdetails_GHA");
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return ds = null;
            }
            finally
            {
                //da = null;
                if (ds != null)
                    ds.Dispose();
                //Pname = null;
                //Pvalue = null;
                //Ptype = null;
            }
        }

        /// <summary>
        /// Below method is used to check configuration  for Walking Agent
        /// This is AGI Requirenment
        /// Created On:2105-10-13
        /// Created By:Badiuz khan
        /// </summary>
        /// <param name="Appkey"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<DataSet> RemoveAgentTagforWalkingCustomer(string Appkey, string agtParameter)
        {
            //      string strConnectionString = Global.GetConnectionString();
            //SQLServer da = new SQLServer();
            DataSet? ds = new DataSet();
            //string[] Pname = new string[2];
            //object[] Pvalue = new object[2];
            //SqlDbType[] Ptype = new SqlDbType[2];

            try
            {
                //Pname[0] = "Appkey";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = Appkey;

                //Pname[1] = "agtParameter";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = agtParameter;
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("Appkey", SqlDbType.VarChar) { Value = Appkey },
                    new SqlParameter("agtParameter", SqlDbType.VarChar) { Value = agtParameter }
                };

                //ds = da.SelectRecords("RemoveAgentTagforWalkingCustomer", Pname, Pvalue, Ptype);
                ds = await _readWriteDao.SelectRecords("RemoveAgentTagforWalkingCustomer", sqlParameters);

                return ds;

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex, "BLExpManifest", "GetAwbTabdetails_GHA");
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return ds = null;
            }
            finally
            {
                //da = null;
                if (ds != null)
                    ds.Dispose();
                //Pname = null;
                //Pvalue = null;
                //Ptype = null;
            }
        }

        /// <summary>
        /// Below Method is used  get Record for Check Reference TAG Configuration
        /// creadted By:Badiuz khan
        /// Created On:2015-10-09
        /// </summary>
        /// <param name="AWBNumber"></param>
        /// <param name="AWBPrefix"></param>
        /// <returns></returns>
        public async Task<DataSet> GetConfigurationofReferenceTag(string ParameterType, string Appkey, string MessageType)
        {
            DataSet? dsconfig = new DataSet("dConfigmessage");
            try
            {
                //SQLServer da = new SQLServer();
                //string[] PName = new string[] { "ParameterType", "Appkey", "MessageType" };
                //object[] PValue = new object[] { ParameterType, Appkey, MessageType };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("ParameterType", SqlDbType.VarChar) { Value = ParameterType },
                    new SqlParameter("Appkey", SqlDbType.VarChar) { Value = Appkey },
                    new SqlParameter("MessageType", SqlDbType.VarChar) { Value = MessageType }
                };
                //dsconfig = da.SelectRecords("GetConfigurationofReferenceTag", PName, PValue, PType);
                dsconfig = await _readWriteDao.SelectRecords("GetConfigurationofReferenceTag", sqlParameters);
            }
            catch (Exception)
            {
                dsconfig = null;
            }
            return dsconfig;
        }

        /// <summary>
        /// Method used Generate Sita Format Address
        /// </summary>
        /// <param name="sitaAddress"></param>

        /// <returns></returns>

        public string MakeMailMessageFormat(string SitaAddress = "", string OriginSitaAddress = "", string strMessageID = "", string strSitaHeaderType = "", string PIMAAddress = "")
        {
            var strbuilder = new StringBuilder();
            try
            {
                if (strSitaHeaderType == "")
                    strSitaHeaderType = "TYPEB";

                var strsitaAddress = SitaAddress.Split(',');
                if (strSitaHeaderType.ToUpper() == "TYPE1")
                {
                    foreach (var sitaId in strsitaAddress)
                        if (sitaId != "")
                            strbuilder.Append("QN " + sitaId + "\r\n");

                    if (OriginSitaAddress != string.Empty)
                        strbuilder.Append("." + OriginSitaAddress);
                    if (strMessageID.Trim() != string.Empty)
                        strbuilder.Append(" " + strMessageID.Trim() + " " + PIMAAddress.Trim());
                }
                else if (strSitaHeaderType.ToUpper() == "TYPE2")
                {

                    strbuilder.Append("QK");
                    foreach (var sitaId in strsitaAddress)
                    {
                        if (!string.IsNullOrWhiteSpace(sitaId))
                            strbuilder.Append(" " + sitaId.Trim());
                    }
                    if (!string.IsNullOrWhiteSpace(OriginSitaAddress))
                    {
                        var utcNow = DateTime.UtcNow;
                        var ddHHmm = utcNow.ToString("ddHHmm");
                        var yymmddHHmmss = utcNow.ToString("yyMMddHHmmss");
                        strbuilder.Append("\r\n." + OriginSitaAddress.Trim() + " " + ddHHmm + " " + OriginSitaAddress.Trim() + yymmddHHmmss);
                    }
                }
                else
                {
                    string strSitaAddress = string.Empty;
                    strbuilder.Append("=HEADER\r\n");
                    strbuilder.Append("=SND," + String.Format("{0:yyyy/M/d HH:mm}", DateTime.Now) + "\r\n");
                    strbuilder.Append("=PRIORITY\r\n");
                    strbuilder.Append("QK\r\n");
                    strbuilder.Append("=DESTINATION TYPE B\r\n");
                    foreach (var sitaID in strsitaAddress)
                        if (sitaID != "")
                            strbuilder.Append("STX," + sitaID + "\r\n");
                    strbuilder.Append("=ORIGIN\r\n");
                    strbuilder.Append("" + OriginSitaAddress + "\r\n");
                    strbuilder.Append("=MSGID\r\n");
                    if (strMessageID != "")
                        strbuilder.Append("" + strMessageID + "\r\n");
                    else
                        strbuilder.Append("" + String.Format("{0:yyyyMd}", DateTime.Now) + "\r\n");
                    strbuilder.Append("=TEXT");
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error :", ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return strbuilder.ToString();
        }


        public async Task<DataSet> GetSitaAddressandMessageVersion(string PartnerCode = "", string sitaMessage = "", string PartnerType = "", string Origin = "", string Destination = "", string FlightNumber = "", string AgentCode = "", string AWBPrefix = "", bool isAutoSendOnTriggerTime = false, string aircrafttype = "", string flighttype = "", string routetype = "")
        {
            DataSet? dssitaMessage = new DataSet();
            try
            {
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "PartnerCode", "MessageType", "PartnerType", "Origin", "Destination", "FlightNumber", "AgentCode", "AWBPrefix", "IsAutoSendOnTriggerTime","FlightType", "AircraftType", "RouteType" };
                //object[] paramvalue = new object[] { PartnerCode, sitaMessage, PartnerType, Origin, Destination, FlightNumber, AgentCode, AWBPrefix, isAutoSendOnTriggerTime,flighttype,aircrafttype,routetype };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("PartnerCode", SqlDbType.VarChar) { Value = PartnerCode },
                    new SqlParameter("MessageType", SqlDbType.VarChar) { Value = sitaMessage },
                    new SqlParameter("PartnerType", SqlDbType.VarChar) { Value = PartnerType },
                    new SqlParameter("Origin", SqlDbType.VarChar) { Value = Origin },
                    new SqlParameter("Destination", SqlDbType.VarChar) { Value = Destination },
                    new SqlParameter("FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("AgentCode", SqlDbType.VarChar) { Value = AgentCode },
                    new SqlParameter("AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("IsAutoSendOnTriggerTime", SqlDbType.Bit) { Value = isAutoSendOnTriggerTime },
                    new SqlParameter("FlightType", SqlDbType.VarChar) { Value = flighttype },
                    new SqlParameter("AircraftType", SqlDbType.VarChar) { Value = aircrafttype },
                    new SqlParameter("RouteType", SqlDbType.VarChar) { Value = routetype }
                };

                //dssitaMessage = da.SelectRecords("SpGetRecordofSitaAddressandSitaMessageVersion", paramname, paramvalue, paramtype);
                dssitaMessage = await _readWriteDao.SelectRecords("SpGetRecordofSitaAddressandSitaMessageVersion", sqlParameters);
            }
            catch (Exception ex)
            {
                dssitaMessage = null;
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dssitaMessage;
        }

        /// <summary>
        /// Get the recipient address, message version & type details for for the given parameters
        /// </summary>
        /// <returns>DataSet</returns>
        public async Task<DataSet> GetSitaAddressandMessageVersionForAutoMessage(string partnerCode, string messageType, string partnerType, string origin, string destination, string flightNumber, string agentCode, string transitStation, string AWBPrefix, bool isAutoSendOnTriggerTime = false, string flightType = "", string aircraftType = "", string routeType = "")
        {
            DataSet dsSitaMessage = new DataSet();
            try
            {
                bool isAutoGenerate = true;
                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "PartnerCode", "MessageType", "PartnerType", "Origin", "Destination", "FlightNumber", "AgentCode", "TransitStation", "AWBPrefix", "IsAutoSendOnTriggerTime", "FlightType", "AircraftType", "RouteType", "IsAutoGenerate" };
                //object[] paramvalue = new object[] { partnerCode, messageType, partnerType, origin, destination, flightNumber, agentCode, transitStation, AWBPrefix, isAutoSendOnTriggerTime, flightType, aircraftType, routeType, isAutoGenerate };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("PartnerCode", SqlDbType.VarChar) { Value = partnerCode },
                    new SqlParameter("MessageType", SqlDbType.VarChar) { Value = messageType },
                    new SqlParameter("PartnerType", SqlDbType.VarChar) { Value = partnerType },
                    new SqlParameter("Origin", SqlDbType.VarChar) { Value = origin },
                    new SqlParameter("Destination", SqlDbType.VarChar) { Value = destination },
                    new SqlParameter("FlightNumber", SqlDbType.VarChar) { Value = flightNumber },
                    new SqlParameter("AgentCode", SqlDbType.VarChar) { Value = agentCode },
                    new SqlParameter("TransitStation", SqlDbType.VarChar) { Value = transitStation },
                    new SqlParameter("AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("IsAutoSendOnTriggerTime", SqlDbType.Bit) { Value = isAutoSendOnTriggerTime },
                    new SqlParameter("FlightType", SqlDbType.VarChar) { Value = flightType },
                    new SqlParameter("AircraftType", SqlDbType.VarChar) { Value = aircraftType },
                    new SqlParameter("RouteType", SqlDbType.VarChar) { Value = routeType },
                    new SqlParameter("IsAutoGenerate", SqlDbType.Bit) { Value = isAutoGenerate }
                };
                //dsSitaMessage = da.SelectRecords("SpGetRecordofSitaAddressandSitaMessageVersion", paramname, paramvalue, paramtype);
                dsSitaMessage = await _readWriteDao.SelectRecords("SpGetRecordofSitaAddressandSitaMessageVersion", sqlParameters);
            }
            catch (Exception ex)
            {
                dsSitaMessage = null;
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsSitaMessage;
        }

        /*Not in use*/
        //private string GetMessageBodyData(string msg)
        //{
        //    try
        //    {
        //        int indexOfbodyStart = 0;
        //        int indexOfbodyEnd = 0;
        //        if (((msg.Contains("<body") || (msg.Contains("<BODY"))) ||
        //             ((msg.Contains("</body>")) || (msg.Contains("</BODY>")))))
        //        {
        //            if (msg.Contains("<body"))
        //            {
        //                indexOfbodyStart = msg.IndexOf("<body");
        //                indexOfbodyEnd = msg.LastIndexOf("</body>");
        //            }
        //            else
        //            {

        //                indexOfbodyStart = msg.IndexOf("<BODY");
        //                if (msg.Contains("BODY>"))
        //                    indexOfbodyEnd = msg.LastIndexOf("BODY>");
        //                else
        //                    indexOfbodyEnd = msg.LastIndexOf("</BODY>");
        //            }

        //            msg = msg.Substring(indexOfbodyStart, (indexOfbodyEnd - indexOfbodyStart) - 1);
        //            msg = Regex.Replace(msg, @"<(.|\n)*?>", String.Empty);
        //            msg = Regex.Replace(msg, @"\r\n\r\n", Environment.NewLine);
        //            msg = Regex.Replace(msg, @"&nbsp;", String.Empty);
        //            msg = Regex.Replace(msg, @"&amp;", String.Empty);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error :", ex);
        //    }
        //    return msg;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        public string ReadValueFromDb(string Parameter)
        {
            try
            {
                //string ParameterValue = string.Empty;
                //SQLServer da = new SQLServer();
                //string[] QName = new string[] { "PType" };
                //object[] QValues = new object[] { Parameter };
                //SqlDbType[] QType = new SqlDbType[] { SqlDbType.VarChar };
                //ParameterValue = da.GetStringByProcedure("spGetSystemParameter", QName, QValues, QType);
                //if (ParameterValue == null)
                //    ParameterValue = "";
                //da = null;
                //QName = null;
                //QValues = null;
                //QType = null;
                //GC.Collect();
                //return ParameterValue;
                return ConfigCache.Get(Parameter);
            }
            catch (Exception objEx)
            {
                // clsLog.WriteLogAzure(objEx, "General Function", "ReadValueFromDb");
                _logger.LogError(objEx, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return null;
        }

        # region UploadDocumentsOnEpouch
        public async Task<bool> UploadDocumentsOnEpouch(string AWBNo, string DocumentName, string UploadedBy, string DocumentNo, string Extension, byte[] Document, string DocumentFileName, string FileUrl)
        {
            try
            {
                //SQLServer da = new SQLServer();
                //string[] QueryNames = new string[8];
                //object[] QueryValues = new object[8];
                //SqlDbType[] QueryTypes = new SqlDbType[8];

                //QueryNames[0] = "AWBNo";
                //QueryNames[1] = "DocumentName";
                //QueryNames[2] = "UploadedBy";
                //QueryNames[3] = "DocumentNo";
                //QueryNames[4] = "Extension";
                //QueryNames[5] = "Document";
                //QueryNames[6] = "FileName";
                //QueryNames[7] = "FileUrl";

                //QueryTypes[0] = SqlDbType.VarChar;
                //QueryTypes[1] = SqlDbType.VarChar;
                //QueryTypes[2] = SqlDbType.VarChar;
                //QueryTypes[3] = SqlDbType.VarChar;
                //QueryTypes[4] = SqlDbType.VarChar;
                //QueryTypes[5] = SqlDbType.VarBinary;
                //QueryTypes[6] = SqlDbType.VarChar;
                //QueryTypes[7] = SqlDbType.VarChar;

                //QueryValues[0] = AWBNo;
                //QueryValues[1] = DocumentName;
                //QueryValues[2] = UploadedBy;
                //QueryValues[3] = DocumentNo;
                //QueryValues[4] = Extension;
                //QueryValues[5] = Document;
                //QueryValues[6] = DocumentFileName;
                //QueryValues[7] = FileUrl;

                bool flag = false;
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("AWBNo", SqlDbType.VarChar) { Value = AWBNo },
                    new SqlParameter("DocumentName", SqlDbType.VarChar) { Value = DocumentName },
                    new SqlParameter("UploadedBy", SqlDbType.VarChar) { Value = UploadedBy },
                    new SqlParameter("DocumentNo", SqlDbType.VarChar) { Value = DocumentNo },
                    new SqlParameter("Extension", SqlDbType.VarChar) { Value = Extension },
                    new SqlParameter("Document", SqlDbType.VarBinary) { Value = Document },
                    new SqlParameter("FileName", SqlDbType.VarChar) { Value = DocumentFileName },
                    new SqlParameter("FileUrl", SqlDbType.VarChar) { Value = FileUrl }
                };

                //flag = da.InsertData("SP_InsertUploadedDocuments", QueryNames, QueryTypes, QueryValues);
                flag = await _readWriteDao.ExecuteNonQueryAsync("SP_InsertUploadedDocuments", sqlParameters);
                return flag;

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;
        }
        #endregion


        #region Incoming mail to Database
        public async Task<bool> SaveIncomingMessageInDatabase(string subject, string body, string fromId, string toId, DateTime recievedOn, DateTime sendOn, string type, string status, String CommunicationType)
        {
            bool flag = false;
            try
            {
                // clsLog.WriteLogAzure("StoreMail to Db: " + (subject.Trim().Length > 55 ? subject.Substring(0, 50) : subject.Trim()));
                _logger.LogInformation("StoreMail to Db: {Subject}", subject.Length > 55 ? subject.Substring(0, 50) : subject.Trim());

                //SQLServer db = new SQLServer(); ;
                //string[] param = { "subject", "body", "fromId", "toId", "recievedOn", "sendOn", "type", "status", "CommunicationType" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //object[] values = { subject, body, fromId, toId, recievedOn, sendOn, type, status, CommunicationType };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("body", SqlDbType.VarChar) { Value = body },
                    new SqlParameter("fromId", SqlDbType.VarChar) { Value = fromId },
                    new SqlParameter("toId", SqlDbType.VarChar) { Value = toId },
                    new SqlParameter("recievedOn", SqlDbType.DateTime) { Value = recievedOn },
                    new SqlParameter("sendOn", SqlDbType.DateTime) { Value = sendOn },
                    new SqlParameter("type", SqlDbType.VarChar) { Value = type },
                    new SqlParameter("status", SqlDbType.VarChar) { Value = status },
                    new SqlParameter("CommunicationType", SqlDbType.VarChar) { Value = CommunicationType }
                };

                //flag = db.InsertData("spSavetoInbox", param, sqldbtypes, values);
                flag = await _readWriteDao.ExecuteNonQueryAsync("spSavetoInbox", sqlParameters);
                //db = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error : In(StoreEmail) [" + DateTime.Now + "]" + ex.ToString());
                _logger.LogError(ex, "Error : In(StoreEmail) [{DateTime}]", DateTime.Now);
                flag = false;
            }
            return flag;
        }
        #endregion

        #region Update table inbox
        public async Task<bool> UpdateInboxFromMessageParameter(int messageID, string strAWBNumber, string FlightNumber, string FlightOrigin, string FlightDestination, string MessageType, string UpdatedBy, DateTime strFlightDate)
        {
            bool flag = false;
            try
            {
                //SQLServer db = new SQLServer(); ;
                string[] param = { "MessageID", "AWBNumber", "FlightNumber", "FlightOrigin", "FlightDestination", "MessageType", "UpdatedBy", "FlightDate" };
                SqlDbType[] sqldbtypes = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
                object[] values = { messageID, strAWBNumber, FlightNumber, FlightOrigin, FlightDestination, MessageType, UpdatedBy, strFlightDate };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("MessageID", SqlDbType.Int) { Value = messageID },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = strAWBNumber },
                    new SqlParameter("FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin },
                    new SqlParameter("FlightDestination", SqlDbType.VarChar) { Value = FlightDestination },
                    new SqlParameter("MessageType", SqlDbType.VarChar) { Value = MessageType },
                    new SqlParameter("UpdatedBy", SqlDbType.VarChar) { Value = UpdatedBy },
                    new SqlParameter("FlightDate", SqlDbType.DateTime) { Value = strFlightDate }
                };

                //flag = db.InsertData("UpdateInboxFromMessageParameter", param, sqldbtypes, values);
                flag = await _readWriteDao.ExecuteNonQueryAsync("UpdateInboxFromMessageParameter", sqlParameters);

                //db = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error : In(StoreEmail) [" + DateTime.Now + "]" + ex.ToString());
                _logger.LogError(ex, "Error : In(StoreEmail) [{DateTime}]", DateTime.Now);
                flag = false;
            }
            return flag;

        }
        #endregion

        #region RemoveSITAHeader
        public string RemoveSITAHeader(string msg)
        {
            string retstr = "";
            try
            {
                if (msg.StartsWith("=SMI", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("=TEXT", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("ZCZC", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QU", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QP", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QD", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QK", StringComparison.OrdinalIgnoreCase))
                {
                    int n = 0;


                    string[] lines = msg.Split(Environment.NewLine.ToCharArray()).Skip(n).ToArray();
                    lines = lines.Where(str => !str.StartsWith("ZCZC", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith("QU", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith("QD", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith("QK", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith("QP", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith(".", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith("=TEXT", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => !str.StartsWith("=SMI", StringComparison.OrdinalIgnoreCase)).ToArray();
                    lines = lines.Where(str => str != "").ToArray();
                    lines = lines.Where(str => str != "=").ToArray();
                    lines = lines.Where(str => !str.StartsWith("NNNN", StringComparison.OrdinalIgnoreCase)).ToArray();


                    retstr = string.Join(Environment.NewLine, lines);
                }
                else
                {
                    retstr = msg;
                }
            }
            catch (Exception)
            {
                retstr = msg;
            }
            return retstr;
        }
        #endregion

        #region Function to select Data between Start & end string
        public string ExtractFromString(string text, string start, string end)
        {
            List<string> Matched = new List<string>();
            int index_start = 0, index_end = 0;
            bool exit = false;
            while (!exit)
            {
                index_start = text.IndexOf(start);
                if (end == "" && end.Length < 1)
                    index_end = text.Length;//- index_start;
                else
                    index_end = text.IndexOf(end);

                if (index_start != -1 && index_end != -1)
                {
                    Matched.Add(text.Substring(index_start + start.Length, index_end - index_start - start.Length));
                    text = text.Substring(index_end + end.Length);
                }
                else
                    exit = true;
            }
            return start + Matched[0] + Environment.NewLine + end;
        }

        #endregion

        #region ReplaceBlankSpaces
        public string ReplaceBlankSpaces(string Message)
        {
            string val = Message;
            try
            {
                string[] lines = Message.Split(' ');
                // Message = String.Join(Environment.NewLine, Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                // Message = Regex.Replace(Message, @"\s+", Environment.NewLine);
                for (int i = 0; i < lines.Length; i++)
                {
                    //if (lines[i].Contains('-'))
                    //{
                    //    int k = lines[i].IndexOf('-');
                    //    if (k <= 4)
                    //    {
                    //        lines[i] = Environment.NewLine + lines[i];
                    //    }
                    //}
                    if (lines[i].StartsWith("ULD/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("OSI/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("DIM/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("SSR/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("SCI/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("COR/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("OCI/", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = Environment.NewLine + lines[i];
                    }

                }
                Message = string.Join(" ", lines);
            }
            catch (Exception)
            {
                Message = val;
            }
            return Message;
        }
        #endregion

        #region Reemove special character
        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion

        public string GetStorageKey()
        {
            //String Key = "NUro8/C7+kMqtwOwLbe6agUvA83s+8xSTBqrkMwSjPP6MAxVkdtsLDGjyfyEqQIPv6JHEEf5F5s4a+DFPsSQfg==";

            if (string.IsNullOrEmpty(BlobKey))
            {
                //BlobKey = ReadValueFromDb("BlobStorageKey");
                BlobKey = ConfigCache.Get("BlobStorageKey");

            }

            return BlobKey;
        }

        public string GetStorageName()
        {

            if (string.IsNullOrEmpty(BlobName))
            {
                //BlobName = ReadValueFromDb("BlobStorageName");
                BlobName = ConfigCache.Get("BlobStorageName");
            }

            return BlobName;
        }

        //public async Task<bool> IsFileExistOnBlob(string filename, string containerName)
        //{
        //    try
        //    {
        //        containerName = containerName.ToLower();
        //        //byte[] downloadStream = null;

        //        //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(GetStorageName(), GetStorageKey());
        //        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        //CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        //CloudBlob blob = blobClient.GetBlobReference(string.Format("{0}/{1}", containerName, filename));
        //        //downloadStream = blob.DownloadByteArray();

        //        // Set TLS 1.2 (still recommended for compatibility)
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        // Use connection string or account name + key
        //        string storageAccountName = getStorageName();
        //        string storageKey = getStorageKey();

        //        // Preferred: Use shared key credential
        //        string accountUrl = $"https://{storageAccountName}.blob.core.windows.net";
        //        var blobServiceClient = new BlobServiceClient(
        //            new Uri(accountUrl),
        //            new StorageSharedKeyCredential(storageAccountName, storageKey)
        //        );
        //        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        //        // Try with extracted container + blob name
        //        BlobClient blobClient = containerClient.GetBlobClient(filename);

        //        return await blobClient.ExistsAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        // //clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        //        return false;
        //    }

        //}

        public async Task<bool> UpdateInboxFromMessageParameter(int messageID, string strAWBNumber, string FlightNumber, string FlightOrigin, string FlightDestination, string MessageType, string UpdatedBy, DateTime strFlightDate, bool IsNilFFM, string FFMFinalStatus, string strMsg = "", int ffmSequenceNo = 1)
        {
            bool flag = false;
            try
            {
                //SQLServer db = new SQLServer(); ;
                //string[] param = { "MessageID", "AWBNumber", "FlightNumber", "FlightOrigin", "FlightDestination", "MessageType", "UpdatedBy", "FlightDate", "IsNilFFM", "FFMFinalStatus", "MessageBody", "FFMSequenceNo" };
                //SqlDbType[] sqldbtypes = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int };
                //object[] values = { messageID, strAWBNumber, FlightNumber, FlightOrigin, FlightDestination, MessageType, UpdatedBy, strFlightDate, IsNilFFM, FFMFinalStatus, strMsg, ffmSequenceNo };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("MessageID", SqlDbType.Int) { Value = messageID },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = strAWBNumber },
                    new SqlParameter("FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin },
                    new SqlParameter("FlightDestination", SqlDbType.VarChar) { Value = FlightDestination },
                    new SqlParameter("MessageType", SqlDbType.VarChar) { Value = MessageType },
                    new SqlParameter("UpdatedBy", SqlDbType.VarChar) { Value = UpdatedBy },
                    new SqlParameter("FlightDate", SqlDbType.DateTime) { Value = strFlightDate },
                    new SqlParameter("IsNilFFM", SqlDbType.Bit) { Value = IsNilFFM },
                    new SqlParameter("FFMFinalStatus", SqlDbType.VarChar) { Value = FFMFinalStatus },
                    new SqlParameter("MessageBody", SqlDbType.VarChar) { Value = strMsg },
                    new SqlParameter("FFMSequenceNo", SqlDbType.Int) { Value = ffmSequenceNo }
                };

                //flag = db.InsertData("UpdateInboxFromMessageParameter", param, sqldbtypes, values);
                flag = await _readWriteDao.ExecuteNonQueryAsync("UpdateInboxFromMessageParameter", sqlParameters);
                //db = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error : In(StoreEmail) [" + DateTime.Now + "]" + ex.ToString());
                _logger.LogError(ex, "Error : In(StoreEmail) [{DateTime}]", DateTime.Now);
                flag = false;
            }
            return flag;

        }

        public string UploadToBlob(Stream stream, string fileName, string containerName, string filePathToUpload = "")
        {
            try
            {
                containerName = containerName.ToLower();

                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(GetStorageName(), GetStorageKey());
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
                //CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
                //blobContainer.CreateIfNotExist();
                //CloudBlob blob = blobContainer.GetBlobReference(fileName);

                //blobClient.Properties.ContentType = "";
                //clsLog.WriteLogAzure(filePathToUpload);
                //if (filePathToUpload != "" && (filePathToUpload.ToUpper().Contains(".XLS") || filePathToUpload.ToUpper().Contains(".XLSX")))
                //{
                //    clsLog.WriteLogAzure(filePathToUpload);
                //    //blob.UploadFile(filePathToUpload);
                //    blobClient.Upload(filePathToUpload);
                //}
                //else
                //{
                //    //blob.UploadFromStream(stream);
                //    blobClient.UploadAsync(stream);
                //}

                //return "https://" + GetStorageName() + ".blob.core.windows.net/" + containerName + "/" + fileName;

                // Set TLS 1.2 (still recommended for compatibility)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Use connection string or account name + key
                string storageAccountName = getStorageName();
                string storageKey = getStorageKey();

                // Preferred: Use shared key credential
                string accountUrl = $"https://{storageAccountName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new StorageSharedKeyCredential(storageAccountName, storageKey)
                );
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Try with extracted container + blob name
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                // Set ContentType = "" (explicitly empty)
                blobClient.SetHttpHeaders(new BlobHttpHeaders { ContentType = "" });

                // Log file path
                // clsLog.WriteLogAzure(filePathToUpload);
                _logger.LogInformation(filePathToUpload);

                // Upload: either from file (XLS/XLSX) or stream
                if (!string.IsNullOrEmpty(filePathToUpload) &&
                    (filePathToUpload.ToUpper().Contains(".XLS") || filePathToUpload.ToUpper().Contains(".XLSX")))
                {
                    // clsLog.WriteLogAzure(filePathToUpload);
                    _logger.LogInformation(filePathToUpload);
                    blobClient.Upload(filePathToUpload, overwrite: true); // UploadFile → Upload with overwrite
                }
                else
                {
                    stream.Position = 0; // Ensure stream is at start
                    blobClient.Upload(stream, overwrite: true);   // UploadFromStream → Upload
                }

                // Return public blob URL
                return $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }

        }

        /// <summary>
        /// To insert Master Upload Summary Log data.
        /// </summary>
        /// <param name="SrNo"></param>
        /// <param name="FileName"></param>
        /// <param name="MasterType"></param>
        /// <param name="UploadedBy"></param>
        /// <param name="RecordCount"></param>
        /// <param name="SuccessCount"></param>
        /// <param name="FailedCount"></param>
        /// <param name="Station"></param>
        /// <param name="Status"></param>
        /// <param name="ProgressStatus"></param>
        /// <param name="BolbName"></param>
        /// <param name="ContainerName"></param>
        /// <param name="FolderName"></param>
        /// <param name="ProcessMethod"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="IsProcessed"></param>
        /// <returns></returns>
        public async Task<DataSet> InsertMasterSummaryLog(int SrNo, string FileName, string MasterType, string UploadedBy, int RecordCount,
                                              int SuccessCount, int FailedCount, string Station, string Status, int ProgressStatus,
                                              string BolbName, string ContainerName, string FolderName, string ProcessMethod, string ErrorMessage,
                                              bool IsProcessed)
        {
            DataSet? dataSetResult = new DataSet();
            //SQLServer sqlServer = new SQLServer();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@SrNo", SrNo),
                                                                   new SqlParameter("@FileName", FileName),
                                                                   new SqlParameter("@MasterType", MasterType),
                                                                   new SqlParameter("@UploadedBy", UploadedBy),
                                                                   new SqlParameter("@RecordCount", RecordCount),
                                                                   new SqlParameter("@SuccessCount", SuccessCount),
                                                                   new SqlParameter("@FailedCount", FailedCount),
                                                                   new SqlParameter("@Station", Station),
                                                                   new SqlParameter("@Status", Status),
                                                                   new SqlParameter("@ProgressStatus", ProgressStatus),
                                                                   new SqlParameter("@BolbName", BolbName),
                                                                   new SqlParameter("@ContainerName", ContainerName),
                                                                   new SqlParameter("@FolderName", FolderName),
                                                                   new SqlParameter("@ProcessMethod", ProcessMethod),
                                                                   new SqlParameter("@ErrorMessage", ErrorMessage),
                                                                   new SqlParameter("@IsProcessed", IsProcessed)
                                                                 };

                //dataSetResult = sqlServer.SelectRecords("Masters.uspInsertMasterUploadSummaryStatusLog", sqlParameter);
                dataSetResult = await _readWriteDao.SelectRecords("Masters.uspInsertMasterUploadSummaryStatusLog", sqlParameter);
            }
            catch (Exception exception)
            {
                // clsLog.WriteLogAzure(exception);
                _logger.LogError(exception, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dataSetResult;
        }

        //public bool MoveAllFilesToBlob(string sourcePath, string containerName)
        //{
        //    bool IsMoveSuccess = false;
        //    try
        //    {
        //        //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(GetStorageName(), GetStorageKey());
        //        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        //CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        //CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
        //        //blobContainer.CreateIfNotExist();

        //        //foreach (var srcPath in Directory.GetFiles(sourcePath))
        //        //{
        //        //    using (var fileStream = System.IO.File.OpenRead(srcPath))
        //        //    {
        //        //        int LastIndex = srcPath.LastIndexOf("\\");
        //        //        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(srcPath.Substring(LastIndex + 1));
        //        //        blockBlob.UploadFromStream(fileStream);
        //        //    }
        //        //}
        //        //IsMoveSuccess = true;

        //        // Set TLS 1.2 for compatibility
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        string storageAccountName = GetStorageName();
        //        string storageKey = GetStorageKey();

        //        // Create BlobServiceClient with account key
        //        string accountUrl = $"https://{storageAccountName}.blob.core.windows.net";
        //        var blobServiceClient = new BlobServiceClient(
        //            new Uri(accountUrl),
        //            new StorageSharedKeyCredential(storageAccountName, storageKey)
        //        );

        //        // Get container and ensure it exists
        //        BlobContainerClient blobContainer = blobServiceClient.GetBlobContainerClient(containerName);
        //        blobContainer.CreateIfNotExists();

        //        foreach (var srcPath in Directory.GetFiles(sourcePath))
        //        {
        //            using (var fileStream = File.OpenRead(srcPath))
        //            {
        //                int LastIndex = srcPath.LastIndexOf("\\");
        //                string fileName = srcPath.Substring(LastIndex + 1);

        //                BlobClient blobClient = blobContainer.GetBlobClient(fileName);

        //                // Upload from stream
        //                fileStream.Position = 0;
        //                blobClient.Upload(fileStream, overwrite: true);
        //            }
        //        }

        //        IsMoveSuccess = true;

        //    }
        //    catch (Exception ex)
        //    {
        //        IsMoveSuccess = false;
        //        // //clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        //    }
        //    return IsMoveSuccess;
        //}

        public string UploadMastersToBlob(Stream stream, string fileName, string containerName)
        {
            try
            {
                //containerName = containerName.ToLower();

                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(GetStorageName(), GetStorageKey());
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
                //CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
                //blobContainer.CreateIfNotExist();
                //CloudBlob blob = blobContainer.GetBlobReference(fileName);
                //BlobContainerPermissions containerPermissions = new BlobContainerPermissions();
                //containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                //blobContainer.SetPermissions(containerPermissions);
                //blob.Properties.ContentType = "";
                //blob.UploadFromStream(stream);

                //return "https://" + GetStorageName() + ".blob.core.windows.net/" + containerName + "/" + fileName;

                containerName = containerName.ToLower();

                // Set TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Create BlobServiceClient with shared key
                string storageName = GetStorageName();
                string storageKey = GetStorageKey();
                string accountUrl = $"https://{storageName}.blob.core.windows.net";

                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new Azure.Storage.StorageSharedKeyCredential(storageName, storageKey)
                );

                // Get container and create if not exists
                BlobContainerClient blobContainer = blobServiceClient.GetBlobContainerClient(containerName);
                blobContainer.CreateIfNotExists();

                // Set container to public (full read access)
                blobContainer.SetAccessPolicy(accessType: PublicAccessType.BlobContainer);

                // Get blob client
                BlobClient blob = blobContainer.GetBlobClient(fileName);

                // Set ContentType = "" (explicitly empty)
                blob.SetHttpHeaders(new BlobHttpHeaders { ContentType = "" });

                // Upload from stream
                stream.Position = 0;
                blob.Upload(stream, overwrite: true);

                // Return public blob URL
                //return blob.Uri.ToString();
                return $"https://{storageName}.blob.core.windows.net/{containerName}/{fileName}";
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex.Message + " " + ex.StackTrace);
                _logger.LogError($"{ex.Message} {ex.StackTrace}", $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }

        }

        /// <summary>
        /// Method to update the Error/Warning message to inbox table
        /// </summary>
        /// <param name="RefNo"></param>
        /// <param name="ErrorMsg"></param>
        /// <param name="MessageType"></param>
        /// <param name="IsASMSSM"></param>
        /// <param name="MessageBody"></param>
        /// <param name="ErrorWarning">Error(true)/Warning(false)</param>
        public async Task UpdateErrorMessageToInbox(int RefNo, string ErrorMsg, string MessageType = "", bool IsASMSSM = false, string MessageBody = "", bool ErrorWarning = true)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[]{
                    new SqlParameter("@ErrorMessage",ErrorMsg)
                    , new SqlParameter("@MessageSNo",RefNo)
                    , new SqlParameter("@Type", MessageType)
                    , new SqlParameter("@IsASMSSM",IsASMSSM)
                    , new SqlParameter("@MessageBody", MessageBody)
                    , new SqlParameter ("@ErrorWarning", ErrorWarning)
                };
                //sqlServer.SelectRecords("spUpdateStatusformessage", sqlParameter);
                await _readWriteDao.SelectRecords("spUpdateStatusformessage", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                _logger.LogError($"Message: {ex.Message} StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Returning the configuration value from input parameter name
        /// </summary>
        /// <param name="Parameter">Configuration Key Name</param>
        /// <returns>Configuration Key Value</returns>
        //public string ConfigCache.Get(string Parameter)
        //{
        //    try
        //    {
        //        string value = string.Empty;
        //        if (Configuration.ConfigurationValues.TryGetValue(Parameter, out value))
        //        {
        //            return value;
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //    return null;

        //}

        #region GetFBLData
        public async Task<DataSet> GetRecordforGenerateFBLMessage(string strFlightOrigin, string strFlightDestination, string FlightNo, string FlightDate)
        {

            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer(true);
                //string procedure = "spGetFBLDataForSend";

                //string[] paramname = new string[] { "FlightNo", "FlightOrigin", "FlightDestination", "FltDate" };

                //object[] paramvalue = new object[] { FlightNo, strFlightOrigin, strFlightDestination, FlightDate };

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                    new SqlParameter("FlightDestination", SqlDbType.VarChar) { Value = strFlightDestination },
                    new SqlParameter("FltDate", SqlDbType.DateTime) { Value = FlightDate }
                };

                //dsData = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);
                dsData = await _readWriteDao.SelectRecords("spGetFBLDataForSend", sqlParameters);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex.Message); 
                _logger.LogError(ex, "Error in GetRecordforGenerateFBLMessage");
            }

            return dsData;
        }
        #endregion

        #region DATASET HELPER
        private static bool ColumnEqual(object A, object B)
        {
            // Compares two values to see if they are equal. Also compares DBNULL.Value.           
            if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
                return true;
            if (A == DBNull.Value || B == DBNull.Value) //  only one is BNull.Value
                return false;
            return (A.Equals(B));  // value type standard comparison
        }
        public static DataTable SelectDistinct(DataTable SourceTable, string FieldName)
        {
            // Create a Datatable â€“ datatype same as FieldName
            DataTable dt = new DataTable(SourceTable.TableName);
            dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);
            // Loop each row & compare each value with one another
            // Add it to datatable if the values are mismatch
            object LastValue = null;
            foreach (DataRow dr in SourceTable.Select("", FieldName))
            {
                if (LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])))
                {
                    LastValue = dr[FieldName];
                    dt.Rows.Add(new object[] { LastValue });
                }
            }
            return dt;
        }
        #endregion

        /// <summary>
        /// Get XML format for specified message type
        /// </summary>
        /// <param name="ListType"></param>
        /// <returns></returns>
        public async Task<DataSet?> GetXMLMessageData(string ListType)
        {

            //SQLServer da = new SQLServer();
            DataSet ds = new DataSet();
            SqlParameter[] sqlParameter = new SqlParameter[] {
                      new SqlParameter("@ListType",ListType)
             };
            //return da.SelectRecords("uspGetXMLMessageData", sqlParameter);
            return await _readWriteDao.SelectRecords("uspGetXMLMessageData", sqlParameter);

        }

        /// <summary>
        /// Update IsSent to 1 in AWB Status Message
        /// </summary>
        /// <param name="TID"></param>
        public async Task updateAWBStatusMSG(string TID)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@TID", TID) };
                //sqlServer.SelectRecords("uspUpdateIsSentInAWBStatusMsg", sqlParameter);
                await _readWriteDao.SelectRecords("uspUpdateIsSentInAWBStatusMsg", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                _logger.LogError($"Message: {ex.Message} StackTrace: {ex.StackTrace}", $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task<DataSet?> GetStationCodeforOMDAC()
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                return await _readWriteDao.SelectRecords("Messaging.uspGetStationCodeforOMDAC");
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        /// <summary>
        /// Method to get the private key file path from the local machine
        /// </summary>
        /// <param name="ppkFileName">Private key file name</param>
        /// <returns>Loacal file path</returns>
        public string GetPPKFilePath(string ppkFileName)
        {
            string ppkLocalFilePath = string.Empty;
            //GenericFunction genericFunction = new GenericFunction();
            try
            {
                if (!File.Exists(System.IO.Path.GetFullPath(ConfigCache.Get("DownLoadFilePath") + "/" + ConfigCache.Get("PPKFileFolderName") + "/" + ppkFileName)))
                {
                    //UploadMasterCommon uploadMasterCommon = new UploadMasterCommon();
                    if (!_uploadMasterCommonFactory().DoDownloadBLOB(ppkFileName, "ppkfiles", "PPKFiles", out ppkLocalFilePath))
                    {
                        ppkLocalFilePath = string.Empty;
                    }
                }
                else
                {
                    ppkLocalFilePath = Path.GetFullPath(ConfigCache.Get("DownLoadFilePath") + "/" + ConfigCache.Get("PPKFileFolderName") + "/" + ppkFileName);
                }
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return ppkLocalFilePath;
        }

        internal async Task<DataSet> GetAWBMasterLogNewRecord(string awbPrefix, string awbNumber)
        {
            DataSet? dsResult = new DataSet();
            //SQLServer sqlServer = new SQLServer(true);
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] {
                    new SqlParameter("@AWBPrefix",awbPrefix )
                    , new SqlParameter("@AWBNumber",awbNumber )
                };

                //dsResult = sqlServer.SelectRecords("uspGetBookingMasterAuditLog", sqlParameter);
                dsResult = await _readWriteDao.SelectRecords("uspGetBookingMasterAuditLog", sqlParameter);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("FindLog Error in GetAWBMasterLogNewRecord 9991 - ");
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _logger.LogError(ex, "FindLog Error in GetAWBMasterLogNewRecord 9991 - ");
                dsResult = null;
            }
            return dsResult;
        }

        internal async Task MasterAuditLog(DataTable dtOldValues, DataTable dtNewValues, string awbPrefix, string awbNumber, string action, string updatedBy, DateTime updatedOn)
        {
            ///Add paramerer
            try
            {

                string[] BookingSummaryDetails = { "DocumentType", "AWBPrefix", "AWBNumber", "PiecesCount", "GrossWeight", "ChargedWeight", "VolumetricWeight", "OriginCode"
                    , "DestinationCode", "AgentCode", "AgentName", "ServiceCargoClassId", "HandlingInfo", "ExecutionDate", "ExecutedBy", "ExecutedAt", "IsConsole"
                    , "Remarks", "Customs", "DVCustom", "DVCarriage", "SHCCodes", "ProductType", "DesigCode", "CustomerCode", "ShippingAWB", "Documents", "ShipmentDate"
                    , "UOM", "PackagingInfo", "AdditionalInfo", "Location", "ShippingAgentCode", "ShippingAgentName", "AWBAccPcs", "AWBAccWt", "AcceptedBy", "AcceptedDate"
                    , "ViaMobile", "Volume", "VolumeUnit", "VolumetricExemp", "AWBRoutingInfo", "ShipperName", "ShipperAddress", "ShipperCountry", "ShipperTelephone"
                    , "ConsigneeName","ConsigneeAddress", "ConsigneeCountry", "ConsigneeTelephone", "ShipAdd2", "ShipCity", "ShipState", "ShipPincode", "ConsigAdd2"
                    , "ConsigCity", "ConsigState", "ConsigPincode", "ShipperAccCode","ConsigAccCode","ShipperEmailId","ConsigEmailId", "Dimensions", "CommCode", "PayMode"
                    , "FrIATA", "FrMKT", "ValCharge", "OCDueCar","OCDueAgent", "SpotRate", "ServiceTax", "Total", "RatePerKg","SpotRateId", "RateClass","Currency"
                    , "SpotFreight","IATATax","MKTTax","OCTax","OATax","SpotTax","Commission","CommTax","Discount","CommPercent","SpotStatus","IATARateID","MKTRateID"
                    , "TotalSpot","IATATotal","MKTTotal","AppliedRateType","ApprovalStatus","SpotTotalTax","LoadableVol","Stackable"};

                MasterSummary objMaster = new MasterSummary();
                objMaster.Action = action;
                if (action.ToUpper() == "SAVE")
                {
                    if (dtOldValues != null && updatedBy == "FWB")
                    {
                        objMaster.Description = "Executed";
                        objMaster.Message = "AWB Executed";
                    }
                    else if (dtOldValues != null)
                    {
                        objMaster.Description = "Save";
                        objMaster.Message = "Booking Updated";
                    }
                    else
                    {
                        objMaster.Description = "Save";
                        objMaster.Message = "Booking Created.";
                    }
                }
                else if (action.ToUpper() == "ACCEPTED")
                {
                    if (dtOldValues != null && Convert.ToString(dtOldValues.Rows[0]["AWBAccPcs"]) != "0")
                    {
                        objMaster.Description = "Accepted";
                        objMaster.Message = "Acceptance Updated";
                    }
                    else
                    {
                        objMaster.Description = "Accepted";
                        objMaster.Message = "Booking Accepted";
                    }
                }
                else if (action.ToUpper() == "UPDATE")
                {
                    objMaster.Description = "Updated";
                    objMaster.Message = "AWB Updated";
                }
                objMaster.MasterKey = "BookingMaster";
                objMaster.MasterValue = awbPrefix + "-" + awbNumber;
                objMaster.SerialNumber = Guid.NewGuid().ToString();
                objMaster.UpdatedBy = updatedBy;
                objMaster.UpdatedOn = updatedOn;

                objMaster.MasterDetails = GetMasterDetailsXml(dtOldValues, BookingSummaryDetails, dtNewValues, objMaster.MasterKey);

                //GenericFunction genericFunction = new GenericFunction();
                await SaveLog(LogType.MasterSummary, objMaster.MasterKey, objMaster.MasterValue, objMaster);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("FindLog Error in MasterAuditLog 9992 - ");
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _logger.LogError(ex, "FindLog Error in MasterAuditLog 9992 - ");
            }
        }

        private string GetMasterDetailsXml(DataTable dtOldValues, string[] MasterFields, DataTable dtNewValues, string masterKey)
        {
            DataSet dsMasterDetails = new DataSet(masterKey);
            DataTable dtMasterDetails = new DataTable(masterKey + "Details");
            DataRow drMasterDetails = null;
            dtMasterDetails.Columns.Add("FieldName");
            dtMasterDetails.Columns.Add("OldValue");
            dtMasterDetails.Columns.Add("NewValue");

            if (dtOldValues == null)
            {
                for (int count = 0; count < MasterFields.Length; count++)
                {
                    drMasterDetails = dtMasterDetails.NewRow();
                    drMasterDetails["FieldName"] = MasterFields[count];
                    drMasterDetails["OldValue"] = String.Empty;
                    // New Value

                    try
                    {
                        if (dtNewValues.Rows[0][MasterFields[count]] is DateTime)
                            drMasterDetails["NewValue"] = Convert.ToDateTime(Convert.ToString(dtNewValues.Rows[0][MasterFields[count]])).ToString(ConfigCache.Get("DateFormat"));
                        else
                            drMasterDetails["NewValue"] = Convert.ToString(dtNewValues.Rows[0][MasterFields[count]]);
                    }
                    catch (Exception ex)
                    {
                        // //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    dtMasterDetails.Rows.Add(drMasterDetails);
                }
            }
            else
            {
                for (int count = 0; count < MasterFields.Length; count++)
                {
                    drMasterDetails = dtMasterDetails.NewRow();
                    drMasterDetails["FieldName"] = MasterFields[count];
                    // Old Value
                    try
                    {
                        if (dtOldValues.Rows[0][MasterFields[count]] is DateTime)
                            drMasterDetails["OldValue"] = Convert.ToDateTime(dtOldValues.Rows[0][MasterFields[count]]).ToString(ConfigCache.Get("DateFormat"));
                        else
                            drMasterDetails["OldValue"] = Convert.ToString(dtOldValues.Rows[0][MasterFields[count]]);
                    }
                    catch (Exception ex)
                    {
                        // //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    // New Value
                    try
                    {
                        if (dtOldValues.Rows[0][MasterFields[count]] is DateTime)
                            drMasterDetails["NewValue"] = Convert.ToDateTime(Convert.ToString(dtNewValues.Rows[0][MasterFields[count]])).ToString(ConfigCache.Get("DateFormat"));
                        else
                            drMasterDetails["NewValue"] = Convert.ToString(Convert.ToString(dtNewValues.Rows[0][MasterFields[count]]));
                    }
                    catch (Exception ex)
                    {
                        // //clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    dtMasterDetails.Rows.Add(drMasterDetails);
                }
            }
            dsMasterDetails.Tables.Add(dtMasterDetails);
            return dsMasterDetails.GetXml();
        }

        public async Task<bool> SaveLog(LogType logType, string KeyType, string KeyValue, Object objEntity)
        {
            switch (logType.ToString())
            {
                //case "AWBOperations":
                //    return SaveLog_AWBOperations(logType.ToString(), KeyType, KeyValue, (AWBOperations)objEntity);
                case "MasterSummary":
                    return await SaveLog_MasterSummary(logType.ToString(), KeyType, KeyValue, (MasterSummary)objEntity);
                //case "MasterDetails":
                //    return SaveLog_MasterDetails(logType.ToString(), KeyType, KeyValue, (MasterDetails)objEntity);
                //case "Report":
                //    return SaveLog_Report(logType.ToString(), KeyType, KeyValue, (Report)objEntity);
                //case "UserAcces":
                //    return SaveLog_UserAccess(logType.ToString(), KeyType, KeyValue, (UserAccess)objEntity);
                //case "ULD":
                //    return SaveLog_ULDNumber(logType.ToString(), KeyType, KeyValue, (ULDNumber)objEntity);
                //case "InMessage":
                //    return SaveLog_IncomingingMessages(logType.ToString(), KeyType, KeyValue, (InMessage)objEntity);
                //case "OutMessage":
                //    return SaveLog_OutgoingMessages(logType.ToString(), KeyType, KeyValue, (OutBox)objEntity);
                //case "ULDOperations":
                //    return SaveLog_ULDOperations(logType.ToString(), KeyType, KeyValue, (ULDOperations)objEntity);
                //case "BillingAudit":
                //    return SaveLog_BillingAudit(logType.ToString(), KeyType, KeyValue, (BillingAudit)objEntity);
                //case "VendorAudit":
                //    return SaveLog_VendorAudit(logType.ToString(), KeyType, KeyValue, (VendorAudit)objEntity);
                default:
                    break;
            }

            return (true);
        }

        private async Task<bool> SaveLog_MasterSummary(string LogType, string KeyType, string KeyValue, MasterSummary objMasterSummary)
        {
            try
            {
                string StorageType = "sqltable";
                //if (StorageType == "azuretable")
                //{
                //    //Set Keys and TimeStamp for Audit Log Entity.
                //    objMasterSummary.PartitionKey = "PK_" + objMasterSummary.MasterKey + "_" + objMasterSummary.UpdatedOn.ToString("yyyyMM");
                //    objMasterSummary.RowKey = objMasterSummary.SerialNumber;
                //    objMasterSummary.Timestamp = objMasterSummary.UpdatedOn;

                //    //Insert log in Table Storage.
                //    InsertLogInAzureTable(LogType, objMasterSummary);
                //}

                if (StorageType.ToUpper() == "SQLTABLE")
                {
                    //Insert log in SQL Tabele.
                    Object[] Values = new object[8];
                    int i = 0;

                    //1
                    Values.SetValue(objMasterSummary.MasterKey, i);
                    i++;

                    //2
                    Values.SetValue(objMasterSummary.MasterValue, i);
                    i++;

                    //3
                    Values.SetValue(objMasterSummary.Action, i);
                    i++;

                    //4
                    Values.SetValue(objMasterSummary.Message, i);
                    i++;

                    //5
                    Values.SetValue(objMasterSummary.Description, i);
                    i++;

                    //6
                    Values.SetValue(objMasterSummary.UpdatedBy, i);
                    i++;

                    //7
                    Values.SetValue(objMasterSummary.UpdatedOn, i);
                    i++;

                    //8
                    Values.SetValue(objMasterSummary.MasterDetails, i);
                    i++;

                    return await AddMasterAuditLog(Values);

                }
            }
            catch (Exception)
            {
                return (false);
            }
            return (true);
        }

        private async Task<bool> AddMasterAuditLog(object[] MasterInfo)
        {
            try
            {
                //SQLServer da = new SQLServer();
                //string[] ColumnNames = new string[8];
                //SqlDbType[] DataType = new SqlDbType[8];
                //Object[] Values = new object[8];
                //int i = 0;

                ////1
                //ColumnNames.SetValue("Master", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                ////2
                //ColumnNames.SetValue("MasterValue", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                ////3
                //ColumnNames.SetValue("Action", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;


                ////4
                //ColumnNames.SetValue("Message", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                ////5
                //ColumnNames.SetValue("Description", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                ////6
                //ColumnNames.SetValue("UpdatedBy", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                ////7
                //ColumnNames.SetValue("UpdatedOn", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                ////8
                //ColumnNames.SetValue("MasterDetails", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(MasterInfo.GetValue(i), i);
                //i++;

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("@Master", SqlDbType.VarChar) { Value = MasterInfo.GetValue(0) },
                    new SqlParameter("@MasterValue", SqlDbType.VarChar) { Value = MasterInfo.GetValue(1) },
                    new SqlParameter("@Action", SqlDbType.VarChar) { Value = MasterInfo.GetValue(2) },
                    new SqlParameter("@Message", SqlDbType.VarChar) { Value = MasterInfo.GetValue(3) },
                    new SqlParameter("@Description", SqlDbType.VarChar) { Value = MasterInfo.GetValue(4) },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = MasterInfo.GetValue(5) },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = MasterInfo.GetValue(6) },
                    new SqlParameter("@MasterDetails", SqlDbType.VarChar) { Value = MasterInfo.GetValue(7) }
                };

                //if (!da.ExecuteProcedure("SPAddMasterAuditLog", ColumnNames, DataType, Values))
                if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddMasterAuditLog", sqlParameters))
                    return false;
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
        }

        internal async Task<string> GetCountryCode(string stationCode)
        {
            string countryCode = string.Empty;
            try
            {
                //SQLServer sqlServer = new SQLServer();
                SqlParameter[] sqlParameter = new SqlParameter[] {
                        new SqlParameter("@StationCode",stationCode)
                };

                //DataSet dsCountryCode = sqlServer.SelectRecords("uspGetCountryCode", sqlParameter);
                DataSet? dsCountryCode = await _readWriteDao.SelectRecords("uspGetCountryCode", sqlParameter);
                if (dsCountryCode != null)
                {
                    if (dsCountryCode.Tables.Count > 0 && dsCountryCode.Tables[0].Rows.Count > 0)
                    {
                        countryCode = dsCountryCode.Tables[0].Rows[0]["CountryCode"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return countryCode;
        }

        public async Task<bool> FlightDisabled(string FLTNo, DateTime FltDate)
        {
            try
            {
                //string procedure = "Messaging.uspFlightDisable";
                //SQLServer dtb = new SQLServer();
                DataSet? dsDisabled = new DataSet();

                //string[] paramname = new string[] {
                //                                "FLTNo",
                //                                "FltDate"};

                //object[] paramvalue = new object[] {FLTNo,
                //                                FltDate};

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.DateTime};

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("FLTNo", SqlDbType.VarChar) { Value = FLTNo },
                    new SqlParameter("FltDate", SqlDbType.DateTime) { Value = FltDate }
                };

                //dsDisabled = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);
                dsDisabled = await _readWriteDao.SelectRecords("Messaging.uspFlightDisable", sqlParameters);

                if (dsDisabled != null && dsDisabled.Tables[0].Rows.Count >= 1)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;

            }
            return false;
        }

        public async Task<DataSet?> ExchangeRateCall(string Procedure, DateTime CurrentUTCDatetime)
        {
            try
            {
                //SQLServer dtb = new SQLServer();
                DataSet dsExchangeRate = new DataSet();

                //string[] paramname = new string[] { "UTCTime" };
                //object[] paramvalue = new object[] { CurrentUTCDatetime };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.DateTime };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("UTCTime", SqlDbType.DateTime) { Value = CurrentUTCDatetime }
                };

                //return dsExchangeRate = dtb.SelectRecords(Procedure, paramname, paramvalue, paramtype);
                return dsExchangeRate = await _readWriteDao.SelectRecords(Procedure, sqlParameters);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        public async Task<bool> CreateExchangeRatesAPI(string ExchangeRates, DateTime ExchangeDate, DateTime TimeStamp)
        {
            try
            {
                string procedure = "Masters.uspCreateExchangeRatesAPI";
                //SQLServer dtb = new SQLServer();

                //string[] paramname = new string[] { "ExchangeRates", "ExchangeDate", "UpdatedOn" };
                //object[] paramvalue = new object[] { ExchangeRates, ExchangeDate, TimeStamp };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.DateTime };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("ExchangeRates", SqlDbType.VarChar) { Value = ExchangeRates },
                    new SqlParameter("ExchangeDate", SqlDbType.DateTime) { Value = ExchangeDate },
                    new SqlParameter("UpdatedOn", SqlDbType.DateTime) { Value = TimeStamp }
                };

                //return dtb.ExecuteProcedure(procedure, paramname, paramtype, paramvalue);
                return await _readWriteDao.ExecuteNonQueryAsync(procedure, sqlParameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("ExchangeRates: " + ex.Message);
                _logger.LogError("ExchangeRates: {0}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SaveForexAPILog(string InterfaceId, string Request, string Response, string ConversionSnapshotId, string Error)
        {
            try
            {
                string procedure = "Log.uspSaveForexAPILog";
                //SQLServer dtb = new SQLServer();
                DataSet? dsDisabled = new DataSet();
                string CreatedBy = "API";
                DateTime CreatedOn = DateTime.UtcNow.AddHours(+8);

                //string[] paramname = new string[] { "InterfaceId", "Request", "Response", "ConversionSnapshotId", "Createdby", "CreatedOn", "Error" };
                //object[] paramvalue = new object[] { InterfaceId, Request, Response, ConversionSnapshotId, CreatedBy, CreatedOn, Error };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.VarChar, SqlDbType.NVarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("InterfaceId", SqlDbType.NVarChar) { Value = InterfaceId },
                    new SqlParameter("Request", SqlDbType.NVarChar) { Value = Request },
                    new SqlParameter("Response", SqlDbType.VarChar) { Value = Response },
                    new SqlParameter("ConversionSnapshotId", SqlDbType.NVarChar) { Value = ConversionSnapshotId },
                    new SqlParameter("Createdby", SqlDbType.VarChar) { Value = CreatedBy },
                    new SqlParameter("CreatedOn", SqlDbType.DateTime) { Value = CreatedOn },
                    new SqlParameter("Error", SqlDbType.VarChar) { Value = Error }
                };
                //return dtb.ExecuteProcedure(procedure, paramname, paramtype, paramvalue);
                return await _readWriteDao.ExecuteNonQueryAsync(procedure, sqlParameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("ExchangeRates: " + ex.Message);
                _logger.LogError("ExchangeRates: {0}", ex.Message);
                return false;
            }
        }

        public async Task<DataSet?> AutoReleaseCapacityAllocation(string Procedure, int FrequencyInMinutes)
        {
            try
            {
                //SQLServer dtb = new SQLServer();
                DataSet? dsExchangeRate = new DataSet();

                //string[] paramname = new string[] { "FrequencyInMinutes" };
                //object[] paramvalue = new object[] { FrequencyInMinutes };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.Int };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("FrequencyInMinutes", SqlDbType.Int) { Value = FrequencyInMinutes }
                };

                //return dsExchangeRate = dtb.SelectRecords(Procedure, paramname, paramvalue, paramtype);
                return dsExchangeRate = await _readWriteDao.SelectRecords(Procedure, sqlParameters);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        public async Task<DataSet?> NoShowCalculation(string Procedure, DateTime CurrentUTCDatetime)
        {
            try
            {
                //SQLServer dtb = new SQLServer();
                DataSet? dsNoShowCalculation = new DataSet();

                //string[] paramname = new string[] { "UTCTime" };
                //object[] paramvalue = new object[] { CurrentUTCDatetime };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.DateTime };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("UTCTime", SqlDbType.DateTime) { Value = CurrentUTCDatetime }
                };

                //return dsNoShowCalculation = dtb.SelectRecords(Procedure, paramname, paramvalue, paramtype);
                return dsNoShowCalculation = await _readWriteDao.SelectRecords(Procedure, sqlParameters);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        public async Task<bool> SendInformationtoSP(string Procedure)
        {
            try
            {
                //SQLServer dtb = new SQLServer();

                return await _readWriteDao.ExecuteNonQueryAsync(Procedure);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
        }

        public async Task<DataSet?> CheckIfAWBOnBLOB(string awbNo, string fileUrl, string container)
        {
            try
            {
                //SQLServer da = new SQLServer();

                //string[] paramname1 = new string[3];
                //object[] paramvalue1 = new object[3];
                //SqlDbType[] paramtype1 = new SqlDbType[3];

                //paramname1[0] = "DONumber";
                //paramname1[1] = "FileURL";
                //paramname1[2] = "Container";

                //paramvalue1[0] = awbNo;
                //paramvalue1[1] = fileUrl;
                //paramvalue1[2] = container;

                //paramtype1[0] = SqlDbType.VarChar;
                //paramtype1[1] = SqlDbType.VarChar;
                //paramtype1[2] = SqlDbType.VarChar;

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("DONumber", SqlDbType.VarChar) { Value = awbNo },
                    new SqlParameter("FileURL", SqlDbType.VarChar) { Value = fileUrl },
                    new SqlParameter("Container", SqlDbType.VarChar) { Value = container }
                };

                //return da.SelectRecords("SP_GetOrSetFileURL", paramname1, paramvalue1, paramtype1);
                return await _readWriteDao.SelectRecords("SP_GetOrSetFileURL", sqlParameters);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        public async Task<string> GeteAWBPrintPrefence(string agentcode, string awbNumber, string awbPrefix)
        {
            //SQLServer db = new SQLServer();
            string? ratePref;
            try
            {
                //string[] paramNames = new string[] { "agentcode", "AWBNumber", "AWBPrefix" };
                //object[] paramValues = new object[] { agentcode, awbNumber, awbPrefix };
                //SqlDbType[] paramTypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("agentcode", SqlDbType.VarChar) { Value = agentcode },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = awbNumber },
                    new SqlParameter("AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix }
                };
                //ratePref = db.GetStringByProcedure("spGeteAWBPrintPreference", paramNames, paramValues, paramTypes);
                ratePref = await _readWriteDao.GetStringByProcedureAsync("spGeteAWBPrintPreference", sqlParameters);

            }
            catch (Exception ex)
            {
                ratePref = "";
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return ratePref;
        }
        public async Task<string> GetSHCCodesandDesc(string shcCodes)
        {
            string output = string.Empty;
            //SQLServer da = new SQLServer();
            DataSet? ds = null;

            try
            {
                //string[] pName = new string[] { "SHCCodes" };
                //object[] pValue = new object[] { shcCodes };
                //SqlDbType[] pType = new SqlDbType[] { SqlDbType.VarChar };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("SHCCodes", SqlDbType.VarChar) { Value = shcCodes }
                };
                //ds = da.SelectRecords("sp_GetSHCCodeDesc", pName, pValue, pType);
                ds = await _readWriteDao.SelectRecords("sp_GetSHCCodeDesc", sqlParameters);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    output = ds.Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                ds = null;
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
            return output;
        }
        public async Task<string> checkexportValidation(string dest)
        {
            string query = "select Countrycode from airportmaster   WHERE Airportcode = '" + dest + "'";

            //SQLServer da = new SQLServer();

            var Dest = string.Empty;

            try
            {
                //DataSet ds = da.GetDataset(query);
                DataSet? ds = await _readWriteDao.GetDatasetAsync(query);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    Dest = Convert.ToString(ds.Tables[0].Rows[0][0]);
                }

            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return Dest;

        }
        public async Task<DataSet> GetShipmentTypeNew(string origin, string destination)
        {
            //SQLServer da = new SQLServer();
            DataSet? ds;

            //string[] pName = new string[] { "Origin", "Destination" };
            //object[] pValue = new object[] { origin, destination };
            //SqlDbType[] pType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
            SqlParameter[] sqlParameters = new SqlParameter[] {
                new SqlParameter("Origin", SqlDbType.VarChar) { Value = origin },
                new SqlParameter("Destination", SqlDbType.VarChar) { Value = destination }
            };
            try
            {
                //ds = da.SelectRecords("sp_GetShipmentTypeNew", pName, pValue, pType);
                ds = await _readWriteDao.SelectRecords("sp_GetShipmentTypeNew", sqlParameters);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ds = null;
            }

            return ds;
        }

        public async Task<DataSet?> GetAWBExecutionInfo(string awbPrefix, string awbNumber)
        {
            try
            {
                //SQLServer da = new SQLServer();

                //string[] paramname1 = { "AWBPrefix", "AWBNumber" };
                //object[] paramvalue1 = { awbPrefix, awbNumber };
                //SqlDbType[] paramtype1 = { SqlDbType.VarChar, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = awbNumber }
                };
                //return da.SelectRecords("spGetAWBExecutionInfo", paramname1, paramvalue1, paramtype1);
                return await _readWriteDao.SelectRecords("spGetAWBExecutionInfo", sqlParameters);
            }
            catch (Exception ex)
            {
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        public async Task<DataSet> GetClientName()
        {
            DataSet? ds = new DataSet("BAlgetClientName");
            //SQLServer da = new SQLServer();
            try
            {
                //ds = da.SelectRecords("SP_GetClientName");
                ds = await _readWriteDao.SelectRecords("SP_GetClientName");
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
            return ds;
        }

        public async Task<DataSet> GetAirlineDetails(string Origin, string Dest, string selectedAirlineCode)
        {
            DataSet? ds = new DataSet();
            //string[] Pname = new string[3];
            //object[] Pvalue = new object[3];
            //SqlDbType[] Ptype = new SqlDbType[3];
            //SQLServer da = new SQLServer();

            //if (da == null)
            //    da = new SQLServer();

            try
            {

                //Pname[0] = "Origin";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = Origin;

                //Pname[1] = "Destination";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = Dest;

                //Pname[2] = "selectedAirlineCode";
                //Ptype[2] = SqlDbType.VarChar;
                //Pvalue[2] = selectedAirlineCode;
                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("Origin", SqlDbType.VarChar) { Value = Origin },
                    new SqlParameter("Destination", SqlDbType.VarChar) { Value = Dest },
                    new SqlParameter("selectedAirlineCode", SqlDbType.VarChar) { Value = selectedAirlineCode }
                };

                //ds = da.SelectRecords("SPGetAirlineDetails", Pname, Pvalue, Ptype);
                ds = await _readWriteDao.SelectRecords("SPGetAirlineDetails", sqlParameters);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return ds;
            }
            finally
            {
                //da = null;
                //Pname = null;
                //Pvalue = null;
                //Ptype = null;
            }
            return ds;


        }

        public async Task<string?> getorg(string orgin)
        {
            string query = "select Airportcode+'-'+AirportName from airportmaster   WHERE Airportcode = '" + orgin + "'";

            //SQLServer da = new SQLServer();

            string org = string.Empty;

            try
            {
                //DataSet ds = da.GetDataset(query);
                DataSet? ds = await _readWriteDao.GetDatasetAsync(query);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    org = Convert.ToString(ds.Tables[0].Rows[0][0]);
                }

            }
            catch (Exception ex)
            { //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return org;

        }

        public async Task<string?> GetAWBStatus(string awbPrefix, string awbNumber)
        {
            string query = "SELECT TOP 1 AWBStatus FROM AWBSummaryMaster WHERE AWBNumber = '" + awbNumber + "' AND AWBPrefix = '" + awbPrefix + "'";

            //SQLServer da = new SQLServer();
            string awbStatus = string.Empty;

            try
            {
                //DataSet ds = da.GetDataset(query);
                DataSet? ds = await _readWriteDao.GetDatasetAsync(query);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    awbStatus = Convert.ToString(ds.Tables[0].Rows[0][0]);
                }
                else
                    awbStatus = "B";
            }
            catch (Exception ex)
            { //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return awbStatus;
        }
        public async Task<DataSet?> GetAWBRateLog(string awbPrefix, string awbNumber, bool isAsAgreed, string curRole)
        {
            try
            {
                //SQLServer da = new SQLServer();

                //string[] paramname1 = { "AWBPrefix", "AWBNumber", "IsAsAgreed", "CurRole" };
                //object[] paramvalue1 = { awbPrefix, awbNumber, isAsAgreed, curRole };
                //SqlDbType[] paramtype1 = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };

                SqlParameter[] sqlParameters = new SqlParameter[] {
                    new SqlParameter("AWBPrefix", SqlDbType.VarChar) { Value = awbPrefix },
                    new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = awbNumber },
                    new SqlParameter("IsAsAgreed", SqlDbType.Bit) { Value = isAsAgreed },
                    new SqlParameter("CurRole", SqlDbType.VarChar) { Value = curRole }
                };

                //return da.SelectRecords("uspGetAWBRateLogForPrint", paramname1, paramvalue1, paramtype1);
                return await _readWriteDao.SelectRecords("uspGetAWBRateLogForPrint", sqlParameters);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }
        /*Not in user*/
        //public DataSet GetAWBDimensions(string awbNumber, string awbPrefix)
        //{
        //    SQLServer da = new SQLServer();
        //    try
        //    {
        //        string[] pName = new string[] { "AWBPrefix", "AWBNumber" };
        //        object[] pvalues = new object[] { awbPrefix, awbNumber };
        //        SqlDbType[] pTypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

        //        DataSet ds = da.SelectRecords("uspGetAWBDimension", pName, pvalues, pTypes);
        //        return ds;
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //        return null;
        //    }
        //}

        public string GetSASBlobUrl(string filenameOrUrl)
        {
            try
            {

                string containerName = "";
                string str = filenameOrUrl;
                if (filenameOrUrl.Contains('/'))
                {
                    // filenameOrUrl = filenameOrUrl.ToLower();
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }
                containerName = containerName.ToLower();

                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //string sas = GetSASUrl(containerName, storageAccount);
                //StorageCredentialsSharedAccessSignature sasCreds = new StorageCredentialsSharedAccessSignature(sas);
                //CloudBlobClient sasBlobClient = new CloudBlobClient(storageAccount.BlobEndpoint,
                //new StorageCredentialsSharedAccessSignature(sas));
                //CloudBlob blob = sasBlobClient.GetBlobReference(containerName + @"/" + filenameOrUrl);
                //return "https://" + getStorageName() + ".blob.core.windows.net/" + containerName + "/" + filenameOrUrl + sas;

                // Enforce TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Get storage account details
                string storageName = getStorageName();
                string storageKey = getStorageKey();

                // Create BlobServiceClient with shared key (to generate SAS)
                string accountUrl = $"https://{storageName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new StorageSharedKeyCredential(storageName, storageKey)
                );

                // Generate SAS token (using your updated GetSASUrl)
                string sasToken = GetSASUrl(containerName, blobServiceClient); // Returns "?sv=..."

                // Construct full blob URL with SAS
                string blobUrlWithSas = $"{accountUrl}/{containerName}/{filenameOrUrl}{sasToken}";

                return blobUrlWithSas;

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return "";
        }

        /*Deprecated*/
        //public static string GetSASUrl(string containerName, CloudStorageAccount storageAccount)
        //{
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        //    container.CreateIfNotExist();

        //    BlobContainerPermissions containerPermissions = new BlobContainerPermissions();

        //    string sasactivetime = getSettingFromDB("BlobStorageactiveSASTime");
        //    double _SaSactiveTime = string.IsNullOrWhiteSpace(sasactivetime) ? 5 : Convert.ToDouble(sasactivetime);

        //    containerPermissions.SharedAccessPolicies.Add("defaultpolicy", new SharedAccessPolicy()
        //    {
        //        SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-1),
        //        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(_SaSactiveTime),
        //        Permissions = SharedAccessPermissions.Write | SharedAccessPermissions.Read | SharedAccessPermissions.List
        //    });

        //    string IsBlobPrivate = getSettingFromDB("IsBlobPrivate");
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
                // //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }

        }

        private static string getStorageName()
        {
            //GenericFunction genericFunction = new GenericFunction();
            try
            {
                if (String.IsNullOrEmpty(BlobName))
                {
                    //BAL.LoginBLL objBL = new BAL.LoginBLL();

                    //BlobName = genericFunction.ReadValueFromDb("BlobStorageName");
                    BlobName = ConfigCache.Get("BlobStorageName");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return BlobName;
        }

        private static string getStorageKey()
        {
            //GenericFunction genericFunction = new GenericFunction();
            try
            {
                if (String.IsNullOrEmpty(BlobKey))
                {
                    //BAL.LoginBLL objBL = new BAL.LoginBLL();
                    //BlobKey = genericFunction.ReadValueFromDb("BlobStorageKey");
                    BlobKey = ConfigCache.Get("BlobStorageKey");
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return BlobKey;
        }

        public async Task<string?> GetMasterConfiguration(string Parameter)
        {
            string? ParameterValue = string.Empty;
            //SQLServer da = new SQLServer(true);
            //string[] QName = new string[] { "PType" };
            //object[] QValues = new object[] { Parameter };
            //SqlDbType[] QType = new SqlDbType[] { SqlDbType.VarChar };

            SqlParameter[] sqlParameters = new SqlParameter[] {
                new SqlParameter("PType", SqlDbType.VarChar) { Value = Parameter }
            };
            //ParameterValue = da.GetStringByProcedure("spGetSystemParameter", QName, QValues, QType);
            ParameterValue = await _readWriteDao.GetStringByProcedureAsync("spGetSystemParameter", sqlParameters);
            if (ParameterValue == null)
                ParameterValue = "";
            //da = null;
            //QName = null;
            //QValues = null;
            //QType = null;

            return ParameterValue;
        }

        //private static string getSettingFromDB(string parameterKey)
        private async Task<string?> getSettingFromDB(string parameterKey)
        {
            //GenericFunction genericFunction = new GenericFunction();
            string? _settingValue = string.Empty;
            try
            {

                // BAL.LoginBLL objBL = new BAL.LoginBLL();
                _settingValue = await GetMasterConfiguration(parameterKey);

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return _settingValue;
        }

        public async Task<string> GetRoundoffvalueSingle(string origin, string destination, string param, string value, string date, string agent, string shipper, string consignee, string ProductType, string strCurrency, string Commcode)
        {
            string? val = "";
            //SQLServer objSql = new SQLServer();
            try
            {
                if (value.Length > 0 && value != "")
                {

                    //string[] pName = new string[] { "Origin", "Destination", "Parameter", "Value", "Date", "Agent", "Shipper", "Consignee", "ProductType", "Currency", "CommCode" };
                    //object[] pValue = new object[] { origin, destination, param, value, date, agent, shipper, consignee, ProductType, strCurrency, Commcode };
                    //SqlDbType[] pType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

                    SqlParameter[] pParameter = new SqlParameter[] {
                        new SqlParameter("Origin", SqlDbType.VarChar) { Value = origin },
                        new SqlParameter("Destination", SqlDbType.VarChar) { Value = destination },
                        new SqlParameter("Parameter", SqlDbType.VarChar) { Value = param },
                        new SqlParameter("Value", SqlDbType.VarChar) { Value = value },
                        new SqlParameter("Date", SqlDbType.VarChar) { Value = date },
                        new SqlParameter("Agent", SqlDbType.VarChar) { Value = agent },
                        new SqlParameter("Shipper", SqlDbType.VarChar) { Value = shipper },
                        new SqlParameter("Consignee", SqlDbType.VarChar) { Value = consignee },
                        new SqlParameter("ProductType", SqlDbType.VarChar) { Value = ProductType },
                        new SqlParameter("Currency", SqlDbType.VarChar) { Value = strCurrency },
                        new SqlParameter("CommCode", SqlDbType.VarChar) { Value = Commcode }

                    };
                    //val = objSql.GetStringByProcedure("spGetRoundingValue", pName, pValue, pType);
                    val = await _readWriteDao.GetStringByProcedureAsync("spGetRoundingValue", pParameter);
                }
            }
            catch (Exception ex)
            { //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return val;
        }

        public byte[] DownloadFromBlob(string filenameOrUrl)
        {
            try
            {
                string containerName = "";
                string str = filenameOrUrl;
                if (filenameOrUrl.Contains('/'))
                {
                    //filenameOrUrl = filenameOrUrl.ToLower();
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }
                byte[] downloadStream = null;
                containerName = containerName.ToLower();

                //StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
                //string sas = GetSASUrl(containerName, storageAccount);
                //StorageCredentialsSharedAccessSignature sasCreds = new StorageCredentialsSharedAccessSignature(sas);
                //CloudBlobClient sasBlobClient = new CloudBlobClient(storageAccount.BlobEndpoint,
                //new StorageCredentialsSharedAccessSignature(sas));
                //CloudBlob blob = sasBlobClient.GetBlobReference(containerName + @"/" + filenameOrUrl);
                //try
                //{
                //    downloadStream = blob.DownloadByteArray();
                //}
                //catch (Exception ex)
                //{
                //    //clsLog.WriteLogAzure(ex);
                //    return null;
                //}

                // Enforce TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Get storage credentials
                string storageName = getStorageName();
                string storageKey = getStorageKey();

                // Create BlobServiceClient with shared key (to generate SAS)
                string accountUrl = $"https://{storageName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new StorageSharedKeyCredential(storageName, storageKey)
                );

                // Generate SAS token (your existing updated GetSASUrl)
                string sasToken = GetSASUrl(containerName, blobServiceClient); // Returns "?sv=..."

                // Construct full SAS URI for the blob
                Uri blobSasUri = new Uri($"{accountUrl}/{containerName}/{filenameOrUrl}{sasToken}");

                // Create BlobClient using SAS URI
                BlobClient blob = new BlobClient(blobSasUri);

                try
                {
                    // Download entire blob as byte array
                    BlobDownloadResult result = blob.DownloadContent();
                    downloadStream = result.Content.ToArray();
                }
                catch (Exception ex)
                {
                    //clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    return null;
                }

                return downloadStream;

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }

        }
        public async Task<DataSet?> GetFlightNotification(string AWBPrefix, string AWBNumber, string Type, int Pices, decimal Weight, string FLTOrigin, string FLTDestination, string Status)
        {
            //SQLServer da = new SQLServer();
            DataSet? ds = null;

            //string[] pName = new string[] { "AWBPrefix", "AWBNumber", "Type", "Pices", "Weight", "FLTOrigin", "FLTDestination", "Status" };
            //object[] pValue = new object[] { AWBPrefix, AWBNumber, Type, Pices, Weight, FLTOrigin, FLTDestination, Status };
            //SqlDbType[] pType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

            SqlParameter[] sqlParameters = new SqlParameter[] {
                new SqlParameter("AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                new SqlParameter("AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                new SqlParameter("Type", SqlDbType.VarChar) { Value = Type },
                new SqlParameter("Pices", SqlDbType.Int) { Value = Pices },
                new SqlParameter("Weight", SqlDbType.Decimal) { Value = Weight },
                new SqlParameter("FLTOrigin", SqlDbType.VarChar) { Value = FLTOrigin },
                new SqlParameter("FLTDestination", SqlDbType.VarChar) { Value = FLTDestination },
                new SqlParameter("Status", SqlDbType.VarChar) { Value = Status }
            };

            try
            {
                //ds = da.SelectRecords("uspSendNotification", pName, pValue, pType);
                ds = await _readWriteDao.SelectRecords("uspSendNotification", sqlParameters);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ds = null;
            }

            return ds;
        }

        public async Task<DataSet> GenerateMessageSequence(string partnercode = "", string triggerpont = "", string messagetype = "")
        {
            //SQLServer da = new SQLServer();
            //string[] paramname = new string[] { "PartnerCode", "TriggerPoint", "messagetype" };
            //object[] paramvalue = new object[] { partnercode, triggerpont, messagetype };
            //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

            SqlParameter[] sqlParameters = new SqlParameter[] {
                new SqlParameter("PartnerCode", SqlDbType.VarChar) { Value = partnercode },
                new SqlParameter("TriggerPoint", SqlDbType.VarChar) { Value = triggerpont },
                new SqlParameter("messagetype", SqlDbType.VarChar) { Value = messagetype }
            };

            //DataSet ds = da.SelectRecords("GetMessageSequence", paramname, paramvalue, paramtype);
            DataSet? ds = await _readWriteDao.SelectRecords("GetMessageSequence", sqlParameters);
            return ds;
        }

        public async Task<bool> SendBookingConfirmation(string awbPrefix, string awbNumber, string Type, string flightNumber = "", string flightDate = "", string flightOrigin = "", string flightDestination = "", string updatedBy = "")
        {
            //SQLServer sqlServer = new SQLServer();

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] {
                    new SqlParameter("AWBPrefix", awbPrefix)
                    , new SqlParameter("AWBNumber", awbNumber)
                    , new SqlParameter("Type", Type)
                    , new SqlParameter("FlightNumber", flightNumber)
                    , new SqlParameter("FlightDate", flightDate)
                    , new SqlParameter("FLTOrigin", flightOrigin)
                    , new SqlParameter("FLTDestination", flightDestination)
                    , new SqlParameter("UpdatedBy", updatedBy)
                };
                //DataSet dsResult = sqlServer.SelectRecords("uspSendNotification", sqlParameter);
                DataSet? dsResult = await _readWriteDao.SelectRecords("uspSendNotification", sqlParameter);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsResult.Tables.Count; i++)
                    {
                        if (dsResult.Tables[i].Columns.Contains("BookingConfirmationResult"))
                            return Convert.ToBoolean(dsResult.Tables[i].Rows[0]["BookingConfirmationResult"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return false;
        }

        //public string GetKeySecreteValue(string Keysecrete = "")
        //{
        //    string KeyValutValue = string.Empty;
        //    try
        //    {
        //        string keyVaultUri = ConfigurationManager.AppSettings["keyVaultUri"].ToString();
        //        var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        //        KeyVaultSecret secret = secretClient.GetSecret(Keysecrete);
        //        KeyValutValue = secret.Value;
        //    }
        //    catch (Exception ex)
        //    {
        //        //clsLog.WriteLogAzure(ex);
        //    }
        //    return KeyValutValue;
        //}
    }
}


