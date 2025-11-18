using AE.Net.Mail;
using Azure.Messaging.ServiceBus;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using EAGetMail;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NReco.PdfGenerator;
using Pop3;
using QueueManager;
using RestSharp;
using SmartKargo.MessagingService.Configurations;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;


namespace QidWorkerRole
{
    public class Cls_BL
    {

        #region :: Variable Declaration ::

        //public static string smsUID;//ConfigurationManager.AppSettings["SMSUN"].ToString();
        //public static string smspswd;// = ConfigurationManager.AppSettings["SMSPASS"].ToString();
        //public static string SIATFTP = null;
        //public static string ftpInMsgFolder = null;
        //public static string SITAUser = null;
        //public static string SITAPWD = null;
        //public static string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //public static string accountEmail = null;
        //public static string password = null;
        //public static string BlobKey = string.Empty;
        //public static string BlobName = string.Empty;
        //public static string MailInServer = string.Empty;
        //public static bool f_ReadEmailIMAP = true;
        //public static bool f_SendOutgoingMail = true;
        //public static bool f_CreateDBLogSnapshot = false;
        //public static string f_AutoFBL = string.Empty;
        //public int dbSyncTime = 10000;
        //public string SFTPAddress = string.Empty;
        //public string SFTPUserName = string.Empty;
        //public string SFTPPassWord = string.Empty;
        //public string SFTPFingerPrint = string.Empty;
        //public string SFTPRemote = string.Empty;
        //public string MailsendPort = string.Empty;
        //public string MailReceivedIncomingPort = string.Empty;
        //public static AzureDrive drive = null;
        //const string PAGE_NAME = "MessageService-->Cls_BL";

        //SCMExceptionHandlingWorkRole scmexception = new SCMExceptionHandlingWorkRole();
        //string accessTokenUrl = ConfigurationManager.AppSettings["AccessTokenUrl"].ToString();
        //string sendSMSUrl = ConfigurationManager.AppSettings["SendSMSUrl"].ToString();
        //string basicAuthenticationHeader = ConfigurationManager.AppSettings["BasicAuthenticationHeader"].ToString();
        //string SMSNewAPI = ConfigurationManager.AppSettings["SMSNewAPI"].ToString();
        //AWBDetailsAPI objAWBdetailsAPI = new AWBDetailsAPI();
        //string[] eAWBPrintArray = null;
        //CultureInfo bz;
        //_appConfig.Sms.SendSMSUrl

        public static string BlobName = string.Empty;
        public static string BlobKey = string.Empty;
        public static bool f_SendOutgoingMail = true;
        public static string accountEmail = null;
        public static string password = null;
        public string MailsendPort = string.Empty;
        public string SFTPAddress = string.Empty;
        public string SFTPUserName = string.Empty;
        public string SFTPPassWord = string.Empty;
        public string SFTPFingerPrint = string.Empty;
        public string SFTPRemote = string.Empty;
        public static string SIATFTP = null;
        public static string SITAUser = null;
        public static string SITAPWD = null;
        public static string ftpInMsgFolder = null;
        string[]? eAWBPrintArray = null;

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<Cls_BL> _logger;
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<Cls_BL> _staticLogger => _loggerFactory?.CreateLogger<Cls_BL>();
        private readonly AppConfig _appConfig;
        private readonly FTP _ftp;
        private readonly AWBDetailsAPI _aWBDetailsAPI;
        private readonly EMAILOUT _emailOut;
        private readonly GenericFunction _genericFunction;
        private readonly AzureDrive _azureDrive;
        private readonly cls_SCMBL _cls_SCMBL;
        private readonly XFSUMessageProcessor _xFSUMessageProcessor;
        private readonly cls_Encode_Decode _cls_Encode_Decode;
        private readonly FFAMessageProcessor _fFAMessageProcessor;
        private readonly MailKitManager _mailKitManager;
        private readonly RapidInterfaceMethods _rapidInterfaceMethods;
        private readonly SMSOUT _sMSOUT;
        private readonly WebService _webService;
        private readonly ExchangeRateExpiryAlert _exchangeRateExpiryAlert;
        private readonly UnDepartedAWBListAlert _unDepartedAWBListAlert;
        private readonly FBLMessageProcessor _fBLMessageProcessor;
        private readonly FWBMessageProcessor _fWBMessageProcessor;
        private readonly FHLMessageProcessor _fHLMessageProcessor;
        private readonly RapidException _rapidException;
        private readonly RateExpiryAlert _rateExpiryAlert;
        private readonly TcpIMAP _tcpIMAP;

        #endregion

        #region :: Constructor ::
        public Cls_BL(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<Cls_BL> logger,
            AppConfig appConfig,
            FTP ftp,
            AWBDetailsAPI aWBDetailsAPI,
            EMAILOUT emailOut,
            GenericFunction genericFunction,
            AzureDrive azureDrive,
            ILoggerFactory loggerFactory,
            cls_SCMBL cls_SCMBL,
            XFSUMessageProcessor xFSUMessageProcessor,
            cls_Encode_Decode cls_Encode_Decode,
            FFAMessageProcessor fFAMessageProcessor,
            MailKitManager mailKitManager,
            RapidInterfaceMethods rapidInterfaceMethods,
            SMSOUT sMSOUT,
            WebService webService,
            ExchangeRateExpiryAlert exchangeRateExpiryAlert,
            UnDepartedAWBListAlert unDepartedAWBListAlert,
            FBLMessageProcessor fBLMessageProcessor,
            FWBMessageProcessor fWBMessageProcessor,
            FHLMessageProcessor fHLMessageProcessor,
            RapidException rapidException,
            RateExpiryAlert rateExpiryAlert,
            TcpIMAP tcpIMAP
            )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _loggerFactory ??= loggerFactory;
            _ftp = ftp;
            _aWBDetailsAPI = aWBDetailsAPI;
            _emailOut = emailOut;
            _genericFunction = genericFunction;
            _cls_SCMBL = cls_SCMBL;
            _xFSUMessageProcessor = xFSUMessageProcessor;
            _cls_Encode_Decode = cls_Encode_Decode;
            _fFAMessageProcessor = fFAMessageProcessor;
            _azureDrive = azureDrive;
            _mailKitManager = mailKitManager;
            _rapidInterfaceMethods = rapidInterfaceMethods;
            _sMSOUT = sMSOUT;
            _webService = webService;
            _exchangeRateExpiryAlert = exchangeRateExpiryAlert;
            _unDepartedAWBListAlert = unDepartedAWBListAlert;
            _fBLMessageProcessor = fBLMessageProcessor;
            _fWBMessageProcessor = fWBMessageProcessor;
            _fHLMessageProcessor = fHLMessageProcessor;
            _rapidException = rapidException;
            _rateExpiryAlert = rateExpiryAlert;
            _tcpIMAP=tcpIMAP;
            //GenericFunction genericFunction = new GenericFunction();
            //smsUID = ConfigCache.Get("SMSUN");
            //smspswd = ConfigCache.Get("SMSPASS");
        }
        #endregion

        #region :: Public Methods ::
        public async Task ReadMailFromMailBox()
        {

            try
            {
                //clsLog.WriteLogAzure("In ReadMailFromMailBox()");
                //GenericFunction genericFunction = new GenericFunction();

                _logger.LogInformation("In ReadMailFromMailBox()");

                string MailInServer = ConfigCache.Get("msgService_EmailInServer");
                string accountEmail = ConfigCache.Get("msgService_EmailId");
                string password = ConfigCache.Get("msgService_EmailPassword");
                bool f_ReadEmailIMAP = (ConfigCache.Get("msgService_EmailServerType")).ToUpper().Contains("IMAP");
                string MailsendPort = ConfigCache.Get("msgService_OutgoingMessagePort");
                string MailReceivedIncomingPort = ConfigCache.Get("msgService_EmailPort");

                string gmailMailINServer = ConfigCache.Get("GMAILMailINServer");
                string gmailAccountEmail = ConfigCache.Get("GMAILAccountEmail");
                string gmailPassword = ConfigCache.Get("GMAILAccountPassword");
                string gmailInPortNo = ConfigCache.Get("GMAILEmailInPortNo");

                string Office365AuthType = ConfigCache.Get("Office365AuthType");
                string Office365OAuth2ClientID = ConfigCache.Get("Office365OAuth2ClientID");
                string Office365OAuth2ClientSecretKey = ConfigCache.Get("Office365OAuth2ClientSecretKey");
                string Office365OAuth2TenantID = ConfigCache.Get("Office365OAuth2TenantID");


                if (f_ReadEmailIMAP)
                {
                    if (Office365AuthType.ToUpper() == "OAUTH2.0")
                    {
                        await RetrieveOffice365EmailsUsingOAuth2(accountEmail, Office365OAuth2ClientID, Office365OAuth2ClientSecretKey, Office365OAuth2TenantID);
                    }
                    else
                    {
                        if (MailInServer.ToUpper() == "IMAP.IONOS.COM")
                            await ReadFromIMAP(MailInServer, accountEmail, password, MailReceivedIncomingPort);

                        if (MailInServer != "" && accountEmail != "" && password != "")
                            await ReceiveMail_IMAP(MailInServer, accountEmail, password, MailReceivedIncomingPort);

                        if (gmailMailINServer != "" && gmailAccountEmail != "" && gmailPassword != "")
                            await ReadFromIMAPGmail(gmailMailINServer, gmailAccountEmail, gmailPassword, gmailInPortNo);
                    }
                }
                else
                {
                    await ReceiveMail_POP(MailInServer, accountEmail, password, false);
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task ReadFromIMAPGmail(string mailInServer, string accountEmail, string password, string mailsendPort)
        {
            try
            {
                int portNo = 0;
                portNo = mailsendPort.Trim() == string.Empty ? 993 : Convert.ToInt32(mailsendPort);

                //ImapClient ic = new ImapClient("imap.gmail.com", "cargouat@airasia.com", "Airasia123", AuthMethods.Login, 993, true);
                //ImapClient ic = new ImapClient("imap.gmail.com", "cargo@airasia.com", "ghavrahwmqzmewab", AuthMethods.Login, 993, true);

                ImapClient ic = new ImapClient(mailInServer, accountEmail, password, AuthMethods.Login, portNo, true);

                ic.ServerTimeout = 100000;
                ic.SelectMailbox("INBOX");

                Lazy<MailMessage>[] messages = ic.SearchMessages(SearchCondition.Unseen(), false, true);

                foreach (Lazy<MailMessage> message in messages)
                {
                    string _receivedString = string.Empty, MessageType = string.Empty;
                    MailMessage obj = message.Value;
                    _receivedString = obj.Body.ToString().ToUpper();

                    try
                    {
                        _receivedString = DecodeQuotedPrintables(_receivedString);

                        if (_receivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
                        {
                            ic.DeleteMessage(obj.Uid);
                            continue;
                        }
                        if ((_receivedString.Contains("FHL/")) || (_receivedString.Contains("FWB/")) ||
                                  (_receivedString.Contains("FFM/")) || (_receivedString.Contains("FFR/")) ||
                                  (_receivedString.Contains("FBL/")) || (_receivedString.Contains("FSB")) ||
                                  (_receivedString.Contains("FSU/")) || (_receivedString.Contains("FWR")) ||
                                  (_receivedString.Contains("UCM")) || (_receivedString.Contains("FBR/")) ||
                                  (_receivedString.Contains("FNA/")) || (_receivedString.Contains("FMB/")) ||
                                  (_receivedString.Contains("ASM")) || (_receivedString.Contains("MVT")) || (_receivedString.Contains("CPM")) || (_receivedString.Contains("FSN")) || (_receivedString.Contains("CARDIT"))

                                  || (_receivedString.Contains("FMA")))
                        {

                            #region changed contains by starts with fun
                            if (_receivedString.Contains("CARDIT"))
                                MessageType = "CARDIT";
                            if (_receivedString.StartsWith("FFR/"))
                                MessageType = "FFR";
                            else if (_receivedString.StartsWith("FSN"))
                                MessageType = "FSN";
                            else if (_receivedString.StartsWith("CPM"))
                                MessageType = "CPM";
                            else if (_receivedString.StartsWith("ASM"))
                                MessageType = "ASM";
                            else if (_receivedString.StartsWith("MVT"))
                                MessageType = "MVT";
                            else if (_receivedString.StartsWith("FWB/"))
                                MessageType = "FWB";
                            else if (_receivedString.StartsWith("FFM/"))
                                MessageType = "FFM";
                            else if (_receivedString.StartsWith("FAD/"))
                                MessageType = "FAD";
                            else if (_receivedString.StartsWith("FHL/"))
                                MessageType = "FHL";
                            else if (_receivedString.StartsWith("FSU/"))
                                MessageType = "FSU";
                            else if (_receivedString.StartsWith("UCM"))
                                MessageType = "UCM";
                            else if (_receivedString.StartsWith("FBL/"))
                                MessageType = "FBL";
                            else if (_receivedString.StartsWith("SCM"))
                                MessageType = "SCM";
                            else if (_receivedString.StartsWith("FSB"))
                                MessageType = "FSB";
                            else if (_receivedString.StartsWith("FWR/"))
                                MessageType = "FWR";
                            else if (_receivedString.StartsWith("FBR/"))
                                MessageType = "FBR/";
                            else if (_receivedString.StartsWith("FNA/"))
                                MessageType = "FNA";
                            else if (_receivedString.StartsWith("FMA"))
                                MessageType = "FMA";
                            else if (_receivedString.StartsWith("FMB"))
                                MessageType = "FMB";
                            #endregion

                            if (MessageType.ToUpper() != "CARDIT")
                            {
                                _receivedString = _receivedString.Substring(_receivedString.IndexOf(MessageType),
                                                                            _receivedString.Length -
                                                                            _receivedString.IndexOf(MessageType));
                            }
                            if (_receivedString.ToUpper().Contains("------=_NEXTPART"))
                            {
                                int indexOfEndPart = _receivedString.IndexOf("------=_NEXTPART");
                                _receivedString = _receivedString.Substring(0, indexOfEndPart);
                            }
                        }
                        string Subject = obj.Subject;
                        if (string.IsNullOrEmpty(Subject))
                            Subject = "Subject";

                        string fromEmail = obj.From.Address;
                        fromEmail = fromEmail.Replace("<", "");
                        fromEmail = fromEmail.Replace(">", "");

                        string toEmail = "";
                        toEmail = toEmail.Replace("<", "");
                        toEmail = toEmail.Replace(">", "");

                        string recievedDate = Convert.ToString(obj.Date);
                        string status = "Active";
                        // "01 Feb 2013 15:51:12"
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                        // string status = "Active";
                        try
                        {
                            if (await StoreIROPSEmail(Subject.ToUpper(), _receivedString.ToUpper(), fromEmail, toEmail, dtRec, dtSend, MessageType == "" ? Subject : MessageType, status, "GMAIL"))
                            {
                                // clsLog.WriteLogAzure("Email saved From imap.gmail.com");
                                _logger.LogInformation("Email saved From imap.gmail.com");
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ReadFromIMAPServer");
                        }

                        try
                        {
                            ic.DeleteMessage(obj.Uid);
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ReadFromIMAPServer");
                        }
                    }
                    catch (Exception ex)
                    {
                        string subject = string.Empty, fromEmail = string.Empty;
                        bool toSave = false;
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        string recievedDate = obj.Date.ToString();
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception exp)
                        {
                            // clsLog.WriteLogAzure(exp);
                            _logger.LogError(exp, "Error on ReadFromIMAPServer");
                        }
                        subject = obj.Subject;
                        if (ex.Message.Trim().ToUpper() == "COULD NOT FIND ANY RECOGNIZABLE DIGITS.")
                        {
                            toSave = true;
                        }
                        else if (subject.ToString().ToUpper() == "UNDELIVERED MAIL RETURNED TO SENDER")
                        {
                            toSave = true;
                        }
                        if (toSave)
                        {
                            fromEmail = obj.From.Address;
                            fromEmail = fromEmail.Replace("<", "").Replace(">", "");
                            if (await StoreIROPSEmail(subject, _receivedString.ToUpper().Substring(0, 2000), fromEmail, "", dtRec, dtSend, "Unsupported message", "Processed", "EMAIL"))
                                // clsLog.WriteLogAzure("Email saved From imap.gmail.com");
                                _logger.LogError("Email saved From imap.gmail.com");
                            ic.DeleteMessage(obj.Uid);
                        }
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on ReadFromIMAPServer");
                    }
                }

                ic.Dispose();

                #region Commented Code
                #endregion
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ReadFromIMAPServer");
            }
        }

        public async Task ReadFromIMAP(string mailInServer, string accountEmail, string password, string mailsendPort)
        {
            try
            {
                int portNo = 0;
                portNo = mailsendPort.Trim() == string.Empty ? 993 : Convert.ToInt32(mailsendPort);
                ImapClient ic = new ImapClient(mailInServer, accountEmail, password, AuthMethods.Login, portNo, true);
                ic.Ssl = true;
                ic.ServerTimeout = 100000;
                ic.SelectMailbox("INBOX");

                Lazy<MailMessage>[] messages = ic.SearchMessages(SearchCondition.Unseen(), false, true);

                foreach (Lazy<MailMessage> message in messages)
                {
                    string _receivedString = System.String.Empty, MessageType = string.Empty;
                    MailMessage obj = message.Value;
                    _receivedString = obj.Body.ToString().ToUpper().Trim();

                    try
                    {
                        _receivedString = DecodeQuotedPrintables(_receivedString);

                        if (_receivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
                        {
                            ic.DeleteMessage(obj.Uid);
                            continue;
                        }
                        if ((_receivedString.Contains("FHL/")) || (_receivedString.Contains("FWB/")) ||
                                  (_receivedString.Contains("FFM/")) || (_receivedString.Contains("FFR/")) ||
                                  (_receivedString.Contains("FBL/")) || (_receivedString.Contains("FSB")) ||
                                  (_receivedString.Contains("FSU/")) || (_receivedString.Contains("FWR")) ||
                                  (_receivedString.Contains("UCM")) || (_receivedString.Contains("FBR/")) ||
                                  (_receivedString.Contains("FNA/")) || (_receivedString.Contains("FMB/")) ||
                                  (_receivedString.Contains("ASM")) || (_receivedString.Contains("MVT")) || (_receivedString.Contains("CPM")) || (_receivedString.Contains("FSN")) || (_receivedString.Contains("CARDIT"))

                                  || (_receivedString.Contains("FMA")))
                        {

                            #region changed contains by starts with fun
                            if (_receivedString.Contains("CARDIT"))
                                MessageType = "CARDIT";
                            if (_receivedString.StartsWith("FFR/"))
                                MessageType = "FFR";
                            else if (_receivedString.StartsWith("FSN"))
                                MessageType = "FSN";
                            else if (_receivedString.StartsWith("CPM"))
                                MessageType = "CPM";
                            else if (_receivedString.StartsWith("ASM"))
                                MessageType = "ASM";
                            else if (_receivedString.StartsWith("MVT"))
                                MessageType = "MVT";
                            else if (_receivedString.StartsWith("FWB/"))
                                MessageType = "FWB";
                            else if (_receivedString.StartsWith("FFM/"))
                                MessageType = "FFM";
                            else if (_receivedString.StartsWith("FAD/"))
                                MessageType = "FAD";
                            else if (_receivedString.StartsWith("FHL/"))
                                MessageType = "FHL";
                            else if (_receivedString.StartsWith("FSU/"))
                                MessageType = "FSU";
                            else if (_receivedString.StartsWith("UCM"))
                                MessageType = "UCM";
                            else if (_receivedString.StartsWith("FBL/"))
                                MessageType = "FBL";
                            else if (_receivedString.StartsWith("SCM"))
                                MessageType = "SCM";
                            else if (_receivedString.StartsWith("FSB"))
                                MessageType = "FSB";
                            else if (_receivedString.StartsWith("FWR/"))
                                MessageType = "FWR";
                            else if (_receivedString.StartsWith("FBR/"))
                                MessageType = "FBR/";
                            else if (_receivedString.StartsWith("FNA/"))
                                MessageType = "FNA";
                            else if (_receivedString.StartsWith("FMA"))
                                MessageType = "FMA";
                            else if (_receivedString.StartsWith("FMB"))
                                MessageType = "FMB";
                            #endregion

                            if (MessageType.ToUpper() != "CARDIT")
                            {
                                _receivedString = _receivedString.Substring(_receivedString.IndexOf(MessageType),
                                                                            _receivedString.Length -
                                                                            _receivedString.IndexOf(MessageType));
                            }
                            if (_receivedString.ToUpper().Contains("------=_NEXTPART"))
                            {
                                int indexOfEndPart = _receivedString.IndexOf("------=_NEXTPART");
                                _receivedString = _receivedString.Substring(0, indexOfEndPart);
                            }
                        }
                        string Subject = obj.Subject;
                        if (string.IsNullOrEmpty(Subject))
                            Subject = "Subject";

                        string fromEmail = obj.From.Address;
                        fromEmail = fromEmail.Replace("<", "");
                        fromEmail = fromEmail.Replace(">", "");

                        string toEmail = "";
                        toEmail = toEmail.Replace("<", "");
                        toEmail = toEmail.Replace(">", "");

                        string recievedDate = Convert.ToString(obj.Date);
                        string status = "Active";
                        // "01 Feb 2013 15:51:12"
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                        try
                        {
                            if (await StoreIROPSEmail(Subject.ToUpper(), _receivedString.ToUpper(), fromEmail, toEmail, dtRec, dtSend, MessageType == "" ? Subject : MessageType, status, "GMAIL"))
                            {
                                // clsLog.WriteLogAzure("Email saved From imap.gmail.com");
                                _logger.LogInformation("Email saved From imap.gmail.com");
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ReadFromIMAP");
                        }

                        try
                        {
                            ic.DeleteMessage(obj.Uid);
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ReadFromIMAP");
                        }
                    }
                    catch (Exception ex)
                    {
                        string subject = string.Empty, fromEmail = string.Empty;
                        bool toSave = false;
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        string recievedDate = obj.Date.ToString();
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception exp)
                        {
                            // clsLog.WriteLogAzure(exp);
                            _logger.LogError(exp, "Error on ReadFromIMAP");
                        }
                        subject = obj.Subject;
                        if (ex.Message.Trim().ToUpper() == "COULD NOT FIND ANY RECOGNIZABLE DIGITS.")
                        {
                            toSave = true;
                        }
                        else if (subject.ToString().ToUpper() == "UNDELIVERED MAIL RETURNED TO SENDER")
                        {
                            toSave = true;
                        }
                        if (toSave)
                        {
                            fromEmail = obj.From.Address;
                            fromEmail = fromEmail.Replace("<", "").Replace(">", "");
                            if (await StoreIROPSEmail(subject, _receivedString.ToUpper().Substring(0, 2000), fromEmail, "", dtRec, dtSend, "Unsupported message", "Processed", "EMAIL"))
                                // clsLog.WriteLogAzure("Email saved From imap.gmail.com");
                                _logger.LogError("Email saved From imap.gmail.com");
                            ic.DeleteMessage(obj.Uid);
                        }
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on ReadFromIMAP");
                    }
                }

                ic.Dispose();

                #region Commented Code
                #endregion
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ReadFromIMAP");
            }
        }

        public string PostString(string uri, string requestData)
        {
            HttpWebRequest httpRequest = WebRequest.Create(uri) as HttpWebRequest;
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";

            using (Stream requestStream = httpRequest.GetRequestStream())
            {
                byte[] requestBuffer = Encoding.UTF8.GetBytes(requestData);
                requestStream.Write(requestBuffer, 0, requestBuffer.Length);
                requestStream.Close();
            }

            try
            {
                HttpWebResponse httpResponse = httpRequest.GetResponse() as HttpWebResponse;
                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = reader.ReadToEnd();
                    // Console.WriteLine(responseText);
                    _logger.LogInformation(responseText);
                    return responseText;
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        // Console.WriteLine("HTTP: " + response.StatusCode);
                        _logger.LogError("HTTP: {0}", response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = reader.ReadToEnd();
                            // Console.WriteLine(responseText);
                            _logger.LogError(responseText, "Error on PostString");
                        }
                    }
                }

                throw;
            }
        }

        public async Task RetrieveOffice365EmailsUsingOAuth2(string accountEmail, string client_id, string client_secret, string tenantId)
        {
            try
            {
                // If your application is not created by Office365 administrator,
                // please use Office365 directory tenant id, you should ask Offic365 administrator to send it to you.
                // Office365 administrator can query tenant id in https://portal.azure.com/ - Azure Active Directory.

                string requestData = string.Format("client_id={0}&client_secret={1}&scope=https://graph.microsoft.com/.default&grant_type=client_credentials", client_id, client_secret);

                string tokenUri = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId);
                string responseText = PostString(tokenUri, requestData);

                OAuthResponseParser parser = new OAuthResponseParser();
                parser.Load(responseText);

                // Microsoft Graph API server address
                MailServer oServer = new MailServer("graph.microsoft.com",
                        accountEmail,
                        parser.AccessToken, // use access token as password
                        ServerProtocol.MsGraphApi); // use Http Graph API protocol

                // Set OAUTH 2.0
                oServer.AuthType = ServerAuthType.AuthXOAUTH2;
                // Enable SSL/TLS connection
                oServer.SSLConnection = true;

                MailClient oClient = new MailClient("EG-C1653719494-01380-2F99UAE78FFD9A51-E86393AV65FDUB87");
                // Get new email only, if you want to get all emails, please remove this line
                oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

                //Console.WriteLine("Connecting {0} ...", oServer.Server);
                oClient.Connect(oServer);

                MailInfo[] infos = oClient.GetMailInfos();

                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];

                    // Receive email from email server
                    Mail obj = oClient.GetMail(info);

                    string _receivedString = string.Empty, MessageType = string.Empty;

                    _receivedString = obj.TextBody.ToString().ToUpper().Trim();

                    try
                    {
                        _receivedString = DecodeQuotedPrintables(_receivedString);

                        if (_receivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
                        {
                            oClient.Delete(info);
                            continue;
                        }
                        if ((_receivedString.Contains("FHL/")) || (_receivedString.Contains("FWB/")) ||
                                  (_receivedString.Contains("FFM/")) || (_receivedString.Contains("FFR/")) ||
                                  (_receivedString.Contains("FBL/")) || (_receivedString.Contains("FSB")) ||
                                  (_receivedString.Contains("FSU/")) || (_receivedString.Contains("FWR")) ||
                                  (_receivedString.Contains("UCM")) || (_receivedString.Contains("FBR/")) ||
                                  (_receivedString.Contains("FNA/")) || (_receivedString.Contains("FMB/")) ||
                                  (_receivedString.Contains("ASM")) || (_receivedString.Contains("MVT")) || (_receivedString.Contains("CPM")) || (_receivedString.Contains("FSN")) || (_receivedString.Contains("CARDIT"))

                                  || (_receivedString.Contains("FMA")))
                        {

                            #region changed contains by starts with fun
                            if (_receivedString.Contains("CARDIT"))
                                MessageType = "CARDIT";
                            if (_receivedString.StartsWith("FFR/"))
                                MessageType = "FFR";
                            else if (_receivedString.StartsWith("FSN"))
                                MessageType = "FSN";
                            else if (_receivedString.StartsWith("CPM"))
                                MessageType = "CPM";
                            else if (_receivedString.StartsWith("ASM"))
                                MessageType = "ASM";
                            else if (_receivedString.StartsWith("MVT"))
                                MessageType = "MVT";
                            else if (_receivedString.StartsWith("FWB/"))
                                MessageType = "FWB";
                            else if (_receivedString.StartsWith("FFM/"))
                                MessageType = "FFM";
                            else if (_receivedString.StartsWith("FAD/"))
                                MessageType = "FAD";
                            else if (_receivedString.StartsWith("FHL/"))
                                MessageType = "FHL";
                            else if (_receivedString.StartsWith("FSU/"))
                                MessageType = "FSU";
                            else if (_receivedString.StartsWith("UCM"))
                                MessageType = "UCM";
                            else if (_receivedString.StartsWith("FBL/"))
                                MessageType = "FBL";
                            else if (_receivedString.StartsWith("SCM"))
                                MessageType = "SCM";
                            else if (_receivedString.StartsWith("FSB"))
                                MessageType = "FSB";
                            else if (_receivedString.StartsWith("FWR/"))
                                MessageType = "FWR";
                            else if (_receivedString.StartsWith("FBR/"))
                                MessageType = "FBR/";
                            else if (_receivedString.StartsWith("FNA/"))
                                MessageType = "FNA";
                            else if (_receivedString.StartsWith("FMA"))
                                MessageType = "FMA";
                            else if (_receivedString.StartsWith("FMB"))
                                MessageType = "FMB";
                            #endregion

                            if (MessageType.ToUpper() != "CARDIT")
                            {
                                _receivedString = _receivedString.Substring(_receivedString.IndexOf(MessageType),
                                                                            _receivedString.Length -
                                                                            _receivedString.IndexOf(MessageType));
                            }
                            if (_receivedString.ToUpper().Contains("------=_NEXTPART"))
                            {
                                int indexOfEndPart = _receivedString.IndexOf("------=_NEXTPART");
                                _receivedString = _receivedString.Substring(0, indexOfEndPart);
                            }
                        }
                        string Subject = obj.Subject;
                        if (string.IsNullOrEmpty(Subject))
                            Subject = "Subject";
                        else
                            Subject = Subject.Replace("(Trial Version)", "");

                        string fromEmail = obj.From.Address;
                        fromEmail = fromEmail.Replace("<", "");
                        fromEmail = fromEmail.Replace(">", "");

                        string toEmail = "";
                        toEmail = toEmail.Replace("<", "");
                        toEmail = toEmail.Replace(">", "");

                        string recievedDate = Convert.ToString(obj.ReceivedDate);
                        string status = "Active";
                        // "01 Feb 2013 15:51:12"
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex); 
                            _logger.LogError(ex, "Error on RetrieveOffice365EmailsUsingOAuth2");
                        }

                        try
                        {
                            if (await StoreIROPSEmail(Subject.ToUpper(), _receivedString.ToUpper(), fromEmail, toEmail, dtRec, dtSend, MessageType == "" ? Subject : MessageType, status, "O365"))
                            {
                                //Mark email as read to prevent retrieving this email again.
                                oClient.MarkAsRead(info, true);
                                //oClient.Delete(info);
                                // clsLog.WriteLogAzure("Email saved From outlook.office365.com");
                                _logger.LogInformation("Email saved From outlook.office365.com");
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on RetrieveOffice365EmailsUsingOAuth2");
                        }

                    }
                    catch (Exception ex)
                    {
                        string subject = string.Empty, fromEmail = string.Empty;
                        bool toSave = false;
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        string recievedDate = obj.ReceivedDate.ToString();
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception exp)
                        {
                            // clsLog.WriteLogAzure(exp);
                            _logger.LogError(exp, "Error on RetrieveOffice365EmailsUsingOAuth2");
                        }
                        subject = obj.Subject;
                        if (ex.Message.Trim().ToUpper() == "COULD NOT FIND ANY RECOGNIZABLE DIGITS.")
                        {
                            toSave = true;
                        }
                        else if (subject.ToString().ToUpper() == "UNDELIVERED MAIL RETURNED TO SENDER")
                        {
                            toSave = true;
                        }
                        if (toSave)
                        {
                            fromEmail = obj.From.Address;
                            fromEmail = fromEmail.Replace("<", "").Replace(">", "");
                            if (await StoreIROPSEmail(subject, _receivedString.ToUpper().Substring(0, 2000), fromEmail, "", dtRec, dtSend, "Unsupported message", "Processed", "EMAIL"))
                                // clsLog.WriteLogAzure("Email saved From imap.gmail.com");
                                _logger.LogError("Email saved From imap.gmail.com");
                            oClient.Delete(info);
                        }
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on RetrieveOffice365EmailsUsingOAuth2");
                    }
                }
                // Quit and expunge emails marked as deleted from server.
                oClient.Quit();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on RetrieveOffice365EmailsUsingOAuth2");
            }
        }


        public async Task<DataSet> DBCalls()
        {
            //clsLog.WriteLogAzure("In DBCalls()");
            _logger.LogInformation("In DBCalls()");
            DataSet dsUploadMasters = new();
            try
            {
                ///Process messages from tblInbox
                await CheckMessagesforProcessing();

                ///Check FFA for send
                //FFAMessageProcessor ffmMessage = new FFAMessageProcessor();
                await _fFAMessageProcessor.MakeanSendFFAMessage();

                ///Generate auto messages using configuration
                await AutoMessages();

                //SQLServer sqlServerCargoPayLoad = new SQLServer();
                //sqlServerCargoPayLoad.SelectRecords("Messaging.uspSentCargoPayLoad");

                // Auto Volaris Payload functionality
                await _readWriteDao.SelectRecords("Messaging.uspSentCargoPayLoad");

                //SQLServer sqlServerMakeFSUMessage = new SQLServer();
                //sqlServerMakeFSUMessage.SelectRecords("GetRecordforMakeFSUMessage");

                //Auto Send FSU Message
                await _readWriteDao.SelectRecords("GetRecordforMakeFSUMessage");


                //SQLServer ProcessASMMessages = new SQLServer();
                //ProcessASMMessages.SelectRecords("Messaging.uspProcessASMMessages");

                //Process ASM messages from Queue
                await _readWriteDao.SelectRecords("Messaging.uspProcessASMMessages");


                #region : Auto NIL FFM :
                //GenericFunction genericFunction = new GenericFunction();

                bool sendAutoNILFFM = bool.Parse(ConfigCache.Get("AutoNILFFM").Trim() == string.Empty ? "false" : ConfigCache.Get("AutoNILFFM").Trim());
                bool sendAutoDepartManifestedFlight = bool.Parse(ConfigCache.Get("AutoDepartManifestedFlight").Trim() == string.Empty ? "false" : ConfigCache.Get("AutoDepartManifestedFlight").Trim());
                bool isAutoDepartTruck = bool.Parse(ConfigCache.Get("IsAutoDepartTruck").Trim() == string.Empty ? "false" : ConfigCache.Get("IsAutoDepartTruck").Trim());
                if (sendAutoNILFFM || sendAutoDepartManifestedFlight || isAutoDepartTruck)
                {
                    //SQLServer sqlServerNILFlights = new SQLServer();
                    //DataSet dsAutoDepartFlights = sqlServerNILFlights.SelectRecords("uspAutoDepartFlights");
                    DataSet? dsAutoDepartFlights = await _readWriteDao.SelectRecords("uspAutoDepartFlights");

                    if (dsAutoDepartFlights != null && dsAutoDepartFlights.Tables.Count > 0)
                    {
                        for (int j = 0; j < dsAutoDepartFlights.Tables.Count; j++)
                        {
                            if (dsAutoDepartFlights.Tables[j].Rows.Count > 0)
                            {
                                if (dsAutoDepartFlights.Tables[j].Rows[0]["Type"].ToString().ToUpper() == "AUTODEPARTFLIGHTS")
                                {
                                    // clsLog.WriteLogAzure("There are " + dsAutoDepartFlights.Tables[0].Rows.Count.ToString() + " Flights to be departed");
                                    _logger.LogInformation("There are {FlightsCount} Flights to be departed", dsAutoDepartFlights.Tables[0].Rows.Count);

                                    //cls_SCMBL cls_scmbl = new cls_SCMBL();

                                    for (int i = 0; i < dsAutoDepartFlights.Tables[j].Rows.Count; i++)
                                    {
                                        string source = string.Empty, destination = string.Empty, flightNumber = string.Empty, flightDate = string.Empty, msgVersion = string.Empty, SitaMessageHeader = string.Empty, SFTPHeaderSITAddress = string.Empty, ffmMessageBody = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, WEBAPIAddress = string.Empty, WebAPIURL = string.Empty;


                                        source = dsAutoDepartFlights.Tables[j].Rows[i]["Source"].ToString();
                                        destination = dsAutoDepartFlights.Tables[j].Rows[i]["Dest"].ToString();
                                        flightNumber = dsAutoDepartFlights.Tables[j].Rows[i]["FltNo"].ToString();
                                        flightDate = dsAutoDepartFlights.Tables[j].Rows[i]["DATE"].ToString();

                                        DataSet dsMessageConfig = await _genericFunction.GetSitaAddressandMessageVersion(flightNumber.Substring(0, 2), "FFM", "AIR", source, destination, flightNumber, string.Empty);
                                        if (dsMessageConfig != null && dsMessageConfig.Tables.Count > 0 && dsMessageConfig.Tables[0].Rows.Count > 0)
                                        {
                                            msgVersion = Convert.ToString(dsMessageConfig.Tables[0].Rows[0]["MessageVersion"]);
                                            strEmailid = dsMessageConfig.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                                            strSITAHeaderType = dsMessageConfig.Tables[0].Rows[0]["SITAHeaderType"].ToString();
                                            WebAPIURL = dsMessageConfig.Tables[0].Rows[0]["WebAPIURL"].ToString();

                                            if (dsMessageConfig.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                                            {
                                                SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsMessageConfig.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsMessageConfig.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsMessageConfig.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                                            }
                                            if (dsMessageConfig.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                                            {
                                                SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsMessageConfig.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsMessageConfig.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsMessageConfig.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                                            }
                                            if (WebAPIURL.Length > 0)
                                            {
                                                WEBAPIAddress = _genericFunction.MakeMailMessageFormat(dsMessageConfig.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsMessageConfig.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsMessageConfig.Tables[0].Rows[0]["MessageID"].ToString(), dsMessageConfig.Tables[0].Rows[0]["WEBAPIHeaderType"].ToString());
                                            }
                                        }

                                        ffmMessageBody = await _cls_SCMBL.EncodeFFM(source, flightNumber, Convert.ToDateTime(flightDate), msgVersion);
                                        // clsLog.WriteLogAzure(ffmMessageBody);
                                        _logger.LogInformation(ffmMessageBody);
                                        if (ffmMessageBody.Length > 3)
                                        {
                                            if (SitaMessageHeader != "")
                                                await _genericFunction.SaveMessageOutBox("FFM", SitaMessageHeader.ToString() + "\r\n" + ffmMessageBody, "", "SITAFTP", source, destination, flightNumber, flightDate.ToString(), "");

                                            if (SFTPHeaderSITAddress != "")
                                                await _genericFunction.SaveMessageOutBox("FFM", SFTPHeaderSITAddress.ToString() + "\r\n" + ffmMessageBody, "", "SFTP", source, destination, flightNumber, flightDate.ToString(), "");

                                            if (strEmailid != "")
                                                await _genericFunction.SaveMessageOutBox("FFM", ffmMessageBody, "", strEmailid, source, destination, flightNumber, flightDate, "");

                                            if (WEBAPIAddress != "")
                                                await _genericFunction.SaveMessageOutBox("FFM", WEBAPIAddress.ToString() + "\r\n" + ffmMessageBody, "", "WEBAPI", source, destination, flightNumber, flightDate.ToString(), "");

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                ///Get all records to upload masters
                if (ConfigCache.Get("MSServiceType").ToUpper() != "WINDOWSSERVICE")
                {
                    //SQLServer sqlServerUplodedFile = new SQLServer();
                    //dsUploadMasters = sqlServerUplodedFile.SelectRecords("uspGetUplodedFile");

                    dsUploadMasters = await _readWriteDao.SelectRecords("uspGetUplodedFile");

                }

                await _genericFunction.AIMSLoadPlan();

                ///Oman Data Dump
                string dataDumpFolderPath = ConfigCache.Get("DataDumpFolderPath");
                if (dataDumpFolderPath != string.Empty)
                {
                    //FTP ftp = new FTP();
                    //SQLServer OmanBIDataDump = new SQLServer();
                    //DataSet dsOmanBIDataDump = OmanBIDataDump.SelectRecords("BI.ExportOmanBIDataDump");

                    DataSet? dsOmanBIDataDump = await _readWriteDao.SelectRecords("BI.ExportOmanBIDataDump");


                    if (dsOmanBIDataDump != null && dsOmanBIDataDump.Tables.Count > 0 && dsOmanBIDataDump.Tables[0].Rows.Count > 0)
                    {
                        _ftp.ZIPandSFPTUpload(dsOmanBIDataDump);
                        _ftp.UploadDataDumpFileToSFTP(dataDumpFolderPath, dsOmanBIDataDump.Tables[0].Rows[0]["ZIPFileName"].ToString());
                    }
                }

                //SMS process for ARRIVAL SMS Notifications
                string SendArrivalSMSNotifications = ConfigCache.Get("SendArrivalSMSNotifications");
                if (SendArrivalSMSNotifications != "" && SendArrivalSMSNotifications.ToUpper() == "TRUE")
                    await ProcessArrivalSMSNotifications();

                string SendLyingListAutoGenerated = ConfigCache.Get("LyingList");
                if (SendLyingListAutoGenerated != "" && SendLyingListAutoGenerated.ToUpper() == "TRUE")
                {
                    await SendLyingListReport();
                }

                //Added For VJ-62
                string SendFlightControlGenerated = ConfigCache.Get("AutoSendFlightControlExportData");
                if (SendFlightControlGenerated != "" && SendFlightControlGenerated.ToUpper() == "TRUE")
                {
                    await SendFlightControlListReport();
                }

                string SendAutoExportToManifest = ConfigCache.Get("AutoSendCargoLoadBSTD");
                if (SendAutoExportToManifest != "" && SendAutoExportToManifest.ToUpper() == "TRUE")
                {
                    await SendBSTDDataInXML();
                }

                string AutoSendLoadPlanData = ConfigCache.Get("AutoSendLoadPlanData");
                if (AutoSendLoadPlanData != "" && AutoSendLoadPlanData.ToUpper() == "TRUE")
                {
                    await SendCargoLoadPlan();
                }

                string AutoSendUnDepartedAleart = ConfigCache.Get("AutoSendUnDepartedAleart");
                if (AutoSendUnDepartedAleart != "" && AutoSendUnDepartedAleart.ToUpper() == "TRUE")
                {
                    await AutoSendUnDepartedAleartFunc();
                }

                // Added by Aishwarya for VJ-32
                string AutoSendFlightLoadPlanData = ConfigCache.Get("AutoSendFlightLoadPlanData");
                if (AutoSendFlightLoadPlanData != "" && AutoSendFlightLoadPlanData.ToUpper() == "TRUE")
                {
                    await SendFlightLoadPlan();
                }

                // Added by Ujjaini for VJ-221

                string AutoSendManageCapacityFLP = ConfigCache.Get("AutoSendManageCapacityFLP");
                if (AutoSendManageCapacityFLP != "" && AutoSendManageCapacityFLP.ToUpper() == "TRUE")
                {
                    await SendManageCapacityFlightLoadPlan();
                }

                await RemoveLyingListProcess();

                string enableForexAPIupload = ConfigCache.Get("ForexRateAPIURL");
                if (enableForexAPIupload != "")
                    await ExchangeRates();

                string updateULDDetailsFromJson = ConfigCache.Get("UpdateULDDetailsFromJson");
                if (updateULDDetailsFromJson.ToUpper() == "TRUE")
                {
                    await UpdateULDStock();
                }

                string updateRapidFile = ConfigCache.Get("updateRapidFile");

                //RapidInterfaceMethods objrapid = new RapidInterfaceMethods();

                if (updateRapidFile.ToUpper() == "TRUE")
                {
                    //objrapid.UpdateRapidDetails();
                    await _rapidInterfaceMethods.UpdateRapidDetails();
                }

                string updateRapidFileForCebu = ConfigCache.Get("updateRapidFileForCebu");

                //RapidInterfaceMethods objRapidforCebu = new RapidInterfaceMethods();

                if (updateRapidFileForCebu.ToUpper() == "TRUE")
                {
                    //objrapid.UpdateRapidDetailsForCebu();
                    await _rapidInterfaceMethods.UpdateRapidDetailsForCebu();

                }

                // Added by Ravendra for AK-3426
                string AutoReleaseAllocation = ConfigCache.Get("AutoReleaseAllocation");
                if (AutoReleaseAllocation != "" && AutoReleaseAllocation.ToUpper() == "TRUE")
                {
                    await AutoReleaseCapacityAllocation();
                }

                await NoShowCalculationAsPerAgent();

                // Added by Anil for HC-49
                string AutoSendRateLineExpiryNotification = ConfigCache.Get("AutoSendRateLineExpiryNotification");
                if (AutoSendRateLineExpiryNotification != "" && AutoSendRateLineExpiryNotification.ToUpper() == "TRUE")
                {
                    //RateExpiryAlert rateExpiryAlert = new RateExpiryAlert();
                    await _rateExpiryAlert.RateExpiryListener();
                }

                // Added by Poorna for CEBV4-6372
                await SendDwellTimeInformation();
                // Added by Nikhil for AK-3625
                await Lockuser90Days();

                //Added by Ravikumar for HC - 78

                string AutoSendExchangeRateLineExpiryNotification = ConfigCache.Get("AutoSendExchangeRateExpiryNotification");
                if (AutoSendExchangeRateLineExpiryNotification != "" && AutoSendExchangeRateLineExpiryNotification.ToUpper() == "TRUE")
                {
                    //ExchangeRateExpiryAlert exchangerateExpiryAlert = new ExchangeRateExpiryAlert();
                    await _exchangeRateExpiryAlert.ExchangeRateExpiryListener();
                }

                try
                {

                    //if (genericFunction.GetConfigurationValues("PushDatatoFCT") == "true")
                    if (ConfigCache.Get("PushDatatoFCT").ToUpper() == "TRUE")
                    {
                        //objAWBdetailsAPI.GetPendingAWBList();
                        await _aWBDetailsAPI.GetPendingAWBList();
                    }
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on DBCalls");
                }

                //UnDepartedAWBListAlert unDepartedAWBListAlert = new UnDepartedAWBListAlert();
                try
                {
                    await _unDepartedAWBListAlert.UnDepartedAWBListListener();
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on DBCalls");
                }

                //finally
                //{
                //    if (unDepartedAWBListAlert != null)
                //        unDepartedAWBListAlert = null;
                //}

                try
                {
                    await GetPendingNotification();
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error in DBCalls");
                }

                //if (!string.IsNullOrEmpty(genericFunction.GetConfigurationValues("ExcelUploadBookingEmail")) && genericFunction.GetConfigurationValues("ExcelUploadBookingEmail").Contains("@"))
                if (!string.IsNullOrEmpty(ConfigCache.Get("ExcelUploadBookingEmail")) && ConfigCache.Get("ExcelUploadBookingEmail").Contains("@"))
                {
                    await SendExcelUploadBookingEmailNotification();
                }

                try
                {
                    string NotificationAlertBondExpiry = ConfigCache.Get("BondNotification");
                    if (NotificationAlertBondExpiry != "" && NotificationAlertBondExpiry.ToUpper() == "TRUE")
                    {
                        await SendNotificationAlertBondExpiry();
                    }
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on DBCalls");
                }

                try
                {
                    string RejectedMessages = ConfigCache.Get("RejectedMessages");
                    if (RejectedMessages != "" && RejectedMessages.ToUpper() == "TRUE")
                    {
                        //SQLServer sqlServerFmsg = new SQLServer();
                        //sqlServerFmsg.SelectRecords("Messaging.uspFailedMessageDetails");

                        await _readWriteDao.SelectRecords("Messaging.uspFailedMessageDetails");

                    }

                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on DBCalls");
                }
                //
                try
                {
                    string updateRapidExceptionFileForCebu = ConfigCache.Get("updateRapidExceptionFileForCebu");

                    //RapidException objRapidExceptionCebu = new RapidException();

                    //if (updateRapidExceptionFileForCebu.ToUpper() == "TRUE")
                    if (updateRapidExceptionFileForCebu.ToUpper() == "TRUE")
                    {
                        await _rapidException.RapidExceptionCEBU();
                    }
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on DBCalls");
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on DBCalls");
            }
            return dsUploadMasters;
        }

        public async Task SendExcelUploadBookingEmailNotification()
        {
            try
            {
                string htmlContent = "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title></title>\r\n    <style type=\"text/css\">\r\n        .FontBold {\r\n            font-weight: bold;\r\n            font-size: medium;\r\n            width: auto;\r\n        }\r\n\r\n        .tableBorder {\r\n            border: 1px solid black;\r\n            border-collapse: collapse;\r\n            border-color: gray;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <div style=\"font-family:Verdana;\">\r\n        <div>\r\n            <table style=\"width: 100%\" cellspacing=\"5\" cellpadding=\"5\">\r\n                @NotificationDetails@\r\n            </table>           \r\n        </div>\r\n    </div>\r\n</body>\r\n</html>";

                string emailAddress = string.Empty
                    , totalRecords = string.Empty
                    , successRecords = string.Empty
                    , failedRecords = string.Empty
                    , convertedToBookingRecords = string.Empty
                    , ffrFailedRecords = string.Empty;

                StringBuilder sbNotification = new StringBuilder();

                //GenericFunction genericFunction = new GenericFunction();

                DataSet? dsUploadDetails = new DataSet();

                //SQLServer sqlServer = new SQLServer();
                //dsUploadDetails = sqlServer.SelectRecords("Messaging.uspSendExcelBookingUploadNotification");

                dsUploadDetails = await _readWriteDao.SelectRecords("Messaging.uspSendExcelBookingUploadNotification");


                if (dsUploadDetails != null && dsUploadDetails.Tables.Count > 0 && dsUploadDetails.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsUploadDetails.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            sbNotification.Append("<tr style='page-break-inside: avoid'>");
                            sbNotification.Append("<td class='tableBorder' style='font-size:13px;'><b>SrNo</b></td>");
                            sbNotification.Append("<td class='tableBorder' style='font-size:13px;'><b>AWBNumber</b></td>");
                            sbNotification.Append("<td class='tableBorder' style='font-size:13px;'><b>SuccessFail</b></td>");
                            sbNotification.Append("<td class='tableBorder' style='font-size:13px;'><b>ConvertedToFFR</b></td>");
                            sbNotification.Append("<td class='tableBorder' style='font-size:13px;'><b>ErrorWarning</b></td>");
                            sbNotification.Append("</tr>");
                        }

                        sbNotification.Append("<tr style='page-break-inside: avoid'>");
                        sbNotification.Append("<td class='tableBorder' style='font-size:13px;'>" + Convert.ToString(dsUploadDetails.Tables[0].Rows[i]["RowNum"]) + "</td>");
                        sbNotification.Append("<td class='tableBorder' style='font-size:13px;'>" + Convert.ToString(dsUploadDetails.Tables[0].Rows[i]["AWBNumber"]) + "</td>");
                        sbNotification.Append("<td class='tableBorder' style='font-size:13px;'>" + Convert.ToString(dsUploadDetails.Tables[0].Rows[i]["SuccessFail"]) + "</td>");
                        sbNotification.Append("<td class='tableBorder' style='font-size:13px;'>" + Convert.ToString(dsUploadDetails.Tables[0].Rows[i]["ConvertedToFFR"]) + "</td>");
                        sbNotification.Append("<td class='tableBorder' style='font-size:13px;'>" + Convert.ToString(dsUploadDetails.Tables[0].Rows[i]["ErrorWarning"]) + "</td>");
                        sbNotification.Append("</tr>");

                    }

                    //emailAddress = genericFunction.GetConfigurationValues("ExcelUploadBookingEmail");
                    emailAddress = ConfigCache.Get("ExcelUploadBookingEmail");

                    htmlContent = htmlContent.Replace("@NotificationDetails@", Convert.ToString(sbNotification.ToString()));

                    var byteArray = Encoding.ASCII.GetBytes(htmlContent);
                    MemoryStream msExcel = new MemoryStream(byteArray);


                    string DocfileName = Convert.ToString(dsUploadDetails.Tables[0].Rows[0]["FileName"]).Replace(".xlsx", ".xls").Replace(".XLSX", ".xls");

                    string FileExcelURL = UploadToBlob(msExcel, DocfileName, "exceluploadbookingsummary");

                    if (dsUploadDetails.Tables.Count > 1 && dsUploadDetails.Tables[1].Rows.Count > 0)
                    {
                        totalRecords = Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["RecordCount"]);
                        successRecords = Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["SuccessCount"]);
                        failedRecords = Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["FailedCount"]);
                        convertedToBookingRecords = Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["ConvertedToBookingRecords"]);
                        ffrFailedRecords = Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["FFRFailedRecords"]);
                        if (!string.IsNullOrWhiteSpace(Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["UserEmail"])) && Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["UserEmail"]).Contains("@"))
                        {
                            emailAddress = Convert.ToString(dsUploadDetails.Tables[1].Rows[0]["UserEmail"]);
                        }
                    }

                    DateTime TimeStamp = DateTime.UtcNow;
                    string sMailSubject = "Cargo Wise Upload file Notification";
                    System.String sMailBody = "\r\nHello, \r\nThe Cargo Wise Excel file (" + DocfileName + ") has been successfully processed in SmartKargo. Below is the summary of the results:"
                        + "\r\n\r\nFile Name: " + DocfileName
                        + "\r\nTotal Entries in File: " + totalRecords
                        + "\r\nSuccessful Entries: " + successRecords
                        + "\r\nFailed Entries: " + failedRecords
                        + "\r\nConverted to Bookings via FFR: " + convertedToBookingRecords
                        + "\r\nFailed booking/FFR error : " + ffrFailedRecords
                        + "\r\n\r\nThanks.\r\n\r\n";

                    await DumpInterfaceInformation(sMailSubject, sMailBody, TimeStamp, "ExcelUploadBookingNotif", "", true, ConfigCache.Get("msgService_EmailId"), emailAddress, msExcel, ".xls", FileExcelURL, "0", "Outbox", FileExcelURL, msExcel, DocfileName.Replace(".xls", "").Replace(".XLS", ""));
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on SendExcelUploadNotification");
            }
        }

        public DataTable CreateFFRDetailDataTable()
        {
            try
            {
                DataTable table = new DataTable("YourTableName");

                // Define columns
                table.Columns.Add("ID", typeof(int));
                table.Columns.Add("Subject", typeof(string)).MaxLength = 200;
                table.Columns.Add("Body", typeof(string)); // MaxLength is unlimited for VARCHAR(MAX) equivalent
                table.Columns.Add("FromID", typeof(string)).MaxLength = 50;
                table.Columns.Add("ToID", typeof(string)).MaxLength = 50;
                table.Columns.Add("STATUS", typeof(string)).MaxLength = 20;
                table.Columns.Add("Type", typeof(string)).MaxLength = 50;
                table.Columns.Add("UpdatedBy", typeof(string)).MaxLength = 100;
                table.Columns.Add("UpdatedOn", typeof(DateTime));
                table.Columns.Add("AWBNumber", typeof(string)).MaxLength = 15;

                return table;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on CreateFFRDetailDataTable");
                throw;
            }
        }

        private async Task SendManageCapacityFlightLoadPlan()
        {
            // clsLog.WriteLogAzure("AutoManageCapacityFLP: In SendManageCapacityFlightLoadPlan");
            _logger.LogInformation("AutoManageCapacityFLP: In SendManageCapacityFlightLoadPlan");
            //SQLServer objSQL = new SQLServer();
            DataSet? dsFLPData = null;
            DataSet? dsEmail = null;
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
            try
            {
                string cssClassCenter = string.Empty;
                System.Text.StringBuilder strreport = new System.Text.StringBuilder();
                DataTable dtTable1 = new DataTable("FltPlan_btnPrintLoadPlan_dtTable1");
                DataTable dtTable2 = new DataTable();
                DataTable dtTailNo = new DataTable();
                DataTable dtSummary = new DataTable();
                DataTable dtFLPData2 = new DataTable();

                string FltNo = string.Empty;
                string fltDate = "", FlightDate;


                // clsLog.WriteLogAzure("AutoManageCapacityFLP: Before calling uspSendManageCapacityLoadPlan");
                _logger.LogInformation("AutoManageCapacityFLP: Before calling uspSendManageCapacityLoadPlan");

                //dsFLPData = objSQL.SelectRecords("uspSendManageCapacityLoadPlan");
                //dsEmail = objSQL.SelectRecords("usp_getTblConfigurationStatus", "MessageType", "AutoManageCapacityFLP", SqlDbType.VarChar);

                dsFLPData = await _readWriteDao.SelectRecords("uspSendManageCapacityLoadPlan");

                SqlParameter[] parameters =
                 [
                     new("@MessageType", SqlDbType.VarChar) { Value = "AutoManageCapacityFLP" }
                 ];

                dsEmail = await _readWriteDao.SelectRecords("usp_getTblConfigurationStatus", parameters);

                // clsLog.WriteLogAzure("AutoManageCapacityFLP: After called uspSendManageCapacityLoadPlan");
                _logger.LogInformation("AutoManageCapacityFLP: After called uspSendManageCapacityLoadPlan");

                if (dsFLPData != null && dsFLPData.Tables.Count > 0 && dsFLPData.Tables[0] != null && dsFLPData.Tables[0].Rows.Count > 0)
                {
                    // clsLog.WriteLogAzure("AutoManageCapacityFLP: Data is available to send load plan");
                    _logger.LogInformation("AutoManageCapacityFLP: Data is available to send load plan");

                    bool IsBatchLoad = false;
                    string BatchType = "";

                    if (dsFLPData.Tables[4].Rows.Count > 0)
                    {
                        IsBatchLoad = Convert.ToBoolean(dsFLPData.Tables[4].Rows[0]["IsBatchLoad"]);
                        BatchType = Convert.ToString(dsFLPData.Tables[4].Rows[0]["DOMINT"]);

                    }

                    if (dsEmail != null && dsEmail.Tables[1].Rows.Count > 0 && IsBatchLoad)
                    {

                        string batchtype = "";

                        if (BatchType == "DOM")
                        {
                            batchtype = "Domestic";
                        }
                        else
                        {
                            batchtype = "International";
                        }

                        dsEmail.Tables[1].DefaultView.RowFilter = "RouteType='" + batchtype + "'";

                        for (int k = 0; k < dsEmail.Tables[1].DefaultView.ToTable().Rows.Count; k++)
                        {
                            if (Convert.ToString(dsEmail.Tables[1].DefaultView.ToTable().Rows[k]["OrgAirportCode"]) != "")
                            {
                                string[] OriginCodes = Convert.ToString(dsEmail.Tables[1].DefaultView.ToTable().Rows[k]["OrgAirportCode"]).Split(',');
                                string AircraftType = Convert.ToString(dsEmail.Tables[1].DefaultView.ToTable().Rows[k]["AirCraftType"]);
                                string FlightType = Convert.ToString(dsEmail.Tables[1].DefaultView.ToTable().Rows[k]["FlightType"]);

                                foreach (var org in OriginCodes)
                                {
                                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                                    string pathhtml = Directory.GetParent(path).Parent.FullName;
                                    StringReader htmlFile = new StringReader(File.ReadAllText(pathhtml + "/Reports/CargoLoadPlan.html"));
                                    string htmlContent = htmlFile.ReadToEnd().ToString();
                                    StringBuilder sbULD = new StringBuilder(string.Empty);
                                    string logo = "";
                                    logo = pathhtml + "//Reports//Client_logo.png";
                                    htmlContent = htmlContent.Replace("@logo@", logo);
                                    dtFLPData2 = new DataTable();
                                    //var dttemp = dsFLPData.Tables[4];
                                    //dsFLPData.Tables[4].DefaultView.RowFilter = "FlightOrigin ='" + org + "'";
                                    int NoDataFound = 0;
                                    try
                                    {

                                        if (AircraftType != "" && FlightType != "")
                                        {
                                            dtFLPData2 = dsFLPData.Tables[4].AsEnumerable().Where(s => s.Field<string>("FlightOrigin") == org.Trim() && s.Field<string>("AirCraftType") == AircraftType && s.Field<string>("FlightType") == FlightType).CopyToDataTable(); //dsFLPData.Tables[4].DefaultView.ToTable();
                                        }
                                        else if (AircraftType.Trim() != "")
                                        {
                                            dtFLPData2 = dsFLPData.Tables[4].AsEnumerable().Where(s => s.Field<string>("FlightOrigin") == org.Trim() && s.Field<string>("AirCraftType") == AircraftType).CopyToDataTable();
                                        }
                                        else if (FlightType.Trim() != "")
                                        {
                                            dtFLPData2 = dsFLPData.Tables[4].AsEnumerable().Where(s => s.Field<string>("FlightOrigin") == org.Trim() && s.Field<string>("FlightType") == FlightType).CopyToDataTable();
                                        }
                                        else
                                        {
                                            dtFLPData2 = dsFLPData.Tables[4].AsEnumerable().Where(s => s.Field<string>("FlightOrigin") == org.Trim()).CopyToDataTable();
                                        }

                                    }
                                    catch
                                    {
                                        NoDataFound = 1;
                                    }


                                    if (dtFLPData2.Rows.Count > 0 && NoDataFound == 0)
                                    {
                                        //clsLog.WriteLogAzure("AutoManageCapacityFLP: Flights are available to send load plan");

                                        for (int i = 0; i < dtFLPData2.Rows.Count; i++)
                                        {
                                            string flightOrigin = string.Empty, flightDest = string.Empty, cutofftime = string.Empty;
                                            FltNo = dtFLPData2.Rows[i]["FlightNo"].ToString();
                                            fltDate = dtFLPData2.Rows[i]["FlightDate"].ToString();

                                            flightOrigin = dtFLPData2.Rows[i]["FlightOrigin"].ToString();
                                            flightDest = dtFLPData2.Rows[i]["FlightDestination"].ToString();
                                            cutofftime = dtFLPData2.Rows[i]["Cutofftime"].ToString();

                                            //clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: FltNo: " + FltNo);
                                            //clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: fltDate: " + fltDate);
                                            //clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: flightOrigin: " + flightOrigin);
                                            //clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: flightDest: " + flightDest);

                                            dsFLPData.Tables[0].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                            dtTable1 = dsFLPData.Tables[0].DefaultView.ToTable();
                                            dsFLPData.Tables[1].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                            dtTable2 = dsFLPData.Tables[1].DefaultView.ToTable();
                                            dsFLPData.Tables[2].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                            dtTailNo = dsFLPData.Tables[2].DefaultView.ToTable();
                                            dsFLPData.Tables[3].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                            dtSummary = dsFLPData.Tables[3].DefaultView.ToTable();


                                            StringBuilder FlightHeader = new StringBuilder();


                                            string FlightSchDept;

                                            //clsLog.WriteLogAzure("AutoManageCapacityFLP: Choose html file:" + FltNo + fltDate);

                                            if (dtTable1 != null && dtTable1.Rows.Count > 0)
                                            {
                                                FlightSchDept = dtTable2.Rows[0]["SchDeptTime"].ToString().Split('(')[0];
                                            }
                                            else
                                            {
                                                FlightSchDept = "";
                                            }

                                            fltDate = fltDate.Split(' ')[0];



                                            string station = "", Dest_ = "", reg = "", planner = "", ver = "";

                                            if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                            {
                                                station = dtTable2.Rows[0]["Origin"].ToString();

                                            }


                                            if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                            {

                                                reg = Convert.ToString(dtTailNo.Rows[0]["AirCraftType"]);
                                                planner = Convert.ToString(dtTailNo.Rows[0]["PlannerStaff"]);
                                                ver = Convert.ToString(dtTailNo.Rows[0]["VersionNo"]);

                                            }


                                            if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                            {
                                                Dest_ = dtTable2.Rows[0]["FlightDestination"].ToString();
                                            }


                                            FlightHeader.Append("<div><table style='width:100% '>");
                                            FlightHeader.Append("<tr>");
                                            FlightHeader.Append(" <td style='font - size:13px;'><b>Station:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + station + "</u></td>");
                                            FlightHeader.Append(" <td style='font - size:13px;'><b>Flight No:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + FltNo + "</u></td>");
                                            FlightHeader.Append(" <td style='font - size:13px;'><b>Planner:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + planner + "</u></td>");
                                            FlightHeader.Append(" <td style='font - size:13px;'><b>Destination:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + Dest_ + "</u></td>");
                                            FlightHeader.Append("  <td></td><td></td></ tr >");
                                            FlightHeader.Append("<tr>");
                                            FlightHeader.Append("<td style='font - size:13px; '><b>Registration:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + reg + "</u></td>");
                                            FlightHeader.Append("<td style='font - size:13px; '><b>Date:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + Convert.ToDateTime(fltDate).ToString("dd/MM/yyyy") + "</u></td>");
                                            FlightHeader.Append("<td style='font - size:13px; '><b>Version:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + ver + "</u></td>");
                                            FlightHeader.Append("<td style='font - size:13px; '><b>Departure Time:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + Convert.ToString(FlightSchDept.Substring(0, 5)) + "</u></td>");
                                            FlightHeader.Append("<td style='font - size:13px; '><b>Cut-Off Time:</b></td>");
                                            FlightHeader.Append(" <td style='text - align:left'><u>" + cutofftime + "</u></td>");
                                            FlightHeader.Append("</tr></table></div>");



                                            DataTable dttable3 = new DataTable();
                                            dttable3.Columns.Add("ORDER");
                                            dttable3.Columns.Add("AWBNo");
                                            dttable3.Columns.Add("PCS");
                                            dttable3.Columns.Add("Wt");
                                            dttable3.Columns.Add("Volume");
                                            dttable3.Columns.Add("ORG");
                                            dttable3.Columns.Add("DEST");
                                            dttable3.Columns.Add("CommodityDesc");
                                            dttable3.Columns.Add("SHCCode");
                                            dttable3.Columns.Add("InbFligtNo");
                                            dttable3.Columns.Add("Priority");
                                            dttable3.Columns.Add("Agent");
                                            dttable3.Columns.Add("Remark");
                                            dttable3.Columns.Add("ProductType");
                                            dttable3.Columns.Add("AllotmentCode");
                                            dttable3.Columns.Add("IsOffload");
                                            dttable3.Columns.Add("SortFlag");

                                            //clsLog.WriteLogAzure("AutoManageCapacityFLP: check data to create HTML:" + FltNo + fltDate);

                                            sbULD.Append(FlightHeader.ToString());

                                            if (dtTable1 != null && dtTable1.Rows.Count > 0)
                                            {
                                                //clsLog.WriteLogAzure("AutoManageCapacityFLP: creating HTML:" + FltNo + fltDate);

                                                for (int Count = 0; Count < dtTable1.Rows.Count; Count++)
                                                {
                                                    sbULD.Append("<div style='padding: 20px 0px 0px 0px'><table style='width:100%;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                                    sbULD.Append("<tr style='page-break-inside: avoid'>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%;' colspan=3><b> ULD# : " + Convert.ToString(dtTable1.Rows[Count]["Uldno"]) + "</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%' colspan=3><b> ULD Type : " + Convert.ToString(dtTable1.Rows[Count]["ULDType"]) + "</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;' colspan='5'></td>");
                                                    //sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%' colspan=2><b> Priority : " + Convert.ToString(dtTable1.Rows[Count]["LoadingPriority"]) + "</b></td>");
                                                    //sbULD.Append("<td class='tableBorder' style='font-size:13px;width:70%' colspan=7><b> Remarks : " + Convert.ToString(dtTable1.Rows[Count]["LoadingRemarks"]) + "</b></td>");
                                                    sbULD.Append("</tr>");

                                                    sbULD.Append("<tr style='page-break-inside: avoid'>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>ORD</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AgentDBA</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AWBNo</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>PCS</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Wt</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Volume</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>ORG</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>DEST</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>SHC</b></td>");
                                                    //sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>I/B Flight</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>CommodityDesc</b></td>");
                                                    //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Priority</b></td>");
                                                    // sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Product Type</b></td>");
                                                    //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AgentDBA</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Remark</b></td>");
                                                    sbULD.Append("</tr>");
                                                    StringBuilder sbBulkload = new StringBuilder(string.Empty);

                                                    if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                                    {
                                                        DataTable dtTable4 = new DataTable();

                                                        for (int j = 0; j < dtTable2.Rows.Count; j++)
                                                        {
                                                            DataRow[] DtRow = new DataRow[0];
                                                            Array.Resize(ref DtRow, DtRow.Length + 1);

                                                            if (dtTable2.Rows[j]["ULD"].ToString() == dtTable1.Rows[Count]["Uldno"].ToString())
                                                            {
                                                                DtRow[DtRow.Length - 1] = dttable3.NewRow();
                                                                DtRow[DtRow.Length - 1]["ORDER"] = dtTable2.Rows[j]["ORDER"].ToString();
                                                                DtRow[DtRow.Length - 1]["Agent"] = Convert.ToString(dtTable2.Rows[j]["AgentDBA"]);
                                                                DtRow[DtRow.Length - 1]["AWBNo"] = dtTable2.Rows[j]["awbnumber"].ToString();
                                                                DtRow[DtRow.Length - 1]["PCS"] = dtTable2.Rows[j]["builtpcs"].ToString();
                                                                DtRow[DtRow.Length - 1]["Wt"] = dtTable2.Rows[j]["builtwgt"].ToString();
                                                                DtRow[DtRow.Length - 1]["Volume"] = dtTable2.Rows[j]["PlanVolume"].ToString();
                                                                DtRow[DtRow.Length - 1]["ORG"] = dtTable2.Rows[j]["Origin"].ToString();
                                                                DtRow[DtRow.Length - 1]["DEST"] = dtTable2.Rows[j]["Destination"].ToString();
                                                                DtRow[DtRow.Length - 1]["CommodityDesc"] = dtTable2.Rows[j]["description"].ToString();
                                                                DtRow[DtRow.Length - 1]["SHCCode"] = Convert.ToString(dtTable2.Rows[j]["SHCCode"]);
                                                                DtRow[DtRow.Length - 1]["InbFligtNo"] = Convert.ToString(dtTable2.Rows[j]["InbFligtNo"]);
                                                                // DtRow[DtRow.Length - 1]["Priority"] = Convert.ToString(dtTable2.Rows[j]["BookingPriority"]);
                                                                DtRow[DtRow.Length - 1]["Remark"] = Convert.ToString(dtTable2.Rows[j]["BookingRemark"]);
                                                                // DtRow[DtRow.Length - 1]["ProductType"] = Convert.ToString(dtTable2.Rows[j]["ProductType"]);
                                                                //DtRow[DtRow.Length - 1]["Agent"] = Convert.ToString(dtTable2.Rows[j]["AgentDBA"]);
                                                                DtRow[DtRow.Length - 1]["AllotmentCode"] = Convert.ToString(dtTable2.Rows[j]["AllotmentCode"]);
                                                                DtRow[DtRow.Length - 1]["Isoffload"] = Convert.ToString(dtTable2.Rows[j]["Isoffload"]);
                                                                DtRow[DtRow.Length - 1]["SortFlag"] = Convert.ToString(dtTable2.Rows[j]["SortFlag"]);
                                                                dttable3.Rows.Add(DtRow[DtRow.Length - 1]);
                                                            }
                                                        }

                                                        if (dttable3 != null && dttable3.Rows.Count > 0)
                                                        {
                                                            var DtAgent = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "A").ToList();
                                                            var DTSwiftAgent = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "SWIFT247").ToList();
                                                            var DTHardOffloadBlock = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "OHB").ToList();
                                                            var DTSoftOffloadBlock = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "SHB").ToList();

                                                            int count = 0;

                                                            if (DtAgent.ToList().Count > 0)
                                                            {
                                                                foreach (var item in DtAgent)
                                                                {
                                                                    sbULD.Append("<tbody style='display:table-row-group;'>");
                                                                    sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                                    count += 1;
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder'></td></tr>");
                                                                }

                                                            }
                                                            sbULD.Append("<tr><td class='tableBorder' style='font-size:12px; text-align: center;font-weight:Bold;color:Red' colspan='11'>SWIFT247</td></tr>");

                                                            if (DTSwiftAgent.ToList().Count > 0)
                                                            {
                                                                foreach (var item in DTSwiftAgent)
                                                                {
                                                                    sbULD.Append("<tbody style='display:table-row-group;'>");
                                                                    sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                                    count += 1;
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder'></td></tr>");
                                                                }
                                                            }

                                                            sbULD.Append("<tr><td class='tableBorder' style='font-size:12px; text-align: center;font-weight:Bold;color:Red' colspan='11'>ALL OFFLOAD HARDBLOCK</td></tr>");

                                                            if (DTHardOffloadBlock.ToList().Count > 0)
                                                            {
                                                                foreach (var item in DTHardOffloadBlock)
                                                                {
                                                                    sbULD.Append("<tbody style='display:table-row-group;'>");
                                                                    sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                                    count += 1;
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");

                                                                    if (Convert.ToString(item.Field<string>("AllotmentCode")) == "")
                                                                    {
                                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;width:25%;vertical-align: middle;'>" + Convert.ToString(item.Field<string>("Remark")) + "</td>");
                                                                    }
                                                                    else
                                                                    {
                                                                        if (Convert.ToString(item.Field<string>("Remark")) == "")
                                                                        {
                                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;width:25%'>" + "HARDBLOCK" + "</td>");
                                                                        }
                                                                        else
                                                                        {
                                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;width:25%;vertical-align: middle;' >" + "HARDBLOCK- " + Convert.ToString(item.Field<string>("Remark")) + "</td>");
                                                                        }

                                                                    }

                                                                    sbULD.Append("</tr>");

                                                                }
                                                            }

                                                            sbULD.Append("<tr><td class='tableBorder' style='font-size:12px; text-align: center;font-weight:Bold; color:Red' colspan='11'>ALL OFFLOAD SOFTBLOCK</td></tr>");

                                                            if (DTSoftOffloadBlock.ToList().Count > 0)
                                                            {
                                                                foreach (var item in DTSoftOffloadBlock)
                                                                {
                                                                    sbULD.Append("<tbody style='display:table-row-group;'>");
                                                                    sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                                    count += 1;
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");
                                                                    sbULD.Append("<td class='tableBorder'></td></tr>");
                                                                }
                                                            }

                                                            //add total row per ULD/Bulk
                                                            sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;width: 110px'><b>Total</b></td>");
                                                            sbULD.Append("<td class='tableBorder'></td>");
                                                            sbULD.Append("<td class='tableBorder'></td>");
                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["BookedPcs"]) + "</b></td>");
                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["BookedWgt"]) + "</b></td>");
                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["PlanVolume"]) + "</b></td>");
                                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;' colspan='5'></td>");
                                                            sbULD.Append("</tr>");
                                                            sbULD.Append("</tbody>");
                                                            //end total
                                                            sbULD.Append("</table></div>");
                                                            // sbULD.Append("</table>");
                                                            sbULD.Append("</br>");
                                                            //delete previous row 

                                                            for (int m = dttable3.Rows.Count - 1; m >= 0; m--)
                                                            {
                                                                DataRow dr = dttable3.Rows[m];
                                                                dr.Delete();
                                                                //if (dttable3.Columns.Contains("SeqNo") == false)
                                                                //{ dttable3.Columns.Add("SeqNo"); }

                                                                if (dttable3.Columns.Contains("AWBNo") == false)
                                                                { dttable3.Columns.Add("AWBNo"); }

                                                                if (dttable3.Columns.Contains("PCS") == false)
                                                                { dttable3.Columns.Add("PCS"); }

                                                                if (dttable3.Columns.Contains("Wt") == false)
                                                                { dttable3.Columns.Add("Wt"); }

                                                                if (dttable3.Columns.Contains("Volume") == false)
                                                                { dttable3.Columns.Add("Volume"); }

                                                                if (dttable3.Columns.Contains("ORG") == false)
                                                                { dttable3.Columns.Add("ORG"); }

                                                                if (dttable3.Columns.Contains("DEST") == false)
                                                                { dttable3.Columns.Add("DEST"); }

                                                                if (dttable3.Columns.Contains("CommodityDesc") == false)
                                                                { dttable3.Columns.Add("CommodityDesc"); }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbBulkload.Append(" <tr class='tableBorder' style='page-break-inside: avoid'>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                        sbBulkload.Append("<td class='tableBorder'></td>");
                                                        sbBulkload.Append("</tr>");
                                                        //htmlContent = htmlContent.Replace("@BULKLOADDETAILS@", Convert.ToString(sbBulkload.ToString()));
                                                    }
                                                }
                                                //summary Table.
                                                if (dtSummary != null && dtSummary.Rows.Count > 0)
                                                {
                                                    sbULD.Append("<table style='width:50%;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                                    sbULD.Append("<tr style='page-break-inside: avoid'>");
                                                    sbULD.Append("<td rowspan='2' class='tableBorder' style='font-size:13px;width: 100px;'><b>Total Planned</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Pieces</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Weight</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Volume</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>ULD</b></td>");
                                                    sbULD.Append("</tr>");
                                                    sbULD.Append("<tr style='page-break-inside: avoid'>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalPcs"]) + "</td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalWgt"]) + "</td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalVolume"]) + "</td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalULD"]) + "</td>");
                                                    sbULD.Append("</tr>");
                                                    sbULD.Append("</table>");
                                                    sbULD.Append("</br>");
                                                }
                                                //
                                            }
                                            else
                                            {

                                                sbULD.Append(" <tr class='tableBorder' style='page-break-inside: avoid'>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'>-</td>");
                                                sbULD.Append("<td class='tableBorder'></td>");
                                                sbULD.Append("</tr>");
                                                //htmlContent = htmlContent.Replace("@ULDLOADDETAILS@", Convert.ToString(sbULD.ToString()));
                                            }

                                            sbULD.Append("<table style='width:49%; float:right;margin-top: -64px;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                            sbULD.Append("<tr style='page-break-inside: avoid'>");
                                            sbULD.Append("<th class='tableBorder' style='font-size:13px;'><b>Remark</b></th>");
                                            sbULD.Append("</tr>");
                                            sbULD.Append("<tr style='page-break-inside: avoid'>");
                                            if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                            {
                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;'>" + Convert.ToString(dtTailNo.Rows[0]["FlightRemark"]) + "</td>");

                                            }
                                            else
                                            {
                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;'></td>");

                                            }
                                            sbULD.Append("</tr>");
                                            sbULD.Append("</table>");
                                            sbULD.Append("</br>");




                                            //null Objects
                                            dtTable1 = null;
                                            dtTable2 = null;
                                            dttable3 = null;
                                            dtTailNo = null;
                                            dtSummary = null;
                                            //sbULD = null;

                                        }
                                    }
                                    //
                                    if (NoDataFound == 0)
                                    {
                                        htmlContent = htmlContent.Replace("@BULKLOADDETAILS@", Convert.ToString(sbULD.ToString()));

                                        string Domestic = "";

                                        if (BatchType == "DOM")
                                        {
                                            Domestic = "Domestic";
                                        }
                                        else
                                        {
                                            Domestic = "International";
                                        }

                                        if (fltDate == "")
                                        {
                                            fltDate = DateTime.Now.ToString();
                                        }

                                        DateTime TimeStamp = DateTime.Now;
                                        string sMailSubject = "VJC " + Domestic + " Load Plan " + Convert.ToDateTime(fltDate).ToString("dd/MM/yyyy") + " Ex:" + org;
                                        System.String sMailBody = "\r\nDear All, \r\n\t<br/><br/>Please see " + Domestic + " Load Plan for " + Convert.ToDateTime(fltDate).ToString("dd/MM/yyyy") + " Ex:" + org + " is below:";


                                        sMailBody = sMailBody + "\n" + htmlContent;


                                        string EmailID = Convert.ToString(dsEmail.Tables[1].DefaultView.ToTable().Rows[k]["PartnerEmailiD"]);
                                        //clsLog.WriteLogAzure("AutoManageCapacityFLP: ToID: " + EmailID);

                                        addMsgToOutBox(sMailSubject, sMailBody, "", EmailID, false, true, "AutoManageCapacityFLP");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        if (dsFLPData.Tables[4].Rows.Count > 0)
                        {
                            // clsLog.WriteLogAzure("AutoManageCapacityFLP: Flights are available to send load plan");
                            _logger.LogInformation("AutoManageCapacityFLP: Flights are available to send load plan");

                            for (int i = 0; i < dsFLPData.Tables[4].Rows.Count; i++)
                            {
                                string flightOrigin = string.Empty, flightDest = string.Empty, cutofftime = string.Empty, aircrafttype = string.Empty, flighttype = string.Empty, routetype = string.Empty;
                                FltNo = dsFLPData.Tables[4].Rows[i]["FlightNo"].ToString();
                                fltDate = dsFLPData.Tables[4].Rows[i]["FlightDate"].ToString();

                                flightOrigin = dsFLPData.Tables[4].Rows[i]["FlightOrigin"].ToString();
                                flightDest = dsFLPData.Tables[4].Rows[i]["FlightDestination"].ToString();
                                cutofftime = dsFLPData.Tables[4].Rows[i]["Cutofftime"].ToString();
                                aircrafttype = dsFLPData.Tables[4].Rows[i]["AirCraftType"].ToString();
                                flighttype = dsFLPData.Tables[4].Rows[i]["FlightType"].ToString();
                                routetype = dsFLPData.Tables[4].Rows[i]["DOMINT"].ToString();

                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: FltNo: " + FltNo);
                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: fltDate: " + fltDate);
                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: flightOrigin: " + flightOrigin);
                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: Flight details: flightDest: " + flightDest);
                                _logger.LogInformation("AutoManageCapacityFLP: Flight details: FltNo: {0}", FltNo);
                                _logger.LogInformation("AutoManageCapacityFLP: Flight details: fltDate: {0}", fltDate);
                                _logger.LogInformation("AutoManageCapacityFLP: Flight details: flightOrigin: {0}", flightOrigin);
                                _logger.LogInformation("AutoManageCapacityFLP: Flight details: flightDest: {0}", flightDest);

                                dsFLPData.Tables[0].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                dtTable1 = dsFLPData.Tables[0].DefaultView.ToTable();
                                dsFLPData.Tables[1].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                dtTable2 = dsFLPData.Tables[1].DefaultView.ToTable();
                                dsFLPData.Tables[2].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                dtTailNo = dsFLPData.Tables[2].DefaultView.ToTable();
                                dsFLPData.Tables[3].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                dtSummary = dsFLPData.Tables[3].DefaultView.ToTable();

                                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                                string pathhtml = Directory.GetParent(path).Parent.FullName;
                                StringReader htmlFile = new StringReader(File.ReadAllText(pathhtml + "/Reports/CargoLoadPlan.html"));
                                string htmlContent = htmlFile.ReadToEnd().ToString();
                                string logo = "";
                                logo = pathhtml + "//Reports//Client_logo.png";
                                string FlightSchDept;

                                StringBuilder FlightHeader = new StringBuilder();



                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: Choose html file:" + FltNo + fltDate);
                                _logger.LogInformation("AutoManageCapacityFLP: Choose html file: {0}", FltNo + fltDate);

                                if (dtTable1 != null && dtTable1.Rows.Count > 0)
                                {
                                    FlightSchDept = dtTable2.Rows[0]["SchDeptTime"].ToString().Split('(')[0];
                                }
                                else
                                {
                                    FlightSchDept = "";
                                }

                                fltDate = fltDate.Split(' ')[0];



                                string station = "", Dest_ = "", reg = "", planner = "", ver = "";

                                if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                {
                                    station = dtTable2.Rows[0]["Origin"].ToString();

                                }


                                if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                {

                                    reg = Convert.ToString(dtTailNo.Rows[0]["AirCraftType"]);
                                    planner = Convert.ToString(dtTailNo.Rows[0]["PlannerStaff"]);
                                    ver = Convert.ToString(dtTailNo.Rows[0]["VersionNo"]);

                                }


                                if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                {
                                    Dest_ = dtTable2.Rows[0]["FlightDestination"].ToString();
                                }


                                FlightHeader.Append("<div><table style='width:100%'>");
                                FlightHeader.Append("<tr>");
                                FlightHeader.Append(" <td style='font - size:13px;'><b>Station:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + station + "</u></td>");
                                FlightHeader.Append(" <td style='font - size:13px;'><b>Flight No:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + FltNo + "</u></td>");
                                FlightHeader.Append(" <td style='font - size:13px;'><b>Planner:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + planner + "</u></td>");
                                FlightHeader.Append(" <td style='font - size:13px;'><b>Destination:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + Dest_ + "</u></td>");
                                FlightHeader.Append("  <td></td><td></td></ tr >");
                                FlightHeader.Append("<tr>");
                                FlightHeader.Append("<td style='font - size:13px; '><b>Registration:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + reg + "</u></td>");
                                FlightHeader.Append("<td style='font - size:13px; '><b>Date:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + Convert.ToDateTime(fltDate).ToString("dd/MM/yyyy") + "</u></td>");
                                FlightHeader.Append("<td style='font - size:13px; '><b>Version:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + ver + "</u></td>");
                                FlightHeader.Append("<td style='font - size:13px; '><b>Departure Time:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + Convert.ToString(FlightSchDept.Substring(0, 5)) + "</u></td>");
                                FlightHeader.Append("<td style='font - size:13px; '><b>Cut-Off Time:</b></td>");
                                FlightHeader.Append(" <td style='text - align:left'><u>" + cutofftime + "</u></td>");
                                FlightHeader.Append("</tr></table></div>");

                                fltDate = fltDate.Split(' ')[0];

                                htmlContent = htmlContent.Replace("@logo@", logo);
                                //if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                //{
                                //    htmlContent = htmlContent.Replace("@Station@", dtTable2.Rows[0]["Origin"].ToString());

                                //}
                                //else
                                //{
                                //    htmlContent = htmlContent.Replace("@Station@", "");

                                //}
                                //htmlContent = htmlContent.Replace("@FlightNo@", FltNo);
                                //if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                //{

                                //    htmlContent = htmlContent.Replace("@Registration@", Convert.ToString(dtTailNo.Rows[0]["AirCraftType"]));
                                //    htmlContent = htmlContent.Replace("@Planner@", Convert.ToString(dtTailNo.Rows[0]["PlannerStaff"]));
                                //    htmlContent = htmlContent.Replace("@Version@", Convert.ToString(dtTailNo.Rows[0]["VersionNo"]));

                                //}
                                //else
                                //{
                                //    htmlContent = htmlContent.Replace("@Registration@", "");
                                //    htmlContent = htmlContent.Replace("@Planner@", "");
                                //    htmlContent = htmlContent.Replace("@Version@", "");
                                //}

                                //if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                //{
                                //    htmlContent = htmlContent.Replace("@Dest@", dtTable2.Rows[0]["Destination"].ToString());
                                //}
                                //else
                                //{
                                //    htmlContent = htmlContent.Replace("@Dest@", "");
                                //}
                                //htmlContent = htmlContent.Replace("@DeptTime@", Convert.ToString(FlightSchDept.Substring(0, 5)));
                                //htmlContent = htmlContent.Replace("@Date@", Convert.ToDateTime(fltDate).ToString("dd/MM/yyyy"));

                                StringBuilder sbULD = new StringBuilder(string.Empty);
                                DataTable dttable3 = new DataTable();
                                dttable3.Columns.Add("ORDER");
                                dttable3.Columns.Add("AWBNo");
                                dttable3.Columns.Add("PCS");
                                dttable3.Columns.Add("Wt");
                                dttable3.Columns.Add("Volume");
                                dttable3.Columns.Add("ORG");
                                dttable3.Columns.Add("DEST");
                                dttable3.Columns.Add("CommodityDesc");
                                dttable3.Columns.Add("SHCCode");
                                dttable3.Columns.Add("InbFligtNo");
                                dttable3.Columns.Add("Priority");
                                dttable3.Columns.Add("Agent");
                                dttable3.Columns.Add("Remark");
                                dttable3.Columns.Add("ProductType");
                                dttable3.Columns.Add("AllotmentCode");
                                dttable3.Columns.Add("IsOffload");
                                dttable3.Columns.Add("SortFlag");

                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: check data to create HTML:" + FltNo + fltDate);
                                _logger.LogInformation("AutoManageCapacityFLP: check data to create HTML:{0}", FltNo + fltDate);
                                sbULD.Append(FlightHeader.ToString());
                                if (dtTable1 != null && dtTable1.Rows.Count > 0)
                                {
                                    // clsLog.WriteLogAzure("AutoManageCapacityFLP: creating HTML:" + FltNo + fltDate);
                                    _logger.LogInformation("AutoManageCapacityFLP: creating HTML:{0}", FltNo + fltDate);

                                    for (int Count = 0; Count < dtTable1.Rows.Count; Count++)
                                    {
                                        sbULD.Append("<div style='padding: 20px 0px 0px 0px'><table style='width:100%;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                        sbULD.Append("<tr style='page-break-inside: avoid'>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%;' colspan=3><b> ULD# : " + Convert.ToString(dtTable1.Rows[Count]["Uldno"]) + "</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%' colspan=3><b> ULD Type : " + Convert.ToString(dtTable1.Rows[Count]["ULDType"]) + "</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;' colspan='5'></td>");
                                        //sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%' colspan=2><b> Priority : " + Convert.ToString(dtTable1.Rows[Count]["LoadingPriority"]) + "</b></td>");
                                        //sbULD.Append("<td class='tableBorder' style='font-size:13px;width:70%' colspan=7><b> Remarks : " + Convert.ToString(dtTable1.Rows[Count]["LoadingRemarks"]) + "</b></td>");
                                        sbULD.Append("</tr>");

                                        sbULD.Append("<tr style='page-break-inside: avoid'>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>ORD</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AgentDBA</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AWBNo</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>PCS</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Wt</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Volume</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>ORG</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>DEST</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>SHC</b></td>");
                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>I/B Flight</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>CommodityDesc</b></td>");
                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Priority</b></td>");
                                        // sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Product Type</b></td>");
                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AgentDBA</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Remark</b></td>");
                                        sbULD.Append("</tr>");
                                        StringBuilder sbBulkload = new StringBuilder(string.Empty);

                                        if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                        {
                                            DataTable dtTable4 = new DataTable();

                                            for (int j = 0; j < dtTable2.Rows.Count; j++)
                                            {
                                                DataRow[] DtRow = new DataRow[0];
                                                Array.Resize(ref DtRow, DtRow.Length + 1);

                                                if (dtTable2.Rows[j]["ULD"].ToString() == dtTable1.Rows[Count]["Uldno"].ToString())
                                                {
                                                    DtRow[DtRow.Length - 1] = dttable3.NewRow();
                                                    DtRow[DtRow.Length - 1]["ORDER"] = dtTable2.Rows[j]["ORDER"].ToString();
                                                    DtRow[DtRow.Length - 1]["Agent"] = Convert.ToString(dtTable2.Rows[j]["AgentDBA"]);
                                                    DtRow[DtRow.Length - 1]["AWBNo"] = dtTable2.Rows[j]["awbnumber"].ToString();
                                                    DtRow[DtRow.Length - 1]["PCS"] = dtTable2.Rows[j]["builtpcs"].ToString();
                                                    DtRow[DtRow.Length - 1]["Wt"] = dtTable2.Rows[j]["builtwgt"].ToString();
                                                    DtRow[DtRow.Length - 1]["Volume"] = dtTable2.Rows[j]["PlanVolume"].ToString();
                                                    DtRow[DtRow.Length - 1]["ORG"] = dtTable2.Rows[j]["Origin"].ToString();
                                                    DtRow[DtRow.Length - 1]["DEST"] = dtTable2.Rows[j]["Destination"].ToString();
                                                    DtRow[DtRow.Length - 1]["CommodityDesc"] = dtTable2.Rows[j]["description"].ToString();
                                                    DtRow[DtRow.Length - 1]["SHCCode"] = Convert.ToString(dtTable2.Rows[j]["SHCCode"]);
                                                    DtRow[DtRow.Length - 1]["InbFligtNo"] = Convert.ToString(dtTable2.Rows[j]["InbFligtNo"]);
                                                    // DtRow[DtRow.Length - 1]["Priority"] = Convert.ToString(dtTable2.Rows[j]["BookingPriority"]);
                                                    DtRow[DtRow.Length - 1]["Remark"] = Convert.ToString(dtTable2.Rows[j]["BookingRemark"]);
                                                    // DtRow[DtRow.Length - 1]["ProductType"] = Convert.ToString(dtTable2.Rows[j]["ProductType"]);
                                                    //DtRow[DtRow.Length - 1]["Agent"] = Convert.ToString(dtTable2.Rows[j]["AgentDBA"]);
                                                    DtRow[DtRow.Length - 1]["AllotmentCode"] = Convert.ToString(dtTable2.Rows[j]["AllotmentCode"]);
                                                    DtRow[DtRow.Length - 1]["Isoffload"] = Convert.ToString(dtTable2.Rows[j]["Isoffload"]);
                                                    DtRow[DtRow.Length - 1]["SortFlag"] = Convert.ToString(dtTable2.Rows[j]["SortFlag"]);
                                                    dttable3.Rows.Add(DtRow[DtRow.Length - 1]);
                                                }
                                            }

                                            if (dttable3 != null && dttable3.Rows.Count > 0)
                                            {
                                                var DtAgent = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "A").ToList();
                                                var DTSwiftAgent = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "SWIFT247").ToList();
                                                var DTHardOffloadBlock = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "OHB").ToList();
                                                var DTSoftOffloadBlock = dttable3.AsEnumerable().Where(s => s.Field<string>("SortFlag") == "SHB").ToList();

                                                int count = 0;

                                                if (DtAgent.ToList().Count > 0)
                                                {
                                                    foreach (var item in DtAgent)
                                                    {
                                                        sbULD.Append("<tbody style='display:table-row-group;'>");
                                                        sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                        count += 1;
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder'></td></tr>");
                                                    }

                                                }
                                                sbULD.Append("<tr><td class='tableBorder' style='font-size:12px; text-align: center;font-weight:Bold;color:Red' colspan='11'>SWIFT247</td></tr>");

                                                if (DTSwiftAgent.ToList().Count > 0)
                                                {
                                                    foreach (var item in DTSwiftAgent)
                                                    {
                                                        sbULD.Append("<tbody style='display:table-row-group;'>");
                                                        sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                        count += 1;
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder'></td></tr>");
                                                    }
                                                }

                                                sbULD.Append("<tr><td class='tableBorder' style='font-size:12px; text-align: center;font-weight:Bold;color:Red' colspan='11'>ALL OFFLOAD HARDBLOCK</td></tr>");

                                                if (DTHardOffloadBlock.ToList().Count > 0)
                                                {
                                                    foreach (var item in DTHardOffloadBlock)
                                                    {
                                                        sbULD.Append("<tbody style='display:table-row-group;'>");
                                                        sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                        count += 1;
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");

                                                        if (Convert.ToString(item.Field<string>("AllotmentCode")) == "")
                                                        {
                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;width:25%;vertical-align: middle;'>" + Convert.ToString(item.Field<string>("Remark")) + "</td>");
                                                        }
                                                        else
                                                        {
                                                            if (Convert.ToString(item.Field<string>("Remark")) == "")
                                                            {
                                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;width:25%'>" + "HARDBLOCK" + "</td>");
                                                            }
                                                            else
                                                            {
                                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;width:25%;vertical-align: middle;' >" + "HARDBLOCK- " + Convert.ToString(item.Field<string>("Remark")) + "</td>");
                                                            }

                                                        }

                                                        sbULD.Append("</tr>");

                                                    }
                                                }

                                                sbULD.Append("<tr><td class='tableBorder' style='font-size:12px; text-align: center;font-weight:Bold; color:Red' colspan='11'>ALL OFFLOAD SOFTBLOCK</td></tr>");

                                                if (DTSoftOffloadBlock.ToList().Count > 0)
                                                {
                                                    foreach (var item in DTSoftOffloadBlock)
                                                    {
                                                        sbULD.Append("<tbody style='display:table-row-group;'>");
                                                        sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                        count += 1;
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center; width:3%'>" + count + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:16%'>" + Convert.ToString(item.Field<string>("Agent")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:13%'>" + Convert.ToString(item.Field<string>("AWBNo")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:5%'>" + Convert.ToString(item.Field<string>("PCS")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:7%'>" + Convert.ToString(item.Field<string>("Wt")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right; width:6%'>" + Convert.ToString(item.Field<string>("Volume")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("ORG")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; width:4%'>" + Convert.ToString(item.Field<string>("DEST")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:3%' >" + Convert.ToString(item.Field<string>("SHCCode")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left; word-wrap:break-word ;width:32%' >" + Convert.ToString(item.Field<string>("CommodityDesc")) + "</td>");
                                                        sbULD.Append("<td class='tableBorder'></td></tr>");
                                                    }
                                                }

                                                //add total row per ULD/Bulk
                                                sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;width: 110px'><b>Total</b></td>");
                                                sbULD.Append("<td class='tableBorder'></td>");
                                                sbULD.Append("<td class='tableBorder'></td>");
                                                sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["BookedPcs"]) + "</b></td>");
                                                sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["BookedWgt"]) + "</b></td>");
                                                sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["PlanVolume"]) + "</b></td>");
                                                sbULD.Append("<td class='tableBorder' style='font-size:13px;' colspan='5'></td>");
                                                sbULD.Append("</tr>");
                                                sbULD.Append("</tbody>");
                                                //end total
                                                sbULD.Append("</table></div>");
                                                // sbULD.Append("</table>");
                                                sbULD.Append("</br>");
                                                //delete previous row 

                                                for (int m = dttable3.Rows.Count - 1; m >= 0; m--)
                                                {
                                                    DataRow dr = dttable3.Rows[m];
                                                    dr.Delete();
                                                    //if (dttable3.Columns.Contains("SeqNo") == false)
                                                    //{ dttable3.Columns.Add("SeqNo"); }

                                                    if (dttable3.Columns.Contains("AWBNo") == false)
                                                    { dttable3.Columns.Add("AWBNo"); }

                                                    if (dttable3.Columns.Contains("PCS") == false)
                                                    { dttable3.Columns.Add("PCS"); }

                                                    if (dttable3.Columns.Contains("Wt") == false)
                                                    { dttable3.Columns.Add("Wt"); }

                                                    if (dttable3.Columns.Contains("Volume") == false)
                                                    { dttable3.Columns.Add("Volume"); }

                                                    if (dttable3.Columns.Contains("ORG") == false)
                                                    { dttable3.Columns.Add("ORG"); }

                                                    if (dttable3.Columns.Contains("DEST") == false)
                                                    { dttable3.Columns.Add("DEST"); }

                                                    if (dttable3.Columns.Contains("CommodityDesc") == false)
                                                    { dttable3.Columns.Add("CommodityDesc"); }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            sbBulkload.Append(" <tr class='tableBorder' style='page-break-inside: avoid'>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'>-</td>");
                                            sbBulkload.Append("<td class='tableBorder'></td>");
                                            sbBulkload.Append("</tr>");
                                            htmlContent = htmlContent.Replace("@BULKLOADDETAILS@", Convert.ToString(sbBulkload.ToString()));
                                        }
                                    }
                                    //summary Table.
                                    if (dtSummary != null && dtSummary.Rows.Count > 0)
                                    {
                                        sbULD.Append("<table style='width:50%;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                        sbULD.Append("<tr style='page-break-inside: avoid'>");
                                        sbULD.Append("<td rowspan='2' class='tableBorder' style='font-size:13px;width: 100px;'><b>Total Planned</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Pieces</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Weight</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Volume</b></td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>ULD</b></td>");
                                        sbULD.Append("</tr>");
                                        sbULD.Append("<tr style='page-break-inside: avoid'>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalPcs"]) + "</td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalWgt"]) + "</td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalVolume"]) + "</td>");
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalULD"]) + "</td>");
                                        sbULD.Append("</tr>");
                                        sbULD.Append("</table>");
                                        sbULD.Append("</br>");
                                    }
                                    //
                                }
                                else
                                {

                                    sbULD.Append(" <tr class='tableBorder' style='page-break-inside: avoid'>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'>-</td>");
                                    sbULD.Append("<td class='tableBorder'></td>");
                                    sbULD.Append("</tr>");
                                    htmlContent = htmlContent.Replace("@ULDLOADDETAILS@", Convert.ToString(sbULD.ToString()));
                                }

                                sbULD.Append("<table style='width:49%; float:right;margin-top: -64px;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                sbULD.Append("<tr style='page-break-inside: avoid'>");
                                sbULD.Append("<th class='tableBorder' style='font-size:13px;'><b>Remark</b></th>");
                                sbULD.Append("</tr>");
                                sbULD.Append("<tr style='page-break-inside: avoid'>");
                                if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                {
                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'>" + Convert.ToString(dtTailNo.Rows[0]["FlightRemark"]) + "</td>");

                                }
                                else
                                {
                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;'></td>");

                                }
                                sbULD.Append("</tr>");
                                sbULD.Append("</table>");
                                sbULD.Append("</br>");

                                htmlContent = htmlContent.Replace("@BULKLOADDETAILS@", Convert.ToString(sbULD.ToString()));

                                //convert to pdf
                                //HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
                                //var margins = new NReco.PdfGenerator.PageMargins();
                                //margins.Bottom = 10;
                                //margins.Top = 10;
                                //margins.Left = 6;
                                //margins.Right = 5;
                                //htmlToPdfConverter.Margins = margins;
                                //htmlToPdfConverter.PageFooterHtml = $@"<div style='float:right;font-family: verdana;font-size: large;margin-right: 5%;margin-bottom: 13%'>  page <span class=""page""></span> of <span class=""topage""></span></div>";

                                //var pdfBytes = htmlToPdfConverter.GeneratePdf(htmlContent);
                                //var byteArray = Encoding.ASCII.GetBytes(htmlContent);
                                //MemoryStream msExcel = new MemoryStream(byteArray);
                                //MemoryStream ms = new MemoryStream(pdfBytes);

                                //GenericFunction genericFunction = new GenericFunction();

                                FlightDate = dsFLPData.Tables[4].Rows[i]["FLTDate"].ToString();



                                //clsLog.WriteLogAzure("AutoManageCapacityFLP: Uploading files to blob: " + DocfileName);
                                //string sFileUrl = UploadToBlob(ms, DocfileName + ".pdf", "AutoManageCapacityFLP");
                                //string FileExcelURL = UploadToBlob(msExcel, DocfileName + ".xls", "AutoManageCapacityFLP");

                                //clsLog.WriteLogAzure("AutoManageCapacityFLP: Uploaded files to blob:" + DocfileName);

                                DateTime TimeStamp = DateTime.Now;
                                string sMailSubject = "FLIGHT LOAD PLAN FOR " + FltNo + "/" + Convert.ToDateTime(FlightDate).ToString("ddMMMyyyy");
                                System.String sMailBody = "\r\nDear All, \r\n\t<br/><br/>Please see Flight Load Plan for " + " Flight No: " + FltNo + ", Flight Date : " + Convert.ToDateTime(FlightDate).ToString("dd/MM/yyyy") + "\r\n\r\n";
                                sMailBody = sMailBody + htmlContent;
                                //clsLog.WriteLogAzure("AutoManageCapacityFLP: Parameters to GetSitaAddressandMessageVersion: " + FltNo.Substring(0, 2) + "-" + "AutoManageCapacityFLP" + "-" + "AIR" + "-" + flightOrigin + "-" + flightDest + "-" + "''" + "-" + "''");

                                DataSet dsconfiguration = await _genericFunction.GetSitaAddressandMessageVersion(FltNo.Substring(0, 2), "AutoManageCapacityFLP", "AIR", flightOrigin, flightDest, "", string.Empty, string.Empty, false, aircrafttype, flighttype, routetype);

                                string EmailID = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                                // clsLog.WriteLogAzure("AutoManageCapacityFLP: ToID: " + EmailID);
                                _logger.LogInformation("AutoManageCapacityFLP: ToID: {0}", EmailID);

                                //clsLog.WriteLogAzure("AutoManageCapacityFLP: sMailSubject: " + sMailSubject.Length.ToString()
                                //    + ":: sMailBody: " + sMailBody.Length.ToString()
                                //    + ":: ms: " + ms.Length.ToString()
                                //    + ":: sFileUrl: " + sFileUrl.Length.ToString()
                                //    + ":: FileExcelURL: " + FileExcelURL.Length.ToString()
                                //    + ":: msExcel: " + msExcel.Length.ToString());
                                addMsgToOutBox(sMailSubject, sMailBody, "", EmailID, false, true, "AutoManageCapacityFLP");
                                //DumpInterfaceInformation(sMailSubject, sMailBody, TimeStamp, "AutoManageCapacityFLP", "", true, ConfigCache.Get("msgService_EmailId"), EmailID, ms, ".pdf", sFileUrl, "0", "Outbox", FileExcelURL, msExcel, DocfileName);

                                //clsLog.WriteLogAzure("DumpInterfaceInformation: Message sent for: " + DocfileName);

                                //null Objects
                                dtTable1 = null;
                                dtTable2 = null;
                                dttable3 = null;
                                dtTailNo = null;
                                dtSummary = null;
                                sbULD = null;
                                //htmlToPdfConverter = null;
                                //ms = null;
                                //sFileUrl = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("DumpInterfaceInformation: Exception");
                // clsLog.WriteLogAzure(ex);
                _logger.LogError("DumpInterfaceInformation: Exception");
                _logger.LogError(ex, "Error on SendManageCapacityFlightLoadPlan"); //(ex);
            }
            //finally
            //{
            //    objSQL = null;
            //    dsFLPData = null;
            //    GC.Collect();
            //}
        }

        // Added by Aishwarya for VJ-32
        private async Task SendFlightLoadPlan()
        {
            //SQLServer objSQL = new SQLServer();
            DataSet? dsFLPData = null;
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
            try
            {
                string cssClassCenter = string.Empty;
                System.Text.StringBuilder strreport = new System.Text.StringBuilder();
                DataTable dtTable1 = new DataTable("FltPlan_btnPrintLoadPlan_dtTable1");
                DataTable dtTable2 = new DataTable();
                DataTable dtTailNo = new DataTable();
                DataTable dtSummary = new DataTable();
                string FltNo = string.Empty;
                string fltDate, FlightDate;


                //dsFLPData = objSQL.SelectRecords("uspSendFlightLoadPlan");
                dsFLPData = await _readWriteDao.SelectRecords("uspSendFlightLoadPlan");

                if (dsFLPData != null)
                {
                    if (dsFLPData.Tables.Count > 0)
                    {
                        if (dsFLPData.Tables[0] != null && dsFLPData.Tables[0].Rows.Count > 0)
                        {
                            if (dsFLPData.Tables[4].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsFLPData.Tables[4].Rows.Count; i++)
                                {
                                    FltNo = dsFLPData.Tables[4].Rows[i]["FlightNo"].ToString();
                                    fltDate = dsFLPData.Tables[4].Rows[i]["FlightDate"].ToString();

                                    dsFLPData.Tables[0].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                    dtTable1 = dsFLPData.Tables[0].DefaultView.ToTable();
                                    dsFLPData.Tables[1].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                    dtTable2 = dsFLPData.Tables[1].DefaultView.ToTable();
                                    dsFLPData.Tables[2].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                    dtTailNo = dsFLPData.Tables[2].DefaultView.ToTable();
                                    dsFLPData.Tables[3].DefaultView.RowFilter = "FlightNo = '" + FltNo + "'and FlightDate = '" + fltDate + "'";
                                    dtSummary = dsFLPData.Tables[3].DefaultView.ToTable();

                                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                                    string pathhtml = Directory.GetParent(path).Parent.FullName;
                                    StringReader htmlFile = new StringReader(File.ReadAllText(pathhtml + "/Reports/CargoLoadPlan.html"));
                                    string htmlContent = htmlFile.ReadToEnd().ToString();
                                    string logo = "";
                                    logo = pathhtml + "//Reports//Client_logo.png";
                                    string FlightSchDept;

                                    if (dtTable1 != null && dtTable1.Rows.Count > 0)
                                    {
                                        FlightSchDept = dtTable1.Rows[0]["SchDeptTime"].ToString().Split('(')[0];
                                    }
                                    else
                                    {
                                        FlightSchDept = "";
                                    }

                                    fltDate = fltDate.Split(' ')[0];

                                    htmlContent = htmlContent.Replace("@logo@", logo);
                                    if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                    {
                                        htmlContent = htmlContent.Replace("@Station@", dtTable2.Rows[0]["Origin"].ToString());

                                    }
                                    else
                                    {
                                        htmlContent = htmlContent.Replace("@Station@", "");

                                    }
                                    htmlContent = htmlContent.Replace("@FlightNo@", FltNo);
                                    if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                    {

                                        htmlContent = htmlContent.Replace("@Registration@", Convert.ToString(dtTailNo.Rows[0]["AirCraftType"]));
                                        htmlContent = htmlContent.Replace("@Planner@", Convert.ToString(dtTailNo.Rows[0]["PlannerStaff"]));
                                        htmlContent = htmlContent.Replace("@Version@", Convert.ToString(dtTailNo.Rows[0]["VersionNo"]));

                                    }
                                    else
                                    {
                                        htmlContent = htmlContent.Replace("@Registration@", "");
                                        htmlContent = htmlContent.Replace("@Planner@", "");
                                        htmlContent = htmlContent.Replace("@Version@", "");
                                    }

                                    if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                    {
                                        htmlContent = htmlContent.Replace("@Dest@", dtTable2.Rows[0]["Destination"].ToString());
                                    }
                                    else
                                    {
                                        htmlContent = htmlContent.Replace("@Dest@", "");
                                    }
                                    htmlContent = htmlContent.Replace("@DeptTime@", Convert.ToString(FlightSchDept.Substring(0, 5)));
                                    htmlContent = htmlContent.Replace("@Date@", Convert.ToDateTime(fltDate).ToString("dd/MM/yyyy"));

                                    StringBuilder sbULD = new StringBuilder(string.Empty);
                                    DataTable dttable3 = new DataTable();
                                    dttable3.Columns.Add("ORDER");
                                    dttable3.Columns.Add("AWBNo");
                                    dttable3.Columns.Add("PCS");
                                    dttable3.Columns.Add("Wt");
                                    dttable3.Columns.Add("Volume");
                                    dttable3.Columns.Add("ORG");
                                    dttable3.Columns.Add("DEST");
                                    dttable3.Columns.Add("CommodityDesc");
                                    dttable3.Columns.Add("SHCCode");
                                    dttable3.Columns.Add("InbFligtNo");
                                    dttable3.Columns.Add("Priority");
                                    dttable3.Columns.Add("Agent");
                                    dttable3.Columns.Add("Remark");
                                    dttable3.Columns.Add("ProductType");
                                    dttable3.Columns.Add("AllotmentCode");


                                    if (dtTable1 != null && dtTable1.Rows.Count > 0)
                                    {
                                        for (int Count = 0; Count < dtTable1.Rows.Count; Count++)
                                        {
                                            sbULD.Append("<table style='width:100%;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                            sbULD.Append("<tr style='page-break-inside: avoid'>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%;' colspan=3><b> ULD# : " + Convert.ToString(dtTable1.Rows[Count]["Uldno"]) + "</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%' colspan=3><b> ULD Type : " + Convert.ToString(dtTable1.Rows[Count]["ULDType"]) + "</b></td>");
                                            //sbULD.Append("<td class='tableBorder' style='font-size:13px;' colspan='9'></td>");
                                            //sbULD.Append("<td class='tableBorder' style='font-size:13px;width:10%' colspan=2><b> Priority : " + Convert.ToString(dtTable1.Rows[Count]["LoadingPriority"]) + "</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width:70%' colspan=9><b> Remarks : " + Convert.ToString(dtTable1.Rows[Count]["LoadingRemarks"]) + "</b></td>");
                                            sbULD.Append("</tr>");

                                            sbULD.Append("<tr style='page-break-inside: avoid'>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>ORD</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AgentDBA</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AWBNo</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>PCS</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Wt</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Volume</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>ORG</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>DEST</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>SHC</b></td>");
                                            //sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>I/B Flight</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;' ><b>CommodityDesc</b></td>");
                                            //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Priority</b></td>");
                                            //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Product Type</b></td>");
                                            //sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>AgentDBA</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;'><b>Remark</b></td>");
                                            sbULD.Append("</tr>");
                                            StringBuilder sbBulkload = new StringBuilder(string.Empty);

                                            if (dtTable2 != null && dtTable2.Rows.Count > 0)
                                            {
                                                DataTable dtTable4 = new DataTable();

                                                for (int j = 0; j < dtTable2.Rows.Count; j++)
                                                {
                                                    DataRow[] DtRow = new DataRow[0];
                                                    Array.Resize(ref DtRow, DtRow.Length + 1);

                                                    if (dtTable2.Rows[j]["ULD"].ToString() == dtTable1.Rows[Count]["Uldno"].ToString())
                                                    {
                                                        DtRow[DtRow.Length - 1] = dttable3.NewRow();
                                                        DtRow[DtRow.Length - 1]["ORDER"] = dtTable2.Rows[j]["ORDER"].ToString();
                                                        DtRow[DtRow.Length - 1]["Agent"] = Convert.ToString(dtTable2.Rows[j]["Agent"]);
                                                        DtRow[DtRow.Length - 1]["AWBNo"] = dtTable2.Rows[j]["awbnumber"].ToString();
                                                        DtRow[DtRow.Length - 1]["PCS"] = dtTable2.Rows[j]["builtpcs"].ToString();
                                                        DtRow[DtRow.Length - 1]["Wt"] = dtTable2.Rows[j]["builtwgt"].ToString();
                                                        DtRow[DtRow.Length - 1]["Volume"] = dtTable2.Rows[j]["PlanVolume"].ToString();
                                                        DtRow[DtRow.Length - 1]["ORG"] = dtTable2.Rows[j]["Origin"].ToString();
                                                        DtRow[DtRow.Length - 1]["DEST"] = dtTable2.Rows[j]["Destination"].ToString();
                                                        DtRow[DtRow.Length - 1]["CommodityDesc"] = dtTable2.Rows[j]["description"].ToString();
                                                        DtRow[DtRow.Length - 1]["SHCCode"] = Convert.ToString(dtTable2.Rows[j]["SHCCode"]);
                                                        DtRow[DtRow.Length - 1]["InbFligtNo"] = Convert.ToString(dtTable2.Rows[j]["InbFligtNo"]);
                                                        // DtRow[DtRow.Length - 1]["Priority"] = Convert.ToString(dtTable2.Rows[j]["BookingPriority"]);
                                                        DtRow[DtRow.Length - 1]["Remark"] = Convert.ToString(dtTable2.Rows[j]["BookingRemark"]);
                                                        // DtRow[DtRow.Length - 1]["ProductType"] = Convert.ToString(dtTable2.Rows[j]["ProductType"]);
                                                        //DtRow[DtRow.Length - 1]["Agent"] = Convert.ToString(dtTable2.Rows[j]["Agent"]);
                                                        DtRow[DtRow.Length - 1]["AllotmentCode"] = Convert.ToString(dtTable2.Rows[j]["AllotmentCode"]);

                                                        dttable3.Rows.Add(DtRow[DtRow.Length - 1]);
                                                    }
                                                }

                                                if (dttable3 != null && dttable3.Rows.Count > 0)
                                                {
                                                    for (int j = 0; j < dttable3.Rows.Count; j++)
                                                    {
                                                        sbULD.Append("<tbody style='display:table-row-group;'>");
                                                        sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: center;width:3%'>" + Convert.ToString(dttable3.Rows[j]["ORDER"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left;width:16%'>" + Convert.ToString(dttable3.Rows[j]["Agent"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left;width:13%'>" + Convert.ToString(dttable3.Rows[j]["AWBNo"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width:5%'>" + Convert.ToString(dttable3.Rows[j]["PCS"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width:7%'>" + Convert.ToString(dttable3.Rows[j]["Wt"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width:6%'>" + Convert.ToString(dttable3.Rows[j]["Volume"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left;width:4%'>" + Convert.ToString(dttable3.Rows[j]["ORG"]) + "</td>");
                                                        sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: left;width:4%'>" + Convert.ToString(dttable3.Rows[j]["DEST"]) + "</td>");
                                                        sbULD.Append("<td  class='tableBorder' style='font-size:12px; text-align: right;border-top:0;word-wrap:break-word ;width:3%' >" + Convert.ToString(dttable3.Rows[j]["SHCCode"]) + "</td>");
                                                        //sbULD.Append("<td  class='tableBorder' style='font-size:12px; text-align: right;border-top:0;word-wrap:break-word ;width:6%' >" + Convert.ToString(dttable3.Rows[j]["InbFligtNo"]) + "</td>");
                                                        sbULD.Append("<td  class='tableBorder' style='font-size:12px; text-align: left;border-top:0;word-wrap:break-word ;width:32%' >" + Convert.ToString(dttable3.Rows[j]["CommodityDesc"]) + "</td>");
                                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;width:4%'>" + Convert.ToString(dttable3.Rows[j]["Priority"]) + "</td>");
                                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;width:9%'>" + Convert.ToString(dttable3.Rows[j]["ProductType"]) + "</td>");
                                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;width:9%'>" + Convert.ToString(dttable3.Rows[j]["Agent"]) + "</td>");
                                                        //sbULD.Append("<td class='tableBorder' style='font-size:12px;width:13%'>" + Convert.ToString(dttable3.Rows[j]["Remark"]) + "</td>");
                                                        if (Convert.ToString(dttable3.Rows[j]["AllotmentCode"]) == "")
                                                        {
                                                            sbULD.Append("<td class='tableBorder' style='font-size:12px;width:28%;vertical-align: middle;'>" + Convert.ToString(dttable3.Rows[j]["Remark"]) + "</td>");
                                                        }
                                                        else
                                                        {
                                                            if (Convert.ToString(dttable3.Rows[j]["Remark"]) == "")
                                                            {
                                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;width:28%'>" + "HARDBLOCK" + "</td>");
                                                            }
                                                            else
                                                            {
                                                                sbULD.Append("<td class='tableBorder' style='font-size:12px;word-wrap:break-word ;width:28%;vertical-align: middle;' >" + "HARDBLOCK- " + Convert.ToString(dttable3.Rows[j]["Remark"]) + "</td>");
                                                            }

                                                        }
                                                        sbULD.Append("</tr>");
                                                    }
                                                    //add total row per ULD/Bulk
                                                    sbULD.Append(" <tr style='page-break-inside: avoid'>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px;width: 110px'><b>Total</b></td>");
                                                    sbULD.Append("<td class='tableBorder'></td>");
                                                    sbULD.Append("<td class='tableBorder'></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["BookedPcs"]) + "</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["BookedWgt"]) + "</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px'><b>" + Convert.ToString(dtTable1.Rows[Count]["PlanVolume"]) + "</b></td>");
                                                    sbULD.Append("<td class='tableBorder' style='font-size:13px;' colspan='9'></td>");
                                                    sbULD.Append("</tr>");
                                                    sbULD.Append("</tbody>");
                                                    //end total
                                                    sbULD.Append("</table>");
                                                    // sbULD.Append("</table>");
                                                    sbULD.Append("</br>");
                                                    //delete previous row 

                                                    for (int m = dttable3.Rows.Count - 1; m >= 0; m--)
                                                    {
                                                        DataRow dr = dttable3.Rows[m];
                                                        dr.Delete();
                                                        //if (dttable3.Columns.Contains("SeqNo") == false)
                                                        //{ dttable3.Columns.Add("SeqNo"); }

                                                        if (dttable3.Columns.Contains("AWBNo") == false)
                                                        { dttable3.Columns.Add("AWBNo"); }

                                                        if (dttable3.Columns.Contains("PCS") == false)
                                                        { dttable3.Columns.Add("PCS"); }

                                                        if (dttable3.Columns.Contains("Wt") == false)
                                                        { dttable3.Columns.Add("Wt"); }

                                                        if (dttable3.Columns.Contains("Volume") == false)
                                                        { dttable3.Columns.Add("Volume"); }

                                                        if (dttable3.Columns.Contains("ORG") == false)
                                                        { dttable3.Columns.Add("ORG"); }

                                                        if (dttable3.Columns.Contains("DEST") == false)
                                                        { dttable3.Columns.Add("DEST"); }

                                                        if (dttable3.Columns.Contains("CommodityDesc") == false)
                                                        { dttable3.Columns.Add("CommodityDesc"); }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sbBulkload.Append(" <tr class='tableBorder' style='page-break-inside: avoid'>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'>-</td>");
                                                sbBulkload.Append("<td class='tableBorder'></td>");
                                                sbBulkload.Append("</tr>");
                                                htmlContent = htmlContent.Replace("@BULKLOADDETAILS@", Convert.ToString(sbBulkload.ToString()));
                                            }
                                        }
                                        //summary Table.
                                        if (dtSummary != null && dtSummary.Rows.Count > 0)
                                        {
                                            sbULD.Append("<table style='width:50%;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                            sbULD.Append("<tr style='page-break-inside: avoid'>");
                                            sbULD.Append("<td rowspan='2' class='tableBorder' style='font-size:13px;width: 100px;'><b>Total Planned</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Pieces</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Weight</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>Volume</b></td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:13px;width: 90px;'><b>ULD</b></td>");
                                            sbULD.Append("</tr>");
                                            sbULD.Append("<tr style='page-break-inside: avoid'>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalPcs"]) + "</td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalWgt"]) + "</td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalVolume"]) + "</td>");
                                            sbULD.Append("<td class='tableBorder' style='font-size:12px; text-align: right;width: 90px;'>" + Convert.ToString(dtSummary.Rows[0]["TotalULD"]) + "</td>");
                                            sbULD.Append("</tr>");
                                            sbULD.Append("</table>");
                                            sbULD.Append("</br>");
                                        }
                                        //
                                    }
                                    else
                                    {

                                        sbULD.Append(" <tr class='tableBorder' style='page-break-inside: avoid'>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'>-</td>");
                                        sbULD.Append("<td class='tableBorder'></td>");
                                        sbULD.Append("</tr>");
                                        htmlContent = htmlContent.Replace("@ULDLOADDETAILS@", Convert.ToString(sbULD.ToString()));
                                    }

                                    sbULD.Append("<table style='width:49%; float:right;margin-top: -64px;' cellpadding='3' cellspacing='3' class='tableBorder' >");
                                    sbULD.Append("<tr style='page-break-inside: avoid'>");
                                    sbULD.Append("<th class='tableBorder' style='font-size:13px;'><b>Remark</b></th>");
                                    sbULD.Append("</tr>");
                                    sbULD.Append("<tr style='page-break-inside: avoid'>");
                                    if (dtTailNo != null && dtTailNo.Rows.Count > 0)
                                    {
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'>" + Convert.ToString(dtTailNo.Rows[0]["FlightRemark"]) + "</td>");

                                    }
                                    else
                                    {
                                        sbULD.Append("<td class='tableBorder' style='font-size:12px;'></td>");

                                    }
                                    sbULD.Append("</tr>");
                                    sbULD.Append("</table>");
                                    sbULD.Append("</br>");

                                    htmlContent = htmlContent.Replace("@BULKLOADDETAILS@", Convert.ToString(sbULD.ToString()));

                                    //convert to pdf
                                    HtmlToPdfConverter htmlToPdfConverter = new HtmlToPdfConverter();
                                    var margins = new NReco.PdfGenerator.PageMargins();
                                    margins.Bottom = 10;
                                    margins.Top = 10;
                                    margins.Left = 6;
                                    margins.Right = 5;
                                    htmlToPdfConverter.Margins = margins;
                                    htmlToPdfConverter.PageFooterHtml = $@"<div style='float:right;font-family: verdana;font-size: large;margin-right: 5%;margin-bottom: 13%'>  page <span class=""page""></span> of <span class=""topage""></span></div>";

                                    var pdfBytes = htmlToPdfConverter.GeneratePdf(htmlContent);
                                    var byteArray = Encoding.ASCII.GetBytes(htmlContent);
                                    MemoryStream msExcel = new MemoryStream(byteArray);
                                    MemoryStream ms = new MemoryStream(pdfBytes);

                                    //GenericFunction genericFunction = new GenericFunction();

                                    FlightDate = dsFLPData.Tables[4].Rows[i]["FLTDate"].ToString();

                                    string DocfileName = FltNo + "-" + FlightDate;
                                    // String FileNameForExcel = "Flight Load Plan_" + FltNo + "-" + FlightDate;

                                    string sFileUrl = UploadToBlob(ms, DocfileName + ".pdf", "AutoFLP");
                                    string FileExcelURL = UploadToBlob(msExcel, DocfileName + ".xls", "AutoFLP");


                                    DateTime TimeStamp = DateTime.Now;
                                    string sMailSubject = "Flight Load Plan for " + FltNo + "/" + FlightDate;
                                    System.String sMailBody = "\r\nHello, \r\n\tPlease find attached Flight Load Plan: " + " Flight No: " + FltNo + ", Flight Date : " + FlightDate + "\r\n\r\n Thanks\r\n\r\n";
                                    DataSet dsconfiguration = await _genericFunction.GetSitaAddressandMessageVersion("VJ", "AutoFLP", "AIR", "", "", "", string.Empty);

                                    string EmailID = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                                    await DumpInterfaceInformation(sMailSubject, sMailBody, TimeStamp, "AutoFLP", "", true, ConfigCache.Get("msgService_EmailId"), EmailID, ms, ".pdf", sFileUrl, "0", "Outbox", FileExcelURL, msExcel, DocfileName);

                                    //null Objects
                                    dtTable1 = null;
                                    dtTable2 = null;
                                    dttable3 = null;
                                    dtTailNo = null;
                                    dtSummary = null;
                                    sbULD = null;
                                    htmlToPdfConverter = null;
                                    ms = null;
                                    sFileUrl = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on SendFlightLoadPlan");
            }
            //finally
            //{
            //    objSQL = null;
            //    dsFLPData = null;
            //    GC.Collect();
            //}
        }



        public async Task UpdateULDStock()
        {
            // clsLog.WriteLogAzure("UpdateULDStock()");
            _logger.LogInformation("UpdateULDStock()");

            try
            {
                Task<IReadOnlyList<ServiceBusReceivedMessage>> myTask = ReceiveMessagesAsync();

                // clsLog.WriteLogAzure("Start Foreach loop");
                _logger.LogInformation("Start Foreach loop");
                foreach (ServiceBusReceivedMessage receivedMessage in myTask.Result)
                {
                    //GenericFunction generic = new GenericFunction();

                    string UpdatedBy = "Service Bus";
                    int rowIndex = 0;
                    string columnName = string.Empty, status = string.Empty, uldIdentifier = string.Empty;
                    string json = string.Empty;
                    bool hasColumnName = false;

                    DataTable dtULDDetails = new DataTable();
                    dtULDDetails.Columns.Add("id", typeof(string));
                    dtULDDetails.Columns.Add("uldIdentifier", typeof(string));
                    dtULDDetails.Columns.Add("uldType", typeof(string));
                    dtULDDetails.Columns.Add("eventDateTimeUtc", typeof(string));
                    dtULDDetails.Columns.Add("event", typeof(string));
                    dtULDDetails.Columns.Add("conditionShortCode", typeof(string));
                    dtULDDetails.Columns.Add("portShortCode", typeof(string));
                    dtULDDetails.Columns.Add("locationShortName", typeof(string));
                    dtULDDetails.Columns.Add("mainUserName", typeof(string));
                    dtULDDetails.Columns.Add("mainUserShortCode", typeof(string));
                    dtULDDetails.Columns.Add("subUserName", typeof(string));

                    json = receivedMessage.Body.ToString();
                    // clsLog.WriteLogAzure("Json :" + json);
                    _logger.LogInformation("Json : {json}", json);

                    if (json.Trim() != string.Empty)
                    {
                        json = "[" + json.Replace("}", "},").TrimEnd(',') + "]";
                        try
                        {
                            using (var reader = new JsonTextReader(new StringReader(json)))
                            {
                                while (reader.Read())
                                {
                                    if (reader.TokenType == JsonToken.EndArray)
                                        break;
                                    if (reader.TokenType == JsonToken.StartObject)
                                    {
                                        DataRow drRateLine = dtULDDetails.NewRow();
                                        dtULDDetails.Rows.Add(drRateLine);
                                    }
                                    if (reader.TokenType == JsonToken.EndObject)
                                        rowIndex++;
                                    if (reader.TokenType == JsonToken.PropertyName)
                                    {
                                        columnName = reader.Value.ToString();
                                        if (dtULDDetails.Columns.Contains(columnName))
                                        {
                                            hasColumnName = true;
                                        }
                                    }
                                    else if (hasColumnName)
                                    {
                                        dtULDDetails.Rows[rowIndex][columnName] = reader.Value == null ? "" : reader.Value;
                                        hasColumnName = false;
                                    }
                                }

                                uldIdentifier = dtULDDetails.Rows[0]["uldIdentifier"].ToString();

                                if (!string.IsNullOrEmpty(uldIdentifier))
                                {
                                    //SQLServer sqlServer = new SQLServer();
                                    //sqlServer.SelectRecords("Messaging.uspUpdateULDDetailsFromJson", sqlParameters);

                                    SqlParameter[] sqlParameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@tblULDDetailJson", dtULDDetails),
                                        new SqlParameter("@UpdatedBy", UpdatedBy)
                                    };
                                    await _readWriteDao.SelectRecords("Messaging.uspUpdateULDDetailsFromJson", sqlParameters);

                                    status = "Processed";
                                    dtULDDetails = null;
                                }
                                else
                                {
                                    status = "Failed";
                                    uldIdentifier = "Incomplete Info";
                                    // clsLog.WriteLogAzure("Failed to update ULD Info : Incomplite Info");
                                    _logger.LogWarning("Failed to update ULD Info : Incomplite Info");
                                    dtULDDetails = null;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure("Failed to update ULD Info " + ex.Message);
                            _logger.LogError("Failed to update ULD Info " + ex.Message);
                            status = "Failed";
                        }
                        await _genericFunction.SaveIncomingMessageInDatabase(uldIdentifier, json, "Service Bus", "", DateTime.UtcNow, DateTime.UtcNow, "ULD Info", status, "AZSRV BUS");
                    }
                    else
                    {
                        // clsLog.WriteLogAzure("No Json Data Found ");
                        _logger.LogWarning("No Json Data Found ");
                    }
                }
                // clsLog.WriteLogAzure("\r\n");
                _logger.LogInformation("\r\n");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on UpdateULDStock");
            }
        }

        static async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync()
        {
            string connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"].ToString();
            string queueName = ConfigurationManager.AppSettings["QueueName"].ToString();
            string Receive_Mode = ConfigurationManager.AppSettings["IsPeekLock"].ToString();
            try
            {
                // clsLog.WriteLogAzure("connectionString: " + connectionString);
                // clsLog.WriteLogAzure("queueName: " + queueName);

                // clsLog.WriteLogAzure("Initializing connection");
                _staticLogger?.LogInformation("connectionString: {0}", connectionString);
                _staticLogger?.LogInformation("queueName: {0}", queueName);

                _staticLogger?.LogInformation("Initializing connection");
                var client = new ServiceBusClient(connectionString);

                #region Send
                // create the sender
                //Console.WriteLine("\r\n11");
                //ServiceBusSender sender = client.CreateSender(queueName);
                ////create a message that we can send.UTF - 8 encoding is used when providing a string.
                //Console.ReadLine();
                //Console.WriteLine("\r\n2");
                //ServiceBusMessage message = new ServiceBusMessage("Hello world!");
                //Console.ReadLine();
                ////// send the message
                //Console.WriteLine("\r\n33");
                //Console.ReadLine();
                //await sender.SendMessageAsync(message);
                //Console.ReadLine();
                //Console.WriteLine("\r\n44");
                #endregion Send

                #region Receive
                // create a receiver that we can use to receive the message
                // clsLog.WriteLogAzure("Creating receiver");
                _staticLogger?.LogInformation("Creating receiver");
                ServiceBusReceiverOptions sbro = new ServiceBusReceiverOptions();
                sbro.ReceiveMode = Receive_Mode.ToUpper() == "TRUE" ? ServiceBusReceiveMode.PeekLock : ServiceBusReceiveMode.ReceiveAndDelete;
                //sbro.ReceiveMode = ServiceBusReceiveMode.PeekLock;

                ServiceBusReceiver receiver = client.CreateReceiver(queueName, "", sbro);
                // clsLog.WriteLogAzure("Receiver created");

                // clsLog.WriteLogAzure("receiver.PrefetchCount: " + Convert.ToString(receiver.PrefetchCount));
                // clsLog.WriteLogAzure("receiver.IsClosed: " + Convert.ToString(receiver.IsClosed));
                // clsLog.WriteLogAzure("receiver.ReceiveMode: " + Convert.ToString(receiver.ReceiveMode));
                // clsLog.WriteLogAzure("receiver.EntityPath: " + Convert.ToString(receiver.EntityPath));
                // clsLog.WriteLogAzure("receiver.FullyQualifiedNamespace: " + Convert.ToString(receiver.FullyQualifiedNamespace));

                // // the received message is a different type as it contains some service set properties
                // clsLog.WriteLogAzure("Receive message");
                _staticLogger?.LogInformation("Receiver created");

                _staticLogger?.LogInformation("receiver.PrefetchCount: {0}", (receiver.PrefetchCount));
                _staticLogger?.LogInformation("receiver.IsClosed: {0}", (receiver.IsClosed));
                _staticLogger?.LogInformation("receiver.ReceiveMode: {0}", (receiver.ReceiveMode));
                _staticLogger?.LogInformation("receiver.EntityPath: {0}", (receiver.EntityPath));
                _staticLogger?.LogInformation("receiver.FullyQualifiedNamespace: {0}", (receiver.FullyQualifiedNamespace));

                // the received message is a different type as it contains some service set properties
                _staticLogger?.LogInformation("Receive message");
                IReadOnlyList<ServiceBusReceivedMessage> receivedMessages = await receiver.ReceiveMessagesAsync(10, TimeSpan.FromSeconds(10));
                // clsLog.WriteLogAzure($"Received {receivedMessages.Count} from the queue {queueName} ");
                _staticLogger?.LogInformation($"Received {receivedMessages.Count} from the queue {queueName} ");

                #endregion Receive

                return receivedMessages;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger?.LogError(ex, "Error on ReceiveMessagesAsync");
                return null;
            }
        }
        private async Task AutoMessages()
        {
            try
            {
                //Task Task1 = AsyncAutoMessages();
                //Task TaskAutoXFSU = AutoXFSUMessages();
                //await Task.WhenAll(Task1, TaskAutoXFSU);

                await AsyncAutoMessages();
                await AutoXFSUMessages();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task AsyncAutoMessages()
        {
            //return Task.Run(async () => { await AutoGenerateMessages(); });
            await AutoGenerateMessages();
        }


        private async Task AutoXFSUMessages()
        {
            //return Task.Run(async () => { await AutoSendXFSUMessages(); });
            await AutoSendXFSUMessages();
        }

        /// <summary>
        /// Call methods to read messages from FTP and SFTP
        /// </summary>
        public async Task FTPListener()
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                bool stopReadFromALLFTP = false, stopReadFromFTP = false, stopReadFromSFTP = false;

                //string stopCallingThreadOrMethod = genericFunction.GetConfigurationValues("SkipUnnecessaryThreadOrMethod");
                string stopCallingThreadOrMethod = ConfigCache.Get("SkipUnnecessaryThreadOrMethod");

                if (!string.IsNullOrWhiteSpace(stopCallingThreadOrMethod))
                {
                    var stopMethods = new HashSet<string>(
                        stopCallingThreadOrMethod.Split(','),
                        StringComparer.OrdinalIgnoreCase
                    );

                    stopReadFromALLFTP = stopMethods.Contains("ReadFromALLFTP");
                    stopReadFromFTP = stopMethods.Contains("ReadFromFTP");
                    stopReadFromSFTP = stopMethods.Contains("ReadFromSFTP");
                }

                if (!stopReadFromALLFTP)
                    await ReadFromALLFTP();

                if (!stopReadFromFTP)
                    await ReadFromFTP();

                if (!stopReadFromSFTP)
                    await ReadFromSFTP();


                //SSIMFTPUpload();
                //if (_genericFunction.GetConfigurationValues("SISFileAutomation").Equals("True", StringComparison.OrdinalIgnoreCase))
                if (ConfigCache.Get("SISFileAutomation").Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    //FTP _ftp = new FTP();
                    _ftp.SISFilesReadProcess();
                    await UploadSISReceivableFileonSFTP();
                }

                string updateRapidFile = ConfigCache.Get("updateRapidFile");
                if (updateRapidFile.ToUpper() == "TRUE")
                {
                    //RapidInterfaceMethods RapidObj = new RapidInterfaceMethods();
                    DataTable dt = new DataTable();
                    await _rapidInterfaceMethods.SaveRapidStatus("SendRapidAleart");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on FTPListener");
            }
        }

        /// <summary>
        /// DB Log Snapshot For Performance Monitor
        /// </summary>
        public async Task LOGDBSnapshot()
        {
            //SQLServer db = new SQLServer();
            //GenericFunction genericFunction = new GenericFunction();
            try
            {
                // clsLog.WriteLogAzure("DB Log Snapshot For Performance Monitor ");
                _logger.LogInformation("DB Log Snapshot For Performance Monitor");
                #region LOGDBSnapshot
                try
                {
                    bool f_CreateDBLogSnapshot = Convert.ToBoolean(ConfigCache.Get("CreateDBLogSnapshot"));
                    if (f_CreateDBLogSnapshot)
                    {
                        //db.ExecuteProcedure("sp_WHO3Logging")

                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("sp_WHO3Logging");
                        if (dbRes)
                        {
                            // clsLog.WriteLogAzure("DB Log Snapshot created for Performance Monitor @" + DateTime.Now.ToString());
                            _logger.LogInformation($"DB Log Snapshot created for Performance Monitor @{DateTime.Now.ToString()}");
                        }
                    }

                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on LOGODBSnapshot");
                }
                //finally
                //{
                //    db = null;
                //    GC.Collect();
                //}
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on LOGDBSnapshot");
            }
        }

        public async Task ReceiveMail_POP(string sServer, string sUserName, string sPassword, bool bSSLConnection)
        {
            var popClient = new Pop3.Pop3Client();
            try
            {
                string EmailSubject = string.Empty;
                try
                {
                    if (popClient.Connected)
                        popClient.Disconnect();
                    if (!popClient.Connected)
                        popClient.Connect(sServer, sUserName, sPassword);
                }

                catch (MailServerException ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, "Error on ReceiveMail_POP");
                    return;
                }

                List<Pop3Message> messages = popClient.List();
                foreach (Pop3Message message in messages)
                {
                    try
                    {
                        string MessageType = string.Empty;
                        popClient.Retrieve(message);
                        if (message.Subject == null)
                            EmailSubject = string.Empty;
                        else if ((!message.Subject.ToUpper().Contains("RE")))
                        {
                            EmailSubject = string.Empty;
                            EmailSubject = message.Subject;
                        }
                        EmailSubject = message.Subject;
                        string EmailDate = message.Date;
                        string ReceivedString = string.Empty;
                        string EmailFrom = string.Empty;
                        string EmailTo = string.Empty;
                        try
                        {
                            EmailFrom = message.From;
                            if (EmailFrom.Contains("<"))
                            {
                                int indexOfLessThan = EmailFrom.IndexOf('<');
                                int indexOfGreaterThan = EmailFrom.IndexOf('>');
                                string emailfromId = EmailFrom.Substring(indexOfLessThan + 1, ((indexOfGreaterThan - 1) - (indexOfLessThan)));
                                EmailFrom = emailfromId;
                            }
                            EmailTo = message.To;
                            if (EmailTo.Contains("<"))
                            {
                                int indexOfLessThan = EmailTo.IndexOf('<');
                                int indexOfGreaterThan = EmailTo.IndexOf('>');
                                string emailToId = EmailTo.Substring(indexOfLessThan + 1, ((indexOfGreaterThan - 1) - (indexOfLessThan)));
                                EmailTo = emailToId;
                            }

                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ReceiveMail_POP");
                        }
                        //Read Meassage Data and store in the string
                        try
                        {
                            if (message.BodyData != null && message.BodyData.Length >= 1)
                            {
                                ReceivedString = Encoding.UTF8.GetString(message.BodyData);
                                ReceivedString = DecodeQuotedPrintables(ReceivedString);
                            }
                            else
                            {
                                // clsLog.WriteLogAzure("Mail body is blank.Mail came from " + EmailFrom);
                                _logger.LogInformation("Mail body is blank.Mail came from {0}", EmailFrom);

                                ReceivedString = "Mail body is blank";
                                popClient.Delete(message);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, "Error on ReceiveMail_POP");
                            ReceivedString = "Message not in Plain Text format.";
                            popClient.Delete(message);
                            continue;
                        }
                        //if (ReceivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
                        //{
                        //    popClient.Delete(message);
                        //    continue;
                        //}
                        //ReceivedString = Encoding.UTF8.GetString(message.BodyData);


                        string FormatName = string.Empty;
                        if (ReceivedString.Contains("This is a multi-part message in MIME format."))
                        {

                            ReceivedString = GetMessageBodyData(ReceivedString);

                            if ((ReceivedString.Contains("FHL/")) || (ReceivedString.Contains("FWB/")) ||
                                 (ReceivedString.Contains("FFM/")) || (ReceivedString.Contains("FFR/")) ||
                                 (ReceivedString.Contains("FBL/")) || (ReceivedString.Contains("FSB")) ||
                                 (ReceivedString.Contains("FSU/")) || (ReceivedString.Contains("FWR")) ||
                                 (ReceivedString.Contains("UCM")) || (ReceivedString.Contains("FBR/")) ||
                                 (ReceivedString.Contains("FNA/")) || (ReceivedString.Contains("FMB/")) ||
                                 (ReceivedString.Contains("ASM")) || (ReceivedString.Contains("MVT")) || (ReceivedString.Contains("CPM")) || (ReceivedString.Contains("FSN"))

                                 || (ReceivedString.Contains("FMA")))
                            {
                                //if (ReceivedString.StartsWith("FFR/"))
                                //    FormatName = "FFR";
                                //else if (ReceivedString.Contains("FSN"))
                                //    FormatName = "FSN";
                                //else if (ReceivedString.Contains("CPM"))
                                //    FormatName = "CPM";
                                //else if (ReceivedString.Contains("ASM"))
                                //    FormatName = "ASM";
                                //else if (ReceivedString.Contains("MVT"))
                                //    FormatName = "MVT";
                                //else if (ReceivedString.Contains("FWB/"))
                                //    FormatName = "FWB";
                                //else if (ReceivedString.Contains("FFM/"))
                                //    FormatName = "FFM";
                                //else if (ReceivedString.Contains("FAD/"))
                                //    FormatName = "FAD";
                                //else if (ReceivedString.Contains("FHL/"))
                                //    FormatName = "FHL";
                                //else if (ReceivedString.Contains("FSU/"))
                                //    FormatName = "FSU";
                                //else if (ReceivedString.Contains("UCM"))
                                //    FormatName = "UCM";
                                //else if (ReceivedString.Contains("FBL/"))
                                //    FormatName = "FBL";
                                //else if (ReceivedString.Contains("SCM"))
                                //    FormatName = "SCM";
                                //else if (ReceivedString.Contains("FSB"))
                                //    FormatName = "FSB";
                                //else if (ReceivedString.Contains("FWR/"))
                                //    FormatName = "FWR";
                                //else if (ReceivedString.Contains("FBR/"))
                                //    FormatName = "FBR/";
                                //else if (ReceivedString.StartsWith("FNA"))
                                //    FormatName = "FNA";
                                //else if (ReceivedString.StartsWith("FMA"))
                                //    FormatName = "FMA";
                                //else if (ReceivedString.Contains("FMB"))
                                //    FormatName = "FMB";


                                if (ReceivedString.StartsWith("FFR/"))
                                    FormatName = "FFR";
                                else if (ReceivedString.StartsWith("FSN"))
                                    FormatName = "FSN";
                                else if (ReceivedString.StartsWith("CPM"))
                                    FormatName = "CPM";
                                else if (ReceivedString.StartsWith("ASM"))
                                    FormatName = "ASM";
                                else if (ReceivedString.StartsWith("MVT"))
                                    FormatName = "MVT";
                                else if (ReceivedString.StartsWith("FWB/"))
                                    FormatName = "FWB";
                                else if (ReceivedString.StartsWith("FFM/"))
                                    FormatName = "FFM";
                                else if (ReceivedString.StartsWith("FAD/"))
                                    FormatName = "FAD";
                                else if (ReceivedString.StartsWith("FHL/"))
                                    FormatName = "FHL";
                                else if (ReceivedString.StartsWith("FSU/"))
                                    FormatName = "FSU";
                                else if (ReceivedString.StartsWith("UCM"))
                                    FormatName = "UCM";
                                else if (ReceivedString.StartsWith("FBL/"))
                                    FormatName = "FBL";
                                else if (ReceivedString.StartsWith("SCM"))
                                    FormatName = "SCM";
                                else if (ReceivedString.StartsWith("FSB"))
                                    FormatName = "FSB";
                                else if (ReceivedString.StartsWith("FWR/"))
                                    FormatName = "FWR";
                                else if (ReceivedString.StartsWith("FBR/"))
                                    FormatName = "FBR/";
                                else if (ReceivedString.StartsWith("FNA"))
                                    FormatName = "FNA";
                                else if (ReceivedString.StartsWith("FMA"))
                                    FormatName = "FMA";
                                else if (ReceivedString.StartsWith("FMB"))
                                    FormatName = "FMB";


                                ReceivedString = ReceivedString.Substring(ReceivedString.IndexOf(MessageType),
                                                                                ReceivedString.Length -
                                                                                ReceivedString.IndexOf(MessageType));

                                if (ReceivedString.Contains("------=_NextPart"))
                                {
                                    int indexOfEndPart = ReceivedString.IndexOf("------=_NextPart");
                                    ReceivedString = ReceivedString.Substring(0, indexOfEndPart);
                                }
                            }
                        }
                        else
                        {
                            ReceivedString = GetMessageBodyData(ReceivedString);
                            if ((ReceivedString.Contains("FHL/")) || (ReceivedString.Contains("FWB/")) ||
                                (ReceivedString.Contains("FFM/")) || (ReceivedString.Contains("FFR/")) ||
                                (ReceivedString.Contains("FBL/")) || (ReceivedString.Contains("FSB")) ||
                                (ReceivedString.Contains("FSU/")) || (ReceivedString.Contains("FWR")) ||
                                (ReceivedString.Contains("UCM")) || (ReceivedString.Contains("FBR/")) ||
                                (ReceivedString.Contains("FNA/")) || (ReceivedString.Contains("FMB/")) ||
                                (ReceivedString.Contains("ASM")) || (ReceivedString.Contains("MVT")) || (ReceivedString.Contains("CPM")) || (ReceivedString.Contains("FSN"))
                                || (ReceivedString.Contains("FMA"))
                                )
                            {
                                if (ReceivedString.StartsWith("FFR/"))
                                    FormatName = "FFR";
                                else if (ReceivedString.Contains("FSN"))
                                    FormatName = "FSN";
                                else if (ReceivedString.Contains("CPM"))
                                    FormatName = "CPM";
                                else if (ReceivedString.Contains("ASM"))
                                    FormatName = "ASM";
                                else if (ReceivedString.Contains("MVT"))
                                    FormatName = "MVT";
                                else if (ReceivedString.Contains("FWB/"))
                                    FormatName = "FWB";
                                else if (ReceivedString.Contains("FFM/"))
                                    FormatName = "FFM";
                                else if (ReceivedString.Contains("FAD/"))
                                    FormatName = "FAD";
                                else if (ReceivedString.Contains("FHL/"))
                                    FormatName = "FHL";
                                else if (ReceivedString.Contains("FSU/"))
                                    FormatName = "FSU";
                                else if (ReceivedString.Contains("UCM"))
                                    FormatName = "UCM";
                                else if (ReceivedString.Contains("FBL/"))
                                    FormatName = "FBL";
                                else if (ReceivedString.Contains("SCM"))
                                    FormatName = "SCM";
                                else if (ReceivedString.Contains("FSB"))
                                    FormatName = "FSB";
                                else if (ReceivedString.Contains("FWR/"))
                                    FormatName = "FWR";
                                else if (ReceivedString.Contains("FBR/"))
                                    FormatName = "FBR/";
                                else if (ReceivedString.StartsWith("FNA/"))
                                    FormatName = "FNA";
                                else if (ReceivedString.StartsWith("FMA"))
                                    FormatName = "FMA";
                                else if (ReceivedString.Contains("FMB"))
                                    FormatName = "FMB";

                                ReceivedString = ReceivedString.Substring(ReceivedString.IndexOf(FormatName),
                                                                            ReceivedString.Length -
                                                                            ReceivedString.IndexOf(FormatName));

                                int indexOfFFRTag = ReceivedString.IndexOf("FFR/");
                                int indexOfFWBTag = ReceivedString.IndexOf("FWB/");
                                int indexOfFFMTag = ReceivedString.IndexOf("FFM/");
                                int indexOfFHLTag = ReceivedString.IndexOf("FHL/");
                                int indexOfFSUTag = ReceivedString.IndexOf("FSU/");
                                int indexOfFNATag = ReceivedString.IndexOf("FNA/");
                                int indexOfFMATag = ReceivedString.IndexOf("FMA");

                                int indexOfFSRTag = ReceivedString.IndexOf("FSB/");
                                int indexOfFYTTag = ReceivedString.IndexOf("FYT/");
                                int indexOfFWRTag = ReceivedString.IndexOf("SSM");

                                if (indexOfFFMTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFFMTag);
                                else if (indexOfFWBTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFWBTag);
                                else if (indexOfFFRTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFFRTag);
                                else if (indexOfFSUTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFSUTag);
                                else if (indexOfFSRTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFSRTag);
                                else if (indexOfFYTTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFYTTag);
                                else if (indexOfFWRTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFWRTag);
                                else if (indexOfFHLTag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFHLTag);
                                else if (indexOfFNATag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFNATag);
                                else if (indexOfFMATag != -1)
                                    ReceivedString = ReceivedString.Substring(indexOfFMATag);

                                int indexOfEndPart = ReceivedString.Length;
                                ReceivedString = ReceivedString.Substring(0, indexOfEndPart);
                            }
                        }



                        if (await StoreIROPSEmail(EmailSubject, ReceivedString.ToUpper(), EmailFrom, EmailTo, DateTime.Now, DateTime.Now, FormatName == "" ? EmailSubject : FormatName, "ACTIVE", "EMAIL"))
                        {
                            // clsLog.WriteLogAzure("Email Saved");
                            _logger.LogInformation("Email Saved");
                        }
                        popClient.Delete(message);


                    }
                    catch (MailServerException ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, "Error on RecieveMail_Pop");
                    }
                }
                popClient.Disconnect();
            }
            catch (Exception ex)
            {
                popClient.Disconnect();
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on RecieveMail_Pop");
            }
        }

        public string GetMessageBodyData(string msg)
        {
            try
            {
                int indexOfbodyStart = 0;
                int indexOfbodyEnd = 0;
                if (((msg.Contains("<body") || (msg.Contains("<BODY"))) ||
                     ((msg.Contains("</body>")) || (msg.Contains("</BODY>")))))
                {
                    if (msg.Contains("<body"))
                    {
                        indexOfbodyStart = msg.IndexOf("<body");
                        indexOfbodyEnd = msg.LastIndexOf("</body>");
                    }
                    else
                    {

                        indexOfbodyStart = msg.IndexOf("<BODY");
                        if (msg.Contains("BODY>"))
                            indexOfbodyEnd = msg.LastIndexOf("BODY>");
                        else
                            indexOfbodyEnd = msg.LastIndexOf("</BODY>");
                    }

                    msg = msg.Substring(indexOfbodyStart, (indexOfbodyEnd - indexOfbodyStart) - 1);
                    msg = Regex.Replace(msg, @"<(.|\n)*?>", string.Empty);
                    msg = Regex.Replace(msg, @"\r\n\r\n", Environment.NewLine);
                    msg = Regex.Replace(msg, @"&nbsp;", string.Empty);
                    msg = Regex.Replace(msg, @"&amp;", string.Empty);
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on GetMessageBodyData");
            }
            return msg;
        }

        /// <summary>
        /// Download unread folowed by read email-messages
        /// </summary>
        //public void ReceiveMail_IMAP(string host, string username, string password, string inPortNo)
        //{
        //    try
        //    {
        //        if (host != "" && username != "" && password != "")
        //        {
        //            int incomingport = 143, UnreadMailCount = 0;
        //            incomingport = int.Parse(inPortNo == "" ? "143" : inPortNo);
        //            TcpIMAP imap = new TcpIMAP();
        //            imap.Connect(host, incomingport);
        //            imap.AuthenticateUser(username, password);
        //            imap.SelectInbox();
        //            UnreadMailCount = imap.UnreadMailCount();

        //            string[] arrUID = imap.GetUnreadMsgUids();
        //            StoreEmailToInbox(arrUID, imap);

        //            string[] arrReadUID = imap.GetReadMsgUids();
        //            StoreEmailToInbox(arrReadUID, imap);

        //        }
        //    }
        //    catch (MailServerException ep)
        //    {
        //        Console.WriteLine("Server Respond: {0}", ep.Message);
        //    }
        //    catch (System.Net.Sockets.SocketException ep)
        //    {
        //        Console.WriteLine("Socket Error: {0}", ep.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        /// <summary>
        /// Download email in sequence 
        /// </summary>
        public async Task ReceiveMail_IMAP(string host, string username, string password, string inPortNo)
        {
            try
            {
                if (host != "" && username != "" && password != "")
                {
                    int incomingport = 143;
                    incomingport = int.Parse(inPortNo == "" ? "143" : inPortNo);
                    
                    //TcpIMAP imap = new TcpIMAP();

                    _tcpIMAP.Connect(host, incomingport);
                    _tcpIMAP.AuthenticateUser(username, password);
                    _tcpIMAP.SelectInbox();
                    int totalMessageCount = 0;
                    string[] arrUnreadMsgUids = _tcpIMAP.GetUnreadMsgUids();
                    string[] arrReadMsgUids = _tcpIMAP.GetReadMsgUids();
                    string[] strArrAllUids;
                    int[] intArrAllUids;
                    DateTime[] dateTimeArrAllUids;
                    Dictionary<string, DateTime> uidsAndDate = new Dictionary<string, DateTime>();

                    if (arrReadMsgUids.Length > 0 && arrReadMsgUids[0] != string.Empty)
                    {
                        totalMessageCount = arrUnreadMsgUids.Length == 1 && arrUnreadMsgUids[0] == "" ? 0 : arrUnreadMsgUids.Length;
                        totalMessageCount += arrReadMsgUids.Length == 1 && arrReadMsgUids[0] == "" ? 0 : arrReadMsgUids.Length;

                        int index = 0;
                        strArrAllUids = new string[totalMessageCount];
                        intArrAllUids = new int[totalMessageCount];
                        dateTimeArrAllUids = new DateTime[totalMessageCount];
                        if (!(arrUnreadMsgUids.Length == 1 && arrUnreadMsgUids[0] == ""))
                        {
                            for (int i = 0; i < arrUnreadMsgUids.Length; i++)
                            {
                                strArrAllUids[i] = arrUnreadMsgUids[i];
                                index = i;
                            }
                            index++;
                        }
                        for (int i = 0; i < arrReadMsgUids.Length; i++)
                        {
                            strArrAllUids[index] = arrReadMsgUids[i];
                            index++;
                        }
                        for (int i = 0; i < strArrAllUids.Length; i++)
                        {
                            dateTimeArrAllUids[i] = Convert.ToDateTime(this._tcpIMAP.GetDateByUid(strArrAllUids[i]));
                            uidsAndDate.Add(strArrAllUids[i], dateTimeArrAllUids[i]);
                        }

                        var sortedUidsAndDate = uidsAndDate.OrderBy(x => x.Value).ThenBy(x => x.Key);


                        index = 0;
                        foreach (var item in sortedUidsAndDate)
                        {
                            strArrAllUids[index] = item.Key;
                            index++;
                        }
                        await StoreEmailToInbox(strArrAllUids, _tcpIMAP);
                    }
                    else
                    {
                        await StoreEmailToInbox(arrUnreadMsgUids, _tcpIMAP);
                    }
                }
            }
            catch (MailServerException ex)
            {
                // Console.WriteLine("Server Respond: {0}", ex.Message);
                _logger.LogError("Server Respond: {0}", ex.Message);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                // Console.WriteLine("Socket Error: {0}", ex.Message);
                _logger.LogError("Socket Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ReceiveMail_IMAP");
            }
        }

        private async Task StoreEmailToInbox(string[] arrUID, TcpIMAP imap)
        {
            try
            {

                for (int i = 0; i < arrUID.Length; i++)
                {
                    string _receivedString = string.Empty, MessageType = string.Empty;
                    try
                    {
                        if (string.IsNullOrEmpty(arrUID[i]))
                            continue;

                        _receivedString = imap.GetBodyByUid(arrUID[i]).ToString().ToUpper();
                        _receivedString = DecodeQuotedPrintables(_receivedString);

                        if (_receivedString.Contains("THIS MESSAGE WAS CREATED AUTOMATICALLY BY MAIL DELIVERY SOFTWARE"))
                        {
                            imap.Delete(arrUID[i]);
                            continue;
                        }


                        if ((_receivedString.Contains("FHL/")) || (_receivedString.Contains("FWB/")) ||
                                  (_receivedString.Contains("FFM/")) || (_receivedString.Contains("FFR/")) ||
                                  (_receivedString.Contains("FBL/")) || (_receivedString.Contains("FSB")) ||
                                  (_receivedString.Contains("FSU/")) || (_receivedString.Contains("FWR")) ||
                                  (_receivedString.Contains("UCM")) || (_receivedString.Contains("FBR/")) ||
                                  (_receivedString.Contains("FNA/")) || (_receivedString.Contains("FMB/")) ||
                                  (_receivedString.Contains("ASM")) || (_receivedString.Contains("MVT")) || (_receivedString.Contains("CPM")) || (_receivedString.Contains("FSN")) || (_receivedString.Contains("CARDIT"))

                                  || (_receivedString.Contains("FMA")))
                        {

                            #region changed contains by starts with fun
                            if (_receivedString.Contains("CARDIT"))
                                MessageType = "CARDIT";
                            if (_receivedString.StartsWith("FFR/"))
                                MessageType = "FFR";
                            else if (_receivedString.StartsWith("FSN"))
                                MessageType = "FSN";
                            else if (_receivedString.StartsWith("CPM"))
                                MessageType = "CPM";
                            else if (_receivedString.StartsWith("ASM"))
                                MessageType = "ASM";
                            else if (_receivedString.StartsWith("MVT"))
                                MessageType = "MVT";
                            else if (_receivedString.StartsWith("FWB/"))
                                MessageType = "FWB";
                            else if (_receivedString.StartsWith("FFM/"))
                                MessageType = "FFM";
                            else if (_receivedString.StartsWith("FAD/"))
                                MessageType = "FAD";
                            else if (_receivedString.StartsWith("FHL/"))
                                MessageType = "FHL";
                            else if (_receivedString.StartsWith("FSU/"))
                                MessageType = "FSU";
                            else if (_receivedString.StartsWith("UCM"))
                                MessageType = "UCM";
                            else if (_receivedString.StartsWith("FBL/"))
                                MessageType = "FBL";
                            else if (_receivedString.StartsWith("SCM"))
                                MessageType = "SCM";
                            else if (_receivedString.StartsWith("FSB"))
                                MessageType = "FSB";
                            else if (_receivedString.StartsWith("FWR/"))
                                MessageType = "FWR";
                            else if (_receivedString.StartsWith("FBR/"))
                                MessageType = "FBR/";
                            else if (_receivedString.StartsWith("FNA/"))
                                MessageType = "FNA";
                            else if (_receivedString.StartsWith("FMA"))
                                MessageType = "FMA";
                            else if (_receivedString.StartsWith("FMB"))
                                MessageType = "FMB";
                            #endregion

                            if (MessageType.ToUpper() != "CARDIT")
                            {
                                _receivedString = _receivedString.Substring(_receivedString.IndexOf(MessageType),
                                                                            _receivedString.Length -
                                                                            _receivedString.IndexOf(MessageType));
                            }


                            if (_receivedString.ToUpper().Contains("------=_NEXTPART"))
                            {
                                int indexOfEndPart = _receivedString.IndexOf("------=_NEXTPART");
                                _receivedString = _receivedString.Substring(0, indexOfEndPart);
                            }

                        }


                        string Subject = imap.GetSubjectByUid(arrUID[i]).ToString();
                        if (string.IsNullOrEmpty(Subject))
                            Subject = "Subject";

                        string fromEmail = imap.GetFromByUid(arrUID[i]).ToString();
                        fromEmail = fromEmail.Replace("<", "");
                        fromEmail = fromEmail.Replace(">", "");

                        string toEmail = "";
                        toEmail = toEmail.Replace("<", "");
                        toEmail = toEmail.Replace(">", "");

                        string recievedDate = imap.GetDateByUid(arrUID[i]).ToString();
                        string status = "Active";
                        // "01 Feb 2013 15:51:12"
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                        // string status = "Active";
                        try
                        {
                            if (await StoreIROPSEmail(Subject.ToUpper(), _receivedString.ToUpper(), fromEmail, toEmail, dtRec, dtSend, MessageType == "" ? Subject : MessageType, status, "EMAIL"))
                            {
                                // clsLog.WriteLogAzure("Email " + (i + 1) + " Saved");
                                _logger.LogInformation($"Email {i + 1}  Saved");
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                        try
                        {
                            imap.Delete(arrUID[i]);
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        string subject = string.Empty, fromEmail = string.Empty;
                        bool toSave = false;
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        string recievedDate = imap.GetDateByUid(arrUID[i]).ToString();
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception exp)
                        {
                            // clsLog.WriteLogAzure(exp);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        if (ex.Message.Trim().ToUpper() == "COULD NOT FIND ANY RECOGNIZABLE DIGITS.")
                        {
                            subject = ex.Message.Trim().ToUpper();
                            toSave = true;
                        }
                        else if (imap.GetSubjectByUid(arrUID[i]).ToString().ToUpper() == "UNDELIVERED MAIL RETURNED TO SENDER")
                        {
                            subject = imap.GetSubjectByUid(arrUID[i]).ToString().ToUpper();
                            toSave = true;
                        }
                        if (toSave)
                        {
                            fromEmail = imap.GetFromByUid(arrUID[i]).ToString();
                            fromEmail = fromEmail.Replace("<", "").Replace(">", "");
                            if (await StoreIROPSEmail(subject, _receivedString.ToUpper().Substring(0, 2000), fromEmail, "", dtRec, dtSend, "Unsupported message", "Processed", "EMAIL"))
                                // clsLog.WriteLogAzure("Email " + (i + 1) + " Saved");
                                _logger.LogError($"Email {i + 1} Saved");
                            imap.Delete(arrUID[i]);
                        }
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task<bool> StoreIROPSEmail(string subject, string body, string fromId, string toId, DateTime recievedOn, DateTime sendOn, string type, string status, string CommunicationType)
        {
            bool flag = false;
            try
            {
                // clsLog.WriteLogAzure("StoreMail to Db: " + (subject.Trim().Length > 55 ? subject.Substring(0, 50) : subject.Trim()));
                _logger.LogInformation("StoreMail to Db: {0}", (subject.Trim().Length > 55 ? subject.Substring(0, 50) : subject.Trim()));

                //SQLServer db = new SQLServer(); ;
                //string[] param = { "subject", "body", "fromId", "toId", "recievedOn", "sendOn", "type", "status", "CommunicationType" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //object[] values = { subject, body, fromId, toId, recievedOn, sendOn, type, status, CommunicationType };

                //flag = db.InsertData("spSavetoInbox", param, sqldbtypes, values);
                //db = null;
                //GC.Collect();

                SqlParameter[] parameters =
                [
                    new("@subject", SqlDbType.VarChar) { Value = subject },
                    new("@body", SqlDbType.VarChar) { Value = body },
                    new("@fromId", SqlDbType.VarChar) { Value = fromId },
                    new("@toId", SqlDbType.VarChar) { Value = toId },
                    new("@recievedOn", SqlDbType.DateTime) { Value = recievedOn },
                    new("@sendOn", SqlDbType.DateTime) { Value = sendOn },
                    new("@type", SqlDbType.VarChar) { Value = type },
                    new("@status", SqlDbType.VarChar) { Value = status },
                    new("@CommunicationType", SqlDbType.VarChar) { Value = CommunicationType }
                ];

                flag = await _readWriteDao.ExecuteNonQueryAsync("spSavetoInbox", parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on StoreIROPSEmail.");
                flag = false;
            }
            return flag;
        }

        //public string ReadValueFromDb(string Parameter)
        //{
        //    try
        //    {
        //        //string ParameterValue = string.Empty;
        //        //SQLServer da = new SQLServer();
        //        //string[] QName = new string[] { "PType" };
        //        //object[] QValues = new object[] { Parameter };
        //        //SqlDbType[] QType = new SqlDbType[] { SqlDbType.VarChar };
        //        //ParameterValue = da.GetStringByProcedure("spGetSystemParameter", QName, QValues, QType);
        //        //if (ParameterValue == null)
        //        //    ParameterValue = "";
        //        //da = null;
        //        //QName = null;
        //        //QValues = null;
        //        //QType = null;
        //        //GC.Collect();
        //        //return ParameterValue;

        //        return GetConfigurationValues(Parameter);
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    return null;
        //}


        /*Not in use*/
        //public byte[] DownloadBlobX(string filenameOrUrl)
        //{
        //    try
        //    {

        //        string containerName = ""; //container must be lowercase, no special characters
        //        if (filenameOrUrl.Contains('/'))
        //        {
        //            filenameOrUrl = filenameOrUrl.ToLower();
        //            containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
        //            filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
        //        }

        //        byte[] downloadStream = null;
        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey("hawaiianstorage", "wcafYuM5usLvBUfQ642acJs41ZCOe6ZlGIFt3PFT2xooLfwTiZpKS+Fs73m7cmfwUN1BAxBYfLcpBsicwoRe8A==");
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);

        //        //get a reference to the blob
        //        CloudBlob blob = blobClient.GetBlobReference("/attachments/2.png");//(string.Format("{0}/{1}", containerName, filenameOrUrl));

        //        //write the file to the http response
        //        //blob.DownloadToStream(downloadStream);
        //        downloadStream = blob.DownloadByteArray();
        //        //FetchAttributes();

        //        return downloadStream;

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return null;
        //    }

        //}

        /*WindowsAzure.Storage nuget package is deprecated and using the Azure.Storage.Blobs*/
        //public byte[] DownloadBlob(string filenameOrUrl)
        //{
        //    try
        //    {

        //        string containerName = "";
        //        string str = filenameOrUrl;
        //        if (filenameOrUrl.Contains('/'))
        //        {
        //            containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
        //            containerName = containerName.ToLower();//Container name should be in lower case.
        //            filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
        //        }
        //        byte[] downloadStream = null;
        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);
        //        try
        //        {
        //            CloudBlob blob = blobClient.GetBlobReference(string.Format("{0}/{1}", containerName, filenameOrUrl));
        //            downloadStream = blob.DownloadByteArray();


        //        }
        //        catch (Exception ex)
        //        {
        //            clsLog.WriteLogAzure(ex);
        //            CloudBlob blob = blobClient.GetBlobReference(str);
        //            downloadStream = blob.DownloadByteArray();
        //        }

        //        return downloadStream;

        //    }
        //    catch (Exception ex)
        //    {

        //        clsLog.WriteLogAzure(ex);
        //        return null;

        //    }

        //}

        public byte[] DownloadBlob(string filenameOrUrl)
        {
            try
            {

                string containerName = "";
                string str = filenameOrUrl;
                if (filenameOrUrl.Contains('/'))
                {
                    containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
                    containerName = containerName.ToLower();//Container name should be in lower case.
                    filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
                }

                byte[] downloadStream = null;

                // Set TLS 1.2 (still recommended for compatibility)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Use connection string or account name + key
                string storageAccountName = getStorageName();
                string storageKey = getStorageKey();

                // Preferred: Use shared key credential
                string accountUrl = $"https://{storageAccountName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new StorageSharedKeyCredential(storageAccountName, storageKey)
                );
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                try
                {
                    // Try with extracted container + blob name
                    BlobClient blobClient = containerClient.GetBlobClient(filenameOrUrl);
                    downloadStream = DownloadBlobContent(blobClient);
                }
                catch (Exception ex)
                {
                    // Log exception (preserving original behavior)
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

                    // Fallback: Use full URL or original string as blob URI
                    BlobClient blobClient = containerClient.GetBlobClient(str);
                    downloadStream = DownloadBlobContent(blobClient);
                }

                return downloadStream;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        // Helper method to download blob as byte[]
        private byte[] DownloadBlobContent(BlobClient blobClient)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    blobClient.DownloadTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        /*Not in use*/
        //public byte[] DownloadBlobOld(string filenameOrUrl)
        //{
        //    try
        //    {

        //        string containerName = ""; //container must be lowercase, no special characters
        //        if (filenameOrUrl.Contains('/'))
        //        {
        //            filenameOrUrl = filenameOrUrl.ToLower();
        //            containerName = filenameOrUrl.Substring(filenameOrUrl.IndexOf("windows.net") + ("windows.net".Length) + 1, filenameOrUrl.LastIndexOf('/') - filenameOrUrl.IndexOf("windows.net") - ("windows.net".Length) - 1);
        //            filenameOrUrl = filenameOrUrl.Substring(filenameOrUrl.LastIndexOf('/') + 1);
        //        }

        //        byte[] downloadStream = null;
        //        StorageCredentialsAccountAndKey cred = new StorageCredentialsAccountAndKey(getStorageName(), getStorageKey());
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        CloudStorageAccount storageAccount = new CloudStorageAccount(cred, true);
        //        CloudBlobClient blobClient = new CloudBlobClient(storageAccount.BlobEndpoint.AbsoluteUri, cred);

        //        //get a reference to the blob
        //        //containerName = "attachments";
        //        //filenameOrUrl = "2.jpg";
        //        CloudBlob blob = blobClient.GetBlobReference(string.Format("{0}/{1}", containerName, filenameOrUrl));

        //        //write the file to the http response
        //        //blob.DownloadToStream(downloadStream);
        //        downloadStream = blob.DownloadByteArray();
        //        //FetchAttributes();

        //        return downloadStream;

        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        return null;
        //    }

        //}



        #region need to uncomment when SFTP/SITA/FTP is implemented for parallelism.
        //public void SendMail()
        //{
        //    //clsLog.WriteLogAzure("In SendMail()");
        //    try
        //    {
        //        GenericFunction genericFunction = new GenericFunction();
        //        // int outport = 0;
        //        f_SendOutgoingMail = Convert.ToBoolean(ConfigCache.Get("msgService_SendEmail") == string.Empty ? "false" : ConfigCache.Get("msgService_SendEmail"));
        //        bool isOn = false;
        //        // bool ishtml = false;
        //        // string status = "Processed";
        //        SQLServer objsql = new SQLServer();
        //        do
        //        {
        //            string ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty, ccadd = string.Empty, FileExtension = string.Empty, msgCommType = string.Empty;
        //            isOn = false;
        //            DataSet ds = null;
        //            ds = objsql.SelectRecords("[spSendMessages_Test]");

        //            #region SEND EMAIL
        //            if ((f_SendOutgoingMail == true))
        //            {
        //                if (ds != null)
        //                {
        //                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //                    {
        //                        isOn = true;

        //                        EMAILOUT objmail = new EMAILOUT();
        //                        objmail.sendEmail(ds);


        //                    }
        //                }

        //                else
        //                    isOn = false;
        //            }

        //            else
        //                isOn = false;
        //            #endregion 

        //        } while (isOn);

        //        objsql = null;
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        #endregion



        public async Task SendMail()
        {
            // clsLog.WriteLogAzure("In SendMail()");
            _logger.LogInformation("In SendMail()");
            try
            {
                //GenericFunction genericFunction = new GenericFunction();

                int outport = 0;
                f_SendOutgoingMail = Convert.ToBoolean(ConfigCache.Get("msgService_SendEmail") == string.Empty ? "false" : ConfigCache.Get("msgService_SendEmail"));
                bool isOn = false;
                bool ishtml = false;
                string status = "Processed";
                string SMTPUserName = string.Empty;

                //SQLServer objsql = new SQLServer();
                do
                {
                    string ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty, ccadd = string.Empty, FileExtension = string.Empty
                        , msgCommType = string.Empty;
                    isOn = false;
                    //ds = objsql.SelectRecords("spMailtoSend");

                    DataSet? ds = null;
                    ds = await _readWriteDao.SelectRecords("spMailtoSend");

                    if (ds != null)
                    {
                        // clsLog.WriteLogAzure("ds != null");
                        _logger.LogInformation("ds != null");

                        if (ds.Tables != null && ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                // clsLog.WriteLogAzure("Outgoing Mail Row Count" + ds.Tables[0].Rows.Count.ToString());
                                _logger.LogInformation("Outgoing Mail Row Count {0}", ds.Tables[0].Rows.Count);
                                isOn = true;
                                bool isMessageSent = false;
                                bool isFTPUploadSuccessfully = true;
                                DataRow dr = ds.Tables[0].Rows[0];
                                string subject = dr[1].ToString();
                                msgCommType = "EMAIL";
                                DataRow drMsg = null;
                                DataRow drEmailAccount = null;
                                string FileName = dr["Subject"].ToString();
                                string body = dr[2].ToString();
                                string messageType = dr["Type"].ToString();
                                string actualMsg = dr["ActualMsg"].ToString();
                                string SITAFolderPath = dr["SITAFolderPath"].ToString().Trim();
                                string awbNumber = dr["AWBNumber"].ToString();
                                string flightNo = dr["FlightNo"].ToString();

                                DateTime flightDate = default(DateTime);
                                try
                                {
                                    flightDate = Convert.ToDateTime(dr["FlightDt"]);
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }

                                string sentadd = dr[4].ToString().Trim(',');
                                if (dr[3].ToString().Length > 3)
                                    ccadd = dr[3].ToString().Trim(',');
                                ishtml = bool.Parse(dr["ishtml"].ToString() == "" ? "False" : dr["ishtml"].ToString());

                                if (ds.Tables[2].Rows.Count > 0)
                                {
                                    drMsg = ds.Tables[2].Rows[0];
                                    msgCommType = drMsg["MsgCommType"].ToString().ToUpper().Trim();
                                    FileExtension = drMsg["FileExtension"].ToString().ToUpper().Trim();
                                    if (FileExtension.ToUpper() == "XML")
                                    {
                                        FileExtension = drMsg["FileExtension"].ToString().Trim();
                                    }
                                }

                                if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
                                    status = "Processed";
                                if (ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                                    status = "Re-Processed";

                                #region FTP File Upload

                                if (msgCommType.ToUpper() == "FTP" || msgCommType.ToUpper() == "ALL")
                                {

                                    //FTP _ftp = new FTP();
                                    if (drMsg != null && drMsg.ItemArray.Length > 0)
                                    {
                                        if (drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
                                        {
                                            ftpUrl = drMsg["FTPID"].ToString();
                                            ftpUserName = drMsg["FTPUserName"].ToString();
                                            ftpPassword = drMsg["FTPPassword"].ToString();

                                        } // Rohidas added else condition for live issue on 30 aug 2017
                                        else
                                        {
                                            ftpUrl = ConfigCache.Get("FTPURLofFileUpload");
                                            ftpUserName = ConfigCache.Get("FTPUserofFileUpload");
                                            ftpPassword = ConfigCache.Get("FTPPasswordofFileUpload");
                                        }
                                    }
                                    else
                                    {
                                        ftpUrl = ConfigCache.Get("FTPURLofFileUpload");
                                        ftpUserName = ConfigCache.Get("FTPUserofFileUpload");
                                        ftpPassword = ConfigCache.Get("FTPPasswordofFileUpload");
                                    }
                                    if (FileName.ToUpper().Contains(".TXT"))
                                    {

                                        int fileIndex = FileName.IndexOf(".");
                                        if (fileIndex > 0)
                                        {
                                            FileName = FileName.Substring(0, fileIndex);
                                            FileExtension = "txt";
                                        }
                                    }
                                    else if (FileName.ToUpper().Contains(".XML"))
                                    {
                                        int fileIndex = FileName.IndexOf(".");
                                        if (fileIndex > 0)
                                        {
                                            FileName = FileName.Substring(0, fileIndex);
                                            if (FileExtension.ToUpper() != "XML")
                                            {
                                                FileExtension = "xml";
                                            }
                                        }
                                    }
                                    else
                                        FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
                                    if (ftpUrl != "")
                                    {
                                        bool uploadRes = _ftp.UploadfileThrougFTPAndRenamefileAfterUploaded(ftpUrl, ftpUserName, ftpPassword, body, FileName, FileExtension == "" ? "SND" : FileExtension);
                                        if (uploadRes)
                                        {
                                            //string[] pname = { "num", "Status" };
                                            //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                            //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                            //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                            //    clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());

                                            isMessageSent = true;

                                            SqlParameter[] parameters =
                                            {
                                                new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                            };
                                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                            if (dbRes)
                                            {
                                                // clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
                                                _logger.LogInformation("uploaded on ftp successfully to: {0}" + dr[0]);
                                            }

                                        }
                                        else
                                        {
                                            isMessageSent = false;
                                            isFTPUploadSuccessfully = false;
                                        }
                                    }


                                }
                                #endregion

                                // clsLog.WriteLogAzure("msgCommType-" + msgCommType);
                                _logger.LogInformation("msgCommType- {0}", msgCommType);

                                #region Email
                                if ((msgCommType.ToUpper() == "EMAIL" || msgCommType.ToUpper() == "ALL") && (f_SendOutgoingMail == true))
                                {
                                    accountEmail = ConfigCache.Get("msgService_OutEmailId");
                                    password = ConfigCache.Get("msgService_OutEmailPassword");
                                    string MailIouterver = ConfigCache.Get("msgService_EmailOutServer");
                                    MailsendPort = ConfigCache.Get("msgService_OutgoingMessagePort");


                                    if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
                                    {
                                        drEmailAccount = ds.Tables[3].Rows[0];
                                        if (drEmailAccount["EmailAddress"].ToString().Trim() != string.Empty
                                            && drEmailAccount["Password"].ToString().Trim() != string.Empty
                                            && drEmailAccount["ServerName"].ToString().Trim() != string.Empty
                                            && drEmailAccount["PortNumber"].ToString().Trim() != string.Empty)
                                        {
                                            accountEmail = drEmailAccount["EmailAddress"].ToString().Trim();
                                            password = drEmailAccount["Password"].ToString().Trim();
                                            MailIouterver = drEmailAccount["ServerName"].ToString().Trim();
                                            MailsendPort = drEmailAccount["PortNumber"].ToString().Trim();
                                            SMTPUserName = drEmailAccount["SMTPUserName"].ToString().Trim();
                                        }
                                    }


                                    if (MailsendPort != "")
                                        outport = int.Parse(MailsendPort == "" ? "110" : MailsendPort);
                                    else
                                        outport = int.Parse(Convert.ToString(ConfigCache.Get("OutPort")));//outport = int.Parse(ConfigurationManager.AppSettings["OutPort"].ToString());

                                    #region Email
                                    //EMAILOUT objmail = new EMAILOUT();
                                    //MailKitManager ObjMailKit = new MailKitManager();

                                    if (sentadd.Length > 2 && sentadd.Contains("@") && sentadd.Contains("."))
                                    {
                                        if (MailIouterver.ToUpper() == "SENDGRID")
                                        {
                                            bool success = false;
                                            try
                                            {
                                                if (sentadd.Length > 0 && sentadd.Contains("@") && accountEmail != "" && password != "" && sentadd != "" && body != "")
                                                {
                                                    success = _emailOut.SendEmailOutSendgrid(ds, accountEmail, sentadd, password, subject, body, ishtml, ccadd, subject);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Exception in sending SendEmailOutSendgrid" + ex); 
                                                _logger.LogError(ex, "Exception in sending SendEmailOutSendgrid");
                                                success = false;
                                            }
                                            if (success)
                                            {
                                                //string[] pname = { "num", "Status" };
                                                //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                //{
                                                //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                //}

                                                isMessageSent = true;
                                                SqlParameter[] parameters =
                                                {
                                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                };
                                                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                if (dbRes)
                                                {
                                                    // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                    _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                }
                                            }
                                            else
                                            {
                                                isMessageSent = false;
                                            }
                                        }
                                        else if (MailIouterver.ToUpper() == "EMAIL-SMTP.AP-SOUTHEAST-1.AMAZONAWS.COM")
                                        {
                                            bool success = false;
                                            try
                                            {
                                                if (sentadd.Length > 0 && sentadd.Contains("@") && accountEmail != "" && password != "" && sentadd != "" && body != "")
                                                {
                                                    success = _mailKitManager.SendEmailMailKitManager(ds, accountEmail, sentadd, password, subject, body, ishtml, ccadd, subject, SMTPUserName, MailIouterver);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure("Exception in sending SendEmailOutSendgrid" + ex); 
                                                _logger.LogError(ex, "Exception in sending SendEmailOutSendgrid");
                                                success = false;
                                            }
                                            if (success)
                                            {
                                                //string[] pname = { "num", "Status" };
                                                //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                //{
                                                //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                //}

                                                isMessageSent = true;
                                                SqlParameter[] parameters =
                                                {
                                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                };
                                                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                if (dbRes)
                                                {
                                                    // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                    _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                }
                                            }
                                            else
                                            {
                                                isMessageSent = false;
                                            }

                                        }
                                        else
                                        {
                                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                                            {

                                                if (ds.Tables[1].Rows[0][0].ToString().ToUpper() != "NA")
                                                {
                                                    #region Mail with attachment
                                                    // clsLog.WriteLogAzure("Inside Table 1 for MessageID :" + dr[0].ToString());
                                                    _logger.LogInformation("Inside Table 1 for MessageID: {0}", dr[0].ToString());
                                                    if (ds.Tables[1].Rows.Count > 0)
                                                    {
                                                        // clsLog.WriteLogAzure("Inside Table 1 Rows for MessageID :" + dr[0].ToString());
                                                        _logger.LogInformation("Inside Table 1 Rows for MessageID: {0}", dr[0].ToString());
                                                        MemoryStream[] Attachments = new MemoryStream[0];
                                                        string[] Extensions = new string[0];
                                                        string[] AttachmentName = new string[0];
                                                        foreach (DataRow drow in ds.Tables[1].Rows)
                                                        {
                                                            try
                                                            {
                                                                Array.Resize(ref Attachments, Attachments.Length + 1);
                                                                Attachments[Attachments.Length - 1] = new MemoryStream(DownloadBlob(drow["FileUrl"].ToString()));

                                                                Array.Resize(ref Extensions, Extensions.Length + 1);
                                                                Extensions[Extensions.Length - 1] = drow["MIMEType"].ToString();
                                                                if (!Extensions[Extensions.Length - 1].Contains('.'))
                                                                    Extensions[Extensions.Length - 1] = "." + Extensions[Extensions.Length - 1];
                                                                Array.Resize(ref AttachmentName, AttachmentName.Length + 1);
                                                                AttachmentName[AttachmentName.Length - 1] = drow["AttachmentName"].ToString();
                                                            }
                                                            catch (Exception)
                                                            {
                                                                // clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                                                _logger.LogError("Error in EmailOut for SRNO:: {0}", dr[0]);
                                                            }

                                                        }
                                                        if (accountEmail != "" && password != "" && sentadd != "" && body != "")
                                                        {
                                                            //if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, Attachments, AttachmentName, Extensions, messageType, MailIouterver))

                                                            bool isSuccess = _emailOut.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, Attachments, AttachmentName, Extensions, messageType, MailIouterver);

                                                            if (isSuccess)
                                                            {

                                                                //string[] pname = { "num", "Status" };
                                                                //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                                //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                                //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());

                                                                isMessageSent = true;
                                                                // clsLog.WriteLogAzure("After Sending Mail with Attachment for MessageID :" + dr[0].ToString());
                                                                _logger.LogInformation("After Sending Mail with Attachment for MessageID : {0}", dr[0]);
                                                                SqlParameter[] parameters =
                                                                {
                                                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                                };
                                                                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                                if (dbRes)
                                                                {
                                                                    // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                                    _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                isMessageSent = false;
                                                                // clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                                                _logger.LogWarning("Error in EmailOut for SRNO:: {0}", dr[0]);

                                                            }
                                                        }


                                                    }

                                                    #endregion Mail with attachment
                                                }
                                                else
                                                {
                                                    #region Send Email
                                                    if (accountEmail != "" && password != "" && sentadd != "" && body != "")
                                                    {
                                                        //if (objmail.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, messageType, MailIouterver))

                                                        if (_emailOut.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, messageType, MailIouterver))
                                                        {
                                                            //string[] pname = { "num", "Status" };
                                                            //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                            //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                            //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                            //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());

                                                            isMessageSent = true;
                                                            SqlParameter[] parameters =
                                                                {
                                                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                                };
                                                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                            if (dbRes)
                                                            {
                                                                // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                                _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                            }

                                                        }
                                                        else
                                                        {
                                                            isMessageSent = false;
                                                            // clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                                            _logger.LogWarning("Error in EmailOut for SRNO:: {0}", dr[0]);

                                                        }
                                                    }
                                                    #endregion send email
                                                }
                                            }
                                            else
                                            {
                                                #region Send Email
                                                if (accountEmail != "" && password != "" && sentadd != "" && body != "")
                                                {

                                                    if (_emailOut.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, "", MailIouterver))
                                                    {
                                                        //isMessageSent = true;
                                                        //string[] pname = { "num", "Status" };
                                                        //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                        //{
                                                        //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                        //}

                                                        isMessageSent = true;
                                                        SqlParameter[] parameters =
                                                        {
                                                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                        };
                                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                        if (dbRes)
                                                        {
                                                            // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                            _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        isMessageSent = false;
                                                        // clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                                        _logger.LogWarning("Error in EmailOut for SRNO:: {0}", dr[0]);

                                                    }
                                                }
                                                #endregion send email
                                            }
                                        }
                                    }
                                    #endregion

                                }

                                #endregion

                                #region SFTP Upload
                                if (msgCommType.ToUpper() == "SFTP" || msgCommType.ToUpper() == "ALL" || msgCommType.Equals("SFTP", StringComparison.OrdinalIgnoreCase))
                                {
                                    //FTP _ftp = new FTP();

                                    string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;
                                    foreach (DataRow drSFTP in ds.Tables[2].Rows)
                                    {
                                        if (drSFTP != null && drSFTP.ItemArray.Length > 0 && drSFTP["FTPID"].ToString() != "" && drSFTP["FTPUserName"].ToString() != "" && (drSFTP["FTPPassword"].ToString() != "" || drSFTP["PPKFileName"].ToString() != ""))
                                        {
                                            SFTPAddress = drSFTP["FTPID"].ToString();
                                            SFTPUserName = drSFTP["FTPUserName"].ToString();
                                            SFTPPassWord = drSFTP["FTPPassword"].ToString();
                                            ppkFileName = drSFTP["PPKFileName"].ToString().Trim();
                                            SFTPFingerPrint = drSFTP["FingerPrint"].ToString();
                                            StpFolerParth = drSFTP["RemotePath"].ToString();
                                            SFTPPortNumber = drSFTP["PortNumber"].ToString();
                                            GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");
                                        }
                                        else
                                        {
                                            SFTPAddress = ConfigCache.Get("msgService_IN_SITAFTP");
                                            SFTPUserName = ConfigCache.Get("msgService_IN_SITAUser");
                                            SFTPPassWord = ConfigCache.Get("msgService_IN_SITAPWD");
                                            ppkFileName = ConfigCache.Get("PPKFileName");
                                            SFTPFingerPrint = ConfigCache.Get("msgService_IN_SFTPFingerPrint");
                                            StpFolerParth = ConfigCache.Get("msgService_OUT_FolderPath");
                                            SFTPPortNumber = ConfigCache.Get("msgService_IN_SITAPort");
                                            GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");
                                        }

                                        if (ppkFileName != string.Empty)
                                        {
                                            //ppkLocalFilePath = genericFunction.GetPPKFilePath(ppkFileName);
                                            ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                                        }

                                        FileName = dr["Subject"].ToString();

                                        if (FileName.ToUpper().Contains(".TXT"))
                                        {
                                            int fileIndex = FileName.IndexOf(".");
                                            if (fileIndex > 0)
                                                FileName = FileName.Substring(0, fileIndex);

                                        }
                                        else if (FileName.ToUpper().Contains(".XML"))
                                        {
                                            int fileIndex = FileName.IndexOf(".");
                                            if (fileIndex > 0)
                                            {
                                                FileName = FileName.Substring(0, fileIndex);
                                                FileExtension = "xml";
                                            }
                                        }
                                        else if (FileName.ToUpper().Contains(".CSV"))
                                        {
                                            int fileIndex = FileName.IndexOf(".");
                                            if (fileIndex > 0)
                                                FileName = FileName.Substring(0, fileIndex);

                                        }
                                        else
                                            FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");


                                        if (SFTPAddress != "" && SFTPUserName != "" && (SFTPPassWord != "" || ppkFileName != string.Empty) && SFTPFingerPrint != "" && StpFolerParth != "" && SFTPPortNumber.Trim() != string.Empty)
                                        {
                                            int portNumber = Convert.ToInt32(SFTPPortNumber);
                                            if (_ftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, body, FileName, FileExtension == string.Empty ? ".SND" : FileExtension, StpFolerParth, portNumber, ppkLocalFilePath, GHAOutFolderPath))
                                            {
                                                //isMessageSent = true;
                                                //string[] pname = { "num", "Status" };
                                                //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                //    clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());

                                                isMessageSent = true;
                                                SqlParameter[] parameters =
                                                {
                                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                };
                                                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                if (dbRes)
                                                {
                                                    // clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());
                                                    _logger.LogInformation($"File uploaded on sftp successfully to:{dr[0]}");
                                                }
                                            }
                                            else
                                            {
                                                isFTPUploadSuccessfully = false;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region SITA Upload
                                if (msgCommType.ToUpper() == "SITA" || msgCommType.ToUpper() == "SITAFTP"
                                    || msgCommType.ToUpper() == "ALL"
                                    || msgCommType.Equals("SITA", StringComparison.OrdinalIgnoreCase)
                                    || msgCommType.Equals("SITAFTP", StringComparison.OrdinalIgnoreCase))
                                {
                                    //FTP _ftp = new FTP();
                                    string SFTPFingerPrint = string.Empty, StpFolerParth = string.Empty, SFTPPortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty;

                                    if (drMsg != null && drMsg.ItemArray.Length > 0 && drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
                                    {
                                        SFTPAddress = drMsg["FTPID"].ToString();
                                        SFTPUserName = drMsg["FTPUserName"].ToString();
                                        SFTPPassWord = drMsg["FTPPassword"].ToString();
                                        ppkFileName = drMsg["PPKFileName"].ToString().Trim();
                                        SFTPFingerPrint = drMsg["FingerPrint"].ToString();
                                        StpFolerParth = drMsg["RemotePath"].ToString();
                                        SFTPPortNumber = drMsg["PortNumber"].ToString();
                                        GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");
                                    }
                                    else
                                    {
                                        SFTPAddress = ConfigCache.Get("msgService_IN_SITAFTP");
                                        SFTPUserName = ConfigCache.Get("msgService_IN_SITAUser");
                                        SFTPPassWord = ConfigCache.Get("msgService_IN_SITAPWD");
                                        ppkFileName = ConfigCache.Get("PPKFileName");
                                        SFTPFingerPrint = ConfigCache.Get("msgService_IN_SFTPFingerPrint");
                                        StpFolerParth = ConfigCache.Get("msgService_OUT_FolderPath");
                                        SFTPPortNumber = ConfigCache.Get("msgService_IN_SITAPort");
                                        GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");
                                    }
                                    if (ppkFileName != string.Empty)
                                    {
                                        ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                                    }
                                    if (FileName.ToUpper().Contains(".TXT"))
                                    {
                                        int fileIndex = FileName.IndexOf(".");
                                        if (fileIndex > 0)
                                            FileName = FileName.Substring(0, fileIndex);

                                    }
                                    else if (FileName.ToUpper().Contains(".XML"))
                                    {
                                        int fileIndex = FileName.IndexOf(".");
                                        if (fileIndex > 0)
                                        {
                                            FileName = FileName.Substring(0, fileIndex);
                                            FileExtension = "xml";
                                        }
                                    }
                                    else
                                        FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");


                                    if (SFTPAddress != "" && SFTPUserName != "" && (SFTPPassWord != "" || ppkFileName != string.Empty) && SFTPFingerPrint != "" && StpFolerParth != "" && SFTPPortNumber.Trim() != string.Empty)
                                    {
                                        int portNumber = Convert.ToInt32(SFTPPortNumber);
                                        if (_ftp.SaveSFTPUpload(SFTPAddress, SFTPUserName, SFTPPassWord, SFTPFingerPrint, body, FileName, FileExtension == string.Empty ? ".SND" : FileExtension, StpFolerParth, portNumber, ppkLocalFilePath, GHAOutFolderPath))
                                        {
                                            //string[] pname = { "num", "Status" };
                                            //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                            //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                            //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                            //    clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());

                                            isMessageSent = true;
                                            SqlParameter[] parameters =
                                            {
                                                    new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                    new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                };
                                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                            if (dbRes)
                                            {
                                                // clsLog.WriteLogAzure("File uploaded on sftp successfully to:" + dr[0].ToString());
                                                _logger.LogInformation($"File uploaded on sftp successfully to:{dr[0]}");
                                            }

                                        }
                                        else
                                        {
                                            isFTPUploadSuccessfully = false;
                                        }
                                    }

                                }
                                #endregion

                                #region AZURE DRIVE
                                if (msgCommType == "DRIVE" || msgCommType == "ALL")
                                {
                                    //AzureDrive drive = new AzureDrive();
                                    if (_azureDrive.UploadToDrive(DateTime.Now.ToString("yyyyMMdd_hhmmss_fff"), body, SITAFolderPath))
                                    {
                                        //isMessageSent = true;
                                        //string[] pname = { "num", "Status" };
                                        //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                        //    clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dr[0].ToString());

                                        isMessageSent = true;
                                        SqlParameter[] parameters =
                                        {
                                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                        };
                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                        if (dbRes)
                                        {
                                            // clsLog.WriteLogAzure("uploaded on Azure Share Drive successfully to:" + dr[0].ToString());
                                            _logger.LogInformation($"uploaded on Azure Share Drive successfully to:{dr[0]}");
                                        }

                                    }
                                    else
                                        isMessageSent = false;

                                }
                                #endregion

                                #region : MQ Message :
                                ///Region added by prashant on 13-Dec-2016
                                if (msgCommType.ToUpper() == "MESSAGE QUEUE" || msgCommType == "ALL")
                                {
                                    if (drMsg != null)
                                    {
                                        int WaitInterval = 0;

                                        if (body.Trim() != string.Empty)
                                        {
                                            string MQManager = Convert.ToString(drMsg["MQManager"]);
                                            string MQChannel = Convert.ToString(drMsg["MQChannel"]);
                                            string MQHost = Convert.ToString(drMsg["MQHost"]);
                                            string MQPort = Convert.ToString(drMsg["MQPort"]);
                                            string MQUser = Convert.ToString(drMsg["MQUser"]);
                                            string MQInqueue = Convert.ToString(drMsg["MQOutQueue"]);//"CG.BOOKINGS.CARGOSPOT.SMARTKARGO";
                                            string MQOutqueue = "";
                                            string ErrorMessage = string.Empty;
                                            string Message = body;

                                            if (MQManager.Trim() != string.Empty && MQChannel.Trim() != string.Empty && MQHost.Trim() != string.Empty && MQPort.Trim() != string.Empty && MQInqueue.Trim() != string.Empty)
                                            {
                                                MQAdapter mqAdapter = new MQAdapter(MessagingType.ASync, MQManager, MQChannel, MQHost, MQPort, MQUser, MQInqueue, MQOutqueue, WaitInterval);

                                                //Console.WriteLine();
                                                //Console.WriteLine("*********** Send ***********");
                                                //Console.WriteLine("MQInqueue :" + MQInqueue);
                                                //Console.WriteLine("MQOutqueue :" + MQOutqueue);
                                                //Console.WriteLine("Message :" + Message);

                                                string result = mqAdapter.SendMessage(Message, out ErrorMessage);

                                                if (ErrorMessage.Trim() == string.Empty)
                                                {
                                                    //clsLog.WriteLogAzure("MQMessage sent successfully");
                                                    //Console.WriteLine("MQMessage sent successfully");
                                                    //isMessageSent = true;
                                                    //string[] pname = { "num", "Status" };
                                                    //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                                    //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                                    //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                    //    clsLog.WriteLogAzure("MQ Message Sent successfully to:" + dr[0].ToString());

                                                    isMessageSent = true;
                                                    SqlParameter[] parameters =
                                                    {
                                                        new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                                    };
                                                    var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                    if (dbRes)
                                                    {
                                                        // clsLog.WriteLogAzure("MQ Message Sent successfully to:" + dr[0].ToString());
                                                        _logger.LogInformation($"MQ Message Sent successfully to:{dr[0]}");
                                                    }
                                                }
                                                else
                                                {
                                                    // clsLog.WriteLogAzure("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
                                                    // Console.WriteLine("Fail to send MQMessage : ErrorMessage :" + ErrorMessage);
                                                    _logger.LogWarning("Fail to send MQMessage : ErrorMessage : {0}", ErrorMessage);
                                                }
                                                //TO DO : below statement is to be removed
                                                //Console.ReadLine();
                                                mqAdapter.DisposeQueue();
                                            }
                                            else
                                            {
                                                // clsLog.WriteLogAzure("Info : In(SendMail() method):Insufficient MQ Message Configuration");
                                                _logger.LogWarning("Info : In(SendMail() method):Insufficient MQ Message Configuration");
                                            }

                                        }
                                    }
                                }
                                #endregion

                                #region UFTP
                                if (msgCommType.ToUpper() == "UFTP" || msgCommType == "ALL")
                                {

                                    //FTP _ftp = new FTP();
                                    if (drMsg != null && drMsg.ItemArray.Length > 0)
                                    {
                                        if (drMsg["FTPID"].ToString() != "" && drMsg["FTPUserName"].ToString() != "" && drMsg["FTPPassword"].ToString() != "")
                                        {
                                            ftpUrl = drMsg["FTPID"].ToString();
                                            ftpUserName = drMsg["FTPUserName"].ToString();
                                            ftpPassword = drMsg["FTPPassword"].ToString();

                                        } // Rohidas added else condition for live issue on 30 aug 2017
                                        else
                                        {
                                            ftpUrl = ConfigCache.Get("FTPURLofFileUpload");
                                            ftpUserName = ConfigCache.Get("FTPUserofFileUpload");
                                            ftpPassword = ConfigCache.Get("FTPPasswordofFileUpload");
                                        }
                                    }
                                    else
                                    {
                                        ftpUrl = ConfigCache.Get("FTPURLofFileUpload");
                                        ftpUserName = ConfigCache.Get("FTPUserofFileUpload");
                                        ftpPassword = ConfigCache.Get("FTPPasswordofFileUpload");
                                    }
                                    if (FileName.ToUpper().Contains(".TXT"))
                                    {

                                        int fileIndex = FileName.IndexOf(".");
                                        if (fileIndex > 0)
                                        {
                                            FileName = FileName.Substring(0, fileIndex);
                                            FileExtension = "txt";
                                        }
                                    }
                                    else if (FileName.ToUpper().Contains(".XML"))
                                    {
                                        int fileIndex = FileName.IndexOf(".");
                                        if (fileIndex > 0)
                                        {
                                            FileName = FileName.Substring(0, fileIndex);
                                            FileExtension = "xml";
                                        }
                                    }
                                    else
                                        FileName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");
                                    if (ftpUrl != "")
                                    {
                                        if (_ftp.DownloadBlobAndFTPUpload(actualMsg, ftpUrl, ftpUserName, ftpPassword, body, FileName, FileExtension == "" ? "SND" : FileExtension))
                                        {
                                            //isMessageSent = true;
                                            //string[] pname = { "num", "Status" };
                                            //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                            //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                            //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                            //    clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());

                                            isMessageSent = true;
                                            SqlParameter[] parameters =
                                            {
                                                 new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                 new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                            };
                                            var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                            if (dbRes)
                                            {
                                                // clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
                                                _logger.LogInformation($"uploaded on ftp successfully to:{dr[0].ToString()}");
                                            }
                                        }
                                        else
                                        {
                                            isMessageSent = false;
                                            isFTPUploadSuccessfully = false;
                                        }
                                    }

                                }
                                #endregion

                                #region WebService
                                if ((msgCommType.ToUpper() == "WEBSERVICE" || msgCommType.ToUpper() == "ALL") && drMsg["WebServiceURL"].ToString() != "")
                                {
                                    try
                                    {
                                        string results = string.Empty;
                                        string username = drMsg["WebServiceUserName"].ToString();
                                        string Password = drMsg["WebServicePassword"].ToString();
                                        string URL = drMsg["WebServiceUrl"].ToString();
                                        string PartnerCode = drMsg["PartnerCode"].ToString();
                                        string customsName = drMsg["CustomsName"].ToString();
                                        if (messageType == "XFFM")
                                        {

                                            //ws.Params.Add("airline", PartnerCode);
                                            //ws.Params.Add("xffm", body);

                                            //WebService ws = new WebService(URL, "sendMessageXFFM", username, Password, body);
                                            //ws.Invoke(customsName);
                                            //results = ws.ResultString;

                                            results = _webService.Invoke(URL, "sendMessageXFFM", username, password, body, customsName);
                                        }
                                        else if (messageType == "XFWB")
                                        {

                                            // ws.Params.Add("airline", PartnerCode);
                                            //ws.Params.Add("xfwb", body);

                                            //WebService ws = new WebService(URL, "sendMessageXFWB", username, Password, body);
                                            //ws.Invoke(customsName);
                                            //results = ws.ResultString;

                                            results = _webService.Invoke(URL, "sendMessageXFWB", username, password, body, customsName);

                                        }
                                        else if (messageType == "XFZB")
                                        {

                                            //ws.Params.Add("airline", PartnerCode);
                                            //ws.Params.Add("xfzb", body);

                                            //WebService ws = new WebService(URL, "sendMessageXFZB", username, Password, body);
                                            //ws.Invoke(customsName);
                                            //results = ws.ResultString;

                                            results = _webService.Invoke(URL, "sendMessageXFZB", username, password, body, customsName);
                                        }

                                        await SaveMessage("DACCustoms", results, "WebService", "", DateTime.Now, DateTime.Now, messageType, "Active", "WebService", awbNumber, flightNo, flightDate);
                                        // clsLog.WriteLogAzure("Response message for " + messageType + ": " + results);
                                        _logger.LogInformation($"Response message for {messageType}: {results}");

                                        //isMessageSent = true;
                                        //string[] pname = { "num", "Status" };
                                        //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                        //    clsLog.WriteLogAzure("Message Sent SuccessFully to:" + dr[0].ToString());

                                        isMessageSent = true;
                                        SqlParameter[] parameters =
                                        {
                                             new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                             new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                         };
                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                        if (dbRes)
                                        {
                                            // clsLog.WriteLogAzure("Message Sent SuccessFully to:" + dr[0].ToString());
                                            _logger.LogInformation($"Message Sent SuccessFully to:{dr[0].ToString()}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        isMessageSent = false;
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                                #region WEBAPI
                                if ((msgCommType.ToUpper() == "WEBAPI" || msgCommType.ToUpper() == "ALL") && drMsg["WebServiceURL"].ToString() != "")
                                {
                                    try
                                    {
                                        string results = string.Empty;
                                        string username = drMsg["WebServiceUserName"].ToString();
                                        string Password = drMsg["WebServicePassword"].ToString();
                                        string URL = drMsg["WebServiceUrl"].ToString();
                                        string PartnerCode = drMsg["PartnerCode"].ToString();
                                        string customsName = drMsg["CustomsName"].ToString();
                                        if (messageType == "FFM")
                                        {

                                            //ws.Params.Add("airline", PartnerCode);
                                            //ws.Params.Add("xffm", body);

                                            //WebService ws = new WebService(URL, "", username, Password, body);
                                            //ws.Invoke(customsName);
                                            //results = ws.ResultString;

                                            results = _webService.Invoke(URL, "", username, password, body, customsName);

                                        }
                                        else if (messageType == "FWB")
                                        {

                                            // ws.Params.Add("airline", PartnerCode);
                                            //ws.Params.Add("xfwb", body);

                                            //WebService ws = new WebService(URL, "", username, Password, body);
                                            //results = ws.ResultString;

                                            /*This is commented earlier and not invoked so setting empty string for results*/
                                            ////ws.Invoke(customsName);

                                            /*Updated code*/
                                            //results = _webService.Invoke(URL, "", username, password, body, customsName);
                                            results = string.Empty;

                                        }
                                        else if (messageType == "FHL")
                                        {

                                            //ws.Params.Add("airline", PartnerCode);
                                            //ws.Params.Add("xfzb", body);

                                            //WebService ws = new WebService(URL, "", username, Password, body);
                                            //ws.Invoke(customsName);
                                            //results = ws.ResultString;

                                            results = _webService.Invoke(URL, "", username, password, body, customsName);

                                        }

                                        await SaveMessage("DACCustoms", results, "WebService", "", DateTime.Now, DateTime.Now, messageType, "Active", "WebService", awbNumber, flightNo, flightDate);

                                        // clsLog.WriteLogAzure("Response message for " + messageType + ": " + results);
                                        _logger.LogInformation($"Response message for {messageType}: {results}");

                                        //isMessageSent = true;
                                        //string[] pname = { "num", "Status" };
                                        //object[] pvalue = { int.Parse(dr[0].ToString()), status };
                                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                        //    clsLog.WriteLogAzure("Message Sent SuccessFully to:" + dr[0].ToString());

                                        isMessageSent = true;
                                        SqlParameter[] parameters =
                                        {
                                             new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                             new SqlParameter("@Status", SqlDbType.VarChar) { Value = status }
                                         };
                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                        if (dbRes)
                                        {
                                            // clsLog.WriteLogAzure("Message Sent SuccessFully to:" + dr[0].ToString());
                                            _logger.LogInformation($"Message Sent SuccessFully to:{dr[0].ToString()}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        isMessageSent = false;
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                                if (!isMessageSent)
                                {
                                    string ErrorMsg = "Error occured while processing sending request";
                                    if (!isFTPUploadSuccessfully)
                                    {
                                        //FTP ftp = new FTP();
                                        _ftp.FTPConnectionAlert();
                                        ErrorMsg = "FTP folder of SITA is not accessible";
                                    }

                                    //string[] pname = { "num", "Status", "ErrorMsg", "MsgDeliveryType" };
                                    //object[] pvalue = { int.Parse(dr[0].ToString()), status, ErrorMsg, msgCommType.ToUpper().Trim() };
                                    //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                                    //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                    //    clsLog.WriteLogAzure("Fail to sent email to:" + dr[0].ToString());

                                    SqlParameter[] parameters =
                                    {
                                        new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = status },
                                        new SqlParameter("@ErrorMsg", SqlDbType.VarChar) { Value = ErrorMsg },
                                        new SqlParameter("@MsgDeliveryType", SqlDbType.VarChar) { Value = msgCommType.ToUpper().Trim() }
                                    };

                                    var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                    if (dbRes)
                                    {
                                        // clsLog.WriteLogAzure("Fail to sent email to:" + dr[0].ToString());
                                        _logger.LogInformation($"Fail to sent email to:{dr[0]}");
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        isOn = false;
                    }
                    // clsLog.WriteLogAzure("SendMail While loop : " + isOn.ToString());
                    _logger.LogInformation($"SendMail While loop : {isOn}");

                } while (isOn);

                //objsql = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            // clsLog.WriteLogAzure("End SendMail()");
            _logger.LogInformation("End SendMail()");
        }


        /// <summary>
        /// Method to send the message via perticuler communication type
        /// </summary>
        public async Task SendMessage()
        {
            try
            {
                string ftpUrl = string.Empty, ftpUserName = string.Empty, ftpPassword = string.Empty, OutgoingMailServer = string.Empty, msgCommType = string.Empty;
                int outport = 0;
                //FTP ftp = new FTP();
                bool isSendOutgoingMail = false;

                //GenericFunction _genericFunction = new GenericFunction();

                isSendOutgoingMail = Convert.ToBoolean(ConfigCache.Get("msgService_SendEmail") == string.Empty ? "false" : ConfigCache.Get("msgService_SendEmail"));

                //SQLServer sqlServer = new SQLServer();
                DataSet? dsMessagesToSend = new DataSet();
                //dsMessagesToSend = sqlServer.SelectRecords("spSendMessages");

                dsMessagesToSend = await _readWriteDao.SelectRecords("spSendMessages");

                if (dsMessagesToSend != null && dsMessagesToSend.Tables.Count > 0)
                {
                    #region : Send Email :
                    if (dsMessagesToSend.Tables[0].Rows.Count > 0 && isSendOutgoingMail)
                    {
                        #region Email Configurations
                        accountEmail = ConfigCache.Get("msgService_OutEmailId");
                        password = ConfigCache.Get("msgService_OutEmailPassword");
                        MailsendPort = ConfigCache.Get("msgService_OutgoingMessagePort");
                        OutgoingMailServer = ConfigCache.Get("msgService_EmailOutServer");
                        if (MailsendPort != "")
                            outport = int.Parse(MailsendPort == "" ? "110" : MailsendPort);
                        else
                            outport = int.Parse(Convert.ToString(ConfigCache.Get("OutPort")));

                        #endregion
                        if (accountEmail != "" && password != "" && OutgoingMailServer != "" && MailsendPort != "")
                        {
                            //EMAILOUT emailout = new EMAILOUT();
                            await _emailOut.sendEmail(dsMessagesToSend, accountEmail, password, MailsendPort, OutgoingMailServer, outport);
                        }

                    }
                    #endregion Send Email

                    #region : SITA Upload :
                    if (dsMessagesToSend.Tables[1].Rows.Count > 0)
                    {
                        #region : Variable declaration & Get SITA Server Configurations :
                        string SITAAddress = string.Empty, SITAUserName = string.Empty, SITAPassWord = string.Empty, SITAFingerPrint = string.Empty, SITAFolerParth = string.Empty, SITAPortNumber = string.Empty;
                        string FingerPrint = string.Empty, FolerParth = string.Empty, PortNumber = string.Empty, GHAOutFolderPath = string.Empty, ppkLocalFilePath = string.Empty;
                        int portNumber = 0;

                        SITAAddress = ConfigCache.Get("msgService_IN_SITAFTP");
                        SITAUserName = ConfigCache.Get("msgService_IN_SITAUser");
                        SITAPassWord = ConfigCache.Get("msgService_IN_SITAPWD");
                        SITAFingerPrint = ConfigCache.Get("msgService_IN_SFTPFingerPrint");
                        SITAFolerParth = ConfigCache.Get("msgService_OUT_FolderPath");
                        SITAPortNumber = ConfigCache.Get("msgService_IN_SITAPort");
                        GHAOutFolderPath = ConfigCache.Get("msgService_OUTGHAMCT_FolderPath");
                        #endregion Variable declaration & Get SITA Server Configurations

                        if (SITAAddress != "" && SITAUserName != "" && SITAPassWord != "" && SITAFingerPrint != "" && SITAFolerParth != "" && SITAPortNumber.Trim() != string.Empty)
                        {
                            portNumber = Convert.ToInt32(SITAPortNumber);
                            await _ftp.SITAUpload(dsMessagesToSend.Tables[1], SITAAddress, SITAUserName, SITAPassWord, SITAFingerPrint, SITAFolerParth, portNumber, GHAOutFolderPath, ppkLocalFilePath);
                        }
                    }
                    #endregion SITA Upload

                    #region : FTP Upload :
                    if (dsMessagesToSend.Tables[2].Rows.Count > 0)
                    {
                        DataTable dtFTPIDs = dsMessagesToSend.Tables[2].DefaultView.ToTable(true, "FTPID");
                        for (int i = 0; i < dtFTPIDs.Rows.Count; i++)
                        {
                            DataTable dtMessagesToSend = dsMessagesToSend.Tables[2];
                            if (dtFTPIDs.Rows[i]["FTPID"].ToString().Trim() != string.Empty)
                            {
                                dtMessagesToSend.DefaultView.RowFilter = "FTPID = '" + dtFTPIDs.Rows[i]["FTPID"].ToString() + "'";
                                dtMessagesToSend = dtMessagesToSend.DefaultView.ToTable();
                                await _ftp.FTPUpload(dtMessagesToSend);
                                for (int j = dsMessagesToSend.Tables[2].Rows.Count - 1; j >= 0; j--)
                                {
                                    if (dsMessagesToSend.Tables[2].Rows[j]["FTPID"].ToString().Trim() == dtFTPIDs.Rows[i]["FTPID"].ToString().Trim())
                                    {
                                        dsMessagesToSend.Tables[2].Rows[j].Delete();
                                    }
                                }
                                dsMessagesToSend.Tables[2].AcceptChanges();
                            }
                        }
                        if (dsMessagesToSend.Tables[2].Rows.Count > 0)
                        {
                            await _ftp.FTPUpload(dsMessagesToSend.Tables[2]);
                        }
                    }
                    #endregion FTP Upload

                    #region : SFTP Upload:
                    if (dsMessagesToSend.Tables[3].Rows.Count > 0)
                    {
                        DataTable dtFTPIDs = dsMessagesToSend.Tables[3].DefaultView.ToTable(true, "FTPID");
                        for (int i = 0; i < dtFTPIDs.Rows.Count; i++)
                        {
                            DataTable dtMessagesToSend = dsMessagesToSend.Tables[3];
                            if (dtFTPIDs.Rows[i]["FTPID"].ToString().Trim() != string.Empty)
                            {
                                dtMessagesToSend.DefaultView.RowFilter = "FTPID = '" + dtFTPIDs.Rows[i]["FTPID"].ToString() + "'";
                                dtMessagesToSend = dtMessagesToSend.DefaultView.ToTable();
                                await _ftp.SFTPUpload(dtMessagesToSend);
                                for (int j = dsMessagesToSend.Tables[4].Rows.Count - 1; j >= 0; j--)
                                {
                                    if (dsMessagesToSend.Tables[3].Rows[j]["FTPID"].ToString().Trim() == dtFTPIDs.Rows[i]["FTPID"].ToString().Trim())
                                    {
                                        dsMessagesToSend.Tables[3].Rows[j].Delete();
                                    }
                                }
                                dsMessagesToSend.Tables[3].AcceptChanges();
                            }
                        }
                        if (dsMessagesToSend.Tables[3].Rows.Count > 0)
                        {
                            await _ftp.SFTPUpload(dsMessagesToSend.Tables[3]);
                        }
                    }
                    #endregion SFTP Upload

                    #region : DRIVE Upload :
                    if (dsMessagesToSend.Tables[4].Rows.Count > 0)
                    {
                        //AzureDrive azureDrive = new AzureDrive();
                        await _azureDrive.DRIVEUpload(dsMessagesToSend.Tables[4]);
                    }
                    #endregion DRIVE Upload

                    #region : Message Queue :
                    if (dsMessagesToSend.Tables[5].Rows.Count > 0)
                    {
                        DataTable dtMQPort = dsMessagesToSend.Tables[5].DefaultView.ToTable(true, "MQPort");
                        for (int i = 0; i < dtMQPort.Rows.Count; i++)
                        {
                            DataTable dtMessagesToSend = dsMessagesToSend.Tables[5];
                            if (dtMQPort.Rows[i]["FTPID"].ToString().Trim() != string.Empty)
                            {
                                dtMessagesToSend.DefaultView.RowFilter = "MQPort = '" + dtMQPort.Rows[i]["MQPort"].ToString() + "'";
                                dtMessagesToSend = dtMessagesToSend.DefaultView.ToTable();
                                await _ftp.SendMQMessage(dtMessagesToSend);
                                for (int j = dsMessagesToSend.Tables[5].Rows.Count - 1; j >= 0; j--)
                                {
                                    if (dsMessagesToSend.Tables[5].Rows[j]["FTPID"].ToString().Trim() == dtMQPort.Rows[i]["FTPID"].ToString().Trim())
                                    {
                                        dsMessagesToSend.Tables[5].Rows[j].Delete();
                                    }
                                }
                                dsMessagesToSend.Tables[5].AcceptChanges();
                            }
                        }
                        if (dsMessagesToSend.Tables[5].Rows.Count > 0)
                        {
                            await _ftp.SendMQMessage(dsMessagesToSend.Tables[5]);
                        }

                    }
                    #endregion Message Queue

                    #region : UFTP Upload :
                    if (dsMessagesToSend.Tables[6].Rows.Count > 0)
                    {

                    }
                    #endregion UFTP Upload
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numChars"></param>
        /// <returns></returns>
        //public string GetRandomNumbers(int numChars)
        //{
        //    string[] chars = {
        //                         "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "P", "Q", "R",
        //                         "S",
        //                         "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        //                     };
        //    var rnd = new Random();
        //    string random = string.Empty;
        //    for (int i = 0; i < numChars; i++)
        //    {
        //        random += chars[rnd.Next(0, 33)];
        //    }
        //    return random;
        //}

        public async Task ReadFromFTP()
        {
            //clsLog.WriteLogAzure("In ReadFromFTP()");
            //FTP _ftp = new FTP();
            //GenericFunction genericFunction = new GenericFunction();
            try
            {

                SIATFTP = ConfigCache.Get("msgService_IN_SITAFTP"); //ConfigurationSettings.AppSettings["SITAIN"].ToString();
                SITAUser = ConfigCache.Get("msgService_IN_SITAUser"); //ConfigurationSettings.AppSettings["SITAUser"].ToString();
                SITAPWD = ConfigCache.Get("msgService_IN_SITAPWD"); //ConfigurationSettings.AppSettings["SITAPWD"].ToString();
                ftpInMsgFolder = ConfigCache.Get("msgService_IN_SITAFolder");

                //Get list of files from FTP path.
                string[] strFiles = null;
                if (!string.IsNullOrEmpty(SIATFTP))
                {
                    if (!string.IsNullOrEmpty(ftpInMsgFolder))
                    {
                        strFiles = _ftp.FTPFilesList(SIATFTP + "/" + ftpInMsgFolder, SITAUser, SITAPWD);
                    }
                    else
                    {
                        strFiles = _ftp.FTPFilesList(SIATFTP, SITAUser, SITAPWD);
                    }
                }

                if (strFiles == null || strFiles.Length <= 0)
                {
                    return;
                }

                int filecount = 0;

                // clsLog.WriteLogAzure(" File count on FTP " + strFiles.Length);
                _logger.LogInformation(" File count on FTP " + strFiles.Length);

                foreach (string filename in strFiles.OrderBy(x => x))
                {

                    if (filename == null || filename == "")
                    {
                        continue;
                    }
                    else
                    {
                        filecount++;
                        if (filecount >= 10)
                            break;
                        //Read file contents.
                        string status = "Active";
                        // "01 Feb 2013 15:51:12"
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        string ftpFilePath = "";
                        if (SIATFTP.Trim().EndsWith("/"))
                        {
                            ftpFilePath = SIATFTP + filename;
                        }
                        else
                        {
                            ftpFilePath = SIATFTP + "/" + filename;
                        }
                        string strData = _ftp.ReadFTPFile(ftpFilePath, SITAUser, SITAPWD);
                        if (strData == null || strData.Contains("Error:") || strData.Length < 5)
                        {
                            ///Save and delete Error files
                            try
                            {
                                if (_ftp.SaveFTP(SIATFTP.Trim('/') + "/BackupMsgs", SITAUser, SITAPWD, filename.Replace('/', '-'), strData, "txt"))
                                {
                                    // clsLog.WriteLogAzure("FILE Backup Created on FTP:" + filename.Replace('/', '-'));
                                    _logger.LogInformation("FILE Backup Created on FTP:{0}", filename.Replace('/', '-'));
                                }
                            }
                            catch (Exception ex)
                            {
                                // clsLog.WriteLogAzure("Error in Saving File:" + filename.Replace('/', '-') + "-Error:" + ex.Message);
                                _logger.LogError("Error in Saving File:{0}-Error:{1}", filename.Replace('/', '-'), ex.Message);
                            }


                            if (_ftp.DeleteFTPFile(ftpFilePath, SITAUser, SITAPWD))
                            {
                                // clsLog.WriteLogAzure("File Deleted from FTP:" + filename);
                                _logger.LogInformation("File Deleted from FTP:{0}", filename);
                            }
                            continue;
                        }
                        else
                        {
                            if (!await StoreIROPSEmail("MSG:" + filename, strData, "FTP", "", dtRec, dtSend, "SITA", status, "FTP"))
                            {
                                // clsLog.WriteLogAzure("FTP file not saved:" + filename);
                                _logger.LogWarning("FTP file not saved:{0}", filename);
                            }

                            #region SAve the file before delete
                            try
                            {
                                if (_ftp.SaveFTP(SIATFTP.Trim('/') + "/BackupMsgs", SITAUser, SITAPWD, filename.Replace('/', '-'), strData, "txt"))
                                {
                                    // clsLog.WriteLogAzure("FILE Backup Created on FTP:" + filename.Replace('/', '-'));
                                    _logger.LogInformation("FILE Backup Created on FTP:{0}", filename.Replace('/', '-'));
                                }
                            }
                            catch (Exception ex)
                            {
                                // clsLog.WriteLogAzure("Error in Saving File:" + filename.Replace('/', '-') + "-Error:" + ex.Message);
                                _logger.LogError("Error in Saving File:{0}-Error:{1}", filename.Replace('/', '-'), ex.Message);
                            }
                            #endregion

                            if (_ftp.DeleteFTPFile(ftpFilePath, SITAUser, SITAPWD))
                            {
                                // clsLog.WriteLogAzure("File Deleted from FTP:" + filename);
                                _logger.LogInformation("File Deleted from FTP:{0}", filename);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task ReadFromALLFTP()
        {
            //FTP _ftp = new FTP();

            try
            {
                //SQLServer objsql = new SQLServer();
                //ds = objsql.SelectRecords("sp_MessageConfigurationIn", sqlParameter);

                DataSet? ds = null;
                SqlParameter[] sqlParameter = new SqlParameter[] {
                    new SqlParameter("@MsgCommType", "FTP")
                };
                ds = await _readWriteDao.SelectRecords("sp_MessageConfigurationIn", sqlParameter);

                //ftpInMsgFolder="IN";
                foreach (DataRow drMsg in ds.Tables[0].Rows)
                {
                    if (drMsg["MsgCommType"].ToString().ToUpper().Trim() != "FTP")
                    {
                        continue;
                    }

                    //FTP _ftp = new FTP();

                    ftpInMsgFolder = "";
                    SIATFTP = drMsg["FTPID"].ToString();
                    SITAUser = drMsg["FTPUserName"].ToString();
                    SITAPWD = drMsg["FTPPassword"].ToString();

                    string[] strFiles = null;
                    if (!string.IsNullOrEmpty(SIATFTP))
                    {
                        if (!string.IsNullOrEmpty(ftpInMsgFolder))
                        {
                            strFiles = _ftp.FTPFilesList(SIATFTP + "/" + ftpInMsgFolder, SITAUser, SITAPWD);
                        }
                        else
                        {
                            strFiles = _ftp.FTPFilesList(SIATFTP, SITAUser, SITAPWD);
                        }
                    }

                    if (strFiles == null || strFiles.Length <= 0)
                    {
                        return;
                    }

                    int filecount = 0;

                    // clsLog.WriteLogAzure(" File count on FTP " + strFiles.Length);
                    _logger.LogInformation(" File count on FTP {0}", strFiles.Length);

                    foreach (string filename in strFiles.OrderBy(x => x))
                    {

                        if (filename == null || filename == "")
                        {
                            continue;
                        }
                        else
                        {
                            filecount++;
                            if (filecount >= 10)
                                break;
                            //Read file contents.
                            string status = "Active";
                            // "01 Feb 2013 15:51:12"
                            DateTime dtRec = DateTime.Now;
                            DateTime dtSend = DateTime.Now;
                            string ftpFilePath = "";
                            if (SIATFTP.Trim().EndsWith("/"))
                            {
                                ftpFilePath = SIATFTP + filename;
                            }
                            else
                            {
                                ftpFilePath = SIATFTP + "/" + filename;
                            }
                            string strData = _ftp.ReadFTPFile(ftpFilePath, SITAUser, SITAPWD);
                            if (strData == null || strData.Contains("Error:") || strData.Length < 5)
                            {
                                continue;
                            }
                            else
                            {
                                if (!await StoreIROPSEmail("MSG:" + filename, strData, "FTP", "", dtRec, dtSend, "SITA", status, "FTP"))
                                {
                                    // clsLog.WriteLogAzure("FTP file not saved:" + filename.Replace('/', '-'));
                                    _logger.LogInformation("FTP file not saved:{0}", filename.Replace('/', '-'));
                                }
                                #region SAve the file before delete
                                try
                                {
                                    if (_ftp.SaveFTP(SIATFTP.Trim('/') + "/BackupMsgs", SITAUser, SITAPWD, filename.Replace('/', '-'), strData, "txt"))
                                    {
                                        // clsLog.WriteLogAzure("FILE Backup Created on FTP:" + filename.Replace('/', '-'));
                                        _logger.LogInformation("FILE Backup Created on FTP:{0}", filename.Replace('/', '-'));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure("Error in Saving File:" + filename.Replace('/', '-') + "-Error:" + ex.Message);
                                    _logger.LogError(ex, "Error in Saving File:{0}-Error:{1}", filename.Replace('/', '-'), ex.Message);
                                }
                                #endregion
                                if (_ftp.DeleteFTPFile(ftpFilePath, SITAUser, SITAPWD))
                                {
                                    // clsLog.WriteLogAzure("File Deleted from FTP:" + filename);
                                    _logger.LogInformation("File Deleted from FTP:{0}", filename);
                                }
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

        }

        public async Task RecieveMailfromIMAPServer(string sServer, string sUserName, string sPassword)
        {
            try
            {
                //TcpIMAP _tcpIMAP = new TcpIMAP();
                //imap.Connect(host, 993);

                _tcpIMAP.Connect(sServer, 143);
                _tcpIMAP.AuthenticateUser(sUserName, sPassword);

                //Console.WriteLine("Total Messages " + imap.MailCount());
                //Console.WriteLine("Total Unread Messages " + imap.MailUnreadCount());

                // You need to select the inbox in order to view the your messages
                _tcpIMAP.SelectInbox();

                // clsLog.WriteLogAzure("Server Connected..[" + DateTime.Now + "]");
                _logger.LogInformation("Server Connected..[{0}]", DateTime.Now);
                int Count = 0;
                Count = _tcpIMAP.MailCount();
                if (Count > 0)
                {
                    // clsLog.WriteLogAzure("Message Count:" + Count);
                    _logger.LogInformation("Message Count:{0}", Count);
                    for (int i = 0; i < Count; i++)
                    {
                        //try
                        //{
                        string strHeaders = _tcpIMAP.GetMessageHeaders(i).ToString().ToLower();
                        string status = "Active";
                        string Subject = "Message", fromEmail = "", toEmail = "", recievedDate = "";
                        if (strHeaders.Contains("from:") && strHeaders.Contains("subject:"))
                        {
                            string FromAdd = strHeaders.Substring(strHeaders.IndexOf("from:") + 5, strHeaders.IndexOf("subject:") - strHeaders.IndexOf("from:") - 5).Trim();
                        }
                        if (strHeaders.Contains("from:") && strHeaders.Contains("date:"))
                        {
                            string strDate = strHeaders.Substring(strHeaders.IndexOf("date:") + 5, strHeaders.IndexOf("from:") - strHeaders.IndexOf("date:") - 5).Trim();
                        }
                        if (strHeaders.Contains("subject:"))
                        {
                            string strSub = strHeaders.Substring(strHeaders.IndexOf("subject:")).Trim();
                        }
                        string MailBody = System.Web.HttpUtility.HtmlEncode(_tcpIMAP.GetMessage(i).ToString());
                        DateTime dtRec = DateTime.Now;
                        DateTime dtSend = DateTime.Now;
                        try
                        {
                            dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                            dtSend = dtRec;
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        try
                        {
                            if (await StoreIROPSEmail(Subject, MailBody, fromEmail, toEmail, dtRec, dtSend, Subject, status, "EMAIL"))
                            {
                                // clsLog.WriteLogAzure("Email " + (i + 1) + " Saved");
                                _logger.LogInformation("Email {0} Saved", (i + 1));
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                    }

                }

            }
            catch (MailServerException ep)
            {
                //Message contains the information returned by mail server
                // Console.WriteLine("Server Respond: {0}", ep.Message);
                _logger.LogError(ep, "Server Respond: {0}", ep.Message);
            }
            catch (SocketException ep)
            {
                // Console.WriteLine("Socket Error: {0}", ep.Message);
                _logger.LogError(ep, "Socket Error: {0}", ep.Message);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on RecieveMailfromIMAPServer");
            }


        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public async Task ReadFromSFTP()
        {
            // clsLog.WriteLogAzure("In FTPListener() => ReadFromSFTP()");
            _logger.LogInformation("In FTPListener() => ReadFromSFTP()");
            try
            {
                string FTPAddress = string.Empty, UserName = string.Empty, Password = string.Empty, remotePath = string.Empty, localPath = string.Empty
                    , fingerPrint = string.Empty, ppkFileName = string.Empty, ppkLocalFilePath = string.Empty, messageType = string.Empty, archivalPath = string.Empty;
                int portNumber = 0;

                //SQLServer objsql = new SQLServer();
                //ds = objsql.SelectRecords("sp_MessageConfigurationIn", sqlParameter);

                DataSet? ds = new DataSet("ds_SSIMFTPUpload");
                SqlParameter[] sqlParameter = new SqlParameter[] {
                    new SqlParameter("@MsgCommType", "SFTP")
                };
                ds = await _readWriteDao.SelectRecords("sp_MessageConfigurationIn", sqlParameter);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    //FTP FTPFuction = new FTP();

                    foreach (DataRow drow in ds.Tables[0].Rows)
                    {
                        if (drow["MsgCommType"].ToString() == "SFTP")
                        {
                            FTPAddress = drow["FTPID"].ToString();
                            UserName = drow["FTPUserName"].ToString();
                            Password = drow["FTPPassword"].ToString();
                            ppkFileName = drow["PPKFileName"].ToString();
                            remotePath = drow["RemotePath"].ToString();
                            localPath = drow["LocalPath"].ToString();
                            fingerPrint = drow["FingerPrint"].ToString();
                            messageType = drow["Messagetype"].ToString().Trim() == string.Empty ? "SITA" : drow["Messagetype"].ToString().Trim();
                            portNumber = drow["FingerPrint"].ToString() == string.Empty ? 0 : Convert.ToInt32(drow["PortNumber"].ToString());
                            archivalPath = drow["ArchivalPath"].ToString();
                            string TempPath = Path.GetTempPath();
                            if (ppkFileName != string.Empty)
                            {
                                //GenericFunction genericFunction = new GenericFunction();
                                ppkLocalFilePath = _genericFunction.GetPPKFilePath(ppkFileName);
                            }

                            if (await _ftp.SFTPDownload(FTPAddress, remotePath, Path.GetTempPath() + localPath, UserName, Password, fingerPrint, portNumber, ppkLocalFilePath, messageType, archivalPath))
                            {
                                // clsLog.WriteLogAzure("SFTP Downloaded successfully for SFTP Address: " + FTPAddress + " !! RemotePath : " + remotePath);
                                _logger.LogInformation($"SFTP Downloaded successfully for SFTP Address: {FTPAddress} !! RemotePath : {remotePath}");
                            }
                            else
                            {
                                // clsLog.WriteLogAzure("SFTP Download failed for SFTP Address: " + FTPAddress + " !! RemotePath : " + remotePath);
                                _logger.LogWarning("SFTP Download failed for SFTP Address: {0} !! RemotePath : {1}", FTPAddress, remotePath);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ReadFromSFTP");
            }
        }

        /*Not in use*/

        //public async Task SSIMFTPUpload()
        //{
        //    //clsLog.WriteLogAzure("In SSIMFTPUpload()");
        //    try
        //    {

        //        #region : Copy files to blob :
        //        //GenericFunction genericFunction = new GenericFunction();

        //        string sourcePath = Convert.ToString(ConfigCache.Get("sourcePathSSIM"));  //ConfigurationManager.AppSettings["sourcePathSSIM"].ToString();
        //        string containerName = GenericFunction.ContainerName.schedules.ToString();
        //        _genericFunction.MoveAllFilesToBlob(sourcePath, containerName);
        //        DirectoryInfo directory = new DirectoryInfo(sourcePath);
        //        FileInfo[] allFiles = directory.GetFiles();
        //        foreach (FileInfo file in allFiles)
        //        {
        //            if (await _genericFunction.IsFileExistOnBlob(file.Name, containerName))
        //            {
        //                DataSet dsSerialNumber = await _genericFunction.InsertMasterSummaryLog(0, file.Name, "SSIM", "", 0, 0,
        //                                                                                0, "", "", 0, BlobName, containerName,
        //                                                                                "", "", "", false);
        //                // clsLog.WriteLogAzure(file.Name + " : File uploaded Successfully");
        //                _logger.LogInformation($"{file.Name} : File uploaded Successfully");
        //            }
        //        }
        //        #endregion

        //        #region : Code commented to change the storage location of capacity file to blob :
        //        //string localPath = Path.GetTempPath() + "Schedules\\";
        //        //DirectoryInfo dir = new DirectoryInfo(localPath);
        //        //FileInfo[] files = dir.GetFiles();
        //        //foreach (FileInfo file in files)
        //        //{
        //        //    if (File.Exists(file.Directory.FullName + "\\" + file.Name))
        //        //    {
        //        //        string SSIM = File.ReadAllText(file.Directory.FullName + "\\" + file.Name);
        //        //        if (SSIM.Length > 0 && !SSIM.StartsWith("Error"))
        //        //        {
        //        //            clsLog.WriteLogAzure(file.Name + " : SSIM upload started!!");
        //        //            GetSSIMData(SSIM);
        //        //            clsLog.WriteLogAzure(file.Name + " : File deleted Successfully!!");
        //        //            file.Delete();
        //        //            return;
        //        //        }
        //        //        else
        //        //        {
        //        //            return;
        //        //        }
        //        //    }
        //        //    else
        //        //        return;
        //        //}
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        // clsLog.WriteLogAzure(ex);
        //        _logger.LogError(ex, "Error on SSIMFTPUpload");
        //    }
        //}

        public async Task GetSSIMData(string SSIMUpload)
        {
            try
            {
                //QID.DataAccess.SQLServer db = new SQLServer(); ;
                string[] DesignationCode = new string[0];
                string[] AirlinePrefix = new string[0];
                string[] ArrivalTimeZone = new string[0];
                string[] TailNo = new string[0];
                string[] Constant = new string[0];
                string[] Origin = new string[0];
                string[] Destination = new string[0];
                string[] AirCraftType = new string[0];
                string[] AirlineFrequency = new string[0];
                string[] DepartureTimeZone = new string[0];
                string[] Arrival = new string[0];
                string[] Departure = new string[0];
                string[] FilghtID = new string[0];
                string[] FlightNo = new string[0];
                string[] FromDate = new string[0];
                string[] ToDate = new string[0];
                string[] RowID = new string[0];
                string[] Frequency = new string[0];
                string[] FinalFrequency = new string[0];
                string[] AirFrequency = new string[0];
                string[] AirSchMasterFlightID = new string[0];
                string[] AirSchMasterOrigin = new string[0];
                string[] AirSchMasterDest = new string[0];
                string[] AirSchMasterArrivalTime = new string[0];
                string[] AirSchMasterDepartureTime = new string[0];
                string[] AirSchDeptTimeZone = new string[0];
                string[] AirSchArrTimeZone = new string[0];
                string[] AirSchFromDate = new string[0];
                string[] AirSchToDate = new string[0];
                string[] AirSchTailNo = new string[0];
                string[] AirSchAirCraftType = new string[0];
                string[] AirSchFrequency = new string[0];
                string[] AirSchAirlinePrefix = new string[0];
                string[] UTCDeptDay = new string[0];
                string[] UTCArrDay = new string[0];
                string[] AirSchUTCDeptDay = new string[0];
                string[] AirSchUTCArrDay = new string[0];
                string[] Iteneary = new string[0];
                string[] LegSequence = new string[0];
                string[] AirSchIteneary = new string[0];
                string[] AirSchLegSequence = new string[0];

                if (SSIMUpload.Length > 0)
                {
                    Stream Content = GenerateStreamFromString(SSIMUpload);
                    StreamReader sr = new StreamReader(Content);
                    int count = 0;
                    do
                    {
                        string s = sr.ReadLine();


                        if (s.StartsWith("3"))
                        {
                            count++;

                            Array.Resize(ref RowID, RowID.Length + 1);
                            RowID[RowID.Length - 1] = s.Substring(0, 1);
                            Array.Resize(ref Constant, Constant.Length + 1);
                            Constant[Constant.Length - 1] = s.Substring(6, 8);
                            Array.Resize(ref FromDate, FromDate.Length + 1);
                            FromDate[FromDate.Length - 1] = s.Substring(14, 7);

                            Array.Resize(ref ToDate, ToDate.Length + 1);
                            ToDate[ToDate.Length - 1] = s.Substring(21, 7);

                            Array.Resize(ref DesignationCode, DesignationCode.Length + 1);
                            DesignationCode[DesignationCode.Length - 1] = s.Substring(2, 3).Trim();
                            Array.Resize(ref AirlineFrequency, AirlineFrequency.Length + 1);
                            AirlineFrequency[AirlineFrequency.Length - 1] = s.Substring(28, 7);
                            Array.Resize(ref Origin, Origin.Length + 1);
                            Origin[Origin.Length - 1] = s.Substring(36, 3);
                            Array.Resize(ref Departure, Departure.Length + 1);
                            Departure[Departure.Length - 1] = s.Substring(39, 4);
                            Array.Resize(ref DepartureTimeZone, DepartureTimeZone.Length + 1);
                            DepartureTimeZone[DepartureTimeZone.Length - 1] = s.Substring(43, 9);
                            Array.Resize(ref Arrival, Arrival.Length + 1);
                            Array.Resize(ref Destination, Destination.Length + 1);
                            Destination[Destination.Length - 1] = s.Substring(54, 3);
                            Arrival[Arrival.Length - 1] = s.Substring(57, 4);
                            Array.Resize(ref ArrivalTimeZone, ArrivalTimeZone.Length + 1);
                            ArrivalTimeZone[ArrivalTimeZone.Length - 1] = s.Substring(61, 9);
                            Array.Resize(ref AirCraftType, AirCraftType.Length + 1);
                            AirCraftType[AirCraftType.Length - 1] = s.Substring(72, 3);
                            Array.Resize(ref AirlinePrefix, AirlinePrefix.Length + 1);
                            AirlinePrefix[AirlinePrefix.Length - 1] = s.Substring(2, 3).Trim();
                            Array.Resize(ref FlightNo, FlightNo.Length + 1);
                            FlightNo[FlightNo.Length - 1] = s.Substring(5, 4).Trim();
                            Array.Resize(ref TailNo, TailNo.Length + 1);
                            TailNo[TailNo.Length - 1] = s.Substring(172, 20).Trim();
                            Array.Resize(ref FilghtID, FilghtID.Length + 1);
                            FilghtID[FilghtID.Length - 1] = s.Substring(2, 3).Trim() + s.Substring(5, 4).Trim();
                            Array.Resize(ref UTCDeptDay, UTCDeptDay.Length + 1);
                            UTCDeptDay[UTCDeptDay.Length - 1] = s.Substring(192, 1).Trim();
                            Array.Resize(ref UTCArrDay, UTCArrDay.Length + 1);
                            UTCArrDay[UTCArrDay.Length - 1] = s.Substring(193, 1).Trim();
                            Array.Resize(ref Iteneary, Iteneary.Length + 1);
                            Iteneary[Iteneary.Length - 1] = s.Substring(9, 2).Trim();
                            Array.Resize(ref LegSequence, LegSequence.Length + 1);
                            LegSequence[LegSequence.Length - 1] = s.Substring(11, 2).Trim();

                        }

                    }
                    while (sr.Peek() != -1);
                    sr.Close();

                }

                for (int i = 0; i < AirlineFrequency.Length; i++)
                {
                    string zero = "";
                    if (AirlineFrequency[i].Contains<char>(' '))
                    {
                        zero = AirlineFrequency[i].Replace(" ", "0");
                    }
                    else
                        zero = AirlineFrequency[i];
                    Array.Resize(ref Frequency, Frequency.Length + 1);
                    Frequency[Frequency.Length - 1] = zero;
                }
                for (int i = 0; i < Frequency.Length; i++)
                {
                    char[] chararr = new char[0];

                    foreach (char ch in Frequency[i])
                    {
                        if (ch == '2' || ch == '3' || ch == '4' || ch == '5' || ch == '6' || ch == '7')
                        {
                            Array.Resize(ref chararr, chararr.Length + 1);
                            chararr[chararr.Length - 1] = '1';

                        }
                        else
                        {
                            Array.Resize(ref chararr, chararr.Length + 1);
                            chararr[chararr.Length - 1] = ch;
                        }


                    }
                    Array.Resize(ref FinalFrequency, FinalFrequency.Length + 1);
                    FinalFrequency[FinalFrequency.Length - 1] = new string(chararr);


                }
                char[] finalchar = new char[0];
                string FinalFrequencys = string.Empty;
                for (int j = 0; j < FinalFrequency.Length; j++)
                {

                    FinalFrequencys = FinalFrequency[j].Insert(1, ",");
                    FinalFrequencys = FinalFrequency[j].Insert(3, ",");
                    FinalFrequencys = FinalFrequency[j].Insert(5, ",");
                    FinalFrequencys = FinalFrequency[j].Insert(7, ",");
                    FinalFrequencys = FinalFrequency[j].Insert(9, ",");
                    FinalFrequencys = FinalFrequency[j].Insert(11, ",");
                    FinalFrequency[j] = FinalFrequencys;
                }
                for (int i = FilghtID.Length - 1; i >= 0; i--)
                {
                    if (FilghtID[i].Contains("      "))
                    {
                        //FilghtID[i] = FilghtID[i + 1];
                        //FlightNo[i] = FlightNo[i + 1];

                    }
                    else
                    {

                        Array.Resize(ref AirSchMasterDest, AirSchMasterDest.Length + 1);
                        AirSchMasterDest[AirSchMasterDest.Length - 1] = Destination[i];
                        Array.Resize(ref AirSchMasterArrivalTime, AirSchMasterArrivalTime.Length + 1);
                        AirSchMasterArrivalTime[AirSchMasterArrivalTime.Length - 1] = Arrival[i];
                        Array.Resize(ref AirSchArrTimeZone, AirSchArrTimeZone.Length + 1);
                        AirSchArrTimeZone[AirSchArrTimeZone.Length - 1] = ArrivalTimeZone[i];
                        Array.Resize(ref AirSchMasterFlightID, AirSchMasterFlightID.Length + 1);
                        AirSchMasterFlightID[AirSchMasterFlightID.Length - 1] = FilghtID[i];
                        Array.Resize(ref AirSchToDate, AirSchToDate.Length + 1);
                        AirSchToDate[AirSchToDate.Length - 1] = ToDate[i];
                        Array.Resize(ref AirSchAirlinePrefix, AirSchAirlinePrefix.Length + 1);
                        AirSchAirlinePrefix[AirSchAirlinePrefix.Length - 1] = AirlinePrefix[i];
                        Array.Resize(ref AirSchMasterOrigin, AirSchMasterOrigin.Length + 1);
                        AirSchMasterOrigin[AirSchMasterOrigin.Length - 1] = Origin[i];
                        Array.Resize(ref AirSchFromDate, AirSchFromDate.Length + 1);
                        AirSchFromDate[AirSchFromDate.Length - 1] = FromDate[i];
                        Array.Resize(ref AirSchTailNo, AirSchTailNo.Length + 1);
                        AirSchTailNo[AirSchTailNo.Length - 1] = TailNo[i];
                        Array.Resize(ref AirSchFrequency, AirSchFrequency.Length + 1);
                        AirSchFrequency[AirSchFrequency.Length - 1] = FinalFrequency[i];
                        Array.Resize(ref AirSchMasterDepartureTime, AirSchMasterDepartureTime.Length + 1);
                        AirSchMasterDepartureTime[AirSchMasterDepartureTime.Length - 1] = Departure[i];
                        Array.Resize(ref AirSchDeptTimeZone, AirSchDeptTimeZone.Length + 1);
                        AirSchDeptTimeZone[AirSchDeptTimeZone.Length - 1] = DepartureTimeZone[i];
                        Array.Resize(ref AirSchAirCraftType, AirSchAirCraftType.Length + 1);
                        AirSchAirCraftType[AirSchAirCraftType.Length - 1] = AirCraftType[i];
                        Array.Resize(ref AirSchUTCDeptDay, AirSchUTCDeptDay.Length + 1);
                        AirSchUTCDeptDay[AirSchUTCDeptDay.Length - 1] = UTCDeptDay[i];
                        Array.Resize(ref AirSchUTCArrDay, AirSchUTCArrDay.Length + 1);
                        AirSchUTCArrDay[AirSchUTCArrDay.Length - 1] = UTCArrDay[i];
                        Array.Resize(ref AirSchIteneary, AirSchIteneary.Length + 1);
                        AirSchIteneary[AirSchIteneary.Length - 1] = Iteneary[i];
                        Array.Resize(ref AirSchLegSequence, AirSchLegSequence.Length + 1);
                        AirSchLegSequence[AirSchLegSequence.Length - 1] = LegSequence[i];



                    }
                }

                #region Preparing Parameters for SSIM Export to Update Airline Schedule

                //string[] QueryNames = new string[17];
                //object[] QueryValues = new object[17];
                //SqlDbType[] QueryTypes = new SqlDbType[17];
                //string[] Names = new string[17];
                //object[] Values = new object[17];
                //SqlDbType[] Types = new SqlDbType[17];

                //for (int i = 0; i < AirSchMasterFlightID.Length; i++)
                //{
                //    QueryNames[0] = "FromDate";
                //    QueryNames[1] = "ToDate";
                //    QueryNames[2] = "FlightID";
                //    QueryNames[3] = "Source";
                //    QueryNames[4] = "Dest";
                //    QueryNames[5] = "ScheduleDepttime";
                //    QueryNames[6] = "SchArrtime";
                //    QueryNames[7] = "frequency";
                //    QueryNames[8] = "EquipmentNo";
                //    QueryNames[9] = "ArrTimeZone";
                //    QueryNames[10] = "DeptTimeZone";
                //    QueryNames[11] = "FlightPrefix";
                //    QueryNames[12] = "AircraftType";
                //    QueryNames[13] = "UTCDeptDay";
                //    QueryNames[14] = "UTCArrDay";
                //    QueryNames[15] = "Itinerary";
                //    QueryNames[16] = "LegSeqNo";

                //    QueryValues[0] = AirSchFromDate[i];
                //    QueryValues[1] = AirSchToDate[i];
                //    QueryValues[2] = AirSchMasterFlightID[i];
                //    QueryValues[3] = AirSchMasterOrigin[i];
                //    QueryValues[4] = AirSchMasterDest[i];
                //    QueryValues[5] = AirSchMasterDepartureTime[i];
                //    QueryValues[6] = AirSchMasterArrivalTime[i];
                //    QueryValues[7] = AirSchFrequency[i];
                //    QueryValues[8] = AirSchTailNo[i];
                //    QueryValues[9] = AirSchArrTimeZone[i];
                //    QueryValues[10] = AirSchDeptTimeZone[i];
                //    QueryValues[11] = AirSchAirlinePrefix[i];
                //    QueryValues[12] = AirSchAirCraftType[i];
                //    QueryValues[13] = AirSchUTCDeptDay[i];
                //    QueryValues[14] = AirSchUTCArrDay[i];
                //    QueryValues[15] = AirSchIteneary[i];
                //    QueryValues[16] = AirSchLegSequence[i];


                //    QueryTypes[0] = SqlDbType.VarChar;
                //    QueryTypes[1] = SqlDbType.VarChar;
                //    QueryTypes[2] = SqlDbType.VarChar;
                //    QueryTypes[3] = SqlDbType.VarChar;
                //    QueryTypes[4] = SqlDbType.VarChar;
                //    QueryTypes[5] = SqlDbType.VarChar;
                //    QueryTypes[6] = SqlDbType.VarChar;
                //    QueryTypes[7] = SqlDbType.VarChar;
                //    QueryTypes[8] = SqlDbType.VarChar;
                //    QueryTypes[9] = SqlDbType.VarChar;
                //    QueryTypes[10] = SqlDbType.VarChar;
                //    QueryTypes[11] = SqlDbType.VarChar;
                //    QueryTypes[12] = SqlDbType.VarChar;
                //    QueryTypes[13] = SqlDbType.VarChar;
                //    QueryTypes[14] = SqlDbType.VarChar;
                //    QueryTypes[15] = SqlDbType.VarChar;
                //    QueryTypes[16] = SqlDbType.VarChar;

                //    #region Preparing Parameters for SSIM Export to Update Airline Schedule

                //    bool val = db.InsertData("spSavePartnerSchedule_SSIM_ViaLogic_Partner", QueryNames, QueryTypes, QueryValues);
                //    if (!val)
                //    {
                //        clsLog.WriteLogAzure("SSIM Updating failed!");
                //        return;
                //    }
                //    #endregion
                //}

                // define your parameter names and types ONCE outside the loop
                string[] QueryNames =
                {
                    "FromDate", "ToDate", "FlightID", "Source", "Dest",
                    "ScheduleDepttime", "SchArrtime", "frequency", "EquipmentNo",
                    "ArrTimeZone", "DeptTimeZone", "FlightPrefix", "AircraftType",
                    "UTCDeptDay", "UTCArrDay", "Itinerary", "LegSeqNo"
                };

                SqlDbType[] QueryTypes = Enumerable.Repeat(SqlDbType.VarChar, QueryNames.Length).ToArray();

                // loop through all flights
                for (int i = 0; i < AirSchMasterFlightID.Length; i++)
                {
                    object[] QueryValues =
                    {
                        AirSchFromDate[i],
                        AirSchToDate[i],
                        AirSchMasterFlightID[i],
                        AirSchMasterOrigin[i],
                        AirSchMasterDest[i],
                        AirSchMasterDepartureTime[i],
                        AirSchMasterArrivalTime[i],
                        AirSchFrequency[i],
                        AirSchTailNo[i],
                        AirSchArrTimeZone[i],
                        AirSchDeptTimeZone[i],
                        AirSchAirlinePrefix[i],
                        AirSchAirCraftType[i],
                        AirSchUTCDeptDay[i],
                        AirSchUTCArrDay[i],
                        AirSchIteneary[i],
                        AirSchLegSequence[i]
                    };

                    // convert arrays to SqlParameter[]
                    SqlParameter[] parameters = QueryNames
                        .Select((name, index) => new SqlParameter("@" + name, QueryTypes[index])
                        {
                            Value = QueryValues[index] ?? DBNull.Value
                        })
                        .ToArray();

                    // execute your stored procedure
                    bool val = await _readWriteDao.ExecuteNonQueryAsync("spSavePartnerSchedule_SSIM_ViaLogic_Partner", parameters);

                    if (!val)
                    {
                        // clsLog.WriteLogAzure("SSIM Updating failed!");
                        _logger.LogInformation("SSIM Updating failed!");
                        return;
                    }
                }


                //db.ExecuteProcedure("sp_UpdateDatesSSIM");

                //To update the From Date & To Date for conflicting schedules
                bool dbRes = await _readWriteDao.ExecuteNonQueryAsync("sp_UpdateDatesSSIM");

                if (dbRes)
                {
                    // clsLog.WriteLogAzure("Schedule Uploaded Successfully!");
                    _logger.LogInformation("Schedule Uploaded Successfully!");
                }
                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on GetSSIMData");
            }

        }

        public Stream GenerateStreamFromString(string s)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Read MQ Message and save it to the inbox
        /// Added by prashantz
        /// </summary>
        public async Task ReceiveMQMessage()
        {
            DataSet? dsMQConfiguration = new DataSet();
            //SQLServer sqlServer = new SQLServer();

            try
            {
                //dsMQConfiguration = sqlServer.SelectRecords("uspGetMQConfiguration");

                dsMQConfiguration = await _readWriteDao.SelectRecords("uspGetMQConfiguration");

                if (dsMQConfiguration != null)
                {
                    if (dsMQConfiguration.Tables.Count > 0 && dsMQConfiguration.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsMQConfiguration.Tables[0].Rows.Count; i++)
                        {
                            DataRow drMQConfiguration = dsMQConfiguration.Tables[0].Rows[i];
                            string MessageBody = string.Empty;
                            string ErrorMessage = "";

                            string MQManager = drMQConfiguration["MQManager"].ToString();
                            string MQChannel = drMQConfiguration["MQChannel"].ToString();
                            string MQHost = drMQConfiguration["MQHost"].ToString();
                            string MQPort = drMQConfiguration["MQPort"].ToString();
                            string MQUser = drMQConfiguration["MQUser"].ToString();
                            string MQInqueue = "";
                            string MQOutqueue = drMQConfiguration["MQInQueue"].ToString();

                            if (MQManager.Trim() != string.Empty && MQChannel.Trim() != string.Empty && MQHost.Trim() != string.Empty && MQPort.Trim() != string.Empty && MQOutqueue.Trim() != string.Empty)
                            {
                                MQAdapter mqAdapter = new MQAdapter(MessagingType.ASync, MQManager, MQChannel, MQHost, MQPort, MQUser, MQInqueue, MQOutqueue, 0);

                                MessageBody = mqAdapter.ReadMessage(out ErrorMessage);

                                string Subject = "MQ Message";
                                string fromEmail = "";
                                string toEmail = "";
                                DateTime dtRec = DateTime.Now;
                                DateTime dtSend = DateTime.Now;
                                string MessageType = MessageData.MessageTypeName.SK2CS;
                                string status = string.Empty;

                                if (MessageBody.Trim() != string.Empty)
                                {
                                    if (await SaveMessage(Subject.ToUpper(), MessageBody.Trim(), fromEmail, toEmail, dtRec, dtSend, MessageType, status, "Message Queue", "", "", Convert.ToDateTime("1900-01-01 00:00:00.000")))
                                    {
                                        //clsLog.WriteLogAzure("MQMessage saved successfully in to inbox : " + DateTime.Now);
                                    }
                                    else
                                    {
                                        // clsLog.WriteLogAzure("Fail to save MQMessage : " + DateTime.Now);
                                        _logger.LogWarning("Fail to save MQMessage : {0}", DateTime.Now);
                                    }
                                }
                                mqAdapter.DisposeQueue();
                            }
                            else
                            {
                                // clsLog.WriteLogAzure("Info : In(ReceiveMQMessage() method):Insufficient MQ Message Configuration");
                                _logger.LogWarning("Info : In(ReceiveMQMessage() method):Insufficient MQ Message Configuration");
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, "Error on ReceiveMQMessage");
            }
        }

        /// <summary>
        /// Save message to the 'tblInbox' table 
        /// Added by prashantz
        /// </summary>
        public async Task<bool> SaveMessage(string subject, string body, string fromId, string toId, DateTime recievedOn, DateTime sendOn, string type, string status, string CommunicationType, string awbNumber, string flightNo, DateTime flightDate)
        {
            bool flag = false;
            DataSet? dsResult = new DataSet();
            //SQLServer sqlServer = new SQLServer();
            try
            {

                //dsResult = sqlServer.SelectRecords("spSavetoInbox", sqlParameter);

                ///Paramters are changes by prashantz
                SqlParameter[] sqlParameter = new SqlParameter[] {
                     new SqlParameter("@subject",subject)
                    ,new SqlParameter("@body",body)
                    ,new SqlParameter("@fromId",fromId)
                    ,new SqlParameter("@toId",toId)
                    ,new SqlParameter("@recievedOn",recievedOn)
                    ,new SqlParameter("@sendOn",sendOn)
                    ,new SqlParameter("@type",type)
                    ,new SqlParameter("@status",status)
                    ,new SqlParameter("@CommunicationType",CommunicationType)
                    ,new SqlParameter("@AWBNumber",awbNumber)
                    ,new SqlParameter("@FlightNo",flightNo)
                    ,new SqlParameter("@FlightDate",flightDate)
                };

                dsResult = await _readWriteDao.SelectRecords("spSavetoInbox", sqlParameter);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                    if (Convert.ToInt32(dsResult.Tables[0].Rows[0][0].ToString()) > 0)
                        flag = true;
                    else
                        flag = false;
                else
                    flag = false;

                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }
        public static string DecodeQuotedPrintables(string input)
        {
            try
            {
                var occurences = new Regex(@"=[0-9A-H]{2}", RegexOptions.Multiline);
                var matches = occurences.Matches(input);
                foreach (Match match in matches)
                {
                    char hexChar = (char)Convert.ToInt32(match.Groups[0].Value.Substring(1), 16);
                    input = input.Replace(match.Groups[0].Value, hexChar.ToString());
                }
                return input.Replace("=\r\n", "");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _staticLogger?.LogError(ex, $"Error on {MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }


        #endregion

        #region :: Internal Methods ::
        internal async Task SyncTblueData()
        {
            try
            {
                //SQLServer db = new SQLServer(); ;
                //db.ExecuteProcedure("sp_UpdateBudgetSummary");
                //db.ExecuteProcedure("sp_BIUpdateBookingTransaction");
                //db.ExecuteProcedure("sp_BI_UpdateBookingSummary");

                await _readWriteDao.SelectRecords("sp_UpdateBudgetSummary");
                await _readWriteDao.SelectRecords("sp_BIUpdateBookingTransaction");
                await _readWriteDao.SelectRecords("sp_BIUpdateBookingTransaction");

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        #endregion

        #region :: Private Methods ::
        private async Task CheckMessagesforProcessing()
        {
            try
            {
                //SQLServer db = new SQLServer();
                //ds = db.SelectRecords("spGetMessageForInsert");

                DataSet? ds = null;
                string status = "Re-Processed", MessageFrom = string.Empty;


                ds = await _readWriteDao.SelectRecords("spGetMessageForInsert");

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
                    {
                        int srno = Convert.ToInt32(ds.Tables[0].Rows[row][0].ToString());
                        string message = ds.Tables[0].Rows[row]["body"].ToString();
                        string strFromID = ds.Tables[0].Rows[row]["FromiD"].ToString();
                        string strStatus = ds.Tables[0].Rows[row]["STATUS"].ToString();
                        string msgDeliveryType = ds.Tables[0].Rows[row]["msgDeliveryType"].ToString();
                        string PIMAAddress = string.Empty;

                        try
                        {
                            if ((message.Contains("=ORIGIN")))
                            {
                                int indexOfMessageTag = message.IndexOf("=ORIGIN");
                                MessageFrom = message.Substring(indexOfMessageTag);
                                MessageFrom = MessageFrom.Substring(8, 8).ToString();
                            }
                            else
                            {
                                MessageFrom = ds.Tables[0].Rows[row]["UpdatedBy"].ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            MessageFrom = string.Empty;
                        }


                        message = RemoveBinaryData(message);

                        if (message.Contains("ZCZC") && message.Contains("NNNN"))
                        {
                            message = ExtractFromString(message, "ZCZC", "NNNN");
                        }
                        if (message.Contains("=SMI"))
                        {
                            message = ExtractFromString(message, "=SMI", "");
                        }
                        else if (message.Contains("=TEXT"))
                        {
                            message = ExtractFromString(message, "=TEXT", "");
                        }


                        else if (message.StartsWith("QK ") || message.StartsWith("\r\nQK ") || message.StartsWith("QN ") || message.StartsWith("\r\nQN "))
                        {
                            string PIMAString = message.Replace("\r\n", "$");
                            PIMAString = PIMAString.Replace("\n", "$");
                            string[] lines = PIMAString.Split('$');
                            string[] sublines = new string[0];
                            for (int i = 0; i < lines.Length; i++)
                                if (lines[i].StartsWith("."))
                                {
                                    sublines = lines.Length > 0 && lines[i].StartsWith(".") ? lines[i].Split(' ') : null;
                                    break;
                                }
                            MessageFrom = sublines.Length > 0 ? sublines[0].Replace(".", string.Empty) : MessageFrom;
                            PIMAAddress = sublines.Length > 2 ? sublines[2] : string.Empty;
                        }
                        message = ReplaceBlankSpaces(message);


                        string msgType = string.Empty;
                        //cls_SCMBL clscmbl = new cls_SCMBL();
                        string origmsg = RemoveSITAHeader(message);
                        string Errmsg = string.Empty;

                        if ((msgDeliveryType.ToUpper() == "COMPOSED" || strStatus.ToUpper() == "PROCESSED" || strStatus.ToUpper() == "RE-PROCESSED") && !MessageFrom.Contains("@"))
                        {
                            MessageFrom = string.Empty;
                            strFromID = string.Empty;
                        }

                        //if (!clscmbl.addBookingFromMsg(origmsg, srno, MessageFrom, out msgType, strFromID, strStatus, PIMAAddress, out Errmsg))
                        bool success = false;
                        (success, msgType, Errmsg) = await _cls_SCMBL.addBookingFromMsg(origmsg, srno, MessageFrom, strFromID, strStatus, PIMAAddress);
                        if (!success)
                        {
                            status = "Failed";
                        }

                        if (ds.Tables[0].Rows[row]["STATUS"].ToString().Equals("Failed", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) || ds.Tables[0].Rows[0]["STATUS"].ToString().Length < 1)
                            status = "Processed";
                        if (ds.Tables[0].Rows[row]["STATUS"].ToString().Equals("Processed", StringComparison.OrdinalIgnoreCase))
                            status = "Re-Processed";

                        if (!string.IsNullOrEmpty(Errmsg) && Errmsg.Contains("incorrect ISU format"))
                        {
                            status = "Failed";
                        }
                        else if (!string.IsNullOrEmpty(Errmsg))
                        {
                            status = "Failed";
                            Errmsg = Errmsg.Replace("Input string was not in a correct format.", "");
                        }

                        //string[] PName = new string[] { "srno", "status", "body", "PIMAddress", "FromID", "MSGBody", "Error" }; //  In sp body will update to Mssage Type (Cond is handeled in DB)
                        //object[] PValues = new object[] { Convert.ToInt32(ds.Tables[0].Rows[row][0].ToString()), status, msgType, PIMAAddress, MessageFrom, origmsg.Trim(), Errmsg };
                        //SqlDbType[] PType = new SqlDbType[] { SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                        //if (!db.ExecuteProcedure("spUpdateMessageStatus", PName, PType, PValues))
                        //{
                        //    clsLog.WriteLogAzure("Error Status Update:" + ds.Tables[0].Rows[row][0].ToString());
                        //}

                        SqlParameter[] parameters =
                        {
                            new SqlParameter("@srno", SqlDbType.Int) { Value = Convert.ToInt32(ds.Tables[0].Rows[row][0].ToString()) },
                            new SqlParameter("@status", SqlDbType.VarChar) { Value = status },
                            new SqlParameter("@body", SqlDbType.VarChar) { Value = msgType },
                            new SqlParameter("@PIMAddress", SqlDbType.VarChar) { Value = PIMAAddress },
                            new SqlParameter("@FromID", SqlDbType.VarChar) { Value = MessageFrom },
                            new SqlParameter("@MSGBody", SqlDbType.VarChar) { Value = origmsg.Trim() },
                            new SqlParameter("@Error", SqlDbType.VarChar) { Value = Errmsg }
                        };

                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spUpdateMessageStatus", parameters);
                        if (!dbRes)
                        {
                            // clsLog.WriteLogAzure("Error Status Update:" + ds.Tables[0].Rows[row][0].ToString());
                            _logger.LogInformation("Error Status Update: {0}", ds.Tables[0].Rows[row][0]);
                        }
                    }
                }

                //db = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task sendmail_V1()
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                string OutEmailPassword = ConfigCache.Get("msgService_OutEmailPassword");
                string OutEmailId = ConfigCache.Get("msgService_OutEmailId");
                string OutEmailServer = ConfigCache.Get("msgService_EmailOutServer");
                int outport = int.Parse(Convert.ToString(ConfigCache.Get("OutPort")));  //int.Parse(ConfigurationManager.AppSettings["OutPort"].ToString());
                bool isOn = false;
                bool ishtml = false;
                //SQLServer objsql = new SQLServer();
                do
                {
                    //DataSet ds = null;
                    //ds = objsql.SelectRecords("spMailtoSend");

                    isOn = false;
                    DataSet? ds = await _readWriteDao.SelectRecords("spMailtoSend");

                    if (ds != null)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                isOn = true;
                                bool isMessageSent = false;
                                DataRow dr = ds.Tables[0].Rows[0];
                                string subject = dr[1].ToString();
                                string msgCommType = "EMAIL";
                                DataRow drMsg = null;
                                string body = dr[2].ToString();
                                string sentadd = dr[4].ToString().Trim(',');
                                string ccadd = "";
                                if (dr[3].ToString().Length > 3)
                                {
                                    ccadd = dr[3].ToString().Trim(',');
                                }
                                try
                                {
                                    ishtml = bool.Parse(dr["ishtml"].ToString());
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    ishtml = false;
                                }

                                if (ds.Tables[2].Rows.Count > 0)
                                {
                                    drMsg = ds.Tables[2].Rows[0];
                                    msgCommType = drMsg["MsgCommType"].ToString().ToUpper().Trim();
                                }
                                if (msgCommType == "FTP" || msgCommType == "ALL")
                                {
                                    try
                                    {
                                        isMessageSent = true;
                                        //FTP _ftp = new FTP();
                                        if (drMsg != null)
                                        {
                                            _ftp.SaveFTP(drMsg["FTPID"].ToString(), drMsg["FTPUserName"].ToString(), drMsg["FTPPassword"].ToString(), body, DateTime.Now.ToString("yyyyMMdd_hhmmss_fff"));
                                        }

                                        //string pname = "num";
                                        //object pvalue = int.Parse(dr[0].ToString());
                                        //SqlDbType ptype = SqlDbType.Int;
                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                        //{
                                        //    clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
                                        //}

                                        SqlParameter[] parameters =
                                        {
                                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                        };

                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                        if (dbRes)
                                        {
                                            // clsLog.WriteLogAzure("uploaded on ftp successfully to:" + dr[0].ToString());
                                            _logger.LogInformation("uploaded on ftp successfully to: {0}", dr[0]);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        isMessageSent = false;
                                    }
                                }

                                if (msgCommType == "EMAIL" || msgCommType == "ALL")
                                {
                                    try
                                    {
                                        // isMessageSent = true;
                                        #region Email
                                        //EMAILOUT objmail = new EMAILOUT();
                                        if (sentadd.Length > 2 && sentadd.Contains("@") && sentadd.Contains("."))
                                        {
                                            isMessageSent = true;
                                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                                            {
                                                #region Mail with attachment
                                                // clsLog.WriteLogAzure("Inside Table 1 for MessageID :" + dr[0].ToString());
                                                _logger.LogInformation("Inside Table 1 for MessageID : {0}", dr[0]);
                                                if (ds.Tables[1].Rows.Count > 0)
                                                {
                                                    // clsLog.WriteLogAzure("Inside Table 1 Rows for MessageID :" + dr[0].ToString());
                                                    _logger.LogInformation("Inside Table 1 Rows for MessageID : {0}", dr[0]);
                                                    MemoryStream[] Attachments = new MemoryStream[0];
                                                    string[] Extensions = new string[0];
                                                    string[] AttachmentName = new string[0];
                                                    foreach (DataRow drow in ds.Tables[1].Rows)
                                                    {
                                                        try
                                                        {
                                                            Array.Resize(ref Attachments, Attachments.Length + 1);
                                                            Attachments[Attachments.Length - 1] = new MemoryStream(DownloadBlob(drow["FileUrl"].ToString()));

                                                            Array.Resize(ref Extensions, Extensions.Length + 1);
                                                            Extensions[Extensions.Length - 1] = drow["MIMEType"].ToString();
                                                            Array.Resize(ref AttachmentName, AttachmentName.Length + 1);
                                                            AttachmentName[AttachmentName.Length - 1] = drow["AttachmentName"].ToString();
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            // clsLog.WriteLogAzure(ex);
                                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }

                                                    }
                                                    if (_emailOut.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, Attachments, AttachmentName, Extensions))
                                                    {
                                                        // clsLog.WriteLogAzure("After Sending Mail with Attachment for MessageID :" + dr[0].ToString());
                                                        _logger.LogInformation("After Sending Mail with Attachment for MessageID : {0}", dr[0]);
                                                        //string pname = "num";
                                                        //object pvalue = int.Parse(dr[0].ToString());
                                                        //SqlDbType ptype = SqlDbType.Int;
                                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                        //{
                                                        //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                        //}

                                                        SqlParameter[] parameters =
                                                        {
                                                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                        };

                                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                        if (dbRes)
                                                        {
                                                            // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                            _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                                        _logger.LogWarning("Error in EmailOut for SRNO::{0}", dr[0]);

                                                    }

                                                }

                                                #endregion Mail with attachment
                                            }
                                            else
                                            {
                                                #region Send Email
                                                if (_emailOut.sendMail(accountEmail, sentadd, password, subject, body, ishtml, outport, ccadd, ""))
                                                {
                                                    //string pname = "num";
                                                    //object pvalue = int.Parse(dr[0].ToString());
                                                    //SqlDbType ptype = SqlDbType.Int;
                                                    //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                                    //{
                                                    //    clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                    //}

                                                    SqlParameter[] parameters =
                                                    {
                                                        new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                                    };

                                                    var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                                    if (dbRes)
                                                    {
                                                        // clsLog.WriteLogAzure("Email Sent successfully to:" + dr[0].ToString());
                                                        _logger.LogInformation("Email Sent successfully to: {0}", dr[0]);
                                                    }
                                                }
                                                else
                                                {
                                                    // clsLog.WriteLogAzure("Error in EmailOut for SRNO::" + dr[0].ToString());
                                                    _logger.LogWarning("Error in EmailOut for SRNO:: {0}", dr[0]);

                                                }
                                                #endregion send email
                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception)
                                    {
                                        isMessageSent = false;
                                    }
                                }

                                if (!isMessageSent)
                                {
                                    try
                                    {
                                        //string[] pname = { "num", "ErrorMsg" };
                                        //object[] pvalue = { int.Parse(dr[0].ToString()), "Outgoing FTP or emaild not configured." };
                                        //SqlDbType[] ptype = { SqlDbType.Int, SqlDbType.VarChar };
                                        //if (objsql.ExecuteProcedure("spMailSent", pname, ptype, pvalue))
                                        //{
                                        //    clsLog.WriteLogAzure("Fail to sent Email to:" + dr[0].ToString());
                                        //}

                                        SqlParameter[] parameters =
                                        {
                                            new SqlParameter("@num", SqlDbType.Int) { Value = int.Parse(dr[0].ToString()) },
                                            new SqlParameter("@ErrorMsg", SqlDbType.VarChar) { Value = "Outgoing FTP or email not configured." }
                                        };
                                        var dbRes = await _readWriteDao.ExecuteNonQueryAsync("spMailSent", parameters);
                                        if (dbRes)
                                        {
                                            // clsLog.WriteLogAzure("Fail to sent Email to:" + dr[0].ToString());
                                            _logger.LogInformation("Fail to sent Email to:{0}", dr[0]);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                else
                                    isOn = false;
                            }
                            else
                                isOn = false;
                        }
                        else
                            isOn = false;
                    }
                    else
                        isOn = false;

                    Thread.Sleep(1000);

                } while (isOn);

                //objsql = null;
                //GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private string getStorageKey()
        {
            //String Key = "NUro8/C7+kMqtwOwLbe6agUvA83s+8xSTBqrkMwSjPP6MAxVkdtsLDGjyfyEqQIPv6JHEEf5F5s4a+DFPsSQfg==";
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                if (string.IsNullOrEmpty(BlobKey))
                {
                    BlobKey = ConfigCache.Get("BlobStorageKey");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return BlobKey;
        }

        private string getStorageName()
        {
            try
            {
                //GenericFunction genericFunction = new GenericFunction();
                if (string.IsNullOrEmpty(BlobName))
                {
                    BlobName = ConfigCache.Get("BlobStorageName");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return BlobName;
        }

        /*Not in use*/
        //private bool setSMSFlag(string SrNo)
        //{
        //    bool flag = false;
        //    try
        //    {
        //        SQLServer objSQL = new SQLServer();
        //        string procedure = "spSetSMSProcessFlag";
        //        flag = objSQL.UpdateData(procedure, "SrNo", SqlDbType.BigInt, SrNo);
        //        objSQL = null;
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //    return flag;
        //}

        private bool SendSMS(string MobileNo, string Message)
        {
            bool flag = false;
            string smsUID = ConfigCache.Get("SMSUN");
            string smspswd = ConfigCache.Get("SMSPASS");
            try
            {
                if (MobileNo.Trim().Length > 0)
                {
                    string[] strArrMobNo = MobileNo.Split(',');
                    if (strArrMobNo.Length > 0)
                    {
                        for (int i = 0; i < strArrMobNo.Length; i++)
                        {
                            //SMSOUT sendSMS = new SMSOUT();

                            flag = _sMSOUT.sendSMS(strArrMobNo[i].Trim(), Message, smsUID, smspswd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        private async Task AutoGenerateMessages()
        {
            try
            {
                DataSet? dsFlightsDetail = new DataSet();
                //SQLServer db = new SQLServer();

                //GenericFunction genericFunction = new GenericFunction();

                string f_AutoFBL = ConfigCache.Get("AutoFBL");
                string autoFWBFHL = ConfigCache.Get("AutoForwardFWBFHL");

                DateTime lastFBLSentOn = System.DateTime.UtcNow;

                if (f_AutoFBL.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    #region : FBL to SFTP :

                    //dsFlightsDetail = db.SelectRecords("Messaging.uspGetFlightsForFBL", sqlParameter);

                    SqlParameter[] sqlParameter = new SqlParameter[] {
                        new SqlParameter("SendFBLToSFTP", "true")
                    };
                    dsFlightsDetail = await _readWriteDao.SelectRecords("Messaging.uspGetFlightsForFBL", sqlParameter);

                    // clsLog.WriteLogAzure("FBLTOSFTP BBB");
                    _logger.LogInformation("FBLTOSFTP BBB");
                    if (dsFlightsDetail != null && dsFlightsDetail.Tables.Count > 0 && dsFlightsDetail.Tables[0].Rows.Count > 0)
                    {
                        // clsLog.WriteLogAzure("Total flights for FBL: " + dsFlightsDetail.Tables[0].Rows.Count.ToString());
                        _logger.LogInformation("Total flights for FBL: {0}", dsFlightsDetail.Tables[0].Rows.Count);


                        //FBLMessageProcessor fblMessageProcessor = new FBLMessageProcessor();

                        for (int i = 0; i < dsFlightsDetail.Tables[0].Rows.Count; i++)
                        {
                            DataRow drFlightsDetail = dsFlightsDetail.Tables[0].Rows[i];

                            bool isAutoSendOnTriggerTime = drFlightsDetail["IsAutoSendOnTriggerTime"].ToString() == "" ? false
                                    : Convert.ToBoolean(drFlightsDetail["IsAutoSendOnTriggerTime"].ToString());

                            if (dsFlightsDetail.Tables[0].Rows[i]["UTCLastFBLSent"].ToString().Trim() != string.Empty)
                                lastFBLSentOn = Convert.ToDateTime(dsFlightsDetail.Tables[0].Rows[i]["UTCLastFBLSent"].ToString());

                            string messageType = drFlightsDetail["MessageType"].ToString() == "" ? "FBL"
                                    : drFlightsDetail["MessageType"].ToString();

                            if (f_AutoFBL.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                            {
                                //await Task.Run(() => fblMessageProcessor.GenerateFBLMessage(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                                //                                    , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), isAutoSendOnTriggerTime, messageType));

                                await _fBLMessageProcessor.GenerateFBLMessage(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                                    , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), isAutoSendOnTriggerTime, messageType);
                            }

                        }
                    }

                    #endregion FBL to SFTP

                    #region : Auto FBL :

                    //SQLServer sqlServer = new SQLServer();
                    //dsFlightsDetail = sqlServer.SelectRecords("Messaging.uspGetFlightsForFBL");

                    dsFlightsDetail = null;
                    dsFlightsDetail = await _readWriteDao.SelectRecords("Messaging.uspGetFlightsForFBL", sqlParameter);

                    if (dsFlightsDetail != null && dsFlightsDetail.Tables.Count > 0 && dsFlightsDetail.Tables[0].Rows.Count > 0)
                    {
                        // clsLog.WriteLogAzure("Total flights for FBL: " + dsFlightsDetail.Tables[0].Rows.Count.ToString());
                        _logger.LogInformation("Total flights for FBL: {0}", dsFlightsDetail.Tables[0].Rows.Count);

                        //FBLMessageProcessor fblMessageProcessor = new FBLMessageProcessor();

                        for (int i = 0; i < dsFlightsDetail.Tables[0].Rows.Count; i++)
                        {
                            DataRow drFlightsDetail = dsFlightsDetail.Tables[0].Rows[i];

                            bool isAutoSendOnTriggerTime = drFlightsDetail["IsAutoSendOnTriggerTime"].ToString() == "" ? false
                                    : Convert.ToBoolean(drFlightsDetail["IsAutoSendOnTriggerTime"].ToString());

                            if (dsFlightsDetail.Tables[0].Rows[i]["UTCLastFBLSent"].ToString().Trim() != string.Empty)
                                lastFBLSentOn = Convert.ToDateTime(dsFlightsDetail.Tables[0].Rows[i]["UTCLastFBLSent"].ToString());

                            string messageType = drFlightsDetail["MessageType"].ToString() == "" ? "FBL"
                                    : drFlightsDetail["MessageType"].ToString();

                            if (f_AutoFBL.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                            {
                                //await Task.Run(() => fblMessageProcessor.GenerateFBLMessage(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                                //   , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), isAutoSendOnTriggerTime, messageType));

                                await _fBLMessageProcessor.GenerateFBLMessage(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                                   , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), isAutoSendOnTriggerTime, messageType);
                            }
                        }

                    }

                    #endregion Auto FBL
                }

                if (autoFWBFHL.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    #region : Auto FWB/FHL :

                    //SQLServer sqlServer1 = new SQLServer();
                    //dsFlightsDetail = sqlServer1.SelectRecords("Messaging.uspGetFlightsForFWBFHL");

                    dsFlightsDetail = null;
                    dsFlightsDetail = await _readWriteDao.SelectRecords("Messaging.uspGetFlightsForFWBFHL");

                    if (dsFlightsDetail != null && dsFlightsDetail.Tables.Count > 0 && dsFlightsDetail.Tables[0].Rows.Count > 0)
                    {
                        // clsLog.WriteLogAzure("Total flights for FBL: " + dsFlightsDetail.Tables[0].Rows.Count.ToString());
                        _logger.LogInformation("Total flights for FBL: {0}", dsFlightsDetail.Tables[0].Rows.Count);

                        //FWBMessageProcessor fwbMessageProcessor = new FWBMessageProcessor();
                        //FHLMessageProcessor fhlMessageProcessor = new FHLMessageProcessor();

                        for (int i = 0; i < dsFlightsDetail.Tables[0].Rows.Count; i++)
                        {
                            DataRow drFlightsDetail = dsFlightsDetail.Tables[0].Rows[i];

                            bool isAutoSendOnTriggerTime = drFlightsDetail["IsAutoSendOnTriggerTime"].ToString() == "" ? false
                                    : Convert.ToBoolean(drFlightsDetail["IsAutoSendOnTriggerTime"].ToString());

                            if (dsFlightsDetail.Tables[0].Rows[i]["LastFWBFHLSentOn"].ToString().Trim() != string.Empty)
                                lastFBLSentOn = Convert.ToDateTime(dsFlightsDetail.Tables[0].Rows[i]["UTCLastFWBFHLSent"].ToString());

                            string messageType = drFlightsDetail["MessageType"].ToString() == "" ? "FBL"
                                    : drFlightsDetail["MessageType"].ToString();

                            //await Task.Run(() => fwbMessageProcessor.GenerateFWB(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                            //    , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), System.DateTime.UtcNow, lastFBLSentOn, isAutoSendOnTriggerTime));

                            //await Task.Run(() => fhlMessageProcessor.GenerateFHL(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                            //    , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), System.DateTime.UtcNow, lastFBLSentOn, isAutoSendOnTriggerTime));

                            await _fWBMessageProcessor.GenerateFWB(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                               , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), System.DateTime.UtcNow, lastFBLSentOn, isAutoSendOnTriggerTime);

                            await _fHLMessageProcessor.GenerateFHL(drFlightsDetail["Source"].ToString(), drFlightsDetail["Dest"].ToString()
                                , drFlightsDetail["FlightID"].ToString(), drFlightsDetail["Date"].ToString(), System.DateTime.UtcNow, lastFBLSentOn, isAutoSendOnTriggerTime);
                        }
                    }

                    #endregion Auto FWB/FHL
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task AutoSendXFSUMessages()
        {
            //SQLServer sqlServer = new SQLServer();
            DataSet? dsXFSUData = new DataSet();
            //GenericFunction genericFunction = new GenericFunction();
            try
            {
                //if (!string.IsNullOrEmpty(genericFunction.GetConfigurationValues("AutoSendXFSUMessages")) && genericFunction.GetConfigurationValues("AutoSendXFSUMessages").Equals("True", StringComparison.OrdinalIgnoreCase))
                if (!string.IsNullOrEmpty(ConfigCache.Get("AutoSendXFSUMessages")) && ConfigCache.Get("AutoSendXFSUMessages").Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    //dsXFSUData = sqlServer.SelectRecords("Messaging.GetRecordforMakeXFSUMessage");
                    dsXFSUData = await _readWriteDao.SelectRecords("Messaging.GetRecordforMakeXFSUMessage");

                    if (dsXFSUData != null)
                    {
                        if (dsXFSUData.Tables.Count > 0)
                        {
                            string Toid = "";
                            // clsLog.WriteLogAzure("XFSU Messages Count: " + dsXFSUData.Tables.Count.ToString());
                            _logger.LogInformation("XFSU Messages Count: {0}", dsXFSUData.Tables.Count);
                            if (dsXFSUData.Tables[0].Rows.Count > 0 && dsXFSUData.Tables[1].Rows.Count > 0)
                            {
                                //XFSUMessageProcessor fsuMessage = new XFSUMessageProcessor();

                                string xfsuMessage = string.Empty, strEmailid = "", lblMsgCommType = "", messageVersion = "", sitaHeaderType = "", SitaMessageHeader = "", xffrMessage = string.Empty, xfwbMessage = string.Empty;
                                bool flag = false;
                                for (int i = 0; i < dsXFSUData.Tables[0].Rows.Count; i++)
                                {
                                    string sno = dsXFSUData.Tables[0].Rows[i]["SNo"].ToString();
                                    string messageType = dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString();
                                    DataView dvToAdress = new DataView(dsXFSUData.Tables[1]);
                                    dvToAdress.RowFilter = "SNo = " + sno;
                                    DataTable dtToAdress = dvToAdress.ToTable();

                                    //await Task.Run(() => xfsuMessage = fsuMessage.GenerateXFSUMessageofTheAWBV3(dsXFSUData.Tables[0].Rows[i]["AWBPrefix"].ToString(), dsXFSUData.Tables[0].Rows[i]["AWBNo"].ToString()
                                    //    , dsXFSUData.Tables[0].Rows[i]["StnCode"].ToString(), dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString(), "", dsXFSUData.Tables[0].Rows[i]["FlightNo"].ToString()
                                    //    , dsXFSUData.Tables[0].Rows[i]["FlightDate"].ToString(), Convert.ToInt32(0), Convert.ToDouble(0.0), dsXFSUData.Tables[0].Rows[i]["EventDate"].ToString()));

                                    xfsuMessage = await _xFSUMessageProcessor.GenerateXFSUMessageofTheAWBV3(dsXFSUData.Tables[0].Rows[i]["AWBPrefix"].ToString(), dsXFSUData.Tables[0].Rows[i]["AWBNo"].ToString()
                                        , dsXFSUData.Tables[0].Rows[i]["StnCode"].ToString(), dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString(), "", dsXFSUData.Tables[0].Rows[i]["FlightNo"].ToString()
                                        , dsXFSUData.Tables[0].Rows[i]["FlightDate"].ToString(), Convert.ToInt32(0), Convert.ToDouble(0.0), dsXFSUData.Tables[0].Rows[i]["EventDate"].ToString());

                                    if (xfsuMessage.Length > 3)
                                    {
                                        if (dsXFSUData.Tables[1] != null && dsXFSUData.Tables[1].Rows.Count > 0)
                                        {
                                            strEmailid = Convert.ToString(dtToAdress.Rows[0]["EmailiD"]);
                                            lblMsgCommType = Convert.ToString(dtToAdress.Rows[0]["MsgCommType"]);
                                            messageVersion = Convert.ToString(dtToAdress.Rows[0]["MessageVersionNo"]);
                                            sitaHeaderType = Convert.ToString(dtToAdress.Rows[0]["SFTPHeaderType"]);
                                            if (lblMsgCommType.Equals("ALL", StringComparison.OrdinalIgnoreCase) ||
                                                lblMsgCommType.Equals("SITA", StringComparison.OrdinalIgnoreCase))
                                            {
                                                //GenericFunction genericFunction1 = new GenericFunction();

                                                SitaMessageHeader = _genericFunction.MakeMailMessageFormat(Convert.ToString(dtToAdress.Rows[0]["SFTPHeaderSITAAddress"]), Convert.ToString(dtToAdress.Rows[0]["OriginSenderAddress"]),
                                                    Convert.ToString(dtToAdress.Rows[0]["MessageID"]), sitaHeaderType);
                                            }
                                        }

                                        if (SitaMessageHeader.Length > 0)
                                        {
                                            // clsLog.WriteLogAzure(" AutoSendXFSUMessages SitaMessageHeader send ");
                                            _logger.LogInformation(" AutoSendXFSUMessages SitaMessageHeader send ");

                                            flag = await _genericFunction.SaveMessageOutBox("xFSU/" + dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString(), Convert.ToString(xfsuMessage), ""
                                                , Convert.ToString(SitaMessageHeader.ToString()), "", "", "", "",
                                                      dsXFSUData.Tables[0].Rows[i]["AWBPrefix"].ToString() + "-" + dsXFSUData.Tables[0].Rows[i]["AWBNo"].ToString(), "Auto", dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString());
                                            // clsLog.WriteLogAzure(" AutoSendXFSUMessages SitaMessageHeader send end ");
                                            _logger.LogInformation(" AutoSendXFSUMessages SitaMessageHeader send end ");
                                        }
                                        if (strEmailid != "")
                                        {
                                            // clsLog.WriteLogAzure(" AutoSendXFSUMessages send " + strEmailid);
                                            _logger.LogInformation(" AutoSendXFSUMessages send {0}", strEmailid);

                                            flag = await _genericFunction.SaveMessageOutBox("xFSU/" + dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString(), Convert.ToString(xfsuMessage), "", strEmailid, "", "", "", "",
                                                  dsXFSUData.Tables[0].Rows[i]["AWBPrefix"].ToString() + "-" + dsXFSUData.Tables[0].Rows[i]["AWBNo"].ToString(), "Auto", dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString());

                                            // clsLog.WriteLogAzure(" AutoSendXFSUMessages send end " + strEmailid);
                                            _logger.LogInformation(" AutoSendXFSUMessages send end {0}", strEmailid);
                                        }
                                        if (lblMsgCommType == "ServiceBus")
                                        {
                                            //ServiceBusQueueProcessor serviceBusQueueProcessor = new ServiceBusQueueProcessor(ConfigurationManager.AppSettings["ServiceBusConnectionString"].ToString(), ConfigurationManager.AppSettings["QueueName"].ToString());
                                            //await serviceBusQueueProcessor.SendMessageToQueueAsync(xfsuMessage);

                                            //clsLog.WriteLogAzure(messageType + " sent to Service Bus Queue");

                                            flag = await _genericFunction.SaveMessageOutBox("xFSU/" + dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString(), Convert.ToString(xfsuMessage), "", lblMsgCommType, "", "", "", "",
                                                  dsXFSUData.Tables[0].Rows[i]["AWBPrefix"].ToString() + "-" + dsXFSUData.Tables[0].Rows[i]["AWBNo"].ToString(), "Auto", dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString());
                                        }
                                        if (lblMsgCommType == "Message Queue")
                                        {
                                            flag = await _genericFunction.SaveMessageOutBox("xFSU/" + dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString(), Convert.ToString(xfsuMessage), "", lblMsgCommType, "", "", "", "",
                                                  dsXFSUData.Tables[0].Rows[i]["AWBPrefix"].ToString() + "-" + dsXFSUData.Tables[0].Rows[i]["AWBNo"].ToString(), "Auto", dsXFSUData.Tables[0].Rows[i]["MessageType"].ToString());
                                        }
                                    }
                                    Toid = Toid + "," + dsXFSUData.Tables[0].Rows[i]["TblAWbId"].ToString();
                                }
                                Toid = Toid.Remove(0, 1);
                                if (Toid != "")
                                {
                                    //string[] PFWB = new string[] { "TOID" };
                                    //SqlDbType[] ParamSqlType = new SqlDbType[] { SqlDbType.VarChar };
                                    //object[] paramValue = { Toid };
                                    //string strProcedure = "SPUpdateIsStatusSentFoxXFSU";
                                    //sqlServer.InsertData(strProcedure, PFWB, ParamSqlType, paramValue);

                                    string strProcedure = "SPUpdateIsStatusSentFoxXFSU";
                                    SqlParameter[] parameters =
                                    [
                                        new("@TOID", SqlDbType.VarChar) { Value = Toid }
                                    ];
                                    await _readWriteDao.ExecuteNonQueryAsync(strProcedure, parameters);
                                }
                            }
                            else
                            {
                                // clsLog.WriteLogAzure(" AutoSendXFSUMessages No record ");
                                _logger.LogWarning(" AutoSendXFSUMessages No record ");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /*Not in use*/

        /// <summary>
        /// Send Auto FFR to System
        /// </summary>
        //private async Task sendAutoFFR()
        //{
        //    try
        //    {
        //        //SQLServer db = new SQLServer(); ;
        //        //ds = db.SelectRecords("spStimulateAutoFFR");

        //        DataSet? ds = null;
        //        ds = await _readWriteDao.SelectRecords("spStimulateAutoFFR");

        //        if (ds != null)
        //        {
        //            if (ds.Tables.Count > 0)
        //            {
        //                if (ds.Tables[0].Rows.Count > 0)
        //                {
        //                    cls_SCMBL clscmbl = new cls_SCMBL();
        //                    clscmbl.EncodeFFRForSend(ds, 0);
        //                }
        //            }
        //        }

        //        //db = null;
        //        //GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //    }
        //}

        private string RemoveBinaryData(string orgmsg)
        {
            try
            {
                string msg = string.Empty;
                if (orgmsg != null && orgmsg.Length > 0)
                {
                    StringBuilder sb = new StringBuilder(orgmsg.Length);
                    foreach (char c in orgmsg)
                    {

                        if (c < 128 && c > 9)
                        {
                            sb.Append(c);
                        }
                    }
                    orgmsg = sb.ToString();
                }
                return msg = orgmsg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        private string RemoveSITAHeader(string msg)
        {
            string retstr = "";
            string orgmsg = msg;


            try
            {
                if (msg.StartsWith(".", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("=SMI", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("=TEXT", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("ZCZC", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QU", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QP", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QD", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QK", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("QN", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("PDM", StringComparison.OrdinalIgnoreCase) || msg.StartsWith("\r\n", StringComparison.OrdinalIgnoreCase))
                {
                    int n = 0;

                    string[] lines = msg.Split(Environment.NewLine.ToCharArray()).Skip(n).ToArray();

                    if (Array.Exists(lines, element => element.StartsWith("UCM", StringComparison.OrdinalIgnoreCase) && element.EndsWith("UCM", StringComparison.OrdinalIgnoreCase)))
                    {
                        lines = lines.Where(str => !str.StartsWith("ZCZC", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QU", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QD", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QK", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QN", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("PDM", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QP", StringComparison.OrdinalIgnoreCase)).ToArray();
                        //lines = lines.Where(str => !str.StartsWith(".", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("=TEXT", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("=SMI", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => str != "").ToArray();
                        lines = lines.Where(str => str != "=").ToArray();
                        lines = lines.Where(str => !str.StartsWith("NNNN", StringComparison.OrdinalIgnoreCase)).ToArray();
                    }
                    else
                    {
                        lines = lines.Where(str => !str.StartsWith("ZCZC", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QU", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QD", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QK", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QN", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("PDM", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("QP", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith(".", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("=TEXT", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => !str.StartsWith("=SMI", StringComparison.OrdinalIgnoreCase)).ToArray();
                        lines = lines.Where(str => str != "").ToArray();
                        lines = lines.Where(str => str != "=").ToArray();
                        lines = lines.Where(str => !str.StartsWith("NNNN", StringComparison.OrdinalIgnoreCase)).ToArray();
                    }

                    retstr = string.Join(Environment.NewLine, lines);
                }
                else
                {
                    retstr = msg;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                retstr = msg;
            }
            return retstr;
        }

        /// <summary>
        /// Function to select Data between Start & end string
        /// </summary>
        private static string ExtractFromString(string text, string start, string end)
        {
            try
            {
                List<string> Matched = new List<string>();
                int index_start = 0, index_end = 0;
                bool exit = false;
                while (!exit)
                {
                    index_start = text.IndexOf(start);
                    if (end == "" && end.Length < 1)
                        index_end = text.Length;//- index_start;
                    else
                        index_end = text.IndexOf(end);

                    if (index_start != -1 && index_end != -1)
                    {
                        Matched.Add(text.Substring(index_start + start.Length, index_end - index_start - start.Length));
                        text = text.Substring(index_end + end.Length);
                    }
                    else
                        exit = true;
                }
                return start + Matched[0] + Environment.NewLine + end;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _staticLogger.LogError(ex, $"Error on {MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        private static string ReplaceBlankSpaces(string Message)
        {
            string val = Message;
            try
            {
                string[] lines = Message.Split(' ');
                // Message = string.Join(Environment.NewLine, Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                // Message = Regex.Replace(Message, @"\s+", Environment.NewLine);
                for (int i = 0; i < lines.Length; i++)
                {
                    //if (lines[i].Contains('-'))
                    //{
                    //    int k = lines[i].IndexOf('-');
                    //    if (k <= 4)
                    //    {
                    //        lines[i] = Environment.NewLine + lines[i];
                    //    }
                    //}
                    if (lines[i].StartsWith("ULD/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("OSI/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("DIM/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("SSR/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("SCI/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("COR/", StringComparison.OrdinalIgnoreCase)
                    || lines[i].StartsWith("OCI/", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = Environment.NewLine + lines[i];
                    }

                }
                Message = string.Join(" ", lines);
            }
            catch (Exception ex)
            {
                Message = val;
                //_logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _staticLogger.LogError(ex, $"Error on {MethodBase.GetCurrentMethod()?.Name}");
            }
            return Message;
        }

        private async Task<DataSet> getAWBListForAlert()
        {
            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer objSQL = new SQLServer();
                //dsData = objSQL.SelectRecords(procedure);
                //objSQL = null;
                //GC.Collect();

                string procedure = "spGetDataForDwellTimeAlert";
                dsData = await _readWriteDao.SelectRecords(procedure);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsData;
        }

        private async Task<bool> sendMailLogic()
        {
            bool flag = false;
            try
            {
                DataSet dsData = new DataSet();
                dsData = await getAWBListForAlert();
                if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0)
                {
                    string AWBPrefix = "";
                    string AWBNo = "";
                    string Station = "";
                    string EmailID = "";
                    foreach (DataRow dr in dsData.Tables[0].Rows)
                    {
                        AWBPrefix = dr[0].ToString();
                        AWBNo = dr[1].ToString();
                        Station = dr[2].ToString();
                        EmailID = dr[3].ToString();
                        // clsLog.WriteLogAzure("Inserting Into OutBox @ " + DateTime.Now);
                        _logger.LogInformation("Inserting Into OutBox @ {0}", DateTime.Now);
                        flag = await InsertIntoOutBox(AWBPrefix, AWBNo, Station, EmailID);
                        // clsLog.WriteLogAzure("Inserting Result: " + flag.ToString());
                        _logger.LogInformation("Inserting Result: {0}", flag);

                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        private async Task<bool> InsertIntoOutBox(string AWBPrifix, string AWBNo, string Station, string EmailID)
        {
            bool flag = false;
            try
            {
                //cls_SCMBL clscmbl = new cls_SCMBL();
                string Subject = "Dwell Time Violation: " + AWBPrifix.Trim() + "-" + AWBNo.Trim();
                string Body = "Hello,\n\n";
                Body += "Below AWB(s) have violated the allowed Dwell Time.\n";
                Body += "AWBNo: " + AWBPrifix.Trim() + "-" + AWBNo.Trim() + "\n";
                Body += "Station: " + Station.Trim() + "\n";
                Body += "Please take appropriate Action.\n\n";
                Body += "Thanks,\nSmart Kargo Team.\n\n";
                Body += "Note: This is auto-generated Email,\nPlease do not reply.";
                flag = await _cls_SCMBL.addMsgToOutBox(Subject, Body, "", EmailID);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        private async Task CheckAWBDataforProcessing()
        {
            try
            {
                //SQLServer db = new SQLServer(); ;
                //db.ExecuteProcedure("spProcessUploadedAWBData");
                //db = null;
                //GC.Collect();
                await _readWriteDao.SelectRecords("spProcessUploadedAWBData");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task ProcessArrivalSMSNotifications()
        {
            DataSet? dsData = null;
            //SQLServer objSQL = new SQLServer();
            int rowID = 0;
            string errorDesc = string.Empty;
            string SMSResponse = string.Empty;
            object[] consigneeInfo = null;
            bool isSMSNewApi = _appConfig.Sms.IsSMSNewApi;

            try
            {
                // Get pending records to process the SMS

                //dsData = objSQL.SelectRecords("Log.uspGetArrivalSMSNotification");

                dsData = await _readWriteDao.SelectRecords("Log.uspGetArrivalSMSNotification");

                if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0)
                {
                    for (int intCount = 0; intCount < dsData.Tables[0].Rows.Count; intCount++)
                    {
                        rowID = Convert.ToInt32(dsData.Tables[0].Rows[intCount]["SerialNumber"]);
                        errorDesc = string.Empty;
                        string consigneePhoneNumber = Convert.ToString(dsData.Tables[0].Rows[intCount]["ConsigneePhoneNo"]);
                        string consigneesName = Convert.ToString(dsData.Tables[0].Rows[intCount]["ConsigneeName"]);
                        int arrivedPieces = Convert.ToInt32(dsData.Tables[0].Rows[intCount]["ArrivedPieces"]);
                        decimal arrivedWeight = Convert.ToDecimal(dsData.Tables[0].Rows[intCount]["ArrivedWeight"]);
                        string awbNumber = Convert.ToString(dsData.Tables[0].Rows[intCount]["AWBNumber"]);
                        string origin = Convert.ToString(dsData.Tables[0].Rows[intCount]["Origin"]);
                        string destination = Convert.ToString(dsData.Tables[0].Rows[intCount]["Destination"]);
                        string clientName = Convert.ToString(dsData.Tables[0].Rows[intCount]["ClientName"]);
                        string clientPhoneNumber = Convert.ToString(dsData.Tables[0].Rows[intCount]["ClientPhoneNo"]);
                        string UpdateStatus = Convert.ToString(dsData.Tables[0].Rows[intCount]["Update"]);
                        string subscriberKey = awbNumber + "_" + consigneePhoneNumber + "_" + DateTime.Now;

                        object[] consigneeInfoNew = { consigneePhoneNumber, consigneesName, arrivedPieces, arrivedWeight, awbNumber,
                            origin, destination, clientName, clientPhoneNumber, UpdateStatus,subscriberKey};

                        object[] consigneeInfoOld = { consigneePhoneNumber, consigneesName, arrivedPieces, arrivedWeight, awbNumber,
                            origin, destination, clientName, clientPhoneNumber, UpdateStatus};

                        //if (SMSNewAPI.ToUpper() == "TRUE")
                        if (isSMSNewApi)
                        {
                            consigneeInfo = consigneeInfoNew;
                        }
                        else
                            consigneeInfo = consigneeInfoOld;

                        //Call SMS API and process the SMS
                        string apiToken = await GenerateAPIToken();

                        if (!string.IsNullOrEmpty(apiToken))
                        {
                            errorDesc = SendSMS(apiToken, consigneeInfo, ref SMSResponse);
                        }

                        // Update the SMS log with result
                        await updateSMSStatus(rowID, errorDesc, SMSResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    dsData = null;
            //    objSQL = null;
            //    GC.Collect();
            //}
        }

        private async Task RemoveLyingListProcess()
        {
            //SQLServer objSQL = new SQLServer();
            DataSet? ds = null;
            try
            {
                //ds = objSQL.SelectRecords("dbo.UspRemoveLyingListOfShipment");

                ds = await _readWriteDao.SelectRecords("dbo.UspRemoveLyingListOfShipment");

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string PartnerEmailIds = ds.Tables[0].Rows[0][0].ToString();
                    string subject = ds.Tables[0].Rows[0][1].ToString();
                    string message = ds.Tables[0].Rows[0][2].ToString();

                    //GenericFunction obj = new GenericFunction();
                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        #region Code to Convert Data Table to Excel File

                        byte[] byteArray = null;
                        MemoryStream stream = null;
                        string FileURL = null;

                        System.String FileNameFormat = "LyingExcel_" + DateTime.Now.ToString("yyyyMMddhhmmss");

                        StringBuilder Excel = GetExcelDatafromDT(ds.Tables[1]);
                        byteArray = Encoding.ASCII.GetBytes(Excel.ToString());
                        stream = new MemoryStream(byteArray);

                        FileURL = _genericFunction.UploadToBlob(stream, FileNameFormat + ".xls", "sis");

                        await _genericFunction.DumpInterfaceInformation(subject, message, DateTime.Now, "LYINGLIST", "", false, "", PartnerEmailIds, stream, ".XLS", FileURL, "0", "outbox", FileNameFormat + ".xls");

                        #endregion
                    }
                    else
                    {
                        await _genericFunction.SaveMessageOutBox(subject, "There were no AWBs to be cleaned up on the Lying List.", "", PartnerEmailIds);
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    objSQL = null;
            //    GC.Collect();
            //}
        }

        private async Task SendLyingListReport()
        {
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
            //SQLServer objSQL = new SQLServer();
            DataSet? ds = null;
            try
            {
                //ds = objSQL.SelectRecords("dbo.UspSendLyingListReport");

                //ds = objSQL.SelectRecords("dbo.UspSendLyingListReport", "UTCDatetime", UTCDatetime, SqlDbType.DateTime);
                SqlParameter[] parameters =
                [
                    new("@UTCDatetime", SqlDbType.DateTime) { Value = UTCDatetime }
                ];
                ds = await _readWriteDao.SelectRecords("dbo.UspSendLyingListReport", parameters);

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            string PartnerEmailIds = ds.Tables[0].Rows[0]["EmailIds"].ToString();
                            string subject = ds.Tables[0].Rows[0]["Subject"].ToString();
                            string message = ds.Tables[0].Rows[0]["Message"].ToString() + ds.Tables[2].Rows[0]["Summary"].ToString();


                            //GenericFunction obj = new GenericFunction();

                            if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                            {
                                #region Code to Convert Data Table to Excel File

                                byte[] byteArray = null;
                                MemoryStream stream = null;
                                string FileURL = null;

                                System.String FileNameFormat = "WareHouseInventoryExportFltLevel_" + UTCDatetime.ToString("MMddyyyyhhmm");

                                StringBuilder Excel = GetExcelDatafromDT(ds.Tables[1]);
                                byteArray = Encoding.ASCII.GetBytes(Excel.ToString());
                                stream = new MemoryStream(byteArray);

                                FileURL = _genericFunction.UploadToBlob(stream, FileNameFormat + ".xls", "sis");

                                await _genericFunction.DumpInterfaceInformation(subject, message, UTCDatetime, "LYINGLIST", "", false, "", PartnerEmailIds, stream, ".XLS", FileURL, "0", "outbox", FileNameFormat);


                                #endregion
                            }
                            else
                            {
                                await _genericFunction.SaveMessageOutBox(subject, "There is NIL cargo in the warehouse.", "", PartnerEmailIds, "", "", "", null, "", "Auto", "LYINGLIST");

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    objSQL = null;
            //    GC.Collect();
            //}
        }


        /// <summary>
        /// SendFlightControlListReport
        /// </summary>
        private async Task SendFlightControlListReport()
        {
            try
            {
                DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
                                                                     //SQLServer objSQL = new SQLServer();

                DataSet? DsEmails = null;

                try
                {
                    //DsEmails = objSQL.SelectRecords("USPGetEmailIdsForFltCnt");
                    DsEmails = await _readWriteDao.SelectRecords("USPGetEmailIdsForFltCnt");

                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                if (DsEmails != null)
                {
                    if (DsEmails.Tables.Count > 0)
                    {
                        if (DsEmails.Tables[0] != null && DsEmails.Tables[0].Rows.Count > 0)
                        {
                            int length = DsEmails.Tables[0].Rows.Count;

                            for (int i = 0; i < length; i++)
                            {

                                try
                                {
                                    string Source = Convert.ToString(DsEmails.Tables[0].Rows[i]["Source"]);
                                    string Dest = Convert.ToString(DsEmails.Tables[0].Rows[i]["Dest"]);
                                    string FlightType = Convert.ToString(DsEmails.Tables[0].Rows[i]["IsFlighttype"]);

                                    short IsUpdate = 0;

                                    if (i == length - 1)
                                    {
                                        IsUpdate = 1;
                                    }
                                    DataSet? ds = null;


                                    //SQLServer sqlServer = new SQLServer();
                                    //ds = sqlServer.SelectRecords("USPSendFlightControlData", sqlParameters);

                                    SqlParameter[] sqlParameters = new SqlParameter[] { new SqlParameter("@Source", Source), new SqlParameter("@Dest", Dest), new SqlParameter("@IsUpdate", IsUpdate), new SqlParameter("@IsFlighttype", FlightType) };
                                    ds = await _readWriteDao.SelectRecords("USPSendFlightControlData", sqlParameters);

                                    if (ds != null)
                                    {
                                        if (ds.Tables.Count > 0)
                                        {
                                            if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                            {
                                                string PartnerEmailIds = Convert.ToString(DsEmails.Tables[0].Rows[i]["PartnerEmailiD"]);
                                                string subject = Convert.ToString(DsEmails.Tables[0].Rows[i]["Subject"]);
                                                string message = Convert.ToString(DsEmails.Tables[0].Rows[i]["Message"]);

                                                //GenericFunction obj = new GenericFunction();

                                                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                                                {
                                                    #region Code to Convert Data Table to Excel File


                                                    StringBuilder Excel = GetExcelFlightExportData(ds.Tables[0]);

                                                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                                                    string pathhtml = Directory.GetParent(path).Parent.FullName;
                                                    StringReader htmlFile = new StringReader(File.ReadAllText(pathhtml + "/Reports/FlightControlDataForVJ.html"));
                                                    string htmlContent = htmlFile.ReadToEnd().ToString();

                                                    htmlContent = htmlContent.Replace("@message@", message);
                                                    htmlContent = htmlContent.Replace("@FlightControlData@", Excel.ToString());


                                                    addMsgToOutBox(subject, htmlContent, "", PartnerEmailIds, false, true, "FLTCNTDATA");

                                                    #endregion
                                                }
                                                else
                                                {
                                                    await _genericFunction.SaveMessageOutBox(subject, "There is no records found..!", "", PartnerEmailIds, "", "", "", null, "", "Auto", "FLTCNTDATA");

                                                }
                                            }
                                        }
                                        ds = null;
                                        Source = null;
                                        Dest = null;
                                        FlightType = null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                                //finally
                                //{
                                //    objSQL = null;
                                //    GC.Collect();

                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }

        }


        private async Task SendBSTDDataInXML()
        {
            ///SQLServer objSQL = new SQLServer();
            DataSet? ds = null;

            try
            {
                //ds = objSQL.SelectRecords("uspGetPlanCargoLoadsDetails");

                ds = await _readWriteDao.SelectRecords("uspGetPlanCargoLoadsDetails");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                if (ds.Tables[0].Rows[i]["IsExportToManifest"].ToString() == "N")
                                {
                                    XmlDocument doc = new XmlDocument();
                                    XmlNode docNode = doc.CreateXmlDeclaration("1.0", null, null);
                                    doc.AppendChild(docNode);

                                    XmlElement cargoBaggageInformation = doc.CreateElement("cargoBaggageInformation");
                                    (cargoBaggageInformation).SetAttribute("xmlns", "http://www.smart4aviation.aero/xsd/cargoBaggageInformation");
                                    (cargoBaggageInformation).SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                                    (cargoBaggageInformation).SetAttribute("xsi:schemaLocation", "http://www.smart4aviation.aero/xsd/cargoBaggageInformation ../Schema/S4A_cargoBaggageInformation-1.12.xsd");
                                    doc.AppendChild(cargoBaggageInformation);


                                    XmlNode msgTimestamp = doc.CreateElement("msgTimestamp");
                                    msgTimestamp.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["msgTimestamp"].ToString()));
                                    cargoBaggageInformation.AppendChild(msgTimestamp);

                                    XmlElement FlightIdentifier = doc.CreateElement("FlightIdentifier");
                                    (FlightIdentifier).SetAttribute("airlineCode", ds.Tables[0].Rows[i]["airlineCode"].ToString());
                                    (FlightIdentifier).SetAttribute("flightNo", ds.Tables[0].Rows[i]["flightNo"].ToString());
                                    (FlightIdentifier).SetAttribute("os", ds.Tables[0].Rows[i]["os"].ToString());
                                    (FlightIdentifier).SetAttribute("originDate", ds.Tables[0].Rows[i]["originDate"].ToString());
                                    (FlightIdentifier).SetAttribute("departureAirportCode", ds.Tables[0].Rows[i]["departureAirportCode"].ToString());
                                    (FlightIdentifier).SetAttribute("arrivalAirportCode", ds.Tables[0].Rows[i]["arrivalAirportCode"].ToString());
                                    cargoBaggageInformation.AppendChild(FlightIdentifier);

                                    // ContainerElement CargoEstimates
                                    XmlNode ContainerElement1 = doc.CreateElement("CargoEstimates");
                                    doc.DocumentElement.AppendChild(ContainerElement1);

                                    // ContainerElement CargoEstimatesPerDestination
                                    XmlNode ContainerElement2 = doc.CreateElement("CargoEstimatesPerDestination");
                                    ContainerElement1.AppendChild(ContainerElement2);

                                    //cargoDestinationIATA
                                    XmlNode cargoDestinationIATA = doc.CreateElement("cargoDestinationIATA");
                                    cargoDestinationIATA.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["arrivalAirportCode"].ToString()));
                                    ContainerElement2.AppendChild(cargoDestinationIATA);

                                    //mailWeight
                                    XmlNode mailWeight = doc.CreateElement("mailWeight");
                                    mailWeight.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["mailWeight"].ToString()));
                                    ContainerElement2.AppendChild(mailWeight);

                                    //mailVolume
                                    XmlNode mailVolume = doc.CreateElement("mailVolume");
                                    mailVolume.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["mailVolume"].ToString()));
                                    ContainerElement2.AppendChild(mailVolume);

                                    //cargoWeight
                                    XmlNode cargoWeight = doc.CreateElement("cargoWeight");
                                    cargoWeight.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["cargoWeight"].ToString()));
                                    ContainerElement2.AppendChild(cargoWeight);

                                    //cargoVolume
                                    XmlNode cargoVolume = doc.CreateElement("cargoVolume");
                                    cargoVolume.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["cargoVolume"].ToString()));
                                    ContainerElement2.AppendChild(cargoVolume);

                                    //volumeUnit
                                    XmlNode volumeUnit = doc.CreateElement("volumeUnit");
                                    volumeUnit.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["volumeUnit"].ToString()));
                                    ContainerElement2.AppendChild(volumeUnit);

                                    //weightUnit
                                    XmlNode weightUnit = doc.CreateElement("weightUnit");
                                    weightUnit.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["weightUnit"].ToString()));
                                    ContainerElement2.AppendChild(weightUnit);

                                    //ListType
                                    XmlNode ListType = doc.CreateElement("ListType");
                                    cargoBaggageInformation.AppendChild(ListType);

                                    XmlNode dataType = doc.CreateElement("dataType");
                                    dataType.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["dataType"].ToString()));
                                    ListType.AppendChild(dataType);

                                    XmlNode dataSource = doc.CreateElement("dataSource");
                                    dataSource.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["dataSource"].ToString()));
                                    ListType.AppendChild(dataSource);

                                    XmlNode dataTimestamp = doc.CreateElement("dataTimestamp");
                                    dataTimestamp.AppendChild(doc.CreateTextNode(ds.Tables[0].Rows[i]["dataTimestamp"].ToString()));
                                    ListType.AppendChild(dataTimestamp);

                                    DateTime FlightDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["originDate"]);
                                    //string subject = ds.Tables[0].Rows[i]["airlineCode"].ToString() + ds.Tables[0].Rows[i]["flightNo"].ToString()  + "_" + FlightDate.Day.ToString().PadLeft(2, '0') + FlightDate.Month.ToString().PadLeft(2, '0') + FlightDate.Year.ToString() + "_" + "BSTD" + "_" + DateTime.UtcNow.Year.ToString() + DateTime.UtcNow.Month.ToString().PadLeft(2, '0') + DateTime.UtcNow.Day.ToString().PadLeft(2, '0') + DateTime.UtcNow.Hour.ToString() + DateTime.UtcNow.Minute.ToString() + ".xml";

                                    string subject = ds.Tables[0].Rows[i]["airlineCode"].ToString() + ds.Tables[0].Rows[i]["flightNo"].ToString() + "_" + FlightDate.ToString("dMMMyy") + "_" + ds.Tables[0].Rows[i]["Interval"].ToString() + ".xml";

                                    //GenericFunction obj = new GenericFunction();
                                    await _genericFunction.SaveMessageOutBox(subject, doc.InnerXml, "", "SFTP", ds.Tables[0].Rows[i]["departureAirportCode"].ToString(), ds.Tables[0].Rows[i]["arrivalAirportCode"].ToString(), ds.Tables[0].Rows[i]["airlineCode"].ToString() + ds.Tables[0].Rows[i]["flightNo"].ToString(), FlightDate.ToString("yyyy-MM-dd HH:mm:ss"), "", "AutoSend", "CargoLoadPlanBSTD");
                                }
                                if (ds.Tables[0].Rows[i]["IsExportToManifest"].ToString() == "Y")
                                {
                                    // clsLog.WriteLogAzure("Before sending UWS: "
                                    //     + ds.Tables[0].Rows[i]["airlineCode"].ToString()
                                    //     + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                    //     + " : " + ds.Tables[0].Rows[i]["originDate"].ToString());
                                    _logger.LogInformation("Before sending UWS: {0} : {1}",
                                         ds.Tables[0].Rows[i]["airlineCode"].ToString()
                                        + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                        , ds.Tables[0].Rows[i]["originDate"]);

                                    await SendUWSMessage(ds.Tables[0].Rows[i]["airlineCode"].ToString() + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                        , ds.Tables[0].Rows[i]["originDate"].ToString()
                                        , ds.Tables[0].Rows[i]["departureAirportCode"].ToString()
                                        , ds.Tables[0].Rows[i]["arrivalAirportCode"].ToString()
                                        , true);

                                    // clsLog.WriteLogAzure("Before sending NTM: "
                                    //     + ds.Tables[0].Rows[i]["airlineCode"].ToString()
                                    //     + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                    //     + " : " + ds.Tables[0].Rows[i]["originDate"].ToString());
                                    _logger.LogInformation("Before sending NTM: {0} : {1}",
                                         ds.Tables[0].Rows[i]["airlineCode"].ToString()
                                        + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                        , ds.Tables[0].Rows[i]["originDate"]);

                                    await SendNTMMessage(ds.Tables[0].Rows[i]["airlineCode"].ToString() + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                        , ds.Tables[0].Rows[i]["originDate"].ToString()
                                        , ds.Tables[0].Rows[i]["departureAirportCode"].ToString()
                                        , ds.Tables[0].Rows[i]["arrivalAirportCode"].ToString()
                                        , true);

                                    // clsLog.WriteLogAzure("After sent NTM: "
                                    //     + ds.Tables[0].Rows[i]["airlineCode"].ToString()
                                    //     + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                    //     + " : " + ds.Tables[0].Rows[i]["originDate"].ToString());
                                    _logger.LogInformation("After sent NTM: {0} : {1}",
                                         ds.Tables[0].Rows[i]["airlineCode"].ToString()
                                        + ds.Tables[0].Rows[i]["flightNo"].ToString()
                                        , ds.Tables[0].Rows[i]["originDate"]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    objSQL = null;
            //    GC.Collect();
            //}
        }

        private async Task SendCargoLoadPlan()
        {

            //SQLServer objSQL = new SQLServer();
            DataSet? dsCLPData = null;
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
            try
            {
                //dsCLPData = objSQL.SelectRecords("uspSendCargoLoadPlanToSFTP");
                dsCLPData = await _readWriteDao.SelectRecords("uspSendCargoLoadPlanToSFTP");

                if (dsCLPData != null)
                {
                    if (dsCLPData.Tables.Count > 0)
                    {
                        if (dsCLPData.Tables[0] != null && dsCLPData.Tables[0].Rows.Count > 0)
                        {

                            var result = new StringBuilder();

                            foreach (DataRow row in dsCLPData.Tables[0].Rows) // Select each Row
                            {
                                for (int i = 0; i < dsCLPData.Tables[0].Columns.Count; i++)// Write Each coloumn in a Row
                                {
                                    result.Append(row[i].ToString());
                                    result.Append(i == dsCLPData.Tables[0].Columns.Count - 1 ? "\n" : "");
                                }
                            }

                            StringBuilder sbLoadPlan = new StringBuilder();
                            sbLoadPlan = result;
                            if (sbLoadPlan != null)
                            {
                                try
                                {
                                    string FileName = "CARGO_PLAN_WEIGHT_" + UTCDatetime.ToString("yyyyMMddHHmmss") + ".csv";
                                    addMsgToOutBox(FileName, sbLoadPlan.ToString(), "", "SFTP", false, false, "LoadPlan");
                                }
                                catch (Exception exc)
                                {
                                    // clsLog.WriteLogAzure(exc);
                                    _logger.LogError(exc, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    objSQL = null;
            //    GC.Collect();
            //}
        }

        private async Task AutoSendUnDepartedAleartFunc()
        {
            //SQLServer objSQL = new SQLServer();
            DataSet? ds = null;
            try
            {
                //ds = objSQL.SelectRecords("dbo.UspSendAleartUnDepartedFlt");

                ds = await _readWriteDao.SelectRecords("dbo.UspSendAleartUnDepartedFlt");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {
                                if (ds.Tables[0].Rows[i]["EmailIds"].ToString() != "" && ds.Tables[0].Rows[i]["EmailIds"].ToString() != null)
                                {
                                    //GenericFunction obj = new GenericFunction();
                                    //cls_SCMBL clscmbl = new cls_SCMBL();

                                    string PartnerEmailIds = ds.Tables[0].Rows[i]["EmailIds"].ToString();
                                    string Subject = ds.Tables[0].Rows[i]["EmailSubject"].ToString();
                                    string ClinetName = ds.Tables[0].Rows[i]["ClientName1"].ToString();

                                    string Body = "DEAR TEAM,\n\n";
                                    Body += "KINDLY NOTE THAT THE FLIGHT BELOW HAS NOT DEPARTED ACCORDINGLY IN THE SYSTEM. YOUR IMMEDIATE ACTION IS HIGHLY APPRECIATED.\n\n";
                                    Body += "FLIGHT NUMBER  : " + ds.Tables[0].Rows[i]["FlightNo"].ToString() + "\n";
                                    Body += "FLIGHT  DATE   : " + ds.Tables[0].Rows[i]["FlightDate"].ToString() + "\n";
                                    Body += "DEPARTURE TIME : " + ds.Tables[0].Rows[i]["FltDeptTime"].ToString() + "\n\n";
                                    Body += "REGARDS,\n";
                                    Body += ClinetName.ToUpper();

                                    await _cls_SCMBL.addMsgToOutBox(Subject, Body.ToString(), "", PartnerEmailIds, "Auto", null, "UnDepartedAlert", string.Empty, ds.Tables[0].Rows[i]["FlightNo"].ToString(), null, ds.Tables[0].Rows[i]["Source"].ToString(), "");

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    objSQL = null;
            //    GC.Collect();
            //}
        }

        #region Arrival SMS Integration
        public async Task<string> GenerateAPIToken()
        {
            DataSet? dsResult = new DataSet();
            string tokenKey = string.Empty;
            string accessTokenUrl = _appConfig.Authentication.AccessTokenUrl;
            string basicAuthenticationHeader = _appConfig.Authentication.BasicAuthenticationHeader;
            bool isSMSNewApi = _appConfig.Sms.IsSMSNewApi;
            try
            {
                //Check Token is expired or not.
                dsResult = await validateOrUpdateToken(tokenKey, "C");
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    //If token is not expired return exsting Token key
                    if (Convert.ToString(dsResult.Tables[0].Rows[0]["IsExpired"]).Equals("N"))
                    {
                        tokenKey = dsResult.Tables[0].Rows[0]["TokenKey"].ToString();
                        return tokenKey;
                    }

                    //If token is expired generate new Token
                    if (Convert.ToString(dsResult.Tables[0].Rows[0]["IsExpired"]).Equals("Y"))
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;

                        //var authCredential = Encoding.UTF8.GetBytes("E8Lxqumi5oe8H2GbRQhfALTMmuEVdb7h:ryhSsA67XJ4GhWRi");
                        //basicAuthenticationHeader = Convert.ToBase64String(authCredential);
                        var client = new RestClient(accessTokenUrl);
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        string ClientId = ConfigurationManager.AppSettings["ClientId"].ToString();
                        string ClientSeceret = ConfigurationManager.AppSettings["ClientSeceret"].ToString();

                        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                        //if (SMSNewAPI.ToUpper() == "TRUE")

                        if (isSMSNewApi)
                        {
                            request.AddHeader("cache-control", "no-cache");
                            request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=" + ClientId + "&client_secret=" + ClientSeceret, ParameterType.RequestBody);
                        }
                        else
                        {
                            request.AddHeader("Authorization", "Basic " + basicAuthenticationHeader);
                            request.AddParameter("grant_type", "client_credentials");
                        }

                        IRestResponse response = client.Execute(request);
                        string jsonData = "[" + response.Content + "]";
                        var obj = JArray.Parse(jsonData);

                        //if (SMSNewAPI.ToUpper() == "TRUE")
                        //    tokenKey = (string)obj[0]["access_token"];
                        //else
                        //    tokenKey = (string)obj[0]["accessToken"]["accessToken"];

                        if (isSMSNewApi)
                        {
                            tokenKey = (string)obj[0]["access_token"];
                        }
                        else
                        {
                            tokenKey = (string)obj[0]["accessToken"]["accessToken"];
                        }

                        if (!string.IsNullOrEmpty(tokenKey))
                        {
                            //update new token key to database
                            dsResult = await validateOrUpdateToken(tokenKey, "U");
                            return tokenKey;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "";
            }
            return tokenKey;
        }
        public string SendSMS(string token, object[] consigneeInfo, ref string strResponce)
        {
            string errorDesc = string.Empty;
            string sendSMSUrl = _appConfig.Sms.SendSMSUrl;
            bool isSMSNewApi = _appConfig.Sms.IsSMSNewApi;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;
                var smsClient = new RestClient(sendSMSUrl);
                smsClient.Timeout = -1;
                var request = new RestRequest(Method.POST);

                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Content-type", "application/json");

                //if (SMSNewAPI.ToUpper() != "TRUE")
                if (isSMSNewApi)
                {
                    request.AddHeader("token", token);
                    request.AddHeader("Accept", "application/json");
                }

                string eventDefinitionKey = ConfigurationManager.AppSettings["EventDefinitionKey"].ToString();

                //if (SMSNewAPI.ToUpper() == "TRUE")
                if (isSMSNewApi)
                {
                    var bodyNew = new
                    {
                        ContactKey = consigneeInfo[10],
                        EventDefinitionKey = eventDefinitionKey,
                        Data = new
                        {
                            consigneePhoneNumber = consigneeInfo[0],
                            consigneesName = consigneeInfo[1],
                            arrivedPieces = consigneeInfo[2],
                            arrivedWeight = consigneeInfo[3],
                            awbNumber = consigneeInfo[4],
                            origin = consigneeInfo[5],
                            destination = consigneeInfo[6],
                            clientName = consigneeInfo[7],
                            clientPhoneNumber = consigneeInfo[8],
                            UpdateStatus = consigneeInfo[9],
                            subscriberKey = consigneeInfo[10]
                        }
                    };
                    request.AddJsonBody(bodyNew);
                }
                else
                {
                    var bodyOld = new
                    {
                        consigneePhoneNumber = consigneeInfo[0],
                        consigneesName = consigneeInfo[1],
                        arrivedPieces = consigneeInfo[2],
                        arrivedWeight = consigneeInfo[3],
                        awbNumber = consigneeInfo[4],
                        origin = consigneeInfo[5],
                        destination = consigneeInfo[6],
                        clientName = consigneeInfo[7],
                        clientPhoneNumber = consigneeInfo[8],
                        UpdateStatus = consigneeInfo[9],
                    };
                    request.AddJsonBody(bodyOld);
                }

                IRestResponse smsResponse = smsClient.Execute(request);
                string str = "[" + smsResponse.Content + "]";
                string message = string.Empty;
                StringBuilder apiMessage = new StringBuilder();
                var obj = JArray.Parse(str);
                switch (Convert.ToString(smsResponse.StatusCode))
                {
                    case "BadRequest":
                        str = "[" + smsResponse.Content + "]";
                        obj = JArray.Parse(str);
                        apiMessage.Append(Convert.ToString(obj[0]["message"]["errorMessage"]));
                        break;

                    case "PreconditionFailed":
                        str = smsResponse.Content;
                        obj = JArray.Parse(str);
                        for (int i = 0; i < obj.Count; i++)
                        {
                            apiMessage.Append(Convert.ToString(obj[i]["path"] + "-" + obj[i]["message"]) + " ");
                        }
                        break;
                    case "OK":
                        apiMessage.Append("");
                        break;
                    case "Forbidden":
                        apiMessage.Append("Forbidden");
                        break;
                    default:
                        apiMessage.Append(Convert.ToString(obj[0]["message"]));
                        break;
                }

                if (apiMessage.Length > 0)
                    errorDesc = apiMessage.ToString();

                strResponce = smsResponse.Content.ToString();
            }
            catch (Exception ex)
            {
                strResponce = ex.InnerException.Message;
                errorDesc = ex.InnerException.Message;
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

            return errorDesc;
        }
        public async Task<DataSet?> validateOrUpdateToken(string tokenKey, string flag)
        {
            DataSet? dsResult = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //dsResult = sqlServer.SelectRecords("uspUpdateTokenForArrivalSMS", sqlParameter);
                //GC.Collect();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                     new SqlParameter("@TokenKey",tokenKey)
                    ,new SqlParameter("@Flag",flag)
                };
                dsResult = await _readWriteDao.SelectRecords("uspUpdateTokenForArrivalSMS", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
            return dsResult;
        }

        public async Task<DataSet?> updateSMSStatus(int rowID, string errorDesc, string APIResponse)
        {
            DataSet? dsResult = new DataSet();
            try
            {


                //SQLServer sqlServer = new SQLServer();
                //dsResult = sqlServer.SelectRecords("Log.uspUpdateArrivalSMSNotification", sqlParameter);
                //GC.Collect();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                             new SqlParameter("@ROWId",rowID)
                            ,new SqlParameter("@ErrorDesc",errorDesc)
                            ,new SqlParameter("@APIResponse",APIResponse)
                        };
                dsResult = await _readWriteDao.SelectRecords("Log.uspUpdateArrivalSMSNotification", sqlParameter);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
            return dsResult;
        }
        #endregion

        private StringBuilder GetExcelFlightExportData(DataTable objDs)
        {
            try
            {
                StringBuilder Excel = new StringBuilder();


                Excel.Append("<table border='1' class='PageInsideTable ' Id='tblShipperBillingReport'>");
                Excel.Append("<tr><td class='dataTableHeader'>Flight</td><td class='dataTableHeader'>Flight Date</td><td class='dataTableHeader'>Aircraft Type</td><td class='dataTableHeader'>Origin</td><td class='dataTableHeader'>Dest</td><td class='dataTableHeader'>Dep Time</td><td class='dataTableHeader'>Arr Time</td>");
                Excel.Append("<td class='dataTableHeader'><span style='Color:Red;'><strong>ESTIMATED WEIGHT</strong></span> <br/> Confirmed Wt.</td>");

                Excel.Append("</tr>");

                // To Write Data in Excel Format (Tab)
                foreach (DataRow dr in objDs.Rows)
                {
                    Excel.Append("<tr>");
                    Excel.Append("<td>" + dr["FlightId"].ToString() + "</td>");
                    Excel.Append("<td>" + dr["FlightDate"].ToString() + "</td>");
                    //Excel.Append("<td>" + dr["FlightType"].ToString() + "</td>");
                    Excel.Append("<td>" + dr["AircraftType"].ToString() + "</td>");
                    Excel.Append("<td>" + dr["Source"].ToString() + "</td>");
                    Excel.Append("<td>" + dr["Dest"].ToString() + "</td>");
                    Excel.Append("<td>" + dr["DepTime"].ToString() + "</td>");
                    Excel.Append("<td>" + dr["ArrTime"].ToString() + "</td>");

                    string ConfirmedWt = dr["ConfirmedWeight"].ToString() == "" ? "0.00" : dr["ConfirmedWeight"].ToString();
                    Excel.Append("<td style='mso-number-format:00.00;text-align:right;'>" + ConfirmedWt + "</td>");
                    Excel.Append("</tr>");
                }

                Excel.Append("</table>");

                return Excel;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }


        private StringBuilder GetExcelDatafromDT(DataTable objDs)
        {
            try
            {
                StringBuilder Excel = new StringBuilder();


                int intColCount = 0;
                foreach (DataColumn dc in objDs.Columns)
                {
                    if (intColCount == 0)
                        Excel.Append(dc.ColumnName);
                    else
                        Excel.Append("\t" + dc.ColumnName);
                    intColCount++;
                }
                Excel.Append("\n");

                // To Write Data in Excel Format (Tab)
                for (int intRow = 0; intRow < objDs.Rows.Count; intRow++)
                {
                    for (int intCol = 0; intCol < objDs.Columns.Count; intCol++)
                    {
                        if (intCol == 0)
                            Excel.Append(objDs.Rows[intRow][intCol].ToString());
                        else
                        {
                            Excel.Append("\t" + objDs.Rows[intRow][intCol].ToString());
                        }

                    }
                    Excel.Append("\n");
                }

                return Excel;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }
        #endregion

        #region "Exchange Rates API"

        public async Task<string> GenerateExchangeAPIToken()
        {

            DataSet dsResult = new DataSet();
            string tokenKey = string.Empty;
            string tokenURL = string.Empty;
            string forexClientId = string.Empty;
            string forexClientSecret = string.Empty;
            try
            {
                //Check Token is expired or not.
                dsResult = await validateOrUpdateForexToken(tokenKey, "C");
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables[0].Rows.Count > 0)
                {
                    //If token is not expired return exsting Token key
                    if (Convert.ToString(dsResult.Tables[0].Rows[0]["IsExpired"]).Equals("N"))
                    {
                        tokenKey = dsResult.Tables[0].Rows[0]["TokenKey"].ToString();
                        return tokenKey;
                    }

                    // If token is expired generate new Token
                    if (Convert.ToString(dsResult.Tables[0].Rows[0]["IsExpired"]).Equals("Y"))
                    {
                        //GenericFunction objgenericFunction = new GenericFunction();
                        tokenURL = ConfigCache.Get("ForexTokenURL").Trim();
                        forexClientId = ConfigCache.Get("ForexClientID").Trim();
                        forexClientSecret = ConfigCache.Get("ForexClientSecret").Trim();

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                               | SecurityProtocolType.Tls11
                               | SecurityProtocolType.Tls12
                               | SecurityProtocolType.Ssl3;
                        var client = new RestClient(tokenURL);
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Accept", "application/json");
                        request.AddHeader("Content-type", "application/json");
                        var body = new
                        {
                            clientId = forexClientId,
                            clientSecret = forexClientSecret
                        };
                        request.AddJsonBody(body);
                        IRestResponse response = client.Execute(request);
                        string jsonData = "[" + response.Content + "]";
                        var obj = JArray.Parse(jsonData);
                        tokenKey = (string)obj[0]["token"];
                        string tokenStatus = (string)obj[0]["status"];

                        if (!string.IsNullOrEmpty(tokenKey))
                        {
                            //update new token key to database
                            dsResult = await validateOrUpdateForexToken(tokenKey, "U");
                            return tokenKey;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "";
            }
            return tokenKey;
        }

        public async Task<DataSet?> validateOrUpdateForexToken(string tokenKey, string flag)
        {
            DataSet? dsResult = new DataSet();
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //dsResult = sqlServer.SelectRecords("uspUpdateTokenForForexAPI", sqlParameter);
                //GC.Collect();

                SqlParameter[] sqlParameter = new SqlParameter[] {
                     new SqlParameter("@TokenKey",tokenKey)
                    ,new SqlParameter("@Flag",flag)
                };
                dsResult = await _readWriteDao.SelectRecords("uspUpdateTokenForForexAPI", sqlParameter);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
            return dsResult;
        }

        private async Task ExchangeRates()
        {
            //GenericFunction genericFunction = new GenericFunction();
            //DataSet dsMessageConfig = null;
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
            string apiToken = string.Empty;
            string strError = "Success";
            string InterfaceID = "ForexAPI";
            StringBuilder strCurrency = new StringBuilder();
            string Procedure = "Masters.uspCheckExchangeRates";

            try
            {
                DataSet? dsMessageConfig = await _genericFunction.ExchangeRateCall(Procedure, UTCDatetime);
                if (dsMessageConfig != null && dsMessageConfig.Tables.Count > 0 && dsMessageConfig.Tables[0].Rows.Count > 0)
                {
                    apiToken = await GenerateExchangeAPIToken();
                    if (!string.IsNullOrEmpty(apiToken))
                    {
                        string strRequest = string.Empty;
                        string strResponse = string.Empty;
                        string forexRateAPIURL = string.Empty;
                        string conversionSnapshotID = string.Empty;
                        string baseConversionRates = string.Empty;
                        forexRateAPIURL = ConfigCache.Get("ForexRateAPIURL").Trim();
                        try
                        {
                            string errorDesc = string.Empty;
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                   | SecurityProtocolType.Tls11
                                   | SecurityProtocolType.Tls12
                                   | SecurityProtocolType.Ssl3;
                            var smsClient = new RestClient(forexRateAPIURL);
                            smsClient.Timeout = -1;
                            var request = new RestRequest(Method.GET);
                            request.AddHeader("token", apiToken);
                            request.AddHeader("Authorization", "Bearer " + apiToken);
                            request.AddHeader("Accept", "application/json");
                            request.AddHeader("Content-type", "application/json");

                            IRestResponse forexResponse = smsClient.Execute(request);
                            string str = "[" + forexResponse.Content + "]";
                            string message = string.Empty;
                            StringBuilder apiMessage = new StringBuilder();
                            var obj = JArray.Parse(str);

                            switch (Convert.ToString(forexResponse.StatusCode))
                            {
                                case "Fail":
                                case "Unauthorized":
                                    str = "[" + forexResponse.Content + "]";
                                    obj = JArray.Parse(str);
                                    apiMessage.Append(Convert.ToString(obj[0]["message"]));
                                    break;

                                case "OK":
                                    apiMessage.Append("");
                                    baseConversionRates = Convert.ToString(obj[0]["baseConversionRates"]);
                                    conversionSnapshotID = Convert.ToString(obj[0]["conversionSnapshotId"]);
                                    var forexdict = JsonConvert.DeserializeObject<Dictionary<string, string>>(baseConversionRates);
                                    foreach (var currecny in forexdict)
                                    {
                                        strCurrency.Append("Insert into #tmp values ('");
                                        strCurrency.Append(currecny.Key);
                                        strCurrency.Append("'," + currecny.Value + ");");
                                    }
                                    break;
                                default:
                                    apiMessage.Append(Convert.ToString(obj[0]["message"]));
                                    break;
                            }
                            if (apiMessage.Length > 0)
                                strError = apiMessage.ToString();
                        }
                        catch (Exception Ex) { strError = Ex.Message; }

                        await _genericFunction.CreateExchangeRatesAPI(strCurrency.ToString(), UTCDatetime, UTCDatetime);
                        await _genericFunction.SaveForexAPILog(InterfaceID, forexRateAPIURL, baseConversionRates, conversionSnapshotID, strError);
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    genericFunction = null;
            //    dsMessageConfig = null;
            //}
        }

        public async Task SendUWSMessage(string FlightNumber, string fltDate, string fltOrigin, string fltDest, bool isAuto)
        {
            try
            {
                string MessageVersion = "4";
                DataSet ds = new DataSet();
                string email = string.Empty, msgCommType = string.Empty, message = string.Empty, updatedBy = "Auto ExpToMan";

                //GenericFunction _genericFunction = new GenericFunction();

                string SFTPMessageHeader = string.Empty, messageHeader = string.Empty;

                ds = await _genericFunction.GetSitaAddressandMessageVersion(FlightNumber.Substring(0, 2), "UWS", "AIR", fltOrigin, fltDest, FlightNumber, string.Empty);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    MessageVersion = ds.Tables[0].Rows[0]["MessageVersion"].ToString();
                    email = ds.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                    msgCommType = ds.Tables[0].Rows[0]["MsgCommType"].ToString();


                    if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                    {
                        if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().ToUpper() != "WITHOUT SFTP HEADER")
                            SFTPMessageHeader = _genericFunction.MakeMailMessageFormat(Convert.ToString(ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"]), Convert.ToString(ds.Tables[0].Rows[0]["OriginSenderAddress"]), Convert.ToString(ds.Tables[0].Rows[0]["MessageID"]), ds.Tables[0].Rows[0]["SFTPHeaderType"].ToString());
                        else
                        {
                            SFTPMessageHeader = ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().ToUpper();
                        }
                    }
                    if (ds.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                        messageHeader = _genericFunction.MakeMailMessageFormat(Convert.ToString(ds.Tables[0].Rows[0]["PatnerSitaID"]), Convert.ToString(ds.Tables[0].Rows[0]["OriginSenderAddress"]), Convert.ToString(ds.Tables[0].Rows[0]["MessageID"]), ds.Tables[0].Rows[0]["SITAHeaderType"].ToString());

                }

                //string UWSConfig = _genericFunction.GetConfigurationValues("ShowVolumeInUWSSI");
                string UWSConfig = ConfigCache.Get("ShowVolumeInUWSSI");

                //cls_Encode_Decode clsEncodeDecode = new cls_Encode_Decode();
                message = await _cls_Encode_Decode.EncodeUWS(fltOrigin, FlightNumber, Convert.ToDateTime(fltDate), "1", UWSConfig);

                if (isAuto)
                {
                    if (message.Length > 0)
                    {
                        if (email.Length > 0)
                            await _genericFunction.SaveMessageOutBox("UWS", message, "", email, fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "UWS");

                        if (messageHeader.Trim().Length > 0)
                            await _genericFunction.SaveMessageOutBox("SITA:UWS", messageHeader + "\r\n" + message, "", "SITA", fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "UWS");

                        if (SFTPMessageHeader.Length > 0)
                        {
                            if (SFTPMessageHeader.Trim() == "WITHOUT SFTP HEADER")
                                await _genericFunction.SaveMessageOutBox("SFTP:UWS", message, "", "SFTP", fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "UWS");
                            else
                                await _genericFunction.SaveMessageOutBox("SFTP:UWS", SFTPMessageHeader + "\r\n" + message, "", "SFTP", fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "UWS");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private async Task SendNTMMessage(string FlightNumber, string fltDate, string fltOrigin, string fltDest, bool isAuto)
        {
            try
            {
                // clsLog.WriteLogAzure("SendNTMMessage(): " + FlightNumber + " : " + fltDate + " : " + fltOrigin + " : " + fltDest);
                _logger.LogInformation("SendNTMMessage(): {0} : {1} : {2} : {3}", FlightNumber, fltDate, fltOrigin, fltDest);

                string email = string.Empty, msgCommType = string.Empty, message = string.Empty, aircraftregistrtionno = "", updatedBy = "Auto ExpToMan";

                //GenericFunction genericFunction = new GenericFunction();
                string SFTPMessageHeader = string.Empty, messageHeader = string.Empty;

                DataSet dsMSGAddress = new DataSet();

                dsMSGAddress = await _genericFunction.GetSitaAddressandMessageVersion(FlightNumber.Substring(0, 2), "NTM", "AIR", fltOrigin, fltDest, FlightNumber, string.Empty);

                if (dsMSGAddress != null && dsMSGAddress.Tables[0].Rows.Count > 0)
                {
                    email = dsMSGAddress.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                    msgCommType = dsMSGAddress.Tables[0].Rows[0]["MsgCommType"].ToString();

                    if (dsMSGAddress.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                    {
                        if (dsMSGAddress.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().ToUpper() != "WITHOUT SFTP HEADER")
                            SFTPMessageHeader = _genericFunction.MakeMailMessageFormat(dsMSGAddress.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsMSGAddress.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsMSGAddress.Tables[0].Rows[0]["MessageID"].ToString(), dsMSGAddress.Tables[0].Rows[0]["SITAHeaderType"].ToString());
                        else
                        {
                            SFTPMessageHeader = dsMSGAddress.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().ToUpper();
                        }

                    }
                    if (dsMSGAddress.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                    {
                        messageHeader = _genericFunction.MakeMailMessageFormat(dsMSGAddress.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsMSGAddress.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsMSGAddress.Tables[0].Rows[0]["MessageID"].ToString(), dsMSGAddress.Tables[0].Rows[0]["SITAHeaderType"].ToString());
                    }
                }

                //cls_Encode_Decode clsEncodeDecode = new cls_Encode_Decode();

                message = await _cls_Encode_Decode.EncodeNTM(fltOrigin, FlightNumber, Convert.ToDateTime(fltDate), "", aircraftregistrtionno);

                if (isAuto)
                {
                    if (message.Length > 0)
                    {
                        // clsLog.WriteLogAzure("NTM SFTP Header: " + SFTPMessageHeader + " : " + FlightNumber + " : " + fltDate);
                        _logger.LogInformation("NTM SFTP Header: {0} : {1} : {2}", SFTPMessageHeader, FlightNumber, fltDate);
                        if (email.Length > 0)
                            await _genericFunction.SaveMessageOutBox("NTM", message, "", email, fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "NTM");

                        if (messageHeader.Trim().Length > 0)
                            await _genericFunction.SaveMessageOutBox("SITA:NTM", messageHeader + "\r\n" + message, "", "SITAFTP", fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "NTM");

                        if (SFTPMessageHeader.Trim() == "WITHOUT SFTP HEADER")
                        {
                            await _genericFunction.SaveMessageOutBox("SFTP:NTM", message, "", "SFTP", fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "NTM");
                            // clsLog.WriteLogAzure("NTM Message sent: " + FlightNumber + " : " + fltDate);
                            _logger.LogInformation("NTM Message sent: {0} : {1}", FlightNumber, fltDate);
                        }
                        else if (SFTPMessageHeader.Trim().Length > 0)
                            await _genericFunction.SaveMessageOutBox("SFTP:NTM", SFTPMessageHeader + "\r\n" + message, "", "SFTP", fltOrigin, fltDest, FlightNumber, fltDate, "", updatedBy, "NTM");
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public static bool addMsgToOutBox(string subject, string Msg, string FromEmailID, string ToEmailID, bool isInternal, bool isHTML, string type)
        {
            bool flag = false;
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "spInsertMsgToOutbox";
                cmd.Connection = con;
                SqlParameter[] prm = new SqlParameter[] {
                    new SqlParameter("@Subject",subject)
                    ,new SqlParameter("@Body",Msg)
                    ,new SqlParameter("@FromEmailID",FromEmailID)
                    ,new SqlParameter("@ToEmailID",ToEmailID)
                    ,new SqlParameter("@Type",type)
                    ,new SqlParameter("@IsHTML",isHTML)
                };

                cmd.Parameters.AddRange(prm);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }


        /*Deprecated*/
        //public string uploadtoblob(stream stream, string filename, string containername)
        //{
        //    try
        //    {
        //        containername = containername.tolower();
        //        storagecredentialsaccountandkey cred = new storagecredentialsaccountandkey(getstoragename(), getstoragekey());
        //        servicepointmanager.securityprotocol = securityprotocoltype.tls12;
        //        cloudstorageaccount storageaccount = new cloudstorageaccount(cred, true);
        //        string sas = getsasurl(containername, storageaccount);
        //        storagecredentialssharedaccesssignature sascreds = new storagecredentialssharedaccesssignature(sas);
        //        cloudblobclient sasblobclient = new cloudblobclient(storageaccount.blobendpoint,
        //        new storagecredentialssharedaccesssignature(sas));
        //        cloudblob blob = sasblobclient.getblobreference(containername + @"/" + filename);
        //        blob.properties.contenttype = "";
        //        blob.metadata["filename"] = filename;
        //        blob.uploadfromstream(stream);
        //        return "https://" + getstoragename() + ".blob.core.windows.net/" + containername + "/" + filename;
        //    }
        //    catch (exception ex)
        //    {
        //        clslog.writelogazure(ex);
        //        return "";
        //    }
        //}

        public string UploadToBlob(Stream stream, string fileName, string containerName)
        {
            try
            {
                containerName = containerName.ToLower();

                // Set TLS 1.2 for compatibility
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string storageAccountName = getStorageName();
                string storageKey = getStorageKey();

                // Create BlobServiceClient with account key
                string accountUrl = $"https://{storageAccountName}.blob.core.windows.net";
                var blobServiceClient = new BlobServiceClient(
                    new Uri(accountUrl),
                    new Azure.Storage.StorageSharedKeyCredential(storageAccountName, storageKey)
                );

                // Generate SAS for the container (preserving original GetSASUrl logic)
                string sas = GetSASUrl(containerName, blobServiceClient);

                // Create BlobClient using SAS
                Uri blobSasUri = new Uri($"{accountUrl}/{containerName}/{fileName}?{sas}");
                BlobClient blobClient = new BlobClient(blobSasUri);

                // Set headers (ContentType and Metadata)
                BlobUploadOptions options = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "" // Explicitly empty as in original
                    },
                    Metadata = new Dictionary<string, string>
                {
                    { "FileName", fileName }
                }
                };

                // Upload stream
                stream.Position = 0; // Ensure stream is at start
                blobClient.Upload(stream, options);

                // Return public URL (without SAS)
                return $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{fileName}";
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "";
            }
        }

        /*Deprecated*/
        //public string GetSASUrl(string containerName, CloudStorageAccount storageAccount)
        //{
        //    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        //    CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        //    container.CreateIfNotExist();

        //    BlobContainerPermissions containerPermissions = new BlobContainerPermissions();
        //    GenericFunction genericFunction = new GenericFunction();
        //    string sasactivetime = ConfigCache.Get("BlobStorageactiveSASTime");
        //    double _SaSactiveTime = string.IsNullOrWhiteSpace(sasactivetime) ? 5 : Convert.ToDouble(sasactivetime);

        //    containerPermissions.SharedAccessPolicies.Add("defaultpolicy", new SharedAccessPolicy()
        //    {
        //        SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-1),
        //        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(_SaSactiveTime),
        //        Permissions = SharedAccessPermissions.Write | SharedAccessPermissions.Read | SharedAccessPermissions.List
        //    });

        //    string IsBlobPrivate = ConfigCache.Get("IsBlobPrivate");
        //    IsBlobPrivate = string.IsNullOrWhiteSpace(sasactivetime) ? "NA" : IsBlobPrivate.Trim();
        //    if (IsBlobPrivate == "1")
        //    {
        //        containerPermissions.PublicAccess = BlobContainerPublicAccessType.Off;
        //    }
        //    else
        //    {
        //        containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
        //    }
        //    container.SetPermissions(containerPermissions);
        //    string sas = container.GetSharedAccessSignature(new SharedAccessPolicy(), "defaultpolicy");
        //    return sas;
        //}

        public string GetSASUrl(string containerName, BlobServiceClient blobServiceClient)
        {
            try
            {
                // 1. Get (or create) the container
                BlobContainerClient container = blobServiceClient.GetBlobContainerClient(containerName);
                container.CreateIfNotExists();               // same as container.CreateIfNotExist()

                // 2. Read configuration values
                string sasactivetime = ConfigCache.Get("BlobStorageactiveSASTime");
                double _SaSactiveTime = string.IsNullOrWhiteSpace(sasactivetime) ? 5 : Convert.ToDouble(sasactivetime);

                string isBlobPrivate = ConfigCache.Get("IsBlobPrivate");
                isBlobPrivate = string.IsNullOrWhiteSpace(isBlobPrivate) ? "NA" : isBlobPrivate.Trim();

                // 3. Build the shared-access-policy
                var policy = new BlobSignedIdentifier
                {
                    Id = "defaultpolicy",
                    AccessPolicy = new BlobAccessPolicy
                    {
                        StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
                        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_SaSactiveTime),
                        Permissions = "rwl" // CORRECT: string with r=read, w=write, l=list
                    }
                };

                // 4. Apply public-access setting
                PublicAccessType publicAccess = (isBlobPrivate == "1")
                    ? PublicAccessType.None
                    : PublicAccessType.BlobContainer;   // Container = full public read

                // 5. Set permissions + policy in ONE call
                container.SetAccessPolicy(
                    permissions: new[] { policy },
                    accessType: publicAccess);

                // 6. Generate the SAS token for the *container* using the stored policy
                BlobSasBuilder sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    Resource = "c",                     // container-level SAS
                    Identifier = "defaultpolicy"        // use the stored policy
                };

                // The SDK adds the leading '?' – we strip it to match the old behaviour
                string sas = container.GenerateSasUri(sasBuilder).Query.TrimStart('?');
                return sas;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }

        }

        public async Task<int> DumpInterfaceInformation(string subject, string Msg, DateTime TimeStamp, string MessageType, string ErrorDesc, bool IsBlog,
            string FromEmailId, string ToEmailId, MemoryStream Attachments, string AttachmentExtension, string FileUrl, string isProcessed, string MessageBoxType, string FileUrlExcel = null, MemoryStream attachExcel = null, string AttachmentName = "")
        {
            int SerialNo = 0;

            try
            {


                //SQLServer dtb = new SQLServer();
                //    string[] paramname = new string[] { "Subject",
                //                                    "Body",
                //                                    "TimeStamp",
                //                                    "MessageType",
                //                                    "ErrorDesc",
                //                                    "IsBlog",
                //"FromId", "ToId","Attachment","Extension","FileUrl","isProcessed","MessageBoxType", "AttachmentExcel","FileUrlExcel","AttachmentName"};




                //    object[] paramvalue = new object[] {subject,
                //                                    Msg,
                //                                    TimeStamp,
                //                                    MessageType,
                //                                    ErrorDesc,
                //                                    IsBlog, FromEmailId,ToEmailId, objBytes,AttachmentExtension,FileUrl,isProcessed,MessageBoxType,attachExcel,FileUrlExcel,AttachmentName};

                //    SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.DateTime,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.VarChar,
                //                                         SqlDbType.Bit,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarBinary,SqlDbType.VarChar, SqlDbType.VarChar};

                //objDS = dtb.SelectRecords(procedure, paramname, paramvalue, paramtype);


                string procedure = "uspAddMessageAttachmentDetails";
                DataSet? objDS = null;
                byte[]? objBytes = null;

                if (Attachments != null)
                {
                    objBytes = Attachments.ToArray();
                }
                SqlParameter[] parameters =
                [
                    new("@Subject", SqlDbType.VarChar)         { Value = subject },
                    new("@Body", SqlDbType.VarChar)            { Value = Msg },
                    new("@TimeStamp", SqlDbType.DateTime)      { Value = TimeStamp },
                    new("@MessageType", SqlDbType.VarChar)     { Value = MessageType },
                    new("@ErrorDesc", SqlDbType.VarChar)       { Value = ErrorDesc },
                    new("@IsBlog", SqlDbType.Bit)              { Value = IsBlog },
                    new("@FromId", SqlDbType.VarChar)          { Value = FromEmailId },
                    new("@ToId", SqlDbType.VarChar)            { Value = ToEmailId },
                    new("@Attachment", SqlDbType.VarBinary)    { Value = objBytes },
                    new("@Extension", SqlDbType.VarChar)       { Value = AttachmentExtension },
                    new("@FileUrl", SqlDbType.VarChar)         { Value = FileUrl },
                    new("@isProcessed", SqlDbType.VarChar)     { Value = isProcessed },
                    new("@MessageBoxType", SqlDbType.VarChar)  { Value = MessageBoxType },
                    new("@AttachmentExcel", SqlDbType.VarBinary) { Value = attachExcel },
                    new("@FileUrlExcel", SqlDbType.VarChar)    { Value = FileUrlExcel },
                    new("@AttachmentName", SqlDbType.VarChar)  { Value = AttachmentName }
                ];

                objDS = await _readWriteDao.SelectRecords(procedure, parameters);


                if (objDS != null && objDS.Tables.Count > 0 && objDS.Tables[0].Rows.Count > 0)
                {
                    SerialNo = Convert.ToInt32(objDS.Tables[0].Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                SerialNo = 0;
            }

            return SerialNo;
        }

        private async Task AutoReleaseCapacityAllocation()
        {
            //GenericFunction genericFunction = new GenericFunction();
            //DataSet dsMessageConfig = null;
            //StringBuilder strCurrency = new StringBuilder();
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+8); //ARS time;
            string Procedure = "uspReleaseAllocatedCapacityAtCutoff";

            try
            {
                //dsMessageConfig = genericFunction.AutoReleaseCapacityAllocation(Procedure, 5);
                await _genericFunction.AutoReleaseCapacityAllocation(Procedure, 5);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    genericFunction = null;
            //    dsMessageConfig = null;
            //}
        }

        private async Task NoShowCalculationAsPerAgent()
        {
            //GenericFunction genericFunction = new GenericFunction();
            //DataSet dsMessageConfig = null;
            DateTime UTCDatetime = DateTime.UtcNow.AddHours(+7); //ARS time;
            string Procedure = "uspCalculateNoShowAsPerAgent";

            try
            {
                //dsMessageConfig = _genericFunction.NoShowCalculation(Procedure, UTCDatetime);
                await _genericFunction.NoShowCalculation(Procedure, UTCDatetime);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    genericFunction = null;
            //    dsMessageConfig = null;
            //}
        }
        public static Int32 Next(Int32 minValue, Int32 maxValue)
        {
            try
            {
                if (minValue > maxValue)
                    throw new ArgumentOutOfRangeException("minValue");
                if (minValue == maxValue) return minValue;
                Int64 diff = maxValue - minValue;
                while (true)
                {
                    byte[] randomBytes = new byte[4];
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    rng.GetBytes(randomBytes);
                    int seed = BitConverter.ToInt32(randomBytes, 0);

                    Int64 max = (1 + (Int64)UInt32.MaxValue);
                    Int64 remainder = max % diff;
                    if (seed > 0 && seed < max - remainder)
                    {
                        return (Int32)(minValue + (seed % diff));

                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                _staticLogger?.LogError(ex, $"Error on {MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        private async Task SendDwellTimeInformation()
        {
            //GenericFunction genericFunction = new GenericFunction();
            string Procedure = "uspSendDwellTimeInformation";
            bool blnResult = false;

            try
            {
                blnResult = await _genericFunction.SendInformationtoSP(Procedure);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    genericFunction = null;
            //}
        }

        private async Task Lockuser90Days()
        {
            //GenericFunction genericFunction = new GenericFunction();
            string Procedure = "uspAutoLockuser";
            bool blnResult = false;

            try
            {
                blnResult = await _genericFunction.SendInformationtoSP(Procedure);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            //finally
            //{
            //    genericFunction = null;
            //}
        }
        public async Task SendNotificationAlertBondExpiry()
        {
            try
            {
                //SQLServer db = new SQLServer();
                //db.SelectRecords("uspNotificationAlertBondExpiry");

                await _readWriteDao.SelectRecords("uspNotificationAlertBondExpiry");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }
        public async Task GetPendingNotification()
        {
            try
            {
                //SQLServer db = new SQLServer();
                //dsAWBlist = db.SelectRecords("USPGetPendingNotificationList");

                DataSet? dsAWBlist = new DataSet();
                dsAWBlist = await _readWriteDao.SelectRecords("USPGetPendingNotificationList");

                if (dsAWBlist != null && dsAWBlist.Tables.Count > 0 && dsAWBlist.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drAWBDetails in dsAWBlist.Tables[0].Rows)
                    {
                        await GetAWBPrefix(Convert.ToString(drAWBDetails["AWBPrefix"]), Convert.ToString(drAWBDetails["AWBNumber"]), Convert.ToString(drAWBDetails["Status"]), Convert.ToInt32(drAWBDetails["SerialNumber"]));
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task GetAWBPrefix(string awbPrefix, string awbNumber, string Status, int SerialNumber)
        {
            try
            {
                DataSet? dsAWBDeatils = new DataSet();

                //StringBuilder[] sb = new StringBuilder[0];
                //GenericFunction genericFunction = new GenericFunction();

                string container = "eawb";

                string specifier = string.Empty;
                CultureInfo bz;
                MemoryStream ms = new MemoryStream();

                //SQLServer db = new SQLServer();
                //string[] QueryNames = { "AWBprefix", "AWBNumber" };
                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                //string[] QueryValues = { awbPrefix, awbNumber };

                SqlParameter[] parameters =
                [
                    new("@AWBprefix", SqlDbType.VarChar) { Value = awbPrefix },
                 new("@AWBNumber", SqlDbType.VarChar) { Value = awbNumber }
                ];
                dsAWBDeatils = await _readWriteDao.SelectRecords("SP_GetAWBDetailsPrefix", parameters);

                string AWBNumber = dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim();
                string AWBPrefix = dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim();
                string FLTOrigin = dsAWBDeatils.Tables[3].Rows[0]["FltOrigin"].ToString().Trim();
                string FLTDestination = dsAWBDeatils.Tables[3].Rows[0]["FltDestination"].ToString().Trim();
                int Pices = 0;
                Pices = Convert.ToInt32(dsAWBDeatils.Tables[3].Rows[0]["Pcs"].ToString().Trim());
                decimal Weight = 0;
                Weight = Convert.ToDecimal(dsAWBDeatils.Tables[3].Rows[0]["Wt"].ToString().Trim());
                string Type = "BKDCNFNotification";
                {
                    if (dsAWBDeatils != null && dsAWBDeatils.Tables.Count > 0 && dsAWBDeatils.Tables[0].Rows.Count > 0)
                    {

                        try
                        {
                            DataSet dsBLOB = new DataSet();
                            DataSet? dsnotification = await _genericFunction.GetFlightNotification(AWBPrefix, AWBNumber, Type, Pices, Weight, FLTOrigin, FLTDestination, Status);
                            if (dsnotification != null && dsnotification.Tables.Count > 0 && dsnotification.Tables[0].Rows.Count > 0)
                            {
                                string Toid = dsnotification.Tables[0].Rows[0]["Toid"].ToString().Trim();
                                string Subject = dsnotification.Tables[0].Rows[0]["Subject"].ToString().Trim();
                                string body = dsnotification.Tables[0].Rows[0]["Body"].ToString().Trim();

                                bool IsAgreed = false;
                                string strAgentPreference = string.Empty;

                                strAgentPreference = await _genericFunction.GeteAWBPrintPrefence(dsAWBDeatils.Tables[0].Rows[0]["AgentCode"].ToString().Trim(), dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim(), dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim());

                                if (strAgentPreference.Length < 1 || strAgentPreference == "")
                                    strAgentPreference = "IATA";

                                if (strAgentPreference == "As Agreed" || Convert.ToBoolean(dsAWBDeatils.Tables[0].Rows[0]["Agreed"]) == true)
                                    IsAgreed = true;

                                string DocType = dsAWBDeatils.Tables[0].Rows[0]["DocumentType"].ToString().Trim();
                                string AWBprefix = dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim() + "|" + dsAWBDeatils.Tables[0].Rows[0]["OriginCode"].ToString().Trim() + "|" + dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim();
                                string AirlinePrefix = dsAWBDeatils.Tables[0].Rows[0]["DesigCode"].ToString().Trim();
                                string AWBno = dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim() + "-" + dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim();
                                string AirLineCode = dsAWBDeatils.Tables[0].Rows[0]["DesigCode"].ToString().Trim();
                                string Origin = dsAWBDeatils.Tables[0].Rows[0]["OriginCode"].ToString().Trim();
                                string Dest = dsAWBDeatils.Tables[0].Rows[0]["DestinationCode"].ToString().Trim();
                                string AgentCode = dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentCode"].ToString().Trim();
                                string AgentName = dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentName"].ToString().Trim();
                                string AgentNameOnly = dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentName"].ToString().Trim();
                                string Serviceclass = dsAWBDeatils.Tables[0].Rows[0]["ServiceCargoClassId"].ToString().Trim();
                                string Handlinginfo = dsAWBDeatils.Tables[0].Rows[0]["HandlingInfo"].ToString().Trim();
                                string AccountInfo = dsAWBDeatils.Tables[1].Rows[0]["AccountInfo"].ToString().Trim();
                                string ProductType = string.Empty;

                                ProductType = dsAWBDeatils.Tables[0].Rows[0]["ProductType"].ToString().Trim();
                                string SHCDesc = string.Empty;
                                bool SCHDesc = false;
                                SCHDesc = Convert.ToBoolean(ConfigCache.Get("eAWBSHCDesc"));

                                if (SCHDesc)
                                {
                                    if (dsAWBDeatils.Tables[0].Rows[0]["SHCCodes"].ToString().Trim() != "")
                                    {
                                        SHCDesc = await _genericFunction.GetSHCCodesandDesc(dsAWBDeatils.Tables[0].Rows[0]["SHCCodes"].ToString().Trim());
                                        SHCDesc = SHCDesc.Replace("&amp;", "&");
                                    }
                                }
                                else { SHCDesc = "SHC:" + dsAWBDeatils.Tables[0].Rows[0]["SHCCodes"].ToString().Trim(); }

                                if (Handlinginfo != "")
                                    Handlinginfo = Handlinginfo + " | " + SHCDesc;
                                else
                                    Handlinginfo = SHCDesc;

                                string CommCode = dsAWBDeatils.Tables[1].Rows[0]["CommodityCode"].ToString().Trim();
                                string CommDesc = dsAWBDeatils.Tables[1].Rows[0]["CodeDescription"].ToString().Trim();

                                string Pcs = "0";
                                Pcs = dsAWBDeatils.Tables[0].Rows[0]["PiecesCount"].ToString().Trim();
                                int TotalPcsU = 0;
                                TotalPcsU = Convert.ToInt32(dsAWBDeatils.Tables[0].Rows[0]["PiecesCount"].ToString().Trim());
                                string GrossWgt = Convert.ToDecimal(dsAWBDeatils.Tables[0].Rows[0]["GrossWeight"].ToString().Trim()).ToString("0.00");
                                decimal totalgwt = 0;
                                totalgwt = Convert.ToDecimal(dsAWBDeatils.Tables[0].Rows[0]["GrossWeight"].ToString().Trim());
                                string Volume = "0";
                                try
                                {   //CEBV4-3456 issue added try catch
                                    Volume = Convert.ToDecimal(dsAWBDeatils.Tables[0].Rows[0]["VolumetricWeight"].ToString().Trim()).ToString("0.00");
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                                string ChargeWgt = "0";
                                ChargeWgt = Convert.ToDecimal(dsAWBDeatils.Tables[0].Rows[0]["ChargedWeight"].ToString().Trim()).ToString("0.00");

                                ///function added for total of iata mkt rate on 6 may 12
                                DataSet dsResult = new DataSet("GHA_QuickBooking_30");
                                dsResult = GetChargeSummury(dsAWBDeatils);
                                string frateIATA = "0.0";
                                string frateMKT = "0.0";
                                double ValCharge = 0.0;
                                string PayMode = "";

                                try
                                {
                                    if (dsAWBDeatils.Tables[0].Rows.Count > 0)
                                    {
                                        frateIATA = Convert.ToDouble(dsAWBDeatils.Tables[0].Rows[0][0].ToString()).ToString("0.00");
                                        frateMKT = Convert.ToDouble(dsAWBDeatils.Tables[0].Rows[0][0].ToString()).ToString("0.00");
                                        ValCharge = 0;
                                        PayMode = dsAWBDeatils.Tables[1].Rows[0]["PaymentMode"].ToString();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }

                                string OCDueCar = "";
                                string OCDueAgent = "";

                                OCDueCar = dsResult.Tables[0].Rows[0][2].ToString();
                                OCDueAgent = dsResult.Tables[0].Rows[0][3].ToString();

                                double SpotRate = 0;
                                double DynaRate = 0;
                                double SerTax = Convert.ToDouble(dsResult.Tables[0].Rows[0][4].ToString());
                                double Total = Convert.ToDouble(dsResult.Tables[0].Rows[0][5].ToString());

                                Math.Round(Total, 2);
                                Math.Round((decimal)Total, 2);

                                Math.Round(SpotRate, 2);
                                Math.Round((decimal)SpotRate, 2);

                                Math.Round(DynaRate, 2);
                                Math.Round((decimal)DynaRate, 2);

                                Math.Round(SerTax, 2);
                                Math.Round((decimal)SerTax, 2);

                                string FltOrg = dsResult.Tables[3].Rows[0]["FltOrigin"].ToString();
                                string FltDest = dsResult.Tables[3].Rows[0]["FltDestination"].ToString();

                                #region flt no as per configuration
                                string FltNo = "";
                                string FltDate = "";
                                string SecondFltNo = "";
                                string SecondFltDate = "";
                                string TransitPoint = "";
                                string SenderRefNo, MiscRefNo, BagTagNo, TicketNo;
                                SenderRefNo = MiscRefNo = BagTagNo = TicketNo = string.Empty;
                                bool fltresult = true;
                                //string Systemdateformat ;

                                try
                                {
                                    fltresult = Convert.ToBoolean(ConfigCache.Get("FlightDescInEAWBPrint"));
                                }
                                catch (Exception ex)
                                {
                                    fltresult = true;
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }

                                if (fltresult)
                                {
                                    //DateTime.ParseExact(dsResult.Tables[3].Rows[0]["FltDate"].ToString().Trim(), ConfigCache.Get("SystemDateFormat"), null);
                                    for (int i = 0; i < dsResult.Tables[3].Rows.Count && i < 3; i++)
                                    {
                                        FltNo = FltNo + dsResult.Tables[3].Rows[0]["FltNumber"].ToString() + ",";
                                        FltDate = FltDate + Convert.ToDateTime(dsResult.Tables[3].Rows[0]["FltDate"]).ToString(ConfigCache.Get("SystemDateFormat"), CultureInfo.InvariantCulture) + ",";

                                    }

                                    if (FltNo != "")
                                    {
                                        FltNo = FltNo.Remove(FltNo.Length - 1, 1);
                                    }

                                    if (FltDate != "")
                                    {
                                        FltDate = FltDate.Remove(FltDate.Length - 1, 1);
                                    }
                                }
                                #endregion

                                //For CBV FlightNo & FlightDate
                                if (FltNo.IndexOf(',') > 0 && FltDate.IndexOf(',') > 0)
                                {
                                    string fltNo = FltNo.Split(',')[0];
                                    string fltDate = FltDate.Split(',')[0];
                                    SecondFltNo = FltNo.Split(',')[1];
                                    SecondFltDate = FltDate.Split(',')[1];
                                    FltNo = fltNo;
                                    FltDate = fltDate;
                                }

                                string fstleg = "";
                                string fstlegcarrier = "";
                                string seconleg = "";
                                string seconlegcarrier = "";
                                string thirdleg = "";
                                string thirdlegcarrier = "";
                                for (int i = 0; i < dsResult.Tables[3].Rows.Count; i++)
                                {
                                    switch (i.ToString())
                                    {
                                        case "0":
                                            fstleg = dsResult.Tables[3].Rows[0]["FltDestination"].ToString();
                                            fstlegcarrier = dsResult.Tables[3].Rows[0]["Carrier"].ToString();
                                            break;
                                        case "1":
                                            seconleg = dsResult.Tables[3].Rows[0]["FltDestination"].ToString();
                                            seconlegcarrier = dsResult.Tables[3].Rows[0]["Carrier"].ToString();
                                            break;
                                        case "2":
                                            thirdleg = dsResult.Tables[3].Rows[0]["FltDestination"].ToString();
                                            thirdlegcarrier = dsResult.Tables[3].Rows[0]["Carrier"].ToString();
                                            break;
                                    }
                                }

                                #region handlininfo
                                string HandlingInfo_Extra = "";
                                bool handleres = false;
                                string export = await _genericFunction.checkexportValidation(Origin);
                                if (export == "US")
                                {
                                    if (ConfigCache.Get("Handlinginfo_EAWB") != string.Empty)
                                        handleres = bool.Parse(ConfigCache.Get("Handlinginfo_EAWB"));
                                }

                                if (handleres)
                                    HandlingInfo_Extra = "These commodities,technology or software were exported from the U.S in accordance with Export Administration Regulations";
                                else
                                    HandlingInfo_Extra = "";

                                #endregion

                                bool FFRChecked = false;

                                DataTable DTExportSubDetails = new DataTable("GHA_QuickBooking_158");

                                DTExportSubDetails.Columns.Add("OtherCharges");
                                DTExportSubDetails.Columns.Add("Amount");
                                DTExportSubDetails.Columns.Add("Type");

                                string strOtherCharges = "";

                                DataSet dsOtherDetails = new DataSet("GHA_QuickBooking_31");
                                dsOtherDetails = dsAWBDeatils;

                                if (dsOtherDetails != null && dsOtherDetails.Tables.Count > 0 && dsOtherDetails.Tables[0].Rows.Count > 0)
                                {
                                    //int Intcount = 0;
                                    foreach (DataRow row in dsOtherDetails.Tables[5].Rows)
                                    {
                                        try
                                        {
                                            if (row["ChargeType"].ToString() == "DC" || row["ChargeType"].ToString() == "DA")
                                            {
                                                string strChargeType = string.Empty;
                                                string strChargeCode = row["ChargeHeadCode"].ToString().Trim();

                                                if (strChargeCode.Trim().IndexOf('/') > 0)
                                                    strChargeCode = strChargeCode.Substring(0, strChargeCode.IndexOf("/"));
                                                else
                                                    strChargeCode = strChargeCode.Trim();

                                                if (row["ChargeType"].ToString().Trim() == "DC")
                                                    strChargeType = "Due Carrier";
                                                else
                                                    strChargeType = "Due Agent";

                                                if (IsAgreed)
                                                    DTExportSubDetails.Rows.Add(strChargeCode, "As agreed", strChargeType);
                                                else
                                                    DTExportSubDetails.Rows.Add(strChargeCode, row["Charge"].ToString(), strChargeType);

                                                if (dsOtherDetails.Tables[0].Columns["ChargeHead"] != null)
                                                {
                                                    if (row["ChargeHead"].ToString().Substring(0, row["ChargeHead"].ToString().IndexOf('/')).ToUpper() == "VLC" ||
                                                        row["ChargeHead"].ToString().Substring(0, row["ChargeHead"].ToString().IndexOf('/')).ToUpper().Equals("VL", StringComparison.OrdinalIgnoreCase))
                                                        ValCharge = Convert.ToDouble(row["Charge"].ToString());
                                                    else
                                                        strOtherCharges = strOtherCharges + row["ChargeHead"].ToString().Substring(0, row["ChargeHead"].ToString().IndexOf('/')) + ":" + row["Charge"].ToString() + ", ";
                                                }
                                                else if (dsOtherDetails.Tables[0].Columns["ChargeHeadCode"] != null)
                                                {

                                                    string strChargeCodeVLC = row["ChargeHeadCode"].ToString().Trim();

                                                    if (strChargeCodeVLC.Trim().IndexOf('/') > 0)
                                                        strChargeCodeVLC = strChargeCode.Substring(0, strChargeCodeVLC.IndexOf("/"));
                                                    else
                                                        strChargeCodeVLC = strChargeCodeVLC.Trim();

                                                    if (strChargeCodeVLC.ToUpper() == "VLC" || strChargeCodeVLC.ToUpper() == "VL")
                                                        ValCharge = Convert.ToDouble(row["Charge"].ToString());

                                                    else
                                                    {
                                                        string strCharge = row["ChargeHeadCode"].ToString().Trim();
                                                        if (strCharge.Trim().IndexOf('/') > 0)
                                                            strOtherCharges = strOtherCharges + row["ChargeHeadCode"].ToString().Substring(0, row["ChargeHeadCode"].ToString().IndexOf('/')) + ":" + row["Charge"].ToString() + ", ";
                                                        else
                                                            strOtherCharges = strOtherCharges.Trim();

                                                        //AC-186 changes done
                                                        if (strOtherCharges.Contains("MOA:0"))
                                                            strOtherCharges = strOtherCharges.Replace("MOA:0", "");

                                                        if (strOtherCharges.Contains("MOC:0"))
                                                            strOtherCharges = strOtherCharges.Replace("MOC:0", "");
                                                    }
                                                }
                                            }
                                            if (row[0].ToString().Contains('/'))
                                            {
                                                if (row[0].ToString().ToUpper() != "VAL")
                                                {
                                                    strOtherCharges = strOtherCharges + row[0].ToString().Substring(0, row[0].ToString().IndexOf('/')) + ":" + row[1].ToString() + " , ";
                                                    DTExportSubDetails.Rows.Add(row[0].ToString().Substring(0, row[0].ToString().IndexOf('/')), row[1].ToString(), "Due Carriers");

                                                    strOtherCharges = strOtherCharges + row[1].ToString() + ":" + row[3].ToString() + " , ";

                                                    if (strOtherCharges.Contains("MOA:0,"))
                                                        strOtherCharges = strOtherCharges.Replace("MOA:0,", "");
                                                    if (strOtherCharges.Contains("MOC:0,"))
                                                        strOtherCharges = strOtherCharges.Replace("MOC:0,", "");
                                                }
                                                else
                                                    ValCharge = ValCharge + Convert.ToDouble(row[3].ToString());
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                    }
                                }
                                else
                                {
                                    DTExportSubDetails.Rows.Add("-", "-", "-");
                                }

                                // string AccountInfo = "";
                                string accountnumber = "";
                                bool res = (dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentCode"].ToString().Trim().Contains("WALKIN") || dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentCode"].ToString().Trim().Contains("WALK-IN"));

                                DataSet dsShipmentType = await _genericFunction.GetShipmentTypeNew(Origin, Dest);
                                string shipmentType = string.Empty;
                                if (dsShipmentType != null && dsShipmentType.Tables.Count > 0 && dsShipmentType.Tables[0].Rows.Count > 0)
                                    shipmentType = Convert.ToString(dsShipmentType.Tables[0].Rows[0]["AWBShipmentType"]).Trim();

                                bool res2 = (shipmentType.Equals("ID") || shipmentType.Equals("INT"));

                                if (res == true || res2 == true)
                                {
                                    AccountInfo = "";
                                    accountnumber = "";
                                }
                                else
                                {
                                    AccountInfo = dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentCode"].ToString().Trim();
                                    accountnumber = dsAWBDeatils.Tables[0].Rows[0]["ShippingAgentCode"].ToString().Trim();
                                }

                                if (AccountInfo.Length > 0)
                                {
                                    if (dsAWBDeatils.Tables[1].Rows[0]["AccountInfo"].ToString().Trim().Length > 0)
                                        AccountInfo = AccountInfo + " - " + dsAWBDeatils.Tables[1].Rows[0]["AccountInfo"].ToString().Trim();
                                }
                                else
                                    AccountInfo = dsAWBDeatils.Tables[1].Rows[0]["AccountInfo"].ToString().Trim();


                                string strDimension = "";
                                string prepaid = "";
                                string TotalPrepaid = "";
                                string ExecDate = string.Empty, ExecBy = string.Empty, ExecAT = string.Empty;

                                //// Get AWB Executed At, Executed By, and Execution Date
                                DataSet? dsExec = await _genericFunction.GetAWBExecutionInfo(AWBPrefix, AWBNumber);

                                if (dsExec != null && dsExec.Tables.Count > 0 && dsExec.Tables[0].Rows.Count > 0)
                                {
                                    ExecDate = Convert.ToDateTime(dsExec.Tables[0].Rows[0]["ExecutionDate"]).ToString(ConfigCache.Get("SystemDateFormat"), CultureInfo.InvariantCulture) + " " + Convert.ToString(dsExec.Tables[0].Rows[0]["ExecutionTime"]);
                                    ExecBy = Convert.ToString(dsExec.Tables[0].Rows[0]["ExecutedBy"]);
                                    ExecAT = Convert.ToString(dsExec.Tables[0].Rows[0]["ExecutedAt"]);
                                }
                                //else
                                //{
                                //    ExecDate = txtExecutionDate1.Value.ToString(Convert.ToString(Session["DateFormat"])) + " " + txtExecTime.Text;
                                //    ExecBy = (Session["UpdtBy"] != null && Session["UpdtBy"].ToString() != string.Empty) ? Session["UpdtBy"].ToString() : txtExecutedBy.Text;
                                //    ExecAT = txtExecutedAt.Text;
                                //}

                                // Shipper Name and Address
                                string SAcNo = dsAWBDeatils.Tables[6].Rows[0]["ShipperAccCode"].ToString().Trim();
                                string CAcNo = dsAWBDeatils.Tables[6].Rows[0]["ConsigAccCode"].ToString().Trim();

                                if (SAcNo.Contains("WALKIN") || SAcNo.Contains("Walk-in"))
                                    SAcNo = "";

                                if (CAcNo.Contains("WALKIN") || CAcNo.Contains("Walk-in"))
                                    CAcNo = "";

                                string shipperState = string.Empty;
                                string shipperCountry = string.Empty;
                                string shipperCity = string.Empty;
                                string ShprName = string.Empty, ShrpAddress1 = string.Empty, ShptAddress2 = string.Empty;

                                ShprName = dsAWBDeatils.Tables[6].Rows[0]["ShipperName"].ToString().Trim();
                                ShrpAddress1 = dsAWBDeatils.Tables[6].Rows[0]["ShipperAddress"].ToString().Trim();
                                ShptAddress2 = dsAWBDeatils.Tables[6].Rows[0]["ShipperAdd2"].ToString().Trim();


                                string ShipperName = ShprName + Environment.NewLine + ShrpAddress1;

                                if (!string.IsNullOrEmpty(ShptAddress2))
                                    ShipperName += ", " + ShptAddress2;

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipperState"].ToString().Trim()))
                                    shipperState = dsAWBDeatils.Tables[6].Rows[0]["ShipperState"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipperCountry"].ToString().Trim()))
                                    shipperCountry = Environment.NewLine + dsAWBDeatils.Tables[6].Rows[0]["ShipperCountry"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipperCity"].ToString().Trim()))
                                    ShipperName += Environment.NewLine + dsAWBDeatils.Tables[6].Rows[0]["ShipperCity"].ToString().Trim() + ", ";
                                else if (string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipperCity"].ToString().Trim()))
                                    ShipperName += Environment.NewLine;

                                if (!string.IsNullOrEmpty(shipperState))
                                    ShipperName += shipperState;

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipperPincode"].ToString().Trim()))
                                    ShipperName += " " + dsAWBDeatils.Tables[6].Rows[0]["ShipperPincode"].ToString().Trim();

                                if (!string.IsNullOrEmpty(shipperCountry))
                                    ShipperName += shipperCountry;
                                string Clientname = string.Empty;
                                DataSet dsClientName = new DataSet("dsClientName");
                                dsClientName = await _genericFunction.GetClientName();
                                Clientname = Convert.ToString(dsClientName.Tables[0].Rows[0]["ClientName"]);
                                if (!dsAWBDeatils.Tables[0].Rows[0]["DocumentType"].ToString().Trim().Equals("CBV"))
                                {
                                    if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipperTelephone"].ToString().Trim()) && !Clientname.Contains("VietJet"))
                                        ShipperName += Environment.NewLine + dsAWBDeatils.Tables[6].Rows[0]["ShipperTelephone"].ToString().Trim();
                                }

                                // Consignee name and address
                                string consignerState = string.Empty;
                                string consignerCountry = string.Empty;
                                string consignerCity = string.Empty;

                                string ConsName = string.Empty, ConsAddress1 = string.Empty, ConsAddress2 = string.Empty;

                                ConsName = dsAWBDeatils.Tables[6].Rows[0]["ConsigneeName"].ToString().Trim();
                                ConsAddress1 = dsAWBDeatils.Tables[6].Rows[0]["ConsigneeAddress"].ToString().Trim();
                                ConsAddress2 = dsAWBDeatils.Tables[6].Rows[0]["ConsigneeAddress2"].ToString().Trim();


                                string Consigneename = ConsName + Environment.NewLine + ConsAddress1;

                                if (!string.IsNullOrEmpty(ConsAddress2))
                                    Consigneename += ", " + ConsAddress2;

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsigneeState"].ToString().Trim()))
                                    consignerState = dsAWBDeatils.Tables[6].Rows[0]["ConsigneeState"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsigneeCountry"].ToString().Trim()))
                                    consignerCountry = Environment.NewLine + dsAWBDeatils.Tables[6].Rows[0]["ConsigneeCountry"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsigneeCity"].ToString().Trim()))
                                    Consigneename += Environment.NewLine + dsAWBDeatils.Tables[6].Rows[0]["ConsigneeCity"].ToString().Trim() + ", ";
                                else if (string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsigneeCity"].ToString().Trim()))
                                    Consigneename += Environment.NewLine;

                                if (!string.IsNullOrEmpty(consignerState))
                                    Consigneename += consignerState;

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsigneePincode"].ToString().Trim()))
                                    Consigneename += " " + dsAWBDeatils.Tables[6].Rows[0]["ConsigneePincode"].ToString().Trim();

                                if (!string.IsNullOrEmpty(consignerCountry))
                                    Consigneename += consignerCountry;

                                if (!dsAWBDeatils.Tables[0].Rows[0]["DocumentType"].ToString().Trim().Equals("CBV"))
                                {
                                    if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsigneeTelephone"].ToString().Trim()) && !Clientname.Contains("VietJet"))
                                        Consigneename += Environment.NewLine + dsAWBDeatils.Tables[6].Rows[0]["ConsigneeTelephone"].ToString().Trim();
                                }

                                string ShipperTelephoneNo = dsAWBDeatils.Tables[6].Rows[0]["ShipperTelephone"].ToString().Trim();
                                string ConsigneeTelephoneNo = dsAWBDeatils.Tables[6].Rows[0]["ConsigneeTelephone"].ToString().Trim();
                                string ShipperNameFor = dsAWBDeatils.Tables[6].Rows[0]["ShipperName"].ToString().Trim();

                                string RatePerKg = dsAWBDeatils.Tables[1].Rows[0]["RatePerKg"].ToString().Trim();
                                string SCI = dsAWBDeatils.Tables[0].Rows[0]["SCI"].ToString().Trim();
                                DataTable DTExport = new DataTable("GHA_QuickBooking_159");

                                DTExport.Columns.Add("DocType");
                                DTExport.Columns.Add("AWBPrefix");
                                DTExport.Columns.Add("AWBNo");
                                DTExport.Columns.Add("AirLineCode");
                                DTExport.Columns.Add("Origin");
                                DTExport.Columns.Add("Dest");
                                DTExport.Columns.Add("AgentCode");
                                DTExport.Columns.Add("AgentName");
                                DTExport.Columns.Add("Serviceclass");
                                DTExport.Columns.Add("HandlingInfo");
                                DTExport.Columns.Add("ProductType");
                                DTExport.Columns.Add("CommCode");
                                DTExport.Columns.Add("CommDesc");
                                DTExport.Columns.Add("PCS");
                                DTExport.Columns.Add("GrossWGT");
                                DTExport.Columns.Add("Volume");
                                DTExport.Columns.Add("ChargeWGT");

                                DTExport.Columns.Add("frateIATA");
                                DTExport.Columns.Add("frateMKT");
                                DTExport.Columns.Add("ValCharge");
                                DTExport.Columns.Add("PayMode");
                                DTExport.Columns.Add("OCDueCar");
                                DTExport.Columns.Add("OCDueAgent");
                                DTExport.Columns.Add("SpotRate");
                                DTExport.Columns.Add("DynaRate");
                                DTExport.Columns.Add("SerTax");
                                DTExport.Columns.Add("Total");

                                DTExport.Columns.Add("FltOrg");
                                DTExport.Columns.Add("FltDest");
                                DTExport.Columns.Add("FltNo");
                                DTExport.Columns.Add("FltDate");
                                DTExport.Columns.Add("FFRChecked");
                                DTExport.Columns.Add("ExecDate");
                                DTExport.Columns.Add("ExecBy");
                                DTExport.Columns.Add("ExecAT");

                                DTExport.Columns.Add("ConsigneeName");
                                DTExport.Columns.Add("Prepaid");
                                DTExport.Columns.Add("TotalPrepaid");
                                DTExport.Columns.Add("ShippersName");
                                DTExport.Columns.Add("OtherCharges");
                                DTExport.Columns.Add("Dimension");

                                DTExport.Columns.Add("ShipperAccountNo");
                                DTExport.Columns.Add("ConsigneeAcNo");
                                DTExport.Columns.Add("IssuingCarrierName");
                                DTExport.Columns.Add("AgentIataCode");
                                DTExport.Columns.Add("AccountCode");
                                DTExport.Columns.Add("AccountInformation");

                                DTExport.Columns.Add("ChargesCode");
                                DTExport.Columns.Add("WtVal");
                                DTExport.Columns.Add("watvalother");
                                DTExport.Columns.Add("DeclValCarr");
                                DTExport.Columns.Add("DeclValcustoms");
                                DTExport.Columns.Add("InsAmount");
                                DTExport.Columns.Add("RateClassKG");

                                DTExport.Columns.Add("RateClassN");
                                DTExport.Columns.Add("CommodityItem");
                                DTExport.Columns.Add("NatureOfgoods");
                                DTExport.Columns.Add("Length");
                                DTExport.Columns.Add("Width");
                                DTExport.Columns.Add("Height");

                                DTExport.Columns.Add("collectvalCharge");
                                DTExport.Columns.Add("collecttax");
                                DTExport.Columns.Add("collectDueAgent");
                                DTExport.Columns.Add("CollectDueCarrier");
                                DTExport.Columns.Add("collecttotal");
                                DTExport.Columns.Add("CurrencyRate");

                                DTExport.Columns.Add("CCDestCurrency");
                                DTExport.Columns.Add("ForCarrOnlydest");
                                DTExport.Columns.Add("chargeAtDest");
                                DTExport.Columns.Add("AirlineAddress");

                                DTExport.Columns.Add("AilinePrefix");
                                DTExport.Columns.Add("RatePerKg");
                                DTExport.Columns.Add("AccountInfo");
                                DTExport.Columns.Add("DepartureCity");
                                DTExport.Columns.Add("ArrivalCity");
                                DTExport.Columns.Add("BarCode", System.Type.GetType("System.Byte[]"));
                                DTExport.Columns.Add("CopyType");
                                DTExport.Columns.Add("Logo", System.Type.GetType("System.Byte[]"));
                                DTExport.Columns.Add("CustomerSupportInfo");

                                //New Columns
                                DTExport.Columns.Add("WTPPD");
                                DTExport.Columns.Add("WTCOLL");
                                DTExport.Columns.Add("OtherPPD");
                                DTExport.Columns.Add("OtherCOLL");
                                DTExport.Columns.Add("VLCCollect");
                                DTExport.Columns.Add("PcsULDNo");
                                //new added field
                                DTExport.Columns.Add("SCI");
                                DTExport.Columns.Add("HandlingInfo_Extra");

                                //added columns for shipper and consigneee
                                DTExport.Columns.Add("SAcNo");
                                DTExport.Columns.Add("CAcNo");
                                //added column for 3rd leg destination
                                DTExport.Columns.Add("fstleg");//fstleg
                                DTExport.Columns.Add("seconleg");
                                DTExport.Columns.Add("thirdleg");
                                //for carriercode
                                DTExport.Columns.Add("fstlegcarrier");//fstleg
                                DTExport.Columns.Add("seconlegcarrier");
                                DTExport.Columns.Add("thirdlegcarrier");
                                DTExport.Columns.Add("TotalPcsU");
                                DTExport.Columns.Add("totalgwt");
                                //totalRateUnit
                                DTExport.Columns.Add("totalRateUnit");

                                //tottal chargeable wt

                                DTExport.Columns.Add("TotalChargeWt");
                                //total rate perkg
                                DTExport.Columns.Add("TotalRatePerKg");
                                DTExport.Columns.Add("Dims");
                                DTExport.Columns.Add("totalRateUnitCC");
                                DTExport.Columns.Add("TotalFrtCharge");

                                // New Fields added
                                DTExport.Columns.Add("ShippersTel");
                                DTExport.Columns.Add("ConsigneeTel");
                                DTExport.Columns.Add("ShipperNameFor");
                                DTExport.Columns.Add("SecondFltNo");
                                DTExport.Columns.Add("SecondFltDate");
                                DTExport.Columns.Add("TransitPoint");

                                DTExport.Columns.Add("SenderRefNo");
                                DTExport.Columns.Add("MiscRefNo");
                                DTExport.Columns.Add("BagTagNo");
                                DTExport.Columns.Add("TicketNo");
                                DTExport.Columns.Add("PANNumber");
                                DTExport.Columns.Add("STNumber");
                                DTExport.Columns.Add("HSCodes");

                                DTExport.Columns.Add("SHP");
                                DTExport.Columns.Add("ConsId");

                                string SHP_print = string.Empty;
                                string ConsId = string.Empty;

                                SHP_print = dsAWBDeatils.Tables[6].Rows[0]["ShipUSPassportNum"].ToString().Trim() + dsAWBDeatils.Tables[6].Rows[0]["ShipIDCode"].ToString().Trim();
                                if (SHP_print == " ")
                                    SHP_print = "";
                                else
                                    SHP_print = "ID: " + SHP_print;

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsIDCode"].ToString().Trim()))
                                    ConsId = "ID: " + dsAWBDeatils.Tables[6].Rows[0]["ConsIDCode"].ToString().Trim();
                                else
                                    ConsId = "";

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ShipAEONum"].ToString().Trim()))
                                    ShipperName += Environment.NewLine + "AEO: " + dsAWBDeatils.Tables[6].Rows[0]["ShipAEONum"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["ConsAEONum"].ToString().Trim()))
                                    Consigneename = Consigneename + Environment.NewLine + "AEO: " + dsAWBDeatils.Tables[6].Rows[0]["ConsAEONum"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["NotifyName"].ToString().Trim()))
                                    Consigneename = Consigneename + Environment.NewLine + "Notify Name: " + dsAWBDeatils.Tables[6].Rows[0]["NotifyName"].ToString().Trim();

                                if (!string.IsNullOrEmpty(dsAWBDeatils.Tables[6].Rows[0]["NotifyTelephone"].ToString().Trim()))
                                    Consigneename = Consigneename + Environment.NewLine + "Notify Phone: " + dsAWBDeatils.Tables[6].Rows[0]["NotifyTelephone"].ToString().Trim();

                                DataTable dsDimesionAll = new DataTable("GHA_QuickBooking_32");
                                //dsDimesionAll = GenerateAWBDimensions(txtAWBNo.Text.Trim(), Convert.ToInt32(Pcs), (DataSet)Session["dsDimesionAll"], Convert.ToDecimal(GrossWgt), false, txtAwbPrefix.Text.Trim(), 0, "", false);
                                dsDimesionAll = dsAWBDeatils.Tables[2];
                                DataTable DTvolume = new DataTable("GHA_QuickBooking_160");
                                DTvolume.Columns.Add("CommDesc");
                                DTvolume.Columns.Add("Length");
                                DTvolume.Columns.Add("Width");
                                DTvolume.Columns.Add("Height");
                                DTvolume.Columns.Add("Volume");
                                DTvolume.Columns.Add("PCSCount");

                                float Length = 0; float Breadth = 0; float Height = 0;
                                int dimPCS = 0;
                                string Units = string.Empty;
                                ArrayList arr1 = new ArrayList();
                                if (dsDimesionAll != null && dsDimesionAll.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dsDimesionAll.Rows.Count; i++)
                                    {
                                        dimPCS = 0;

                                        Length = float.Parse(dsDimesionAll.Rows[i]["Length"].ToString());
                                        Breadth = float.Parse(dsDimesionAll.Rows[i]["Breadth"].ToString());
                                        Height = float.Parse(dsDimesionAll.Rows[i]["Height"].ToString());
                                        dimPCS = int.Parse(dsDimesionAll.Rows[i]["PieceNo"].ToString());
                                        Units = dsDimesionAll.Rows[0]["Units"].ToString();
                                        arr1.Add(dsDimesionAll.Rows[i]["ULDNo"].ToString());
                                        if (Length > 0 && Breadth > 0 && Height > 0)
                                        {
                                            Volume = ((Length * Breadth * Height) * dimPCS).ToString("0.00");
                                            DTvolume.Rows.Add(CommDesc, Length, Breadth, Height, Volume, dimPCS);
                                            strDimension = strDimension + "  DIMS: " + Length + " * " + Breadth + " * " + Height + " * " + dimPCS + "  " + Units + " ;    ";
                                        }
                                    }
                                }
                                else
                                {
                                    Length = 0;
                                    Breadth = 0;
                                    Height = 0;

                                    DTvolume.Rows.Add(CommDesc, Length, Breadth, Height, Volume, dimPCS);
                                }

                                string ShipperAccountNo = "";
                                string ConsigneeAcNo = "";
                                string IssuingCarrierName = "";
                                string AgentIataCode = "";
                                string AccountCode = "";
                                string AccountInformation = "";
                                string ChargesCode = dsAWBDeatils.Tables[1].Rows[0]["PaymentMode"].ToString().Trim();
                                string AirlineAddress = "";
                                string PANNumber = "";
                                string STNumber = "";

                                string RateClause = dsAWBDeatils.Tables[7].Rows[0]["RateClass"].ToString().Trim();
                                string OriginCity = "";
                                string DestinationCity = "";
                                string CopyType = string.Empty;
                                string CustomerSupportInfo = "";

                                string PscULDNo = "";


                                //if (Session["PieceTypeULDNo_ArrayList"] != null)
                                //    arr1 = (ArrayList)Session["PieceTypeULDNo_ArrayList"];

                                if (arr1.Count > 0)
                                {
                                    foreach (string li in arr1)
                                    {
                                        PscULDNo += li.ToString() + ",";
                                    }

                                    PscULDNo = PscULDNo.Remove(PscULDNo.Length - 1);
                                }

                                string wtPPD = "", wtCOLL = "", OtherPPD = "", OtherCOLL = "", ClientName = "";
                                if (ChargesCode == "PP" || ChargesCode == "PX")
                                    wtPPD = OtherPPD = "XX";
                                if (ChargesCode == "CC")
                                    wtCOLL = OtherCOLL = "XX";

                                //MasterBAL ObjMsBAl = new MasterBAL();
                                DataSet dsMasterAirline = new DataSet("GHA_QuickBooking_33");
                                //Added by swati
                                dsMasterAirline = await _genericFunction.GetAirlineDetails(Origin, Dest, AirlinePrefix);
                                //ObjMsBAl = null;

                                if (dsMasterAirline != null)
                                {
                                    if (dsMasterAirline.Tables.Count > 0)
                                    {
                                        if (dsMasterAirline.Tables[1].Rows.Count > 0)
                                        {
                                            OriginCity = await _genericFunction.getorg(Origin);

                                            if (dsMasterAirline.Tables[2].Rows.Count > 0)
                                            {
                                                DestinationCity = await _genericFunction.getorg(Dest);
                                                if (dsMasterAirline.Tables[0].Rows.Count > 0)
                                                    CustomerSupportInfo = dsMasterAirline.Tables[0].Rows[0]["CustomerSupportInfo"].ToString();
                                            }
                                        }
                                    }

                                    if (dsMasterAirline.Tables.Count > 0)
                                    {
                                        if (dsMasterAirline.Tables[0].Rows.Count > 0)
                                        {
                                            AirlineAddress = dsMasterAirline.Tables[0].Rows[0][0].ToString() + ", " + dsMasterAirline.Tables[0].Rows[0][1].ToString();
                                            PANNumber = dsMasterAirline.Tables[0].Rows[0]["PANNumber"].ToString();
                                            STNumber = dsMasterAirline.Tables[0].Rows[0]["STNumber"].ToString();
                                            ClientName = dsMasterAirline.Tables[0].Rows[0][0].ToString();
                                        }
                                    }

                                    // Added to get the No. Of eAWB Copies
                                    if (dsMasterAirline.Tables.Count > 0)
                                    {
                                        eAWBPrintArray = new string[dsMasterAirline.Tables[3].Rows.Count];

                                        if (dsMasterAirline.Tables[3].Rows.Count > 0)
                                        {
                                            for (int i = 0; i < dsMasterAirline.Tables[3].Rows.Count; i++)
                                            {
                                                eAWBPrintArray[i] = dsMasterAirline.Tables[3].Rows[i]["eAWBPageName"].ToString();
                                            }
                                        }
                                    }
                                }

                                //CEBV4-3209
                                string? AWBStatus = await _genericFunction.GetAWBStatus(dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim(), dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim());
                                //Session["AWBStatus"] = AWBStatus;

                                if (dsAWBDeatils.Tables[0].Rows[0]["AWBStatus"].ToString().Trim() != null && dsAWBDeatils.Tables[0].Rows[0]["AWBStatus"].ToString().Trim().Equals("B"))
                                {
                                    if (!ClientName.Contains("AirAsia"))
                                        CopyType = "Draft Copy: Updated on " + Convert.ToDateTime(dsAWBDeatils.Tables[0].Rows[0]["UpdatedOn"].ToString().Trim()) + " Printed on " + Convert.ToDateTime(dsAWBDeatils.Tables[0].Rows[0]["UpdatedOn"].ToString().Trim());
                                }
                                else
                                {
                                    CopyType = "Final Copy: Updated on " + Convert.ToDateTime(dsAWBDeatils.Tables[0].Rows[0]["UpdatedOn"].ToString().Trim()) + " Printed on " + Convert.ToDateTime(dsAWBDeatils.Tables[0].Rows[0]["UpdatedOn"].ToString().Trim());
                                    CopyType = "Final Copy: Updated on " + Convert.ToDateTime(dsAWBDeatils.Tables[0].Rows[0]["UpdatedOn"].ToString().Trim()) + " Printed on " + Convert.ToDateTime(dsAWBDeatils.Tables[0].Rows[0]["UpdatedOn"].ToString().Trim());
                                }
                                //Set dv for carriage and customs if blank or 0.
                                float declaredValue = 0;
                                string dvForCarriage = "NVD";
                                if (!float.TryParse(dsAWBDeatils.Tables[0].Rows[0]["DVCarriage"].ToString().Trim(), out declaredValue))
                                    declaredValue = 0;

                                if (declaredValue > 0)
                                    dvForCarriage = dsAWBDeatils.Tables[0].Rows[0]["DVCarriage"].ToString().Trim();
                                else
                                    dvForCarriage = "NVD";

                                declaredValue = 0;
                                string dvForCustoms = "NCV";
                                if (!float.TryParse(dsAWBDeatils.Tables[0].Rows[0]["DVCarriage"].ToString().Trim(), out declaredValue))
                                    declaredValue = 0;
                                if (declaredValue > 0)
                                    dvForCustoms = dsAWBDeatils.Tables[0].Rows[0]["DVCarriage"].ToString().Trim();
                                else
                                    dvForCustoms = "NCV";

                                declaredValue = 0;
                                string InsAmount = "XXX";
                                if (!float.TryParse(dsAWBDeatils.Tables[0].Rows[0]["InsuranceAmount"].ToString().Trim(), out declaredValue))
                                    declaredValue = 0;
                                if (declaredValue > 0)
                                    InsAmount = dsAWBDeatils.Tables[0].Rows[0]["InsuranceAmount"].ToString().Trim();
                                else
                                    InsAmount = "XXX";

                                // HA-373: Get multiple rate lines
                                DataSet? dsRateLog = await _genericFunction.GetAWBRateLog(dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim(), dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim(), IsAgreed, dsAWBDeatils.Tables[0].Rows[0]["UpdatedBy"].ToString().Trim());
                                string UOM = string.Empty;
                                string totalRate = string.Empty;
                                string dims = string.Empty;
                                string totalFrtCharge = string.Empty;
                                string totalTax = string.Empty;
                                string totalAmount = string.Empty;
                                //string Logo = "";

                                //int  pcs1 = 0;
                                //decimal  tgwt = 0;
                                decimal chargewt = 0, TotalChargeWt = 0, TotalRatePerKg = 0, RateKg = 0;//, Totalrt = 0;
                                string RateLogRatePref = "", IncludeOCDCInPrint = "TRUE", HSCodes = "";
                                if (dsRateLog != null && dsRateLog.Tables.Count > 0 && dsRateLog.Tables[0].Rows.Count > 0)
                                {
                                    TotalPcsU = 0;
                                    totalgwt = 0;
                                    Pcs = string.Empty;
                                    GrossWgt = string.Empty;
                                    RateClause = string.Empty;
                                    CommCode = string.Empty;
                                    ChargeWgt = string.Empty;
                                    RatePerKg = string.Empty;

                                    foreach (DataRow dr in dsRateLog.Tables[0].Rows)
                                    {
                                        Pcs += Convert.ToString(dr["Pieces"]) + Environment.NewLine + Environment.NewLine;

                                        GrossWgt += Convert.ToDecimal(dr["GWeight"]).ToString("0.00") + Environment.NewLine + Environment.NewLine;

                                        UOM += Convert.ToString(dr["UOM"]) + Environment.NewLine + Environment.NewLine;

                                        RateClause += Convert.ToString(dr["MKTRateClass"]) + Environment.NewLine + Environment.NewLine;

                                        CommCode += Convert.ToString(dr["CommCode"]) + Environment.NewLine + Environment.NewLine;

                                        ChargeWgt += Convert.ToDecimal(dr["CWeight"]).ToString("0.00") + Environment.NewLine + Environment.NewLine;
                                        chargewt = Convert.ToDecimal(dr["CWeight"]);
                                        TotalChargeWt += chargewt;

                                        if (!Convert.ToString(dr["RatePerKg"]).Equals("As Agreed", StringComparison.OrdinalIgnoreCase))
                                        {
                                            RatePerKg += Convert.ToDecimal(dr["RatePerKg"]).ToString("0.00") + Environment.NewLine + Environment.NewLine;
                                            RateKg = Convert.ToDecimal(dr["RatePerKg"]);
                                            TotalRatePerKg += RateKg;
                                        }
                                        else
                                        {
                                            RatePerKg += Convert.ToString(dr["RatePerKg"]) + Environment.NewLine + Environment.NewLine;
                                        }

                                        if (!Convert.ToString(dr["Total"]).Equals("As Agreed", StringComparison.OrdinalIgnoreCase))
                                            totalRate += Convert.ToDecimal(dr["Total"]).ToString("0.00") + Environment.NewLine + Environment.NewLine;
                                        else
                                            totalRate += Convert.ToString(dr["Total"]) + Environment.NewLine + Environment.NewLine;

                                        dims += Convert.ToString(dr["Dims"]).Replace("|", Environment.NewLine) + Environment.NewLine;
                                    }

                                    try
                                    {
                                        Pcs = Pcs.Substring(0, Pcs.Length - 4);
                                        GrossWgt = GrossWgt.Substring(0, GrossWgt.Length - 4);
                                        UOM = UOM.Substring(0, UOM.Length - 4);
                                        RateClause = RateClause.Substring(0, RateClause.Length - 4);
                                        CommCode = CommCode.Substring(0, CommCode.Length - 4);
                                        ChargeWgt = ChargeWgt.Substring(0, ChargeWgt.Length - 4);
                                        RatePerKg = RatePerKg.Substring(0, RatePerKg.Length - 4);
                                        totalRate = totalRate.Substring(0, totalRate.Length - 4);
                                        dims = dims.Substring(0, dims.Length - 2);

                                        totalFrtCharge = Convert.ToString(dsRateLog.Tables[1].Rows[0]["FrtCharge"]);
                                        //totalTax = Convert.ToString(dsRateLog.Tables[1].Rows[0]["FrtTax"]);
                                        totalTax = dsAWBDeatils.Tables[7].Rows[0]["ServTax"].ToString().Trim();
                                        //totalAmount = Convert.ToString(dsRateLog.Tables[1].Rows[0]["TotalAmount"]);
                                        totalAmount = dsAWBDeatils.Tables[7].Rows[0]["Total"].ToString().Trim();

                                        if (!string.Equals(Convert.ToString(dsRateLog.Tables[1].Rows[0]["TotalAmount"]), "As Agreed", StringComparison.OrdinalIgnoreCase))
                                            Total = Convert.ToDouble(dsRateLog.Tables[1].Rows[0]["TotalAmount"]);

                                        if (!string.Equals(Convert.ToString(dsRateLog.Tables[1].Rows[0]["FrtTax"]), "As Agreed", StringComparison.OrdinalIgnoreCase))
                                            SerTax = Convert.ToDouble(dsRateLog.Tables[1].Rows[0]["FrtTax"]);

                                        // Show pcs and wt from rate log table
                                        TotalPcsU = Convert.ToInt32(dsRateLog.Tables[2].Rows[0]["Pieces"]);
                                        totalgwt = Convert.ToDecimal(dsRateLog.Tables[2].Rows[0]["GrossWeight"]);

                                        //if (!totalTax.Equals("As Agreed"))
                                        //    SerTax = Convert.ToDouble(totalTax);

                                        //if (!totalAmount.Equals("As Agreed"))
                                        //    Total = Convert.ToDouble(totalAmount);

                                        RateLogRatePref = dsRateLog.Tables[1].Rows[0]["RatePreference"].ToString();
                                        HSCodes = "";//dsRateLog.Tables[1].Rows[0]["HSCodes"].ToString();
                                        AgentIataCode = dsRateLog.Tables[1].Rows[0]["AgentIATACode"].ToString();

                                        if (RateLogRatePref.Trim().Equals("As Agreed"))
                                            IsAgreed = true;

                                        IncludeOCDCInPrint = dsRateLog.Tables[1].Rows[0]["IncludeOCChargesINSpot"].ToString();
                                        try
                                        {
                                            if (Convert.ToString(dsRateLog.Tables[1].Rows[0]["AgentCity"]).Length > 0)
                                                AgentName = AgentName + Environment.NewLine + dsRateLog.Tables[1].Rows[0]["AgentCity"].ToString();
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(x); 
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                System.IO.MemoryStream LogoClient = null;
                                //try
                                //{
                                //    Logo = null;
                                //}
                                //catch (Exception ex)
                                //{
                                LogoClient = new System.IO.MemoryStream();
                                //clsLog.WriteLogAzure(ex); ;
                                //}
                                string drpCurrency = " ";
                                drpCurrency = dsAWBDeatils.Tables[7].Rows[0]["Currency"].ToString().Trim();
                                if (IsAgreed == true)
                                {
                                    if (ChargesCode == "PP" || ChargesCode == "PX" || ChargesCode == "PZ")
                                    {
                                        DTExport.Rows.Add(DocType, AWBprefix, AWBno, AirLineCode, Origin, FltDest, AgentCode, AgentName, Serviceclass, Handlinginfo, ProductType,
                                                            CommCode, CommDesc, Pcs, GrossWgt, Volume, ChargeWgt,
                                                            "As Agreed", "As Agreed", "As Agreed", PayMode, "As Agreed", "As Agreed", "As Agreed", "As Agreed", "As Agreed", "As Agreed", FltOrg, Dest, FltNo, FltDate, FFRChecked, ExecDate, ExecBy, ExecAT, Consigneename,
                                                            "As Agreed", "As Agreed", ShipperName, "As Agreed", strDimension, ShipperAccountNo, ConsigneeAcNo, IssuingCarrierName, AgentIataCode, AccountCode, AccountInformation, ChargesCode, "P", "P", dvForCarriage, dvForCustoms, InsAmount,
                                                            UOM, RateClause, CommCode, CommDesc, Length, Breadth, Height, "", "", "", "", "", "", drpCurrency, "", "", AirlineAddress, AirlinePrefix, "As Agreed", AccountInfo, OriginCity, DestinationCity, ms.ToArray(), CopyType, LogoClient.ToArray(),
                                                            CustomerSupportInfo, wtPPD, wtCOLL, OtherPPD, OtherCOLL, "", PscULDNo, SCI, HandlingInfo_Extra, SAcNo, CAcNo, fstleg, seconleg, thirdleg, fstlegcarrier, seconlegcarrier, thirdlegcarrier, TotalPcsU, totalgwt, "As Agreed", TotalChargeWt, TotalRatePerKg, dims, "As Agreed", "As Agreed", ShipperTelephoneNo, ConsigneeTelephoneNo, ShipperNameFor, SecondFltNo, SecondFltDate, TransitPoint, SenderRefNo, MiscRefNo, BagTagNo, TicketNo, PANNumber, STNumber, HSCodes, SHP_print, ConsId);
                                    }
                                    else
                                    {
                                        DTExport.Rows.Add(DocType, AWBprefix, AWBno, AirLineCode, Origin, FltDest, AgentCode, AgentName, Serviceclass, Handlinginfo, ProductType,
                                                            CommCode, CommDesc, Pcs, GrossWgt, Volume, ChargeWgt,
                                                            "", "", "", PayMode, "", "", "", "", "", "", FltOrg, Dest, FltNo, FltDate, FFRChecked, ExecDate, ExecBy, ExecAT, Consigneename,
                                                            "", "", ShipperName, "As Agreed", strDimension, ShipperAccountNo, ConsigneeAcNo, IssuingCarrierName, AgentIataCode, AccountCode, AccountInformation, ChargesCode, "P", "P", dvForCarriage, dvForCustoms, InsAmount,
                                                            UOM, RateClause, CommCode, CommDesc, Length, Breadth, Height, "As Agreed", "As Agreed", "As Agreed", "As Agreed", "As Agreed", "", drpCurrency, "", "", AirlineAddress, AirlinePrefix, "As Agreed", AccountInfo, OriginCity, DestinationCity, ms.ToArray(), CopyType, LogoClient.ToArray(),
                                                            CustomerSupportInfo, wtPPD, wtCOLL, OtherPPD, OtherCOLL, "As Agreed", PscULDNo, SCI, HandlingInfo_Extra, SAcNo, CAcNo, fstleg, seconleg, thirdleg, fstlegcarrier, seconlegcarrier, thirdlegcarrier, TotalPcsU, totalgwt, "As Agreed", TotalChargeWt, TotalRatePerKg, dims, "As Agreed", "As Agreed", ShipperTelephoneNo, ConsigneeTelephoneNo, ShipperNameFor, SecondFltNo, SecondFltDate, TransitPoint, SenderRefNo, MiscRefNo, BagTagNo, TicketNo, PANNumber, STNumber, HSCodes, SHP_print, ConsId);
                                    }
                                }
                                else
                                {
                                    string freight = frateIATA;
                                    if (dsRateLog != null && dsRateLog.Tables.Count > 0 && dsRateLog.Tables[0].Rows.Count > 1)
                                    {
                                        freight = totalRate;
                                    }
                                    else
                                    {
                                        if (strAgentPreference == "IATA")
                                            freight = frateIATA;
                                        else if (strAgentPreference == "MKT" && RateLogRatePref.Equals("SPOT", StringComparison.OrdinalIgnoreCase))
                                            freight = dsAWBDeatils.Tables[7].Rows[0]["SpotFreight"].ToString().Trim();
                                        else if (strAgentPreference == "MKT")
                                            freight = frateMKT;
                                    }

                                    try
                                    {
                                        if (IncludeOCDCInPrint.Trim().ToUpper().Equals("TRUE"))
                                        {
                                            OCDueCar = (Convert.ToDecimal(OCDueCar) - Convert.ToDecimal(ValCharge)).ToString();
                                            OCDueCar = OCDueCar != string.Empty ? Convert.ToDecimal(OCDueCar).ToString() : OCDueCar;

                                            OCDueAgent = OCDueAgent != string.Empty ? Convert.ToDecimal(OCDueAgent).ToString() : OCDueAgent;
                                        }
                                        else
                                        {
                                            OCDueCar = "0";
                                            ValCharge = Convert.ToDouble("0");
                                            OCDueAgent = "0";
                                            strOtherCharges = "";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }


                                    bz = new CultureInfo(ConfigCache.Get("ShowCurrencyFormat"));
                                    specifier = ConfigCache.Get("AllowedDecimalNumber");


                                    string zeroValueFormat = Convert.ToDecimal(0).ToString(specifier, bz);


                                    if (ChargesCode == "PP" || ChargesCode == "PX" || ChargesCode == "PZ")
                                    {
                                        DTExport.Rows.Add(DocType, AWBprefix, AWBno, AirLineCode, Origin, FltDest, AgentCode, AgentName, Serviceclass, Handlinginfo, ProductType,
                                                            CommCode, CommDesc, Pcs, GrossWgt, Volume, ChargeWgt, freight, frateMKT, ValCharge.ToString(specifier, bz), PayMode, OCDueCar, OCDueAgent,
                                            SpotRate.ToString(specifier, bz), DynaRate.ToString(specifier, bz), SerTax.ToString(specifier, bz), Total.ToString(specifier, bz), FltOrg, Dest, FltNo, FltDate, FFRChecked, ExecDate, ExecBy, ExecAT, Consigneename,
                                    prepaid, TotalPrepaid, ShipperName, strOtherCharges, strDimension, ShipperAccountNo, ConsigneeAcNo, IssuingCarrierName, AgentIataCode, AccountCode, AccountInformation, ChargesCode, "P", "P", dvForCarriage, dvForCustoms, InsAmount,
                                                            UOM, RateClause, CommCode, CommDesc, Length, Breadth, Height, zeroValueFormat, zeroValueFormat, zeroValueFormat, zeroValueFormat, zeroValueFormat, "", drpCurrency, "", "", AirlineAddress, AirlinePrefix, RatePerKg, AccountInfo, OriginCity, DestinationCity, ms.ToArray(), CopyType, LogoClient.ToArray(),
                                                        CustomerSupportInfo, wtPPD, wtCOLL, OtherPPD, OtherCOLL, zeroValueFormat, PscULDNo, SCI, HandlingInfo_Extra, SAcNo, CAcNo, fstleg, seconleg, thirdleg, fstlegcarrier, seconlegcarrier, thirdlegcarrier, TotalPcsU, totalgwt, totalFrtCharge, TotalChargeWt, TotalRatePerKg, dims, "", totalFrtCharge, ShipperTelephoneNo, ConsigneeTelephoneNo, ShipperNameFor, SecondFltNo, SecondFltDate, TransitPoint, SenderRefNo, MiscRefNo, BagTagNo, TicketNo, PANNumber, STNumber, HSCodes, SHP_print, ConsId);
                                    }
                                    else
                                    {
                                        DTExport.Rows.Add(DocType, AWBprefix, AWBno, AirLineCode, Origin, FltDest, AgentCode, AgentName, Serviceclass, Handlinginfo, ProductType,
                                                            CommCode, CommDesc, Pcs, GrossWgt, Volume, ChargeWgt,
                                    "", "", "", PayMode, "", "", "", "", "", "", FltOrg, Dest, FltNo, FltDate, FFRChecked, ExecDate, ExecBy, ExecAT, Consigneename,
                                    prepaid, TotalPrepaid, ShipperName, strOtherCharges, strDimension, ShipperAccountNo, ConsigneeAcNo, IssuingCarrierName, AgentIataCode, AccountCode, AccountInformation, ChargesCode, "P", "P", dvForCarriage, dvForCustoms, InsAmount,
                                                            UOM, RateClause, CommCode, CommDesc, Length, Breadth, Height, freight, SerTax.ToString(specifier, bz), OCDueAgent, OCDueCar, Total.ToString(specifier, bz), "", drpCurrency, "", "", AirlineAddress, AirlinePrefix, RatePerKg, AccountInfo, OriginCity, DestinationCity, ms.ToArray(), CopyType, LogoClient.ToArray(),
                                                        CustomerSupportInfo, wtPPD, wtCOLL, OtherPPD, OtherCOLL, ValCharge.ToString(specifier, bz), PscULDNo, SCI, HandlingInfo_Extra, SAcNo, CAcNo, fstleg, seconleg, thirdleg, fstlegcarrier, seconlegcarrier, thirdlegcarrier, TotalPcsU, totalgwt, "", TotalChargeWt, TotalRatePerKg, dims, totalFrtCharge, totalFrtCharge, ShipperTelephoneNo, ConsigneeTelephoneNo, ShipperNameFor, SecondFltNo, SecondFltDate, TransitPoint, SenderRefNo, MiscRefNo, BagTagNo, TicketNo, PANNumber, STNumber, HSCodes, SHP_print, ConsId);
                                    }
                                }

                                // HA-642: added by swati for signature field..
                                DataSet? dsSign = new DataSet("dsEAWBSignature");
                                dsSign = await _genericFunction.CheckIfAWBOnBLOB(dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim() + dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim(), "", "AWBSignature");

                                System.IO.MemoryStream signMemStream = null;

                                if (dsSign != null && dsSign.Tables.Count > 0 && !string.IsNullOrEmpty(Convert.ToString(dsSign.Tables[0].Rows[0]["FileUrl"]).Trim()))
                                {
                                    byte[] sign = null;
                                    sign = _genericFunction.DownloadFromBlob(Convert.ToString(dsSign.Tables[0].Rows[0]["FileUrl"].ToString().Trim()));
                                    signMemStream = (sign == null ? new System.IO.MemoryStream() : new System.IO.MemoryStream(sign));
                                }

                                DataColumn dcSign = new DataColumn("Signature", System.Type.GetType("System.Byte[]"));

                                if (signMemStream != null)
                                    dcSign.DefaultValue = signMemStream.ToArray();

                                DTExport.Columns.Add(dcSign);

                                // Adding new columns for HTML PDF generator
                                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                                string pathhtml = Directory.GetParent(path).Parent.FullName;
                                string logo = "";
                                logo = pathhtml + "//Reports//ClientLogoAK.png";

                                //string logo = (new Uri(HttpContext.Current.Request.Url.AbsoluteUri)).GetLeftPart(UriPartial.Authority) + "//Reports//Client_Logo.png";
                                DataColumn dcLogo = new DataColumn("HTMLLogo", System.Type.GetType("System.String"));
                                dcLogo.DefaultValue = logo;

                                //Added By Niranjan 24/09/2015
                                // Adding new columns for HTML WaterMark -------------------------------------------------------
                                string WaterMark = "";//(new Uri(HttpContext.Current.Request.Url.AbsoluteUri)).GetLeftPart(UriPartial.Authority) + "//Images//WaterMark002.png";
                                DataColumn dcWaterMark = new DataColumn("WaterMark", System.Type.GetType("System.String"));

                                if (ConfigCache.Get("IsWaterMarkPrintEawb") == "true")
                                    dcWaterMark.DefaultValue = WaterMark;
                                else
                                    dcWaterMark.DefaultValue = "";

                                DTExport.Columns.Add(dcWaterMark);
                                // End Of  Added By Niranjan 24/09/2015 --------------------------------------------------------

                                string signUrl = string.Empty;
                                string clientName = dsAWBDeatils.Tables[0].Rows[0]["DesigCode"].ToString().Trim();//Convert.ToString("AirlinePrefix");
                                if (clientName.Trim().ToUpper() == "VJ" || clientName.Trim().ToUpper() == "VZ")
                                {
                                    signUrl = AgentNameOnly;
                                }
                                else if (!string.IsNullOrEmpty(Convert.ToString(dsSign.Tables[0].Rows[0]["FileUrl"].ToString().Trim())))
                                {
                                    string FileUrl = _genericFunction.GetSASBlobUrl(Convert.ToString(dsSign.Tables[0].Rows[0]["FileUrl"].ToString().Trim()));
                                    signUrl = "<img src=\"" + FileUrl + "\" width=\"192px\" height=\"24px\" />";
                                }
                                DataColumn dcSignUrl = new DataColumn("HTMLSignature", System.Type.GetType("System.String"));
                                dcSignUrl.DefaultValue = signUrl;


                                // Barcode
                                DataColumn dcBarCode = new DataColumn("HTMLBarCode", System.Type.GetType("System.String"));
                                string barCodeUrl = string.Empty;

                                DataSet? dsBarCode = await _genericFunction.CheckIfAWBOnBLOB(dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim() + dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim(), "", "barcode");

                                if (dsBarCode != null && dsBarCode.Tables.Count > 0 && !string.IsNullOrEmpty(Convert.ToString(dsBarCode.Tables[0].Rows[0]["FileUrl"]).Trim()))
                                {
                                    barCodeUrl = dsBarCode.Tables[0].Rows[0]["fileurl"].ToString().Trim();
                                    barCodeUrl = _genericFunction.GetSASBlobUrl(barCodeUrl);
                                }
                                else
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    barCodeUrl = _genericFunction.UploadToBlob(ms, dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim() + "_" + dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim() + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png", "barcode");
                                    await _genericFunction.CheckIfAWBOnBLOB(dsAWBDeatils.Tables[0].Rows[0]["AWBPrefix"].ToString().Trim() + dsAWBDeatils.Tables[0].Rows[0]["AWBNumber"].ToString().Trim(), barCodeUrl, "barcode");
                                    barCodeUrl = _genericFunction.GetSASBlobUrl(barCodeUrl);
                                }

                                if (ConfigCache.Get("ShowBarCodeInEAWBPrint") == "1")
                                    dcBarCode.DefaultValue = "<img src=\"" + barCodeUrl + "\"  width=\"192px\" height=\"24px\" />";
                                else
                                    dcBarCode.DefaultValue = "";

                                // WaterMark for collect shipment
                                string waterMark = ""; //(new Uri(HttpContext.Current.Request.Url.AbsoluteUri)).GetLeftPart(UriPartial.Authority) + "/Images/CollectWaterMark.png";
                                DataColumn dcCCWaterMark = new DataColumn("CCWaterMark", System.Type.GetType("System.String"));
                                DataColumn dcDraftCopy = new DataColumn("DraftCopy", System.Type.GetType("System.String"));
                                int ddlServiceclass;
                                ddlServiceclass = Convert.ToInt32(dsAWBDeatils.Tables[0].Rows[0]["ServiceCargoClassId"]);
                                if (ChargesCode.Equals("CC") || ChargesCode.Equals("CZ") && ddlServiceclass != 0)
                                    dcCCWaterMark.DefaultValue = waterMark;
                                else
                                    dcCCWaterMark.DefaultValue = "";

                                if (ddlServiceclass == 0)
                                {
                                    string VoidWaterMark = "";// (new Uri(HttpContext.Current.Request.Url.AbsoluteUri)).GetLeftPart(UriPartial.Authority) + "/Images/VoidWaterMark.png";
                                    dcCCWaterMark = new DataColumn("CCWaterMark", System.Type.GetType("System.String"));
                                    dcCCWaterMark.DefaultValue = VoidWaterMark;
                                    dcDraftCopy.DefaultValue = VoidWaterMark;
                                }

                                dcDraftCopy.DefaultValue = "";
                                if (dsAWBDeatils.Tables[0].Rows[0]["AWBStatus"].ToString().Trim() != null && dsAWBDeatils.Tables[0].Rows[0]["AWBStatus"].ToString().Trim().Equals("B") && ddlServiceclass != 0)
                                {
                                    string DraftWaterMark = "";// (new Uri(HttpContext.Current.Request.Url.AbsoluteUri)).GetLeftPart(UriPartial.Authority) + "/Images/cebudraft.png";
                                    dcDraftCopy.DefaultValue = DraftWaterMark;
                                }

                                DTExport.Columns.Add(dcLogo);
                                DTExport.Columns.Add(dcSignUrl);
                                DTExport.Columns.Add(dcBarCode);
                                DTExport.Columns.Add(dcCCWaterMark);
                                DTExport.Columns.Add(dcDraftCopy);

                                string HTMLData = string.Empty;
                                // Generate PDF from Html or RDLC based on config
                                if (ConfigCache.Get("EAWBHTMLPrint") == "1")
                                    HTMLData = await RenderReportHtml(DTExport, dsRateLog, dsAWBDeatils.Tables[0].Rows[0]["DocumentType"].ToString().Trim(), dsDimesionAll);

                                HtmlToPdfConverter htmlToPdfConverter = null;
                                //try
                                //{
                                htmlToPdfConverter = new HtmlToPdfConverter();
                                var margins = new PageMargins();
                                margins.Bottom = 0;
                                margins.Top = 5; // Changed by Sainyam on 24JAN2018 for HA
                                margins.Left = 6;
                                margins.Right = 5;
                                htmlToPdfConverter.Margins = margins;
                                DateTime TimeStamp = DateTime.Now;
                                var pdfBytes = htmlToPdfConverter.GeneratePdf(HTMLData);
                                ms = new MemoryStream(pdfBytes);
                                //string FileExcelURL = "";


                                string fileUrl = _genericFunction.UploadToBlob(ms, AWBPrefix + "_" + AWBNumber + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + "_DFC.pdf", container);
                                await _genericFunction.CheckIfAWBOnBLOB(AWBPrefix + AWBNumber, fileUrl, container);
                                await DumpInterfaceInformation(Subject, body, TimeStamp, "BKDCNFNotification", "", true, "", Toid, ms, ".pdf", fileUrl, "0", "Outbox", "", null, AWBPrefix + "_" + AWBNumber + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + "_DFC");
                                await GetUpdateNotification(AWBPrefix, AWBNumber, SerialNumber);

                                try
                                {
                                    if (dsResult != null)
                                        dsResult.Dispose();
                                    if (DTExport != null)
                                        DTExport.Dispose();
                                    if (DTExportSubDetails != null)
                                        DTExportSubDetails.Dispose();
                                    if (DTvolume != null)
                                        DTvolume.Dispose();
                                    if (dsDimesionAll != null)
                                        dsDimesionAll.Dispose();
                                    if (dsOtherDetails != null)
                                        dsOtherDetails.Dispose();
                                }
                                catch (Exception ex)
                                {

                                    dsResult = null;
                                    DTExport = null;
                                    DTExportSubDetails = null;
                                    DTvolume = null;
                                    dsDimesionAll = null;
                                    dsOtherDetails = null;
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public DataSet GetChargeSummury(DataSet dsAWBDeatils)
        {
            try
            {
                DataTable dtRates = new DataTable("GHA_QuickBooking_19");
                //dtRates = dsAWBDeatils.Tables[5];
                DataTable dsDetails = new DataTable("GHA_QuickBooking_20");
                //dsDetails = dsAWBDeatils.Tables[7];

                // Rates
                decimal frtiata, frtmkt, ocdc, ocda, tax, alltotal;
                frtiata = frtmkt = ocdc = ocda = tax = alltotal = 0;

                DataSet dsRates = new DataSet("GHA_QuickBooking_21");
                string errormessage = string.Empty;


                if (dsAWBDeatils != null && dsAWBDeatils.Tables.Count > 6 && dsAWBDeatils.Tables[7].Rows.Count > 0)
                {
                    dtRates = dsAWBDeatils.Tables[7].Copy();
                    //Session["dtRates"] = dsRates.Tables[7].Copy();
                    //dsRates = null;
                }


                if (dtRates != null)
                {
                    foreach (DataRow rw in dtRates.Rows)
                    {
                        frtiata += decimal.Parse(rw["FrIATA"].ToString());
                        frtmkt += decimal.Parse(rw["FrMKT"].ToString());
                        ocda += decimal.Parse(rw["OcDueAgent"].ToString());
                        ocdc += decimal.Parse(rw["OcDueCar"].ToString());
                        tax += decimal.Parse(rw["ServTax"].ToString());
                        alltotal += decimal.Parse(rw["Total"].ToString());
                    }
                }

                //DataRow dsResultRow = dsAWBDeatils.Tables[7].NewRow();

                //dsResultRow["FrIATA"] = frtiata;
                //dsResultRow["FrMKT"] = frtmkt;
                ////dsResultRow["OCDC"] = ocdc;
                ////dsResultRow["OCDA"] = ocda;
                //dsResultRow["ServTax"] = tax;
                //dsResultRow["Total"] = alltotal;

                //dsAWBDeatils.Tables[7].Rows.Add(dsResultRow);

                ArrayList ChargeHeads = new ArrayList();
                //dsDetails = dsAWBDeatils.Tables[5].Copy();
                if (dsAWBDeatils != null && dsAWBDeatils.Tables.Count > 0 && dsAWBDeatils.Tables[5].Rows.Count > 0)
                {
                    foreach (DataRow row in dsAWBDeatils.Tables[5].Rows)
                    {
                        if (!ChargeHeads.Contains(row["ChargeHeadCode"].ToString()))
                            ChargeHeads.Add(row["ChargeHeadCode"].ToString());
                    }

                    if (ChargeHeads != null && ChargeHeads.Count > 0)
                    {
                        for (int i = 0; i < ChargeHeads.Count; i++)
                        {
                            decimal total = 0;
                            foreach (DataRow rw in dsAWBDeatils.Tables[5].Rows)
                            {
                                if (rw["ChargeHeadCode"].ToString() == ChargeHeads[i].ToString())
                                    total += decimal.Parse(rw["Charge"].ToString());
                            }

                            DataRow newrow = dsAWBDeatils.Tables[5].NewRow();
                            newrow["ChargeHeadCode"] = ChargeHeads[i].ToString();
                            newrow["Charge"] = "" + total;

                            dsAWBDeatils.Tables[5].Rows.Add(newrow);
                        }
                    }
                }

                return dsAWBDeatils;
            }
            catch (Exception ex)
            {
                dsAWBDeatils = null;
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return null;
            }
        }

        private async Task<string> RenderReportHtml(DataTable dtTable, DataSet dsRateLog, string documentType, DataTable dsDimesionAll)
        {
            try
            {
                string AWBNo = dtTable.Rows[0][2].ToString();
                string[] AWBPrefix = AWBNo.Split('-');
                string rateInfo = string.Empty;
                int maxCommDescLen = 0;

                //GenericFunction genericFunction = new GenericFunction();

                string specifier = string.Empty;
                CultureInfo bz;
                bz = new CultureInfo(ConfigCache.Get("ShowCurrencyFormat"));
                specifier = ConfigCache.Get("AllowedDecimalNumber");
                // Read HTML in string
                StringReader htmlFile = new StringReader("");

                bool IsAgreed = false;
                string strAgentPreference = string.Empty;

                strAgentPreference = await this._genericFunction.GeteAWBPrintPrefence(dtTable.Rows[0]["AgentCode"].ToString(), dtTable.Rows[0]["AWBno"].ToString(), dtTable.Rows[0]["AWBPrefix"].ToString());

                if (strAgentPreference.Length < 1 || strAgentPreference == "")
                    strAgentPreference = "IATA";


                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
                string pathhtml = Directory.GetParent(path).Parent.FullName;
                htmlFile = new StringReader(File.ReadAllText(pathhtml + "/Reports/EAWB.html"));
                //htmlFile = new StringReader(File.ReadAllText(Server.MapPath("~/Reports/EAWB.html")));
                //}

                string htmlContent = string.Format(htmlFile.ReadToEnd());


                //-------- SQF Number Added by Nitin for CEBU

                string SQFName = "";
                SQFName = ConfigCache.Get("QSFPrintEAWB");
                if (!string.IsNullOrEmpty(SQFName))
                {
                    string[] strArray = SQFName.Split('|');
                    htmlContent = htmlContent.Replace("@QSF1@", strArray[0].ToString());
                    htmlContent = htmlContent.Replace("@QSF2@", strArray[1].ToString());
                    htmlContent = htmlContent.Replace("@QSF3@", strArray[2].ToString());
                }
                else
                {
                    htmlContent = htmlContent.Replace("@QSF1@", string.Empty);
                    htmlContent = htmlContent.Replace("@QSF2@", string.Empty);
                    htmlContent = htmlContent.Replace("@QSF3@", string.Empty);
                }
                //-----------Nitin End----
                // Change Shipper/Consignee
                dtTable.Rows[0]["ShippersName"] = Convert.ToString(dtTable.Rows[0]["ShippersName"]).Replace(Environment.NewLine, "<br />");
                dtTable.Rows[0]["ConsigneeName"] = Convert.ToString(dtTable.Rows[0]["ConsigneeName"]).Replace(Environment.NewLine, "<br />");

                #region  Replace 0.00 values to null for JetAirways requirement added by manoj on 17-10-2015
                try
                {
                    string[] Item_list = new string[] { "collecttotal", "totalRateUnitcc", "vlccollect", "collectduecarrier", "collectdueagent", "collecttax" };
                    for (int i = 0; i < Item_list.Length; i++)
                    {
                        string item_val = Item_list[i].ToString();
                        decimal OutResult;
                        if (dtTable.Rows[0][item_val].ToString() != "" && dtTable.Rows[0][item_val].ToString() != null && decimal.TryParse(dtTable.Rows[0][item_val].ToString(), out OutResult))
                        {
                            if (Convert.ToDouble(dtTable.Rows[0][item_val]) == 0.00 || Convert.ToDouble(dtTable.Rows[0][item_val]) <= 0 || dtTable.Rows[0][item_val].ToString().Equals("0.00", StringComparison.OrdinalIgnoreCase))
                            {
                                dtTable.Rows[0][item_val] = "";
                                dtTable.AcceptChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ShowMessage(ref lblStatus, skResourceManager.GetString("msgErrorPrintingeAWB", skCultureInfo), MessageType.ErrorMessage);
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                #endregion

                htmlContent = htmlContent.Replace("@IATAAgentCode@", dtTable.Rows[0]["AgentCode"].ToString());

                // Map data in HTML
                foreach (DataColumn dc in dtTable.Columns)
                    htmlContent = htmlContent.Replace("@" + dc.ColumnName + "@", Convert.ToString(dtTable.Rows[0][dc.ColumnName]));

                DataSet dsClientName = new DataSet("dsClientName");
                dsClientName = await _genericFunction.GetClientName();
                int maxDimsRowsOnAWBPrint = 0;
                Int32.TryParse(ConfigCache.Get("MaxDimsRowsOnAWBPrint"), out maxDimsRowsOnAWBPrint);
                string DimsData = string.Empty;

                decimal dcTotalVal = 0;
                // Map rate details
                if (dsRateLog != null && dsRateLog.Tables.Count > 0 && dsRateLog.Tables[0].Rows.Count > 0)
                {
                    for (int rowCount = 0; rowCount < dsRateLog.Tables[0].Rows.Count; rowCount++)
                    {
                        try
                        {
                            string ProductType = "";
                            decimal num = 0;
                            if (Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Total"]) == "As Agreed")
                                num = 0;
                            else
                                num = Convert.ToDecimal(dsRateLog.Tables[0].Rows[rowCount]["Total"]);

                            ProductType = dtTable.Rows[0]["ProductType"].ToString();

                            string Val = await this._genericFunction.GetRoundoffvalueSingle(dtTable.Rows[0]["AgentCode"].ToString(), dtTable.Rows[0]["AgentCode"].ToString(), "SCM_FREIGHT", num.ToString(), dtTable.Rows[0]["AgentCode"].ToString(),
                               dtTable.Rows[0]["ShippersName"].ToString(), dtTable.Rows[0]["AgentCode"].ToString(), dtTable.Rows[0]["AgentCode"].ToString(), ProductType, dtTable.Rows[0]["CCDestCurrency"].ToString(), dtTable.Rows[0]["AgentCode"].ToString());//Currency Added By kalyani on 17 Mar 2017.Jira SC-1010 For IATA Currency Rounding;
                            if (Decimal.TryParse(Val, out num))
                                dcTotalVal = Convert.ToDecimal(Val);

                            //objProcessRate = null;
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }

                        string tmpTotalVal = string.Empty, tmpRatePerKG = string.Empty;
                        if (IsAgreed)
                        {
                            tmpRatePerKG = "0.00";
                            tmpTotalVal = "0.00";
                        }
                        else
                        {
                            tmpRatePerKG = Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["RatePerKg"]);
                            tmpTotalVal = dcTotalVal > 0 ? dcTotalVal.ToString(specifier, bz) : Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Total"]);
                        }

                        if (documentType.Equals("CBV"))
                        {
                            rateInfo += "<tr>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Pieces"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["GWeight"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["UOM"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:5%; border-left: 0px;\">0</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:10%; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["CWeight"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + tmpRatePerKG + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + Convert.ToString(dtTable.Rows[0]["CCDestCurrency"]) + "</td>";
                            //string tmpTotalVal = dcTotalVal > 0 ? dcTotalVal.ToString(specifier, bz) : Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Total"]);
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + tmpTotalVal + "</td>";
                            if (Convert.ToString(dsClientName.Tables[0].Rows[0]["ClientName"]).Contains("AirAsia") &&
                                maxDimsRowsOnAWBPrint > 0 &&
                                Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Split(';').Length - 1 > maxDimsRowsOnAWBPrint)
                            {
                                rateInfo += "<td valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; height: 100px; word-break:break-all;\">Refer to next page for dimension details</td>";
                                DimsData += Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]);
                            }
                            else if (Convert.ToString(dsClientName.Tables[0].Rows[0]["ClientName"]).Contains("VietJet"))
                            {
                                if (maxDimsRowsOnAWBPrint > 0 && Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Split(';').Length - 1 > maxDimsRowsOnAWBPrint)
                                {
                                    rateInfo += "<td  valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; height: 100px; word-break:break-all;\">Refer to next page for dimension details</td>";
                                    DimsData += Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Replace("|", "<br/>").Replace(";", "<br/>");
                                }
                                else
                                {
                                    rateInfo += "<td valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; word-break:break-all;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Replace("|", "<br/>").Replace(";", "<br/>") + "</td>";
                                }
                            }
                            else
                                rateInfo += "<td valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; word-break:break-all;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]) + "</td>";
                            rateInfo += "</tr>";
                        }
                        else
                        {
                            rateInfo += "<tr>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Pieces"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["GWeight"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["UOM"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:5%; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["MKTRateClass"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:10%; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["CommCode"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["CWeight"]) + "</td>";
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + tmpRatePerKG + "</td>";
                            //string tmpTotalVal = dcTotalVal > 0 ? dcTotalVal.ToString(specifier, bz) : Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Total"]);
                            rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + tmpTotalVal + "</td>";
                            if (Convert.ToString(dsClientName.Tables[0].Rows[0]["ClientName"]).Contains("AirAsia") &&
                                maxDimsRowsOnAWBPrint > 0 &&
                                Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Split(';').Length - 1 > maxDimsRowsOnAWBPrint)
                            {
                                rateInfo += "<td  valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; height: 100px; word-break:break-all;\">Refer to next page for dimension details</td>";
                                DimsData += Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Replace(Environment.NewLine, "<br/>");
                            }
                            else if (Convert.ToString(dsClientName.Tables[0].Rows[0]["ClientName"]).Contains("VietJet"))
                            {
                                if (maxDimsRowsOnAWBPrint > 0 && Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Split(';').Length - 1 > maxDimsRowsOnAWBPrint)
                                {
                                    rateInfo += "<td  valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; height: 100px; word-break:break-all;\">Refer to next page for dimension details</td>";
                                    DimsData += Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Replace("|", "<br/>").Replace(";", "<br/>");
                                }
                                else
                                {
                                    rateInfo += "<td valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; word-break:break-all;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Replace("|", "<br/>").Replace(";", "<br/>") + "</td>";
                                }
                            }
                            else
                                rateInfo += "<td  valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; word-break:break-all;\">" + Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Replace("|", "<br/>") + "</td>";
                            rateInfo += "</tr>";
                        }

                        if (maxCommDescLen < Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Length)
                            maxCommDescLen = Convert.ToString(dsRateLog.Tables[0].Rows[rowCount]["Dims"]).Length;
                    }
                    #region SLACULDDetails 
                    // Added to show SLACULDDetails on the report without rate calculation
                    //DataSet dsAwbDimensions = new DataSet();
                    if (!string.IsNullOrEmpty(dtTable.Rows[0]["AWBNo"].ToString()) && !string.IsNullOrEmpty(dtTable.Rows[0]["AWBPrefix"].ToString()))
                    {
                        //DataTable dtDimensions = new DataTable("dtAWBDimensions");
                        try
                        {
                            //dsAwbDimensions = genericFunction.GetAWBDimensions(dtTable.Rows[0]["AWBNo"].ToString(), dtTable.Rows[0]["AWBPrefix"].ToString());
                            if (dsDimesionAll != null && dsDimesionAll.Rows.Count > 0)
                            {
                                //dtDimensions = (DataTable)dsAwbDimensions.Tables[0];
                                var dr = dsDimesionAll.AsEnumerable().Where(r => r.Field<string>("PieceType").Trim() == "SLAC");
                                if (dr != null && dr.ToList().Count > 0)
                                {
                                    foreach (DataRow row in dr)
                                    {
                                        rateInfo += "<tr>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:5%; border-left: 0px;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:10%; border-left: 0px;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + "</td>";
                                        rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">" + "</td>";
                                        string strSLACULDPcsWt = Convert.ToString(row["ULDNo"]) + "|" + Convert.ToString(row["PcsCount"]) + "|" + Convert.ToString(row["GrossWt"]);
                                        rateInfo += "<td  valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px; width:200px; word-break:break-all;\">" + strSLACULDPcsWt + "</td>";
                                        rateInfo += "</tr>";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        finally
                        {
                            if (dsDimesionAll != null)
                                dsDimesionAll.Dispose();
                        }
                    }
                    #endregion SLACULDDetails

                    if (!documentType.Equals("CBV"))
                    {
                        if (maxCommDescLen <= 200)
                        {
                            int j = 5;
                            if (maxCommDescLen >= 145)
                            {
                                j = 3;
                            }
                            for (int rowCount = 0; rowCount < (j - dsRateLog.Tables[0].Rows.Count); rowCount++)
                            {
                                rateInfo += "<tr>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000;\">&nbsp;</td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\" border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">&nbsp;</td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">&nbsp;</td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:5%; border-left: 0px;\"></td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; width:10%; border-left: 0px;\"></td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">&nbsp;</td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">&nbsp;</td>";
                                rateInfo += "<td align=\"center\" valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">&nbsp;</td>";
                                rateInfo += "<td valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; border-left: 0px;\">&nbsp;</td>";
                                rateInfo += "</tr>";
                            }
                        }
                    }
                }

                htmlContent = htmlContent.Replace("@RateDetails@", rateInfo);
                htmlContent = htmlContent.Replace("@AccountInfoNew@", "");
                if (dsRateLog != null)
                {
                    // Map total values
                    htmlContent = htmlContent.Replace("@TotalPieces@", Convert.ToString(dsRateLog.Tables[2].Rows[0]["Pieces"]));   //'dsRateLog' is null on at least one execution path.
                    htmlContent = htmlContent.Replace("@TotalGrossWeight@", Convert.ToString(dsRateLog.Tables[2].Rows[0]["GrossWeight"]));
                    htmlContent = htmlContent.Replace("@TotalFrtCharge@", Convert.ToString(dsRateLog.Tables[1].Rows[0]["FrtCharge"]));


                    // Added By Niranjan 23-9-2015
                    if (Convert.ToString(dsRateLog.Tables[1].Rows[0]["AccountInfo"]) != "")
                        htmlContent = htmlContent.Replace("@AccountInfoNew@", " - " + Convert.ToString(dsRateLog.Tables[1].Rows[0]["AccountInfo"]));
                }

                //Code to Create Multiple AWB Copies with Dynamic Data
                string HTMLData = string.Empty;
                string DimsHTMLData = string.Empty;

                if (!string.IsNullOrEmpty(DimsData))
                {
                    DimsHTMLData += "<table width=\"99%\" align=\"center\" border=\"0\" style=\"border-top:1px solid #000000; border-bottom:1px solid #000000\" cellspacing=\"0\" cellpadding=\"1\">";
                    DimsHTMLData += "<tr><td valign=\"top\" style=\"border:1px solid #000000; border-top:0px;\"><strong>Nature and Quantity of Goods<br />(incl. Dimensions or Volume)</strong><br/></td></tr>";
                    DimsHTMLData += "<tr><td valign=\"top\" style=\"border-right:1px solid #000000; border-left:1px solid #000000; word-break:break-all;\">" + DimsData + "</td></tr>";
                    DimsHTMLData += "</table>";
                }

                if (eAWBPrintArray != null)
                {
                    for (int i = 0; i < eAWBPrintArray.Length; i++)
                    {
                        HTMLData = HTMLData + htmlContent;
                        HTMLData = HTMLData.Replace("@CopyPageName@", eAWBPrintArray[i].ToString());

                        if (i != eAWBPrintArray.Length - 1)
                            HTMLData = HTMLData + "<div style=\"page-break-after:always\"></div>";

                        if (!string.IsNullOrEmpty(DimsData))
                        {
                            if (i == eAWBPrintArray.Length - 1)
                                HTMLData = HTMLData + "<div style=\"page-break-after:always\"></div>";
                            HTMLData = HTMLData + DimsHTMLData;
                            if (i != eAWBPrintArray.Length - 1)
                                HTMLData = HTMLData + "<div style=\"page-break-after:always\"></div>";
                        }
                    }

                    if (Convert.ToString(dsClientName.Tables[0].Rows[0]["ClientName"]).Contains("AirAsia"))
                    {
                        //htmlFile = new StringReader(File.ReadAllText(Server.MapPath("~/Reports/eAWBRule.html")));
                        htmlFile = new StringReader(File.ReadAllText(pathhtml + "/Reports/eAWBRule.html"));
                        htmlContent = string.Format(htmlFile.ReadToEnd());
                        HTMLData = HTMLData + htmlContent;
                    }
                    dsClientName = null;
                }
                else
                {
                    HTMLData = htmlContent;

                    if (!string.IsNullOrEmpty(DimsData))
                    {
                        HTMLData = HTMLData + "<div style=\"page-break-after:always\"></div>";
                        HTMLData = HTMLData + DimsHTMLData;
                    }
                }
                return HTMLData;

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        public async Task GetUpdateNotification(string awbPrefix, string awbNumber, int SerialNumber)
        {
            try
            {
                DataSet? dsUpdate = new DataSet();
                //string[] QueryNames = { "AWBprefix", "AWBNumber", "SerialNumber" };
                //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //string[] QueryValues = { awbPrefix, awbNumber, SerialNumber.ToString() };
                //SQLServer db = new SQLServer();
                //dsUpdate = db.SelectRecords("USPUpdatePendingNotificationList", QueryNames, QueryValues, QueryTypes);

                SqlParameter[] parameters =
                [
                    new("@AWBprefix", SqlDbType.VarChar)   { Value = awbPrefix },
                    new("@AWBNumber", SqlDbType.VarChar)   { Value = awbNumber },
                    new("@SerialNumber", SqlDbType.VarChar){ Value = SerialNumber.ToString() }
                ];
                dsUpdate = await _readWriteDao.SelectRecords("USPUpdatePendingNotificationList", parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        public async Task UploadSISReceivableFileonSFTP()
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //DataSet dsSFTPDetails = sqlServer.SelectRecords("uspGetMessageConfiguration2", sqlParameter);

                SqlParameter[] sqlParameter = new SqlParameter[] {
                    new SqlParameter("@Messagetype", MessageData.MessageTypeName.SISFILES)
                };
                DataSet? dsSFTPDetails = await _readWriteDao.SelectRecords("uspGetMessageConfiguration2", sqlParameter);

                if (dsSFTPDetails != null && dsSFTPDetails.Tables.Count > 0 && dsSFTPDetails.Tables[0].Rows.Count > 0)
                {
                    //FTP _ftp = new FTP();
                    _ftp.UploadSISReceivableFileonSFTP(dsSFTPDetails.Tables[0]);
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;

            }
        }

    }
    #endregion

    /*Moved this new class file WebService*/
    #region WebServiceCalss
    /// <summary>
    /// name: Sushant Gavas,
    /// Added webservice class
    /// AK-3714
    /// </summary>
    //public class WebService
    //{
    //    public string Url { get; set; }
    //    public string MethodName { get; set; }
    //    public Dictionary<string, string> Params = new Dictionary<string, string>();
    //    public XDocument ResultXML;
    //    public string ResultString;
    //    string UserName;
    //    string Password;
    //    string SoapRequest;


    //    public WebService()
    //    {

    //    }

    //    public WebService(string url, string methodName, string userName, string password, string MessageBody)
    //    {
    //        Url = url;
    //        MethodName = methodName;
    //        UserName = userName;
    //        Password = password;
    //        SoapRequest = MessageBody;
    //    }

    //    /// <summary>
    //    /// Invokes service
    //    /// </summary>
    //    public void Invoke(string customsName)
    //    {
    //        Invoke(true, customsName);
    //    }

    //    /// <summary>
    //    /// Invokes service
    //    /// </summary>
    //    /// <param name="encode">Added parameters will encode? (default: true)</param>
    //    //public void Invoke(bool encode)
    //    //{
    //    //    string soapStr = SoapRequest;         

    //    //    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
    //    //    req.Headers.Add("SOAPAction", "\"http://tempuri.org/" + MethodName + "\"");
    //    //    req.ContentType = "text/xml;charset=\"utf-8\"";
    //    //    req.Accept = "text/xml";
    //    //    req.Method = "POST";
    //    //    req.Credentials = new NetworkCredential(UserName, Password);

    //    //    using (Stream stm = req.GetRequestStream())
    //    //    {              
    //    //        //soapStr = string.Format(soapStr, MethodName);
    //    //        using (StreamWriter stmw = new StreamWriter(stm))
    //    //        {
    //    //            stmw.Write(soapStr);
    //    //        }
    //    //    }

    //    //    using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
    //    //    {
    //    //        string result = responseReader.ReadToEnd();
    //    //        ResultXML = XDocument.Parse(result);
    //    //        ResultString = result.Replace("&lt;", "<");
    //    //    }
    //    //}

    //    public void Invoke(bool encode, string customsName)
    //    {
    //        string soapStr = SoapRequest;
    //        string strMsgName = MethodName;
    //        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
    //        if (customsName.ToUpper() == "DAKAR")
    //        {
    //            req.Headers.Add("SOAPAction", "");
    //            req.Headers.Add("Username", UserName);
    //            req.Headers.Add("Password", Password);
    //        }
    //        else
    //        {
    //            req.Headers.Add("SOAPAction", strMsgName);
    //        }
    //        req.ContentType = "text/xml;charset=\"utf-8\"";
    //        req.Method = "POST";

    //        req.Credentials = new NetworkCredential(UserName, Password);

    //        try
    //        {
    //            using (Stream stm = req.GetRequestStream())
    //            {
    //                using (StreamWriter stmw = new StreamWriter(stm))
    //                {
    //                    stmw.Write(soapStr);
    //                }
    //            }
    //            using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
    //            {
    //                string result = responseReader.ReadToEnd();
    //                ResultXML = XDocument.Parse(result);
    //                ResultString = result.Replace("&lt;", "<");
    //            }
    //        }
    //        catch (WebException webex)
    //        {
    //            clsLog.WriteLogAzure("WEBSERVICE Error" + webex.Response.ToString());
    //        }
    //    }
    //}
    #endregion
}