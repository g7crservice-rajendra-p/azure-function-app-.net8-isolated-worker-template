#region FFR Message Processor Class Description
/* FFR Message Processor Class Description.
      * Company             :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright             :   Copyright © 2016 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose               : 
      * Created By           :   Badiuzzaman Khan
      * Created On          :   2016-04-02
      * Approved By        :
      * Approved Date     :
      * Modified By          :  
      * Modified On         :   
      * Description          :   
     */
#endregion

using System;
using System.Linq;
using System.Text;
using System.Data;
using QID.DataAccess;
using System.Configuration;
using System.Data.SqlClient;

namespace QidWorkerRole
{
    public class FFRMessageProcessor
    {
        #region :: Variable Declaration ::
        SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();
        static string unloadingportsequence = string.Empty;
        static string uldsequencenum = string.Empty;
        static string awbref = string.Empty;
        const string PAGE_NAME = "cls_Encode_Decode";
        #endregion Variable Declaration

        #region :: Public Methods ::
        public bool DecodeFFRReceiveMessage(int srno, string ffrmsg, ref MessageData.ffrinfo data, ref MessageData.ULDinfo[] uld, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.FltRoute[] fltroute, ref MessageData.dimensionnfo[] objDimension, out string errorMessage)
        {
            errorMessage = string.Empty;
            const string FUN_NAME = "DecodeFFRMessage";
            bool flag = false;
            string lastrec = "NA";
            string AWBPrefix = "", AWBNumber = "";
            int line = 0;


            try
            {
                if (ffrmsg.StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
                {
                    ffrmsg = ffrmsg.Replace("\r\n", "$");
                    if (!ffrmsg.Contains("$REF"))
                    {
                        errorMessage = "Invalid FFR format";
                        return false;
                    }
                    string[] str = ffrmsg.Split('$');

                    if (str.Length >= 3)
                    {
                        for (int i = 0; i < str.Length; i++)
                        {
                            flag = true;

                            #region Line 1
                            if (str[i].StartsWith("FFR", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    data.ffrversionnum = msg[1];
                                }
                                catch (Exception ex) { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME, "Error while reading ffr header.", ""); }
                            }
                            #endregion

                            #region Line Consigment Details
                            if (i > 0)
                            {
                                string strconsigmentdetails = str[i];
                                string[] msg = str[i].Split('/');

                                // try
                                // {
                                if ((i + 1) < str.Length)
                                {
                                    if (str[i + 1].StartsWith("/"))
                                    {
                                        strconsigmentdetails = str[i] + str[i + 1];
                                        msg = strconsigmentdetails.Split('/');
                                    }
                                }
                                // }
                                // catch (Exception ex) { }

                                if (msg[0].Contains('-'))
                                {
                                    //Decode consigment info
                                    DecodeConsigmentDetails(strconsigmentdetails, ref consinfo, ref AWBPrefix, ref AWBNumber);
                                }
                            }
                            #endregion

                            #region Line 3 flight details
                            if (i > 1 && !str[i].StartsWith("/") && !str[i].Contains('-'))
                            {
                                try
                                {
                                    MessageData.FltRoute flight = new MessageData.FltRoute("");
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 0 && msg[0].Length > 3)
                                    {
                                        flight.carriercode = msg[0].Substring(0, 2);
                                        flight.fltnum = msg[0].Substring(2);
                                        flight.date = msg[1].Substring(0, 2);
                                        flight.month = msg[1].Substring(2);
                                        flight.fltdept = msg[2].Substring(0, 3);
                                        flight.fltarrival = msg[2].Substring(3);
                                        try
                                        {
                                            flight.spaceallotmentcode = msg[3].Length > 0 ? msg[3] : "";
                                            if (msg[4].Length > 0)
                                            {
                                                flight.allotidentification = msg[4].ToString();
                                            }

                                        }
                                        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                                        Array.Resize(ref fltroute, fltroute.Length + 1);
                                        fltroute[fltroute.Length - 1] = flight;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    clsLog.WriteLogAzure(ex);
                                    continue;
                                }
                            }
                            #endregion

                            #region Line 4 ULD Specification
                            if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    //int uldnum = 0;
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        data.noofuld = msg[1];
                                        MessageData.ULDinfo uldinfo = new MessageData.ULDinfo("");
                                        if (int.Parse(msg[1]) > 0)
                                        {
                                            for (int k = 2; k < msg.Length; k += 2)
                                            {
                                                string[] splitstr = msg[k].Split('-');
                                                uldinfo.uldtype = splitstr[0].Substring(0, 3);
                                                uldinfo.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 5);
                                                uldinfo.uldowner = splitstr[0].Substring(splitstr[0].Length - 2, 2);
                                                uldinfo.uldloadingindicator = splitstr.Length > 1 ? splitstr[1] : "";
                                                uldinfo.uldweightcode = msg[k + 1].Substring(0, 1);
                                                uldinfo.uldweight = msg[k + 1].Substring(1);

                                                Array.Resize(ref uld, uld.Length + 1);
                                                uld[uld.Length - 1] = uldinfo;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
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
                                    line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        data.specialservicereq1 = msg[1];
                                    }

                                }
                                catch (Exception ex)
                                {
                                    clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
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
                                    line = 0;
                                    if (msg[1].Length > 0)
                                    {
                                        data.otherserviceinfo1 = msg[1];
                                    }
                                }
                                catch (Exception ex) { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
                            }
                            #endregion

                            #region Line 7 booking reference
                            if (str[i].StartsWith("REF", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1 && msg[1].Length == 7)
                                    {
                                            data.bookingrefairport = msg[1].Substring(0, 3);
                                            data.officefundesignation = msg[1].Substring(3, 2);
                                            data.companydesignator = msg[1].Substring(5, 2);
                                    }
                                    if (msg.Length > 2)
                                    {
                                        data.participentidetifier = msg[2].Length > 0 ? msg[2] : "";
                                    }
                                    if (msg.Length > 3)
                                    {
                                        data.participentcode = msg[3].Length > 0 ? msg[3] : "";
                                    }
                                    if (msg.Length > 4)
                                    {
                                        data.participentairportcity = msg[4].Length > 0 ? msg[4] : "";
                                    }
                                }
                                catch (Exception ex)
                                { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
                            }

                            #endregion

                            #region Line 8 Dimendion info
                            if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    lastrec = "DIM";
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length == 4)
                                    {
                                        MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                        if (msg[1].Substring(0, 1).Equals("K", StringComparison.OrdinalIgnoreCase) || msg[1].Substring(0, 1).Equals("L", StringComparison.OrdinalIgnoreCase))
                                        {
                                            dimension.weightcode = msg[1].Substring(0, 1);
                                            dimension.weight = msg[1].Substring(1);
                                        }
                                        if (msg.Length > 0)
                                        {
                                            string select = "";
                                            for (int n = 0; n < msg.Length; n++)
                                            {
                                                if (msg[n].Contains('-'))
                                                {
                                                    select = msg[n];
                                                }
                                            }
                                            string[] dimstr = select.Split('-');
                                            dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                            dimension.length = dimstr[0].Substring(3);
                                            dimension.width = dimstr[1];
                                            dimension.height = dimstr[2];
                                        }
                                        try
                                        {
                                            int val;
                                            if (int.TryParse(msg[msg.Length - 1], out val))
                                            {
                                                dimension.piecenum = msg[msg.Length - 1];
                                            }
                                        }
                                        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                                        Array.Resize(ref objDimension, objDimension.Length + 1);
                                        objDimension[objDimension.Length - 1] = dimension;

                                    }
                                    else if (msg.Length > 0)
                                    {
                                        GenericFunction genericFunction = new GenericFunction();
                                        errorMessage = "Incomplete DIMS line";
                                        genericFunction.UpdateErrorMessageToInbox(srno, errorMessage, "FFR", false, "", false);
                                        return false;
                                    }
                                }
                                catch (Exception e8) { clsLog.WriteLogAzure(e8, PAGE_NAME, FUN_NAME); }
                            }
                            #endregion

                            #region Line 9 Product information
                            if (str[i].StartsWith("PID", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        data.servicecode = msg[1];
                                        data.rateclasscode = msg[2];
                                        data.commoditycode = msg[3];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                                }
                            }
                            #endregion

                            #region Line 10 Shipper Infor
                            if (str[i].StartsWith("SHP", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    line = 0;
                                    if (msg.Length > 1)
                                    {
                                        data.shipperaccnum = msg[1];

                                    }
                                }
                                catch (Exception ex)
                                { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
                            }
                            #endregion

                            #region Line 11 Consignee
                            if (str[i].StartsWith("CNE", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    line = 0;
                                    if (msg.Length > 1)
                                    {
                                        data.consaccnum = msg[1];
                                    }
                                }
                                catch (Exception ex)
                                { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
                            }
                            #endregion

                            #region Line 12 Customer Identification
                            if (str[i].StartsWith("CUS", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    lastrec = msg[0];
                                    if (msg.Length > 1)
                                    {
                                        data.custaccnum = msg[1].Length > 0 ? msg[1] : "";
                                        data.iatacargoagentcode = msg[2].Length > 0 ? msg[2] : "";
                                        data.cargoagentcasscode = msg[3].Length > 0 ? msg[3] : "";
                                        data.participentidetifier = msg[4].Length > 0 ? msg[4] : "";

                                    }
                                }
                                catch (Exception e12) { clsLog.WriteLogAzure(e12, PAGE_NAME, FUN_NAME); }
                            }
                            #endregion

                            #region Line 13 shipment refence info
                            if (str[i].StartsWith("SRI", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1)
                                    {
                                        data.shiprefnum = msg[1].Length > 0 ? msg[1] : null;
                                        data.supplemetryshipperinfo1 = msg[2].Length > 0 ? msg[2] : null;
                                        data.supplemetryshipperinfo2 = msg[3].Length > 0 ? msg[3] : null;
                                    }
                                }
                                catch (Exception ex) { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
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
                                        data.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                    }
                                    if (lastrec == "OSI")
                                    {

                                        if (msg[1].Length > 0)
                                        {
                                            data.otherserviceinfo2 = data.otherserviceinfo2 + msg[1];
                                        }

                                    }
                                    #region SHP Data
                                    if (lastrec == "SHP")
                                    {
                                        line++;
                                        if (line == 1)
                                        {
                                            data.shippername = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                        if (line == 2)
                                        {
                                            data.shipperadd = msg[1].Length > 0 ? msg[1] : "";

                                        }
                                        if (line == 3)
                                        {
                                            data.shipperplace = msg[1].Length > 0 ? msg[1] : "";
                                            data.shipperstate = msg[2].Length > 0 ? msg[2] : "";
                                        }
                                        if (line == 4)
                                        {
                                            data.shippercountrycode = msg[1].Length > 0 ? msg[1] : "";
                                            data.shipperpostcode = msg[2].Length > 0 ? msg[2] : "";
                                            data.shippercontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                            data.shippercontactnum = msg[4].Length > 0 ? msg[4] : "";

                                        }

                                    }
                                    #endregion

                                    #region CNE Data
                                    if (lastrec == "CNE")
                                    {
                                        line++;
                                        if (line == 1)
                                        {
                                            data.consname = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                        if (line == 2)
                                        {
                                            data.consadd = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                        if (line == 3)
                                        {
                                            data.consplace = msg[1].Length > 0 ? msg[1] : "";
                                            data.consstate = msg[2].Length > 0 ? msg[2] : "";
                                        }
                                        if (line == 4)
                                        {
                                            data.conscountrycode = msg[1].Length > 0 ? msg[1] : "";
                                            data.conspostcode = msg[2].Length > 0 ? msg[2] : "";
                                            data.conscontactidentifier = msg[3].Length > 0 ? msg[3] : "";
                                            data.conscontactnum = msg[4].Length > 0 ? msg[4] : "";
                                        }

                                    }
                                    #endregion

                                    #region CUS data
                                    if (lastrec == "CUS")
                                    {
                                        line++;
                                        if (line == 1)
                                        {
                                            data.custname = msg[1].Length > 0 ? msg[1] : "";

                                        }
                                        if (line == 2)
                                        {
                                            data.custplace = msg[1].Length > 0 ? msg[1] : "";
                                        }
                                    }
                                    #endregion

                                    #region DIM info
                                    if (lastrec.Equals("DIM", StringComparison.OrdinalIgnoreCase))
                                    {
                                        try
                                        {
                                            if (msg.Length > 1)
                                            {
                                                MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                                dimension.weightcode = msg[1].Substring(0, 1);
                                                dimension.weight = msg[1].Substring(1);
                                                if (msg.Length > 0)
                                                {
                                                    string[] dimstr = msg[2].Split('-');
                                                    dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                    dimension.length = dimstr[0].Substring(3);
                                                    dimension.width = dimstr[1];
                                                    dimension.height = dimstr[2];
                                                }
                                                try
                                                {
                                                    dimension.piecenum = msg[3];
                                                }
                                                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                                                Array.Resize(ref objDimension, objDimension.Length + 1);
                                                objDimension[objDimension.Length - 1] = dimension;

                                            }
                                        }
                                        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
                            }
                            #endregion
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
            }
            return flag;
        }

        public void DecodeConsigmentDetails(string inputstr, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber)
        {
            const string FUN_NAME = "Decodeconsigmentdetails";
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                string[] msg = inputstr.Split('/');
                string[] decmes = msg[0].Split('-');
                //consinfo[num] = new MessageData.consignmnetinfo("");
                consig.airlineprefix = decmes[0];
                string[] sptarr = stringsplitter(decmes[1]);
                if (sptarr.Length > 0)
                {
                    try
                    {
                        consig.awbnum = sptarr[0];
                        if (sptarr[1].Length == 3)
                        {
                            consig.origin = "";
                            consig.dest = sptarr[1];
                        }
                        else if (sptarr[1].Length == 6)
                        {
                            consig.origin = sptarr[1].Substring(0, 3);
                            consig.dest = sptarr[1].Substring(3); ;
                        }
                    }
                    catch (Exception ex)
                    {
                        clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                else
                {
                    try
                    {
                        consig.awbnum = decmes[1].Substring(0, decmes[1].Length - 6);
                        consig.origin = decmes[1].Substring(decmes[1].Length - 6, 3);
                        consig.dest = decmes[1].Substring(decmes[1].Length - 3, 3);
                    }
                    catch (Exception ex)
                    {
                        clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                //1
                if (msg[1].Length > 0)
                {
                    try
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
                        consig.consigntype = strarr[0];
                        consig.pcscnt = strarr[1];//int.Parse(strarr[1]);
                        consig.weightcode = strarr[2];
                        consig.weight = strarr[3];//float.Parse(strarr[3]);
                        if (consig.consigntype.Equals("T"))
                        {//total pieces
                            consig.numshp = consig.pcscnt;
                        }
                        for (k = 4; k < strarr.Length; k += 2)
                        {
                            if (strarr[k] != null)
                            {
                                if (strarr[k] == "T")
                                {
                                    consig.shpdesccode = strarr[k];
                                    consig.numshp = strarr[k + 1];
                                    k = strarr.Length + 1;
                                    //if (consig.consigntype.Equals("S")) 
                                    //{
                                    //    consig.pcscnt = consig.numshp;
                                    //}
                                }
                                else if (strarr[k] == "DG")
                                {
                                    consig.densityindicator = strarr[k];
                                    consig.densitygrp = strarr[k + 1];
                                }
                                else
                                {
                                    consig.volumecode = strarr[k];
                                    consig.volumeamt = strarr[k + 1];
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                if (msg.Length > 2)
                {
                    try
                    {

                        consig.manifestdesc = msg[2];
                    }
                    catch (Exception ex)
                    {
                        clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                    }

                }
                if (msg.Length > 3)
                {
                    try
                    {
                        consig.splhandling = "";
                        for (int j = 3; j < msg.Length; j++)
                            consig.splhandling = consig.splhandling + msg[j] + ",";
                        if (consig.splhandling.Length > 0)
                        {
                            consig.splhandling = consig.splhandling.Substring(0, consig.splhandling.Length - 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                    }
                }
                try
                {
                    if (unloadingportsequence.Length > 0)
                        consig.portsequence = unloadingportsequence;
                    if (uldsequencenum.Length > 0)
                        consig.uldsequence = uldsequencenum;
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                awbprefix = consig.airlineprefix;
                awbnumber = consig.awbnum;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                consinfo[consinfo.Length - 1] = consig;
                awbref = consinfo.Length.ToString();
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
            }
        }

        public string[] stringsplitter(string str)
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

        public string EncodeFFRMessageForSend(MessageData.ffrinfo data, MessageData.ULDinfo[] uld)
        {
            string ffr = null;
            const String FUN_NAME = "encodeFFRforsend";
            try
            {
                #region Line 1
                string line1 = "FFR" + "/" + data.ffrversionnum;
                #endregion

                #region Line 2 AWB Data

                string line2 = string.Empty;
                #endregion

                #region Line 3 Flight Schedule
                string line3 = "";
                #endregion

                #region Line 4
                string line4 = "";
                if (data.noofuld.Length > 0)
                {
                    line4 = "ULD/" + data.noofuld + "/";
                    string uldinfo = null;
                    for (int i = 0; i < int.Parse(data.noofuld); i++)
                    {
                        uldinfo = null;
                        uldinfo = uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "-" + uld[i].uldloadingindicator + "/" + uld[i].uldweightcode + uld[i].uldweight;
                        if (uldinfo.Length > 2)
                            line4 = line4 + uldinfo + "/";
                    }
                }
                #endregion

                #region Line 5
                string line5 = "";
                if (data.specialservicereq1.Length > 0 || data.specialservicereq2.Length > 0)
                {
                    line5 = "SSR/" + data.specialservicereq1 + "\r\n" + "/" + data.specialservicereq2;
                }
                #endregion

                #region Line 6
                string line6 = "";
                if (data.otherserviceinfo1.Length > 0 || data.otherserviceinfo2.Length > 0)
                {
                    line6 = "SSR/" + data.otherserviceinfo1 + "\r\n" + "/" + data.otherserviceinfo2;
                }
                #endregion

                #region Line 7
                string line7 = "";
                line7 = "REF/" + data.bookingrefairport + data.officefundesignation + data.companydesignator;
                if (data.bookingfileref.Length > 0)
                {
                    line7 = line7 + "/" + data.bookingfileref;
                }
                if (data.participentidetifier.Length > 0)
                {
                    line7 = line7 + "/" + data.participentidetifier;
                }
                if (data.participentcode.Length > 0)
                {
                    line7 = line7 + "/" + data.participentcode;
                }
                if (data.participentairportcity.Length > 0)
                {
                    line7 = line7 + "/" + data.participentairportcity;
                }
                #endregion

                #region Line 8 Dimension Info
                string line8 = "";


                #endregion

                #region Line 9
                string line9 = "";
                if (data.servicecode.Length > 0 || data.rateclasscode.Length > 0 || data.commoditycode.Length > 0 || data.producttype.Length > 0)
                {
                    //line9 = "PID/" + data.servicecode + "/" + data.rateclasscode + "/" + data.commoditycode;
                    line9 = "PID/" + data.producttype;
                }

                #endregion

                #region Line 10
                string line10 = "";
                string str1 = "", str2 = "", str3 = "", str4 = "";
                try
                {
                    if (data.shippername.Length > 0)
                    {
                        str1 = "/" + data.shippername;
                    }
                    if (data.shipperadd.Length > 0)
                    {
                        str2 = "/" + data.shipperadd;
                    }

                    if (data.shipperplace.Length > 0 || data.shipperstate.Length > 0)
                    {
                        str3 = "/" + data.shipperplace + "/" + data.shipperstate;
                    }
                    if (data.shippercountrycode.Length > 0 || data.shipperpostcode.Length > 0 || data.shippercontactidentifier.Length > 0 || data.shippercontactnum.Length > 0)
                    {
                        str4 = "/" + data.shippercountrycode + "/" + data.shipperpostcode + "/" + data.shippercontactidentifier + "/" + data.shippercontactnum;
                    }

                    if (data.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line10 = "SHP/" + data.shipperaccnum;
                        if (str4.Length > 0)
                        {
                            line10 = line10.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME); }
                #endregion

                #region Line 11
                string line11 = "";
                str1 = "";
                str2 = "";
                str3 = "";
                str4 = "";
                try
                {
                    if (data.consname.Length > 0)
                    {
                        str1 = "/" + data.consname;
                    }
                    if (data.consadd.Length > 0)
                    {
                        str2 = "/" + data.consadd;
                    }

                    if (data.consplace.Length > 0 || data.consstate.Length > 0)
                    {
                        str3 = "/" + data.custplace + "/" + data.consstate;
                    }
                    if (data.conscountrycode.Length > 0 || data.conspostcode.Length > 0 || data.conscontactidentifier.Length > 0 || data.conscontactnum.Length > 0)
                    {
                        str4 = "/" + data.conscountrycode + "/" + data.conspostcode + "/" + data.conscontactidentifier + "/" + data.conscontactnum;
                    }

                    if (data.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line11 = "CNE/" + data.consaccnum;
                        if (str4.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                #endregion

                #region Line 12
                string line12 = "";
                str1 = "";
                str2 = "";
                try
                {
                    if (data.custname.Length > 0)
                    {
                        str1 = "/" + data.custname;
                    }
                    if (data.custplace.Length > 0)
                    {
                        str2 = "/" + data.custplace;
                    }
                    if (data.custaccnum.Length > 0 || data.iatacargoagentcode.Length > 0 || data.cargoagentcasscode.Length > 0 || data.participentidetifier.Length > 0 || str1.Length > 0 || str2.Length > 0)
                    {
                        //line12 = "CUS/" + data.shipperaccnum + "/" + data.iatacargoagentcode + "/" + data.cargoagentcasscode + "/" + data.participentidetifier;
                        line12 = "CUS//" + data.iatacargoagentcode + "/" + data.cargoagentcasscode + "/" + data.participentidetifier;
                        if (str2.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                #endregion

                #region Line 13
                string line13 = "";
                if (data.shiprefnum.Length > 0 || data.supplemetryshipperinfo1.Length > 0 || data.supplemetryshipperinfo2.Length > 0)
                {
                    line13 = "SRI/" + data.shiprefnum + "/" + data.supplemetryshipperinfo1 + "/" + data.supplemetryshipperinfo2;
                }
                #endregion

                #region BuildFFR
                ffr = line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
                if (line4.Length > 0)
                {
                    ffr = ffr + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    ffr = ffr + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    ffr = ffr + "\r\n" + line6.Trim('/');
                }
                if (line7.Length > 0)
                {
                    ffr = ffr + "\r\n" + line7.Trim('/');
                }
                if (line8.Length > 0)
                {
                    ffr = ffr + "\r\n" + line8.Trim('/');
                }
                if (line9.Length > 0)
                {
                    ffr = ffr + "\r\n" + line9.Trim('/');
                }
                if (line10.Length > 0)
                {
                    ffr = ffr + "\r\n" + line10.Trim('/');
                }
                if (line11.Length > 0)
                {
                    ffr = ffr + "\r\n" + line11.Trim('/');
                }
                if (line12.Length > 0)
                {
                    ffr = ffr + "\r\n" + line12.Trim('/');
                }
                if (line13.Length > 0)
                {
                    ffr = ffr + "\r\n" + line13.Trim('/');
                }
                #endregion

            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
                ffr = "ERR";
            }
            return ffr;
        }

        public string EncodeFFRMessageForSend(ref MessageData.ffrinfo data, ref MessageData.ULDinfo[] uld, ref MessageData.consignmnetinfo consigment, ref MessageData.FltRoute[] FltRoute, ref MessageData.dimensionnfo[] dimension)
        {
            string ffr = null;
            const string FUN_NAME = "EncodeFFRMessageForsend";
            try
            {
                #region Line 1
                string line1 = "FFR" + "/" + data.ffrversionnum;
                #endregion

                #region Line 2 AWB Data
                //    string splhandling = "";

                string line2 = consigment.airlineprefix + "-" + consigment.awbnum + consigment.origin + consigment.dest + "/" + consigment.consigntype + consigment.pcscnt + consigment.weightcode + consigment.weight + consigment.volumecode + consigment.volumeamt + consigment.densityindicator + consigment.densitygrp + consigment.shpdesccode + consigment.numshp + "/" + consigment.manifestdesc + consigment.splhandling;
                #endregion

                #region Line 3 Flight Schedule
                string line3 = "";
                if (FltRoute.Length > 0)
                {
                    for (int i = 0; i < FltRoute.Length; i++)
                    {
                        line3 = line3 + FltRoute[i].carriercode + FltRoute[i].fltnum + "/" + FltRoute[i].date + FltRoute[i].month + "/" + FltRoute[i].fltdept + FltRoute[i].fltarrival + "/" + FltRoute[i].spaceallotmentcode + (FltRoute[i].allotidentification.Length > 0 ? ("/" + FltRoute[i].allotidentification) : "") + "$";
                    }
                }
                line3 = line3.Trim('$');
                line3 = line3.Replace("$", "\r\n");
                //
                #endregion

                #region Line 4
                string line4 = "";
                if (data.noofuld.Length > 0)
                {
                    line4 = "ULD/" + data.noofuld + "/";
                    string uldinfo = null;
                    for (int i = 0; i < int.Parse(data.noofuld); i++)
                    {
                        uldinfo = null;
                        uldinfo = uld[i].uldtype + uld[i].uldsrno + uld[i].uldowner + "-" + uld[i].uldloadingindicator + "/" + uld[i].uldweightcode + uld[i].uldweight;
                        if (uldinfo.Length > 2)
                            line4 = line4 + uldinfo + "/";
                    }
                }
                #endregion

                #region Line 5
                string line5 = "";
                if (data.specialservicereq1.Length > 0 || data.specialservicereq2.Length > 0)
                {
                    line5 = "SSR/" + data.specialservicereq1 + "\r\n" + "/" + data.specialservicereq2;
                }
                #endregion

                #region Line 6
                string line6 = "";
                if (data.otherserviceinfo1.Length > 0 || data.otherserviceinfo2.Length > 0)
                {
                    line6 = "SSR/" + data.otherserviceinfo1 + "\r\n" + "/" + data.otherserviceinfo2;
                }
                #endregion

                #region Line 7
                string line7 = "";
                line7 = "REF/" + data.bookingrefairport + data.officefundesignation + data.companydesignator;
                if (data.bookingfileref.Length > 0)
                {
                    line7 = line7 + "/" + data.bookingfileref;
                }
                if (data.participentidetifier.Length > 0)
                {
                    line7 = line7 + "/" + data.participentidetifier;
                }
                if (data.participentcode.Length > 0)
                {
                    line7 = line7 + "/" + data.participentcode;
                }
                if (data.participentairportcity.Length > 0)
                {
                    line7 = line7 + "/" + data.participentairportcity;
                }
                #endregion

                #region Line 8 Dimension Info
                string line8 = "";
                if (dimension.Length > 0)
                {
                    for (int i = 0; i < dimension.Length; i++)
                    {
                        if (dimension[i].weight.Length > 0 || dimension[i].length.Length > 0 || dimension[i].width.Length > 0 || dimension[i].height.Length > 0 || dimension[i].piecenum.Length > 0)
                        {
                            line8 = line8 + dimension[i].weightcode + dimension[i].weight + "/" + dimension[i].mesurunitcode + dimension[i].length + "-" + dimension[i].width + "-" + dimension[i].height + "/" + dimension[i].piecenum + "$";
                        }
                    }
                }
                line8 = line8.Trim('$');
                if (line8.Length > 0)
                {
                    line8 = "DIM/" + line8.Replace("$", "\r\n");
                }
                #endregion

                #region Line 9
                string line9 = "";
                if (data.servicecode.Length > 0 || data.rateclasscode.Length > 0 || data.commoditycode.Length > 0)
                {
                    line9 = "PID/" + data.servicecode + "/" + data.rateclasscode + "/" + data.commoditycode;
                }
                #endregion

                #region Line 10
                string line10 = "";
                string str1 = "", str2 = "", str3 = "", str4 = "";
                try
                {
                    if (data.shippername.Length > 0)
                    {
                        str1 = "/" + data.shippername;
                    }
                    if (data.shipperadd.Length > 0)
                    {
                        str2 = "/" + data.shipperadd;
                    }

                    if (data.shipperplace.Length > 0 || data.shipperstate.Length > 0)
                    {
                        str3 = "/" + data.shipperplace + "/" + data.shipperstate;
                    }
                    if (data.shippercountrycode.Length > 0 || data.shipperpostcode.Length > 0 || data.shippercontactidentifier.Length > 0 || data.shippercontactnum.Length > 0)
                    {
                        str4 = "/" + data.shippercountrycode + "/" + data.shipperpostcode + "/" + data.shippercontactidentifier + "/" + data.shippercontactnum;
                    }

                    if (data.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line10 = "SHP/" + data.shipperaccnum;
                        if (str4.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line10 = line10.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                #endregion

                #region Line 11
                string line11 = "";
                str1 = "";
                str2 = "";
                str3 = "";
                str4 = "";
                try
                {
                    if (data.consname.Length > 0)
                    {
                        str1 = "/" + data.consname;
                    }
                    if (data.consadd.Length > 0)
                    {
                        str2 = "/" + data.consadd;
                    }

                    if (data.consplace.Length > 0 || data.consstate.Length > 0)
                    {
                        str3 = "/" + data.custplace + "/" + data.consstate;
                    }
                    if (data.conscountrycode.Length > 0 || data.conspostcode.Length > 0 || data.conscontactidentifier.Length > 0 || data.conscontactnum.Length > 0)
                    {
                        str4 = "/" + data.conscountrycode + "/" + data.conspostcode + "/" + data.conscontactidentifier + "/" + data.conscontactnum;
                    }

                    if (data.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line11 = "CNE/" + data.consaccnum;
                        if (str4.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line11 = line11.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                #endregion

                #region Line 12
                string line12 = "";
                str1 = "";
                str2 = "";
                try
                {
                    if (data.custname.Length > 0)
                    {
                        str1 = "/" + data.custname;
                    }
                    if (data.custplace.Length > 0)
                    {
                        str2 = "/" + data.custplace;
                    }
                    if (data.custaccnum.Length > 0 || data.iatacargoagentcode.Length > 0 || data.cargoagentcasscode.Length > 0 || data.participentidetifier.Length > 0 || str1.Length > 0 || str2.Length > 0)
                    {
                        //line12 = "CUS/" + data.shipperaccnum + "/" + data.iatacargoagentcode + "/" + data.cargoagentcasscode + "/" + data.participentidetifier;
                        line12 = "CUS//" + data.iatacargoagentcode + "/" + data.cargoagentcasscode + "/" + data.participentidetifier;
                        if (str2.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line12 = line12.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                #endregion

                #region Line 13
                string line13 = "";
                if (data.shiprefnum.Length > 0 || data.supplemetryshipperinfo1.Length > 0 || data.supplemetryshipperinfo2.Length > 0)
                {
                    line13 = "SRI/" + data.shiprefnum + "/" + data.supplemetryshipperinfo1 + "/" + data.supplemetryshipperinfo2;
                }
                #endregion

                #region BuildFFR
                ffr = line1.Trim('/') + "\r\n" + line2.Trim('/') + "\r\n" + line3.Trim('/');
                if (line4.Length > 0)
                {
                    ffr = ffr + "\r\n" + line4.Trim('/');
                }
                if (line5.Length > 0)
                {
                    ffr = ffr + "\r\n" + line5.Trim('/');
                }
                if (line6.Length > 0)
                {
                    ffr = ffr + "\r\n" + line6.Trim('/');
                }
                if (line7.Length > 0)
                {
                    ffr = ffr + "\r\n" + line7.Trim('/');
                }
                if (line8.Length > 0)
                {
                    ffr = ffr + "\r\n" + line8.Trim('/');
                }
                if (line9.Length > 0)
                {
                    ffr = ffr + "\r\n" + line9.Trim('/');
                }
                if (line10.Length > 0)
                {
                    ffr = ffr + "\r\n" + line10.Trim('/');
                }
                if (line11.Length > 0)
                {
                    ffr = ffr + "\r\n" + line11.Trim('/');
                }
                if (line12.Length > 0)
                {
                    ffr = ffr + "\r\n" + line12.Trim('/');
                }
                if (line13.Length > 0)
                {
                    ffr = ffr + "\r\n" + line13.Trim('/');
                }
                #endregion

            }
            catch (Exception ex)
            {
                ffr = "ERR";
                clsLog.WriteLogAzure(ex, PAGE_NAME, FUN_NAME);
            }
            return ffr;
        }

        public bool ElcodeFFRAndPrepareMsg(DataSet dsFFR, string fromEmailID, string toEmailID)
        {

            bool flag = false;
            try
            {

                MessageData.ffrinfo objFFRInfo = new MessageData.ffrinfo("");
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];

                if (dsFFR != null)
                {
                    if (dsFFR.Tables.Count > 0)
                    {
                        if (dsFFR.Tables[0].Rows.Count > 0)
                        {
                            DataRow drAWBRateMaster = dsFFR.Tables[0].Rows[0];
                            DataRow drAWBRouteMaster = dsFFR.Tables[1].Rows[0];
                            DataRow drAWBShipperConsigneeDetails = dsFFR.Tables[2].Rows[0];
                            DataRow drAWBSummaryMaster = dsFFR.Tables[3].Rows[0];


                            DateTime dtTo = new DateTime();
                            //DateTime dtfrom = new DateTime();
                            string dt = (drAWBRouteMaster["FltDate"].ToString());


                            //dt = dt + " " + DateTime.Now.ToShortTimeString();
                            //dtfrom = DateTime.ParseExact(dt,"dd-MM-yyyy",null);
                            dtTo = DateTime.ParseExact(drAWBRouteMaster["FltDate"].ToString(), "dd/MM/yyyy", null);
                            //ToDt = dt.ToString();
                            string day = dt.Substring(0, 2);
                            string mon = dtTo.ToString("MMM");
                            //string mon = dt.Substring(3, 2);
                            string yr = dt.Substring(6, 4);

                            #region PrepareFFRStructureObject

                            //line 1 
                            objFFRInfo.ffrversionnum = "6";

                            #region Consigment Section
                            //line 2
                            //objFFRInfo.airlineprefix= drAWBSummaryMaster["AWBPrefix"].ToString();
                            //objFFRInfo.awbnum=drAWBSummaryMaster["AWBNumber"].ToString();;
                            //objFFRInfo.origin = drAWBSummaryMaster["OriginCode"].ToString();
                            //objFFRInfo.dest=drAWBSummaryMaster["DestinationCode"].ToString(); ;
                            //objFFRInfo.consigntype="T";
                            //objFFRInfo.pcscnt = drAWBRateMaster["Pieces"].ToString();
                            //objFFRInfo.weightcode="K";
                            //objFFRInfo.weight = drAWBSummaryMaster["GrossWeight"].ToString();
                            //objFFRInfo.volumecode="";
                            //objFFRInfo.volumeamt="";
                            //objFFRInfo.densityindicator="";
                            //objFFRInfo.densitygrp="";
                            //objFFRInfo.shpdesccode="";
                            //objFFRInfo.numshp = "";//drAWBRateMaster["Pieces"].ToString();
                            ////objFFRInfo.manifestdesc = drAWBRateMaster["CommodityDesc"].ToString().Length > 1 ? drAWBRateMaster["CommodityDesc"].ToString() : "GEN";
                            //objFFRInfo.manifestdesc = drAWBRateMaster["CommodityCode"].ToString().Length > 1 ? drAWBRateMaster["CommodityCode"].ToString() : "GEN";
                            //objFFRInfo.manifestdesc += "-";
                            //objFFRInfo.manifestdesc += drAWBRateMaster["CommodityDesc"].ToString().Length > 0 ? drAWBRateMaster["CommodityDesc"].ToString() : "GEN";
                            //objFFRInfo.splhandling="";
                            #endregion


                            //line 3
                            #region FLTROUTE
                            //objFFRInfo.carriercode=drAWBSummaryMaster["AWBPrefix"].ToString();
                            // objFFRInfo.fltnum=drAWBRouteMaster["FltNumber"].ToString().Substring(2);
                            // objFFRInfo.date=day.ToString();
                            // objFFRInfo.month=mon.ToString();
                            // objFFRInfo.fltdept=drAWBRouteMaster["FltOrigin"].ToString();
                            // objFFRInfo.fltarrival = drAWBRouteMaster["FltDestination"].ToString();
                            // objFFRInfo.spaceallotmentcode="";
                            // objFFRInfo.allotidentification="LL";
                            // try
                            // {
                            //     string AWBStatus = "";
                            //     AWBStatus = drAWBRouteMaster["Status"].ToString();
                            //     if (AWBStatus.Trim() != "")
                            //     {
                            //         if (AWBStatus.Trim() == "Q")
                            //         {
                            //             objFFRInfo.spaceallotmentcode = "LL";
                            //         }
                            //         else if (AWBStatus.Trim() == "C")
                            //         {
                            //             objFFRInfo.spaceallotmentcode = "KK";
                            //         }
                            //     }
                            //     else
                            //     {
                            //         objFFRInfo.spaceallotmentcode = drAWBRouteMaster["Status"].ToString(); ;
                            //     }
                            // }
                            // catch (Exception ex)
                            // {clsLog.WriteLogAzure(ex.Message);}
                            #endregion

                            //line 4
                            objFFRInfo.noofuld = "";
                            //line 5 
                            objFFRInfo.specialservicereq1 = "";
                            objFFRInfo.specialservicereq2 = "";
                            //line 6
                            objFFRInfo.otherserviceinfo1 = "";
                            objFFRInfo.otherserviceinfo2 = "";
                            //line 7
                            objFFRInfo.bookingrefairport = drAWBSummaryMaster["OriginCode"].ToString();
                            objFFRInfo.officefundesignation = "FF";
                            objFFRInfo.companydesignator = "XX";
                            objFFRInfo.bookingfileref = "";
                            objFFRInfo.participentidetifier = "";
                            objFFRInfo.participentcode = "";
                            objFFRInfo.participentairportcity = "";
                            // objFFRInfo.participentairportcity = drAWBSummaryMaster["OriginCode"].ToString();
                            // objFFRInfo.participentcode = "";
                            // objFFRInfo.participentidetifier = "";

                            //line 8
                            #region Dimension
                            //if (dsFFR.Tables.Count > 3)
                            //{
                            //    objFFRInfo.line8weightcode = "K";
                            //    objFFRInfo.line8weight = drAWBRateMaster["GrossWeight"].ToString(); ;
                            //    //objFFRInfo.line8mesurunitcode = "";
                            //    //objFFRInfo.line8length = "";
                            //    //objFFRInfo.line8width = "";
                            //    //objFFRInfo.line8height = "";
                            //    //objFFRInfo.line8piecenum = "";
                            //    // objFFRInfo.line8height = "";
                            //    //objFFRInfo.line8length = "";
                            //    //objFFRInfo.line8mesurunitcode = "";
                            //    //objFFRInfo.line8piecenum = drAWBRateMaster["Pieces"].ToString();
                            //    //objFFRInfo.line8weight = 
                            //    //objFFRInfo.line8weightcode = "K";
                            //    //objFFRInfo.line8width = "";
                            //}

                            // Dimensions

                            //if (dsFFR.Tables[4].Rows.Count > 0)
                            //{
                            //    DataRow drAWBDimensions = dsFFR.Tables[4].Rows[0];
                            //    if (drAWBDimensions[0].ToString().Trim() != "")
                            //    {
                            //        if (drAWBDimensions[0].ToString().Trim().ToUpper() == "CMS")
                            //        {
                            //            objFFRInfo.line8mesurunitcode = "CMT";
                            //        }
                            //        else if (drAWBDimensions[0].ToString().Trim().ToUpper() == "INCHES")
                            //        {
                            //            objFFRInfo.line8mesurunitcode = "INH";
                            //        }
                            //    }

                            //    objFFRInfo.line8length = drAWBDimensions[1].ToString();
                            //    objFFRInfo.line8width = drAWBDimensions[2].ToString();
                            //    objFFRInfo.line8height = drAWBDimensions[3].ToString();
                            //    objFFRInfo.line8piecenum = "";
                            //}
                            #endregion
                            //line 9 
                            objFFRInfo.servicecode = "";
                            objFFRInfo.rateclasscode = "";
                            objFFRInfo.commoditycode = "";
                            //objFFRInfo.rateclasscode = "";
                            //objFFRInfo.servicecode = "A";
                            //objFFRInfo.commoditycode = drAWBRateMaster["CommodityCode"].ToString();

                            //line 10                      
                            objFFRInfo.shipperaccnum = "";
                            objFFRInfo.shippername = drAWBShipperConsigneeDetails["ShipperName"].ToString();
                            objFFRInfo.shipperadd = drAWBShipperConsigneeDetails["ShipperAddress"].ToString();
                            objFFRInfo.shipperplace = "";//drAWBShipperConsigneeDetails["ShipperAddress"].ToString();
                            objFFRInfo.shipperstate = "";
                            objFFRInfo.shippercountrycode = drAWBShipperConsigneeDetails["ShipperCountry"].ToString().Substring(0, 2);
                            objFFRInfo.shipperpostcode = "";
                            objFFRInfo.shippercontactidentifier = "TE";
                            objFFRInfo.shippercontactnum = drAWBShipperConsigneeDetails["ShipperTelephone"].ToString();

                            //line 11
                            objFFRInfo.consaccnum = "";
                            objFFRInfo.consname = drAWBShipperConsigneeDetails["ConsigneeName"].ToString();
                            objFFRInfo.consadd = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString();
                            objFFRInfo.consplace = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString();
                            objFFRInfo.consstate = "";
                            objFFRInfo.conscountrycode = drAWBShipperConsigneeDetails["ConsigneeCountry"].ToString().Substring(0, 2);
                            objFFRInfo.conspostcode = "";
                            objFFRInfo.conscontactidentifier = "TE";
                            objFFRInfo.conscontactnum = drAWBShipperConsigneeDetails["ConsigneeTelephone"].ToString();

                            //line 12
                            objFFRInfo.custaccnum = "";
                            objFFRInfo.iatacargoagentcode = drAWBSummaryMaster["AgentCode"].ToString();
                            objFFRInfo.cargoagentcasscode = "";
                            objFFRInfo.custparticipentidentifier = "";
                            objFFRInfo.custname = "";
                            objFFRInfo.custplace = "";
                            //line 13
                            objFFRInfo.shiprefnum = "";
                            objFFRInfo.supplemetryshipperinfo1 = "";
                            objFFRInfo.supplemetryshipperinfo2 = "";


                            #endregion

                            string strMsg = "";  //= EncodeFFRForSend(ref objFFRInfo, ref objULDInfo);
                            if (strMsg != null)
                            {
                                if (strMsg.Trim() != "")
                                {
                                    GenericFunction GF = new GenericFunction();
                                    flag = GF.SaveFFRMessageInOutBox(strMsg, fromEmailID, toEmailID);
                                    //flag = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { clsLog.WriteLogAzure(ex.Message); }
            return flag;
        }

        public static bool EncodeFFRForSend(DataSet ds, int refNO)
        {
            bool flag = false;
            try
            {
                MessageData.ffrinfo objFFRInfo = new MessageData.ffrinfo("");
                MessageData.consignmnetinfo consigment = new MessageData.consignmnetinfo("");
                MessageData.FltRoute[] fltRoute = new MessageData.FltRoute[0];
                MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                MessageData.dimensionnfo[] dimension = new MessageData.dimensionnfo[0];
                string agentcode = "", awbnum = "";
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DataRow drAWBRateMaster = ds.Tables[1].Rows[0];

                            DataRow drAWBSummaryMaster = ds.Tables[0].Rows[0];

                            #region PrepareFFRStructureObject

                            //line 1 
                            objFFRInfo.ffrversionnum = "6";

                            #region Consigment Section
                            //line 2
                            awbnum = drAWBSummaryMaster["AWBNumber"].ToString();
                            consigment.airlineprefix = drAWBSummaryMaster["AWBPrefix"].ToString();
                            consigment.awbnum = drAWBSummaryMaster["AWBNumber"].ToString();
                            consigment.origin = drAWBSummaryMaster["OriginCode"].ToString();
                            consigment.dest = drAWBSummaryMaster["DestinationCode"].ToString(); ;
                            consigment.consigntype = "T";
                            consigment.pcscnt = drAWBRateMaster["Pieces"].ToString();
                            consigment.weightcode = drAWBSummaryMaster["UOM"].ToString();
                            consigment.weight = drAWBSummaryMaster["GrossWeight"].ToString();
                            consigment.volumecode = "";
                            consigment.volumeamt = "";
                            consigment.densityindicator = "";
                            consigment.densitygrp = "";
                            consigment.shpdesccode = "";
                            consigment.numshp = "";//drAWBRateMaster["Pieces"].ToString();
                            try
                            {
                                consigment.manifestdesc = drAWBRateMaster["CodeDescription"].ToString().Length > 1 ? drAWBRateMaster["CommodityDesc"].ToString().Substring(0, 14) : "";
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                            //consigment.manifestdesc = drAWBRateMaster["CommodityCode"].ToString().Length > 0 ? drAWBRateMaster["CommodityCode"].ToString() : "GEN";
                            consigment.splhandling = "";
                            #endregion


                            //line 3
                            #region FLTROUTE
                            if (ds.Tables.Count > 3)
                            {
                                if (ds.Tables[3].Rows.Count > 0)
                                {
                                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                                    {
                                        DataRow drAWBRouteMaster = ds.Tables[3].Rows[i];
                                        MessageData.FltRoute route = new MessageData.FltRoute("");
                                        try
                                        {
                                            DateTime dtTo = new DateTime();
                                            //  DateTime dtfrom = new DateTime();
                                            string dt = (drAWBRouteMaster["FltDate"].ToString());
                                            //dt = dt + " " + DateTime.Now.ToShortTimeString();
                                            //dtfrom = DateTime.ParseExact(dt,"dd-MM-yyyy",null);
                                            dtTo = DateTime.ParseExact(drAWBRouteMaster["FltDate"].ToString(), "dd/MM/yyyy", null);
                                            //ToDt = dt.ToString();
                                            string day = dt.Substring(0, 2);
                                            string mon = dtTo.ToString("MMM");
                                            //string mon = dt.Substring(3, 2);
                                            string yr = dt.Substring(6, 4);
                                            route.date = day.ToString();
                                            route.month = mon.ToString();

                                        }
                                        catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                                        route.carriercode = "";
                                        route.fltnum = drAWBRouteMaster["FltNumber"].ToString();

                                        route.fltdept = drAWBRouteMaster["FltOrigin"].ToString();
                                        route.fltarrival = drAWBRouteMaster["FltDestination"].ToString();
                                        route.spaceallotmentcode = "";
                                        route.allotidentification = "";
                                        try
                                        {
                                            string AWBStatus = "";
                                            AWBStatus = drAWBRouteMaster["Status"].ToString();
                                            if (AWBStatus.Trim() != "")
                                            {
                                                if (AWBStatus.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    route.spaceallotmentcode = "LL";
                                                }
                                                else if (AWBStatus.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    route.spaceallotmentcode = "KK";
                                                }
                                            }
                                            else
                                            {
                                                route.spaceallotmentcode = drAWBRouteMaster["Status"].ToString(); ;
                                            }
                                        }
                                        catch (Exception ex)
                                        { clsLog.WriteLogAzure(ex.Message); }

                                        Array.Resize(ref fltRoute, fltRoute.Length + 1);
                                        fltRoute[fltRoute.Length - 1] = route;
                                    }
                                }
                            }
                            #endregion

                            //line 4
                            objFFRInfo.noofuld = "";
                            //line 5 
                            objFFRInfo.specialservicereq1 = "";
                            objFFRInfo.specialservicereq2 = "";
                            //line 6
                            objFFRInfo.otherserviceinfo1 = "";
                            objFFRInfo.otherserviceinfo2 = "";
                            //line 7

                            objFFRInfo.bookingrefairport = drAWBSummaryMaster["OriginCode"].ToString();
                            objFFRInfo.officefundesignation = "FF";
                            objFFRInfo.companydesignator = "XX";
                            objFFRInfo.bookingfileref = "";
                            objFFRInfo.participentidetifier = "";
                            objFFRInfo.participentcode = "";
                            objFFRInfo.participentairportcity = "";
                            // objFFRInfo.participentairportcity = drAWBSummaryMaster["OriginCode"].ToString();
                            // objFFRInfo.participentcode = "";
                            // objFFRInfo.participentidetifier = "";

                            //line 8
                            #region Dimension
                            //please don't send the dimensions for Auto FFR Setting
                            try
                            {
                                /*
                                if (ds.Tables.Count > 2)
                                {
                                   
                                    if (ds.Tables[2].Rows.Count > 0)
                                    {
                                        for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                                        {
                                            MessageData.dimensionnfo dim = new MessageData.dimensionnfo("");
                                            DataRow drAWBDimensions = ds.Tables[2].Rows[i];
                                            dim.weightcode = "";
                                            dim.weight = "";
                                            dim.mesurunitcode = "";
                                            if (drAWBDimensions["MeasureUnit"].ToString().Trim().ToUpper() == "CMS")
                                            {
                                                dim.mesurunitcode = "CMT";
                                            }
                                            else if (drAWBDimensions["MeasureUnit"].ToString().Trim().ToUpper() == "INCHES")
                                            {
                                                dim.mesurunitcode = "INH";
                                            }
                                            dim.length = drAWBDimensions["Length"].ToString();
                                            dim.width = drAWBDimensions["Breadth"].ToString();
                                            dim.height = drAWBDimensions["Height"].ToString();
                                            dim.piecenum = drAWBDimensions["PcsCount"].ToString();
                                            Array.Resize(ref dimension, dimension.Length + 1);
                                            dimension[dimension.Length - 1] = dim;

                                        }
                                    }
                                }*/
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                            #endregion
                            //line 9 
                            objFFRInfo.servicecode = "";
                            objFFRInfo.rateclasscode = "";
                            objFFRInfo.commoditycode = "";
                            objFFRInfo.producttype = ds.Tables[5].Rows[0][0].ToString();

                            //line 10    
                            try
                            {
                                if (ds.Tables[6].Rows.Count > 0)
                                {
                                    DataRow drAWBShipperConsigneeDetails = ds.Tables[6].Rows[0];
                                    objFFRInfo.shipperaccnum = "";//[ShipperAccCode]
                                    objFFRInfo.shippername = drAWBShipperConsigneeDetails["ShipperName"].ToString();
                                    objFFRInfo.shipperadd = drAWBShipperConsigneeDetails["ShipperAddress"].ToString() + " " + drAWBShipperConsigneeDetails["ShipperAdd2"].ToString();
                                    objFFRInfo.shipperplace = drAWBShipperConsigneeDetails["ShipperCity"].ToString();
                                    objFFRInfo.shipperstate = drAWBShipperConsigneeDetails["ShipperState"].ToString();
                                    objFFRInfo.shippercountrycode = drAWBShipperConsigneeDetails["ShipperCountry"].ToString().Substring(0, 2);
                                    objFFRInfo.shipperpostcode = drAWBShipperConsigneeDetails["ShipperPincode"].ToString();
                                    objFFRInfo.shippercontactidentifier = "TE";
                                    objFFRInfo.shippercontactnum = drAWBShipperConsigneeDetails["ShipperTelephone"].ToString();

                                    //line 11
                                    objFFRInfo.consaccnum = "";//[ConsigAccCode]
                                    objFFRInfo.consname = drAWBShipperConsigneeDetails["ConsigneeName"].ToString();
                                    objFFRInfo.consadd = drAWBShipperConsigneeDetails["ConsigneeAddress"].ToString() + " " + drAWBShipperConsigneeDetails["ConsigneeAddress2"].ToString(); ;
                                    objFFRInfo.consplace = drAWBShipperConsigneeDetails["ConsigneeCity"].ToString();
                                    objFFRInfo.consstate = drAWBShipperConsigneeDetails["ConsigneeState"].ToString();
                                    objFFRInfo.conscountrycode = drAWBShipperConsigneeDetails["ConsigneeCountry"].ToString().Substring(0, 2);
                                    objFFRInfo.conspostcode = drAWBShipperConsigneeDetails["ConsigneePincode"].ToString(); ;
                                    objFFRInfo.conscontactidentifier = "TE";
                                    objFFRInfo.conscontactnum = drAWBShipperConsigneeDetails["ConsigneeTelephone"].ToString();
                                }
                            }
                            catch (Exception ex) { clsLog.WriteLogAzure(ex.Message); }
                            //line 12
                            objFFRInfo.custaccnum = "";

                            objFFRInfo.iatacargoagentcode = RemoveSpecialCharacters(drAWBSummaryMaster["AgentCode"].ToString()).Substring(0, 7);

                            objFFRInfo.cargoagentcasscode = "";
                            objFFRInfo.custparticipentidentifier = "";
                            objFFRInfo.custname = "";
                            objFFRInfo.custplace = "";
                            agentcode = drAWBSummaryMaster["AgentCode"].ToString();
                            //line 13
                            objFFRInfo.shiprefnum = "";
                            objFFRInfo.supplemetryshipperinfo1 = "";
                            objFFRInfo.supplemetryshipperinfo2 = "";

                            //Start:-modified by priyanka for CUS/AGT info on 08-11-2016

                            if (ds.Tables[4].Rows.Count > 0)
                            {
                                DataRow custinfo = ds.Tables[4].Rows[0];

                                objFFRInfo.custname = custinfo["AgentName"].ToString();
                                objFFRInfo.iatacargoagentcode = custinfo["IATAAgentCode"].ToString();
                                objFFRInfo.custaccnum = custinfo["CustomerCode"].ToString();
                                objFFRInfo.cargoagentcasscode = custinfo["CASSID"].ToString();
                                objFFRInfo.custparticipentidentifier = custinfo["Participentidentifier"].ToString();
                                objFFRInfo.custplace = custinfo["Address"].ToString();

                            }


                            //End changes



                            #endregion

                            string strMsg = ""; // EncodeFFRForSend(objFFRInfo, objULDInfo,consigment, fltRoute, dimension);
                            if (strMsg != null)
                            {
                                if (strMsg.Trim() != "")
                                {
                                    FTP objFTP = new FTP();
                                    if (!objFTP.Saveon72FTP(strMsg, awbnum))
                                    {
                                        clsLog.WriteLogAzure("Error of AWB upload on FTP:" + awbnum);
                                    }
                                    GenericFunction GF = new GenericFunction();
                                    flag = GF.SaveMessageOutBox("FFR", strMsg, "swapnil@qidtech.com", "", agentcode, refNO);


                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure("Error in EncodeFFR" + ex.Message);
            }
            return flag;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                // if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')

                if ((c >= '0' && c <= '9') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public bool ValidaeSaveFFRMessage(MessageData.ffrinfo objFFRData, MessageData.consignmnetinfo[] objConsInfo, MessageData.FltRoute[] objRouteInfo, MessageData.dimensionnfo[] objDimension, int RefNo, string strFFRMessage, string strMessageFrom, string strFromID, string strStatus, out string ErrorMsg, MessageData.ULDinfo[] Uldinfo = null)
        {
            string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            bool flag = false, updateDIMSwt = false;
            string awbnum = string.Empty, AWBPrefix = string.Empty, strErrorMessage = string.Empty, strAWBOrigin = string.Empty, strAWBDestination = string.Empty, strFlightNo = string.Empty, strFlightOrigin = string.Empty, strFlightDestination = string.Empty;
            int awbPcs = 0, legCount = 0;
            decimal awbWeight = 0;
            decimal totalDimsWt = 0;
            int totalDimsPieces = 0;
            bool isOriginDestinationMismatch = false, isAllotmentCodeExist = false, isAllotmentCodeAvailableInFFR = false, isAllAllotmentCodeSame = true
                , isAllotmentCodeValid = false, isRFSFlight = false, useFFRAllotment = false;
            string DensityGrp = string.Empty;
            string AWBOriginAirportCode = string.Empty, AWBDestAirportCode = string.Empty;
            
            objFFRData.otherserviceinfo1 = objFFRData.otherserviceinfo1 + objFFRData.otherserviceinfo2;
            try
            {
                DataSet dsAWBMaterLogOldValues = new DataSet();
                ErrorMsg = string.Empty;
                SQLServer dtb = new SQLServer();
                if (objConsInfo.Length > 0)
                {

                    #region : Check flight extsts or not in schedule :
                    bool isCheckValidFlight = false, isDesignatorCodeExists = false;
                    GenericFunction genericFunction = new GenericFunction();
                    useFFRAllotment = Convert.ToBoolean(genericFunction.GetConfigurationValues("UseFFRAllotment") == string.Empty ? "false" : genericFunction.GetConfigurationValues("UseFFRAllotment"));
                    isCheckValidFlight = Convert.ToBoolean(genericFunction.ReadValueFromDb("ChkFltPresentAndAWBStatus") == string.Empty ? "false" : genericFunction.ReadValueFromDb("ChkFltPresentAndAWBStatus"));
                    if (isCheckValidFlight)
                    {
                        if (objRouteInfo.Length > 0)
                        {
                            if (useFFRAllotment)
                            {
                                for (int lstIndex = 0; lstIndex < objRouteInfo.Length; lstIndex++)
                                {
                                    legCount++;

                                    if (!isAllotmentCodeAvailableInFFR && objRouteInfo[lstIndex].allotidentification != "")
                                    {
                                        isAllotmentCodeAvailableInFFR = true;
                                    }
                                    if (lstIndex > 0 && isAllAllotmentCodeSame && objRouteInfo[lstIndex - 1].allotidentification != objRouteInfo[lstIndex].allotidentification)
                                    {
                                        isAllAllotmentCodeSame = false;
                                    }
                                }
                            }
                            for (int lstIndex = 0; lstIndex < objRouteInfo.Length; lstIndex++)
                            {
                                #region : Switch Flight Month :
                                string fltMonth = "", fltDate = string.Empty;
                                switch (objRouteInfo[lstIndex].month.Trim().ToUpper())
                                {
                                    case "JAN":
                                        {
                                            fltMonth = "01";
                                            break;
                                        }
                                    case "FEB":
                                        {
                                            fltMonth = "02";
                                            break;
                                        }
                                    case "MAR":
                                        {
                                            fltMonth = "03";
                                            break;
                                        }
                                    case "APR":
                                        {
                                            fltMonth = "04";
                                            break;
                                        }
                                    case "MAY":
                                        {
                                            fltMonth = "05";
                                            break;
                                        }
                                    case "JUN":
                                        {
                                            fltMonth = "06";
                                            break;
                                        }
                                    case "JUL":
                                        {
                                            fltMonth = "07";
                                            break;
                                        }
                                    case "AUG":
                                        {
                                            fltMonth = "08";
                                            break;
                                        }
                                    case "SEP":
                                        {
                                            fltMonth = "09";
                                            break;
                                        }
                                    case "OCT":
                                        {
                                            fltMonth = "10";
                                            break;
                                        }
                                    case "NOV":
                                        {
                                            fltMonth = "11";
                                            break;
                                        }
                                    case "DEC":
                                        {
                                            fltMonth = "12";
                                            break;
                                        }
                                    default:
                                        {
                                            fltMonth = "00";
                                            break;
                                        }
                                }
                                fltDate = fltMonth.PadLeft(2, '0') + "/" + objRouteInfo[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

                                ///Find out if flight date with current year is less than server date time by at least 100 days.
                                DateTime dtFlightDate = DateTime.Now;
                                if (DateTime.TryParseExact(fltDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFlightDate))
                                {
                                    if (DateTime.Now.AddDays(-100) > dtFlightDate)
                                    {   ///Advance year in flight date to next year.
                                        dtFlightDate = dtFlightDate.AddYears(1);
                                        fltDate = dtFlightDate.ToString("MM/dd/yyyy");
                                    }
                                }
                                else
                                {
                                    ErrorMsg = "Flight Date is invalid";
                                    return false;
                                }

                                string date = dtFlightDate.ToString("MM/dd/yyyy");

                                #endregion Switch Flight Month

                                for (int i = 0; i < objConsInfo.Length; i++)
                                {
                                    awbnum = objConsInfo[i].awbnum;
                                    AWBPrefix = objConsInfo[i].airlineprefix;
                                    strAWBOrigin = objConsInfo[i].origin;
                                    strAWBDestination = objConsInfo[i].dest;
                                }

                                if (strAWBDestination == objRouteInfo[lstIndex].fltdept)
                                {
                                    ErrorMsg = AWBPrefix + '-' + awbnum + " has mismatch Origin/Destination.";
                                    return false;
                                }

                                #region : Check Valid Flights :
                                string[] parms = new string[]
                                {
                                        "FltOrigin",
                                        "FltDestination",
                                        "FlightNo",
                                        "flightDate",
                                        "AWBNumber",
                                        "AWBPrefix",
                                        "RefNo",
                                        "UpdatedBy",
                                        "AllotmentCode",
                                        "LegCount"
                                };
                                SqlDbType[] dataType = new SqlDbType[]
                                {
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.DateTime,
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.Int,
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.Int
                                };
                                object[] value = new object[]
                                {

                                        objRouteInfo[lstIndex].fltdept,
                                        objRouteInfo[lstIndex].fltarrival,
                                        objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                        DateTime.Parse(fltDate),
                                       string.Empty,
                                        string.Empty,
                                        RefNo,
                                        "FFR",
                                        objRouteInfo[lstIndex].allotidentification,
                                        legCount
                                };
                                DataSet dsdata = dtb.SelectRecords("Messaging.uspValidateFFRFWBFlight", parms, value, dataType);
                                if (dsdata != null && dsdata.Tables.Count > 0)
                                {
                                    isAllotmentCodeExist = false;
                                    isRFSFlight = false;
                                    for (int i = 0; i < dsdata.Tables.Count; i++)
                                    {
                                        if (!isDesignatorCodeExists && dsdata.Tables[i].Columns.Contains("IsDesignatorCodeExists")
                                            && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                        {
                                            isDesignatorCodeExists = true;
                                        }

                                        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() == "0"
                                            && dsdata.Tables[i].Columns.Contains("ErrorMessage") && dsdata.Tables[i].Rows[0]["ErrorMessage"].ToString().Trim() != ""
                                            && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                        {
                                            ErrorMsg = dsdata.Tables[i].Rows[0]["ErrorMessage"].ToString().Trim();
                                            return false;
                                        }
                                        else if (dsdata.Tables[i].Columns.Contains("ScheduleID") && dsdata.Tables[i].Rows[0]["ScheduleID"].ToString() == "0"
                                            && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsDesignatorCodeExists"].ToString()))
                                        {
                                            ErrorMsg = "Flight number is invalid";
                                            return false;
                                        }
                                        if (useFFRAllotment)
                                        {
                                            if (dsdata.Tables[i].Columns.Contains("IsAllotmentCodeExist")
                                                && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsAllotmentCodeExist"].ToString()))
                                            {
                                                isAllotmentCodeExist = true;
                                            }
                                            if (dsdata.Tables[i].Columns.Contains("IsRFSFlight")
                                                && Convert.ToBoolean(dsdata.Tables[i].Rows[0]["IsRFSFlight"].ToString()))
                                            {
                                                isRFSFlight = true;
                                            }
                                        }
                                    }
                                }
                                if (useFFRAllotment)
                                {
                                    if (legCount > 1 && isAllotmentCodeAvailableInFFR && isAllAllotmentCodeSame && isRFSFlight)
                                    {
                                        objRouteInfo[lstIndex].allotidentification = "";
                                    }
                                    if (legCount > 1 && isAllotmentCodeAvailableInFFR && isAllAllotmentCodeSame && !isAllotmentCodeExist && !isRFSFlight)
                                    {
                                        objRouteInfo[lstIndex].allotidentification = "";
                                    }
                                    if (legCount > 1 && !isAllotmentCodeValid && isAllotmentCodeAvailableInFFR && objRouteInfo[lstIndex].allotidentification != "" && !isRFSFlight)
                                    {
                                        isAllotmentCodeValid = true;
                                    }
                                }
                                #endregion Check Valid Flights
                            }
                            if (!isDesignatorCodeExists)
                            {
                                ErrorMsg = "Flight number is invalid";
                                return false;
                            }
                            if (useFFRAllotment && legCount > 1 && isAllotmentCodeAvailableInFFR && !isAllotmentCodeValid && !isRFSFlight)
                            {
                                ErrorMsg = "Allotment code not exists";
                                return false;
                            }
                        }
                    }
                    #endregion
                    for (int i = 0; i < objConsInfo.Length; i++)
                    {
                        awbnum = objConsInfo[i].awbnum;
                        AWBPrefix = objConsInfo[i].airlineprefix;
                        strAWBOrigin = objConsInfo[i].origin;
                        strAWBDestination = objConsInfo[i].dest;
                        awbPcs = int.Parse(objConsInfo[i].pcscnt);
                        awbWeight = Convert.ToDecimal(objConsInfo[i].weight);
                        DensityGrp = objConsInfo[i].densitygrp;

                        GenericFunction gf = new GenericFunction();
                        ///MasterLog                      
                        dsAWBMaterLogOldValues = gf.GetAWBMasterLogNewRecord(AWBPrefix, awbnum);

                        gf.UpdateInboxFromMessageParameter(RefNo, AWBPrefix + "-" + awbnum, string.Empty, string.Empty, string.Empty, "FFR", "FFR", DateTime.Parse("1900-01-01"));

                        //calculate volume from gross wt. Default volumecode in MC
                        if (objConsInfo[0].volumecode == "" && objConsInfo[0].volumeamt == "")
                        {
                            string defaultDensityIndex = genericFunction.GetConfigurationValues("DefaultDensityIndex");
                            decimal densityIndex = defaultDensityIndex.Trim() == string.Empty ? 0 : Convert.ToDecimal(defaultDensityIndex);
                            if (densityIndex > 0)
                            {
                                objConsInfo[0].volumeamt = Convert.ToString(Convert.ToDecimal(awbWeight) * densityIndex);
                            }
                            else
                            {
                                objConsInfo[0].volumeamt = Convert.ToString(Convert.ToDecimal(awbWeight) / Convert.ToDecimal(166.67));
                            }
                            objConsInfo[0].volumecode = "MC";
                        }

                        decimal VolumeWt = 0;
                        if (objConsInfo[0].volumecode != "" && Convert.ToDecimal(objConsInfo[0].volumeamt == "" ? "0" : objConsInfo[0].volumeamt) > 0)
                        {
                            switch (objConsInfo[0].volumecode.ToUpper())
                            {
                                case "MC":
                                    VolumeWt = decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal(Convert.ToDecimal(objConsInfo[0].volumeamt == "" ? "0" : objConsInfo[0].volumeamt) * decimal.Parse("166.67"))));
                                    break;

                                case "CI":
                                    VolumeWt =
                                        decimal.Parse(String.Format("{0:0.00}", Convert.ToDecimal((Convert.ToDecimal(objConsInfo[0].volumeamt == "" ? "0" : objConsInfo[0].volumeamt) /
                                                                                      decimal.Parse("366")))));
                                    break;
                                case "CF":
                                    VolumeWt =
                                        decimal.Parse(String.Format("{0:0.00}",
                                                                    Convert.ToDecimal(Convert.ToDecimal(objConsInfo[0].volumeamt == "" ? "0" : objConsInfo[0].volumeamt) *
                                                                                      decimal.Parse("4.7194"))));
                                    break;
                                case "CC":
                                    VolumeWt =
                                       decimal.Parse(String.Format("{0:0.00}",
                                                                   Convert.ToDecimal(((Convert.ToDecimal(objConsInfo[0].volumeamt == "" ? "0" : objConsInfo[0].volumeamt) /
                                                                                      decimal.Parse("6000"))))));
                                    break;
                                default:
                                    VolumeWt = Convert.ToDecimal(Convert.ToDecimal(objConsInfo[0].volumeamt == "" ? "0" : objConsInfo[0].volumeamt));
                                    break;

                            }
                        }
                        decimal ChargeableWeight = 0;
                        if (VolumeWt > 0)
                        {
                            if (Convert.ToDecimal(objConsInfo[i].weight == "" ? "0" : objConsInfo[i].weight) > VolumeWt)
                                ChargeableWeight = Convert.ToDecimal(objConsInfo[i].weight == "" ? "0" : objConsInfo[i].weight);
                            else
                                ChargeableWeight = VolumeWt;
                        }
                        else
                        {
                            ChargeableWeight = Convert.ToDecimal(objConsInfo[i].weight == "" ? "0" : objConsInfo[i].weight);
                        }

                        if (objConsInfo[i].pcscnt == "" || objConsInfo[i].pcscnt == "0")
                        {
                            strErrorMessage = "AWB Pieces should be greater than Zero";
                            ErrorMsg = strErrorMessage;
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };
                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);
                            //else
                            GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);

                            strErrorMessage = string.Empty;
                            return flag = false;
                        }

                        if (objConsInfo[i].weight == "" || objConsInfo[i].weight == "0")
                        {
                            strErrorMessage = "AWB GrossWeight  should be greater than Zero";
                            ErrorMsg = strErrorMessage;
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };
                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);
                            //else
                            GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);
                            strErrorMessage = string.Empty;
                            return flag = false;
                        }
                        if (objDimension.Length > 0)
                        {
                            for (int j = 0; j < objDimension.Length; j++)
                            {
                                totalDimsWt = totalDimsWt + Convert.ToDecimal(objDimension[j].weight.Trim() == string.Empty ? "0" : objDimension[j].weight.Trim());
                                totalDimsPieces = totalDimsPieces + Convert.ToInt32(objDimension[j].piecenum.Trim() == string.Empty ? "0" : objDimension[j].piecenum.Trim());
                            }

                            if (totalDimsPieces != Convert.ToInt32(objConsInfo[i].pcscnt))
                            {
                                Array.Resize(ref objDimension, 0);
                            }
                        }



                        if (totalDimsWt == Convert.ToDecimal(objConsInfo[i].weight))
                        {
                            updateDIMSwt = true;
                        }

                        DataSet dsawb = new DataSet();
                        dsawb = CheckValidateFFRMessage(AWBPrefix, awbnum, strAWBOrigin, strAWBDestination, "FFR", objRouteInfo[0].fltdept, objRouteInfo[objRouteInfo.Length - 1].fltarrival);
                        if (dsawb != null && dsawb.Tables.Count > 0)
                        {
                            for (int l = 0; l < dsawb.Tables.Count; l++)
                            {
                                if (dsawb.Tables[l].Columns.Contains("ErrorMessage"))
                                {
                                    if (dsawb.Tables[l].Rows.Count > 0)
                                    {
                                        strErrorMessage = dsawb.Tables[l].Rows[0]["ErrorMessage"].ToString();
                                        if (strErrorMessage.Trim() != string.Empty)
                                        {
                                            ErrorMsg = strErrorMessage;
                                            GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);
                                            strErrorMessage = string.Empty;
                                            return flag = false;
                                        }
                                    }
                                }
                            }
                        }

                        if (dsawb != null && dsawb.Tables.Count > 0 && dsawb.Tables[0].Rows.Count > 0 && dsawb.Tables[0].Columns.Contains("AWBOriginAirportCode"))
                        {
                            AWBOriginAirportCode = dsawb.Tables[0].Rows[0]["AWBOriginAirportCode"].ToString();
                            AWBDestAirportCode = dsawb.Tables[0].Rows[0]["AWBDestAirportCode"].ToString();
                        }

                        bool originMatch = false, destinationMatch = false;

                        if (objRouteInfo.Length > 0)
                        {
                            for (int s = 0; s < objRouteInfo.Length; s++)
                            {
                                string FlightOrigin = objRouteInfo[s].fltdept;
                                string FlightDestination = objRouteInfo[s].fltarrival;
                                if (FlightOrigin == AWBOriginAirportCode)
                                    originMatch = true;
                                if (FlightDestination == AWBDestAirportCode)
                                    destinationMatch = true;
                                if (originMatch && destinationMatch)
                                    break;
                            }
                            if (!(originMatch && destinationMatch))
                            {
                                isOriginDestinationMismatch = true;
                            }
                        }
                        else
                        {
                            strErrorMessage = "Wrong Message format of FFR Message.FFR Message   Rejected For " + awbnum;
                            ErrorMsg = strErrorMessage;
                            //string[] QueryNames = { "ErrorMessage", "MessageSNo" };
                            //SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.Int };
                            //object[] QueryValues = { strErrorMessage, RefNo };

                            //if (dtb.UpdateData("spUpdateStatusformessage", QueryNames, QueryTypes, QueryValues))
                            //    GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);
                            //else
                            GenerateFNAMessage(strFFRMessage, strErrorMessage, AWBPrefix, awbnum, strFromID);
                            return flag = false;
                        }

                        DateTime Flightdt = DateTime.Parse(objRouteInfo[0].month + "/" + objRouteInfo[0].date + "/" + System.DateTime.Now.Year.ToString());

                        string[] paramname = new string[] { "AirlinePrefix", "AWBNum", "Origin", "Dest", "PcsCount", "Weight", "Volume", "ComodityCode", "ComodityDesc", "CarrierCode", "FlightNum", "FlightDate", "FlightOrigin", "FlightDest", "ShipperName", "ShipperAddr", "ShipperPlace", "ShipperState", "ShipperCountryCode", "ShipperContactNo", "ConsName", "ConsAddr", "ConsPlace", "ConsState", "ConsCountryCode", "ConsContactNo", "CustAccNo", "IATACargoAgentCode", "CustName", "SystemDate", "MeasureUnit", "Length", "Breadth", "Height", "PartnerStatus", "REFNo", "UpdatedBy", "SpecialHandelingCode", "WeightCode", "ChargeableWeight", "ShipperPincode", "ConsingneePinCode", "ShipperAccountCode", "VolumeWt", "VolumeCode", "DensityCode", "OtherServiceInformation", "ServiceCode", "Remarks" };

                        //object[] paramvalue = new object[] { objConsInfo[i].airlineprefix, objConsInfo[i].awbnum, objConsInfo[i].origin, objConsInfo[i].dest, objConsInfo[i].pcscnt, objConsInfo[i].weight, objConsInfo[i].volumeamt, objConsInfo[i].commodity, objConsInfo[i].manifestdesc, objRouteInfo[0].carriercode, objRouteInfo[0].carriercode + objRouteInfo[0].fltnum, Flightdt, objRouteInfo[0].fltdept, objRouteInfo[0].fltarrival, objFFRData.shippername, objFFRData.shipperadd.Trim(','), objFFRData.shipperplace.Trim(','), objFFRData.shipperstate, objFFRData.shippercountrycode, objFFRData.shippercontactnum, objFFRData.consname, objFFRData.consadd.Trim(','), objFFRData.consplace.Trim(','), objFFRData.consstate, objFFRData.conscountrycode, objFFRData.conscontactnum, objFFRData.custaccnum, objFFRData.iatacargoagentcode, objFFRData.custname, System.DateTime.Now.ToString("yyyy-MM-dd"), "", "", "", "", "", RefNo, "FFR", objConsInfo[i].splhandling, objConsInfo[i].weightcode, ChargeableWeight, objFFRData.shipperpostcode, objFFRData.conspostcode, objFFRData.shipperaccnum, VolumeWt, objConsInfo[i].volumecode, DensityGrp };

                        object[] paramvalue = new object[] { objConsInfo[i].airlineprefix, objConsInfo[i].awbnum, AWBOriginAirportCode, AWBDestAirportCode, objConsInfo[i].pcscnt, objConsInfo[i].weight, objConsInfo[i].volumeamt, objConsInfo[i].commodity, objConsInfo[i].manifestdesc, objRouteInfo[0].carriercode, objRouteInfo[0].carriercode + objRouteInfo[0].fltnum, Flightdt, objRouteInfo[0].fltdept, objRouteInfo[0].fltarrival, objFFRData.shippername, objFFRData.shipperadd.Trim(','), objFFRData.shipperplace.Trim(','), objFFRData.shipperstate, objFFRData.shippercountrycode, objFFRData.shippercontactnum, objFFRData.consname, objFFRData.consadd.Trim(','), objFFRData.consplace.Trim(','), objFFRData.consstate, objFFRData.conscountrycode, objFFRData.conscontactnum, objFFRData.custaccnum, objFFRData.iatacargoagentcode, objFFRData.custname, System.DateTime.Now.ToString("yyyy-MM-dd"), "", "", "", "", "", RefNo, "FFR", objConsInfo[i].splhandling, objConsInfo[i].weightcode, ChargeableWeight, objFFRData.shipperpostcode, objFFRData.conspostcode, objFFRData.shipperaccnum, VolumeWt, objConsInfo[i].volumecode, DensityGrp, objFFRData.otherserviceinfo1, objFFRData.servicecode.Trim(','), objFFRData.participentidetifier };

                        SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

                        string procedure = "spInsertBookingDataFromFFR";

                        flag = dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                    }
                }
                string FlightDate = string.Empty;

                if (flag)
                {
                    #region : Save Audit Log - Booked :
                    DateTime flightDate = DateTime.UtcNow;
                    string flightdate = System.DateTime.Now.ToString("dd/MM/yyyy");
                    if (objRouteInfo.Length > 0)
                    {
                        strFlightNo = objRouteInfo[0].carriercode + objRouteInfo[0].fltnum;
                        strFlightOrigin = objRouteInfo[0].fltdept;
                        strFlightDestination = objRouteInfo[0].fltarrival;

                        if (objRouteInfo[0].date != "")
                        {
                            #region : Switch Flight Month :
                            string FlightMonth = "";
                            switch (objRouteInfo[0].month.Trim().ToUpper())
                            {
                                case "JAN":
                                    {
                                        FlightMonth = "01";
                                        break;
                                    }
                                case "FEB":
                                    {
                                        FlightMonth = "02";
                                        break;
                                    }
                                case "MAR":
                                    {
                                        FlightMonth = "03";
                                        break;
                                    }
                                case "APR":
                                    {
                                        FlightMonth = "04";
                                        break;
                                    }
                                case "MAY":
                                    {
                                        FlightMonth = "05";
                                        break;
                                    }
                                case "JUN":
                                    {
                                        FlightMonth = "06";
                                        break;
                                    }
                                case "JUL":
                                    {
                                        FlightMonth = "07";
                                        break;
                                    }
                                case "AUG":
                                    {
                                        FlightMonth = "08";
                                        break;
                                    }
                                case "SEP":
                                    {
                                        FlightMonth = "09";
                                        break;
                                    }
                                case "OCT":
                                    {
                                        FlightMonth = "10";
                                        break;
                                    }
                                case "NOV":
                                    {
                                        FlightMonth = "11";
                                        break;
                                    }
                                case "DEC":
                                    {
                                        FlightMonth = "12";
                                        break;
                                    }
                                default:
                                    {
                                        FlightMonth = "00";
                                        break;
                                    }
                            }
                            flightdate = objRouteInfo[0].date.PadLeft(2, '0') + "/" + FlightMonth.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

                            #endregion Switch Flight Month
                        }
                        else
                            flightdate = DateTime.Now.ToString("dd/MM/yyyy");
                    }

                    try
                    {
                        flightDate = DateTime.ParseExact(flightdate, "dd/MM/yyyy", null);
                    }
                    catch (Exception)
                    {
                        flightDate = DateTime.UtcNow;
                    }
                    string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "Volume" };
                    SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                    object[] CValues = new object[] { objConsInfo[0].airlineprefix, objConsInfo[0].awbnum, AWBOriginAirportCode, AWBDestAirportCode, objConsInfo[0].pcscnt, objConsInfo[0].weight, strFlightNo, flightDate, strFlightOrigin, strFlightDestination, "Booked", "FFR", "AWB Booked Through FFR Message", "FFR", DateTime.Now.ToString(), 1, objConsInfo[0].volumeamt.Trim() == "" ? "0" : objConsInfo[0].volumeamt };
                    if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                        clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                    #endregion

                    #region : Save AWB Route :
                    string status = "Q";
                    bool val = true;
                    if (objRouteInfo.Length > 0)
                    {
                        string[] paramname = new string[] { "AWBNum", "AWBPrefix" };
                        object[] paramobject = new object[] { awbnum, AWBPrefix };
                        SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                        if (dtb.ExecuteProcedure("spDeleteAWBRouteFFR", paramname, paramtype, paramobject))
                        {
                            for (int lstIndex = 0; lstIndex < objRouteInfo.Length; lstIndex++)
                            {
                                #region : Switch Flight Month :
                                string FlightMonth = "";
                                switch (objRouteInfo[lstIndex].month.Trim().ToUpper())
                                {
                                    case "JAN":
                                        {
                                            FlightMonth = "01";
                                            break;
                                        }
                                    case "FEB":
                                        {
                                            FlightMonth = "02";
                                            break;
                                        }
                                    case "MAR":
                                        {
                                            FlightMonth = "03";
                                            break;
                                        }
                                    case "APR":
                                        {
                                            FlightMonth = "04";
                                            break;
                                        }
                                    case "MAY":
                                        {
                                            FlightMonth = "05";
                                            break;
                                        }
                                    case "JUN":
                                        {
                                            FlightMonth = "06";
                                            break;
                                        }
                                    case "JUL":
                                        {
                                            FlightMonth = "07";
                                            break;
                                        }
                                    case "AUG":
                                        {
                                            FlightMonth = "08";
                                            break;
                                        }
                                    case "SEP":
                                        {
                                            FlightMonth = "09";
                                            break;
                                        }
                                    case "OCT":
                                        {
                                            FlightMonth = "10";
                                            break;
                                        }
                                    case "NOV":
                                        {
                                            FlightMonth = "11";
                                            break;
                                        }
                                    case "DEC":
                                        {
                                            FlightMonth = "12";
                                            break;
                                        }
                                    default:
                                        {
                                            FlightMonth = "00";
                                            break;
                                        }
                                }
                                FlightDate = FlightMonth.PadLeft(2, '0') + "/" + objRouteInfo[lstIndex].date.PadLeft(2, '0') + "/" + System.DateTime.Now.Year.ToString();

                                ///Find out if flight date with current year is less than server date time by at least 100 days.
                                DateTime dtFlightDate = DateTime.Now;
                                if (DateTime.TryParseExact(FlightDate, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtFlightDate))
                                {
                                    if (DateTime.Now.AddDays(-100) > dtFlightDate)
                                    {   ///Advance year in flight date to next year.
                                        dtFlightDate = dtFlightDate.AddYears(1);
                                        FlightDate = dtFlightDate.ToString("MM/dd/yyyy");
                                    }
                                }

                                string date = dtFlightDate.ToString("MM/dd/yyyy");

                                #endregion Switch Flight Month

                                #region : Set Route Status :

                                if (objRouteInfo[lstIndex].spaceallotmentcode.Trim().Equals("KK", StringComparison.OrdinalIgnoreCase))
                                    status = "C";
                                else if (objRouteInfo[lstIndex].spaceallotmentcode.Trim().Equals("XX", StringComparison.OrdinalIgnoreCase))
                                    status = "X";
                                else if (objRouteInfo[lstIndex].spaceallotmentcode.Trim().Equals("LL", StringComparison.OrdinalIgnoreCase))
                                    status = "Q";
                                else
                                    status = "Q";

                                #endregion Route Status

                                #region : Check Valid Flights :
                                string[] parms = new string[]
                                {
                                        "FltOrigin",
                                        "FltDestination",
                                        "FlightNo",
                                        "flightDate",
                                        "AWBNumber",
                                        "AWBPrefix",
                                        "RefNo",
                                        "UpdatedBy"
                                };
                                SqlDbType[] dataType = new SqlDbType[]
                                {
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.DateTime,
                                        SqlDbType.VarChar,
                                        SqlDbType.VarChar,
                                        SqlDbType.Int,
                                        SqlDbType.VarChar
                                };
                                object[] value = new object[]
                                {

                                        objRouteInfo[lstIndex].fltdept,
                                        objRouteInfo[lstIndex].fltarrival,
                                        objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                        DateTime.Parse(FlightDate),
                                        awbnum,
                                        AWBPrefix,
                                        RefNo,
                                        "FFR"
                                };
                                DataSet dsdata = dtb.SelectRecords("Messaging.uspValidateFFRFWBFlight", parms, value, dataType);
                                int schedid = 0;

                                if (dsdata != null && dsdata.Tables.Count > 0)
                                {
                                    for (int i = 0; i < dsdata.Tables.Count; i++)
                                    {
                                        if (dsdata.Tables[i].Columns.Contains("ScheduleID") && Convert.ToInt32(dsdata.Tables[i].Rows[0]["ScheduleID"].ToString()) > 0)
                                        {
                                            val = true;
                                            schedid = Convert.ToInt32(dsdata.Tables[i].Rows[0]["ScheduleID"].ToString());
                                        }
                                    }
                                }
                                #endregion Check Valid Flights

                                #region : Save AWB Route :

                                #region : Parameter Name & Data Types :
                                string[] paramNames = new string[]
                                {
                                    "AWBNumber", "FltOrigin", "FltDestination", "FltNumber", "FltDate", "Status", "UpdatedBy", "UpdatedOn", "IsFFR", "REFNo",
                                    "date", "AWBPrefix", "schedid", "allotmentcode", "voluemcode", "volume"
                                };
                                SqlDbType[] dataTypes = new SqlDbType[]
                                {
                                    SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Int,
                                    SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal
                                };
                                object[] values;
                                #endregion Parameter Name & Data Types

                                if (!isOriginDestinationMismatch)
                                {
                                    #region : O & D Matched :
                                    values = new object[]
                                    {
                                            awbnum,
                                            objRouteInfo[lstIndex].fltdept,
                                            objRouteInfo[lstIndex].fltarrival,
                                            objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                            DateTime.Parse(FlightDate),
                                            status,
                                            "FFR",
                                            DateTime.Now,
                                            1,
                                            RefNo,
                                            DateTime.Parse(FlightDate),
                                            AWBPrefix,
                                            schedid,
                                            (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                            (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                            (objConsInfo[0].volumeamt!=null && objConsInfo[0].volumeamt!="" ?  objConsInfo[0].volumeamt:"0.00")
                                    };
                                    if (!dtb.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))
                                        clsLog.WriteLogAzure("Error in Save AWB Route FFR " + dtb.LastErrorDescription);
                                    #endregion O & D Matched
                                }
                                else
                                {
                                    #region : O & D Mismatched :
                                    bool isSecondLegAdded = false;
                                    ///Origin Miasmatched
                                    if (lstIndex == 0 && objRouteInfo[0].fltdept.Trim() != AWBOriginAirportCode.Trim())
                                    {
                                        ///1st Leg
                                        values = new object[]
                                        {
                                            awbnum,
                                            strAWBOrigin,
                                            objRouteInfo[lstIndex].fltdept,
                                            "",
                                            DateTime.Now,
                                            status,
                                            "FFR",
                                            DateTime.Now,
                                            1,
                                            RefNo,
                                            DateTime.Now,
                                            AWBPrefix,
                                            schedid,
                                            (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                            (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                            (objConsInfo[0].volumeamt!=null && objConsInfo[0].volumeamt!="" ?  objConsInfo[0].volumeamt:"0.00")
                                        };
                                        SaveFFRAWBRoute(paramNames, dataTypes, values);

                                        ///2nd Leg
                                        values = new object[]
                                        {
                                            awbnum,
                                            objRouteInfo[lstIndex].fltdept,
                                            objRouteInfo[lstIndex].fltarrival,
                                            objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                            DateTime.Parse(FlightDate),
                                            status,
                                            "FFR",
                                            DateTime.Now,
                                            1,
                                            RefNo,
                                            DateTime.Parse(FlightDate),
                                            AWBPrefix,
                                            schedid,
                                            (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                            (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                            (objConsInfo[0].volumeamt!=null && objConsInfo[0].volumeamt!="" ?  objConsInfo[0].volumeamt:"0.00")
                                        };
                                        SaveFFRAWBRoute(paramNames, dataTypes, values);
                                        isSecondLegAdded = true;
                                    }
                                    ///Destination Miasmatched
                                    if (lstIndex == objRouteInfo.Length - 1 && objRouteInfo[objRouteInfo.Length - 1].fltarrival.Trim() != AWBDestAirportCode.Trim())
                                    {
                                        ///1st Leg
                                        if (!isSecondLegAdded)///This flag handled the condition when FFR has only one route with O and D mismathed
                                        {
                                            values = new object[]
                                            {
                                                awbnum,
                                                objRouteInfo[lstIndex].fltdept,
                                                objRouteInfo[lstIndex].fltarrival,
                                                objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                                DateTime.Parse(FlightDate),
                                                status,
                                                "FFR",
                                                DateTime.Now,
                                                1,
                                                RefNo,
                                                DateTime.Parse(FlightDate),
                                                AWBPrefix,
                                                schedid,
                                                (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                                (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                                (objConsInfo[0].volumeamt!=null && objConsInfo[0].volumeamt!="" ?  objConsInfo[0].volumeamt:"0.00")
                                            };
                                            SaveFFRAWBRoute(paramNames, dataTypes, values);
                                        }
                                        ///2nd Leg
                                        values = new object[]
                                        {
                                            awbnum,
                                            objRouteInfo[lstIndex].fltarrival.Trim(),
                                            strAWBDestination,
                                            "",
                                            DateTime.Parse(FlightDate).AddDays(1),
                                            status,
                                            "FFR",
                                            DateTime.Now,
                                            1,
                                            RefNo,
                                            DateTime.Parse(FlightDate),
                                            AWBPrefix,
                                            schedid,
                                            (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                            (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                            (objConsInfo[0].volumeamt!=null && objConsInfo[0].volumeamt!="" ?  objConsInfo[0].volumeamt:"0.00")
                                        };
                                        SaveFFRAWBRoute(paramNames, dataTypes, values);
                                    }
                                    else
                                    {
                                        ///Transit leg(s)
                                        values = new object[]
                                        {
                                            awbnum,
                                            objRouteInfo[lstIndex].fltdept,
                                            objRouteInfo[lstIndex].fltarrival,
                                            objRouteInfo[lstIndex].carriercode+objRouteInfo[lstIndex].fltnum,
                                            DateTime.Parse(FlightDate),
                                            status,
                                            "FFR",
                                            DateTime.Now,
                                            1,
                                            RefNo,
                                            DateTime.Parse(FlightDate),
                                            AWBPrefix,
                                            schedid,
                                            (objRouteInfo[lstIndex].allotidentification!=null?objRouteInfo[lstIndex].allotidentification:""),
                                            (objConsInfo[0].volumecode!=null?objConsInfo[0].volumecode:""),
                                            (objConsInfo[0].volumeamt!=null && objConsInfo[0].volumeamt!="" ?  objConsInfo[0].volumeamt:"0.00")
                                        };
                                        SaveFFRAWBRoute(paramNames, dataTypes, values);
                                    }
                                    #endregion O & D Mismatched
                                }
                                #endregion Save AWB Route

                                #region : Save Audit Log - Booked :
                                string[] CANname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "Volume" };
                                SqlDbType[] CAType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar };
                                object[] CAValues = new object[] { objConsInfo[0].airlineprefix, objConsInfo[0].awbnum, AWBOriginAirportCode, AWBDestAirportCode, objConsInfo[0].pcscnt, objConsInfo[0].weight, objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum, DateTime.Parse(FlightDate), objRouteInfo[lstIndex].fltdept, objRouteInfo[lstIndex].fltarrival, "Booked", "FFR", "AWB Flight Information", "FFR", DateTime.Now.ToString(), 1, objConsInfo[0].volumeamt.Trim() == "" ? "0" : objConsInfo[0].volumeamt };
                                if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CANname, CAType, CAValues))
                                    clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                                #endregion Save Audit Log

                                #region : Refresh Capacity :
                                //if ((objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum).Trim() != string.Empty && flightDate != null && objRouteInfo[lstIndex].fltdept.Trim() != string.Empty)
                                //{
                                //    SQLServer sqlServer = new SQLServer();
                                //    SqlParameter[] sqlParameter = new SqlParameter[]{
                                //    new SqlParameter("@FlightID",(objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum).Trim())
                                //        , new SqlParameter("@FlightDate",FlightDate)
                                //        , new SqlParameter("@Source",objRouteInfo[lstIndex].fltdept.Trim())
                                //    };
                                //    DataSet dsRefreshCapacity = sqlServer.SelectRecords("uspRefreshCapacity", sqlParameter);
                                //}
                                #endregion
                            }
                            #region : Delete AWB Data if No Route Present :
                            if (val == true)
                            {
                                string[] QueryNames = { "AWBPrefix", "AWBNumber" };
                                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                object[] QueryValues = { AWBPrefix, awbnum };
                                if (!dtb.UpdateData("spDeleteAWBDetailsNoRoute", QueryNames, QueryTypes, QueryValues))
                                {
                                    clsLog.WriteLogAzure("Error in Deleting AWB Details " + dtb.LastErrorDescription);
                                }
                            }
                            #endregion Delete AWB Data if No Route Present
                        }
                    }
                    #endregion Save AWB Route

                    bool IsDimsPresent = false;
                    if (val)
                    {
                        #region AWB Dimensions
                        if (objDimension.Length > 0)
                        {
                            IsDimsPresent = true;
                            //priyanka check awbgross wt=dims weight

                            //decimal totalDimsWt = 0;                    

                            //for (int i = 0; i < objDimension.Length; i++)
                            //{

                            //    totalDimsWt = totalDimsWt + Convert.ToDecimal(objDimension[i].weight);
                            //}

                            if (awbWeight > totalDimsWt)
                            {
                                objDimension[0].weight = Convert.ToString(Convert.ToDecimal(objDimension[0].weight) + (awbWeight - totalDimsWt));
                            }



                            //Badiuz khan
                            //Description: Delete Dimension if Dimension 
                            string[] dparam = { "AWBPrefix", "AWBNumber" };
                            SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                            object[] dbparamvalues = { AWBPrefix, awbnum };

                            if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
                                clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                            else
                            {

                                for (int i = 0; i < objDimension.Length; i++)
                                {
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

                                    string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "UpdatedBy" };
                                    SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar };
                                    //Decimal DimWeight = 0;


                                    if (string.IsNullOrEmpty(objDimension[i].length))
                                    {
                                        objDimension[i].length = "0";
                                    }
                                    if (string.IsNullOrEmpty(objDimension[i].width))
                                    {
                                        objDimension[i].width = "0";
                                    }
                                    if (string.IsNullOrEmpty(objDimension[i].height))
                                    {
                                        objDimension[i].height = "0";
                                    }

                                    if (!updateDIMSwt)
                                    {
                                        if (i == 0)
                                        {
                                            objDimension[i].weight = objConsInfo[0].weight;
                                        }
                                        else
                                        {
                                            objDimension[i].weight = "0";
                                        }
                                    }



                                    object[] value =
                                        {
                                            awbnum,"1",objDimension[i].length,objDimension[i].width,objDimension[i].height,
                                             objDimension[i].piecenum,objDimension[i].mesurunitcode,AWBPrefix,Convert.ToDecimal( objDimension[i].weight),"FFR"
                                        };

                                    if (!dtb.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
                                        clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                                }
                                #region : Update audit trail volume :
                                string[] QueryNames = { "AWBPrefix", "AWBNumber", "UpdatedBy" };
                                SqlDbType[] QueryTypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                                object[] QueryValues = { AWBPrefix, awbnum, "FFR" };
                                if (!dtb.UpdateData("Messaging.uspUpdateAuditLogVolume", QueryNames, QueryTypes, QueryValues))
                                {
                                    clsLog.WriteLogAzure("Error while Update Volume In AuditLog " + dtb.LastErrorDescription);
                                }
                                #endregion Update audit trail volume

                            }
                        }
                        else
                        {
                            string[] dDparam = { "AWBPrefix", "AWBNumber" };
                            SqlDbType[] dDbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                            object[] dDbparamvalues = { AWBPrefix, awbnum };

                            if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dDparam, dDbparamtypes, dDbparamvalues))
                                clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                        }
                        #endregion

                        //Add ULD Info It is as BUP
                        if (Uldinfo != null && Uldinfo.Length > 0)
                        {
                            if (!IsDimsPresent)
                            {
                                // delete Dims info if no DIMS present in same Message

                                string[] dparam = { "AWBPrefix", "AWBNumber" };
                                SqlDbType[] dbparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                object[] dbparamvalues = { AWBPrefix, awbnum };

                                if (!dtb.InsertData("SpDeleteDimensionThroughMessage", dparam, dbparamtypes, dbparamvalues))
                                    clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);
                            }

                            //Add data in AWBDIMS



                            for (int i = 0; i < Uldinfo.Length; i++)
                            {

                                //string[] param = { "AWBNumber", "RowIndex", "Length", "Breadth", "Height", "PcsCount", "MeasureUnit", "AWBPrefix", "Weight", "UpdatedBy" };
                                //SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Decimal, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.VarChar };
                                ////Decimal DimWeight = 0;
                                //object[] value =
                                //{
                                //        awbnum,"1","0","0","0"
                                //            ,1,Uldinfo[i].uldweightcode ,AWBPrefix, Uldinfo[i].uldweight==string.Empty?0:Convert.ToDecimal( Uldinfo[i].uldweight),"FFR"
                                //        };

                                //if (!dtb.InsertData("SP_SaveAWBDimensions_FFR", param, dbtypes, value))
                                //    clsLog.WriteLogAzure("Error  Delete Dimension Through Message :" + awbnum);

                                string uldno = Uldinfo[i].uldtype + Uldinfo[i].uldsrno + Uldinfo[i].uldowner;
                                int uldslacPcs = 0;
                                string[] param = { "AWBPrefix", "AWBNumber", "ULDNo", "SlacPcs", "PcsCount", "Volume", "GrossWeight", };
                                SqlDbType[] dbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.Decimal };
                                object[] value = { AWBPrefix, awbnum, uldno, uldslacPcs, 1, "0", decimal.Parse(Uldinfo[i].uldweight == "" ? "0" : Uldinfo[i].uldweight) };

                                if (!dtb.InsertData("SaveandUpdateShippperBUPThroughFWB", param, dbtypes, value))
                                {
                                    string str = dtb.LastErrorDescription.ToString();
                                    clsLog.WriteLogAzure("BUP ULD is not Updated  for:" + awbnum + Environment.NewLine + "Error : " + dtb.LastErrorDescription);

                                }
                            }


                        }

                        #region ProcessRateFunction
                        DataSet dsrateCheck = CheckAirlineForRateProcessing(AWBPrefix, "FFR");
                        if (dsrateCheck != null && dsrateCheck.Tables.Count > 0 && dsrateCheck.Tables[0].Rows.Count > 0)
                        {

                            string[] CRNname = new string[] { "AWBNumber", "AWBPrefix", "UpdatedBy", "UpdatedOn", "ValidateMin", "UpdateBooking", "RouteFrom", "UpdateBilling" };
                            SqlDbType[] CRType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.Bit };
                            object[] CRValues = new object[] { awbnum, AWBPrefix, "FFR", System.DateTime.Now, 1, 1, "B", 0 };
                            if (!dtb.ExecuteProcedure("sp_CalculateAWBRatesReprocess", CRNname, CRType, CRValues))
                            {
                                clsLog.WriteLog("Rates Not Calculated for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                            }

                        }
                        #endregion

                        ///MasterLog
                        GenericFunction gf = new GenericFunction();
                        DataSet dsAWBMaterLogNewValues = new DataSet();
                        dsAWBMaterLogNewValues = gf.GetAWBMasterLogNewRecord(AWBPrefix, awbnum);
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
                            gf.MasterAuditLog(dtOldValues, dtNewValues, AWBPrefix, awbnum, "Save", "FFR", System.DateTime.UtcNow);
                        }

                        #region Capacity Update

                        string[] cparam = { "AWBPrefix", "AWBNumber", "UpdatedBy" };
                        SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                        object[] cparamvalues = { AWBPrefix, awbnum, "FFR" };
                        if (!dtb.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))
                            clsLog.WriteLogAzure("Error  on Update capacity Plan :" + awbnum);

                        #endregion

                        if (flag)
                            GenerateFMAMessage(strFFRMessage, "FFR RECEVIED AND WILL CONFORM THE BOOKING ASAP FOR THIS AWBNO " + AWBPrefix + '-' + awbnum, AWBPrefix, awbnum, strFromID);

                        #region : Refresh Capacity :
                        for (int lstIndex = 0; lstIndex < objRouteInfo.Length; lstIndex++)
                        {
                            if ((objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum).Trim() != string.Empty && flightDate != null && objRouteInfo[lstIndex].fltdept.Trim() != string.Empty)
                            {
                                SQLServer sqlServer = new SQLServer();
                                SqlParameter[] sqlParameter = new SqlParameter[]{
                                    new SqlParameter("@FlightID",(objRouteInfo[lstIndex].carriercode + objRouteInfo[lstIndex].fltnum).Trim())
                                        , new SqlParameter("@FlightDate",FlightDate)
                                        , new SqlParameter("@Source",objRouteInfo[lstIndex].fltdept.Trim())
                                    };
                                DataSet dsRefreshCapacity = sqlServer.SelectRecords("uspRefreshCapacity", sqlParameter);
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                ErrorMsg = string.Empty;
            }
            return flag;
        }

        public DataSet ValidateFFRAWB(string awbPrefix, string awbNumber, string awbOrigin, string awbDestination, string updatedBy, bool isValidateAWB, bool isFirstLeg = false, bool isLastLeg = false, bool isDestinationAdjusted = false, bool isFlightDayAvailable = false, string flightOrigin = "", string flightDest = "", string flightNumber = "", int flightDay = 0, int REFNo = 0, string shipperSignature = "", string IATAAgentCode = "", string issueDate = "")
        {
            DataSet dsResult = new DataSet();
            SQLServer sqlServer = new SQLServer();
            try
            {
                string[] paramNames = new string[] { "AirlinePrefix", "AWBNo", "AWBOrigin", "AWBDestination", "UpdatedBy", "IsValidateAWB"
                    , "IsFirstLeg", "IsLastLeg", "IsDestinationAdjusted", "IsFlightDayAvailable"
                    , "FlightOrigin", "FlightDest", "FlightNumber", "FlightDay", "REFNo", "SHPSignature", "IATAAgentCode", "IssueDate"};

                object[] paramValues = new object[] { awbPrefix, awbNumber, awbOrigin, awbDestination, updatedBy, isValidateAWB
                    , isFirstLeg, isLastLeg, isDestinationAdjusted, isFlightDayAvailable
                    , flightOrigin, flightDest, flightNumber , flightDay, REFNo, shipperSignature, IATAAgentCode, issueDate};

                SqlDbType[] paramTypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit
                    , SqlDbType.Bit, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.Bit
                    , SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar};

                dsResult = sqlServer.SelectRecords("Messaging.uspValidateFFRAWB", paramNames, paramValues, paramTypes);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
            return dsResult;
        }

        private void SaveFFRAWBRoute(string[] paramNames, SqlDbType[] dataTypes, object[] values)
        {
            try
            {
                SQLServer sqlServer = new SQLServer();
                if (!sqlServer.UpdateData("spSaveFFRAWBRoute", paramNames, dataTypes, values))
                    clsLog.WriteLogAzure("Error in Save AWB Route FFR " + sqlServer.LastErrorDescription);
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strRecord"></param>
        public DataSet CheckValidateFFRMessage(string AirlinePrefix, string AWBNo, string AWBOrigin, string AWBDestination, string UpdateBy, string FlightOrigin = "", string FlightDest = "", string FlightID = "", string FlightDate = "", String OrgFltID = "", int REFNo = 0)
        {
            string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            SQLServer da = new SQLServer();
            DataSet ds = new DataSet();
            string[] Pname = new string[11];
            object[] Pvalue = new object[11];
            SqlDbType[] Ptype = new SqlDbType[11];

            try
            {
                Pname[0] = "AirlinePrefix";
                Ptype[0] = SqlDbType.VarChar;
                Pvalue[0] = AirlinePrefix;

                Pname[1] = "AWBNo";
                Ptype[1] = SqlDbType.VarChar;
                Pvalue[1] = AWBNo;

                Pname[2] = "AWBOrigin";
                Ptype[2] = SqlDbType.VarChar;
                Pvalue[2] = AWBOrigin;

                Pname[3] = "AWBDestination";
                Ptype[3] = SqlDbType.VarChar;
                Pvalue[3] = AWBDestination;

                Pname[4] = "UpdateBy";
                Ptype[4] = SqlDbType.VarChar;
                Pvalue[4] = UpdateBy;

                Pname[5] = "FlightOrigin";
                Ptype[5] = SqlDbType.VarChar;
                Pvalue[5] = FlightOrigin;

                Pname[6] = "FlightDest";
                Ptype[6] = SqlDbType.VarChar;
                Pvalue[6] = FlightDest;

                Pname[7] = "FlightID";
                Ptype[7] = SqlDbType.VarChar;
                Pvalue[7] = FlightID;

                Pname[8] = "FlightDate";
                Ptype[8] = SqlDbType.VarChar;
                Pvalue[8] = FlightDate;

                Pname[9] = "OrgFltID";
                Ptype[9] = SqlDbType.VarChar;
                Pvalue[9] = OrgFltID;

                Pname[10] = "REFNo";
                Ptype[10] = SqlDbType.Int;
                Pvalue[10] = REFNo;

                ds = da.SelectRecords("SPValidateFFRAWB", Pname, Pvalue, Ptype);

                return ds;

            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                //clsLog.WriteLogAzure(ex, "BLExpManifest", "GetAwbTabdetails_GHA");
                return ds = null;
            }
            finally
            {
                da = null;
                if (ds != null)
                    ds.Dispose();
                Pname = null;
                Pvalue = null;
                Ptype = null;
            }
        }

        public DataSet CheckValidateXFFRMessage(string AirlinePrefix, string AWBNo, string AWBOrigin, string AWBDestination, string UpdateBy, string FlightOrigin = "", string FlightDest = "", string FlightID = "", string FlightDate = "", String OrgFltID = "", int REFNo = 0)
        {
            string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            SQLServer da = new SQLServer();
            DataSet ds = new DataSet();
            string[] Pname = new string[11];
            object[] Pvalue = new object[11];
            SqlDbType[] Ptype = new SqlDbType[11];

            try
            {
                Pname[0] = "AirlinePrefix";
                Ptype[0] = SqlDbType.VarChar;
                Pvalue[0] = AirlinePrefix;

                Pname[1] = "AWBNo";
                Ptype[1] = SqlDbType.VarChar;
                Pvalue[1] = AWBNo;

                Pname[2] = "AWBOrigin";
                Ptype[2] = SqlDbType.VarChar;
                Pvalue[2] = AWBOrigin;

                Pname[3] = "AWBDestination";
                Ptype[3] = SqlDbType.VarChar;
                Pvalue[3] = AWBDestination;

                Pname[4] = "UpdateBy";
                Ptype[4] = SqlDbType.VarChar;
                Pvalue[4] = UpdateBy;

                Pname[5] = "FlightOrigin";
                Ptype[5] = SqlDbType.VarChar;
                Pvalue[5] = FlightOrigin;

                Pname[6] = "FlightDest";
                Ptype[6] = SqlDbType.VarChar;
                Pvalue[6] = FlightDest;

                Pname[7] = "FlightID";
                Ptype[7] = SqlDbType.VarChar;
                Pvalue[7] = FlightID;

                Pname[8] = "FlightDate";
                Ptype[8] = SqlDbType.VarChar;
                Pvalue[8] = FlightDate;

                Pname[9] = "OrgFltID";
                Ptype[9] = SqlDbType.VarChar;
                Pvalue[9] = OrgFltID;

                Pname[10] = "REFNo";
                Ptype[10] = SqlDbType.Int;
                Pvalue[10] = REFNo;

                ds = da.SelectRecords("Messaging.uspValidateXFFRAWB", Pname, Pvalue, Ptype);

                return ds;

            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                //clsLog.WriteLogAzure(ex, "BLExpManifest", "GetAwbTabdetails_GHA");
                return ds = null;
            }
            finally
            {
                da = null;
                if (ds != null)
                    ds.Dispose();
                Pname = null;
                Pvalue = null;
                Ptype = null;
            }
        }
        /// <summary>
        /// Validate AWBNumber and flight
        /// Merged from DEV V5
        /// </summary>
        public DataSet CheckValidateXFFRMessage(string AirlinePrefix, string AWBNo, string AWBOrigin, string AWBDestination,
            string UpdateBy, string FlightOrigin, string FlightDest, string FlightID, string FlightDate, String OrgFltID, int Refno, string agentParticipentIdentifier)
        {
            string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
            SQLServer da = new SQLServer();
            DataSet ds = new DataSet();
            string[] Pname = new string[12];
            object[] Pvalue = new object[12];
            SqlDbType[] Ptype = new SqlDbType[12];

            try
            {
                Pname[0] = "AirlinePrefix";
                Ptype[0] = SqlDbType.VarChar;
                Pvalue[0] = AirlinePrefix;

                Pname[1] = "AWBNo";
                Ptype[1] = SqlDbType.VarChar;
                Pvalue[1] = AWBNo;

                Pname[2] = "AWBOrigin";
                Ptype[2] = SqlDbType.VarChar;
                Pvalue[2] = AWBOrigin;

                Pname[3] = "AWBDestination";
                Ptype[3] = SqlDbType.VarChar;
                Pvalue[3] = AWBDestination;

                Pname[4] = "UpdateBy";
                Ptype[4] = SqlDbType.VarChar;
                Pvalue[4] = UpdateBy;

                Pname[5] = "FlightOrigin";
                Ptype[5] = SqlDbType.VarChar;
                Pvalue[5] = FlightOrigin;

                Pname[6] = "FlightDest";
                Ptype[6] = SqlDbType.VarChar;
                Pvalue[6] = FlightDest;

                Pname[7] = "FlightID";
                Ptype[7] = SqlDbType.VarChar;
                Pvalue[7] = FlightID;

                Pname[8] = "FlightDate";
                Ptype[8] = SqlDbType.VarChar;
                Pvalue[8] = FlightDate;

                Pname[9] = "OrgFltID";
                Ptype[9] = SqlDbType.VarChar;
                Pvalue[9] = OrgFltID;

                Pname[10] = "Refno";
                Ptype[10] = SqlDbType.Int;
                Pvalue[10] = Refno;

                Pname[11] = "FWBAgentCode";
                Ptype[11] = SqlDbType.VarChar;
                Pvalue[11] = agentParticipentIdentifier;

                ds = da.SelectRecords("Messaging.uspValidateXFFRAWB", Pname, Pvalue, Ptype);

                return ds;

            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                //clsLog.WriteLogAzure(ex, "BLExpManifest", "GetAwbTabdetails_GHA");
                return ds = null;
            }
            finally
            {
                da = null;
                if (ds != null)
                    ds.Dispose();
                Pname = null;
                Pvalue = null;
                Ptype = null;
            }
        }

        public DataSet CheckAirlineForRateProcessing(string AirlinePrefix, string MessageType)
        {
            DataSet dssitaMessage = new DataSet();
            try
            {
                string conStr = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
                SQLServer dtb = new SQLServer(true);

                string[] paramname = new string[] { "AirlinePrefix", "MessageType" };
                object[] paramvalue = new object[] { AirlinePrefix, MessageType };
                SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                dssitaMessage = dtb.SelectRecords("CheckAirlineForRateProcessing", paramname, paramvalue, paramtype);
            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                dssitaMessage = null;
            }
            return dssitaMessage;


        }
        #endregion Public Methods

        #region :: Private Methods ::
        private void GenerateFNAMessage(string strMessage, string strErrorMessage, string AWBPrefix, string awbNumber, string strFromID)
        {
            GenericFunction GF = new GenericFunction();
            string SitaMessageHeader = string.Empty, SFTPHeaderSITAddress = string.Empty, Emailaddress = string.Empty, FNAMessageVersion = string.Empty, messageid = string.Empty;
            //strMessage = strMessage.Replace("$", "\r\n");
            //strMessage = strMessage.Replace("$", "\n");
            //strMessage = strMessage.Replace("$$", "\r\n");




            DataSet dscheckconfiguration = GF.GetSitaAddressandMessageVersion("", "FNA", "AIR", "", "", "", string.Empty, AWBPrefix);
            if (dscheckconfiguration != null && dscheckconfiguration.Tables[0].Rows.Count > 0)
            {
                Emailaddress = dscheckconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                string MessageCommunicationType = dscheckconfiguration.Tables[0].Rows[0]["MsgCommType"].ToString();
                FNAMessageVersion = dscheckconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                messageid = dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString();


            }

            StringBuilder strFNAMessage = new StringBuilder();
            strFNAMessage.Append("FNA/1\r\n");
            strFNAMessage.Append("ACK/");
            strFNAMessage.Append(strErrorMessage + "\r\n");
            strFNAMessage.Append(strMessage);
            if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows.Count > 0)
            {
                if (dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString() != string.Empty)
                {

                    SitaMessageHeader = GF.MakeMailMessageFormat(strFromID, dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                    GF.SaveMessageOutBox("FNA", SitaMessageHeader + "\r\n" + strFNAMessage, "SITAFTP", "SITAFTP", "", "", "", "", AWBPrefix + "-" + awbNumber);
                }

                if (dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                {

                    SFTPHeaderSITAddress = GF.MakeMailMessageFormat(dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                    GF.SaveMessageOutBox("FNA", SFTPHeaderSITAddress + "\r\n" + strFNAMessage, "SFTP", "SFTP", "", "", "", "", AWBPrefix + "-" + awbNumber);
                }
            }
            string ToEmailAddress = (strFromID == string.Empty ? Emailaddress : strFromID + "," + Emailaddress);
            //ToEmailAddress = (ToEmailAddress == string.Empty ? "prashant@smartkargo.com" : ToEmailAddress);

            if (ToEmailAddress.Trim().Length > 0)
            {
                GF.SaveMessageOutBox("FNA", strFNAMessage.ToString(), string.Empty, ToEmailAddress, "", "", "", "", AWBPrefix + "-" + awbNumber);
            }
            //GenericFunction GF = new GenericFunction();
            //strMessage = strMessage.Replace("$", "\r\n");
            //strMessage = strMessage.Replace("$", "\n");
            //strMessage = strMessage.Replace("$$", "\r\n");

            //StringBuilder strFNAMessage = new StringBuilder();
            //strFNAMessage.Append("FNA/1\r\n");
            //strFNAMessage.Append("ACK/");
            //strFNAMessage.Append(strErrorMessage + "\r\n");
            //strFNAMessage.Append(strMessage);
            //GF.SaveMessageOutBox("MSG:FNA", strFNAMessage.ToString(), "", "SITAFTP");

        }

        private void GenerateFMAMessage(string strMessage, string strSuccessMessage, string AWBPrefix, string awbNumber, string strFromID)
        {
            GenericFunction GF = new GenericFunction();

            string SitaMessageHeader = string.Empty, Emailaddress = string.Empty, FMAMessageVersion = string.Empty, messageid = string.Empty;
            strMessage = strMessage.Replace("$", "\r\n");
            strMessage = strMessage.Replace("$", "\n");
            strMessage = strMessage.Replace("$$", "\r\n");

            StringBuilder strFMAMessage = new StringBuilder();
            strFMAMessage.Append("FMA\r\n");
            strFMAMessage.Append("ACK/");
            strFMAMessage.Append(strSuccessMessage + "\r\n");
            strFMAMessage.Append(strMessage);

            DataSet dscheckconfiguration = GF.GetSitaAddressandMessageVersion("", "FMA", "AIR", "", "", "", string.Empty, AWBPrefix);
            if (dscheckconfiguration != null && dscheckconfiguration.Tables[0].Rows.Count > 0)
            {
                Emailaddress = dscheckconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                string MessageCommunicationType = dscheckconfiguration.Tables[0].Rows[0]["MsgCommType"].ToString();
                FMAMessageVersion = dscheckconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                messageid = dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString();


            }
            if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows.Count > 0)
            {
                if (dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString() != string.Empty)
                {
                    SitaMessageHeader = GF.MakeMailMessageFormat(strFromID, dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), messageid);
                    GF.SaveMessageOutBox("FMA", SitaMessageHeader + "\r\n" + strFMAMessage, "SITAFTP", "SITAFTP", "", "", "", "", AWBPrefix + "-" + awbNumber);
                }
            }

            if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
            {
                string SFTPHeaderSITAddress = string.Empty;
                SFTPHeaderSITAddress = GF.MakeMailMessageFormat(dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                GF.SaveMessageOutBox("FNA", SFTPHeaderSITAddress + "\r\n" + strFMAMessage, "SFTP", "SFTP", "", "", "", "", AWBPrefix + "-" + awbNumber);
            }
            string ToEmailAddress = (strFromID == string.Empty ? Emailaddress : strFromID + "," + Emailaddress);
            //ToEmailAddress = (ToEmailAddress == string.Empty ? "prashant@smartkargo.com" : ToEmailAddress);
            if (ToEmailAddress.Trim().Length > 0)
            {
                GF.SaveMessageOutBox("FMA", strFMAMessage.ToString(), string.Empty, ToEmailAddress, "", "", "", "", AWBPrefix + "-" + awbNumber);
            }
            //GenericFunction GF = new GenericFunction();
            //strMessage = strMessage.Replace("$", "\r\n");
            //strMessage = strMessage.Replace("$", "\n");
            //strMessage = strMessage.Replace("$$", "\r\n");

            //StringBuilder strFMAMessage = new StringBuilder();
            //strFMAMessage.Append("FMA\r\n");
            //strFMAMessage.Append("ACK/");
            //strFMAMessage.Append(strSuccessMessage + "\r\n");
            //strFMAMessage.Append(strMessage);
            //GF.SaveMessageOutBox("MSG:FMA", strFMAMessage.ToString(), "", "SITAFTP");

        }
        #endregion Private Methods
    }
}
