//using EAGetMail;
//using Pop3;
//using System.Text;
//using System.Text.RegularExpressions;
//namespace QidWorkerRole
//{
//    public  class InComingMessageProcessor
//    {
//        GenericFunction gf = new GenericFunction();
       
//        bool f_ReadEmailIMAP = true;
//        string accountEmail = string.Empty;
//        string password = string.Empty;
//        string MailInServer = string.Empty;
//        string  SFTPAddress = string.Empty;
//        string SFTPUserName = string.Empty;
//        string SFTPPassWord = string.Empty;
//        public bool f_SendOutgoingMail = true;
//        public  string BlobKey = string.Empty;
//        public  string BlobName = string.Empty;
//        SCMExceptionHandlingWorkRole scmeception = new SCMExceptionHandlingWorkRole();
//        public InComingMessageProcessor()
//        {
          
//        }
  
//        #region Read Incoming Message from Mail and Sita Folder and FTP Folder and Azzure Drive

//        public void  ReadIncomingMessageFromMailBoxandFTPFolderandAzzure()
//        {
//            try
//            {

//                accountEmail = gf.ReadValueFromDb("msgService_EmailId");
//                password = gf.ReadValueFromDb("msgService_EmailPassword");
//                MailInServer = gf.ReadValueFromDb("msgService_EmailInServer");
//                f_ReadEmailIMAP = (gf.ReadValueFromDb("msgService_EmailServerType")).ToUpper().Contains("IMAP");

//                if (MailInServer != "" && accountEmail != "" && password != "")
//                {
//                    if (f_ReadEmailIMAP)
//                        ReceiveMail_IMAP(MailInServer, accountEmail, password, false);
//                    else
//                        ReceiveMail_POP(MailInServer, accountEmail, password, false);
//                }

//                // Read file from SFTP 
//                FTP ftp = new FTP();
//                ftp.SITASFTPDownloadFile();

//                //Read File From Share drive
//                AzureDrive adrive = new AzureDrive();
//                adrive.ReadFromSITADrive();
//            }
//            catch (Exception ex)
//            {
//                clsLog.WriteLogAzure(ex);
//            }
//        }

//        #region ReadMailfromMailBox


//        private void ReceiveMail_POP(string sServer, string sUserName, string sPassword, bool bSSLConnection)
//        {
//            var popClient = new Pop3Client();
//            try
//            {

//                string EmailSubject = string.Empty;
//                try
//                {
//                    if (popClient.Connected)
//                        popClient.Disconnect();
//                    if (!popClient.Connected)
//                        popClient.Connect(sServer, sUserName, sPassword);
//                }

//                catch (MailServerException ex)
//                {
//                    clsLog.WriteLogAzure(ex.ErrorMessage);
//                    return;
//                }

//                List<Pop3Message> messages = popClient.List();
//                foreach (Pop3Message message in messages)
//                {
//                    try
//                    {
//                        string MessageType = string.Empty;
//                        popClient.Retrieve(message);
//                        if (message.Subject == null)
//                            EmailSubject = string.Empty;
//                        else if ((!message.Subject.ToUpper().Contains("RE")))
//                        {
//                            EmailSubject = string.Empty;
//                            EmailSubject = message.Subject;
//                        }
//                        EmailSubject = message.Subject;
//                        string EmailDate = message.Date;
//                        string ReceivedString = string.Empty;
//                        string EmailFrom = string.Empty;
//                        string EmailTo = string.Empty;
//                        try
//                        {
//                            EmailFrom = message.From;
//                            if (EmailFrom.Contains("<"))
//                            {
//                                int indexOfLessThan = EmailFrom.IndexOf('<');
//                                int indexOfGreaterThan = EmailFrom.IndexOf('>');
//                                string emailfromId = EmailFrom.Substring(indexOfLessThan + 1, ((indexOfGreaterThan - 1) - (indexOfLessThan)));
//                                EmailFrom = emailfromId;
//                            }
//                            EmailTo = message.To;
//                            if (EmailTo.Contains("<"))
//                            {
//                                int indexOfLessThan = EmailTo.IndexOf('<');
//                                int indexOfGreaterThan = EmailTo.IndexOf('>');
//                                string emailToId = EmailTo.Substring(indexOfLessThan + 1, ((indexOfGreaterThan - 1) - (indexOfLessThan)));
//                                EmailTo = emailToId;
//                            }

//                        }
//                        catch (Exception)
//                        {
//                            //scmeception.logexception(ref ex);
//                        }
//                        //Read Meassage Data and store in the string
//                        try
//                        {
//                            if (message.BodyData != null && message.BodyData.Length >= 1)
//                            {
//                                ReceivedString = Encoding.UTF8.GetString(message.BodyData);
//                            }
//                            else
//                            {
//                                clsLog.WriteLogAzure("Mail body is blank.Mail came from " + EmailFrom);
//                                ReceivedString = "Mail body is blank";
//                                popClient.Delete(message);
//                                continue;
//                            }
//                        }
//                        catch (Exception exBody)
//                        {
//                            clsLog.WriteLogAzure("Exception in reading message body: " + exBody.Message);
//                            ReceivedString = "Message not in Plain Text format.";
//                            popClient.Delete(message);
//                            continue;
//                        }
//                        //if (ReceivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
//                        //{
//                        //    popClient.Delete(message);
//                        //    continue;
//                        //}
//                        //ReceivedString = Encoding.UTF8.GetString(message.BodyData);


//                        string FormatName = string.Empty;
//                        if (ReceivedString.Contains("This is a multi-part message in MIME format."))
//                        {
//                            ReceivedString = GetMessageBodyData(ReceivedString);

//                            if ((ReceivedString.Contains("FHL/")) || (ReceivedString.Contains("FWB/")) ||
//                                 (ReceivedString.Contains("FFM/")) || (ReceivedString.Contains("FFR/")) ||
//                                 (ReceivedString.Contains("FBL/")) || (ReceivedString.Contains("FSB")) ||
//                                 (ReceivedString.Contains("FSU/")) || (ReceivedString.Contains("FWR")) ||
//                                 (ReceivedString.Contains("UCM")) || (ReceivedString.Contains("FBR/")) ||
//                                 (ReceivedString.Contains("FNA/")) || (ReceivedString.Contains("FMB/")) ||
//                                 (ReceivedString.Contains("ASM")) || (ReceivedString.Contains("MVT")) || (ReceivedString.Contains("CPM")) || (ReceivedString.Contains("FSN")))
//                            {
//                                if (ReceivedString.Contains("FFR/"))
//                                    FormatName = "FFR";
//                                else if (ReceivedString.Contains("FSN"))
//                                    FormatName = "FSN";
//                                else if (ReceivedString.Contains("CPM"))
//                                    FormatName = "CPM";
//                                else if (ReceivedString.Contains("ASM"))
//                                    FormatName = "ASM";
//                                else if (ReceivedString.Contains("MVT"))
//                                    FormatName = "MVT";
//                                else if (ReceivedString.Contains("FWB/"))
//                                    FormatName = "FWB";
//                                else if (ReceivedString.Contains("FFM/"))
//                                    FormatName = "FFM";
//                                else if (ReceivedString.Contains("FAD/"))
//                                    FormatName = "FAD";
//                                else if (ReceivedString.Contains("FHL/"))
//                                    FormatName = "FHL";
//                                else if (ReceivedString.Contains("FSU/"))
//                                    FormatName = "FSU";
//                                else if (ReceivedString.Contains("UCM"))
//                                    FormatName = "UCM";
//                                else if (ReceivedString.Contains("FBL/"))
//                                    FormatName = "FBL";
//                                else if (ReceivedString.Contains("SCM"))
//                                    FormatName = "SCM";
//                                else if (ReceivedString.Contains("FSB"))
//                                    FormatName = "FSB";
//                                else if (ReceivedString.Contains("FWR/"))
//                                    FormatName = "FWR";
//                                else if (ReceivedString.Contains("FBR/"))
//                                    FormatName = "FBR/";
//                                else if (ReceivedString.Contains("FNA"))
//                                    FormatName = "FNA";
//                                else if (ReceivedString.Contains("FMB"))
//                                    FormatName = "FMB";

//                                ReceivedString = ReceivedString.Substring(ReceivedString.IndexOf(MessageType),
//                                                                                ReceivedString.Length -
//                                                                                ReceivedString.IndexOf(MessageType));

//                                if (ReceivedString.Contains("------=_NextPart"))
//                                {
//                                    int indexOfEndPart = ReceivedString.IndexOf("------=_NextPart");
//                                    ReceivedString = ReceivedString.Substring(0, indexOfEndPart);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            ReceivedString = GetMessageBodyData(ReceivedString);
//                            if ((ReceivedString.Contains("FHL/")) || (ReceivedString.Contains("FWB/")) ||
//                                (ReceivedString.Contains("FFM/")) || (ReceivedString.Contains("FFR/")) ||
//                                (ReceivedString.Contains("FBL/")) || (ReceivedString.Contains("FSB")) ||
//                                (ReceivedString.Contains("FSU/")) || (ReceivedString.Contains("FWR")) ||
//                                (ReceivedString.Contains("UCM")) || (ReceivedString.Contains("FBR/")) ||
//                                (ReceivedString.Contains("FNA/")) || (ReceivedString.Contains("FMB/")) ||
//                                (ReceivedString.Contains("ASM")) || (ReceivedString.Contains("MVT")) || (ReceivedString.Contains("CPM")) || (ReceivedString.Contains("FSN")))
//                            {
//                                if (ReceivedString.Contains("FFR/"))
//                                    FormatName = "FFR";
//                                else if (ReceivedString.Contains("FSN"))
//                                    FormatName = "FSN";
//                                else if (ReceivedString.Contains("CPM"))
//                                    FormatName = "CPM";
//                                else if (ReceivedString.Contains("ASM"))
//                                    FormatName = "ASM";
//                                else if (ReceivedString.Contains("MVT"))
//                                    FormatName = "MVT";
//                                else if (ReceivedString.Contains("FWB/"))
//                                    FormatName = "FWB";
//                                else if (ReceivedString.Contains("FFM/"))
//                                    FormatName = "FFM";
//                                else if (ReceivedString.Contains("FAD/"))
//                                    FormatName = "FAD";
//                                else if (ReceivedString.Contains("FHL/"))
//                                    FormatName = "FHL";
//                                else if (ReceivedString.Contains("FSU/"))
//                                    FormatName = "FSU";
//                                else if (ReceivedString.Contains("UCM"))
//                                    FormatName = "UCM";
//                                else if (ReceivedString.Contains("FBL/"))
//                                    FormatName = "FBL";
//                                else if (ReceivedString.Contains("SCM"))
//                                    FormatName = "SCM";
//                                else if (ReceivedString.Contains("FSB"))
//                                    FormatName = "FSB";
//                                else if (ReceivedString.Contains("FWR/"))
//                                    FormatName = "FWR";
//                                else if (ReceivedString.Contains("FBR/"))
//                                    FormatName = "FBR/";
//                                else if (ReceivedString.Contains("FNA"))
//                                    FormatName = "FNA";
//                                else if (ReceivedString.Contains("FMB"))
//                                    FormatName = "FMB";

//                                ReceivedString = ReceivedString.Substring(ReceivedString.IndexOf(FormatName),
//                                                                            ReceivedString.Length -
//                                                                            ReceivedString.IndexOf(FormatName));

//                                int indexOfFFRTag = ReceivedString.IndexOf("FFR/");
//                                int indexOfFWBTag = ReceivedString.IndexOf("FWB/");
//                                int indexOfFFMTag = ReceivedString.IndexOf("FFM/");
//                                int indexOfFHLTag = ReceivedString.IndexOf("FHL/");
//                                int indexOfFSUTag = ReceivedString.IndexOf("FSU/");

//                                int indexOfFSRTag = ReceivedString.IndexOf("FSB/");
//                                int indexOfFYTTag = ReceivedString.IndexOf("FYT/");
//                                int indexOfFWRTag = ReceivedString.IndexOf("SSM");

//                                if (indexOfFFMTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFFMTag);
//                                else if (indexOfFWBTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFWBTag);
//                                else if (indexOfFFRTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFFRTag);
//                                else if (indexOfFSUTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFSUTag);
//                                else if (indexOfFSRTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFSRTag);
//                                else if (indexOfFYTTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFYTTag);
//                                else if (indexOfFWRTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFWRTag);
//                                else if (indexOfFHLTag != -1)
//                                    ReceivedString = ReceivedString.Substring(indexOfFHLTag);

//                                int indexOfEndPart = ReceivedString.Length;
//                                ReceivedString = ReceivedString.Substring(0, indexOfEndPart);
//                            }
//                        }


//                        if (gf.SaveIncomingMessageInDatabase(EmailSubject, ReceivedString.ToUpper(), EmailFrom, EmailTo, DateTime.Now, DateTime.Now, FormatName == "" ? EmailSubject : FormatName, "ACTIVE", "EMAIL"))
//                        {
//                            clsLog.WriteLogAzure("Email Saved");
//                        }
//                        popClient.Delete(message);


//                    }
//                    catch (MailServerException ex)
//                    {
//                        clsLog.WriteLogAzure(ex.Message);
//                    }
//                }
//                popClient.Disconnect();
//            }
//            catch (Exception ex)
//            {
//                popClient.Disconnect();
//                clsLog.WriteLogAzure(ex.Message);
//            }
//        }

//        private string GetMessageBodyData(string msg)
//        {
//            try
//            {
//                int indexOfbodyStart = 0;
//                int indexOfbodyEnd = 0;
//                if (((msg.Contains("<body") || (msg.Contains("<BODY"))) ||
//                     ((msg.Contains("</body>")) || (msg.Contains("</BODY>")))))
//                {
//                    if (msg.Contains("<body"))
//                    {
//                        indexOfbodyStart = msg.IndexOf("<body");
//                        indexOfbodyEnd = msg.LastIndexOf("</body>");
//                    }
//                    else
//                    {

//                        indexOfbodyStart = msg.IndexOf("<BODY");
//                        if (msg.Contains("BODY>"))
//                            indexOfbodyEnd = msg.LastIndexOf("BODY>");
//                        else
//                            indexOfbodyEnd = msg.LastIndexOf("</BODY>");
//                    }

//                    msg = msg.Substring(indexOfbodyStart, (indexOfbodyEnd - indexOfbodyStart) - 1);
//                    msg = Regex.Replace(msg, @"<(.|\n)*?>", String.Empty);
//                    msg = Regex.Replace(msg, @"\r\n\r\n", Environment.NewLine);
//                    msg = Regex.Replace(msg, @"&nbsp;", String.Empty);
//                    msg = Regex.Replace(msg, @"&amp;", String.Empty);
//                }
//            }
//            catch(Exception ex)
//            {
//                clsLog.WriteLogAzure("Error :", ex);
//            }
//            return msg;
//        }

//        private void ReceiveMail_IMAP(string host, string username, string password, bool bSSLConnection)
//        {
//            try
//            {

//                if (host != "" && username != "" && password != "")
//                {
//                    string _receivedString = string.Empty, MessageType = string.Empty;
//                    TcpIMAP imap = new TcpIMAP();
//                    imap.Connect(host, 143);
//                    imap.AuthenticateUser(username, password);
//                    imap.SelectInbox();
//                    int Count = imap.UnreadMailCount();
//                    string[] arrUID = imap.GetUnreadMsgUids();

//                    for (int i = 0; i < arrUID.Length; i++)
//                    {
//                        try
//                        {
//                            if (string.IsNullOrEmpty(arrUID[i]))
//                                continue;

//                            //   string MailBody = imap.GetBodyByUid(arrUID[i]).ToString().ToUpper();
//                            _receivedString = imap.GetBodyByUid(arrUID[i]).ToString().ToUpper();

//                            if (_receivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
//                            {
//                                imap.Delete(arrUID[i]);
//                                continue;
//                            }

//                            if ((_receivedString.Contains("FHL/")) || (_receivedString.Contains("FWB/")) ||
//                                      (_receivedString.Contains("FFM/")) || (_receivedString.Contains("FFR/")) ||
//                                      (_receivedString.Contains("FBL/")) || (_receivedString.Contains("FSB")) ||
//                                      (_receivedString.Contains("FSU/")) || (_receivedString.Contains("FWR")) ||
//                                      (_receivedString.Contains("UCM")) || (_receivedString.Contains("FBR/")) ||
//                                      (_receivedString.Contains("FNA/")) || (_receivedString.Contains("FMB/")) ||
//                                      (_receivedString.Contains("ASM")) || (_receivedString.Contains("MVT")) || (_receivedString.Contains("CPM")) || (_receivedString.Contains("FSN")) || (_receivedString.Contains("CARDIT")))
//                            {
//                                if (_receivedString.Contains("CARDIT"))
//                                    MessageType = "CARDIT";
//                                if (_receivedString.Contains("FFR/"))
//                                    MessageType = "FFR";
//                                else if (_receivedString.Contains("FSN"))
//                                    MessageType = "FSN";
//                                else if (_receivedString.Contains("CPM"))
//                                    MessageType = "CPM";
//                                else if (_receivedString.Contains("ASM"))
//                                    MessageType = "ASM";
//                                else if (_receivedString.Contains("MVT"))
//                                    MessageType = "MVT";
//                                else if (_receivedString.Contains("FWB/"))
//                                    MessageType = "FWB";
//                                else if (_receivedString.Contains("FFM/"))
//                                    MessageType = "FFM";
//                                else if (_receivedString.Contains("FAD/"))
//                                    MessageType = "FAD";
//                                else if (_receivedString.Contains("FHL/"))
//                                    MessageType = "FHL";
//                                else if (_receivedString.Contains("FSU/"))
//                                    MessageType = "FSU";
//                                else if (_receivedString.Contains("UCM"))
//                                    MessageType = "UCM";
//                                else if (_receivedString.Contains("FBL/"))
//                                    MessageType = "FBL";
//                                else if (_receivedString.Contains("SCM"))
//                                    MessageType = "SCM";
//                                else if (_receivedString.Contains("FSB"))
//                                    MessageType = "FSB";
//                                else if (_receivedString.Contains("FWR/"))
//                                    MessageType = "FWR";
//                                else if (_receivedString.Contains("FBR/"))
//                                    MessageType = "FBR/";
//                                else if (_receivedString.Contains("FNA"))
//                                    MessageType = "FNA";
//                                else if (_receivedString.Contains("FMB"))
//                                    MessageType = "FMB";
//                                if (MessageType.ToUpper() != "CARDIT")
//                                {
//                                    _receivedString = _receivedString.Substring(_receivedString.IndexOf(MessageType),
//                                                                                _receivedString.Length -
//                                                                                _receivedString.IndexOf(MessageType));
//                                }


//                                if (_receivedString.ToUpper().Contains("------=_NEXTPART"))
//                                {
//                                    int indexOfEndPart = _receivedString.IndexOf("------=_NEXTPART");
//                                    _receivedString = _receivedString.Substring(0, indexOfEndPart);
//                                }

//                            }


//                            string Subject = imap.GetSubjectByUid(arrUID[i]).ToString();
//                            if (String.IsNullOrEmpty(Subject))
//                                Subject = "Subject";

//                            string fromEmail = imap.GetFromByUid(arrUID[i]).ToString();
//                            fromEmail = fromEmail.Replace("<", "");
//                            fromEmail = fromEmail.Replace(">", "");

//                            string toEmail = "";
//                            toEmail = toEmail.Replace("<", "");
//                            toEmail = toEmail.Replace(">", "");

//                            string recievedDate = imap.GetDateByUid(arrUID[i]).ToString();
//                            string status = "Active";
//                            // "01 Feb 2013 15:51:12"
//                            DateTime dtRec = DateTime.Now;
//                            DateTime dtSend = DateTime.Now;
//                            try
//                            {
//                                dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
//                                dtSend = dtRec;
//                            }
//                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }

//                            // string status = "Active";
//                            try
//                            {
//                                if (gf.SaveIncomingMessageInDatabase(Subject.ToUpper(), _receivedString.ToUpper(), fromEmail, toEmail, dtRec, dtSend, MessageType == "" ? Subject : MessageType, status, "EMAIL"))
//                                {
//                                    clsLog.WriteLogAzure("Email " + (i + 1) + " Saved");
//                                }
//                            }
//                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }

//                            try
//                            {
//                                imap.Delete(arrUID[i]);
//                            }
//                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
//                        }
//                        catch (Exception)
//                        {
//                            //scmeception.logexception(ref objEx);
//                        }
//                    }
//                }
//            }
//            catch (MailServerException ep)
//            {
//                //Message contains the information returned by mail server
//                Console.WriteLine("Server Respond: {0}", ep.Message);
//            }
//            catch (System.Net.Sockets.SocketException ep)
//            {
//                Console.WriteLine("Socket Error: {0}", ep.Message);
//            }
//            catch (Exception ep)
//            {
//                Console.WriteLine("System Error: {0}", ep.Message);
//            }



//        }
//        #endregion



//        #endregion
        
//    }


//}
