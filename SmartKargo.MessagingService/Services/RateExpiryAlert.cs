/**********************************************************
 * Object Name: Rate Expiry Alert
 * Prepared By: Sushant Gavas
 * Prepared On: 28 FEB 2020
 * Description: This class provides for performing operations 
 *              related to send email to user for rateline expire alert
***********************************************************/

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;

namespace QidWorkerRole
{
    public class RateExpiryAlert
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<RateExpiryAlert> _logger;
        private readonly GenericFunction _genericFunction;

        #region Constructor
        public RateExpiryAlert(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<RateExpiryAlert> logger,
            GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }
        #endregion
        #region Variables
        //public static string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //SQLServer db;
        #endregion

        #region Main function
        /// <summary>
        /// This function send a rate expiry details to user through email.
        /// </summary>
        public async Task RateExpiryListener()
        {
            try
            {
                //db = new SQLServer();
                DataSet? ds = null;

                //ds = db.SelectRecords("USPGETAGENTRATEEXPIRYDETAILS");
                ds = await _readWriteDao.SelectRecords("USPGETAGENTRATEEXPIRYDETAILS");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        string htmlContent;
                        StringBuilder StbRateExpiryData = new StringBuilder(string.Empty);
                        //GenericFunction genericFunction = new GenericFunction();
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            StbRateExpiryData.Append(Convert.ToString(ds.Tables[0].Rows[i]["body"].ToString()));
                        }
                        StbRateExpiryData.Append("</table>");
                        htmlContent = StbRateExpiryData.ToString();

                        NReco.PdfGenerator.HtmlToPdfConverter htmlToPdfConverter = null;
                        MemoryStream memoryStream = null;
                        htmlToPdfConverter = new NReco.PdfGenerator.HtmlToPdfConverter();
                        htmlToPdfConverter.Margins.Top = 5.0f;
                        htmlToPdfConverter.Margins.Bottom = 10.0f;
                        htmlToPdfConverter.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                        var pdfbyteArray = htmlToPdfConverter.GeneratePdf(htmlContent);
                        memoryStream = new MemoryStream(pdfbyteArray);
                        string Docfilename = "Rate Expiry Details" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                        String FileUrl = _genericFunction.UploadToBlob(memoryStream, Docfilename, "sis");

                        string emailBody = "Hi, the followed rateline will be expired soon. Please see the attached file.";
                        string emailtext = "Hi, " + "</BR></BR>" + htmlContent;
                        //bool flg = insertData("Rate Line Expire Notification", emailtext, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), DateTime.Now.ToString(), DateTime.Now.ToString(), "RATEEXP", "");

                        await SendRateExpiryAlertWithAttachment("Rate Line Expire Notification", emailBody, DateTime.Now, "RATEEXP", "", true, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), memoryStream, ".pdf", FileUrl, "0", "Outbox", Docfilename);

                    }
                }
            }
            catch (Exception ex)
            {

                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //not required as we are using dependency injection
            //finally
            //{
            //    if (db != null)
            //    {
            //        db = null;
            //    }

            //}



        }
        #endregion

        #region Send mail with attachment
        //public  static int SendRateExpiryAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName)
        public async Task<int> SendRateExpiryAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName)
        {
            int SerialNo = 0;

            try
            {
                string procedure = "uspAddMessageAttachmentDetails";
                // SQLServer dtb = new SQLServer();
                DataSet? objDS = null;
                byte[] objBytes = null;

                if (Attachments != null)
                    objBytes = Attachments.ToArray();

                //    string[] paramname = new string[] { "Subject",
                //                                    "Body",
                //                                    "TimeStamp",
                //                                    "MessageType",
                //                                    "ErrorDesc",
                //                                    "IsBlog",
                //"FromId", "ToId","Attachment","Extension","FileUrl","isProcessed","MessageBoxType", "AttachmentName"};

                //    object[] paramvalue = new object[] {subject,
                //                                    Msg,
                //                                    TimeStamp,
                //                                    MessageType,
                //                                    ErrorDesc,
                //                                    IsBlog, FromEmailId,ToEmailId, objBytes,AttachmentExtension,FileUrl,isProcessed,MessageBoxType, AttachmentName};

                //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.DateTime,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.VarChar,
                //                                     SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};

                var parameters = new SqlParameter[]
                {
                    new("@Subject", SqlDbType.VarChar) { Value = subject },
                    new("@Body", SqlDbType.VarChar) { Value = Msg },
                    new("@TimeStamp", SqlDbType.DateTime) { Value = TimeStamp },
                    new("@MessageType", SqlDbType.VarChar) { Value = MessageType },
                    new("@ErrorDesc", SqlDbType.VarChar) { Value = ErrorDesc },
                    new("@IsBlog", SqlDbType.Bit) { Value = IsBlog },
                    new("@FromId", SqlDbType.VarChar) { Value = FromEmailId },
                    new("@ToId", SqlDbType.VarChar) { Value = ToEmailId },
                    new("@Attachment", SqlDbType.VarBinary) { Value = objBytes },
                    new("@Extension", SqlDbType.VarChar) { Value = AttachmentExtension },
                    new("@FileUrl", SqlDbType.VarChar) { Value = FileUrl },
                    new("@isProcessed", SqlDbType.VarChar) { Value = isProcessed },
                    new("@MessageBoxType", SqlDbType.VarChar) { Value = MessageBoxType },
                    new("@AttachmentName", SqlDbType.VarChar) { Value = AttachmentName }
                };

                //objDS = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);
                objDS = await _readWriteDao.SelectRecords(procedure, parameters);

                if (objDS != null && objDS.Tables.Count > 0 && objDS.Tables[0].Rows.Count > 0)
                    SerialNo = Convert.ToInt32(objDS.Tables[0].Rows[0][0]);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                SerialNo = 0;
            }

            return SerialNo;
        }

        #endregion

        #region Inset Data into tblOutbox
        public async Task<bool> insertData(string subject, string body, string fromID, String ToId, string sendOn, string recievedOn, string type, string status)
        {
            bool flag = false;
            try
            {
                //SQLServer db = new SQLServer();//new SQLServer(conStr);
                //string[] param = { "Subject", "Body", "FromEmailID", "ToEmailID", "CreatedOn", "Agentcode", "refNo", "isInternal", "Type" , "IsHTML"};
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                //object[] values = { subject, body, fromID, ToId, DateTime.Now, "", 0, 0, type, 1 };
                var parameters = new SqlParameter[]
                {
                    new("@Subject", SqlDbType.VarChar)     { Value = subject },
                    new("@Body", SqlDbType.VarChar)        { Value = body },
                    new("@FromEmailID", SqlDbType.VarChar) { Value = fromID },
                    new("@ToEmailID", SqlDbType.VarChar)   { Value = ToId },
                    new("@CreatedOn", SqlDbType.DateTime)  { Value = DateTime.Now },
                    new("@Agentcode", SqlDbType.VarChar)   { Value = "" },
                    new("@refNo", SqlDbType.Int)           { Value = 0 },
                    new("@isInternal", SqlDbType.Bit)      { Value = 0 },
                    new("@Type", SqlDbType.VarChar)        { Value = type },
                    new("@IsHTML", SqlDbType.Bit)          { Value = 1 }
                };
                //flag = db.InsertData("spInsertMsgToOutbox", param, sqldbtypes, values);
                flag = await _readWriteDao.ExecuteNonQueryAsync("spInsertMsgToOutbox", parameters);

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        #endregion
    }
}
