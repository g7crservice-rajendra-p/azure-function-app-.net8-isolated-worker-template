#region FFM Message Processor Class Description
/* FFMMessage Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-03-02
      * Approved By          :
      * Approved Date        :
      * Modified By          :  
      * Modified On          :   
      * Description          :   
     */
#endregion

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Data;
using System.Text.RegularExpressions;

namespace QidWorkerRole
{
    public class FHLMessageProcessor
    {
        //string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        //SCMExceptionHandlingWorkRole scmException = new SCMExceptionHandlingWorkRole();

        private readonly ILogger<FHLMessageProcessor> _logger;
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ISqlDataHelperDao _readOnlyDao;
        private readonly FFRMessageProcessor _ffrMessageProcessor;
        private readonly GenericFunction _genericFunction;

        public FHLMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FHLMessageProcessor> logger,
            FFRMessageProcessor ffrMessageProcessor,
            GenericFunction genericFunction)
        {
            _logger = logger;
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _readOnlyDao = sqlDataHelperFactory.Create(readOnly: true);
            _ffrMessageProcessor = ffrMessageProcessor;
            _genericFunction = genericFunction;
        }

        /// <summary>
        /// Decode received FHL message
        /// </summary>
        public bool DecodeReceiveFHLMessage(string fhlmsg, ref MessageData.fhlinfo fhldata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.customsextrainfo[] custominfo, out string errorMessage)
        {
            string awbref = string.Empty;
            bool flag = false;
            errorMessage = string.Empty;
            try
            {
                string lastrec = "NA", innerrec = "NA";
                int line = 0, version = 4; int textCount = 0;
                try
                {
                    if (fhlmsg.StartsWith("FHL", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] str = fhlmsg.Split('$');
                        if (str.Length > 3)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {

                                flag = true;
                                #region Line 1 version
                                if (str[i].StartsWith("FHL", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        fhldata.fhlversionnum = msg[1];
                                        version = Convert.ToInt16(msg[1]);
                                    }
                                    catch (Exception ex) {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 2 awb consigment details
                                if (str[i].StartsWith("MBI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "AWB";
                                        line = 0;
                                        string[] msg = str[i].Split('/');
                                        //0th element
                                        string[] decmes = msg[1].Split('-');
                                        fhldata.airlineprefix = decmes[0];
                                        fhldata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                                        fhldata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                                        fhldata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                                        //1
                                        if (msg[2].Length > 0)
                                        {
                                            int k = 0;
                                            char lastchr = 'A';
                                            char[] arr = msg[2].ToCharArray();
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
                                            fhldata.consigntype = strarr[0];
                                            fhldata.pcscnt = strarr[1];//int.Parse(strarr[1]);
                                            fhldata.weightcode = strarr[2];
                                            fhldata.weight = strarr[3];//float.Parse(strarr[3]);                                            
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //SCMExceptionHandling.logexception(ref e);
                                        continue;
                                    }
                                }
                                #endregion

                                #region  line 3 onwards check consignment details
                                if (str[i].StartsWith("HBS", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "AWB";
                                        line = 0;
                                        DecodeFHLConsigmentDetails(str[i], ref consinfo, version);
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        continue;
                                    }
                                }
                                #endregion

                                #region  line 4 Free Text
                                if (str[i].StartsWith("TXT", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = "TXT";
                                        line = 0;
                                        textCount = 1;
                                        //consinfo[consinfo.Length - 1].freetextGoodDesc = msg[1].Substring(0, 65);
                                        consinfo[consinfo.Length - 1].freetextGoodDesc = msg[1].Length > 65 ? msg[1].Substring(0, 65) : msg[1];

                                    }
                                    catch (Exception ex)
                                    {
                                        //SCMExceptionHandling.logexception(ref e);
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        continue;
                                    }
                                }
                                #endregion

                                #region  line 4 Harmonised Tariff Schedule
                                if (str[i].StartsWith("HTS", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = "HTS";
                                        line = 0;

                                        string strTempharmonizedcode = msg[1];

                                        Regex r = new Regex("^[A-Z0-9]*$");
                                        if (!r.IsMatch(strTempharmonizedcode))
                                            strTempharmonizedcode = "";

                                        if (!(strTempharmonizedcode.Length > 0 && strTempharmonizedcode.Length >= 6 && strTempharmonizedcode.Length <= 18))
                                            strTempharmonizedcode = "";

                                        consinfo[consinfo.Length - 1].commodity = consinfo[consinfo.Length - 1].commodity.Length > 0 ? consinfo[consinfo.Length - 1].commodity + "," + strTempharmonizedcode : strTempharmonizedcode;

                                        string tmpharmonizedcode = string.Empty;
                                        tmpharmonizedcode = consinfo[consinfo.Length - 1].commodity.ToString();

                                        if (tmpharmonizedcode.Length > 208)
                                        {
                                            int index = tmpharmonizedcode.LastIndexOf(',');
                                            tmpharmonizedcode = tmpharmonizedcode.Substring(0, index);
                                        }
                                        consinfo[consinfo.Length - 1].commodity = tmpharmonizedcode;
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }

                                #endregion

                                #region Line5 custom extra info
                                if (str[i].StartsWith("OCI", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 0)
                                    {
                                        lastrec = "OCI";
                                        MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                        custom.IsoCountryCodeOci = msg[1];
                                        custom.InformationIdentifierOci = msg[2];
                                        custom.CsrIdentifierOci = msg[3];
                                        custom.SupplementaryCsrIdentifierOci = msg[4];
                                        custom.consigref = awbref;
                                        Array.Resize(ref custominfo, custominfo.Length + 1);
                                        custominfo[custominfo.Length - 1] = custom;
                                    }
                                }
                                #endregion

                                #region Line 5 Shipper Infor
                                if (str[i].StartsWith("SHP", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg.Length > 1)
                                        {
                                            fhldata.shippername = msg[1];

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                     }
                                }
                                #endregion

                                #region Line 6 Consignee
                                if (str[i].StartsWith("CNE", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg.Length > 1)
                                        {
                                            fhldata.consname = msg[1];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 11 Charge declaration
                                if (str[i].StartsWith("CVD", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        try
                                        {
                                            fhldata.currency = msg[1];
                                            fhldata.chargecode = msg[2].Length > 0 ? msg[2] : "";
                                            fhldata.chargedec = msg[3].Length > 0 ? msg[3] : "";
                                            fhldata.declaredvalue = msg[3];
                                            fhldata.declaredcustomvalue = msg[4];
                                            fhldata.insuranceamount = msg[5];
                                            line++;
                                        }
                                        catch (Exception ex) {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                         }
                                    }
                                }
                                #endregion

                                #region Second Line Version 5

                                if (fhldata.fhlversionnum == "5")
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');

                                        #region SHP
                                        if (lastrec == "SHP" && (!str[i].StartsWith("SHP")))
                                        {
                                            //line++;
                                            if (str[i].StartsWith("NAM", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "NAM";
                                                fhldata.shippername = msg[1].Length > 0 ? msg[1] : "";
                                            }

                                            if (innerrec == "NAM" && msg[0] == "")
                                            {

                                                //fwbdata.shippername = fwbdata.shippername + " " + msg[1].ToString();
                                                fhldata.shippername2 = msg[1].ToString();
                                            }


                                            if (str[i].StartsWith("ADR", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "ADR";
                                                fhldata.shipperadd = msg[1].Length > 0 ? msg[1] : "";
                                            }

                                            if (innerrec == "ADR" && msg[0] == "")
                                            {

                                                //fwbdata.shipperadd = fwbdata.shipperadd + " " + msg[1].ToString();
                                                fhldata.shipperadd2 = msg[1].ToString();
                                            }


                                            if (str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "LOC";
                                                if (msg.Length > 2)
                                                {
                                                    fhldata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    fhldata.shipperstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                }
                                                else
                                                    fhldata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";

                                            }

                                            if (line == 3 && (!str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                int len = msg.Length, p;

                                                for (p = 0; p < len; p++)
                                                {
                                                    if (p == 1)
                                                        fhldata.shippercountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    if (p == 2)
                                                        fhldata.shipperpostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                    if (p == 3)
                                                        fhldata.shippercontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
                                                    if (p == 4)
                                                        fhldata.shippercontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";
                                                }

                                            }


                                        }

                                        #endregion

                                        #region CNE
                                        if (lastrec == "CNE" && (!str[i].StartsWith("CNE")))
                                        {
                                            //line++;
                                            if (str[i].StartsWith("NAM", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "NAM";
                                                fhldata.consname = msg[1].Length > 0 ? msg[1] : "";
                                            }

                                            if (innerrec == "NAM" && msg[0] == "")
                                            {

                                                //fwbdata.consname = fwbdata.consname + " " + msg[1].ToString();
                                                fhldata.consname2 = msg[1].ToString();
                                            }

                                            if (str[i].StartsWith("ADR", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "ADR";
                                                fhldata.consadd = msg[1].Length > 0 ? msg[1] : "";
                                            }

                                            if (innerrec == "ADR" && msg[0] == "")
                                            {

                                                //fwbdata.consadd = fwbdata.consname + " " + msg[1].ToString();
                                                fhldata.consadd2 = msg[1].ToString();
                                            }

                                            if (str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "LOC";
                                                if (msg.Length > 2)
                                                {
                                                    fhldata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    fhldata.consstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                }
                                                else
                                                    fhldata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";

                                            }

                                            if (line == 3 && (!str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                int p, len = msg.Length;
                                                for (p = 0; p < len; p++)
                                                {
                                                    if (p == 1)
                                                        fhldata.conscountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    if (p == 2)
                                                        fhldata.conspostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                    if (p == 3)
                                                        fhldata.conscontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
                                                    if (p == 4)
                                                        fhldata.conscontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";

                                                }

                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Other Info
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        #region SHP Data
                                        if (lastrec == "SHP" && fhldata.fhlversionnum != "5")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                fhldata.shipperadd = msg[1].Length > 0 ? msg[1] : "";

                                            }
                                            if (line == 2)
                                            {
                                                fhldata.shipperplace = msg[1].Length > 0 ? msg[1] : "";
                                                fhldata.shipperstate = msg[2].Length > 0 ? msg[2] : "";
                                            }
                                            if (line == 3)
                                            {
                                                fhldata.shippercountrycode = msg[1].Length > 0 ? msg[1] : "";
                                                fhldata.shipperpostcode = msg[2].Length > 0 ? msg[2] : "";
                                                fhldata.shippercontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                                fhldata.shippercontactnum = msg[4].Length > 0 ? msg[4] : "";

                                            }

                                        }
                                        #endregion

                                        #region CNE Data
                                        if (lastrec == "CNE" && fhldata.fhlversionnum != "5")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                fhldata.consadd = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                fhldata.consplace = msg[1].Length > 0 ? msg[1] : "";
                                                fhldata.consstate = msg[2].Length > 0 ? msg[2] : "";
                                            }
                                            if (line == 3)
                                            {
                                                fhldata.conscountrycode = msg[1].Length > 0 ? msg[1] : "";
                                                fhldata.conspostcode = msg[2].Length > 0 ? msg[2] : "";
                                                fhldata.conscontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                                fhldata.conscontactnum = msg[4].Length > 0 ? msg[4] : "";
                                            }

                                        }
                                        #endregion

                                        #region Commodity
                                        if (lastrec == "HTS")
                                        {
                                            if (str[i].Length > 1 && msg.Length > 1 && msg[1].Trim() != string.Empty)
                                                consinfo[consinfo.Length - 1].commodity
                                                    = consinfo[consinfo.Length - 1].commodity.Trim() == string.Empty ? msg[1]
                                                    : consinfo[consinfo.Length - 1].commodity.Trim() + "," + msg[1];
                                        }
                                        #endregion

                                        #region freetextGoodDesc
                                        if (lastrec == "TXT")
                                        {
                                            if (str[i].Length > 0)
                                            {
                                                textCount++;
                                                if (textCount <= 9)
                                                {
                                                    //consinfo[consinfo.Length - 1].freetextGoodDesc = msg[1].Substring(0, 65);
                                                    consinfo[consinfo.Length - 1].freetextGoodDesc += (msg[1].Length > 65 ? msg[1].Substring(0, 65) : msg[1]);

                                                }
                                                else
                                                {
                                                    errorMessage = "TXT line format is invalid ";
                                                    return false;
                                                }
                                            }
                                        }
                                        #endregion

                                        #region Splhandling
                                        if (lastrec == "AWB")
                                        {
                                            if (str[i].Length > 1)
                                            {
                                                consinfo[consinfo.Length - 1].splhandling = str[i].Replace('/', ',');
                                            }
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region OCI
                                        if (lastrec == "OCI")
                                        {
                                            string[] msgdata = str[i].Split('/');
                                            if (msgdata.Length > 0)
                                            {
                                                lastrec = "OCI";
                                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                custom.IsoCountryCodeOci = msgdata[1];
                                                custom.InformationIdentifierOci = msgdata[2];
                                                custom.CsrIdentifierOci = msgdata[3];
                                                custom.SupplementaryCsrIdentifierOci = msgdata[4];
                                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                                custominfo[custominfo.Length - 1] = custom;
                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Decode consignment line from message
        /// </summary>
        private void DecodeFHLConsigmentDetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo, int version)
        {
            MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
            try
            {
                string[] msg = inputstr.Split('/');
                consig.awbnum = msg[1];
                consig.origin = msg[2].Substring(0, 3);
                consig.dest = msg[2].Substring(3);
                consig.consigntype = "";
                consig.pcscnt = msg[3];//int.Parse(strarr[1]);
                consig.weightcode = msg[4].Substring(0, 1);
                consig.weight = msg[4].Substring(1);
                if (version == 2)
                {
                    if (msg.Length > 4)
                    {
                        consig.manifestdesc = msg[5];
                    }
                }
                else
                {
                    if (msg.Length > 4)
                    {
                        consig.slac = msg[5];
                    }
                    if (msg.Length > 5)
                    {
                        consig.manifestdesc = msg[6];
                    }
                }

            }
            catch (Exception ex) {
                // clsLog.WriteLogAzure(ex.Message);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
             }
            Array.Resize(ref consinfo, consinfo.Length + 1);
            consinfo[consinfo.Length - 1] = consig;
        }

        /// <summary>
        /// validate and insert house data
        /// </summary>
        public async Task<(bool success, MessageData.fhlinfo fhl, MessageData.consignmnetinfo[] consinfo, MessageData.customsextrainfo[] customextrainfo)> validateAndInsertFHLData(MessageData.fhlinfo fhl, MessageData.consignmnetinfo[] consinfo, MessageData.customsextrainfo[] customextrainfo, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        {
            bool flag = false;
            //GenericFunction gf = new GenericFunction();
            //FFRMessageProcessor ffRMessageProcessor = new FFRMessageProcessor();
            try
            {
                bool isAWBPresent = false;
                string AWBNum = fhl.awbnum;
                string AWBPrefix = fhl.airlineprefix;

                //SQLServer db = new SQLServer();

                _genericFunction.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + AWBNum, string.Empty, string.Empty, string.Empty, "FHL", strMessageFrom, DateTime.Parse("1900-01-01"));

                #region Check AWB Present or Not
                DataSet? ds = new DataSet();

                //string[] pname = new string[] { "AWBNumber", "AWBPrefix" };
                //object[] values = new object[] { AWBNum, AWBPrefix };
                //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                //ds = db.SelectRecords("sp_getawbdetails", pname, values, ptype);

                SqlParameter[] parameters = [
                    new SqlParameter("@AWBNumber",AWBNum)
                    ,new SqlParameter("@AWBPrefix",AWBPrefix)
                ];
                ds = await _readWriteDao.SelectRecords("sp_getawbdetails", parameters);

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            isAWBPresent = true;
                        }
                    }
                }
                #endregion

                #region Add AWB details
                if (!isAWBPresent)
                {
                    SqlParameter[] sqlParameter = [
                        new SqlParameter("@RefNo",REFNo)
                        ,new SqlParameter("@Status","Processed")
                        ,new SqlParameter("@Error","AWB number not present")
                        ,new SqlParameter("@UpdatedOn",DateTime.Now)
                    ];
                    /*Replaced SelectRecords as it does update operation and not returning any dataset*/
                    //db.SelectRecords("uspUpdateMsgFromInbox", sqlParameter);
                    await _readWriteDao.ExecuteNonQueryAsync("uspUpdateMsgFromInbox", sqlParameter);
                    return (false, fhl, consinfo, customextrainfo);
                }
                #endregion

                #region Add HAWB Details
                for (int i = 0; i < consinfo.Length; i++)
                {
                    consinfo[i].weight = consinfo[i].weight.Length > 0 ? consinfo[i].weight : "0";
                    consinfo[i].pcscnt = consinfo[i].pcscnt.Length > 0 ? consinfo[i].pcscnt : "0";
                    string HAWBNo = consinfo[i].awbnum;
                    int HAWBPcs = Convert.ToInt16(consinfo[i].pcscnt.ToString());
                    float HAWBWt = float.Parse(consinfo[i].weight.ToString());
                    string Origin = consinfo[i].origin;
                    string Destination = consinfo[i].dest;
                    string description = consinfo[i].manifestdesc;
                    string commodity = consinfo[i].commodity;
                    string txtDesc = consinfo[i].freetextGoodDesc;
                    string SHC = consinfo[i].splhandling;
                    string CustID = "";
                    string CustName = fhl.shippername;
                    string CustAddress = fhl.shipperadd.Trim(',');
                    string City = fhl.shipperplace.Trim(',');
                    string Zipcode = fhl.shipperpostcode;
                    string HAWBPrefix = consinfo[i].airlineprefix;
                    string slac = consinfo[i].pcscnt.ToString();
                    decimal insuranceAmount = fhl.insuranceamount.Trim() == "" ? 0 :
                        (fhl.insuranceamount.Trim() == "X" || fhl.insuranceamount.Trim() == "XX" || fhl.insuranceamount.Trim() == "XXX") ? 0 : Convert.ToDecimal(fhl.insuranceamount);
                    decimal DecValueHAWB = fhl.declaredcustomvalue.ToUpper() == "" ? 0 : fhl.declaredcustomvalue.ToUpper() == "NCV" ? 0 : System.Convert.ToDecimal(fhl.declaredcustomvalue.ToUpper());
                    slac = consinfo[i].slac.Length > 0 ? consinfo[i].slac : consinfo[i].pcscnt;

                    Decimal DecValueCarriageHAWB = fhl.declaredvalue.ToUpper() == "" ? 0 : fhl.declaredvalue.ToUpper() == "NVD" ? 0 : System.Convert.ToDecimal(fhl.declaredvalue.ToUpper());


                    flag = await PutHAWBDetails(AWBNum, HAWBNo, HAWBPcs, HAWBWt, description, CustID, CustName, CustAddress, City, Zipcode, Origin, Destination, SHC, HAWBPrefix, AWBPrefix, "", "", "", "", "",
                        fhl.consname, fhl.consadd.Trim(','), fhl.consplace.Trim(','), fhl.consstate, fhl.conscountrycode.Trim(','), fhl.conspostcode, fhl.shipperstate, fhl.shippercountrycode, "", slac, "", "",
                        fhl.shippercontactnum, "", fhl.conscontactnum, customextrainfo, commodity, REFNo, DecValueHAWB, DecValueCarriageHAWB, fhl.currency, insuranceAmount, txtDesc);
                }
                #endregion

                #region ProcessRateFunction
                //if (ds.Tables[0].Rows[0]["IsAccepted"].ToString() == "True")
                //{
                DataSet? dsrateCheck = await _ffrMessageProcessor.CheckAirlineForRateProcessing(AWBPrefix, "FHL");
                if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
                {
                    bool billingReprocess = false;
                    if (ds.Tables[0].Rows[0]["IsAccepted"].ToString() == "True")
                        billingReprocess = true;

                    //string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
                    //SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                    //object[] CRValues = new object[] { AWBNum, AWBPrefix, "FHL", System.DateTime.Now, 1, 1, "B", billingReprocess };

                    SqlParameter[] parameters1 =
                    {
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FHL" },
                        new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                        new SqlParameter("@ValidateMin", SqlDbType.Bit) { Value = 1 },
                        new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = 1 },
                        new SqlParameter("@RouteFrom", SqlDbType.VarChar) { Value = "B" },
                        new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = billingReprocess }
                    };

                    //if (!db.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
                    var dbRes1 = await _readWriteDao.ExecuteNonQueryAsync("sp_CalculateAWBRatesReprocess", parameters1);
                    if (!dbRes1)
                    {
                        // clsLog.WriteLogAzure("Rates Not Calculated for:" + AWBNum + Environment.NewLine);
                        _logger.LogWarning("Rates Not Calculated for: {0}" , AWBNum + Environment.NewLine);
                    }
                }
                //}

                #endregion
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            //return flag;
            return (flag, fhl, consinfo, customextrainfo);

        }

        /// <summary>
        /// Save HAWB Details
        /// </summary>
        public async Task<bool> PutHAWBDetails(string MAWBNo, string HAWBNo, int HAWBPcs, float HAWBWt, string Description, string CustID, string CustName,
            string CustAddress, string CustCity, string Zipcode, string Origin, string Destination, string SHC,
            string HAWBPrefix, string AWBPrefix, string FltOrigin, string FltDest, string ArrivalStatus, string FlightNo,
            string FlightDt, string ConsigneeName, string ConsigneeAddress, string ConsigneeCity, string ConsigneeState, string ConsigneeCountry, string ConsigneePostalCode,
            string CustState, string CustCountry, string UOM, string SLAC, string ConsigneeID, string ShipperEmail, string ShipperTelephone, string ConsigneeEmail,
            string ConsigneeTelephone, MessageData.customsextrainfo[] customextrainfo, string harmonizedcodes, int REFNo, decimal DecValueHAWB,
            decimal DecValueCarriageHAWB, string currency, decimal insuranceAmount, string DescriptionOfGoods)
        {
            try
            {
                #region : OCI :
                string customShipIDCode = string.Empty, customConsIDCode = string.Empty, customShipperTelephone = string.Empty;
                string customConsigneeTelephone = string.Empty, customConsigneeContactPerson = string.Empty, customShipAEONum = string.Empty, customConsAEONum = string.Empty;
                for (int i = 0; i < customextrainfo.Length; i++)
                {
                    switch (customextrainfo[i].CsrIdentifierOci)
                    {
                        case "T":
                            if (customextrainfo[i].InformationIdentifierOci == "SHP")
                            {
                                customShipIDCode = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            else if (customextrainfo[i].InformationIdentifierOci == "CNE")
                            {
                                customConsIDCode = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            break;
                        case "U":
                            if (customextrainfo[i].InformationIdentifierOci == "CNE")
                            {
                                customConsigneeTelephone = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            break;
                        case "KC":
                            if (customextrainfo[i].InformationIdentifierOci == "CNE")
                            {
                                customConsigneeContactPerson = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            break;
                        case "E":
                            if (customextrainfo[i].InformationIdentifierOci == "SHP")
                            {
                                customShipAEONum = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            else if (customextrainfo[i].InformationIdentifierOci == "CNE")
                            {
                                customConsAEONum = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            break;
                        default:
                            break;
                    }
                }
                #endregion OCI
                DataSet? ds = new DataSet();

                //SQLServer da = new SQLServer();

                //string[] paramname = new string[50];
                //paramname[0] = "MAWBNo";
                //paramname[1] = "HAWBNo";
                //paramname[2] = "HAWBPcs";
                //paramname[3] = "HAWBWt";
                //paramname[4] = "Description";
                //paramname[5] = "CustID";
                //paramname[6] = "CustName";
                //paramname[7] = "CustAddress";
                //paramname[8] = "CustCity";
                //paramname[9] = "Zipcode";
                //paramname[10] = "Origin";
                //paramname[11] = "Destination";
                //paramname[12] = "SHC";
                //paramname[13] = "HAWBPrefix";
                //paramname[14] = "AWBPrefix";
                //paramname[15] = "ArrivalStatus";
                //paramname[16] = "FlightNo";
                //paramname[17] = "FlightDt";
                //paramname[18] = "FlightOrigin";
                //paramname[19] = "flightDest";
                //paramname[20] = "ConsigneeName";
                //paramname[21] = "ConsigneeAddress";
                //paramname[22] = "ConsigneeCity";
                //paramname[23] = "ConsigneeState";
                //paramname[24] = "ConsigneeCountry";
                //paramname[25] = "ConsigneePostalCode";
                //paramname[26] = "CustState";
                //paramname[27] = "CustCountry";
                //paramname[28] = "UOM";
                //paramname[29] = "SLAC";
                //paramname[30] = "ConsigneeID";
                //paramname[31] = "ShipperEmail";
                //paramname[32] = "ShipperTelephone";
                //paramname[33] = "ConsigneeEmail";
                //paramname[34] = "ConsigneeTelephone";

                //paramname[35] = "ShipIDCode";
                //paramname[36] = "ShipAEONum";
                //paramname[37] = "ConsIDCode";
                //paramname[38] = "ConsAEONum";
                //paramname[39] = "ConsContactName";
                //paramname[40] = "ConsContactTelephone";
                //paramname[41] = "Customs";
                //paramname[42] = "DecValueHAWB";
                //paramname[43] = "ConsigEORICode";
                //paramname[44] = "ShippEORICode";
                //paramname[45] = "REFNo";
                //paramname[46] = "DecValueCarriage";
                //paramname[47] = "HAWBInsuranceAmt";
                //paramname[48] = "HAWBCurrency";
                //paramname[49] = "DescriptionOfGoods";

                //object[] paramvalue = new object[50];
                //paramvalue[0] = MAWBNo;
                //paramvalue[1] = HAWBNo;
                //paramvalue[2] = HAWBPcs;
                //paramvalue[3] = HAWBWt;
                //paramvalue[4] = Description;
                //paramvalue[5] = CustID;
                //paramvalue[6] = CustName;
                //paramvalue[7] = CustAddress;
                //paramvalue[8] = CustCity;
                //paramvalue[9] = Zipcode;
                //paramvalue[10] = Origin;
                //paramvalue[11] = Destination;
                //paramvalue[12] = SHC;
                //paramvalue[13] = HAWBPrefix;
                //paramvalue[14] = AWBPrefix;
                //paramvalue[15] = ArrivalStatus;
                //paramvalue[16] = FlightNo;
                //if (FlightDt == "")
                //{
                //    paramvalue[17] = DateTime.Now.ToString();
                //}
                //else
                //{
                //    paramvalue[17] = FlightDt;
                //}
                //paramvalue[18] = FltOrigin;
                //paramvalue[19] = FltDest;
                //paramvalue[20] = ConsigneeName;
                //paramvalue[21] = ConsigneeAddress;
                //paramvalue[22] = ConsigneeCity;
                //paramvalue[23] = ConsigneeState;
                //paramvalue[24] = ConsigneeCountry;
                //paramvalue[25] = ConsigneePostalCode;
                //paramvalue[26] = CustState;
                //paramvalue[27] = CustCountry;
                //paramvalue[28] = UOM;
                //paramvalue[29] = SLAC != string.Empty ? SLAC : "0";
                //paramvalue[30] = ConsigneeID;
                //paramvalue[31] = ShipperEmail;
                //paramvalue[32] = ShipperTelephone;
                //paramvalue[33] = ConsigneeEmail;
                //paramvalue[34] = ConsigneeTelephone;
                //paramvalue[35] = customShipIDCode;
                //paramvalue[36] = customShipAEONum;
                //paramvalue[37] = customConsIDCode;
                //paramvalue[38] = customConsAEONum;
                //paramvalue[39] = customConsigneeContactPerson;
                //paramvalue[40] = customConsigneeTelephone;
                //paramvalue[41] = harmonizedcodes;
                //paramvalue[42] = DecValueHAWB;
                //paramvalue[43] = customConsIDCode;
                //paramvalue[44] = customShipIDCode;
                //paramvalue[45] = REFNo;
                //paramvalue[46] = DecValueCarriageHAWB;
                //paramvalue[47] = insuranceAmount;
                //paramvalue[48] = currency;
                //paramvalue[49] = DescriptionOfGoods;

                //SqlDbType[] paramtype = new SqlDbType[50];
                //paramtype[0] = SqlDbType.VarChar;
                //paramtype[1] = SqlDbType.VarChar;
                //paramtype[2] = SqlDbType.Int;
                //paramtype[3] = SqlDbType.Float;
                //paramtype[4] = SqlDbType.VarChar;
                //paramtype[5] = SqlDbType.VarChar;
                //paramtype[6] = SqlDbType.VarChar;
                //paramtype[7] = SqlDbType.VarChar;
                //paramtype[8] = SqlDbType.VarChar;
                //paramtype[9] = SqlDbType.VarChar;
                //paramtype[10] = SqlDbType.VarChar;
                //paramtype[11] = SqlDbType.VarChar;
                //paramtype[12] = SqlDbType.VarChar;
                //paramtype[13] = SqlDbType.VarChar;
                //paramtype[14] = SqlDbType.VarChar;
                //paramtype[15] = SqlDbType.VarChar;
                //paramtype[16] = SqlDbType.VarChar;
                //paramtype[17] = SqlDbType.DateTime;
                //paramtype[18] = SqlDbType.VarChar;
                //paramtype[19] = SqlDbType.VarChar;
                //paramtype[20] = SqlDbType.VarChar;
                //paramtype[21] = SqlDbType.VarChar;
                //paramtype[22] = SqlDbType.VarChar;
                //paramtype[23] = SqlDbType.VarChar;
                //paramtype[24] = SqlDbType.VarChar;
                //paramtype[25] = SqlDbType.VarChar;
                //paramtype[26] = SqlDbType.VarChar;
                //paramtype[27] = SqlDbType.VarChar;
                //paramtype[28] = SqlDbType.VarChar;
                //paramtype[29] = SqlDbType.Int;
                //paramtype[30] = SqlDbType.VarChar;
                //paramtype[31] = SqlDbType.VarChar;
                //paramtype[32] = SqlDbType.VarChar;
                //paramtype[33] = SqlDbType.VarChar;
                //paramtype[34] = SqlDbType.VarChar;
                //paramtype[35] = SqlDbType.VarChar;
                //paramtype[36] = SqlDbType.VarChar;
                //paramtype[37] = SqlDbType.VarChar;
                //paramtype[38] = SqlDbType.VarChar;
                //paramtype[39] = SqlDbType.VarChar;
                //paramtype[40] = SqlDbType.VarChar;
                //paramtype[41] = SqlDbType.VarChar;
                //paramtype[42] = SqlDbType.Decimal;
                //paramtype[43] = SqlDbType.VarChar;
                //paramtype[44] = SqlDbType.VarChar;
                //paramtype[45] = SqlDbType.Int;
                //paramtype[46] = SqlDbType.Decimal;
                //paramtype[47] = SqlDbType.Decimal;
                //paramtype[48] = SqlDbType.VarChar;
                //paramtype[49] = SqlDbType.VarChar;


                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAWBNo", SqlDbType.VarChar) { Value = MAWBNo },
                    new SqlParameter("@HAWBNo", SqlDbType.VarChar) { Value = HAWBNo },
                    new SqlParameter("@HAWBPcs", SqlDbType.Int) { Value = HAWBPcs },
                    new SqlParameter("@HAWBWt", SqlDbType.Float) { Value = HAWBWt },
                    new SqlParameter("@Description", SqlDbType.VarChar) { Value = Description },
                    new SqlParameter("@CustID", SqlDbType.VarChar) { Value = CustID },
                    new SqlParameter("@CustName", SqlDbType.VarChar) { Value = CustName },
                    new SqlParameter("@CustAddress", SqlDbType.VarChar) { Value = CustAddress },
                    new SqlParameter("@CustCity", SqlDbType.VarChar) { Value = CustCity },
                    new SqlParameter("@Zipcode", SqlDbType.VarChar) { Value = Zipcode },
                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = Origin },
                    new SqlParameter("@Destination", SqlDbType.VarChar) { Value = Destination },
                    new SqlParameter("@SHC", SqlDbType.VarChar) { Value = SHC },
                    new SqlParameter("@HAWBPrefix", SqlDbType.VarChar) { Value = HAWBPrefix },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@ArrivalStatus", SqlDbType.VarChar) { Value = ArrivalStatus },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightDt", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(FlightDt) ? DateTime.Now : DateTime.Parse(FlightDt) },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = FltOrigin },
                    new SqlParameter("@flightDest", SqlDbType.VarChar) { Value = FltDest },
                    new SqlParameter("@ConsigneeName", SqlDbType.VarChar) { Value = ConsigneeName },
                    new SqlParameter("@ConsigneeAddress", SqlDbType.VarChar) { Value = ConsigneeAddress },
                    new SqlParameter("@ConsigneeCity", SqlDbType.VarChar) { Value = ConsigneeCity },
                    new SqlParameter("@ConsigneeState", SqlDbType.VarChar) { Value = ConsigneeState },
                    new SqlParameter("@ConsigneeCountry", SqlDbType.VarChar) { Value = ConsigneeCountry },
                    new SqlParameter("@ConsigneePostalCode", SqlDbType.VarChar) { Value = ConsigneePostalCode },
                    new SqlParameter("@CustState", SqlDbType.VarChar) { Value = CustState },
                    new SqlParameter("@CustCountry", SqlDbType.VarChar) { Value = CustCountry },
                    new SqlParameter("@UOM", SqlDbType.VarChar) { Value = UOM },
                    new SqlParameter("@SLAC", SqlDbType.Int) { Value = string.IsNullOrEmpty(SLAC) ? 0 : Convert.ToInt32(SLAC) },
                    new SqlParameter("@ConsigneeID", SqlDbType.VarChar) { Value = ConsigneeID },
                    new SqlParameter("@ShipperEmail", SqlDbType.VarChar) { Value = ShipperEmail },
                    new SqlParameter("@ShipperTelephone", SqlDbType.VarChar) { Value = ShipperTelephone },
                    new SqlParameter("@ConsigneeEmail", SqlDbType.VarChar) { Value = ConsigneeEmail },
                    new SqlParameter("@ConsigneeTelephone", SqlDbType.VarChar) { Value = ConsigneeTelephone },
                    new SqlParameter("@ShipIDCode", SqlDbType.VarChar) { Value = customShipIDCode },
                    new SqlParameter("@ShipAEONum", SqlDbType.VarChar) { Value = customShipAEONum },
                    new SqlParameter("@ConsIDCode", SqlDbType.VarChar) { Value = customConsIDCode },
                    new SqlParameter("@ConsAEONum", SqlDbType.VarChar) { Value = customConsAEONum },
                    new SqlParameter("@ConsContactName", SqlDbType.VarChar) { Value = customConsigneeContactPerson },
                    new SqlParameter("@ConsContactTelephone", SqlDbType.VarChar) { Value = customConsigneeTelephone },
                    new SqlParameter("@Customs", SqlDbType.VarChar) { Value = harmonizedcodes },
                    new SqlParameter("@DecValueHAWB", SqlDbType.Decimal) { Value = DecValueHAWB },
                    new SqlParameter("@ConsigEORICode", SqlDbType.VarChar) { Value = customConsIDCode },
                    new SqlParameter("@ShippEORICode", SqlDbType.VarChar) { Value = customShipIDCode },
                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                    new SqlParameter("@DecValueCarriage", SqlDbType.Decimal) { Value = DecValueCarriageHAWB },
                    new SqlParameter("@HAWBInsuranceAmt", SqlDbType.Decimal) { Value = insuranceAmount },
                    new SqlParameter("@HAWBCurrency", SqlDbType.VarChar) { Value = currency },
                    new SqlParameter("@DescriptionOfGoods", SqlDbType.VarChar) { Value = DescriptionOfGoods }
                };


                //if (da.ExecuteProcedure("SP_PutHAWBDetails_V2", paramname, paramtype, paramvalue))
                var dbRes = await _readWriteDao.ExecuteNonQueryAsync("SP_PutHAWBDetails_V2", parameters);
                if (dbRes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return false;
            }
        }

        /// <summary>
        /// Used to generate FHL Message
        /// </summary>
        public async Task GenerateFHL(string DepartureAirport, string flightdest, string FlightNo, string FlightDate, DateTime itdate)
        {
            string SitaMessageHeader = string.Empty, FHLMessageversion = string.Empty, Emailaddress = string.Empty, SFTPHeaderSITAddress = string.Empty, error = string.Empty;

            //GenericFunction gf = new GenericFunction();

            MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
            MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
            MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];

            //FHLMessageProcessor FHL = new FHLMessageProcessor();

            DataSet dsData = _genericFunction.GetRecordforGenerateFBLMessage(DepartureAirport, flightdest, FlightNo, FlightDate);

            if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
            {

                DataSet dsmessage = _genericFunction.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FHL", "AIR", DepartureAirport, flightdest, FlightNo, string.Empty);

                if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                {
                    Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                    string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                    FHLMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();
                    if (MessageCommunicationType.Equals("ALL", StringComparison.OrdinalIgnoreCase) || MessageCommunicationType.Equals("SITA", StringComparison.OrdinalIgnoreCase))
                        SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                    if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                        SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                }
                DataTable dt = new DataTable();


                if (dsData.Tables[1] != null && dsData.Tables[1].Rows.Count > 0)
                {

                    dt = dsData.Tables[1];
                    dt = GenericFunction.SelectDistinct(dt, "AWBNumbers");
                    string carrierCode = string.Empty;
                    carrierCode = FlightNo.Trim() == "" || FlightNo.Trim().Length < 2 ? "" : FlightNo.Trim().Substring(0, 2);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            string awbNumber = string.Empty, awbPrefix = string.Empty;
                            awbNumber = dt.Rows[i]["AWBNumbers"].ToString().Substring(4, 8);
                            awbPrefix = dt.Rows[i]["AWBNumbers"].ToString().Substring(0, 3);
                            string FHLMsg = string.Empty;
                            (FHLMsg, error) = await EncodeFHLForSend(awbNumber, awbPrefix, error, FHLMessageversion);
                            if (FHLMsg.Length > 3)
                            {
                                string[] msg = FHLMsg.Split('~');
                                GenericFunction genericFunction = new GenericFunction();
                                for (int j = 0; j < msg.Length; j++)
                                {
                                    string fhlMessage = msg[j].Trim();
                                    genericFunction.SaveMessageToOutbox(string.Empty, fhlMessage, Emailaddress, SitaMessageHeader, SFTPHeaderSITAddress, "FHL", awbPrefix + "-" + awbNumber
                                        , FlightNo, FlightDate.ToString(), DepartureAirport, flightdest, carrierCode);
                                }
                            }
                        }
                        catch (Exception ex) {
                            // clsLog.WriteLogAzure(ex); 
                            _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                    }
                }
            }
        }

        public async Task GenerateFHL(string DepartureAirport, string flightdest, string FlightNo, string FlightDate, DateTime itdate, DateTime lstFBLSent, bool isAutoSendOnTriggerTime)
        {
            try
            {
                string SitaMessageHeader = string.Empty, FHLMessageversion = string.Empty, Emailaddress = string.Empty, SFTPHeaderSITAddress = string.Empty, error = string.Empty;
    
                //GenericFunction gf = new GenericFunction();
    
                MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
    
                //FHLMessageProcessor FHL = new FHLMessageProcessor();
    
                DataSet dsData = _genericFunction.GetRecordforGenerateFBLMessage(DepartureAirport, flightdest, FlightNo, FlightDate);
    
                if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
                {
                    DataSet dsmessage = _genericFunction.GetSitaAddressandMessageVersionForAutoMessage(FlightNo.Substring(0, 2), "FHL", "AIR", DepartureAirport, flightdest, FlightNo, string.Empty, string.Empty, string.Empty, isAutoSendOnTriggerTime: isAutoSendOnTriggerTime);
    
                    if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                    {
                        Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                        FHLMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();
                        if (MessageCommunicationType.Equals("ALL", StringComparison.OrdinalIgnoreCase) || MessageCommunicationType.Equals("SITA", StringComparison.OrdinalIgnoreCase))
                            SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                        if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                            SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                    }
                    DataTable dt = new DataTable();
    
    
                    if (dsData.Tables[1] != null && dsData.Tables[1].Rows.Count > 0)
                    {
    
                        dt = dsData.Tables[1];
                        dt = GenericFunction.SelectDistinct(dt, "AWBNumbers");
    
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
    
                                //if ((Convert.ToDateTime(dt.Rows[i]["UTCUpdatedOn"].ToString()) <= lstFBLSent) && (lstFBLSent != Convert.ToDateTime("1900-01-01 00:00:00.000")))
                                //    return;
    
                                string awbPrefix = string.Empty, awbNumber = string.Empty, carrierCode = string.Empty;
                                awbPrefix = dt.Rows[i]["AWBNumbers"].ToString().Substring(0, 3);
                                awbNumber = dt.Rows[i]["AWBNumbers"].ToString().Substring(4, 8);
                                carrierCode = FlightNo.Trim() == "" || FlightNo.Trim().Length < 2 ? "" : FlightNo.Trim().Substring(0, 2);
    
                                //string FHLMsg = await EncodeFHLForSend(awbNumber, awbPrefix, ref error, FHLMessageversion);
                                string FHLMsg = string.Empty;
                                (FHLMsg, error) = await EncodeFHLForSend(awbNumber, awbPrefix, error, FHLMessageversion);
    
                                if (FHLMsg.Length > 3)
                                {
                                    string[] msg = FHLMsg.Split('~');
    
                                    //GenericFunction genericFunction = new GenericFunction();
    
                                    for (int j = 0; j < msg.Length; j++)
                                    {
                                        string fhlMessage = msg[j].Trim();
                                        _genericFunction.SaveMessageToOutbox(string.Empty, fhlMessage, Emailaddress, SitaMessageHeader, SFTPHeaderSITAddress, "FHL", awbPrefix + "-" + awbNumber
                                            , FlightNo, FlightDate.ToString(), DepartureAirport, flightdest, carrierCode);
                                        if (fhlMessage.Trim().Length > 3)
                                        {
                                            if (SitaMessageHeader.Trim().Length > 0)
                                                _genericFunction.SaveMessageOutBox("SITA:FHL", SitaMessageHeader.Trim() + "\r\n" + fhlMessage.Trim(), "", "SITAFTP", DepartureAirport, flightdest, FlightNo, FlightDate.ToString(), awbPrefix + "-" + awbNumber, "Auto", "FHL");
    
                                            if (SFTPHeaderSITAddress.Trim().Length > 0)
                                                _genericFunction.SaveMessageOutBox("SITA:FHL", SFTPHeaderSITAddress.Trim() + "\r\n" + fhlMessage.Trim(), "", "SFTP", DepartureAirport, flightdest, FlightNo, FlightDate.ToString(), awbPrefix + "-" + awbNumber, "Auto", "FHL");
    
                                            if (Emailaddress.Trim().Length > 0)
                                                _genericFunction.SaveMessageOutBox("FHL", fhlMessage.Trim(), "", Emailaddress.Trim(), DepartureAirport, flightdest, FlightNo, FlightDate.ToString(), awbPrefix + "-" + awbNumber, "Auto", "FHL");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                // clsLog.WriteLogAzure(ex); 
                                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Get FHL information and Encode FHL Message
        /// </summary>
        /// <returns>Return message (with seperator'~' for multiple houses)</returns>
        //public static async Task<string> EncodeFHLForSend(string AWBNo, string AWBPrefix, ref string Error, string MsgVer, string SitaMessageHeader = "", string Emailaddress = "", string SFTPHeaderSITAddress = "")
        public async Task<(string fhlMsg, string Error)> EncodeFHLForSend(string AWBNo, string AWBPrefix, string Error, string MsgVer, string SitaMessageHeader = "", string Emailaddress = "", string SFTPHeaderSITAddress = "")
        {
            string fhlMessage = string.Empty;
            //GenericFunction gf = new GenericFunction();
            try
            {
                DataSet? ds = new DataSet();

                //SQLServer da = new SQLServer(true);
                //string[] paramname = new string[] { "MAWBNo", "MAWBPrefix" };
                //object[] paramvalue = new object[] { AWBNo, AWBPrefix };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                SqlParameter[] parameters =
                [
                    new SqlParameter("@MAWBNo", SqlDbType.VarChar) { Value = AWBNo },
                    new SqlParameter("@MAWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                ];

                //ds = da.SelectRecords("spGetHAWBSummary", paramname, paramvalue, paramtype);
                ds = await _readOnlyDao.SelectRecords("spGetHAWBSummary", parameters);

                MessageData.fhlinfo fhl = new MessageData.fhlinfo("");
                MessageData.consignmnetinfo[] objTempConsInfo = new MessageData.consignmnetinfo[1];
                MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        string WeightCode = string.Empty;
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            //Master AWB number
                            DataRow dr = ds.Tables[0].Rows[0];
                            if (MsgVer.Length > 0)
                            {
                                fhl.fhlversionnum = MsgVer;
                            }
                            else
                            {
                                fhl.fhlversionnum = "4";
                            }
                            fhl.airlineprefix = dr["AWBPrefix"].ToString();
                            fhl.awbnum = dr["AWBNumber"].ToString();
                            fhl.origin = dr["OriginCode"].ToString();
                            fhl.dest = dr["DestinationCode"].ToString();
                            fhl.consigntype = "T";
                            fhl.pcscnt = dr["PiecesCount"].ToString();
                            fhl.weightcode = dr["UOM"].ToString();
                            fhl.weight = dr["GrossWeight"].ToString();
                            WeightCode = dr["UOM"].ToString();
                            fhl.declaredvalue = "NVD";
                        }
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                            {
                                try
                                {
                                    DataRow dr = ds.Tables[1].Rows[i];
                                    objTempConsInfo[0] = new MessageData.consignmnetinfo("");
                                    objTempConsInfo[0].awbnum = dr["HAWBNo"].ToString();
                                    objTempConsInfo[0].origin = dr["Origin"].ToString().ToUpper();
                                    objTempConsInfo[0].dest = dr["Destination"].ToString();
                                    objTempConsInfo[0].consigntype = "";
                                    objTempConsInfo[0].pcscnt = dr["HAWBPcs"].ToString();
                                    objTempConsInfo[0].weightcode = WeightCode != "" ? WeightCode : "K";
                                    objTempConsInfo[0].commodity = dr["Customs"].ToString().Trim();
                                    try
                                    {
                                        objTempConsInfo[0].weight = dr["HAWBWt"].ToString();
                                        if ((Convert.ToDouble(dr["HAWBWt"].ToString()) - Convert.ToInt32(dr["HAWBWt"].ToString())) == 0)
                                        {
                                            objTempConsInfo[0].weight = Convert.ToInt32(dr["HAWBWt"].ToString()).ToString();
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //BAL.SCMException.logexception(ref ex);
                                    }
                                    objTempConsInfo[0].manifestdesc = dr["Description"].ToString();
                                    objTempConsInfo[0].splhandling = dr["SHC"].ToString().Length > 0 ? dr["SHC"].ToString() : "";
                                    objTempConsInfo[0].slac = dr["SLAC"].ToString();
                                    if (dr["DescriptionOfGoods"].ToString().Length > 0)
                                    {
                                        objTempConsInfo[0].freetextGoodDesc = dr["DescriptionOfGoods"].ToString().ToUpper();
                                    }
                                    fhl.shippername = dr["ShipperName"].ToString();
                                    string ShipAdd = dr["ShipperAddress"].ToString() + dr["ShipperAdd2"].ToString();
                                    if (ShipAdd.Length > 35)
                                    {
                                        fhl.shipperadd = ShipAdd.Substring(0, 35);
                                    }
                                    else
                                    {
                                        fhl.shipperadd = dr["ShipperAddress"].ToString() + dr["ShipperAdd2"].ToString();
                                    }
                                    fhl.shipperplace = dr["ShipperCity"].ToString();
                                    fhl.shipperstate = dr["ShipperState"].ToString();
                                    fhl.shippercountrycode = dr["ShipperCountry"].ToString();
                                    fhl.shipperpostcode = dr["ShipperPincode"].ToString();
                                    fhl.shippercontactidentifier = dr["ShipperTelephone"].ToString().Length > 0 ? "TE" : string.Empty;
                                    fhl.shippercontactnum = dr["ShipperTelephone"].ToString();

                                    //6 consignee info                    
                                    fhl.consname = dr["ConsigneeName"].ToString();
                                    string consAdd = dr["ConsigneeAddress"].ToString() + dr["ConsigneeAddress2"].ToString();
                                    if (consAdd.Length > 35)
                                    {
                                        fhl.consadd = consAdd.Substring(0, 35);
                                    }
                                    else
                                    {
                                        fhl.consadd = dr["ConsigneeAddress"].ToString() + dr["ConsigneeAddress2"].ToString();
                                    }
                                    fhl.consplace = dr["ConsigneeCity"].ToString();
                                    fhl.consstate = dr["ConsigneeState"].ToString();
                                    fhl.conscountrycode = dr["ConsigneeCountry"].ToString();
                                    fhl.conspostcode = dr["ConsigneePincode"].ToString();
                                    fhl.conscontactidentifier = dr["ConsigneeTelephone"].ToString().Length > 0 ? "TE" : string.Empty;
                                    fhl.conscontactnum = dr["ConsigneeTelephone"].ToString();
                                    fhl.declaredcustomvalue = dr["DecValueHAWB"].ToString().ToUpper() == "0.00" || dr["DecValueHAWB"].ToString().ToUpper() == "0" ? "NCV" : dr["DecValueHAWB"].ToString().ToUpper();
                                    string PaymentMode = string.Empty;
                                    PaymentMode = dr["PayMode"].ToString().ToUpper();
                                    fhl.chargedec = (PaymentMode.Length < 0 ? "PP" : PaymentMode);
                                    fhl.currency = dr["HAWBCurrency"].ToString();
                                    fhl.insuranceamount = dr["HAWBInsuranceAmt"].ToString();

                                    fhl.declaredcarriervalue = dr["DecValueCarriage"].ToString().ToUpper() == "0.0" || dr["DecValueCarriage"].ToString().ToUpper() == "0.00" || dr["DecValueCarriage"].ToString().ToUpper() == "0.000" || dr["DecValueCarriage"].ToString().ToUpper() == "0" ? "NVD" : dr["DecValueCarriage"].ToString().ToUpper();

                                    if (string.IsNullOrEmpty(fhl.declaredcarriervalue))
                                    {
                                        fhl.declaredcarriervalue = "NVD";
                                    }

                                    try
                                    {
                                        #region OCI Information
                                        if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
                                        {
                                            custominfo = new MessageData.customsextrainfo[0];
                                            DataTable dtOCI = ds.Tables[2].Select("HAWBNo='" + objTempConsInfo[0].awbnum + "'").CopyToDataTable();

                                            if (dtOCI != null && dtOCI.Rows.Count > 0)
                                            {
                                                for (int j = 0; j < dtOCI.Rows.Count; j++)
                                                {
                                                    MessageData.customsextrainfo custInfo = new MessageData.customsextrainfo("");
                                                    custInfo.IsoCountryCodeOci = dtOCI.Rows[j]["CountryCode"].ToString();
                                                    custInfo.InformationIdentifierOci = dtOCI.Rows[j]["InformationIdentifier"].ToString();
                                                    custInfo.CsrIdentifierOci = dtOCI.Rows[j]["CustomsIdentifier"].ToString();
                                                    custInfo.SupplementaryCsrIdentifierOci = dtOCI.Rows[j]["CustomsInfo"].ToString().Trim();
                                                    Array.Resize(ref custominfo, custominfo.Length + 1);
                                                    custominfo[custominfo.Length - 1] = custInfo;
                                                }
                                            }
                                        }
                                        #endregion

                                        string tempFHL = string.Empty;
                                        tempFHL = cls_Encode_Decode.EncodeFHLforsend(ref fhl, ref objTempConsInfo, ref custominfo);
                                        if (fhlMessage != string.Empty)
                                            fhlMessage += "~" + tempFHL.Trim();
                                        else
                                            fhlMessage = tempFHL.Trim();
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                            }
                        }
                        else
                        {
                            Error = "Required Data Not Availabe for Message";
                            //return fhlMessage;
                            return (fhlMessage, Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                Error = ex.Message;
            }
            //return fhlMessage;
            return (fhlMessage, Error);
        }

        /// <summary>
        /// Method to generate FHL messages for All AWB from the flight
        /// Method added by prashant
        /// </summary>
        public async Task GenerateFHL(string PartnerCode, string DepartureAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate, string AWBnumbers)
        {
            try
            {
                string FlightDestination = string.Empty;
    
                //GenericFunction genericFunction = new GenericFunction();
    
                string SitaMessageHeader = string.Empty, SFTPHeaderSITAddress = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, MsgVer = "4";
    
                DataSet dsData = new DataSet();
                DataSet ds = new DataSet();
                DataSet dsFlt = new DataSet();
    
                string[] awbArrray = AWBnumbers.Split(',');
                for (int i = 0; i < awbArrray.Length; i++)
                {
                    FWBMessageProcessor fwbMessageProcessor = new FWBMessageProcessor();
                    DataSet dsfwb = fwbMessageProcessor.GetAWBRecordForGenerateFWBMessage(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString());
                    string awbDestination = dsfwb.Tables[0].Rows[0]["DestinationCode"].ToString();
                    DataSet dsconfiguration = _genericFunction.GetSitaAddressandMessageVersion(PartnerCode, "FHL", "AIR", "", awbDestination, FlightNo, string.Empty);
                    if (dsconfiguration != null && dsconfiguration.Tables[0].Rows.Count > 0)
                    {
                        strEmailid = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
    
                        MsgVer = dsconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                        strSITAHeaderType = dsconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString();
    
                        if (dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                        {
                            SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                        }
                        if (dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                        {
                            SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                        }
                    }
    
                    string carrierCode = string.Empty;
                    string FHLMsg = string.Empty;
                    //string FHLMsg =await EncodeFHLForSend(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString(), ref error, MsgVer);
                    (FHLMsg, error) = await EncodeFHLForSend(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString(), error, MsgVer);
                    carrierCode = FlightNo.Trim() == "" || FlightNo.Trim().Length < 2 ? "" : FlightNo.Trim().Substring(0, 2);
                    try
                    {
                        if (FHLMsg.Length > 3)
                        {
                            string[] msg = FHLMsg.Split('~');
                            for (int j = 0; j < msg.Length; j++)
                            {
                                string fhlMessage = msg[j].Trim();
                                _genericFunction.SaveMessageToOutbox(string.Empty, fhlMessage, strEmailid, SitaMessageHeader, SFTPHeaderSITAddress, "FHL", awbArrray[i].Trim().ToString()
                                           , FlightNo, FlightDate.ToString(), DepartureAirport, FlightDestination, carrierCode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        /// <summary>
        /// Method to generate FHL messages for All AWB from the flight
        /// Method added by prashant
        /// </summary>
        public async Task GenerateFHL(string PartnerCode, string DepartureAirport, string ArrivalAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate, string AWBnumbers)
        {
            try
            {
                string FlightDestination = string.Empty;

                //GenericFunction genericFunction = new GenericFunction();

                string SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, MsgVer = "4", WEBAPIAddress = string.Empty, WebAPIURL = string.Empty;

                DataSet dsData = new DataSet();
                DataSet ds = new DataSet();
                DataSet dsFlt = new DataSet();

                string[] awbArrray = AWBnumbers.Split(',');
                for (int i = 0; i < awbArrray.Length; i++)
                {
                    FWBMessageProcessor fwbMessageProcessor = new FWBMessageProcessor();
                    DataSet dsfwb = fwbMessageProcessor.GetAWBRecordForGenerateFWBMessage(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString());

                    string awbDestination = dsfwb.Tables[0].Rows[0]["DestinationCode"].ToString();
                    awbDestination = awbDestination + "," + ArrivalAirport.Trim();
                    string awbOrigin = dsfwb.Tables[0].Rows[0]["OriginCode"].ToString();
                    string SFTPHeaderSITAddress = string.Empty;

                    DataSet dsconfiguration = _genericFunction.GetSitaAddressandMessageVersionForAutoMessage(PartnerCode, "FHL", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty, string.Empty, string.Empty);
                    if (dsconfiguration != null && dsconfiguration.Tables[0].Rows.Count > 0)
                    {
                        strEmailid = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();

                        MsgVer = dsconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                        strSITAHeaderType = dsconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString();
                        WebAPIURL = dsconfiguration.Tables[0].Rows[0]["WebAPIURL"].ToString();
                        if (dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                        {
                            SitaMessageHeader = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                        }
                        if (dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                        {
                            SFTPHeaderSITAddress = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                        }
                        if (WebAPIURL.Length > 0)
                        {
                            WEBAPIAddress = _genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), dsconfiguration.Tables[0].Rows[0]["WEBAPIHeaderType"].ToString());
                        }
                    }

                    //string FHLMsg = await EncodeFHLForSend(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString(), ref error, MsgVer);
                    string FHLMsg = string.Empty;
                    (FHLMsg, error) = await EncodeFHLForSend(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString(), error, MsgVer);

                    try
                    {
                        if (FHLMsg.Length > 3)
                        {
                            string[] msg = FHLMsg.Split('~');
                            for (int j = 0; j < msg.Length; j++)
                            {
                                string fhlMessage = msg[j].Trim();

                                if (SitaMessageHeader.Trim().Length > 0)
                                    _genericFunction.SaveMessageOutBox("SITA:FHL", SitaMessageHeader.ToString() + "\r\n" + fhlMessage, "", "SITAFTP", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FHL");

                                if (SFTPHeaderSITAddress.Trim().Length > 0)
                                    _genericFunction.SaveMessageOutBox("SITA:FHL", SFTPHeaderSITAddress.ToString() + "\r\n" + fhlMessage, "", "SFTP", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FHL");

                                if (strEmailid.Trim().Length > 0)
                                    _genericFunction.SaveMessageOutBox("FHL", fhlMessage, "", strEmailid, DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FHL");

                                if (WEBAPIAddress.Trim().Length > 0)
                                    _genericFunction.SaveMessageOutBox("FHL", WEBAPIAddress.ToString() + "\r\n" + fhlMessage, "", "WEBAPI", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FHL");
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }        
        }
    }
}
