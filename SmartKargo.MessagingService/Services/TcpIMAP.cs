using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using QidWorkerRole;
using Microsoft.Extensions.Logging;

namespace QidWorkerRole
{
    public class TcpIMAP
    {

        private TcpClient _imapClient;
        private NetworkStream _imapNs;
        private StreamWriter _imapSw;
        private StreamReader _imapSr;

        private readonly ILogger<TcpIMAP> _logger;
        SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();
        public TcpIMAP(ILogger<TcpIMAP> logger)
        {
            _logger = logger;
        }

        public TcpIMAP(string hostname, int port)
        {
            InitializeConnection(hostname, port);
        }

        public void Connect(string hostname, int port)
        {
            InitializeConnection(hostname, port);
        }

        private void InitializeConnection(string hostname, int port)
        {
            try
            {
                _imapClient = new TcpClient(hostname, port);
                _imapNs = _imapClient.GetStream();
                _imapSw = new StreamWriter(_imapNs);
                _imapSr = new StreamReader(_imapNs);

                // Console.WriteLine("*** Connected ***");
                _logger.LogInformation("*** Connected ***");
                Response();
            }
            catch (SocketException ex)
            {
                // Console.WriteLine(ex.Message);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }


        public void AuthenticateUser(string username, string password)
        {
            _imapSw.WriteLine("$ LOGIN " + username + " " + password);
            _imapSw.Flush();
            Response();
        }


        public int MailCount()
        {
            _imapSw.WriteLine("$ STATUS INBOX (messages)");
            _imapSw.Flush();

            string res = Response();
            Match m = Regex.Match(res, "[0-9]*[0-9]");
            return Convert.ToInt32(m.ToString());
        }
        public int UnreadMailCount()
        {
            try
            {
                _imapSw.WriteLine("$ STATUS INBOX (unseen)");
                _imapSw.Flush();

                string res = Response();
                Match m = Regex.Match(res, "[0-9]*[0-9]");
                return Convert.ToInt32(m.ToString());
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return 0;
        }

        public string[] GetUnreadMsgUids()
        {
            try
            {
                _imapSw.WriteLine("$ UID SEARCH unseen");
                _imapSw.Flush();

                string res = Response();
                Match m;
                m = Regex.Match(res, "[0-9, ]*[0-9, ] [0-9]*[0-9]");
                if (String.IsNullOrEmpty(m.Value))
                    m = Regex.Match(res, "[0-9]*[0-9]");
                return m.ToString().Trim().Split(' ');
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return null;
        }

        public string[] GetReadMsgUids()
        {
            try
            {
                _imapSw.WriteLine("$ UID SEARCH seen");
                _imapSw.Flush();

                string res = Response();
                Match m;
                m = Regex.Match(res, "[0-9, ]*[0-9, ] [0-9]*[0-9]");
                if (String.IsNullOrEmpty(m.Value))
                    m = Regex.Match(res, "[0-9]*[0-9]");
                return m.ToString().Trim().Split(' ');
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return null;
        }

        public object GetMessageHeadersByUid(string uid)
        {
            try
            {
                _imapSw.WriteLine("$ UID FETCH " + uid + " (body[header.fields (from subject date)])");
                _imapSw.Flush();
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return Response();
        }
        public object GetMessageHeadersByUidX(string uid)
        {
            try
            {
                // _imapSw.WriteLine("$ UID FETCH " + uid + " (body[header.fields (from subject date)])");
                _imapSw.WriteLine("$ UID FETCH " + uid + " (body[header.fields (date)])");
                _imapSw.Flush();
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return Response();
        }
        public object GetFromByUid(string uid)
        {
            string strResult = null;

            try
            {
                _imapSw.WriteLine("$ UID FETCH " + uid + " (body[header.fields (from)])");
                _imapSw.Flush();

                strResult = Response().ToLower();
                if (!strResult.Contains("from:"))
                {
                    strResult = Response().ToLower();
                }
                if (!String.IsNullOrEmpty(strResult))
                {
                    if (strResult.Contains("<") && strResult.Contains(">"))
                    {
                        return strResult.Substring(strResult.IndexOf("<") + 1, strResult.LastIndexOf(">") - strResult.IndexOf("<")).Trim();
                    }
                    else
                    {
                        return strResult.Substring(strResult.IndexOf("from:") + 5, strResult.LastIndexOf(")") - strResult.IndexOf("from:") - 5).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return strResult;
        }
        public object GetSubjectByUid(string uid)
        {
            string strResult = null;
            try
            {
                _imapSw.WriteLine("$ UID FETCH " + uid + " (body[header.fields (subject date from flag)])");
                _imapSw.Flush();
                strResult = Response().ToLower();
                if (!strResult.Contains("subject:"))
                {
                    strResult = Response().ToLower();
                }
                if (!string.IsNullOrEmpty(strResult))
                {
                    return strResult.Substring(strResult.IndexOf("subject:") + 8, strResult.LastIndexOf(")") - strResult.IndexOf("subject:") - 8).Trim();
                }
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");  
            }
            return strResult;
        }
        public object GetDateByUid(string uid)
        {
            string strResult = null;
            try
            {
                _imapSw.WriteLine("$ UID FETCH " + uid + " (body[header.fields (date)])");
                _imapSw.Flush();
                strResult = Response().ToLower();
                if (!strResult.Contains("date:"))
                {
                    strResult = Response().ToLower();
                }
                if (!String.IsNullOrEmpty(strResult))
                {

                    return strResult.Substring(strResult.IndexOf("date:") + 5, strResult.LastIndexOf(")") - strResult.IndexOf("date:") - 5).Trim();
                }
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strResult;
        }

        public object GetBodyByUid(string uid)
        {
            string strResult = null;
            try
            {
                _imapSw.WriteLine("$ UID FETCH " + uid + " (body[text])");
                _imapSw.Flush();
                strResult = Response().ToLower();
                if (!strResult.Contains("body[text]"))
                {
                    strResult = Response().ToLower();
                }
                while (strResult.Contains("body[text]") && strResult.Contains("\r\n"))
                    strResult = strResult.Substring(strResult.IndexOf("\r\n")).Trim();
                strResult = strResult.Substring(0, strResult.LastIndexOf(')'));
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strResult;
        }



        public void SelectInbox()
        {
            _imapSw.WriteLine("$ SELECT INBOX");
            _imapSw.Flush();
            Response();
        }


        public object GetMessageHeaders(int index)
        {
            _imapSw.WriteLine("$ FETCH " + index + " (body[header.fields (from subject date)])");
            _imapSw.Flush();

            return Response();
        }

        public object GetMessage(int index)
        {
            _imapSw.WriteLine("$ FETCH " + index + " body[text]");
            _imapSw.Flush();

            return Response();
        }
        //public object GetTrash()
        //{
        //    _imapSw.WriteLine("$ LIST \"\" *");
        //    _imapSw.Flush();

        //    string str= Response().ToString();
        //    return null;
        //}


        public void Disconnect()
        {
            _imapSw.WriteLine("$ LOGOUT");
            _imapSw.Flush();
            _imapClient.Close();
        }

        private string Response()
        {
            try
            {
                byte[] data = new byte[_imapClient.ReceiveBufferSize];
                int ret = _imapNs.Read(data, 0, data.Length);
                return Encoding.ASCII.GetString(data).TrimEnd();
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return "";
        }


        public object Delete(string uid)
        {
            string strResp = null;
            try
            {
                _imapSw.WriteLine("$ UID STORE " + uid + " +flags (\\deleted)");
                _imapSw.Flush();
                strResp = Response().ToLower();
                if (!strResp.Contains("uid store completed"))
                {
                    strResp = Response();
                }
                EXPUNGE(uid);
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref objEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strResp;
        }
        public object MoveTrash(string uid)
        {
            _imapSw.WriteLine("$ UID COPY " + uid + " //trash");
            _imapSw.Flush();
            return Response();
        }
        public object EXPUNGE(String uid)
        {
            string strRes = null;
            try
            {
                _imapSw.WriteLine("$ UID EXPUNGE " + uid);
                _imapSw.Flush();
                strRes = Response().ToLower();
                if (!strRes.Contains("uid expunge completed"))
                {
                    strRes = Response().ToLower();
                }

            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ObjEx);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strRes;
        }

    }
}