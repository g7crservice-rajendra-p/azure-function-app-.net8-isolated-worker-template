/**********************************************************
 * Object Name: Rate Expiry Alert
 * Prepared By: Sushant Gavas
 * Prepared On: 28 FEB 2020
 * Description: This class provides for performing operations 
 *              related to send email to user for rateline expire alert
***********************************************************/

using System;
using System.Data;
using System.IO;
using System.Text;
using QID.DataAccess;

namespace QidWorkerRole
{
   public class RateExpiryAlert
    {
        #region Variables
        //public static string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        SQLServer db;
        #endregion

        #region Main function
        /// <summary>
        /// This function send a rate expiry details to user through email.
        /// </summary>
        public void RateExpiryListener()
        {
            try
            {
                db = new SQLServer();
                DataSet ds = null;

                ds = db.SelectRecords("USPGETAGENTRATEEXPIRYDETAILS");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        string htmlContent;
                        StringBuilder StbRateExpiryData = new StringBuilder(string.Empty);
                        GenericFunction genericFunction = new GenericFunction();
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
                        String FileUrl = genericFunction.UploadToBlob(memoryStream, Docfilename, "sis");

                        string emailBody = "Hi, the followed rateline will be expired soon. Please see the attached file.";
                        string emailtext = "Hi, " + "</BR></BR>" + htmlContent;
                        //bool flg = insertData("Rate Line Expire Notification", emailtext, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), DateTime.Now.ToString(), DateTime.Now.ToString(), "RATEEXP", "");

                        SendRateExpiryAlertWithAttachment("Rate Line Expire Notification", emailBody, DateTime.Now, "RATEEXP", "", true, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), memoryStream, ".pdf", FileUrl, "0", "Outbox", Docfilename);

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
        public static int SendRateExpiryAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName)
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

        #region Inset Data into tblOutbox
        public bool insertData(string subject, string body, string fromID, String ToId, string sendOn, string recievedOn, string type, string status)
        {
            bool flag = false;
            try
            {
                //SQLServer db = new SQLServer();//new SQLServer(conStr);
                string[] param = { "Subject", "Body", "FromEmailID", "ToEmailID", "CreatedOn", "Agentcode", "refNo", "isInternal", "Type" , "IsHTML"};
                SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                object[] values = { subject, body, fromID, ToId, DateTime.Now, "", 0, 0, type, 1 };
                flag = db.InsertData("spInsertMsgToOutbox", param, sqldbtypes, values);

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        #endregion
    }
}
