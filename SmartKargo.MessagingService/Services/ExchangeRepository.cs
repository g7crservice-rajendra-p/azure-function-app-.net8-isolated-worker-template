using EAGetMail;
using System.Globalization;

namespace QidWorkerRole
{
    public class ExchangeRepository
    {
        // Generate an unqiue email file name based on date time
        static string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
                currentDateTime.Millisecond,
                sequence);
        }

        /*Not in use*/
        public void ReadMail()
        {
            try
            {
                // Create a folder named "inbox" under current directory
                //// to save the email retrieved.
                //string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
                //// If the folder is not existed, create it.
                //if (!Directory.Exists(localInbox))
                //{
                //    Directory.CreateDirectory(localInbox);
                //}

                MailServer oServer = new MailServer("exchange2019.ionos.com",
                            "airasia@smartkargomsg.com",
                            "SCMAKLive#1$4",
                            ServerProtocol.Imap4);

                // Enable SSL/TLS connection, most modern email server require SSL/TLS by default
                oServer.SSLConnection = true;
                oServer.Port = 993;

                // if your server doesn't support SSL/TLS, please use the following codes
                // oServer.SSLConnection = false;
                // oServer.Port = 143;

                MailClient oClient = new MailClient("");
                oClient.Connect(oServer);

                MailInfo[] infos = oClient.GetMailInfos();
                Console.WriteLine("Total {0} email(s)\r\n", infos.Length);
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];
                    Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                        info.Index, info.Size, info.UIDL);

                    // Receive email from IMAP4 server
                    Mail oMail = oClient.GetMail(info);

                    Console.WriteLine("From: {0}", oMail.From.ToString());
                    Console.WriteLine("Subject: {0}\r\n", oMail.Subject);

                    // Generate an unqiue email file name based on date time.
                    string fileName = _generateFileName(i + 1);
                    //string fullPath = string.Format("{0}\\{1}", localInbox, fileName);

                    // Save email to local disk
                    //oMail.SaveAs(fullPath, true);

                    // Mark email as deleted from IMAP4 server.
                    oClient.Delete(info);
                }

                // Quit and expunge emails marked as deleted from IMAP4 server.
                oClient.Quit();
                Console.WriteLine("Completed!");
            }
            catch (Exception ep)
            {
                Console.WriteLine(ep.Message);
            }
        }

    }
}