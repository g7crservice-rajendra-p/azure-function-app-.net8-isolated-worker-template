using QID.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole
{
    public class UnDepartedAWBListAlert
    {
        #region Variables
        SQLServer db;
        #endregion
        #region Main function
        public void UnDepartedAWBListListener()
        {
            try
            {
                db = new SQLServer();
                DataSet ds = null;

                ds = db.SelectRecords("uspGetUnDepartedAWBList");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0) && (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0))
                    {
                        string htmlContent;
                        StringBuilder sbUnDepartedAWBList = new StringBuilder(string.Empty);
                        GenericFunction genericFunction = new GenericFunction();
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sbUnDepartedAWBList.Append(Convert.ToString(ds.Tables[0].Rows[i]["body"].ToString()));
                        }
                        sbUnDepartedAWBList.Append("</table>");
                        htmlContent = sbUnDepartedAWBList.ToString();

                        NReco.PdfGenerator.HtmlToPdfConverter htmlToPdfConverter = null;
                        MemoryStream memoryStream = null;
                        htmlToPdfConverter = new NReco.PdfGenerator.HtmlToPdfConverter();
                        htmlToPdfConverter.Margins.Top = 5.0f;
                        htmlToPdfConverter.Margins.Bottom = 10.0f;
                        htmlToPdfConverter.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                        var pdfbyteArray = htmlToPdfConverter.GeneratePdf(htmlContent);
                        var byteArray = Encoding.ASCII.GetBytes(htmlContent);
                        MemoryStream msExcel = new MemoryStream(byteArray);
                        memoryStream = new MemoryStream(pdfbyteArray);
                        string Docfilename = "UnDepartedAWBList" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        String FileUrl = genericFunction.UploadToBlob(memoryStream, Docfilename + ".pdf", "UnDepAWBs");
                        string FileExcelURL = genericFunction.UploadToBlob(msExcel, Docfilename + ".xls", "UnDepAWBs");

                        string emailBody = "\r\nHi, \r\n\tPlease see attached Un-departed AWB list on date("+ ds.Tables[1].Rows[0]["UnDepAWBsOnDate"].ToString() + ").\r\n\r\n Thanks.\r\n\r\n Best Regards,\r\n\r\n" + ds.Tables[1].Rows[0]["ClientName"].ToString() + ".";

                        string EmailID = string.Empty;
                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            EmailID = ds.Tables[1].Rows[0]["Emailids"].ToString();
                        }

                        if (!string.IsNullOrEmpty(EmailID))
                        {
                            SendUnDepartedAWBListAlertWithAttachment("Undeparted AWB list notification", emailBody, DateTime.Now, "UnDepAWBListAlert", "", true, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), memoryStream, ".pdf", FileUrl, "0", "Outbox", Docfilename, FileExcelURL, msExcel);
                        }
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
        public static int SendUnDepartedAWBListAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName, string FileUrlExcel = null, MemoryStream attachExcel = null)
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
            "FromId", "ToId","Attachment","Extension","FileUrl","isProcessed","MessageBoxType", "AttachmentName","AttachmentExcel","FileUrlExcel"};

                object[] paramvalue = new object[] {subject,
                                                Msg,
                                                TimeStamp,
                                                MessageType,
                                                ErrorDesc,
                                                IsBlog, FromEmailId,ToEmailId, objBytes,AttachmentExtension,FileUrl,isProcessed,MessageBoxType, AttachmentName,attachExcel,FileUrlExcel};

                SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.DateTime,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.VarChar,
                                                     SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar};

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
