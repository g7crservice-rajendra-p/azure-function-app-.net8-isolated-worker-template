#region XFNM Message Processor Class Description
/* XFNM Message Processor Class Description.
      * Company              :   QID SmartKargo(I)	Pvt. Ltd.
      * Copyright            :   Copyright © 2017 QID SmartKargo(I)	Pvt. Ltd.
      * Purpose              :   XFNM Message Processor Class
      * Created By           :   Yoginath
      * Created On           :   2017-07-19
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
using System.Text;
using System.Xml;

namespace QidWorkerRole
{
    public class XFNMMessageProcessor
    {

        #region :: Public Methods ::
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FlightNo"></param>
        /// <param name="FlightDate"></param>
        /// <param name="FlightOrigin"></param>
        /// <param name="awbNumber"></param>
        /// <param name="awbPrefix"></param>
        /// <returns></returns>
        /// 
        private readonly ISqlDataHelperDao _readWriteDao;
        private readonly ILogger<XFNMMessageProcessor> _logger;
        private readonly GenericFunction _genericFunction;

        #region Constructor
        public XFNMMessageProcessor(ISqlDataHelperFactory sqlDataHelperFactory,
            ILogger<XFNMMessageProcessor> logger,
            GenericFunction genericFunction)
        {
            _readWriteDao = sqlDataHelperFactory.Create(readOnly: false);
            _logger = logger;
            _genericFunction = genericFunction;
        }
        #endregion
        public async Task GenerateXFNMMessage(string strMessage, string strErrorMessage, string awbPrefix = "", string awbNumber = "", string strMessageFrom = "", string commType = "", string conditionCode = "", string msgName = "")
        {
            StringBuilder sbgenerateXFNMMessage = new StringBuilder();
            //GenericFunction generalFunction = new GenericFunction();

            //int sequence = 0;
            try
            {
                DataSet dsxfnmMessage = new DataSet();
                DataRow[] drs;
                dsxfnmMessage = await GetRecordtoGenerateXFNMMessage(awbPrefix, awbNumber, "XFNM");

                if (dsxfnmMessage != null && dsxfnmMessage.Tables != null && dsxfnmMessage.Tables.Count > 0 && dsxfnmMessage.Tables[0].Rows.Count > 0)
                {
                    var xfnmDataSet = new DataSet();
                    var xmlSchema = await _genericFunction.GetXMLMessageData("XFNM");

                    if (xmlSchema != null && xmlSchema.Tables.Count > 0 && xmlSchema.Tables[0].Rows.Count > 0)
                    {
                        string messageXML = Convert.ToString(xmlSchema.Tables[0].Rows[0]["XMLMessageData"]);
                        messageXML = ReplacingNodeNames(messageXML);
                        //Original Message
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(messageXML);
                        XmlNode node = doc.CreateNode(XmlNodeType.Element, "ResponseDetail", null);
                        node.AppendChild(doc.CreateCDataSection(strMessage));
                        doc.DocumentElement.AppendChild(node);
                        messageXML = doc.OuterXml;
                        var txMessage = new StringReader(messageXML);
                        xfnmDataSet.ReadXml(txMessage);

                        if (dsxfnmMessage != null && dsxfnmMessage.Tables != null && dsxfnmMessage.Tables.Count > 0 && dsxfnmMessage.Tables[0].Rows.Count > 0)
                        {
                            // Master Header Docunent
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["ID"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["ReferenceNumber"]);
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["Name"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["MessageName"]);
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["MessageTypeCode"]);
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["IssueDateTime"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["MessageCreatedDate"]);
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["PurposeCode"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["PurposeCode"]);
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["VersionNumber"]);
                            xfnmDataSet.Tables["MessageHeaderDocument"].Rows[0]["ConversationID"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["ConversionID"]);

                            //SenderParty
                            if (xfnmDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = xfnmDataSet.Tables["PrimaryID"].Select("SenderParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    drs[0]["schemeID"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["SenderIdentification"]);
                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["SenderText"]);
                                }
                            }
                            //RecipientParty
                            if (xfnmDataSet.Tables.Contains("PrimaryID"))
                            {
                                drs = xfnmDataSet.Tables["PrimaryID"].Select("RecipientParty_Id=0");
                                if (drs.Length > 0)
                                {
                                    drs[0]["schemeID"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["RecipientIdentification"]);
                                    drs[0]["PrimaryID_Text"] = Convert.ToString(dsxfnmMessage.Tables[0].Rows[0]["RequestText"]);
                                }
                            }

                            //BussinessHeaderDocunment
                            xfnmDataSet.Tables["BusinessHeaderDocument"].Rows[0]["ID"] = Convert.ToString("");
                            xfnmDataSet.Tables["BusinessHeaderDocument"].Rows[0]["Name"] = Convert.ToString(msgName);
                            switch (msgName)
                            {
                                case "XFWB":
                                    xfnmDataSet.Tables["BusinessHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString("740");
                                    break;
                                case "XFFR":
                                    xfnmDataSet.Tables["BusinessHeaderDocument"].Rows[0]["TypeCode"] = Convert.ToString("335");
                                    break;
                                default:
                                    break;
                            }

                            if (conditionCode == "Error")
                            {
                                xfnmDataSet.Tables["BusinessHeaderDocument"].Rows[0]["StatusCode"] = Convert.ToString("Rejected");
                            }
                            else
                            {
                                xfnmDataSet.Tables["BusinessHeaderDocument"].Rows[0]["StatusCode"] = Convert.ToString("Processed");
                            }

                            //ResponseStatus
                            xfnmDataSet.Tables["ResponseStatus"].Rows[0]["ConditionCode"] = Convert.ToString(conditionCode);
                            xfnmDataSet.Tables["ResponseStatus"].Rows[0]["ReasonCode"] = Convert.ToString("");
                            xfnmDataSet.Tables["ResponseStatus"].Rows[0]["Reason"] = Convert.ToString(strErrorMessage);
                            xfnmDataSet.Tables["ResponseStatus"].Rows[0]["Information"] = Convert.ToString(strErrorMessage);


                            //xfnmDataSet.Tables["ResponseDetail"].Row[0]=strMessage;


                        }
                        sbgenerateXFNMMessage = new StringBuilder(xfnmDataSet.GetXml());
                        sbgenerateXFNMMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns:rsm=\"iata: waybill:1\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns:ram=\"iata: datamodel:3\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                        sbgenerateXFNMMessage.Replace(" xsi:schemaLocation=\"iata: waybill:1 Waybill_1.xsd\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns: ram = \"iata: datamodel:3\"", "");
                        sbgenerateXFNMMessage.Replace("xmlns: ram = \"iata: datamodel:3\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns:rsm=\"iata:waybill:1\"", "");
                        sbgenerateXFNMMessage.Replace(" xmlns:ram=\"iata:datamodel:3\"", "");

                        sbgenerateXFNMMessage.Insert(13, " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:rsm=\"iata:response:3\" xmlns:ccts=\"urn:un:unece:uncefact:documentation:standard:CoreComponentsTechnicalSpecification:2\" xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:8\"  xmlns:ram=\"iata:datamodel:3\"  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"  xsi:schemaLocation=\"iata:response:3 Response_3.xsd\"");
                        sbgenerateXFNMMessage.Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                        xfnmDataSet.Dispose();


                        //GenericFunction GF = new GenericFunction();

                        string SitaMessageHeader = string.Empty, Emailaddress = string.Empty, FNAMessageVersion = string.Empty, messageid = string.Empty;

                        DataSet dscheckconfiguration = await _genericFunction.GetSitaAddressandMessageVersion("", "XFNM", "AIR", "", "", "", string.Empty, awbPrefix);
                        if (dscheckconfiguration != null && dscheckconfiguration.Tables[0].Rows.Count > 0)
                        {
                            Emailaddress = Convert.ToString(dscheckconfiguration.Tables[0].Rows[0]["PartnerEmailiD"]);
                            string MessageCommunicationType = Convert.ToString(dscheckconfiguration.Tables[0].Rows[0]["MsgCommType"]);
                            FNAMessageVersion = Convert.ToString(dscheckconfiguration.Tables[0].Rows[0]["MessageVersion"]);
                            messageid = Convert.ToString(dscheckconfiguration.Tables[0].Rows[0]["MessageID"]);
                        }
                        if (dscheckconfiguration != null && dscheckconfiguration.Tables.Count > 0 && dscheckconfiguration.Tables[0].Rows.Count > 0)
                        {
                            if (commType.Contains("SITA"))
                            {
                                SitaMessageHeader = _genericFunction.MakeMailMessageFormat(strMessageFrom, dscheckconfiguration.Tables[0].Rows[0]["OriginSenderAddress"].ToString(), dscheckconfiguration.Tables[0].Rows[0]["MessageID"].ToString());
                                await _genericFunction.SaveMessageOutBox("XFNM", SitaMessageHeader + "\r\n" + Convert.ToString(sbgenerateXFNMMessage), "SITAFTP", "SITAFTP", "", "", "", "", awbPrefix + "-" + awbNumber);
                            }
                        }
                        else
                        {
                            string ToEmailAddress = (strMessageFrom == string.Empty ? Emailaddress : strMessageFrom + "," + Emailaddress);
                            ToEmailAddress = (ToEmailAddress == string.Empty ? "priyanka@smartkargo.com" : ToEmailAddress);
                            await _genericFunction.SaveMessageOutBox("XFNM", Convert.ToString(sbgenerateXFNMMessage).ToString(), string.Empty, ToEmailAddress, "", "", "", "", awbPrefix + "-" + awbNumber);
                        }
                    }
                    else
                    {
                        sbgenerateXFNMMessage.Append("No Message format available in the system.");
                    }
                }
                else
                {
                    sbgenerateXFNMMessage.Append("No Data available in the system to generate message.");
                }

            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }

        }


        /// <summary>
        /// Used to decode XFNMMessage
        /// </summary>
        /// <param name="fnmMsg"></param>
        /// <param name="fnadata"></param>
        /// <returns>bool</returns>
        public bool DecodeXFNMMessage(string fnmMsg, ref MessageData.FNA fnadata)
        {
            bool flag = true;
            try
            {
                var fnmXmlDataSet = new DataSet();

                var tx = new StringReader(fnmMsg);
                fnmXmlDataSet.ReadXml(tx);

                fnadata.MessageType = "XFNM";
                fnadata.MsgVersion = Convert.ToString(fnmXmlDataSet.Tables["MessageHeaderDocument"].Rows[0]["VersionID"]);

                fnadata.AckInfo = Convert.ToString(fnmXmlDataSet.Tables["ResponseStatus"].Rows[0]["Reason"]);

                //fnadata.AWBPrefix = strFNAMsg[j].Substring(0, 3);
                //fnadata.AWBnumber = strFNAMsg[j].Substring(4, 8);
                //fnadata.Origin = strFNAMsg[j].Substring(12, 3);
                //fnadata.Destination = strFNAMsg[j].Substring(15, 3);

                fnadata.originalmessage = Convert.ToString(fnmXmlDataSet.Tables["Response"].Rows[0]["ResponseDetail"]);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
        public async Task<bool> SaveAndValidateFNAMessage(int refno, MessageData.FNA fnadata)
        {

            bool flag = true;
            try
            {
                //SQLServer dtb = new SQLServer();
                //string[] pnames = new string[] { "Acknowledgement", "OrignlMsg", "MsgId", "AWBnumber", "AWBPrefix", "Origin", "Destination", "UpdatedOn", "MessageType" };
                //SqlDbType[] ptypes = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.Int, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.DateTime, SqlDbType.VarChar };
                //object[] pvalues = new object[] { fnadata.AckInfo, fnadata.originalmessage, refno, fnadata.AWBnumber, fnadata.AWBPrefix, fnadata.Origin, fnadata.Destination, System.DateTime.Now, fnadata.MessageType };

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Acknowledgement", fnadata.AckInfo),
                    new SqlParameter("@OrignlMsg", fnadata.originalmessage),
                    new SqlParameter("@MsgId", refno),
                    new SqlParameter("@AWBnumber", fnadata.AWBnumber),
                    new SqlParameter("@AWBPrefix", fnadata.AWBPrefix),
                    new SqlParameter("@Origin", fnadata.Origin),
                    new SqlParameter("@Destination", fnadata.Destination),
                    new SqlParameter("@UpdatedOn", System.DateTime.Now),
                    new SqlParameter("@MessageType", fnadata.MessageType)
                };
                //if (!dtb.UpdateData("spUpdateFNAMessageError", pnames, ptypes, pvalues))
                if (!await _readWriteDao.ExecuteNonQueryAsync("spUpdateFNAMessageError", sqlParameters))
                    flag = false;
            }
            catch (Exception ex)
            {
                //scm.logexception(ref ex);
                flag = false;
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
            }
            return flag;
        }

        /// <summary>
        /// Below Method for get House AWB to generate XFHL Message
        /// </summary>
        /// <param name="FlightNo"></param>
        /// <param name="FlightDate"></param>
        /// <param name="FlightOrigin"></param>
        /// <param name="AwbPrefix"></param>
        /// <param name="AWBNo"></param>
        /// <returns></returns>
        public async Task<DataSet> GetRecordtoGenerateXFNMMessage(string AwbPrefix, string AWBNo, string msgtype)
        {
            DataSet? dsmessage = new DataSet();
            try
            {

                //SQLServer da = new SQLServer();
                //string[] paramname = new string[] { "AWBPrefix", "AWBNo", "Msgtype" };
                //object[] paramvalue = new object[] { AwbPrefix, AWBNo, msgtype };
                //SqlDbType[] paramtype = new SqlDbType[] { SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@AWBPrefix", AwbPrefix),
                    new SqlParameter("@AWBNo", AWBNo),
                    new SqlParameter("@Msgtype", msgtype)
                };
                //dsmessage = da.SelectRecords("Messaging.uspGetRecordToGenerateXFNMMessage", paramname, paramvalue, paramtype);
                dsmessage = await _readWriteDao.SelectRecords("Messaging.uspGetRecordToGenerateXFNMMessage", sqlParameters);
            }
            catch (Exception ex)
            {
                //clsLog.WriteLogAzure(ex);
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                dsmessage = null;
            }
            return dsmessage;
        }

        /// <summary>
        /// Replacing duplicate nodes from xml
        /// </summary>
        /// <param name="xmlMsg"></param>
        /// <returns></returns>
        public string ReplacingNodeNames(string xmlMsg)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlMsg);
                XmlNode nodeToFind;
                XmlElement root = doc.DocumentElement;
                XmlNodeList xmlNodelst;
                if (root.Name.Equals("rsm:HouseManifest"))
                {
                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:TransportContractDocument')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_TransportContractDocument");
                        }
                    }

                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:OriginLocation')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_OriginLocation");
                        }
                    }

                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:FinalDestinationLocation')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_FinalDestinationLocation");
                        }
                    }

                    xmlNodelst = root.SelectNodes("//*[starts-with(name(), 'ram:IncludedCustomsNote')]");
                    foreach (XmlNode xmlNode in xmlNodelst)
                    {
                        if (xmlNode.ParentNode.Name.Equals("rsm:MasterConsignment"))
                        {
                            nodeToFind = RenameNode(xmlNode, "ram:MasterConsignment_IncludedCustomsNote");
                        }
                    }

                    xmlMsg = doc.OuterXml;
                    xmlMsg = xmlMsg.Replace("MasterConsignment_TransportContractDocument", "ram:MasterConsignment_TransportContractDocument");
                    xmlMsg = xmlMsg.Replace("MasterConsignment_OriginLocation", "ram:MasterConsignment_OriginLocation");
                    xmlMsg = xmlMsg.Replace("MasterConsignment_FinalDestinationLocation", "ram:MasterConsignment_FinalDestinationLocation");
                    xmlMsg = xmlMsg.Replace("MasterConsignment_IncludedCustomsNote", "ram:MasterConsignment_IncludedCustomsNote");

                    return xmlMsg;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
            return xmlMsg;
        }

        #endregion Public Methods

        #region :: Private Methods ::
        /// <summary>
        /// Rename the existing node with new name
        /// </summary>
        /// <param name="e"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        private XmlNode RenameNode(XmlNode e, string newName)
        {
            try
            {
                XmlDocument doc = e.OwnerDocument;
                XmlNode newNode = doc.CreateNode(e.NodeType, newName, null);
                while (e.HasChildNodes)
                {
                    newNode.AppendChild(e.FirstChild);
                }
                XmlAttributeCollection ac = e.Attributes;
                while (ac.Count > 0)
                {
                    newNode.Attributes.Append(ac[0]);
                }
                XmlNode parent = e.ParentNode;
                parent.ReplaceChild(newNode, e);
                return newNode;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error on {System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
                throw;
            }
        }

        #endregion Private Methods

    }
}
