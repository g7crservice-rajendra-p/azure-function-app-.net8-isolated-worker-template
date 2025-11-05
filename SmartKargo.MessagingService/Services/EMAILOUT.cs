using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QID.DataAccess;
using SendGrid;
using SendGrid.Helpers.Mail;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Data.DbConstants;
using SmartKargo.MessagingService.Functions.Activities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace QidWorkerRole
{
    #region EMAILOUT
    /// <summary>
    /// This class provides facility to sending mail.
    /// </summary>
    public class EMAILOUT
    {
        bool success = false;
        private readonly HttpClient _httpClient;
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<EMAILOUT> _logger;
        //private static readonly HttpClient _httpClient = new HttpClient();
        //GenericFunction genericFunction = new GenericFunction();
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public EMAILOUT(HttpClient httpClient,
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<EMAILOUT> logger)
        {
            _httpClient = httpClient;
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }
        #endregion


        #region Send Mail
        /// <summary>
        /// Send Emails in bulk
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>

        public async Task sendEmail(DataSet ds, string accountEmail, string password, string MailsendPort, string OutgoingMailServer, int outport)
        {
            #region variables and objects declaration

            bool sendEmail = false, ishtml = true, isMessageSent = true;
            string ccadd = string.Empty, subject = string.Empty, FileName = string.Empty, status = string.Empty, FileExtension = string.Empty, body = string.Empty, actualMsg = string.Empty, sentadd = string.Empty;
            //SQLServer _readWriteDao = new SQLServer();
            //Cls_BL clsbl = new Cls_BL();
            #endregion

            SmtpClient smtp = new SmtpClient(OutgoingMailServer, outport);
            smtp.Credentials = new NetworkCredential(accountEmail, password);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];
                subject = dr[1].ToString();
                FileName = dr["Subject"].ToString();
                body = dr["Body"].ToString();
                actualMsg = dr["Body"].ToString();
                sentadd = dr["ToID"].ToString().Trim(',');
                if (dr["CCAdd"].ToString().Length > 3)
                {
                    ccadd = dr["CCAdd"].ToString().Trim(',');
                }
                ishtml = bool.Parse(dr["ishtml"].ToString() == "" ? "False" : dr["ishtml"].ToString());
                FileExtension = dr["FileExtension"].ToString().ToUpper().Trim();

                if (ds.Tables[0].Rows[i]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[i]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[i]["STATUS"].ToString().Length < 1)
                {
                    status = "Processed";
                }
                if (ds.Tables[0].Rows[i]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                {
                    status = "Re-Processed";
                }

                try
                {
                    if (sentadd.Length > 2 && sentadd.Contains("@") && sentadd.Contains("."))
                    {
                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            string expression = "MessageId=" + dr["MessageId"].ToString();
                            DataRow[] attchRow = ds.Tables[1].Select(expression);

                            MemoryStream[] Attachments = new MemoryStream[0];
                            string[] Extensions = new string[0];
                            string[] AttachmentName = new string[0];
                            if (AttachmentName.Length > 0)
                            {
                                foreach (DataRow drow in attchRow)
                                {
                                    try
                                    {
                                        Array.Resize(ref Attachments, Attachments.Length + 1);
                                        Attachments[Attachments.Length - 1] = new MemoryStream(clsbl.DownloadBlob(drow["FileUrl"].ToString()));

                                        Array.Resize(ref Extensions, Extensions.Length + 1);
                                        Extensions[Extensions.Length - 1] = drow["MIMEType"].ToString();
                                        if (!Extensions[Extensions.Length - 1].Contains('.'))
                                            Extensions[Extensions.Length - 1] = "." + Extensions[Extensions.Length - 1];
                                        Array.Resize(ref AttachmentName, AttachmentName.Length + 1);
                                        AttachmentName[AttachmentName.Length - 1] = drow["AttachmentName"].ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        isMessageSent = false;
                                        _logger.LogError(ex, "Error in EmailOut for SRNO::{SrNo}", dr[0].ToString());
                                        //clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + );
                                    }
                                }

                                if (sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, Attachments, AttachmentName, Extensions, smtp))
                                {
                                    isMessageSent = true;
                                    _logger.LogInformation("After Sending Mail with Attachment for MessageID : {MessageID}", dr[0].ToString());
                                    //clsLog.WriteLogAzure("After Sending Mail with Attachment for MessageID :" + dr[0].ToString());

                                    //string[] pname = { "num", "Status" };
                                    //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                    //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                    //if (_readWriteDao.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                    //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());

                                    SqlParameter[] parameters =
                                    [
                                        new("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                        new("@Status", SqlDbType.VarChar) { Value = status }
                                    ];
                                    var dbRes = await _readWriteDao.ExecuteNonQueryAsync(StoredProcedures.MailSent, parameters);
                                    if (dbRes)
                                    {
                                        _logger.LogInformation("Email Sent successfully to:{MessageID}",dr[0].ToString());
                                        //clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Email send failed for :{MessageID}", dr[0].ToString());
                                    }

                                }
                                else
                                {
                                    isMessageSent = false;
                                    //clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                    _logger.LogWarning("Error in EmailOut for SRNO::{SrNo}", dr[0].ToString());

                                }
                            }
                            else
                            {
                                sendEmail = true;
                            }

                        }
                        else
                        {
                            sendEmail = true;
                        }

                        if (sendEmail)
                        {
                            #region Send Email
                            if (sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, ""))
                            {
                                isMessageSent = true;

                                //string[] pname = { "num", "Status" };
                                //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                //if (_readWriteDao.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                //{
                                //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                //}

                                SqlParameter[] parameters =
                                    [
                                        new("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                        new("@Status", SqlDbType.VarChar) { Value = status }
                                    ];
                                var dbRes = await _readWriteDao.ExecuteNonQueryAsync(StoredProcedures.MailSent, parameters);
                                if (dbRes)
                                {
                                    _logger.LogInformation("Email Sent successfully to:{MessageID}", dr[0].ToString());
                                    //clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                }
                                else
                                {
                                    _logger.LogWarning("Email send failed for :{MessageID}", dr[0].ToString());
                                }
                            }
                            else
                            {
                                isMessageSent = false;
                                //clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                _logger.LogWarning("Error in EmailOut for SRNO::{SrNo}", dr[0].ToString());
                            }

                            #endregion

                        }

                    }


                    if (!isMessageSent)
                    {
                        //string ErrorMsg = "Error occured while processing sending request";
                        //string[] pname = { "num", "Status", "ErrorMsg", "MsgDeliveryType" };
                        //object[] pvalue = { int.Parse(dr[0].ToString()), status, ErrorMsg, "EMAIL".ToUpper().Trim() };
                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                        //if (_readWriteDao.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                        //    clsLog.WriteLogAzure("Fail to sent email to:" + dr[0].ToString());

                        string ErrorMsg = "Error occured while processing sending request";
                        SqlParameter[] parameters =
                        [
                            new("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                            new("@Status", SqlDbType.VarChar) { Value = status },
                            new("@ErrorMsg", SqlDbType.VarChar) { Value = ErrorMsg },
                            new("@MsgDeliveryType", SqlDbType.VarChar) { Value = "EMAIL".ToUpper().Trim() }
                        ];
                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync(StoredProcedures.MailSent, parameters);
                        if (dbRes)
                        {
                            _logger.LogInformation("Email Sent successfully to:{MessageID}", dr[0].ToString());
                            //clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                        }
                        else
                        {
                            _logger.LogWarning("Email send failed for :{MessageID}", dr[0].ToString());
                        }
                    }
                }
                catch (Exception ex) 
                {
                    //clsLog.WriteLogAzure(ex); 
                    _logger.LogError(ex, "Error on sendEmail");
                }
            }
        }

        public bool sendMail(string fromEmailId, string toEmailId, string password, string subject, string body, bool isBodyHTML, int outmailport, string CCEmailID, MemoryStream[] Document, string[] DocumentName, string[] Extension, SmtpClient smtpclient = null)
        {

            #region New Code For Attached Emails Deepak
            bool flag = false;
            try
            {
                string OutgoingMailServer = genericFunction.ReadValueFromDb("msgService_EmailOutServer");
                MailMessage Mail = new MailMessage();

                if (genericFunction.ReadValueFromDb("DoNotReplyEmailID") != "")
                    Mail.From = new MailAddress(genericFunction.ReadValueFromDb("DoNotReplyEmailID"));
                else
                    Mail.From = new MailAddress(fromEmailId);

                if (CCEmailID.Length > 3)
                {
                    Mail.CC.Add(CCEmailID);
                }
                Mail.Subject = subject;
                Mail.IsBodyHtml = isBodyHTML;
                Mail.Body = body;

                if (smtpclient == null)
                {
                    SmtpClient smtp = new SmtpClient(OutgoingMailServer, outmailport);
                    smtp.Credentials = new NetworkCredential(fromEmailId, password);
                    if (OutgoingMailServer.ToUpper().Contains("GMAIL") || OutgoingMailServer.ToUpper().Contains("OFFICE365") || OutgoingMailServer.ToUpper().Contains("SMTP.1AND1.COM"))
                        smtp.EnableSsl = true;
                    smtpclient = smtp;
                }
                Mail.Priority = MailPriority.High;
                for (int i = 0; i < Document.Length; i++)
                {
                    Mail.Attachments.Add(new System.Net.Mail.Attachment(Document[i], DocumentName[i] + Extension[i]));
                }
                if (toEmailId.Length > 200)
                {
                    string tempToEmail = string.Empty;
                    string[] arrEmail = toEmailId.Split(',');
                    for (int i = 0; i < arrEmail.Length; i++)
                    {
                        if ((tempToEmail.Length + arrEmail[i].Length) < 200)
                        {
                            tempToEmail = tempToEmail == string.Empty ? arrEmail[i] : tempToEmail + "," + arrEmail[i];
                            Mail.To.Add(arrEmail[i]);
                        }
                        else
                        {
                            smtpclient.Send(Mail);
                            Mail.To.Clear();
                            Mail.To.Add(arrEmail[i]);
                            tempToEmail = arrEmail[i];
                        }
                    }
                    if (Mail.To.Count > 0)
                    {
                        smtpclient.Send(Mail);
                    }
                }
                else
                {
                    Mail.To.Add(toEmailId);
                    smtpclient.Send(Mail);
                }

                flag = true;
                clsLog.WriteLogAzure("Mail Sent @ " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                clsLog.WriteLogAzure("Exception while collection Mail Info : ", ex);
                flag = false;
            }
            return flag;
            #endregion
        }

        public bool sendMail(string fromEmailId, string toEmailId, string password, string subject, string body, bool isBodyHTML, int outmailport, string CCEmailID, string messageType, string OutgoingMailServer = "")
        {
            bool flag = false;
            try
            {
                if (OutgoingMailServer.Trim() == string.Empty)
                    OutgoingMailServer = genericFunction.ReadValueFromDb("msgService_EmailOutServer");

                clsLog.WriteLogAzure("OutgoingMailServer : " + OutgoingMailServer);
                MailMessage Mail = new MailMessage();

                if (genericFunction.ReadValueFromDb("DoNotReplyEmailID") != "")
                    Mail.From = new MailAddress(genericFunction.ReadValueFromDb("DoNotReplyEmailID"));
                else
                    Mail.From = new MailAddress(fromEmailId);

                if (CCEmailID.Length > 3)
                {
                    Mail.CC.Add(CCEmailID);
                }
                Mail.Subject = subject;
                Mail.IsBodyHtml = isBodyHTML;
                Mail.Body = body;

                SmtpClient smtp = new SmtpClient(OutgoingMailServer, outmailport);
                smtp.Credentials = new NetworkCredential(fromEmailId, password);
                Mail.Priority = MailPriority.High;
                if (OutgoingMailServer.ToUpper().Contains("GMAIL") || OutgoingMailServer.ToUpper().Contains("OFFICE365") || OutgoingMailServer.ToUpper().Contains("SMTP.1AND1.COM"))
                    smtp.EnableSsl = true;
                if (toEmailId.Length > 200)
                {
                    string tempToEmail = string.Empty;
                    string[] arrEmail = toEmailId.Split(',');
                    for (int i = 0; i < arrEmail.Length; i++)
                    {
                        if ((tempToEmail.Length + arrEmail[i].Length) < 200)
                        {
                            tempToEmail = tempToEmail == string.Empty ? arrEmail[i] : tempToEmail + "," + arrEmail[i];
                            Mail.To.Add(arrEmail[i]);
                        }
                        else
                        {
                            smtp.Send(Mail);
                            Mail.To.Clear();
                            Mail.To.Add(arrEmail[i]);
                            tempToEmail = arrEmail[i];
                        }
                    }
                    if (Mail.To.Count > 0)
                    {
                        clsLog.WriteLogAzure("In toEmailId.Length > 200 ");
                        smtp.Send(Mail);
                    }
                }
                else
                {
                    clsLog.WriteLogAzure("In Mail.To.Add(toEmailId) ");
                    Mail.To.Add(toEmailId);
                    smtp.Send(Mail);
                }
                flag = true;

            }
            catch (Exception ex)
            {

                //SCMExceptionHandling.logexception(ref ex);

                clsLog.WriteLogAzure("Exception while collection Mail Info : ", ex);
                flag = false;
            }
            clsLog.WriteLogAzure("return flag from SENDMAIL():" + flag.ToString());
            return flag;
        }

        private string SetDoNotReplyFromEmailID(string fromEmailId, string messageType)
        {
            try
            {
                string doNotReplyEmailID = genericFunction.ReadValueFromDb("DoNotReplyEmailID");
                string doNotReplyAlertTypes = genericFunction.ReadValueFromDb("DoNotReplyAlertTypes");
                string[] arrAlertType = doNotReplyAlertTypes.Split(',');
                if (doNotReplyEmailID != string.Empty && doNotReplyAlertTypes != string.Empty && arrAlertType.Contains(messageType))
                    fromEmailId = doNotReplyEmailID;

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return fromEmailId;
        }

        public bool sendMail(string fromEmailId, string toEmailId, string password, string subject, string body, bool isBodyHTML, int outmailport, string CCEmailID, MemoryStream[] Document, string[] DocumentName, string[] Extension, string messageType, string OutgoingMailServer)
        {

            #region New Code For Attached Emails Deepak
            bool flag = false;
            try
            {
                MailMessage Mail = new MailMessage();

                if (genericFunction.ReadValueFromDb("DoNotReplyEmailID") != "")
                    Mail.From = new MailAddress(genericFunction.ReadValueFromDb("DoNotReplyEmailID"));
                else
                    Mail.From = new MailAddress(fromEmailId);

                if (CCEmailID.Length > 3)
                {
                    Mail.CC.Add(CCEmailID);
                }
                Mail.Subject = subject;
                Mail.IsBodyHtml = isBodyHTML;
                Mail.Body = body;

                SmtpClient smtp = new SmtpClient(OutgoingMailServer, outmailport);
                smtp.Credentials = new NetworkCredential(fromEmailId, password);
                Mail.Priority = MailPriority.High;
                if (OutgoingMailServer.ToUpper().Contains("GMAIL") || OutgoingMailServer.ToUpper().Contains("OFFICE365") || OutgoingMailServer.ToUpper().Contains("SMTP.1AND1.COM"))
                    smtp.EnableSsl = true;
                for (int i = 0; i < Document.Length; i++)
                {
                    Mail.Attachments.Add(new System.Net.Mail.Attachment(Document[i], DocumentName[i] + Extension[i]));
                }
                if (toEmailId.Length > 200)
                {
                    string tempToEmail = string.Empty;
                    string[] arrEmail = toEmailId.Split(',');
                    for (int i = 0; i < arrEmail.Length; i++)
                    {
                        if ((tempToEmail.Length + arrEmail[i].Length) < 200)
                        {
                            tempToEmail = tempToEmail == string.Empty ? arrEmail[i] : tempToEmail + "," + arrEmail[i];
                            Mail.To.Add(arrEmail[i]);
                        }
                        else
                        {
                            smtp.Send(Mail);
                            Mail.To.Clear();
                            Mail.To.Add(arrEmail[i]);
                            tempToEmail = arrEmail[i];
                        }
                    }
                    if (Mail.To.Count > 0)
                    {
                        smtp.Send(Mail);
                    }
                }
                else
                {
                    Mail.To.Add(toEmailId);
                    smtp.Send(Mail);
                }

                flag = true;
                clsLog.WriteLogAzure("Mail Sent @ " + DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                clsLog.WriteLogAzure("Exception while collection Mail Info : ", ex);
                flag = false;
            }
            return flag;
            #endregion
        }

        public bool SendEmailOutSendgrid(DataSet ds, string accountEmail, string sentadd, string password, string subject, string body, bool ishtml, string CCEmailId, string FileName = "")
        {

            #region SendGrid Email send

            try
            {
                string apiKey = password;
                Cls_BL clsbl = new Cls_BL();

                string[] emailids = sentadd.Split(',');
                string[] ccEmailids = CCEmailId.Split(',');

                var personlisation = new Personalization
                {
                    Tos = new List<EmailAddress>(emailids.Count()),

                };
                foreach (var tos in emailids)
                {
                    if (tos.Length > 0)
                        personlisation.Tos.Add(new EmailAddress(tos));
                }


                int xCCEmail = 0;
                foreach (var ccs in ccEmailids)
                {

                    if (ccs.Length > 0)
                    {
                        if (xCCEmail == 0)
                        {
                            personlisation.Ccs = new List<EmailAddress>(ccEmailids.Count());
                        }
                        personlisation.Ccs.Add(new EmailAddress(ccs));
                    }
                    xCCEmail++;
                }



                var client = new SendGridClient(_httpClient, new SendGridClientOptions { ApiKey = apiKey, HttpErrorAsException = true });

                var cl = new SendGridClient(new SendGridClientOptions { ApiKey = apiKey, HttpErrorAsException = true });
                SendGridMessage message;
                if (ishtml)
                {
                    message = new SendGridMessage
                    {

                        Subject = FileName,
                        From = new EmailAddress(accountEmail),
                        HtmlContent = body,


                    };
                }
                else
                {
                    message = new SendGridMessage
                    {

                        Subject = FileName,
                        From = new EmailAddress(accountEmail),
                        PlainTextContent = body,

                    };
                }


                if (ds != null)
                {
                    if (ds.Tables[1].Rows.Count > 0)
                    {


                        int x = 0;

                        foreach (DataRow drow in ds.Tables[1].Rows)
                        {
                            try
                            {
                                x++;

                                string[] filenames = drow["FileUrl"].ToString().Split('/');

                                if (drow["FileUrl"].ToString().Length > 0)
                                {
                                    string type = "";

                                    byte[] byteData12 = clsbl.DownloadBlob(drow["FileUrl"].ToString());

                                    switch (drow["MIMEType"].ToString().ToUpper())
                                    {
                                        case "PDF":
                                        case ".PDF":
                                            type = "application/pdf";
                                            break;

                                        case "TXT":
                                        case ".TXT":
                                            type = "application/text";
                                            break;

                                        case "PNG":
                                        case ".PNG":
                                            type = "image/png";
                                            break;

                                        case "JPEG":
                                        case ".JPEG":
                                            type = "image/jpeg";
                                            break;

                                        case ".ZIP":
                                        case "ZIP":
                                            type = "application/pdf";
                                            break;

                                        case ".XLSX":
                                        case "XLSX":
                                            type = "text/html";
                                            break;

                                        case ".XLS":
                                        case "XLS":
                                            type = "text/html";
                                            break;

                                        case "JPG":
                                        case ".JPG":
                                            type = "image/jpeg";
                                            break;

                                        case "GIF":
                                        case ".GIF":
                                            type = "image/gif";
                                            break;

                                        case "DOCX":
                                        case ".DOCX":
                                            type = "application/pdf";
                                            break;

                                        case "DOC":
                                        case ".DOC":
                                            type = "application/pdf";
                                            break;

                                        default:
                                            type = "application/octet-stream";
                                            break;
                                    }


                                    message.AddAttachment(new SendGrid.Helpers.Mail.Attachment
                                    {
                                        Content = Convert.ToBase64String(byteData12),
                                        Filename = filenames[filenames.Length - 1].ToString(),
                                        ContentId = x.ToString(),
                                        Type = type,
                                        Disposition = "attachment"
                                    });

                                }
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex); }
                        }

                    }
                }

                message.Personalizations = new List<Personalization>(1)
                                        {
                                        personlisation
                                        };

                SendEmailMessage(message, client);



            }

            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Exception in send grid functionality" + ex);

            }


            #endregion
            return success;
        }
        private void SendEmailMessage(SendGridMessage message, SendGridClient client)
        {
            try
            {

                var result = Task.Run(async () => await client.SendEmailAsync(message)).Result;

                if (result.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    success = true;
                }

                else
                    success = false;


            }
            catch (Exception ex)
            {
                //scmexception.logexception(ref ex);
                clsLog.WriteLogAzure("SendEmailMessage Function Error:", ex);
            }
        }
        #endregion

    }
    #endregion

}
