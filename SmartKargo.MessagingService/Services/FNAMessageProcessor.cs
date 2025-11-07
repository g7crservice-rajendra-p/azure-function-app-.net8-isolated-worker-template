using System;
using System.Data;
using QID.DataAccess;
using System.Text;
namespace QidWorkerRole
{
    public class FNAMessageProcessor
    {
        SCMExceptionHandlingWorkRole scm = new SCMExceptionHandlingWorkRole();

        public FNAMessageProcessor()
        { }

        /// <summary>
        /// Used to decode FNAMessage
        /// </summary>
        /// <param name="FNAMsg"></param>
        /// <param name="fnadata"></param>
        /// <returns>bool</returns>
        public bool DecodeFNAMessage(string FNAMsg, ref MessageData.FNA fnadata)
        {
            bool flag = true;
            try
            {
                string ack = string.Empty, lastTag = string.Empty, rsmsg = string.Empty;
                int j = 0;


                if (FNAMsg.StartsWith("FNA", StringComparison.OrdinalIgnoreCase) || FNAMsg.StartsWith("FMA", StringComparison.OrdinalIgnoreCase))
                {

                    string[] strFNAMsg = FNAMsg.Split('$');

                    if (strFNAMsg.Length > 3)
                    {

                        for (int i = 0; i < strFNAMsg.Length; i++)
                        {
                            if (strFNAMsg[i].StartsWith("FNA", StringComparison.OrdinalIgnoreCase) || strFNAMsg[i].StartsWith("FMA", StringComparison.OrdinalIgnoreCase))
                            {

                                string[] msg = strFNAMsg[i].Split('/');
                                fnadata.MessageType = msg[0];

                                if (msg.Length > 2)
                                    fnadata.MsgVersion = msg[1];


                            }
                            if (strFNAMsg[i].StartsWith("ACK", StringComparison.OrdinalIgnoreCase))
                            {
                                string[] msg = strFNAMsg[i].Split('/');

                                fnadata.AckInfo = msg[1].ToString();



                                lastTag = "ACK";

                            }

                            if (strFNAMsg[i].StartsWith("/") && lastTag == "ACK")
                            {
                                string[] msg = strFNAMsg[i].Split('/');
                                //ack = ack + " " + msg[1].ToString();
                                fnadata.AckInfo = fnadata.AckInfo + " " + msg[1].ToString();

                            }
                            if (i > 1 && (strFNAMsg[i].Substring(0, 1) != "/"))
                            {
                                lastTag = "Orgnlmsg";
                            }
                            if (lastTag != "ACK" && i > 1)
                            {
                                for (j = i; j < strFNAMsg.Length; j++)
                                {
                                    if (j == i + 1)
                                    {
                                        fnadata.AWBPrefix = strFNAMsg[j].Substring(0, 3);
                                        fnadata.AWBnumber = strFNAMsg[j].Substring(4, 8);
                                        fnadata.Origin = strFNAMsg[j].Substring(12, 3);
                                        fnadata.Destination = strFNAMsg[j].Substring(15, 3);
                                    }

                                    fnadata.originalmessage = fnadata.originalmessage + strFNAMsg[j].Replace(" ", "");

                                }
                            }


                            if (j == strFNAMsg.Length)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }

            return flag;
        }

        /// <summary>
        /// Save error/ack in tbloutbox
        /// to do:- need to update error message against that awb number
        /// </summary>
        /// <param name="refno"></param>
        /// <param name="fnadata"></param>
        /// <returns>bool</returns>
        public bool SaveAndValidateFNAMessage(int refno, MessageData.FNA fnadata)
        {

            bool flag = true;
            try
            {
                SQLServer dtb = new SQLServer();
                string[] pnames = new string[] { "Acknowledgement", "OrignlMsg", "MsgId", "AWBnumber", "AWBPrefix", "Origin", "Destination", "UpdatedOn", "MessageType" };
                SqlDbType[] ptypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                object[] pvalues = new object[] { fnadata.AckInfo, fnadata.originalmessage, refno, fnadata.AWBnumber, fnadata.AWBPrefix, fnadata.Origin, fnadata.Destination, System.DateTime.Now, fnadata.MessageType };
                if (!dtb.UpdateData("spUpdateFNAMessageError", pnames, ptypes, pvalues))
                    flag = false;
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Relay FNA message to the PIMA address or to the Primary addredd
        /// </summary>
        public void GenerateFNAMessage(string strMessage, string strErrorMessage, string AWBPrefix = "", string awbnum = "", string strMessageFrom = "", string commType = "", string PIMAAddress = "")
        {
            try
            {
                GenericFunction genericFunction = new GenericFunction();
                bool relayFMAFNAWithPIMAAddress = Convert.ToBoolean(genericFunction.ReadValueFromDb("RelayFMAFNAWithPIMAAddress") == string.Empty ? "false" : genericFunction.ReadValueFromDb("RelayFMAFNAWithPIMAAddress"));
                bool relayFMAFNAWithPrimaryAddress = Convert.ToBoolean(genericFunction.ReadValueFromDb("RelayFMAFNAWithPrimaryAddress") == string.Empty ? "false" : genericFunction.ReadValueFromDb("RelayFMAFNAWithPrimaryAddress"));

                if ((relayFMAFNAWithPIMAAddress && PIMAAddress.Trim().Length > 0) || relayFMAFNAWithPrimaryAddress)
                {
                    string SitaMessageHeader = string.Empty, Emailaddress = string.Empty, FNAMessageVersion = string.Empty, messageid = string.Empty;
                    strMessage = strMessage.Replace("$", "\r\n");
                    strMessage = strMessage.Replace("$", "\n");
                    strMessage = strMessage.Replace("$$", "\r\n");

                    DataSet dscheckconfiguration = genericFunction.GetSitaAddressandMessageVersion("", "FNA", "AIR", "", "", "", string.Empty, AWBPrefix);
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
                    strFNAMessage.Append(strErrorMessage.Trim().ToUpper().Replace("/", " ").Replace(",", "") + "\r\n");
                    strFNAMessage.Append(strMessage);

                    string SFTPHeaderSITAddress = string.Empty, ToEmailAddress = string.Empty;

                    if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows.Count > 0)
                    {
                        if ((strMessageFrom.Trim().Length > 0 && !strMessageFrom.Trim().Contains("@")) || dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Length > 0)
                        {
                            string patnerSitaID = strMessageFrom + "," + dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString();
                            SitaMessageHeader = genericFunction.MakeMailMessageFormat(patnerSitaID.Trim(','), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString(), PIMAAddress);
                        }
                        if (dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Length > 0)
                            SFTPHeaderSITAddress = genericFunction.MakeMailMessageFormat(dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString(), PIMAAddress);
                    }

                    if (strMessageFrom.Trim().Contains("@"))
                        ToEmailAddress = strMessageFrom + "," + Emailaddress;
                    else
                        ToEmailAddress = Emailaddress;

                    if (SitaMessageHeader == string.Empty && (commType.ToUpper() == "SITAFTP"))
                    {
                        SitaMessageHeader = genericFunction.MakeMailMessageFormat(strMessageFrom, dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString(), PIMAAddress);
                    }

                    
                        genericFunction.SaveMessageToOutbox("", strFNAMessage.ToString(), ToEmailAddress, SitaMessageHeader, SFTPHeaderSITAddress, type: "FNA", awbNumber: AWBPrefix + "-" + awbnum);
                  

                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }

        /// <summary>
        /// Relay FMA message to the PIMA address or to the Primary addredd
        /// </summary>
        public void GenerateFMAMessage(string strMessage, string strSuccessMessage, string AWBPrefix = "", string awbnum = "", string strMessageFrom = "", string commType = "", string PIMAAddress = "")
        {
            try
            {
                string SFTPHeaderSITAddress = string.Empty, ToEmailAddress = string.Empty;
                GenericFunction genericFunction = new GenericFunction();
                bool relayFMAFNAWithPIMAAddress = Convert.ToBoolean(genericFunction.ReadValueFromDb("RelayFMAFNAWithPIMAAddress") == string.Empty ? "false" : genericFunction.ReadValueFromDb("RelayFMAFNAWithPIMAAddress"));
                bool relayFMAFNAWithPrimaryAddress = Convert.ToBoolean(genericFunction.ReadValueFromDb("RelayFMAFNAWithPrimaryAddress") == string.Empty ? "false" : genericFunction.ReadValueFromDb("RelayFMAFNAWithPrimaryAddress"));

                PIMAAddress = !relayFMAFNAWithPIMAAddress ? string.Empty : PIMAAddress;
                if ((relayFMAFNAWithPIMAAddress && PIMAAddress.Trim().Length > 0) || relayFMAFNAWithPrimaryAddress)
                {
                    string SitaMessageHeader = string.Empty, Emailaddress = string.Empty, FMAMessageVersion = string.Empty, messageid = string.Empty;
                    strMessage = strMessage.Replace("$", "\r\n");
                    strMessage = strMessage.Replace("$", "\n");
                    strMessage = strMessage.Replace("$$", "\r\n");

                    StringBuilder strFMAMessage = new StringBuilder();
                    strFMAMessage.Append("FMA\r\n");
                    strFMAMessage.Append("ACK/");
                    strFMAMessage.Append(strSuccessMessage.Trim().ToUpper().Replace("/", " ").Replace(",", "") + "\r\n");
                    strFMAMessage.Append(strMessage);

                    DataSet dscheckconfiguration = genericFunction.GetSitaAddressandMessageVersion("", "FMA", "AIR", "", "", "", string.Empty, AWBPrefix);
                    if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows.Count > 0)
                    {
                        Emailaddress = dscheckconfiguration.Tables[0].Rows[0]["PartnerEmailiD"].ToString();
                        string MessageCommunicationType = dscheckconfiguration.Tables[0].Rows[0]["MsgCommType"].ToString();
                        FMAMessageVersion = dscheckconfiguration.Tables[0].Rows[0]["MessageVersion"].ToString();
                        messageid = dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString();

                        if ((strMessageFrom.Trim().Length > 0 && !strMessageFrom.Trim().Contains("@")) || dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString().Trim().Length > 0)
                        {
                            string patnerSitaID = strMessageFrom + "," + dscheckconfiguration.Tables[0].Rows[0]["PatnerSitaID"].ToString();
                            SitaMessageHeader = genericFunction.MakeMailMessageFormat(patnerSitaID.Trim(','), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), messageid, dscheckconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString(), PIMAAddress);
                        }
                    }
                    if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString().Trim().Length > 0)
                        SFTPHeaderSITAddress = genericFunction.MakeMailMessageFormat(dscheckconfiguration.Tables[0].Rows[0]["SFTPHeaderSITAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["SITAHeaderType"].ToString(), PIMAAddress);
                    if (strMessageFrom.Trim().Contains("@") || Emailaddress.Trim().Length > 0)
                        ToEmailAddress = (strMessageFrom == string.Empty ? Emailaddress : strMessageFrom + "," + Emailaddress);

                    genericFunction.SaveMessageToOutbox("", strFMAMessage.ToString(), ToEmailAddress, SitaMessageHeader, SFTPHeaderSITAddress, type: "FMA", awbNumber: AWBPrefix + "-" + awbnum);
                }
            }
            catch (Exception ex)
            {
                clsLog.WriteLogAzure(ex);
            }
        }
    }
}
