using System;
using System.Data;
using System.IO;
using System.Text;
using QID.DataAccess;

namespace QidWorkerRole
{
    class ExchangeRateExpiryAlert
    {
        #region Variables
        SQLServer db;
        #endregion
        #region Main function
        public void ExchangeRateExpiryListener()
        {
            try
            {
                db = new SQLServer();
                DataSet ds = null;

                ds = db.SelectRecords("Usp_ExchangeRateNotification");

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

                        SendExRateExpiryAlertWithAttachment("Exchange Rate Expire Notification", emailBody, DateTime.Now, "ExchangeRateEXP", "", true, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), memoryStream, ".pdf", FileUrl, "0", "Outbox", Docfilename);

                    }
                }
            }
            catch (Exception ex)
            {

                clsLog.WriteLogAzure(ex);
            }
            finally
            {
                if (db != null)
                {
                    db = null;
                }

            }

        }
        #endregion

        #region Send mail with attachment
        public static int SendExRateExpiryAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName)
        {
            int SerialNo = 0;

            try
            {
                string procedure = "uspAddMessageAttachmentDetails";
                SQLServer dtb = new SQLServer();
                DataSet objDS = null;
                byte[] objBytes = null;

                if (Attachments != null)
                    objBytes = Attachments.ToArray();

                string[] paramname = new string[] { "Subject",
                                                "Body",
                                                "TimeStamp",
                                                "MessageType",
                                                "ErrorDesc",
                                                "IsBlog",
            "FromId", "ToId","Attachment","Extension","FileUrl","isProcessed","MessageBoxType", "AttachmentName"};

                object[] paramvalue = new object[] {subject,
                                                Msg,
                                                TimeStamp,
                                                MessageType,
                                                ErrorDesc,
                                                IsBlog, FromEmailId,ToEmailId, objBytes,AttachmentExtension,FileUrl,isProcessed,MessageBoxType, AttachmentName};

                SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.DateTime,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};

                objDS = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);

                if (objDS != null && objDS.Tables.Count > 0 && objDS.Tables[0].Rows.Count > 0)
                    SerialNo = Convert.ToInt32(objDS.Tables[0].Rows[0][0]);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                SerialNo = 0;
            }

            return SerialNo;
        }

        #endregion
    }
}
