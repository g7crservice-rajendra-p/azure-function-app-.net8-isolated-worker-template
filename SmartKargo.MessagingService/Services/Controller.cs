
using EAGetMail;
//using EmailClient; //Not in used
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenPop.Pop3;//New latest package installed
//using OpenPOP.POP3;//Not in used
//using QID.DataAccess;//Not in used
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;
//using System;//Not in used
//using System.Configuration;//Not in used
using System.Data;
//using System.Linq;//Not in used
//using System.Threading;//Not in used

namespace QidWorkerRole
{
    class Controller
    {

        //GenericFunction genericFunction = new GenericFunction();
        //string conStr; //= ConfigurationSettings.AppSettings["srccon"].ToString();
        // private string constr = ConfigurationSettings.AppSettings["ConStr"].ToString();
        #region constructor
        private readonly ILogger<Controller> _logger;//instance logger
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly GenericFunction _genericFunction;
        private readonly Pop3Client _popClient;
        public Controller(ISqlDataHelperFactory sqlDataHelperFactory, ILogger<Controller> logger, GenericFunction genericFunction, Pop3Client popClient)
        {
            //GenericFunction genericFunction = new GenericFunction();
            //conStr = Convert.ToString(genericFunction.ReadValueFromDb("srccon"));
            //conStr = ConfigCache.Get("srccon");
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
            _popClient = popClient;
        }
        #endregion

        #region Service Call
        public void ServiceMethod()
        {
            while (true)
            {
                try
                {

                    //ReceiveMail(ConfigurationSettings.AppSettings["Server"].ToString(),ConfigurationSettings.AppSettings["Username"].ToString(), ConfigurationSettings.AppSettings["Password"].ToString(),bool.Parse(ConfigurationSettings.AppSettings["UseSSL"].ToString()));
                    //ReceiveMail(Convert.ToString(genericFunction.ReadValueFromDb("Server")), Convert.ToString(genericFunction.ReadValueFromDb("Username")), Convert.ToString(genericFunction.ReadValueFromDb("Password")), bool.Parse(Convert.ToString(genericFunction.ReadValueFromDb("UseSSL"))));
                    ReceiveMail(Convert.ToString(ConfigCache.Get("Server")), Convert.ToString(ConfigCache.Get("Username")), Convert.ToString(ConfigCache.Get("Password")), bool.Parse(Convert.ToString(ConfigCache.Get("UseSSL"))));
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure("Error :", ex);
                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                //Thread.Sleep(1000 * 60 * int.Parse(Convert.ToString(genericFunction.ReadValueFromDb("Interval"))));//ConfigurationSettings.AppSettings["Interval"].ToString()
                Thread.Sleep(1000 * 60 * int.Parse(Convert.ToString(ConfigCache.Get("Interval"))));//ConfigurationSettings.AppSettings["Interval"].ToString()

            }
        }
        #endregion

        //#region constructor
        //public Controller()
        //{ 
        //    GenericFunction genericFunction = new GenericFunction();
        //    conStr = Convert.ToString(genericFunction.ReadValueFromDb("srccon"));
        //}
        //#endregion

        #region RecieveMail
        public void ReceiveMail(string sServer, string sUserName, string sPassword, bool bSSLConnection)
        {
            //by default, the pop3 port is 110, imap4 port is 143, 
            //the pop3 ssl port is 995, imap4 ssl port is 993

            try
            {
                //
                //POPClient poppy = new POPClient();
                //poppy.Connect(sServer, 110, bSSLConnection);
                //poppy.Authenticate(sUserName, sPassword);
                _popClient.Connect(sServer, 110, bSSLConnection);
                _popClient.Authenticate(sUserName, sPassword);
                // clsLog.WriteLogAzure("Server Connected..[" + DateTime.Now + "]", null);
                _logger.LogInformation($"Server Connected..[ {DateTime.Now}");
                //int Count = poppy.GetMessageCount();
                int Count = _popClient.GetMessageCount();
                if (Count > 0)
                {
                    for (int i = Count; i >= 1; i -= 1)
                    {
                        //subject, body, fromId ,toId ,recievedOn ,sendOn ,type ,status
                        //OpenPOP.MIMEParser.Message m = poppy.GetMessage(i, false);
                        //string MailBody = m.MessageBody[0].ToString();
                        //string Subject = m.Subject.ToString();
                        //string fromEmail = m.FromEmail.ToString();
                        //string toEmail = m.TO[0];//m.TO.ToString();
                        //string recievedDate = m.Date.ToString();
                        //string status = "Active";
                        // "01 Feb 2013 15:51:12"
                        //use the parsed mail in variable 'm'
                        var m = _popClient.GetMessage(i);// Changes done based on new OpenPop package                        
                        string MailBody = m.MessagePart?.GetBodyAsText() ?? "";
                        string Subject = m.Headers?.Subject ?? "";
                        string fromEmail = m.Headers?.From?.ToString() ?? "";
                        string toEmail = m.Headers?.To?.FirstOrDefault()?.ToString() ?? "";
                        string recievedDate = m.Headers?.DateSent.ToString() ?? "";
                        string status = "Active";


                        DateTime dtRec = DateTime.ParseExact(recievedDate, "dd MMM yyyy HH:mm:ss", null);
                        DateTime dtSend = dtRec;


                        // clsLog.WriteLogAzure("Email Received : Subject :" + Subject + " [" + DateTime.Now + "]");
                        _logger.LogInformation("Email Received : Subject : {0} [{1}]" , Subject , DateTime.Now);
                        StoreIROPSEmail(Subject, MailBody, fromEmail, toEmail, dtRec, dtSend, Subject, status);
                        // clsLog.WriteLogAzure("Email " + (i + 1) + " Saved");
                        _logger.LogInformation($"Email {i+1} Saved");
                        //poppy.DeleteMessage(i);
                        _popClient.DeleteMessage(i);

                    }
                }

                //poppy.Disconnect(); 
                _popClient.Disconnect();


            }
            catch (MailServerException ep)
            {
                //Message contains the information returned by mail server
                // Console.WriteLine("Server Respond: {0}", ep.Message);
                _logger.LogError("Server Respond: {0}", ep.Message);
            }
            catch (System.Net.Sockets.SocketException ep)
            {
                // Console.WriteLine("Socket Error: {0}", ep.Message);
                _logger.LogError("Socket Error: {0}", ep.Message);
            }
            catch (Exception ep)
            {
                // Console.WriteLine("System Error: {0}", ep.Message);
                _logger.LogError("System Error: {0}", ep.Message);
            }


        }
        #endregion

        #region preocessmail
        //public void ProcessMail()
        //{
        //    try
        //    {
        //        DataSet dsMail = new DataSet();
        //        dsMail = FetchEmail();
        //        if (dsMail.Tables[0].Rows.Count == 0)
        //        {
        //            Log.WriteLog("Error Fetching mail from DB " + DateTime.Now + "]");
        //            return;
        //        }
        //        int rc = 0;
        //        List<string> MailX = new List<string>();
        //        while (rc < dsMail.Tables[0].Rows.Count)
        //        {
        //            ParseMail(dsMail.Tables[0].Rows[rc][0].ToString());
        //            rc++;
        //            MailX[rc] = dsMail.Tables[0].Rows[rc][0].ToString();
        //        }

        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        #endregion


        #region Extra
        //public bool InsertIROPS(DataSet dsIROPS,string subject,string mail)
        //{
        //    try
        //    {
        //        StoreIROPSEmail(subject, mail.Replace("\n", "     "), dsIROPS.Tables[0].Rows.Count);

        //        foreach (DataRow row in dsIROPS.Tables[0].Rows)
        //        {

        //            Database db = new Database();

        //            string[] parameters = {  "FlightID", "Source", "Dest", "ActDeptTime", "ActArrTime", "SchDeptTime", "SchArrTime", "DevCode", "ActEQT", "SchEQT", 
        //                                          "NewFlightID", "Reason", "IROPSDate"};

        //            SqlDbType[] sqldbtypes = { SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Time,SqlDbType.Time,
        //                                           SqlDbType.Time,SqlDbType.Time,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,
        //                                           SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime};


        //            object[] value = { row["FlightID"].ToString(), row["Source"].ToString(), row["Dest"].ToString(), (row["ActDeptTime"].ToString().Trim()=="" ? "00:00:000":row["ActDeptTime"].ToString().Trim()), 
        //                                   (row["ActArrTime"].ToString().Trim()=="" ? "00:00:000":row["ActArrTime"].ToString().Trim()),
        //                                   (row["SchDeptTime"].ToString().Trim()=="" ? "00:00:000":row["SchDeptTime"].ToString().Trim()) ,
        //                                  (row["SchArrTime"].ToString().Trim()=="" ? "00:00:000":row["SchArrTime"].ToString().Trim()) ,
        //                                  row["DevCode"].ToString(), row["ActEQT"].ToString(), row["SchEQT"].ToString(),
        //                                    row["NewFlightID"].ToString(), row["Reason"].ToString(), row["IROPSDate"].ToString()};




        //            db.ExecuteStoredProcedure("SPKFAAnalyzer_InsertIROPSSchedule", parameters, value, sqldbtypes);

        //        }

        //        return true;

        //    }catch(Exception ex)
        //    {
        //        Log.WriteLog("Error : In(InsertIROPS) [" + DateTime.Now + "]");
        //        return false;
        //    }

        //}
        #endregion


        #region Save mail to Mailbox
        public async Task<bool> StoreIROPSEmail(string subject, string body, string fromId, string toId, DateTime recievedOn, DateTime sendOn, string type, string status)
        {
            try
            {

                //SQLServer db = new SQLServer();;
                //string[] param = {"subject", "body", "fromId" ,"toId" ,"recievedOn" ,"sendOn" ,"type" ,"status" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar };
                var parameters = new SqlParameter[]
                {
                    new("@subject", SqlDbType.VarChar)      { Value = subject },
                    new("@body", SqlDbType.VarChar)         { Value = body },
                    new("@fromId", SqlDbType.VarChar)       { Value = fromId },
                    new("@toId", SqlDbType.VarChar)         { Value = toId },
                    new("@recievedOn", SqlDbType.DateTime)  { Value = recievedOn },
                    new("@sendOn", SqlDbType.DateTime)      { Value = sendOn },
                    new("@type", SqlDbType.VarChar)         { Value = type },
                    new("@status", SqlDbType.VarChar)       { Value = status }
                };

                // object[] values = { subject, body, fromId ,toId ,recievedOn ,sendOn ,type ,status};
                //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };

                //bool res;
                //res = db.InsertData("spSavetoInbox", param, sqldbtypes, values);//db.InsertData("[spSavetoInbox]", param, values, sqldbtypes);
                var res = await _readWriteDao.ExecuteNonQueryAsync("spSavetoInbox", parameters);//db.InsertData("[spSavetoInbox]", param, values, sqldbtypes);

                return res;
            }
            catch (Exception)
            {
                // clsLog.WriteLogAzure("Error : In(StoreEmail) [" + DateTime.Now + "]");
                _logger.LogError("Error IN(StoreEmail) [{0}]" , DateTime.Now);
                return false;
            }
        }
        #endregion

        #region Extra
        public void LogRecords(DataSet dsIROPS)
        {
            try
            {
                foreach (DataRow row in dsIROPS.Tables[0].Rows)
                {
                    string strRecord = "";
                    for (int i = 0; i < dsIROPS.Tables[0].Columns.Count; i++)
                    {
                        strRecord += "  " + row[i].ToString();
                    }

                    // clsLog.WriteLogAzure("Record : " + strRecord + "");
                    _logger.LogInformation("Record : {0}" , strRecord );
                }


            }
            catch (Exception)
            {
                // clsLog.WriteLogAzure("Error : In[LogRecords] [" + DateTime.Now + "]");
                _logger.LogError("Error : In[LogRecords] [{0}]" , DateTime.Now);

            }

        }
        #endregion
        //No reference found, so commented out
        //public DataSet FetchEmail()
        //{
        //    try
        //    {

        //        // create table structure
        //        DataSet dsFFMmail = new DataSet();
        //        SQLServer db = new SQLServer();
        //        string[] param = { };
        //        SqlDbType[] sqldbtype = { };
        //        object[] values = { };

        //        dsFFMmail = db.SelectRecords("spFetchFFMmessage", param, values, sqldbtype);//db.ExecuteStoredProcedure("spFetchFFMmessage", param, values, sqldbtype);
        //        return dsFFMmail;
        //    }
        //    catch(Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error :", ex);
        //    }
        //    return null;
        //}

        #region Extra
        //No reference found, so commented out
        //public bool ParseEmail(Client DemoClient,ref DataSet dsIROPS)
        //{
        //    try
        //    {

        //        // create table structure

        //        SQLServer db = new SQLServer();
        //        string[] param = { };
        //        SqlDbType[] sqldbtype = { };
        //        object[] values = { };

        //        dsIROPS = db.SelectRecords("SPKFAAnalyzer_GetIROPSTableStructure",param,values,sqldbtype);//db.ExecuteStoredProcedure("SPKFAAnalyzer_GetIROPSTableStructure", param, values, sqldbtype);


        //        // Format Email

        //        string strmail = DemoClient.Body;

        //        strmail = strmail.Replace("\n", "---XX---XXXXX---XX---");
        //        strmail = strmail.Replace("\t", "---XX---XXXXX---XX---");
        //        string[] seperator = { "---XX---XXXXX---XX---" };
        //        string[] strbodyfragments = strmail.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
        //        string reason = "";


        //        //  Parse

        //        for (int i = 0; i < strbodyfragments.Length; i++)
        //        {
        //            // loop upto 'Event'
        //            if (strbodyfragments[i].Trim() == "Event")
        //            {
        //                reason = strbodyfragments[i - 1].Trim();

        //                // loop upto 'Arr'
        //                for (int j = i + 1; j < strbodyfragments.Length; j++)
        //                {                          

        //                    if (strbodyfragments[j].Trim() == "Arr")
        //                    {
        //                        while (strbodyfragments[j].Trim() != "EQT" 
        //                               && strbodyfragments[j].Trim() != "NEW" 
        //                               && strbodyfragments[j].Trim() != "DLY"
        //                               && strbodyfragments[j].Trim() != "RSD"
        //                               && strbodyfragments[j].Trim() != "TIM"
        //                               && strbodyfragments[j].Trim() != "RRT"
        //                               && strbodyfragments[j].Trim() != "CNL")
        //                        {
        //                            j++;
        //                        }

        //                        // get events
        //                        for (int k = j ; k < strbodyfragments.Length; )
        //                        {
        //                            DataRow row = dsIROPS.Tables[0].NewRow();
        //                            row["DevCode"] = strbodyfragments[k].Trim();
        //                            string devcode = strbodyfragments[k].Trim();

        //                            if (strbodyfragments[k].Trim() == "EQT")
        //                                row["ActEQT"] = strbodyfragments[k + 2].Trim();
        //                            else
        //                                row["SchEQT"] = strbodyfragments[k + 2].Trim();

        //                            if (strbodyfragments[k].Trim() == "NEW")
        //                                row["NewFlightID"] = "IT" + strbodyfragments[k + 3].Trim();
        //                            else
        //                            {
        //                                row["FlightID"] = "IT" + strbodyfragments[k + 3].Trim().PadLeft(3,'0');
        //                            }


        //                            if (strbodyfragments[k + 4].Trim().Contains('-'))
        //                            {
        //                                row["Source"] = strbodyfragments[k + 4].Substring(0, strbodyfragments[k + 4].IndexOf('-'));
        //                                row["Dest"] = strbodyfragments[k + 4].Substring(strbodyfragments[k + 4].IndexOf('-') + 1);
        //                            }

        //                            //080211 0605
        //                            row["IROPSDate"] = "20" + strbodyfragments[k + 5].Substring(4, 2) + "-" + strbodyfragments[k + 5].Substring(2, 2) + "-" + strbodyfragments[k + 5].Substring(0, 2) + " 00:00:00";

        //                            if (strbodyfragments[k].Trim() == "DLY" || strbodyfragments[k].Trim() == "RSD" || strbodyfragments[k].Trim() == "TIM")
        //                            {
        //                                row["ActDeptTime"] = strbodyfragments[k + 5].Substring(7, 2) + ":" + strbodyfragments[k + 5].Substring(9, 2) + ":000";
        //                                row["ActArrTime"] = strbodyfragments[k + 6].Substring(7, 2) + ":" + strbodyfragments[k + 6].Substring(9, 2) + ":000";                                      
        //                            }
        //                            else
        //                            {
        //                                row["SchDeptTime"] = strbodyfragments[k + 5].Substring(7, 2) + ":" + strbodyfragments[k + 5].Substring(9, 2) + ":000";
        //                                row["SchArrTime"] = strbodyfragments[k + 6].Substring(7, 2) + ":" + strbodyfragments[k + 6].Substring(9, 2) + ":000";
        //                            }

        //                            row["Reason"] = reason;


        //                            dsIROPS.Tables[0].Rows.Add(row);

        //                            //if (devcode == "DLY")
        //                            //    k = k + 9;
        //                            //else
        //                            //    k = k + 8;

        //                             k = k + 4;

        //                            while (!(strbodyfragments[k].Trim() == "DLY" || 
        //                                     strbodyfragments[k].Trim() == "RSD" ||
        //                                     strbodyfragments[k].Trim() == "TIM" ||
        //                                     strbodyfragments[k].Trim() == "RRT" ||
        //                                     strbodyfragments[k].Trim() == "EQT" ||
        //                                     strbodyfragments[k].Trim() == "CNL" ||
        //                                     strbodyfragments[k].Trim() == "NEW" ||
        //                                     strbodyfragments[k].Trim() == "Movement Timings For All International Flights are in UTC." ||
        //                                     (k + 1) > strbodyfragments.Length

        //                                    )) 
        //                                    k++;



        //                            if (strbodyfragments[k].Trim() == "Movement Timings For All International Flights are in UTC." ||
        //                                (strbodyfragments[k].Trim() != "DLY" && strbodyfragments[k].Trim() != "RSD" && strbodyfragments[k].Trim() != "TIM" &&
        //                                 strbodyfragments[k].Trim() != "RRT" && strbodyfragments[k].Trim() != "EQT" && strbodyfragments[k].Trim() != "CNL"
        //                                 && strbodyfragments[k].Trim() != "NEW"))
        //                            {
        //                                i = j = k = strbodyfragments.Length;
        //                                break;
        //                            }
        //                        }
        //                    }

        //                }


        //            }
        //        }



        //        return true;

        //    }catch(Exception)
        //    {
        //        clsLog.WriteLogAzure("Error : In(ParseEmail) [" + DateTime.Now + "]");
        //        return false;
        //    }

        //}




        //No reference found, so commented out
        //public bool CheckEmailExists(string subject)
        //{
        //    try
        //    {
        //        SQLServer db = new SQLServer();
        //        string[] param = { "subject" };
        //        SqlDbType[] sqldbtypes = { SqlDbType.VarChar };
        //        object[] values = { subject };

        //        DataSet dsIROPS = db.SelectRecords("SPKFAAnalyzer_CheckIROPSEmailExists", param, values, sqldbtypes);//db.ExecuteStoredProcedure("SPKFAAnalyzer_CheckIROPSEmailExists", param, values, sqldbtypes);

        //        if (dsIROPS.Tables[0].Rows.Count == 0)
        //            return false;
        //        else
        //            return true;


        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure("Error :", ex);
        //    }

        //    return false;
        //}
        #endregion

    }
}
