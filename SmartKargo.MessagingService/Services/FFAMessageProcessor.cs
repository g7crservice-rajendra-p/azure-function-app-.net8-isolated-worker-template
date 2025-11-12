#region FFA Message Processor Class Description
/* FFA Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-03-04
      * Approved By         :
      * Approved Date      :
      * Modified By          :  
      * Modified On          :   
      * Description           :   
     */
#endregion

using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using QID.DataAccess;
using System.Text;
using System.Collections.Generic;
namespace QidWorkerRole
{
    public class FFAMessageProcessor
    {
        SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();
        const string PAGE_NAME = "FBLMessageProcessor";
        private readonly ILogger<FFAMessageProcessor> _logger;

        public FFAMessageProcessor(ILogger<FFAMessageProcessor> logger)
        {
            _logger = logger;
        }

        public bool DecodeReceivedFFAMessage(string ffamsg, ref MessageData.ffainfo ffadata, ref MessageData.ffainfo[] flightinfo)
        {
            bool flag = false;
            string lastrec = "NA";
            const String FUN_NAME = "decodereceivedffa";

            MessageData.ffainfo ffaflightinfo = new MessageData.ffainfo("");
            try
            {
                if (ffamsg.StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                {
                    // ffrmsg = ffrmsg.Replace("\r\n","$");
                    //string[] str = Regex.Split(ffamsg, "\r\n");//ffrmsg.Split('$');
                    string[] str = ffamsg.Split('$');
                    if (str.Length > 3)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            flag = true;

                            #region Line 1
                            if (str[i].StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    ffadata.ffaversionnum = msg[1];
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Line 2
                            if (i == 1)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    //0th element
                                    string[] decmes = msg[0].Split('-');
                                    ffadata.airlineprefix = decmes[0];
                                    ffadata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                                    ffadata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                                    ffadata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                                    //1
                                    if (msg[1].Length > 0)
                                    {
                                        int k = 0;
                                        char lastchr = 'A';
                                        char[] arr = msg[1].ToCharArray();
                                        string[] strarr = new string[arr.Length];
                                        for (int j = 0; j < arr.Length; j++)
                                        {
                                            if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
                                            {//number                            
                                                if (lastchr == 'N')
                                                    k--;
                                                strarr[k] = strarr[k] + arr[j].ToString();
                                                lastchr = 'N';
                                            }
                                            if (char.IsLetter(arr[j]))
                                            {//letter
                                                if (lastchr == 'L')
                                                    k--;
                                                strarr[k] = strarr[k] + arr[j].ToString();
                                                lastchr = 'L';
                                            }
                                            k++;
                                        }
                                        ffadata.consigntype = strarr[0];
                                        ffadata.pcscnt = strarr[1];//int.Parse(strarr[1]);
                                        ffadata.weightcode = strarr[2];
                                        ffadata.weight = strarr[3];//float.Parse(strarr[3]);
                                        for (k = 4; k < strarr.Length; k += 2)
                                        {
                                            if (strarr[k] != null)
                                            {
                                                if (strarr[k] == "T")
                                                {
                                                    ffadata.shpdesccode = strarr[k];
                                                    ffadata.numshp = strarr[k + 1];
                                                    k = strarr.Length + 1;
                                                }
                                            }
                                        }
                                    }
                                    if (msg.Length > 2)
                                    {
                                        //2
                                        ffadata.manifestdesc = msg[2];

                                    }
                                    if (msg.Length > 2)
                                    {//3
                                        ffadata.splhandling = "";
                                        for (int j = 3; j < msg.Length; j++)
                                            ffadata.splhandling = ffadata.splhandling + msg[j] + ",";
                                    }

                                }
                                catch (Exception)
                                {
                                    //SCMExceptionHandling.logexception(ref e);
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 3 flight details
                            if (i > 1 && (!str[i].StartsWith("/")) && (!str[i].StartsWith("REF")) && (!str[i].StartsWith("OSI")) && (!str[i].StartsWith("SSR")) && (!str[i].StartsWith("SRI")))
                            //if()

                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    //if (msg[0].StartsWith("REF") || msg[0].StartsWith("OSI") || msg[0].StartsWith("SSR") || str[i].StartsWith("SRI"))
                                    //{
                                    //}
                                    //else
                                    //{
                                    if (msg.Length > 0)
                                    {
                                        ffaflightinfo.carriercode = msg[0].Substring(0, 2);
                                        ffaflightinfo.fltnum = msg[0].Substring(2);
                                        ffaflightinfo.date = msg[1].Substring(0, 2);
                                        ffaflightinfo.month = msg[1].Substring(2);
                                        ffaflightinfo.fltdept = msg[2].Substring(0, 3);
                                        ffaflightinfo.fltarrival = msg[2].Substring(3);
                                        ffaflightinfo.spaceallotmentcode = msg[3].Length > 0 ? msg[3] : null;
                                    }

                                    Array.Resize(ref flightinfo, flightinfo.Length + 1);
                                    flightinfo[flightinfo.Length - 1] = ffaflightinfo;



                                    //}
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 5 Special Service request
                            if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    if (msg[1].Length > 0)
                                    {
                                        ffadata.specialservicereq1 = msg[1];
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                }
                            }
                            #endregion

                            #region Line 6 Other service info
                            if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    if (msg[1].Length > 0)
                                    {
                                        ffadata.otherserviceinfo1 = msg[1];
                                    }
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Line 7 booking reference
                            if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        ffadata.bookingrefairport = msg[1].Substring(0, 3);
                                        ffadata.officefundesignation = msg[1].Substring(3, 2);
                                        ffadata.companydesignator = msg[1].Substring(5, 2);
                                        ffadata.participentidetifier = msg[2].Length > 0 ? msg[2] : null;
                                        ffadata.participentcode = msg[3].Length > 0 ? msg[3] : null;
                                        ffadata.participentairportcity = msg[4].Length > 0 ? msg[4] : null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Line 8 shipment refence info
                            if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        ffadata.shiprefnum = msg[1].Length > 0 ? msg[1] : null;
                                        ffadata.supplemetryshipperinfo1 = msg[2].Length > 0 ? msg[2] : null;
                                        ffadata.supplemetryshipperinfo2 = msg[3].Length > 0 ? msg[3] : null;
                                    }
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Other Info
                            if (str[i].StartsWith("/"))
                            {
                                string[] msg = str[i].Split('/');
                                try
                                {
                                    if (lastrec == "SSR")
                                    {
                                        ffadata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                    }
                                    if (lastrec == "OSI")
                                    {
                                        ffadata.otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                }
                            }
                            #endregion
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _logger.LogError(ex, PAGE_NAME, FUN_NAME);
            }
            return flag;
        }

        public bool decodereceivedffa_V1(string ffamsg, ref MessageData.ffainfo ffadata)
        {
            bool flag = false;
            string lastrec = "NA";
            const String FUN_NAME = "decodereceivedffa";
            try
            {
                if (ffamsg.StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                {
                    // ffrmsg = ffrmsg.Replace("\r\n","$");
                    //string[] str = Regex.Split(ffamsg, "\r\n");//ffrmsg.Split('$');
                    string[] str = ffamsg.Split('$');
                    if (str.Length > 3)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            flag = true;

                            #region Line 1
                            if (str[i].StartsWith("FFA", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    ffadata.ffaversionnum = msg[1];
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                }
                            }
                            #endregion

                            #region Line 2
                            if (i == 1)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    //0th element
                                    string[] decmes = msg[0].Split('-');
                                    ffadata.airlineprefix = decmes[0];
                                    ffadata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                                    ffadata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                                    ffadata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                                    //1
                                    if (msg[1].Length > 0)
                                    {
                                        int k = 0;
                                        char lastchr = 'A';
                                        char[] arr = msg[1].ToCharArray();
                                        string[] strarr = new string[arr.Length];
                                        for (int j = 0; j < arr.Length; j++)
                                        {
                                            if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
                                            {//number                            
                                                if (lastchr == 'N')
                                                    k--;
                                                strarr[k] = strarr[k] + arr[j].ToString();
                                                lastchr = 'N';
                                            }
                                            if (char.IsLetter(arr[j]))
                                            {//letter
                                                if (lastchr == 'L')
                                                    k--;
                                                strarr[k] = strarr[k] + arr[j].ToString();
                                                lastchr = 'L';
                                            }
                                            k++;
                                        }
                                        ffadata.consigntype = strarr[0];
                                        ffadata.pcscnt = strarr[1];//int.Parse(strarr[1]);
                                        ffadata.weightcode = strarr[2];
                                        ffadata.weight = strarr[3];//float.Parse(strarr[3]);
                                        for (k = 4; k < strarr.Length; k += 2)
                                        {
                                            if (strarr[k] != null)
                                            {
                                                if (strarr[k] == "T")
                                                {
                                                    ffadata.shpdesccode = strarr[k];
                                                    ffadata.numshp = strarr[k + 1];
                                                    k = strarr.Length + 1;
                                                }
                                            }
                                        }
                                    }
                                    if (msg.Length > 2)
                                    {
                                        //2
                                        ffadata.manifestdesc = msg[2];

                                    }
                                    if (msg.Length > 2)
                                    {//3
                                        ffadata.splhandling = "";
                                        for (int j = 3; j < msg.Length; j++)
                                            ffadata.splhandling = ffadata.splhandling + msg[j] + ",";
                                    }

                                }
                                catch (Exception)
                                {
                                    //SCMExceptionHandling.logexception(ref exep);
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 3 flight details
                            if (i == 2)
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 0)
                                    {
                                        ffadata.carriercode = msg[0].Substring(0, 2);
                                        ffadata.fltnum = msg[0].Substring(2);
                                        ffadata.date = msg[1].Substring(0, 2);
                                        ffadata.month = msg[1].Substring(2);
                                        ffadata.fltdept = msg[2].Substring(0, 3);
                                        ffadata.fltarrival = msg[2].Substring(3);
                                        ffadata.spaceallotmentcode = msg[3].Length > 0 ? msg[3] : null;
                                    }

                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 5 Special Service request
                            if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    if (msg[1].Length > 0)
                                    {
                                        ffadata.specialservicereq1 = msg[1];
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Line 6 Other service info
                            if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    if (msg[1].Length > 0)
                                    {
                                        ffadata.otherserviceinfo1 = msg[1];
                                    }
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Line 7 booking reference
                            if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        ffadata.bookingrefairport = msg[1].Substring(0, 3);
                                        ffadata.officefundesignation = msg[1].Substring(3, 2);
                                        ffadata.companydesignator = msg[1].Substring(5, 2);
                                        ffadata.participentidetifier = msg[2].Length > 0 ? msg[2] : null;
                                        ffadata.participentcode = msg[3].Length > 0 ? msg[3] : null;
                                        ffadata.participentairportcity = msg[4].Length > 0 ? msg[4] : null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Line 8 shipment refence info
                            if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        ffadata.shiprefnum = msg[1].Length > 0 ? msg[1] : null;
                                        ffadata.supplemetryshipperinfo1 = msg[2].Length > 0 ? msg[2] : null;
                                        ffadata.supplemetryshipperinfo2 = msg[3].Length > 0 ? msg[3] : null;
                                    }
                                }
                                catch (Exception ex) {
                                    // clsLog.WriteLogAzure(ex.Message);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                 }
                            }
                            #endregion

                            #region Other Info
                            if (str[i].StartsWith("/"))
                            {
                                string[] msg = str[i].Split('/');
                                try
                                {
                                    if (lastrec == "SSR")
                                    {
                                        ffadata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                    }
                                    if (lastrec == "OSI")
                                    {
                                        ffadata.otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                    }

                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex.Message); 
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                                }
                            }
                            #endregion
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _logger.LogError(ex, PAGE_NAME, FUN_NAME);
            }
            return flag;
        }

        public bool SaveandUpdateFFAMessage(MessageData.ffainfo objFFAData, int refno, MessageData.ffainfo[] flightinfo)
        {
            bool flag = false;
            try
            {
                SQLServer dtb = new SQLServer();
                string awbno = string.Empty, awbprefix = string.Empty, flightno = string.Empty, flightdate = string.Empty, flightmonth = string.Empty, awborigin = string.Empty, awbdestination = string.Empty, flightorigin = string.Empty, flightdest = string.Empty, spaceallotmentcode = string.Empty, carriercode = string.Empty;
                int pcs = 0;
                decimal weight = 0;

                awbno = objFFAData.awbnum;
                awbprefix = objFFAData.airlineprefix;
                flightno = objFFAData.fltnum;
                flightdate = objFFAData.date;
                flightmonth = objFFAData.month;
                awborigin = objFFAData.origin;
                awbdestination = objFFAData.dest;
                flightorigin = objFFAData.fltdept;
                flightdest = objFFAData.fltarrival;
                pcs = Convert.ToInt16(objFFAData.pcscnt == "" ? "0" : objFFAData.pcscnt);
                weight = Convert.ToDecimal(objFFAData.weight == "" ? "0" : objFFAData.weight);
                spaceallotmentcode = objFFAData.spaceallotmentcode;
                DataTable dt = new DataTable("FFAMessage");
                dt.Columns.Add("awbprefix");
                dt.Columns.Add("awbno");
                dt.Columns.Add("awborigin");
                dt.Columns.Add("awbdestination");
                dt.Columns.Add("flightno");
                dt.Columns.Add("flightorigin");
                dt.Columns.Add("flightdest");
                dt.Columns.Add("flightdate");
                dt.Columns.Add("flightmonth");
                dt.Columns.Add("pcs");
                dt.Columns.Add("weight");
                dt.Columns.Add("spaceallotmentcode");
                dt.Columns.Add("SpclSrvicReq1");
                dt.Columns.Add("SpclSrvicReq2");
                dt.Columns.Add("OtrSrvcInfo1");
                dt.Columns.Add("OtrSrvcInfo2");
                dt.Columns.Add("bookigFilRef");
                dt.Columns.Add("shiprefnum");
                dt.Columns.Add("supplemetryshipperinfo1");
                dt.Columns.Add("supplemetryshipperinfo2");
                DataRow dr;
                for (int y = 0; y < flightinfo.Length; y++)
                {
                    dr = dt.NewRow();
                    dr["awbprefix"] = objFFAData.airlineprefix;
                    dr["awbno"] = objFFAData.awbnum;
                    dr["awborigin"] = objFFAData.origin;
                    dr["awbdestination"] = objFFAData.dest;
                    dr["flightno"] = flightinfo[y].carriercode + flightinfo[y].fltnum;
                    dr["flightorigin"] = flightinfo[y].fltdept;
                    dr["flightdest"] = flightinfo[y].fltarrival;
                    dr["flightdate"] = flightinfo[y].date;
                    dr["flightmonth"] = flightinfo[y].month;
                    dr["pcs"] = flightinfo[y].pcscnt;
                    dr["weight"] = flightinfo[y].weight == "" ? Convert.ToDecimal(0.00) : Convert.ToDecimal(flightinfo[y].weight);
                    dr["spaceallotmentcode"] = flightinfo[y].spaceallotmentcode;
                    dr["SpclSrvicReq1"] = objFFAData.specialservicereq1;
                    dr["SpclSrvicReq2"] = objFFAData.specialservicereq2;
                    dr["OtrSrvcInfo1"] = objFFAData.otherserviceinfo1;
                    dr["OtrSrvcInfo2"] = objFFAData.otherserviceinfo2;
                    dr["bookigFilRef"] = string.Empty;
                    dr["shiprefnum"] = objFFAData.shiprefnum;
                    dr["supplemetryshipperinfo1"] = objFFAData.supplemetryshipperinfo1;
                    dr["supplemetryshipperinfo2"] = objFFAData.supplemetryshipperinfo2;
                    dt.Rows.Add(dr);
                }
                string[] PName = new string[] { "FFADataTypedata" };
                object[] PValues = new object[] { dt };
                SqlDbType[] PType = new SqlDbType[] { SqlDbType.Structured };
                flag = dtb.InsertData("spInsertAWBStatusFFA_V2", PName, PType, PValues);
                if (!flag)
                    // clsLog.WriteLogAzure("Error Status Update:" + dt.Rows[0]["awbno"].ToString());
                    _logger.LogWarning("Error Status Update: {0}" , dt.Rows[0]["awbno"]);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Make and Send FFA Message
        /// </summary>
        public void MakeanSendFFAMessage(string awbPrefix = "", string awbNumber = "")
        {
            try
            {
                SQLServer sqlServer = new SQLServer();
                DataSet dsFFAData = null;
                bool flag = false;
                string strSitaAddress = string.Empty, strMessageVersion = string.Empty, strOriginAddress = string.Empty, MessageID = string.Empty;
                do
                {
                    flag = false;
                    SqlParameter[] sqlParameter = new SqlParameter[]{
                        new SqlParameter("@AWBPrefix",awbPrefix)
                        , new SqlParameter("@AWBNumber",awbNumber)
                    };
                    dsFFAData = sqlServer.SelectRecords("Messaging.uspGetFFAData", sqlParameter);
                    if (dsFFAData != null)
                    {
                        if (dsFFAData.Tables.Count > 0 && dsFFAData.Tables.Count == 3)
                        {
                            if (dsFFAData.Tables[0].Rows.Count > 0 && dsFFAData.Tables[1].Rows.Count > 0)
                            {
                                flag = true;
                                if (dsFFAData.Tables.Count > 1)
                                {
                                    if (!EncodeFFAForSend(dsFFAData))
                                        // clsLog.WriteLogAzure("FFA not Update:" + dsFFAData.Tables[0].Rows[0][0].ToString());
                                        _logger.LogWarning("FFA not Update: {0}" , dsFFAData.Tables[0].Rows[0][0]);
                                }
                                string[] PName = new string[] { "AWBNo", "AWBPrefix" };
                                object[] PValues = new object[] { dsFFAData.Tables[0].Rows[0]["AWBNumber"].ToString(), dsFFAData.Tables[0].Rows[0]["AWBPrefix"].ToString() };
                                SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                                if (!sqlServer.ExecuteProcedure("spUpdateFFAStatus", PName, PType, PValues))
                                    // clsLog.WriteLogAzure("Error Status Update:" + dsFFAData.Tables[0].Rows[0][0].ToString());
                                    _logger.LogWarning("Error Status Update: {0}" , dsFFAData.Tables[0].Rows[0][0]);
                            }
                        }
                    }
                } while (flag);
                sqlServer = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }
        }

        public bool EncodeFFAForSend(DataSet dsData, string FromEmailID, string ToEmailID, string SitaAddress, string MessageVersion, string Sitaoriginaddress, string MessageID)
        {
            bool flag = false;
            try
            {
                if (dsData != null && dsData.Tables.Count > 0 && dsData.Tables[0].Rows.Count > 0)
                {

                    int i = 0;
                    MessageData.ffainfo objFFA = new MessageData.ffainfo();
                    MessageData.FltRoute[] FltRoute = new MessageData.FltRoute[0];
                    objFFA.ffaversionnum = MessageVersion == "" ? "4" : MessageVersion;
                    objFFA.airlineprefix = dsData.Tables[0].Rows[i]["AWBPrefix"].ToString();
                    objFFA.awbnum = dsData.Tables[0].Rows[i]["AWBNumber"].ToString();
                    objFFA.origin = dsData.Tables[0].Rows[i]["OriginCode"].ToString().ToUpper();
                    objFFA.dest = dsData.Tables[0].Rows[i]["DestinationCode"].ToString().ToUpper();
                    objFFA.consigntype = "T";
                    objFFA.pcscnt = dsData.Tables[0].Rows[i]["PiecesCount"].ToString();
                    objFFA.weightcode = dsData.Tables[0].Rows[i]["UOM"].ToString();
                    objFFA.weight = dsData.Tables[0].Rows[i]["GrossWeight"].ToString();
                    objFFA.shpdesccode = "";
                    objFFA.numshp = "";
                    objFFA.manifestdesc = dsData.Tables[0].Rows[i]["CommodityDesc"].ToString();
                    objFFA.splhandling = "";

                    for (int k = 0; k < dsData.Tables[0].Rows.Count; k++)
                    {
                        var FltInfo = from P in dsData.Tables[0].AsEnumerable()
                                      where P.Field<int>("SerialNumber") == Convert.ToInt32(dsData.Tables[0].Rows[i]["SerialNumber"])
                                      select P;

                        DataTable flt = FltInfo.CopyToDataTable();

                        Array.Resize(ref FltRoute, flt.Rows.Count);
                        for (int fltcnt = 0; fltcnt < flt.Rows.Count; fltcnt++)
                        {
                            FltRoute[fltcnt].carriercode = "";
                            FltRoute[fltcnt].fltnum = flt.Rows[fltcnt]["FltNumber"].ToString();
                            string[] str = flt.Rows[fltcnt]["FltDate"].ToString().Split('/');


                            FltRoute[fltcnt].date = str[0];
                            FltRoute[fltcnt].month = str[1];
                            FltRoute[fltcnt].fltdept = flt.Rows[fltcnt]["FltOrigin"].ToString().ToUpper();
                            FltRoute[fltcnt].fltarrival = flt.Rows[fltcnt]["FltDestination"].ToString().ToUpper();
                            string stat = flt.Rows[fltcnt]["Status"].ToString();
                            #region Status Code
                            if (stat.Length < 1)
                            {
                                stat = "LL";
                            }
                            if (stat.Length > 0 && stat.Equals("Q", StringComparison.OrdinalIgnoreCase))
                            {
                                stat = "LL";
                            }
                            if (stat.Length > 0 || stat.Equals("C", StringComparison.OrdinalIgnoreCase))
                            {
                                stat = "KK";
                            }
                            #endregion
                            FltRoute[fltcnt].spaceallotmentcode = stat;

                        }

                        objFFA.specialservicereq1 = "";
                        objFFA.specialservicereq2 = "";
                        objFFA.otherserviceinfo1 = "";
                        objFFA.otherserviceinfo2 = "";
                        objFFA.originsitaaddress = dsData.Tables[0].Rows[i]["OriginSITAAddress"].ToString();
                        objFFA.bookingrefairport = dsData.Tables[0].Rows[i]["OriginCode"].ToString();
                        objFFA.officefundesignation = dsData.Tables[0].Rows[i]["SenderOfficeDesignator"].ToString();
                        objFFA.companydesignator = dsData.Tables[0].Rows[i]["CompanyDesignatior"].ToString();
                        objFFA.bookingfileref = "";
                        objFFA.participentidetifier =
                        objFFA.participentcode =
                        objFFA.participentairportcity = string.Empty;
                        objFFA.shiprefnum = "";
                        objFFA.supplemetryshipperinfo1 = "";
                        objFFA.supplemetryshipperinfo2 = "";

                        string ffaMessage = EncodeFFAMessageForsend(ref objFFA, ref FltRoute);
                        GenericFunction gf = new GenericFunction();
                        if (ToEmailID != "")
                        {
                            if (gf.SaveMessageOutBox("FFA", ffaMessage, FromEmailID, ToEmailID))
                                flag = true;
                            else
                                // clsLog.WriteLogAzure("Error: Not inserted in outbox");
                                _logger.LogWarning("Error: Not inserted in outbox");
                                

                        }
                        if (SitaAddress != "")
                        {
                            string messageheader = gf.MakeMailMessageFormat(SitaAddress, Sitaoriginaddress, MessageID);
                            if (gf.SaveMessageOutBox("SITA:FFA", messageheader + "\r\n" + ffaMessage, "", "SITAFTP"))
                                flag = true;
                            else
                                // clsLog.WriteLogAzure("Error: Not inserted in outbox");
                                _logger.LogWarning("Error: Not inserted in outbox");

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("Error in encode FFA:" + ex.Message);
                _logger.LogError("Error in encode FFA: {0}" , ex.Message);
            }
            return flag;
        }

        public bool EncodeFFAForSend(DataSet dsData)
        {
            bool flag = false;
            DataTable confInfo = null;
            string sitaOriginAddress = string.Empty;
            try
            {
                DataTable dtDistinctAWB = dsData.Tables[0].DefaultView.ToTable(true, "SerialNumber");
                for (int i = 0; i < dtDistinctAWB.Rows.Count; i++)
                {
                    MessageData.ffainfo objFFA = new MessageData.ffainfo();
                    MessageData.FltRoute[] FltRoute = new MessageData.FltRoute[0];

                    objFFA.airlineprefix = dsData.Tables[0].Rows[i]["AWBPrefix"].ToString();
                    objFFA.awbnum = dsData.Tables[0].Rows[i]["AWBNumber"].ToString();
                    objFFA.origin = dsData.Tables[0].Rows[i]["OriginCode"].ToString().ToUpper();
                    objFFA.dest = dsData.Tables[0].Rows[i]["DestinationCode"].ToString().ToUpper();
                    objFFA.consigntype = "T";
                    objFFA.pcscnt = dsData.Tables[0].Rows[i]["PiecesCount"].ToString();
                    objFFA.weightcode = dsData.Tables[0].Rows[i]["UOM"].ToString();
                    objFFA.weight = dsData.Tables[0].Rows[i]["GrossWeight"].ToString();
                    objFFA.shpdesccode = "";
                    objFFA.numshp = "";
                    objFFA.manifestdesc = dsData.Tables[0].Rows[i]["CommodityDesc"].ToString();
                    objFFA.splhandling = "";

                    var FltInfo = from P in dsData.Tables[0].AsEnumerable()
                                  where P.Field<int>("SerialNumber") == Convert.ToInt32(dtDistinctAWB.Rows[i]["SerialNumber"])
                                  select P;

                    DataTable dtFlt = FltInfo.CopyToDataTable();

                    Array.Resize(ref FltRoute, dtFlt.Rows.Count);
                    for (int fltcnt = 0; fltcnt < dtFlt.Rows.Count; fltcnt++)
                    {
                        FltRoute[fltcnt].carriercode = "";
                        FltRoute[fltcnt].fltnum = dtFlt.Rows[fltcnt]["FltNumber"].ToString();
                        FltRoute[fltcnt].date = Convert.ToDateTime(dtFlt.Rows[fltcnt]["FltDate"]).Day.ToString().PadLeft(2, '0');
                        FltRoute[fltcnt].month = Convert.ToDateTime(dtFlt.Rows[fltcnt]["FltDate"]).Month.ToString();
                        FltRoute[fltcnt].fltdept = dtFlt.Rows[fltcnt]["FltOrigin"].ToString().ToUpper();
                        FltRoute[fltcnt].fltarrival = dtFlt.Rows[fltcnt]["FltDestination"].ToString().ToUpper();
                        string stat = dtFlt.Rows[fltcnt]["Status"].ToString().Trim();

                        if (stat.Length < 1)
                            stat = "LL";
                        else if (stat.Equals("C", StringComparison.OrdinalIgnoreCase))
                            stat = "KK";
                        else if (stat.Equals("Q", StringComparison.OrdinalIgnoreCase))
                            stat = "LL";
                        else if (stat.Equals("X", StringComparison.OrdinalIgnoreCase))
                            //stat = "XX";
                            stat = "CN";
                        else if (stat.Equals("H", StringComparison.OrdinalIgnoreCase))
                            stat = "HK";
                        FltRoute[fltcnt].spaceallotmentcode = stat;
                    }

                    objFFA.specialservicereq1 = "";
                    objFFA.specialservicereq2 = "";
                    objFFA.otherserviceinfo1 = "";
                    objFFA.otherserviceinfo2 = "";
                    objFFA.originsitaaddress = dsData.Tables[0].Rows[i]["OriginSITAAddress"].ToString();
                    objFFA.bookingrefairport = dsData.Tables[0].Rows[i]["OriginCode"].ToString();
                    objFFA.officefundesignation = dsData.Tables[0].Rows[i]["SenderOfficeDesignator"].ToString();
                    objFFA.companydesignator = dsData.Tables[0].Rows[i]["CompanyDesignatior"].ToString();
                    objFFA.bookingfileref = "";
                    objFFA.participentidetifier =
                    objFFA.participentcode =
                    objFFA.participentairportcity = string.Empty;
                    objFFA.shiprefnum = "";
                    objFFA.supplemetryshipperinfo1 = "";
                    objFFA.supplemetryshipperinfo2 = "";

                    string ffaMessage = EncodeFFAMessageForsend(ref objFFA, ref FltRoute);
                    string MessageVersion = "";
                    var configs = from P in dsData.Tables[1].AsEnumerable()
                                  where P.Field<int>("AWBID") == Convert.ToInt64(dtDistinctAWB.Rows[i]["SerialNumber"])
                                  select P;

                    int MessageID = Convert.ToInt32(dsData.Tables[2].Rows[0]["MessageId"]);

                    if (configs.Count() == 0)
                        ffaMessage = "FFA/4" + ffaMessage.Substring(3, (ffaMessage.Length - 3));

                    for (int j = 0; j < configs.Count(); j++)
                    {
                        MessageID = MessageID + 1;
                        confInfo = configs.CopyToDataTable();
                        GenericFunction gf = new GenericFunction();
                        MessageVersion = confInfo.Rows[j]["MessageVersion"].ToString() == "" ? "4" : confInfo.Rows[j]["MessageVersion"].ToString();
                        ffaMessage = "FFA/" + MessageVersion + ffaMessage.Substring(3, (ffaMessage.Length - 3));
                        if (confInfo.Rows[j]["PartnerEmailiD"].ToString() != "")
                        {
                            gf.SaveMessageOutBox("FFA", ffaMessage, "", confInfo.Rows[j]["PartnerEmailiD"].ToString(), AWBNo: objFFA.airlineprefix + "-" + objFFA.awbnum);
                        }
                        if (confInfo.Rows[j]["PartnerSitaID"].ToString() != "")
                        {
                            sitaOriginAddress = confInfo.Rows[j]["OriginSenderAddress"].ToString();
                            string messageheader = gf.MakeMailMessageFormat(confInfo.Rows[j]["PartnerSitaID"].ToString(), sitaOriginAddress, MessageID.ToString());
                            gf.SaveMessageOutBox("SITA:FFA", messageheader + "\r\n" + ffaMessage, "", "SITAFTP", AWBNo: objFFA.airlineprefix + "-" + objFFA.awbnum);
                        }
                        if (confInfo.Rows[j]["SFTPHeaderSITAAddress"].ToString() != "")
                        {
                            sitaOriginAddress = confInfo.Rows[j]["OriginSenderAddress"].ToString();
                            string messageheader = gf.MakeMailMessageFormat(confInfo.Rows[j]["SFTPHeaderSITAAddress"].ToString(), sitaOriginAddress, MessageID.ToString());
                            gf.SaveMessageOutBox("SFTP:FFA", messageheader + "\r\n" + ffaMessage, "", "SITAFTP", AWBNo: objFFA.airlineprefix + "-" + objFFA.awbnum);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }
            return flag;
        }

        private string EncodeFFAMessageForsend(ref MessageData.ffainfo ffadata, ref MessageData.FltRoute[] FltRoute)
        {
            string ffa = null;
            const String FUN_NAME = "EncodeFFAMessageForsend";
            try
            {
                #region Line 1
                string line1 = "FFA" + "/" + ffadata.ffaversionnum;
                #endregion

                #region Line 2
                string splhandling = "";
                if (ffadata.splhandling.Length > 0 && ffadata.splhandling != null)
                {
                    splhandling = ffadata.splhandling.Replace(',', '/');
                    splhandling = "/" + splhandling;
                }
                string line2 = ffadata.airlineprefix + "-" + ffadata.awbnum + ffadata.origin + ffadata.dest + "/" + ffadata.consigntype + ffadata.pcscnt + ffadata.weightcode + ffadata.weight + ffadata.shpdesccode + ffadata.numshp + "/" + ffadata.manifestdesc + splhandling;
                #endregion line 2

                #region Line 3
                string line3 = "";
                for (int i = 0; i < FltRoute.Length; i++)
                {
                    line3 = line3 + FltRoute[i].carriercode + FltRoute[i].fltnum + "/" + FltRoute[i].date + (new DateTime(2010, int.Parse(FltRoute[i].month), 1).ToString("MMM", CultureInfo.InvariantCulture)).ToUpper() + "/" + FltRoute[i].fltdept + FltRoute[i].fltarrival + "/" + FltRoute[i].spaceallotmentcode + "$";
                }
                line3 = line3.Trim('$');
                line3 = line3.Replace("$", "\r\n");
                #endregion

                #region Line 4
                string line4 = "";
                if (ffadata.specialservicereq1.Length > 0 || ffadata.specialservicereq2.Length > 0)
                {
                    line4 = "SSR/" + ffadata.specialservicereq1 + "\r\n" + "/" + ffadata.specialservicereq2;
                }
                #endregion

                #region Line 5
                string line5 = "";
                if (ffadata.otherserviceinfo1.Length > 0 || ffadata.otherserviceinfo2.Length > 0)
                {
                    line5 = "SSR/" + ffadata.otherserviceinfo1 + "\r\n" + "/" + ffadata.otherserviceinfo2;
                }
                #endregion

                #region Line 6
                string line6 = "";
                line6 = "REF/" + ffadata.originsitaaddress;// + ffadata.officefundesignation + ffadata.companydesignator;
                if (ffadata.bookingfileref.Length > 0)
                {
                    line6 = line6 + "/" + ffadata.bookingfileref;
                }
                if (ffadata.participentidetifier.Length > 0)
                {
                    line6 = line6 + "/" + ffadata.participentidetifier;
                }
                if (ffadata.participentcode.Length > 0)
                {
                    line6 = line6 + "/" + ffadata.participentcode;
                }
                if (ffadata.participentairportcity.Length > 0)
                {
                    line6 = line6 + "/" + ffadata.participentairportcity;
                }
                #endregion

                #region Line 7
                string line7 = "";
                if (ffadata.shiprefnum.Length > 0 || ffadata.supplemetryshipperinfo1.Length > 0 || ffadata.supplemetryshipperinfo2.Length > 0)
                {
                    line7 = "SRI/" + ffadata.shiprefnum + "/" + ffadata.supplemetryshipperinfo1 + "/" + ffadata.supplemetryshipperinfo2;
                }
                #endregion

                #region BuildFFA
                ffa = line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
                if (line4.Length > 0)
                {
                    ffa = ffa + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    ffa = ffa + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    ffa = ffa + "\r\n" + line6.Trim('/');
                }
                if (line7.Length > 0)
                {
                    ffa = ffa + "\r\n" + line7.Trim('/');
                }

                #endregion

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                _logger.LogError(ex, PAGE_NAME, FUN_NAME);
                ffa = "ERR";
            }
            return ffa;
        }

        //public void MakeanSendFFAMessage()
        //{
        //    try
        //    {
        //        SQLServer db = new SQLServer();
        //        DataSet ds = null;
        //        bool flag = false;
        //        string Emailid = string.Empty, strSitaAddress = string.Empty, strMessageVersion = string.Empty, strOriginAddress = string.Empty, MessageID = string.Empty;
        //        do
        //        {
        //            flag = false;
        //            ds = db.SelectRecords("spGetFFAData");
        //            if (ds != null)
        //            {
        //                // if (ds.Tables.Count > 0 && ds.Tables.Count == 3)
        //                {
        //                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[1].Rows.Count > 0)
        //                    {
        //                        flag = true;
        //                        if (ds.Tables.Count > 1)
        //                        {
        //                            try
        //                            {
        //                                if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
        //                                    Emailid = ds.Tables[2].Rows[0][0].ToString() + ",";
        //                                if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
        //                                {
        //                                    Emailid = Emailid + (ds.Tables[3].Rows[0]["PartnerEmailID"].ToString().Length > 0 ? ds.Tables[3].Rows[0]["PartnerEmailID"].ToString() : "");
        //                                    strSitaAddress = ds.Tables[3].Rows[0]["PatnerSitaid"].ToString();
        //                                    strMessageVersion = ds.Tables[3].Rows[0]["MessageVersion"].ToString();
        //                                    strOriginAddress = ds.Tables[3].Rows[0]["OriginSenderAddress"].ToString();
        //                                    MessageID = ds.Tables[3].Rows[0]["MessageID"].ToString();

        //                                }


        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Emailid = "swapnil@qidtech.com";
        //                                clsLog.WriteLogAzure("FFA MailID:", ex);

        //                            }
        //                        }
        //                        if (!EncodeFFAForSend(ds, "swapnil@qidtech.com", Emailid.Trim(','), strSitaAddress, strMessageVersion, strOriginAddress, MessageID))
        //                        {
        //                            clsLog.WriteLogAzure("FFA not Update:" + ds.Tables[0].Rows[0][0].ToString());
        //                        }
        //                        string[] PName = new string[] { "AWBNo", "AWBPrefix" };
        //                        object[] PValues = new object[] { ds.Tables[0].Rows[0]["AWBNumber"].ToString(), ds.Tables[0].Rows[0]["AWBPrefix"].ToString() };
        //                        SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
        //                        if (!db.ExecuteProcedure("spUpdateFFAStatus", PName, PType, PValues))
        //                        {
        //                            clsLog.WriteLogAzure("Error Status Update:" + ds.Tables[0].Rows[0][0].ToString());
        //                        }

        //                    }
        //                }
        //            }
        //        } while (flag);
        //        db = null;
        //        GC.Collect();
        //    }
        //    catch (Exception ex)
        //    {
        //        clsLog.WriteLogAzure(ex);
        //        clsLog.WriteLogAzure("Exception in FFA send:", ex);
        //    }
        //}
    }
}
