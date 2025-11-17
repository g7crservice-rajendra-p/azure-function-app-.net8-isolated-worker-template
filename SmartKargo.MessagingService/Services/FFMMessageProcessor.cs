#region FFM Message Processor Class Description
/* FFMMessage Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              : 
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
using static Google.Protobuf.Reflection.FieldOptions.Types;
using System.Xml.Linq;
using static QidWorkerRole.MessageData;
using System.ComponentModel.DataAnnotations;

namespace QidWorkerRole
{
    public class FFMMessageProcessor
    {
        #region :: Variable Declaration ::
        static string unloadingportsequence = "";
        static string uldsequencenum = "";
        static string awbref = "";
        #endregion Variable Declaration

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FFMMessageProcessor> _logger;
        private readonly FNAMessageProcessor _fNAMessageProcessor;
        private readonly FHLMessageProcessor _fHLMessageProcessor;
        private readonly cls_SCMBL _cls_SCMBL;
        private readonly CustomsMessageProcessor _customsMessageProcessor;
        private readonly FWBMessageProcessor _fWBMessageProcessor;

        #region Constructor
        public FFMMessageProcessor(
            ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FFMMessageProcessor> logger,
            FNAMessageProcessor fNAMessageProcessor,
            FHLMessageProcessor fHLMessageProcessor,
            cls_SCMBL cls_SCMBL,
            CustomsMessageProcessor customsMessageProcessor,
            FWBMessageProcessor fWBMessageProcessor
            )
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _fNAMessageProcessor = fNAMessageProcessor;
            _fHLMessageProcessor = fHLMessageProcessor;
            _cls_SCMBL = cls_SCMBL;
            _customsMessageProcessor = customsMessageProcessor;
            _fWBMessageProcessor = fWBMessageProcessor;
        }
        #endregion

        #region :: Public Methods ::
        /// <summary>
        /// Decode FFM message to get the AWB Consignment, Route etc. Information
        /// </summary>
        /// <param name="RefNo">Message SrNo from tblInbox</param>
        /// <param name="ffmmsg">FFM Message</param>
        /// <param name="ffmdata">Array contains flight information</param>
        /// <param name="unloadingport">Array contains unloading port</param>
        /// <param name="consinfo">Array contains consignment inforamtion</param>
        /// <param name="dimensioinfo">Array contains AWB dimensions</param>
        /// <param name="uld">Array contains ULD details</param>
        /// <param name="othinfoarray">Array contains OSI information</param>
        /// <param name="custominfo">Array contains custom information</param>
        /// <param name="movementinfo">Array contains  flight movement information i.e. flight number, O&D and date etc.</param>
        /// <returns>Returns true if message is decoded successfully</returns>
        //public bool DecodeReceiveFFMMessage(int RefNo, string ffmmsg, ref MessageData.ffminfo ffmdata, ref MessageData.unloadingport[] unloadingport, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.dimensionnfo[] dimensioinfo, ref MessageData.ULDinfo[] uld, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.customsextrainfo[] custominfo, ref MessageData.movementinfo[] movementinfo, out string errorMessage)
        public async Task<(
            bool success,
            ffminfo ffmdata,
            unloadingport[] unloadingPort,
            consignmnetinfo[] consinfo,
            dimensionnfo[] dimensioinfo,
            ULDinfo[] uld,
            otherserviceinfo[] othinfoarray,
            customsextrainfo[] custominfo,
            movementinfo[] movementInfo,
            string errorMessage)> 
            DecodeReceiveFFMMessage(
            int RefNo, 
            string ffmmsg, 
            ffminfo ffmdata, 
            unloadingport[] unloadingPort, 
            consignmnetinfo[] consinfo, 
            dimensionnfo[] dimensioinfo, 
            ULDinfo[] uld, 
            otherserviceinfo[] othinfoarray, 
            customsextrainfo[] custominfo, 
            movementinfo[] movementInfo,
            string errorMessage)
        {
            errorMessage = string.Empty;
            bool flag = false;
            bool isConsignmentLineValid = true;
            try
            {
                string lastrec = "NA";
                uldsequencenum = "";
                unloadingportsequence = "";
                string AWBPrefix = "", AWBNumber = "";
                try
                {
                    if (ffmmsg.StartsWith("FFM", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] str = ffmmsg.Trim().Split('$');
                        if (str.Length > 3)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                if (!isConsignmentLineValid && !str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase) && !str[i].Split('/')[0].Contains('-'))
                                    continue;
                                else
                                    isConsignmentLineValid = true;

                                if (str[i].StartsWith("CONT", StringComparison.OrdinalIgnoreCase) || str[i].StartsWith("LAST", StringComparison.OrdinalIgnoreCase))
                                {
                                    ffmdata.endmesgcode = str[i].Trim();
                                    i = str.Length + 1;
                                    //return (flag);
                                    return ( flag, ffmdata, unloadingPort, consinfo, dimensioinfo, uld, othinfoarray, custominfo, movementInfo, errorMessage);
                                }
                                flag = true;
                                if (str[str.Length - 1].ToUpper() != "LAST" && str[str.Length - 1].ToUpper() != "CONT")
                                {
                                    errorMessage = "Invalid Message Format";
                                    //return false;
                                    return (flag,ffmdata,unloadingPort,consinfo, dimensioinfo, uld, othinfoarray, custominfo,movementInfo, errorMessage);
                                     }
                                #region Line 1
                                if (str[i].StartsWith("FFM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        if (i > 0)
                                        {
                                            errorMessage = "Invalid Message Format";//Error #6
                                            //return false;
                                            return (
                                                flag, ffmdata, unloadingPort, consinfo, dimensioinfo, uld, othinfoarray, custominfo, movementInfo, errorMessage);

                                        }
                                        string[] msg = str[i].Split('/');
                                        ffmdata.ffmversionnum = msg[1];
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 2 flight data
                                if (i == 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 1)
                                        {
                                            int ffmSeqNumber = 0;
                                            if (Int32.TryParse(msg[0].Trim(), out ffmSeqNumber))
                                            {
                                                if (ffmSeqNumber > 0 && ffmSeqNumber < 100)
                                                {
                                                    ffmdata.messagesequencenum = msg[0];
                                                    ffmdata.carriercode = msg[1].Substring(0, 2);
                                                    ffmdata.fltnum = msg[1].Substring(2);
                                                    ffmdata.fltdate = msg[2].Substring(0, 2);
                                                    ffmdata.month = msg[2].Substring(2, 3);
                                                    ffmdata.time = msg[2].Substring(5).Length > 0 ? msg[2].Substring(5) : "";
                                                    ffmdata.fltairportcode = msg[3];
                                                    if (ffmdata.fltairportcode.Trim().Length != 3 || ffmdata.fltairportcode.Trim().Any(char.IsDigit))
                                                    {
                                                        errorMessage = "Invalid airport code";
                                                        //return false;
                                                        return (flag, ffmdata, unloadingPort, consinfo, dimensioinfo, uld, othinfoarray, custominfo, movementInfo, errorMessage);

                                                    }
                                                    if (msg.Length > 4)
                                                        ffmdata.aircraftregistration = msg[4];
                                                    if (msg.Length > 5)
                                                        ffmdata.countrycode = msg[5];
                                                    if (msg.Length > 6)
                                                    {
                                                        ffmdata.fltdate1 = msg[6].Substring(0, 2);
                                                        ffmdata.fltmonth1 = msg[6].Substring(2, 3);
                                                        ffmdata.flttime1 = msg[6].Substring(5);
                                                    }
                                                    if (msg.Length > 7)
                                                        ffmdata.fltairportcode1 = msg[7];
                                                }
                                                else
                                                {
                                                    errorMessage = "Invalid Message Format";//Error #3
                                                    //return false;
                                                    return (flag, ffmdata, unloadingPort, consinfo, dimensioinfo, uld, othinfoarray, custominfo, movementInfo, errorMessage);
                                                }
                                            }
                                            else
                                            {
                                                errorMessage = "Invalid Message Format";//Error #3
                                                //return false;
                                                return (flag, ffmdata, unloadingPort, consinfo, dimensioinfo, uld, othinfoarray, custominfo, movementInfo, errorMessage);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region line 3 point of unloading
                                if (i >= 2)
                                {
                                    MessageData.unloadingport unloading = new MessageData.unloadingport("");
                                    if (str[i].Contains('/') && (!str[i].StartsWith("/")) && (!str[i].Contains("-")))
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length >= 2)
                                        {
                                            if (msg[0].Length > 0 && !msg[0].StartsWith("/") && !msg[0].Contains('-')
                                                && !msg[0].Equals("SSR", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("SCI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("OSI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("ULD", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("COR", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("OCI", StringComparison.OrdinalIgnoreCase)
                                                && !msg[0].Equals("DIM", StringComparison.OrdinalIgnoreCase))
                                            {
                                                unloading.unloadingairport = msg[0].Trim();
                                                if (msg[1].Length == 3 && msg[1].ToUpper() == "NIL")
                                                    unloading.nilcargocode = msg[1];
                                                try
                                                {
                                                    if (msg.Length > 2)
                                                    {
                                                        unloading.day = msg[2].Substring(0, 2);
                                                        unloading.month = msg[2].Substring(2, 3);
                                                        unloading.time = msg[2].Substring(5);
                                                    }
                                                    if (msg.Length > 3)
                                                    {
                                                        unloading.day1 = msg[3].Substring(0, 2);
                                                        unloading.month1 = msg[3].Substring(2, 3);
                                                        unloading.time1 = msg[3].Substring(5);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref unloadingPort, unloadingPort.Length + 1);
                                                unloadingPort[unloadingPort.Length - 1] = unloading;
                                                //for sequence app
                                                unloadingPort[unloadingPort.Length - 1].sequencenum = unloadingPort.Length.ToString();
                                                unloadingportsequence = unloadingPort.Length.ToString();
                                                uldsequencenum = "";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (str[i].Trim().Length == 3 && (!str[i].Contains("-")) && (!str[i].Contains("/")))
                                        {
                                            unloading.unloadingairport = str[i].Trim();
                                            Array.Resize(ref unloadingPort, unloadingPort.Length + 1);
                                            unloadingPort[unloadingPort.Length - 1] = unloading;
                                            //for sequence app
                                            unloadingPort[unloadingPort.Length - 1].sequencenum = unloadingPort.Length.ToString();
                                            unloadingportsequence = unloadingPort.Length.ToString();
                                            uldsequencenum = "";
                                        }
                                        if (str[i].Trim().Length == 7 && str[i].ToUpper().Contains("NIL"))
                                        {
                                            unloading.unloadingairport = str[i].Trim();
                                            Array.Resize(ref unloadingPort, unloadingPort.Length + 1);
                                            unloadingPort[unloadingPort.Length - 1] = unloading;
                                            //for sequence app
                                            unloadingPort[unloadingPort.Length - 1].sequencenum = unloadingPort.Length.ToString();
                                            unloadingportsequence = unloadingPort.Length.ToString();
                                            uldsequencenum = "";
                                        }
                                    }
                                }
                                #endregion

                                #region  line 4 onwards check consignment details
                                if (i > 1)
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg[0].Contains('-'))
                                        {
                                            try
                                            {
                                                ///Version below 5 - 1 row of AWB information (AWB+SHC)
                                                ///V5 - 2 Rows - 1.AWB info 2. SHC
                                                AWBPrefix = "";
                                                AWBNumber = "";
                                                if (Convert.ToInt16(ffmdata.ffmversionnum) > 4)
                                                {

                                                    lastrec = "AWB";
                                                }
                                                else
                                                {
                                                    lastrec = "NA";
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                            }

                                            if (!DecodeConsigmentDetails(RefNo, str[i], ref consinfo, ref AWBPrefix, ref AWBNumber, out errorMessage))
                                            {
                                                GenericFunction genericFunction = new GenericFunction();
                                                errorMessage = AWBPrefix + "-" + AWBNumber + " has " + errorMessage;
                                                genericFunction.UpdateErrorMessageToInbox(RefNo, errorMessage, "FFM", false, "", false);
                                                //isConsignmentLineValid = false;
                                                continue;
                                            }


                                            ///Validate AWB Number
                                            if (AWBNumber.Trim().Length != 8)
                                            {
                                                flag = false;
                                                GenericFunction genericFunction = new GenericFunction();
                                                genericFunction.UpdateErrorMessageToInbox(RefNo, "AWB# " + AWBNumber + " not valid.");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        continue;
                                    }
                                }
                                #endregion

                                #region Line 5 Dimendion info
                                if (str[i].StartsWith("DIM", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        int total = msg.Length / 3;
                                        Array.Resize(ref dimensioinfo, dimensioinfo.Length + total + 1);
                                        for (int cnt = 0; cnt < total; cnt++)
                                        {
                                            int place = 3 * cnt;
                                            MessageData.dimensionnfo dimension = new MessageData.dimensionnfo("");
                                            dimension.weightcode = msg[place + 1].Substring(0, 1);
                                            dimension.weight = msg[place + 1].Substring(1);
                                            if (msg.Length > 0)
                                            {
                                                string[] dimstr = msg[place + 2].Split('-');
                                                dimension.mesurunitcode = dimstr[0].Substring(0, 3);
                                                dimension.length = dimstr[0].Substring(3);
                                                dimension.width = dimstr[1];
                                                dimension.height = dimstr[2];
                                            }
                                            dimension.piecenum = msg[place + 3];
                                            dimension.consigref = awbref;
                                            dimension.AWBPrefix = AWBPrefix;
                                            dimension.AWBNumber = AWBNumber;
                                            dimensioinfo[cnt] = dimension;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 10 ULD Specification
                                if (str[i].StartsWith("ULD", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageData.ULDinfo ulddata = new MessageData.ULDinfo("");
                                    try
                                    {
                                        string[] msg = str[i].Trim().Split('/');
                                        if (msg.Length > 1)
                                        {
                                            string[] splitstr = msg[1].Split('-');
                                            ulddata.uldno = splitstr[0];
                                            ulddata.uldtype = splitstr[0].Substring(0, 3);
                                            ulddata.uldsrno = splitstr[0].Substring(3, splitstr[0].Length - 5);
                                            ulddata.uldowner = splitstr[0].Substring(splitstr[0].Length - 2, 2);
                                            if (splitstr.Length > 1)
                                            {
                                                ulddata.uldloadingindicator = splitstr[1];
                                            }
                                            if (msg.Length > 2)
                                            {
                                                ulddata.uldremark = msg[2];
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                    Array.Resize(ref uld, uld.Length + 1);
                                    uld[uld.Length - 1] = ulddata;
                                    if (int.Parse(unloadingportsequence) > 0)
                                    {
                                        uld[uld.Length - 1].portsequence = unloadingportsequence;
                                        uld[uld.Length - 1].refuld = uld.Length.ToString();
                                        uldsequencenum = uld.Length.ToString();
                                    }
                                }
                                #endregion

                                #region Line 7 Other service info
                                if (str[i].StartsWith("OSI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        lastrec = msg[0];
                                        if (msg[1].Length > 0)
                                        {
                                            Array.Resize(ref othinfoarray, othinfoarray.Length + 1);
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo1 = msg[1];
                                            othinfoarray[othinfoarray.Length - 1].consigref = awbref;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Line 8 COR
                                if (str[i].StartsWith("COR", StringComparison.OrdinalIgnoreCase))
                                {
                                    string[] msg = str[i].Split('/');
                                    if (msg.Length > 1 && msg[1].Length > 0)
                                    {
                                        consinfo[int.Parse(awbref) - 1].customorigincode = msg[1];
                                    }
                                }
                                #endregion

                                #region Line9 custom extra info
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

                                #region Line11 special custom information
                                if (str[i].StartsWith("SCI", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        string[] msg = str[i].Split('/');
                                        if (msg.Length > 0)
                                        {
                                            if (msg.Length > 2)
                                                consinfo[int.Parse(awbref) - 1].customorigincode = msg[2];
                                            if (msg.Length > 1)
                                                consinfo[int.Parse(awbref) - 1].customref = msg[1];
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                    }
                                }
                                #endregion

                                #region Last line
                                if (i > str.Length - 3)
                                {
                                    if (str[i].Trim().StartsWith("LAST", StringComparison.OrdinalIgnoreCase) || str[i].Trim().StartsWith("CONT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ffmdata.endmesgcode = str[i].Trim();
                                    }
                                }
                                #endregion Last line

                                #region Other Info
                                if (str[i].StartsWith("/"))
                                {
                                    string[] msg = str[i].Split('/');
                                    try
                                    {
                                        #region line 6 movementinfo info
                                        try
                                        {
                                            if (msg.Length > 0 && msg[0].Length == 0 && lastrec == "NA")
                                            {
                                                lastrec = "MOV";
                                                MessageData.movementinfo movement = new MessageData.movementinfo("");
                                                try
                                                {
                                                    movement.AirportCode = msg[1].Substring(0, 3);
                                                    movement.CarrierCode = "";
                                                    movement.FlightNumber = msg[1].Substring(3);
                                                    if (msg.Length > 2)
                                                    {
                                                        movement.FlightDay = msg[2].Substring(0, 2);
                                                        movement.FlightMonth = msg[2].Substring(2);
                                                    }
                                                    movement.consigref = awbref;
                                                }
                                                catch (Exception ex)
                                                {
                                                    // clsLog.WriteLogAzure(ex);
                                                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                                }
                                                Array.Resize(ref movementInfo, movementInfo.Length + 1);
                                                movementInfo[movementInfo.Length - 1] = movement;
                                            }

                                            if (lastrec == "MOV")
                                            {
                                                if (msg[1].Length > 0)
                                                {
                                                    movementInfo[movementInfo.Length - 1].PriorityorVolumecode = msg[1];

                                                }
                                                lastrec = "NA";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // clsLog.WriteLogAzure(ex);
                                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        }
                                        #endregion

                                        #region SSR 2
                                        if (lastrec == "SSR")
                                        {
                                            ffmdata.specialservicereq2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region OSI 2
                                        if (lastrec == "OSI")
                                        {
                                            othinfoarray[othinfoarray.Length - 1].otherserviceinfo2 = msg[1].Length > 0 ? msg[1] : "";
                                            lastrec = "NA";
                                        }
                                        #endregion

                                        #region Splhandling
                                        if (lastrec == "AWB")
                                        {
                                            try
                                            {
                                                if (str[i].Length > 1)
                                                {
                                                    string[] bupsplhandcode = str[i].Split('/');
                                                    //if (bupsplhandcode[1].Trim().Length == 3)
                                                    //{
                                                    if (bupsplhandcode[1].ToString() == "BUP")
                                                        consinfo[consinfo.Length - 1].IsBup = "true";

                                                    if (str[i].ToString().Contains("BUP"))
                                                    {
                                                        //consinfo[consinfo.Length - 1].splhandling = (str[i].Replace("/BUP", "/")).Replace('/', ',').TrimStart(',');
                                                        consinfo[consinfo.Length - 1].splhandling = (str[i].Replace('/', ',').TrimStart(','));
                                                        if (consinfo[consinfo.Length - 1].splhandling.Substring(0, 3).ToUpper() == "BUP")
                                                        {
                                                            uld[Convert.ToInt16(uldsequencenum) - 1].IsBUP = "true";
                                                        }
                                                        else
                                                        {
                                                            uld[Convert.ToInt16(uldsequencenum) - 1].IsBUP = "false";
                                                        }
                                                        uld[Convert.ToInt16(uldsequencenum) - 1].AWBNumber = consinfo[consinfo.Length - 1].awbnum;
                                                        uld[Convert.ToInt16(uldsequencenum) - 1].AWBPrefix = consinfo[consinfo.Length - 1].airlineprefix;
                                                    }
                                                    else
                                                        consinfo[consinfo.Length - 1].splhandling = str[i].Replace("/", ",");
                                                    //}
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // clsLog.WriteLogAzure(ex);
                                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
                                                if (msgdata.Length > 1)
                                                    custom.IsoCountryCodeOci = msgdata[1];
                                                if (msgdata.Length > 2)
                                                    custom.InformationIdentifierOci = msgdata[2];
                                                if (msgdata.Length > 3)
                                                    custom.CsrIdentifierOci = msgdata[3];
                                                if (msgdata.Length > 4)
                                                    custom.SupplementaryCsrIdentifierOci = msgdata[4];
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
                        }
                    }
                    if (consinfo.Length == 0 && errorMessage != "")
                        flag = false;
                    else
                        flag = true;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on DecodeReceiveFFMMessage");
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            //return flag;
            return ( flag, ffmdata, unloadingPort, consinfo, dimensioinfo, uld, othinfoarray, custominfo, movementInfo, errorMessage);

        }

        /// <summary>
        /// Method to validate the FFM message and save the all operantional data that already decoded
        /// </summary>
        /// <param name="ffmdata">Array contains flight information</param>
        /// <param name="consinfo"></param>
        /// <param name="unloadingport">Array contains unloading port</param>
        /// <param name="objDimension">Array contains AWB dimensions</param>
        /// <param name="uld">Array contains ULD details</param>
        /// <param name="REFNo">Message SrNo from tblInbox</param>
        /// <param name="strMessage">FFM message string</param>
        /// <param name="strMessageFrom">Updated By</param>
        /// <param name="strFromID">From emailID</param>
        /// <param name="strStatus">Message status</param>
        //public async Task<bool> SaveValidateFFMMessage(ref MessageData.ffminfo ffmdata, ref MessageData.consignmnetinfo[] consinfo, ref MessageData.unloadingport[] unloadingport, ref MessageData.dimensionnfo[] objDimension, ref MessageData.ULDinfo[] uld, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus, string strMsg, string PIMAAddress, out string ErrorMsg)
        public async Task<(bool,
            ffminfo ffmdata,
            consignmnetinfo[] consinfo,
            unloadingport[] unloadingport,
            dimensionnfo[] objDimension,
            ULDinfo[] uld,
            string ErrorMsg
            )>
            SaveValidateFFMMessage(ffminfo ffmdata, consignmnetinfo[] consinfo,unloadingport[] unloadingport, dimensionnfo[] objDimension,ULDinfo[] uld, int REFNo, string strMessage, string strMessageFrom, string strFromID, string strStatus, string strMsg, string PIMAAddress,string ErrorMsg)
        {
            string source = string.Empty, dest = string.Empty, strAWbStatus = string.Empty, awbNumbers = string.Empty;
            ErrorMsg = string.Empty;
            string TotalWgt = string.Empty, AuditWgt = string.Empty, AuditPcs = string.Empty;
            int ffmSequenceNo = 1, ManifestID = 0;
            bool flag = true;
            DateTime flightdate = new DateTime();
            GenericFunction genericFunction = new GenericFunction();

            //FNAMessageProcessor FNA = new FNAMessageProcessor();

            MessageData.consignmnetinfo[] auditconsinfo = new MessageData.consignmnetinfo[0];
            List<string> lstDeliveredAWB = new List<string>();
            List<string> lstOrgDstMissmatchAWB = new List<string>();
            List<string> lstAWB = new List<string>();
            List<string> duplicateawbs = new List<string>();
            if (consinfo.Length > 0)
            {
                for (int i = 0; i < consinfo.Length; i++)
                {
                    if (consinfo[i].consigntype == "S" && consinfo[i].uldsequence == "")
                    {
                        lstAWB.Add(consinfo[i].airlineprefix + "-" + consinfo[i].awbnum);
                    }
                }
                duplicateawbs = lstAWB.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();
            }

            try
            {
                //SQLServer sqlServer = new SQLServer();
                string flightnum = ffmdata.carriercode + ffmdata.fltnum;

                flightdate = DateTime.Parse(DateTime.Parse("1." + ffmdata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + ffmdata.fltdate.PadLeft(2, '0') + "/" + +System.DateTime.Today.Year);
                if (System.DateTime.Today.Month == 12 && ffmdata.month == "JAN")
                    flightdate = DateTime.Parse(DateTime.Parse("1." + ffmdata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + ffmdata.fltdate.PadLeft(2, '0') + "/" + +(System.DateTime.Today.Year + 1));
                if (System.DateTime.Today.Month == 1 && ffmdata.month == "DEC")
                    flightdate = DateTime.Parse(DateTime.Parse("1." + ffmdata.month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + ffmdata.fltdate.PadLeft(2, '0') + "/" + +(System.DateTime.Today.Year - 1));

                //string[] PName = new string[] { "flightnum", "date" };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.NVarChar, SqlDbType.DateTime };
                //object[] PValue = new object[] { flightnum, flightdate };
                //DataSet ds = sqlServer.SelectRecords("spGetDestCodeForFFM", PName, PValue, PType);
                

                SqlParameter[] parameters =
                {
                    new SqlParameter("@flightnum", SqlDbType.NVarChar) { Value = flightnum },
                    new SqlParameter("@date", SqlDbType.DateTime) { Value = flightdate }
                };
                DataSet? ds = await _readWriteDao.SelectRecords("spGetDestCodeForFFM", parameters);
                DataSet? ds1 = new DataSet();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    source = ds.Tables[0].Rows[0]["source"].ToString();
                    dest = ds.Tables[0].Rows[0]["Dest"].ToString();
                }
                if (ffmdata.fltairportcode != "")
                    source = ffmdata.fltairportcode;

                ffmSequenceNo = int.Parse(ffmdata.messagesequencenum == "" ? "0" : ffmdata.messagesequencenum);
                if (!(ffmSequenceNo == 1 && ffmdata.endmesgcode == "LAST"))
                {
                    genericFunction.UpdateInboxFromMessageParameter(REFNo, string.Empty, flightnum, source, dest, "FFM", "Log-Part-FFM", flightdate, false, ffmdata.endmesgcode, strMsg, ffmSequenceNo);
                    //return true;
                    return (true, ffmdata, consinfo, unloadingport, objDimension, uld, ErrorMsg);
                }

                #region : Save Tail Number :
                //string[] Pname = new string[5];
                //object[] Pvalue = new object[5];
                //SqlDbType[] Ptype = new SqlDbType[5];

                //Pname[0] = "FLTNo";
                //Pname[1] = "FltDate";
                //Pname[2] = "TailNo";
                //Pname[3] = "FlightOrigin";
                //Pname[4] = "FlightDestination";

                //Ptype[0] = SqlDbType.VarChar;
                //Ptype[1] = SqlDbType.DateTime;
                //Ptype[2] = SqlDbType.VarChar;
                //Ptype[3] = SqlDbType.VarChar;
                //Ptype[4] = SqlDbType.VarChar;

                //Pvalue[0] = ffmdata.carriercode + ffmdata.fltnum;
                //Pvalue[1] = flightdate;
                //Pvalue[2] = ffmdata.aircraftregistration;
                //Pvalue[3] = source;
                //Pvalue[4] = dest;
                
                //sqlServer = new SQLServer();
                //dsTailNo = sqlServer.SelectRecords("SPSaveTailNumber", Pname, Pvalue, Ptype);
                SqlParameter[] parametersFLT =
                {
                    new SqlParameter("@FLTNo", SqlDbType.VarChar)           { Value = ffmdata.carriercode + ffmdata.fltnum },
                    new SqlParameter("@FltDate", SqlDbType.DateTime)       { Value = flightdate },
                    new SqlParameter("@TailNo", SqlDbType.VarChar)         { Value = ffmdata.aircraftregistration },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar)   { Value = source },
                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = dest }
                };
                
                DataSet? dsTailNo = new DataSet();
                dsTailNo = await _readWriteDao.SelectRecords("SPSaveTailNumber", parametersFLT);
                if (dsTailNo != null && dsTailNo.Tables.Count > 0 && dsTailNo.Tables[0].Rows.Count > 0)
                {
                    ffmdata.aircraftregistration = dsTailNo.Tables[0].Rows[0]["TailNo"].ToString();
                }
                #endregion Save Tail Number

                if (unloadingport.Length > 0)
                {
                    #region NIL Flight
                    for (int k = 0; k < unloadingport.Length; k++)
                    {
                        if (unloadingport[k].nilcargocode.Trim() != "")
                        {
                            ManifestID = await ExportManifestSummary(ffmdata.carriercode + ffmdata.fltnum, ffmdata.fltairportcode, unloadingport[k].unloadingairport, flightdate, ffmdata.aircraftregistration, REFNo);
                            if (ManifestID > 0)
                            {
                                //sqlServer = new SQLServer();
                                ffmSequenceNo = int.Parse(ffmdata.messagesequencenum == "" ? "0" : ffmdata.messagesequencenum);
                                //string[] PStatusName = new string[] { "ManifestID", "FFMStatus", "FlightNumber", "FlightDate", "FlightOrigin", "FlightDestination", "FFMSequenceNo", "isNIL" };
                                //object[] PStatusValues = new object[] { ManifestID, ffmdata.endmesgcode, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, ffmSequenceNo, true };
                                //SqlDbType[] sqlStatusType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Bit };
                                //sqlServer.InsertData("MSG_uSPCheckFFMFlightStatus", PStatusName, sqlStatusType, PStatusValues);

                                SqlParameter[] parametersMani =
                                {
                                    new SqlParameter("@ManifestID", SqlDbType.VarChar)       { Value = ManifestID },
                                    new SqlParameter("@FFMStatus", SqlDbType.VarChar)       { Value = ffmdata.endmesgcode },
                                    new SqlParameter("@FlightNumber", SqlDbType.VarChar)    { Value = ffmdata.carriercode + ffmdata.fltnum },
                                    new SqlParameter("@FlightDate", SqlDbType.DateTime)     { Value = flightdate },
                                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar)    { Value = ffmdata.fltairportcode },
                                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = unloadingport[k].unloadingairport },
                                    new SqlParameter("@FFMSequenceNo", SqlDbType.Int)       { Value = ffmSequenceNo },
                                    new SqlParameter("@isNIL", SqlDbType.Bit)              { Value = true }
                                };

                               await _readWriteDao.ExecuteNonQueryAsync("MSG_uSPCheckFFMFlightStatus", parametersMani);
                            }
                            UpdateDepartureDataForCapacity(flightnum, flightdate, source);
                            genericFunction.UpdateInboxFromMessageParameter(REFNo, string.Empty, flightnum, source, dest, "FFM", strMessageFrom == "" ? strFromID : strMessageFrom, flightdate, true, ffmdata.endmesgcode);
                        }
                    }
                    #endregion NIL Flight
                    for (int k = 0; k < unloadingport.Length; k++)
                    {
                        string previousNilCargoCode = string.Empty;
                        if (k > 0)
                        {
                            previousNilCargoCode = unloadingport[k - 1].nilcargocode.Trim();
                        }

                        if (unloadingport[k].nilcargocode.Trim() == "")
                        {
                            dest = unloadingport[k].unloadingairport;

                            string allAWBsInFFM = string.Empty;
                            if (consinfo.Length > 0)
                            {
                                for (int i = 0; i < consinfo.Length; i++)
                                {
                                    if (!allAWBsInFFM.Contains(consinfo[i].airlineprefix + "-" + consinfo[i].awbnum))
                                    {
                                        if (duplicateawbs.Contains(consinfo[i].airlineprefix + "-" + consinfo[i].awbnum))
                                        {
                                            lstDeliveredAWB.Add(consinfo[i].airlineprefix + "-" + consinfo[i].awbnum);
                                        }
                                        else
                                        {
                                            allAWBsInFFM = allAWBsInFFM == string.Empty ? consinfo[i].consigntype + "-" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + "-" + consinfo[i].origin + "-" + consinfo[i].dest : allAWBsInFFM + "," + consinfo[i].consigntype + "-" + consinfo[i].airlineprefix + "-" + consinfo[i].awbnum + "-" + consinfo[i].origin + "-" + consinfo[i].dest;
                                        }
                                    }
                                }
                            }

                            ManifestID = await ExportManifestSummary(ffmdata.carriercode + ffmdata.fltnum, ffmdata.fltairportcode, unloadingport[k].unloadingairport, flightdate, ffmdata.aircraftregistration, REFNo);
                            if (ManifestID > 0)
                            {
                                if (k == 0)
                                {
                                    for (int i = 0; i < unloadingport.Length; i++)
                                    {
                                        //sqlServer = new SQLServer();
                                        ffmSequenceNo = int.Parse(ffmdata.messagesequencenum == "" ? "0" : ffmdata.messagesequencenum);
                                        //string[] PStatusName = new string[] { "ManifestID", "FFMStatus", "FlightNumber", "FlightDate", "FlightOrigin", "FlightDestination", "FFMSequenceNo", "AllAWBsInFFM" };
                                        //object[] PStatusValues = new object[] { ManifestID, ffmdata.endmesgcode, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[i].unloadingairport, ffmSequenceNo, allAWBsInFFM };
                                        //SqlDbType[] sqlStatusType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar };
                                        //sqlServer.InsertData("MSG_uSPCheckFFMFlightStatus", PStatusName, sqlStatusType, PStatusValues);
                                        //ds1 = sqlServer.SelectRecords("MSG_uSPCheckFFMFlightStatus", PStatusName, PStatusValues, sqlStatusType);

                                        SqlParameter[] parametersManiF =
                                        {
                                            new SqlParameter("@ManifestID", SqlDbType.VarChar)        { Value = ManifestID },
                                            new SqlParameter("@FFMStatus", SqlDbType.VarChar)        { Value = ffmdata.endmesgcode },
                                            new SqlParameter("@FlightNumber", SqlDbType.VarChar)     { Value = ffmdata.carriercode + ffmdata.fltnum },
                                            new SqlParameter("@FlightDate", SqlDbType.DateTime)      { Value = flightdate },
                                            new SqlParameter("@FlightOrigin", SqlDbType.VarChar)     { Value = ffmdata.fltairportcode },
                                            new SqlParameter("@FlightDestination", SqlDbType.VarChar){ Value = unloadingport[i].unloadingairport },
                                            new SqlParameter("@FFMSequenceNo", SqlDbType.Int)        { Value = ffmSequenceNo },
                                            new SqlParameter("@AllAWBsInFFM", SqlDbType.VarChar)    { Value = allAWBsInFFM }
                                        };
                                        ds1 = await _readWriteDao.SelectRecords("MSG_uSPCheckFFMFlightStatus", parametersManiF);
                                    }
                                }
                                MessageData.consignmnetinfo[] ReceivedConsigInfo = new MessageData.consignmnetinfo[consinfo.Length];
                                Array.Copy(consinfo, ReceivedConsigInfo, consinfo.Length);

                                MessageData.consignmnetinfo[] FFMConsig = new MessageData.consignmnetinfo[consinfo.Length];
                                Array.Copy(consinfo, FFMConsig, consinfo.Length);

                                if (consinfo.Length > 0)
                                {
                                    bool isDestinationMismatch = false;
                                    #region : Club splited and divided AWB's data :
                                    DataTable dtAWBDetail = new DataTable();
                                    dtAWBDetail.Columns.Add("AirlinePrefix");
                                    dtAWBDetail.Columns.Add("AWBNo");
                                    dtAWBDetail.Columns.Add("Origin");
                                    dtAWBDetail.Columns.Add("Destination");
                                    dtAWBDetail.Columns.Add("AWbPieceCode");
                                    dtAWBDetail.Columns.Add("AWBPieces", SqlDbType.Int.GetType());
                                    dtAWBDetail.Columns.Add("FFMPiecesCode");
                                    dtAWBDetail.Columns.Add("FFMPieces", SqlDbType.Int.GetType());
                                    dtAWBDetail.Columns.Add("GrossWt");
                                    dtAWBDetail.Columns.Add("VolumeCode");
                                    dtAWBDetail.Columns.Add("VolumeWt");
                                    dtAWBDetail.Columns.Add("ComodityCode");
                                    dtAWBDetail.Columns.Add("ComodityDesc");

                                    for (int i = 0; i < consinfo.Length; i++)
                                    {
                                        DataRow drawb = dtAWBDetail.NewRow();
                                        drawb["AirlinePrefix"] = consinfo[i].airlineprefix;
                                        drawb["AWBNo"] = consinfo[i].awbnum;
                                        drawb["Origin"] = consinfo[i].origin;
                                        drawb["Destination"] = consinfo[i].dest;
                                        drawb["AWBPieces"] = consinfo[i].numshp;
                                        if (consinfo[i].consigntype != "T")
                                        {
                                            drawb["AWbPieceCode"] = consinfo[i].shpdesccode;
                                            drawb["FFMPiecesCode"] = consinfo[i].consigntype;
                                            drawb["FFMPieces"] = consinfo[i].pcscnt;
                                        }
                                        else
                                        {
                                            drawb["AWbPieceCode"] = consinfo[i].consigntype;
                                        }
                                        drawb["GrossWt"] = consinfo[i].weight;
                                        drawb["VolumeWt"] = consinfo[i].volumeamt;
                                        drawb["VolumeCode"] = consinfo[i].volumecode;
                                        drawb["ComodityCode"] = "";
                                        drawb["ComodityDesc"] = consinfo[i].manifestdesc;
                                        dtAWBDetail.Rows.Add(drawb);
                                    }
                                    DataTable dtWeightUpdate = new DataTable("dtWeightUpdate");
                                    dtWeightUpdate.Columns.Add("AirlinePrefix");
                                    dtWeightUpdate.Columns.Add("AWBNo");
                                    dtWeightUpdate.Columns.Add("Origin");
                                    dtWeightUpdate.Columns.Add("Destination");
                                    dtWeightUpdate.Columns.Add("AWbPieceCode");
                                    dtWeightUpdate.Columns.Add("AWBPieces", SqlDbType.Int.GetType());
                                    dtWeightUpdate.Columns.Add("FFMPiecesCode");
                                    dtWeightUpdate.Columns.Add("FFMPieces", SqlDbType.Int.GetType());
                                    dtWeightUpdate.Columns.Add("GrossWt");
                                    dtWeightUpdate.Columns.Add("VolumeCode");
                                    dtWeightUpdate.Columns.Add("VolumeWt");
                                    dtWeightUpdate.Columns.Add("ComodityCode");
                                    dtWeightUpdate.Columns.Add("ComodityDesc");
                                    DataRow[] drs = dtAWBDetail.Select("", "AWBNo");
                                    decimal gross = 0, volume = 0;
                                    int pieces = 0, ffmPieces = 0;

                                    string awbNo = drs[0]["AWBNo"].ToString();
                                    string awbPrefix = Convert.ToString(drs[0]["AirlinePrefix"]);
                                    DataRow drn = dtWeightUpdate.NewRow();
                                    drn["AirlinePrefix"] = drs[0]["AirlinePrefix"].ToString();
                                    drn["AWBNo"] = drs[0]["AWBNo"].ToString();
                                    drn["Origin"] = drs[0]["Origin"].ToString();
                                    drn["Destination"] = drs[0]["Destination"].ToString();
                                    drn["AWbPieceCode"] = drs[0]["AWbPieceCode"].ToString() == ""
                                                              ? ""
                                                              : drs[0]["AWbPieceCode"].ToString();
                                    drn["FFMPiecesCode"] = drs[0]["FFMPiecesCode"].ToString();
                                    drn["AWBPieces"] = drs[0]["AWBPieces"].ToString() == ""
                                                       ? "0"
                                                       : drs[0]["AWBPieces"].ToString();
                                    drn["FFMPieces"] = drs[0]["FFMPieces"].ToString() == ""
                                                        ? "0"
                                                        : drs[0]["FFMPieces"].ToString();
                                    drn["VolumeCode"] = drs[0]["VolumeCode"].ToString() == ""
                                                        ? ""
                                                        : drs[0]["VolumeCode"].ToString();
                                    drn["ComodityCode"] = drs[0]["ComodityCode"].ToString() == ""
                                                        ? ""
                                                        : drs[0]["ComodityCode"].ToString();
                                    drn["ComodityDesc"] = drs[0]["ComodityDesc"].ToString() == ""
                                                        ? ""
                                                        : drs[0]["ComodityDesc"].ToString();
                                    foreach (DataRow dr in drs)
                                    {
                                        if (awbNo == dr["AWBNo"].ToString() && awbPrefix == Convert.ToString(dr["AirlinePrefix"]))
                                        {
                                            gross +=
                                                Convert.ToDecimal(dr["GrossWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : dr["GrossWt"].ToString());

                                            ffmPieces +=
                                                int.Parse(dr["FFMPieces"].ToString() == "" ? "0" : dr["FFMPieces"].ToString());

                                            volume +=
                                                Convert.ToDecimal(drs[0]["VolumeWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : drs[0]["VolumeWt"].ToString());
                                            drn["AWbPieceCode"] = dr["AWbPieceCode"].ToString();

                                            pieces = int.Parse(drs[0]["AWBPieces"].ToString() == "" ? "0" : drs[0]["AWBPieces"].ToString());

                                            drn["FFMPiecesCode"] = dr["FFMPiecesCode"].ToString();

                                            drn["FFMPieces"] = ffmPieces;

                                            drn["GrossWt"] =
                                                Convert.ToDecimal(drn["GrossWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : drn["GrossWt"].ToString()) +
                                                Convert.ToDecimal(dr["GrossWt"].ToString() == "" ? "0" : dr["GrossWt"].ToString());

                                            drn["VolumeWt"] = Convert.ToDecimal(drn["VolumeWt"].ToString() == ""
                                                                      ? "0.0"
                                                                      : drn["VolumeWt"].ToString()) +
                                                Convert.ToDecimal(dr["VolumeWt"].ToString() == "" ? "0" : dr["VolumeWt"].ToString());
                                        }
                                        else
                                        {
                                            dtWeightUpdate.Rows.Add(drn);
                                            drn = dtWeightUpdate.NewRow();
                                            gross =
                                                Convert.ToDecimal(dr["GrossWt"].ToString() == "" ? "0" : dr["GrossWt"].ToString());
                                            if (int.Parse(dr["AWBPieces"].ToString()) < Int16.MinValue || int.Parse(dr["AWBPieces"].ToString()) > Int16.MaxValue)
                                            {
                                                ErrorMsg = "AWB " + dr["AWBNo"].ToString() + " exceed AWBPieces.";
                                            }
                                            else
                                            {
                                                pieces =
                                               Convert.ToInt16(dr["AWBPieces"].ToString() == "" ? "0" : dr["AWBPieces"].ToString());

                                            }

                                            volume =
                                                Convert.ToDecimal(dr["VolumeWt"].ToString() == ""
                                                                      ? "0"
                                                                      : dr["VolumeWt"].ToString());
                                            ffmPieces = Convert.ToInt16(dr["FFMPieces"].ToString() == "" ? "0" : dr["FFMPieces"].ToString());

                                            awbNo = dr["AWBNo"].ToString();
                                            awbPrefix = Convert.ToString(dr["AirlinePrefix"]);
                                            drn["AirlinePrefix"] = dr["AirlinePrefix"].ToString();
                                            drn["AWBNo"] = dr["AWBNo"].ToString();
                                            drn["Origin"] = dr["Origin"].ToString();
                                            drn["Destination"] = dr["Destination"].ToString();
                                            drn["AWbPieceCode"] = dr["AWbPieceCode"].ToString();
                                            drn["AWBPieces"] = dr["AWBPieces"].ToString();
                                            drn["FFMPiecesCode"] = dr["FFMPiecesCode"].ToString();
                                            drn["FFMPieces"] = dr["FFMPieces"].ToString() == "" ? "0" : dr["FFMPieces"].ToString();
                                            drn["GrossWt"] = gross;
                                            drn["VolumeWt"] = volume;
                                            drn["VolumeCode"] = dr["volumeCode"].ToString();
                                            drn["ComodityCode"] = dr["ComodityCode"].ToString();
                                            drn["ComodityDesc"] = dr["ComodityDesc"].ToString();

                                        }
                                    }
                                    dtWeightUpdate.Rows.Add(drn);
                                    #endregion Club splited and divided AWB's data

                                    #region : Validate AWB's and save audit log :
                                    if (consinfo.Length > 0)
                                    {
                                        Boolean bIsAllAWBNotPresent = false;
                                        int cntAWBNotPresent = 0;
                                        string AWBNotPresent = string.Empty;

                                        #region : Club Pieces and Weight :
                                        MessageData.consignmnetinfo[] FFMAuditLogConsig = new MessageData.consignmnetinfo[consinfo.Length];
                                        Array.Copy(consinfo, FFMAuditLogConsig, consinfo.Length);
                                        ReProcessConsigment(ref FFMAuditLogConsig, ref auditconsinfo);
                                        #endregion

                                        awbNumbers = string.Empty;
                                        for (int i = 0; i < auditconsinfo.Length; i++)
                                        {
                                            if (auditconsinfo[i].airlineprefix != null && auditconsinfo[i].awbnum != null)
                                            {
                                                string AWBNum = auditconsinfo[i].awbnum;
                                                string AWBPrefix = auditconsinfo[i].airlineprefix;
                                                string MftPcs = auditconsinfo[i].pcscnt;
                                                string MftWt = auditconsinfo[i].weight;
                                                string ConsignmentType = auditconsinfo[i].consigntype;
                                                string awbDeliveredStatus = string.Empty;
                                                string originCode = string.Empty;
                                                string destinationCode = string.Empty;
                                                int ffmPcs = 0, ffmTotalPieces = 0;
                                                ffmPcs = auditconsinfo[i].pcscnt == "" ? 0 : Convert.ToInt32(auditconsinfo[i].pcscnt);
                                                ffmTotalPieces = auditconsinfo[i].numshp == "" ? 0 : Convert.ToInt32(auditconsinfo[i].numshp);
                                                awbNumbers = awbNumbers.Trim() == string.Empty ? AWBPrefix + "-" + AWBNum : awbNumbers + "," + AWBPrefix + "-" + AWBNum;
                                                string errorMessage = string.Empty;
                                                bool IsStatus = false;
                                                (IsStatus, awbDeliveredStatus, strAWbStatus, originCode, destinationCode, errorMessage,TotalWgt) = await IsAWBPresent(AWBNum, AWBPrefix, ConsignmentType, ffmPcs, ffmTotalPieces,  awbDeliveredStatus,  strAWbStatus,  originCode,  destinationCode,  errorMessage, ffmdata.fltairportcode,  TotalWgt, REFNo, "FFM");
                                                //if (!IsAWBPresent(AWBNum, AWBPrefix, ConsignmentType, ffmPcs, ffmTotalPieces, out awbDeliveredStatus, out strAWbStatus, out originCode, out destinationCode, out errorMessage, ffmdata.fltairportcode, out TotalWgt, REFNo, "FFM"))

                                                if (!IsStatus)
                                                {
                                                    bIsAllAWBNotPresent = true;
                                                    cntAWBNotPresent++;
                                                    AWBNotPresent = AWBNotPresent.Length > 0 ? AWBNotPresent + "," + AWBPrefix + "-" + AWBNum : AWBNotPresent + AWBPrefix + "-" + AWBNum;
                                                    continue;
                                                }
                                                else
                                                {
                                                    bIsAllAWBNotPresent = false;
                                                    if (awbDeliveredStatus == "C")
                                                        lstDeliveredAWB.Add(AWBNum);
                                                    if (errorMessage.Trim() != string.Empty)
                                                    {
                                                        if (errorMessage.ToUpper().Contains("AWB IS VOIDED"))
                                                        {
                                                            cntAWBNotPresent++;
                                                            bIsAllAWBNotPresent = true;
                                                            lstDeliveredAWB.Add(AWBNum);
                                                        }
                                                        if (errorMessage.Contains("Shipper is Inactive OR Expired"))
                                                        {
                                                            cntAWBNotPresent++;
                                                            bIsAllAWBNotPresent = true;
                                                            lstDeliveredAWB.Add(AWBNum);
                                                        }
                                                        if (errorMessage.Contains("DGR is not yet approved"))
                                                        {
                                                            cntAWBNotPresent++;
                                                            bIsAllAWBNotPresent = true;
                                                            lstDeliveredAWB.Add(AWBNum);
                                                        }

                                                        genericFunction.UpdateErrorMessageToInbox(REFNo, errorMessage, "FFM", false, "", false);
                                                        continue;
                                                    }
                                                    if (duplicateawbs.Contains(awbPrefix + "-" + AWBNum))
                                                    {
                                                        genericFunction.UpdateErrorMessageToInbox(REFNo, awbPrefix + "-" + AWBNum + " Invalid Message Format");
                                                        lstDeliveredAWB.Add(AWBNum);
                                                    }
                                                    if (originCode != auditconsinfo[i].origin && destinationCode != auditconsinfo[i].dest)
                                                    {
                                                        genericFunction.UpdateErrorMessageToInbox(REFNo, AWBNum + " has origin and destination mismatch");
                                                        lstDeliveredAWB.Add(AWBNum);
                                                        lstOrgDstMissmatchAWB.Add(AWBNum);
                                                    }
                                                    else if (originCode != auditconsinfo[i].origin)
                                                    {

                                                        genericFunction.UpdateErrorMessageToInbox(REFNo, awbPrefix + "-" + AWBNum + " has origin mismatch");
                                                        lstDeliveredAWB.Add(AWBNum);
                                                        lstOrgDstMissmatchAWB.Add(AWBNum);
                                                        _fNAMessageProcessor.GenerateFNAMessage(strMessage, awbPrefix + "-" + AWBNum + " has origin mismatch", awbPrefix, AWBNum, strMessageFrom == "" ? strFromID : strMessageFrom, strFromID, PIMAAddress);

                                                    }
                                                    else if (destinationCode != auditconsinfo[i].dest)
                                                    {
                                                        genericFunction.UpdateErrorMessageToInbox(REFNo, AWBNum + " has destination mismatch");
                                                        isDestinationMismatch = true;
                                                        lstDeliveredAWB.Add(AWBNum);
                                                        lstOrgDstMissmatchAWB.Add(AWBNum);
                                                        _fNAMessageProcessor.GenerateFNAMessage(strMessage, awbPrefix + "-" + AWBNum + " has destination mismatch", awbPrefix, AWBNum, strMessageFrom == "" ? strFromID : strMessageFrom, strFromID, PIMAAddress);


                                                    }

                                                    if (lstOrgDstMissmatchAWB.Contains(AWBNum))
                                                    {
                                                        //string[] PStatusName = new string[] { "ManifestID", "FFMStatus", "FlightNumber", "FlightDate", "FlightOrigin", "FlightDestination", "FFMSequenceNo", "AllAWBsInFFM", "ValidateOriginDestMissmatch" };
                                                        //object[] PStatusValues = new object[] { ManifestID, ffmdata.endmesgcode, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, ffmSequenceNo, AWBNum, 1 };
                                                        //SqlDbType[] sqlStatusType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Bit };
                                                        //sqlServer.SelectRecords("MSG_uSPCheckFFMFlightStatus", PStatusName, PStatusValues, sqlStatusType);

                                                        SqlParameter[] parametersFF =
                                                        {
                                                            new SqlParameter("@ManifestID", SqlDbType.VarChar)                { Value = ManifestID },
                                                            new SqlParameter("@FFMStatus", SqlDbType.VarChar)                { Value = ffmdata.endmesgcode },
                                                            new SqlParameter("@FlightNumber", SqlDbType.VarChar)             { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                            new SqlParameter("@FlightDate", SqlDbType.DateTime)              { Value = flightdate },
                                                            new SqlParameter("@FlightOrigin", SqlDbType.VarChar)             { Value = ffmdata.fltairportcode },
                                                            new SqlParameter("@FlightDestination", SqlDbType.VarChar)        { Value = unloadingport[k].unloadingairport },
                                                            new SqlParameter("@FFMSequenceNo", SqlDbType.Int)                { Value = ffmSequenceNo },
                                                            new SqlParameter("@AllAWBsInFFM", SqlDbType.VarChar)             { Value = AWBNum },
                                                            new SqlParameter("@ValidateOriginDestMissmatch", SqlDbType.Bit)  { Value = 1 }
                                                        };
                                                        
                                                        await _readWriteDao.SelectRecords("MSG_uSPCheckFFMFlightStatus", parametersFF);


                                                    }

                                                    if (!lstDeliveredAWB.Contains(auditconsinfo[i].awbnum))
                                                    {
                                                        //sqlServer = new SQLServer();
                                                        string[] arrMilestone;

                                                        if (strAWbStatus == "BK")
                                                            arrMilestone = new string[2] { "Executed", "Accepted" };
                                                        else if (strAWbStatus == "EX")
                                                            arrMilestone = new string[1] { "Accepted" };
                                                        else
                                                            arrMilestone = new string[0] { };

                                                        for (int z = 0; z < arrMilestone.Length; z++)
                                                        {
                                                            AuditWgt = auditconsinfo[i].consigntype == "S" ? TotalWgt : auditconsinfo[i].weight;
                                                            AuditPcs = auditconsinfo[i].consigntype == "S" ? auditconsinfo[i].numshp : auditconsinfo[i].pcscnt;

                                                            //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination"
                                                            //            , "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "ULDNo", "Shipmenttype", "Volume" };
                                                            //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
                                                            //            , SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                                                            //object[] CValues = new object[] { auditconsinfo[i].airlineprefix, auditconsinfo[i].awbnum, auditconsinfo[i].origin, auditconsinfo[i].dest,AuditPcs , AuditWgt, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[k].unloadingairport, arrMilestone[z], "AWB " +arrMilestone[z],
                                                            //                          "AWB " +arrMilestone[z] + " by FFM", "FFM", DateTime.UtcNow.ToString(), 1, string.Empty, auditconsinfo[i].consigntype, auditconsinfo[i].volumeamt.Trim() == string.Empty ? "0" : auditconsinfo[i].volumeamt.Trim() };
                                                            //if (!sqlServer.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues))
                                                            //    clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);

                                                            SqlParameter[] parametersAWB =
                                                            {
                                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar)       { Value = auditconsinfo[i].airlineprefix },
                                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar)       { Value = auditconsinfo[i].awbnum },
                                                                new SqlParameter("@Origin", SqlDbType.VarChar)          { Value = auditconsinfo[i].origin },
                                                                new SqlParameter("@Destination", SqlDbType.VarChar)     { Value = auditconsinfo[i].dest },
                                                                new SqlParameter("@Pieces", SqlDbType.VarChar)          { Value = AuditPcs },
                                                                new SqlParameter("@Weight", SqlDbType.VarChar)          { Value = AuditWgt },
                                                                new SqlParameter("@FlightNo", SqlDbType.VarChar)        { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                                new SqlParameter("@FlightDate", SqlDbType.DateTime)     { Value = flightdate },
                                                                new SqlParameter("@FlightOrigin", SqlDbType.VarChar)    { Value = ffmdata.fltairportcode },
                                                                new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = unloadingport[k].unloadingairport },
                                                                new SqlParameter("@Action", SqlDbType.VarChar)          { Value = arrMilestone[z] },
                                                                new SqlParameter("@Message", SqlDbType.VarChar)         { Value = "AWB " + arrMilestone[z] },
                                                                new SqlParameter("@Description", SqlDbType.VarChar)     { Value = "AWB " + arrMilestone[z] + " by FFM" },
                                                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar)       { Value = "FFM" },
                                                                new SqlParameter("@UpdatedOn", SqlDbType.VarChar)       { Value = DateTime.UtcNow.ToString() },
                                                                new SqlParameter("@Public", SqlDbType.Bit)             { Value = 1 },
                                                                new SqlParameter("@ULDNo", SqlDbType.VarChar)           { Value = string.Empty },
                                                                new SqlParameter("@Shipmenttype", SqlDbType.VarChar)    { Value = auditconsinfo[i].consigntype },
                                                                new SqlParameter("@Volume", SqlDbType.VarChar)          { Value = string.IsNullOrWhiteSpace(auditconsinfo[i].volumeamt) ? "0" : auditconsinfo[i].volumeamt.Trim() }
                                                            };

                                                            if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", parametersAWB))
                                                                //clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);
                                                                // clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine );
                                                                _logger.LogWarning("AWB Audit log  for: {0}" , consinfo[i].awbnum + Environment.NewLine );

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (AWBNotPresent.Length > 0)
                                        {
                                            if (ErrorMsg.Trim() != string.Empty)
                                                ErrorMsg = ErrorMsg + " Doest not exists AWB " + AWBNotPresent;
                                            else
                                                ErrorMsg = "Does not exists AWB " + AWBNotPresent;

                                            genericFunction.UpdateErrorMessageToInbox(REFNo, ErrorMsg);
                                        }
                                        if (bIsAllAWBNotPresent && auditconsinfo.Length == cntAWBNotPresent)
                                            //return false;
                                            return (false, ffmdata, consinfo, unloadingport, objDimension, uld, ErrorMsg);
                                    }
                                    #endregion Validate AWB's and save audit log

                                    consinfo = new MessageData.consignmnetinfo[ReceivedConsigInfo.Length];
                                    Array.Copy(ReceivedConsigInfo, consinfo, ReceivedConsigInfo.Length);

                                    double TotalWeight = 0.0, Val = 0.0;

                                    #region Check Availabe ULD data if Present insert into ExportManifestULDAWB Association
                                    if (consinfo.Length > 0)
                                    {
                                        for (int i = 0; i < consinfo.Length; i++)
                                        {
                                            string UNID = string.Empty;
                                            if (!lstDeliveredAWB.Contains(consinfo[i].awbnum))
                                            {
                                                int refNum = Convert.ToInt16(consinfo[i].portsequence) - 1;
                                                int ULDRef = 0;
                                                string ULDNo = "BULK";

                                                int TotalAWBPcs = 0;
                                                if (consinfo[i].numshp != "")
                                                    TotalAWBPcs = int.Parse(consinfo[i].numshp);
                                                else
                                                    TotalAWBPcs = int.Parse(consinfo[i].pcscnt);

                                                if (consinfo[i].weight != "")
                                                {
                                                    Val = double.Parse(consinfo[i].weight);
                                                    TotalWeight += Val;
                                                }
                                                if (int.Parse(consinfo[i].numshp.ToString()) < Int16.MinValue || int.Parse(consinfo[i].numshp.ToString()) > Int16.MaxValue)
                                                {
                                                    continue;
                                                }

                                                if (consinfo[i].uldsequence.Length > 0)
                                                {
                                                    ULDRef = Convert.ToInt16(consinfo[i].uldsequence);
                                                    if (ULDRef > 0)
                                                        ULDNo = uld[ULDRef - 1].uldno;
                                                }
                                                if (previousNilCargoCode == "NIL")
                                                {
                                                    unloadingport[refNum].unloadingairport = dest;
                                                }
                                                else if (k > 0)
                                                {
                                                    ffmSequenceNo = 99;
                                                    unloadingport[refNum].unloadingairport = dest;
                                                }

                                                if ((ds1 != null && ds1.Tables.Count > 0 && ds1.Tables[0].Rows.Count > 0))
                                                {
                                                    for (int j = 0; j < ds1.Tables[0].Rows.Count; j++)
                                                    {
                                                        if (awbPrefix == ds1.Tables[0].Rows[j]["AWBPrefix"].ToString() && awbNo == ds1.Tables[0].Rows[j]["AWBNumber"].ToString() && ULDNo == ds1.Tables[0].Rows[j]["ULDNo"].ToString())
                                                        {
                                                            UNID = ds1.Tables[0].Rows[j]["UNID"].ToString();

                                                        }

                                                    }
                                                }

                                                if (!await ExportManifestDetails(ULDNo, ffmdata.fltairportcode, unloadingport[refNum].unloadingairport, consinfo[i].origin, consinfo[i].dest, consinfo[i].awbnum, consinfo[i].splhandling.Trim(','), consinfo[i].volumeamt == "" ? "0.01" : consinfo[i].volumeamt, consinfo[i].pcscnt, consinfo[i].weight,
                                                    consinfo[i].manifestdesc, flightdate, ManifestID, flightnum, consinfo[i].airlineprefix, consinfo[i].weightcode == "" ? "L" : consinfo[i].weightcode, ffmSequenceNo, TotalAWBPcs, UNID, REFNo, consinfo[i].IsBup == "true" ? true : false, consinfo[i].densityindicator, consinfo[i].consigntype, ffmdata.endmesgcode))
                                                    // clsLog.WriteLogAzure("Error in FFM Manifest Details:" + consinfo[i].awbnum);
                                                    _logger.LogWarning("Error in FFM Manifest Details: {0}" , consinfo[i].awbnum);
                                                else
                                                {
                                                    if (!await ExportManifestULDAWBAssociation(ULDNo, ffmdata.fltairportcode, unloadingport[refNum].unloadingairport, consinfo[i].origin, consinfo[i].dest, consinfo[i].awbnum, consinfo[i].splhandling.Trim(',') == "" ? "GEN" : consinfo[i].splhandling.Trim(','), consinfo[i].volumeamt == "" ? "0" : consinfo[i].volumeamt, consinfo[i].pcscnt, consinfo[i].weight, consinfo[i].manifestdesc, flightdate, ManifestID, consinfo[i].airlineprefix, ffmdata.carriercode + ffmdata.fltnum, consinfo[i].numshp, consinfo[i].weight, consinfo[i].consigntype, TotalAWBPcs, ffmSequenceNo, consinfo[i].weightcode, REFNo, consinfo[i].IsBup == "true" ? true : false))
                                                        // clsLog.WriteLogAzure("Error in FFM Manifest Details:" + consinfo[i].awbnum);
                                                        _logger.LogWarning("Error in FFM Manifest Details:{0}" , consinfo[i].awbnum);

                                                    //sqlServer = new SQLServer();
                                                    //string[] PVName = new string[] { "AWBPrefix", "AWBNumber", "MType", "desc", "date", "time", "refno", "FlightNo", "FlightDate", "PCS", "WT", "UOM", "UpdatedBy", "UpdatedOn", "StnCode" };
                                                    //object[] PValues = new object[] { consinfo[i].airlineprefix, consinfo[i].awbnum, "FFM", ffmdata.fltairportcode + "-" + flightnum + "-" + flightdate, "", "", 0, ffmdata.carriercode + ffmdata.fltnum, flightdate, TotalAWBPcs, Val, consinfo[i].weightcode, "FFM", flightdate, ffmdata.fltairportcode };
                                                    //SqlDbType[] sqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                                                    //sqlServer.InsertData("spInsertAWBMessageStatus", PVName, sqlType, PValues);

                                                    SqlParameter[] parametersMT =
                                                    {
                                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar)     { Value = consinfo[i].airlineprefix },
                                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar)     { Value = consinfo[i].awbnum },
                                                        new SqlParameter("@MType", SqlDbType.VarChar)         { Value = "FFM" },
                                                        new SqlParameter("@desc", SqlDbType.VarChar)          { Value = ffmdata.fltairportcode + "-" + flightnum + "-" + flightdate },
                                                        new SqlParameter("@date", SqlDbType.NVarChar)         { Value = "" },
                                                        new SqlParameter("@time", SqlDbType.NVarChar)         { Value = "" },
                                                        new SqlParameter("@refno", SqlDbType.Int)            { Value = 0 },
                                                        new SqlParameter("@FlightNo", SqlDbType.VarChar)      { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                        new SqlParameter("@FlightDate", SqlDbType.DateTime)   { Value = flightdate },
                                                        new SqlParameter("@PCS", SqlDbType.Int)              { Value = TotalAWBPcs },
                                                        new SqlParameter("@WT", SqlDbType.Decimal)           { Value = Val },
                                                        new SqlParameter("@UOM", SqlDbType.VarChar)          { Value = consinfo[i].weightcode },
                                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar)    { Value = "FFM" },
                                                        new SqlParameter("@UpdatedOn", SqlDbType.DateTime)    { Value = flightdate },
                                                        new SqlParameter("@StnCode", SqlDbType.VarChar)      { Value = ffmdata.fltairportcode }
                                                    };
                                                    await _readWriteDao.ExecuteNonQueryAsync("spInsertAWBMessageStatus", parametersMT);


                                                    //sqlServer = new SQLServer();
                                                    //string[] CNname = new string[] { "AWBPrefix", "AWBNumber", "Origin", "Destination", "Pieces", "Weight", "FlightNo", "FlightDate", "FlightOrigin", "FlightDestination", "Action", "Message", "Description", "UpdatedBy", "UpdatedOn", "Public", "ULDNo", "Shipmenttype", "Volume" };
                                                    //SqlDbType[] CType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                                                    //object[] CValues = new object[] { consinfo[i].airlineprefix, consinfo[i].awbnum, consinfo[i].origin, consinfo[i].dest, consinfo[i].pcscnt, consinfo[i].weight, ffmdata.carriercode + ffmdata.fltnum, flightdate, ffmdata.fltairportcode, unloadingport[refNum].unloadingairport, "Departed", "AWB Departed", "AWB Departed In (" + ULDNo.ToUpper() + ")", "FFM", DateTime.UtcNow.ToString(), 1, ULDNo.ToUpper(), consinfo[i].consigntype, consinfo[i].volumeamt.Trim() == string.Empty ? "0" : consinfo[i].volumeamt.Trim() };
                                                    //if (!sqlServer.ExecuteProcedure("SPAddAWBAuditLog", CNname, CType, CValues, 120000))
                                                    //{
                                                    //    clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);
                                                    //}
                                                    SqlParameter[] parametersOrg =
                                                    {
                                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar)       { Value = consinfo[i].airlineprefix },
                                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar)       { Value = consinfo[i].awbnum },
                                                        new SqlParameter("@Origin", SqlDbType.VarChar)          { Value = consinfo[i].origin },
                                                        new SqlParameter("@Destination", SqlDbType.VarChar)     { Value = consinfo[i].dest },
                                                        new SqlParameter("@Pieces", SqlDbType.VarChar)          { Value = consinfo[i].pcscnt },
                                                        new SqlParameter("@Weight", SqlDbType.VarChar)          { Value = consinfo[i].weight },
                                                        new SqlParameter("@FlightNo", SqlDbType.VarChar)        { Value = ffmdata.carriercode + ffmdata.fltnum },
                                                        new SqlParameter("@FlightDate", SqlDbType.DateTime)     { Value = flightdate },
                                                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar)    { Value = ffmdata.fltairportcode },
                                                        new SqlParameter("@FlightDestination", SqlDbType.VarChar){ Value = unloadingport[refNum].unloadingairport },
                                                        new SqlParameter("@Action", SqlDbType.VarChar)          { Value = "Departed" },
                                                        new SqlParameter("@Message", SqlDbType.VarChar)         { Value = "AWB Departed" },
                                                        new SqlParameter("@Description", SqlDbType.VarChar)     { Value = "AWB Departed In (" + ULDNo.ToUpper() + ")" },
                                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar)       { Value = "FFM" },
                                                        new SqlParameter("@UpdatedOn", SqlDbType.VarChar)       { Value = DateTime.UtcNow.ToString() },
                                                        new SqlParameter("@Public", SqlDbType.Bit)             { Value = 1 },
                                                        new SqlParameter("@ULDNo", SqlDbType.VarChar)           { Value = ULDNo.ToUpper() },
                                                        new SqlParameter("@Shipmenttype", SqlDbType.VarChar)    { Value = consinfo[i].consigntype },
                                                        new SqlParameter("@Volume", SqlDbType.VarChar)          { Value = string.IsNullOrWhiteSpace(consinfo[i].volumeamt) ? "0" : consinfo[i].volumeamt.Trim() }
                                                    };

                                                    if (!await _readWriteDao.ExecuteNonQueryAsync("SPAddAWBAuditLog", parametersOrg, 120000))
                                                    {
                                                        //clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine + "Error: " + sqlServer.LastErrorDescription);
                                                        // clsLog.WriteLog("AWB Audit log  for:" + consinfo[i].awbnum + Environment.NewLine );
                                                        _logger.LogWarning("AWB Audit log  for: {0}" , consinfo[i].awbnum + Environment.NewLine );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion Check Availabe ULD data if Present insert into ExportManifestULDAWB Association

                                    #region Reprocess the Consigment Info
                                    FFMConsig = new MessageData.consignmnetinfo[consinfo.Length];
                                    Array.Copy(consinfo, FFMConsig, consinfo.Length);
                                    consinfo = new MessageData.consignmnetinfo[0];
                                    ReProcessConsigment(ref FFMConsig, ref consinfo);
                                    #endregion Reprocess the Consigment Info

                                    #region Add dimensions details
                                    if (consinfo.Length > 0)
                                    {
                                        for (int i = 0; i < consinfo.Length; i++)
                                        {
                                            if (!lstDeliveredAWB.Contains(consinfo[i].awbnum))
                                            {
                                                string AWBNum = consinfo[i].awbnum, AWBPrefix = consinfo[i].airlineprefix;
                                                int strAwbPcs = 0;

                                                if (consinfo[i].numshp != "")
                                                    strAwbPcs = int.Parse(consinfo[i].numshp == "" ? "0" : consinfo[i].numshp);
                                                else
                                                    strAwbPcs = int.Parse(consinfo[i].pcscnt == "" ? "0" : consinfo[i].pcscnt);

                                                try
                                                {
                                                    if (objDimension.Length > 0 || uld.Length > 0)
                                                    {
                                                        DataSet dsDimension = await GenertateAWBDimensions(AWBNum, strAwbPcs, null, Convert.ToDecimal(consinfo[i].weight), "FFM", DateTime.Now, false, AWBPrefix);

                                                        for (int j = 0; j < objDimension.Length; j++)
                                                        {
                                                            if (objDimension[j].AWBPrefix == consinfo[i].airlineprefix && objDimension[j].AWBNumber == consinfo[i].awbnum)
                                                            {
                                                                if (objDimension[j].mesurunitcode.Trim() != "")
                                                                {
                                                                    if (objDimension[j].mesurunitcode.Trim().ToUpper() == "CMT")
                                                                        objDimension[j].mesurunitcode = "Cms";
                                                                    else if (objDimension[j].mesurunitcode.Trim().ToUpper() == "INH")
                                                                        objDimension[j].mesurunitcode = "Inches";
                                                                }
                                                                if (objDimension[j].length.Trim() == "")
                                                                    objDimension[j].length = "0";
                                                                if (objDimension[j].width.Trim() == "")
                                                                    objDimension[j].width = "0";
                                                                if (objDimension[j].height.Trim() == "")
                                                                    objDimension[j].height = "0";

                                                                if (dsDimension != null)
                                                                {
                                                                    if (dsDimension.Tables[0].Rows.Count < j + 1)
                                                                    {
                                                                        DataRow dr = dsDimension.Tables[0].NewRow();
                                                                        dsDimension.Tables[0].Rows.Add(dr);
                                                                    }
                                                                    dsDimension.Tables[0].Rows[j]["Length"] = Convert.ToInt16(objDimension[j].length);
                                                                    if (dsDimension.Tables[0].Columns.Contains("Breath"))
                                                                        dsDimension.Tables[0].Rows[j]["Breath"] = Convert.ToInt16(objDimension[j].width);
                                                                    else
                                                                        dsDimension.Tables[0].Rows[j]["breadth"] = Convert.ToInt16(objDimension[j].width);
                                                                    dsDimension.Tables[0].Rows[j]["Height"] = Convert.ToInt16(objDimension[j].height);
                                                                    dsDimension.Tables[0].Rows[j]["Units"] = objDimension[j].mesurunitcode;
                                                                }

                                                            }
                                                        }
                                                        ///Need more clarification on, how to capture dims from FFM 
                                                        ///1. In case of BUP
                                                        ///2. existing dimensions has multiple  dims lines
                                                        //if (objDimension.Length > 0)
                                                        //    GenertateAWBDimensions(AWBNum, Convert.ToInt16(consinfo[i].pcscnt), dsDimension, Convert.ToDecimal(consinfo[i].weight), "FFM", System.DateTime.Now, true, AWBPrefix);
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
                                    #endregion Add dimensions details

                                    List<string> lstAcceptedAWB = new List<string>();
                                    string FFMPiecesCode = string.Empty;
                                    for (int route = 0; route < dtWeightUpdate.Rows.Count; route++)
                                    {
                                        if (!lstDeliveredAWB.Contains(dtWeightUpdate.Rows[route]["AWBNo"].ToString()))
                                        {

                                            //sqlServer = new SQLServer();
                                            DataSet? dscheckroute = new DataSet();
                                            //string[] pnameroute = new string[] { "AWBNumber", "AWBPrefix" };
                                            //object[] valuesroute = new object[] { dtWeightUpdate.Rows[route]["AWBNo"].ToString(), dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() };
                                            //SqlDbType[] ptyperoute = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                                            //dscheckroute = sqlServer.SelectRecords("sp_getawbdetails", pnameroute, valuesroute, ptyperoute);
                                            
                                            SqlParameter[] parametersRoute =
                                            {
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() }
                                            };
                                            
                                            dscheckroute = await _readWriteDao.SelectRecords("sp_getawbdetails", parametersRoute);

                                            //string[] pchangeAWB = new string[] { "AWBNumber", "AWBPrefix", "AWBOrigin", "AWBDestination", "IsDestinationMismatch" };
                                            //SqlDbType[] PchangeAWBtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Bit };
                                            //object[] pchangeAWbValue = new object[] { dtWeightUpdate.Rows[route]["AWBNo"].ToString(), dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(), dtWeightUpdate.Rows[route]["Origin"].ToString(), dtWeightUpdate.Rows[route]["Destination"].ToString(), isDestinationMismatch };
                                            //if (!sqlServer.UpdateData("MSG_uspUpdateShipmentDestinationthroughFFM", pchangeAWB, PchangeAWBtype, pchangeAWbValue))
                                            //clsLog.WriteLogAzure("Error in Update AWB Destination" + sqlServer.LastErrorDescription);

                                            SqlParameter[] parametersChangeAWB =
                                            {
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                                new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["Origin"].ToString() },
                                                new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["Destination"].ToString() },
                                                new SqlParameter("@IsDestinationMismatch", SqlDbType.Bit) { Value = isDestinationMismatch }
                                            };

                                            if (!await _readWriteDao.ExecuteNonQueryAsync("MSG_uspUpdateShipmentDestinationthroughFFM", parametersChangeAWB))
                                                // clsLog.WriteLogAzure("Error in Update AWB Destination");
                                                _logger.LogWarning("Error in Update AWB Destination");

                                            decimal ChargeableWeight = 0, TotalChargeableWeight = 0;
                                            ChargeableWeight = Convert.ToDecimal(dtWeightUpdate.Rows[route]["GrossWt"].ToString() == "" ? "0" : dtWeightUpdate.Rows[route]["GrossWt"]);

                                            //sqlServer = new SQLServer();
                                            //string[] cPname = new string[] { "AWBNumber", "AWBPrefix" };
                                            //SqlDbType[] checkPtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                                            //object[] ccheckpavlue = new object[] { dtWeightUpdate.Rows[route]["AWBNo"].ToString(), dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() };
                                            //DataSet dscheckawbn = sqlServer.SelectRecords("MSG_uSPCheckBookingDoneByEDIMessage", cPname, ccheckpavlue, checkPtype);

                                            SqlParameter[] checkParameters =
                                            {
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() }
                                            };

                                            DataSet? dscheckawbn = await _readWriteDao.SelectRecords("MSG_uSPCheckBookingDoneByEDIMessage", checkParameters);
                                            if (dscheckawbn != null && dscheckawbn.Tables.Count > 0 && dscheckawbn.Tables[0].Rows.Count > 0)
                                            {
                                                if (dscheckawbn.Tables[0].Rows[0]["AWBCreatedStatus"].ToString() == "1")
                                                {
                                                    #region Make AWB Route through FFM Message
                                                    MessageData.FltRoute[] fltroute = new MessageData.FltRoute[0];
                                                    MessageData.FltRoute flight = new MessageData.FltRoute("");
                                                    if (!(dtWeightUpdate.Rows[route]["Origin"].ToString() == source && dest == dtWeightUpdate.Rows[route]["Destination"].ToString()))
                                                    {
                                                        if (dtWeightUpdate.Rows[route]["Origin"].ToString() == source)
                                                        {
                                                            flight.carriercode = flightnum.Substring(0, 2);
                                                            flight.fltnum = flightnum;
                                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                                            flight.fltdept = dtWeightUpdate.Rows[route]["Origin"].ToString();
                                                            flight.fltarrival = dest;

                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;

                                                            if (dest != dtWeightUpdate.Rows[route]["Destination"].ToString())
                                                            {
                                                                flight.carriercode = string.Empty;
                                                                flight.fltnum = string.Empty;
                                                                flight.date = flightdate.ToString("MM/dd/yyyy");
                                                                flight.fltdept = dest;
                                                                flight.fltarrival = dtWeightUpdate.Rows[route]["Destination"].ToString();
                                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                                fltroute[fltroute.Length - 1] = flight;
                                                            }
                                                        }

                                                        if (dtWeightUpdate.Rows[route]["Origin"].ToString() != source)
                                                        {
                                                            flight.carriercode = string.Empty;
                                                            flight.fltnum = string.Empty;
                                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                                            flight.fltdept = dtWeightUpdate.Rows[route]["Origin"].ToString();
                                                            DataTable dtRoute = dscheckroute.Tables[3];
                                                            DataRow _dr = dtRoute.AsEnumerable().Where(r => r.Field<string>("FltOrigin").Trim().Equals(dtWeightUpdate.Rows[route]["Origin"].ToString())).FirstOrDefault();

                                                            string fltdestination = string.Empty;

                                                            if (_dr != null)
                                                            {
                                                                fltdestination = Convert.ToString(_dr["FltDestination"].ToString());
                                                            }

                                                            if (!string.IsNullOrEmpty(fltdestination))
                                                                flight.fltarrival = fltdestination;
                                                            else
                                                                flight.fltarrival = source;

                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;

                                                            if (source != dtWeightUpdate.Rows[route]["Destination"].ToString())
                                                            {
                                                                if (fltdestination != source)
                                                                {
                                                                    flight.carriercode = string.Empty;
                                                                    flight.fltnum = string.Empty;
                                                                    flight.date = flightdate.ToString("MM/dd/yyyy");
                                                                    flight.fltdept = fltdestination;
                                                                    flight.fltarrival = source;
                                                                    Array.Resize(ref fltroute, fltroute.Length + 1);
                                                                    fltroute[fltroute.Length - 1] = flight;
                                                                }
                                                                flight.carriercode = flightnum.Substring(0, 2);
                                                                flight.fltnum = flightnum;
                                                                flight.date = flightdate.ToString("MM/dd/yyyy");
                                                                flight.fltdept = source;
                                                                flight.fltarrival = dest;
                                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                                fltroute[fltroute.Length - 1] = flight;
                                                            }

                                                            if (dest != dtWeightUpdate.Rows[route]["destination"].ToString())
                                                            {
                                                                flight.carriercode = string.Empty;
                                                                flight.fltnum = string.Empty;
                                                                flight.date = flightdate.ToString("MM/dd/yyyy");
                                                                flight.fltdept = dest;
                                                                flight.fltarrival = dtWeightUpdate.Rows[route]["destination"].ToString();
                                                                Array.Resize(ref fltroute, fltroute.Length + 1);
                                                                fltroute[fltroute.Length - 1] = flight;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (dtWeightUpdate.Rows[route]["Origin"].ToString() == source && dest == dtWeightUpdate.Rows[route]["Destination"].ToString())
                                                        {
                                                            flight.carriercode = flightnum.Substring(0, 2);
                                                            flight.fltnum = flightnum;
                                                            flight.date = flightdate.ToString("MM/dd/yyyy");
                                                            flight.fltdept = dtWeightUpdate.Rows[route]["Origin"].ToString();
                                                            flight.fltarrival = dest;
                                                            Array.Resize(ref fltroute, fltroute.Length + 1);
                                                            fltroute[fltroute.Length - 1] = flight;
                                                        }
                                                    }
                                                    if (fltroute.Length > 0)
                                                    {
                                                        for (int row = 0; row < fltroute.Length; row++)
                                                        {
                                                            if (dscheckroute != null && dscheckroute.Tables.Count > 2)
                                                            {
                                                                if (fltroute[row].fltnum == string.Empty)
                                                                {
                                                                    for (int z = 0; z < dscheckroute.Tables[3].Rows.Count; z++)
                                                                    {
                                                                        if (dscheckroute.Tables[3].Rows[z]["FltOrigin"].ToString() == fltroute[row].fltdept
                                                                            && dscheckroute.Tables[3].Rows[z]["FltDestination"].ToString() == fltroute[row].fltarrival)
                                                                        {
                                                                            fltroute[row].fltnum = dscheckroute.Tables[3].Rows[z]["FltNumber"].ToString();
                                                                            fltroute[row].date = dscheckroute.Tables[3].Rows[z]["FltDate"].ToString();
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            int RemainingPcs = 0, strManifestPcs = 0;
                                                            if (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "P" || dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "D")
                                                            {
                                                                strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                                RemainingPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) - int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                            }
                                                            else
                                                            {
                                                                strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString());
                                                            }
                                                            //sqlServer = new SQLServer();
                                                            //string[] RName = new string[]
                                                            //{
                                                            //    "AWBNumber", "FltOrigin","FltDestination","FltNumber","FltDate","Status","UpdatedBy","UpdatedOn","IsFFR", "REFNo"
                                                            //    ,"date","AWBPrefix",  "MftPcs","MftWt", "ConsignmentType","FFMFlightOrigin","FFMFlightDestination","RemainingPcs"
                                                            //    ,"FFMPiecesCode","TotalAWBPcs","ManifestID", "FFMSequenceNo","ChargeableWeight","AWBOrigin", "AWBDestination","Volume","VolumeUnit"
                                                            //};
                                                            //SqlDbType[] RType = new SqlDbType[]
                                                            //{
                                                            //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime,  SqlDbType.Bit, SqlDbType.Int
                                                            //    ,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Int
                                                            //    ,SqlDbType.VarChar,SqlDbType.Int,SqlDbType.Int,SqlDbType.Int, SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar
                                                            //    , SqlDbType.Decimal,SqlDbType.VarChar
                                                            //};
                                                            //object[] RValue = new object[]
                                                            //{
                                                            //    dtWeightUpdate.Rows[route ]["AWBNo"].ToString(), fltroute[row].fltdept, fltroute[row].fltarrival,fltroute[row].fltnum, fltroute[row].date, "C",  "FFM",  DateTime.Now, 1,   0
                                                            //    , flightdate, dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(),strManifestPcs,Convert.ToDecimal(dtWeightUpdate.Rows[route]["GrossWt"].ToString()),dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString(), source,dest,RemainingPcs
                                                            //    , dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString(),int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()),ManifestID,ffmSequenceNo,ChargeableWeight, dtWeightUpdate.Rows[route]["Origin"].ToString(), dtWeightUpdate.Rows[route]["Destination"].ToString()
                                                            //    , Convert.ToDecimal(dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim() == string.Empty ? "0" : dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim())
                                                            //    , dtWeightUpdate.Rows[route]["VolumeCode"].ToString()
                                                            //};
                                                            //sqlServer = new SQLServer();
                                                            //if (!sqlServer.UpdateData("MSG_spSaveFFMAWBRoute", RName, RType, RValue))
                                                            //    clsLog.WriteLogAzure("Error in Save AWB Route FFM " + sqlServer.LastErrorDescription);

                                                            SqlParameter[] routeParameters =
                                                            {
                                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                                new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = fltroute[row].fltdept },
                                                                new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = fltroute[row].fltarrival },
                                                                new SqlParameter("@FltNumber", SqlDbType.VarChar) { Value = fltroute[row].fltnum },
                                                                new SqlParameter("@FltDate", SqlDbType.VarChar) { Value = fltroute[row].date },
                                                                new SqlParameter("@Status", SqlDbType.VarChar) { Value = "C" },
                                                                new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFM" },
                                                                new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                                                new SqlParameter("@IsFFR", SqlDbType.Bit) { Value = 1 },
                                                                new SqlParameter("@REFNo", SqlDbType.Int) { Value = 0 },
                                                                new SqlParameter("@date", SqlDbType.VarChar) { Value = flightdate },
                                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                                                new SqlParameter("@MftPcs", SqlDbType.VarChar) { Value = strManifestPcs },
                                                                new SqlParameter("@MftWt", SqlDbType.VarChar) { Value = Convert.ToDecimal(dtWeightUpdate.Rows[route]["GrossWt"].ToString()) },
                                                                new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString() },
                                                                new SqlParameter("@FFMFlightOrigin", SqlDbType.VarChar) { Value = source },
                                                                new SqlParameter("@FFMFlightDestination", SqlDbType.VarChar) { Value = dest },
                                                                new SqlParameter("@RemainingPcs", SqlDbType.Int) { Value = RemainingPcs },
                                                                new SqlParameter("@FFMPiecesCode", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() },
                                                                new SqlParameter("@TotalAWBPcs", SqlDbType.Int) { Value = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) },
                                                                new SqlParameter("@ManifestID", SqlDbType.Int) { Value = ManifestID },
                                                                new SqlParameter("@FFMSequenceNo", SqlDbType.Int) { Value = ffmSequenceNo },
                                                                new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                                                                new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["Origin"].ToString() },
                                                                new SqlParameter("@AWBDestination", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["Destination"].ToString() },
                                                                new SqlParameter("@Volume", SqlDbType.Decimal)
                                                                {
                                                                    Value = Convert.ToDecimal(
                                                                        dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim() == string.Empty
                                                                        ? "0"
                                                                        : dtWeightUpdate.Rows[route]["VolumeWt"].ToString().Trim())
                                                                },
                                                                new SqlParameter("@VolumeUnit", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["VolumeCode"].ToString() }
                                                            };

                                                            if (!await _readWriteDao.ExecuteNonQueryAsync("MSG_spSaveFFMAWBRoute", routeParameters))
                                                                // clsLog.WriteLogAzure("Error in Save AWB Route FFM " );
                                                                _logger.LogWarning("Error in Save AWB Route FFM " );


                                                        }
                                                    }
                                                    #endregion Make AWB Route through FFM Message
                                                }
                                                else
                                                {
                                                    #region Update AWB Route Information
                                                    ///we need to add code, if FFR contain 1 leg and FFM contains transit route
                                                    ///in this case we need to form a route.
                                                    int RemainingPcs = 0, strManifestPcs = 0;
                                                    if (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "P" || dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "D")
                                                    {
                                                        strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                        RemainingPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) - int.Parse(dtWeightUpdate.Rows[route]["FFMPieces"].ToString());
                                                    }
                                                    else
                                                    {
                                                        strManifestPcs = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString());
                                                    }

                                                    //string[] RName = new string[]
                                                    //{
                                                    //    "AWBPrefix","AWBNumber","FltOrigin","FltDestination","FltNumber","FltDate","Status","MftPcs","MftWt","ConsignmentType",
                                                    //    "UpdatedBy","UpdatedOn","RemainingPcs","FFMPiecesCode","Date","TotalAWBPcs","ManifestID", "FFMSequenceNo","ChargeableWeight","REFNo","Volume","VolumeUnit","AWBOrigin"
                                                    //};
                                                    //SqlDbType[] RType = new SqlDbType[]
                                                    //{
                                                    //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,
                                                    //    SqlDbType.VarChar,SqlDbType.Int,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.VarChar, SqlDbType.DateTime,SqlDbType.Int,
                                                    //    SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.Int,SqlDbType.Int,SqlDbType.Int,SqlDbType.Decimal,SqlDbType.VarChar,SqlDbType.Decimal ,SqlDbType.VarChar,SqlDbType.VarChar
                                                    //};

                                                    //object[] RValue = new object[]
                                                    //{
                                                    //    dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(),dtWeightUpdate.Rows[route]["AWBNo"].ToString(),source, dest,flightnum,flightdate, "C",  strManifestPcs
                                                    //    ,decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()), dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString()
                                                    //    ,"FFM",DateTime.Now,RemainingPcs,dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString(),flightdate
                                                    //    ,int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()),ManifestID,ffmSequenceNo,ChargeableWeight,REFNo
                                                    //    ,decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()),dtWeightUpdate.Rows[route]["VolumeCode"].ToString(),dtWeightUpdate.Rows[route]["Origin"].ToString()
                                                    //};
                                                    //sqlServer = new SQLServer();

                                                    SqlParameter[] routeParams =
                                                    {
                                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                        new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = source },
                                                        new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = dest },
                                                        new SqlParameter("@FltNumber", SqlDbType.VarChar) { Value = flightnum },
                                                        new SqlParameter("@FltDate", SqlDbType.VarChar) { Value = flightdate },
                                                        new SqlParameter("@Status", SqlDbType.VarChar) { Value = "C" },
                                                        new SqlParameter("@MftPcs", SqlDbType.Int) { Value = strManifestPcs },
                                                        new SqlParameter("@MftWt", SqlDbType.Decimal) { Value = decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()) },
                                                        new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWbPieceCode"].ToString() },
                                                        new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFM" },
                                                        new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                                                        new SqlParameter("@RemainingPcs", SqlDbType.Int) { Value = RemainingPcs },
                                                        new SqlParameter("@FFMPiecesCode", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() },
                                                        new SqlParameter("@Date", SqlDbType.VarChar) { Value = flightdate },
                                                        new SqlParameter("@TotalAWBPcs", SqlDbType.Int) { Value = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) },
                                                        new SqlParameter("@ManifestID", SqlDbType.Int) { Value = ManifestID },
                                                        new SqlParameter("@FFMSequenceNo", SqlDbType.Int) { Value = ffmSequenceNo },
                                                        new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = ChargeableWeight },
                                                        new SqlParameter("@REFNo", SqlDbType.VarChar) { Value = REFNo },
                                                        new SqlParameter("@Volume", SqlDbType.Decimal)
                                                        {
                                                            Value = decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString())
                                                        },
                                                        new SqlParameter("@VolumeUnit", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["VolumeCode"].ToString() },
                                                        new SqlParameter("@AWBOrigin", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["Origin"].ToString() }
                                                    };


                                                    DataSet? dsAWBAccted = new DataSet();
                                                    //string[] pname = new string[] { "AWBNumber", "AWBPrefix" };
                                                    //object[] values = new object[] { dtWeightUpdate.Rows[route]["AWBNo"].ToString(), dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() };
                                                    //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar };
                                                    //dsAWBAccted = sqlServer.SelectRecords("sp_getawbdetails", pname, values, ptype);

                                                    SqlParameter[] parametersAWB = new SqlParameter[]
                                                    {
                                                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) {Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString()},
                                                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) {Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString()}
                                                    };
                                                    dsAWBAccted = await _readWriteDao.SelectRecords("sp_getawbdetails", parametersAWB);

                                                    if (dsAWBAccted != null && dsAWBAccted.Tables.Count > 3)
                                                    {
                                                        if (dsAWBAccted.Tables[3].Rows.Count > 0)
                                                        {
                                                            if (dsAWBAccted.Tables[3].Rows[0]["Accepted"].ToString().Equals("Y", StringComparison.OrdinalIgnoreCase))
                                                                lstAcceptedAWB.Add(dtWeightUpdate.Rows[route]["AWBNo"].ToString());
                                                        }
                                                    }
                                                    //sqlServer = new SQLServer();
                                                    //if (!sqlServer.UpdateData("MSG_uSpUpdateAWBRouteInformationthroughFFM", RName, RType, RValue))
                                                    //    clsLog.WriteLogAzure("Error in Save AWB Route FFM " + sqlServer.LastErrorDescription);


                                                    if (!await _readWriteDao.ExecuteNonQueryAsync("MSG_uSpUpdateAWBRouteInformationthroughFFM", routeParams))
                                                        // clsLog.WriteLogAzure("Error in Save AWB Route FFM ");
                                                        _logger.LogWarning("Error in Save AWB Route FFM ");
                                                    #endregion Update AWB Route Information
                                                }
                                            }
                                            #region Update Weight and Pieces in Awbsummarymaster and AWBRoutemaster
                                            string isbup = string.Empty;
                                            if (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "P")
                                            {
                                                for (int i = 0; i < consinfo.Length; i++)
                                                {
                                                    if (consinfo[i].airlineprefix == dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() && consinfo[i].awbnum == dtWeightUpdate.Rows[route]["AWBNo"].ToString() && consinfo[i].IsBup.Equals("true"))
                                                    {
                                                        isbup = "true";
                                                        break;
                                                    }
                                                }
                                            }
                                            FFMPiecesCode = (dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString() == "" ? "T" : dtWeightUpdate.Rows[route]["FFMPiecesCode"].ToString());
                                            //string[] PVName = new string[] { "AWBPrefix", "AWBNumber", "TotalAWBPcs", "TotalAWBWeight", "VolumeCode", "VolumeWeight", "ManifestID", "ConsignmentType", "ChargeableWeight", "SystemDate", "ComodityCode", "ComodityDesc", "isbup" };
                                            //object[] PValues = new object[] { dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString(), dtWeightUpdate.Rows[route]["AWBNo"].ToString(), int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()), decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()), dtWeightUpdate.Rows[route]["VolumeCode"].ToString(), decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()), ManifestID, FFMPiecesCode, TotalChargeableWeight, DateTime.UtcNow, dtWeightUpdate.Rows[route]["ComodityCode"].ToString(), dtWeightUpdate.Rows[route]["ComodityDesc"].ToString(), isbup };
                                            //SqlDbType[] sqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Decimal, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                                            ///sqlServer = new SQLServer();
                                            //sqlServer.InsertData("MSG_spUpdateTotalWeightAndPiecesfromFFM", PVName, sqlType, PValues);


                                            SqlParameter[] parametersAWBP = new SqlParameter[]
                                            {
                                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AirlinePrefix"].ToString() },
                                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["AWBNo"].ToString() },
                                                new SqlParameter("@TotalAWBPcs", SqlDbType.Int) { Value = int.Parse(dtWeightUpdate.Rows[route]["AWBPieces"].ToString()) },
                                                new SqlParameter("@TotalAWBWeight", SqlDbType.Decimal) { Value = decimal.Parse(dtWeightUpdate.Rows[route]["GrossWt"].ToString()) },
                                                new SqlParameter("@VolumeCode", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["VolumeCode"].ToString() },
                                                new SqlParameter("@VolumeWeight", SqlDbType.Decimal) { Value = decimal.Parse(dtWeightUpdate.Rows[route]["VolumeWt"].ToString()) },
                                                new SqlParameter("@ManifestID", SqlDbType.Int) { Value = ManifestID },
                                                new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = FFMPiecesCode },
                                                new SqlParameter("@ChargeableWeight", SqlDbType.Decimal) { Value = TotalChargeableWeight },
                                                new SqlParameter("@SystemDate", SqlDbType.DateTime) { Value = DateTime.UtcNow },
                                                new SqlParameter("@ComodityCode", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["ComodityCode"].ToString() },
                                                new SqlParameter("@ComodityDesc", SqlDbType.VarChar) { Value = dtWeightUpdate.Rows[route]["ComodityDesc"].ToString() },
                                                new SqlParameter("@isbup", SqlDbType.VarChar) { Value = isbup }
                                            };
                                            await _readWriteDao.ExecuteNonQueryAsync("MSG_spUpdateTotalWeightAndPiecesfromFFM", parametersAWBP);

                                            #endregion
                                        }
                                    }
                                    #region Billing on Acceptance
                                    if (auditconsinfo.Length > 0)
                                    {
                                        for (int i = 0; i < auditconsinfo.Length; i++)
                                        {
                                            string strAWB = auditconsinfo[i].awbnum;
                                            if (!lstDeliveredAWB.Contains(strAWB))
                                            {
                                                if (!lstAcceptedAWB.Contains(strAWB))
                                                {
                                                    // clsLog.WriteLogAzure("Billing Entry for AC :" + auditconsinfo[i].airlineprefix + "-" + auditconsinfo[i].awbnum);
                                                    _logger.LogInformation("Billing Entry for AC : {0} - {1}" , auditconsinfo[i].airlineprefix , auditconsinfo[i].awbnum);
                                                    FFMPiecesCode = FFMPiecesCode == string.Empty ? "T" : FFMPiecesCode;
                                                    #region Prepare Parameters
                                                    object[] AWBInfo = new object[11];
                                                    int irow = 0;
                                                    AWBInfo.SetValue(auditconsinfo[i].awbnum, irow);
                                                    irow++;
                                                    AWBInfo.SetValue(auditconsinfo[i].airlineprefix, irow);
                                                    irow++;
                                                    string UserName = strMessageFrom;
                                                    AWBInfo.SetValue(UserName, irow);
                                                    irow++;
                                                    AWBInfo.SetValue(DateTime.UtcNow, irow);
                                                    irow++;
                                                    AWBInfo.SetValue(1, irow);
                                                    irow++;
                                                    AWBInfo.SetValue(1, irow);
                                                    irow++;
                                                    AWBInfo.SetValue("B", irow);
                                                    irow++;
                                                    AWBInfo.SetValue(1, irow);
                                                    irow++;
                                                    AWBInfo.SetValue(0, irow);
                                                    irow++;
                                                    AWBInfo.SetValue(0, irow);
                                                    irow++;
                                                    AWBInfo.SetValue("FFM-" + FFMPiecesCode, irow);
                                                    #endregion Prepare Parameters

                                                    string res = "";
                                                    res = await InsertAWBDataInBilling(AWBInfo);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }

                            genericFunction.UpdateInboxFromMessageParameter(REFNo, allAWBsInFFM, flightnum, source, dest, "FFM", strMessageFrom == "" ? strFromID : strMessageFrom, flightdate, false, ffmdata.endmesgcode);
                            UpdateDepartureDataForCapacity(flightnum, flightdate, source);

                        }
                    }
                }

                #region Lying List Alert
                try
                {
                    //sqlServer = new SQLServer();
                    //string[] QueryName = new string[4];
                    //object[] QueryValue = new object[4];
                    //SqlDbType[] QueryType = new SqlDbType[4];

                    //QueryName[0] = "FlightNo";
                    //QueryName[1] = "FlightDate";
                    //QueryName[2] = "FltOrigin";
                    //QueryName[3] = "FltDestination";

                    //QueryType[0] = SqlDbType.VarChar;
                    //QueryType[1] = SqlDbType.DateTime;
                    //QueryType[2] = SqlDbType.VarChar;
                    //QueryType[3] = SqlDbType.VarChar;

                    //QueryValue[0] = ffmdata.carriercode + ffmdata.fltnum;
                    //QueryValue[1] = flightdate;
                    //QueryValue[2] = source;
                    //QueryValue[3] = dest;
                    //sqlServer.InsertData("uspLyingListAlert", QueryName, QueryType, QueryValue);

                    SqlParameter[] parametersFlight = new SqlParameter[]
                    {
                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = ffmdata.carriercode + ffmdata.fltnum },
                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                        new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = source },
                        new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = dest }
                    };

                    await _readWriteDao.ExecuteNonQueryAsync("uspLyingListAlert", parametersFlight);
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure("Lying List Error: " + ex.ToString());
                    _logger.LogError("Lying List Error: {0}" , ex);
                }
                #endregion

                #region Subsequent Leg update from FFM
                try
                {
                    //sqlServer = new SQLServer();
                    //string[] QueryName = new string[4];
                    //object[] QueryValue = new object[4];
                    //SqlDbType[] QueryType = new SqlDbType[4];

                    //QueryName[0] = "FlightNo";
                    //QueryName[1] = "FlightDate";
                    //QueryName[2] = "FlightOrigin";
                    //QueryName[3] = "FlightDestination";

                    //QueryType[0] = SqlDbType.VarChar;
                    //QueryType[1] = SqlDbType.DateTime;
                    //QueryType[2] = SqlDbType.VarChar;
                    //QueryType[3] = SqlDbType.VarChar;

                    //QueryValue[0] = ffmdata.carriercode + ffmdata.fltnum;
                    //QueryValue[1] = flightdate;
                    //QueryValue[2] = source;
                    //QueryValue[3] = dest;
                    //sqlServer.InsertData("uspUpdateSubsequentLegfromFFM", QueryName, QueryType, QueryValue);

                    SqlParameter[] parametersFlightNo = new SqlParameter[]
                    {
                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = ffmdata.carriercode + ffmdata.fltnum },
                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                        new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = source },
                        new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = dest }
                    };

                    await _readWriteDao.ExecuteNonQueryAsync("uspUpdateSubsequentLegfromFFM", parametersFlightNo);
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure("Sub sequent leg update error: " + ex.ToString());
                    _logger.LogError("Sub sequent leg update error: {0}" , ex);
                }
                #endregion

                #region ULD
                string origin = "";
                string destination = "";
                string movement = "OUT";
                string loading = "";

                origin = ffmdata.fltairportcode;

                for (int i = 0; i < uld.Length; i++)
                {
                    try
                    {
                        int refdest = int.Parse(uld[i].portsequence) - 1;
                        destination = unloadingport[refdest].unloadingairport;
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    try
                    {
                        loading = uld[i].uldloadingindicator.Length > 0 ? uld[i].uldloadingindicator : "";
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                    //string[] paramname = new string[]
                    //{
                    //    "ULDNo",
                    //    "LocatedOn",
                    //    "MovType",
                    //    "CargoIndic",
                    //    "Ori",
                    //    "dest",
                    //    "FltNo",
                    //    "UpdateBy",
                    //    "IsBUP",
                    //    "AWBNumber",
                    //    "AWBPrefix"
                    //};

                    //object[] paramvalue = new object[]
                    //{
                    //    uld[i].uldno.Trim(),
                    //    flightdate,
                    //    movement,
                    //    loading,
                    //    origin,
                    //    destination,
                    //    ffmdata.carriercode+ffmdata.fltnum,
                    //    "FFM",
                    //    uld[i].IsBUP.Length>0? Convert.ToBoolean(uld[i].IsBUP):false,
                    //    uld[i].AWBNumber.ToString(),
                    //    uld[i].AWBPrefix.ToString()
                    //};

                    //SqlDbType[] paramtype = new SqlDbType[]
                    //{
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.DateTime,
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.NVarChar,
                    //    SqlDbType.Bit,
                    //    SqlDbType.VarChar,
                    //    SqlDbType.VarChar
                    //};
                    //sqlServer = new SQLServer();
                    //string procedure = "spUpdateviaUCMMsgFFM";
                    //if (!sqlServer.InsertData(procedure, paramname, paramtype, paramvalue))
                    //{
                    //    clsLog.WriteLogAzure("Error in FFM ULD:" + sqlServer.LastErrorDescription);
                    //}
                    SqlParameter[] parametersUL = new SqlParameter[]
                    {
                        new SqlParameter("@ULDNo", SqlDbType.NVarChar) { Value = uld[i].uldno.Trim() },
                        new SqlParameter("@LocatedOn", SqlDbType.DateTime) { Value = flightdate },
                        new SqlParameter("@MovType", SqlDbType.NVarChar) { Value = movement },
                        new SqlParameter("@CargoIndic", SqlDbType.NVarChar) { Value = loading },
                        new SqlParameter("@Ori", SqlDbType.NVarChar) { Value = origin },
                        new SqlParameter("@dest", SqlDbType.NVarChar) { Value = destination },
                        new SqlParameter("@FltNo", SqlDbType.NVarChar) { Value = ffmdata.carriercode + ffmdata.fltnum },
                        new SqlParameter("@UpdateBy", SqlDbType.NVarChar) { Value = "FFM" },
                        new SqlParameter("@IsBUP", SqlDbType.Bit) { Value = uld[i].IsBUP.Length > 0 ? Convert.ToBoolean(uld[i].IsBUP) : false },
                        new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = uld[i].AWBNumber.ToString() },
                        new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = uld[i].AWBPrefix.ToString() }
                    };
                    
                    if (!await _readWriteDao.ExecuteNonQueryAsync("spUpdateviaUCMMsgFFM", parametersUL))
                    {
                        // clsLog.WriteLogAzure("Error in FFM ULD:");
                        _logger.LogWarning("Error in FFM ULD:");
                    }
                }
                #endregion

                #region Billing on depart
                if ((!string.IsNullOrEmpty(genericFunction.ReadValueFromDb("FlownRateProcessing").Trim()) && Convert.ToBoolean(genericFunction.ReadValueFromDb("FlownRateProcessing").Trim())))
                {
                    if (auditconsinfo.Length > 0)
                    {
                        for (int i = 0; i < auditconsinfo.Length; i++)
                        {
                            if (!lstDeliveredAWB.Contains(auditconsinfo[i].awbnum))
                            {
                                // clsLog.WriteLogAzure("Billing Entry for DP :" + auditconsinfo[i].airlineprefix + "-" + auditconsinfo[i].awbnum);
                                _logger.LogInformation("Billing Entry for DP :{0} - {1}" , auditconsinfo[i].airlineprefix , auditconsinfo[i].awbnum);
                                #region Prepare Parameters

                                object[] AWBInfo = new object[11];
                                int irow = 0;
                                AWBInfo.SetValue(auditconsinfo[i].awbnum, irow);
                                irow++;
                                AWBInfo.SetValue(auditconsinfo[i].airlineprefix, irow);
                                irow++;
                                string UserName = strMessageFrom;
                                AWBInfo.SetValue(UserName, irow);
                                irow++;
                                AWBInfo.SetValue(DateTime.UtcNow, irow);
                                irow++;
                                AWBInfo.SetValue(1, irow);
                                irow++;
                                AWBInfo.SetValue(1, irow);
                                irow++;
                                AWBInfo.SetValue("M", irow);
                                irow++;
                                AWBInfo.SetValue(1, irow);
                                irow++;
                                AWBInfo.SetValue(0, irow);
                                irow++;
                                AWBInfo.SetValue(0, irow);
                                irow++;
                                AWBInfo.SetValue("FFM-", irow);

                                #endregion Prepare Parameters

                                string res = "";
                                res = await InsertAWBDataInBilling(AWBInfo);
                            }
                        }
                        AddCharterBillingDetails(flightnum, flightdate, source);
                    }
                }
                if (auditconsinfo.Length > 0)
                {
                    for (int i = 0; i < auditconsinfo.Length; i++)
                    {

                        DataSet? dataSetAWBProrationDetails = new DataSet();
                        //SQLServer sqlServerProration = new SQLServer();

                        //string[] paramNamesProration = { "AWBPrefix", "AWBNumber", "UpdatedBy", "UpdatedOn", "UpdateBooking", "UpdateBilling", "CallFrom" };
                        //object[] paramValuesProration = { auditconsinfo[i].airlineprefix, auditconsinfo[i].awbnum, "FFM", DateTime.Now, 0, 0, "dbo.sp_CalculateAWBRatesReprocess" };
                        //SqlDbType[] paramSqlDbTypesProration = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Bit, SqlDbType.VarChar };

                        //dataSetAWBProrationDetails = sqlServerProration.SelectRecords("ABilling.uspProrationEngine", paramNamesProration, paramValuesProration, paramSqlDbTypesProration);

                        SqlParameter[] parametersProration = new SqlParameter[]
                        {
                            new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = auditconsinfo[i].airlineprefix },
                            new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = auditconsinfo[i].awbnum },
                            new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = "FFM" },
                            new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = 0 },
                            new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = 0 },
                            new SqlParameter("@CallFrom", SqlDbType.VarChar) { Value = "dbo.sp_CalculateAWBRatesReprocess" }
                        };

                        dataSetAWBProrationDetails = await _readWriteDao.SelectRecords("ABilling.uspProrationEngine", parametersProration);
                    }
                }
                #endregion Billing on depart

                #region : Auto Reassign :
                if (!string.IsNullOrEmpty(genericFunction.ReadValueFromDb("AutoReassignAWBOnFFM").Trim()) && Convert.ToBoolean(genericFunction.ReadValueFromDb("AutoReassignAWBOnFFM").Trim()) && (ffmdata.carriercode + ffmdata.fltnum).Trim() != string.Empty && flightdate != null && source.Trim() != string.Empty)
                {
                    //sqlServer = new SQLServer();
                    SqlParameter[] sqlParameter = new SqlParameter[]{
                    new SqlParameter("@FlightNumber",ffmdata.carriercode + ffmdata.fltnum)
                        , new SqlParameter("@FlightDate",flightdate)
                        , new SqlParameter("@POL",source)
                        , new SqlParameter("@POU",dest)
                        , new SqlParameter("@UpdatedBy", "FFM")
                        , new SqlParameter("@UpdatedOn", System.DateTime.UtcNow)
                        , new SqlParameter("@OpsStation", source)
                    };
                    //DataSet dsRefreshCapacity = sqlServer.SelectRecords("uspAutoReassignAWB", sqlParameter);
                    DataSet? dsRefreshCapacity = await _readWriteDao.SelectRecords("uspAutoReassignAWB", sqlParameter);
                }
                #endregion Auto Reassign

                #region : Refresh Capacity :
                if ((ffmdata.carriercode + ffmdata.fltnum).Trim() != string.Empty && flightdate != null && source.Trim() != string.Empty)
                {
                    //sqlServer = new SQLServer();
                    SqlParameter[] sqlParameter = new SqlParameter[]{
                    new SqlParameter("@FlightID",ffmdata.carriercode + ffmdata.fltnum)
                        , new SqlParameter("@FlightDate",flightdate)
                        , new SqlParameter("@Source",source)
                    };
                    //DataSet dsRefreshCapacity = sqlServer.SelectRecords("uspRefreshCapacity", sqlParameter);
                    DataSet? dsRefreshCapacity = await _readWriteDao.SelectRecords("uspRefreshCapacity", sqlParameter);
                }
                #endregion Refresh Capacity

                #region Create Alloment
                try
                {
                    //sqlServer = new SQLServer();
                    //string[] QueryName = new string[4];
                    //object[] QueryValue = new object[4];
                    //SqlDbType[] QueryType = new SqlDbType[4];

                    //QueryName[0] = "FlightNo";
                    //QueryName[1] = "FlightDate";
                    //QueryName[2] = "FltOrigin";
                    //QueryName[3] = "FltDestination";

                    //QueryType[0] = SqlDbType.VarChar;
                    //QueryType[1] = SqlDbType.DateTime;
                    //QueryType[2] = SqlDbType.VarChar;
                    //QueryType[3] = SqlDbType.VarChar;

                    //QueryValue[0] = ffmdata.carriercode + ffmdata.fltnum;
                    //QueryValue[1] = flightdate;
                    //QueryValue[2] = source;
                    //QueryValue[3] = dest;

                    SqlParameter[] sqlParameters = new SqlParameter[]
                    {
                        new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = ffmdata.carriercode + ffmdata.fltnum },
                        new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                        new SqlParameter("@FltOrigin", SqlDbType.VarChar) { Value = source },
                        new SqlParameter("@FltDestination", SqlDbType.VarChar) { Value = dest }
                    };

                    //sqlServer.InsertData("uspCreateAllotmentFFM", QueryName, QueryType, QueryValue);
                    await _readWriteDao.ExecuteNonQueryAsync("uspCreateAllotmentFFM", sqlParameters);
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure("Create Allotment Error: " + ex.ToString());
                    _logger.LogError("Create Allotment Error: {0}",ex);
                }
                #endregion

                RelayMessages(awbNumbers, ffmdata.carriercode, source, dest, flightnum, flightdate, origin, strMessage, ffmdata.endmesgcode);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ErrorMsg = ex.Message.ToString();
                flag = false;
            }
            //return flag;
            return (flag, ffmdata, consinfo, unloadingport, objDimension, uld, ErrorMsg);
        }
        #endregion Public Methods

        #region :: Private Methods ::
        /// <summary>
        /// Method to update flight capacity after departure
        /// </summary>
        private async Task UpdateDepartureDataForCapacity(string flightnum, DateTime flightdate, string source)
        {
            try
            {
                //SQLServer sqlServer = new SQLServer();
                //string[] paramName = new string[] { "FlightID", "FlightDate", "Source" };
                //object[] paramValues = new object[] { flightnum, flightdate, source };
                //SqlDbType[] paramSqlType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                //sqlServer.InsertData("uspUpdateDepartureDataForCapacity", paramName, paramSqlType, paramValues);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightID", SqlDbType.VarChar)    { Value = flightnum },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = flightdate },
                    new SqlParameter("@Source", SqlDbType.VarChar)     { Value = source }
                };

                await _readWriteDao.ExecuteNonQueryAsync("uspUpdateDepartureDataForCapacity", parameters);

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        /// <summary>
        /// Split the string  by validating number and character
        /// </summary>
        /// <param name="str">String to split</param>
        /// <returns>Array of number and string elements</returns>
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
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                strarr = null;
            }
            return strarr;
        }

        /// <summary>
        /// Method to generate/relay messages when FFM recived
        /// </summary>
        private async Task RelayMessages(string awbNumbers, string carriercode, string source, string dest, string flightnum, DateTime flightdate, string origin, string strMessage, string endmesgcode)
        {
            try
            {
                GenericFunction genericFunction = new GenericFunction();
                if (endmesgcode.Trim().ToUpper() == "LAST")
                {
                    #region : Auto generate FWB and FHL  :
                    bool isAutoSendFWBFHLonFFM = false;
                    if (bool.TryParse(genericFunction.GetConfigurationValues("AutoSendFWBFHLonFFM"), out isAutoSendFWBFHLonFFM))
                    {
                        if (isAutoSendFWBFHLonFFM)
                        {
                            if (awbNumbers.Trim() != string.Empty && awbNumbers.Trim().Length >= 8)
                            {
                                //FWBMessageProcessor fwbMessageProcessor = new FWBMessageProcessor();
                                
                                await _fWBMessageProcessor.GenerateFWB(carriercode, source, dest, flightnum, flightdate, "FFM", DateTime.UtcNow, awbNumbers);

                                //FHLMessageProcessor fhlMessageProcessor = new FHLMessageProcessor();
                                
                                await _fHLMessageProcessor.GenerateFHL(carriercode, source, dest, flightnum, flightdate, "FFM", DateTime.UtcNow, awbNumbers);
                            }
                        }
                    }
                    #endregion
                    #region : Auto generate CXML Message  :
                    bool isAutoSendCXMLonFFM = false;
                    if (bool.TryParse(genericFunction.GetConfigurationValues("AutoSendCXMLonFFM"), out isAutoSendCXMLonFFM))
                    {
                        if (isAutoSendCXMLonFFM)
                        {
                            if (awbNumbers.Trim() != string.Empty && awbNumbers.Trim().Length >= 8)
                            {
                                await GenerateXFWB(carriercode, source, dest, flightnum, flightdate, "FFM", DateTime.UtcNow);

                                await GenerateXFZBMessage(carriercode, source, dest, flightnum, flightdate, "FFM", DateTime.UtcNow, awbNumbers);
                            }
                            GenerateXFFM(carriercode, source, dest, flightnum, flightdate, "FFM", DateTime.UtcNow);

                        }
                    }
                    #endregion
                    #region : Auto Cusotm Message :
                    bool IsAutoGenerateCustomMessage = false;
                    if (bool.TryParse(genericFunction.GetConfigurationValues("IsAutoGenerateCustomMessage"), out IsAutoGenerateCustomMessage))
                    {
                        if (IsAutoGenerateCustomMessage)
                        {
                            // clsLog.WriteLog("IsAutoGenerateCustomMessage: " + genericFunction.GetConfigurationValues("IsAutoGenerateCustomMessage"));
                            _logger.LogInformation("IsAutoGenerateCustomMessage: {0}", genericFunction.GetConfigurationValues("IsAutoGenerateCustomMessage"));
                            GenericFunction generalFunction = new GenericFunction();
                            
                            //CustomsMessageProcessor customsMessageProcessor = new CustomsMessageProcessor();
                            
                            string PartnerCodeOri = generalFunction.GetCountryCode(source);
                            string PartnerCodeDest = generalFunction.GetCountryCode(dest);

                            // clsLog.WriteLog("GenerateCustomMessageXML() Parameters: " + PartnerCodeOri + ":" + PartnerCodeDest + ":" + flightnum + ":" + flightdate.ToString() + ":" + source + ":" + dest);
                            _logger.LogInformation("GenerateCustomMessageXML() Parameters: {0}:{1}:{2}:{3}:{4}:{5}" , PartnerCodeOri, PartnerCodeDest, flightnum, flightdate.ToString(), source, dest);

                            if (PartnerCodeOri != "PH" && PartnerCodeDest == "PH")
                            {
                                await _customsMessageProcessor.GenerateCustomMessageXML(flightnum, flightdate, source, dest, "False", "", awbNumbers, "FFM", DateTime.UtcNow, PartnerCodeDest);
                            }
                            else
                            {

                                DataSet dsCountryCode = generalFunction.GetStationCodeforOMDAC();
                                DataTable dtCountryCode = dsCountryCode.Tables[0];
                                var OrgCountrycodes = dtCountryCode.AsEnumerable().Where(x => x.Field<string>("AirportCode") == origin).FirstOrDefault();
                                var DestCountrycodes = dtCountryCode.AsEnumerable().Where(x => x.Field<string>("AirportCode") == dest).FirstOrDefault();
                                if (OrgCountrycodes != null && DestCountrycodes != null)
                                {
                                    var OrgCountry = OrgCountrycodes[2].ToString();
                                    var DestCountry = OrgCountrycodes[2].ToString();
                                    if (OrgCountry.Trim() == "OM" || DestCountry.Trim() == "OM" || OrgCountry.Trim() == "BD" || DestCountry.Trim() == "BD")
                                    {
                                        string ExpImp = OrgCountry == "OM" ? "1" : DestCountry == "OM" ? "0" : OrgCountry == "BD" ? "1" : "0";

                                        await _customsMessageProcessor.GenerateCustomMessageXML(flightnum, flightdate, origin, dest, ExpImp, "", awbNumbers, "FFM", DateTime.UtcNow, PartnerCodeOri);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #region Relay_FFM
                bool IsRelay_FFM = false;
                if (bool.TryParse(genericFunction.GetConfigurationValues("IsRelay_FFM"), out IsRelay_FFM))
                {
                    string MessageVersion = "8", SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, SFTPHeaderSITAddress = string.Empty;
                    DataSet dsconfiguration = genericFunction.GetSitaAddressandMessageVersion(carriercode, "FFM", "AIR", source, dest, flightnum, string.Empty);

                    if (dsconfiguration != null && dsconfiguration.Tables[0].Rows.Count > 0)
                    {
                        strEmailid = dsconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        MessageVersion = dsconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                        strSITAHeaderType = dsconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString();
                        if (dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                            SitaMessageHeader = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                        if (dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                            SFTPHeaderSITAddress = genericFunction.MakeMailMessageFormat(dsconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), strSITAHeaderType);
                    }
                    if (strMessage.Length > 3)
                    {
                        if (SitaMessageHeader.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("SITA:FFM", SitaMessageHeader.ToString() + "\r\n" + strMessage, "", "SITAFTP", source, dest, flightnum, flightdate.ToString());
                        if (SFTPHeaderSITAddress.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("SITA:FFM", SFTPHeaderSITAddress.ToString() + "\r\n" + strMessage, "", "SFTP", source, dest, flightnum, flightdate.ToString());
                        if (strEmailid.Trim().Length > 0)
                            genericFunction.SaveMessageOutBox("FFM", strMessage, "", strEmailid, source, dest, flightnum, flightdate.ToString());
                    }
                }
                #endregion

                GenerateAutoMessages(source, flightnum, flightdate, "Auto", System.DateTime.UtcNow, dest);

                bool isDEPARRNotification = false;
                if (bool.TryParse(genericFunction.GetConfigurationValues("DEPARRNotification"), out isDEPARRNotification))
                {
                    if (isDEPARRNotification)
                    {

                        string PartnerEmailiD = string.Empty, AWB_Numbers = string.Empty, AWBNumberPrifix = string.Empty, AwbPcs = string.Empty, Pcs = string.Empty;
                        string strAWB = string.Empty;
                        //SQLServer sqlServer = new SQLServer();


                        string[] awbArrray = awbNumbers.Split(',');
                        for (int i = 0; i < awbArrray.Length; i++)
                        {
                            strAWB = string.Empty;
                            string AWBNumber = string.Empty, AWBPrifix = string.Empty, flightNumber = string.Empty, POU = string.Empty, agentcode = string.Empty, partnerCode = string.Empty;
                            string ULD = string.Empty;
                            AWB_Numbers = awbArrray[i].Substring(4, 8).ToString();
                            AWBNumber = awbArrray[i].Trim().Substring(4);
                            AWBPrifix = awbArrray[i].Trim().Substring(0, 3).ToString();

                            //string[] paraName = { "AWBNumber", "AWBPrefix" };
                            //object[] paraValue = { AWBNumber, AWBPrifix };
                            //SqlDbType[] paraType = { SqlDbType.VarChar, SqlDbType.VarChar };
                            //DataSet ds = sqlServer.SelectRecords("SP_GetAWBDetails", paraName, paraValue, paraType);

                            SqlParameter[] parameters =
                            {
                                new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNumber },
                                new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrifix }
                            };

                            string Date = string.Empty;
                            Date = flightdate.ToString("dd/MM/yyyy");
                            DataSet? ds = await  _readWriteDao.SelectRecords("SP_GetAWBDetails", parameters);
                            DataTable dtULD = ds.Tables[11];

                            DataTable tblFiltered = new DataTable();
                            try
                            {
                                tblFiltered = dtULD.AsEnumerable().Where(row => row.Field<string>("FlightNo") == flightnum && row.Field<string>("FlightDate") == Date)
                             .OrderByDescending(row => row.Field<int>("SerialNumber"))
                             .CopyToDataTable();
                            }
                            catch (Exception ex)
                            {
                                // clsLog.WriteLogAzure(ex);
                                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            }

                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                agentcode = ds.Tables[0].Rows[0]["ShippingAgentCode"].ToString();
                                partnerCode = ds.Tables[0].Rows[0]["DesigCode"].ToString();
                                Pcs = ds.Tables[0].Rows[0]["Piecescount"].ToString();
                            }
                            if (tblFiltered != null && tblFiltered.Rows.Count > 0)
                            {

                                for (int j = 0; j < tblFiltered.Rows.Count; j++)
                                {
                                    if (tblFiltered.Rows[j]["ULD"].ToString() != "")
                                    {
                                        strAWB = strAWB + "AWB No:" + Convert.ToString(tblFiltered.Rows[j]["AWB"]) + ";" + "Pieces:" + Convert.ToString(tblFiltered.Rows[j]["ManifestedPcs"]) + "/" + Pcs + "/ " + Convert.ToString(tblFiltered.Rows[j]["ULD"]) + "\r\n";
                                    }

                                    else if (Convert.ToInt32(tblFiltered.Rows[j]["ManifestedPcs"]) > 0)
                                    {
                                        strAWB = strAWB + "AWB No:" + Convert.ToString(tblFiltered.Rows[j]["AWB"]) + ";" + "Pieces:" + Convert.ToString(tblFiltered.Rows[j]["ManifestedPcs"]) + "/" + Pcs + "/ " + "BULK" + "\r\n";
                                    }
                                }
                            }
                            //string[] paramname = new string[] { "PartnerCode", "MessageType", "PartnerType", "Origin", "Destination", "FlightNumber", "AgentCode" };
                            //object[] paramvalue = new object[] { partnerCode, "DEPARRNotification", "AIR", origin, dest, flightNumber, agentcode };
                            //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                            //ds = sqlServer.SelectRecords("SpGetRecordofSitaAddressandSitaMessageVersion", paramname, paramvalue, paramtype);

                            SqlParameter[] parametersPar =
                            {
                                new SqlParameter("@PartnerCode", SqlDbType.VarChar)     { Value = partnerCode },
                                new SqlParameter("@MessageType", SqlDbType.VarChar)     { Value = "DEPARRNotification" },
                                new SqlParameter("@PartnerType", SqlDbType.VarChar)     { Value = "AIR" },
                                new SqlParameter("@Origin", SqlDbType.VarChar)          { Value = origin },
                                new SqlParameter("@Destination", SqlDbType.VarChar)     { Value = dest },
                                new SqlParameter("@FlightNumber", SqlDbType.VarChar)    { Value = flightNumber },
                                new SqlParameter("@AgentCode", SqlDbType.VarChar)       { Value = agentcode }
                            };
                            ds = await _readWriteDao.SelectRecords("SpGetRecordofSitaAddressandSitaMessageVersion", parametersPar);

                            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                PartnerEmailiD = PartnerEmailiD == "" ? Convert.ToString(ds.Tables[0].Rows[0]["PartnerEmailiD"]).Trim() : PartnerEmailiD.Contains(PartnerEmailiD) ? PartnerEmailiD.Trim() : PartnerEmailiD + "," + Convert.ToString(ds.Tables[0].Rows[0]["PartnerEmailiD"]).Trim();

                            }
                            if (Convert.ToString(ds.Tables[0].Rows[0]["PartnerEmailiD"]).Trim() != string.Empty)
                            {
                                AWBNumberPrifix = AWBNumberPrifix == "" ? awbArrray[i] : AWBNumberPrifix.Contains(awbArrray[i]) ? AWBNumberPrifix : AWBNumberPrifix = AWBNumberPrifix + "\r\n" + awbArrray[i];
                                //AwbPcs = AwbPcs == "" ? "AWB No:" + awbArrray[i] + ";" + "Pieces:" + Pcs : AwbPcs.Contains("AWB No:" + awbArrray[i] + ";" + "Pieces:" + Pcs) ? AwbPcs : AwbPcs + "\r\n" + "AWB No:" + awbArrray[i] + ";" + "Pieces:" + Pcs;

                                //AwbPcs = AwbPcs + "AWB No:" + AWB_Numbers + ";" + "Pieces:" + Pcs + "/" + Pcs + "/ " +"BULK"  + "\r\n";
                                AwbPcs = AwbPcs + strAWB;

                            }
                        }
                        string Flightdate = flightdate.ToString(genericFunction.GetConfigurationValues("SystemDateFormat"));

                        if (AWBNumberPrifix != string.Empty && PartnerEmailiD != string.Empty)
                        {
                            string procedure = "spInsertMsgToOutbox";
                            //SQLServer dtb = new SQLServer();

                            //string[] paramname = new string[] { "Subject"
                            //          ,"Body"
                            //          ,"FromEmailID"
                            //          ,"ToEmailID"
                            //          ,"CreatedOn"
                            //          ,"Type"
                            //          ,"CreatedBy"
                            //          ,"FlightNumber"
                            //};

                            //object[] paramvalue = new object[] { "Departure Notification "
                            //  ,"Below AWB's has been Departed From " + origin + " on " + flightnum + "/" + Flightdate + "\r\n" + AwbPcs //"AWB Has Been Departed from:" + origin + "\r\n" + AWBNumberPrifix
                            //  , ""
                            //  , PartnerEmailiD
                            //  , System.DateTime.Now
                            //  ,"DEPARRNotification"
                            //  ,"FFM"
                            //  , flightnum };
                            //SqlDbType[] paramtype = new SqlDbType[] {SqlDbType.VarChar
                            //                                    ,SqlDbType.VarChar
                            //                                    ,SqlDbType.VarChar
                            //                                    ,SqlDbType.VarChar
                            //                                    ,SqlDbType.DateTime
                            //                                    ,SqlDbType.VarChar
                            //                                    ,SqlDbType.VarChar
                            //                                    ,SqlDbType.VarChar };
                            //dtb.InsertData(procedure, paramname, paramtype, paramvalue);

                            SqlParameter[] parameters =
                            {
                                new SqlParameter("@Subject", SqlDbType.VarChar)      { Value = "Departure Notification " },
                                new SqlParameter("@Body", SqlDbType.VarChar)         { Value = "Below AWB's has been Departed From " + origin + " on " + flightnum + "/" + Flightdate + "\r\n" + AwbPcs },
                                new SqlParameter("@FromEmailID", SqlDbType.VarChar)  { Value = "" },
                                new SqlParameter("@ToEmailID", SqlDbType.VarChar)    { Value = PartnerEmailiD },
                                new SqlParameter("@CreatedOn", SqlDbType.DateTime)   { Value = DateTime.Now },
                                new SqlParameter("@Type", SqlDbType.VarChar)         { Value = "DEPARRNotification" },
                                new SqlParameter("@CreatedBy", SqlDbType.VarChar)    { Value = "FFM" },
                                new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = flightnum }
                            };
                            await _readWriteDao.ExecuteNonQueryAsync(procedure, parameters);

                        }
                    }
                }
                #region Vendor Accounting
                await InsertVendorBilingDeails(awbNumbers, "DP", flightnum, flightdate, source, "FFM", DateTime.Now);
                #endregion

                genericFunction.SendBookingConfirmation("", "", "AWBUpdateNotificationDep", flightnum, flightdate.ToString("yyyy/MM/dd"), origin.ToUpper(), "", "Auto (FFM)");
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
        }

        private void GenerateAutoMessages(string source, string flightnum, DateTime flightdate, string username, DateTime utcNow, string dest)
        {
            try
            {
                GenericFunction gf = new GenericFunction();
                DataSet msgSeq = gf.GenerateMessageSequence(flightnum.Substring(0, 2), "FFM");
                if (msgSeq != null && msgSeq.Tables.Count > 0 && msgSeq.Tables[0].Rows.Count > 0)
                {
                    for (int noofMsg = 0; noofMsg < msgSeq.Tables[0].Rows.Count; noofMsg++)
                    {

                        switch (msgSeq.Tables[0].Rows[noofMsg]["MessageName"].ToString())
                        {
                            //case "FFM":
                            //    GenerateFFM(source, flightnum, flightdate, username, utcNow, dest, true);
                            //    break;
                            //case "FWB":
                            //    string str = string.Empty;
                            //    GenerateFWBExport(source, flightnum, flightdate, username, utcNow, dest, true);
                            //    break;
                            //case "FHL":
                            //    GenerateFHLExport(source, flightnum, flightdate, username, utcNow, dest, true);
                            //    break;
                            case "XFFM":
                                GenerateXFFM(flightnum.Substring(0, 2), source, dest, flightnum, flightdate, username, utcNow);
                                break;
                            case "XFWB":
                                GenerateXFWB(flightnum.Substring(0, 2), source, dest, flightnum, flightdate, username, utcNow);
                                break;
                                //case "XFZB":
                                //    GenerateXFZBExport(source, flightnum, flightdate, username, utcNow, dest, true);
                                //    break;
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

        private void GenerateXFFMExport(string source, string flightnum, DateTime flightdate, string username, DateTime utcNow, string dest, bool v)
        {
            throw new NotImplementedException();
        }

        private void ReProcessConsigmentRe(ref MessageData.consignmnetinfo[] consinfo, List<string> lstDeliveredAWB)
        {
            try


            {
                Array.Resize(ref consinfo, consinfo.Length + 1);
                //  Array.Copy(FFMConsig, consinfo, 1);

                for (int i = 0; i < consinfo.Length; i++)
                {
                    if (lstDeliveredAWB.Contains(consinfo[i].awbnum))
                    {
                        Array.Resize(ref consinfo, consinfo.Length + 1);
                        // Array.Copy(FFMConsig, i, consinfo, consinfo.Length - 1, 1);

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
        /// Method to decode consignment details
        /// </summary>
        /// <param name="inputstr">String containing consingment details</param>
        /// <param name="consinfo">Reference array that returns the consignment info</param>
        /// <param name="awbprefix">AWB Prefix</param>
        /// <param name="awbnumber">AWB Number</param>
        private bool DecodeConsigmentDetails(int srno, string inputstr, ref MessageData.consignmnetinfo[] consinfo, ref string awbprefix, ref string awbnumber, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool isConsignmentLineValid = true;
            try
            {
                MessageData.consignmnetinfo consig = new MessageData.consignmnetinfo("");
                string[] msg = inputstr.Split('/');
                string[] decmes = msg[0].Split('-');

                if (decmes.Length > 0)
                    consig.airlineprefix = decmes[0];
                if (decmes.Length > 1)
                {
                    string[] sptarr = StringSplitter(decmes[1]);
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
                            else if (sptarr[1].Length > 6)
                            {
                                errorMessage = "invalid consingment line";//Error #8
                                isConsignmentLineValid = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                        }
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
                        decimal volume = 0, grossweight = 0;
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

                        consig.consigntype = strarr[0] == null ? "" : strarr[0];
                        consig.pcscnt = strarr[1] == null ? "" : strarr[1];
                        consig.weightcode = strarr[2] == null ? "" : strarr[2];
                        if (consig.weightcode.Trim().ToUpper() != "K" && consig.weightcode.Trim().ToUpper() != "L")
                        {
                            if (errorMessage.Trim() == string.Empty)
                            {
                                errorMessage = "invalid weight code";
                                isConsignmentLineValid = false;
                            }
                        }
                        try
                        {
                            consig.weight = strarr[3];
                            grossweight = Convert.ToDecimal(consig.weight.ToString() == "" ? "0" : consig.weight.ToString());
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex);
                            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                            errorMessage = " invalid consignment line";
                            isConsignmentLineValid = false;
                        }
                        if (consig.consigntype.Equals("T"))
                        {
                            ///total pieces
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
                                }
                                else if (strarr[k] == "DG")
                                {
                                    consig.densityindicator = strarr[k];
                                    consig.densitygrp = strarr[k + 1];
                                }

                                else if (strarr[k] == "MC")
                                {
                                    try
                                    {
                                        consig.volumecode = strarr[k];
                                        consig.volumeamt = strarr[k + 1];
                                        volume = Convert.ToDecimal(consig.volumeamt.ToString() == "" ? "0" : consig.volumeamt.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        // clsLog.WriteLogAzure(ex);
                                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                        errorMessage = " invalid consignment line";
                                        isConsignmentLineValid = false;
                                    }
                                }
                                else
                                {
                                    errorMessage = " invalid consignment line";//Error #9
                                    isConsignmentLineValid = false;
                                }
                            }
                        }
                        if (!(consig.consigntype.Equals("P") || consig.consigntype.Equals("S") || consig.consigntype.Equals("D") || consig.consigntype.Equals("T"))
                            && (isConsignmentLineValid || errorMessage.Trim() == string.Empty))
                        {
                            errorMessage = "incorrect shipment code";
                            isConsignmentLineValid = false;
                        }
                        if ((consig.consigntype.Equals("P") || consig.consigntype.Equals("S") || consig.consigntype.Equals("D")) && (consig.numshp == "" || consig.shpdesccode == "")
                            && (isConsignmentLineValid || errorMessage.Trim() == string.Empty))
                        {
                            errorMessage = "incomplete consignment line";
                            isConsignmentLineValid = false;
                        }
                        if (consig.pcscnt != "" && consig.pcscnt == "0")
                        {
                            errorMessage = "zero pieces";
                            isConsignmentLineValid = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }

                }
                if (msg.Length > 3)
                {
                    try
                    {
                        consig.splhandling = "";
                        for (int j = 3; j < msg.Length; j++)
                            consig.splhandling = consig.splhandling + msg[j] + ",";
                    }
                    catch (Exception ex)
                    {
                        // clsLog.WriteLogAzure(ex);
                        _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                    }
                }
                try
                {
                    if (unloadingportsequence.Length > 0)
                        consig.portsequence = unloadingportsequence;
                    if (uldsequencenum.Length > 0)
                        consig.uldsequence = uldsequencenum;
                }
                catch (Exception ex)
                {
                    // clsLog.WriteLogAzure(ex);
                    _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                }
                awbprefix = consig.airlineprefix;
                awbnumber = consig.awbnum;
                if (isConsignmentLineValid)
                {
                    Array.Resize(ref consinfo, consinfo.Length + 1);
                    consinfo[consinfo.Length - 1] = consig;
                }
                awbref = consinfo.Length.ToString();
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return isConsignmentLineValid;
        }

        /// <summary>
        /// Method to club pices, weight and volume for same AWB
        /// </summary>
        /// <param name="FFMConsig">Split consignment information</param>
        /// <param name="consinfo">Clubed consignment information</param>
        private void ReProcessConsigment(ref MessageData.consignmnetinfo[] FFMConsig, ref MessageData.consignmnetinfo[] consinfo)
        {
            try
            {
                bool AWBMatch = false;
                Array.Resize(ref consinfo, consinfo.Length + 1);
                Array.Copy(FFMConsig, consinfo, 1);
                for (int i = 1; i < FFMConsig.Length; i++)
                {
                    AWBMatch = false;
                    for (int j = 0; j < consinfo.Length; j++)
                    {
                        if (consinfo[j].awbnum != null)
                        {
                            if (consinfo[j].awbnum.Equals(FFMConsig[i].awbnum) && consinfo[j].airlineprefix.Equals(FFMConsig[i].airlineprefix) && consinfo[j].origin.Equals(FFMConsig[i].origin) && consinfo[j].dest.Equals(FFMConsig[i].dest))
                            {
                                AWBMatch = true;
                                consinfo[j].weight = (Convert.ToDecimal(consinfo[j].weight) + Convert.ToDecimal(FFMConsig[i].weight)).ToString();
                                consinfo[j].pcscnt = (Convert.ToDecimal(consinfo[j].pcscnt) + Convert.ToDecimal(FFMConsig[i].pcscnt)).ToString();
                                consinfo[j].volumeamt = Convert.ToString(Convert.ToDecimal(string.IsNullOrEmpty(consinfo[j].volumeamt) ? "0" : consinfo[j].volumeamt) + Convert.ToDecimal(string.IsNullOrEmpty(FFMConsig[i].volumeamt) ? "0" : FFMConsig[i].volumeamt));
                            }
                        }
                    }
                    if (!AWBMatch)
                    {
                        Array.Resize(ref consinfo, consinfo.Length + 1);
                        Array.Copy(FFMConsig, i, consinfo, consinfo.Length - 1, 1);
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
        /// Method to save AWB dimensions
        /// </summary>
        /// <param name="AWBNumber">AWB Number</param>
        /// <param name="AWBPieces">AWB Pices</param>
        /// <param name="Dimensions">AWB Dimensions</param>
        /// <param name="AWBWt">AWB Weight</param>
        /// <param name="UserName">Updated by Name i.e. FFM</param>
        /// <param name="TimeStamp">Current date time</param>
        /// <param name="IsCreate">To create dimentions or not</param>
        /// <param name="AWBPrefix">AWB Prefix</param>
        /// <returns></returns>
        private async Task<DataSet> GenertateAWBDimensions(string AWBNumber, int AWBPieces, DataSet Dimensions, decimal AWBWt, string UserName, DateTime TimeStamp, bool IsCreate, string AWBPrefix)
        {
            //SQLServer da = new SQLServer();
            DataSet? ds = null;
            try
            {
                System.Text.StringBuilder strDimensions = new System.Text.StringBuilder();

                if (Dimensions != null && Dimensions.Tables.Count > 0 && Dimensions.Tables[0].Rows.Count > 0)
                {
                    for (int intCount = 0; intCount < Dimensions.Tables[0].Rows.Count; intCount++)
                    {
                        strDimensions.Append("Insert into #tblPieceInfo(PieceNo, IdentificationNo, Length, Breath, Height, Vol, Wt, Units, PieceType, BagNo, ULDNo, Location, FlightNo, FlightDate) values (");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceNo"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["IdentificationNo"]);
                        strDimensions.Append("',");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Length"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Breadth"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Height"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Vol"]);
                        strDimensions.Append(",");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Wt"]);
                        strDimensions.Append(",'");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Units"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["PieceType"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["BagNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["ULDNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["Location"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightNo"]);
                        strDimensions.Append("','");
                        strDimensions.Append(Dimensions.Tables[0].Rows[intCount]["FlightDate"]);
                        strDimensions.Append("'); ");
                    }
                }

                //string[] PName = new string[] { "AWBNumber", "Pieces", "PieceInfo", "UserName", "TimeStamp", "IsCreate", "AWBWeight", "AWBPrefix" };
                //object[] PValue = new object[] { AWBNumber, AWBPieces, strDimensions.ToString(), UserName, TimeStamp, IsCreate, AWBWt, AWBPrefix };
                //SqlDbType[] PType = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Bit, SqlDbType.Decimal, SqlDbType.VarChar };
                //ds = da.SelectRecords("sp_StoreCourierDetails", PName, PValue, PType);
                
                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar)  { Value = AWBNumber },
                    new SqlParameter("@Pieces", SqlDbType.Int)         { Value = AWBPieces },
                    new SqlParameter("@PieceInfo", SqlDbType.VarChar)  { Value = strDimensions.ToString() },
                    new SqlParameter("@UserName", SqlDbType.VarChar)   { Value = UserName },
                    new SqlParameter("@TimeStamp", SqlDbType.DateTime) { Value = TimeStamp },
                    new SqlParameter("@IsCreate", SqlDbType.Bit)       { Value = IsCreate },
                    new SqlParameter("@AWBWeight", SqlDbType.Decimal)  { Value = AWBWt },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar)  { Value = AWBPrefix }
                };
                ds = await _readWriteDao.SelectRecords("sp_StoreCourierDetails", parameters);

                //PName = null;
                //PValue = null;
                //PType = null;
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ds = null;
            }
            finally
            {
                //da = null;
            }
            return ds;
        }

        /// <summary>
        /// Methdo to save the flight manifest summary
        /// </summary>
        /// <param name="FlightNo">Flight number</param>
        /// <param name="POL">Point of loading</param>
        /// <param name="POU">Point of unloading</param>
        /// <param name="FltDate">Flight date</param>
        /// <param name="AircraftRegistration">Aircraft ragistration</param>
        /// <param name="REFNo">Message SrNo from tblInbox</param>
        /// <returns>Manifest ID</returns>
        private async Task<int> ExportManifestSummary(string FlightNo, string POL, string POU, DateTime FltDate, string AircraftRegistration, int REFNo)
        {
            int ID = 0;
            try
            {


                //SQLServer slqServer = new SQLServer();
                //string[] param = { "FLTno", "POL", "POU", "FLTDate", "TailNo", "REFNo" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Int };
                //object[] values = { FlightNo, POL, POU, FltDate.ToShortDateString(), AircraftRegistration, REFNo };
                //ID = slqServer.GetIntegerByProcedure("spExpManifestSummaryFFM", param, values, sqldbtypes);
                
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@FLTno", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar) { Value = POU },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime) { Value = FltDate.ToShortDateString() },
                    new SqlParameter("@TailNo", SqlDbType.VarChar) { Value = AircraftRegistration },
                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo }
                };

                ID = await _readWriteDao.GetIntegerByProcedureAsync("spExpManifestSummaryFFM", sqlParameters);
                if (ID < 1)
                {
                    // clsLog.WriteLogAzure("Error saving ExportFFM:");
                    _logger.LogWarning("Error saving ExportFFM:");
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                ID = 0;
            }
            return ID;
        }

        /// <summary>
        /// Method to save the flight manifest details
        /// </summary>
        /// <param name="POL">Point of loading</param>
        /// <param name="POU">Point of uploading</param>
        /// <param name="ORG">Flight origin</param>
        /// <param name="DES">Flight destination</param>
        /// <param name="AWBno">AWB Number</param>
        /// <param name="SCC"></param>
        /// <param name="VOL">Volume</param>
        /// <param name="PCS">Pices</param>
        /// <param name="WGT">Weight</param>
        /// <param name="Desc">Manifest descroption</param>
        /// <param name="FltDate">Flight date</param>
        /// <param name="ManifestID">Flight manifest ID</param>
        /// <returns>If record saved successfully returns true</returns>
        private async Task<bool> ExportManifestDetails(string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string Densityindicator)
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "Densityindicator" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int, SqlDbType.VarChar };
                //object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, Densityindicator };
                //if (db.InsertData("spExpManifestDetailsFFM", param, sqldbtypes, values))
                //{
                //    res = true;
                //}
                //else
                //{
                //    clsLog.WriteLogAzure("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                //    res = false;
                //}
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar) { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar) { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar) { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar) { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar) { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar) { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar) { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar) { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar) { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime) { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.Int) { Value = ManifestID },
                    new SqlParameter("@Densityindicator", SqlDbType.VarChar) { Value = Densityindicator }
                };

                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestDetailsFFM", sqlParameters))
                {
                    res = true;
                }
                else
                {
                    // clsLog.WriteLogAzure("Failes ManifDetails Save:" + AWBno);
                    _logger.LogWarning("Failes ManifDetails Save:{0}" , AWBno);
                    res = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        /// <summary>
        /// Method to save the flight manifest details
        /// </summary>
        /// <param name="ULDNo">ULD number</param>
        /// <param name="POL">Point of loading</param>
        /// <param name="POU">Point of uploading</param>
        /// <param name="ORG">Flight origin</param>
        /// <param name="DES">Flight destination</param>
        /// <param name="AWBno">AWB Number</param>
        /// <param name="SCC"></param>
        /// <param name="VOL">Volume</param>
        /// <param name="PCS">Manifested Pices</param>
        /// <param name="WGT">Manifisted Weight</param>
        /// <param name="Desc">Manifest descroption</param>
        /// <param name="FltDate">Flight date</param>
        /// <param name="ManifestID">Flight manifest ID</param>
        /// <param name="FlightNo">Flight number</param>
        /// <param name="AWBPrefix">AWB Prefix</param>
        /// <param name="weightcode">Weight code</param>
        /// <param name="fffmSequenceSNo"></param>
        /// <param name="AwbPcs">AWB Pices</param>
        /// <param name="REFNo">Message SrNo from tblInbox</param>
        /// <returns>If record saved successfully returns true</returns>
        private async Task<bool> ExportManifestDetails(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string FlightNo, string AWBPrefix, string weightcode, int fffmSequenceSNo, int AwbPcs, string UNID, int REFNo = 0, bool IsBUP = false, string Densityindicator = "", string consignmentType = "", string FFMEndMesgCode = "")
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "ULDNo", "TotalAWBPcs", "UOM", "FFMSequenceSNo", "REFNo", "IsBUP", "Densityindicator", "ConsignmentType", "FFMEndMesgCode", "UNID" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.Bit, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //object[] values = { POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, AWBPrefix, FlightNo, ULDNo, AwbPcs, weightcode, fffmSequenceSNo, REFNo, IsBUP, Densityindicator, consignmentType, FFMEndMesgCode, UNID };
                //if (db.InsertData("spExpManifestDetailsFFM", param, sqldbtypes, values))
                //    res = true;
                //else
                //{
                //    clsLog.WriteLogAzure("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                //    res = false;
                //}
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar) { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar) { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar) { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar) { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar) { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar) { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar) { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar) { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar) { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime) { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.Int) { Value = ManifestID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = ULDNo },
                    new SqlParameter("@TotalAWBPcs", SqlDbType.Int) { Value = AwbPcs },
                    new SqlParameter("@UOM", SqlDbType.VarChar) { Value = weightcode },
                    new SqlParameter("@FFMSequenceSNo", SqlDbType.Int) { Value = fffmSequenceSNo },
                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                    new SqlParameter("@IsBUP", SqlDbType.Bit) { Value = IsBUP },
                    new SqlParameter("@Densityindicator", SqlDbType.VarChar) { Value = Densityindicator },
                    new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = consignmentType },
                    new SqlParameter("@FFMEndMesgCode", SqlDbType.VarChar) { Value = FFMEndMesgCode },
                    new SqlParameter("@UNID", SqlDbType.VarChar) { Value = UNID }
                };
                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestDetailsFFM", sqlParameters))
                    res = true;
                else
                {
                    // clsLog.WriteLogAzure("Failes ManifDetails Save:" + AWBno);
                    _logger.LogWarning("Failes ManifDetails Save:{0}" , AWBno);
                    res = false;
                }
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        private async Task<bool> ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, string FltDate, int ManifestID, string awbprefix, string flightno, string BkdPcs, string BkdWt, string ConsignmentType)
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "BkdPcs", "BkdWt", "Source", "ConsignmentType" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,
                //                             SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar,SqlDbType.VarChar,SqlDbType.VarChar };
                //object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno, BkdPcs, BkdWt, "M", ConsignmentType };

                //if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
                //{
                //    res = true;
                //}
                //else
                //{
                //    clsLog.WriteLog("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                //    res = false;
                //}

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = ULDNo },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar) { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar) { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar) { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar) { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar) { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar) { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar) { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar) { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar) { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.VarChar) { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.BigInt) { Value = ManifestID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbprefix },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = flightno },
                    new SqlParameter("@BkdPcs", SqlDbType.VarChar) { Value = BkdPcs },
                    new SqlParameter("@BkdWt", SqlDbType.VarChar) { Value = BkdWt },
                    new SqlParameter("@Source", SqlDbType.VarChar) { Value = "M" },
                    new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = ConsignmentType }
                };
                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestULDAWBFFM", sqlParameters))
                {
                    res = true;
                }
                else
                {
                    // clsLog.WriteLog("Failes ManifDetails Save:" + AWBno );
                    _logger.LogWarning("Failes ManifDetails Save:{0}" , AWBno );
                    res = false;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        private async Task<bool> ExportManifestULDAWBAssociation(string ULDNo, string POL, string POU, string ORG, string DES, string AWBno, string SCC, string VOL, string PCS, string WGT, string Desc, DateTime FltDate, int ManifestID, string awbprefix, string flightno, string BkdPcs, string BkdWt, string ConsignmentType, int TotalAWBPcs, int ffmSequenceSNo, string WeightCode, int REFNo = 0, bool IsBUP = false)
        {
            bool res;
            try
            {
                //SQLServer db = new SQLServer();
                //string[] param = { "ULDNo", "POL", "POU", "ORG", "DES", "AWBno", "SCC", "VOL", "PCS", "WGT", "Desc", "FLTDate", "ManifestID", "AWBPrefix", "FlightNo", "BkdPcs", "BkdWt", "Source", "ConsignmentType", "TotalAWBPcs", "FFMSequenceSNo", "Uom", "REFNo", "IsBUP" };
                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.BigInt, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.Bit };
                //object[] values = { ULDNo, POL, POU, ORG, DES, AWBno, SCC, VOL, PCS, WGT, Desc, FltDate, ManifestID, awbprefix, flightno, BkdPcs, BkdWt, "M", ConsignmentType, TotalAWBPcs, ffmSequenceSNo, WeightCode, REFNo, IsBUP };
                //if (db.InsertData("spExpManifestULDAWBFFM", param, sqldbtypes, values))
                //    res = true;
                //else
                //{
                //    clsLog.WriteLog("Failes ManifDetails Save:" + AWBno + " Error: " + db.LastErrorDescription);
                //    res = false;
                //}
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = ULDNo },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar) { Value = POU },
                    new SqlParameter("@ORG", SqlDbType.VarChar) { Value = ORG },
                    new SqlParameter("@DES", SqlDbType.VarChar) { Value = DES },
                    new SqlParameter("@AWBno", SqlDbType.VarChar) { Value = AWBno },
                    new SqlParameter("@SCC", SqlDbType.VarChar) { Value = SCC },
                    new SqlParameter("@VOL", SqlDbType.VarChar) { Value = VOL },
                    new SqlParameter("@PCS", SqlDbType.VarChar) { Value = PCS },
                    new SqlParameter("@WGT", SqlDbType.VarChar) { Value = WGT },
                    new SqlParameter("@Desc", SqlDbType.VarChar) { Value = Desc },
                    new SqlParameter("@FLTDate", SqlDbType.DateTime) { Value = FltDate },
                    new SqlParameter("@ManifestID", SqlDbType.BigInt) { Value = ManifestID },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = awbprefix },
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = flightno },
                    new SqlParameter("@BkdPcs", SqlDbType.VarChar) { Value = BkdPcs },
                    new SqlParameter("@BkdWt", SqlDbType.VarChar) { Value = BkdWt },
                    new SqlParameter("@Source", SqlDbType.VarChar) { Value = "M" },
                    new SqlParameter("@ConsignmentType", SqlDbType.VarChar) { Value = ConsignmentType },
                    new SqlParameter("@TotalAWBPcs", SqlDbType.Int) { Value = TotalAWBPcs },
                    new SqlParameter("@FFMSequenceSNo", SqlDbType.Int) { Value = ffmSequenceSNo },
                    new SqlParameter("@Uom", SqlDbType.VarChar) { Value = WeightCode },
                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                    new SqlParameter("@IsBUP", SqlDbType.Bit) { Value = IsBUP }
                };

                if (await _readWriteDao.ExecuteNonQueryAsync("spExpManifestULDAWBFFM", sqlParameters))
                    res = true;
                else
                {
                    // clsLog.WriteLog("Failes ManifDetails Save:" + AWBno );
                    _logger.LogWarning("Failes ManifDetails Save: {0}" , AWBno );
                    res = false;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        private async Task<bool> ULDawbAssociation(string FltNo, string POL, string POU, string AWBno, string PCS, string WGT, DateTime FltDate, string ULDNo)
        {
            bool res;
            try
            {
                int _pcs = int.Parse(PCS);
                float _wgt = float.Parse(WGT);
                //SQLServer db = new SQLServer();
                //string[] param = { "ULDtripid", "ULDNo", "AWBNumber", "POL", "POU", "FltNo", "Pcs", "Wgt", "AvlPcs", "AvlWgt", "Updatedon", "Updatedby", "Status", "Manifested", "FltDate" };


                //SqlDbType[] sqldbtypes = { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar
                //                             , SqlDbType.Int, SqlDbType.Float, SqlDbType.Int, SqlDbType.Float,SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.Bit,SqlDbType.Bit, SqlDbType.DateTime };
                //object[] values = { "", ULDNo, AWBno, POL, POU, FltNo, 0, 0, _pcs, _wgt, DateTime.Now, "FFM", false, false, FltDate };

                //if (db.InsertData("SPImpManiSaveUldAwbAssociation", param, sqldbtypes, values))
                //{
                //    res = true;
                //}
                //else
                //{
                //    clsLog.WriteLogAzure("Failes ULDAWBAssociation Save:" + AWBno + " Error: " + db.LastErrorDescription);
                //    res = false;
                //}

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@ULDtripid", SqlDbType.VarChar) { Value = "" },
                    new SqlParameter("@ULDNo", SqlDbType.VarChar) { Value = ULDNo },
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBno },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@POU", SqlDbType.VarChar) { Value = POU },
                    new SqlParameter("@FltNo", SqlDbType.VarChar) { Value = FltNo },
                    new SqlParameter("@Pcs", SqlDbType.Int) { Value = 0 },
                    new SqlParameter("@Wgt", SqlDbType.Float) { Value = 0 },
                    new SqlParameter("@AvlPcs", SqlDbType.Int) { Value = _pcs },
                    new SqlParameter("@AvlWgt", SqlDbType.Float) { Value = _wgt },
                    new SqlParameter("@Updatedon", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@Updatedby", SqlDbType.VarChar) { Value = "FFM" },
                    new SqlParameter("@Status", SqlDbType.Bit) { Value = false },
                    new SqlParameter("@Manifested", SqlDbType.Bit) { Value = false },
                    new SqlParameter("@FltDate", SqlDbType.DateTime) { Value = FltDate }
                };

                if (await _readWriteDao.ExecuteNonQueryAsync("SPImpManiSaveUldAwbAssociation", sqlParameters))
                {
                    res = true;
                }
                else
                {
                    // clsLog.WriteLogAzure("Failes ULDAWBAssociation Save:" + AWBno);
                    _logger.LogWarning("Failes ULDAWBAssociation Save:{0}" , AWBno);
                    res = false;
                }

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                res = false;
            }
            return res;
        }

        private async Task<string> InsertAWBDataInBilling(object[] AWBInfo)
        {
            //SQLServer da = new SQLServer();
            try
            {
                //string[] ColumnNames = new string[11];
                //SqlDbType[] DataType = new SqlDbType[11];
                //Object[] Values = new object[11];
                //int i = 0;

                //ColumnNames.SetValue("AWBNumber", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("AWBPrefix", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdatedBy", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdatedOn", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("ValidateMin", i);
                //DataType.SetValue(SqlDbType.Bit, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdateBooking", i);
                //DataType.SetValue(SqlDbType.Bit, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("RouteFrom", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdateBilling", i);
                //DataType.SetValue(SqlDbType.Bit, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("SpotApplied", i);
                //DataType.SetValue(SqlDbType.Bit, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("AllInSpotRate", i);
                //DataType.SetValue(SqlDbType.Bit, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("CallFrom", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);

                //string res = da.GetStringByProcedure("sp_CalculateAWBRatesReprocess", ColumnNames, Values, DataType);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBInfo.GetValue(0) },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBInfo.GetValue(1) },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = AWBInfo.GetValue(2) },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = AWBInfo.GetValue(3) },
                    new SqlParameter("@ValidateMin", SqlDbType.Bit) { Value = AWBInfo.GetValue(4) },
                    new SqlParameter("@UpdateBooking", SqlDbType.Bit) { Value = AWBInfo.GetValue(5) },
                    new SqlParameter("@RouteFrom", SqlDbType.VarChar) { Value = AWBInfo.GetValue(6) },
                    new SqlParameter("@UpdateBilling", SqlDbType.Bit) { Value = AWBInfo.GetValue(7) },
                    new SqlParameter("@SpotApplied", SqlDbType.Bit) { Value = AWBInfo.GetValue(8) },
                    new SqlParameter("@AllInSpotRate", SqlDbType.Bit) { Value = AWBInfo.GetValue(9) },
                    new SqlParameter("@CallFrom", SqlDbType.VarChar) { Value = AWBInfo.GetValue(10) }
                };
                //Updated by Shrishail- change sp name -Discuss with Swapnil
                //string res = da.GetStringByProcedure("USP_InsertBulkAWBDataInBilling", ColumnNames, Values, DataType);
                string? res = await _readWriteDao.GetStringByProcedureAsync("sp_CalculateAWBRatesReprocess", sqlParameters);
                return res;

            }

            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "error";
            }
        }

        private async Task<string> InsertAWBDataInInterlineInvoice(object[] AWBInfo)
        {
            //SQLServer da = new SQLServer();
            try
            {
                //string[] ColumnNames = new string[4];
                //SqlDbType[] DataType = new SqlDbType[4];
                //Object[] Values = new object[4];
                //int i = 0;

                //ColumnNames.SetValue("AWBNumber", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("BillingFlag", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UserName", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdatedOn", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);

                //string res = da.GetStringByProcedure("SP_InsertAWBDataInInterlineInvoice", ColumnNames, Values, DataType);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBInfo.GetValue(0) },
                    new SqlParameter("@BillingFlag", SqlDbType.VarChar) { Value = AWBInfo.GetValue(1) },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = AWBInfo.GetValue(2) },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = AWBInfo.GetValue(3) }
                };
                string? res = await _readWriteDao.GetStringByProcedureAsync("SP_InsertAWBDataInInterlineInvoice", sqlParameters);

                return res;

            }

            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "error";
            }
        }

        private async Task<string> InsertAWBDataInInterlineCreditNote(object[] AWBInfo)
        {
            //SQLServer da = new SQLServer();
            try
            {
                //string[] ColumnNames = new string[4];
                //SqlDbType[] DataType = new SqlDbType[4];
                //Object[] Values = new object[4];
                //int i = 0;

                //ColumnNames.SetValue("AWBNumber", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("BillingFlag", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UserName", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //i++;

                //ColumnNames.SetValue("UpdatedOn", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(AWBInfo.GetValue(i), i);
                //string res = da.GetStringByProcedure("SP_InsertAWBDataInInterlineCreditNote", ColumnNames, Values, DataType);
                
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBInfo.GetValue(0) },
                    new SqlParameter("@BillingFlag", SqlDbType.VarChar) { Value = AWBInfo.GetValue(1) },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = AWBInfo.GetValue(2) },
                    new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = AWBInfo.GetValue(3) }
                };

                string? res = await _readWriteDao.GetStringByProcedureAsync("SP_InsertAWBDataInInterlineCreditNote", sqlParameters);
                return res;

            }

            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "error";
            }
        }

        /// <summary>
        /// Check whether the awb is present or not
        /// </summary>
        /// <param name="AWBNum">AWB Number</param>
        /// <param name="AWBPrefix">Airline Prefix</param>
        /// <returns>Return true if AWB is present</returns>
        //private bool IsAWBPresent(string AWBNum, string AWBPrefix, string ConsignmentType, int ffmPieces, int ffmTotalPieces, out string AWBDeliveryStatus, out string AwbStatus, out string OriginCode, out string DestinationCode, out string errorMessage, string POL, out string TotalWgt, int REFNo, string UpdatedBy)


        private async Task<(bool,
            string AWBDeliveryStatus, 
            string AwbStatus, 
            string OriginCode, 
            string DestinationCode, 
            string errorMessage,
            string TotalWgt
            )> IsAWBPresent(string AWBNum, string AWBPrefix, string ConsignmentType, int ffmPieces, int ffmTotalPieces, string AWBDeliveryStatus, string AwbStatus, string OriginCode, string DestinationCode, string errorMessage, string POL, string TotalWgt, int REFNo, string UpdatedBy)
        {
            bool isAWBPresent = false;
            errorMessage = "";
            int acceptedPieces = 0, totalManifestedPieces = 0;
            try
            {
                AwbStatus = string.Empty;
                AWBDeliveryStatus = string.Empty;
                OriginCode = string.Empty;
                DestinationCode = string.Empty;
                TotalWgt = string.Empty;
                DataSet? dsCheck = new DataSet();
                //SQLServer sqlServer = new SQLServer();
                //string[] pname = new string[] { "AWBNumber", "AWBPrefix", "POL" , "REFNo", "UpdatedBy" };
                //object[] values = new object[] { AWBNum, AWBPrefix, POL, REFNo, UpdatedBy };
                //SqlDbType[] ptype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar };
                //dsCheck = sqlServer.SelectRecords("sp_getawbdetails", pname, values, ptype);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBNumber", SqlDbType.VarChar) { Value = AWBNum },
                    new SqlParameter("@AWBPrefix", SqlDbType.VarChar) { Value = AWBPrefix },
                    new SqlParameter("@POL", SqlDbType.VarChar) { Value = POL },
                    new SqlParameter("@REFNo", SqlDbType.Int) { Value = REFNo },
                    new SqlParameter("@UpdatedBy", SqlDbType.VarChar) { Value = UpdatedBy }
                };
                dsCheck = await _readWriteDao.SelectRecords("sp_getawbdetails", sqlParameters);

                if (dsCheck != null)
                {
                    if (dsCheck.Tables.Count > 0)
                    {
                        if (dsCheck.Tables[0].Rows.Count > 0)
                        {
                            if (dsCheck.Tables[0].Rows[0]["AWBNumber"].ToString().Equals(AWBNum, StringComparison.OrdinalIgnoreCase))
                            {
                                isAWBPresent = true;
                                
                                if (dsCheck.Tables[10].Rows.Count > 0)
                                {
                                    AWBDeliveryStatus = dsCheck.Tables[10].Rows[0]["DeliveryStatus"].ToString();
                                    acceptedPieces = Convert.ToInt32(dsCheck.Tables[10].Rows[0]["AcceptedPieces"].ToString());
                                    totalManifestedPieces = Convert.ToInt32(dsCheck.Tables[10].Rows[0]["TotalManifestedPieces"].ToString());
                                }

                                if (ConsignmentType.ToUpper() == "P" && ffmTotalPieces == acceptedPieces && (ffmPieces + totalManifestedPieces) <= ffmTotalPieces && AWBDeliveryStatus.ToUpper() == "C")
                                    AWBDeliveryStatus = string.Empty;

                                if (ConsignmentType.ToUpper() == "P" && ffmTotalPieces == acceptedPieces && (ffmPieces + totalManifestedPieces) > ffmTotalPieces && AWBDeliveryStatus.ToUpper() == "C")
                                    errorMessage = AWBPrefix + "-" + AWBNum + " manifested pieces exceeded";

                                if (dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() == "V")
                                    errorMessage = AWBPrefix + "-" + AWBNum + " AWB is Voided";

                                if (dsCheck.Tables.Count > 13 && dsCheck.Tables[13].Columns.Contains("error") && dsCheck.Tables[13].Rows[0]["error"].ToString() == "Shipper is Inactive OR Expired")
                                    errorMessage = AWBPrefix + "-" + AWBNum + " Shipper is Inactive OR Expired";

                                if (dsCheck.Tables.Count > 13 && dsCheck.Tables[13].Columns.Contains("error"))
                                    errorMessage = AWBPrefix + "-" + AWBNum + dsCheck.Tables[13].Rows[0]["error"].ToString();


                                AwbStatus = dsCheck.Tables[0].Rows[0]["AWBStatus"].ToString().ToUpper() == "E" ? "EX" : "BK";
                                AwbStatus = dsCheck.Tables[0].Rows[0]["IsAccepted"].ToString().ToUpper() == "TRUE" && Convert.ToInt32(dsCheck.Tables[0].Rows[0]["AWBAccPcs"].ToString()) > 0 ? "AC" : AwbStatus;
                                OriginCode = dsCheck.Tables[0].Rows[0]["OriginCode"].ToString();
                                DestinationCode = dsCheck.Tables[0].Rows[0]["DestinationCode"].ToString();
                                TotalWgt = dsCheck.Tables[0].Rows[0]["GrossWeight"].ToString();

                            }
                        }
                    }
                }
                return (isAWBPresent, AWBDeliveryStatus, AwbStatus, OriginCode, DestinationCode, errorMessage, TotalWgt);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                AwbStatus = string.Empty;
                AWBDeliveryStatus = string.Empty;
                OriginCode = string.Empty;
                DestinationCode = string.Empty;
                TotalWgt = string.Empty;
            }
            return (isAWBPresent, AWBDeliveryStatus, AwbStatus, OriginCode, DestinationCode, errorMessage, TotalWgt);
        }
        #endregion Private Methods
        private async Task InsertVendorBilingDeails(string AWBList, string strEvent, string FltNo, DateTime fltDate, string Origin, string username, DateTime updatedon)
        {
            try
            {


                //SQLServer da = new SQLServer();
                DataSet ds = null;
                //string[] paramname = new string[] { "AWBPrefixNumberList", "EventFlag", "FlightNumber", "FlightDate", "Station", "UserName", "UpdatedOn" };
                //object[] paramvalue = new object[] { AWBList, strEvent, FltNo, fltDate, Origin, username, updatedon };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                //ds = da.SelectRecords("ABilling.USPInsertVendorAWBCostDetails", paramname, paramvalue, paramtype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@AWBPrefixNumberList", SqlDbType.VarChar) { Value = AWBList },
                    new SqlParameter("@EventFlag", SqlDbType.VarChar) { Value = strEvent },
                    new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = FltNo },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = fltDate },
                    new SqlParameter("@Station", SqlDbType.VarChar) { Value = Origin },
                    new SqlParameter("@UserName", SqlDbType.VarChar) { Value = username },
                    new SqlParameter("@UpdatedOn", SqlDbType.VarChar) { Value = updatedon }
                };

                ds = await _readWriteDao.SelectRecords("ABilling.USPInsertVendorAWBCostDetails", parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

            }

            // objBill.AddVendorAWBDetails(AWBInfo);
            //  strEvent.ToUpper(), txtAwbPrefix.Text + "-" + HidAWBNumber.Value.Trim(), Session["Station"].ToString(), Session["UserName"].ToString(), Convert.ToDateTime(Session["IT"]));



        }


        private async Task<string> AddCharterBillingDetails(string FlightNumber, DateTime FlightDate, string FlightOrigin)
        {
            //SQLServer da = new SQLServer();
            try
            {
                //string[] ColumnNames = new string[3];
                //SqlDbType[] DataType = new SqlDbType[3];
                //Object[] Values = new object[3];
                //int i = 0;

                //ColumnNames.SetValue("FlightNumber", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(FlightNumber, i);
                //i++;

                //ColumnNames.SetValue("FlightDate", i);
                //DataType.SetValue(SqlDbType.DateTime, i);
                //Values.SetValue(FlightDate, i);
                //i++;

                //ColumnNames.SetValue("FlightOrigin", i);
                //DataType.SetValue(SqlDbType.VarChar, i);
                //Values.SetValue(FlightOrigin, i);
                //i++;

                //string res = da.GetStringByProcedure("uspBulkAWBManualReprocess_Charter_VJ", ColumnNames, Values, DataType);

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@FlightNumber", SqlDbType.VarChar) { Value = FlightNumber },
                    new SqlParameter("@FlightDate", SqlDbType.DateTime) { Value = FlightDate },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = FlightOrigin }
                };
                string? res = await _readWriteDao.GetStringByProcedureAsync("uspBulkAWBManualReprocess_Charter_VJ", sqlParameters);

                return res;

            }

            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                return "error";
            }
        }


        public async Task GenerateXFWB(string PartnerCode, string DepartureAirport, string ArrivalAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate)
        {
           try
           {
             string FlightDestination = string.Empty, customsName = string.Empty;
             GenericFunction genericFunction = new GenericFunction();
             //cls_SCMBL _SCMBL = new cls_SCMBL();
             string MessageVersion = "8", SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, xfwbMessage = string.Empty, SFTPMessageHeader = string.Empty, messagetype = string.Empty;
             DataSet dsData = new DataSet();
             XFWBMessageProcessor xfwbMessageProcessor = new XFWBMessageProcessor();
             GenericFunction generalfunction = new GenericFunction();
             DataSet ds = new DataSet();
             DataSet? dsdata1 = new DataSet();
             // DataSet dsData = new DataSet();
             dsdata1 = await _cls_SCMBL.GetFlightInformationforFFM(DepartureAirport, FlightNo, FlightDate);
             if (dsdata1 != null && dsdata1.Tables.Count > 1 && dsdata1.Tables[1].Rows.Count > 0)
             {
                 customsName = dsdata1.Tables[1].Rows[0]["CustomsName"].ToString();
             }
             ds = generalfunction.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "XFWB", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty);
             SitaMessageHeader = string.Empty;
 
             if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
             {
                 strEmailid = ds.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                 messagetype = ds.Tables[0].Rows[0]["MsgCommType"].ToString();
                 MessageVersion = ds.Tables[0].Rows[0]["MessageVersion"].ToString();
 
                 if (ds.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                 {
                     SitaMessageHeader = generalfunction.MakeMailMessageFormat(ds.Tables[0].Rows[0]["PatnerSitaID"].ToString(), ds.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), ds.Tables[0].Rows[0]["MessageID"].ToString(), ds.Tables[0].Rows[0]["SITAHeaderType"].ToString());
                 }
 
 
                 if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                 {
                     if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString() == ds.Tables[0].Rows[0]["SFTPAddress"].ToString())
                     {
                         SFTPMessageHeader = messagetype;
                     }
                     else
                     {
                         SFTPMessageHeader = generalfunction.MakeMailMessageFormat(ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(),
                             ds.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), ds.Tables[0].Rows[0]["MessageID"].ToString(),
                             ds.Tables[0].Rows[0]["SFTPHeaderType"].ToString());
                     }
                 }
                 DataTable dt = new DataTable();
                 if (dsdata1 != null && dsdata1.Tables[0].Rows.Count > 0)
                 {
                     foreach (DataRow drdestination in dsdata1.Tables[0].Rows)
                     {
                         MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                         dsData = await _cls_SCMBL.GetRecordforGenerateFFM(DepartureAirport, FlightNo, FlightDate, drdestination["DepartureStation"].ToString());
                         if (dsData.Tables[3] != null && dsData.Tables[3].Rows.Count > 0)
                             dt.Merge(dsData.Tables[3]);
                     }
                 }
                 if (dt != null && dt.Rows.Count > 0)
                 {
                     dt = GenericFunction.SelectDistinct(dt, "AWBNumber");
 
                     // dt = SelectDistinct(dsData.Tables[3], "AWBNumber");
 
                     for (int i = 0; i < dt.Rows.Count; i++)
                     {
                         if (Convert.ToString(dt.Rows[i]["AWBNumber"]).Length >= 12)
                         {
 
                             xfwbMessage = xfwbMessageProcessor.GenerateXFWBMessageV3(dt.Rows[i]["AWBNumber"].ToString().Substring(0, 3), dt.Rows[i]["AWBNumber"].ToString().Substring(4, 8), customsName);
 
                             //ds = GF.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FWB", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty);
 
                             if (xfwbMessage != string.Empty && xfwbMessage.Length > 0)
                             {
                                 Dictionary<string, string> Params = new Dictionary<string, string>();
                                 if (customsName.ToUpper() == "DAKAR")
                                 {
                                     Params.Add("xfwb", xfwbMessage);
                                 }
                                 else
                                 {
                                     xfwbMessage = "<![CDATA[" + xfwbMessage + "]]>";
                                     Params.Add("airline", FlightNo.Substring(0, 2).ToString());
                                     Params.Add("xfwb", xfwbMessage);
                                 }
 
                                 xfwbMessage = GenerateSoapXMLRequest(Params, "sendMessageXFWB", customsName);
 
                                 if (SitaMessageHeader != "" && SitaMessageHeader.Length > 0)
                                 {
                                     generalfunction.SaveMessageOutBox("SITA:XFWB", SitaMessageHeader.ToString() + "\r\n" + xfwbMessage, "", "SITAFTP", username, "", "XFWB", "", FlightNo);
                                 }
                                 if (SFTPMessageHeader != "" && SFTPMessageHeader.Length > 0)
                                 {
                                     generalfunction.SaveMessageOutBox("SITA:XFWB", SFTPMessageHeader.ToString() + "\r\n" + xfwbMessage, "", "SFTP", username, "", "XFWB", "", FlightNo);
                                 }
                                 if (strEmailid != "")
                                 {
                                     generalfunction.SaveMessageOutBox("XFWB", xfwbMessage, string.Empty, strEmailid, "", "", FlightNo, "", Convert.ToString(dt.Rows[i]["AWBNumber"]));
                                 }
                                 if (ds.Tables[0].Rows[0]["WebServiceURL"].ToString().Length > 0)
                                 {
                                     generalfunction.SaveMessageOutBox("XFWB", xfwbMessage, string.Empty, "WEBSERVICE", "", "", FlightNo, "", Convert.ToString(dt.Rows[i]["AWBNumber"]));
                                 }
                             }
                         }
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
        public async Task GenerateXFZBMessage(string PartnerCode, string DepartureAirport, string ArrivalAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate, string AWBnumbers)
        {
            try
            {
                string FlightDestination = string.Empty;
                string hawbNumber = string.Empty;
                //cls_SCMBL _SCMBL = new cls_SCMBL();
                XFZBMessageProcessor xfhlMessageProcessor = new XFZBMessageProcessor();
                string xfzbMessage = string.Empty, partnerCode = string.Empty, flightorigin = string.Empty, flightDestination = string.Empty, messageHeader = string.Empty, SFTPMessageHeader = string.Empty;
                GenericFunction genericFunction = new GenericFunction();
                string SitaMessageHeader = string.Empty, error = string.Empty, strEmailid = string.Empty, strSITAHeaderType = string.Empty, awbPrefix = string.Empty, awbNumber = string.Empty, messagetype = string.Empty, MessageVersion = "4";
                DataSet dsdata2 = new DataSet();
                DataSet dsData = new DataSet();
                DataTable dt = new DataTable();
                DataSet? dthwb = new DataSet();
                DataSet? ds = new DataSet();
                dsdata2 = await _cls_SCMBL.GetFlightInformationforFFM(DepartureAirport, FlightNo, FlightDate);
                if (DepartureAirport == "DAC" || ArrivalAirport == "DAC")
                {
                    if (dsdata2 != null && dsdata2.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow drdestination in dsdata2.Tables[0].Rows)
                        {
                            MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                            dsData = await _cls_SCMBL.GetRecordforGenerateFFM(DepartureAirport, FlightNo, FlightDate, drdestination["DepartureStation"].ToString());
                            if (dsData.Tables[3] != null && dsData.Tables[3].Rows.Count > 0)
                                dt.Merge(dsData.Tables[3]);
                        }
                    }
    
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        dt = GenericFunction.SelectDistinct(dt, "AWBNumber");
                        bool IsXFZB = true;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (Convert.ToString(dt.Rows[i]["AWBNumber"]).Length >= 12)
                            {
                                dthwb = await _cls_SCMBL.GetChildHAWB(dt.Rows[i]["AWBNumber"].ToString().Substring(0, 3), dt.Rows[i]["AWBNumber"].ToString().Substring(4, 8), FlightNo,
                               FlightDate, true, "", IsXFZB);
    
                                if (dthwb != null && dthwb.Tables[0].Rows.Count > 0)
                                {
                                    foreach (DataRow dr in dthwb.Tables[0].Rows)
                                    {
                                        hawbNumber = dr["HAWBNo"].ToString();
    
                                        xfzbMessage = xfhlMessageProcessor.GenerateXFZBMessage(dt.Rows[i]["AWBNumber"].ToString().Substring(0, 3), dt.Rows[i]["AWBNumber"].ToString().Substring(4, 8), hawbNumber);
    
    
                                        //xfzbMessage = fwbManagement.GenerateXFWBMessageV3(dt.Rows[i]["AWBNumber"].ToString().Substring(0, 3), dt.Rows[i]["AWBNumber"].ToString().Substring(4, 8));
    
                                        //ds = GF.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FWB", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty);
    
                                        if (xfzbMessage != string.Empty && xfzbMessage.Length > 0 && xfzbMessage.ToString().Contains("<rsm:"))
                                        {
                                            ds = genericFunction.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "XFZB", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty);
                                            SitaMessageHeader = string.Empty;
    
                                            if (ds != null && ds.Tables[0].Rows.Count > 0)
                                            {
    
                                                strEmailid = ds.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                                                messagetype = ds.Tables[0].Rows[0]["MsgCommType"].ToString();
                                                MessageVersion = ds.Tables[0].Rows[0]["MessageVersion"].ToString();
    
                                                if (ds.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                                                {
                                                    SitaMessageHeader = genericFunction.MakeMailMessageFormat(ds.Tables[0].Rows[0]["PatnerSitaID"].ToString(), ds.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), ds.Tables[0].Rows[0]["MessageID"].ToString(), ds.Tables[0].Rows[0]["SITAHeaderType"].ToString());
                                                }
    
    
                                                if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                                                {
                                                    if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString() == ds.Tables[0].Rows[0]["SFTPAddress"].ToString())
                                                    {
                                                        SFTPMessageHeader = messagetype;
                                                    }
                                                    else
                                                    {
                                                        SFTPMessageHeader = genericFunction.MakeMailMessageFormat(ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(),
                                                            ds.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), ds.Tables[0].Rows[0]["MessageID"].ToString(),
                                                            ds.Tables[0].Rows[0]["SFTPHeaderType"].ToString());
                                                    }
    
                                                }
    
                                            }
    
                                        }
                                        xfzbMessage = "<![CDATA[" + xfzbMessage + "]]>";
    
                                        Dictionary<string, string> Params = new Dictionary<string, string>();
                                        Params.Add("airline", FlightNo.Substring(0, 2).ToString());
                                        Params.Add("xfzb", xfzbMessage);
                                        xfzbMessage = GenerateSoapXMLRequest(Params, "sendMessageXFZB");
    
                                        if (SitaMessageHeader != "" && SitaMessageHeader.Length > 0)
                                            genericFunction.SaveMessageOutBox("SITA:XFZB", SitaMessageHeader.ToString() + "\r\n" + xfzbMessage, "", "SITAFTP", "", "", "FlightNo", "", "");
                                        if (SFTPMessageHeader != "" && SFTPMessageHeader.Length > 0)
                                            genericFunction.SaveMessageOutBox("SITA:XFZB", SFTPMessageHeader.ToString() + "\r\n" + xfzbMessage, "", "SFTP", "", "", "FlightNo", "", "");
                                        if (strEmailid != "")
                                            genericFunction.SaveMessageOutBox("XFZB", xfzbMessage, string.Empty, strEmailid, "", "", FlightNo, "", Convert.ToString(dt.Rows[i]["AWBNumber"]));
                                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 &&
                                            ds.Tables[0].Rows[0]["WebServiceURL"].ToString().Length > 0)
                                            genericFunction.SaveMessageOutBox("XFZB", xfzbMessage, string.Empty, "WEBSERVICE", "", "", FlightNo, "", Convert.ToString(dt.Rows[i]["AWBNumber"]));
    
                                    }
                                }
                            }
                        }
                    }
                }
            
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }}

        public void GenerateXFFM(string PartnerCode, string DepartureAirport, string ArrivalAirport, string FlightNo, DateTime FlightDate, string username, DateTime itdate)

        {
           try
           {
             string strEmailid = string.Empty, strSITAHeaderType = string.Empty, MessageVersion = string.Empty, SitaMessageHeader = string.Empty;
 
             string xffmMessage = string.Empty, partnerCode = string.Empty, flightorigin = string.Empty, flightDestination = string.Empty, messageHeader = string.Empty;
             string SFTPMessageHeader = string.Empty, messagetype = string.Empty, customsName = string.Empty;
 
             XFFMMessageProcessor xffmMessageProcessor = new XFFMMessageProcessor();
             xffmMessage = xffmMessageProcessor.GenerateXFFMMessage(FlightNo, FlightDate, DepartureAirport, out customsName);
 
             GenericFunction generalfunction = new GenericFunction();
             DataSet ds = new DataSet();
             if (xffmMessage != string.Empty && xffmMessage.Length > 0)
             {
                 ds = generalfunction.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "XFFM", "AIR", DepartureAirport, ArrivalAirport, FlightNo, string.Empty);
 
                 SitaMessageHeader = string.Empty;
 
                 if (ds != null && ds.Tables[0].Rows.Count > 0)
                 {
 
                     strEmailid = ds.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                     messagetype = ds.Tables[0].Rows[0]["MsgCommType"].ToString();
                     MessageVersion = ds.Tables[0].Rows[0]["MessageVersion"].ToString();
 
                     if (ds.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 1)
                     {
                         SitaMessageHeader = generalfunction.MakeMailMessageFormat(ds.Tables[0].Rows[0]["PatnerSitaID"].ToString(), ds.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), ds.Tables[0].Rows[0]["MessageID"].ToString(), ds.Tables[0].Rows[0]["SITAHeaderType"].ToString());
                     }
 
 
                     if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                     {
                         if (ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString() == ds.Tables[0].Rows[0]["SFTPAddress"].ToString())
                         {
                             SFTPMessageHeader = messagetype;
                         }
                         else
                         {
                             SFTPMessageHeader = generalfunction.MakeMailMessageFormat(ds.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(),
                                 ds.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), ds.Tables[0].Rows[0]["MessageID"].ToString(),
                                 ds.Tables[0].Rows[0]["SFTPHeaderType"].ToString());
                         }
                     }
 
                     Dictionary<string, string> Params = new Dictionary<string, string>();
                     if (customsName.ToUpper() == "DAKAR")
                     {
                         Params.Add("xffm", xffmMessage);
                     }
                     else
                     {
                         xffmMessage = "<![CDATA[" + xffmMessage + "]]>";
                         Params.Add("airline", FlightNo.Substring(0, 2).ToString());
                         Params.Add("xffm", xffmMessage);
                     }
 
                     xffmMessage = GenerateSoapXMLRequest(Params, "sendMessageXFFM", customsName);
                     if (SitaMessageHeader != "" && SitaMessageHeader.Length > 0)
                     {
                         generalfunction.SaveMessageOutBox("SITA:XFFM", SitaMessageHeader.ToString() + "\r\n" + xffmMessage, "", "SITAFTP", "", "", FlightNo, FlightDate.ToString("yyyy-MM-dd"), "");
                     }
                     if (SFTPMessageHeader != "" && SFTPMessageHeader.Length > 0)
                     {
                         generalfunction.SaveMessageOutBox("SITA:XFFM", SFTPMessageHeader.ToString() + "\r\n" + xffmMessage, "", "SFTP", "", "", FlightNo, FlightDate.ToString("yyyy-MM-dd"), "");
                     }
                     if (strEmailid != "")
                     {
                         generalfunction.SaveMessageOutBox("XFFM", xffmMessage, "", strEmailid, "", "", FlightNo, FlightDate.ToString("yyyy-MM-dd"), "");
                     }
                     if (ds.Tables[0].Rows[0]["WebServiceURL"].ToString().Length > 0)
                     {
                         generalfunction.SaveMessageOutBox("XFFM", xffmMessage, "", "WEBSERVICE", "", "", FlightNo, FlightDate.ToString("yyyy-MM-dd"), "");
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


        public string GenerateSoapXMLRequest(Dictionary<string, string> Params, string MethodName)
        {
            string soapStr =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:api2=""http://www.asycuda.org/api2"">
             <soapenv:Header/>
              <soapenv:Body>
                <api2:{0}>
                  {1}
                </api2:{0}>
              </soapenv:Body>
            </soapenv:Envelope>";


            string postValues = "";
            foreach (var param in Params)
            {

                postValues += string.Format("<api2:{0}>{1}</api2:{0}>", param.Key, param.Value);

            }

            soapStr = string.Format(soapStr, MethodName, postValues);

            return soapStr;
        }

        public string GenerateSoapXMLRequest(Dictionary<string, string> Params, string MethodName, string customsName)
        {
           try
           {
             string soapStr = string.Empty, postValues = string.Empty;
 
             if (customsName.ToUpper() == "DAKAR")
             {
                 soapStr = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:flig=""http://flightmanifest.iata/"">
                     <soapenv:Header/> 
                         <soapenv:Body>  
                             <flig:envoyerManifeste>   
                                 <arg0>
                                     {0}
                                 </arg0>    
                             </flig:envoyerManifeste>     
                         </soapenv:Body>
                     </soapenv:Envelope>";
 
                 foreach (var param in Params)
                 {
                     postValues += string.Format(soapStr, param.Value);
                 }
 
                 soapStr = postValues;
             }
             else
             {
                 soapStr =
                     @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:api2=""http://www.asycuda.org/api2"">
              <soapenv:Header/>
               <soapenv:Body>
                 <api2:{0}>
                   {1}
                 </api2:{0}>
               </soapenv:Body>
             </soapenv:Envelope>";
 
 
                 foreach (var param in Params)
                 {
                     postValues += string.Format("<api2:{0}>{1}</api2:{0}>", param.Key, param.Value);
                 }
 
                 soapStr = string.Format(soapStr, MethodName, postValues);
             }
             return soapStr;
           }
           catch (System.Exception ex)
           {
            _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            throw;
           }
        }
    }

}
