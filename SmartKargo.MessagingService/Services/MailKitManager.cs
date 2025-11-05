using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Net.Mail;
using System.Data;
using QID.DataAccess;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Http;
using System.Threading.Tasks;

using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace QidWorkerRole
{
    public class MailKitManager
    {
        bool success = false;
        public bool SendEmailMailKitManager(DataSet ds, string accountEmail, string sentadd, string password, string subject, string body, bool ishtml, string CCEmailId, string FileName = "",string SMTPUserName="",string MailIouterver="")
        {

            #region MailKit Email send

            try
            {

                var message = new MimeMessage();

                var attachment = new MimePart();

                Cls_BL clsbl = new Cls_BL();


                // Sender address
                message.From.Add(new MailboxAddress(subject, accountEmail)); // Must be verified in SES


                // Split and add each email using loop
                foreach (var email in sentadd.Split(','))
                {
                    message.To.Add(MailboxAddress.Parse(email.Trim()));
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
                                                                        

                                    // Let's assume this is your content type string (e.g., "application/pdf")
                                    string contentTypeString = type;

                                    // Parse it into a ContentType object
                                    var contentType = new ContentType(contentTypeString.Split('/')[0], contentTypeString.Split('/')[1]);


                                    // Now use this in MimePart
                                    attachment = new MimePart(contentType)
                                    {
                                        Content = new MimeContent(new MemoryStream(byteData12)),
                                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                                        ContentTransferEncoding = ContentEncoding.Base64,
                                        FileName = filenames[filenames.Length - 1],
                                        ContentId = x.ToString()
                                    };

                                }
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex); }
                        }

                    }
                }

                // 4. Combine text and attachment into multipart
                var multipart = new Multipart("mixed");

                if (ishtml)
                {
                    
                    multipart.Add(new TextPart("html")
                    {
                        Text = body
                    });
                }
                else
                {
                    
                    multipart.Add(new TextPart("plain")
                    {
                        Text = body
                    });
                }


                // ✅ Add attachment (MimePart created correctly)
                multipart.Add(attachment);

                // ✅ Assign to message
                message.Body = multipart;


                //string smtpServer = "email-smtp.ap-southeast-1.amazonaws.com";
                //int smtpPort = 587; // or 465 for SSL
                //string smtpUsername = "AKIA3NBBODN2KRU2V37H";
                //string smtpPassword = "BCBoLhsiGE1zym3GTt0chODjM3I1SiE4e+aMlNwBDkeX";

                string smtpServer = MailIouterver;
                int smtpPort = 587; // or 465 for SSL
                string smtpUsername = SMTPUserName;
                string smtpPassword = password;

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // Accept TLS only
                    client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);

                    client.Authenticate(smtpUsername, smtpPassword);
                    client.Send(message);
                    client.Disconnect(true);

                    success = true;
                }

            }

            catch (Exception ex)
            {                
                clsLog.WriteLogAzure("Exception in send grid functionality" + ex);

            }


            #endregion
            return success;
        }

    }
}
