using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;

namespace QidWorkerRole
{
    public class ExchangeRateExpiryAlert
    {
        //#region Variables
        //SQLServer db;
        //#endregion

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<ExchangeRateExpiryAlert> _logger;
        public ExchangeRateExpiryAlert(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<ExchangeRateExpiryAlert> logger
        )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }

        #region Main function
        public async Task ExchangeRateExpiryListener()
        {
            try
            {
                //db = new SQLServer();
                DataSet? ds = null;

                //ds = db.SelectRecords("Usp_ExchangeRateNotification");
                ds = await _readWriteDao.SelectRecords("Usp_ExchangeRateNotification");


                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        string htmlContent;
                        StringBuilder StbExRateExpiryData = new StringBuilder(string.Empty);
                        GenericFunction genericFunction = new GenericFunction();
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            StbExRateExpiryData.Append(Convert.ToString(ds.Tables[0].Rows[i]["body"].ToString()));
                        }
                        StbExRateExpiryData.Append("</table>");
                        htmlContent = StbExRateExpiryData.ToString();

                        NReco.PdfGenerator.HtmlToPdfConverter htmlToPdfConverter = null;
                        MemoryStream memoryStream = null;
                        htmlToPdfConverter = new NReco.PdfGenerator.HtmlToPdfConverter();
                        htmlToPdfConverter.Margins.Top = 5.0f;
                        htmlToPdfConverter.Margins.Bottom = 10.0f;
                        htmlToPdfConverter.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                        var pdfbyteArray = htmlToPdfConverter.GeneratePdf(htmlContent);
                        memoryStream = new MemoryStream(pdfbyteArray);
                        string Docfilename = "Exchange Rate Expiry Details" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                        String FileUrl = genericFunction.UploadToBlob(memoryStream, Docfilename, "sis");

                        string emailBody = "Hi, the followed exchange rate will be expired soon. Please see the attached file.";
                        string emailtext = "Hi, " + "</BR></BR>" + htmlContent;
                        //bool flg = insertData("Rate Line Expire Notification", emailtext, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), DateTime.Now.ToString(), DateTime.Now.ToString(), "RATEEXP", "");

                        await SendExRateExpiryAlertWithAttachment("Exchange Rate Expire Notification", emailBody, DateTime.Now, "ExchangeRateEXP", "", true, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), memoryStream, ".pdf", FileUrl, "0", "Outbox", Docfilename);

                    }
                }
            }
            catch (Exception ex)
            {

                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }
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
        public async Task<int> SendExRateExpiryAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName)
        {
            int SerialNo = 0;

            try
            {
                string procedure = "uspAddMessageAttachmentDetails";

                //SQLServer dtb = new SQLServer();

                DataSet? objDS = null;
                byte[] objBytes = null;

                if (Attachments != null)
                {
                    objBytes = Attachments.ToArray();
                }

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

                //    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.DateTime,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};

                //objDS = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Subject", SqlDbType.VarChar) { Value = subject },
                    new SqlParameter("@Body", SqlDbType.VarChar) { Value = Msg },
                    new SqlParameter("@TimeStamp", SqlDbType.DateTime) { Value = TimeStamp },
                    new SqlParameter("@MessageType", SqlDbType.VarChar) { Value = MessageType },
                    new SqlParameter("@ErrorDesc", SqlDbType.VarChar) { Value = ErrorDesc },
                    new SqlParameter("@IsBlog", SqlDbType.Bit) { Value = IsBlog },
                    new SqlParameter("@FromId", SqlDbType.VarChar) { Value = FromEmailId },
                    new SqlParameter("@ToId", SqlDbType.VarChar) { Value = ToEmailId },
                    new SqlParameter("@Attachment", SqlDbType.VarBinary) { Value = objBytes },
                    new SqlParameter("@Extension", SqlDbType.VarChar) { Value = AttachmentExtension },
                    new SqlParameter("@FileUrl", SqlDbType.VarChar) { Value = FileUrl },
                    new SqlParameter("@isProcessed", SqlDbType.VarChar) { Value = isProcessed },
                    new SqlParameter("@MessageBoxType", SqlDbType.VarChar) { Value = MessageBoxType },
                    new SqlParameter("@AttachmentName", SqlDbType.VarChar) { Value = AttachmentName }
                };

                objDS = await _readWriteDao.SelectRecords(procedure, parameters);

                if (objDS != null && objDS.Tables.Count > 0 && objDS.Tables[0].Rows.Count > 0)
                {
                    SerialNo = Convert.ToInt32(objDS.Tables[0].Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                SerialNo = 0;
            }

            return SerialNo;
        }

        #endregion
    }
}
