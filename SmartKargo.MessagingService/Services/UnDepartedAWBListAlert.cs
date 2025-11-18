using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text;
using NReco.PdfGenerator;
using Microsoft.Data.SqlClient;

namespace QidWorkerRole
{
    public class UnDepartedAWBListAlert
    {
        #region Variables
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<UnDepartedAWBListAlert> _logger;
        private readonly GenericFunction _genericFunction;
        //SQLServer db;
        #endregion
        #region Constructor
        public UnDepartedAWBListAlert(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<UnDepartedAWBListAlert> logger,
            GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }
        #endregion
        #region Main function
        public async Task UnDepartedAWBListListener()
        {
            try
            {
                //db = new SQLServer();
                DataSet? ds = null;

                //ds = db.SelectRecords("uspGetUnDepartedAWBList");
                ds = await _readWriteDao.SelectRecords("uspGetUnDepartedAWBList");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0 && (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0) && (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0))
                    {
                        string htmlContent;
                        StringBuilder sbUnDepartedAWBList = new StringBuilder(string.Empty);
                        //GenericFunction genericFunction = new GenericFunction();

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            sbUnDepartedAWBList.Append(Convert.ToString(ds.Tables[0].Rows[i]["body"].ToString()));
                        }
                        sbUnDepartedAWBList.Append("</table>");
                        htmlContent = sbUnDepartedAWBList.ToString();

                        HtmlToPdfConverter htmlToPdfConverter = null;
                        MemoryStream memoryStream = null;
                        htmlToPdfConverter = new HtmlToPdfConverter();
                        htmlToPdfConverter.Margins.Top = 5.0f;
                        htmlToPdfConverter.Margins.Bottom = 10.0f;
                        htmlToPdfConverter.Orientation = PageOrientation.Landscape;
                        var pdfbyteArray = htmlToPdfConverter.GeneratePdf(htmlContent);
                        var byteArray = Encoding.ASCII.GetBytes(htmlContent);
                        MemoryStream msExcel = new MemoryStream(byteArray);
                        memoryStream = new MemoryStream(pdfbyteArray);
                        string Docfilename = "UnDepartedAWBList" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        String FileUrl = _genericFunction.UploadToBlob(memoryStream, Docfilename + ".pdf", "UnDepAWBs");
                        string FileExcelURL = _genericFunction.UploadToBlob(msExcel, Docfilename + ".xls", "UnDepAWBs");

                        string emailBody = "\r\nHi, \r\n\tPlease see attached Un-departed AWB list on date(" + ds.Tables[1].Rows[0]["UnDepAWBsOnDate"].ToString() + ").\r\n\r\n Thanks.\r\n\r\n Best Regards,\r\n\r\n" + ds.Tables[1].Rows[0]["ClientName"].ToString() + ".";

                        string EmailID = string.Empty;
                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            EmailID = ds.Tables[1].Rows[0]["Emailids"].ToString();
                        }

                        if (!string.IsNullOrEmpty(EmailID))
                        {
                            await SendUnDepartedAWBListAlertWithAttachment("Undeparted AWB list notification", emailBody, DateTime.Now, "UnDepAWBListAlert", "", true, "", ds.Tables[1].Rows[0]["Emailids"].ToString(), memoryStream, ".pdf", FileUrl, "0", "Outbox", Docfilename, FileExcelURL, msExcel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error in UnDepartedAWBListListener: {ErrorMessage}", ex.Message);
            }

            //finally
            //{
            //if (db != null)
            //{
            //    db = null;
            //}

            //}

        }
        #endregion

        #region Send mail with attachment
        public async Task<int> SendUnDepartedAWBListAlertWithAttachment(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog, string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string AttachmentName, string FileUrlExcel = null, MemoryStream attachExcel = null)
        {
            int SerialNo = 0;

            try
            {
                //string procedure = "uspAddMessageAttachmentDetails";
                //SQLServer dtb = new SQLServer();
                DataSet objDS = null;
                byte[] objBytes = null;

                if (Attachments != null)
                    objBytes = Attachments.ToArray();

                //    string[] paramname = new string[] { "Subject",
                //                                    "Body",
                //                                    "TimeStamp",
                //                                    "MessageType",
                //                                    "ErrorDesc",
                //                                    "IsBlog",
                //"FromId", "ToId","Attachment","Extension","FileUrl","isProcessed","MessageBoxType", "AttachmentName","AttachmentExcel","FileUrlExcel"};

                //    object[] paramvalue = new object[] {subject,
                //                                    Msg,
                //                                    TimeStamp,
                //                                    MessageType,
                //                                    ErrorDesc,
                //                                    IsBlog, FromEmailId,ToEmailId, objBytes,AttachmentExtension,FileUrl,isProcessed,MessageBoxType, AttachmentName,attachExcel,FileUrlExcel};

                //    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.DateTime,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar};

                SqlParameter[] sqlParameters = new SqlParameter[] {
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
                    new SqlParameter("@AttachmentName", SqlDbType.VarChar) { Value = AttachmentName },
                    new SqlParameter("@AttachmentExcel", SqlDbType.VarBinary) { Value = attachExcel },
                    new SqlParameter("@FileUrlExcel", SqlDbType.VarChar) { Value = FileUrlExcel }
                };
                //objDS = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);
                objDS = await _readWriteDao.SelectRecords("uspAddMessageAttachmentDetails", sqlParameters);

                if (objDS != null && objDS.Tables.Count > 0 && objDS.Tables[0].Rows.Count > 0)
                    SerialNo = Convert.ToInt32(objDS.Tables[0].Rows[0][0]);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                SerialNo = 0;
                _logger.LogError(ex, "Error in SendUnDepartedAWBListAlertWithAttachment: {ErrorMessage}", ex.Message);
            }

            return SerialNo;
        }

        #endregion
    }
}
