#region FWB Message Processor Class Description
/* FWBMessage Processor Class Description.
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
    public class FWBMessageProcessor
    {
        //#region :: Constructor ::
        //public FWBMessageProcessor()
        //{

        //}
        //#endregion Constructor

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ISqlDataHelperDao _readOnlyDao;
        private readonly ILogger<FWBMessageProcessor> _logger;
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<FWBMessageProcessor> _staticLogger => _loggerFactory?.CreateLogger<FWBMessageProcessor>();

        private readonly FNAMessageProcessor _fNAMessageProcessor;
        private readonly FFRMessageProcessor _fFRMessageProcessor;


        #region Constructor
        public FWBMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FWBMessageProcessor> logger,
            FNAMessageProcessor fNAMessageProcessor,
            FFRMessageProcessor fFRMessageProcessor,
            ILoggerFactory loggerFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _loggerFactory = loggerFactory;
            _fNAMessageProcessor = fNAMessageProcessor;
            _fFRMessageProcessor = fFRMessageProcessor;
            _readOnlyDao = sqlDataHelperFactory.Create(readOnly: false);
        }
        #endregion

        #region :: Public Methods ::
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fwbmsg"></param>
        /// <param name="fwbdata"></param>
        /// <param name="fltroute"></param>
        /// <param name="fwbOtherCharge"></param>
        /// <param name="othinfoarray"></param>
        /// <param name="fwbrate"></param>
        /// <param name="custominfo"></param>
        /// <param name="objDimension"></param>
        /// <param name="objAwbBup"></param>
        /// <returns></returns>
        public bool DecodeReceiveFWBMessage(string fwbmsg, ref MessageData.fwbinfo fwbdata, ref MessageData.FltRoute[] fltroute, ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.RateDescription[] fwbrate, ref MessageData.customsextrainfo[] custominfo, ref MessageData.dimensionnfo[] objDimension, ref MessageData.AWBBuildBUP[] objAwbBup, int refNO, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool flag = false;
            MessageData.AWBBuildBUP awbBup = new MessageData.AWBBuildBUP("");
            MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
            try
            {
                string strFightNo = string.Empty, harmonizedcode = string.Empty, awbNumber = string.Empty;
                string lastrec = "NA";
                string innerrec = "NA";
                int line = 0;
                try
                {
                    if (fwbmsg.StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
                    {

                        string[] str = fwbmsg.Split('$');
                        if (str.Length >= 3)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {

                                flag = true;

                                #region Line 1
                                if (str[i].StartsWith("FWB", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    fwbdata.fwbversionnum = msg[1];
                                }
                                #endregion

                                #region Line 2 awb consigment details
                                if (i == 1)
                                {
                                    try
                                    {
                                        lastrec = "AWB";
                                        line = 0;
                                        string[] msg = str[i].Split('/');
                                        string[] decmes = msg[0].Split('-');
                                        fwbdata.airlineprefix = decmes[0];
                                        fwbdata.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                                        awbNumber = fwbdata.awbnum;
                                        fwbdata.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                                        fwbdata.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
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
                                            fwbdata.consigntype = strarr[0];
                                            fwbdata.pcscnt = strarr[1];//int.Parse(strarr[1]);
                                            fwbdata.weightcode = strarr[2];
                                            fwbdata.weight = strarr[3];//float.Parse(strarr[3]);
                                            for (k = 4; k < strarr.Length; k += 2)
                                            {
                                                if (strarr[k] != null)
                                                {
                                                    if (strarr[k] == "DG")
                                                    {
                                                        fwbdata.densityindicator = strarr[k];
                                                        fwbdata.densitygrp = strarr[k];
                                                    }
                                                    else//if (strarr[k + 1].Length > 3)
                                                    {
                                                        fwbdata.volumecode = strarr[k];
                                                        fwbdata.volumeamt = strarr[k + 1];
                                                    }
                                                }
                                            }
                                        }


                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }
                                #endregion

                                #region Line 3 Flight Booking
                                //Added By :Badiuz khan
                                //Added On:2015-12-17
                                //Description: Correct FLT Tag of  FWB For Saving Record
                                if (str[i].StartsWith("FLT", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            for (int k = 0; k < msg.Length; k++)
                                            {
                                                if (msg[k].ToUpper() != "FLT")
                                                {
                                                    if (msg[k].All(char.IsNumber) && k % 2 == 0)
                                                    {
                                                        if (fwbdata.fltday == "")
                                                            fwbdata.fltday = msg[k];
                                                        else
                                                            fwbdata.fltday += "," + msg[k];
                                                    }
                                                    else
                                                    {
                                                        if (fwbdata.fltnum == "")
                                                            fwbdata.fltnum = msg[k];
                                                        else
                                                            fwbdata.fltnum += "," + msg[k];
                                                    }
                                                }
                                            }
                                            //    fwbdata.carriercode = msg[1].Substring(0, 2);
                                            //fwbdata.fltnum = msg[1].Substring(2);
                                            //strFightNo = msg[1].Substring(2);
                                            //fwbdata.fltday = msg[2];
                                            //if (msg.Length > 2)
                                            //{
                                            //    fwbdata.carriercode = fwbdata.carriercode + "," + msg[3].Substring(0, 2);
                                            //    fwbdata.fltnum = fwbdata.fltnum + "," + msg[3].Substring(2);
                                            //    fwbdata.fltday = fwbdata.fltday + "," + msg[4];
                                            //}
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 4 Routing
                                //Added By :Badiuz khan
                                //Added On:2015-10-14
                                //Description: Add Rouute Tag of FWB For Saving Record
                                if (str[i].StartsWith("RTG", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {

                                        string[] msg = str[i].Split('/');
                                        MessageData.FltRoute flight = new MessageData.FltRoute("");
                                        if (msg.Length >= 2)
                                        {
                                            //if (msg.Length == 3 && msg[1].Length == 2 && msg[2].Length == 3)
                                            //{
                                            //    errorMessage = "Invalid RTG line";
                                            //    return false;
                                            //}
                                            bool isRouteValid = false;
                                            for (int k = 1; k < msg.Length; k++)
                                            {
                                                string[] strFight = fwbdata.fltnum.Split(',');
                                                //if (strFight.Length == 2 && msg[1].Length == 2)
                                                //{
                                                //    errorMessage = "Invalid RTG line";
                                                //    return false;
                                                //}
                                                if (fwbdata.fltnum != "" && k <= strFight.Length)
                                                {
                                                    if (fwbdata.fltnum.Contains(','))
                                                    {
                                                        if (k == 1)
                                                        {
                                                            flight.carriercode = strFight[0].Substring(0, 2);
                                                            flight.fltnum = strFight[0].Substring(2);
                                                        }
                                                        else
                                                        {
                                                            if (k > strFight.Length)
                                                            {
                                                                flight.carriercode = "";
                                                                flight.fltnum = "";
                                                            }
                                                            else
                                                            {
                                                                flight.carriercode = strFight[k - 1].Substring(0, 2);
                                                                flight.fltnum = strFight[k - 1].Substring(2);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (msg.Length > 2 && k > 1)
                                                        {
                                                            if (msg[k].Trim().Length == 5)
                                                                flight.carriercode = msg[k].Trim().Substring(3, 2);
                                                            else
                                                                flight.carriercode = "";
                                                            flight.fltnum = "";
                                                        }
                                                        else
                                                        {
                                                            flight.carriercode = fwbdata.fltnum.Substring(0, 2);
                                                            flight.fltnum = fwbdata.fltnum.Substring(2);
                                                        }
                                                    }
                                                }

                                                else
                                                {
                                                    if (msg[k].Length == 2)
                                                    {
                                                        flight.carriercode = msg[k];
                                                    }
                                                    else
                                                    {
                                                        flight.carriercode = msg[k].Substring(3);
                                                        flight.fltnum = "";
                                                    }
                                                }
                                                if (fwbdata.fltday != "")
                                                {
                                                    if (fwbdata.fltday.Contains(',') && k <= strFight.Length)
                                                    {
                                                        string[] strFightDay = fwbdata.fltday.Split(',');
                                                        if (k == 1)
                                                        {
                                                            flight.date = strFightDay[0];
                                                            flight.month = DateTime.Now.Month.ToString();
                                                        }
                                                        else
                                                        {
                                                            if (k > strFightDay.Length)
                                                            {
                                                                flight.date = strFightDay[0];
                                                                flight.month = DateTime.Now.Month.ToString();
                                                            }
                                                            else
                                                            {
                                                                flight.date = strFightDay[k - 1];
                                                                flight.month = DateTime.Now.Month.ToString();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (msg.Length > 2 && k > 1)
                                                        {
                                                            flight.date = "";
                                                            flight.month = "";
                                                        }
                                                        else
                                                        {
                                                            flight.date = fwbdata.fltday;
                                                            flight.month = DateTime.Now.Month.ToString();
                                                        }
                                                    }

                                                }


                                                if (k == 1)
                                                    flight.fltdept = fwbdata.origin;
                                                else
                                                    flight.fltdept = msg[k - 1].Substring(0, 3);

                                                if (msg[k].Length == 5)
                                                {
                                                    flight.fltarrival = msg[k].Substring(0, 3);
                                                }
                                                else if (msg[k].Length > 2)
                                                {
                                                    flight.fltarrival = msg[k].Substring(0, 3);
                                                }
                                                else
                                                {
                                                    flight.fltarrival = "";
                                                }


                                                if (isRouteValid && flight.fltarrival != fwbdata.dest)
                                                {
                                                    errorMessage = "Route origin and destination mismatch with AWB";
                                                    GenericFunction genericFunction = new GenericFunction();
                                                    genericFunction.UpdateErrorMessageToInbox(refNO, errorMessage, "FWB", false, "", false);
                                                    break;
                                                }
                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                fltroute[fltroute.Length - 1] = flight;

                                                if (flight.fltarrival == fwbdata.dest && !isRouteValid)
                                                {
                                                    isRouteValid = true;
                                                }

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
                                            fwbdata.shipperaccnum = msg[1];

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
                                            fwbdata.consaccnum = msg[1];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 7 Agent
                                if (str[i].StartsWith("AGT", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg.Length > 1)
                                        {
                                            fwbdata.agentaccnum = msg[1];
                                            fwbdata.agentIATAnumber = msg[2].Length > 0 ? msg[2] : "";
                                            if (msg.Length > 2)
                                            {
                                                fwbdata.agentCASSaddress = msg[3].Length > 0 ? msg[3] : "";
                                            }
                                            if (msg.Length > 3)
                                            {
                                                fwbdata.agentParticipentIdentifier = msg[4].Length > 0 ? msg[4] : "";
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 8 Special Service request
                                if (str[i].StartsWith("SSR", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg[1].Length > 0)
                                        {
                                            fwbdata.specialservicereq1 = msg[1];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 9 Notify
                                if (str[i].StartsWith("NFY", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg.Length > 0)
                                        {
                                            fwbdata.notifyname = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 10 Accounting Information
                                if (str[i].StartsWith("ACC", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    line = 0;
                                    if (msg.Length > 1)
                                    {
                                        fwbdata.accountinginfoidentifier = fwbdata.accountinginfoidentifier + msg[1] + ",";
                                        fwbdata.accountinginfo = fwbdata.accountinginfo + msg[2] + ",";
                                    }
                                }
                                #endregion

                                #region Line 11 Charge declaration
                                if (str[i].StartsWith("CVD", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');

                                        lastrec = msg[0];
                                        if (msg.Length > 1)
                                        {
                                            fwbdata.currency = msg[1];
                                            fwbdata.chargecode = msg[2].Length > 0 ? msg[2] : "";
                                            fwbdata.chargedec = msg[3].Length > 0 ? msg[3] : "";

                                            string msgNVD = msg[4].Replace('.', '0');
                                            fwbdata.insuranceamount = msg[6];
                                            if (!string.IsNullOrEmpty(msg[4]) && msgNVD.All(char.IsDigit) || msg[4] == "NVD")
                                            {
                                                fwbdata.declaredvalue = msg[4];
                                            }
                                            else
                                            {
                                                errorMessage = "Invalid message format issue";
                                                return false;
                                            }
                                            string msgNCV = msg[5].Replace('.', '0');
                                            if (!string.IsNullOrEmpty(msg[5]) && msgNCV.All(char.IsDigit) || msg[5] == "NCV")
                                            {
                                                fwbdata.declaredcustomvalue = msg[5];
                                            }
                                            else
                                            {
                                                errorMessage = "Invalid message format issue";
                                                return false;
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 12 Rate Description
                                if (str[i].StartsWith("RTD", StringComparison.OrdinalIgnoreCase))
                                {

                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        lastrec = msg[0];
                                        line = 0;
                                        MessageData.RateDescription rate = new MessageData.RateDescription("");

                                        try
                                        {
                                            rate.linenum = msg[1];
                                            for (int k = 2; k < msg.Length; k++)
                                            {
                                                if (msg[k].Substring(0, 1).Equals("P", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.pcsidentifier = msg[k].Substring(0, 1);
                                                    rate.numofpcs = msg[k].Substring(1);
                                                }
                                                if (msg[k].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.weightindicator = msg[k].Substring(0, 1);
                                                    rate.weight = msg[k].Substring(1).Length > 0 ? msg[k].Substring(1) : "0";
                                                }
                                                if (msg[k].Substring(0, 1).Equals("C", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.rateclasscode = msg[k].Substring(1);
                                                }
                                                if (msg[k].Substring(0, 1).Equals("S", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.commoditynumber = msg[k].Substring(1);
                                                }
                                                if (msg[k].Substring(0, 1).Equals("W", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.awbweight = msg[k].Substring(1);
                                                }
                                                if (msg[k].Substring(0, 1).Equals("R", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.chargerate = msg[k].Substring(1);
                                                }
                                                if (msg[k].Substring(0, 1).Equals("T", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    rate.chargeamt = msg[k].Substring(1);
                                                }
                                            }
                                            Array.Resize(ref fwbrate, fwbrate.Length + 1);
                                            fwbrate[fwbrate.Length - 1] = rate;
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex.Message);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                    }
                                }
                                #endregion

                                #region Line 13 Other Charges
                                if (str[i].StartsWith("OTH", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "OTH";
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            string[] opstr = StringSplitter(msg[2]);
                                            for (int k = 0; k < opstr.Length; k = k + 2)
                                            {
                                                if (opstr[k].Length > 0)
                                                {
                                                    MessageData.othercharges oth = new MessageData.othercharges("");
                                                    oth.otherchargecode = opstr[k].Substring(0, 2);
                                                    oth.entitlementcode = opstr[k].Substring(2);
                                                    oth.chargeamt = opstr[k + 1];
                                                    Array.Resize(ref fwbOtherCharge, fwbOtherCharge.Length + 1);
                                                    fwbOtherCharge[fwbOtherCharge.Length - 1] = oth;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 14 Prepaid Charge Summery
                                if (str[i].StartsWith("PPD", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "PPD";
                                        string[] msg = str[i].Split('/');
                                        for (int k = 1; k < msg.Length; k++)
                                        {
                                            if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.PPweightCharge = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.PPValuationCharge = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.PPTaxesCharge = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.PPOCDA = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.PPOCDC = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.PPTotalCharges = msg[k].Substring(2);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 15 Collect Charge Summery
                                if (str[i].StartsWith("COL", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        lastrec = "COL";
                                        string[] msg = str[i].Split('/');
                                        for (int k = 1; k < msg.Length; k++)
                                        {
                                            if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.CCweightCharge = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.CCValuationCharge = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.CCTaxesCharge = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.CCOCDA = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.CCOCDC = msg[k].Substring(2);
                                            }
                                            if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
                                            {
                                                fwbdata.CCTotalCharges = msg[k].Substring(2);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 16 Shipper Certification
                                if (str[i].StartsWith("CER", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg[1].Length > 0)
                                    {
                                        fwbdata.shippersignature = msg[1];
                                    }
                                }
                                #endregion

                                #region Line 17 Carrier Execution
                                if (str[i].StartsWith("ISU", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        if (msg.Length > 0)
                                        {

                                            fwbdata.carrierdate = msg.Length > 1 ? msg[1].Length > 1 ? msg[1].Substring(0, 2) : string.Empty : string.Empty;
                                            fwbdata.carriermonth = msg.Length > 1 ? msg[1].Length > 1 ? msg[1].Substring(2, 3) : string.Empty : string.Empty;

                                            bool isUpper = fwbdata.carriermonth.All(char.IsUpper);

                                            if (!isUpper || msg[1].Length != 7)
                                            {
                                                errorMessage = "AWB:" + awbNumber + " not created(or updatted), incorrect ISU format";
                                                return false;
                                            }
                                            fwbdata.carrieryear = msg.Length > 1 ? msg[1].Length > 1 ? msg[1].Substring(5) : string.Empty : string.Empty;
                                            fwbdata.carrierplace = msg.Length > 2 ? msg[2] : string.Empty;
                                            fwbdata.carriersignature = msg.Length > 3 ? msg[3] : string.Empty;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 18 Other service info
                                if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        line = 0;
                                        if (msg[1].Length > 0)
                                        {
                                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];

                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 19 Charge in destination currency
                                if (str[i].StartsWith("CDC", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            fwbdata.cccurrencycode = msg[1].Substring(0, 3);
                                            fwbdata.ccexchangerate = msg[1].Substring(3);
                                            for (int j = 2; j < msg.Length; j++)
                                                fwbdata.ccchargeamt += msg[j] + ",";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 20 Sender Reference
                                if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {

                                            if (msg[1].Length > 1)
                                            {
                                                try
                                                {
                                                    fwbdata.senderairport = msg[1].Substring(0, 3);
                                                    fwbdata.senderofficedesignator = msg[1].Substring(3, 2);
                                                    fwbdata.sendercompanydesignator = msg[1].Substring(5);
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex.Message);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                            }
                                            fwbdata.senderFileref = msg[2];
                                            fwbdata.senderParticipentIdentifier = msg[3];
                                            fwbdata.senderParticipentCode = msg[4];
                                            fwbdata.senderPariticipentAirport = msg[5];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 21 Custom Origin
                                if (str[i].StartsWith("COR", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg[1].Length > 0)
                                        {
                                            fwbdata.customorigincode = msg[1];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 22 Commission Information
                                if (str[i].StartsWith("COI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            fwbdata.commisioncassindicator = msg[1];
                                            for (int k = 2; k < msg.Length; k++)
                                                fwbdata.commisionCassSettleAmt += msg[k] + ",";
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 23 Sales Incentive Info
                                if (str[i].StartsWith("SII", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg[1].Length > 0)
                                        {
                                            fwbdata.saleschargeamt = msg[1];
                                            fwbdata.salescassindicator = msg[2];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 24 Agent Reference
                                if (str[i].StartsWith("ARD", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg[1].Length > 0)
                                        {
                                            fwbdata.agentfileref = msg[1];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 25 Special Handling
                                if (str[i].StartsWith("SPH", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg[1].Length > 0)
                                        {
                                            string temp = str[i].Replace("/", ",");
                                            fwbdata.splhandling = temp.Replace("SPH", "");
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 26 Nominated Handling Party
                                if (str[i].StartsWith("NOM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            fwbdata.handlingname = msg[1];
                                            fwbdata.handlingplace = msg[2];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message); 
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 27 Shipment Reference Info
                                if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            fwbdata.shiprefnum = msg[1];
                                            fwbdata.supplemetryshipperinfo1 = msg[2];
                                            fwbdata.supplemetryshipperinfo2 = msg[3];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 28 Other Service Information
                                if (str[i].StartsWith("OPI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            lastrec = msg[0];
                                            fwbdata.othparticipentname = msg[1];
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 29 custom extra info
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
                                        custom.OCIInfo = str[i].Replace("OCI", "");
                                        Array.Resize(ref custominfo, custominfo.Length + 1);
                                        custominfo[custominfo.Length - 1] = custom;
                                    }
                                }
                                #endregion
                                #region Second Line Version 17

                                if (fwbdata.fwbversionnum == "17")
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');

                                        #region SHP
                                        if (lastrec == "SHP" && (!str[i].StartsWith("SHP")))
                                        {

                                            if (str[i].StartsWith("NAM", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "NAM";
                                                fwbdata.shippername = msg[1].Length > 0 ? msg[1] : "";
                                            }

                                            if (innerrec == "NAM" && msg[0] == "")
                                            {

                                                //fwbdata.shippername = fwbdata.shippername + " " + msg[1].ToString();
                                                fwbdata.shippername2 = msg[1].ToString();
                                            }


                                            if (str[i].StartsWith("ADR", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "ADR";
                                                fwbdata.shipperadd = msg[1].Length > 0 ? msg[1] : "";
                                            }


                                            if (innerrec == "ADR" && msg[0] == "")
                                            {

                                                //fwbdata.shipperadd = fwbdata.shipperadd + " " + msg[1].ToString();
                                                fwbdata.shipperadd2 = msg[1].ToString();
                                            }


                                            if (str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "LOC";
                                                if (msg.Length > 2)
                                                {
                                                    fwbdata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    fwbdata.shipperstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                }
                                                else
                                                    fwbdata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";

                                            }
                                            if (line == 3 && (!str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                int len = msg.Length, p;

                                                for (p = 0; p < len; p++)
                                                {
                                                    if (p == 1)
                                                        fwbdata.shippercountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    if (p == 2)
                                                        fwbdata.shipperpostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                    if (p == 3)
                                                        fwbdata.shippercontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
                                                    if (p == 4)
                                                        fwbdata.shippercontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";
                                                }

                                            }


                                        }

                                        #endregion
                                        #region CNE
                                        if (lastrec == "CNE" && (!str[i].StartsWith("CNE")))
                                        {

                                            if (str[i].StartsWith("NAM", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "NAM";
                                                fwbdata.consname = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (innerrec == "NAM" && msg[0] == "")
                                            {

                                                //fwbdata.consname = fwbdata.consname + " " + msg[1].ToString();
                                                fwbdata.consname2 = msg[1].ToString();
                                            }

                                            if (str[i].StartsWith("ADR", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "ADR";
                                                fwbdata.consadd = msg[1].Length > 0 ? msg[1] : "";
                                            }

                                            if (innerrec == "ADR" && msg[0] == "")
                                            {

                                                //fwbdata.consadd = fwbdata.consname + " " + msg[1].ToString();
                                                fwbdata.consadd2 = msg[1].ToString();
                                            }


                                            if (str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase))
                                            {
                                                line++;
                                                innerrec = "LOC";
                                                if (msg.Length > 2)
                                                {
                                                    fwbdata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    fwbdata.consstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                }
                                                else
                                                    fwbdata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";

                                            }
                                            if (line == 3 && (!str[i].StartsWith("LOC", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                int p, len = msg.Length;
                                                for (p = 0; p < len; p++)
                                                {
                                                    if (p == 1)
                                                        fwbdata.conscountrycode = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    if (p == 2)
                                                        fwbdata.conspostcode = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                    if (p == 3)
                                                        fwbdata.conscontactidentifier = msg[3].Length > 0 || msg[3] == null ? msg[3] : "";
                                                    if (p == 4)
                                                        fwbdata.conscontactnum = msg[4].Length > 0 || msg[4] == null ? msg[4] : "";

                                                }

                                                //fwbdata.conscountrycode = msg[1].Length > 0 ||msg[1]==null? msg[1] : "";
                                                //fwbdata.conspostcode = msg[2].Length > 0 ||msg[2]==null? msg[2] : "";
                                                //fwbdata.conscontactidentifier = msg[3].Length > 0 ||msg[3]==null? msg[3] : "";
                                                //fwbdata.conscontactnum = msg[4].Length > 0 ||msg[4]==null? msg[4] : "";
                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Second Line
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        #region SHP Data version 16
                                        if (lastrec == "SHP" && fwbdata.fwbversionnum != "17")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                fwbdata.shippername = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                fwbdata.shipperadd = msg[1].Length > 0 ? msg[1] : "";

                                            }
                                            if (line == 3)
                                            {
                                                if (msg.Length > 2)
                                                {
                                                    fwbdata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    fwbdata.shipperstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                }
                                                else
                                                    fwbdata.shipperplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                            }
                                            if (line == 4)
                                            {
                                                int len = msg.Length, p;

                                                for (p = 1; p < len; p++)
                                                {
                                                    if (p == 1)
                                                        fwbdata.shippercountrycode = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                    if (p == 2)
                                                        fwbdata.shipperpostcode = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                    if (p >= 3 && p % 2 == 1)
                                                        fwbdata.shippercontactidentifier = fwbdata.shippercontactidentifier + "," + (msg[p].Length > 0 || msg[p] == null ? msg[p] : "");
                                                    if (p >= 4 && p % 2 == 0)
                                                    {
                                                        if (msg[p - 1] == "TE")
                                                            fwbdata.shippercontactnum = (msg[p].Length > 0 || msg[p] == null ? msg[p] : "");
                                                        if (msg[p - 1] == "FX")
                                                            fwbdata.shipperfaxnum = (msg[p].Length > 0 || msg[p] == null ? msg[p] : "");
                                                        if (msg[p - 1] == "TL")
                                                            fwbdata.shippertelexnum = (msg[p].Length > 0 || msg[p] == null ? msg[p] : "");
                                                    }
                                                }
                                                fwbdata.shippercontactidentifier = fwbdata.shippercontactidentifier.Trim(',');
                                            }
                                        }
                                        #endregion

                                        #region CNE Data version 16
                                        if (lastrec == "CNE" && fwbdata.fwbversionnum != "17")
                                        {

                                            line++;
                                            if (line == 1)
                                            {
                                                fwbdata.consname = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                fwbdata.consadd = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 3)
                                            {
                                                if (msg.Length > 2)
                                                {
                                                    fwbdata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                                    fwbdata.consstate = msg[2].Length > 0 || msg[2] == null ? msg[2] : "";
                                                }
                                                else
                                                    fwbdata.consplace = msg[1].Length > 0 || msg[1] == null ? msg[1] : "";
                                            }
                                            if (line == 4)
                                            {
                                                int p, len = msg.Length;
                                                for (p = 1; p < len; p++)
                                                {
                                                    if (p == 1)
                                                        fwbdata.conscountrycode = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                    if (p == 2)
                                                        fwbdata.conspostcode = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                    if (p >= 3 && p % 2 == 1)
                                                        fwbdata.conscontactidentifier = fwbdata.conscontactidentifier + "," + (msg[p].Length > 0 || msg[p] == null ? msg[p] : "");
                                                    if (p >= 4 && p % 2 == 0)
                                                    {
                                                        if (msg[p - 1] == "TE")
                                                            fwbdata.conscontactnum = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                        if (msg[p - 1] == "FX")
                                                            fwbdata.consfaxnum = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                        if (msg[p - 1] == "TL")
                                                            fwbdata.constelexnum = msg[p].Length > 0 || msg[p] == null ? msg[p] : "";
                                                    }

                                                }
                                                fwbdata.conscontactidentifier = fwbdata.conscontactidentifier.Trim(',');
                                            }
                                        }
                                        #endregion

                                        #region AgentData
                                        if (lastrec == "AGT")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                fwbdata.agentname = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                fwbdata.agentplace = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                        }
                                        #endregion

                                        #region SSR 2
                                        if (lastrec == "SSR")
                                        {
                                            fwbdata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region Also notify Data
                                        if (lastrec == "NFY")
                                        {
                                            line++;
                                            if (line == 1)
                                            {
                                                fwbdata.notifyadd = msg[1].Length > 0 ? msg[1] : "";
                                            }
                                            if (line == 2)
                                            {
                                                fwbdata.notifyplace = msg[1].Length > 0 ? msg[1] : "";
                                                fwbdata.notifystate = msg[2].Length > 0 ? msg[2] : "";
                                            }
                                            if (line == 3)
                                            {
                                                fwbdata.notifycountrycode = msg[1].Length > 0 ? msg[1] : "";
                                                fwbdata.notifypostcode = msg[2].Length > 0 ? msg[2] : "";
                                                fwbdata.notifycontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                                fwbdata.notifycontactnum = msg[4].Length > 0 ? msg[4] : "";
                                            }
                                        }
                                        #endregion

                                        #region Account Info
                                        if (lastrec.Equals("ACC", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (msg.Length > 1)
                                            {
                                                fwbdata.accountinginfoidentifier = fwbdata.accountinginfoidentifier + msg[1] + ",";
                                                fwbdata.accountinginfo = fwbdata.accountinginfo + msg[2] + ",";
                                            }
                                        }
                                        #endregion

                                        #region RateData
                                        if (lastrec.Equals("RTD", StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {

                                                if (msg.Length > 1)
                                                {
                                                    int res, k = 1;
                                                    if (int.TryParse(msg[k].ToString(), out res))
                                                    {
                                                        k++;
                                                    }
                                                    if (msg[k].Equals("NG", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        if (fwbrate[0].goodsnature == "")
                                                        {
                                                            fwbrate[fwbrate.Length - 1].goodsnature = msg[k + 1];

                                                            if (fwbrate[fwbrate.Length - 1].ngnc == "" && msg[k + 1].Length > 0)
                                                            {
                                                                fwbrate[fwbrate.Length - 1].ngnc = "NG";
                                                            }
                                                        }
                                                        fwbrate[fwbrate.Length - 1].ProductType = msg.Length > k + 2 ? msg[k + 2] : "";
                                                    }
                                                    if (msg[k].Equals("NC", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        if (fwbrate[fwbrate.Length - 1].goodsnature1 == "" && fwbrate[fwbrate.Length - 1].ngnc == "" && msg[k + 1].Length > 0)
                                                        {
                                                            fwbrate[fwbrate.Length - 1].ngnc = "NC";
                                                        }

                                                        fwbrate[fwbrate.Length - 1].goodsnature1 = fwbrate[fwbrate.Length - 1].goodsnature1 == "" ? msg[k + 1] : fwbrate[fwbrate.Length - 1].goodsnature1 + msg[k + 1];

                                                        if (fwbrate[fwbrate.Length - 1].ProductType == "")
                                                        {
                                                            fwbrate[fwbrate.Length - 1].ProductType = msg.Length > k + 2 ? msg[k + 2] : "";
                                                        }
                                                    }
                                                    if (msg[k].Equals("ND", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        string[] msg1 = str[i].Split('/');
                                                        dimension = new MessageData.dimensionnfo("");
                                                        if (msg.Length >= 6)
                                                        {
                                                            if (msg[k + 1].Length > 0)
                                                            {
                                                                if (msg[k + 1].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                    dimension.weightcode = msg[k + 1].Substring(0, 1);
                                                                    dimension.weight = msg[k + 1].Substring(1);
                                                                    k++;
                                                                }
                                                                if (msg[k + 1].Substring(0, 1).Equals("L", StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                    dimension.weightcode = msg[k + 1].Substring(0, 1);
                                                                    dimension.weight = msg[k + 1].Substring(1);
                                                                    k++;
                                                                }
                                                            }
                                                            if (msg.Length >= 4)
                                                                dimension.mesurunitcode = msg[4].ToString().Substring(0, 3);

                                                            if (msg.Length >= 4)
                                                            {
                                                                string[] strDimTag = msg[4].ToString().Split('-');
                                                                //Commented by priyanka , it takes only 2 digits.
                                                                //dimension.length = strDimTag[0].ToString().Substring(strDimTag[0].Length - 3);
                                                                dimension.length = strDimTag[0].ToString().Substring(3);//modified
                                                                dimension.width = strDimTag[1];
                                                                dimension.height = strDimTag[2];

                                                            }
                                                            if (msg.Length >= 5)
                                                            {
                                                                dimension.piecenum = msg[5].ToString();
                                                                dimension.PieceType = "Bulk";
                                                            }
                                                            Array.Resize(ref objDimension, objDimension.Length + 1);
                                                            objDimension[objDimension.Length - 1] = dimension;

                                                        }
                                                        else if (msg.Length >= 5)
                                                        {
                                                            if (msg[k + 1].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                dimension.weightcode = msg[k + 1].Substring(0, 1);
                                                                dimension.weight = msg[k + 1].Substring(1);
                                                                k++;
                                                            }
                                                            if (msg[k + 1].Substring(0, 1).Equals("L", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                dimension.weightcode = msg[k + 1].Substring(0, 1);
                                                                dimension.weight = msg[k + 1].Substring(1);
                                                                k++;
                                                            }

                                                            if (msg[k].Length >= 3)
                                                                dimension.mesurunitcode = msg[3].ToString().Substring(0, 3);

                                                            if (msg[k].Length >= 3)
                                                            {
                                                                //Modified by priyanka .. it was accessing only two digits of dimensions of length.

                                                                //dimension.length = strDimTag[0].ToString().Substring(strDimTag[0].Length - 2);
                                                                string[] strDimTag = msg[3].ToString().Split('-');
                                                                dimension.length = strDimTag[0].ToString().Substring(3);
                                                                dimension.width = strDimTag[1];
                                                                dimension.height = strDimTag[2];

                                                            }
                                                            if (msg[k].Length >= 3)
                                                            {
                                                                dimension.piecenum = msg[4].ToString();
                                                                dimension.PieceType = "Bulk";
                                                            }
                                                            Array.Resize(ref objDimension, objDimension.Length + 1);
                                                            objDimension[objDimension.Length - 1] = dimension;

                                                        }
                                                    }
                                                    if ((!(msg[k].ToUpper().StartsWith("ND")) && msg[k].ToUpper().StartsWith("K", StringComparison.OrdinalIgnoreCase) || msg[k].ToUpper().StartsWith("L", StringComparison.OrdinalIgnoreCase)))
                                                    {
                                                        string[] msg1 = str[i].Split('/');
                                                        dimension = new MessageData.dimensionnfo("");
                                                        if (msg.Length > 1)
                                                        {

                                                            if (msg[k].Length >= 1)
                                                            {
                                                                dimension.weightcode = msg[1].Substring(0, 1);
                                                                dimension.weight = msg[1].Substring(1);
                                                                k++;
                                                            }

                                                            if (msg[k].Length >= 2)
                                                                dimension.mesurunitcode = msg[2].ToString().Substring(0, 3);

                                                            if (msg[k].Length >= 3)
                                                            {
                                                                //modified by priyanka 
                                                                string[] strDimTag = msg[2].ToString().Split('-');
                                                                //dimension.length = strDimTag[0].ToString().Substring(strDimTag[0].Length - 2);
                                                                dimension.length = strDimTag[0].ToString().Substring(3);
                                                                dimension.width = strDimTag[1];
                                                                dimension.height = strDimTag[2];

                                                            }
                                                            if (msg[k].Length >= 3)
                                                            {
                                                                dimension.piecenum = msg[3].ToString();
                                                                dimension.PieceType = "Bulk";
                                                            }

                                                            Array.Resize(ref objDimension, objDimension.Length + 1);
                                                            objDimension[objDimension.Length - 1] = dimension;
                                                        }
                                                    }

                                                    if (msg[k].Equals("NV", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbrate[fwbrate.Length - 1].volcode = msg[k + 1].Substring(0, 2);
                                                        fwbrate[fwbrate.Length - 1].volamt = msg[k + 1].Substring(2);
                                                    }
                                                    if (msg[k].Equals("NU", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        //  awbBup.UldType = msg[k + 1].Substring(0, 3);
                                                        awbBup.ULDNo = msg[k + 1].ToString();

                                                        //awbBup.ULDOwnerCode = msg[k + 1].Substring(msg[k + 1].Length - 2);
                                                    }
                                                    if (msg[k].Equals("NS", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        bool IsSlackCount = false;
                                                        if (msg.Length > k + 1)
                                                        {
                                                            awbBup.SlacCount = msg[k + 1];
                                                        }

                                                        if (Convert.ToInt32(msg[k + 1]) == 1 && awbBup.ULDNo != "")
                                                        {
                                                            IsSlackCount = true;
                                                            awbBup.SlacCount = msg[k + 1];
                                                            Array.Resize(ref objAwbBup, objAwbBup.Length + 1);
                                                            objAwbBup[objAwbBup.Length - 1] = awbBup;

                                                            for (int d = 0; d < objDimension.Length; d++)
                                                            {
                                                                if (Convert.ToInt32(objDimension[d].piecenum) == 1)
                                                                {
                                                                    objDimension[d].PieceType = "ULD";
                                                                    objDimension[d].UldNo = awbBup.ULDNo;
                                                                }
                                                            }
                                                        }
                                                        else if (objDimension.Length > 0)
                                                        {

                                                            for (int d = 0; d < objDimension.Length; d++)
                                                            {
                                                                if (Convert.ToInt32(objDimension[d].piecenum) == Convert.ToInt32(msg[k + 1]))
                                                                {
                                                                    objDimension[d].PieceType = "SLAC";
                                                                    objDimension[d].UldNo = awbBup.ULDNo;
                                                                }

                                                            }
                                                        }
                                                        if (!IsSlackCount)
                                                        {
                                                            Array.Resize(ref objAwbBup, objAwbBup.Length + 1);
                                                            objAwbBup[objAwbBup.Length - 1] = awbBup;
                                                        }
                                                        awbBup = new MessageData.AWBBuildBUP("");

                                                    }

                                                    if (msg[k].Equals("NH", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        //harmonizedcode = harmonizedcode.Length > 0 ? harmonizedcode + "," + msg[k + 1] : msg[k + 1];
                                                        //fwbrate[fwbrate.Length - 1].hermonisedcomoditycode = harmonizedcode;// msg[k + 1];

                                                        string strTempharmonizedcode = msg[k + 1];

                                                        //Regex r = new Regex("^[A-Z0-9]+,"); for Comma seperator
                                                        Regex r = new Regex("^[A-Z0-9]*$");
                                                        if (!r.IsMatch(strTempharmonizedcode))
                                                            strTempharmonizedcode = "";

                                                        if (!(strTempharmonizedcode.Length > 0 && strTempharmonizedcode.Length >= 6 && strTempharmonizedcode.Length <= 18))
                                                            strTempharmonizedcode = "";

                                                        harmonizedcode = harmonizedcode.Length > 0 ? harmonizedcode + "," + strTempharmonizedcode : strTempharmonizedcode;

                                                        if (harmonizedcode.Length > 208)
                                                        {
                                                            int index = harmonizedcode.LastIndexOf(',');
                                                            harmonizedcode = harmonizedcode.Substring(0, index);
                                                        }
                                                        fwbrate[fwbrate.Length - 1].hermonisedcomoditycode = harmonizedcode;
                                                    }
                                                    if (msg[k].Equals("NO", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbrate[fwbrate.Length - 1].isocountrycode = msg[k + 1];
                                                    }

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex.Message);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                        }
                                        #endregion

                                        #region Other Charges
                                        if (lastrec.Equals("OTH", StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {
                                                string[] opstr = StringSplitter(msg[2]);
                                                for (int k = 0; k < opstr.Length; k = k + 2)
                                                {
                                                    MessageData.othercharges oth = new MessageData.othercharges("");
                                                    oth.otherchargecode = opstr[k].Substring(0, 2);
                                                    oth.entitlementcode = opstr[k].Substring(2);
                                                    oth.chargeamt = opstr[k + 1];
                                                    Array.Resize(ref fwbOtherCharge, fwbOtherCharge.Length + 1);
                                                    fwbOtherCharge[fwbOtherCharge.Length - 1] = oth;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex.Message);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                        }
                                        #endregion

                                        #region Line 14 Collect Charge Summery
                                        if (lastrec.Equals("PPD", StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {
                                                for (int k = 1; k < msg.Length; k++)
                                                {
                                                    if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.PPweightCharge = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.PPValuationCharge = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.PPTaxesCharge = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.PPOCDA = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.PPOCDC = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.PPTotalCharges = msg[k].Substring(2);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex.Message);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                        }
                                        #endregion

                                        #region Line 15 Prepaid Charge Summery
                                        if (lastrec.Equals("COL", StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {
                                                for (int k = 1; k < msg.Length; k++)
                                                {
                                                    if (msg[k].Substring(0, 2).Equals("WT", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.CCweightCharge = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("VC", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.CCValuationCharge = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("TX", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.CCTaxesCharge = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("OA", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.CCOCDA = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("OC", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.CCOCDC = msg[k].Substring(2);
                                                    }
                                                    if (msg[k].Substring(0, 2).Equals("CT", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        fwbdata.CCTotalCharges = msg[k].Substring(2);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex.Message);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                        }
                                        #endregion

                                        #region Line 18 OSI 2
                                        if (lastrec == "OSI")
                                        {
                                            //othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                            if (msg[1].Length > 0)
                                            {
                                                othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 + msg[1];
                                            }
                                            lastrec = "NA";
                                        }
                                        #endregion


                                        #region OCI
                                        if (lastrec.Equals("OCI", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string[] msgdata = str[i].Split('/');
                                            if (msgdata.Length > 0)
                                            {
                                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                custom.IsoCountryCodeOci = msgdata[1];
                                                custom.InformationIdentifierOci = msgdata[2];
                                                custom.CsrIdentifierOci = msgdata[3];
                                                custom.SupplementaryCsrIdentifierOci = msgdata[4];
                                                custom.OCIInfo = str[i];
                                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                                custominfo[custominfo.Length - 1] = custom;
                                            }
                                        }
                                        #endregion

                                        #region OPI
                                        if (lastrec.Equals("OPI", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string[] msgdata = str[i].Split('/');
                                            if (msgdata.Length > 0)
                                            {
                                                fwbdata.othairport = msgdata[1].Substring(0, 3);
                                                fwbdata.othofficedesignator = msgdata[1].Substring(3, 2);
                                                fwbdata.othcompanydesignator = msgdata[1].Substring(5);
                                                fwbdata.othfilereference = msgdata[2];
                                                fwbdata.othparticipentidentifier = msgdata[3];
                                                fwbdata.othparticipentcode = msgdata[4];
                                                fwbdata.othparticipentairport = msgdata[5];
                                            }
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex.Message);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    flag = true;
                }
                catch (Exception)
                {
                    flag = false;
                }
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Method to save the operation data through FWB
        /// </summary>
        /// <param name="fwbdata"></param>
        /// <param name="fltroute"></param>
        /// <param name="OtherCharges"></param>
        /// <param name="othinfoarray"></param>
        /// <param name="fwbrates"></param>
        /// <param name="customextrainfo"></param>
        /// <param name="objDimension"></param>
        /// <param name="REFNo"></param>
        /// <param name="objAWBBup"></param>
        /// <returns></returns>
        /// public async Task<bool> SaveandValidateFWBMessage(MessageData.fwbinfo fwbdata, MessageData.FltRoute[] fltroute, MessageData.othercharges[] OtherCharges, MessageData.otherserviceinfo[] othinfoarray, MessageData.RateDescription[] fwbrates, MessageData.customsextrainfo[] customextrainfo, MessageData.dimensionnfo[] objDimension, int REFNo, MessageData.AWBBuildBUP[] objAWBBup, string strMessage, string strMessageFrom, string strFromID, string strStatus, string PIMAAddress, out string ErrorMsg)
        public async Task<(bool, string ErrorMsg)> SaveandValidateFWBMessage(MessageData.fwbinfo fwbdata, MessageData.FltRoute[] fltroute, MessageData.othercharges[] OtherCharges, MessageData.otherserviceinfo[] othinfoarray, MessageData.RateDescription[] fwbrates, MessageData.customsextrainfo[] customextrainfo, MessageData.dimensionnfo[] objDimension, int REFNo, MessageData.AWBBuildBUP[] objAWBBup, string strMessage, string strMessageFrom, string strFromID, string strStatus, string PIMAAddress, string ErrorMsg)
        {
            bool flag = false;
            try
            {
                #region : Local Variables :
                ErrorMsg = string.Empty;
                string awbnum = fwbdata.awbnum;
                string AWBPrefix = fwbdata.airlineprefix;
                string SHCCode = fwbdata.splhandling;
                string flightnum = "NA", commcode = "", commtype = string.Empty, harmonizedcodes = string.Empty, ProductType = string.Empty;
                string flightdate = System.DateTime.Now.ToString("dd/MM/yyyy");
                string strFlightNo = string.Empty, strFlightOrigin = string.Empty, strFlightDestination = string.Empty;
                string Slac = string.Empty;
                string AWBOriginAirportCode = string.Empty, AWBDestAirportCode = string.Empty, AWBCreatedAndExecutedBy = string.Empty;
                string FltOrg = string.Empty, FltDest = string.Empty;
                string strErrorMessage = string.Empty;
                string fltDate = string.Empty;
                string Priority = string.Empty;
                string shipperSignature = fwbdata.shippersignature.Trim();
                string IATAAgentCode = fwbdata.agentIATAnumber.Trim() + fwbdata.agentCASSaddress.Trim();
                string VolumeAmount = string.Empty;
                DateTime flightDate = DateTime.UtcNow;
                bool val = true
                    , isDesignatorCodeExists = false
                    , isUpdateDIMSWeight = false
                    , isUpdateRouteThroughFWB = false
                    , isDestinationAdjusted = false
                    , stopCreateBookingThroughFWB = false;

                DataSet dsAWBMaterLogOldValues = new DataSet();
                GenericFunction genericFunction = new GenericFunction();
                //FNAMessageProcessor fnaMessageProcessor = new FNAMessageProcessor();
                //FFRMessageProcessor ffRMessageProcessor = new FFRMessageProcessor();

                //SQLServer dtb = new SQLServer();
                //SQLServer sqlServerReadOnly = new SQLServer(true);

                string otherinfostr = string.Empty;
                string screeningInfo = string.Empty;

                if (othinfoarray.Length > 0)
                {
                    otherinfostr = (othinfoarray[0].otherserviceinfo1) + (othinfoarray[0].otherserviceinfo2);

                }

                isUpdateRouteThroughFWB = Convert.ToBoolean(genericFunction.ReadValueFromDb("UpdateRouteThroughFWB") == string.Empty ? "false" : genericFunction.ReadValueFromDb("UpdateRouteThroughFWB"));
                stopCreateBookingThroughFWB = Convert.ToBoolean(genericFunction.ReadValueFromDb("StopCreateBookingThroughFWB") == string.Empty ? "false" : genericFunction.ReadValueFromDb("StopCreateBookingThroughFWB"));
                #endregion Local Variables

                // clsLog.WriteLogAzure("FindLog 101 Start UpdateInboxFromMessageParameter " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 101 Start UpdateInboxFromMessageParameter {0}-{1}", AWBPrefix, awbnum);
                genericFunction.UpdateInboxFromMessageParameter(REFNo, AWBPrefix + "-" + awbnum, string.Empty, string.Empty, string.Empty, "FWB", strMessageFrom == "" ? strFromID : strMessageFrom, DateTime.Parse("1900-01-01"));
                // clsLog.WriteLogAzure("FindLog 101 End UpdateInboxFromMessageParameter " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 101 End UpdateInboxFromMessageParameter {0} - {1}", AWBPrefix, awbnum);

                ///MasterLog
                // clsLog.WriteLogAzure("FindLog 102 Start GetAWBMasterLogNewRecord " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 102 Start GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);
                dsAWBMaterLogOldValues = genericFunction.GetAWBMasterLogNewRecord(AWBPrefix, awbnum);
                // clsLog.WriteLogAzure("FindLog 102 End GetAWBMasterLogNewRecord " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 102 End GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);

                #region Check AWB is present or not
                bool isAWBPresent = false;
                DataSet dsCheck = new DataSet();
                //SQLServer dtbsp_getawbdetails = new SQLServer();
                //string[] paramName1 = new string[] { "AWBNumber", "AWBPrefix" };
                //object[] paramValues1 = new object[] { awbnum, AWBPrefix };
                //SqlDbType[] paramType1 = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };

                SqlParameter[] sqlParameters1 =
                [
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                ];
                // clsLog.WriteLogAzure("FindLog 103 Start sp_getawbdetails " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 103 Start sp_getawbdetails {0} - {1}", AWBPrefix, awbnum);
                //dsCheck = dtbsp_getawbdetails.SelectRecords("sp_getawbdetails", paramName1, paramValues1, paramType1);
                dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameters1);
                // clsLog.WriteLogAzure("FindLog 103 sp_getawbdetails End " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 103 sp_getawbdetails End {0} - {1}", AWBPrefix, awbnum);

                //dtbsp_getawbdetails = null;

                if (dsCheck != null && dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0)
                {
                    if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(awbnum, StringComparison.OrdinalIgnoreCase))
                    {
                        isAWBPresent = true;
                    }
                    if (dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() == "V")
                    {
                        ErrorMsg = AWBPrefix + "-" + awbnum + " AWB is Voided";
                        //return false;
                        return (false, ErrorMsg);
                    }
                    if (dsCheck.Tables[0].Rows[0]["IsVerified"].ToString().ToUpper() == "TRUE")
                    {
                        ErrorMsg = AWBPrefix + "-" + awbnum + " AWB already verified";
                        //return false;
                        return (false, ErrorMsg);
                    }
                }
                if (!isAWBPresent && stopCreateBookingThroughFWB)
                {
                    ErrorMsg = AWBPrefix + "-" + awbnum + " AWB not exists ";
                    //return false;
                    return (false, ErrorMsg);
                }
                #endregion Check AWB is present or not

                //if (dsCheck != null && dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0)
                //{
                //    if (Convert.ToInt32(dsCheck.Tables[12].Rows[0]["AWBRouteCount"]) > 1)
                //    {
                //        for (int i = 0; i < fltroute.Length; i++)
                //        {
                //            if (fltroute[i].fltarrival == "")
                //            {
                //                ErrorMsg = "Invalid RTG line";  
                //                return false;
                //            }
                //        }


                //    }
                //}

                if (fltroute.Length == 1)
                {
                    if (fltroute[0].fltarrival == "")
                    {
                        fltroute[0].fltarrival = fwbdata.dest;
                    }
                }

                #region Set Flight Destination For Incomplete Route(RTG line)
                if ((!isAWBPresent || isUpdateRouteThroughFWB) && fltroute.Length > 0)
                {
                    bool isRouteComplete = false;
                    for (int i = 0; i < fltroute.Length; i++)
                    {
                        if (fltroute[i].fltarrival == fwbdata.dest)
                        {
                            isRouteComplete = true;
                            break;
                        }
                    }
                    if (!isRouteComplete)
                    {
                        DataSet? dsFlightDestForIncompleteRoute = new DataSet();
                        //string[] paramName2 = new string[] { "AWBDestination", "FlightDestination", "SetFlightDestForIncompleteRoute" };
                        //object[] paramValues2 = new object[] { fwbdata.dest, fltroute[fltroute.Length - 1].fltarrival, true };
                        //SqlDbType[] paramType2 = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };

                        SqlParameter[] sqlParameters2 = new SqlParameter[]
                        {
                            new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = fwbdata.dest },
                            new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fltroute[fltroute.Length - 1].fltarrival },
                            new SqlParameter("@SetFlightDestForIncompleteRoute", SqlDbType.Bit) { Value = true }
                        };

                        // clsLog.WriteLogAzure("FindLog 104 Start Messaging.uspGetRequiredDataToProcessFWB " + AWBPrefix + "-" + awbnum);
                        _logger.LogInformation("FindLog 104 Start Messaging.uspGetRequiredDataToProcessFWB {0} - {1}", AWBPrefix, awbnum);

                        //dsFlightDestForIncompleteRoute = sqlServerReadOnly.SelectRecords("Messaging.uspGetRequiredDataToProcessFWB", paramName2, paramValues2, paramType2);
                        dsFlightDestForIncompleteRoute = await _readOnlyDao.SelectRecords("Messaging.uspGetRequiredDataToProcessFWB", sqlParameters2);

                        // clsLog.WriteLogAzure("FindLog 104 End Messaging.uspGetRequiredDataToProcessFWB " + AWBPrefix + "-" + awbnum);
                        _logger.LogInformation("FindLog 104 End Messaging.uspGetRequiredDataToProcessFWB {0} - {1}", AWBPrefix, awbnum);

                        if (dsFlightDestForIncompleteRoute != null && dsFlightDestForIncompleteRoute.Tables.Count > 0 && dsFlightDestForIncompleteRoute.Tables[0].Rows.Count > 0)
                        {
                            if (Convert.ToBoolean(dsFlightDestForIncompleteRoute.Tables[0].Rows[0]["IsAddNewRoute"].ToString()))
                            {
                                MessageData.FltRoute flightRoute = new MessageData.FltRoute("");

                                flightRoute.fltarrival = dsFlightDestForIncompleteRoute.Tables[0].Rows[0]["FlightDestination"].ToString();
                                flightRoute.fltdept = fltroute[fltroute.Length - 1].fltarrival;
                                //flightRoute.carriercode = fltroute[fltroute.Length - 1].carriercode;

                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                fltroute[fltroute.Length - 1] = flightRoute;
                                isDestinationAdjusted = true;
                            }
                        }
                    }
                }
                #endregion Set Flight Destination For Incomplete Route(RTG line)                

                #region : OCI :
                string customShipIDCode = string.Empty, customConsIDCode = string.Empty, customShipperTelephone = string.Empty;
                string customConsigneeTelephone = string.Empty, customConsigneeContactPerson = string.Empty, customShipAEONum = string.Empty, customConsAEONum = string.Empty, customConsigneeContactCountry = string.Empty;
                string customShipContactPerson = string.Empty, customConsContactPerson = string.Empty, customShipContactTelephone = string.Empty, customConsContactTelephone = string.Empty, customRegulatedPartyCategory = string.Empty;
                string customScreeningMethod = string.Empty, customScreenerName = string.Empty, customScreeningDate = string.Empty, knownConsiner = string.Empty, customRegulatedPartyCategoryCountryCode = string.Empty, customRegulatedPartyCategoryISS = string.Empty, customRegulatedPartyCategoryOSS = string.Empty, customScreeningMethodOSS = string.Empty, customScreenerNameOSS = string.Empty, customScreeningDateOSS = string.Empty, customRegulatedPartyCategoryCountryCodeOSS = string.Empty, customRegulatedPartyCategoryOSSInfo = string.Empty;
                bool showIATAOCIInfo = Convert.ToBoolean(genericFunction.GetConfigurationValues("ShowIATAOCIInfo") == string.Empty ? "false"
                    : genericFunction.GetConfigurationValues("ShowIATAOCIInfo"));
                for (int i = 0; i < customextrainfo.Length; i++)
                {
                    customRegulatedPartyCategoryOSS = customextrainfo[i].InformationIdentifierOci == string.Empty ? customRegulatedPartyCategoryOSS : customextrainfo[i].InformationIdentifierOci;
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
                            else if (customextrainfo[i].InformationIdentifierOci == "NFY")
                            {
                                customConsIDCode = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            break;
                        case "U":
                            if (customextrainfo[i].InformationIdentifierOci == "CNE" && !showIATAOCIInfo)
                            {
                                customConsigneeTelephone = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            }
                            break;
                        case "KC":
                            if (customextrainfo[i].InformationIdentifierOci == "CNE" && !showIATAOCIInfo)
                            {
                                customConsigneeContactPerson = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                customConsigneeContactCountry = customextrainfo[i].IsoCountryCodeOci;
                            }
                            else
                            {
                                knownConsiner = knownConsiner + "$" + customextrainfo[i].IsoCountryCodeOci + "/" + "/" + customextrainfo[i].CsrIdentifierOci + "/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
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
                        case "CP":
                            if (showIATAOCIInfo)
                            {
                                if (customextrainfo[i].InformationIdentifierOci == "SHP")
                                {
                                    customShipContactPerson = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else if (customextrainfo[i].InformationIdentifierOci == "CNE")
                                {
                                    customConsContactPerson = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else if (customextrainfo[i].InformationIdentifierOci == "NFY")
                                {
                                    customConsigneeContactPerson = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                    customConsigneeContactCountry = customextrainfo[i].IsoCountryCodeOci;
                                }
                            }
                            break;
                        case "CT":
                            if (showIATAOCIInfo)
                            {
                                if (customextrainfo[i].InformationIdentifierOci == "SHP")
                                {
                                    customShipContactTelephone = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else if (customextrainfo[i].InformationIdentifierOci == "CNE")
                                {
                                    customConsContactTelephone = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else if (customextrainfo[i].InformationIdentifierOci == "NFY")
                                {
                                    customConsigneeTelephone = customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }
                            break;
                        case "RA":
                            if (customRegulatedPartyCategoryOSS == "ISS")
                            {
                                customRegulatedPartyCategoryCountryCode = customextrainfo[i].IsoCountryCodeOci;
                                customRegulatedPartyCategoryISS = customextrainfo[i].InformationIdentifierOci;
                                customRegulatedPartyCategory = customRegulatedPartyCategoryCountryCode + "$" + customRegulatedPartyCategoryISS + "$RA/" + customextrainfo[i].SupplementaryCsrIdentifierOci;

                            }
                            else
                            {
                                customRegulatedPartyCategoryCountryCodeOSS = customextrainfo[i].IsoCountryCodeOci;
                                customRegulatedPartyCategoryOSS = customextrainfo[i].InformationIdentifierOci;
                                customRegulatedPartyCategoryOSSInfo = customRegulatedPartyCategoryCountryCodeOSS + "$" + customRegulatedPartyCategoryOSS + "$RA/" + customextrainfo[i].SupplementaryCsrIdentifierOci;

                            }
                            break;
                        case "SM":
                            if (customRegulatedPartyCategoryOSS == "OSS")
                            {
                                if (customScreeningMethodOSS.Contains("$"))
                                {
                                    customScreeningMethodOSS = customScreeningMethodOSS + ";SM/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else
                                {
                                    customScreeningMethodOSS = customScreeningMethodOSS + "$SM/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }
                            else
                            {
                                if (customScreeningMethod.Contains("$"))
                                {
                                    customScreeningMethod = customScreeningMethod + ";SM/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else
                                {
                                    customScreeningMethod = customScreeningMethod + "$SM/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }

                            break;
                        case "SN":
                            if (customRegulatedPartyCategoryOSS.Contains("OSS"))
                            {
                                if (customScreenerNameOSS.Contains("$"))
                                {
                                    customScreenerNameOSS = customScreenerNameOSS + ";SN/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else
                                {
                                    customScreenerNameOSS = customScreenerNameOSS + "$SN/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }
                            else
                            {
                                if (customScreenerName.Contains("$"))
                                {
                                    customScreenerName = customScreenerName + ";SN/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else
                                {
                                    customScreenerName = customScreenerName + "$SN/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }
                            break;
                        case "SD":

                            if (customRegulatedPartyCategoryOSS.Contains("OSS"))
                            {
                                if (customScreeningDateOSS.Contains("$"))
                                {
                                    customScreeningDateOSS = customScreeningDateOSS + ";SD/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else
                                {
                                    customScreeningDateOSS = customScreeningDateOSS + "$SD/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }
                            else
                            {
                                if (customScreeningDate.Contains("$"))
                                {
                                    customScreeningDate = customScreeningDate + ";SD/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                                else
                                {
                                    customScreeningDate = customScreeningDate + "$SD/" + customextrainfo[i].SupplementaryCsrIdentifierOci;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                //screeningInfo = customRegulatedPartyCategoryCountryCode + "$" + customRegulatedPartyCategoryISS + "$" + "RA/" +customRegulatedPartyCategory + "$" + "SM/"+ customScreeningMethod + "$" + "SN/" + customScreenerName + "$" + "SD/"+customScreeningDate;
                screeningInfo = customRegulatedPartyCategory + customScreeningMethod + customScreenerName + customScreeningDate + "," + customRegulatedPartyCategoryOSSInfo + customScreeningMethodOSS + customScreenerNameOSS + customScreeningDateOSS;
                #endregion OCI
                #region OCIInfo
                if (customextrainfo.Length > 0)
                {
                    try
                    {
                        DataTable dtCustom = new DataTable();
                        dtCustom.Columns.Add("MRNNumber", Type.GetType("System.String"));
                        dtCustom.Columns.Add("MsgType", Type.GetType("System.String"));
                        dtCustom.Columns.Add("Country", Type.GetType("System.String"));
                        dtCustom.Columns.Add("DataID", Type.GetType("System.String"));
                        dtCustom.Columns.Add("DataValue", Type.GetType("System.String"));
                        dtCustom.Columns.Add("FlightNo", Type.GetType("System.String"));
                        dtCustom.Columns.Add("FlightDate", Type.GetType("System.DateTime"));
                        dtCustom.Columns.Add("Info", Type.GetType("System.String"));
                        dtCustom.Columns.Add("ProcessedBy", Type.GetType("System.String"));
                        dtCustom.Columns.Add("ProcessedDate", Type.GetType("System.DateTime"));
                        dtCustom.Columns.Add("Custom", Type.GetType("System.String"));
                        dtCustom.Columns.Add("IsActive", Type.GetType("System.String"));
                        dtCustom.Columns.Add("HAWBNumber", Type.GetType("System.String"));
                        dtCustom.Columns.Add("OCILine", Type.GetType("System.String"));

                        for (int i = 0; i < customextrainfo.Length; i++)
                        {
                            DataRow drawb = dtCustom.NewRow();

                            drawb["MRNNumber"] = "";
                            drawb["MsgType"] = "";
                            drawb["Country"] = customextrainfo[i].IsoCountryCodeOci;
                            drawb["DataID"] = customextrainfo[i].InformationIdentifierOci;
                            drawb["DataValue"] = customextrainfo[i].SupplementaryCsrIdentifierOci;
                            drawb["FlightNo"] = "";
                            drawb["FlightDate"] = System.DateTime.Now;
                            drawb["Info"] = customextrainfo[i].CsrIdentifierOci;
                            drawb["ProcessedBy"] = "FWB";
                            drawb["ProcessedDate"] = System.DateTime.Now;
                            drawb["Custom"] = "";
                            drawb["IsActive"] = "1";
                            drawb["HAWBNumber"] = "";
                            drawb["OCILine"] = customextrainfo[i].OCIInfo;

                            dtCustom.Rows.Add(drawb);
                        }

                        //SQLServer dtbuspGetSetOCIDetails = new SQLServer();

                        SqlParameter[] sqlParametersAWB = new SqlParameter[] {
                                        new SqlParameter("AWBPrefix",AWBPrefix)
                                        , new SqlParameter("AWBNumber",awbnum)
                                        , new SqlParameter("SHCCode",SHCCode)
                                        , new SqlParameter("Custom", dtCustom)
                                         };
                        // clsLog.WriteLogAzure("FindLog 105 Start Messaging.uspGetSetOCIDetails " + AWBPrefix + "-" + awbnum);
                        _logger.LogInformation("FindLog 105 Start Messaging.uspGetSetOCIDetails {0} - {1}", AWBPrefix, awbnum);
                        //dtbuspGetSetOCIDetails.SelectRecords("Messaging.uspGetSetOCIDetails", sqlParameters);
                        await _readWriteDao.SelectRecords("Messaging.uspGetSetOCIDetails", sqlParametersAWB);
                        // clsLog.WriteLogAzure("FindLog 105 End Messaging.uspGetSetOCIDetails " + AWBPrefix + "-" + awbnum);
                        _logger.LogInformation("FindLog 105 End Messaging.uspGetSetOCIDetails {0} - {1}", AWBPrefix, awbnum);

                        //dtbuspGetSetOCIDetails = null;

                    }
                    catch
                    {
                        // clsLog.WriteLogAzure("Error while save FWB OCI information Message:" + awbnum);
                        _logger.LogError("Error while save FWB OCI information Message: {0}", awbnum);
                    }
                }
                #endregion OCIInfo

                // clsLog.WriteLogAzure("Is AWB Present: " + AWBPrefix + "-" + awbnum + ": " + Convert.ToString(isAWBPresent));
                _logger.LogInformation("Is AWB Present: {0} - {1}:{2}", AWBPrefix, awbnum, isAWBPresent);
                #region : Update SHP/CNE After Acceptence :
                if (isAWBPresent)
                {
                    // clsLog.WriteLogAzure("Parameters: " + AWBPrefix + "-" + awbnum + ": " + fwbdata.origin + fwbdata.dest);
                    _logger.LogInformation("Parameters: {0} - {1}:{2}", AWBPrefix, awbnum, fwbdata.origin + fwbdata.dest);
                    DataSet dsAWBStatus = new DataSet();
                    try
                    {
                        flightDate = DateTime.ParseExact(fwbdata.fltday.PadLeft(2, '0') + "/" + DateTime.Now.ToString("MM/yyyy"), "dd/MM/yyyy", null);
                    }
                    catch (Exception)
                    {
                        flightDate = DateTime.UtcNow;
                    }

                    // clsLog.WriteLogAzure("FindLog 106 Start GetAWBStatus " + AWBPrefix + "-" + awbnum);
                    _logger.LogInformation("FindLog 106 Start GetAWBStatus {0} - {1}", AWBPrefix, awbnum);
                    dsAWBStatus = await GetAWBStatus(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "FWB");
                    _logger.LogInformation("FindLog 106 End GetAWBStatus {0} - {1}", AWBPrefix, awbnum);
                    if (dsAWBStatus != null && dsAWBStatus.Tables.Count > 0 && dsAWBStatus.Tables[0].Rows.Count > 0)
                    {
                        if (dsAWBStatus.Tables[0].Columns.Contains("IsAccepted"))
                            // clsLog.WriteLogAzure("Is Accepted: " + AWBPrefix + "-" + awbnum + ": " + dsAWBStatus.Tables[0].Rows[0]["IsAccepted"].ToString());
                            _logger.LogInformation("Is Accepted: {0} - {1}:{2}", AWBPrefix, awbnum, dsAWBStatus.Tables[0].Rows[0]["IsAccepted"]);
                        else
                            // clsLog.WriteLogAzure("IsAccepted cloumn not exists: " + AWBPrefix + "-" + awbnum);
                            _logger.LogWarning("IsAccepted cloumn not exists: {0} - {1}", AWBPrefix, awbnum);

                        AWBCreatedAndExecutedBy = dsAWBStatus.Tables[0].Rows[0]["AWBCreatedAndExecutedBy"].ToString().ToUpper();
                        if (dsAWBStatus.Tables[0].Rows[0]["IsAccepted"].ToString().ToUpper() == "TRUE")
                        {
                            if (dsAWBStatus.Tables[0].Rows[0]["ErrorMessage"].ToString().Trim() == string.Empty)
                            {
                                //string[] PFWB = new string[] { "AirlinePrefix", "AWBNum", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ShipperPincode", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "ConsingneePinCode", "CustAccNo", "IATACargoAgentCode", "CustName", "REFNo", "UpdatedBy", "ComodityCode", "ComodityDesc", "ChargedWeight"
                                //,"CustomShipIDCode","CustomConsIDCode","CustomConsigneeTelephone","CustomConsigneeContactPerson", "CustomShipAEONum", "CustomConsAEONum","CustomConsigneeContactCountry"
                                //,"customShipContactPerson", "customShipContactTelephone", "customConsContactPerson", "customConsContactTelephone"
                                //, "NotifyName", "NotifyAddress", "NotifyCity", "NotifyState", "NotifyCountry", "NotifyPincode", "NotifyTelephone","ScreeningInfo","KnownConsiner"};

                                //SqlDbType[] ParamSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal
                                //, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
                                //, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar
                                //, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};

                                //object[] paramValue = { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.shippername, fwbdata.shipperadd.Trim(','), fwbdata.shipperplace.Trim(','), fwbdata.shipperstate, fwbdata.shippercountrycode, fwbdata.shippercontactnum, fwbdata.shipperpostcode, fwbdata.consname, fwbdata.consadd.Trim(','), fwbdata.consplace.Trim(','), fwbdata.consstate, fwbdata.conscountrycode, fwbdata.conscontactnum, fwbdata.conspostcode, fwbdata.agentaccnum, fwbdata.agentIATAnumber, fwbdata.agentname, REFNo, "FWB", "", commcode, fwbrates[0].awbweight.Trim() == string.Empty ? "0" : fwbrates[0].awbweight.Trim()
                                //                        , customShipIDCode, customConsIDCode, customConsigneeTelephone, customConsigneeContactPerson, customShipAEONum, customConsAEONum, customConsigneeContactCountry
                                //                        , customShipContactPerson, customShipContactTelephone, customConsContactPerson, customConsContactTelephone
                                //                        , fwbdata.notifyname, fwbdata.notifyadd, fwbdata.notifyplace, fwbdata.notifystate, fwbdata.notifycountrycode, fwbdata.notifypostcode, fwbdata.notifycontactnum,"",""};

                                SqlParameter[] sqlParametersPFWB = new SqlParameter[]
                                {
                                    new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                    new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                    new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fwbdata.shippername },
                                    new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = fwbdata.shipperadd.Trim(',') },
                                    new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = fwbdata.shipperplace.Trim(',') },
                                    new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = fwbdata.shipperstate },
                                    new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = fwbdata.shippercountrycode },
                                    new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = fwbdata.shippercontactnum },
                                    new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = fwbdata.shipperpostcode },
                                    new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = fwbdata.consname },
                                    new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = fwbdata.consadd.Trim(',') },
                                    new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = fwbdata.consplace.Trim(',') },
                                    new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = fwbdata.consstate },
                                    new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = fwbdata.conscountrycode },
                                    new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = fwbdata.conscontactnum },
                                    new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = fwbdata.conspostcode },
                                    new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = fwbdata.agentaccnum },
                                    new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = fwbdata.agentIATAnumber },
                                    new SqlParameter("@CustName", SqlDbType.VarChar) { Value = fwbdata.agentname },
                                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                                    new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = commcode },
                                    new SqlParameter("@ChargedWeight", SqlDbType.Decimal) { Value = string.IsNullOrWhiteSpace(fwbrates[0].awbweight) ? "0" : fwbrates[0].awbweight.Trim() },

                                    new SqlParameter("@CustomShipIDCode", SqlDbType.VarChar) { Value = customShipIDCode },
                                    new SqlParameter("@CustomConsIDCode", SqlDbType.VarChar) { Value = customConsIDCode },
                                    new SqlParameter("@CustomConsigneeTelephone", SqlDbType.VarChar) { Value = customConsigneeTelephone },
                                    new SqlParameter("@CustomConsigneeContactPerson", SqlDbType.VarChar) { Value = customConsigneeContactPerson },
                                    new SqlParameter("@CustomShipAEONum", SqlDbType.VarChar) { Value = customShipAEONum },
                                    new SqlParameter("@CustomConsAEONum", SqlDbType.VarChar) { Value = customConsAEONum },
                                    new SqlParameter("@CustomConsigneeContactCountry", SqlDbType.VarChar) { Value = customConsigneeContactCountry },

                                    new SqlParameter("@customShipContactPerson", SqlDbType.VarChar) { Value = customShipContactPerson },
                                    new SqlParameter("@customShipContactTelephone", SqlDbType.VarChar) { Value = customShipContactTelephone },
                                    new SqlParameter("@customConsContactPerson", SqlDbType.VarChar) { Value = customConsContactPerson },
                                    new SqlParameter("@customConsContactTelephone", SqlDbType.VarChar) { Value = customConsContactTelephone },

                                    new SqlParameter("@NotifyName", SqlDbType.VarChar) { Value = fwbdata.notifyname },
                                    new SqlParameter("@NotifyAddress", SqlDbType.VarChar) { Value = fwbdata.notifyadd },
                                    new SqlParameter("@NotifyCity", SqlDbType.VarChar) { Value = fwbdata.notifyplace },
                                    new SqlParameter("@NotifyState", SqlDbType.VarChar) { Value = fwbdata.notifystate },
                                    new SqlParameter("@NotifyCountry", SqlDbType.VarChar) { Value = fwbdata.notifycountrycode },
                                    new SqlParameter("@NotifyPincode", SqlDbType.VarChar) { Value = fwbdata.notifypostcode },
                                    new SqlParameter("@NotifyTelephone", SqlDbType.VarChar) { Value = fwbdata.notifycontactnum },
                                    new SqlParameter("@ScreeningInfo", SqlDbType.VarChar) { Value = "" },
                                    new SqlParameter("@KnownConsiner", SqlDbType.VarChar) { Value = "" }
                                };


                                // clsLog.WriteLogAzure("FindLog 107 Start GenerateFNAMessage " + AWBPrefix + "-" + awbnum);
                                _logger.LogInformation("FindLog 107 Start GenerateFNAMessage {0} - {1}", AWBPrefix, awbnum);
                                _fNAMessageProcessor.GenerateFNAMessage(strMessage, "AWB IS ALREADY ACCEPTED WE WILL ONLY UPDATE SHIPPER AND CONSIGNEE INFO", AWBPrefix, awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype, PIMAAddress);
                                // clsLog.WriteLogAzure("FindLog 107 End GenerateFNAMessage " + AWBPrefix + "-" + awbnum);
                                _logger.LogInformation("FindLog 107 End GenerateFNAMessage {0} - {1}", AWBPrefix, awbnum);

                                //string strProcedure = "uspUpdateShipperConsigneeforFWB";
                                //SQLServer dtbuspUpdateShipperConsigneeforFWB = new SQLServer();

                                // clsLog.WriteLogAzure("FindLog 108 Start uspUpdateShipperConsigneeforFWB.InsertData " + AWBPrefix + "-" + awbnum);
                                _logger.LogInformation("FindLog 108 Start uspUpdateShipperConsigneeforFWB.InsertData {0} - {1}", AWBPrefix, awbnum);
                                //if (dtbuspUpdateShipperConsigneeforFWB.InsertData(strProcedure, PFWB, ParamSqlType, paramValue))
                                if (await _readWriteDao.ExecuteNonQueryAsync("uspUpdateShipperConsigneeforFWB", sqlParametersPFWB))
                                {
                                    // clsLog.WriteLogAzure("FindLog 108 End uspUpdateShipperConsigneeforFWB.InsertData " + AWBPrefix + "-" + awbnum);
                                    _logger.LogInformation("FindLog 108 End uspUpdateShipperConsigneeforFWB.InsertData {0} - {1}", AWBPrefix, awbnum);
                                    //dtbuspUpdateShipperConsigneeforFWB = null;

                                    //SQLServer dtbSPAddAWBAuditLog = new SQLServer();
                                    //string[] CaNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "Volume" };
                                    //SqlDbType[] CaType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                    //object[] CaValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin, fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, fltroute[0].carriercode + fltroute[0].fltnum, flightDate, fltroute[0].fltdept, fltroute[0].fltarrival, "Updated", "AWB Updated", "AWB Updated Through FWB", "FWB", DateTime.UtcNow.ToString(), 1, VolumeAmount.Trim() == "" ? "0" : VolumeAmount };
                                    //if (!dtbSPAddAWBAuditLog.ExecuteProcedure("SPAddAWBAuditLog", CaNname, CaType, CaValues, 600))

                                    SqlParameter[] sqlParametersCaNname = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                        new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fwbdata.origin },
                                        new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fwbdata.dest },
                                        new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                                        new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[0].carriercode + fltroute[0].fltnum },
                                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightDate },
                                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fltroute[0].fltdept },
                                        new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fltroute[0].fltarrival },
                                        new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Updated" },
                                        new SqlParameter("@Message", SqlDbType.VarChar) { Value = "AWB Updated" },
                                        new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Updated Through FWB" },
                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                                        new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.UtcNow.ToString() },
                                        new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                                        new SqlParameter("@Volume", SqlDbType.VarChar) { Value = string.IsNullOrWhiteSpace(VolumeAmount) ? "0" : VolumeAmount }
                                    };


                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParametersCaNname, 600))
                                        // clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine);
                                        _logger.LogInformation("AWB Audit log  for: {0}", fwbdata.awbnum + Environment.NewLine);
                                    ///MasterLog

                                    //dtbSPAddAWBAuditLog = null;

                                    GenericFunction gf = new GenericFunction();
                                    DataSet dsAWBMaterLogNewValues = new DataSet();

                                    // clsLog.WriteLogAzure("FindLog 109 Start GetAWBMasterLogNewRecord " + AWBPrefix + "-" + awbnum);
                                    _logger.LogInformation("FindLog 109 Start GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);
                                    dsAWBMaterLogNewValues = gf.GetAWBMasterLogNewRecord(AWBPrefix, awbnum);
                                    // clsLog.WriteLogAzure("FindLog 109 End GetAWBMasterLogNewRecord " + AWBPrefix + "-" + awbnum);
                                    _logger.LogInformation("FindLog 109 End GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);
                                    if (dsAWBMaterLogNewValues != null && dsAWBMaterLogNewValues.Tables.Count > 0 && dsAWBMaterLogNewValues.Tables[0].Rows.Count > 0)
                                    {
                                        DataTable dtMasterAuditLog = new DataTable();
                                        DataTable dtOldValues = new DataTable();
                                        DataTable dtNewValues = new DataTable();
                                        if (dsAWBMaterLogOldValues != null && dsAWBMaterLogOldValues.Tables.Count > 0 && dsAWBMaterLogOldValues.Tables[0].Rows.Count > 0)
                                            dtOldValues = dsAWBMaterLogOldValues.Tables[0];
                                        else
                                            dtOldValues = null;
                                        dtNewValues = dsAWBMaterLogNewValues.Tables[0];
                                        gf.MasterAuditLog(dtOldValues, dtNewValues, AWBPrefix, awbnum, "Update", "FWB", System.DateTime.Now);
                                    }
                                    //return flag = true;
                                    return (true, ErrorMsg);
                                }
                                else
                                {
                                    // clsLog.WriteLogAzure("FindLog 108 DataNotSaved uspUpdateShipperConsigneeforFWB.InsertData " + AWBPrefix + "-" + awbnum);
                                    _logger.LogWarning("FindLog 108 DataNotSaved uspUpdateShipperConsigneeforFWB.InsertData {0} - {1}", AWBPrefix, awbnum);
                                    //return flag = false;
                                    return (false, ErrorMsg);
                                }
                            }
                            else
                            {
                                // clsLog.WriteLogAzure("FindLog 555 Error " + AWBPrefix + "-" + awbnum + " :" + dsAWBStatus.Tables[0].Rows[0]["ErrorMessage"].ToString());
                                _logger.LogWarning("FindLog 555 Error {0} - {1}:{2}", AWBPrefix, awbnum, dsAWBStatus.Tables[0].Rows[0]["ErrorMessage"]);

                                ErrorMsg = dsAWBStatus.Tables[0].Rows[0]["ErrorMessage"].ToString();
                                //return flag = false;
                                return (false, ErrorMsg);
                            }
                        }
                    }
                }

                #endregion Update SHP/CNE After Acceptence

                if (strFromID.Contains("SITA"))
                    commtype = "SITAFTP";
                else
                    commtype = "EMAIL";

                string strAWbIssueDate = string.Empty;
                if (fwbdata.carrierdate != "" && fwbdata.carriermonth != "" && fwbdata.carrieryear != "")
                {

                    int month = DateTime.Parse("1." + (fwbdata.carriermonth.ToString().PadLeft(2, '0')) + " 2008").Month;
                    strAWbIssueDate = month + "/" + fwbdata.carrierdate.PadLeft(2, '0') + "/" + "20" + fwbdata.carrieryear;
                }
                else
                {
                    strAWbIssueDate = System.DateTime.Now.ToString("MM/dd/yyyy");
                }

                #region : Validate AWB :
                DataSet? dsValidateFFRAWB = new DataSet();

                // clsLog.WriteLogAzure("FindLog 110 Start ValidateFFRAWB " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 110 Start ValidateFFRAWB {0} - {1}", AWBPrefix, awbnum);
                dsValidateFFRAWB = await _fFRMessageProcessor.ValidateFFRAWB(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "FWB", true, shipperSignature: shipperSignature, IATAAgentCode: IATAAgentCode);
                // clsLog.WriteLogAzure("FindLog 110 End ValidateFFRAWB " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 110 End ValidateFFRAWB {0} - {1}", AWBPrefix, awbnum);
                if (dsValidateFFRAWB != null && dsValidateFFRAWB.Tables.Count > 0 && dsValidateFFRAWB.Tables[0].Rows.Count > 0)
                {
                    if (dsValidateFFRAWB.Tables[0].Columns.Contains("ErrorMessage") && dsValidateFFRAWB.Tables[0].Rows[0]["ErrorMessage"].ToString() != string.Empty)
                    {
                        // clsLog.WriteLogAzure("FindLog 556 Error " + AWBPrefix + "-" + awbnum + " :- " + dsValidateFFRAWB.Tables[0].Rows[0]["ErrorMessage"].ToString());
                        _logger.LogInformation("FindLog 556 Error {0} - {1}:{2}", AWBPrefix, awbnum, dsValidateFFRAWB.Tables[0].Rows[0]["ErrorMessage"]);

                        ErrorMsg = dsValidateFFRAWB.Tables[0].Rows[0]["ErrorMessage"].ToString();
                        //return flag = false;
                        return (false, ErrorMsg);
                    }
                }
                #endregion Validate AWB

                if (fltroute.Length > 0)
                {
                    #region : Check flight extsts or not in schedule :
                    bool isCheckValidFlight = false;
                    isCheckValidFlight = Convert.ToBoolean(genericFunction.ReadValueFromDb("ChkFltPresentAndAWBStatus") == string.Empty ? "false" : genericFunction.ReadValueFromDb("ChkFltPresentAndAWBStatus"));
                    if (isCheckValidFlight)
                    {
                        DataSet dsawbFlt = new DataSet();
                        bool isUpdateRoute = true, isFirstLeg = false, isLastLeg = false;
                        int flightDay = 0;
                        for (int lstIndex = 0; lstIndex < fltroute.Length; lstIndex++)
                        {
                            bool isFlightDayAvailable = false;
                            isFirstLeg = lstIndex == 0 ? true : false;
                            isLastLeg = lstIndex == fltroute.Length - 1 ? true : false;
                            //flightDay = fltroute[lstIndex].date.Trim() == string.Empty ? flightDay == 0 ? 0 : flightDay + 1 : Convert.ToInt32(fltroute[lstIndex].date.Trim());
                            flightDay = fltroute[lstIndex].date.Trim() == string.Empty ? flightDay == 0 ? 0 : flightDay : Convert.ToInt32(fltroute[lstIndex].date.Trim());
                            isFlightDayAvailable = fltroute[lstIndex].date.Trim() == string.Empty ? false : true;
                            DataSet? dsOnDAirportCode = new DataSet();

                            // clsLog.WriteLogAzure("FindLog 111 Start ValidateFFRAWB1 " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 111 Start ValidateFFRAWB1 {0} - {1}", AWBPrefix, awbnum);
                            dsOnDAirportCode = await _fFRMessageProcessor.ValidateFFRAWB(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "FWB", false, isFirstLeg, isLastLeg
                                , isDestinationAdjusted, isFlightDayAvailable, fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival
                                , fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, flightDay, REFNo, issueDate: strAWbIssueDate);
                            // clsLog.WriteLogAzure("FindLog 111 End ValidateFFRAWB1 " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 111 End ValidateFFRAWB1 {0} - {1}", AWBPrefix, awbnum);
                            if (flightDay > 31)
                            {
                                ErrorMsg = "Incorrect flight date format";
                                //return false;
                                return (false, ErrorMsg);
                            }

                            if (dsOnDAirportCode != null && dsOnDAirportCode.Tables.Count > 0 && dsOnDAirportCode.Tables[0].Rows.Count > 0)
                            {
                                fltDate = dsOnDAirportCode.Tables[0].Rows[0]["FlightDateMM"].ToString();
                                flightDay = Convert.ToInt32(dsOnDAirportCode.Tables[0].Rows[0]["FlightDay"].ToString());
                                if (isDestinationAdjusted && lstIndex == fltroute.Length - 1)
                                    fltroute[lstIndex].fltarrival = dsOnDAirportCode.Tables[0].Rows[0]["FlightDestination"].ToString();
                                if (isLastLeg)
                                    AWBDestAirportCode = dsOnDAirportCode.Tables[0].Rows[0]["AWBDestination"].ToString();
                            }
                            if (lstIndex == 0)
                            {
                                if (dsOnDAirportCode != null && dsOnDAirportCode.Tables.Count > 0 && dsOnDAirportCode.Tables[0].Rows.Count > 0)
                                {
                                    FltOrg = dsOnDAirportCode.Tables[0].Rows[0]["FlightOrigin"].ToString();
                                    FltDest = fltroute[lstIndex].fltarrival;
                                    ///Set airport code when awb origin stated with city code
                                    if (FltOrg.Trim() != string.Empty)
                                        fltroute[lstIndex].fltdept = FltOrg;
                                    AWBOriginAirportCode = dsOnDAirportCode.Tables[0].Rows[0]["AWBOrigin"].ToString();
                                    flightdate = dsOnDAirportCode.Tables[0].Rows[0]["FlightDateDD"].ToString();
                                }
                            }
                            else
                            {
                                FltOrg = fltroute[lstIndex].fltdept;
                                FltDest = fltroute[lstIndex].fltarrival;
                            }

                            #region : Check Valid Flights :
                            //string[] parms = new string[]
                            //    {
                            //            "FltOrigin",
                            //            "FltDestination",
                            //            "FlightNo",
                            //            "flightDate",
                            //            "AWBNumber",
                            //            "AWBPrefix",
                            //            "RefNo",
                            //            "UpdatedBy"
                            //    };
                            //SqlDbType[] dataType = new SqlDbType[]
                            //    {
                            //            SqlDbType.VarChar,
                            //            SqlDbType.VarChar,
                            //            SqlDbType.VarChar,
                            //            SqlDbType.DateTime,
                            //            SqlDbType.VarChar,
                            //            SqlDbType.VarChar,
                            //            SqlDbType.Int,
                            //            SqlDbType.VarChar
                            //    };
                            //object[] value = new object[]
                            //    {
                            //            FltOrg,
                            //            FltDest,
                            //            fltroute[lstIndex].carriercode+fltroute[lstIndex].fltnum,
                            //            DateTime.Parse(fltDate),
                            //            awbnum,
                            //            AWBPrefix,
                            //            REFNo,
                            //            "FWB"
                            //    };
                            //SQLServer dtbuspValidateFFRFWBFlight = new SQLServer();

                            SqlParameter[] sqlParametersft = new SqlParameter[]
                            {
                                new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = FltOrg },
                                new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = FltDest },
                                new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = DateTime.Parse(fltDate) },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@RefNo", SqlDbType.Int) { Value = REFNo },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" }
                            };

                            // clsLog.WriteLogAzure("FindLog 112 Start Messaging.uspValidateFFRFWBFlight " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 112 Start Messaging.uspValidateFFRFWBFlight {0} - {1}", AWBPrefix, awbnum);
                            //DataSet dsdata = dtbuspValidateFFRFWBFlight.SelectRecords("Messaging.uspValidateFFRFWBFlight", parms, value, dataType);
                            DataSet? dsdata = await _readWriteDao.SelectRecords("Messaging.uspValidateFFRFWBFlight", sqlParametersft);
                            // clsLog.WriteLogAzure("FindLog 112 End Messaging.uspValidateFFRFWBFlight " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 112 End Messaging.uspValidateFFRFWBFlight {0} - {1}", AWBPrefix, awbnum);
                            //dtbuspValidateFFRFWBFlight = null;
                            if (dsdata != null && dsdata.Tables.Count > 0)
                            {
                                for (int i = 0; i < dsdata.Tables.Count; i++)
                                {
                                    if (isUpdateRoute)
                                    {
                                        if (dsdata.Tables[i].Columns.Contains("UpdateRouteThroughFWB")
                                            && !Convert.ToBoolean(dsdata.Tables[i].Rows[0]["UpdateRouteThroughFWB"].ToString()))
                                        {
                                            isUpdateRoute = false;
                                            break;
                                        }
                                        if (!isDesignatorCodeExists && dsdata.Tables[i].Columns.Contains("IsDesignatorCodeExists")
                                            && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                            isDesignatorCodeExists = true;
                                        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() == "0"
                                           && dsdata.Tables[i].Columns.Contains("ErrorMessage") && dsdata.Tables[i].Rows[0]["ErrorMessage"].ToString().Trim() != ""
                                           && dsdata.Tables[i].Columns.Contains("IsDesignatorCodeExists")
                                           && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                        {
                                            ErrorMsg = dsdata.Tables[i].Rows[0]["ErrorMessage"].ToString().Trim();
                                            //return false;
                                            return (false, ErrorMsg);
                                        }
                                        else if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() == "0"
                                           && dsdata.Tables[i].Columns.Contains("IsDesignatorCodeExists")
                                           && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                        {
                                            ErrorMsg = "No available flight for this route";
                                            //return false;
                                            return (false, ErrorMsg);
                                        }
                                        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() != "0"
                                           && dsdata.Tables[i].Columns.Contains("IsDesignatorCodeExists") && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                        {
                                            fltroute[lstIndex].flightdate = fltDate;
                                            fltroute[lstIndex].scheduleid = dsdata.Tables[i].Rows[0]["ScheduleID"].ToString();
                                        }
                                        if (fltroute[lstIndex].carriercode.Trim() + fltroute[lstIndex].fltnum.Trim() == string.Empty
                                           && dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() != "0")
                                        {
                                            isDesignatorCodeExists = true;
                                            fltroute[lstIndex].flightdate = fltDate;
                                            fltroute[lstIndex].scheduleid = dsdata.Tables[i].Rows[0]["ScheduleID"].ToString();
                                        }
                                        if (dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() == "0")
                                        {
                                            fltroute[lstIndex].flightdate = fltDate;
                                        }
                                    }
                                }
                            }
                            #endregion Check Valid Flights
                        }
                        if (!isDesignatorCodeExists && isUpdateRoute)
                        {
                            ErrorMsg = "Flight number is invalid";
                            //return false;
                            return (false, ErrorMsg);
                        }
                    }
                    #endregion Check flight extsts or not in schedule

                    flightnum = fltroute[0].carriercode + fltroute[0].fltnum;
                    strFlightOrigin = fltroute[0].fltdept;
                    strFlightDestination = fltroute[0].fltarrival;
                    //if (fltroute[0].date != "")
                    //    flightdate = fltroute[0].date + "/" + DateTime.Now.ToString("MM/yyyy");
                    //else
                    //    flightdate = DateTime.Now.ToString("dd/MM/yyyy");
                }
                else
                {
                    if (fwbdata.fltnum.Length > 0 && !(fwbdata.fltnum.Contains(',')))
                    {
                        flightnum = fwbdata.fltnum;
                        flightdate = fwbdata.fltday.PadLeft(2, '0') + "/" + DateTime.Now.ToString("MM/yyyy");
                    }
                }
                if (fwbrates[0].ngnc == "NG")
                {
                    commcode = fwbrates[0].goodsnature.Length > 0
                        ? fwbrates[0].goodsnature1.Length > 0
                            ? fwbrates[0].goodsnature + "," + fwbrates[0].goodsnature1
                            : fwbrates[0].goodsnature
                        : fwbrates[0].goodsnature1;
                }
                else
                {
                    commcode = fwbrates[0].goodsnature1.Length > 0
                        ? fwbrates[0].goodsnature.Length > 0
                            ? fwbrates[0].goodsnature1 + "," + fwbrates[0].goodsnature
                            : fwbrates[0].goodsnature1
                        : fwbrates[0].goodsnature;
                }

                if (fwbrates[0].ProductType.Length > 0)
                    ProductType = fwbrates[0].ProductType;

                for (int i = 0; i < fwbrates.Length; i++)
                {
                    if (fwbrates[i].hermonisedcomoditycode.Length > 1)
                        harmonizedcodes += fwbrates[i].hermonisedcomoditycode;
                }

                //DataSet dsawb = new DataSet();
                //dsawb = ffRMessageProcessor.CheckValidateFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "FWB", fltroute[0].fltdept, fltroute[fltroute.Length - 1].fltarrival, fltroute[fltroute.Length - 1].carriercode + fltroute[fltroute.Length - 1].fltnum, fltDate, fltroute[0].carriercode + fltroute[0].fltnum, REFNo);
                //if (dsawb != null && dsawb.Tables.Count > 1 && dsawb.Tables[1].Rows.Count > 0)
                //{
                //    if (!(dsawb.Tables.Count > 0 && dsawb.Tables[1].Rows.Count > 0 && dsawb.Tables[1].Columns.Count == 2 && dsawb.Tables[1].Columns.Contains("AWBSttus")))
                //    {
                //        strErrorMessage = dsawb.Tables[1].Rows[0]["ErrorMessage"].ToString();
                //        ErrorMsg = strErrorMessage;
                //        if (!ErrorMsg.Contains("AWBNo has mismatch Origin/Destination."))
                //        {
                //            strErrorMessage = string.Empty;
                //            return flag = false;
                //        }
                //    }
                //}

                //if (dsawb != null && dsawb.Tables.Count > 0 && dsawb.Tables[0].Rows.Count > 0 && dsawb.Tables[0].Columns.Contains("AWBOriginAirportCode"))
                //{
                //    AWBOriginAirportCode = dsawb.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                //    AWBDestAirportCode = dsawb.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                //}

                // clsLog.WriteLogAzure("FindLog 113 Start GenerateFMAMessage " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 113 Start GenerateFMAMessage {0} - {1}", AWBPrefix, awbnum);
                _fNAMessageProcessor.GenerateFMAMessage(strMessage, "WE WILL BOOK EXECUTE AWB " + fwbdata.airlineprefix + "-" + fwbdata.awbnum + " SHORTLY", fwbdata.airlineprefix, fwbdata.awbnum, strMessageFrom == "" ? strFromID : strMessageFrom, commtype, PIMAAddress);
                // clsLog.WriteLogAzure("FindLog 113 End GenerateFMAMessage " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 113 End GenerateFMAMessage {0} - {1}", AWBPrefix, awbnum);
                #region : Chargeable Weight Calculation(using volume and gross weight):

                //string VolumeAmount = string.Empty,
                string volcode = string.Empty;
                try
                {
                    VolumeAmount = (fwbdata.volumeamt.Length > 0 ? fwbdata.volumeamt : fwbrates[0].volamt);
                    volcode = (fwbdata.volumecode.Length > 0 ? fwbdata.volumecode : fwbrates[0].volcode);
                }
                catch (Exception)
                {
                    VolumeAmount = fwbdata.volumeamt;
                    volcode = (fwbdata.volumecode.Length > 0 ? fwbdata.volumecode : fwbrates[0].volcode);
                }

                decimal VolumeWt = 0;
                if (volcode != "" && Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) > 0)
                {
                    switch (volcode.ToUpper())
                    {
                        case "MC":
                            VolumeWt = decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) *
                                                                              decimal.Parse("166.67"))));
                            break;

                        case "CI":
                            VolumeWt =
                                decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal((Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) /
                                                                              decimal.Parse("366")))));
                            VolumeAmount = String.Format("{0:0.00}", (Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) / 1728));
                            volcode = "MC";
                            break;
                        case "CF":
                            VolumeWt =
                                decimal.Parse(String.Format("{0:0.00}",
                                                            Convert.ToDecimal(Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) *
                                                                              decimal.Parse("4.7194"))));
                            break;
                        case "CC":
                            VolumeWt =
                               decimal.Parse(String.Format("{0:0.00}",
                                                           Convert.ToDecimal(((Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) /
                                                                              decimal.Parse("6000"))))));
                            VolumeAmount = String.Format("{0:0.00}", (Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount) / 1000000));
                            volcode = "MC";
                            break;
                        default:
                            VolumeWt = Convert.ToDecimal(Convert.ToDecimal(VolumeAmount == "" ? "0" : VolumeAmount));
                            break;
                    }
                }

                if ((volcode == "MC" && VolumeAmount != string.Empty && Convert.ToDecimal(VolumeAmount) > 100)
                    || (volcode == "CF" && VolumeAmount != string.Empty && Convert.ToDecimal(VolumeAmount) > 3546))
                {
                    genericFunction.UpdateErrorMessageToInbox(REFNo, "Please check AWB volume details", "FWB", false, "", false);
                }


                decimal ChargeableWeight = 0;
                bool doNotCalculateChargeableWeightFromVolume = !string.IsNullOrEmpty(genericFunction.ReadValueFromDb("DoNotCalculateChargeableWeightFromVolume").Trim()) && Convert.ToBoolean(genericFunction.ReadValueFromDb("DoNotCalculateChargeableWeightFromVolume").Trim());

                if (!doNotCalculateChargeableWeightFromVolume)
                {

                    if (VolumeWt > 0)
                    {
                        if (Convert.ToDecimal(fwbdata.weight == "" ? "0" : fwbdata.weight) > VolumeWt)
                            ChargeableWeight = Convert.ToDecimal(fwbdata.weight == "" ? "0" : fwbdata.weight);
                        else
                            ChargeableWeight = VolumeWt;
                    }
                }
                else
                    ChargeableWeight = Convert.ToDecimal(fwbdata.weight == "" ? "0" : fwbdata.weight);

                #endregion Chargeable Weight Calculation(using volume and gross weight)

                #region : Save AWB Details :

                if (objAWBBup.Length > 0)
                {
                    if (objAWBBup[0].SlacCount != "" && objAWBBup[0].SlacCount != null)
                    {
                        Slac = objAWBBup[0].SlacCount;
                    }
                }

                //string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode"
                //    , "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace"
                //    , "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode"
                //    , "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height"
                //    , "PartnerStatus", "REFNo", "UpdatedBy", "SpecialHandelingCode", "Paymode", "ShipperPincode", "ConsingneePinCode", "WeightCode"
                //    , "AWBIssueDate", "VolumeCode","VolumeWt","ChargeableWeight", "Slac", "HarmonizedCodes"
                //    , "CustomShipIDCode","CustomConsIDCode","CustomConsigneeTelephone","CustomConsigneeContactPerson", "CustomShipAEONum", "CustomConsAEONum","CustomConsigneeContactCountry"
                //    , "customShipContactPerson", "customShipContactTelephone", "customConsContactPerson", "customConsContactTelephone", "AgentCASSaddress", "SHPSignature","ProductType","OtherServiceInformation"
                //    , "NotifyName", "NotifyAddress", "NotifyCity", "NotifyState", "NotifyCountry", "NotifyPincode", "NotifyTelephone","ScreeningInfo","KnownConsiner"};

                //object[] paramvalue = new object[] {fwbdata.airlineprefix,fwbdata.awbnum,AWBOriginAirportCode, AWBDestAirportCode,fwbdata.pcscnt, fwbdata.weight,
                //    VolumeAmount.Trim() == "" ? "0" : VolumeAmount, "", commcode,fwbdata.carriercode,flightnum,flightdate, strFlightOrigin,strFlightDestination, fwbdata.shippername.Trim(' '),
                //                                         fwbdata.shipperadd.Trim(','), fwbdata.shipperplace.Trim(','), fwbdata.shipperstate, fwbdata.shippercountrycode, fwbdata.shippercontactnum, fwbdata.consname.Trim(' '), fwbdata.consadd.Trim(','), fwbdata.consplace.Trim(','), fwbdata.consstate, fwbdata.conscountrycode,
                //                                         fwbdata.conscontactnum, fwbdata.agentaccnum, fwbdata.agentIATAnumber, fwbdata.agentname, DateTime.Now.ToString("yyyy-MM-dd"),"", "", "", "", "",REFNo, "FWB",fwbdata.splhandling,fwbdata.chargecode,fwbdata.shipperpostcode,fwbdata.conspostcode,fwbdata.weightcode,strAWbIssueDate,fwbdata.volumecode,VolumeWt,ChargeableWeight,Slac,harmonizedcodes
                //, customShipIDCode, customConsIDCode, customConsigneeTelephone, customConsigneeContactPerson, customShipAEONum, customConsAEONum, customConsigneeContactCountry
                //, customShipContactPerson, customShipContactTelephone, customConsContactPerson, customConsContactTelephone,fwbdata.agentCASSaddress, shipperSignature,ProductType,otherinfostr
                //, fwbdata.notifyname, fwbdata.notifyadd, fwbdata.notifyplace, fwbdata.notifystate, fwbdata.notifycountrycode, fwbdata.notifypostcode, fwbdata.notifycontactnum,"",""};

                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //                                              SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //                                              SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.Int,
                //                                            SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.VarChar,SqlDbType.Decimal,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar
                //,SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
                //,SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar
                //,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar};


                //string procedure = "spInsertBookingDataFromFFR";
                //SQLServer dtbspInsertBookingDataFromFFR = new SQLServer();

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AirlinePrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                    new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                    new SqlParameter("@Origin", SqlDbType.VarChar) { Value = AWBOriginAirportCode },
                    new SqlParameter("@Dest", SqlDbType.VarChar) { Value = AWBDestAirportCode },
                    new SqlParameter("@PcsCount", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                    new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                    new SqlParameter("@Volume", SqlDbType.VarChar) { Value = VolumeAmount.Trim() == "" ? "0" : VolumeAmount },
                    new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = commcode },
                    new SqlParameter("@CarrierCode", SqlDbType.VarChar) { Value = fwbdata.carriercode },
                    new SqlParameter("@FlightNum", SqlDbType.VarChar) { Value = flightnum },
                    new SqlParameter("@FlightDate", SqlDbType.VarChar) { Value = flightdate },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                    new SqlParameter("@FlightDest", SqlDbType.VarChar) { Value = strFlightDestination },
                    new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fwbdata.shippername.Trim(' ') },
                    new SqlParameter("@ShipperAddr", SqlDbType.VarChar) { Value = fwbdata.shipperadd.Trim(',') },
                    new SqlParameter("@ShipperPlace", SqlDbType.VarChar) { Value = fwbdata.shipperplace.Trim(',') },
                    new SqlParameter("@ShipperState", SqlDbType.VarChar) { Value = fwbdata.shipperstate },
                    new SqlParameter("@ShipperCountryCode", SqlDbType.VarChar) { Value = fwbdata.shippercountrycode },
                    new SqlParameter("@ShipperContactNo", SqlDbType.VarChar) { Value = fwbdata.shippercontactnum },
                    new SqlParameter("@ConsName", SqlDbType.VarChar) { Value = fwbdata.consname.Trim(' ') },
                    new SqlParameter("@ConsAddr", SqlDbType.VarChar) { Value = fwbdata.consadd.Trim(',') },
                    new SqlParameter("@ConsPlace", SqlDbType.VarChar) { Value = fwbdata.consplace.Trim(',') },
                    new SqlParameter("@ConsState", SqlDbType.VarChar) { Value = fwbdata.consstate },
                    new SqlParameter("@ConsCountryCode", SqlDbType.VarChar) { Value = fwbdata.conscountrycode },
                    new SqlParameter("@ConsContactNo", SqlDbType.VarChar) { Value = fwbdata.conscontactnum },
                    new SqlParameter("@CustAccNo", SqlDbType.VarChar) { Value = fwbdata.agentaccnum },
                    new SqlParameter("@IATACargoAgentCode", SqlDbType.VarChar) { Value = fwbdata.agentIATAnumber },
                    new SqlParameter("@CustName", SqlDbType.VarChar) { Value = fwbdata.agentname },
                    new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd") },
                    new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@Length", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@Breadth", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@Height", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@PartnerStatus", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                    new SqlParameter("@SpecialHandelingCode", SqlDbType.VarChar) { Value = fwbdata.splhandling },
                    new SqlParameter("@Paymode", SqlDbType.VarChar) { Value = fwbdata.chargecode },
                    new SqlParameter("@ShipperPincode", SqlDbType.VarChar) { Value = fwbdata.shipperpostcode },
                    new SqlParameter("@ConsingneePinCode", SqlDbType.VarChar) { Value = fwbdata.conspostcode },
                    new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = fwbdata.weightcode },
                    new SqlParameter("@AWBIssueDate", SqlDbType.DateTime) { Value = strAWbIssueDate },
                    new SqlParameter("@VolumeCode", SqlDbType.VarChar) { Value = fwbdata.volumecode },
                    new SqlParameter("@VolumeWt", SqlDbType.Decimal) { Value = VolumeWt },
                    new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                    new SqlParameter("@Slac", SqlDbType.VarChar) { Value = Slac },
                    new SqlParameter("@HarmonizedCodes", SqlDbType.VarChar) { Value = harmonizedcodes },
                    new SqlParameter("@CustomShipIDCode", SqlDbType.VarChar) { Value = customShipIDCode },
                    new SqlParameter("@CustomConsIDCode", SqlDbType.VarChar) { Value = customConsIDCode },
                    new SqlParameter("@CustomConsigneeTelephone", SqlDbType.VarChar) { Value = customConsigneeTelephone },
                    new SqlParameter("@CustomConsigneeContactPerson", SqlDbType.VarChar) { Value = customConsigneeContactPerson },
                    new SqlParameter("@CustomShipAEONum", SqlDbType.VarChar) { Value = customShipAEONum },
                    new SqlParameter("@CustomConsAEONum", SqlDbType.VarChar) { Value = customConsAEONum },
                    new SqlParameter("@CustomConsigneeContactCountry", SqlDbType.VarChar) { Value = customConsigneeContactCountry },
                    new SqlParameter("@customShipContactPerson", SqlDbType.VarChar) { Value = customShipContactPerson },
                    new SqlParameter("@customShipContactTelephone", SqlDbType.VarChar) { Value = customShipContactTelephone },
                    new SqlParameter("@customConsContactPerson", SqlDbType.VarChar) { Value = customConsContactPerson },
                    new SqlParameter("@customConsContactTelephone", SqlDbType.VarChar) { Value = customConsContactTelephone },
                    new SqlParameter("@AgentCASSaddress", SqlDbType.VarChar) { Value = fwbdata.agentCASSaddress },
                    new SqlParameter("@SHPSignature", SqlDbType.VarChar) { Value = shipperSignature },
                    new SqlParameter("@ProductType", SqlDbType.VarChar) { Value = ProductType },
                    new SqlParameter("@OtherServiceInformation", SqlDbType.VarChar) { Value = otherinfostr },
                    new SqlParameter("@NotifyName", SqlDbType.VarChar) { Value = fwbdata.notifyname },
                    new SqlParameter("@NotifyAddress", SqlDbType.VarChar) { Value = fwbdata.notifyadd },
                    new SqlParameter("@NotifyCity", SqlDbType.VarChar) { Value = fwbdata.notifyplace },
                    new SqlParameter("@NotifyState", SqlDbType.VarChar) { Value = fwbdata.notifystate },
                    new SqlParameter("@NotifyCountry", SqlDbType.VarChar) { Value = fwbdata.notifycountrycode },
                    new SqlParameter("@NotifyPincode", SqlDbType.VarChar) { Value = fwbdata.notifypostcode },
                    new SqlParameter("@NotifyTelephone", SqlDbType.VarChar) { Value = fwbdata.notifycontactnum },
                    new SqlParameter("@ScreeningInfo", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@KnownConsiner", SqlDbType.VarChar) { Value = "" }
                };


                // clsLog.WriteLogAzure("FindLog 114 Start spInsertBookingDataFromFFR " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 114 Start spInsertBookingDataFromFFR {0} - {1}", AWBPrefix, awbnum);
                //flag = dtbspInsertBookingDataFromFFR.InsertData(procedure, paramname, paramtype, paramvalue);
                flag = await _readWriteDao.ExecuteNonQueryAsync("spInsertBookingDataFromFFR", sqlParameters);
                // clsLog.WriteLogAzure("FindLog 114 End spInsertBookingDataFromFFR " + AWBPrefix + "-" + awbnum);
                _logger.LogInformation("FindLog 114 End spInsertBookingDataFromFFR {0} - {1}", AWBPrefix, awbnum);


                #endregion Save AWB Details

                if (flag)
                {
                    if (fltroute.Length > 0)
                    {
                        strFlightNo = fltroute[0].carriercode + fltroute[0].fltnum;
                        strFlightOrigin = fltroute[0].fltdept;
                        strFlightDestination = fltroute[0].fltarrival;
                    }
                    //DateTime flightDate = DateTime.UtcNow;
                    try
                    {
                        flightDate = DateTime.ParseExact(flightdate, "dd/MM/yyyy", null);

                    }
                    catch (Exception)
                    {
                        flightDate = DateTime.UtcNow;
                    }

                    //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "Volume" };
                    //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                    ////SQLServer dtbSPAddAWBAuditLog = new SQLServer();
                    //object[] CValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin, fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, strFlightNo, flightDate, strFlightOrigin, strFlightDestination, "Booked", "AWB Booked", "AWB Booked Through FWB", "FWB", DateTime.UtcNow.ToString(), 1, VolumeAmount.Trim() == "" ? "0" : VolumeAmount };
                    //if (!dtbSPAddAWBAuditLog.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues, 600))

                    SqlParameter[] sqlParametersCType = new SqlParameter[]
                    {
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                        new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fwbdata.origin },
                        new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fwbdata.dest },
                        new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                        new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = strFlightNo },
                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightDate },
                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                        new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = strFlightDestination },
                        new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Booked" },
                        new SqlParameter("@Message", SqlDbType.VarChar) { Value = "AWB Booked" },
                        new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Booked Through FWB" },
                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                        new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.UtcNow.ToString() },
                        new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                        new SqlParameter("@Volume", SqlDbType.VarChar) { Value = VolumeAmount.Trim() == "" ? "0" : VolumeAmount }
                    };


                    if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParametersCType, 600))
                        // clsLog.WriteLog("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine);
                        _logger.LogInformation("AWB Audit log  for: {0}", fwbdata.awbnum + Environment.NewLine);
                    //dtbSPAddAWBAuditLog = null;
                    #region Save AWB Routing
                    if ((isUpdateRouteThroughFWB && isAWBPresent) || !isAWBPresent || AWBCreatedAndExecutedBy == "FWB")
                    {
                        string status = "C";
                        if (fltroute.Length > 0)
                        {
                            //string[] parname = new string[] { "AWBNum", "AWBPrefix" };
                            //object[] parobject = new object[] { awbnum, AWBPrefix };
                            //SqlDbType[] partype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                            //SQLServer dtbspDeleteAWBRouteFFR = new SQLServer();

                            SqlParameter[] sqlParametersNum = new SqlParameter[]
                            {
                            new SqlParameter("@AWBNum", SqlDbType.VarChar) { Value = awbnum },
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                            };
                            // clsLog.WriteLogAzure("FindLog 115 Start spDeleteAWBRouteFFR " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 115 Start spDeleteAWBRouteFFR {0} - {1}", AWBPrefix, awbnum);
                            if (await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBRouteFFR", sqlParametersNum))
                            {
                                // clsLog.WriteLogAzure("FindLog 115 End spDeleteAWBRouteFFR " + AWBPrefix + "-" + awbnum);
                                _logger.LogInformation("FindLog 115 End spDeleteAWBRouteFFR {0} - {1}", AWBPrefix, awbnum);
                                //dtbspDeleteAWBRouteFFR = null;
                                for (int lstIndex = 0; lstIndex < fltroute.Length; lstIndex++)
                                {
                                    if (fltroute[lstIndex].fltdept.Trim().ToUpper() != fltroute[lstIndex].fltarrival.Trim().ToUpper())
                                    {
                                        fltroute[lstIndex].flightdate = fltroute[lstIndex].flightdate.Trim() == string.Empty ? DateTime.Now.ToString("MM/dd/yyyy") : fltroute[lstIndex].flightdate;
                                        DateTime dtFlightDate = DateTime.Parse(fltroute[lstIndex].flightdate);

                                        #region Commented Code FWBRoute
                                        //if (fltroute[lstIndex].date != "")
                                        //    dtFlightDate = DateTime.Parse((DateTime.Now.Month.ToString().PadLeft(2, '0') + "/" + fltroute[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString()));

                                        //string dtFltdte = dtFlightDate.ToString("MM/dd/yyyy");
                                        //DataSet dsAWBRflt = new DataSet();

                                        //if (fltroute.Length > 1)
                                        //{
                                        //    if (lstIndex == 0)
                                        //    {
                                        //        dsAWBRflt = ffRMessageProcessor.CheckValidateFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "FWB", fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate, "", REFNo);
                                        //        FltOrg = dsAWBRflt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                                        //        //FltDest = dsAWBRflt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                        //        FltDest = fltroute[lstIndex].fltarrival;
                                        //    }
                                        //    else
                                        //    {
                                        //        FltOrg = fltroute[lstIndex].fltdept;
                                        //        FltDest = fltroute[lstIndex].fltarrival;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    dsAWBRflt = ffRMessageProcessor.CheckValidateFFRMessage(AWBPrefix, awbnum, fwbdata.origin, fwbdata.dest, "FWB", fltroute[lstIndex].fltdept, fltroute[fltroute.Length - 1].fltarrival, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, fltDate, "", REFNo);
                                        //    FltOrg = dsAWBRflt.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                                        //    FltDest = dsAWBRflt.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                                        //}

                                        ////code to check weither flight is valid or not and active
                                        /////Addeed by prashantz to resolve JIRA# CEBV4-1080
                                        //if (fltroute[lstIndex].fltdept.Trim().ToUpper() != fltroute[lstIndex].fltarrival.Trim().ToUpper())
                                        //{
                                        //    string[] parms = new string[]
                                        //    {
                                        //        "FltOrigin",
                                        //        "FltDestination",
                                        //        "FlightNo",
                                        //        "flightDate",
                                        //        "AWBNumber",
                                        //        "AWBPrefix",
                                        //        "RefNo"
                                        //    };
                                        //    SqlDbType[] dataType = new SqlDbType[]
                                        //    {
                                        //        SqlDbType.VarChar,
                                        //        SqlDbType.VarChar,
                                        //        SqlDbType.VarChar,
                                        //        SqlDbType.DateTime,
                                        //        SqlDbType.VarChar,
                                        //        SqlDbType.VarChar,
                                        //        SqlDbType.Int
                                        //    };
                                        //    object[] value = new object[]
                                        //    {

                                        //        //fltroute[lstIndex].fltdept,
                                        //        //fltroute[lstIndex].fltarrival,
                                        //        FltOrg,
                                        //        FltDest,
                                        //        fltroute[lstIndex].carriercode+fltroute[lstIndex].fltnum,
                                        //        dtFlightDate,
                                        //        awbnum,
                                        //        AWBPrefix,
                                        //        REFNo
                                        //    };

                                        //    int schedid = 0;
                                        //    DataSet dsdata = dtb.SelectRecords("Messaging.uspValidateFFRFWBFlight", parms, value, dataType);
                                        //    for (int i = 0; i < dsdata.Tables.Count; i++)
                                        //    {
                                        //        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && Convert.ToInt32(dsdata.Tables[i].Rows[0]["ScheduleID"].ToString()) > 0)
                                        //        {
                                        //            val = true;
                                        //            schedid = Convert.ToInt32(dsdata.Tables[i].Rows[0]["ScheduleID"].ToString());
                                        //        }
                                        //    }
                                        #endregion Commented Code FWBRoute



                                        //string[] paramNames = new string[]
                                        //{
                                        //    "AWBNumber",
                                        //    "FltOrigin",
                                        //    "FltDestination",
                                        //    "FltNumber",
                                        //    "FltDate",
                                        //    "Status",
                                        //    "UpdatedBy",
                                        //    "UpdatedOn",
                                        //    "IsFFR",
                                        //    "REFNo",
                                        //    "date",
                                        //    "AWBPrefix",
                                        //    "carrierCode",
                                        //    "schedid",
                                        //    "voluemcode",
                                        //    "volume"
                                        //};
                                        //SqlDbType[] dataTypes = new SqlDbType[]
                                        //{
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.DateTime,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.DateTime,
                                        //    SqlDbType.Bit,
                                        //    SqlDbType.Int,
                                        //    SqlDbType.DateTime,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.Int,
                                        //    SqlDbType.VarChar,
                                        //    SqlDbType.Decimal
                                        //};

                                        //object[] values = new object[]
                                        //{
                                        //    awbnum,
                                        //    fltroute[lstIndex].fltdept,
                                        //    fltroute[lstIndex].fltarrival,
                                        //    fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum,
                                        //    dtFlightDate,
                                        //    status,
                                        //    "FWB",
                                        //    DateTime.Now,
                                        //    1,
                                        //    REFNo,
                                        //    dtFlightDate,
                                        //    AWBPrefix,
                                        //    fltroute[lstIndex].carriercode,
                                        //    fltroute[lstIndex].scheduleid.Trim() == string.Empty ? "0" : fltroute[lstIndex].scheduleid.Trim(),
                                        //    volcode,
                                        //    VolumeAmount==""?"0":VolumeAmount
                                        //};

                                        SqlParameter[] sqlParametersParam = new SqlParameter[]
                                        {
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                            new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltdept },
                                            new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltarrival },
                                            new SqlParameter("@FltNumber", SqlDbType.VarChar) { Value = fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum },
                                            new SqlParameter("@FltDate", SqlDbType.DateTime) { Value = dtFlightDate },
                                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = status },
                                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                                            new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                            new SqlParameter("@IsFFR", SqlDbType.Bit) { Value = 1 },
                                            new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                                            new SqlParameter("@date", SqlDbType.DateTime) { Value = dtFlightDate },
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                            new SqlParameter("@carrierCode", SqlDbType.VarChar) { Value = fltroute[lstIndex].carriercode },
                                            new SqlParameter("@schedid", SqlDbType.Int) { Value = fltroute[lstIndex].scheduleid.Trim() == string.Empty ? "0" : fltroute[lstIndex].scheduleid.Trim() },
                                            new SqlParameter("@voluemcode", SqlDbType.VarChar) { Value = volcode },
                                            new SqlParameter("@volume", SqlDbType.Decimal) { Value = VolumeAmount==""?"0":VolumeAmount }
                                        };

                                        try
                                        {
                                            decimal VolumeAmountNew = string.IsNullOrWhiteSpace(VolumeAmount) ? 0m : Convert.ToDecimal(VolumeAmount);

                                            // clsLog.WriteLogAzure("Before calling spSaveFFRAWBRoute :");
                                            // clsLog.WriteLogAzure(" awbnum =" + awbnum);
                                            // clsLog.WriteLogAzure(" fltdept =" + fltroute[lstIndex].fltdept);
                                            // clsLog.WriteLogAzure(" fltarrival =" + fltroute[lstIndex].fltarrival);
                                            // clsLog.WriteLogAzure(" carriercode =" + fltroute[lstIndex].carriercode);
                                            // clsLog.WriteLogAzure(" fltnum =" + fltroute[lstIndex].fltnum);
                                            // clsLog.WriteLogAzure(" dtFlightDate =" + dtFlightDate);
                                            // clsLog.WriteLogAzure(" status =" + status);
                                            // clsLog.WriteLogAzure(" REFNo =" + REFNo);
                                            // clsLog.WriteLogAzure(" dtFlightDate =" + dtFlightDate);
                                            // clsLog.WriteLogAzure(" AWBPrefix =" + AWBPrefix);
                                            // clsLog.WriteLogAzure(" carriercode =" + fltroute[lstIndex].carriercode);
                                            // clsLog.WriteLogAzure(" scheduleid =" + (fltroute[lstIndex].scheduleid.Trim() == string.Empty ? "0" : fltroute[lstIndex].scheduleid.Trim()));
                                            // clsLog.WriteLogAzure(" volcode =" + (volcode == string.Empty ? "0" : volcode));
                                            // clsLog.WriteLogAzure(" VolumeAmount =" + VolumeAmountNew);
                                            _logger.LogInformation("Before calling spSaveFFRAWBRoute :");
                                            _logger.LogInformation(" awbnum = {0}", awbnum);
                                            _logger.LogInformation(" fltdept = {0}", fltroute[lstIndex].fltdept);
                                            _logger.LogInformation(" fltarrival = {0}", fltroute[lstIndex].fltarrival);
                                            _logger.LogInformation(" carriercode = {0}", fltroute[lstIndex].carriercode);
                                            _logger.LogInformation(" fltnum = {0}", fltroute[lstIndex].fltnum);
                                            _logger.LogInformation(" dtFlightDate = {0}", dtFlightDate);
                                            _logger.LogInformation(" status ={0}", status);
                                            _logger.LogInformation(" REFNo ={0}", REFNo);
                                            _logger.LogInformation(" dtFlightDate ={0}", dtFlightDate);
                                            _logger.LogInformation(" AWBPrefix ={0}", AWBPrefix);
                                            _logger.LogInformation(" carriercode ={0}", fltroute[lstIndex].carriercode);
                                            _logger.LogInformation(" scheduleid ={0}", fltroute[lstIndex].scheduleid.Trim() == string.Empty ? "0" : fltroute[lstIndex].scheduleid.Trim());
                                            _logger.LogInformation(" volcode ={0}", (volcode == string.Empty ? "0" : volcode));
                                            _logger.LogInformation(" VolumeAmount ={0}", VolumeAmountNew);


                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure("Errorcode 105 in spSaveFFRAWBRoute Parameter Values " + ex.ToString());
                                            _logger.LogError("Errorcode 105 in spSaveFFRAWBRoute Parameter Values {0}", ex);
                                        }

                                        // clsLog.WriteLogAzure("Before calling spSaveFFRAWBRoute awbnum = " + AWBPrefix + "-" + awbnum);
                                        _logger.LogInformation("Before calling spSaveFFRAWBRoute awbnum = {0} - {1}", AWBPrefix, awbnum);
                                        //SQLServer dtbspSaveFFRAWBRoute = new SQLServer();
                                        // clsLog.WriteLogAzure("FindLog 116 Start spSaveFFRAWBRoute " + AWBPrefix + "-" + awbnum);
                                        _logger.LogInformation("FindLog 116 Start spSaveFFRAWBRoute {0} - {1}", AWBPrefix, awbnum);
                                        //if (!dtbspSaveFFRAWBRoute.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))
                                        if (!await _readWriteDao.ExecuteNonQueryAsync("spSaveFFRAWBRoute", sqlParametersParam))
                                            // clsLog.WriteLogAzure("Error in Save AWB Route FWB ");
                                            _logger.LogInformation("Error in Save AWB Route FWB ");
                                        // clsLog.WriteLogAzure("FindLog 116 End spSaveFFRAWBRoute " + AWBPrefix + "-" + awbnum);
                                        _logger.LogInformation("FindLog 116 End spSaveFFRAWBRoute {0} - {1}", AWBPrefix, awbnum);
                                        //dtbspSaveFFRAWBRoute = null;
                                        // clsLog.WriteLogAzure("After called spSaveFFRAWBRoute = " + AWBPrefix + awbnum);
                                        _logger.LogInformation("After called spSaveFFRAWBRoute = {0}", AWBPrefix + awbnum);

                                        //string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "Volume" };
                                        //SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                        //object[] CAValues = new object[] { fwbdata.airlineprefix, fwbdata.awbnum, fwbdata.origin, fwbdata.dest, fwbdata.pcscnt, fwbdata.weight, fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum, dtFlightDate.ToString(), fltroute[lstIndex].fltdept, fltroute[lstIndex].fltarrival, "Booked", "AWB Booked", "AWB Flight Information", "FWB", DateTime.UtcNow.ToString(), 1, VolumeAmount.Trim() == "" ? "0" : VolumeAmount };
                                        //SQLServer dtbSPAddAWBAuditLog1 = new SQLServer();
                                        //if (!dtbSPAddAWBAuditLog1.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))

                                        SqlParameter[] sqlParametersCAValue = new SqlParameter[]
                                        {
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fwbdata.airlineprefix },
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fwbdata.awbnum },
                                            new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fwbdata.origin },
                                            new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fwbdata.dest },
                                            new SqlParameter("@Pieces", SqlDbType.VarChar) { Value = fwbdata.pcscnt },
                                            new SqlParameter("@Weight", SqlDbType.VarChar) { Value = fwbdata.weight },
                                            new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltroute[lstIndex].carriercode + fltroute[lstIndex].fltnum },
                                            new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = dtFlightDate.ToString() },
                                            new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltdept },
                                            new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fltroute[lstIndex].fltarrival },
                                            new SqlParameter("@Action", SqlDbType.VarChar) { Value = "Booked" },
                                            new SqlParameter("@Message", SqlDbType.VarChar) { Value = "AWB Booked" },
                                            new SqlParameter("@Description", SqlDbType.VarChar) { Value = "AWB Flight Information" },
                                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                                            new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = DateTime.UtcNow.ToString() },
                                            new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                                            new SqlParameter("@Volume", SqlDbType.VarChar) { Value = string.IsNullOrWhiteSpace(VolumeAmount) ? "0" : VolumeAmount }
                                        };

                                        if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", sqlParametersCAValue))
                                        {
                                            //clsLog.WriteLogAzure("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine + "Error: " + dtbSPAddAWBAuditLog1.LastErrorDescription);
                                            // clsLog.WriteLogAzure("AWB Audit log  for:" + fwbdata.awbnum + Environment.NewLine);
                                            _logger.LogInformation("AWB Audit log  for: {0}", fwbdata.awbnum + Environment.NewLine);
                                        }
                                        //dtbSPAddAWBAuditLog1 = null;
                                    }
                                }
                                if (val)
                                {
                                    //string[] QueryNames = { "AWBPrefix", "AWBNumber" };
                                    //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                    //object[] QueryValues = { AWBPrefix, awbnum };
                                    //SQLServer dtbspDeleteAWBDetailsNoRoute = new SQLServer();

                                    SqlParameter[] sqlParam = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                                    };

                                    // clsLog.WriteLogAzure("FindLog 117 Start spDeleteAWBDetailsNoRoute " + AWBPrefix + "-" + awbnum);
                                    _logger.LogInformation("FindLog 117 Start spDeleteAWBDetailsNoRoute {0} - {1}", AWBPrefix, awbnum);
                                    //if (!dtbspDeleteAWBDetailsNoRoute.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("spDeleteAWBDetailsNoRoute", sqlParam))
                                    {
                                        // clsLog.WriteLogAzure("FindLog 117 End spDeleteAWBDetailsNoRoute " + AWBPrefix + "-" + awbnum);
                                        _logger.LogInformation("FindLog 117 End spDeleteAWBDetailsNoRoute {0} - {1}", AWBPrefix, awbnum);
                                        //clsLog.WriteLogAzure("Error in Deleting AWB Details.... " + dtbspDeleteAWBDetailsNoRoute.LastErrorDescription);
                                        // clsLog.WriteLogAzure("Error in Deleting AWB Details.... ");
                                        _logger.LogInformation("Error in Deleting AWB Details.... ");
                                    }
                                    //dtbspDeleteAWBDetailsNoRoute = null;
                                }

                            }
                        }
                    }
                    #endregion Save AWB Routing

                    if (val)
                    {
                        #region Rate Decription
                        string freight = "0", paymode = "PX", valcharge = "0", tax = "0", OCDA = "0", OCDC = "0", total = "0", currency = string.Empty, insuranceamount = "0";
                        decimal DeclareCarriageValue = 0, DeclareCustomValue = 0;
                        bool DimsFlag = true;
                        currency = fwbdata.currency;
                        paymode = fwbdata.chargecode == "" ? fwbdata.chargedec : fwbdata.chargecode;

                        if (fwbdata.declaredvalue != "")
                        {
                            DeclareCarriageValue = decimal.Parse(fwbdata.declaredvalue == "NVD" ? "0" : fwbdata.declaredvalue);
                        }
                        if (fwbdata.declaredcustomvalue != "")
                        {
                            DeclareCustomValue = decimal.Parse(fwbdata.declaredcustomvalue == "NCV" ? "0" : fwbdata.declaredcustomvalue);
                        }
                        if (fwbdata.insuranceamount != "")
                        {
                            insuranceamount = fwbdata.insuranceamount.Trim() == "" ? "0" :
                                 (fwbdata.insuranceamount.Trim() == "X" || fwbdata.insuranceamount.Trim() == "XX" || fwbdata.insuranceamount.Trim() == "XXX") ? "0" : fwbdata.insuranceamount;
                        }


                        if (fwbdata.PPweightCharge.Length > 0 || fwbdata.PPValuationCharge.Length > 0 || fwbdata.PPTaxesCharge.Length > 0 ||
                            fwbdata.PPOCDA.Length > 0 || fwbdata.PPOCDC.Length > 0 || fwbdata.PPTotalCharges.Length > 0)
                        {
                            freight = fwbdata.PPweightCharge.Length > 0 ? fwbdata.PPweightCharge : "0";
                            valcharge = fwbdata.PPValuationCharge.Length > 0 ? fwbdata.PPValuationCharge : "0";
                            tax = fwbdata.PPTaxesCharge.Length > 0 ? fwbdata.PPTaxesCharge : "0";
                            OCDC = fwbdata.PPOCDC.Length > 0 ? fwbdata.PPOCDC : "0";
                            OCDA = fwbdata.PPOCDA.Length > 0 ? fwbdata.PPOCDA : "0";
                            total = fwbdata.PPTotalCharges.Length > 0 ? fwbdata.PPTotalCharges : "0";
                        }

                        if (fwbdata.CCweightCharge.Length > 0 || fwbdata.CCValuationCharge.Length > 0 || fwbdata.CCTaxesCharge.Length > 0 ||
                            fwbdata.CCOCDA.Length > 0 || fwbdata.CCOCDC.Length > 0 || fwbdata.CCTotalCharges.Length > 0)
                        {
                            freight = fwbdata.CCweightCharge.Length > 0 ? fwbdata.CCweightCharge : "0";
                            valcharge = fwbdata.CCValuationCharge.Length > 0 ? fwbdata.CCValuationCharge : "0";
                            tax = fwbdata.CCTaxesCharge.Length > 0 ? fwbdata.CCTaxesCharge : "0";
                            OCDC = fwbdata.CCOCDC.Length > 0 ? fwbdata.CCOCDC : "0";
                            OCDA = fwbdata.CCOCDA.Length > 0 ? fwbdata.CCOCDA : "0";
                            total = fwbdata.CCTotalCharges.Length > 0 ? fwbdata.CCTotalCharges : "0";
                        }

                        for (int i = 0; i < fwbrates.Length; i++)
                        {
                            fwbrates[i].chargeamt = fwbrates[i].chargeamt.Length > 0 ? fwbrates[i].chargeamt : "0";
                            fwbrates[i].awbweight = fwbrates[i].awbweight.Length > 0 ? fwbrates[i].awbweight : "0";
                            fwbrates[i].weight = fwbrates[i].weight.Length > 0 ? fwbrates[i].weight : "0";
                            fwbrates[i].chargerate = fwbrates[i].chargerate.Length > 0 ? fwbrates[i].chargerate : freight;
                            //fwbrates[i].rateclasscode = fwbrates[i].rateclasscode;

                            if (fwbrates[i].awbweight.Length > 1)
                                Priority = "RTW";
                            else if (objDimension.Length > 0)
                                Priority = "DIMS";
                            else if (fwbrates[i].volamt.Length > 0)
                                Priority = "Volume";
                            else if (float.Parse(fwbdata.weight) > 1)
                                Priority = "GrossWt";

                            switch (Priority)
                            {
                                case "RTW":
                                    ChargeableWeight = Convert.ToDecimal(fwbrates[i].awbweight);
                                    break;
                                case "DIMS":
                                    ChargeableWeight = 0;
                                    break;
                                //case "Volume": // Un-Used code
                                //    ChargeableWeight.ToString();
                                //    break;
                                case "GrossWt":
                                    ChargeableWeight = Convert.ToDecimal(fwbdata.weight);
                                    break;

                            }

                            // string[] param = new string[]
                            // {
                            // "AWBNumber",
                            // "CommCode",
                            // "PayMode",
                            // "Pcs",
                            // "Wt",
                            // "FrIATA",
                            // "FrMKT",
                            // "ValCharge",
                            // "OcDueCar",
                            // "OcDueAgent",
                            // "SpotRate",
                            // "DynRate",
                            // "ServiceTax",
                            // "Total",
                            // "RatePerKg",
                            // "Currency",
                            // "AWBPrefix",
                            // "ChargeableWeight",
                            // "DeclareCarriageValue",
                            // "DeclareCustomValue",
                            // "RateClass",
                            // "insuranceamount"
                            // };
                            // SqlDbType[] dbtypes = new SqlDbType[]
                            // {
                            // SqlDbType.VarChar,
                            // SqlDbType.VarChar,
                            // SqlDbType.VarChar,
                            // SqlDbType.Int,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Decimal,
                            // SqlDbType.VarChar,
                            // SqlDbType.VarChar,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.Float,
                            // SqlDbType.VarChar,
                            // SqlDbType.Float,
                            // };
                            // object[] values = new object[]
                            // {
                            // awbnum,
                            // commcode,
                            // paymode,
                            // Convert.ToInt16(fwbrates[i].numofpcs),
                            // float.Parse(fwbrates[i].weight),
                            // float.Parse(freight),
                            // float.Parse(fwbrates[i].chargeamt),

                            // float.Parse(valcharge),
                            // float.Parse(OCDC),
                            // float.Parse(OCDA),
                            // 0,
                            // 0,
                            // float.Parse(tax),
                            // float.Parse(total),
                            // Convert.ToDecimal(fwbrates[i].chargerate),
                            // currency,
                            // AWBPrefix,
                            ////float.Parse(fwbrates[i].awbweight),
                            // ChargeableWeight,
                            // DeclareCarriageValue,
                            // DeclareCustomValue,
                            // fwbrates[i].rateclasscode,
                            // float.Parse(insuranceamount)
                            // };

                            SqlParameter[] sqlParametersAWBNumber = new SqlParameter[]
                            {
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                new SqlParameter("@CommCode", SqlDbType.VarChar) { Value = commcode },
                                new SqlParameter("@PayMode", SqlDbType.VarChar) { Value = paymode },
                                new SqlParameter("@Pcs", SqlDbType.Int) { Value = Convert.ToInt16(fwbrates[i].numofpcs) },
                                new SqlParameter("@Wt", SqlDbType.Float) { Value = float.Parse(fwbrates[i].weight) },
                                new SqlParameter("@FrIATA", SqlDbType.Float) { Value = float.Parse(freight) },
                                new SqlParameter("@FrMKT", SqlDbType.Float) { Value = float.Parse(fwbrates[i].chargeamt) },
                                new SqlParameter("@ValCharge", SqlDbType.Float) { Value = float.Parse(valcharge) },
                                new SqlParameter("@OcDueCar", SqlDbType.Float) { Value = float.Parse(OCDC) },
                                new SqlParameter("@OcDueAgent", SqlDbType.Float) { Value = float.Parse(OCDA) },
                                new SqlParameter("@SpotRate", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@DynRate", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@ServiceTax", SqlDbType.Float) { Value = float.Parse(tax) },
                                new SqlParameter("@Total", SqlDbType.Float) { Value = float.Parse(total) },
                                new SqlParameter("@RatePerKg", SqlDbType.Decimal) { Value = Convert.ToDecimal(fwbrates[i].chargerate) },
                                new SqlParameter("@Currency", SqlDbType.VarChar) { Value = currency },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@ChargeableWeight", SqlDbType.Float) { Value = ChargeableWeight },
                                new SqlParameter("@DeclareCarriageValue", SqlDbType.Float) { Value = DeclareCarriageValue },
                                new SqlParameter("@DeclareCustomValue", SqlDbType.Float) { Value = DeclareCustomValue },
                                new SqlParameter("@RateClass", SqlDbType.VarChar) { Value = fwbrates[i].rateclasscode },
                                new SqlParameter("@insuranceamount", SqlDbType.Float) { Value = float.Parse(insuranceamount) }
                            };

                            //SQLServer dtbSP_SaveAWBRatesviaMsg = new SQLServer();
                            // clsLog.WriteLogAzure("FindLog 118 Start SP_SaveAWBRatesviaMsg " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 118 Start SP_SaveAWBRatesviaMsg {0} - {1}", AWBPrefix, awbnum);
                            //if (!dtbSP_SaveAWBRatesviaMsg.UpdateData("SP_SaveAWBRatesviaMsg", param, dbtypes, values))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBRatesviaMsg", sqlParametersAWBNumber))
                            {
                                // clsLog.WriteLogAzure("FindLog 118 End SP_SaveAWBRatesviaMsg " + AWBPrefix + "-" + awbnum);
                                // clsLog.WriteLogAzure("Error Saving FWB rates for:" + awbnum);
                                _logger.LogInformation("FindLog 118 End SP_SaveAWBRatesviaMsg {0} - {1}", AWBPrefix, awbnum);
                                _logger.LogInformation("Error Saving FWB rates for:{0}", awbnum);
                            }
                            //dtbSP_SaveAWBRatesviaMsg = null;

                        }

                        #endregion

                        #region Other Charges
                        //check for other charge exists in systme or not


                        for (int i = 0; i < OtherCharges.Length; i++)
                        {
                            //string[] param = { "AWBNumber", "ChargeHeadCode", "ChargeType", "DiscountPercent",
                            //           "CommPercent", "TaxPercent", "Discount", "Comission", "Tax","Charge","CommCode","AWBPrefix"};
                            //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Float,
                            //                SqlDbType.Float, SqlDbType.Float, SqlDbType.Float, SqlDbType.Float, SqlDbType.Float,SqlDbType.Float,SqlDbType.VarChar,SqlDbType.VarChar};

                            //object[] values = { awbnum, OtherCharges[i].otherchargecode, "D" + OtherCharges[i].entitlementcode, 0, 0, 0, 0, 0, 0, OtherCharges[i].chargeamt, commcode, AWBPrefix };

                            //SQLServer dtbSP_SaveAWBOCRatesDetails = new SQLServer();

                            SqlParameter[] sqlParametersChargeType = new SqlParameter[]
                            {
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                new SqlParameter("@ChargeHeadCode", SqlDbType.VarChar) { Value = OtherCharges[i].otherchargecode },
                                new SqlParameter("@ChargeType", SqlDbType.VarChar) { Value = "D" + OtherCharges[i].entitlementcode },
                                new SqlParameter("@DiscountPercent", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@CommPercent", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@TaxPercent", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@Discount", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@Comission", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@Tax", SqlDbType.Float) { Value = 0 },
                                new SqlParameter("@Charge", SqlDbType.Float) { Value = OtherCharges[i].chargeamt },
                                new SqlParameter("@CommCode", SqlDbType.VarChar) { Value = commcode },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix }
                            };


                            // clsLog.WriteLogAzure("FindLog 119 Start SP_SaveAWBOCRatesDetails " + AWBPrefix + "-" + awbnum);
                            _logger.LogInformation("FindLog 119 Start SP_SaveAWBOCRatesDetails {0} - {1}", AWBPrefix, awbnum);
                            //if (!dtbSP_SaveAWBOCRatesDetails.InsertData("SP_SaveAWBOCRatesDetails", param, dbtypes, values))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBOCRatesDetails", sqlParametersChargeType))
                            {
                                // clsLog.WriteLogAzure("FindLog 119 End SP_SaveAWBOCRatesDetails " + AWBPrefix + "-" + awbnum);
                                // clsLog.WriteLogAzure("Error Saving FWB OCRates for:" + awbnum);
                                _logger.LogInformation("FindLog 119 End SP_SaveAWBOCRatesDetails {0} - {1}", AWBPrefix, awbnum);
                                _logger.LogInformation("Error Saving FWB OCRates for:{0}", awbnum);
                            }
                            //dtbSP_SaveAWBOCRatesDetails = null;

                        }
                        #endregion

                        #region AWB Dimensions
                        bool isDeleteDimsForBUP = true, isMultipleBUPs = false, isBUPValid = true;

                        if (objDimension.Length > 0)
                        {
                            DimsFlag = false;
                            #region : Multiple BUPs Check :
                            if (objAWBBup.Length > 0)
                            {
                                for (int k = 0; k < objAWBBup.Length; k++)
                                {
                                    if (objAWBBup[k].ULDNo == "" || objAWBBup[k].ULDNo == null && objAWBBup[k].SlacCount != "1")
                                        isBUPValid = false;
                                }
                                if (objAWBBup.Length > 1 && isBUPValid && objDimension.Length == objAWBBup.Length)
                                    isMultipleBUPs = true;
                            }
                            #endregion Multiple BUPs Check

                            isDeleteDimsForBUP = false;
                            decimal totalDimsWt = 0;
                            if (objDimension.Length > 0)
                            {
                                for (int j = 0; j < objDimension.Length; j++)
                                {
                                    totalDimsWt = totalDimsWt + Convert.ToDecimal(objDimension[j].weight.Trim() == string.Empty ? "0" : objDimension[j].weight.Trim());
                                }
                            }

                            if (totalDimsWt == Convert.ToDecimal(fwbdata.weight))
                            {
                                isUpdateDIMSWeight = true;
                            }
                            //Badiuz khan
                            //Description: Delete Dimension if Dimension 
                            //string[] dparam = { "AWBPrefix", "AWBNumber" };
                            //SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                            //object[] dbparamvalues = { AWBPrefix, awbnum };

                            //SQLServer dtbSpDeleteDimensionThroughMessage = new SQLServer();

                            SqlParameter[] sqlParametersDeleteDimension = new SqlParameter[]
                            {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum }
                            };

                            //if (!dtbSpDeleteDimensionThroughMessage.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("SpDeleteDimensionThroughMessage", sqlParametersDeleteDimension))
                            {
                                // clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                                _logger.LogWarning("Error  Delete Dimension Through Message :", awbnum);
                                //dtbSpDeleteDimensionThroughMessage = null;
                            }
                            else
                            {
                                string bupULDNumber = string.Empty;
                                for (int i = 0; i < objDimension.Length; i++)
                                {
                                    bupULDNumber = string.Empty;
                                    if (isMultipleBUPs)
                                        bupULDNumber = objAWBBup[i].ULDNo;

                                    if (objDimension[i].mesurunitcode.Trim() != "")
                                    {
                                        if (objDimension[i].mesurunitcode.Trim().ToUpper() == "CMT")
                                        {
                                            objDimension[i].mesurunitcode = "Cms";
                                        }
                                        else if (objDimension[i].mesurunitcode.Trim().ToUpper() == "INH")
                                        {
                                            objDimension[i].mesurunitcode = "Inches";
                                        }
                                    }
                                    if (objDimension[i].length.Trim() == "")
                                    {
                                        objDimension[i].length = "0";
                                    }
                                    if (objDimension[i].width.Trim() == "")
                                    {
                                        objDimension[i].width = "0";
                                    }
                                    if (objDimension[i].height.Trim() == "")
                                    {
                                        objDimension[i].height = "0";
                                    }

                                    if (!isUpdateDIMSWeight)
                                    {
                                        if (i == 0)
                                        {
                                            objDimension[i].weight = fwbdata.weight;
                                        }
                                        else
                                        {
                                            objDimension[i].weight = "0";
                                        }
                                    }
                                    if (bupULDNumber == "")
                                    {
                                        bupULDNumber = objDimension[i].UldNo;
                                    }
                                    Decimal DimWeight = 0;
                                    //string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "WeightCode"
                                    //        , "UpdatedBy", "ChargeableWeightPriority", "ULDNumber","PieceType" };
                                    //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar
                                    //        , SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar };
                                    //object[] value ={awbnum,"1",objDimension[i].length,objDimension[i].width,objDimension[i].height, objDimension[i].piecenum, objDimension[i].mesurunitcode, AWBPrefix, Decimal.TryParse(objDimension[i].weight, out DimWeight)==true?Convert.ToDecimal(objDimension[i].weight):0, objDimension[i].weightcode
                                    //        , "FWB", Priority, bupULDNumber, objDimension[i].PieceType};

                                    //SQLServer dtbSP_SaveAWBDimensions_FFR = new SQLServer();

                                    SqlParameter[] sqlParametersSaveAWB = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@RowIndex", SqlDbType.Int) { Value = "1" },
                                        new SqlParameter("@Length", SqlDbType.Int) { Value = objDimension[i].length },
                                        new SqlParameter("@Breadth", SqlDbType.Int) { Value = objDimension[i].width },
                                        new SqlParameter("@Height", SqlDbType.Int) { Value = objDimension[i].height },
                                        new SqlParameter("@PcsCount", SqlDbType.Int) { Value = objDimension[i].piecenum },
                                        new SqlParameter("@MeasureUnit", SqlDbType.VarChar) { Value = objDimension[i].mesurunitcode },
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@Weight", SqlDbType.Decimal) { Value = Decimal.TryParse(objDimension[i].weight, out DimWeight) ? Convert.ToDecimal(objDimension[i].weight) : 0 },
                                        new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = objDimension[i].weightcode },
                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                                        new SqlParameter("@ChargeableWeightPriority", SqlDbType.VarChar) { Value = Priority },
                                        new SqlParameter("@ULDNumber", SqlDbType.VarChar) { Value = bupULDNumber },
                                        new SqlParameter("@PieceType", SqlDbType.VarChar) { Value = objDimension[i].PieceType }
                                    };

                                    // clsLog.WriteLogAzure("FindLog 120 Start SP_SaveAWBDimensions_FFR " + AWBPrefix + "-" + awbnum);
                                    _logger.LogInformation("FindLog 120 Start SP_SaveAWBDimensions_FFR {0} - {1}", AWBPrefix, awbnum);
                                    //if (!dtbSP_SaveAWBDimensions_FFR.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SP_SaveAWBDimensions_FFR", sqlParametersSaveAWB))
                                    {
                                        // clsLog.WriteLogAzure("FindLog 120 End SP_SaveAWBDimensions_FFR " + AWBPrefix + "-" + awbnum);
                                        // clsLog.WriteLogAzure("Error Saving  Dimension Through Message :" + awbnum);
                                        _logger.LogWarning("FindLog 120 End SP_SaveAWBDimensions_FFR {0} - {1}", AWBPrefix, awbnum);
                                        _logger.LogWarning("Error Saving  Dimension Through Message : {0}", awbnum);
                                    }
                                    //dtbSP_SaveAWBDimensions_FFR = null;
                                }
                            }
                        }

                        #endregion

                        #region FWB Message with BUP Shipment
                        //Badiuz khan
                        //Description: Save Bup through FWB
                        // decimal VolumeWt = 0;
                        if (objAWBBup.Length > 0)
                        {
                            if (fwbrates[0].volcode != "")
                            {
                                switch (fwbrates[0].volcode.ToUpper())
                                {
                                    case "MC":
                                        VolumeWt = decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(fwbrates[0].volamt == "" ? "0" : fwbrates[0].volamt) * decimal.Parse("166.66"))));
                                        break;
                                    default:
                                        VolumeWt = Convert.ToDecimal(fwbrates[0].volamt == "" ? "0" : fwbrates[0].volamt);
                                        break;
                                }
                            }

                            for (int k = 0; k < objAWBBup.Length; k++)
                            {
                                if (objAWBBup[k].ULDNo != "" && objAWBBup[k].ULDNo != null)
                                {
                                    string uldno = objAWBBup[k].ULDNo;
                                    int uldslacPcs = int.Parse(objAWBBup[k].SlacCount == "" ? "0" : objAWBBup[k].SlacCount);
                                    //string[] param = { "AWBPrefix", "AWBNumber", "ULDNo", "SlacPcs", "PcsCount", "Volume", "GrossWeight", "isDeleteDimsForBUP", "IsMultipleBUPs" };
                                    //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Bit, SqlDbType.Bit };
                                    //object[] value = { AWBPrefix, awbnum, uldno, uldslacPcs, fwbdata.pcscnt, VolumeWt, decimal.Parse(fwbdata.weight == "" ? "0" : fwbdata.weight), isDeleteDimsForBUP, isMultipleBUPs };

                                    //SQLServer dtbSaveandUpdateShippperBUPThroughFWB = new SQLServer();

                                    SqlParameter[] sqlParametersSaveandUpdateShipppe = new SqlParameter[]
                                    {
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = uldno },
                                        new SqlParameter("@SlacPcs", SqlDbType.Int) { Value = uldslacPcs },
                                        new SqlParameter("@PcsCount", SqlDbType.Int) { Value = fwbdata.pcscnt },
                                        new SqlParameter("@Volume", SqlDbType.Decimal) { Value = VolumeWt },
                                        new SqlParameter("@GrossWeight", SqlDbType.Decimal) { Value = decimal.Parse(string.IsNullOrEmpty(fwbdata.weight) ? "0" : fwbdata.weight) },
                                        new SqlParameter("@isDeleteDimsForBUP", SqlDbType.Bit) { Value = isDeleteDimsForBUP },
                                        new SqlParameter("@IsMultipleBUPs", SqlDbType.Bit) { Value = isMultipleBUPs }
                                    };

                                    // clsLog.WriteLogAzure("FindLog 121 Start SaveandUpdateShippperBUPThroughFWB " + AWBPrefix + "-" + awbnum);
                                    _logger.LogInformation("FindLog 121 Start SaveandUpdateShippperBUPThroughFWB {0} - {1}", AWBPrefix, awbnum);
                                    //if (!dtbSaveandUpdateShippperBUPThroughFWB.InsertData("SaveandUpdateShippperBUPThroughFWB", param, dbtypes, value))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SaveandUpdateShippperBUPThroughFWB", sqlParametersSaveandUpdateShipppe))
                                    {
                                        // clsLog.WriteLogAzure("FindLog 121 End SaveandUpdateShippperBUPThroughFWB " + AWBPrefix + "-" + awbnum);
                                        _logger.LogWarning("FindLog 121 End SaveandUpdateShippperBUPThroughFWB {0} - {1}", AWBPrefix, awbnum);
                                        //string str = dtbSaveandUpdateShippperBUPThroughFWB.LastErrorDescription.ToString();
                                        //clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + awbnum + Environment.NewLine + "Error : " + dtbSaveandUpdateShippperBUPThroughFWB.LastErrorDescription);
                                        // clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + awbnum + Environment.NewLine);
                                        _logger.LogWarning("BUP ULD is not Updated  for:{0}", awbnum + Environment.NewLine);
                                    }
                                    //dtbSaveandUpdateShippperBUPThroughFWB = null;
                                }
                            }
                        }

                        #endregion

                        #region : Update audit trail volume :
                        //string[] QueryNames = { "AWBPrefix", "AWBNumber", "UpdatedBy" };
                        //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                        //object[] QueryValues = { AWBPrefix, awbnum, "FWB" };
                        //SQLServer dtbuspUpdateAuditLogVolume = new SQLServer();

                        SqlParameter[] sqlParametersUpdateAudit = new SqlParameter[]
                        {
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" }
                        };

                        //if (!dtbuspUpdateAuditLogVolume.UpdateData("Messaging.uspUpdateAuditLogVolume", QueryNames, QueryTypes, QueryValues))
                        if (!await _readWriteDao.ExecuteNonQueryAsync("Messaging.uspUpdateAuditLogVolume", sqlParametersUpdateAudit))

                        {
                            //clsLog.WriteLogAzure("Error while Update Volume In AuditLog " + dtbuspUpdateAuditLogVolume.LastErrorDescription);
                            // clsLog.WriteLogAzure("Error while Update Volume In AuditLog ");
                            _logger.LogWarning("Error while Update Volume In AuditLog ");
                            //dtbuspUpdateAuditLogVolume = null;
                        }
                        #endregion Update audit trail volume

                        #region ProcessRateFunction

                        DataSet? dsrateCheck = await _fFRMessageProcessor.CheckAirlineForRateProcessing(AWBPrefix, "FWB");
                        if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
                        {
                            //string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
                            //SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                            //object[] CRValues = new object[] { awbnum, AWBPrefix, "FWB", System.DateTime.Now, 1, 1, "B", 0 };
                            ////if (!dtb.ExecuteProcedure("sp_CalculateFreightChargesforMessage", "AWBNumber", SqlDbType.VarChar, awbnum))
                            //SQLServer dtbsp_CalculateAWBRatesReprocess = new SQLServer();
                            SqlParameter[] sqlParametersCalculateAWB = new SqlParameter[]
                            {
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" },
                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                new SqlParameter("@ValidateMin", SqlDbType.Bit) { Value = 1 },
                                new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = 1 },
                                new SqlParameter("@RouteFrom", SqlDbType.VarChar) { Value = "B" },
                                new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = 0 }
                            };

                            // clsLog.WriteLogAzure("FindLog 122 Start sp_CalculateAWBRatesReprocess " + AWBPrefix + "-" + awbnum);
                            _logger.LogWarning("FindLog 122 Start sp_CalculateAWBRatesReprocess {0} - {1}", AWBPrefix, awbnum);
                            //if (!dtbsp_CalculateAWBRatesReprocess.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
                            if (!await _readWriteDao.ExecuteNonQueryAsync("sp_CalculateAWBRatesReprocess", sqlParametersCalculateAWB))
                            {
                                // clsLog.WriteLogAzure("FindLog 122 End sp_CalculateAWBRatesReprocess " + AWBPrefix + "-" + awbnum);
                                _logger.LogWarning("FindLog 122 End sp_CalculateAWBRatesReprocess {0} - {1}", AWBPrefix, awbnum);
                                //clsLog.WriteLogAzure("Rates Not Calculated for:" + awbnum + Environment.NewLine + "Error: " + dtbsp_CalculateAWBRatesReprocess.LastErrorDescription);
                                // clsLog.WriteLogAzure("Rates Not Calculated for:" + awbnum + Environment.NewLine);
                                _logger.LogWarning("Rates Not Calculated for: {0}", awbnum, Environment.NewLine);
                                //dtbsp_CalculateAWBRatesReprocess = null;
                            }
                        }

                        #endregion


                        //string[] QueryName = { "AWBNumber", "Status", "AWBPrefix", "UserName", "AWBIssueDate", "ChargeableWeight", "Priority", "REFNo", "ShipperName", "DimsFlag" };
                        //SqlDbType[] QueryType = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Bit };
                        //object[] QueryValue = { awbnum, "E", AWBPrefix, "FWB", strAWbIssueDate, ChargeableWeight, Priority, REFNo, fwbdata.shippername.Trim(' '), DimsFlag };
                        //SQLServer dtbUpdateStatustoExecuted = new SQLServer();

                        SqlParameter[] sqlParametersUpdateStatus = new SqlParameter[]
                        {
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                            new SqlParameter("@Status", SqlDbType.VarChar) { Value = "E" },
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                            new SqlParameter("@UserName", SqlDbType.VarChar) { Value = "FWB" },
                            new SqlParameter("@AWBIssueDate", SqlDbType.DateTime) { Value = strAWbIssueDate },
                            new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                            new SqlParameter("@Priority", SqlDbType.VarChar) { Value = Priority },
                            new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                            new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fwbdata.shippername.Trim(' ') },
                            new SqlParameter("@DimsFlag", SqlDbType.Bit) { Value = DimsFlag }
                        };


                        // clsLog.WriteLogAzure("FindLog 123 Start sp_CalculateAWBRatesReprocess " + AWBPrefix + "-" + awbnum);
                        _logger.LogWarning("FindLog 123 Start sp_CalculateAWBRatesReprocess {0} - {1}", AWBPrefix, awbnum);
                        //if (!dtbUpdateStatustoExecuted.UpdateData("UpdateStatustoExecuted", QueryName, QueryType, QueryValue))
                        if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateStatustoExecuted", sqlParametersUpdateStatus))
                        {
                            // clsLog.WriteLogAzure("FindLog 123 End sp_CalculateAWBRatesReprocess " + AWBPrefix + "-" + awbnum);
                            _logger.LogWarning("FindLog 123 End sp_CalculateAWBRatesReprocess {0} - {1}", AWBPrefix, awbnum);
                            //clsLog.WriteLogAzure("Error in updating AWB status" + dtbUpdateStatustoExecuted.LastErrorDescription);
                            // clsLog.WriteLogAzure("Error in updating AWB status");
                            _logger.LogWarning("Error in updating AWB status");
                        }
                        //dtbUpdateStatustoExecuted = null;

                        ///MasterLog
                        GenericFunction gf = new GenericFunction();
                        DataSet dsAWBMaterLogNewValues = new DataSet();
                        // clsLog.WriteLogAzure("FindLog 124 Start GetAWBMasterLogNewRecord " + AWBPrefix + "-" + awbnum);
                        _logger.LogWarning("FindLog 124 Start GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);
                        dsAWBMaterLogNewValues = gf.GetAWBMasterLogNewRecord(AWBPrefix, awbnum);
                        // clsLog.WriteLogAzure("FindLog 124 End GetAWBMasterLogNewRecord " + AWBPrefix + "-" + awbnum);
                        _logger.LogWarning("FindLog 124 End GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);
                        if (dsAWBMaterLogNewValues != null && dsAWBMaterLogNewValues.Tables.Count > 0 && dsAWBMaterLogNewValues.Tables[0].Rows.Count > 0)
                        {
                            DataTable dtMasterAuditLog = new DataTable();
                            DataTable dtOldValues = new DataTable();
                            DataTable dtNewValues = new DataTable();
                            if (dsAWBMaterLogOldValues != null && dsAWBMaterLogOldValues.Tables.Count > 0 && dsAWBMaterLogOldValues.Tables[0].Rows.Count > 0)
                                dtOldValues = dsAWBMaterLogOldValues.Tables[0];
                            else
                                dtOldValues = null;
                            dtNewValues = dsAWBMaterLogNewValues.Tables[0];
                            gf.MasterAuditLog(dtOldValues, dtNewValues, AWBPrefix, awbnum, "Save", "FWB", System.DateTime.Now);
                            // clsLog.WriteLogAzure("FindLog 124_2 End GetAWBMasterLogNewRecord  " + AWBPrefix + "-" + awbnum);
                            _logger.LogWarning("FindLog 124_2 End GetAWBMasterLogNewRecord {0} - {1}", AWBPrefix, awbnum);

                        }

                        #region capacity
                        //string[] cparam = { "AWBPrefix", "AWBNumber", "UpdatedBy" };
                        //SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                        //object[] cparamvalues = { AWBPrefix, awbnum, "FWB" };

                        SqlParameter[] sqlParametersCapacity = new SqlParameter[]
                        {
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FWB" }
                        };

                        //SQLServer dtbUpdateCapacitythroughMessage = new SQLServer();
                        // clsLog.WriteLogAzure("FindLog 125 Start UpdateCapacitythroughMessage " + AWBPrefix + "-" + awbnum);
                        _logger.LogWarning("FindLog 125 Start UpdateCapacitythroughMessage {0} - {1}", AWBPrefix, awbnum);
                        //if (!dtbUpdateCapacitythroughMessage.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))
                        if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateCapacitythroughMessage", sqlParametersCapacity))
                        {
                            // clsLog.WriteLogAzure("Error  on Update capacity Plan :" + awbnum);
                            _logger.LogWarning("Error  on Update capacity Plan :{0}", awbnum);
                        }
                        // clsLog.WriteLogAzure("FindLog 125 End UpdateCapacitythroughMessage " + AWBPrefix + "-" + awbnum);
                        _logger.LogWarning("FindLog 125 End UpdateCapacitythroughMessage {0} - {1}", AWBPrefix, awbnum);
                        //dtbUpdateCapacitythroughMessage = null;

                        #endregion
                    }
                }
                else
                {
                    //clsLog.WriteLogAzure("Error while save FWB Message:" + awbnum + "-" + dtbspInsertBookingDataFromFFR.LastErrorDescription);
                    // clsLog.WriteLogAzure("Error while save FWB Message:" + awbnum);
                    _logger.LogWarning("Error while save FWB Message:{0}", awbnum);

                }
                //dtbspInsertBookingDataFromFFR = null;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure("FindLog Missing route Log Error 999 :- SaveandValidateFWBMessage ");
                // clsLog.WriteLogAzure(ex);
                _logger.LogError("FindLog Missing route Log Error 999 :- SaveandValidateFWBMessage ");
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ErrorMsg = string.Empty;
                flag = false;
            }
            return (flag, ErrorMsg);
        }

        /// <summary>
        /// Method to select the records(Data) to generate the FWB message
        /// </summary>
        public async Task<DataSet> GetAWBRecordForGenerateFWBMessage(string strAWBNumber, string strAwbPrefix)
        {
            DataSet? dssitaMessage = new DataSet();
            try
            {
                //SQLServer da = new SQLServer(true);
                //string[] paramname = new string[] { "AWBNumber", "AWBPrefix" };
                //object[] paramvalue = new object[] { strAWBNumber, strAwbPrefix };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //dssitaMessage = da.SelectRecords("SP_GetAWBRecordForFWB", paramname, paramvalue, paramtype);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = strAWBNumber },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = strAwbPrefix }
                };
                dssitaMessage = await _readWriteDao.SelectRecords("SP_GetAWBRecordForFWB", sqlParameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                dssitaMessage = null;
            }
            return dssitaMessage;
        }

        /// <summary>
        /// Method generate the FWB for all the AWB's form the flight
        /// Method added by prashant
        /// </summary>
        public async Task GenerateFWB(string PartnerCode, string DepartureAirport, string ArrivalAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate, string AWBNumbers)
        {
            string FlightDestination = string.Empty;
            GenericFunction genericFunction = new GenericFunction();
            string MessageVersion = "8", SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, WEBAPIAddress = string.Empty, WebAPIURL = string.Empty;
            DataSet dsData = new DataSet();

            string[] awbArrray = AWBNumbers.Split(',');
            for (int i = 0; i < awbArrray.Length; i++)
            {
                DataSet? dsfwb = await GetAWBRecordForGenerateFWBMessage(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString());

                string awbDestination = dsfwb.Tables[0].Rows[0]["DestinationCode"].ToString();
                awbDestination = awbDestination + "," + ArrivalAirport.Trim();
                string awbOrigin = dsfwb.Tables[0].Rows[0]["OriginCode"].ToString();
                string SFTPHeaderSITAddress = string.Empty;
                DataSet dsconfiguration = genericFunction.GetSitaAddressandMessageVersionForAutoMessage(PartnerCode, "FWB", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty, string.Empty, string.Empty);
                if (dsconfiguration != null && dsconfiguration.Tables[0].Rows.Count > 0)
                {
                    strEmailid = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                    MessageVersion = dsconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                    strSITAHeaderType = dsconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString();
                    WebAPIURL = dsconfiguration.Tables[0].Rows[0]["WebAPIURL"].ToString();
                    if (dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                    {
                        SitaMessageHeader = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                    }
                    if (dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                    {
                        SFTPHeaderSITAddress = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                    }
                    if (WebAPIURL.Length > 0)
                    {
                        WEBAPIAddress = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), dsconfiguration.Tables[0].Rows[0]["WEBAPIHeaderType"].ToString());
                    }
                }
                //string fwbMsg = EncodeFWB(dsfwb,  error, MessageVersion);
                string ErrorMsg = null;
                (string fwbMsg, ErrorMsg) = await EncodeFWB(dsfwb, ErrorMsg, MessageVersion);
                try
                {
                    if (fwbMsg.Length > 3)
                    {
                        if (SitaMessageHeader.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("SITA:FWB", SitaMessageHeader.ToString() + "\r\n" + fwbMsg, "", "SITAFTP", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FWB");

                        if (SFTPHeaderSITAddress.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("SITA:FWB", SFTPHeaderSITAddress.ToString() + "\r\n" + fwbMsg, "", "SFTP", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FWB");

                        if (strEmailid.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("FWB", fwbMsg, "", strEmailid, DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FWB");

                        if (WEBAPIAddress.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("FWB", WEBAPIAddress.ToString() + "\r\n" + fwbMsg, "", "WEBAPI", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i], "Auto", "FWB");

                    }
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
            }
        }

        /// <summary>
        /// Generate FWB Message
        /// </summary>
        public async Task GenerateFWB(string DepartureAirport, string flightdest, string FlightNo, string FlightDate, DateTime itdate, DateTime lstFBLSent, bool isAutoSendOnTriggerTime)
        {
            try
            {
                string SitaMessageHeader = string.Empty, FWBMessageversion = string.Empty, Emailaddress = string.Empty, error = string.Empty, SFTPHeaderSITAddress = string.Empty;
                GenericFunction gf = new GenericFunction();
                MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                DataSet dsData = gf.GetRecordforGenerateFBLMessage(DepartureAirport, flightdest, FlightNo, FlightDate);

                if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
                {
                    DataSet dsmessage = gf.GetSitaAddressandMessageVersionForAutoMessage(FlightNo.Substring(0, 2), "FWB", "AIR", DepartureAirport, flightdest, FlightNo, string.Empty, string.Empty, string.Empty, isAutoSendOnTriggerTime: isAutoSendOnTriggerTime);
                    if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                    {
                        Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                        FWBMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();

                        if (dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                            SitaMessageHeader = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());

                        if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 1)
                            SFTPHeaderSITAddress = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                    }
                    DataTable dt = new DataTable();

                    if (dsData.Tables[1] != null && dsData.Tables[1].Rows.Count > 0)
                    {
                        dt = dsData.Tables[1];
                        dt = GenericFunction.SelectDistinct(dt, "AWBNumbers");

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            //if (Convert.ToDateTime(dsData.Tables[1].Rows[i]["UTCLastUpdatedOn"].ToString()) >= lstFBLSent && (lstFBLSent != Convert.ToDateTime("1900-01-01 00:00:00.000")))
                            //    return;

                            DataSet? dsfwb = await GetAWBRecordForGenerateFWBMessage(dt.Rows[i]["AWBNumbers"].ToString().Substring(4, 8), dt.Rows[i]["AWBNumbers"].ToString().Substring(0, 3));
                            string errorMSG = null;
                            (string fwbMsg, errorMSG) = await EncodeFWB(dsfwb, errorMSG, FWBMessageversion);

                            if (fwbMsg.Length > 3)
                            {
                                // clsLog.WriteLogAzure(" in FWBMsg len >0" + DateTime.Now + SitaMessageHeader + Emailaddress);
                                _logger.LogInformation(" in FWBMsg len >0 {0}", DateTime.Now + SitaMessageHeader + Emailaddress);

                                if (SitaMessageHeader != "")
                                {
                                    gf.SaveMessageOutBox("FWB", SitaMessageHeader + "\r\n" + fwbMsg, "SITAFTP", "SITAFTP", "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString(), "Auto", "FWB");
                                    // clsLog.WriteLogAzure(" in SaveMessageOutBox SitaMessageHeader" + DateTime.Now);
                                    _logger.LogInformation(" in SaveMessageOutBox SitaMessageHeader {0}", DateTime.Now);
                                }
                                if (SFTPHeaderSITAddress != "")
                                {
                                    gf.SaveMessageOutBox("FWB", SFTPHeaderSITAddress + "\r\n" + fwbMsg, "SFTP", "SFTP", "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString(), "Auto", "FWB");
                                    // clsLog.WriteLogAzure(" in SaveMessageOutBox SFTPHeaderSITAddress" + DateTime.Now);
                                    _logger.LogInformation(" in SaveMessageOutBox SFTPHeaderSITAddress {0}", DateTime.Now);
                                }

                                if (Emailaddress != "")
                                {
                                    gf.SaveMessageOutBox("FWB", fwbMsg, string.Empty, Emailaddress, "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString(), "Auto", "FWB");
                                    // clsLog.WriteLogAzure(" in SaveMessageOutBox  Emailaddress" + DateTime.Now);
                                    _logger.LogInformation(" in SaveMessageOutBox  Emailaddress {0}", DateTime.Now);
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

        /// <summary>
        /// Fill objects with data from data set
        /// </summary>
        public async Task<(string, string Error)> EncodeFWB(DataSet dsAWB, string Error, string fwbMessageVersion)
        {
            string FWBMsg = string.Empty;
            try
            {
                string strDimensionTag = string.Empty, strVolumeTag = string.Empty, strSlacTag = string.Empty, strULDBUP = string.Empty, strRouteTag = string.Empty, Slac = string.Empty;
                bool isSlacAdded = false;
                MessageData.fwbinfo FWBData = new MessageData.fwbinfo("");
                MessageData.othercharges[] othData = new MessageData.othercharges[0];
                MessageData.otherserviceinfo[] othSrvData = new MessageData.otherserviceinfo[0];
                MessageData.RateDescription[] fwbrates = new MessageData.RateDescription[0];
                MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                #region Prepare Structure

                if (dsAWB != null && dsAWB.Tables.Count > 0 && dsAWB.Tables[0].Rows.Count > 0)
                {
                    DataTable DT1 = dsAWB.Tables[0].Copy();
                    DataTable DT2 = dsAWB.Tables[1].Copy();
                    DataTable dtDimension = dsAWB.Tables[2].Copy();
                    DataTable DT4 = dsAWB.Tables[3].Copy();
                    DataTable dtAgent = dsAWB.Tables[4].Copy();
                    DataTable DT7 = dsAWB.Tables[6].Copy();
                    DataTable DT8 = dsAWB.Tables[7].Copy();  //Rate Charges
                    DataTable dtUldSlac = dsAWB.Tables[8].Copy();
                    DataTable DT10 = dsAWB.Tables[9].Copy();

                    FWBData.fwbversionnum = fwbMessageVersion;
                    FWBData.airlineprefix = DT1.Rows[0]["AWBPrefix"].ToString().Trim();
                    FWBData.awbnum = DT1.Rows[0]["AWBNumber"].ToString().Trim();
                    FWBData.origin = DT1.Rows[0]["OriginCode"].ToString().Trim().ToUpper();
                    FWBData.dest = DT1.Rows[0]["DestinationCode"].ToString().Trim().ToUpper();
                    FWBData.splhandling = DT1.Rows[0]["SHCCodes"].ToString().Trim().ToUpper();
                    FWBData.consigntype = "T";
                    FWBData.pcscnt = DT1.Rows[0]["PiecesCount"].ToString().Trim();
                    FWBData.weightcode = DT1.Rows[0]["UOM"].ToString().Trim();
                    FWBData.weight = DT1.Rows[0]["GrossWeight"].ToString().Trim();
                    FWBData.densityindicator = string.Empty;
                    FWBData.densitygrp = string.Empty;
                    Slac = DT1.Rows[0]["SLAC"].ToString().Trim();

                    string FlightNo = string.Empty;
                    if (DT4 != null && DT4.Rows.Count > 0)
                    {

                        if (DT4.Rows[0]["FWBFlightDayTag"].ToString() != "")
                            FWBData.fltnum = DT4.Rows[0]["FWBFlightDayTag"].ToString();
                        strRouteTag = DT4.Rows[0]["FWBRouteTag"].ToString().ToUpper();
                    }
                    else
                    {
                        FWBData.carriercode = string.Empty;
                        FWBData.fltnum = string.Empty;
                        strRouteTag = string.Empty;
                        FWBData.fwbversionnum = "9";
                    }


                    #region RateDescription
                    int RtdIncreament = 1;
                    if (DT8 != null && DT8.Rows.Count > 0)
                    {
                        for (int i = 0; i < DT8.Rows.Count; i++)
                        {
                            MessageData.RateDescription rate = new MessageData.RateDescription("");
                            rate.linenum = (i + 1).ToString();
                            RtdIncreament = (i + 1);
                            rate.pcsidentifier = "P";
                            rate.numofpcs = DT8.Rows[i]["Pcs"].ToString();
                            rate.weightindicator = DT1.Rows[0]["UOM"].ToString().Trim();
                            rate.weight = DT8.Rows[i]["Weight"].ToString();
                            rate.rateclasscode = DT8.Rows[i]["RateClass"].ToString();//RateClass
                            rate.commoditynumber = DT8.Rows[i]["CommCode"].ToString();
                            rate.awbweight = DT8.Rows[i]["ChargedWeight"].ToString();
                            rate.chargerate = DT8.Rows[i]["RatePerKg"].ToString();
                            rate.chargeamt = DT8.Rows[i]["Total"].ToString();
                            if (DT2.Rows[0]["CodeDescription"].ToString().Length > 20)
                                rate.goodsnature = DT2.Rows[0]["CodeDescription"].ToString().Substring(0, 20).Replace(",", " ").Replace(")", " ");
                            else
                                rate.goodsnature = DT2.Rows[0]["CodeDescription"].ToString().Replace(",", " ").Replace(")", " ");
                            if (Convert.ToBoolean(DT2.Rows[0]["IsConsole"].ToString()))
                                rate.isconsole = "1";
                            else
                                rate.isconsole = "0";

                            Array.Resize(ref fwbrates, fwbrates.Length + 1);
                            fwbrates[fwbrates.Length - 1] = rate;
                        }

                    }

                    Decimal volumeWeight = 0;

                    if (dtDimension != null && dtDimension.Rows.Count > 0)
                    {


                        for (int d = 0; d < dtDimension.Rows.Count; d++)
                        {

                            switch (dtDimension.Rows[d]["Units"].ToString().ToUpper())
                            {
                                case "CMT":
                                    volumeWeight += Math.Round(
                                           (decimal.Parse(dtDimension.Rows[d]["Length"].ToString()) * decimal.Parse(dtDimension.Rows[d]["Breadth"].ToString()) *
                                            decimal.Parse(dtDimension.Rows[d]["Height"].ToString()) * decimal.Parse(dtDimension.Rows[d]["PieceNo"].ToString())) / decimal.Parse("6000"),
                                           0);
                                    break;
                                default:
                                    volumeWeight += Math.Round(
                                         (decimal.Parse(dtDimension.Rows[d]["Length"].ToString()) * decimal.Parse(dtDimension.Rows[d]["Breadth"].ToString()) *
                                         decimal.Parse(dtDimension.Rows[d]["Height"].ToString()) * decimal.Parse(dtDimension.Rows[d]["PieceNo"].ToString())) / decimal.Parse("366"),
                                        0);
                                    break;
                            }
                            if (int.Parse(dtDimension.Rows[d]["Length"].ToString()) > 0 && int.Parse(dtDimension.Rows[d]["Breadth"].ToString()) > 0 && int.Parse(dtDimension.Rows[d]["Height"].ToString()) > 0 && int.Parse(dtDimension.Rows[d]["PieceNo"].ToString()) > 0 && ((Slac != "" && RtdIncreament < 9) || (Slac == "" && RtdIncreament < 10)))
                            {
                                RtdIncreament += 1;
                                if (strDimensionTag == "")
                                {
                                    strDimensionTag = "/" + RtdIncreament + "/ND/" + dtDimension.Rows[d]["UOM"].ToString().ToUpper() + dtDimension.Rows[d]["GrossWeight"].ToString() + "/" + dtDimension.Rows[d]["Units"].ToString().ToUpper() + dtDimension.Rows[d]["Length"].ToString() + "-" + dtDimension.Rows[d]["Breadth"].ToString() + "-" + dtDimension.Rows[d]["Height"].ToString() + "/" + dtDimension.Rows[d]["PieceNo"].ToString();
                                }
                                else
                                {
                                    strDimensionTag += "\r\n/" + RtdIncreament + "/ND/" + dtDimension.Rows[d]["UOM"].ToString().ToUpper() + dtDimension.Rows[d]["GrossWeight"].ToString() + "/" + dtDimension.Rows[d]["Units"].ToString().ToUpper() + dtDimension.Rows[d]["Length"].ToString() + "-" + dtDimension.Rows[d]["Breadth"].ToString() + "-" + dtDimension.Rows[d]["Height"].ToString() + "/" + dtDimension.Rows[d]["PieceNo"].ToString();
                                }

                            }
                        }

                        if (volumeWeight > 0)
                        {
                            RtdIncreament += 1;
                            strVolumeTag = "/" + RtdIncreament + "/NV/MC" + String.Format("{0:0.00}", Convert.ToDecimal(volumeWeight / decimal.Parse("166.67")));
                        }

                    }


                    if (dtUldSlac != null && dtUldSlac.Rows.Count > 0)
                    {
                        int tablecount = 0;
                        tablecount = dtUldSlac.Rows.Count;
                        for (int u = 0; u < dtUldSlac.Rows.Count; u++)
                        {
                            RtdIncreament += 1;
                            if (dtUldSlac.Rows[u]["ULDNo"].ToString() != "" && dtUldSlac.Rows[u]["ULDNo"].ToString().Length == 10 && int.Parse(dtUldSlac.Rows[u]["Slac"].ToString()) > 0)
                            {

                                if (strULDBUP == "")
                                {
                                    strULDBUP = "/" + RtdIncreament + "/NU/" + dtUldSlac.Rows[u]["ULDNo"].ToString().ToUpper() + "\r\n/" + "" + (RtdIncreament + 1).ToString() + "/NS/" + dtUldSlac.Rows[u]["Slac"].ToString();
                                    RtdIncreament += 1;
                                }
                                else
                                {

                                    if ((tablecount - 1) == u)
                                        strULDBUP += "\r\n/" + RtdIncreament + "/NU/" + dtUldSlac.Rows[u]["ULDNo"].ToString().ToUpper() + "\r\n/" + "" + (RtdIncreament + 1).ToString() + "/NS/" + dtUldSlac.Rows[u]["Slac"].ToString();
                                    else
                                    {

                                        strULDBUP += "\r\n/" + RtdIncreament + "/NU/" + dtUldSlac.Rows[u]["ULDNo"].ToString().ToUpper() + "\r\n/" + "" + (RtdIncreament + 1).ToString() + "/NS/" + dtUldSlac.Rows[u]["Slac"].ToString();
                                        RtdIncreament += 1;
                                    }
                                }
                            }
                            else if (Slac != "" && RtdIncreament < 12 && !isSlacAdded)
                            {
                                isSlacAdded = true;
                                RtdIncreament += 1;
                                strSlacTag = "/" + RtdIncreament + "/NS/" + Slac;
                            }
                        }
                    }
                    else if (Slac != "" && RtdIncreament < 12 && !isSlacAdded)
                    {
                        isSlacAdded = true;
                        RtdIncreament += 1;
                        strSlacTag = "/" + RtdIncreament + "/NS/" + Slac;
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(DT1.Rows[0]["HarmonizedCode"])))
                    {
                        string[] harmonizedcode = Convert.ToString(DT1.Rows[0]["HarmonizedCode"]).Split(',');
                        foreach (string item in harmonizedcode)
                        {
                            RtdIncreament += 1;
                            if (string.IsNullOrEmpty(strULDBUP))
                                strULDBUP += "/" + RtdIncreament + "/NH/" + item;
                            else
                                strULDBUP += "\r\n/" + RtdIncreament + "/NH/" + item;
                        }
                    }
                    #endregion

                    #region Other Charges

                    DataTable DTOtherCh = dsAWB.Tables[5].Copy();
                    if (DTOtherCh.Rows.Count > 0)
                    {
                        for (int i = 0; i < DTOtherCh.Rows.Count; i++)
                        {
                            MessageData.othercharges tempothData = new MessageData.othercharges("");
                            string ChargeType = DTOtherCh.Rows[i]["ChargeType"].ToString();
                            if (ChargeType.Trim() == "DA")
                            {
                                tempothData.entitlementcode = "A";
                                tempothData.otherchargecode = DTOtherCh.Rows[i]["ChargeHeadCode"].ToString().ToUpper();
                            }
                            else if (ChargeType.Trim() == "DC")
                            {
                                tempothData.entitlementcode = "C";
                                tempothData.otherchargecode = DTOtherCh.Rows[i]["ChargeHeadCode"].ToString().ToUpper();
                            }
                            tempothData.indicator = "P";
                            tempothData.chargeamt = DTOtherCh.Rows[i]["Charge"].ToString();
                            Array.Resize(ref othData, othData.Length + 1);
                            othData[othData.Length - 1] = tempothData;
                        }
                    }

                    #endregion

                    #region OCI Information
                    if (DT10 != null && DT10.Rows.Count > 0)
                    {
                        for (int i = 0; i < DT10.Rows.Count; i++)
                        {
                            MessageData.customsextrainfo custInfo = new MessageData.customsextrainfo("");
                            custInfo.IsoCountryCodeOci = DT10.Rows[i]["CountryCode"].ToString();
                            custInfo.InformationIdentifierOci = DT10.Rows[i]["InformationIdentifier"].ToString();
                            custInfo.CsrIdentifierOci = DT10.Rows[i]["CustomsIdentifier"].ToString();
                            custInfo.SupplementaryCsrIdentifierOci = DT10.Rows[i]["CustomsInfo"].ToString().Trim();
                            Array.Resize(ref custominfo, custominfo.Length + 1);
                            custominfo[custominfo.Length - 1] = custInfo;
                        }

                    }
                    #endregion

                    //DataTable dtcheckWalkingcustomer = RemoveAgentTagforWalkingCustomer("Quick Booking", DT1.Rows[0]["ShippingAgentCode"].ToString()).Tables[0];
                    DataSet ds = await RemoveAgentTagforWalkingCustomer("Quick Booking", DT1.Rows[0]["ShippingAgentCode"].ToString());
                    DataTable dtcheckWalkingcustomer = ds.Tables[0];
                    if (dtcheckWalkingcustomer != null && dtcheckWalkingcustomer.Rows.Count > 0 && dtcheckWalkingcustomer.Rows[0]["AppParameter"].ToString().ToUpper() == "DEFAULTAGENT")
                    {
                        FWBData.agentaccnum = string.Empty;
                        FWBData.agentIATAnumber = string.Empty;
                        FWBData.agentname = string.Empty;
                        FWBData.agentplace = string.Empty;
                    }
                    else
                    {
                        if (dtAgent != null && dtAgent.Rows.Count > 0)
                        {
                            //FWBData.agentaccnum = dtAgent.Rows[0]["AgentCode"].ToString().ToUpper();
                            FWBData.agentaccnum = string.Empty;
                            FWBData.agentIATAnumber = dtAgent.Rows[0]["IATAAgentCode"].ToString().ToUpper();
                            FWBData.agentCASSaddress = dtAgent.Rows[0]["IATACassCode"].ToString().ToUpper();
                            FWBData.agentname = dtAgent.Rows[0]["AgentName"].ToString();
                            FWBData.agentplace = dtAgent.Rows[0]["City"].ToString();

                        }
                        else
                        {
                            FWBData.agentaccnum = string.Empty;
                            FWBData.agentIATAnumber = string.Empty;
                            FWBData.agentname = string.Empty;
                            FWBData.agentplace = string.Empty;
                        }
                        //if (DT1.Rows[0]["ShippingAgentCode"].ToString() != "" && DT1.Rows[0]["ShippingAgentCode"].ToString().All(char.IsNumber))
                        //{
                        //    FWBData.agentaccnum = string.Empty;
                        //    FWBData.agentIATAnumber = DT1.Rows[0]["ShippingAgentCode"].ToString();
                        //    FWBData.agentCASSaddress = DT1.Rows[0]["AgentCassAddress"].ToString();
                        //    FWBData.agentname = DT1.Rows[0]["ShippingAgentName"].ToString().ToUpper();
                        //    FWBData.agentplace = FWBData.origin.ToUpper();
                        //}


                    }

                    //Makeing Reference Tag
                    GenericFunction GF = new GenericFunction();
                    DataSet dsReference = GF.GetConfigurationofReferenceTag("REF", "REFERENCETAG", "FWB");
                    if (dsReference != null && dsReference.Tables[0].Rows.Count > 0 && dsReference.Tables[0].Rows[0]["AppValue"].ToString() == "TRUE")
                    {
                        // FWBData.senderofficedesignator = dsReference.Tables[0].Rows[0]["OriginSitaAddress"].ToString().Trim();

                        FWBData.senderofficedesignator = DT1.Rows[0]["SenderOfficeDesignator"].ToString().ToUpper();
                        FWBData.senderairport = DT1.Rows[0]["OriginCode"].ToString().ToUpper();
                        FWBData.sendercompanydesignator = DT1.Rows[0]["CompanyDesignatior"].ToString().ToUpper();
                        FWBData.senderFileref = DT1.Rows[0]["BookingFileReference"].ToString().ToUpper();
                    }
                    else
                    {
                        if (DT1.Rows[0]["RefereceTag"].ToString() != "")
                        {
                            FWBData.senderofficedesignator = DT1.Rows[0]["RefereceTag"].ToString();
                            FWBData.senderairport = string.Empty;
                            FWBData.sendercompanydesignator = string.Empty;
                            FWBData.senderFileref = string.Empty;
                        }
                        else
                        {
                            FWBData.senderofficedesignator = DT1.Rows[0]["SenderOfficeDesignator"].ToString().ToUpper();
                            FWBData.senderairport = DT1.Rows[0]["OriginCode"].ToString().ToUpper();
                            FWBData.sendercompanydesignator = DT1.Rows[0]["CompanyDesignatior"].ToString().ToUpper();
                            FWBData.senderFileref = DT1.Rows[0]["BookingFileReference"].ToString().ToUpper();
                        }
                    }


                    FWBData.currency = DT2.Rows[0]["CustomCurrency"].ToString().ToUpper();
                    FWBData.declaredvalue = DT1.Rows[0]["DVCarriage"].ToString().ToUpper() == "0.00" || DT1.Rows[0]["DVCarriage"].ToString().ToUpper() == "0" ? "NVD" : DT1.Rows[0]["DVCarriage"].ToString().ToUpper();
                    FWBData.declaredcustomvalue = DT1.Rows[0]["DVCustom"].ToString().ToUpper() == "0.00" || DT1.Rows[0]["DVCustom"].ToString().ToUpper() == "0" ? "NCV" : DT1.Rows[0]["DVCustom"].ToString().ToUpper();
                    FWBData.insuranceamount = DT1.Rows[0]["InsuranceAmmount"].ToString().ToUpper();
                    string PaymentMode = string.Empty;
                    PaymentMode = DT2.Rows[0]["PaymentMode"].ToString().ToUpper();
                    FWBData.chargedec = (PaymentMode.Length < 0 ? "PP" : PaymentMode);
                    FWBData.chargecode = DT1.Rows[0]["DeclaredPayMode"].ToString().ToUpper();

                    if (PaymentMode.Trim() != "CC")
                    {

                        FWBData.PPweightCharge = DT8.Rows[0]["FrIATA"].ToString();
                        FWBData.PPTaxesCharge = DT8.Rows[0]["ServTax"].ToString();
                        FWBData.PPOCDC = DT8.Rows[0]["OCDueCar"].ToString();
                        FWBData.PPOCDA = DT8.Rows[0]["OCDueAgent"].ToString();
                        FWBData.PPTotalCharges = DT8.Rows[0]["TotalCharge"].ToString();

                    }
                    else
                    {

                        FWBData.CCweightCharge = DT8.Rows[0]["FrIATA"].ToString();
                        FWBData.CCTaxesCharge = DT8.Rows[0]["ServTax"].ToString();
                        FWBData.CCOCDC = DT8.Rows[0]["OCDueCar"].ToString();
                        FWBData.CCOCDA = DT8.Rows[0]["OCDueAgent"].ToString();
                        FWBData.CCTotalCharges = DT8.Rows[0]["TotalCharge"].ToString();

                    }

                    DateTime ExecutionDate = Convert.ToDateTime(DT1.Rows[0]["ExecutionDate"].ToString());
                    FWBData.carrierdate = ExecutionDate.ToString("dd");
                    FWBData.carriermonth = ExecutionDate.ToString("MMM").ToUpper();
                    FWBData.carrieryear = ExecutionDate.ToString("yy");
                    FWBData.carrierplace = DT1.Rows[0]["ExecutedAt"].ToString();


                    if (DT7.Rows.Count > 0)
                    {
                        FWBData.shippername = DT7.Rows[0]["ShipperName"].ToString();
                        if (DT7.Rows[0]["ShipperAddress"].ToString().Length > 35)
                            FWBData.shipperadd = DT7.Rows[0]["ShipperAddress"].ToString().Substring(0, 35).ToUpper();
                        else
                            FWBData.shipperadd = DT7.Rows[0]["ShipperAddress"].ToString().ToUpper();

                        FWBData.shipperplace = DT7.Rows[0]["ShipperCity"].ToString().ToUpper();
                        FWBData.shipperstate = DT7.Rows[0]["ShipperState"].ToString().Trim().ToUpper();
                        FWBData.shippercountrycode = DT7.Rows[0]["ShipperCountry"].ToString().ToUpper();
                        FWBData.shippercontactnum = DT7.Rows[0]["ShipperTelephone"].ToString().ToUpper().Trim();
                        if (DT7.Rows[0]["ShipperPincode"].ToString().Length > 0)
                            FWBData.shipperpostcode = DT7.Rows[0]["ShipperPincode"].ToString().Trim().ToUpper();

                        FWBData.consname = DT7.Rows[0]["ConsigneeName"].ToString();
                        if (DT7.Rows[0]["ConsigneeAddress"].ToString().Length > 35)
                            FWBData.consadd = DT7.Rows[0]["ConsigneeAddress"].ToString().Substring(0, 35).ToUpper();

                        else
                            FWBData.consadd = DT7.Rows[0]["ConsigneeAddress"].ToString().ToUpper();
                        FWBData.consplace = DT7.Rows[0]["ConsigneeCity"].ToString().ToUpper();
                        FWBData.consstate = DT7.Rows[0]["ConsigneeState"].ToString().ToUpper();
                        FWBData.conscountrycode = DT7.Rows[0]["ConsigneeCountry"].ToString().ToUpper();
                        FWBData.conscontactnum = DT7.Rows[0]["ConsigneeTelephone"].ToString().Trim().ToUpper();
                        if (DT7.Rows[0]["ConsigneePincode"].ToString().Length > 0)
                        {
                            FWBData.conspostcode = DT7.Rows[0]["ConsigneePincode"].ToString().Trim().ToUpper();
                        }

                        #region : NFY :
                        FWBData.notifyname = DT7.Rows[0]["NotifyName"].ToString();
                        if (DT7.Rows[0]["NotifyAddress"].ToString().Length > 35)
                            FWBData.notifyadd = DT7.Rows[0]["NotifyAddress"].ToString().Substring(0, 35).ToUpper();
                        else
                            FWBData.notifyadd = DT7.Rows[0]["NotifyAddress"].ToString().ToUpper();
                        FWBData.notifyplace = DT7.Rows[0]["NotifyCity"].ToString().ToUpper();

                        FWBData.notifystate = DT7.Rows[0]["NotifyState"].ToString().Trim().ToUpper();
                        FWBData.notifycountrycode = DT7.Rows[0]["NotifyCountry"].ToString().ToUpper();
                        FWBData.notifycontactnum = DT7.Rows[0]["NotifyTelephone"].ToString().ToUpper().Trim();

                        if (DT7.Rows[0]["NotifyPincode"].ToString().Length > 0)
                            FWBData.notifypostcode = DT7.Rows[0]["NotifyPincode"].ToString().Trim().ToUpper();

                        if (FWBData.notifycontactnum.Length > 0)
                            FWBData.notifycontactidentifier = "TE";
                        #endregion NFY

                        if (FWBData.shippercontactnum.Length > 0) { FWBData.shippercontactidentifier = "TE"; }
                        if (FWBData.conscontactnum.Length > 0) { FWBData.conscontactidentifier = "TE"; }

                        if (DT1.Rows[0]["RefereceTag"].ToString() == "")
                        {
                            FWBData.senderPariticipentAirport = DT1.Rows[0]["OriginCode"].ToString().Trim();
                            FWBData.senderParticipentIdentifier = DT1.Rows[0]["SenderParticipentIdentifier"].ToString().Trim();
                            FWBData.senderParticipentCode = DT1.Rows[0]["SenderParticipentCode"].ToString();
                        }



                    }
                    else
                    {
                        Error = "No Shipper/Consignee Info Availabe for FWB";
                        //return FWBMsg;
                        return (FWBMsg, Error);
                    }
                    // }

                    #endregion
                    FWBMsg = EncodeFWBForSend(ref FWBData, ref othData, ref othSrvData, ref fwbrates, ref custominfo, strDimensionTag, strVolumeTag, strULDBUP, strRouteTag, strSlacTag);
                }
            }
            catch (Exception ex)
            {
                //BAL.SCMException.logexception(ref ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                Error = ex.Message;
            }
            //return FWBMsg;
            return (FWBMsg, Error);
        }

        /// <summary>
        /// Encode FWB message and return the same
        /// </summary>
        public static string EncodeFWBForSend(ref MessageData.fwbinfo fwbdata, ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.RateDescription[] fwbrate, ref MessageData.customsextrainfo[] custominfo, string DimensionTag, string VolumeTag, string strULDBUP, string strRouteTag, string SlacTag)
        {
            string fwbstr = null;
            try
            {
                //FWB
                #region Line 1
                string line1 = "FWB/" + fwbdata.fwbversionnum;
                #endregion

                #region Line 2
                string line2 = fwbdata.airlineprefix + "-" + fwbdata.awbnum + fwbdata.origin + fwbdata.dest + "/" + fwbdata.consigntype + fwbdata.pcscnt + fwbdata.weightcode + fwbdata.weight + fwbdata.volumecode + fwbdata.volumeamt + fwbdata.densityindicator + fwbdata.densitygrp;
                #endregion line 2
                //FLT
                #region Line 3
                string line3 = "";
                if (fwbdata.fltnum.Length >= 2)
                {
                    line3 = "FLT/" + fwbdata.fltnum.ToUpper();

                }
                #endregion

                //RTG
                #region Line 4
                string line4 = string.Empty;
                if (strRouteTag != "")
                {
                    line4 = strRouteTag;
                    if (line4.Length >= 5)
                    {
                        line4 = "RTG/" + line4;
                    }
                }
                #endregion

                //SHP
                #region Line 5
                string line5 = "";
                string str1 = "", str2 = "", str3 = "", str4 = "";
                try
                {
                    if (fwbdata.fwbversionnum.Trim() == "17")
                    {
                        if (fwbdata.shippername.Length > 0)
                            str1 = "NAM/" + fwbdata.shippername;
                        if (fwbdata.shipperadd.Length > 0)
                            str2 = "ADR/" + fwbdata.shipperadd;
                        if (fwbdata.shipperplace.Length > 0 || fwbdata.shipperstate.Length > 0)
                            str3 = "LOC/" + fwbdata.shipperplace + "/" + fwbdata.shipperstate;
                    }
                    else
                    {
                        if (fwbdata.shippername.Length > 0)
                            str1 = "/" + fwbdata.shippername;
                        if (fwbdata.shipperadd.Length > 0)
                            str2 = "/" + fwbdata.shipperadd;
                        if (fwbdata.shipperplace.Length > 0 || fwbdata.shipperstate.Length > 0)
                            str3 = "/" + fwbdata.shipperplace + "/" + fwbdata.shipperstate;
                    }
                    if (fwbdata.shippercountrycode.Length > 0 || fwbdata.shipperpostcode.Length > 0 || fwbdata.shippercontactidentifier.Length > 0 || fwbdata.shippercontactnum.Length > 0)
                    {
                        str4 = "/" + fwbdata.shippercountrycode + "/" + fwbdata.shipperpostcode + "/" + fwbdata.shippercontactidentifier + "/" + fwbdata.shippercontactnum;
                    }

                    if (fwbdata.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line5 = "SHP/" + fwbdata.shipperaccnum;
                        if (fwbdata.fwbversionnum.Trim() == "17")
                        {
                            if (str4.Length > 0)
                                line5 = line5.Trim('/') + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line5 = line5.Trim() + "\r\n" + str1.Trim('/');
                        }
                        else
                        {
                            if (str4.Length > 0)
                                line5 = line5.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line5 = line5.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line5 = line5.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line5 = line5.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception) { /*BAL.SCMException.logexception(ref ex);*/ }
                #endregion

                //CNE
                #region Line 6
                string line6 = "";
                str1 = str2 = str3 = str4 = "";
                try
                {
                    if (fwbdata.fwbversionnum.Trim() == "17")
                    {
                        if (fwbdata.consname.Length > 0)
                            str1 = "NAM/" + fwbdata.consname;
                        if (fwbdata.consadd.Length > 0)
                            str2 = "ADR/" + fwbdata.consadd;
                        if (fwbdata.consplace.Length > 0 || fwbdata.consstate.Length > 0)
                            str3 = "LOC/" + fwbdata.consplace + "/" + fwbdata.consstate;
                    }
                    else
                    {
                        if (fwbdata.consname.Length > 0)
                            str1 = "/" + fwbdata.consname;
                        if (fwbdata.consadd.Length > 0)
                            str2 = "/" + fwbdata.consadd;
                        if (fwbdata.consplace.Length > 0 || fwbdata.consstate.Length > 0)
                            str3 = "/" + fwbdata.consplace + "/" + fwbdata.consstate;
                    }
                    if (fwbdata.conscountrycode.Length > 0 || fwbdata.conspostcode.Length > 0 || fwbdata.conscontactidentifier.Length > 0 || fwbdata.conscontactnum.Length > 0)
                        str4 = "/" + fwbdata.conscountrycode + "/" + fwbdata.conspostcode + "/" + fwbdata.conscontactidentifier + "/" + fwbdata.conscontactnum;

                    if (fwbdata.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line6 = "CNE/" + fwbdata.consaccnum;
                        if (fwbdata.fwbversionnum.Trim() == "17")
                        {
                            if (str4.Length > 0)
                                line6 = line6.Trim('/') + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line6 = line6.Trim() + "\r\n" + str1.Trim('/');
                        }
                        else
                        {
                            if (str4.Length > 0)
                                line6 = line6.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line6 = line6.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line6 = line6.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line6 = line6.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception) { /*BAL.SCMException.logexception(ref ex);*/ }
                #endregion

                //AGT
                #region Line 7
                string line7 = "";
                str1 = "";
                str2 = "";
                try
                {
                    if (fwbdata.agentname.Length > 0)
                    {
                        str1 = "/" + fwbdata.agentname;
                    }
                    if (fwbdata.agentplace.Length > 0)
                    {
                        str2 = "/" + fwbdata.agentplace;
                    }
                    if (fwbdata.agentaccnum.Length > 0 || fwbdata.agentIATAnumber.Length > 0 || fwbdata.agentCASSaddress.Length > 0 || fwbdata.agentParticipentIdentifier.Length > 0 || str1.Length > 0 || str2.Length > 0)
                    {

                        fwbdata.agentaccnum = fwbdata.agentaccnum.Trim().Length > 14 ? fwbdata.agentaccnum.Trim().Substring(0, 14) : fwbdata.agentaccnum.Trim();
                        fwbdata.agentIATAnumber = fwbdata.agentIATAnumber.Trim().Length > 7 ? fwbdata.agentIATAnumber.Trim().Substring(0, 7) : fwbdata.agentIATAnumber.Trim();
                        fwbdata.agentCASSaddress = fwbdata.agentCASSaddress.Trim().Length > 4 ? fwbdata.agentCASSaddress.Trim().Substring(0, 4) : fwbdata.agentCASSaddress.Trim();
                        line7 = "AGT/" + fwbdata.agentaccnum + "/" + fwbdata.agentIATAnumber + "/" + fwbdata.agentCASSaddress + "/" + fwbdata.agentParticipentIdentifier;
                        if (str2.Length > 0)
                        {
                            line7 = line7.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line7 = line7.Trim('/') + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception) { /*BAL.SCMException.logexception(ref ex);*/ }
                #endregion

                //SSR
                #region Line 8
                string line8 = "";
                if (fwbdata.specialservicereq1.Length > 0 || fwbdata.specialservicereq2.Length > 0)
                {
                    line8 = "SSR/" + fwbdata.specialservicereq1 + "$" + fwbdata.specialservicereq2;
                }
                line8 = line8.Trim('$');
                line8 = line8.Replace("$", "\r\n");
                #endregion

                //NFY
                #region Line 9
                string line9 = "";
                str1 = str2 = str3 = str4 = "";
                try
                {
                    if (fwbdata.fwbversionnum.Trim() == "17")
                    {
                        if (fwbdata.notifyname.Length > 0)
                            str1 = "NAM/" + fwbdata.notifyname;
                        if (fwbdata.notifyadd.Length > 0)
                            str2 = "ADR/" + fwbdata.notifyadd;
                        if (fwbdata.notifyplace.Length > 0 || fwbdata.notifystate.Length > 0)
                            str3 = "LOC/" + fwbdata.notifyplace + "/" + fwbdata.notifystate;
                    }
                    else
                    {
                        if (fwbdata.notifyname.Length > 0)
                            str1 = "/" + fwbdata.notifyname;
                        if (fwbdata.notifyadd.Length > 0)
                            str2 = "/" + fwbdata.notifyadd;
                        if (fwbdata.notifyplace.Length > 0)
                            str3 = "/" + fwbdata.notifyplace + "/" + fwbdata.notifystate;
                    }
                    if (fwbdata.notifycountrycode.Length > 0)
                        str4 = "/" + fwbdata.notifycountrycode + (fwbdata.notifycontactnum == "" ? (fwbdata.notifypostcode == "" ? "" : "/" + fwbdata.notifypostcode)
                        : "/" + fwbdata.notifypostcode + "/" + fwbdata.notifycontactidentifier + "/" + fwbdata.notifycontactnum);

                    if (fwbdata.notifyname.Length > 0 && str2.Length > 0 && str3.Length > 0 && str4.Length > 0)
                    {
                        line9 = "NFY";
                        if (fwbdata.fwbversionnum.Trim() == "17")
                        {
                            if (str4.Length > 0)
                                line9 = line9.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line9 = line9.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/') + "\r\n" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line9 = line9.Trim() + "\r\n" + str1.Trim('/') + "\r\n" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line9 = line9.Trim() + "\r\n" + str1.Trim('/');
                        }
                        else
                        {
                            if (str4.Length > 0)
                                line9 = line9.Trim() + "/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                            else if (str3.Length > 0)
                                line9 = line9.Trim() + "/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                            else if (str2.Length > 0)
                                line9 = line9.Trim() + "/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                            else if (str1.Length > 0)
                                line9 = line9.Trim() + "/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception) { /*BAL.SCMException.logexception(ref ex);*/ }
                #endregion

                //ACC
                #region Line 10
                string line10 = "";
                if (fwbdata.accountinginfoidentifier.Length > 0 || fwbdata.accountinginfo.Length > 0)
                {
                    line10 = "ACC/" + fwbdata.accountinginfoidentifier + "/" + fwbdata.accountinginfo + "";
                }
                #endregion

                //CVD
                #region Line 11
                string line11 = string.Empty;
                line11 = "CVD/" + fwbdata.currency + "/" + fwbdata.chargedec + "/" + fwbdata.chargecode + "/" + fwbdata.declaredvalue + "/" + fwbdata.declaredcustomvalue + "/" + fwbdata.insuranceamount + "";
                #endregion

                //RTD
                #region Line 12
                string line12 = buildRateNode(ref fwbrate);
                if (line12 == null)
                {
                    return null;
                }
                if (DimensionTag != "")
                    line12 += "\r\n" + DimensionTag;

                if (VolumeTag != "")
                    line12 += "\r\n" + VolumeTag;

                if (SlacTag != "")
                    line12 += "\r\n" + SlacTag;

                if (strULDBUP != "")
                    line12 += "\r\n" + strULDBUP;

                #endregion

                //OTH
                #region Line 13
                string line13 = "";
                for (int i = 0; i < fwbOtherCharge.Length; i++)
                {
                    if (i > 0)
                    {
                        if (i % 3 == 0)
                        {
                            if (i != fwbOtherCharge.Length)
                            {
                                line13 += "\r\n/" + fwbOtherCharge[0].indicator + "/";
                            }
                        }
                    }
                    line13 += fwbOtherCharge[i].otherchargecode + "" + fwbOtherCharge[i].entitlementcode + "" + fwbOtherCharge[i].chargeamt;
                    //if (i % 3 == 0)
                    //{
                    //    if(i != othData.Length)
                    //    {
                    //        FWBStr += "\r\nP";
                    //    }
                    //}
                }
                if (line13.Length > 1)
                {
                    line13 = "OTH/P/" + line13;
                }
                #endregion

                //PPD
                #region Line 14
                string line14 = "", subline14 = "";

                if (fwbdata.PPweightCharge.Length > 0)
                {
                    line14 = line14 + "/WT" + fwbdata.PPweightCharge;
                }
                if (fwbdata.PPValuationCharge.Length > 0)
                {
                    line14 = line14 + "/VC" + fwbdata.PPValuationCharge;
                }
                if (fwbdata.PPTaxesCharge.Length > 0)
                {
                    line14 = line14 + "/TX" + fwbdata.PPTaxesCharge;
                }
                if (fwbdata.PPOCDA.Length > 0)
                {
                    subline14 = subline14 + "/OA" + fwbdata.PPOCDA;
                }
                if (fwbdata.PPOCDC.Length > 0)
                {
                    subline14 = subline14 + "/OC" + fwbdata.PPOCDC;
                }
                if (fwbdata.PPTotalCharges.Length > 0)
                {
                    subline14 = subline14 + "/CT" + fwbdata.PPTotalCharges;
                }
                if (line14.Length > 0 || subline14.Length > 0)
                {
                    line14 = "PPD" + line14 + "$" + subline14;
                }
                line14 = line14.Trim('$');
                line14 = line14.Replace("$", "\r\n");
                #endregion

                //COL
                #region Line 15
                string line15 = "", subline15 = "";
                if (fwbdata.CCweightCharge.Length > 0)
                {
                    line15 = line15 + "/WT" + fwbdata.CCweightCharge;
                }
                if (fwbdata.CCValuationCharge.Length > 0)
                {
                    line15 = line15 + "/VC" + fwbdata.CCValuationCharge;
                }
                if (fwbdata.CCTaxesCharge.Length > 0)
                {
                    line15 = line15 + "/TX" + fwbdata.CCTaxesCharge;
                }
                if (fwbdata.CCOCDA.Length > 0)
                {
                    subline15 = subline15 + "/OA" + fwbdata.CCOCDA;
                }
                if (fwbdata.CCOCDC.Length > 0)
                {
                    subline15 = subline15 + "/OC" + fwbdata.CCOCDC;
                }
                if (fwbdata.CCTotalCharges.Length > 0)
                {
                    subline15 = subline15 + "/CT" + fwbdata.CCTotalCharges;
                }
                if (line15.Length > 0 || subline15.Length > 0)
                {
                    line15 = "COL" + line15 + "$" + subline15;
                }
                line15 = line15.Trim('$');
                line15 = line15.Replace("$", "\r\n");
                #endregion

                //CER
                #region Line 16
                string line16 = "";
                if (fwbdata.shippersignature.Length > 0)
                {
                    line16 = "CER/" + fwbdata.shippersignature;
                }
                #endregion

                //ISU
                #region Line 17
                string line17 = "";
                line17 = "ISU/" + fwbdata.carrierdate.PadLeft(2, '0') + fwbdata.carriermonth.PadLeft(2, '0') + fwbdata.carrieryear.PadLeft(2, '0') + "/" + fwbdata.carrierplace + "/" + fwbdata.carriersignature;
                #endregion

                //OSI
                #region Line 18
                string line18 = "";
                if (othinfoarray.Length > 0)
                {
                    for (int i = 0; i < othinfoarray.Length; i++)
                    {
                        if (othinfoarray[i].otherserviceinfo1.Length > 0)
                        {
                            line18 = "OSI/" + othinfoarray[i].otherserviceinfo1 + "$";
                            if (othinfoarray[i].otherserviceinfo2.Length > 0)
                            {
                                line18 = line18 + "/" + othinfoarray[i].otherserviceinfo2 + "$";
                            }
                        }
                    }
                    line18 = line18.Trim('$');
                    line18 = line18.Replace("$", "\r\n");
                }
                #endregion

                //CDC
                #region Line 19
                string line19 = "";
                if (fwbdata.cccurrencycode.Length > 0 || fwbdata.ccexchangerate.Length > 0 || fwbdata.ccchargeamt.Length > 0)
                {
                    string[] exchnagesplit = fwbdata.ccexchangerate.Split(',');
                    string[] chargesplit = fwbdata.ccchargeamt.Split(',');
                    if (exchnagesplit.Length == chargesplit.Length)
                    {
                        for (int k = 0; k < exchnagesplit.Length; k++)
                        {
                            line19 = line19 + exchnagesplit[k] + "/" + chargesplit[k] + "/";
                        }
                    }
                    line19 = "CDC/" + fwbdata.cccurrencycode + line19.Trim('/');
                }
                #endregion

                //REF
                #region Line 20
                string line20 = "";
                //line20 = fwbdata.agentplace + "" + fwbdata.senderofficedesignator + "" + fwbdata.sendercompanydesignator;

                line20 = fwbdata.senderairport + "" + fwbdata.senderofficedesignator + "" + fwbdata.sendercompanydesignator + "/" + fwbdata.senderFileref + "/" + fwbdata.senderParticipentIdentifier + "/" + fwbdata.senderParticipentCode + "/" + fwbdata.senderPariticipentAirport + "";
                //line20 = line20.Trim('/');
                if (line20.Length > 1)
                {
                    line20 = "REF/" + line20;
                }

                #endregion

                //COR
                #region Line 21
                string line21 = "";
                if (fwbdata.customorigincode.Length > 0)
                {
                    line21 = "COR/" + fwbdata.customorigincode + "";
                }
                #endregion

                //COI
                #region Line 22
                string line22 = "";
                if (fwbdata.commisioncassindicator.Length > 0 || fwbdata.commisionCassSettleAmt.Length > 0)
                {
                    line22 = "COI/" + fwbdata.commisioncassindicator + "/" + fwbdata.commisionCassSettleAmt.Replace(',', '/') + "";
                }
                #endregion

                //SII
                #region Line 23
                string line23 = "";
                if (fwbdata.saleschargeamt.Length > 0 || fwbdata.salescassindicator.Length > 0)
                {
                    line23 = "SII/" + fwbdata.saleschargeamt + "/" + fwbdata.salescassindicator + "";
                }
                #endregion

                //ARD
                #region Line 24
                string line24 = "";
                if (fwbdata.agentfileref.Length > 0)
                {
                    line24 = "ARD/" + fwbdata.agentfileref + "";
                }
                #endregion

                //SPH
                #region Line 25
                string line25 = "";
                if (fwbdata.splhandling.Replace(",", "").Length > 0)
                {
                    line25 = "SPH/" + fwbdata.splhandling.Replace(',', '/');
                }
                #endregion

                //NOM
                #region Line 26
                string line26 = "";
                if (fwbdata.handlingname.Length > 0 || fwbdata.handlingplace.Length > 0)
                {
                    line26 = "NOM/" + fwbdata.handlingname + "/" + fwbdata.handlingplace;
                }
                #endregion

                //SRI
                #region Line 27
                string line27 = "";
                if (fwbdata.shiprefnum.Length > 0 || fwbdata.supplemetryshipperinfo1.Length > 0 || fwbdata.supplemetryshipperinfo2.Length > 0)
                {
                    line27 = "SRI/" + fwbdata.shiprefnum + "/" + fwbdata.supplemetryshipperinfo1 + "/" + fwbdata.supplemetryshipperinfo2;
                }
                #endregion

                //OPI
                #region Line 28
                str1 = "";
                string line28 = "";
                if (fwbdata.othairport.Length > 0 || fwbdata.othofficedesignator.Length > 0 || fwbdata.othcompanydesignator.Length > 0 || fwbdata.othfilereference.Length > 0 || fwbdata.othparticipentidentifier.Length > 0 || fwbdata.othparticipentcode.Length > 0 || fwbdata.othparticipentairport.Length > 0)
                {
                    str1 = "/" + fwbdata.othparticipentairport + "/" +
                    fwbdata.othofficedesignator + "" + fwbdata.othcompanydesignator + "/" + fwbdata.othfilereference + "/" +
                    fwbdata.othparticipentidentifier + "/" + fwbdata.othparticipentcode + "/" + fwbdata.othparticipentairport + "";
                    str1 = str1.Trim('/');
                }

                if (fwbdata.othparticipentname.Length > 0 || str1.Length > 0)
                {
                    line28 = "OPI/" + fwbdata.othparticipentname + "$" + str1;
                }
                line28 = line28.Trim('$');
                line28 = line28.Replace("$", "\r\n");
                #endregion


                //OCI
                #region Line 29
                //string line29 = "";
                //if (custominfo.Length > 0)
                //{
                //    for (int i = 0; i < custominfo.Length; i++)
                //    {
                //        line29 = "/" + custominfo[i].IsoCountryCodeOci + "/" + custominfo[i].InformationIdentifierOci + "/" + custominfo[i].CsrIdentifierOci + "/" + custominfo[i].SupplementaryCsrIdentifierOci + "$";
                //    }
                //    line29 = "OCI" + line4.Trim('$');
                //    line29 = line4.Replace("$", "\r\n");
                //}
                string line29 = "";
                if (custominfo.Length > 0)
                {
                    for (int i = 0; i < custominfo.Length; i++)
                    {
                        line29 += (i == 0 ? "OCI/" : "/") + custominfo[i].IsoCountryCodeOci + "/" + custominfo[i].InformationIdentifierOci + "/" + custominfo[i].CsrIdentifierOci + "/" + custominfo[i].SupplementaryCsrIdentifierOci + "$";
                    }
                    line29 = line29.Replace("$", "\r\n");
                }
                #endregion

                #region Build FWB
                fwbstr = "";
                fwbstr = line1.Trim('/');
                if (line2.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line2.Trim('/');
                }
                if (line3.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line3.Trim('/');
                }
                fwbstr += "\r\n" + line4.Trim('/') + "\r\n" + line5.Trim('/');
                if (line6.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line6.Trim('/');
                }

                if (line7.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line7.Trim('/');
                }
                if (line8.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line8.Trim('/');
                }
                if (line9.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line9.Trim('/');
                }
                if (line10.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line10.Trim('/');
                }
                fwbstr += "\r\n" + line11.Trim('/') + "\r\n" + line12.Trim('/');
                if (line13.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line13.Trim('/');
                }
                if (line14.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line14.Trim('/');
                }
                if (line15.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line15.Trim('/');
                }
                if (line16.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line16.Trim('/');
                }
                if (line17.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line17.Trim('/');
                }
                if (line18.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line18.Trim('/');
                }
                if (line19.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line19.Trim('/');
                }
                if (line20.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line20.Trim('/');
                }
                if (line21.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line21.Trim('/');
                }
                if (line22.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line22.Trim('/');
                }
                if (line23.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line23.Trim('/');
                }
                if (line24.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line24.Trim('/');
                }
                if (line25.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line25.Trim('/');
                }
                if (line26.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line26.Trim('/');
                }
                if (line27.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line27.Trim('/');
                }
                if (line28.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line28.Trim('/');
                }
                if (line29.Trim('/').Length > 0)
                {
                    fwbstr += "\r\n" + line29.Trim('/');
                }
                #endregion
            }
            catch (Exception ex)
            {
                //BAL.SCMException.logexception(ref ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                fwbstr = "ERR";
            }
            return fwbstr;
        }

        /// <summary>
        /// Method is used to make RTD Tag On FWB
        /// </summary>
        /// <param name="fwbrate"></param>
        /// <returns></returns>
        public static string buildRateNode(ref MessageData.RateDescription[] fwbrate)
        {
            string Ratestr = null;
            try
            {
                string str1, str2, str3, str4, str5, str6, str7, str8;
                for (int i = 0; i < fwbrate.Length; i++)
                {
                    int cnt = 1;
                    str1 = str2 = str3 = str4 = str5 = str6 = str7 = str8 = "";
                    if (fwbrate[i].goodsnature.Length > 0)
                    {
                        if (cnt > 1)
                        {
                            if (fwbrate[i].isconsole == "1")
                                str1 = "/" + (cnt++) + "/NC/" + fwbrate[i].goodsnature;
                            else
                                str1 = "/" + (cnt++) + "/NG/" + fwbrate[i].goodsnature;
                        }
                        else
                        {
                            if (fwbrate[i].isconsole == "1")
                                str1 = "/NC/" + fwbrate[i].goodsnature;
                            else
                                str1 = "/NG/" + fwbrate[i].goodsnature;
                            cnt++;
                        }
                    }

                    Ratestr += "RTD/" + (i + 1) + "/" + fwbrate[i].pcsidentifier + fwbrate[i].numofpcs + "/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/C" + fwbrate[i].rateclasscode + "/W" + fwbrate[i].awbweight + "/R" + fwbrate[i].chargerate + "/T" + fwbrate[i].chargeamt;
                    Ratestr = Ratestr.Trim('/') + "$" + str1.Trim() + "$" + str2.Trim() + "$" + str3.Trim() + "$" + str4.Trim() + "$" + str5.Trim() + "$" + str6.Trim() + "$" + str7.Trim() + "$" + str8.Trim();
                    Ratestr = Ratestr.Replace("$$", "$");
                    Ratestr = Ratestr.Trim('$');
                    Ratestr = Ratestr.Replace("$", "\r\n");
                }

            }
            catch (Exception ex)
            {
                //BAL.SCMException.logexception(ref ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                Ratestr = null;
            }
            return Ratestr;
        }

        /// <summary>
        /// Below method is used to check configuration  for Walking Agent
        /// This is AGI Requirenment
        /// Created On:2105-10-13
        /// Created By:Badiuz khan
        /// </summary>
        /// <param name="Appkey"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<DataSet> RemoveAgentTagforWalkingCustomer(string Appkey, string agtParameter)
        {

            //SQLServer da = new SQLServer();
            DataSet? ds = new DataSet();
            //string[] Pname = new string[2];
            //object[] Pvalue = new object[2];
            //SqlDbType[] Ptype = new SqlDbType[2];

            try
            {

                //Pname[0] = "Appkey";
                //Ptype[0] = SqlDbType.VarChar;
                //Pvalue[0] = Appkey;

                //Pname[1] = "agtParameter";
                //Ptype[1] = SqlDbType.VarChar;
                //Pvalue[1] = agtParameter;
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Appkey", SqlDbType.VarChar) { Value = Appkey },
                    new SqlParameter("@agtParameter", SqlDbType.VarChar) { Value = agtParameter }
                };


                //ds = da.SelectRecords("RemoveAgentTagforWalkingCustomer", Pname, Pvalue, Ptype);
                ds = await _readWriteDao.SelectRecords("RemoveAgentTagforWalkingCustomer", sqlParameters);

                return ds;

            }
            catch (Exception ex)
            {
                //BAL.SCMException.logexception(ref ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return ds = null;
            }
            finally
            {
                //da = null;
                if (ds != null)
                    ds.Dispose();
                //Pname = null;
                //Pvalue = null;
                //Ptype = null;
            }
        }

        ///// <summary>
        ///// Generate FWB Message
        ///// </summary>
        //public void GenerateFWB(string DepartureAirport, string flightdest, string FlightNo, string FlightDate, DateTime itdate)
        //{

        //    string SitaMessageHeader = string.Empty, FWBMessageversion = string.Empty, Emailaddress = string.Empty, error = string.Empty, SFTPHeaderSITAddress = string.Empty;
        //    GenericFunction gf = new GenericFunction();
        //    //FBRMessageProcessor fbr = new FBRMessageProcessor();
        //    MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
        //    MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
        //    MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
        //    //DataSet dsData = GenericFunction.GetFlightInformationforFFM(DepartureAirport, FlightNo, FlightDate);
        //    DataSet dsData = gf.GetRecordforGenerateFBLMessage(DepartureAirport, flightdest, FlightNo, FlightDate);




        //    if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
        //    {
        //        // DataSet dsmessage = gf.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FWB", "AIR", DepartureAirport, "", "", string.Empty);

        //        DataSet dsmessage = gf.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FWB", "AIR", DepartureAirport, flightdest, FlightNo, string.Empty);
        //        if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
        //        {
        //            Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
        //            string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
        //            FWBMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();

        //            if (dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
        //                SitaMessageHeader = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());

        //            if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 1)
        //                SFTPHeaderSITAddress = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
        //        }



        //        //GetRecordforGenerateFBLMessage
        //        DataTable dt = new DataTable();
        //        //foreach (DataRow drdestination in dsData.Tables[0].Rows)
        //        //{
        //        //    MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
        //        //    dsData = GenericFunction.GetRecordforGenerateFFM(DepartureAirport, FlightNo, FlightDate, drdestination["DepartureStation"].ToString());
        //        //    if (dsData.Tables[3] != null && dsData.Tables[3].Rows.Count > 0)
        //        //        dt.Merge(dsData.Tables[3]);
        //        //}


        //        if (dsData.Tables[1] != null && dsData.Tables[1].Rows.Count > 0)
        //        {

        //            dt = dsData.Tables[1];
        //            dt = GenericFunction.SelectDistinct(dt, "AWBNumbers");

        //            //dt = SelectDistinct(dsData.Tables[3], "AWBNumber");

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                DataSet dsfwb = GetAWBRecordForGenerateFWBMessage(dt.Rows[i]["AWBNumbers"].ToString().Substring(4, 8), dt.Rows[i]["AWBNumbers"].ToString().Substring(0, 3));
        //                string fwbMsg = EncodeFWB(dsfwb, ref error, FWBMessageversion);

        //                try
        //                {
        //                    if (fwbMsg.Length > 3)
        //                    {
        //                        clsLog.WriteLogAzure(" in FWBMsg len >0" + DateTime.Now + SitaMessageHeader + Emailaddress);

        //                        if (SitaMessageHeader != "")
        //                        {
        //                            gf.SaveMessageOutBox("FWB", SitaMessageHeader + "\r\n" + fwbMsg, "SITAFTP", "SITAFTP", "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString());
        //                            clsLog.WriteLogAzure(" in SaveMessageOutBox SitaMessageHeader" + DateTime.Now);
        //                        }
        //                        if (SFTPHeaderSITAddress != "")
        //                        {
        //                            gf.SaveMessageOutBox("FWB", SFTPHeaderSITAddress + "\r\n" + fwbMsg, "SFTP", "SFTP", "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString());
        //                            clsLog.WriteLogAzure(" in SaveMessageOutBox SFTPHeaderSITAddress" + DateTime.Now);
        //                        }

        //                        if (Emailaddress != "")
        //                        {
        //                            gf.SaveMessageOutBox("FWB", fwbMsg, string.Empty, Emailaddress, "", "", "", "", dt.Rows[i]["AWBNumbers"].ToString());
        //                            clsLog.WriteLogAzure(" in SaveMessageOutBox  Emailaddress" + DateTime.Now);
        //                        }
        //                    }

        //                }
        //                catch (Exception) { }
        //            }

        //        }
        //    }
        //}

        ///// <summary>
        ///// Method generate the FWB for all the AWB's form the flight
        ///// Method added by prashant
        ///// </summary>
        //public void GenerateFWB(string PartnerCode, string DepartureAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate, string AWBNumbers)
        //{
        //    string FlightDestination = string.Empty;
        //    GenericFunction genericFunction = new GenericFunction();
        //    string MessageVersion = "8", SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, SFTPHeaderSITAddress = string.Empty;
        //    DataSet dsData = new DataSet();

        //    string[] awbArrray = AWBNumbers.Split(',');
        //    for (int i = 0; i < awbArrray.Length; i++)
        //    {
        //        DataSet dsfwb = GetAWBRecordForGenerateFWBMessage(awbArrray[i].Substring(4, 8).ToString(), awbArrray[i].Substring(0, 3).ToString());


        //        string awbDestination = dsfwb.Tables[0].Rows[0]["DestinationCode"].ToString();
        //        DataSet dsconfiguration = genericFunction.GetSitaAddressandMessageVersion(PartnerCode, "FWB", "AIR", "", awbDestination, FlightNo, string.Empty);
        //        if (dsconfiguration != null && dsconfiguration.Tables[0].Rows.Count > 0)
        //        {
        //            strEmailid = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
        //            MessageVersion = dsconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
        //            strSITAHeaderType = dsconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString();
        //            if (dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
        //            {
        //                SitaMessageHeader = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
        //            }
        //            if (dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
        //            {
        //                SFTPHeaderSITAddress = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
        //            }
        //        }
        //        string fwbMsg = EncodeFWB(dsfwb, ref error, MessageVersion);
        //        try
        //        {
        //            if (fwbMsg.Length > 3)
        //            {
        //                if (SitaMessageHeader != "")
        //                    genericFunction.SaveMessageOutBox("SITA:FWB", SitaMessageHeader.ToString() + "\r\n" + fwbMsg, "", "SITAFTP", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i]);

        //                if (SFTPHeaderSITAddress != "")
        //                    genericFunction.SaveMessageOutBox("SITA:FWB", SFTPHeaderSITAddress.ToString() + "\r\n" + fwbMsg, "", "SFTP", DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i]);

        //                if (strEmailid != "")
        //                    genericFunction.SaveMessageOutBox("FWB", fwbMsg, "", strEmailid, DepartureAirport, FlightDestination, FlightNo, FlightDate.ToString(), awbArrray[i]);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            clsLog.WriteLogAzure(ex);
        //        }
        //    }
        //}
        #endregion Public Methods

        #region :: Private Methods::
        private async Task<DataSet> GetAWBStatus(string AWBPrefix, string AWBNumber, string AWBOrigin, string AWBDestination, string UpdatedBy)
        {
            DataSet dsAWBStatus = new DataSet();
            try
            {
                //string[] paramName = new string[] { "AWBPrefix", "AWBNumber", "AWBOrigin", "AWBDestination", "UpdatedBy" };
                //SqlDbType[] paramSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //object[] paramValue = new string[] { AWBPrefix, AWBNumber, AWBOrigin, AWBDestination, UpdatedBy };
                //SQLServer sqlServer = new SQLServer();

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                    new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = AWBOrigin },
                    new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = AWBDestination },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = UpdatedBy }
                };

                //dsAWBStatus = sqlServer.SelectRecords("Messaging.GetAWBStatus", paramName, paramValue, paramSqlType);
                dsAWBStatus = await _readWriteDao.SelectRecords("Messaging.GetAWBStatus", sqlParameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return dsAWBStatus;
        }

        private string[] StringSplitter(string str)
        {
            char[] arr = str.ToCharArray();
            string[] strarr = new string[arr.Length];

            try
            {
                if (str.Length > 0)
                {
                    int k = 0;
                    char lastchr = 'A';
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
                }
            }
            catch (Exception)
            {
                strarr = null;
            }
            return strarr;
        }
        #endregion Private Methods
    }
}
