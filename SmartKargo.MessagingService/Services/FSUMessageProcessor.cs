
#region FSU Message Processor Class Description
/* FSUMessage Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-03-04
      * Approved By          :
      * Approved Date        :
      * Modified By          :  
      * Modified On          :   
      * Description          :   
     */
#endregion

//using System; //Not in used
//using System.Collections.Generic;//Not in used
//using System.Linq;//Not in used
//using System.Web;//Not in used
//using System.Text.RegularExpressions;//Not in used
using Grpc.Core;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using SmartKargo.MessagingService.Services;

//using System.Text;//Not in used
//using System.IO;//Not in used
//using System.Reflection;//Not in used
using System.Data;
using System.Globalization;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
//using QID.DataAccess;//Not in used
//using System.Configuration;//Not in used
//using QidWorkerRole;//Not in used
//using System.Data.SqlClient;//Not in used

namespace QidWorkerRole
{
    public class FSUMessageProcessor
    {
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FSUMessageProcessor> _logger;
        private static ILoggerFactory? _loggerFactory;
        private static ILogger<FSUMessageProcessor> _staticLogger => _loggerFactory?.CreateLogger<FSUMessageProcessor>();

        private readonly GenericFunction _genericFunction;
        private readonly Cls_BL _clsBL;
        private readonly FFRMessageProcessor _fFRMessageProcessor;
        private readonly FWBMessageProcessor _fWBMessageProcessor;
        private readonly FHLMessageProcessor _fHLMessageProcessor;


        #region :: Constructor ::
        public FSUMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FSUMessageProcessor> logger, GenericFunction genericFunction,
            Cls_BL clsBL, FFRMessageProcessor fFRMessageProcessor, FWBMessageProcessor fWBMessageProcessor,
            FHLMessageProcessor fHLMessageProcessor,
            ILoggerFactory loggerFactory)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _loggerFactory = loggerFactory;
            _genericFunction = genericFunction;
            _clsBL = clsBL;
            _fFRMessageProcessor = fFRMessageProcessor;
            _fWBMessageProcessor = fWBMessageProcessor;
            _fHLMessageProcessor = fHLMessageProcessor;
        }
        #endregion
        #region :: Public Methods ::

        /// <summary>
        /// Method to decode FSU message string and assing decoded data to the array of structure
        /// </summary>
        /// <param name="refNO">SrNo from tblInbox</param>
        /// <param name="fsamsg">Message string</param>
        /// <param name="fsadata">Contains consignment information</param>
        /// <param name="fsanodes">Contains flight information</param>
        /// <param name="custominfo">Other Customs, Security and Regulatory Information</param>
        /// <param name="uld">ULD information</param>
        /// <param name="othinfoarray">Other service information</param>
        /// <returns>Return true when message decoded successfuly</returns>
        public bool DecodeReceivedFSUMessage(int refNO, string fsamsg, ref MessageData.FSAInfo fsadata, ref MessageData.CommonStruct[] fsanodes, ref MessageData.customsextrainfo[] custominfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray)
        {
            string awbref = string.Empty;
            bool flag = false;
            string lastrec = string.Empty;
            try
            {
                if (fsamsg.StartsWith("FSA", StringComparison.OrdinalIgnoreCase) || fsamsg.StartsWith("FSU", StringComparison.OrdinalIgnoreCase))
                {
                    string[] str = fsamsg.Split('$');
                    if (str.Length > 2)
                    {
                        flag = true;

                        #region : Line 1 :
                        if (str[0].StartsWith("FSA", StringComparison.OrdinalIgnoreCase) || str[0].StartsWith("FSU", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] msg = str[0].Split('/');
                            if (msg.Length > 1)
                                fsadata.fsaversion = msg[1];
                        }
                        #endregion Line 1

                        #region : Line 2 awb consigment details :
                        try
                        {
                            string[] msg = str[1].Split('/');
                            string[] currentLineConsignmentText = str[1].Split('/');
                            string airlinePrefix = string.Empty;
                            string AWBNo = string.Empty;
                            string origin = string.Empty;
                            string destination = string.Empty;

                            int indexofKORL = 0;

                            if (currentLineConsignmentText.Length > 0)
                            {
                                fsadata.airlineprefix = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(0, 3) : "";
                                fsadata.awbnum = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(4, 8) : "";
                                fsadata.origin = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(12, 3) : "";
                                fsadata.dest = currentLineConsignmentText[0] != "" ? currentLineConsignmentText[0].Substring(15, 3) : "";
                            }
                            if (currentLineConsignmentText.Length > 1)
                            {
                                if (currentLineConsignmentText[1].Contains("K"))
                                {
                                    indexofKORL = currentLineConsignmentText[1].LastIndexOf('K');
                                    fsadata.weightcode = "K";
                                }
                                else if (currentLineConsignmentText[1].Contains("L"))
                                {
                                    indexofKORL = currentLineConsignmentText[1].LastIndexOf('L');
                                    fsadata.weightcode = "L";
                                }

                                if (!currentLineConsignmentText[1].Contains("K") && (!currentLineConsignmentText[1].Contains("L")) && (currentLineConsignmentText[1].Substring(0, 1).Contains("T")))
                                {
                                    fsadata.consigntype = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(0, 1)) : "";

                                    fsadata.pcscnt = currentLineConsignmentText[1] != "" ? currentLineConsignmentText[1].Substring(1) : "0";
                                }

                                else if (((currentLineConsignmentText[1].Contains("K")) || (currentLineConsignmentText[1].Contains("L"))) && (currentLineConsignmentText[1].Substring(0, 1).Contains("T")) && (!currentLineConsignmentText[1].Substring(1).Contains("T")))
                                {
                                    fsadata.consigntype = currentLineConsignmentText[1] != ""
                                                     ? (currentLineConsignmentText[1].Substring(0, 1))
                                                     : ("");

                                    fsadata.pcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(1, indexofKORL - 1)) : "0";

                                    fsadata.weight = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexofKORL + 1)) : ("0.0");
                                }
                                else if (((currentLineConsignmentText[1].Contains("K")) || (currentLineConsignmentText[1].Contains("L"))) && (currentLineConsignmentText[1].Substring(0, 1).Contains("P") && currentLineConsignmentText[1].Contains("T")))
                                {
                                    int indexOfLastT = currentLineConsignmentText[1].LastIndexOf('T');
                                    fsadata.consigntype = currentLineConsignmentText[1] != ""
                                                     ? (currentLineConsignmentText[1].Substring(0, 1))
                                                     : ("");
                                    fsadata.totalpcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(1, indexofKORL - 1)) : "0";
                                    fsadata.pcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexOfLastT + 1)) : "0";

                                    fsadata.weight = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexofKORL + 1, indexOfLastT - indexofKORL - 1)) : ("0.0");
                                }
                                else if (((currentLineConsignmentText[1].Contains("K")) || (currentLineConsignmentText[1].Contains("L"))) && (currentLineConsignmentText[1].Substring(1).Contains("T")))
                                {
                                    int indexOfLastT = currentLineConsignmentText[1].LastIndexOf('T');
                                    fsadata.consigntype = currentLineConsignmentText[1] != ""
                                                     ? (currentLineConsignmentText[1].Substring(0, 1))
                                                     : ("");

                                    fsadata.pcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(1, indexofKORL - 1)) : "0";


                                    fsadata.weight = currentLineConsignmentText[1] != ""
                                                 ? (currentLineConsignmentText[1].Substring(indexofKORL + 1,
                                                                                              (indexOfLastT) - (indexofKORL + 1)))
                                                 : ("0.0");

                                    fsadata.totalpcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexOfLastT + 1)) : "0";
                                }
                                else if (((currentLineConsignmentText[1].Contains("K")) || (currentLineConsignmentText[1].Contains("L"))) && (currentLineConsignmentText[1].Substring(0, 1).Contains("P")))
                                {
                                    fsadata.consigntype = currentLineConsignmentText[1] != ""
                                                     ? (currentLineConsignmentText[1].Substring(0, 1))
                                                     : ("");

                                    fsadata.pcscnt = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(1, indexofKORL - 1)) : "0";

                                    fsadata.weight = currentLineConsignmentText[1] != "" ? (currentLineConsignmentText[1].Substring(indexofKORL + 1)) : ("0.0");
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
                        #endregion Line 2 awb consigment details

                        for (int i = 2; i < str.Length; i++)
                        {
                            #region : Decode Status :
                            if (str[i].Length > 0)
                            {
                                string[] msg = str[i].Split('/');
                                MessageData.CommonStruct recdata = new MessageData.CommonStruct("");
                                switch (msg[0])
                                {
                                    case "FOH":
                                        {
                                            #region FOH
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    if (msg.Length > 1)
                                                    {
                                                        recdata.fltday = msg[1].Substring(0, 2);
                                                        recdata.fltmonth = msg[1].Substring(2, 3);
                                                        recdata.flttime = msg[1].Substring(5);
                                                    }
                                                    if (msg.Length > 2)
                                                        recdata.airportcode = msg[2];
                                                    if (msg.Length > 3)
                                                    {
                                                        string[] arr = stringsplitter(msg[3]);
                                                        if (arr.Length > 0)
                                                            recdata.pcsindicator = arr[0];
                                                        if (arr.Length > 1)
                                                            recdata.numofpcs = arr[1];
                                                        if (arr.Length > 2)
                                                            recdata.weightcode = arr[2];
                                                        if (arr.Length > 3)
                                                            recdata.weight = arr[3];
                                                    }
                                                    if (msg.Length > 4)
                                                    {
                                                        if (msg.Length > 4)
                                                            recdata.name = msg[4];
                                                        if (msg.Length > 5)
                                                        {
                                                            string[] strarr = stringsplitter(msg[5]);
                                                            if (strarr.Length > 0)
                                                                recdata.volumecode = strarr[0];
                                                            if (strarr.Length > 1)
                                                                recdata.volumeamt = strarr[1];
                                                            if (strarr.Length > 2)
                                                            {
                                                                recdata.densityindicator = strarr[2].Substring(0, 2);
                                                                recdata.densitygroup = strarr[2].Substring(2);
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "RCS":
                                        {
                                            #region RCS
                                            try
                                            {
                                                string[] currentLineRCSText = str[i].ToString().Split('/');
                                                int indexOfKOrL = 0;

                                                if (currentLineRCSText.Length > 0)
                                                {
                                                    recdata.messageprefix = currentLineRCSText[0] != "" ? currentLineRCSText[0] : "";
                                                }
                                                if (currentLineRCSText.Length > 1)
                                                {
                                                    if (currentLineRCSText[1].Length < 5)
                                                    {
                                                        flag = false;
                                                        GenericFunction genericFunction = new GenericFunction();
                                                        genericFunction.UpdateErrorMessageToInbox(refNO, "Invalid message format");
                                                    }
                                                    else if (currentLineRCSText[1].Length == 9)
                                                    {
                                                        recdata.fltday = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(0, 2) : "";
                                                        recdata.fltmonth = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(2, 3) : "";
                                                        recdata.flttime = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(5, 2) + ":" + currentLineRCSText[1].Substring(7) + ":00" : "";
                                                    }
                                                    else if (currentLineRCSText[1].Length == 8)
                                                    {
                                                        recdata.fltday = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(0, 1) : "";
                                                        recdata.fltmonth = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(1, 3) : "";
                                                        recdata.flttime = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(4) : "";
                                                    }
                                                    else
                                                    {
                                                        recdata.fltday = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(0, 2) : "";
                                                        recdata.fltmonth = currentLineRCSText[1] != "" ? currentLineRCSText[1].Substring(2, 3) : "";
                                                        recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                    }
                                                }
                                                if (currentLineRCSText.Length >= 3)
                                                {
                                                    recdata.airportcode = currentLineRCSText[2] != "" ? currentLineRCSText[2] : "";
                                                }

                                                if (currentLineRCSText.Length >= 4)
                                                {
                                                    if (currentLineRCSText[3].Contains("K"))
                                                    {
                                                        indexOfKOrL = currentLineRCSText[3].LastIndexOf('K');
                                                    }
                                                    else if (currentLineRCSText[3].Contains("L"))
                                                    {
                                                        indexOfKOrL = currentLineRCSText[3].LastIndexOf('L');
                                                    }
                                                    if (((currentLineRCSText[3].Substring(1).Contains('K')) || (currentLineRCSText[3].Substring(1).Contains('L'))) && (currentLineRCSText[3].Substring(0, 1).Contains('T')) || (currentLineRCSText[3].Substring(0, 1).Contains('P')))
                                                    {
                                                        recdata.pcsindicator = currentLineRCSText[3] != ""
                                                                         ? (currentLineRCSText[3].Substring(0, 1))
                                                                         : ("");
                                                        ///Below condition added by prashant on 15-Mar-2017. To resolve JIRA# AS-443.
                                                        if (recdata.pcsindicator.Trim().ToUpper() == "P")
                                                        {
                                                            flag = false;
                                                            //GenericFunction genericFunction = new GenericFunction();
                                                            //genericFunction.UpdateErrorMessageToInbox(refNO, "Partial acceptance not allowed");
                                                            _genericFunction.UpdateErrorMessageToInbox(refNO, "Partial acceptance not allowed");

                                                        }

                                                        recdata.numofpcs = currentLineRCSText[3] != ""
                                                                             ? (currentLineRCSText[3].Substring(1, indexOfKOrL - 1))
                                                                             : "0";

                                                        recdata.weightcode = currentLineRCSText[3] != ""
                                                                         ? (currentLineRCSText[3].Substring(indexOfKOrL, 1))
                                                                         : ("");

                                                        recdata.weight = currentLineRCSText[3] != ""
                                                                     ? (currentLineRCSText[3].Substring(indexOfKOrL + 1))
                                                                     : ("0");
                                                    }
                                                    else if ((!currentLineRCSText[3].Substring(1).Contains('K')) && (!currentLineRCSText[3].Substring(1).Contains('L')) && (currentLineRCSText[3].Substring(0, 1).Contains('T')) || (currentLineRCSText[3].Substring(0, 1).Contains('P')))
                                                    {
                                                        recdata.pcsindicator = currentLineRCSText[3] != ""
                                                                         ? (currentLineRCSText[3].Substring(0, 1))
                                                                         : ("");

                                                        recdata.numofpcs = currentLineRCSText[3] != ""
                                                                              ? (currentLineRCSText[3].Substring(1))
                                                                              : "0";
                                                    }
                                                }
                                                if (currentLineRCSText.Length >= 5)
                                                {
                                                    recdata.name = currentLineRCSText[4] != "" ? currentLineRCSText[4] : "";
                                                }
                                                if (str.Length > i + 1)
                                                {
                                                    int k = 0;
                                                    char lastchr = 'A';
                                                    char[] arr = str[i + 1].ToCharArray();
                                                    string[] strarr = new string[arr.Length];
                                                    for (int j = 0; j < arr.Length; j++)
                                                    {
                                                        if ((char.IsNumber(arr[j])) || (arr[j].Equals('.')))
                                                        {
                                                            ///number                            
                                                            if (lastchr == 'N')
                                                                k--;
                                                            strarr[k] = strarr[k] + arr[j].ToString();
                                                            lastchr = 'N';
                                                        }
                                                        if (char.IsLetter(arr[j]))
                                                        {
                                                            ///letter
                                                            if (lastchr == 'L')
                                                                k--;
                                                            strarr[k] = strarr[k] + arr[j].ToString();
                                                            lastchr = 'L';
                                                        }
                                                        k++;
                                                    }
                                                    for (k = 0; k < strarr.Length; k++)
                                                    {
                                                        if (strarr[k] != null)
                                                        {
                                                            decimal Volume = 0;
                                                            if (strarr[k] == "MC")
                                                                recdata.volumecode = strarr[k];
                                                            else if (decimal.TryParse(strarr[k], out Volume) && strarr[0] != "ULD")
                                                                recdata.volumeamt = Convert.ToString(Volume);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                            fsanodes[fsanodes.Length - 1] = recdata;
                                            #endregion
                                        }
                                        break;
                                    case "RCT":
                                        {
                                            #region RCT
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    string[] currentLineRCTText = str[i].ToString().Split('/');
                                                    string rctTag = string.Empty;
                                                    int indexOfKOrL = 0;
                                                    lastrec = "RCT";

                                                    if (currentLineRCTText.Length >= 1)
                                                        recdata.messageprefix = currentLineRCTText[0] != "" ? currentLineRCTText[0] : "";

                                                    if (currentLineRCTText.Length >= 2)
                                                        recdata.carriercode = currentLineRCTText[1] != "" ? currentLineRCTText[1] : "";

                                                    if (currentLineRCTText.Length >= 3)
                                                        if (currentLineRCTText[2].Length == 9)
                                                        {
                                                            recdata.fltday = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(0, 2) : "";
                                                            recdata.fltmonth = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(2, 3) : "";
                                                            recdata.flttime = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(5, 2) + ":" + currentLineRCTText[2].Substring(7) + ":00" : "";
                                                        }
                                                        else if (currentLineRCTText[2].Length == 8)
                                                        {
                                                            recdata.fltday = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(0, 1) : "";
                                                            recdata.fltmonth = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(1, 3) : "";
                                                            recdata.flttime = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(5, 2) + ":" + currentLineRCTText[2].Substring(7) + ":00" : "";
                                                        }
                                                        else
                                                        {
                                                            recdata.fltday = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(0, 2) : "";
                                                            recdata.fltmonth = currentLineRCTText[2] != "" ? currentLineRCTText[2].Substring(2, 3) : "";
                                                            recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                        }

                                                    if (currentLineRCTText.Length >= 4)
                                                        recdata.fltorg = currentLineRCTText[3] != "" ? currentLineRCTText[3] : "";

                                                    if (currentLineRCTText.Length >= 5)
                                                    {
                                                        if (currentLineRCTText[4].Contains("K"))
                                                            indexOfKOrL = currentLineRCTText[4].LastIndexOf('K');
                                                        else if (currentLineRCTText[4].Contains("L"))
                                                            indexOfKOrL = currentLineRCTText[4].LastIndexOf('L');

                                                        if (!currentLineRCTText[4].Contains("K") && (!currentLineRCTText[4].Contains("L")) && (currentLineRCTText[4].Contains("T") || currentLineRCTText[4].Contains("P")))
                                                        {
                                                            recdata.pcsindicator = currentLineRCTText[4] != ""
                                                                             ? (currentLineRCTText[4].Substring(0, 1))
                                                                             : ("");

                                                            recdata.numofpcs = currentLineRCTText[4].Substring(1);

                                                        }
                                                        else if (((currentLineRCTText[4].Contains("K")) || (currentLineRCTText[4].Contains("L"))) && (currentLineRCTText[4].Contains("T") || currentLineRCTText[4].Contains("P")))
                                                        {
                                                            recdata.pcsindicator = currentLineRCTText[4] != ""
                                                                             ? (currentLineRCTText[4].Substring(0, 1))
                                                                             : ("");

                                                            recdata.numofpcs = currentLineRCTText[4] != ""
                                                                                 ? (currentLineRCTText[4].Substring(1, indexOfKOrL - 1))
                                                                                 : "0";

                                                            recdata.weightcode = currentLineRCTText[4] != ""
                                                                             ? (currentLineRCTText[4].Substring(indexOfKOrL, 1))
                                                                             : "";

                                                            recdata.weight = currentLineRCTText[4] != ""
                                                                         ? (currentLineRCTText[4].Substring(indexOfKOrL + 1))
                                                                         : "0";
                                                        }
                                                        ///Below condition added by prashant on 15-Mar-2017. To resolve JIRA# AS-443.
                                                        if (recdata.pcsindicator.Trim().ToUpper() == "P")
                                                        {
                                                            flag = false;
                                                            //GenericFunction genericFunction = new GenericFunction();
                                                            //genericFunction.UpdateErrorMessageToInbox(refNO, "Partial acceptance not allowed");
                                                            _genericFunction.UpdateErrorMessageToInbox(refNO, "Partial acceptance not allowed");
                                                        }
                                                    }
                                                    if (currentLineRCTText.Length > 5)
                                                        recdata.seccarriercode = currentLineRCTText[5].ToString();
                                                    if (currentLineRCTText.Length > 6)
                                                        recdata.name = currentLineRCTText[6].ToString();
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "DIS":
                                        {
                                            #region DIS
                                            try
                                            {
                                                string[] currentLineDISText = str[i].ToString().Split('/');
                                                string disTag = string.Empty;
                                                string discrepancyCode = string.Empty;
                                                string time = string.Empty;
                                                string airportCode = string.Empty;
                                                int indexOfKOrL = 0;

                                                if (currentLineDISText.Length >= 1)
                                                {
                                                    recdata.messageprefix = currentLineDISText[0].Substring(0, 3) != "" ? currentLineDISText[0].Substring(0, 3) : "";
                                                }
                                                if (currentLineDISText.Length >= 2)
                                                {
                                                    recdata.flightnum = currentLineDISText[1] != "" ? currentLineDISText[1] : "";
                                                }
                                                if (currentLineDISText.Length >= 3)
                                                {
                                                    if (currentLineDISText[2].Length == 9)
                                                    {
                                                        recdata.fltday = currentLineDISText[2].Substring(0, 2) != "" ? currentLineDISText[2].Substring(0, 2) : "";
                                                        recdata.fltmonth = currentLineDISText[2].Substring(2, 3) != "" ? currentLineDISText[2].Substring(2, 3) : "";
                                                        //recdata.flttime = currentLineDISText[2].Substring(5) != "" ? currentLineDISText[2].Substring(5) : "";
                                                        recdata.flttime = currentLineDISText[2] != "" ? currentLineDISText[2].Substring(5, 2) + ":" + currentLineDISText[2].Substring(7) + ":00" : "";
                                                    }
                                                    else if (currentLineDISText[2].Length == 8)
                                                    {
                                                        recdata.fltday = currentLineDISText[2].Substring(0, 1) != "" ? currentLineDISText[2].Substring(0, 1) : "";
                                                        recdata.fltmonth = currentLineDISText[2].Substring(1, 3) != "" ? currentLineDISText[2].Substring(1, 3) : "";
                                                        recdata.flttime = currentLineDISText[2].Substring(4) != "" ? currentLineDISText[2].Substring(4) : "";
                                                    }
                                                    else
                                                    {
                                                        recdata.fltday = currentLineDISText[2] != "" ? currentLineDISText[2].Substring(0, 2) : "";
                                                        recdata.fltmonth = currentLineDISText[2] != "" ? currentLineDISText[2].Substring(2, 3) : "";
                                                        recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                    }
                                                }
                                                if (currentLineDISText.Length >= 4)
                                                {
                                                    recdata.airportcode = currentLineDISText[3] != "" ? currentLineDISText[3] : "";
                                                }
                                                if (currentLineDISText.Length >= 5)
                                                {
                                                    recdata.infocode = currentLineDISText[4] != "" ? currentLineDISText[4] : "";
                                                }
                                                if (currentLineDISText.Length >= 6)
                                                {
                                                    if (currentLineDISText[5].Contains("K"))
                                                    {
                                                        indexOfKOrL = currentLineDISText[5].LastIndexOf('K');
                                                    }
                                                    else if (currentLineDISText[5].Contains("L"))
                                                    {
                                                        indexOfKOrL = currentLineDISText[5].LastIndexOf('L');
                                                    }
                                                    if (!currentLineDISText[5].Contains("K") && (!currentLineDISText[5].Contains("L")) && (currentLineDISText[5].Contains("T")))
                                                    {
                                                        recdata.pcsindicator = currentLineDISText[5].Substring(0, 1) != ""
                                                                         ? (currentLineDISText[5].Substring(0, 1))
                                                                         : ("");
                                                        recdata.numofpcs = currentLineDISText[5].Substring(1) != ""
                                                                              ? (currentLineDISText[5].Substring(1))
                                                                              : "0";
                                                    }
                                                    else if (((currentLineDISText[5].Contains("K")) || (currentLineDISText[5].Contains("L"))) && (currentLineDISText[5].Contains("T")))
                                                    {
                                                        recdata.pcsindicator = currentLineDISText[5].Substring(0, 1) != ""
                                                                         ? (currentLineDISText[5].Substring(0, 1))
                                                                         : ("");

                                                        recdata.numofpcs = currentLineDISText[5].Substring(1) != ""
                                                                              ? (currentLineDISText[5].Substring(1, indexOfKOrL - 1))
                                                                              : "0";

                                                        recdata.weightcode = currentLineDISText[5].Substring(indexOfKOrL + 1, 1) != ""
                                                                            ? (currentLineDISText[5].Substring(indexOfKOrL, 1))
                                                                            : ("");

                                                        recdata.weight = currentLineDISText[5].Substring(indexOfKOrL + 1) != ""
                                                                       ? (currentLineDISText[5].Substring(indexOfKOrL + 1))
                                                                       : ("0");
                                                    }
                                                    else if ((!currentLineDISText[5].Substring(1).Contains('K')) && (!currentLineDISText[5].Substring(1).Contains('L')) && (currentLineDISText[5].Substring(0, 1).Contains('T')) || (currentLineDISText[5].Substring(0, 1).Contains('P')))
                                                    {
                                                        recdata.pcsindicator = currentLineDISText[5] != ""
                                                                         ? (currentLineDISText[5].Substring(0, 1))
                                                                         : ("");

                                                        if (indexOfKOrL > 0)
                                                        {
                                                            recdata.numofpcs = currentLineDISText[5] != ""
                                                                                 ? (currentLineDISText[5].Substring(1, indexOfKOrL - 1))
                                                                                 : "0";

                                                            //added by prashant on 11-OCT-17
                                                            if (currentLineDISText[5].Length > indexOfKOrL)
                                                            {
                                                                recdata.weight = currentLineDISText[5].Substring(indexOfKOrL + 1) != ""
                                                                      ? (currentLineDISText[5].Substring(indexOfKOrL + 1))
                                                                      : ("0");

                                                                recdata.weightcode = currentLineDISText[5].Substring(indexOfKOrL + 1, 1) != ""
                                                                            ? (currentLineDISText[5].Substring(indexOfKOrL, 1))
                                                                            : ("");


                                                            }
                                                        }
                                                        else
                                                        {
                                                            recdata.numofpcs = currentLineDISText[5] != ""
                                                                                ? (currentLineDISText[5].Substring(1))
                                                                                : "0";
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                            fsanodes[fsanodes.Length - 1] = recdata;
                                            #endregion
                                        }
                                        break;
                                    case "NFD":
                                    case "AWD":
                                    case "CCD":
                                    case "DDL":
                                    case "TGC":
                                        {
                                            #region NFD/CCD
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];

                                                    if (msg.Length >= 2)
                                                    {
                                                        if (msg[1].Length == 9)
                                                        {
                                                            recdata.fltday = msg[1] != "" ? msg[1].Substring(0, 2) : "";
                                                            recdata.fltmonth = msg[1] != "" ? msg[1].Substring(2, 3) : "";
                                                            recdata.flttime = msg[1] != "" ? msg[1].Substring(5, 2) + ":" + msg[1].Substring(7) + ":00" : "";
                                                        }
                                                        else if (msg[1].Length == 8)
                                                        {
                                                            recdata.fltday = msg[1] != "" ? msg[1].Substring(0, 1) : "";
                                                            recdata.fltmonth = msg[1] != "" ? msg[1].Substring(1, 3) : "";
                                                            recdata.flttime = msg[1] != "" ? msg[1].Substring(5, 2) + ":" + msg[1].Substring(7) + ":00" : "";
                                                        }
                                                        else
                                                        {
                                                            recdata.fltday = DateTime.UtcNow.ToShortDateString().Substring(3, 2);
                                                            recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                                            recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                        }
                                                    }

                                                    recdata.timeindicator = msg[1].Substring(5).Length == 4 ? msg[1].Substring(5, 2) + ":" + msg[1].Substring(7) : string.Empty;
                                                    recdata.airportcode = msg[2];
                                                    recdata.fltdest = msg[2];
                                                    string[] arr = stringsplitter(msg[3]);
                                                    if (arr.Length > 1)
                                                    {
                                                        try
                                                        {
                                                            recdata.pcsindicator = arr[0];
                                                            recdata.numofpcs = arr[1];
                                                            recdata.weightcode = arr[2];
                                                            recdata.weight = arr[3].Length > 0 ? arr[3] : "";
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            // clsLog.WriteLogAzure(ex);
                                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }
                                                    }
                                                    if (msg.Length > 4)
                                                    {
                                                        recdata.name = msg[4].Length > 0 ? msg[4] : "";
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "AWR":
                                    case "PRE":
                                        {
                                            #region MAN/AWR
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    //0
                                                    recdata.messageprefix = msg[0];
                                                    //1
                                                    recdata.carriercode = msg[1].Substring(0, 2);
                                                    recdata.flightnum = msg[1].Substring(2);
                                                    //2
                                                    string[] split = msg[2].Split('-');
                                                    recdata.fltday = split[0].Substring(0, 2);
                                                    recdata.fltmonth = split[0].Substring(2, 3);
                                                    recdata.flttime = split[0].Substring(5);
                                                    if (split.Length > 1)
                                                    {
                                                        recdata.daychangeindicator = split[1] + ",";
                                                    }
                                                    //3
                                                    if (msg[3].Length > 0)
                                                    {
                                                        try
                                                        {
                                                            if (msg[3].Length > 3)
                                                            {
                                                                recdata.fltorg = msg[3].Substring(0, 3);
                                                                recdata.fltdest = msg[3].Substring(3);
                                                            }
                                                            else
                                                            {
                                                                recdata.airportcode = msg[3];
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            // clsLog.WriteLogAzure(ex);
                                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }
                                                    }
                                                    //4 PCS Indicator
                                                    if (msg[4].Length > 0)
                                                    {
                                                        try
                                                        {
                                                            string[] arr = stringsplitter(msg[4]);
                                                            if (arr != null && arr.Length > 0)
                                                            {
                                                                recdata.pcsindicator = arr[0];
                                                                recdata.numofpcs = arr[1];
                                                                recdata.weightcode = arr[2];
                                                                recdata.weight = arr[3];
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            // clsLog.WriteLogAzure(ex);
                                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                        }
                                                    }
                                                    try
                                                    {
                                                        if (msg.Length > 5)
                                                        {
                                                            try
                                                            {
                                                                if (msg[5].Contains('-'))
                                                                {
                                                                    string[] strarr = msg[5].Split('-');
                                                                    recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                                    recdata.depttime = strarr[0].Substring(1);
                                                                    if (strarr.Length > 1)
                                                                    {
                                                                        recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    recdata.timeindicator = recdata.timeindicator + msg[5].Substring(0, 1) + ",";
                                                                    recdata.depttime = msg[5].Substring(1);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                // clsLog.WriteLogAzure(ex);
                                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                            }
                                                        }
                                                        if (msg.Length > 6)
                                                        {
                                                            try
                                                            {
                                                                if (msg[6].Contains('-'))
                                                                {
                                                                    string[] strarr = msg[6].Split('-');
                                                                    recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                                    recdata.arrivaltime = strarr[0].Substring(1);
                                                                    if (strarr.Length > 1)
                                                                    {
                                                                        recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    recdata.timeindicator = recdata.timeindicator + msg[6].Substring(0, 1) + ",";
                                                                    recdata.arrivaltime = msg[6].Substring(1);
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
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
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "MAN":
                                        #region FSU/MAN
                                        string[] currentMANLineText = str[i].ToString().Split('/');
                                        string manTag = string.Empty;
                                        string manCarrierCode = string.Empty;
                                        string flightNumber = string.Empty;
                                        string day = string.Empty;
                                        string month = string.Empty;
                                        string typeOfTimeIndicator1 = string.Empty;
                                        string timeOfDeparture = string.Empty;
                                        string dayChangeIndicator1 = string.Empty;
                                        string typeOfTimeIndicator2 = string.Empty;
                                        string timeOfArrival = string.Empty;
                                        string dayChangeIndicator2 = string.Empty;

                                        try
                                        {
                                            if (currentMANLineText.Length >= 1)
                                            {
                                                manTag = currentMANLineText[0] != "" ? currentMANLineText[0] : "";
                                                recdata.messageprefix = currentMANLineText[0] != "" ? currentMANLineText[0] : "";
                                            }
                                            if (currentMANLineText.Length >= 2)
                                            {
                                                recdata.carriercode = currentMANLineText[1] != "" ? currentMANLineText[1].Substring(0, 2) : "";
                                                recdata.flightnum = currentMANLineText[1] != "" ? currentMANLineText[1].Substring(2) : "";
                                            }
                                            if (currentMANLineText.Length >= 3)
                                            {
                                                string currentLineTextSplit = currentMANLineText[2];
                                                if (currentLineTextSplit.Length == 5)
                                                {
                                                    recdata.fltday = currentLineTextSplit != "" ? currentLineTextSplit.Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineTextSplit != "" ? currentLineTextSplit.Substring(2) : "";
                                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                }
                                                else if (currentLineTextSplit.Length == 9)
                                                {
                                                    recdata.fltday = currentLineTextSplit != "" ? currentLineTextSplit.Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineTextSplit != "" ? currentLineTextSplit.Substring(2, 3) : "";
                                                    recdata.flttime = currentLineTextSplit.Substring(5, 2) + ":" + currentLineTextSplit.Substring(7, 2) + ":00";
                                                }
                                                else if (currentMANLineText[1].Length == 4)
                                                {
                                                    day = currentLineTextSplit != "" ? currentLineTextSplit.Substring(0, 1) : "";
                                                    month = currentLineTextSplit != "" ? currentLineTextSplit.Substring(1) : "";
                                                }
                                                else
                                                {
                                                    recdata.fltday = DateTime.UtcNow.ToShortDateString().Substring(3, 2);
                                                    recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                }
                                                //if (currentMANLineText.Length == 6)
                                                //{
                                                //    recdata.flttime = currentMANLineText[5] != string.Empty && currentMANLineText[5].Length == 5 ? currentMANLineText[5].Substring(1, 2) + ":" + currentMANLineText[5].Substring(3, 2) + ":00" : "";

                                                //}
                                            }
                                            if (currentMANLineText.Length >= 4)
                                            {
                                                recdata.fltorg = currentMANLineText[3] != "" ? currentMANLineText[3].Substring(0, 3) : "";
                                                recdata.fltdest = currentMANLineText[3] != "" ? currentMANLineText[3].Substring(3) : "";
                                            }
                                            if (currentMANLineText.Length >= 5)
                                            {
                                                int indexOfKOrL = 0;
                                                if (currentMANLineText[4].Contains("K"))
                                                {
                                                    indexOfKOrL = currentMANLineText[4].LastIndexOf('K');
                                                }
                                                else if (currentMANLineText[4].Contains("L"))
                                                {
                                                    indexOfKOrL = currentMANLineText[4].LastIndexOf('L');
                                                }

                                                if (!currentMANLineText[4].Contains("K") && (!currentMANLineText[4].Contains("L")) && (currentMANLineText[4].Contains("T")))
                                                {
                                                    recdata.pcsindicator = currentMANLineText[4].Substring(0, 1);

                                                    recdata.numofpcs = currentMANLineText[4] != "" ? (currentMANLineText[4].Substring(1)) : "0";
                                                }
                                                else if (((currentMANLineText[4].Contains("K")) || (currentMANLineText[4].Contains("L"))) && (currentMANLineText[4].Contains("T")) || (currentMANLineText[4].Contains("P")))
                                                {
                                                    recdata.pcsindicator = currentMANLineText[4] != ""
                                                                     ? Convert.ToString(currentMANLineText[4].Substring(0, 1))
                                                                     : Convert.ToString("");

                                                    if (indexOfKOrL == 0)
                                                    {
                                                        recdata.numofpcs = currentMANLineText[4] != ""
                                                                        ? Convert.ToString(currentMANLineText[4].Substring(1, currentMANLineText[4].Length - 1))
                                                                        : "0";
                                                    }
                                                    else
                                                    {
                                                        recdata.numofpcs = currentMANLineText[4] != ""
                                                                        ? Convert.ToString(currentMANLineText[4].Substring(1, indexOfKOrL - 1))
                                                                        : "0";
                                                    }
                                                    recdata.weightcode = currentMANLineText[4] != ""
                                                                     ? Convert.ToString(currentMANLineText[4].Substring(indexOfKOrL, 1))
                                                                     : Convert.ToString("");

                                                    if (((currentMANLineText[4].Contains("K")) || (currentMANLineText[4].Contains("L"))))
                                                    {
                                                        recdata.weight = currentMANLineText[4] != ""
                                                                 ? Convert.ToString(currentMANLineText[4].Substring(indexOfKOrL + 1))
                                                                 : Convert.ToString("0");
                                                    }
                                                    else
                                                    {
                                                        recdata.weight = Convert.ToString("0");
                                                    }

                                                }
                                                else if ((!currentMANLineText[4].Substring(1).Contains('K')) && (!currentMANLineText[4].Substring(1).Contains('L')) && (currentMANLineText[4].Substring(0, 1).Contains('T')) || (currentMANLineText[4].Substring(0, 1).Contains('P')))
                                                {
                                                    recdata.pcsindicator = currentMANLineText[4] != ""
                                                                     ? (currentMANLineText[4].Substring(0, 1))
                                                                     : ("");

                                                    recdata.numofpcs = currentMANLineText[4] != ""
                                                                          ? (currentMANLineText[4].Substring(1))
                                                                          : "0";
                                                }
                                            }
                                            if (currentMANLineText.Length >= 6)
                                            {
                                                string[] currentLineTextSplit = currentMANLineText[5].Split('-');
                                                if (currentLineTextSplit.Length >= 1)
                                                {
                                                    typeOfTimeIndicator1 = currentLineTextSplit[0] != ""
                                                                               ? currentLineTextSplit[0].Substring(0, 1)
                                                                               : "";
                                                    timeOfDeparture = currentLineTextSplit[0] != ""
                                                                          ? currentLineTextSplit[0].Substring(1)
                                                                          : "";
                                                }
                                                if (currentLineTextSplit.Length >= 2)
                                                {
                                                    dayChangeIndicator1 = currentLineTextSplit[1] != ""
                                                                              ? currentLineTextSplit[1]
                                                                              : "";
                                                }
                                            }
                                            if (currentMANLineText.Length >= 7)
                                            {
                                                string[] currentLineTextSplit = currentMANLineText[6].Split('-');
                                                if (currentLineTextSplit.Length >= 1)
                                                {
                                                    typeOfTimeIndicator2 = currentLineTextSplit[0] != ""
                                                                               ? currentLineTextSplit[0].Substring(0, 1)
                                                                               : "";
                                                    timeOfArrival = currentLineTextSplit[0] != ""
                                                                        ? currentLineTextSplit[0].Substring(1)
                                                                        : "";
                                                }
                                                if (currentLineTextSplit.Length >= 2)
                                                {
                                                    dayChangeIndicator2 = currentLineTextSplit[1] != ""
                                                                              ? currentLineTextSplit[1]
                                                                              : "";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                        fsanodes[fsanodes.Length - 1] = recdata;
                                        #endregion
                                        break;
                                    case "DEP":
                                        #region FSU/DEP
                                        string[] currentLineText = str[i].ToString().Split('/');
                                        string depTag = string.Empty;
                                        string carrierCode = string.Empty;
                                        char piecesCode = ' ';
                                        int numberOfPieces = 0;
                                        char weightCode = ' ';
                                        decimal weight = 0;
                                        try
                                        {
                                            if (currentLineText.Length >= 1)
                                            {
                                                recdata.messageprefix = currentLineText[0] != "" ? currentLineText[0] : "";
                                            }
                                            if (currentLineText.Length >= 2)
                                            {
                                                recdata.carriercode = currentLineText[1] != "" ? currentLineText[1].Substring(0, 2) : "";
                                                recdata.flightnum = currentLineText[1] != "" ? currentLineText[1].Substring(2) : "";
                                            }
                                            if (currentLineText.Length >= 3)
                                            {
                                                string currentLineTextSplit = currentLineText[2];
                                                if (currentLineTextSplit.Length == 9)
                                                {
                                                    recdata.fltday = currentLineTextSplit != "" ? currentLineTextSplit.Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineTextSplit != "" ? currentLineTextSplit.Substring(2, 3) : "";
                                                    // recdata.flttime = currentLineTextSplit != "" ? currentLineTextSplit.Substring(5) : "";
                                                    recdata.flttime = currentLineTextSplit != "" ? currentLineTextSplit.Substring(5, 2) + ":" + currentLineTextSplit.Substring(7) + ":00" : "";
                                                }
                                                else if (currentLineText[1].Length == 8)
                                                {
                                                    recdata.fltday = currentLineTextSplit != "" ? currentLineTextSplit.Substring(0, 1) : "";
                                                    recdata.fltmonth = currentLineTextSplit != "" ? currentLineTextSplit.Substring(1, 3) : "";
                                                    recdata.flttime = currentLineTextSplit != "" ? currentLineTextSplit.Substring(4) : "";
                                                }
                                                else if (currentLineText.Length > 5)
                                                {
                                                    recdata.fltday = currentLineText[2] != "" ? currentLineText[2].Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineText[2] != "" ? currentLineText[2].Substring(2, 3) : "";
                                                    recdata.flttime = currentLineText[5] != "" ? currentLineText[5].Substring(1, 2) + ":" + currentLineText[5].Substring(3) + ":00" : "";
                                                }
                                                else
                                                {
                                                    recdata.fltday = currentLineText[2] != "" ? currentLineText[2].Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineText[2] != "" ? currentLineText[2].Substring(2, 3) : "";
                                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                }
                                            }
                                            if (currentLineText.Length >= 4)
                                            {
                                                recdata.fltorg = currentLineText[3] != "" ? currentLineText[3].Substring(0, 3) : "";
                                                if (currentLineText[3].Length > 5)
                                                {
                                                    recdata.fltdest = currentLineText[3] != "" ? currentLineText[3].Substring(3, 3) : "";
                                                }
                                            }
                                            if (currentLineText.Length >= 5)
                                            {
                                                int indexOfKOrL = 0;
                                                if (currentLineText[4].Contains("K"))
                                                {
                                                    indexOfKOrL = currentLineText[4].LastIndexOf('K');
                                                }
                                                else if (currentLineText[4].Contains("L"))
                                                {
                                                    indexOfKOrL = currentLineText[4].LastIndexOf('L');
                                                }
                                                if (!currentLineText[4].Contains("K") && (!currentLineText[4].Contains("L")) && (currentLineText[4].Contains("T")))
                                                {
                                                    recdata.pcsindicator = currentLineText[4].Substring(0, 1);
                                                    recdata.numofpcs = currentLineText[4].Substring(1);
                                                }
                                                else if (((currentLineText[4].Contains("K")) || (currentLineText[4].Contains("L"))) && (currentLineText[4].Contains("T") || currentLineText[4].Contains("P")))
                                                {
                                                    recdata.pcsindicator = currentLineText[4].Substring(0, 1);
                                                    recdata.numofpcs = currentLineText[4].Substring(1, indexOfKOrL - 1);
                                                    recdata.weightcode = (currentLineText[4].Substring(indexOfKOrL, 1));
                                                    recdata.weight = (currentLineText[4].Substring(indexOfKOrL + 1));
                                                }
                                                else if ((!currentLineText[4].Substring(1).Contains('K')) && (!currentLineText[4].Substring(1).Contains('L')) && (currentLineText[4].Substring(0, 1).Contains('T') || currentLineText[4].Substring(0, 1).Contains('P')))
                                                {
                                                    recdata.pcsindicator = currentLineText[4] != ""
                                                                     ? (currentLineText[4].Substring(0, 1))
                                                                     : ("");

                                                    recdata.numofpcs = currentLineText[4] != ""
                                                                          ? (currentLineText[4].Substring(1))
                                                                          : "0";
                                                }
                                            }
                                            if (currentLineText.Length >= 6)
                                            {
                                                string[] currentLineTextSplit = currentLineText[5].Split('-');
                                                if (currentLineTextSplit.Length >= 1)
                                                {
                                                    typeOfTimeIndicator1 = currentLineTextSplit[0] != ""
                                                                               ? currentLineTextSplit[0].Substring(0, 1)
                                                                               : "";
                                                    timeOfDeparture = currentLineTextSplit[0] != ""
                                                                          ? currentLineTextSplit[0].Substring(1)
                                                                          : "";
                                                }
                                                if (currentLineTextSplit.Length >= 2)
                                                {
                                                    dayChangeIndicator1 = currentLineTextSplit[1] != ""
                                                                              ? currentLineTextSplit[1]
                                                                              : "";
                                                }
                                            }
                                            if (currentLineText.Length >= 7)
                                            {
                                                string[] currentLineTextSplit = currentLineText[6].Split('-');
                                                if (currentLineTextSplit.Length >= 1)
                                                {
                                                    typeOfTimeIndicator2 = currentLineTextSplit[0] != ""
                                                                               ? currentLineTextSplit[0].Substring(0, 1)
                                                                               : "";
                                                    timeOfArrival = currentLineTextSplit[0] != ""
                                                                        ? currentLineTextSplit[0].Substring(1)
                                                                        : "";
                                                }
                                                if (currentLineTextSplit.Length >= 2)
                                                {
                                                    dayChangeIndicator2 = currentLineTextSplit[1] != ""
                                                                              ? currentLineTextSplit[1]
                                                                              : "";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                        fsanodes[fsanodes.Length - 1] = recdata;
                                        #endregion
                                        break;
                                    case "ARR":
                                    case "RCF":
                                        #region FUS/RCF
                                        try
                                        {
                                            string[] currentLineRCFText = str[i].ToString().Split('/');
                                            string arrTag = string.Empty;
                                            string time = string.Empty;

                                            bool fltpresent = false;
                                            bool fltanddatepresent = false;

                                            string airPortCityCode = string.Empty;

                                            string dayChangeIndicator3 = string.Empty;
                                            int indexOfKOrL = 0;
                                            if (currentLineRCFText.Length >= 1)
                                            {
                                                recdata.messageprefix = currentLineRCFText[0] != "" ? currentLineRCFText[0] : "";
                                            }

                                            if (currentLineRCFText.Length >= 2)
                                            {
                                                if (currentLineRCFText[1].Length <= 8)
                                                {
                                                    fltpresent = true;
                                                    recdata.carriercode = currentLineRCFText[1] != "" ? currentLineRCFText[1].Substring(0, 2) : "";
                                                    recdata.flightnum = currentLineRCFText[1] != "" ? currentLineRCFText[1].Substring(2) : "";
                                                }
                                                else
                                                {
                                                    string[] currentLineTextSplit = currentLineRCFText[1].Split('-');
                                                    if (currentLineTextSplit.Length >= 1)
                                                    {
                                                        if (currentLineTextSplit[0].Length == 9)
                                                        {
                                                            recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 2) : "";
                                                            recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                                            recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(5, 2) + ":" + currentLineTextSplit[0].Substring(7) + ":00" : "";
                                                        }
                                                        else if (currentLineRCFText[0].Length == 8)
                                                        {
                                                            recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 1) : "";
                                                            recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(1, 3) : "";
                                                            recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(4) : "";
                                                        }
                                                    }
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        recdata.daychangeindicator = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }
                                            }
                                            if (fltpresent)
                                            {
                                                if (currentLineRCFText.Length >= 3)
                                                {
                                                    string[] currentLineTextSplit = currentLineRCFText[2].Split('-');
                                                    if (currentLineTextSplit.Length >= 1)
                                                    {
                                                        if (currentLineTextSplit[0].Length == 9)
                                                        {
                                                            recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 2) : "";
                                                            recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                                            //recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(5) : ""; 
                                                            recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(5, 2) + ":" + currentLineTextSplit[0].Substring(7) + ":00" : "";
                                                            fltanddatepresent = true;
                                                        }
                                                        else if (currentLineRCFText[0].Length == 8)
                                                        {
                                                            recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 1) : "";
                                                            recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(1, 3) : "";
                                                            recdata.flttime = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(4) : "";
                                                            fltanddatepresent = true;
                                                        }
                                                        else
                                                        {
                                                            recdata.fltday = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(0, 2) : "";
                                                            recdata.fltmonth = currentLineTextSplit[0] != "" ? currentLineTextSplit[0].Substring(2, 3) : "";
                                                            recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                            fltanddatepresent = true;
                                                        }
                                                    }
                                                    if (currentLineTextSplit.Length >= 2)
                                                    {
                                                        recdata.daychangeindicator = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                    }
                                                }
                                                if (fltpresent == true && fltanddatepresent == false)
                                                {
                                                    recdata.fltdest = currentLineRCFText[2] != "" ? currentLineRCFText[2] : "";
                                                }
                                                if ((fltanddatepresent == true && fltpresent == true))
                                                {
                                                    recdata.fltdest = currentLineRCFText[3] != "" ? currentLineRCFText[3] : "";
                                                }
                                                if (currentLineRCFText.Length >= 5)
                                                {
                                                    if (currentLineRCFText[4].Contains("K"))
                                                    {
                                                        indexOfKOrL = currentLineRCFText[4].LastIndexOf('K');
                                                    }
                                                    else if (currentLineRCFText[4].Contains("L"))
                                                    {
                                                        indexOfKOrL = currentLineRCFText[4].LastIndexOf('L');
                                                    }
                                                    if (!currentLineRCFText[4].Contains("K") && (!currentLineRCFText[4].Contains("L")) && (currentLineRCFText[4].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                    {
                                                        recdata.pcsindicator = currentLineRCFText[4] != "" ? currentLineRCFText[4].Substring(0, 1) : "";


                                                        recdata.numofpcs = currentLineRCFText[4] != ""
                                                                             ? (currentLineRCFText[4].Substring(1))
                                                                             : "0";
                                                    }
                                                    else if (((currentLineRCFText[4].Contains("K")) || (currentLineRCFText[4].Contains("L"))) || (currentLineRCFText[4].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                    {
                                                        recdata.pcsindicator = currentLineRCFText[4] != ""
                                                                         ? (currentLineRCFText[4].Substring(0, 1))
                                                                         : "";

                                                        recdata.numofpcs = currentLineRCFText[4] != ""
                                                                             ? (currentLineRCFText[4].Substring(1, indexOfKOrL - 1))
                                                                             : "0";

                                                        recdata.weightcode = currentLineRCFText[4] != ""
                                                                         ? (currentLineRCFText[4].Substring(indexOfKOrL, 1))
                                                                         : "";

                                                        recdata.weight = currentLineRCFText[4] != ""
                                                                     ? (currentLineRCFText[4].Substring(indexOfKOrL + 1))
                                                                     : "0";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (currentLineRCFText.Length >= 3)
                                                {
                                                    recdata.fltdest = currentLineRCFText[2] != "" ? currentLineRCFText[2] : "";
                                                }

                                                if (currentLineRCFText.Length >= 4)
                                                {
                                                    if (currentLineRCFText[3].Contains("K"))
                                                    {
                                                        indexOfKOrL = currentLineRCFText[3].LastIndexOf('K');
                                                    }
                                                    else if (currentLineRCFText[3].Contains("L"))
                                                    {
                                                        indexOfKOrL = currentLineRCFText[3].LastIndexOf('L');
                                                    }
                                                    if (!currentLineRCFText[3].Contains("K") && (!currentLineRCFText[3].Contains("L")) && (currentLineRCFText[3].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                    {
                                                        recdata.pcsindicator = currentLineRCFText[3] != "" ? currentLineRCFText[3].Substring(0, 1) : "";

                                                        recdata.numofpcs = currentLineRCFText[3] != ""
                                                                             ? (currentLineRCFText[3].Substring(1))
                                                                             : "0";
                                                    }
                                                    else if (((currentLineRCFText[3].Contains("K")) || (currentLineRCFText[3].Contains("L"))) || (currentLineRCFText[3].Contains("T") || currentLineRCFText[3].Contains("P")))
                                                    {
                                                        recdata.pcsindicator = currentLineRCFText[3] != ""
                                                                         ? (currentLineRCFText[3].Substring(0, 1))
                                                                         : "";

                                                        recdata.numofpcs = currentLineRCFText[3] != ""
                                                                             ? (currentLineRCFText[3].Substring(1, indexOfKOrL - 1))
                                                                             : "0";

                                                        recdata.weightcode = currentLineRCFText[3] != ""
                                                                         ? (currentLineRCFText[3].Substring(indexOfKOrL, 1))
                                                                         : "";

                                                        recdata.weight = currentLineRCFText[3] != ""
                                                                     ? (currentLineRCFText[3].Substring(indexOfKOrL + 1))
                                                                     : "0";
                                                    }
                                                }
                                            }
                                            if (currentLineRCFText.Length >= 6)
                                            {
                                                string[] currentLineTextSplit = currentLineRCFText[5].Split('-');
                                                if (currentLineTextSplit.Length >= 1)
                                                {
                                                    if (currentLineTextSplit[0] != "")
                                                    {
                                                        recdata.timeindicator = currentLineTextSplit[0] != ""
                                                                                  ? currentLineTextSplit[0].Substring(0, 1)
                                                                                  : "";

                                                        recdata.arrivaltime = currentLineTextSplit[0] != ""
                                                                              ? currentLineTextSplit[0].Substring(1)
                                                                              : "";
                                                    }

                                                }
                                                if (currentLineTextSplit.Length >= 2)
                                                {
                                                    dayChangeIndicator2 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                }
                                            }
                                            if (currentLineRCFText.Length >= 7)
                                            {
                                                string[] currentLineTextSplit = currentLineRCFText[6].Split('-');
                                                if (currentLineTextSplit.Length >= 1)
                                                {
                                                    string timeIndicator = currentLineTextSplit[0] != ""
                                                                               ? currentLineTextSplit[0].Substring(0, 1)
                                                                               : "";
                                                    timeOfArrival = currentLineTextSplit[0] != ""
                                                                        ? currentLineTextSplit[0].Substring(0, 1)
                                                                        : "";
                                                }
                                                if (currentLineTextSplit.Length >= 2)
                                                {
                                                    dayChangeIndicator3 = currentLineTextSplit[1] != "" ? currentLineTextSplit[1] : "";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                        fsanodes[fsanodes.Length - 1] = recdata;
                                        #endregion
                                        break;
                                    case "BKD":
                                        {
                                            #region BKD
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.carriercode = "";
                                                    recdata.flightnum = msg[1];
                                                    string[] split = msg[2].Split('-');
                                                    recdata.fltday = Convert.ToString(split[0].ToString()) == "" ? System.DateTime.Now.Day.ToString() : split[0].Substring(0, 2);
                                                    recdata.fltmonth = Convert.ToString(split[0].ToString()) == "" ? System.DateTime.Now.Month.ToString() : (DateTime.ParseExact(split[0].Substring(2, 3), "MMM", CultureInfo.InvariantCulture).Month).ToString();
                                                    try
                                                    {
                                                        recdata.flttime = split[0].Substring(5);
                                                        recdata.daychangeindicator = split[1] + ",";
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // clsLog.WriteLogAzure(ex);
                                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                    }

                                                    recdata.fltorg = msg[3].Substring(0, 3);
                                                    recdata.fltdest = msg[3].Substring(3);
                                                    string[] arr = stringsplitter(msg[4]);
                                                    if (arr.Length > 0)
                                                    {
                                                        recdata.pcsindicator = arr[0];
                                                        recdata.numofpcs = arr[1];
                                                        recdata.weightcode = arr[2];
                                                        recdata.weight = arr[3];
                                                    }
                                                    if (msg.Length > 5)
                                                    {

                                                        if (msg[5].Contains("S"))
                                                        {
                                                            try
                                                            {
                                                                string[] strarr = msg[5].Split('-');
                                                                recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                                recdata.depttime = strarr[0].Substring(1);
                                                                recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                // clsLog.WriteLogAzure(ex);_logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string[] strarr = stringsplitter(msg[5]);
                                                            if (strarr.Length > 0)
                                                            {
                                                                recdata.volumecode = strarr[0];
                                                                recdata.volumeamt = strarr[1];
                                                                recdata.densityindicator = "DG";
                                                                recdata.densitygroup = strarr[3].Length > 0 ? strarr[3] : "";
                                                            }
                                                        }
                                                    }
                                                    try
                                                    {
                                                        if (msg.Length > 6)
                                                        {
                                                            if (msg[6].Contains("S"))
                                                            {
                                                                string[] strarr = msg[6].Split('-');
                                                                recdata.timeindicator = recdata.timeindicator + strarr[0].Substring(0, 1) + ",";
                                                                recdata.arrivaltime = strarr[0].Substring(1);
                                                                recdata.daychangeindicator = recdata.daychangeindicator + strarr[1] + ",";
                                                            }
                                                            else
                                                            {
                                                                string[] strarr = stringsplitter(msg[7]);
                                                                if (strarr.Length > 0)
                                                                {
                                                                    recdata.volumecode = strarr[0];
                                                                    recdata.volumeamt = strarr[1];
                                                                    recdata.densityindicator = "DG";
                                                                    recdata.densitygroup = strarr[3].Length > 0 ? strarr[3] : "";
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // clsLog.WriteLogAzure(ex);
                                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                    }
                                                    if (msg.Length > 7)
                                                    {
                                                        string[] strarr = stringsplitter(msg[7]);
                                                        if (strarr.Length > 0)
                                                        {
                                                            recdata.volumecode = strarr[0];
                                                            recdata.volumeamt = strarr[1];
                                                            recdata.densityindicator = "DG";
                                                            recdata.densitygroup = strarr[3].Length > 0 ? strarr[3] : "";
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "TRM":
                                        {
                                            #region TRM
                                            string[] currentLineTRMText = str.ToString().Split('/');
                                            string trmTag = string.Empty;
                                            string airportCode = string.Empty;
                                            try
                                            {
                                                if (currentLineTRMText.Length >= 1)
                                                {
                                                    trmTag = currentLineTRMText[0] != "" ? currentLineTRMText[0] : "";
                                                }
                                                if (currentLineTRMText.Length >= 2)
                                                {
                                                    carrierCode = currentLineTRMText[1] != "" ? currentLineTRMText[1] : "";
                                                }

                                                if (currentLineTRMText.Length >= 3)
                                                {
                                                    airportCode = currentLineTRMText[2] != "" ? currentLineTRMText[2] : "";
                                                }

                                                if (currentLineTRMText.Length >= 4)
                                                {
                                                    int indexOfKOrL = 0;

                                                    if (currentLineTRMText[3].Contains("K"))
                                                    {
                                                        indexOfKOrL = currentLineTRMText[3].LastIndexOf('K');
                                                    }
                                                    else if (currentLineTRMText[3].Contains("L"))
                                                    {
                                                        indexOfKOrL = currentLineTRMText[3].LastIndexOf('L');
                                                    }

                                                    if (!currentLineTRMText[3].Contains("K") && (!currentLineTRMText[3].Contains("L")) && (currentLineTRMText[3].Contains("T")))
                                                    {
                                                        piecesCode = currentLineTRMText[3] != ""
                                                                         ? char.Parse(currentLineTRMText[3].Substring(0, 1))
                                                                         : char.Parse("");
                                                        numberOfPieces = currentLineTRMText[3] != ""
                                                                             ? int.Parse(currentLineTRMText[3].Substring(1))
                                                                             : 0;
                                                    }
                                                    else if (((currentLineTRMText[3].Contains("K")) || (currentLineTRMText[3].Contains("L"))) && (currentLineTRMText[3].Contains("T")))
                                                    {
                                                        piecesCode = currentLineTRMText[3] != ""
                                                                         ? char.Parse(currentLineTRMText[3].Substring(0, 1))
                                                                         : char.Parse("");

                                                        numberOfPieces = currentLineTRMText[3] != ""
                                                                             ? int.Parse(currentLineTRMText[3].Substring(1, indexOfKOrL - 1))
                                                                             : 0;

                                                        weightCode = currentLineTRMText[3] != ""
                                                                         ? char.Parse(currentLineTRMText[3].Substring(indexOfKOrL, 1))
                                                                         : char.Parse("");

                                                        weight = currentLineTRMText[3] != ""
                                                                     ? decimal.Parse(currentLineTRMText[3].Substring(indexOfKOrL + 1))
                                                                     : decimal.Parse("0");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                            fsanodes[fsanodes.Length - 1] = recdata;
                                            #endregion
                                        }
                                        break;
                                    case "TFD":
                                        {
                                            #region TFD
                                            string[] currentLineTFDText = str[2].ToString().Split('/');
                                            string tfdTag = string.Empty;
                                            string time = string.Empty;
                                            string airportCode = string.Empty;
                                            string transferManifestNumber = string.Empty;
                                            int indexOfKOrL = 0;
                                            lastrec = "TFD";
                                            try
                                            {
                                                if (currentLineTFDText.Length >= 1)
                                                {
                                                    recdata.messageprefix = currentLineTFDText[0] != "" ? currentLineTFDText[0] : "";
                                                }
                                                if (currentLineTFDText.Length >= 2)
                                                {
                                                    recdata.carriercode = currentLineTFDText[1] != "" ? currentLineTFDText[1] : "";
                                                }
                                                if (currentLineTFDText.Length >= 3)
                                                {
                                                    if (currentLineTFDText[2].Length == 9)
                                                    {
                                                        recdata.fltday = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(0, 2) : "";
                                                        recdata.fltmonth = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(2, 3) : "";
                                                        //recdata.flttime = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(5) : "";
                                                        recdata.flttime = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(5, 2) + ":" + currentLineTFDText[2].Substring(7) + ":00" : "";
                                                    }
                                                    else if (currentLineTFDText[2].Length == 8)
                                                    {
                                                        recdata.fltday = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(0, 1) : "";
                                                        recdata.fltmonth = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(1, 3) : "";
                                                        //recdata.flttime = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(4) : "";
                                                        recdata.flttime = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(5, 2) + ":" + currentLineTFDText[2].Substring(7) : "";
                                                    }
                                                    else
                                                    {
                                                        recdata.fltday = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(0, 2) : "";
                                                        recdata.fltmonth = currentLineTFDText[2] != "" ? currentLineTFDText[2].Substring(2, 3) : "";
                                                        recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                    }
                                                }
                                                if (currentLineTFDText.Length >= 4)
                                                {
                                                    recdata.airportcode = currentLineTFDText[3] != "" ? currentLineTFDText[3] : "";
                                                }
                                                if (currentLineTFDText.Length >= 5)
                                                {
                                                    if (currentLineTFDText[4].Contains("K"))
                                                    {
                                                        indexOfKOrL = currentLineTFDText[4].LastIndexOf('K');
                                                    }
                                                    else if (currentLineTFDText[4].Contains("L"))
                                                    {
                                                        indexOfKOrL = currentLineTFDText[4].LastIndexOf('L');
                                                    }
                                                    if (!currentLineTFDText[4].Contains("K") && (!currentLineTFDText[4].Contains("L")) && ((currentLineTFDText[4].Contains("T")) || (currentLineTFDText[4].Contains("P"))))
                                                    {
                                                        recdata.pcsindicator = currentLineTFDText[4].Substring(0, 1);
                                                        recdata.numofpcs = currentLineTFDText[4].Substring(1);
                                                    }
                                                    else if (((currentLineTFDText[4].Contains("K")) || (currentLineTFDText[4].Contains("L"))) && ((currentLineTFDText[4].Contains("T")) || (currentLineTFDText[4].Contains("P"))))
                                                    {
                                                        recdata.pcsindicator = currentLineTFDText[4].Substring(0, 1) != ""
                                                                         ? (currentLineTFDText[4].Substring(0, 1))
                                                                         : ("");
                                                        recdata.numofpcs = currentLineTFDText[4].Substring(1) != ""
                                                                             ? (currentLineTFDText[4].Substring(1, indexOfKOrL - 1))
                                                                             : "0";

                                                        recdata.weightcode = currentLineTFDText[4] != ""
                                                                         ? (currentLineTFDText[4].Substring(indexOfKOrL, 1))
                                                                         : ("");

                                                        recdata.weight = currentLineTFDText[4] != ""
                                                                     ? (currentLineTFDText[4].Substring(indexOfKOrL + 1))
                                                                     : ("0");
                                                    }
                                                }
                                                if (currentLineTFDText.Length >= 6)
                                                {
                                                    recdata.transfermanifestnumber = currentLineTFDText[5] != "" ? currentLineTFDText[5] : "";
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                            fsanodes[fsanodes.Length - 1] = recdata;
                                            #endregion
                                        }
                                        break;
                                    case "CRC":
                                        {
                                            #region CRC
                                            if (msg.Length > 0)
                                            {
                                                try
                                                {
                                                    recdata.messageprefix = msg[0];
                                                    recdata.fltday = msg[1].Substring(0, 2);
                                                    recdata.fltmonth = msg[1].Substring(2, 3);
                                                    recdata.flttime = msg[1].Substring(5);
                                                    recdata.airportcode = msg[2];
                                                    string[] arr = stringsplitter(msg[3]);
                                                    if (arr.Length > 0)
                                                    {
                                                        recdata.pcsindicator = arr[0];
                                                        recdata.numofpcs = arr[1];
                                                        recdata.weightcode = arr[2];
                                                        recdata.weight = arr[3];
                                                    }
                                                    if (msg.Length > 4)
                                                    {
                                                        recdata.carriercode = msg[4].Substring(0, 2);
                                                        recdata.flightnum = msg[4].Substring(2);
                                                    }
                                                    if (msg.Length > 5)
                                                    {
                                                        recdata.fltday = msg[5].Substring(0, 2);
                                                        recdata.fltmonth = msg[5].Substring(2);
                                                    }
                                                    if (msg.Length > 6)
                                                    {
                                                        recdata.fltdest = msg[6].Substring(0, 3);
                                                        recdata.fltorg = msg[6].Substring(3);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                fsanodes[fsanodes.Length - 1] = recdata;
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "OCI":
                                        {
                                            #region OCI
                                            string[] msgdataOCIdata = str[i].Split('/');
                                            if (msg.Length > 0)
                                            {
                                                lastrec = "OCI";
                                                MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                try
                                                {
                                                    if (fsanodes.Length == 0)
                                                    {
                                                        recdata.messageprefix = msgdataOCIdata[0] != "" ? msgdataOCIdata[0] : "";
                                                    }
                                                    custom.IsoCountryCodeOci = msgdataOCIdata[1];
                                                    custom.InformationIdentifierOci = msgdataOCIdata[2];
                                                    custom.CsrIdentifierOci = msgdataOCIdata[3];
                                                    custom.SupplementaryCsrIdentifierOci = msgdataOCIdata[4];
                                                    custom.consigref = awbref;
                                                    custom.OCIInfo = str[i];

                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref custominfo, custominfo.Length + 1);
                                                custominfo[custominfo.Length - 1] = custom;

                                                if (fsanodes[0].messageprefix != "RCS")
                                                {
                                                    Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                                    fsanodes[fsanodes.Length - 1] = recdata;
                                                }
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "ULD":
                                        {
                                            #region ULD
                                            try
                                            {
                                                int uldnum = 0;
                                                if (msg.Length > 1)
                                                {
                                                    Array.Resize(ref uld, msg.Length - 1);
                                                    for (int k = 1; k < msg.Length; k++)
                                                    {
                                                        string[] splitstr = msg[k].Split('-');
                                                        uld[uldnum].uldtype = splitstr[0].Substring(0, 3);
                                                        uld[uldnum].uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 5);
                                                        uld[uldnum].uldowner = splitstr[0].Substring(splitstr[0].Length - 2, 2);
                                                        if (splitstr.Length > 1)
                                                        {
                                                            uld[uldnum].uldloadingindicator = splitstr[1];
                                                        }
                                                        uldnum++;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "OSI":
                                        {
                                            #region OSI
                                            try
                                            {
                                                lastrec = msg[0];
                                                if (msg[1].Length > 0)
                                                {
                                                    Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                                    othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "DLV":
                                        #region FSU/DLV
                                        try
                                        {
                                            string[] currentLineDLVText = str[2].ToString().Split('/');
                                            string dlvTag = string.Empty;
                                            string time = string.Empty;
                                            string airportCode = string.Empty;
                                            int indexOfKOrL = 0;
                                            if (currentLineDLVText.Length >= 1)
                                            {
                                                recdata.messageprefix = currentLineDLVText[0] != "" ? currentLineDLVText[0] : "";
                                            }

                                            if (currentLineDLVText.Length >= 2)
                                            {
                                                if (currentLineDLVText[1].Length == 9)
                                                {
                                                    recdata.fltday = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(2, 3) : "";
                                                    recdata.flttime = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(5, 2) + ":" + currentLineDLVText[1].Substring(7) + ":00" : "";
                                                }
                                                else if (currentLineDLVText[1].Length == 8)
                                                {
                                                    recdata.fltday = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(0, 1) : "";
                                                    recdata.fltmonth = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(1, 3) : "";
                                                    recdata.flttime = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(5, 2) + ":" + currentLineDLVText[1].Substring(7) : "";
                                                }
                                                else if (currentLineDLVText[1].Length == 5)
                                                {
                                                    recdata.fltday = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(0, 2) : "";
                                                    recdata.fltmonth = currentLineDLVText[1] != "" ? currentLineDLVText[1].Substring(2, 3) : "";
                                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                }
                                                else
                                                {
                                                    recdata.fltday = DateTime.UtcNow.ToShortDateString().Substring(3, 2);
                                                    recdata.fltmonth = DateTime.UtcNow.ToString("MMM").ToUpper();
                                                    recdata.flttime = DateTime.UtcNow.ToLongTimeString().Substring(0, 8) + " (UTC)";
                                                }
                                            }
                                            if (currentLineDLVText.Length >= 3)
                                            {
                                                recdata.fltdest = currentLineDLVText[2] != "" ? currentLineDLVText[2] : "";
                                            }
                                            if (currentLineDLVText.Length >= 4)
                                            {
                                                if (currentLineDLVText[3].Contains("K"))
                                                {
                                                    indexOfKOrL = currentLineDLVText[3].LastIndexOf('K');
                                                }
                                                else if (currentLineDLVText[3].Contains("L"))
                                                {
                                                    indexOfKOrL = currentLineDLVText[3].LastIndexOf('L');
                                                }

                                                if (!currentLineDLVText[3].Contains("K") && (!currentLineDLVText[3].Contains("L")) && (currentLineDLVText[3].Contains("T")))
                                                {
                                                    recdata.pcsindicator = currentLineDLVText[3] != ""
                                                                     ? (currentLineDLVText[3].Substring(0, 1))
                                                                     : ("");
                                                    recdata.numofpcs = currentLineDLVText[3] != "" ? (currentLineDLVText[3].Substring(1)) : "0";
                                                }
                                                else if (((currentLineDLVText[3].Contains("K")) || (currentLineDLVText[3].Contains("L"))) && (currentLineDLVText[3].Contains("T")) || (currentLineDLVText[3].Contains("P")))
                                                {
                                                    recdata.pcsindicator = currentLineDLVText[3] != ""
                                                                     ? (currentLineDLVText[3].Substring(0, 1))
                                                                     : ("");
                                                    if (indexOfKOrL > 0)
                                                    {
                                                        recdata.numofpcs = currentLineDLVText[3] != ""
                                                                             ? (currentLineDLVText[3].Substring(1, indexOfKOrL - 1))
                                                                             : "0";

                                                        recdata.weightcode = currentLineDLVText[3] != ""
                                                                                ? (currentLineDLVText[3].Substring(indexOfKOrL, 1))
                                                                                : ("");

                                                        recdata.weight = currentLineDLVText[3] != ""
                                                                      ? (currentLineDLVText[3].Substring(indexOfKOrL + 1))
                                                                      : ("0");
                                                    }
                                                    else
                                                    {
                                                        recdata.numofpcs = currentLineDLVText[3].Substring(1);
                                                    }
                                                }
                                            }
                                            if (currentLineDLVText.Length >= 5)
                                            {
                                                recdata.name = currentLineDLVText[4] != "" ? currentLineDLVText[4] : "";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        Array.Resize(ref fsanodes, fsanodes.Length + 1);
                                        fsanodes[fsanodes.Length - 1] = recdata;
                                        #endregion
                                        break;
                                    default:
                                        {
                                            #region Other Info
                                            if (str[i].StartsWith("/"))
                                            {
                                                string[] msgdata = str[i].Split('/');
                                                try
                                                {
                                                    #region OSI 2
                                                    if (lastrec == "OSI")
                                                    {
                                                        othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msgdata[1].Length > 0 ? msgdata[1] : "";
                                                        lastrec = "NA";
                                                    }
                                                    #endregion
                                                    if (lastrec == "TFD" || lastrec == "RCT")
                                                    {
                                                        fsanodes[fsanodes.Length - 1].seccarriercode = msgdata[1].Length > 1 ? msgdata[1] : "";
                                                        lastrec = "NA";
                                                    }

                                                    #region OCI
                                                    if (lastrec == "OCI")
                                                    {
                                                        string[] msgdataOCI = str[i].Split('/');
                                                        if (msgdata.Length > 0)
                                                        {
                                                            lastrec = "OCI";
                                                            MessageData.customsextrainfo custom = new MessageData.customsextrainfo("");
                                                            custom.IsoCountryCodeOci = msgdataOCI[1];
                                                            custom.InformationIdentifierOci = msgdataOCI[2];
                                                            custom.CsrIdentifierOci = msgdataOCI[3];
                                                            custom.SupplementaryCsrIdentifierOci = msgdata[4];
                                                            custom.OCIInfo = str[i];
                                                            Array.Resize(ref custominfo, custominfo.Length + 1);
                                                            custominfo[custominfo.Length - 1] = custom;
                                                        }

                                                    }
                                                    #endregion
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                            }
                                            #endregion
                                        }
                                        break;
                                }
                            }
                            #endregion Decode Status
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                flag = false;
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        /// <summary>
        /// Method to save FSU decoded FSU message
        /// </summary>
        /// <param name="strMsg">Message string</param>
        /// <param name="fsadata">Contains consignment information</param>
        /// <param name="fsanodes">Contains flight information</param>
        /// <param name="customextrainfo">Other Customs, Security and Regulatory Information</param>
        /// <param name="ulddata">ULD information</param>
        /// <param name="othinfoarray">Other service information</param>
        /// <param name="refNo">SrNo from tblInbox</param>
        /// <param name="strMessage"></param>
        /// <param name="strMessageFrom"></param>
        /// <param name="strFromID"></param>
        /// <param name="strStatus"></param>
        /// <returns>Returns true if FSU message saved successfuly</returns>
        //public async Task<bool> SaveandUpdateFSUMessage(string strMsg, ref MessageData.FSAInfo fsadata, ref MessageData.CommonStruct[] fsanodes, ref MessageData.customsextrainfo[] customextrainfo, ref MessageData.ULDinfo[] ulddata, ref MessageData.otherserviceinfo[] othinfoarray, int refNo, string strMessage, string strMessageFrom, string strFromID, string strStatus, out string ErrorMsg)
        public async Task<(bool Success, string ErrorMsg, MessageData.FSAInfo fsadata, MessageData.CommonStruct[] fsanodes, MessageData.customsextrainfo[] customextrainfo, MessageData.ULDinfo[] ulddata, MessageData.otherserviceinfo[] othinfoarray)>
        SaveandUpdateFSUMessage(
        string strMsg,
        MessageData.FSAInfo fsadata,
        MessageData.CommonStruct[] fsanodes,
        MessageData.customsextrainfo[] customextrainfo,
        MessageData.ULDinfo[] ulddata,
        MessageData.otherserviceinfo[] othinfoarray,
        int refNo,
        string strMessage,
        string strMessageFrom,
        string strFromID,
        string strStatus)
        {

            {
                //SQLServer dtb = new SQLServer();
                string ErrorMsg = string.Empty;
                bool flag = false;
                string strFSUBooking = string.Empty;
                //Cls_BL objBL = new Cls_BL();
                bool isAWBPresent = false;
                bool AWBAccepted = true;
                try
                {
                    string awbnum = string.Empty, awbprefix = string.Empty;
                    string MessagePrefix = fsanodes.Length > 0 && fsanodes[0].messageprefix.Length > 0 ? "FSU/" + fsanodes[0].messageprefix.ToUpper() : "FSU";
                    //GenericFunction genericfunction = new GenericFunction();
                    //genericfunction.UpdateInboxFromMessageParameter(refNo, fsadata.airlineprefix + "-" + fsadata.awbnum, string.Empty, string.Empty, string.Empty, MessagePrefix, "FSU", DateTime.Parse("1900-01-01"));
                    _genericFunction.UpdateInboxFromMessageParameter(refNo, fsadata.airlineprefix + "-" + fsadata.awbnum, string.Empty, string.Empty, string.Empty, MessagePrefix, "FSU", DateTime.Parse("1900-01-01"));

                    #region Check AWB is present or not

                    //DataSet dsCheck = new DataSet();
                    //dtb = new SQLServer();
                    //string[] parametername = new string[] { "AWBNumber", "AWBPrefix", "refNo" };
                    //object[] AWBvalues = new object[] { fsadata.awbnum, fsadata.airlineprefix, refNo };

                    //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int };
                    //dsCheck = dtb.SelectRecords("sp_getawbdetails", parametername, AWBvalues, ptype);
                    SqlParameter[] sqlParameters = new SqlParameter[]
                    {
                  new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fsadata.awbnum },
                  new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix },
                  new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo },

                    };
                    DataSet dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameters); //name mismatch
                    if (dsCheck != null && dsCheck.Tables.Count > 0 && dsCheck.Tables[0].Rows.Count > 0)
                    {
                        if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(fsadata.awbnum, StringComparison.OrdinalIgnoreCase))
                        {

                            isAWBPresent = true;
                            if (dsCheck.Tables[0].Rows[0]["IsAccepted"].ToString().ToUpper() == "FALSE" || dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() != "E")
                                AWBAccepted = false;
                            if (dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() == "V"
                                && (fsanodes[0].messageprefix.ToUpper().Trim() == "RCS" || fsanodes[0].messageprefix.ToUpper().Trim() == "RCT"))
                            {
                                ErrorMsg = fsadata.awbnum + " AWB is Voided";
                                //return false;
                                return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                            }
                        }
                        if (dsCheck.Tables[0].Rows[0]["IsRetToShipper"].ToString() == "True")
                        {
                            ErrorMsg = "AWB already return to shipper.";
                            //return false;
                            return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                        }

                        if ((dsCheck.Tables[0].Rows[0]["DestinationCode"].ToString() != fsadata.dest || dsCheck.Tables[0].Rows[0]["OriginCode"].ToString() != fsadata.origin) && fsadata.awbnum.Length > 0 && fsanodes.Length > 0
                        && (fsanodes[0].messageprefix.ToUpper() == "DLV" || fsanodes[0].messageprefix.ToUpper() == "RCS" || fsanodes[0].messageprefix.ToUpper() == "RCT" || fsanodes[0].messageprefix.ToUpper() == "RCF"))
                        {
                            ErrorMsg = "Origin/Destination mismatch";
                            //return false;
                            return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                        }

                    }
                    #endregion Check AWB is present or not

                    #region Below Segment of FSU/DIS
                    ///AWB Discrepancy Recorded through FSU/DIS Message
                    if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "DIS")
                    {
                        if (fsanodes.Length > 0)
                        {
                            for (int i = 0; i < fsanodes.Length; i++)
                            {
                                DateTime strDiscrepancyDate = new DateTime();
                                if (fsanodes[i].fltmonth != "" && fsanodes[i].fltday != "")
                                {
                                    string strdate = (fsanodes[i].fltmonth + "/" + fsanodes[i].fltday + "/" + DateTime.Now.Year.ToString());
                                    strDiscrepancyDate = DateTime.Parse(strdate);
                                }

                                //     string[] sqlParameterName = new string[]
                                //     {
                                //          "AWBPrefix",
                                //          "AWBNumber",
                                //          "AWBOrigin",
                                //          "AWBDestination",
                                //         "AWBPieces",
                                //         "AWBGrossWeight" ,
                                //         "FlightNumber",
                                //         "DiscrepancyDate",
                                //         "FlightOrigin",
                                //         "DiscrepancyCode",
                                //         "DiscrepancyPcsCode",
                                //         "DiscrepancyPcs",
                                //         "UOM",
                                //         "Discrepancyweight",
                                //         "UpdatedBy",
                                //         "UpdatedOn",
                                //         "OtherServiceInformation"
                                //};
                                //     object[] sqlParameterValue = new object[] {
                                //        fsadata.airlineprefix,
                                //        fsadata.awbnum,
                                //         fsadata.origin,
                                //         fsadata.dest,
                                //         int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
                                //         decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                                //         fsanodes[i].flightnum,
                                //         strDiscrepancyDate,
                                //         //fsanodes[0].fltorg,
                                //          fsadata.origin,
                                //         fsanodes[0].infocode,
                                //         fsanodes[i].pcsindicator,
                                //         int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
                                //         fsanodes[i].weightcode,
                                //         decimal.Parse(fsanodes[i].weight == "" ? "0" : fsanodes[i].weight),
                                //         "FSU",
                                //         DateTime.Now,
                                //        othinfoarray.Length > 0 ? Convert.ToString(othinfoarray[0].otherserviceinfo1) : ""
                                // };
                                //     SqlDbType[] sqlParameter = new SqlDbType[] {
                                //         SqlDbType.VarChar,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.Int,
                                //         SqlDbType.Decimal,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.DateTime,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.Int,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.Decimal,
                                //         SqlDbType.VarChar,
                                //         SqlDbType.DateTime,
                                //         SqlDbType.VarChar
                                //     };
                                var disParams = new[]
                                {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix  },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fsadata.awbnum  },
                                new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = fsadata.origin  },
                                new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = fsadata.dest  },
                                new SqlParameter("@AWBPieces", SqlDbType.Int)
                                {
                                    Value = int.TryParse(fsadata.pcscnt, out var pcs) ? pcs : 0
                                },
                                new SqlParameter("@AWBGrossWeight", SqlDbType.Decimal)
                                {
                                    Value = decimal.Parse(fsadata.weight==""?"0":fsadata.weight)
                                },
                                new SqlParameter("@FlightNumber", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].flightnum
                                },
                                new SqlParameter("@DiscrepancyDate", SqlDbType.DateTime)
                                {
                                    Value = strDiscrepancyDate
                                },
                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar)
                                {
                                    Value = fsadata.origin
                                },
                                new SqlParameter("@DiscrepancyCode", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].infocode
                                },
                                new SqlParameter("@DiscrepancyPcsCode", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].pcsindicator
                                },
                                new SqlParameter("@DiscrepancyPcs", SqlDbType.Int)
                                {
                                    // Value = int.TryParse(fsanodes[i].numofpcs, out var np) ? np : 0
                                    Value = int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
                                },
                                new SqlParameter("@UOM", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].weightcode
                                },
                                new SqlParameter("@DiscrepancyWeight", SqlDbType.Decimal)
                                {
                                    // Value = decimal.TryParse(fsanodes[i].weight, out var dw) ? dw : 0
                                    Value = decimal.Parse(fsanodes[i].weight == "" ? "0" : fsanodes[i].weight),
                                },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FSU" },
                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                new SqlParameter("@OtherServiceInformation", SqlDbType.VarChar)
                                {
                                    Value = othinfoarray.Length > 0
                                        ? Convert.ToString(othinfoarray[0].otherserviceinfo1)
                                        : string.Empty
                                }
                            };
                                //if (dtb.InsertData("SpSaveFSUAWBDiscrepancy", sqlParameterName, sqlParameter, sqlParameterValue))
                                if (await _readWriteDao.ExecuteNonQueryAsync("SpSaveFSUAWBDiscrepancy", disParams))
                                    flag = true;
                                else
                                {
                                    flag = false;
                                    //return flag;
                                    return (flag, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);

                                }
                            }
                        }
                    }

                    #endregion

                    #region Below Segmnet of FSU/DLV Message
                    ///Make  AWB Delivered  Throgh  DLV Message 
                    if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "DLV")
                    {
                        if (fsanodes.Length > 0)
                        {
                            for (int i = 0; i < fsanodes.Length; i++)
                            {
                                DateTime DeliveryDate = new DateTime();
                                if (fsanodes[i].fltmonth != "" && fsanodes[i].fltday != "")
                                {
                                    string strdate = (fsanodes[i].fltmonth + "/" + fsanodes[i].fltday + "/" + DateTime.Now.Year.ToString());
                                    DeliveryDate = DateTime.Parse(strdate);
                                }
                                //string[] sqlParameterName = new string[]
                                //{
                                //    "AWBPrefix",
                                //    "AWBNo",
                                //    "Origin",
                                //    "Destination",
                                //    "AWbPcs",
                                //    "AWbGrossWt" ,
                                //    "PieceCode",
                                //    "Deliverypcs",
                                //    "WeightCode",
                                //    "DeliveryGross",
                                //    "FlightDestination ",
                                //    "Dname",
                                //    "Deliverydate",
                                //    "UpdatedBy",
                                //    "RefNo"
                                //};
                                //object[] sqlParameterValue = new object[]
                                //{
                                //    fsadata.airlineprefix,
                                //    fsadata.awbnum,
                                //    fsadata.origin,
                                //    fsadata.dest,
                                //    int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
                                //    decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                                //    fsanodes[i].pcsindicator,
                                //    fsanodes[i].numofpcs,
                                //    fsanodes[i].weightcode,
                                //    //fsanodes[i].weight,
                                //    //decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                                //    decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight),
                                //    fsanodes[0].fltdest,
                                //    fsanodes[0].name,
                                //    DeliveryDate,
                                //    "FSU/DLV",
                                //    refNo
                                //};
                                //SqlDbType[] sqlParameter = new SqlDbType[] {
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.Int,
                                //    SqlDbType.Decimal,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.Int,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.Decimal,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.DateTime,
                                //    SqlDbType.VarChar,
                                //    SqlDbType.Int
                                //};
                                var dlvParams = new[]
                                {
                              new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix  },
                              new SqlParameter("@AWBNo", SqlDbType.VarChar) { Value = fsadata.awbnum },
                              new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fsadata.origin  },
                              new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fsadata.dest  },
                              new SqlParameter("@AWbPcs", SqlDbType.Int) { Value = int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt) },
                              new SqlParameter("@AWbGrossWt", SqlDbType.Decimal) { Value = decimal.Parse(fsadata.weight==""?"0":fsadata.weight) },
                              new SqlParameter("@PieceCode", SqlDbType.VarChar) { Value = fsanodes[i].pcsindicator  },
                              new SqlParameter("@Deliverypcs", SqlDbType.Int) { Value = fsanodes[i].numofpcs},
                              new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = fsanodes[i].weightcode  },
                              new SqlParameter("@DeliveryGross", SqlDbType.Decimal) { Value = decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight) },
                              new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = fsanodes[0].fltdest  },
                              new SqlParameter("@Dname", SqlDbType.VarChar) { Value = fsanodes[0].name  },
                              new SqlParameter("@Deliverydate", SqlDbType.DateTime) { Value = DeliveryDate },
                              new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FSU/DLV" },
                              new SqlParameter("@RefNo", SqlDbType.Int) { Value = refNo }
                            };

                                DataSet dsDLVResult = new DataSet();
                                //dsDLVResult = dtb.SelectRecords("MakeAWBDeliveryorderofFSUMessage", sqlParameterName, sqlParameterValue, sqlParameter);
                                dsDLVResult = await _readWriteDao.SelectRecords("MakeAWBDeliveryorderofFSUMessage", dlvParams);

                                if (dsDLVResult != null && dsDLVResult.Tables.Count > 0)
                                {
                                    for (int j = 0; j < dsDLVResult.Tables.Count; j++)
                                    {
                                        if (dsDLVResult.Tables[j].Columns.Contains("ResultStatus") && dsDLVResult.Tables[j].Rows[0]["ResultStatus"].ToString().ToUpper() == "FALSE")
                                            //return true;
                                            return (true, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);

                                    }

                                }
                            }
                        }
                        #endregion

                        #region  Below Segmnet of RCS and RCT Message
                        ///Make  AWB Accepted Throgh  RCS Message 
                        bool isAcceptedbyFSURCS = false;
                        bool isAcceptedbyFSURCT = false;
                        //if (!string.IsNullOrEmpty(genericfunction.ReadValueFromDb("isAcceptedbyFSURCS").Trim()) && Convert.ToBoolean(genericfunction.ReadValueFromDb("isAcceptedbyFSURCS").Trim()))
                        if (!string.IsNullOrEmpty(ConfigCache.Get("isAcceptedbyFSURCS").Trim()) && Convert.ToBoolean(ConfigCache.Get("isAcceptedbyFSURCS").Trim()))
                        {
                            if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper().Trim() == "RCS")
                            {
                                isAcceptedbyFSURCS = true;
                            }
                        }
                        //if (!string.IsNullOrEmpty(genericfunction.ReadValueFromDb("isAcceptedbyFSURCT").Trim()) && Convert.ToBoolean(genericfunction.ReadValueFromDb("isAcceptedbyFSURCT").Trim()))
                        if (!string.IsNullOrEmpty(ConfigCache.Get("isAcceptedbyFSURCT").Trim()) && Convert.ToBoolean(ConfigCache.Get("isAcceptedbyFSURCT").Trim()))
                        {
                            if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsadata.awbnum.Length > 0 && fsanodes[0].messageprefix.ToUpper().Trim() == "RCT")
                            {
                                isAcceptedbyFSURCT = true;
                            }
                        }
                        if (isAcceptedbyFSURCS || isAcceptedbyFSURCT)
                        {
                            if (fsanodes.Length > 0)
                            {

                                if (!isAWBPresent)
                                {
                                    //return false;
                                    return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                }

                                for (int i = 0; i < fsanodes.Length; i++)
                                {
                                    #region : Check AWB is accepted or not :
                                    //FFRMessageProcessor ffrMessageProcessor = new FFRMessageProcessor();
                                    DataSet dsAWBStatus = new DataSet();
                                    //dsAWBStatus = ffrMessageProcessor.CheckValidateFFRMessage(fsadata.airlineprefix, fsadata.awbnum, fsadata.origin, fsadata.dest, "FSU/RCS", "", "");
                                    dsAWBStatus = await _fFRMessageProcessor.CheckValidateFFRMessage(fsadata.airlineprefix, fsadata.awbnum, fsadata.origin, fsadata.dest, "FSU/RCS", "", "");
                                    for (int k = 0; k < dsAWBStatus.Tables.Count; k++)
                                    {
                                        if (dsAWBStatus.Tables[k].Columns.Contains("MessageName") && dsAWBStatus.Tables[k].Columns.Contains("AWBSttus"))
                                        {
                                            if (dsAWBStatus.Tables[k].Rows[0]["AWBSttus"].ToString().ToUpper() == "ACCEPTED")
                                            {
                                                //GenericFunction genericFunction = new GenericFunction();
                                                //genericFunction.UpdateErrorMessageToInbox(refNo, "AWB is already accepted");
                                                _genericFunction.UpdateErrorMessageToInbox(refNo, "AWB is already accepted");
                                                //return true;
                                                return (true, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                            }
                                        }
                                        if (dsAWBStatus.Tables[k].Columns.Contains("ErrorMessage"))
                                        {
                                            string str = dsAWBStatus.Tables[k].Rows[0]["ErrorMessage"].ToString().ToUpper();
                                            if (str.Contains("MANIFESTED"))
                                            {
                                                //GenericFunction genericFunction = new GenericFunction();
                                                //genericFunction.UpdateErrorMessageToInbox(refNo, "AWB is already accepted");
                                                _genericFunction.UpdateErrorMessageToInbox(refNo, "AWB is already accepted");
                                                //return true;
                                                return (true, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                            }

                                            string _errorMsg = dsAWBStatus.Tables[k].Rows[0]["ErrorMessage"].ToString().ToUpper();
                                            if (_errorMsg.Contains("AGENT VALIDITY EXPIRED"))
                                            {
                                                //GenericFunction genericFunction = new GenericFunction();
                                                //genericFunction.UpdateErrorMessageToInbox(refNo, "Agent is Inactive OR Expired");
                                                _genericFunction.UpdateErrorMessageToInbox(refNo, "Agent is Inactive OR Expired");
                                                //return true;
                                                return (true, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                            }

                                            if (dsAWBStatus.Tables[k].Rows[0]["ErrorMessage"].ToString().Contains("DGR is not yet approved"))
                                            {
                                                //GenericFunction genericFunction = new GenericFunction();
                                                //genericFunction.UpdateErrorMessageToInbox(refNo, "DGR is not yet approved");
                                                _genericFunction.UpdateErrorMessageToInbox(refNo, "DGR is not yet approved");
                                                //return true;
                                                return (true, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                            }
                                        }
                                    }
                                    #endregion Check AWB is accepted or not

                                    DateTime AcceptedDate = new DateTime();
                                    string AcceptedTime = string.Empty;
                                    if (fsanodes[i].fltmonth != "" && fsanodes[i].fltday != "")
                                    {
                                        if (fsanodes[i].fltday.Length > 0 || fsanodes[i].fltmonth.Length > 0)
                                            AcceptedDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
                                        if (fsanodes[i].flttime.Length > 0)
                                            AcceptedTime = fsanodes[i].flttime.Substring(0, 2) + ":" + fsanodes[i].flttime.Substring(2) + ":00";
                                    }

                                    DataSet dsAWBMaterLogOldValues = new DataSet();
                                    ///MasterLog
                                    //dsAWBMaterLogOldValues = genericfunction.GetAWBMasterLogNewRecord(fsadata.airlineprefix, fsadata.awbnum);
                                    dsAWBMaterLogOldValues = _genericFunction.GetAWBMasterLogNewRecord(fsadata.airlineprefix, fsadata.awbnum);

                                    //string[] sqlParameterName = new string[]
                                    //                {
                                    //                    "AWBPrefix",
                                    //                    "AWBNo",
                                    //                    "Origin",
                                    //                    "Destination",
                                    //                    "AWbPcs",
                                    //                    "AWbGrossWt" ,
                                    //                    "PieceCode",
                                    //                    "AcceptPieces",
                                    //                    "WeightCode",
                                    //                    "AcceptedGrWeight",
                                    //                    "AccpetedOrigin",
                                    //                    "ShipperName",
                                    //                    "VolumeCode",
                                    //                    "VolumeAmount",
                                    //                    "UpdatedBy",
                                    //                    "AcceptedDate",
                                    //                    "AcceptedTime",
                                    //                    "refNo"
                                    //                };
                                    //object[] sqlParameterValue = new object[]
                                    //                    {
                                    //                        fsadata.airlineprefix,
                                    //                        fsadata.awbnum,
                                    //                        fsadata.origin,
                                    //                        fsadata.dest,
                                    //                        int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
                                    //                        decimal.Parse(fsadata.weight==""?"0":fsadata.weight),
                                    //                        fsanodes[i].pcsindicator,
                                    //                        int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
                                    //                        fsanodes[i].weightcode,
                                    //                        decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight),
                                    //                        fsanodes[0].airportcode,
                                    //                        fsanodes[0].name,
                                    //                        fsanodes[0].volumecode,
                                    //                        decimal.Parse(fsanodes[0].volumeamt==""?"0":fsanodes[0].volumeamt),
                                    //                        MessagePrefix,
                                    //                        AcceptedDate,
                                    //                        AcceptedTime,
                                    //                        refNo
                                    //                    };
                                    //SqlDbType[] sqlParameter = new SqlDbType[]
                                    //                {
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.Int,
                                    //                    SqlDbType.Decimal,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.Int,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.Decimal,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.Decimal,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.DateTime,
                                    //                    SqlDbType.VarChar,
                                    //                    SqlDbType.Int
                                    //                };
                                    var fsuRcsParams = new[]
                                    {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix},
                                new SqlParameter("@AWBNo", SqlDbType.VarChar) { Value = fsadata.awbnum },
                                new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fsadata.origin },
                                new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fsadata.dest },
                                new SqlParameter("@AWbPcs", SqlDbType.Int)
                                {
                                    Value = int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt)
                                },
                                new SqlParameter("@AWbGrossWt", SqlDbType.Decimal)
                                {
                                    Value = decimal.Parse(fsadata.weight==""?"0":fsadata.weight)
                                },
                                new SqlParameter("@PieceCode", SqlDbType.VarChar) { Value = fsanodes[i].pcsindicator },
                                new SqlParameter("@AcceptPieces", SqlDbType.Int)
                                {
                                    Value = int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs)
                                },
                                new SqlParameter("@WeightCode", SqlDbType.VarChar) { Value = fsanodes[i].weightcode },
                                new SqlParameter("@AcceptedGrWeight", SqlDbType.Decimal)
                                {
                                    Value =  decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight)
                                },
                                new SqlParameter("@AccpetedOrigin", SqlDbType.VarChar) { Value = fsanodes[0].airportcode },
                                new SqlParameter("@ShipperName", SqlDbType.VarChar) { Value = fsanodes[0].name },
                                new SqlParameter("@VolumeCode", SqlDbType.VarChar) { Value = fsanodes[0].volumecode },
                                new SqlParameter("@VolumeAmount", SqlDbType.Decimal)
                                {
                                    Value = decimal.Parse(fsanodes[0].volumeamt==""?"0":fsanodes[0].volumeamt)
                                },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = MessagePrefix },
                                new SqlParameter("@AcceptedDate", SqlDbType.DateTime)
                                {
                                    Value = AcceptedDate == default ? DateTime.UtcNow : AcceptedDate
                                },
                                new SqlParameter("@AcceptedTime", SqlDbType.VarChar) { Value = AcceptedTime },
                                new SqlParameter("@refNo", SqlDbType.Int) { Value = refNo }
                            };

                                    DataSet dsFSURCS = new DataSet();
                                    //dsFSURCS = dtb.SelectRecords("MakeAWBAcceptenceThroughFSUMessage", sqlParameterName, sqlParameterValue, sqlParameter);
                                    dsFSURCS = await _readWriteDao.SelectRecords("MakeAWBAcceptenceThroughFSUMessage", fsuRcsParams);

                                    for (int k = 0; k < dsFSURCS.Tables.Count; k++)
                                    {
                                        if (dsFSURCS.Tables[k].Columns.Contains("SuccessResult"))
                                        {
                                            if (Convert.ToBoolean(dsFSURCS.Tables[k].Rows[0]["SuccessResult"].ToString()))
                                            {
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

                                                        for (int j = 0; j < customextrainfo.Length; j++)
                                                        {
                                                            DataRow drawb = dtCustom.NewRow();

                                                            drawb["MRNNumber"] = "";
                                                            drawb["MsgType"] = "";
                                                            drawb["Country"] = customextrainfo[j].IsoCountryCodeOci;
                                                            drawb["DataID"] = customextrainfo[j].InformationIdentifierOci;
                                                            drawb["DataValue"] = customextrainfo[j].SupplementaryCsrIdentifierOci;
                                                            drawb["FlightNo"] = "";
                                                            drawb["FlightDate"] = System.DateTime.Now;
                                                            drawb["Info"] = customextrainfo[j].CsrIdentifierOci;
                                                            drawb["ProcessedBy"] = "FSU";
                                                            drawb["ProcessedDate"] = System.DateTime.Now;
                                                            drawb["Custom"] = "";
                                                            drawb["IsActive"] = "1";
                                                            drawb["HAWBNumber"] = "";
                                                            drawb["OCILine"] = customextrainfo[j].OCIInfo;

                                                            dtCustom.Rows.Add(drawb);
                                                        }

                                                        //        SqlParameter[] sqlParameters = new SqlParameter[] {
                                                        //new SqlParameter("AWBPrefix",fsadata.airlineprefix)
                                                        //, new SqlParameter("AWBNumber",fsadata.awbnum)
                                                        //, new SqlParameter("Custom", dtCustom)
                                                        // };
                                                        //        dtb.SelectRecords("Messaging.uspGetSetOCIDetails", sqlParameters);
                                                        var dlvParams = new[]
                                                        {
                                                 new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix  },
                                                 new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = fsadata.awbnum  },
                                                 new SqlParameter("@Custom", SqlDbType.Structured) { Value = dtCustom  },
                                                };
                                                        await _readWriteDao.SelectRecords("Messaging.uspGetSetOCIDetails", dlvParams);
                                                    }
                                                    catch
                                                    {
                                                        // clsLog.WriteLogAzure("Error while save FSU/RCS OCI information Message:" + awbnum);
                                                        _logger.LogError("Error while save FSU/RCS OCI information Message: {0}" , awbnum);
                                                    }
                                                }
                                                #region : Auto generate FWB and FHL  :
                                                GenericFunction genericFunction = new GenericFunction();
                                                bool isAutoSendFWB = false, isAutoSendFHL = false, isAutoFWBFHL = false;
                                                string PartnerCode = "";
                                                DateTime flightdate = DateTime.UtcNow;

                                                if (bool.TryParse(genericFunction.GetConfigurationValues("FWB"), out isAutoSendFWB))
                                                    isAutoFWBFHL = true;

                                                if (bool.TryParse(genericFunction.GetConfigurationValues("AutoSendFHL"), out isAutoSendFHL))
                                                    isAutoFWBFHL = true;
                                                if (dsFSURCS.Tables[0].Rows[0]["partnercode"].ToString() != "")
                                                {
                                                    PartnerCode = dsFSURCS.Tables[0].Rows[0]["partnercode"].ToString();
                                                }

                                                if (isAutoFWBFHL && (isAutoSendFWB || isAutoSendFHL))
                                                {
                                                    DataSet msgSeq = genericFunction.GenerateMessageSequence("", "AC", "");
                                                    if (msgSeq != null && msgSeq.Tables.Count > 0 && msgSeq.Tables[0].Rows.Count > 0)
                                                    {
                                                        for (int noofMsg = 0; noofMsg < msgSeq.Tables[0].Rows.Count; noofMsg++)
                                                        {
                                                            switch (msgSeq.Tables[0].Rows[noofMsg]["MessageName"].ToString())
                                                            {
                                                                case "FWB":
                                                                    if (isAutoSendFWB)
                                                                    {
                                                                        //FWBMessageProcessor fwbMessageProcessor = new FWBMessageProcessor();
                                                                        //fwbMessageProcessor.GenerateFWB(PartnerCode, fsadata.origin, fsadata.dest, "", flightdate, "FSU/RCS", DateTime.UtcNow, fsadata.airlineprefix + "-" + fsadata.awbnum);
                                                                        _fWBMessageProcessor.GenerateFWB(PartnerCode, fsadata.origin, fsadata.dest, "", flightdate, "FSU/RCS", DateTime.UtcNow, fsadata.airlineprefix + "-" + fsadata.awbnum);
                                                                    }
                                                                    break;
                                                                case "FHL":
                                                                    if (isAutoSendFHL)
                                                                    {
                                                                        //FHLMessageProcessor fhlMessageProcessor = new FHLMessageProcessor();
                                                                        //fhlMessageProcessor.GenerateFHL(PartnerCode, fsadata.origin, fsadata.dest, "", flightdate, "FSU/RCS", DateTime.UtcNow, fsadata.airlineprefix + "-" + fsadata.awbnum);
                                                                        await _fHLMessageProcessor.GenerateFHL(PartnerCode, fsadata.origin, fsadata.dest, "", flightdate, "FSU/RCS", DateTime.UtcNow, fsadata.airlineprefix + "-" + fsadata.awbnum);
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                                #endregion

                                                flag = true;
                                                ///MasterLog
                                                //GenericFunction gf = new GenericFunction();
                                                DataSet dsAWBMaterLogNewValues = new DataSet();
                                                //dsAWBMaterLogNewValues = gf.GetAWBMasterLogNewRecord(fsadata.airlineprefix, fsadata.awbnum);
                                                dsAWBMaterLogNewValues = _genericFunction.GetAWBMasterLogNewRecord(fsadata.airlineprefix, fsadata.awbnum);
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

                                                    if (isAcceptedbyFSURCT == true)
                                                        //gf.MasterAuditLog(dtOldValues, dtNewValues, fsadata.airlineprefix, fsadata.awbnum, "Accepted", "FSU/RCT", System.DateTime.UtcNow);
                                                        _genericFunction.MasterAuditLog(dtOldValues, dtNewValues, fsadata.airlineprefix, fsadata.awbnum, "Accepted", "FSU/RCT", System.DateTime.UtcNow);
                                                    else
                                                        //gf.MasterAuditLog(dtOldValues, dtNewValues, fsadata.airlineprefix, fsadata.awbnum, "Accepted", "FSU/RCS", System.DateTime.UtcNow);
                                                        _genericFunction.MasterAuditLog(dtOldValues, dtNewValues, fsadata.airlineprefix, fsadata.awbnum, "Accepted", "FSU/RCS", System.DateTime.UtcNow);

                                                }
                                            }
                                            else
                                            {
                                                flag = false;
                                            }
                                        }
                                    }
                                    if (flag)
                                    {
                                        flag = true;
                                        #region Capacity Update
                                        //string[] cparam = { "AWBPrefix", "AWBNumber" };
                                        //SqlDbType[] cparamtypes = { SqlDbType.VarChar, SqlDbType.VarChar };
                                        //object[] cparamvalues = { fsadata.airlineprefix, fsadata.awbnum };
                                        var cparam = new SqlParameter[]
                                        {
                                      new("@AWBPrefix", SqlDbType.VarChar) { Value = fsadata.airlineprefix },
                                      new("@AWBNumber", SqlDbType.VarChar) { Value = fsadata.awbnum },

                                        };
                                        //if (!dtb.InsertData("UpdateCapacitythroughMessage", cparam, cparamtypes, cparamvalues))
                                        if (!await _readWriteDao.ExecuteNonQueryAsync("UpdateCapacitythroughMessage", cparam))
                                            // clsLog.WriteLogAzure("Error  on Update capacity Plan :" + awbnum);
                                            _logger.LogWarning("Error  on Update capacity Plan : {0}" , awbnum);

                                        #endregion
                                    }
                                    else
                                    {
                                        flag = false;
                                        //return flag;
                                        return (flag, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Below Segmnet of FSU/TFD Message
                        ///Created By :Badiuz khan
                        ///Created On :2016-05-26
                        ///Make  Transfer  Freight To  other SPA Airline
                        if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && (fsanodes[0].messageprefix.ToUpper() == "TFD" || fsanodes[0].messageprefix.ToUpper() == "RCT"))
                        {
                            string strUpdatedby = "FSU/" + fsanodes[0].messageprefix.ToUpper();
                            string strMessageType = fsanodes[0].messageprefix.ToUpper();

                            if (!isAWBPresent)
                            {
                                //return false;
                                return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                            }

                            if (fsanodes.Length > 0)
                            {
                                for (int i = 0; i < fsanodes.Length; i++)
                                {
                                    DateTime ReceivedDate = new DateTime();
                                    string ReceivedTime = string.Empty;
                                    if (fsanodes[i].fltmonth != "" && fsanodes[i].fltday != "")
                                    {
                                        if (fsanodes[i].fltday.Length > 0 || fsanodes[i].fltmonth.Length > 0)
                                            ReceivedDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
                                        if (fsanodes[i].flttime.Length > 0)
                                            ReceivedTime = fsanodes[i].flttime.Substring(0, 2) + ":" + fsanodes[i].flttime.Substring(2) + ":00";
                                    }

                                    //  string[] sqlParameterName = new string[]
                                    //    {
                                    //       "AWBPrefix",
                                    //       "AWBNo",
                                    //       "Origin",
                                    //       "Destination",
                                    //       "AWbPcs",
                                    //       "AWbWeightCode",
                                    //       "AWbGrossWt" ,
                                    //       "TransferCarrierCode",
                                    //       "ReceivedShipmentDate",
                                    //       "ReceivedOrigin",
                                    //       "PieceCode",
                                    //       "AcceptPieces",
                                    //       "WeightCode",
                                    //       "AcceptedGrWeight",
                                    //       "ReceivedCarrier",
                                    //       "Name",
                                    //       "UpdatedBy",
                                    //       "MessageType",
                                    //       "ManifestNumber"

                                    //    };
                                    //  object[] sqlParameterValue = new object[] {

                                    //      fsadata.airlineprefix,
                                    //      fsadata.awbnum,
                                    //      fsadata.origin,
                                    //      fsadata.dest,

                                    //      int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt),
                                    //      fsadata.weightcode,
                                    //      decimal.Parse(fsadata.weight==""?"0":fsadata.weight),

                                    //      fsanodes[i].carriercode,
                                    //      ReceivedDate,
                                    //      fsanodes[i].fltorg.Trim() == string.Empty ? fsanodes[i].airportcode.Trim() : fsanodes[i].fltorg.Trim(),

                                    //      fsanodes[i].pcsindicator,
                                    //      int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs),
                                    //      fsanodes[i].weightcode,
                                    //      decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight),

                                    //      fsanodes[0].seccarriercode,
                                    //      fsanodes[0].name,
                                    //      strUpdatedby,
                                    //      strMessageType,
                                    //      int.Parse(fsanodes[0].transfermanifestnumber==""?"0":fsanodes[0].transfermanifestnumber)

                                    //  };
                                    //  SqlDbType[] sqlParameter = new SqlDbType[]
                                    //  {
                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.VarChar,

                                    //      SqlDbType.Int,
                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.Decimal,

                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.DateTime,
                                    //      SqlDbType.VarChar,

                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.Int,
                                    //      SqlDbType.VarChar,
                                    //      SqlDbType.Decimal,

                                    //     SqlDbType.VarChar,
                                    //     SqlDbType.VarChar,
                                    //     SqlDbType.VarChar,
                                    //     SqlDbType.VarChar,
                                    //     SqlDbType.Int
                                    //};
                                    var sqlParams = new[]
                                    {
                              new SqlParameter("@AWBPrefix", SqlDbType.VarChar)
                              {
                                  Value = fsadata.airlineprefix
                              },
                              new SqlParameter("@AWBNo", SqlDbType.VarChar)
                              {
                                  Value = fsadata.awbnum
                              },
                              new SqlParameter("@Origin", SqlDbType.VarChar)
                              {
                                  Value = fsadata.origin
                              },
                              new SqlParameter("@Destination", SqlDbType.VarChar)
                              {
                                  Value = fsadata.dest
                              },
                              new SqlParameter("@AWbPcs", SqlDbType.Int)
                              {
                                  Value = int.Parse(fsadata.pcscnt==""?"0":fsadata.pcscnt)
                              },
                              new SqlParameter("@AWbWeightCode", SqlDbType.VarChar)
                              {
                                  Value = fsadata.weightcode
                              },
                              new SqlParameter("@AWbGrossWt", SqlDbType.Decimal)
                              {
                                  Value = decimal.Parse(fsadata.weight==""?"0":fsadata.weight)
                              },
                              new SqlParameter("@TransferCarrierCode", SqlDbType.VarChar)
                              {
                                  Value = fsanodes[i].carriercode
                              },
                              new SqlParameter("@ReceivedShipmentDate", SqlDbType.DateTime)
                              {
                                  Value = ReceivedDate
                              },
                              new SqlParameter("@ReceivedOrigin", SqlDbType.VarChar)
                              {
                                  Value = string.IsNullOrWhiteSpace(fsanodes[i].fltorg)
                                              ? fsanodes[i].airportcode.Trim()
                                              : fsanodes[i].fltorg.Trim()
                              },
                              new SqlParameter("@PieceCode", SqlDbType.VarChar)
                              {
                                  Value = fsanodes[i].pcsindicator
                              },
                              new SqlParameter("@AcceptPieces", SqlDbType.Int)
                              {
                                  Value = int.Parse(fsanodes[i].numofpcs==""?"0":fsanodes[i].numofpcs)
                              },
                              new SqlParameter("@WeightCode", SqlDbType.VarChar)
                              {
                                  Value = fsanodes[i].weightcode
                              },
                              new SqlParameter("@AcceptedGrWeight", SqlDbType.Decimal)
                              {
                                  Value = decimal.Parse(fsanodes[i].weight==""?"0":fsanodes[i].weight)
                              },
                              new SqlParameter("@ReceivedCarrier", SqlDbType.VarChar)
                              {
                                  Value = fsanodes[0].seccarriercode
                              },
                              new SqlParameter("@Name", SqlDbType.VarChar)
                              {
                                  Value = fsanodes[0].name
                              },
                              new SqlParameter("@UpdatedBy", SqlDbType.VarChar)
                              {
                                  Value = strUpdatedby
                              },
                              new SqlParameter("@MessageType", SqlDbType.VarChar)
                              {
                                  Value = strMessageType
                              },
                              new SqlParameter("@ManifestNumber", SqlDbType.Int)
                              {
                                  Value = int.Parse(fsanodes[0].transfermanifestnumber==""?"0":fsanodes[0].transfermanifestnumber)
                              }
                            };

                                    //if (dtb.InsertData("uspSaveAWBThroughFSURCTTFDMessage", sqlParameterName, sqlParameter, sqlParameterValue))
                                    if (await _readWriteDao.ExecuteNonQueryAsync("uspSaveAWBThroughFSURCTTFDMessage", sqlParams))
                                        flag = true;
                                    else
                                    {
                                        flag = false;
                                        //return flag;
                                        return (flag, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                    }


                                }

                            }
                        }


                        #endregion

                        #region  Below Segmnet of RCF Message
                        if (fsadata.awbnum.Length > 0 && fsanodes.Length > 0 && fsanodes[0].messageprefix.ToUpper() == "RCF")
                        {
                            if (AWBAccepted == false)
                            {
                                ErrorMsg = ErrorMsg = fsadata.awbnum + " AWB is not accepted";
                                //return false;
                                return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                            }
                            for (int i = 0; i < fsanodes.Length; i++)
                            {
                                DateTime fltdate = DateTime.Now;
                                fltdate = DateTime.Parse((fsanodes[i].fltmonth + "-" + fsanodes[i].fltday + "-" + DateTime.Now.Year));
                                if (fsanodes[i].messageprefix.ToUpper().Trim() == "RCF")
                                {
                                    //string[] ParaNames = new string[] { "AWBPrefix", "AWBNumber", "Destination", "ConsignmentType", "Pieces", "Weight", "FlighNumber", "ArrivedDate", "WTCode", "RefNo", "FltOrigin", "FltDestination", "FlightDate" };
                                    //SqlDbType[] ParaTypes = new System.Data.SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
                                    //object[] ParaValues = new object[] { fsadata.airlineprefix, fsadata.awbnum, string.Empty, fsanodes[i].pcsindicator, fsanodes[i].numofpcs, (((fsanodes[i].weight).Length == 0) ? "0" : (fsanodes[i].weight)), fsanodes[i].carriercode + fsanodes[i].flightnum, DateTime.Now, fsanodes[i].weightcode, refNo, fsanodes[i].fltorg, fsanodes[i].fltdest, fltdate };
                                    var rcfArrParams = new[]
                                    {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)
                                {
                                    Value = fsadata.airlineprefix
                                },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)
                                {
                                    Value = fsadata.awbnum
                                },
                                new SqlParameter("@Destination", SqlDbType.VarChar)
                                {
                                    Value = string.Empty
                                },
                                new SqlParameter("@ConsignmentType", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].pcsindicator
                                },
                                new SqlParameter("@Pieces", SqlDbType.Int)
                                {
                                    Value = fsanodes[i].numofpcs
                                },
                                new SqlParameter("@Weight", SqlDbType.VarChar)
                                {
                                    Value = (((fsanodes[i].weight).Length == 0) ? "0" : (fsanodes[i].weight))
                                },
                                new SqlParameter("@FlighNumber", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].carriercode + fsanodes[i].flightnum
                                },
                                new SqlParameter("@ArrivedDate", SqlDbType.DateTime)
                                {
                                    Value = DateTime.Now
                                },
                                new SqlParameter("@WTCode", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].weightcode
                                },
                                new SqlParameter("@RefNo", SqlDbType.Int)
                                {
                                    Value = refNo
                                },
                                new SqlParameter("@FltOrigin", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].fltorg
                                },
                                new SqlParameter("@FltDestination", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].fltdest
                                },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime)
                                {
                                    Value = fltdate
                                }
                            };
                                    DataSet dsRCFARR = new DataSet();
                                    //dsRCFARR = dtb.SelectRecords("USPUpdateRCForARRRecord", ParaNames, ParaValues, ParaTypes);
                                    dsRCFARR = await _readWriteDao.SelectRecords("USPUpdateRCForARRRecord", rcfArrParams);

                                    if (dsRCFARR != null && dsRCFARR.Tables.Count > 0)
                                    {
                                        for (int j = 0; j < dsRCFARR.Tables.Count; j++)
                                        {
                                            if ((dsRCFARR.Tables[j].Columns.Contains("IsFlightDeparted") && dsRCFARR.Tables[j].Rows[0]["IsFlightDeparted"].ToString().ToUpper() == "FALSE"))
                                            {
                                                ErrorMsg = "Flight is Not Departed";
                                                //return false;
                                                return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                            }
                                        }
                                    }

                                    if (dsRCFARR != null && dsRCFARR.Tables.Count > 0)
                                    {
                                        for (int j = 0; j < dsRCFARR.Tables.Count; j++)
                                        {
                                            if ((dsRCFARR.Tables[j].Columns.Contains("ResultStatus") && dsRCFARR.Tables[j].Rows[0]["ResultStatus"].ToString().ToUpper() == "FALSE"))
                                            {
                                                ErrorMsg = "Invalid Flight Details";
                                                //return false;
                                                return (false, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                            }
                                        }
                                    }

                                    if (dsRCFARR != null && dsRCFARR.Tables.Count > 0)
                                    {
                                        for (int j = 0; j < dsRCFARR.Tables.Count; j++)
                                        {
                                            if ((dsRCFARR.Tables[j].Columns.Contains("IsFlightDeparted") && dsRCFARR.Tables[j].Rows[0]["IsFlightDeparted"].ToString().ToUpper() == "FALSE")
                                                || (dsRCFARR.Tables[j].Columns.Contains("ResultStatus") && dsRCFARR.Tables[j].Rows[0]["ResultStatus"].ToString().ToUpper() == "FALSE"))
                                                //return true;
                                                return (true, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);
                                        }

                                    }
                                }
                            }
                        }
                        #endregion Below Segmnet of RCF Message

                        //dtb = new SQLServer();
                        if (fsadata.awbnum.Length > 0)
                        {
                            awbnum = fsadata.awbnum;
                            awbprefix = fsadata.airlineprefix;
                            strMsg = strMsg.Replace("\r\n", "$");
                            strMsg = strMsg.Replace("\n", "$");
                            string[] splitStr = strMsg.Split('$');
                            string date = "";
                            //string time = "";

                            string FlightNo = string.Empty, UOM = string.Empty, UpdatedBy = "FSU", StnCode = string.Empty;
                            DateTime FlightDate = DateTime.Now, UpdatedON = DateTime.Now, DayChange = Convert.ToDateTime("1900-01-01");
                            string strDepartureTime = string.Empty;
                            int PCS = 0;
                            decimal Wt = 0;

                            if (splitStr.Length > 1 && fsanodes.Length > 1 && ulddata.Length > 0)
                            {
                                int depTotalPieces = 0;
                                decimal depTotalWeight = 0;
                                bool isMultilineDEP = false;
                                int depIndex = 0;
                                for (int i = 0; i < fsanodes.Length; i++)
                                {
                                    switch (fsanodes[i].messageprefix.ToUpper())
                                    {
                                        case "DEP":
                                            if (fsanodes[i].pcsindicator.Trim().ToUpper() == "P")
                                            {
                                                string depTpcs = fsanodes[i].numofpcs.Trim();
                                                int depTotalPiecesTmp = 0;
                                                if (int.TryParse(depTpcs, out depTotalPiecesTmp))
                                                {
                                                    depTotalPieces += depTotalPiecesTmp;
                                                }
                                                string depTwt = fsanodes[i].weight.Trim();
                                                decimal depTotalWeightTmp = 0;
                                                if (decimal.TryParse(depTwt, out depTotalWeightTmp))
                                                {
                                                    depTotalWeight += depTotalWeightTmp;
                                                }

                                                isMultilineDEP = true;
                                                if (depIndex == 0)
                                                {
                                                    depIndex = i + 1;
                                                }
                                                else
                                                {
                                                    fsanodes[i].pcsindicator = "X";
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                if (isMultilineDEP)
                                {
                                    fsanodes[depIndex - 1].numofpcs = Convert.ToString(depTotalPieces);
                                    if (depTotalWeight > 0)
                                        fsanodes[depIndex - 1].weight = Convert.ToString(depTotalWeight);
                                }
                            }

                            #region UpdateStatus on TblAwbMsg
                            if (splitStr.Length > 1 && fsanodes.Length > 0)
                            {
                                for (int i = 0; i < fsanodes.Length; i++)
                                {
                                    if (fsanodes[i].pcsindicator.Trim().ToUpper() == "X")
                                        continue;
                                    if (fsanodes[i].fltday.Length > 0 || fsanodes[i].fltmonth.Length > 0)
                                    {
                                        date = fsanodes[i].fltday + "/" + DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + System.DateTime.Today.Year;
                                        FlightDate = DateTime.Parse(DateTime.Parse("1." + fsanodes[i].fltmonth + " 2008").Month + "/" + fsanodes[i].fltday + "/" + System.DateTime.Today.Year);
                                    }
                                    else
                                    {
                                        FlightDate = DateTime.Now;
                                    }

                                    if (fsanodes[i].messageprefix.ToUpper() == "RCF")
                                    {
                                        if (fsanodes[i].daychangeindicator != null && fsanodes[i].daychangeindicator.Trim() != string.Empty)
                                        {
                                            switch (fsanodes[i].daychangeindicator.Trim().ToUpper())
                                            {
                                                case "P":
                                                    DayChange = FlightDate.AddDays(-1);
                                                    break;
                                                case "N":
                                                    DayChange = FlightDate.AddDays(1);
                                                    break;
                                                case "S":
                                                    DayChange = FlightDate.AddDays(2);
                                                    break;
                                                case "T":
                                                    DayChange = FlightDate.AddDays(3);
                                                    break;
                                                case "A":
                                                    DayChange = FlightDate.AddDays(4);
                                                    break;
                                                case "B":
                                                    DayChange = FlightDate.AddDays(5);
                                                    break;
                                                case "C":
                                                    DayChange = FlightDate.AddDays(6);
                                                    break;
                                                case "D":
                                                    DayChange = FlightDate.AddDays(7);
                                                    break;
                                                case "E":
                                                    DayChange = FlightDate.AddDays(8);
                                                    break;
                                                case "F":
                                                    DayChange = FlightDate.AddDays(9);
                                                    break;
                                                case "G":
                                                    DayChange = FlightDate.AddDays(10);
                                                    break;
                                                case "H":
                                                    DayChange = FlightDate.AddDays(11);
                                                    break;
                                                case "I":
                                                    DayChange = FlightDate.AddDays(12);
                                                    break;
                                                case "J":
                                                    DayChange = FlightDate.AddDays(13);
                                                    break;
                                                case "K":
                                                    DayChange = FlightDate.AddDays(14);
                                                    break;
                                                case "L":
                                                    DayChange = FlightDate.AddDays(15);
                                                    break;
                                                default:
                                                    break;

                                            }

                                            date = DayChange.ToString("dd/MM/yyyy");
                                        }
                                    }
                                    //if (fsanodes[i].flttime.Length > 0)
                                    //{
                                    //    time = fsanodes[i].flttime + " (UTC)";
                                    //}

                                    // time= 
                                    FlightNo = fsanodes[i].carriercode + fsanodes[i].flightnum;
                                    if (FlightNo.Trim().Length > 0)
                                    {
                                        FlightNo = FlightNo.Replace(".", "").Replace(",", "").Replace("XX", "");
                                    }

                                    UOM = fsanodes[i].weightcode == "" ? "K" : fsanodes[i].weightcode;

                                    if (fsanodes[i].airportcode != "")
                                        StnCode = fsanodes[i].airportcode;
                                    if (fsanodes[i].fltorg != "")
                                        StnCode = fsanodes[i].fltorg;
                                    if (fsanodes[i].fltdest != "")
                                        StnCode = fsanodes[i].fltdest;
                                    if (fsanodes[i].fltdest != "" && fsanodes[i].fltorg != "")
                                        StnCode = fsanodes[i].fltorg;

                                    if (fsanodes[i].numofpcs != "")
                                        PCS = Convert.ToInt16(fsanodes[i].numofpcs);
                                    if (fsanodes[i].weight != "")
                                        Wt = Convert.ToDecimal(fsanodes[i].weight);

                                    //string[] PName = new string[] { "AWBPrefix", "AWBNumber", "MType", "desc", "date", "time", "refno",
                                    //    "FlightNo","FlightDate","PCS","WT","UOM","UpdatedBy","UpdatedOn","StnCode"};
                                    //object[] PValues = new object[] { awbprefix, awbnum, fsanodes[i].messageprefix, splitStr[2 + i], date, fsanodes[i].flttime, 0,
                                    //FlightNo,FlightDate,PCS,Wt,UOM,UpdatedBy,UpdatedON,StnCode};
                                    //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int,
                                    //SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.Int,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.DateTime,SqlDbType.VarChar};
                                    var messageParams = new[]
                                    {
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)
                                {
                                    Value = awbprefix
                                },
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)
                                {
                                    Value = awbnum
                                },
                                new SqlParameter("@MType", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].messageprefix
                                },
                                new SqlParameter("@desc", SqlDbType.VarChar)
                                {
                                    Value = splitStr[2 + i]
                                },
                                new SqlParameter("@date", SqlDbType.VarChar)
                                {
                                    Value = date
                                },
                                new SqlParameter("@time", SqlDbType.VarChar)
                                {
                                    Value = fsanodes[i].flttime
                                },
                                new SqlParameter("@refno", SqlDbType.Int)
                                {
                                    Value = 0
                                },
                                new SqlParameter("@FlightNo", SqlDbType.VarChar)
                                {
                                    Value = FlightNo
                                },
                                new SqlParameter("@FlightDate", SqlDbType.DateTime)
                                {
                                    Value = FlightDate
                                },
                                new SqlParameter("@PCS", SqlDbType.Int)
                                {
                                    Value = PCS
                                },
                                new SqlParameter("@WT", SqlDbType.Decimal)
                                {
                                    Value = Wt
                                },
                                new SqlParameter("@UOM", SqlDbType.VarChar)
                                {
                                    Value = UOM
                                },
                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)
                                {
                                    Value = UpdatedBy
                                },
                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime)
                                {
                                    Value = UpdatedON
                                },
                                new SqlParameter("@StnCode", SqlDbType.VarChar)
                                {
                                    Value = StnCode
                                }
                            };
                                    //if (dtb.InsertData("spInsertAWBMessageStatus", PName, PType, PValues))
                                    if (await _readWriteDao.ExecuteNonQueryAsync("spInsertAWBMessageStatus", messageParams))
                                        if (fsanodes[i].messageprefix == "DEP" || fsanodes[i].messageprefix == "MAN" || fsanodes[i].messageprefix == "OCI")
                                        {
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

                                                    for (int j = 0; j < customextrainfo.Length; j++)
                                                    {
                                                        DataRow drawb = dtCustom.NewRow();

                                                        drawb["MRNNumber"] = "";
                                                        drawb["MsgType"] = "";
                                                        drawb["Country"] = customextrainfo[j].IsoCountryCodeOci;
                                                        drawb["DataID"] = customextrainfo[j].InformationIdentifierOci;
                                                        drawb["DataValue"] = customextrainfo[j].SupplementaryCsrIdentifierOci;
                                                        drawb["FlightNo"] = "";
                                                        drawb["FlightDate"] = System.DateTime.Now;
                                                        drawb["Info"] = customextrainfo[j].CsrIdentifierOci;
                                                        drawb["ProcessedBy"] = "FSU";
                                                        drawb["ProcessedDate"] = System.DateTime.Now;
                                                        drawb["Custom"] = "";
                                                        drawb["IsActive"] = "1";
                                                        drawb["HAWBNumber"] = "";
                                                        drawb["OCILine"] = customextrainfo[j].OCIInfo;

                                                        dtCustom.Rows.Add(drawb);
                                                    }

                                                    //    SqlParameter[] sqlParameters = new SqlParameter[] {
                                                    //  new SqlParameter("AWBPrefix",fsadata.airlineprefix)
                                                    //, new SqlParameter("AWBNumber",fsadata.awbnum)
                                                    //, new SqlParameter("Custom", dtCustom)
                                                    // };
                                                    var paramsSql = new[]
                                                    {
                                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar)
                                            {
                                                Value = fsadata.airlineprefix
                                            },
                                            new SqlParameter("@AWBNumber", SqlDbType.VarChar)
                                            {
                                                Value = fsadata.awbnum
                                            },
                                            new SqlParameter("@Custom", SqlDbType.Structured)
                                            {
                                                Value = dtCustom
                                            }
                                           };
                                                    //dtb.SelectRecords("Messaging.uspGetSetOCIDetails", sqlParameters);
                                                    await _readWriteDao.SelectRecords("Messaging.uspGetSetOCIDetails", paramsSql);

                                                }
                                                catch
                                                {
                                                    // clsLog.WriteLogAzure("Error while save FSU OCI information Message:" + awbnum);
                                                    _logger.LogError("Error while save FSU OCI information Message: {0}" , awbnum);
                                                }
                                            }
                                        }
                                    flag = true;

                                }
                            }
                            #endregion

                            #region Save AWB Record on Audit log Table
                            if (fsanodes.Length > 0)
                            {
                                for (int k = 0; k < fsanodes.Length; k++)
                                {
                                    if (fsanodes[k].pcsindicator.Trim().ToUpper() == "X")
                                        continue;
                                    //Updated On:2017-03-24
                                    //Updated By:Shrishail Ashtage
                                    //Description:Save AWB Record in AWB OperationAuditlog Table
                                    string messageStatus = string.Empty, strDescription = string.Empty, strAction = string.Empty, Updatedby = string.Empty;
                                    switch (fsanodes[k].messageprefix.ToUpper())
                                    {
                                        case "BKD":
                                            messageStatus = "BKD-AWB Booked";
                                            strDescription = "AWB Booked";
                                            strAction = "Booked";
                                            Updatedby = "FSU-BKD";
                                            break;
                                        case "RCS":
                                            messageStatus = "RCS-AWB HandedOver";
                                            strDescription = "AWB Accepted.";
                                            strAction = "Accepted";
                                            Updatedby = "FSU-RCS";
                                            break;
                                        case "MAN":
                                            messageStatus = "MAN-AWB Manifested";
                                            strDescription = "AWB Manifested.";
                                            strAction = "Manifested";
                                            Updatedby = "FSU-MAN";
                                            break;
                                        case "DEP":
                                            messageStatus = "DEP-AWB Departed";
                                            strDescription = "AWB Departed";
                                            strAction = "Departed";
                                            Updatedby = "FSU-DEP";
                                            break;
                                        case "DIS":
                                            messageStatus = "DIS-AWB Discrepancy";
                                            Updatedby = "FSU-DIS";
                                            switch (fsanodes[k].infocode.ToUpper())
                                            {
                                                case "FDAW":
                                                    strDescription = "Found AWB";
                                                    strAction = "Found AWB";
                                                    break;
                                                case "FDCA":
                                                    strDescription = "Found Cargo";
                                                    strAction = "Found Cargo";
                                                    break;
                                                case "MSAW":
                                                    strDescription = "Missing AWB";
                                                    strAction = "Missing AWB";
                                                    break;
                                                case "MSCA":
                                                    strDescription = "Missing Cargo";
                                                    strAction = "Missing Cargo";
                                                    break;
                                                case "FDAV":
                                                    strDescription = "Found Mail Document";
                                                    strAction = "Found Mail Document";
                                                    break;
                                                case "FDMB":
                                                    strDescription = "Found Mailbag";
                                                    strAction = "Found Mailbag";
                                                    break;
                                                case "MSAV":
                                                    strDescription = "Missing Mail Document";
                                                    strAction = "Missing Mail Document";
                                                    break;
                                                case "MSMB":
                                                    strDescription = "Missing Mailbag";
                                                    strAction = "Missing Mailbag";
                                                    break;
                                                case "DFLD":
                                                    strDescription = "Definitely Loaded";
                                                    strAction = "Definitely Loaded";
                                                    break;
                                                case "OFLD":
                                                    strDescription = "AWB Offloaded";
                                                    strAction = "Offloaded";
                                                    break;
                                                case "OVCD":
                                                    strDescription = "Overcarried";
                                                    strAction = "Overcarried";
                                                    break;
                                                case "SSPD":
                                                    strDescription = "Shortshipped";
                                                    strAction = "Shortshipped";
                                                    break;
                                                case "DMGD":
                                                    strDescription = "Damage";
                                                    strAction = "Damage";
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        case "ARR":
                                            messageStatus = "ARR-AWB Arrived";
                                            strDescription = "AWB Arrived";
                                            strAction = "Arrived";
                                            Updatedby = "FSU-ARR";
                                            break;
                                        case "RCF":
                                            messageStatus = "RCF-AWB received";
                                            strDescription = "AWB received from a given flight";
                                            strAction = "Arrived";
                                            Updatedby = "FSU-RCF";
                                            break;
                                        case "DLV":
                                            messageStatus = "DLV-AWB Delivered";
                                            strDescription = "AWB Delivered";
                                            strAction = "Delivered";
                                            Updatedby = "FSU-DLV";
                                            break;
                                        case "RCT":
                                            messageStatus = "RCT-Received Freight from Interline Partner(CTM-In generated)";
                                            strDescription = "Received Freight from Interline Partner";
                                            //strAction = "Accepted"; 
                                            strAction = "CTM-In";
                                            Updatedby = "FSU-RCT";
                                            break;
                                        case "TFD":
                                            messageStatus = "TFD-Transfer  Freight To  Interline Airline(CTM-Out generated)";
                                            strDescription = "Transfer  Freight To  Interline Airline";
                                            strAction = "CTM-Out";
                                            Updatedby = "FSU-TFD";
                                            break;
                                        case "NFD":
                                            messageStatus = "Consignment arrived at destination";
                                            strDescription = "Consignment where consignee or his agent has been informed of its arrival at destination";
                                            strAction = "Notify to Consignee";
                                            Updatedby = "FSU-NFD";
                                            break;
                                        case "AWD":
                                            messageStatus = "Documents delivered to the consignee or agent";
                                            strDescription = "Consignment arrival documents delivered to the consignee or agent";
                                            strAction = "Documents delivered";
                                            Updatedby = "FSU-AWD";
                                            break;
                                        case "PRE":
                                            messageStatus = "The consignment has been prepared";
                                            strDescription = "The consignment has been prepared for loading on this flight for transport";// between these locations on this scheduled date";
                                            strAction = "Consignment Prepared";
                                            Updatedby = "FSU-PRE";
                                            break;
                                        case "CCD":
                                            messageStatus = "Consignment cleared by Customs";
                                            strDescription = "Consignment cleared by Customs";
                                            strAction = "Consignment cleared";
                                            Updatedby = "FSU-CCD";
                                            break;
                                        default:
                                            messageStatus = "FSU";
                                            strDescription = "FSU";
                                            strAction = "FSU";
                                            Updatedby = "FSU";
                                            break;
                                    }

                                    string origin = (fsanodes[k].fltorg).ToString();
                                    string dest = (fsanodes[k].fltdest).ToString();
                                    string fltno = (fsanodes[k].carriercode + fsanodes[k].flightnum).ToString();
                                    if (fltno.Trim().Length > 0)
                                    {
                                        fltno = fltno.Replace(".", "").Replace(",", "").Replace("XX", "");
                                    }
                                    string airportcode = (fsanodes[k].airportcode).ToString();
                                    string messageType = fsanodes[k].messageprefix.ToUpper();
                                    DateTime Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));
                                    DateTime fltdate = DateTime.Now;

                                    switch (messageType.Trim())
                                    {
                                        case "DLV":
                                        case "ARR":
                                        case "RCF":
                                        case "MAN":
                                            airportcode = dest;
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));//Convert.ToDateTime("1900-01-01");
                                            break;
                                        case "RCS":
                                            origin = airportcode;
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            Date = Convert.ToDateTime("1900-01-01");
                                            fsanodes[k].weight = (fsanodes[k].weight.Trim() == "" || fsanodes[k].weight == "0") ? fsadata.weight : fsanodes[k].weight;
                                            break;
                                        case "TFD":
                                            origin = fsanodes[k].airportcode;
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            //Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            break;
                                        case "DIS":
                                            origin = fsanodes[k].airportcode;
                                            dest = fsadata.dest;
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));
                                            break;
                                        case "RCT":
                                            origin = string.Empty;
                                            dest = fsadata.dest;
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            //Date = Convert.ToDateTime("1900-01-01");
                                            break;
                                        case "NFD":
                                        case "AWD":
                                        case "CCD":
                                            origin = string.Empty;
                                            dest = fsadata.dest;
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            Date = Convert.ToDateTime("1900-01-01");
                                            break;
                                        case "DEP":
                                            airportcode = Convert.ToString(fsanodes[k].airportcode);
                                            origin = Convert.ToString(fsanodes[k].fltorg);
                                            dest = Convert.ToString(fsanodes[k].fltdest);
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            strDepartureTime = UpdatedON.ToString("MM/dd/yyyy HH:mm");
                                            Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));
                                            break;
                                        default:
                                            airportcode = Convert.ToString(fsanodes[k].airportcode);
                                            origin = Convert.ToString(fsanodes[k].fltorg);
                                            dest = Convert.ToString(fsanodes[k].fltdest);
                                            UpdatedON = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year + " " + Convert.ToString(fsanodes[k].flttime.Contains("(UTC)") == true ? fsanodes[k].flttime.Substring(0, 8) : fsanodes[k].flttime)));
                                            Date = DateTime.Parse((fsanodes[k].fltmonth + "-" + fsanodes[k].fltday + "-" + DateTime.Now.Year));
                                            break;
                                    }

                                    if (messageType == "RCF" && DayChange != Convert.ToDateTime("1900-01-01"))
                                    {
                                        UpdatedON = DateTime.Parse(DayChange.ToString("MM/dd/yyyy") + " " + (fsanodes[k].flttime));
                                    }

                                    if (fsanodes[k].numofpcs == fsadata.pcscnt && (fsanodes[k].weight == "" || fsanodes[k].weight == "0" || fsanodes[k].weight == "0.00"))
                                    {
                                        fsanodes[k].weight = fsadata.weight;
                                    }



                                    //dtb = new SQLServer();
                                    //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "Shipmenttype", "Volume", "RefNo" };
                                    //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int };
                                    //object[] CValues = new object[] { awbprefix, awbnum, fsadata.origin, fsadata.dest, fsanodes[k].numofpcs, Convert.ToDouble(fsanodes[k].weight == "" ? "0" : fsanodes[k].weight), fltno, Date, origin, dest, strAction, messageStatus, strDescription, Updatedby, strDepartureTime == "" ? UpdatedON.ToString() : strDepartureTime, 1, fsanodes[k].pcsindicator, fsanodes[k].volumeamt.Trim() == "" ? "0" : fsanodes[k].volumeamt, refNo };
                                    var cnParams = new[]
                                    {
                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbprefix },
                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = awbnum },
                                        new SqlParameter("@Origin", SqlDbType.VarChar) { Value = fsadata.origin },
                                        new SqlParameter("@Destination", SqlDbType.VarChar) { Value = fsadata.dest },
                                        new SqlParameter("@Pieces", SqlDbType.VarChar)
                                        {
                                            Value = fsanodes[k].numofpcs
                                        },
                                        new SqlParameter("@Weight", SqlDbType.VarChar)
                                        {
                                            Value = Convert.ToDouble(fsanodes[k].weight == "" ? "0" : fsanodes[k].weight)
                                        },
                                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = fltno },
                                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = Date },
                                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = origin },
                                        new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = dest },
                                        new SqlParameter("@Action", SqlDbType.VarChar) { Value = strAction },
                                        new SqlParameter("@Message", SqlDbType.VarChar) { Value = messageStatus },
                                        new SqlParameter("@Description", SqlDbType.VarChar) { Value = strDescription },
                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = Updatedby },
                                        new SqlParameter("@UpdatedOn", SqlDbType.VarChar)
                                        {
                                            Value = string.IsNullOrWhiteSpace(strDepartureTime)
                                                ? UpdatedON.ToString()
                                                : strDepartureTime
                                        },
                                        new SqlParameter("@Public", SqlDbType.Bit) { Value = 1 },
                                        new SqlParameter("@ShipmentType", SqlDbType.VarChar) { Value = fsanodes[k].pcsindicator },
                                        new SqlParameter("@Volume", SqlDbType.VarChar)
                                        {
                                            Value = fsanodes[k].volumeamt.Trim() == "" ? "0" : fsanodes[k].volumeamt
                                        },
                                        new SqlParameter("@RefNo", SqlDbType.Int) { Value = refNo }
                                    };
                                    //if (!dtb.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", cnParams))
                                    {
                                        //clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine + "Error: " + dtb.LastErrorDescription);
                                        //Fix the error dtb.LastErrorDescription while logging, currently removed + dtb.LastErrorDescription  , needs to add
                                        // clsLog.WriteLog("AWB Audit log  for:" + awbnum + Environment.NewLine + "Error: ");
                                        _logger.LogWarning("AWB Audit log  for: {0}", awbnum + Environment.NewLine + "Error: ");

                                    }
                                }
                            }
                            #endregion Save AWB Record on Audit log Table
                        }
                    }
                }
                catch (Exception ex)
                {
                    flag = false;
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on SaveandUpdateFSUMessage");
                }
                //return flag;
                return (flag, ErrorMsg, fsadata, fsanodes, customextrainfo, ulddata, othinfoarray);

            }
        }

        public static string[] stringsplitter(string str)
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
            catch (Exception ex)
            {
                strarr = null;
                // clsLog.WriteLogAzure(ex);
                _staticLogger?.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return strarr;
        }
        #endregion Public Methods
    }
}
