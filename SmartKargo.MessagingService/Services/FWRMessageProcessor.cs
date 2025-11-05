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
using System;
using System.Linq;
using System.IO;
using System.Data;
using QID.DataAccess;
using System.Configuration;

namespace QidWorkerRole
{
    public class FWRMessageProcessor
    {

        string unloadingportsequence = string.Empty;
        string uldsequencenum = string.Empty;
        string awbref = string.Empty;
        static string strConnection = ConfigurationManager.ConnectionStrings["ConStr"].ToString();
        const string PAGE_NAME = "FWRMessageProcessor";
        SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();

        public FWRMessageProcessor()
        {

        }

        #region FWR Message Decoding for Update Dateabse
        /// <summary>
        /// Created By:Badiuz khan
        /// Created On:2016-01-15
        /// Description:Decoding FWR Message 
        /// </summary>


        public bool DecodingFWRMessge(string strMessage, MessageData.FWRInformation fwrMessaageInformation)
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
                                        fwrMessaageInformation.MessageType = currentLineText[0] != "" ? currentLineText[0] : "";
                                }
                                if (currentLineText.Length >= 2)
                                {
                                    if (currentLineText[1].Length > 0)
                                    {
                                        fwrMessaageInformation.MessageVersion = currentLineText[1] != "" ? currentLineText[1] : "";
                                    }

                                }
                                break;
                            case 2:
                                string[] currentlineText = lineText.Split('/');
                                if (currentlineText.Length >= 1)
                                {
                                    if (currentlineText[0].Length >= 8)
                                    {
                                        int indexOfHyphen = currentlineText[0].IndexOf('-');
                                        if (indexOfHyphen == 3)
                                        {
                                            fwrMessaageInformation.AirlinePrefix = currentlineText[0].Substring(0, 3);
                                            fwrMessaageInformation.AWBNo = currentlineText[0].Substring(4, 8);
                                        }

                                    }
                                }

                                break;
                            default:
                                if (lineText.Trim().Length > 2)
                                {
                                    var tagName = lineText.Substring(0, 3);
                                    switch (tagName)
                                    {

                                        case "REF":
                                            try
                                            {
                                                string[] currentLineReferenceText = lineText.Split('/');
                                                if (currentLineReferenceText.Length >= 1)
                                                    fwrMessaageInformation.RefereceOriginTag = currentLineReferenceText[1];
                                                if (currentLineReferenceText.Length >= 2)
                                                    fwrMessaageInformation.RefereceFileTag = currentLineReferenceText[2];
                                            }
                                            catch (Exception ex)
                                            {
                                                clsLog.WriteLogAzure("Error in Decoding FSB Message REF TAG " + ex.ToString());
                                            }
                                            break;

                                        default:
                                            break;
                                    }
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
                // //scm.logexception(ref ex);
                flag = false;
                clsLog.WriteLogAzure("Error in Decoding FWR Message " + ex.ToString());
            }
            return flag;
        }

        private string ReadFile(string tagName, string strMessage)
        {
            var fsbLine = new StringReader(strMessage);
            string lineText;
            var tagText = string.Empty;
            var readLine = false;
            try
            {

                while ((lineText = fsbLine.ReadLine()) != null)
                {
                    if (readLine)
                    {
                        if (lineText.Trim().Length > 0)
                            if (lineText.Substring(0, 1) == "/")
                                tagText += "#" + lineText;
                            else
                                break;
                    }
                    if (lineText.Trim().Length > 2)
                        if (lineText.Substring(0, 3) == tagName)
                        {
                            tagText = lineText;
                            readLine = true;
                        }
                }


            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);

            }
            return tagText;
        }

        #endregion



        #region validateSaveFWRMessage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsbMessage"></param>
        /// <param name="fsbShipper"></param>
        /// <param name="fsbConsignee"></param>
        /// <param name="RouteIformation"></param>
        /// <param name="Dimensionformation"></param>
        /// <param name="bublistinformation"></param>
        /// <returns></returns>
        public bool ValidateAndSendFWBMessage(MessageData.FWRInformation fwrInformation, int messgeId, string strMessage, string strMessageFrom, string strFromID, string strStatus)
        {
            SQLServer dtb = new SQLServer();

            bool MessageStatus = false;
            try
            {
                GenericFunction gf = new GenericFunction();
                gf.UpdateInboxFromMessageParameter(messgeId, fwrInformation.AirlinePrefix + "-" + fwrInformation.AWBNo, string.Empty, string.Empty, string.Empty, "FWR", strMessageFrom, DateTime.Parse("1900-01-01"));
                string commtype = string.Empty;
                if (strFromID.Contains("SITA"))
                {
                    commtype = "SITAFTP";
                }
                if (strFromID.Contains("SFTP"))
                {
                    commtype = "SFTP";
                }
                if (strFromID == "FTP")
                {
                    commtype = "FTP";
                }
                else
                {
                    commtype = "EMAIL";
                }

                string FWBMessageversion = string.Empty, SitaMessageHeader = string.Empty, Emailaddress = string.Empty, messageid = "", strFWBMessage = "";



                DataSet dscheckconfiguration = gf.GetSitaAddressandMessageVersion("", "FWB", "AIR", "", "", "", string.Empty, fwrInformation.AirlinePrefix);
                if (dscheckconfiguration != null && dscheckconfiguration.Tables[0].Rows.Count > 0)
                {
                    Emailaddress = dscheckconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                    string MessageCommunicationType = dscheckconfiguration.Tables[0].Rows[0]["MsgCommType"].ToString();
                    FWBMessageversion = dscheckconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                    messageid = dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString();


                }


                DataSet dsFWB = GetAWBRecordForGenerateFWBMessage(fwrInformation.AWBNo, fwrInformation.AirlinePrefix);
                string Error = "";
                //string FWBMessage= EncodeFWB(dsFWB, ref Error, FWBMessageversion);

                if (dsFWB != null && dsFWB.Tables.Count > 0 && dsFWB.Tables[0].Rows.Count > 0)
                {
                    strFWBMessage = EncodeFWB(dsFWB, ref Error, FWBMessageversion);
                    if (strFWBMessage != "")
                    {

                        if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows.Count > 0)
                        {
                            if (dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                            {
                                SitaMessageHeader = gf.MakeMailMessageFormat(dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                                gf.SaveMessageOutBox("FWB", SitaMessageHeader + "\r\n" + strFWBMessage, "SITAFTP", "SITAFTP", "", "", "", "", fwrInformation.AirlinePrefix + "-" + fwrInformation.AWBNo);
                            }
                            if (dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                            {
                                string SFTPHeaderSITAddress = string.Empty;
                                SFTPHeaderSITAddress = gf.MakeMailMessageFormat(dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                                gf.SaveMessageOutBox("FWB", SFTPHeaderSITAddress + "\r\n" + strFWBMessage, "SFTP", "SFTP", "", "", "", "", fwrInformation.AirlinePrefix + "-" + fwrInformation.AWBNo);
                            }
                        }
                        string ToEmailAddress = (strMessageFrom == string.Empty ? Emailaddress : strMessageFrom + "," + Emailaddress);
                        if (ToEmailAddress.Trim().Length > 0)
                        {
                            gf.SaveMessageOutBox("FWB", strFWBMessage.ToString(), string.Empty, ToEmailAddress, "", "", "", "", fwrInformation.AirlinePrefix + "-" + fwrInformation.AWBNo);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //scm.logexception(ref ex);
                clsLog.WriteLogAzure("Error on FWR Message Processing " + ex.ToString());
                MessageStatus = false;
            }
            return MessageStatus;
        }




        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sitaMessage"></param>
        /// <returns></returns>
        public DataSet GetAWBRecordForGenerateFWBMessage(string strAWBNumber, string strAwbPrefix)
        {
            DataSet dssitaMessage = new DataSet();
            try
            {

                SQLServer da = new SQLServer();
                string[] paramname = new string[] { "AWBNumber", "AWBPrefix" };
                object[] paramvalue = new object[] { strAWBNumber, strAwbPrefix };
                SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                dssitaMessage = da.SelectRecords("SP_GetAWBRecordForFWB", paramname, paramvalue, paramtype);
            }
            catch (Exception)
            {
                //scm.logexception(ref ex);
                dssitaMessage = null;
            }
            return dssitaMessage;
        }

        #region EncodeFWB

        #region EncodeFWB
        public string EncodeFWB(DataSet dsAWB, ref string Error, string fwbMessageVersion)
        {
            string FWBMsg = string.Empty;
            try
            {
                string strDimensionTag = string.Empty, strVolumeTag = string.Empty, strULDBUP = string.Empty, strRouteTag = string.Empty;
                MessageData.fwbinfo FWBData = new MessageData.fwbinfo("");
                MessageData.othercharges[] othData = new MessageData.othercharges[0];
                MessageData.otherserviceinfo[] othSrvData = new MessageData.otherserviceinfo[0];
                MessageData.RateDescription[] fwbrates = new MessageData.RateDescription[0];
                MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                GenericFunction GF = new GenericFunction();
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
                    //      DataTable DT10 = dsAWB.Tables[9].Copy();

                    FWBData.fwbversionnum = fwbMessageVersion;
                    FWBData.airlineprefix = DT1.Rows[0]["AWBPrefix"].ToString().Trim();
                    FWBData.awbnum = DT1.Rows[0]["AWBNumber"].ToString().Trim();
                    FWBData.origin = DT1.Rows[0]["OriginCode"].ToString().Trim().ToUpper();
                    FWBData.dest = DT1.Rows[0]["DestinationCode"].ToString().Trim().ToUpper();
                    FWBData.consigntype = "T";
                    FWBData.pcscnt = DT1.Rows[0]["PiecesCount"].ToString().Trim();
                    FWBData.weightcode = DT1.Rows[0]["UOM"].ToString().Trim();
                    FWBData.weight = DT1.Rows[0]["GrossWeight"].ToString().Trim();
                    FWBData.densityindicator = string.Empty;
                    FWBData.densitygrp = string.Empty;

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
                            if (DT2.Rows[0]["CodeDescription"].ToString().Length > 15)
                                rate.goodsnature = DT2.Rows[0]["CodeDescription"].ToString().Substring(0, 15).Replace(",", " ").Replace(")", " ");
                            else
                                rate.goodsnature = DT2.Rows[0]["CodeDescription"].ToString().Replace(",", " ").Replace(")", " ");

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
                            if (int.Parse(dtDimension.Rows[d]["Length"].ToString()) > 0 && int.Parse(dtDimension.Rows[d]["Breadth"].ToString()) > 0 && int.Parse(dtDimension.Rows[d]["Height"].ToString()) > 0 && int.Parse(dtDimension.Rows[d]["PieceNo"].ToString()) > 0)
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


                    DataTable dtcheckWalkingcustomer = GF.RemoveAgentTagforWalkingCustomer("Quick Booking", DT1.Rows[0]["ShippingAgentCode"].ToString()).Tables[0];
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
                            FWBData.agentaccnum = dtAgent.Rows[0]["AgentCode"].ToString().ToUpper();
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


                    FWBData.currency = DT2.Rows[0]["Currency"].ToString().ToUpper();
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
                        return FWBMsg;
                    }
                    // }

                    #endregion
                    FWBMsg = EncodeFWBForSend(ref FWBData, ref othData, ref othSrvData, ref fwbrates, ref custominfo, strDimensionTag, strVolumeTag, strULDBUP, strRouteTag);

                    //FWBMsg = EncodeFWBForSend(ref FWBData, ref othData, ref othSrvData, ref fwbrates, ref custominfo, strDimensionTag, strVolumeTag, strULDBUP, strRouteTag);

                    //FWBMsg = EncodeFWBForSend(ref FWBData, ref othData, ref othSrvData, ref fwbrates, ref custominfo, strDimensionTag, strVolumeTag, strULDBUP, strRouteTag);
                }
            }
            catch (Exception ex)
            {
                //SCMExceptionHandling.logexception(ref ex);
                Error = ex.Message;
            }
            return FWBMsg;
        }

        #endregion

        public string GenerateFWBMessage(DataSet dsAWB)
        {
            GenericFunction GF = new GenericFunction();
            string FWBMsg = string.Empty;
            try
            {
                string strDimensionTag = string.Empty, strVolumeTag = string.Empty, strULDBUP = string.Empty, strRouteTag = string.Empty;
                MessageData.fwbinfo FWBData = new MessageData.fwbinfo("");
                MessageData.othercharges[] othData = new MessageData.othercharges[0];
                MessageData.otherserviceinfo[] othSrvData = new MessageData.otherserviceinfo[0];
                MessageData.RateDescription[] fwbrates = new MessageData.RateDescription[0];
                MessageData.customsextrainfo[] custominfo = new MessageData.customsextrainfo[0];
                #region Prepare Structure

                if (dsAWB != null && dsAWB.Tables.Count > 0)
                {


                    DataTable DT1 = dsAWB.Tables[0].Copy();
                    DataTable DT2 = dsAWB.Tables[1].Copy();
                    DataTable DT3 = dsAWB.Tables[2].Copy();
                    DataTable DT4 = dsAWB.Tables[3].Copy();
                    DataTable DT5 = dsAWB.Tables[4].Copy();
                    DataTable DT7 = dsAWB.Tables[6].Copy();
                    DataTable DT8 = dsAWB.Tables[7].Copy();  //Rate Charges
                    DataTable DT9 = dsAWB.Tables[8].Copy();
                    DataTable DT10 = dsAWB.Tables[9].Copy();


                    FWBData.airlineprefix = DT1.Rows[0]["AWBPrefix"].ToString().Trim();
                    FWBData.awbnum = DT1.Rows[0]["AWBNumber"].ToString().Trim();
                    FWBData.origin = DT1.Rows[0]["OriginCode"].ToString().Trim().ToUpper();
                    FWBData.dest = DT1.Rows[0]["DestinationCode"].ToString().Trim().ToUpper();
                    FWBData.consigntype = "T";
                    FWBData.pcscnt = DT1.Rows[0]["PiecesCount"].ToString().Trim();
                    FWBData.weightcode = DT1.Rows[0]["UOM"].ToString().Trim();
                    FWBData.weight = DT1.Rows[0]["GrossWeight"].ToString().Trim();
                    FWBData.densityindicator = string.Empty;
                    FWBData.densitygrp = string.Empty;

                    string FlightNo = string.Empty;
                    if (DT4 != null && DT4.Rows.Count > 0)
                    {

                        String strCarrierCode = DT4.Rows[0]["Carrier"].ToString();

                        DataTable dtmessageversion = GF.GetEdiMessageFormat("FWB", strCarrierCode, "GetEdiMessageVersion").Tables[0];
                        if (dtmessageversion != null && dtmessageversion.Rows.Count > 0)
                            FWBData.fwbversionnum = dtmessageversion.Rows[0]["MessageVersion"].ToString();
                        else
                            FWBData.fwbversionnum = "16";
                        if (DT4.Rows[0]["FWBFlightDayTag"].ToString() != "")
                            FWBData.fltnum = DT4.Rows[0]["FWBFlightDayTag"].ToString();
                        //if (FlightNo.Length <= 2)
                        //{
                        //    FWBData.carriercode = FlightNo;
                        //    FWBData.fltnum =  DT4.Rows[0]["FWBFlightDayTag"].ToString();
                        //}
                        //else
                        //{
                        //    FWBData.carriercode = FlightNo.Substring(0, 2);
                        //    FWBData.fltnum = FlightNo.Substring(2, FlightNo.Length - 2);
                        //    DateTime FlightDate = Convert.ToDateTime(DT4.Rows[0]["FltDate"].ToString());
                        //    FWBData.fltday = FlightDate.Day.ToString().PadLeft(2, '0');

                        //}
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
                            rate.weightindicator = "K";
                            rate.weight = DT8.Rows[i]["Weight"].ToString();
                            rate.rateclasscode = DT8.Rows[i]["RateClass"].ToString();//RateClass
                            rate.commoditynumber = DT8.Rows[i]["CommCode"].ToString();
                            rate.awbweight = DT8.Rows[i]["ChargedWeight"].ToString();
                            rate.chargerate = DT8.Rows[i]["RatePerKg"].ToString();
                            rate.chargeamt = DT8.Rows[i]["Total"].ToString();
                            if (DT2.Rows[0]["CodeDescription"].ToString().Length > 15)
                                rate.goodsnature = DT2.Rows[0]["CodeDescription"].ToString().Substring(0, 15).Replace(",", " ").Replace(")", " ");
                            else
                                rate.goodsnature = DT2.Rows[0]["CodeDescription"].ToString().Replace(",", " ").Replace(")", " ");

                            Array.Resize(ref fwbrates, fwbrates.Length + 1);
                            fwbrates[fwbrates.Length - 1] = rate;
                        }

                    }

                    Decimal volumeWeight = 0;

                    if (DT3 != null && DT3.Rows.Count > 0)
                    {


                        for (int d = 0; d < DT3.Rows.Count; d++)
                        {
                            RtdIncreament += 1;
                            switch (DT3.Rows[d]["Units"].ToString().ToUpper())
                            {
                                case "CMT":
                                    volumeWeight += Math.Round(
                                           (decimal.Parse(DT3.Rows[d]["Length"].ToString()) * decimal.Parse(DT3.Rows[d]["Breadth"].ToString()) *
                                            decimal.Parse(DT3.Rows[d]["Height"].ToString()) * decimal.Parse(DT3.Rows[d]["PieceNo"].ToString())) / decimal.Parse("6000"),
                                           0);
                                    break;
                                default:
                                    volumeWeight += Math.Round(
                                         (decimal.Parse(DT3.Rows[d]["Length"].ToString()) * decimal.Parse(DT3.Rows[d]["Breadth"].ToString()) *
                                         decimal.Parse(DT3.Rows[d]["Height"].ToString()) * decimal.Parse(DT3.Rows[d]["PieceNo"].ToString())) / decimal.Parse("366"),
                                        0);
                                    break;
                            }
                            if (int.Parse(DT3.Rows[d]["Length"].ToString()) > 0 && int.Parse(DT3.Rows[d]["Breadth"].ToString()) > 0 && int.Parse(DT3.Rows[d]["Height"].ToString()) > 0 && int.Parse(DT3.Rows[d]["PieceNo"].ToString()) > 0)
                            {
                                if (strDimensionTag == "")
                                {
                                    strDimensionTag = "/" + RtdIncreament + "/ND/" + DT3.Rows[d]["UOM"].ToString().ToUpper() + DT3.Rows[d]["GrossWeight"].ToString() + "/" + DT3.Rows[d]["Units"].ToString().ToUpper() + DT3.Rows[d]["Length"].ToString() + "-" + DT3.Rows[d]["Breadth"].ToString() + "-" + DT3.Rows[d]["Height"].ToString() + "/" + DT3.Rows[d]["PieceNo"].ToString();
                                }
                                else
                                {
                                    strDimensionTag += "\r\n/" + RtdIncreament + "/ND/" + DT3.Rows[d]["UOM"].ToString().ToUpper() + DT3.Rows[d]["GrossWeight"].ToString() + "/" + DT3.Rows[d]["Units"].ToString().ToUpper() + DT3.Rows[d]["Length"].ToString() + "-" + DT3.Rows[d]["Breadth"].ToString() + "-" + DT3.Rows[d]["Height"].ToString() + "/" + DT3.Rows[d]["PieceNo"].ToString();
                                }

                            }
                        }

                        if (volumeWeight > 0)
                        {
                            RtdIncreament += 1;
                            strVolumeTag = "/" + RtdIncreament + "/NV/MC" + String.Format("{0:0.00}", Convert.ToDecimal(volumeWeight / decimal.Parse("166.66")));
                        }

                    }
                    if (DT10 != null && DT10.Rows.Count > 0)
                    {
                        int tablecount = 0;
                        tablecount = DT10.Rows.Count;
                        for (int u = 0; u < DT10.Rows.Count; u++)
                        {
                            RtdIncreament += 1;
                            if (DT10.Rows[u]["ULDNo"].ToString() != "" && DT10.Rows[u]["ULDNo"].ToString().Length == 10 && int.Parse(DT10.Rows[u]["Slac"].ToString()) > 0)
                            {

                                if (strULDBUP == "")
                                {
                                    strULDBUP = "/" + RtdIncreament + "/NU/" + DT10.Rows[u]["ULDNo"].ToString().ToUpper() + "\r\n/" + "" + (RtdIncreament + 1).ToString() + "/NS/" + DT10.Rows[u]["Slac"].ToString();
                                    RtdIncreament += 1;
                                }
                                else
                                {

                                    if ((tablecount - 1) == u)
                                        strULDBUP += "\r\n/" + RtdIncreament + "/NU/" + DT10.Rows[u]["ULDNo"].ToString().ToUpper() + "\r\n/" + "" + (RtdIncreament + 1).ToString() + "/NS/" + DT10.Rows[u]["Slac"].ToString();
                                    else
                                    {

                                        strULDBUP += "\r\n/" + RtdIncreament + "/NU/" + DT10.Rows[u]["ULDNo"].ToString().ToUpper() + "\r\n/" + "" + (RtdIncreament + 1).ToString() + "/NS/" + DT10.Rows[u]["Slac"].ToString();
                                        RtdIncreament += 1;
                                    }
                                }
                            }
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



                    DataTable dtcheckWalkingcustomer = GF.RemoveAgentTagforWalkingCustomer("Quick Booking", DT1.Rows[0]["ShippingAgentCode"].ToString()).Tables[0];
                    if (dtcheckWalkingcustomer != null && dtcheckWalkingcustomer.Rows.Count > 0 && dtcheckWalkingcustomer.Rows[0]["AppParameter"].ToString().ToUpper() == "DEFAULTAGENT")
                    {
                        FWBData.agentaccnum = string.Empty;
                        FWBData.agentIATAnumber = string.Empty;
                        FWBData.agentname = string.Empty;
                        FWBData.agentplace = string.Empty;
                    }
                    else
                    {
                        if (DT1.Rows[0]["ShippingAgentCode"].ToString() != "" && DT1.Rows[0]["ShippingAgentCode"].ToString().All(char.IsNumber))
                        {
                            FWBData.agentaccnum = string.Empty;
                            FWBData.agentIATAnumber = DT1.Rows[0]["ShippingAgentCode"].ToString();
                            FWBData.agentCASSaddress = DT1.Rows[0]["AgentCassAddress"].ToString();
                            FWBData.agentname = DT1.Rows[0]["ShippingAgentName"].ToString().ToUpper();
                            FWBData.agentplace = FWBData.origin.ToUpper();
                        }
                        else
                        {
                            FWBData.agentaccnum = string.Empty;
                            FWBData.agentIATAnumber = string.Empty;
                            FWBData.agentname = string.Empty;
                            FWBData.agentplace = string.Empty;
                        }

                    }

                    //Makeing Reference Tag

                    DataSet dsReference = GF.GetConfigurationofReferenceTag("REF", "REFERENCETAG", "FWB");
                    if (DT1.Rows.Count > 0 && dsReference != null && dsReference.Tables[0].Rows.Count > 0 && dsReference.Tables[0].Rows[0]["AppValue"].ToString() == "TRUE") //Remove this conditional structure or edit its code blocks so that they're not all the same.
                    {
                        // FWBData.senderofficedesignator = dsReference.Tables[0].Rows[0]["OriginSitaAddress"].ToString().Trim();

                        FWBData.senderofficedesignator = DT1.Rows[0]["SenderOfficeDesignator"].ToString().ToUpper();
                        FWBData.senderairport = DT1.Rows[0]["OriginCode"].ToString().ToUpper();
                        FWBData.sendercompanydesignator = DT1.Rows[0]["CompanyDesignatior"].ToString().ToUpper();
                        FWBData.senderFileref = DT1.Rows[0]["BookingFileReference"].ToString().ToUpper();
                    }
                    //else
                    //{
                    //    FWBData.senderofficedesignator = DT1.Rows[0]["SenderOfficeDesignator"].ToString().ToUpper();
                    //    FWBData.senderairport = DT1.Rows[0]["OriginCode"].ToString().ToUpper();
                    //    FWBData.sendercompanydesignator = DT1.Rows[0]["CompanyDesignatior"].ToString().ToUpper();
                    //    FWBData.senderFileref = DT1.Rows[0]["BookingFileReference"].ToString().ToUpper();
                    //}


                    FWBData.currency = DT2.Rows[0]["Currency"].ToString().ToUpper();
                    FWBData.declaredvalue = DT1.Rows[0]["DVCarriage"].ToString().ToUpper() == "0" ? "NVD" : DT1.Rows[0]["DVCarriage"].ToString().ToUpper();
                    FWBData.declaredcustomvalue = DT1.Rows[0]["DVCustom"].ToString().ToUpper() == "0" ? "NCV" : DT1.Rows[0]["DVCustom"].ToString().ToUpper();
                    FWBData.insuranceamount = DT1.Rows[0]["InsuranceAmmount"].ToString().ToUpper();
                    string PaymentMode = string.Empty;
                    PaymentMode = DT2.Rows[0]["PaymentMode"].ToString().ToUpper();
                    FWBData.chargedec = PaymentMode;
                    if (PaymentMode == "CC")
                        FWBData.chargecode = "CC";
                    else
                        FWBData.chargecode = "PP";

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
                        if (FWBData.shippercontactnum.Length > 0) { FWBData.shippercontactidentifier = "TE"; }
                        if (FWBData.conscontactnum.Length > 0) { FWBData.conscontactidentifier = "TE"; }

                        //FWBData.senderairport = DT1.Rows[0]["OriginCode"].ToString().Trim();
                        FWBData.senderPariticipentAirport = DT1.Rows[0]["OriginCode"].ToString().Trim();
                        FWBData.senderParticipentIdentifier = DT1.Rows[0]["SenderParticipentIdentifier"].ToString().Trim();
                        FWBData.senderParticipentCode = DT1.Rows[0]["SenderParticipentCode"].ToString();
                        //string str=RemoveSpecialCharacters(DT1.Rows[0]["AgentCode"].ToString());
                        //if (str.Length > 7)
                        //    FWBData.senderParticipentCode = str.Substring(0, 7);
                        //else
                        //    FWBData.senderParticipentCode = str.PadLeft(7,'0');


                    }
                    else
                    {
                        FWBMsg = "No Shipper/Consignee Info Availabe for FWB";
                        return FWBMsg;
                    }
                }

                #endregion
                FWBMsg = EncodeFWBForSend(ref FWBData, ref othData, ref othSrvData, ref fwbrates, ref custominfo, strDimensionTag, strVolumeTag, strULDBUP, strRouteTag);
            }
            catch (Exception ex)
            {
                FWBMsg = ex.Message;
            }
            return FWBMsg;
        }
        #endregion


        #region EncodeFWBForSend
        public string EncodeFWBForSend(ref MessageData.fwbinfo fwbdata, ref MessageData.othercharges[] fwbOtherCharge, ref MessageData.otherserviceinfo[] othinfoarray, ref MessageData.RateDescription[] fwbrate, ref MessageData.customsextrainfo[] custominfo, string DimensionTag, string VolumeTag, string strULDBUP, string strRouteTag)
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
                    if (fwbdata.shippername.Length > 0)
                    {
                        str1 = "/" + fwbdata.shippername;
                    }
                    if (fwbdata.shipperadd.Length > 0)
                    {
                        str2 = "/" + fwbdata.shipperadd;
                    }

                    if (fwbdata.shipperplace.Length > 0 || fwbdata.shipperstate.Length > 0)
                    {
                        str3 = "/" + fwbdata.shipperplace + "/" + fwbdata.shipperstate;
                    }
                    if (fwbdata.shippercountrycode.Length > 0 || fwbdata.shipperpostcode.Length > 0 || fwbdata.shippercontactidentifier.Length > 0 || fwbdata.shippercontactnum.Length > 0)
                    {
                        str4 = "/" + fwbdata.shippercountrycode + "/" + fwbdata.shipperpostcode + "/" + fwbdata.shippercontactidentifier + "/" + fwbdata.shippercontactnum;
                    }

                    if (fwbdata.shipperaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line5 = "SHP/" + fwbdata.shipperaccnum;

                        if (str4.Length > 0)
                        {
                            line5 = line5.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line5 = line5.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line5 = line5.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line5 = line5.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure("Error :", ex);
                }
                #endregion

                //CNE
                #region Line 6
                string line6 = "";
                str1 = "";
                str2 = "";
                str3 = "";
                str4 = "";
                try
                {
                    if (fwbdata.consname.Length > 0)
                    {
                        str1 = "/" + fwbdata.consname;
                    }
                    if (fwbdata.consadd.Length > 0)
                    {
                        str2 = "/" + fwbdata.consadd;
                    }

                    if (fwbdata.consplace.Length > 0 || fwbdata.consstate.Length > 0)
                    {
                        str3 = "/" + fwbdata.consplace + "/" + fwbdata.consstate;
                    }
                    if (fwbdata.conscountrycode.Length > 0 || fwbdata.conspostcode.Length > 0 || fwbdata.conscontactidentifier.Length > 0 || fwbdata.conscontactnum.Length > 0)
                    {
                        str4 = "/" + fwbdata.conscountrycode + "/" + fwbdata.conspostcode + "/" + fwbdata.conscontactidentifier + "/" + fwbdata.conscontactnum;
                    }

                    if (fwbdata.consaccnum.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line6 = "CNE/" + fwbdata.consaccnum;
                        if (str4.Length > 0)
                        {
                            line6 = line6.Trim('/') + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/') + "\r\n/" + str4.Trim('/');
                        }
                        else if (str3.Length > 0)
                        {
                            line6 = line6.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line6 = line6.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line6 = line6.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure("Error :", ex);
                }
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
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure("Error :", ex);
                }
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
                    if (fwbdata.notifyadd.Length > 0)
                    {
                        str1 = "/" + fwbdata.notifyadd;
                    }
                    if (fwbdata.notifyplace.Length > 0 || fwbdata.notifystate.Length > 0)
                    {
                        str2 = "/" + fwbdata.notifyplace + "/" + fwbdata.notifystate;
                    }
                    if (fwbdata.notifycountrycode.Length > 0 || fwbdata.notifypostcode.Length > 0 || fwbdata.notifycontactidentifier.Length > 0 || fwbdata.notifycontactnum.Length > 0)
                    {
                        str3 = "/" + fwbdata.notifycountrycode + "/" + fwbdata.notifypostcode + "/" + fwbdata.notifycontactidentifier + "/" + fwbdata.notifycontactnum;
                    }

                    if (fwbdata.notifyname.Length > 0 || str1.Length > 0 || str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
                    {
                        line9 = "NFY/" + fwbdata.shipperaccnum;
                        if (str3.Length > 0)
                        {
                            line9 = line9.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/') + "\r\n/" + str3.Trim('/');
                        }
                        else if (str2.Length > 0)
                        {
                            line9 = line9.Trim() + "\r\n/" + str1.Trim('/') + "\r\n/" + str2.Trim('/');
                        }
                        else if (str1.Length > 0)
                        {
                            line9 = line9.Trim() + "\r\n/" + str1.Trim('/');
                        }
                    }
                }
                catch (Exception ex)
                {
                    clsLog.WriteLogAzure("Error :", ex);
                }
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
                string line29 = "";
                if (custominfo.Length > 0)
                {
                    for (int i = 0; i < custominfo.Length; i++)
                    {
                        line29 = "/" + custominfo[i].IsoCountryCodeOci + "/" + custominfo[i].InformationIdentifierOci + "/" + custominfo[i].CsrIdentifierOci + "/" + custominfo[i].SupplementaryCsrIdentifierOci + "$";
                    }
                    line29 = "OCI" + line4.Trim('$');
                    line29 = line4.Replace("$", "\r\n");
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
                fwbstr += "\r\n" + line4.Trim('/') + "\r\n" + line5.Trim('/') + "\r\n" + line6.Trim('/');
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
            catch (Exception)
            {
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
                            str1 = "/" + (cnt++) + "/NG/" + fwbrate[i].goodsnature;
                        }
                        else
                        {
                            str1 = "/NG/" + fwbrate[i].goodsnature;
                            cnt++;
                        }
                    }

                    ///Update By :Badiuz Aman khan
                    ///Update on :21-09-2015
                    // Remarks:Remove NC,ND ,NU,NN,NS,NH, This is optional Field.

                    //if (fwbrate[i].goodsnature1.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str2 = "/" + (cnt++) + "/NC/" + fwbrate[i].goodsnature1;
                    //    }
                    //    else
                    //    {
                    //        str2 = "/NC/" + fwbrate[i].goodsnature1;
                    //        cnt++;
                    //    }
                    //}
                    //if (fwbrate[i].weight.Length > 0 || fwbrate[i].length.Length > 0 || fwbrate[i].width.Length > 0 || fwbrate[i].height.Length > 0 || fwbrate[i].pcscnt.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        if (fwbrate[i].length.Length > 0 || fwbrate[i].width.Length > 0 || fwbrate[i].height.Length > 0 || fwbrate[i].pcscnt.Length > 0)
                    //            str3 = "/" + (cnt++) + "/ND/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/" + fwbrate[i].unit + fwbrate[i].length + "-" + fwbrate[i].width + "-" + fwbrate[i].height + "/" + fwbrate[i].pcscnt;
                    //        else
                    //            str3 = "/" + (cnt++) + "/ND/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/NDA"; 

                    //    }
                    //    else
                    //    {
                    //        if (fwbrate[i].length.Length > 0 || fwbrate[i].width.Length > 0 || fwbrate[i].height.Length > 0 || fwbrate[i].pcscnt.Length > 0)
                    //            str3 = "/ND/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/" + fwbrate[i].unit + fwbrate[i].length + "-" + fwbrate[i].width + "-" + fwbrate[i].height + "/" + fwbrate[i].pcscnt;
                    //        else
                    //            str3 = "/ND/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/NDA";

                    //        cnt++;
                    //    }
                    //}
                    //else
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str3 = "/" + (cnt++) + "/ND/" + "/" + "NDA";
                    //    }
                    //}
                    //str3 = str3.Replace("--", "");
                    //if (fwbrate[i].volcode.Length > 0 || fwbrate[i].volamt.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str4 = "/" + (cnt++) + "/NV/" + fwbrate[i].volcode + fwbrate[i].volamt;
                    //    }
                    //    else
                    //    {
                    //        str4 = "/NV/" + fwbrate[i].volcode + fwbrate[i].volamt;
                    //        cnt++;
                    //    }
                    //}
                    //if (fwbrate[i].uldtype.Length > 0 || fwbrate[i].uldserialnum.Length > 0 || fwbrate[i].uldowner.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str5 = "/" + (cnt++) + "/NU/" + fwbrate[i].uldtype + fwbrate[i].uldserialnum + fwbrate[i].uldowner;
                    //    }
                    //    else
                    //    {
                    //        str5 = "/NU/" + fwbrate[i].uldtype + fwbrate[i].uldserialnum + fwbrate[i].uldowner;
                    //        cnt++;

                    //    }
                    //}
                    //if (fwbrate[i].slac.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str6 = "/" + (cnt++) + "/NS/" + fwbrate[i].slac;
                    //    }
                    //    else
                    //    {
                    //        str6 = "/NS/" + fwbrate[i].slac;
                    //        cnt++;
                    //    }
                    //}
                    //if (fwbrate[i].hermonisedcomoditycode.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str7 = "/" + (cnt++) + "/NH/" + fwbrate[i].hermonisedcomoditycode;
                    //    }
                    //    else
                    //    {
                    //        str7 = "/NH/" + fwbrate[i].hermonisedcomoditycode;
                    //        cnt++;
                    //    }
                    //}
                    //if (fwbrate[i].isocountrycode.Length > 0)
                    //{
                    //    if (cnt > 1)
                    //    {
                    //        str8 = "/" + (cnt++) + "/NO/" + fwbrate[i].isocountrycode;
                    //    }
                    //    else
                    //    {
                    //        str8 = "/NO/" + fwbrate[i].isocountrycode;
                    //        cnt++;
                    //    }
                    //}
                    Ratestr += "RTD/" + (i + 1) + "/" + fwbrate[i].pcsidentifier + fwbrate[i].numofpcs + "/" + fwbrate[i].weightindicator + fwbrate[i].weight + "/C" + fwbrate[i].rateclasscode + "/W" + fwbrate[i].awbweight + "/R" + fwbrate[i].chargerate + "/T" + fwbrate[i].chargeamt;
                    Ratestr = Ratestr.Trim('/') + "$" + str1.Trim() + "$" + str2.Trim() + "$" + str3.Trim() + "$" + str4.Trim() + "$" + str5.Trim() + "$" + str6.Trim() + "$" + str7.Trim() + "$" + str8.Trim();
                    Ratestr = Ratestr.Replace("$$", "$");
                    Ratestr = Ratestr.Trim('$');
                    Ratestr = Ratestr.Replace("$", "\r\n");
                }

            }
            catch (Exception)
            {
                Ratestr = null;
            }
            return Ratestr;
        }
        #endregion


    }
}
