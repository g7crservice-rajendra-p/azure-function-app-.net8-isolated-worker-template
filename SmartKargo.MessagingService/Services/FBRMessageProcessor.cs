#region FWR Message Processor Class Description
/* FWR Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright              :   Copyright © 2015 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose                : 
      * Created By           :   Badiuzzaman Khan
      * Created On           :   2016-05-04
      * Approved By         :
      * Approved Date      :
      * Modified By          :  
      * Modified On          :   
      * Description           :   
     */
#endregion
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Data.Dao.Interfaces;
using System.Configuration;
using System.Data;


namespace QidWorkerRole
{
    public class FBRMessageProcessor
    {
        string unloadingportsequence = string.Empty;
        string uldsequencenum = string.Empty;
        string awbref = string.Empty;
        static string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        const string PAGE_NAME = "FBRMessageProcessor";

        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<FBRMessageProcessor> _logger;

        SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();
        public FBRMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<FBRMessageProcessor> logger)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
        }

        #region FBR Message Decoding for Update Dateabse
        /// <summary>
        /// Created By:Badiuz khan
        /// Created On:2016--5-08
        /// Description:Decoding FBR Message 
        /// </summary>
        public bool DecodingFBRMessge(string strMessage, MessageData.FBRInformation fbrMsgInformation)
        {
            bool flag = true;
            try
            {
                var fwrLine = new StringReader(strMessage.Replace("$", "\r\n").Replace("$", "\n"));
                int lineNo = 1;
                string lineText;
                while ((lineText = fwrLine.ReadLine()) != null)
                {
                    if (lineText.Length > 0)
                    {
                        switch (lineNo)
                        {
                            case 1:
                                string[] currentLineText = lineText.Split('/');
                                string fwbName = string.Empty;
                                string fwbVersion = string.Empty;
                                if (currentLineText.Length >= 1)
                                {
                                    if (currentLineText[0].Length == 3)
                                        fbrMsgInformation.MessageType = currentLineText[0] != "" ? currentLineText[0] : "";
                                }
                                if (currentLineText.Length >= 2)
                                {
                                    if (currentLineText[1].Length > 0)
                                    {
                                        fbrMsgInformation.MessageVersion = currentLineText[1] != "" ? currentLineText[1] : "";
                                    }
                                }
                                break;
                            case 2:
                                string[] currentlineFlightTextSplit = lineText.Split('/');
                                string fbrflightTag = string.Empty;
                                string fbrFlightno = string.Empty;
                                string fbrFlightDate = string.Empty;
                                string flightOrigin = string.Empty;
                                string flightDestination = string.Empty;
                                try
                                {
                                    if (currentlineFlightTextSplit.Length >= 1)
                                    {
                                        if (currentlineFlightTextSplit.Length >= 1)
                                            fbrflightTag = currentlineFlightTextSplit[0];

                                        if (currentlineFlightTextSplit.Length >= 2)
                                            fbrMsgInformation.FlightNo = currentlineFlightTextSplit[1] != "" ? currentlineFlightTextSplit[1] : "";
                                        if (currentlineFlightTextSplit.Length >= 3)
                                            fbrMsgInformation.FlightDate = currentlineFlightTextSplit[2] == "" ? "" : currentlineFlightTextSplit[2];
                                        if (currentlineFlightTextSplit.Length >= 4)
                                            if (currentlineFlightTextSplit[3].Length == 3)
                                                fbrMsgInformation.FlightOrigin = currentlineFlightTextSplit[3];
                                            else if (currentlineFlightTextSplit[3].Length == 6)
                                            {
                                                fbrMsgInformation.FlightOrigin = currentlineFlightTextSplit[3].Substring(0, 3);
                                                fbrMsgInformation.FlightDestination = currentlineFlightTextSplit[3].Substring(3);
                                            }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }

                                break;
                            default:
                                string[] currentlineTextSplit = lineText.Split('/');
                                string fbrRefTag = string.Empty;
                                string airportCityCode1 = string.Empty;
                                string officeFunctionDesignator = string.Empty;
                                string companyDesignator = string.Empty;
                                string fileReference = string.Empty;
                                string participantIdentifier = string.Empty;
                                string participantCode = string.Empty;
                                string airportCityCode2 = string.Empty;
                                try
                                {
                                    if (currentlineTextSplit.Length >= 1)
                                    {
                                        if (currentlineTextSplit[0].Length >= 1)
                                            fbrRefTag = Convert.ToString(currentlineTextSplit[0]);
                                        if (currentlineTextSplit.Length >= 2)
                                        {
                                            if (currentlineTextSplit[1].Length == 3)
                                                airportCityCode1 = Convert.ToString(currentlineTextSplit[1]);
                                            else if (currentlineTextSplit[1].Length == 5)
                                                officeFunctionDesignator = Convert.ToString(currentlineTextSplit[1].Substring(3, 2));
                                            else if (currentlineTextSplit[1].Length == 7)
                                                companyDesignator = Convert.ToString(currentlineTextSplit[1].Substring(5, 2));
                                        }
                                        if (currentlineTextSplit.Length >= 3)
                                            fileReference = Convert.ToString(currentlineTextSplit[2]);
                                        if (currentlineTextSplit.Length >= 4)
                                            participantIdentifier = Convert.ToString(currentlineTextSplit[3]);
                                        if (currentlineTextSplit.Length >= 5)
                                            participantCode = Convert.ToString(currentlineTextSplit[4]);
                                        if (currentlineTextSplit.Length >= 6)
                                            airportCityCode2 = Convert.ToString(currentlineTextSplit[5]);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // clsLog.WriteLogAzure(ex);
                                    _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                                }
                                break;



                        }
                        lineNo++;
                    }
                }
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                // clsLog.WriteLogAzure("Error in Decoding FBR Message " + ex.ToString());
                _logger.LogError("Error in Decoding FBR Message {0}" , ex.ToString());
            }
            return flag;
        }

        #endregion

        public async Task<bool> ValidatandGenerateFBLMessage(MessageData.FBRInformation fwrInformation, string strMessage, int refNo, string strmessageFrom, string strFromID, string strStatus)
        {
            GenericFunction GF = new GenericFunction();
            bool flag = false;
            try
            {
                MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                int count1 = 0;
                int count2 = 0;
                string strFLight = string.Empty, FBLMessageversion = string.Empty, SitaMessageHeader = string.Empty, Emailaddress = string.Empty;
                DataSet dsData = new DataSet();

                string FlightOrigin = fwrInformation.FlightOrigin, commtype = string.Empty, messageid = "";
                string FlightDestination = (fwrInformation.FlightDestination == null ? "" : fwrInformation.FlightDestination);
                DateTime flightdate = new DateTime();

                string month = fwrInformation.FlightDate.Substring(2, 3);
                string date1 = fwrInformation.FlightDate.Substring(0, 2);

                flightdate = DateTime.Parse(DateTime.Parse("1." + month + " 2008").Month.ToString().PadLeft(2, '0') + "/" + date1.PadLeft(2, '0') + "/" + +System.DateTime.Today.Year);

                if (strFromID.Contains("SITA"))
                    commtype = "SITAFTP";

                if (strFromID.Contains("SFTP"))
                    commtype = "SFTP";

                if (strFromID == "FTP")
                    commtype = "FTP";

                else
                    commtype = "EMAIL";


                GenericFunction gf = new GenericFunction();
                gf.UpdateInboxFromMessageParameter(refNo, string.Empty, fwrInformation.FlightNo, FlightOrigin, FlightDestination, "FBR", "FBR", flightdate);

                string strCarriercode = fwrInformation.FlightNo.Substring(0, 2);

                string strflightno = fwrInformation.FlightNo.Substring(2);
                char ch = strflightno[strflightno.Length - 1];
                if (char.IsNumber(ch))
                {
                    int strfltno = int.Parse(fwrInformation.FlightNo.Substring(2));
                    strFLight = Convert.ToString(strfltno);
                }
                else
                    strFLight = strflightno;
                strFLight = strCarriercode.Trim() + "" + strFLight.Trim();
                string date = string.Empty, flightMonth = string.Empty;
                if (fwrInformation.FlightDate.Length == 5)
                {
                    date = fwrInformation.FlightDate.Substring(0, 2);
                    flightMonth = fwrInformation.FlightDate.Substring(2);
                }
                else
                {
                    date = fwrInformation.FlightDate.Substring(0, 1);
                    flightMonth = fwrInformation.FlightDate.Substring(1);
                }
                string flightdate1 = date + "/" + flightMonth + "/" + System.DateTime.Today.Year;

                dsData = await GetRecordforGenerateFBLMessage(FlightOrigin, FlightDestination, strFLight, flightdate1);
                if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
                {
                    DataSet dscheckconfiguration = gf.GetSitaAddressandMessageVersion(strCarriercode, "FBL", "AIR", "", "", "", string.Empty, "");
                    if (dscheckconfiguration != null && dscheckconfiguration.Tables[0].Rows.Count > 0)
                    {
                        Emailaddress = dscheckconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        string MessageCommunicationType = dscheckconfiguration.Tables[0].Rows[0]["MsgCommType"].ToString();
                        FBLMessageversion = dscheckconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                        messageid = dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString();


                    }
                    DateTime dtFlight = DateTime.Now;
                    objFBLInfo.date = System.DateTime.Now.Day.ToString().PadLeft(2, '0');
                    dtFlight = DateTime.Parse(DateTime.Parse("1." + flightdate1.Substring(3, 3) + " 2008").Month.ToString().PadLeft(2, '0') + "/" + flightdate1.Substring(0, 2) + "/" + +System.DateTime.Today.Year);
                    objFBLInfo.date = dtFlight.ToString("dd");
                    objFBLInfo.month = dtFlight.ToString("MMM").ToUpper();
                    objFBLInfo.fltairportcode = FlightOrigin;
                    objFBLInfo.endmesgcode = "LAST";
                    objFBLInfo.fblversion = FBLMessageversion;
                    objFBLInfo.messagesequencenum = "1";
                    //flight details
                    if (dsData.Tables[0].Rows.Count > 0)
                    {
                        count1 = 1;
                        foreach (DataRow dr in dsData.Tables[0].Rows)
                        {
                            MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                            objTempUnloadingPort.unloadingairport = dr[2].ToString().ToUpper();
                            Array.Resize(ref objUnloadingPort, count1);
                            objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                            objFBLInfo.carriercode = dsData.Tables[0].Rows[0]["FlightID"].ToString().Substring(0, 2);
                            objFBLInfo.fltnum = dsData.Tables[0].Rows[0]["FlightID"].ToString().Substring(3);
                            count1++;
                            if (dsData.Tables[1].Rows.Count > 0)
                            {
                                bool isNil = true;
                                foreach (DataRow drAWB in dsData.Tables[1].Rows)
                                {
                                    if (objTempUnloadingPort.unloadingairport.Trim() == drAWB["DestinationCode"].ToString().Trim())
                                    {
                                        isNil = false;
                                        break;
                                    }
                                }
                                if (isNil)
                                {
                                    objUnloadingPort[count1 - 2].nilcargocode = "NIL";
                                }
                            }
                            else
                            {
                                objUnloadingPort[count1 - 2].nilcargocode = "NIL";
                            }
                        }
                    }
                    //awb details
                    if (dsData.Tables[1].Rows.Count > 0)
                    {

                        objFBLInfo.carriercode = dsData.Tables[1].Rows[0]["CarrierCode"].ToString();
                        objFBLInfo.fltnum = dsData.Tables[1].Rows[0]["FlightNo"].ToString();

                        count2 = 1;
                        foreach (DataRow dr in dsData.Tables[1].Rows)
                        {
                            MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                            string AWBNumber = dr[0].ToString().Trim();
                            objTempConsInfo.airlineprefix = dr["Prefix"].ToString().Trim();//Prefix
                            objTempConsInfo.awbnum = dr["AWBNumber"].ToString().Trim();
                            objTempConsInfo.origin = dr["OrginCode"].ToString().Trim().ToUpper();
                            objTempConsInfo.dest = dr["DestinationCode"].ToString().Trim().ToUpper();
                            if (dr["Piececode"].ToString() == "P")
                            {
                                objTempConsInfo.consigntype = dr["Piececode"].ToString();
                                objTempConsInfo.pcscnt = dr["FlightPcs"].ToString();
                                objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                objTempConsInfo.weight = dr["AWBGwt"].ToString();

                                objTempConsInfo.TotalConsignmentType = "T";
                                objTempConsInfo.AWBPieces = dr["AWBPcs"].ToString();
                            }
                            else
                            {
                                objTempConsInfo.consigntype = dr["Piececode"].ToString();
                                objTempConsInfo.pcscnt = dr["FlightPcs"].ToString();
                                objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                objTempConsInfo.weight = dr["AWBGwt"].ToString();
                            }


                            objTempConsInfo.volumecode = dr["VolumeCode"].ToString();
                            objTempConsInfo.volumeamt = dr["Volume"].ToString();

                            objTempConsInfo.manifestdesc = dr["CommDesc"].ToString().Trim().ToUpper();
                            objTempConsInfo.splhandling = dr["SHCCodes"].ToString().Trim().ToUpper();

                            Array.Resize(ref objConsInfo, count2);
                            objConsInfo[count2 - 1] = objTempConsInfo;
                            count2++;
                        }
                    }
                    if (count1 > 0)
                    {
                        MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                        MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                        MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                        MessageData.otherserviceinfo objOtherInfo = new MessageData.otherserviceinfo("");
                        string FBLMsg = cls_Encode_Decode.EncodeFBLforsend(objFBLInfo, objUnloadingPort, objConsInfo, objDimenInfo, objConsOriginInfo, objULDInfo, objOtherInfo);
                        if (FBLMsg != null)
                        {
                            if (FBLMsg != "")
                            {

                                if (dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Trim().Length > 0)
                                {
                                    SitaMessageHeader = gf.MakeMailMessageFormat(strmessageFrom, dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                                    gf.SaveMessageOutBox("FBL", SitaMessageHeader + "\r\n" + FBLMsg, "SITAFTP", "SITAFTP", FlightOrigin, "", objFBLInfo.carriercode + objFBLInfo.fltnum, dtFlight.ToString(), "");
                                }
                                if (commtype.Contains("SFTP") && dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Trim().Length > 0)
                                {

                                    SitaMessageHeader = gf.MakeMailMessageFormat(strmessageFrom, dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                                    gf.SaveMessageOutBox("FBL", SitaMessageHeader + "\r\n" + FBLMsg, "SFTP", "SFTP", FlightOrigin, "", objFBLInfo.carriercode + objFBLInfo.fltnum, dtFlight.ToString(), "");

                                }
                                if (commtype == "FTP")
                                    gf.SaveMessageOutBox("FBL", FBLMsg, "FTP", "FTP", FlightOrigin, "", objFBLInfo.carriercode + objFBLInfo.fltnum, dtFlight.ToString(), "");

                                else
                                {
                                    string ToEmailAddress = (strmessageFrom == string.Empty ? Emailaddress : strmessageFrom + "," + Emailaddress);
                                    ToEmailAddress = (ToEmailAddress == string.Empty ? "priyanka@smartkargo.com" : ToEmailAddress);
                                    gf.SaveMessageOutBox("FBL", FBLMsg.ToString(), string.Empty, ToEmailAddress, FlightOrigin, "", objFBLInfo.carriercode + objFBLInfo.fltnum, dtFlight.ToString(), "");
                                }


                            }
                        }
                    }
                }

                return flag;

            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                flag = false;
            }
            return flag;
        }

        #region GetFBLData
        private async Task<DataSet> GetRecordforGenerateFBLMessage(string strFlightOrigin, string strFlightDestination, string FlightNo, string FlightDate)
        {

            DataSet? dsData = new DataSet();
            try
            {
                //SQLServer dtb = new SQLServer();

                string procedure = "spGetFBLDataForSend";

                //string[] paramname = new string[] { "FlightNo", "FlightOrigin", "FlightDestination", "FltDate" };
                //object[] paramvalue = new object[] { FlightNo, strFlightOrigin, strFlightDestination, FlightDate };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime };
                //dsData = _readWriteDao.SelectRecords(procedure, paramname, paramvalue, paramtype);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@FlightNo", SqlDbType.VarChar) { Value = FlightNo },
                    new SqlParameter("@FlightOrigin", SqlDbType.VarChar) { Value = strFlightOrigin },
                    new SqlParameter("@FlightDestination", SqlDbType.VarChar) { Value = strFlightDestination },
                    new SqlParameter("@FltDate", SqlDbType.DateTime) { Value = FlightDate }
                };

                dsData = await _readWriteDao.SelectRecords(procedure, parameters);
            }
            catch (Exception ex)
            {
                // clsLog.WriteLogAzure(ex.Message);
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
             }

            return dsData;
        }
        #endregion


        public async Task GenerateFBLMessage(string strFlightOrigin, string strFlightDestination, string FlightNo, string FlightDate)
        {
            try
            {
                string SitaMessageHeader = string.Empty, FblMessageversion = string.Empty, Emailaddress = string.Empty, SFTPMessageHeader = string.Empty;
                GenericFunction gf = new GenericFunction();
                MessageData.fblinfo objFBLInfo = new MessageData.fblinfo("");
                MessageData.unloadingport[] objUnloadingPort = new MessageData.unloadingport[0];
                MessageData.consignmnetinfo[] objConsInfo = new MessageData.consignmnetinfo[0];
                int count1 = 0;
                int count2 = 0;
                DataSet dsData = await GetRecordforGenerateFBLMessage(strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                if (dsData != null && dsData.Tables.Count > 1 && dsData.Tables[0].Rows.Count > 0)
                {
                    DataSet dsmessage = gf.GetSitaAddressandMessageVersion(FlightNo.Substring(0, 2), "FBL", "AIR", strFlightOrigin, strFlightDestination, FlightNo, string.Empty);
                    if (dsmessage != null && dsmessage.Tables[0].Rows.Count > 0)
                    {
                        Emailaddress = dsmessage.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        string MessageCommunicationType = dsmessage.Tables[0].Rows[0]["MsgCommType"].ToString();
                        FblMessageversion = dsmessage.Tables[0].Rows[0]["MessageVersion"].ToString();
                        if (dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                            SitaMessageHeader = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString());
                        if (dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                            SFTPMessageHeader = gf.MakeMailMessageFormat(dsmessage.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dsmessage.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dsmessage.Tables[0].Rows[0]["MessageID"].ToString(), dsmessage.Tables[0].Rows[0]["SFTPHeaderType"].ToString());
                    }
                    if (Emailaddress.Trim() != string.Empty || SitaMessageHeader.Trim() != string.Empty || SFTPMessageHeader.Trim() != string.Empty)
                    {
                        DateTime dtFlight = DateTime.Now;
                        objFBLInfo.date = DateTime.Now.Day.ToString().PadLeft(2, '0');
                        try
                        {
                            dtFlight = DateTime.ParseExact(FlightDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);//, "dd/MM/yyyy", null);
                            objFBLInfo.date = DateTime.ParseExact(FlightDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).Day.ToString().PadLeft(2, '0');
                        }
                        catch (Exception ex)
                        {
                            // clsLog.WriteLogAzure(ex.Message);
                            _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                         }
    
                        objFBLInfo.month = dtFlight.ToString("MMM").ToUpper();
                        objFBLInfo.fltairportcode = strFlightOrigin;
                        objFBLInfo.endmesgcode = "LAST";
                        objFBLInfo.fblversion = FblMessageversion;
                        objFBLInfo.messagesequencenum = "1";
                        //flight details
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            count1 = 1;
                            foreach (DataRow dr in dsData.Tables[0].Rows)
                            {
                                MessageData.unloadingport objTempUnloadingPort = new MessageData.unloadingport("");
                                objTempUnloadingPort.unloadingairport = dr[2].ToString().ToUpper();
                                Array.Resize(ref objUnloadingPort, count1);
                                objUnloadingPort[count1 - 1] = objTempUnloadingPort;
                                objFBLInfo.carriercode = dsData.Tables[0].Rows[0]["FlightID"].ToString().Substring(0, 2);
                                objFBLInfo.fltnum = dsData.Tables[0].Rows[0]["FlightID"].ToString().Substring(2);
                                count1++;
                                if (dsData.Tables[1].Rows.Count > 0)
                                {
                                    bool isNil = true;
                                    foreach (DataRow drAWB in dsData.Tables[1].Rows)
                                    {
                                        if (objTempUnloadingPort.unloadingairport.Trim() == drAWB["DestinationCode"].ToString().Trim())
                                        {
                                            isNil = false;
                                            break;
                                        }
                                    }
                                    if (isNil)
                                    {
                                        objUnloadingPort[count1 - 2].nilcargocode = "NIL";
                                    }
                                }
                                else
                                {
                                    objUnloadingPort[count1 - 2].nilcargocode = "NIL";
                                }
                            }
                        }
                        //awb details
                        if (dsData.Tables[1].Rows.Count > 0)
                        {
                            objFBLInfo.carriercode = dsData.Tables[1].Rows[0]["CarrierCode"].ToString();
                            objFBLInfo.fltnum = dsData.Tables[1].Rows[0]["FlightNo"].ToString();
    
                            count2 = 1;
                            foreach (DataRow dr in dsData.Tables[1].Rows)
                            {
                                MessageData.consignmnetinfo objTempConsInfo = new MessageData.consignmnetinfo("");
                                string AWBNumber = dr[0].ToString().Trim();
                                objTempConsInfo.airlineprefix = dr["Prefix"].ToString().Trim();//Prefix
                                objTempConsInfo.awbnum = dr["AWBNumber"].ToString().Trim();
                                objTempConsInfo.origin = dr["OrginCode"].ToString().Trim().ToUpper();
                                objTempConsInfo.dest = dr["DestinationCode"].ToString().Trim().ToUpper();
                                if (dr["Piececode"].ToString() == "P")
                                {
                                    objTempConsInfo.consigntype = dr["Piececode"].ToString();
                                    objTempConsInfo.pcscnt = dr["FlightPcs"].ToString();
                                    objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                    objTempConsInfo.weight = dr["AWBGwt"].ToString();
    
                                    objTempConsInfo.TotalConsignmentType = "T";
                                    objTempConsInfo.AWBPieces = dr["AWBPcs"].ToString();
                                }
                                else
                                {
                                    objTempConsInfo.consigntype = dr["Piececode"].ToString();
                                    objTempConsInfo.pcscnt = dr["FlightPcs"].ToString();
                                    objTempConsInfo.weightcode = dr["UOM"].ToString().Trim();
                                    objTempConsInfo.weight = dr["AWBGwt"].ToString();
                                }
    
                                objTempConsInfo.volumecode = dr["VolumeCode"].ToString();
                                objTempConsInfo.volumeamt = dr["Volume"].ToString();
    
                                objTempConsInfo.manifestdesc = dr["CommDesc"].ToString().Trim().ToUpper();
                                objTempConsInfo.splhandling = dr["SHCCodes"].ToString().Trim().ToUpper();
    
                                Array.Resize(ref objConsInfo, count2);
                                objConsInfo[count2 - 1] = objTempConsInfo;
                                count2++;
                            }
                        }
                        if (count1 > 0)
                        {
                            MessageData.dimensionnfo[] objDimenInfo = new MessageData.dimensionnfo[0];
                            MessageData.consignmentorigininfo[] objConsOriginInfo = new MessageData.consignmentorigininfo[0];
                            MessageData.ULDinfo[] objULDInfo = new MessageData.ULDinfo[0];
                            MessageData.otherserviceinfo objOtherInfo = new MessageData.otherserviceinfo("");
                            //Cls_BL cls_BL = new Cls_BL();
                            string FBLMsg = cls_Encode_Decode.EncodeFBLforsend(objFBLInfo, objUnloadingPort, objConsInfo, objDimenInfo, objConsOriginInfo, objULDInfo, objOtherInfo);
                            if (FBLMsg != null)
                            {
                                if (FBLMsg.Trim() != "")
                                {
                                    //Multipart FBL
                                    if (FBLMsg.Contains("#"))
                                    {
                                        string[] MulitpartFBL = FBLMsg.Split('#');
    
                                        foreach (string FBLMessage in MulitpartFBL)
                                        {
                                            if (SitaMessageHeader != "")
                                                gf.SaveMessageOutBox("FBL", SitaMessageHeader + "\r\n" + FBLMessage, "SITAFTP", "SITAFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                                            if (Emailaddress != "")
                                                gf.SaveMessageOutBox("FBL", FBLMessage, string.Empty, Emailaddress, strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                                            if (SFTPMessageHeader.Trim().Length > 0)
                                                gf.SaveMessageOutBox("FBL", SFTPMessageHeader + "\r\n" + FBLMessage, "SFTP", "SFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                                        }
                                    }
                                    else
                                    {
                                        if (SitaMessageHeader != "")
                                            gf.SaveMessageOutBox("FBL", SitaMessageHeader + "\r\n" + FBLMsg, "SITAFTP", "SITAFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                                        if (Emailaddress != "")
                                            gf.SaveMessageOutBox("FBL", FBLMsg, string.Empty, Emailaddress, strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                                        if (SFTPMessageHeader.Trim().Length > 0)
                                            gf.SaveMessageOutBox("FBL", SFTPMessageHeader + "\r\n" + FBLMsg, "SFTP", "SFTP", strFlightOrigin, strFlightDestination, FlightNo, FlightDate);
                                    }
                                }
                            }
                        }
                    }
                }
    
            }
            catch (System.Exception ex)
            {   
                _logger.LogError(ex,$"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }        }
    }
}
